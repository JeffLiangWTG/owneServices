using System;
using System.Text.RegularExpressions;

namespace CargoWise.EntityFramework
{
	internal class OrDetector
	{
		public bool ContainsOrOperator(string textToCheck)
		{
			return ContainsOrRegex.IsMatch(textToCheck);
		}

		static Regex ContainsOrRegex
		{
			get
			{
				if (fContainsOrRegex == null)
				{
					string pattern = @"\bOR\b";
					RegexOptions options = (RegexOptions.Compiled | RegexOptions.IgnoreCase);
					fContainsOrRegex = new Regex(pattern, options);
				}
				return fContainsOrRegex;
			}
		}
		[ThreadStatic]
		static Regex fContainsOrRegex;
	}
}
