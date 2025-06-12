extern alias Sys;
using System;
using System.Web.Mvc;
using Sys::System.ComponentModel.DataAnnotations;

namespace eServices.eHubAdmin.ViewModels.Monitors
{
	public class MonitorEdit
	{
		[Required] public string MO_ID { get; set; }

		[Display(Name = "Error Delay Minutes"),
		 Range(1, int.MaxValue, ErrorMessage = "Delay must be a positive integer.")]
		public int ErrorDelayMins { get; set; }

		public bool Enabled { get; set; }
		[AllowHtml] public string Notes { get; set; }

		[DataType(DataType.Time)]
		[DisplayFormat(DataFormatString = @"{0:hh\:mm}", ApplyFormatInEditMode = true)]
		public TimeSpan? MaintenanceStartTime { get; set; }

		[DataType(DataType.Time)]
		[DisplayFormat(DataFormatString = @"{0:hh\:mm}", ApplyFormatInEditMode = true)]
		public TimeSpan? MaintenanceEndTime { get; set; }

		[Display(Name = "From")]
		public int? MaintenanceStartDayOfWeek { get; set; }

		[Display(Name = "To")]
		public int? MaintenanceEndDayOfWeek { get; set; }

		public string DateRangeType { get; set; }

		[Display(Name = "Disable Maintenance")]
		public bool DisableMaintenance { get; set; }
	}
}
