using CargoWise.EntityFramework;
using CargoWise.Types;
using Res = RemotePrinting.Server.Res;

namespace Enterprise.RemotePrinting.Server.Business
{
	public class ClientLogRetrievalInfo : NonPersistentBusinessObject, IObsoleteValidation
	{
		#region FromDate

		public ZDateTime FromDate
		{
			get => fromDate;
			set
			{
				SetNonPersistentPropertyValue(FromDateInfo, ref fromDate, value);
				if (!IsValidationSuspended)
				{
					ValidateFromDate();
				}
			}
		}
		ZDateTime fromDate;

		public ZPropertyInfo FromDateInfo
		{
			get { return GetZPropertyInfo(nameof(FromDate)); }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
		void ValidateFromDate()
		{
			FromDateInfo.ClearAllNotifications();

			if (FromDate.IsEmpty)
			{
				FromDateInfo.AddError("Enter From Date.");
			}
			if (!FromDate.IsEmpty && !ToDate.IsEmpty && FromDate > ToDate)
			{
				FromDateInfo.AddError("From Date should not be after To Date.");
			}
		}

		#endregion

		#region ToDate

		public ZDateTime ToDate
		{
			get => toDate;
			set
			{
				SetNonPersistentPropertyValue(ToDateInfo, ref toDate, value);
				if (!IsValidationSuspended)
				{
					ValidateToDate();
				}
			}
		}
		ZDateTime toDate;

		public ZPropertyInfo ToDateInfo
		{
			get { return GetZPropertyInfo(nameof(ToDate)); }
		}

		void ValidateToDate()
		{
			ToDateInfo.ClearAllNotifications();

			if (ToDate.IsEmpty)
			{
				ToDateInfo.AddError(Res.GetString("60a59460-5cd6-4add-a6b3-6a1351f0c2d4", "Enter To Date."));
			}
			if (!ToDate.IsEmpty && !FromDate.IsEmpty && ToDate < FromDate)
			{
				ToDateInfo.AddError(Res.GetString("661f8df0-48a0-4cb4-a5bb-b2da316234b6", "To Date should not be before From Date."));
			}
		}

		#endregion

		#region Logs

		public ZBool Logs
		{
			get => logs;
			set
			{
				SetNonPersistentPropertyValue(LogsInfo, ref logs, value);
				if (!IsValidationSuspended)
				{
					ValidateLogs();
				}
			}
		}
		ZBool logs;

		public ZPropertyInfo LogsInfo
		{
			get { return GetZPropertyInfo(nameof(Logs)); }
		}

		void ValidateLogs()
		{
			LogsInfo.ClearAllNotifications();

			if (!Logs && !ServiceLogs && !InstallLogs && !WindowsEvents)
			{
				LogsInfo.AddError(Res.GetString("dc49d2cb-9045-4371-94cd-f65a6bf06f39", "Select log types to request."));
			}
		}

		public ZBool ServiceLogs
		{
			get => serviceLogs;
			set
			{
				SetNonPersistentPropertyValue(ServiceLogsInfo, ref serviceLogs, value);
				if (!IsValidationSuspended)
				{
					ValidateLogs();
				}
			}
		}
		ZBool serviceLogs;

		public ZPropertyInfo ServiceLogsInfo
		{
			get { return GetZPropertyInfo(nameof(ServiceLogs)); }
		}

		public ZBool InstallLogs
		{
			get => installLogs;
			set
			{
				SetNonPersistentPropertyValue(InstallLogsInfo, ref installLogs, value);
				if (!IsValidationSuspended)
				{
					ValidateLogs();
				}
			}
		}
		ZBool installLogs;

		public ZPropertyInfo InstallLogsInfo
		{
			get { return GetZPropertyInfo(nameof(InstallLogs)); }
		}

		public ZBool WindowsEvents
		{
			get => windowsEvents;
			set
			{
				SetNonPersistentPropertyValue(WindowsEventsInfo, ref windowsEvents, value);
				if (!IsValidationSuspended)
				{
					ValidateLogs();
				}
			}
		}
		ZBool windowsEvents;

		public ZPropertyInfo WindowsEventsInfo
		{
			get { return GetZPropertyInfo(nameof(WindowsEvents)); }
		}

		#endregion

		#region EmailAddress

		public ZString EmailAddress
		{
			get => emailAddress;
			set
			{
				SetNonPersistentPropertyValue(EmailAddressInfo, ref emailAddress, value);
				if (!IsValidationSuspended)
				{
					ValidateEmailAddress();
				}
			}
		}
		ZString emailAddress;

		public ZPropertyInfo EmailAddressInfo
		{
			get { return GetZPropertyInfo(nameof(EmailAddress)); }
		}

		void ValidateEmailAddress()
		{
			EmailAddressInfo.ClearAllNotifications();

			if (EmailAddress.IsEmpty)
			{
				EmailAddressInfo.AddError(Res.GetString("740f8d29-f3f2-499b-af5a-2f364446c9e6", "Enter Email Address."));
			}
		}

		#endregion

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();

			ValidateFromDate();
			ValidateToDate();
			ValidateEmailAddress();
			ValidateLogs();
		}
	}
}
