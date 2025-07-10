using System;
using System.Data;
using System.Linq;

namespace Enterprise.Build.Database.Script.Public.Accounting.Export.Testing
{
	static class BatchAndSingleTestHelper
	{
		internal static string GetScriptLinesToCompare(string script, string[] excludeLines)
		{
			const string commentBlockMarker = "--------------";
			const string newline = "\r\n";
			var newlineArray = new[] { newline };

			var scriptLines = script.Split(newlineArray, StringSplitOptions.None).Select((text, idx) => (text, idx)).ToArray();
			if (scriptLines.Count(l => l.text.StartsWith(commentBlockMarker)) != 4)
			{
				throw new Exception("Precondition: must be exactly 4 comment lines forming two blocks which identify the core SELECT query");
			}
			var startOfTopCommentBlockIdx = scriptLines.First(l => l.text.StartsWith(commentBlockMarker)).idx;
			var endOfTopCommentBlockIdx = scriptLines.Skip(startOfTopCommentBlockIdx + 1).First(l => l.text.StartsWith(commentBlockMarker)).idx;

			var singleScriptLinesReversed = scriptLines.Reverse().ToArray();
			var endOfBottomCommentBlockIdx = singleScriptLinesReversed.First(l => l.text.StartsWith(commentBlockMarker)).idx;
			var startOfBottomCommentBlockIdx = singleScriptLinesReversed.Skip(scriptLines.Length - endOfBottomCommentBlockIdx + 1).First(l => l.text.StartsWith(commentBlockMarker)).idx;

			var scriptCoreLines = scriptLines.Where(l => l.idx > endOfTopCommentBlockIdx && l.idx < startOfBottomCommentBlockIdx);
			var scriptCoreLinesToCompare = string.Join(newline, scriptCoreLines.Where(l => !excludeLines.Contains(l.text)).Select(l => l.text));
			if (string.IsNullOrWhiteSpace(scriptCoreLinesToCompare))
			{
				throw new Exception("Precondition: core SELECT query cannot be empty");
			}
			return scriptCoreLinesToCompare;
		}
	}
}

