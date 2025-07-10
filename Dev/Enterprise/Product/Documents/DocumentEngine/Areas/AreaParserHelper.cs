using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.DocumentEngine.Areas
{
	public static class AreaHelper
	{
		public static string[] SplitParametersForSectionBody(string parameterText)
		{
			var result = new List<string>();
			var currentParam = new ZStringBuilder();
			var insideQuotes = false;
			var parenthesisLevel = 0;
			foreach (char currentChar in parameterText)
			{
				if ((currentChar == ':' || currentChar == ',') && !(insideQuotes || parenthesisLevel > 0))
				{
					result.Add(currentParam.ToString());
					currentParam = new ZStringBuilder();
				}
				else
				{
					currentParam.Append(currentChar.ToString());
					if (currentChar == '"')
					{
						insideQuotes = !insideQuotes;
					}
					if (currentChar == '(')
					{
						parenthesisLevel++;
					}
					if (currentChar == ')')
					{
						parenthesisLevel--;
					}
				}
			}
			result.Add(currentParam.ToString());
			return result.ToArray();
		}
	}
}