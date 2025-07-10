using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using static CargoWise.EventReference.Constants;
using static Enterprise.Accounting.ElectronicPayment.Universal.AccEPaymentQuoteMessageConstants;
using StatusCodes = Enterprise.MasterFiles.Business.EPaymentStatusCodes.Quote;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Accounting.ElectronicPayment.Universal
{
	public class AccEPaymentQuoteDataContextManager : EventDataContextManager<AccEPaymentQuote>, ITransactionDataContextManager, IDataContextManagerFromEDIMessage
	{
		public override DataContextType DataContextType => DataContextType.AccEPaymentQuote;

		public override ZString DataContextKey => ParentBO.QU_InternalReference;

		public override string DefaultOutputDirectory => null;

		public bool ManagesTransactions => false;

		public ITopLevelDataObjectWriter GetTransactionDataObjectWriter(IDataWritingManager writeManager)
		{
			return null;
		}

		public void OnLogParentFoundFromEDIMessage(IXmlSessionTracker logger, IXmlEventValueObject eventDataObject, IEDIMessage message, BusinessObject businessObject)
		{
			if (businessObject is AccEPaymentQuote parent)
			{
				EPaymentLogManager.AddLogToPaymentApproval(parent, eventDataObject, message);
				parent.QU_LastResponseReceivedUtc = eventDataObject.EventTime.ToZDateTime();
				var eventObject = (UniversalEvent)eventDataObject;
				if (eventObject.ContextCollection == null)
				{
					LoggerWrapper.ReportAndLogError(logger, LogType.Error, Res.GetString("5B259C2E-5B5F-4233-A61B-71978CE910DB", "Event does not contain a Context Collection."));
					return;
				}

				if (eventDataObject.EventType == Events.InterchangeAcknowledgedCode)
				{
					HandleIAKMessage(logger, eventObject, parent);
				}
				else if (eventDataObject.EventType == Events.InterchangeRejectedCode)
				{
					parent.QU_Status = StatusCodes.Error;
					var errorMessage = GetValueForContextKey(eventObject.ContextCollection, XUEFieldNames.ErrorMessage);
					AccEPaymentHelper.InvalidateStaffTokenIfRefreshTokenInvalid(errorMessage, parent.QU_SystemCreateUser, parent.QU_GC, logger);
					SetQuoteErrorDescription(parent, errorMessage);
					var providerCode = GetProviderCodeFromEventMessage(eventObject);
					var reference = $"{providerCode} E-Quote Error: {errorMessage}"; // Log Reference does not need to be translated.
					EPaymentLogManager.SetEPaymentLogReference(parent, Events.InterchangeRejected, reference);
				}
				else
				{
					LoggerWrapper.ReportAndLogError(logger, LogType.Error, Res.GetString("AC3EA007-090D-4005-A86A-570AE3624D1D", "Unexpected Event Type encountered: {0}.", eventDataObject.EventType));
				}
			}
			else
			{
				LoggerWrapper.ReportAndLogError(logger, LogType.Error, Res.GetString("A7095D6A-1EBA-4B9E-A7C9-6A8F289E8838", "Unable to process - unsupported type detected, Parent BO type is {0}", businessObject.GetType()));
				return;
			}
		}

		void HandleIAKMessage(IXmlSessionTracker logger, UniversalEvent eventObject, AccEPaymentQuote parent)
		{
			var messageMode = EventParameters.GetEventParameter(EventReferenceParameters.Codes.MessageSubType, eventObject.EventParameters);
			if (!messageMode.HasValue || !new[] { MessageModes.GetQuote, MessageModes.GetRate }.Contains(messageMode.Value))
			{
				LoggerWrapper.ReportAndLogError(logger, LogType.Error, Res.GetString("E427631D-25FC-4DDC-A19A-4B55D4CD702F", "Message mode Could not find a valid Message Mode in the Event's <MessageSubType> parameter. Value found: '{0}'", messageMode));
				return;
			}

			var fieldValues = eventObject.ContextCollection.Where(x => x.Type.Type.HasValue && x.Value.HasValue && !x.Value.Value.IsEmpty).ToDictionary(x => x.Type.Type.Value, x => x.Value.Value);
			var missingFields = XUEFieldNames.GetRequiredFields(messageMode.Value).Except(fieldValues.Keys);
			if (missingFields.Any())
			{
				LoggerWrapper.ReportAndLogError(logger, LogType.Error, Res.GetString("4D0C5849-B2FC-469F-834A-DD805B241325", @"The following Context fields are either empty or missing:
{0}", string.Join(System.Environment.NewLine, missingFields)));
				return;
			}
			if (parent.PaymentApproval == null)
			{
				LoggerWrapper.ReportAndLogError(logger, LogType.Error, Res.GetString("1D4892FD-0900-4FFD-BB93-4CD51ADC2F1B", "The Quote is not attached to an existing Payment Approval."));
				return;
			}

			var fromCurrency = fieldValues[XUEFieldNames.FromCurrency];
			var toCurrency = fieldValues[XUEFieldNames.ToCurrency];
			var toAmount = new ZDecimal(fieldValues[XUEFieldNames.ToAmount]);

			if (parent.QU_Status == StatusCodes.Discarded)
			{
				logger.LogBoth(LogType.Warning, Res.GetString("2FA6C441-6D34-4C13-85B2-F515A5F11C7F", "Incoming E-Quote Message relates to Payment Quote {0}, but this Quote has been discarded since it was requested.", parent.QU_InternalReference));
			}
			else if (parent.QU_Status != StatusCodes.Requested)
			{
				logger.LogBoth(LogType.Warning, Res.GetString("6E7F060C-FD5D-430F-82A7-B141D105B08A", "Incoming E-Quote Message relates to Payment Quote {0}, but this Quote is {1}. E-Quotes can be imported only when Payment Quote is in Requested status.", parent.QU_InternalReference, parent.Lookups.StatusCodeList.GetDescriptionFromCode(parent.QU_Status)));
				parent.QU_Status = StatusCodes.Error;
			}
			else if (parent.QU_RX_NKFromCurrency != fromCurrency || parent.QU_RX_NKToCurrency != toCurrency || parent.QU_ToAmount != toAmount)
			{
				LoggerWrapper.ReportAndLogError(logger, LogType.Warning, Res.GetString("DA0D045B-5149-499C-A621-6A307AEDF0E3", "Quote details should not change after being requested."));
				parent.QU_Status = StatusCodes.Error;
			}
			else if (parent.PaymentApproval.AV_RX_NKPaymentCurrency != toCurrency || parent.PaymentApproval.AV_Amount != toAmount)
			{
				LoggerWrapper.ReportAndLogError(logger, LogType.Warning, Res.GetString("2780883B-173D-4A89-A313-D73FBC3024B4", "Quote should be discarded after payment details are changed."));
				parent.QU_Status = StatusCodes.Error;
			}
			else
			{
				parent.QU_Status = StatusCodes.Received;
				if (messageMode.Value == MessageModes.GetQuote)
				{
					parent.QU_ProviderReference = fieldValues[XUEFieldNames.ProviderRef];
					parent.QU_RX_NKFeeCurrency = GetValueForContextKey(eventObject.ContextCollection, XUEFieldNames.FeeCurrency);
					parent.QU_FeeAmount = new ZDecimal(GetValueForContextKey(eventObject.ContextCollection, XUEFieldNames.FeeAmount));
				}
				parent.QU_FromAmount = new ZDecimal(fieldValues[XUEFieldNames.FromAmount]);
				parent.QU_ExchangeRate = new ZDecimal(fieldValues[XUEFieldNames.ExchangeRate]);
				parent.QU_ExchangeRateInverted = new ZDecimal(fieldValues[XUEFieldNames.ExchangeRateInverted]);
				var providerCode = GetProviderCodeFromEventMessage(eventObject);
				var reference = messageMode.Value == MessageModes.GetQuote ? $"E-Quote Received from {providerCode}." : $"Indicative Rate Received from {providerCode}."; // Log Reference does not need to be translated.
				EPaymentLogManager.SetEPaymentLogReference(parent, Events.InterchangeAcknowledged, reference);
			}
		}

		ZString GetProviderCodeFromEventMessage(UniversalEvent eventObject) => EventParameters.GetEventParameter(EventReferenceParameters.Codes.MessageType, eventObject.EventParameters) ?? ZString.Empty;

		static internal void SetQuoteErrorDescription(AccEPaymentQuote quote, ZString errorMessage)
		{
			quote.QU_ErrorDescription = AccEPaymentHelper.GetErrorDescription(errorMessage, AccEPaymentQuoteSchema.QU_ErrorDescription.MaxLength);
		}

		protected override ZQuery GetDataContextKeyMatchingQuery(IDataContextMatchingKey matchingValues, BusinessObjectFactory factory, IXmlImportLogger logger)
		{
			Argument.NotNull(matchingValues, nameof(matchingValues));
			Argument.NotNull(factory, nameof(factory));
			var quoteNumber = matchingValues.Key;
			if (quoteNumber.IsEmpty || !(matchingValues.DataObject is UniversalEvent universalEvent))
			{
				return ZQuery.NoResultQuery;
			}

			var providerCode = EventParameters.GetEventParameter(EventReferenceParameters.Codes.MessageType, universalEvent.EventParameters) ?? ZString.Empty;
			if (!(universalEvent.DataContext is UniversalDataBuss.DataObjects.Universal._2012_11.DataContext))
			{
				return ZQuery.NoResultQuery;
			}

			var context = universalEvent.ContextCollection?.FirstOrDefault(x => x.Type == XUEFieldNames.CompanyCode);
			GlbCompany company = null;
			if (context == null || !context.Value.HasValue)
			{
				return ZQuery.NoResultQuery;
			}
			else
			{
				company = factory.LoadFromNaturalKey<GlbCompany>(GlbCompanySchema.GC_Code, context.Value.Value);
			}

			var query = new ZQuery(AccEPaymentQuoteSchema.QU_InternalReference, quoteNumber);
			query.AddToFilter(new ZQuery(AccEPaymentQuoteSchema.QU_GC, company?.PK ?? ZGuid.Empty));
			query.AddToFilter(new ZQuery(AccEPaymentQuoteSchema.QU_ProviderCode, providerCode));

			return query;
		}

		ZString GetValueForContextKey(List<Context> contextCollection, ZString key)
		{
			var value = contextCollection?.FirstOrDefault(x => x.Type == key)?.Value;
			return value ?? ZString.Empty;
		}

		protected override IEnumerable<KeyValuePair<TypeWithDescription, IZType>> GetEventContextValues()
		{
			return Enumerable.Empty<KeyValuePair<TypeWithDescription, IZType>>();
		}

		protected override EventParentFinder GetEventParentFinder(BusinessObjectFactory factory, IXmlImportLogger logger)
		{
			return new AccEPaymentQuoteEventParentFinder(factory, this, logger);
		}
	}
}
