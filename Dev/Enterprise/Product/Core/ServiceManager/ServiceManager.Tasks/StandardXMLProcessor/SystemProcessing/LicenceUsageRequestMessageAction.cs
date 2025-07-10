using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.eHubMessaging.Business;
using Enterprise.Licensing;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.ServiceManager.Tasks.StandardXMLProcessor
{
	public sealed class LicenceUsageRequestMessageAction : IMessageAction
	{
		[SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "factoryProvider")]
		public LicenceUsageRequestMessageAction(BusinessObjectFactoryProvider factoryProvider)
		{
		}

		bool IMessageAction.ExecuteAction(EDIMessage message, INotifications notifications, out List<ITransactionParticipant> participants)
		{
			participants = new List<ITransactionParticipant>(0);

			LicenceUsageRequestInfo info = null;
			try
			{
				info = new LicenceUsageRequestInfo(message.EM_MessageText);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				notifications.AddWarning("Invalid Usage Request");
				return true;
			}

			if (info != null)
			{
				ProcessRequest(info.DateFrom, info.DateTo, info.RequestedBy);
				notifications.Add(new InfoNotification("Usage Report sent " + info.DateFrom.ToShortDateString() + " " + info.DateTo.ToShortDateString() + " " + info.RequestedBy));
			}
			return true;
		}

		void ProcessRequest(ZDateTime dateFromInBillingTimeZone, ZDateTime dateToInBillingTimeZone, string requestedBy)
		{
			// Process the request a month at a time
			var dateFrom = dateFromInBillingTimeZone;
			var dateTo = dateToInBillingTimeZone;
			while (dateFrom < dateTo)
			{
				var nextMonth = dateFrom.AddMonths(1);
				var endOfMonth = new ZDateTime(nextMonth.Year, nextMonth.Month, 1).AddDays(-1);
				if (endOfMonth > dateTo)
				{
					endOfMonth = dateTo;
				}

				var converter = new BillingTimeConverter();
				var dateFromUtcInclusive = converter.ConvertTimeInBillingTimeZoneToUtc(dateFrom);
				var dateToUtcExclusive = converter.ConvertTimeInBillingTimeZoneToUtc(endOfMonth.AddDays(1));

				var usageReport = new LicenceUsageReportBuilder(null);
				usageReport.BuildAndSend(dateFromUtcInclusive, dateToUtcExclusive, requestedBy);
				dateFrom = endOfMonth.AddDays(1);
			}
		}

		void IMessageAction.SendNotificationEmail(ZString subject, ZString body, INotifications notifications, bool onSuccess)
		{
		}
	}
}
