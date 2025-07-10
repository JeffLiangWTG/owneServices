using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.ElectronicMessaging.Germany;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing.EventMessageProcessor;
using Enterprise.Accounting.ElectronicMessaging.Hungary;
using Enterprise.Accounting.ElectronicMessaging.India;
using Enterprise.Accounting.ElectronicMessaging.Italy;
using Enterprise.Accounting.ElectronicMessaging.Taiwan;
using Enterprise.Accounting.ElectronicMessaging.TaxCore;
using Enterprise.Accounting.ElectronicMessaging.Turkey;
using Enterprise.Accounting.ElectronicMessaging.Vietnam;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using static CargoWise.EventReference.Constants;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Accounting.ElectronicMessaging.Common.Universal
{
	public class AccEInvoicingBatchDataContextManager : EventDataContextManager<AccEInvoicingBatch>, ITransactionDataContextManager, IDataContextManagerFromEDIMessage
	{
		public override DataContextType DataContextType
		{
			get { return DataContextType.AccEInvoicingBatch; }
		}

		public override ZString DataContextKey
		{
			get { return ParentBO.AIB_BatchNumber.ToString(); }
		}

		protected override ZQuery GetDataContextKeyMatchingQuery(IDataContextMatchingKey matchingValues, BusinessObjectFactory factory, IXmlImportLogger logger)
		{
			Argument.NotNull(matchingValues, nameof(matchingValues));
			Argument.NotNull(factory, nameof(factory));
			var batchNumber = matchingValues.Key;
			if (batchNumber.IsEmpty || !(matchingValues.DataObject is UniversalEvent universalEvent))
			{
				return ZQuery.NoResultQuery;
			}

			var (eventType, countryCode, messageSubType, reason) = ReadParametersFromUniversalEvent(universalEvent);
			var (countrySupported, doesMatchEvent) = CheckIfCountrySupportedAndDoesMatchEvent(eventType, countryCode, messageSubType);

			if (countrySupported)
			{
				return GetEInvoicingBatchFilterQuery(matchingValues, factory, universalEvent, batchNumber, doesMatchEvent);
			}
			else
			{
				var message = Res.GetString("0db308f6-140a-47f7-b2c2-43cfc36f9d6c", "Event parameter contains unsupported country/region '{0}'", countryCode);
				logger.LogBoth(LogType.Error, message);
#if DEBUG
				if (!(ZArchitecture.Environment.Globals.IsTest && reason.Contains(TestInReleaseModeReason_ForTestOnly)))
				{
					throw new System.InvalidOperationException($"AccEInvoicingBatchDataContextManager.GetDataContextKeyMatchingQuery : {message}");
				}
#endif
				return ZQuery.NoResultQuery;
			}
		}

		(bool countrySupported, bool doesMatchEvent) CheckIfCountrySupportedAndDoesMatchEvent(ZString eventType, ZString countryCode, ZString messageSubType)
		{
			if (TaxCoreCountryHelper.IsThisCountrySupportedByTaxCore(countryCode))
			{
				var countryFactory = TaxCoreEInvoicingResponseObjectFactory.GetICountryEInvoicingResponseObjectFactory(countryCode);
				return (true, checkMatchEvent(countryFactory?.ResponseMessageSubTypeCode ?? string.Empty));
			}
			else
			{
				var countryFactory = GlobalEInvoicingObjectFactory.GetICountryEInvoicingObjectFactory(countryCode);
				if (countryFactory != null)
				{
					return (true, countryFactory.GetGlobalXUEFunctionalityProvider().IsBatchEventMessageProcessSupported(eventType, messageSubType));
				}

				switch (countryCode)
				{
					case Core.Constants.CountryCodes.Italy:
						return (true, checkMatchEvent(EInvoicingEventMessageITProcessor.MessageSubTypeCode.RiceviFile));

					case Core.Constants.CountryCodes.Taiwan:
						return (true, checkInterchangeAcknowledgeCode());

					case Core.Constants.CountryCodes.India:
						return (true, checkMatchEvent(EInvoicingEventMessageINProcessor.MessageSubTypeCode.InvoiceResponse));

					case Core.Constants.CountryCodes.Germany:
						return (true, checkMatchEvent(EInvoicingEventMessageDEProcessor.MessageSubTypeCode.InvoiceResponse));

					case Core.Constants.CountryCodes.Turkey:
						return (true, checkInterchangeAcknowledgeCode());

					case Core.Constants.CountryCodes.Hungary:
						return (true, checkMatchEvent(HungaryEInvoiceAPICommandList.Codes.GenerateInvoiceRequest));

					case Core.Constants.CountryCodes.VietNam:
						return (true, checkInterchangeAcknowledgeCode());

					case Core.Constants.CountryCodes.SaudiArabia:
						return (true, checkInterchangeAcknowledgeCode());

					default:
						return (false, false);
				}
			}

			bool checkMatchEvent(string messageSubTypeToMatch) =>
				eventType == AutoEvents.InterchangeRejectedCode || (eventType == AutoEvents.InterchangeAcknowledgedCode && messageSubType == messageSubTypeToMatch);

			bool checkInterchangeAcknowledgeCode() => eventType == AutoEvents.InterchangeRejectedCode || eventType == AutoEvents.InterchangeAcknowledgedCode;
		}

		(ZString eventType, ZString countryCode, ZString messageSubType, ZString reason) ReadParametersFromUniversalEvent(UniversalEvent universalEvent)
		{
			var eventType = universalEvent.EventType ?? ZString.Empty;
			if (universalEvent.EventParameters == null)
			{
				return (eventType, ZString.Empty, ZString.Empty, ZString.Empty);
			}

			var countryCode = EventParameters.GetEventParameter(EventReferenceParameters.Codes.MessageType, universalEvent.EventParameters) ?? ZString.Empty;
			var messageSubType = EventParameters.GetEventParameter(EventReferenceParameters.Codes.MessageSubType, universalEvent.EventParameters) ?? ZString.Empty;
			var reason = EventParameters.GetEventParameter(EventReferenceParameters.Codes.Reason, universalEvent.EventParameters) ?? ZString.Empty;
			return (eventType, countryCode, messageSubType, reason);
		}

		ZQuery GetEInvoicingBatchFilterQuery(IDataContextMatchingKey matchingValues, BusinessObjectFactory factory, UniversalEvent universalEvent, ZString batchNumber, bool doesMatchEvent)
		{
			if (!doesMatchEvent)
			{
				return ZQuery.NoResultQuery;
			}

			var company = LoadCompany(matchingValues, universalEvent, factory);
			if (company != null)
			{
				var result = new ZQuery(AccEInvoicingBatchSchema.AIB_GC, company.PK);
				result.AddToFilter(AccEInvoicingBatchSchema.AIB_BatchNumber, new ZInt(batchNumber));
				return result;
			}

			return ZQuery.NoResultQuery;
		}

		GlbCompany LoadCompany(IDataContextMatchingKey matchingValues, UniversalEvent universalEvent,  BusinessObjectFactory factory)
		{
			GlbCompany company = null;
			if (universalEvent.DataContext is UniversalDataBuss.DataObjects.Universal._2011_11.DataContext)
			{
				company = factory.LoadFromNaturalKey<GlbCompany>(GlbCompanySchema.GC_Code, matchingValues.CompanyCode);
			}
			else if (universalEvent.DataContext is UniversalDataBuss.DataObjects.Universal._2012_11.DataContext)
			{
				if (universalEvent != null && universalEvent.ContextCollection != null)
				{
					var companyCodeContext = universalEvent.ContextCollection.FirstOrDefault(x => x.Type == EInvoicingEventMessageProcessor.ContextTypeCode.CompanyCode);
					if (companyCodeContext != null && companyCodeContext.Value.HasValue && !companyCodeContext.Value.Value.IsEmpty)
					{
						company = factory.LoadFromNaturalKey<GlbCompany>(GlbCompanySchema.GC_Code, companyCodeContext.Value.Value);
					}
				}
			}
			return company;
		}

		public void OnLogParentFoundFromEDIMessage(IXmlSessionTracker logger, IXmlEventValueObject eventDataObject, IEDIMessage message, BusinessObject businessObject)
		{
			var key = "AccEInvoicingBatchDataContextManager.OnLogParentFoundFromEDIMessage";
			if (businessObject is AccEInvoicingBatch parent)
			{
				var universalEvent = eventDataObject as UniversalEvent;
				var eventType = universalEvent.EventType;
				var countryCode = EventParameters.GetEventParameter(EventReferenceParameters.Codes.MessageType, universalEvent.EventParameters);

				if (eventType.HasValue && (eventType.Value == AutoEvents.InterchangeRejectedCode || eventType.Value == AutoEvents.InterchangeAcknowledgedCode || eventType.Value == AutoEvents.InterchangeSentCode))
				{
					var processor = GetMessageProcessor(countryCode, logger, message, universalEvent, parent);

					if (processor != null)
					{
						processor.Process();
					}
				}
				else
				{
					LoggerWrapper.ReportAndLogError(logger, LogType.Error, Res.GetString("b61f28cd-6dc9-4dfe-b356-e4e784ea9ad9", "Cannot process message for unknown event type '{0}'", eventType ?? ZString.Empty), key);
				}
			}
			else if (businessObject is InvoicingBase invoice)
			{
				var universalEvent = eventDataObject as UniversalEvent;
				var countryCode = EventParameters.GetEventParameter(EventReferenceParameters.Codes.MessageType, universalEvent?.EventParameters);
				var processor = GetMessageChildrenProcessor(countryCode, logger, message, universalEvent, invoice);

				if (processor != null)
				{
					processor.Process();
				}
			}
			else if (businessObject is AccComplianceDocumentHeader)
			{
				// do nothing as compliance documents are just children of the batch, event log has been added to them so nothing else need to be done.
			}
			else
			{
				LoggerWrapper.ReportAndLogError(logger, LogType.Error, Res.GetString("dad14d45-b4ea-48d9-bf3b-5b92bf57b457", "Unable to process - unsupported type detected, Parent BO type is {0}", businessObject.GetType()), key);
			}
		}

		EInvoicingEventMessageProcessor GetMessageProcessor(ZString? countryCode, IXmlSessionTracker logger, IEDIMessage message, UniversalEvent universalEvent, AccEInvoicingBatch parent)
		{
			var key = "AccEInvoicingBatchDataContextManager.messageProcessorCheck";

			if (countryCode.HasValue && TaxCoreCountryHelper.IsThisCountrySupportedByTaxCore(countryCode.Value))
			{
				var countryFactory = TaxCoreEInvoicingResponseObjectFactory.GetICountryEInvoicingResponseObjectFactory(countryCode.Value);
				if (countryFactory != null)
				{
					return new TaxCoreEInvoicingEventMessageProcessor(logger, message, universalEvent, parent, countryFactory);
				}
				else
				{
					logger.LogBoth(LogType.Error, GetNotSupportedErrorMessage());
				}
			}

			if (countryCode.HasValue)
			{
				var countryFactory = GlobalEInvoicingObjectFactory.GetICountryEInvoicingObjectFactory(countryCode.Value);
				if (countryFactory != null)
				{
					var eventType = universalEvent.EventType ?? ZString.Empty;
					var messageSubType = EventParameters.GetEventParameter(EventReferenceParameters.Codes.MessageSubType, universalEvent.EventParameters) ?? ZString.Empty;
					var provider = countryFactory.GetGlobalXUEFunctionalityProvider();
					if (provider.IsBatchEventMessageProcessSupported(eventType, messageSubType))
					{
						return provider.GetEventMessageProcessor(new EventMessageProcessorData(eventType, messageSubType, logger, message, universalEvent, parent));
					}
					else
					{
						LoggerWrapper.ReportAndLogError(logger, LogType.Error, Res.GetString("21f294cc-1613-4165-88aa-1e503eda5562", "Cannot process message for unsupported Event Type '{0}' and Message Sub Type '{1}' for country/region '{2}'.", eventType, messageSubType, countryCode), key);
						return null;
					}
				}
			}

			switch (countryCode)
			{
				case Core.Constants.CountryCodes.Italy:
					return new EInvoicingEventMessageITProcessor(logger, message, universalEvent, parent);
				case Core.Constants.CountryCodes.Taiwan:
					return new EInvoicingEventMessageTWProcessor(logger, message, universalEvent, parent);
				case Core.Constants.CountryCodes.Turkey:
					return new EInvoicingEventMessageTRProcessor(logger, message, universalEvent, parent);
				case Core.Constants.CountryCodes.Hungary:
					return new EInvoicingEventMessageHUProcessor(logger, message, universalEvent, parent);
				case Core.Constants.CountryCodes.VietNam:
					return new EInvoicingEventMessageVNProcessor(logger, message, universalEvent, parent);
				case Core.Constants.CountryCodes.Germany:
					return new EInvoicingEventMessageDEProcessor(logger, message, universalEvent, parent);
				default:
					LoggerWrapper.ReportAndLogError(logger, LogType.Error, GetNotSupportedErrorMessage(), key);
					return null;
			}

			string GetNotSupportedErrorMessage()
			{
				return Res.GetString("dc16679a-44ad-425e-b666-0f87d6cf895b", "Cannot process message for unsupported country/region '{0}'", countryCode);
			}
		}

		EInvoicingEventMessageProcessor GetMessageChildrenProcessor(ZString? countryCode, IXmlSessionTracker logger, IEDIMessage message, UniversalEvent universalEvent, InvoicingBase parent)
		{
			if (countryCode.HasValue)
			{
				var eventType = universalEvent.EventType ?? ZString.Empty;
				var countryFactory = GlobalEInvoicingObjectFactory.GetICountryEInvoicingObjectFactory(countryCode.Value);
				if (countryFactory != null)
				{
					if (countryFactory.GetGlobalXUEFunctionalityProvider().IsEventMessageChildrenProcessSupported(eventType))
					{
						return countryFactory.GetGlobalXUEFunctionalityProvider().GetEventMessageChildrenProcessor(new EventMessageChildrenProcessorData(logger, message, universalEvent, parent));
					}
				}
			}

			return null;
		}

		protected override EventParentFinder GetEventParentFinder(BusinessObjectFactory factory, IXmlImportLogger logger)
		{
			return new AccEInvoicingBatchEventParentFinder(factory, this, logger);
		}

		public bool ManagesTransactions
		{
			get { return false; }
		}

		public ITopLevelDataObjectWriter GetTransactionDataObjectWriter(IDataWritingManager writeManager)
		{
			return null;
		}

		public override string DefaultOutputDirectory
		{
			get { return null; }
		}

		protected override IEnumerable<KeyValuePair<TypeWithDescription, IZType>> GetEventContextValues()
		{
			return Enumerable.Empty<KeyValuePair<TypeWithDescription, IZType>>();
		}

#if DEBUG
		public const string TestInReleaseModeReason_ForTestOnly = "B350A274-AEF8-43e8-A424-D69BFDFE8559 | Test EInvoicing Message in Release mode";
#endif
	}
}
