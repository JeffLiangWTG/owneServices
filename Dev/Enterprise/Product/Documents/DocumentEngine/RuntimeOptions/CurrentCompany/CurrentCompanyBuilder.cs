using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	internal class CurrentCompanyBuilder : FilterBuilder, ICustomBuilder
	{
		public CurrentCompanyBuilder(ValidatorPack validators, BusinessObjectFactory businessObjectFactory, MatchEvaluator evaluatorForDefaultValues, ReportRunningType runtimeReportStyle)
			: base(validators, businessObjectFactory, evaluatorForDefaultValues, runtimeReportStyle)
		{
		}

		protected override IReportDocumenter GetDocumentation(List<string> supportedProperties)
		{
			return new FilterBuilderDocumenter((NoResString)"Current Company", Res.GetString("FilterDocumentation|54988525-B1A4-4AA3-960E-63C639387BA2", "Generates a hidden filter whose value is the current company. Filters data based on that company."), supportedProperties);
		}

		public override bool CanBuild(string filterType) =>
		filterType.Equals((NoResString)"current company", StringComparison.OrdinalIgnoreCase);

		protected override FilterField GetFilterField()
		{
			return new CurrentCompanyField(fBusinessObjectFactory);
		}

		#region ICustomerBuilder Implement

		public void DoCustomBuilding(StringTreeNode fieldTree, FilterField newField)
		{
			((CurrentCompanyField)newField).CreateParameters();
		}

		public void DoCustomBuildingInTaskBuild(StringTreeNode fieldTree, FilterField newField)
		{
		}

		#endregion
	}
}
