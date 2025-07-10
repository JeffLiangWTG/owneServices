using System;
using System.Text;
using System.Text.RegularExpressions;
using CargoWiseOne.ResourceStrings;
using Enterprise.DocumentEngine.Build;
using Enterprise.ResourceStrings.Business;

namespace Enterprise.DocumentEngine.DocBuilder
{
	public static class DocBuilderResourceStrings
	{
		public static string GetString(TemplateSection section, string translatableText)
		{
			var resourceString = Res._GetData(DocLabelAsmid, GetKey(section, translatableText), translatableText);
			return GetTranslationString(resourceString, translatableText);
		}

		public static string GetLocalizedReportName(string translatableText)
		{
			var resourceString = Res._GetData(ReportNamesAsmid, ReportNameKeyPrefix + translatableText, translatableText);
			return GetTranslationString(resourceString, translatableText);
		}

		public static string GetLocalizedReportTitle(string translatableText)
		{
			var resourceString = Res._GetData(ReportTitleAsmid, ReportTitleKeyPrefix + translatableText, translatableText);
			return GetTranslationString(resourceString, translatableText);
		}

		public static string GetReportString(string templateName, string translatableText)
		{
			return string.IsNullOrEmpty(templateName) ? translatableText : GetTranslationString(GetReportStringData(templateName, translatableText), translatableText);
		}

		public static string GetLegacyDocumentString(string templateName, string translatableText)
		{
			return string.IsNullOrEmpty(templateName) ? translatableText : GetTranslationString(GetLegacyStringData(templateName, translatableText), translatableText);
		}

		public static ResourceStringData GetLegacyStringData(string templateName, string translatbaleText)
		{
			var legacyDocumentLabelAsmid = TemplateSerializationHelper.GetAsmidByTemplatePath(templateName);
			return Res._GetData(legacyDocumentLabelAsmid, GetKey(templateName, LegacyDocLabelKeyPrefix, translatbaleText), translatbaleText);
		}

		public static ResourceStringData GetReportStringData(string templateName, string translatableText)
		{
			return Res._GetData(ReportLabelAsmid, GetKey(templateName, ReportLabelKeyPrefix, translatableText), translatableText);
		}

		public static string GetCoverSheetString(string templateName, string translatableText)
		{
			var resourceString = Res._GetData(CoverSheetLabelAsmid, GetKey(templateName, CoverSheetLabelKeyPrefix, translatableText), translatableText);
			return string.IsNullOrEmpty(templateName) ? translatableText : GetTranslationString(resourceString, translatableText);
		}

		public static ResourceStringData GetDocLabelStringData(string translatableText)
		{
			return Res._GetData(DocLabelAsmid, GetKey("", DocLabelKeyPrefix, translatableText), translatableText);
		}

		public static string GetTranslationString(ResourceStringData resourceString, string translatableText)
		{
			string result;
			if (resourceString != null)
			{
				if (!string.IsNullOrEmpty(resourceString.Caption))
				{
					result = resourceString.Caption;
				}
				else if (!string.IsNullOrEmpty(resourceString.FullDescription))
				{
					result = resourceString.FullDescription;
				}
				else
				{
					result = translatableText;
				}
			}
			else
			{
				result = translatableText;
			}
			if (Res.IsRightToLeft(Res.CurrentLanguage) && result == translatableText)
			{
				result = TransformRightToLeft(translatableText);
			}
			return result;
		}

		public static string GetKey(TemplateSection section, string translatableText)
		{
			var sectionName = "";
			if (!translatableText.ContainsLetters() && section != null)
			{
				sectionName = section.SectionName;
			}
			return GetKey(sectionName, DocLabelKeyPrefix, translatableText);
		}

		public static string GetKey(string groupName, string keyPrefix, string translatableText)
		{
			var keyText = Convert.ToBase64String(Encoding.UTF8.GetBytes(translatableText));
			if (!string.IsNullOrEmpty(groupName))
			{
				groupName += "|";
			}
			if (keyText.Length + groupName.Length + keyPrefix.Length >= HelpDataString.Schema.HD_CodeMaxLength)
			{
				keyText = keyText.Substring(0, HelpDataString.Schema.HD_CodeMaxLength - groupName.Length - keyPrefix.Length);
			}
			return keyPrefix + groupName + keyText;
		}

		static string TransformRightToLeft(string text)
		{
			var match = RightToLeftRegex.Match(text);
			if (match.Success)
			{
				text = "";
				var p = match.Groups["p"];
				var w = match.Groups["w"];
				for (int i = p.Captures.Count - 1; i >= 0; i--)
				{
					text += p.Captures[i].Value;
					if (i > 0)
					{
						text += w.Captures[i - 1].Value;
					}
				}
			}
			return text;
		}

		static Regex RightToLeftRegex
		{
			get { return rightToLeftRegex ?? (rightToLeftRegex = new Regex(@"^((?<p>\{[0-9]+.*?\})(?<w>\W*))+$", RegexOptions.Compiled)); }
		}
		static Regex rightToLeftRegex;

		public const string DocLabelKeyPrefix = "DocLabel|";
		public const string LegacyDocLabelKeyPrefix = "LegacyDocLabel|";
		public const string ReportNameKeyPrefix = "ReportName|";
		public const string ReportLabelKeyPrefix = "ReportLabel|";
		public const string ReportTitleKeyPrefix = "ReportTitle|";
		public const string CoverSheetLabelKeyPrefix = "CoverSheetLabel|";

		public const UInt16 DocLabelAsmid = 1;
		public const UInt16 ReportNamesAsmid = 2;
		public const UInt16 ReportTitleAsmid = 4;
		public const UInt16 ReportLabelAsmid = 5;
		public const UInt16 LegacyDocumentLabelAsmid = 6;
		public const UInt16 EDILegacyDocumentLabelAsmid = 7;
		public const UInt16 CoverSheetLabelAsmid = 8;
	}
}
