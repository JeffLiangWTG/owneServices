using System.Collections.Generic;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	internal class AccountingNumberRangeBuilder : FilterBuilder, ICustomBuilder
	{
		public AccountingNumberRangeBuilder(ValidatorPack validators, BusinessObjectFactory businessObjectFactory, MatchEvaluator evaluatorForDefaultValues, ReportRunningType runtimeReportStyle)
			: base(validators, businessObjectFactory, evaluatorForDefaultValues, runtimeReportStyle)
		{
			ExpectedProperties.Add(DefaultFrom);
			ExpectedProperties.Add(DefaultTo);
		}

		protected override IReportDocumenter GetDocumentation(List<string> supportedProperties)
		{
			return new FilterBuilderDocumenter((NoResString)"accounting[ ]no.[ ]range", Res.GetString("FilterDocumentation|AB0AB36C-0ED3-47E3-8CF7-404B9D86FE00", "Generates an accounting-number text filter with a From and To fields, whose values are strings (not numbers). Filters the data within this range."), supportedProperties);
		}

		public override bool CanBuild(string filterType)
		{
			return Regex.IsMatch(filterType, @"accounting\s*No.\s*range", RegexOptions.IgnoreCase);
		}

		protected override FilterField GetFilterField()
		{
			return new AccountingNumberRangeField(fBusinessObjectFactory);
		}

		#region ICustomBuilder Members

		public void DoCustomBuilding(StringTreeNode fieldTree, FilterField newField)
		{
			int result;
			var field = (AccountingNumberRangeField)newField;
			if (fieldTree.ChildExists(DefaultFrom) && int.TryParse(fieldTree.FindChild(DefaultFrom).Child().Value, out result))
			{
				field.From = fieldTree.FindChild(DefaultFrom).Child().Value;
			}
			if (fieldTree.ChildExists(DefaultTo) && int.TryParse(fieldTree.FindChild(DefaultTo).Child().Value, out result))
			{
				field.To = fieldTree.FindChild(DefaultTo).Child().Value;
			}
		}

		public void DoCustomBuildingInTaskBuild(StringTreeNode fieldTree, FilterField newField)
		{
		}

		#endregion
	}
}
