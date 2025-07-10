using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	class NumberRangeBuilder : FilterBuilder, ICustomBuilder
	{
		public NumberRangeBuilder(ValidatorPack validators, BusinessObjectFactory businessObjectFactory, MatchEvaluator evaluatorForDefaultValues, ReportRunningType runtimeReportStyle)
			: base(validators, businessObjectFactory, evaluatorForDefaultValues, runtimeReportStyle)
		{
			ExpectedProperties.Add(DefaultFrom);
			ExpectedProperties.Add(DefaultTo);
		}

		protected override IReportDocumenter GetDocumentation(List<string> supportedProperties)
		{
			return new FilterBuilderDocumenter((NoResString)"number[ ]range", Res.GetString("FilterDocumentation|65B88881-CDDA-4623-84C1-093C8E13C95C", "Generates a number filter with From and To fields. Filters the data in the selected range."), supportedProperties);
		}

		public override bool CanBuild(string filterType)
		{
			return Regex.IsMatch(filterType, @"number\s*range", RegexOptions.IgnoreCase);
		}

		protected override FilterField GetFilterField()
		{
			return new NumberRangeField(fBusinessObjectFactory);
		}

		#region ICustomBuilder Members

		public void DoCustomBuilding(StringTreeNode fieldTree, FilterField newField)
		{
			decimal result;
			if (fieldTree.ChildExists(DefaultFrom) && Decimal.TryParse(fieldTree.FindChild(DefaultFrom).Child().Value, out result))
			{
				((NumberRangeField)(newField)).From = result;
			}
			if (fieldTree.ChildExists(DefaultTo) && Decimal.TryParse(fieldTree.FindChild(DefaultTo).Child().Value, out result))
			{
				((NumberRangeField)(newField)).To = result;
			}
		}

		public void DoCustomBuildingInTaskBuild(StringTreeNode fieldTree, FilterField newField)
		{
		}

		#endregion
	}
}
