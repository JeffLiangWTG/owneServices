using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing;
using Enterprise.Integration;
using Enterprise.Integration.DocumentEngine;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Accounting.ElectronicMessaging
{
	public class EInvoicingEventMessageProcessorForInvoice : EInvoicingEventMessageProcessor
	{
		public EInvoicingEventMessageProcessorForInvoice(IXmlSessionTracker logger, IEDIMessage message, UniversalEvent universalEvent, InvoicingBase invoice) : base(logger, message, universalEvent, invoice)
		{
		}

		protected override GEIEmailNotificationCreator GetEmailNotificationCreator(ZString errorMessage, ILogger logCollector)
		{
			return new GEIEmailNotificationCreator(ediMessage, invoice, new List<ZString>() { errorMessage }, logCollector);
		}

		protected override string LogErrorNotFoundKey => "TransactionNotFound";
		public const string AuthorisedFunctionalityCode = "ACCEGAMI";

		public override void Process()
		{
			if (universalEvent.EventType.Value == AutoEvents.AuthorisedCode)
			{
				if (ObjectFactory.Get<IZZCustomsFunctionalityEffectiveDate>()
					.IsFunctionalityValid(AuthorisedFunctionalityCode, countryCode, ZDateTime.Today, invoice.Company?.LicenceKeyIdentifier ?? string.Empty))
				{
					ProcessATHEventMessage();
				}
				else
				{
					logger.LogBoth(LogType.Warning, (NoResString)"Currently Accounting EInvoicing Generic Authorisation Message Import feature is not implemented");
					return;
				}
			}
		}

		void ProcessATHEventMessage()
		{
			var eventAndDatabaseTransaction = new EventAndDatabaseTransaction()
			{
				Transaction = invoice,
				UniversalEvent = universalEvent,
				CompanyName = companyName,
				AuthRecordType = AccTransactionHeaderAuthorisationRecordTypes.AllOtherCountries,
				AuthRecord = AccTransactionHeaderAuthorisationRecordLoader.LoadByParentID(invoice.Factory, invoice.PK, invoice.Company?.GC_RN_NKCountryCode),
				EventData = UniversalEventTransactionDataObject.FromUniversalEvent(universalEvent)
			};

			eventAndDatabaseTransaction.MapAuthorisationRecordToDatabase(logger); // This is the first proirity. We may need more Map methods later
		}
	}
}
