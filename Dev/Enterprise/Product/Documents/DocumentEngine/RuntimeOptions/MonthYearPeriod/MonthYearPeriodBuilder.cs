using System.Collections.Generic;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	class MonthYearPeriodBuilder : FilterBuilder, ICustomBuilder
	{
		const string MonthField = FilterBuilderPropertyCodeDescriptionList.Codes.Month;
		const string YearField = FilterBuilderPropertyCodeDescriptionList.Codes.Year;

		public MonthYearPeriodBuilder(ValidatorPack validators, BusinessObjectFactory factory, MatchEvaluator evaluatorForDefaultValues, ReportRunningType reportRunningType)
			: base(validators, factory, evaluatorForDefaultValues, reportRunningType)
		{
			ExpectedProperties.Add(MonthField);
			ExpectedProperties.Add(YearField);
		}

		public override bool CanBuild(string filterType)
		{
			return Regex.IsMatch(filterType, @"^\s*MonthYearPeriod\s*$", RegexOptions.IgnoreCase);
		}

		protected override IReportDocumenter GetDocumentation(List<string> supportedProperties)
		{
			return new FilterBuilderDocumenter((NoResString)"MonthYearPeriod", Res.GetString("FilterDocumentation|187addce-a0e2-4d11-8bf9-28bb684faf0d", "Generates a filter for Month / Year period. Filters data in the selected range."), supportedProperties);
		}

		protected override FilterField GetFilterField()
		{
			return new MonthYearPeriodField(fBusinessObjectFactory);
		}

		void ICustomBuilder.DoCustomBuilding(StringTreeNode fieldTree, FilterField newField)
		{
			doBuilding(fieldTree, newField);
		}

		void ICustomBuilder.DoCustomBuildingInTaskBuild(StringTreeNode fieldTree, FilterField newField)
		{
			doBuilding(fieldTree, newField);
		}

		void doBuilding(StringTreeNode fieldTree, FilterField newField)
		{
			var periodField = (MonthYearPeriodField)newField;

			if (fieldTree.ChildExists(MonthField))
			{
				periodField.Month = ZInt.Parse(fieldTree.FindChild(MonthField).Child().Value);
			}

			if (fieldTree.ChildExists(YearField))
			{
				periodField.Year = ZInt.Parse(fieldTree.FindChild(YearField).Child().Value);
			}
		}
	}
}
