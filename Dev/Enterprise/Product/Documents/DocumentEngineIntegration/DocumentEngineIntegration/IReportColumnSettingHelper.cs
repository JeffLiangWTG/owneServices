using System;
using System.Collections.Generic;

namespace Enterprise.DocumentEngineIntegration
{
	public interface IReportColumnSettingHelper
	{
		void UpdateReportColumnSettingSheetNames(List<ReportColumnSettingSheetNameUpdateParameter> sheetNameUpdateConfig);
	}

	public class ReportColumnSettingSheetNameUpdateParameter
	{
		public ReportColumnSettingSheetNameUpdateParameter(Guid reportId, Dictionary<string, string> modifiedSheetNameMapping)
		{
			ReportId = reportId;
			ModifiedSheetNameMapping = modifiedSheetNameMapping;
		}

		public readonly Guid ReportId;

		public readonly Dictionary<string, string> ModifiedSheetNameMapping;
	}
}
