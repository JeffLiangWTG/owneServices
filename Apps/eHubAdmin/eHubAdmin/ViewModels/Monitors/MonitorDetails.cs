extern alias Sys;
using System;
using System.Collections.Generic;
using eServices.eHubDataModel.eHubTransactions;
using Sys::System.ComponentModel.DataAnnotations;

namespace eServices.eHubAdmin.ViewModels.Monitors
{
	public class MonitorDetails
	{
		public eHubMonitor eHubMonitor { get; set; }
		public MonitorCounts MonitorCounts { get; set; }
		public string DateRangeType { get; set; }
		public IEnumerable<MonitorCountDetail> MonitorCountDetails { get; set; }

		[DataType(DataType.Time)]
		[DisplayFormat(DataFormatString = @"{0:hh\:mm}", ApplyFormatInEditMode = true)]
		public TimeSpan? MaintenanceStartTime
		{
			get
			{
				return eHubMonitor.MO_MaintenanceStartTime;
			}
			set
			{
				eHubMonitor.MO_MaintenanceStartTime = value;
			}
		}

		[DataType(DataType.Time)]
		[DisplayFormat(DataFormatString = @"{0:hh\:mm}", ApplyFormatInEditMode = true)]
		public TimeSpan? MaintenanceEndTime
		{
			get
			{
				return eHubMonitor.MO_MaintenanceEndTime;
			}
			set
			{
				eHubMonitor.MO_MaintenanceEndTime = value;
			}
		}

		public int? MaintenanceStartDayOfWeek
		{
			get
			{
				return eHubMonitor.MO_MaintenanceStartDayOfWeek;
			}
			set
			{
				eHubMonitor.MO_MaintenanceStartDayOfWeek = value;
			}
		}
		public int? MaintenanceEndDayOfWeek
		{
			get
			{
				return eHubMonitor.MO_MaintenanceEndDayOfWeek;
			}
			set
			{
				eHubMonitor.MO_MaintenanceEndDayOfWeek = value;
			}
		}
	}
}
