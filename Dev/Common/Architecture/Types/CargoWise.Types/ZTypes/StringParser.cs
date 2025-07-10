using System.Collections.Generic;
using CargoWise.Common;

namespace CargoWise.Types
{
	public static class StringParser
	{
		public static string[] TrimBlankLinesAndNewLines(IEnumerable<string> lines)
		{
			Argument.NotNull(lines, nameof(lines));
			List<string> result = new List<string>();
			foreach (string line in lines)
			{
				if (line != null && !string.IsNullOrEmpty(line.Trim()))
				{
					result.Add(line.Replace("\r", "").Replace("\n", ""));
				}
			}
			return result.ToArray();
		}
	}
}
