using System;
using System.Text.RegularExpressions;
using CargoWise.Types;

namespace Enterprise.DocumentEngine.DocBuilder
{
	public static class SectionRepositoryTemplateNames
	{
		public const string System = Enterprise.Core.Constants.SectionRepositoryTemplateNames.System;
		public const string User = Enterprise.Core.Constants.SectionRepositoryTemplateNames.User;

		internal static ZString GetLanguageSpecificTemplateName(ZString type, ZString languageCode)
		{
			return languageCode.IsEmpty ? "" : type + " [" + languageCode + "]";
		}

		internal static string GetLanguageCodeFromTemplateName(string templateName)
		{
			var result = Enterprise.Core.Constants.Languages.English;

			var match = NameChecker.Match(templateName);
			if (match.Success)
			{
				var group = match.Groups["LanguageCode"];
				if (!string.IsNullOrEmpty(group?.Value))
				{
					result = group.Value;
				}
			}

			return result;
		}

		internal static Regex NameChecker
		{
			get { return nameChecker ?? (nameChecker = new Regex("^(" + System + "|" + User + @")(| " + languageCodeRegex + ")$", RegexOptions.Compiled | RegexOptions.CultureInvariant)); }
		}

		[ThreadStatic]
		static Regex nameChecker;

		internal static Regex SystemNameChecker
		{
			get { return systemNameChecker ?? (systemNameChecker = new Regex("^(" + System + @")(| " + languageCodeRegex + ")$", RegexOptions.Compiled | RegexOptions.CultureInvariant)); }
		}

		[ThreadStatic]
		static Regex systemNameChecker;

		internal static Regex UserNameChecker
		{
			get { return userNameChecker ?? (userNameChecker = new Regex("^(" + User + @")(| " + languageCodeRegex + ")$", RegexOptions.Compiled | RegexOptions.CultureInvariant)); }
		}

		const string languageCodeRegex = @"\[(?<LanguageCode>[A-Z]{2,3}(-[A-Z]{2,3})?)\]";

		[ThreadStatic]
		static Regex userNameChecker;
	}
}
