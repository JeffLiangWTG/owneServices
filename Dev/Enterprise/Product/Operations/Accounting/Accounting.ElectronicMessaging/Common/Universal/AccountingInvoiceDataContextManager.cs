using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.DataTransfer.Universal;
using Enterprise.Accounting.ElectronicMessaging.China;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Schema;
using static CargoWise.EventReference.Constants;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Accounting.ElectronicMessaging.Common.Universal
{
	public class AccountingInvoiceDataContextManager : EventDataContextManager<InvoicingBase>, ITransactionDataContextManager, IDataContextManagerFromEDIMessage
	{
		// Note that Spain is processed via AccEInvoicingBatch events.
		// Some of this code may be redundant.

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard-coded identifier")]
		public static class EventDataConstants
		{
			public const string ElectronicInvoicingEventType = "Electronic Reporting Reception Acknowledgement";
			public const string ElectronicInvoicingEventType_SingleTransactionHeader = "Update If Single Match Is Found";
			public const string ElectronicInvoicingEventSubType_Spain = "Spain";
			public const string ElectronicInvoicingEventType_China = "CN";
			public const string ElectronicInvoicingEventSubType_SIU = "SIU";
			public const string Context_InvoicePK = "EINV_CN_SerialNumber";
			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard-coded identifier, for 'NIF' use ES fallback logic, for 'Other', assume the string is in this structure: <country code><orgcuscode type><orgcuscode number>")]
			public const string Context_TaxRegNumber = "Tax Reg Number";
			public const string Context_InvoiceDate = "Invoice Date";
			public const string Context_OrganizationCode = "Organization Code";
		}

		public override DataContextType DataContextType
		{
			get { return DataContextType.AccountingInvoice; }
		}

		public override ZString DataContextKey
		{
			get { return ParentBO.AH_Ledger + " " + ParentBO.AH_TransactionType + " " + ParentBO.AH_TransactionNum; }
		}

		protected override ZQuery GetDataContextKeyMatchingQuery(IDataContextMatchingKey matchingValues, BusinessObjectFactory factory, IXmlImportLogger logger)
		{
			Argument.NotNull(factory, "factory");
			var result = ZQuery.NoResultQuery;
			var company = factory.LoadFromNaturalKey<GlbCompany>(GlbCompanySchema.GC_Code, matchingValues.CompanyCode);
			var keys = AccountingInvoiceEventParentFinder.GetAllKeys(matchingValues.Key);

			ZString? messageType = null;
			ZString? messageSubType = null;
			var universalEvent = matchingValues.DataObject as UniversalEvent;

			if (universalEvent != null && universalEvent.EventParameters != null)
			{
				messageType = EventParameters.GetEventParameter(EventReferenceParameters.Codes.MessageType, universalEvent.EventParameters);
				messageSubType = EventParameters.GetEventParameter(EventReferenceParameters.Codes.MessageSubType, universalEvent.EventParameters);
			}

			if (company != null && keys.AllKeys)
			{
				if (keys.Ledger != ZArchitecture.Core.LedgerTypes.AccountsPayable)
				{
					messageType = null;
					messageSubType = null;
				}

				// if AP ES eInvoicing acknowledgement or Update Single Transaction Header, we need to handle duplicate invoice number issue with EventParentFinder, otherwise we can match with the context key.
				if (!((messageType.HasValue && messageType.Value == EventDataConstants.ElectronicInvoicingEventType &&
					messageSubType.HasValue && messageSubType.Value == EventDataConstants.ElectronicInvoicingEventSubType_Spain)
					|| (messageType.HasValue && messageType.Value == EventDataConstants.ElectronicInvoicingEventType_SingleTransactionHeader))
					)
				{
					result = new ZQuery(AccTransactionHeaderSchema.AH_GC, company.PK);
					result.AddToFilter(AccTransactionHeaderSchema.AH_Ledger, keys.Ledger);
					result.AddToFilter(AccTransactionHeaderSchema.AH_TransactionType, keys.TransactionType);
					result.AddToFilter(AccTransactionHeaderSchema.AH_TransactionNum, keys.TransactionNum);
				}
			}
			return result;
		}

		public bool ManagesTransactions
		{
			get { return true; }
		}

		public ITopLevelDataObjectWriter GetTransactionDataObjectWriter(IDataWritingManager writeManager)
		{
			return new TransactionDataObjectWriter(writeManager);
		}

		public override string DefaultOutputDirectory
		{
			get { return null; }
		}

		protected override IEnumerable<KeyValuePair<TypeWithDescription, IZType>> GetEventContextValues()
		{
			return Enumerable.Empty<KeyValuePair<TypeWithDescription, IZType>>();
		}

		public void OnLogParentFoundFromEDIMessage(IXmlSessionTracker logger, IXmlEventValueObject eventDataObject, IEDIMessage message, BusinessObject businessObject)
		{
			var parent = businessObject as InvoicingBase;
			if (parent == null)
			{
				logger.Log(LogType.Error, Res.GetString("1FCD70D1-2CE1-4BBA-BCCF-12AA7CBA18F9", "Unable to process - unsupported type detected, Parent BO type is {0}", businessObject.GetType()));
			}
			else
			{
				var universalEvent = eventDataObject as UniversalEvent;
				var messageType = EventParameters.GetEventParameter(EventReferenceParameters.Codes.MessageType, universalEvent.EventParameters); //MessageType
				var messageSubType = EventParameters.GetEventParameter(EventReferenceParameters.Codes.MessageSubType, universalEvent.EventParameters); //MessageSubType
				if (messageType.HasValue && messageType.Value == EventDataConstants.ElectronicInvoicingEventType)
				{
					logger.Log(LogType.Error, Res.GetString("494A4669-C5C8-425A-9EB9-B204E572A856", "Cannot process message for unknown sub type '{0}'", messageSubType.Value));
				}

				if (messageType.HasValue && messageType.Value == EventDataConstants.ElectronicInvoicingEventType_China &&
					messageSubType.HasValue && messageSubType.Value == EventDataConstants.ElectronicInvoicingEventSubType_SIU)
				{
					var processor = new EInvoicingEventMessageCNProcessorForInvoice(logger, message, universalEvent, parent);
					processor.Process();
				}

				if (messageType.HasValue && messageType.Value == EventDataConstants.ElectronicInvoicingEventType_SingleTransactionHeader)
				{
					var processor = new EInvoicingEventMessageProcessorForInvoice(logger, message, universalEvent, parent);
					processor.Process();
				}
			}
		}

		protected override EventParentFinder GetEventParentFinder(BusinessObjectFactory factory, IXmlImportLogger logger)
		{
			return new AccountingInvoiceEventParentFinder(factory, this, logger);
		}
	}
}
