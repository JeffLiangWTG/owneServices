using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.ComponentModel;

namespace Enterprise.ResourceStrings.Business
{
	public static class ParameterConsistencyChecker
	{
		public static void Check(string source, string translation, INotifications notifications)
		{
			var sourceMatches = ParamsRegex.Matches(source);
			var translationMatches = ParamsRegex.Matches(translation);
			var sourceParameters = new HashSet<string>(sourceMatches.Cast<Match>().Select(match => GetMatchValue(match)));
			var translationParameters = new HashSet<string>(translationMatches.Cast<Match>().Select(match => GetMatchValue(match)));
			if (!sourceParameters.SetEquals(translationParameters))
			{
				notifications.AddError("The translation does not have the same set of parameters as the English");
			}
			if (translationParameters.Count > 0)
			{
				try
				{
					var numArgs = translationMatches.Cast<Match>().Max(match => int.Parse(match.Groups["num"].Value));
					var args = new object[numArgs + 1];
					for (int i = 0; i < args.Length; i++)
					{
						args[i] = "";
					}
					string.Format(translation, args);
				}
				catch (FormatException)
				{
					notifications.AddError("Formatting error");
				}
			}
			else
			{
				// Even with no valid parameters, if the source can be passed through string.Format() then the translation should also be able to pass through string.Format()
				try
				{
					string.Format(source);
					try
					{
						string.Format(translation);
					}
					catch (FormatException)
					{
						notifications.AddError("Formatting error");
					}
				}
				catch (FormatException)
				{ }
			}
		}

		internal static string GetMatchValue(Match match)
		{
			string value = match.Value;
			var caseGroup = match.Groups["case"];
			if (caseGroup.Success)
			{
				value = value.Remove(caseGroup.Index - match.Index, caseGroup.Length);
			}
			return value;
		}

		internal static Regex ParamsRegex
		{
			get { return paramsRegex ?? (paramsRegex = new Regex(@"\{(?<num>[0-9]+)(?<case>\:[lLuUtT])?.*?\}", RegexOptions.Compiled | RegexOptions.Multiline)); }
		}

		[ThreadStatic]
		static Regex paramsRegex;
	}
}
