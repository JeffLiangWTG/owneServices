using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	class NumberNotInRangeBuilder : FilterBuilder, ICustomBuilder
	{
		public NumberNotInRangeBuilder(ValidatorPack validators, BusinessObjectFactory businessObjectFactory, MatchEvaluator evaluatorForDefaultValues, ReportRunningType runtimeReportStyle)
			: base(validators, businessObjectFactory, evaluatorForDefaultValues, runtimeReportStyle)
		{
			ExpectedProperties.Add(DefaultFrom);
			ExpectedProperties.Add(DefaultTo);
		}

		protected override IReportDocumenter GetDocumentation(List<string> supportedProperties)
		{
			return new FilterBuilderDocumenter((NoResString)"number[ ]not[ ]in[ ]range", Res.GetString("FilterDocumentation|CC668D19-A776-4249-A68F-CB5DB4782700", "Generates a number filter with From and To fields. Filters the data outside of the selected range."), supportedProperties);
		}

		public override bool CanBuild(string filterType)
		{
			return Regex.IsMatch(filterType, @"number\s*not\s*in\s*range", RegexOptions.IgnoreCase);
		}

		protected override FilterField GetFilterField()
		{
			return new NumberNotInRangeField(fBusinessObjectFactory);
		}

		#region ICustomBuilder Members

		public void DoCustomBuilding(StringTreeNode fieldTree, FilterField newField)
		{
			decimal result;
			var convertedField = newField as NumberNotInRangeField;
			if (fieldTree.ChildExists(DefaultFrom) && Decimal.TryParse(fieldTree.FindChild(DefaultFrom).Child().Value, out result))
			{
				convertedField.From = result;
			}
			if (fieldTree.ChildExists(DefaultTo) && Decimal.TryParse(fieldTree.FindChild(DefaultTo).Child().Value, out result))
			{
				convertedField.To = result;
			}
		}

		public void DoCustomBuildingInTaskBuild(StringTreeNode fieldTree, FilterField newField)
		{
		}

		#endregion
	}
}
