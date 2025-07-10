
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace CargoWise.Common
{
	public class RegexMatcher : IMaskMatcher
	{
		public string GetMatch(IEnumerable<string> masks, string inputString)
		{
			foreach (var mask in masks)
			{
				var currentRegex = new Regex(mask);
				var numGroups = currentRegex.GetGroupNumbers().Length;

				switch (numGroups)
				{
					case 1:
					case 2:
						var result = currentRegex.Match(inputString).Groups[numGroups - 1].Value;
						if (!string.IsNullOrEmpty(result))
						{
							return result;
						}

						break;
					default:
						throw new ArgumentException("masks must be regular expressions with either no groups or only one group", nameof(masks));
				}
			}

			return inputString;
		}
	}
}
