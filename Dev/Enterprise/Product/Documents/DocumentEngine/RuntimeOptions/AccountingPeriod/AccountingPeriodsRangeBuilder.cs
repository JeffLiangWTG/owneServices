using System.Collections.Generic;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	class AccountingPeriodsRangeBuilder : FilterBuilder, ICustomBuilder
	{
		const string RequireBothFromAndToPeriods = FilterBuilderPropertyCodeDescriptionList.Codes.RequireBothFromAndToPeriods;

		public AccountingPeriodsRangeBuilder(ValidatorPack validators, BusinessObjectFactory businessObjectFactory, MatchEvaluator evaluatorForDefaultValues, ReportRunningType runtimeReportStyle)
			: base(validators, businessObjectFactory, evaluatorForDefaultValues, runtimeReportStyle)
		{
			ExpectedProperties.Add(RequireBothFromAndToPeriods);
		}

		protected override IReportDocumenter GetDocumentation(List<string> supportedProperties)
		{
			return new FilterBuilderDocumenter((NoResString)"Period [Accounting ]Range", Res.GetString("FilterDocumentation|63730550-B44B-4ABD-84ED-3106D81B20AB", "Generates an accounting period range control with from and to fields to type in."), supportedProperties);
		}

		public override bool CanBuild(string filterType)
		{
			return Regex.IsMatch(filterType, @"^Period (Accounting )?Range$", RegexOptions.IgnoreCase);
		}

		protected override FilterField GetFilterField()
		{
			return new AccountingPeriodsRangeField(fBusinessObjectFactory);
		}

		public void DoCustomBuilding(StringTreeNode fieldTree, FilterField newField)
		{
			var accountingPeriodsRangeField = (AccountingPeriodsRangeField)newField;
			((AccountingPeriodsRangeField)(newField)).RequireBothFromAndToPeriods = fieldTree.ChildExists(RequireBothFromAndToPeriods);
		}

		public void DoCustomBuildingInTaskBuild(StringTreeNode fieldTree, FilterField newField)
		{
		}
	}
}
