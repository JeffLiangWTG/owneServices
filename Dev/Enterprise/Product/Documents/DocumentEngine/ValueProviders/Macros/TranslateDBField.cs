using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.ResourceStrings.Cache;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.MacroValueProviders
{
	class TranslateDBField : ValueProviderWithLoadControlFactory
	{
		protected override ValueProviderDocumenter GetDocumentation()
		{
			return new ValueProviderDocumenter("<TranslateDBField({DB Field Name},{English Text})>",
				ResString.GetMultilingualString("1a477f26-1950-49cc-962a-75b119888429", @"Translate the English text stored in a particular DB field to the selected language in the report filter. You can specify the Business Object name before the field name to load the Business Object if there are multiple types referencing to the same database field."),
				new List<(string example, object expectedResult)> {
					("<TranslateDBField(AG_Description, Description)>", (NoResString)"Noitpircsed"),
					("<TranslateDBField(StmMenuItem.SU_MenuName, English Text)>", (NoResString)"English Text") });
		}

		protected override object GetReplacementCore(string macro, Report report)
		{
			var match = Regex.Match(macro);
			var dbField = match.Groups[1].Value;

			var dbFieldName = dbField;
			var boName = string.Empty;

			if (dbField.IndexOf(".", StringComparison.Ordinal) != -1)
			{
				var dbFields = dbField.Split('.');
				boName = dbFields[0];
				dbFieldName = dbFields[1];
			}

			var englishText = match.Groups[2].Value;
			object result = englishText;

			var language = "";
			if (report.FilterCollection["Translation Language"] != null && report.FilterCollection["Translation Language"].ValueAsObject != null)
			{
				language = report.FilterCollection["Translation Language"].ValueAsObject.ToString();
			}
			else
			{
				language = report.Language;
			}

			var translatbleDataFieldAttribute = TranslatableDataFieldAttribute.GetAttributeForColumn(dbFieldName, boName);
			if (translatbleDataFieldAttribute != null)
			{
				englishText = englishText.Replace("\\>", ">").Replace("\\<", "<");
				var dataString = CustomizableDataResourceStrings.GetMultilingualString(translatbleDataFieldAttribute, null, englishText);
				if (dataString != null && !string.IsNullOrEmpty(language))
				{
					result = dataString.ToString(language).Replace(">", "\\>").Replace("<", "\\<");
				}
			}
			return result;
		}

		protected override bool PassNestedMacroFormulaAsText { get { return true; } }

		public override Regex Regex
		{
			get { return fRegex; }
		}

		static readonly Regex fRegex = new Regex(@"^<(?:[\s]*)TranslateDBField(?:[\s]*)\((?:[\s]*)(.*?)(?:[\s]*),(?:[\s]*)(.*?)(?:[\s]*)\)(?:[\s]*)>$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
	}
}
