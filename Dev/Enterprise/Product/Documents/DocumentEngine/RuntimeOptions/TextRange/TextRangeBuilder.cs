using System.Collections.Generic;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	class TextRangeBuilder : FilterBuilder, ICustomBuilder
	{
		public TextRangeBuilder(ValidatorPack validators, BusinessObjectFactory businessObjectFactory, MatchEvaluator evaluatorForDefaultValues, ReportRunningType runtimeReportStyle)
			: base(validators, businessObjectFactory, evaluatorForDefaultValues, runtimeReportStyle)
		{
			ExpectedProperties.Add(DefaultFrom);
			ExpectedProperties.Add(DefaultTo);
		}

		protected override IReportDocumenter GetDocumentation(List<string> supportedProperties)
		{
			return new FilterBuilderDocumenter((NoResString)"text[ ]range", Res.GetString("FilterDocumentation|4F15ADAB-F890-4F7C-9681-48256531BD3A", "Generates a text filter with From and To fields. Filters data in the selected range."), supportedProperties);
		}

		public override bool CanBuild(string filterType)
		{
			return Regex.IsMatch(filterType, @"text\s*range", RegexOptions.IgnoreCase);
		}

		protected override FilterField GetFilterField()
		{
			return new TextRangeField(fBusinessObjectFactory);
		}

		public void DoCustomBuilding(StringTreeNode fieldTree, FilterField newField)
		{
			doBuilding(fieldTree, newField);
		}

		public void DoCustomBuildingInTaskBuild(StringTreeNode fieldTree, FilterField newField)
		{
			doBuilding(fieldTree, newField);
		}

		void doBuilding(StringTreeNode fieldTree, FilterField newField)
		{
			var textRangeField = newField as TextRangeField;
			textRangeField.From = ExtractFromTree(fieldTree, newField, DefaultFrom);
			textRangeField.To = ExtractFromTree(fieldTree, newField, DefaultTo);
		}

		string ExtractFromTree(StringTreeNode fieldTree, FilterField newField, string where)
		{
			return (fieldTree.ChildExists(where)) ? fieldTree.FindChild(where).Child().Value : "";
		}
	}
}
