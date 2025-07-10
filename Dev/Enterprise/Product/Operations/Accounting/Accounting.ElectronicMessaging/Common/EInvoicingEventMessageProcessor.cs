using System;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Integration;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Core;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Accounting.ElectronicMessaging.Common
{
	public abstract class EInvoicingEventMessageProcessor
	{
		internal static class ContextTypeCode
		{
			public const string CompanyCode = "CompanyCode";
			public const string ResponseFileResult = "ResponseFileResult";
		}

		protected EInvoicingEventMessageProcessor(IXmlSessionTracker logger, IEDIMessage message, UniversalEvent universalEvent, AccEInvoicingBatch invoiceBatch)
		{
			this.invoiceBatch = Argument.NotNull(invoiceBatch, "invoiceBatch");
			this.universalEvent = Argument.NotNull(universalEvent, "eventDataObject");
			this.logger = Argument.NotNull(logger, "logger");
			companyName = invoiceBatch.Company?.GC_Name ?? ZString.Empty;
			countryCode = invoiceBatch.Company?.Country.Code ?? ZString.Empty;

			ediMessage = Argument.NotNull(message as EDIMessage, "ediMessage");
		}

		protected EInvoicingEventMessageProcessor(IXmlSessionTracker logger, IEDIMessage message, UniversalEvent universalEvent, InvoicingBase invoice)
		{
			this.invoice = Argument.NotNull(invoice, "invoice");
			this.universalEvent = Argument.NotNull(universalEvent, "eventDataObject");
			this.logger = Argument.NotNull(logger, "logger");
			companyName = invoice.Company?.GC_Name ?? ZString.Empty;
			countryCode = invoice.Company?.Country.Code ?? ZString.Empty;

			ediMessage = Argument.NotNull(message as EDIMessage, "ediMessage");
		}

		protected readonly InvoicingBase invoice;
		protected readonly AccEInvoicingBatch invoiceBatch;
		protected readonly UniversalEvent universalEvent;
		protected readonly IXmlSessionTracker logger;
		protected readonly ZString companyName;
		protected readonly ZString countryCode;
		protected readonly EDIMessage ediMessage;

		public abstract void Process();

		protected abstract GEIEmailNotificationCreator GetEmailNotificationCreator(ZString errorMessage, ILogger logCollector);

		#region Helpers

		protected virtual bool ShouldSendEmail => true;
		protected abstract string LogErrorNotFoundKey { get; }

		protected void SendErrorNotificationEmail(ZString errorMessage)
		{
			if (ShouldSendEmail)
			{
				var logCollector = new SimpleLogger();
				var emailCreator = GetEmailNotificationCreator(errorMessage, logCollector);
				if (emailCreator != null)
				{
					emailCreator.SendEmail();

					var logCollectorAsString = logCollector.ToString();
					if (!logCollectorAsString.IsNullOrEmpty())
					{
						logger.LogBoth(LogType.Information, logCollectorAsString);
					}
				}
			}
			else
			{
				ReportAndLogError(LogErrorNotFoundKey, Res.GetString(
					"817C49D5-F03A-46E8-A084-0C2FC422D845",
					"Error Notification Email was not sent due to related {0} was not found for pivot in invoice batch {1} in {2}.",
					LogErrorNotFoundKey, invoiceBatch?.AIB_BatchNumber, companyName));
			}
		}

		//TODO: universalEvent.EventTime is local time. But it is considered UTC. This should be fixed.
		protected ZDateTime LastResponseReceivedTime => universalEvent.EventTime.GetValueOrDefault().ToZDateTime().IsEmpty ? ZDateTime.UtcNow : universalEvent.EventTime.GetValueOrDefault().ToZDateTime();

		protected void ReportAndLogError(string key, string message)
		{
			LoggerWrapper.ReportAndLogError(logger, LogType.Error, message, FormattableString.Invariant($"{this.GetType().Name}_{key}"));
		}

		protected void LogError(string message)
		{
			logger.LogBoth(LogType.Error, message);
		}

		#endregion
	}
}
