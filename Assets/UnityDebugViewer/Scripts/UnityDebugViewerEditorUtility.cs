﻿using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

namespace UnityDebugViewer
{
    public static class UnityDebugViewerEditorUtility
    {
        public const char UnityInternalDirectorySeparator = '/';
        public const string EllipsisStr = "........";
        public const int DisplayLineNumber = 8;

        public static int GetIndexOfTargetString(string inputStr, string targetStr, int appearTimes = 1)
        {
            string regex = "((" + Regex.Escape(targetStr) + ").*?){" + appearTimes + "}";
            Match m = Regex.Match(inputStr, regex);

            if (m.Success)
            {
                return m.Groups[2].Captures[appearTimes - 1].Index;
            }
            else
            {
                return -1;
            }
        }

        public static string GetSourceContent(string filePath, int lineNumber)
        {
            var validFilePath = ConvertToSystemFilePath(filePath);
            if (!File.Exists(validFilePath))
            {
                return string.Empty;
            }

            var lineArray = File.ReadAllLines(validFilePath);

            int fileLineNumber = lineNumber - 1;
            int firstLine = Mathf.Max(fileLineNumber - DisplayLineNumber / 2, 0);
            int lastLine = Mathf.Min(fileLineNumber + DisplayLineNumber / 2 + 1, lineArray.Count());

            string souceContent = string.Empty;
            if(firstLine != 0)
            {
                souceContent = string.Format("{0}\n{1}", EllipsisStr, souceContent);
            }
            for(int index = firstLine;index < lastLine;index++)
            {
                string str = ReplaceTabWithSpace(lineArray[index]) + "\n";
                if(index == fileLineNumber)
                {
                    str = string.Format("<color=#cc0000ff>{0}</color>", str);
                }

                souceContent += str;
            }
            if(lastLine != lineArray.Count())
            {
                souceContent = string.Format("{0}\n{1}", souceContent, EllipsisStr);
            }

            return souceContent;
        }

        public static void ParseLogFile(string logFilePath)
        {
            if (!string.IsNullOrEmpty(logFilePath) && File.Exists(logFilePath))
            {
                var logFineLineArray = File.ReadAllLines(logFilePath);
                if(logFineLineArray == null || logFineLineArray.Length == 0)
                {
                    return;
                }

                var logContent = string.Empty;
                for(int i = 0; i < logFineLineArray.Length; i++)
                {
                    string line = logFineLineArray[i].Trim();
                    if (string.IsNullOrEmpty(line))
                    {
                        UnityDebugViewerLogger.AddLogFileLog(logContent);
                        logContent = string.Empty;
                    }
                    else
                    {
                        logContent = string.Format("{0}{1}\n", logContent, line);
                    }
                }

                if (!string.IsNullOrEmpty(logContent))
                {
                    UnityDebugViewerLogger.AddLogFileLog(logContent);
                }
            }
        }


        /// <summary>
        /// convert the format of the incoming file path to the format of system file path, and complete the incoming file path if necessary
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static string ConvertToSystemFilePath(string filePath)
        {
            string systemFilePath = filePath.Replace(UnityInternalDirectorySeparator, Path.DirectorySeparatorChar);

            /// Only complete the file path of the log generated by this project 
            if (systemFilePath.StartsWith("Assets" + Path.DirectorySeparatorChar))
            {
                systemFilePath = Path.Combine(Directory.GetCurrentDirectory(), systemFilePath);
            }
            return systemFilePath;
        }

        public static string ConvertToUnityFilePath(string filePath)
        {
            string startStr = "Assets" + UnityInternalDirectorySeparator;
            filePath = filePath.Replace(Path.DirectorySeparatorChar, UnityInternalDirectorySeparator);
            if(filePath.StartsWith(startStr) == false)
            {
                int index = filePath.IndexOf(startStr);
                if(index == -1)
                {
                    return string.Empty;
                }
                else
                {
                    return filePath.Substring(index);
                }
            }
            else
            {
                return filePath;
            }
        }

        /// <summary>
        /// replace \t with four \b to ensure consistent code format
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        private static string ReplaceTabWithSpace(string str)
        {
            return str.Replace("\t", "\b\b\b\b");
        }
    }
}
