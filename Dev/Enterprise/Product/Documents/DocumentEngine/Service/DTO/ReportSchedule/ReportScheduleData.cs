
using System.Collections.Generic;

namespace Enterprise.DocumentEngine
{
	public class ReportScheduleData
	{
		public ReportScheduleTaskData ScheduleTask { get; set; }
		public SelectedValueReportData SelectedValueReportData { get; set; }
		public List<ReportScheduleRecipientData> Recipients { get; set; }
		public List<ConfigurationData> Configurations { get; set; }
		public ReportData ReportData { get; set; }
	}
}
