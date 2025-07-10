using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	class ColumnConfigurationFilterBuilder : FilterBuilder
	{
		public ColumnConfigurationFilterBuilder(ValidatorPack validators, BusinessObjectFactory factory, MatchEvaluator evaluatorForDefaultValues, ColumnConfigurationsManager manager, ReportRunningType runtimeReportStyle)
			: base(validators, factory, evaluatorForDefaultValues, runtimeReportStyle)
		{
			this.Manager = manager;
		}
		readonly ColumnConfigurationsManager Manager;

		protected override IReportDocumenter GetDocumentation(List<string> supportedProperties)
		{
			return new FilterBuilderDocumenter((NoResString)"Column Configuration", Res.GetString("FilterDocumentation|B0D12076-88D6-4F1C-95B7-E1B3E4F85F5D", "Generates the column configuration control. This is a system-reserved filter and should not be used."), supportedProperties);
		}

		public override bool CanBuild(string filterType) =>
		filterType.Equals((NoResString)"column configuration", StringComparison.OrdinalIgnoreCase);

		protected override FilterField GetFilterField()
		{
			ColumnConfigurationField field = new ColumnConfigurationField(this.Factory);
			((IColumnHeadingManagerListener)field).SetManager(Manager);
			return field;
		}
	}
}
