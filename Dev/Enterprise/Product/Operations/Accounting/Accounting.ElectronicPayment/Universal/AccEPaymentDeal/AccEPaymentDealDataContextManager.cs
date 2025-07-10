using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.EPayment;
using Enterprise.Accounting.Business.ARAP.PaymentApproval;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static CargoWise.EventReference.Constants;
using static Enterprise.Accounting.ElectronicPayment.Universal.AccEPaymentDealMessageConstants;
using StatusCodes = Enterprise.MasterFiles.Business.EPaymentStatusCodes.Deal;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Accounting.ElectronicPayment.Universal
{
	public class AccEPaymentDealDataContextManager : EventDataContextManager<AccEPaymentDeal>, ITransactionDataContextManager, IDataContextManagerFromEDIMessage
	{
		public override DataContextType DataContextType => DataContextType.AccEPaymentDeal;

		public override ZString DataContextKey => ParentBO.AED_InternalReference;

		public override string DefaultOutputDirectory => null;

		public bool ManagesTransactions => false;

		public ITopLevelDataObjectWriter GetTransactionDataObjectWriter(IDataWritingManager writeManager)
		{
			return null;
		}

		public void OnLogParentFoundFromEDIMessage(IXmlSessionTracker logger, IXmlEventValueObject eventDataObject, IEDIMessage message, BusinessObject businessObject)
		{
			if (businessObject is AccEPaymentDeal deal)
			{
				var eventObject = (UniversalEvent)eventDataObject;
				EPaymentLogManager.AddLogToPaymentApproval(deal, eventDataObject, message);
				if (eventObject.ContextCollection == null)
				{
					LoggerWrapper.ReportAndLogError(logger, LogType.Error, Res.GetString("8a96d883-fbfb-4cde-92d8-5418ef895017", "Event does not contain a Context Collection."));
				}
				else if (eventDataObject.EventType == Events.InterchangeAcknowledgedCode || eventDataObject.EventType == Events.InterchangeRejectedCode)
				{
					var messageSubType = EventParameters.GetEventParameter(EventReferenceParameters.Codes.MessageSubType, eventObject.EventParameters);
					if (!messageSubType.HasValue || !new ZString[] { AutoEvents.StatusUpdatedCode, MessageSubTypes.CreateADeal }.Contains(messageSubType.Value))
					{
						LoggerWrapper.ReportAndLogError(logger, LogType.Error, Res.GetString("3b552b20-4c0e-4b08-895a-ce917db546f6", "Unable to process - unsupported message subtype '{0}' detected", messageSubType));
					}
					else if (eventDataObject.EventType == Events.InterchangeAcknowledgedCode)
					{
						HandleIAKMessage(logger, eventObject, deal);
					}
					else
					{
						HandleIRJMessage(logger, eventObject, deal);
					}
				}
				else
				{
					LoggerWrapper.ReportAndLogError(logger, LogType.Error, Res.GetString("8ce0f5e0-847d-4ef1-ac5d-388d9a9f7663", "Unexpected Event Type encountered: {0}.", eventDataObject.EventType));
				}
			}
			else
			{
				LoggerWrapper.ReportAndLogError(logger, LogType.Error, Res.GetString("04d71b56-6b68-4c83-80a0-e953213a0e68", "Unable to process - unsupported type detected, Parent BO type is {0}", businessObject.GetType()));
			}
		}

		#region HandleIAKMessage

		void HandleIAKMessage(IXmlSessionTracker logger, UniversalEvent eventObject, AccEPaymentDeal deal)
		{
			if (!CheckForMissingOrEmptyContextFeilds(logger, eventObject, out var fieldValues))
			{
				var messageSubType = EventParameters.GetEventParameter(EventReferenceParameters.Codes.MessageSubType, eventObject.EventParameters) ?? ZString.Empty;
				if (messageSubType == Events.StatusUpdatedCode)
				{
					try
					{
						var ofxDealStatus = fieldValues[XUEFieldNames.StatusCode];
						var newStatus = deal.ConvertOFXDealStatusToCW1(ofxDealStatus);
						if (deal.AED_Status == StatusCodes.Cancelled)
						{
							LoggerWrapper.ReportAndLogError(logger, LogType.Error, Res.GetString("EA59BD81-50FE-41DF-9911-91F268CDF342", "Cannot update deal status to '{0}' due to deal has been canceled in CW1 already", newStatus));
							return;
						}
						else
						{
							deal.AED_Status = newStatus;
						}

						SetDealErrorDescription(deal, ofxDealStatus);
						SetLastResponseReceivedUtc(eventObject, deal);
						var reference = $"E-Payment Deal {deal.AED_InternalReference} status changed to {ofxDealStatus}."; // Log Reference does not need to be translated.
						EPaymentLogManager.SetEPaymentLogReference(deal, Events.InterchangeAcknowledged, reference);
					}
					catch (InvalidOperationException ex)
					{
						LoggerWrapper.ReportAndLogError(logger, LogType.Error, ex.Message);
						return;
					}
				}
				else if (messageSubType == MessageSubTypes.CreateADeal && CheckDealIsInRequestedStatus(logger, deal, out var paymentApprovalReference))
				{
					var quoteIdUsedForPayment = fieldValues[XUEFieldNames.QuoteIdUsedForPayment];
					var providerDealStatus = fieldValues[XUEFieldNames.Status];
					var dealProviderReference = fieldValues[XUEFieldNames.DealId];
					var messageType = EventParameters.GetEventParameter(EventReferenceParameters.Codes.MessageType, eventObject.EventParameters);

					if (providerDealStatus == AccEPaymentDealLookups.OFXDealStatus.Booked)
					{
						if (deal.Quote.QU_ProviderReference != quoteIdUsedForPayment)
						{
							var paymentApproval = deal.Quote.PaymentApproval;
							if (deal.Quote.QU_Status != EPaymentStatusCodes.Quote.Accepted)
							{
								logger.LogBoth(LogType.Warning, Res.GetString("73d69904-0643-4eb7-9ef2-1748d5ec1c3a", "Expected E-Quote Status is 'Accepted', but current Payment Approval {0} has E-Quote {1} with status '{2}'.", deal.Quote.QU_InternalReference, paymentApprovalReference, deal.Quote.QU_Status));
							}

							deal.Quote.QU_Status = EPaymentStatusCodes.Quote.Discarded;

							if (fieldValues.TryGetValue(XUEFieldNames.ExpiredQuoteIDs, out var expiredQuoteIds))
							{
								var seperatedExpiredQuoteIds = fieldValues[XUEFieldNames.ExpiredQuoteIDs].Split("||").ToList();
								if (!seperatedExpiredQuoteIds.Contains(deal.Quote.QU_ProviderReference))
								{
									logger.LogBoth(LogType.Warning, Res.GetString("f2e81379-6edf-4c63-a001-bc0b34d3d9da", "Payment booked against new E-Quote compared to the accepted E-Quote, but the provider has not confirmed if the original E-Quote is expired."));
								}
								else
								{
									AccEPaymentQuoteDataContextManager.SetQuoteErrorDescription(deal.Quote, QuoteExpiredMessage);
								}
							}
							else
							{
								logger.LogBoth(LogType.Warning, Res.GetString("f2e81379-6edf-4c63-a001-bc0b34d3d9da", "Payment booked against new E-Quote compared to the accepted E-Quote, but the provider has not confirmed if the original E-Quote is expired."));
							}

							var newQuote = CreateNewQuote(eventObject, paymentApproval, fieldValues, messageType, quoteIdUsedForPayment);
							var paymentApprovalBase = deal.Factory.LoadTop1<PaymentApprovalBase>(new ZQuery(AccPaymentApprovalSchema.PK, paymentApproval.PK));
							var warningMessage = EPaymentQuoteAcceptor.AcceptEPaymentQuoteForAcceptedDeal(newQuote, paymentApprovalBase);
							if (!warningMessage.IsNullOrEmpty())
							{
								logger.LogBoth(LogType.Warning, warningMessage);
							}
							paymentApprovalBase.AV_Status = PaymentApprovalStatus.FullyApproved;
							deal.AED_QU_Quote = newQuote.PK;
						}

						deal.AED_Status = EPaymentStatusCodes.Deal.Accepted;
						deal.AED_ProviderReference = dealProviderReference;
						SetLastResponseReceivedUtc(eventObject, deal);
						var reference = $"E-Payment Deal {deal.AED_InternalReference} accepted by {messageType}."; // Log Reference does not need to be translated.
						EPaymentLogManager.SetEPaymentLogReference(deal, Events.InterchangeAcknowledged, reference);
					}
					else
					{
						LoggerWrapper.ReportAndLogError(logger, LogType.Error, Res.GetString("2e0e0b5d-f8eb-451f-b69a-86cdb1945f8e", "Unexpected E-Payment Provider Deal Status [{0}] encountered.", providerDealStatus));
					}
				}
			}
		}

		static EPaymentQuote CreateNewQuote(UniversalEvent eventObject, AccPaymentApproval paymentApproval, Dictionary<ZString, ZString> fieldValues, ZString? messageType, ZString quoteProviderReference)
		{
			var newQuote = paymentApproval.Factory.New<EPaymentQuote>();
			newQuote.QU_AV = paymentApproval.PK;
			newQuote.QU_GC = paymentApproval.AV_GC;
			newQuote.QU_ProviderCode = messageType ?? ZString.Empty;
			newQuote.QU_ProviderReference = quoteProviderReference;
			newQuote.QU_ToAmount = ZDecimal.TryParse(fieldValues[XUEFieldNames.ToAmount], out var toAmount) ? toAmount : ZDecimal.Zero;
			newQuote.QU_RX_NKToCurrency = fieldValues[XUEFieldNames.ToCurrency];
			newQuote.QU_FromAmount = ZDecimal.TryParse(fieldValues[XUEFieldNames.FromAmount], out var fromAmount) ? fromAmount : ZDecimal.Zero;
			newQuote.QU_RX_NKFromCurrency = fieldValues[XUEFieldNames.FromCurrency];
			newQuote.QU_FeeAmount = ZDecimal.TryParse(fieldValues[XUEFieldNames.FeeAmount], out var feeAmount) ? feeAmount : ZDecimal.Zero;
			newQuote.QU_RX_NKFeeCurrency = fieldValues[XUEFieldNames.FeeCurrency];
			newQuote.QU_ExchangeRate = ZDecimal.TryParse(fieldValues[XUEFieldNames.ExchaneRate], out var exRate) ? exRate : ZDecimal.Zero;
			newQuote.QU_ExchangeRateInverted = ZDecimal.TryParse(fieldValues[XUEFieldNames.ExchaneRateInverted], out var exRateInverted) ? exRateInverted : ZDecimal.Zero;
			var eventTime = eventObject.EventTime.GetValueOrDefault().ToZDateTime();
			newQuote.QU_LastResponseReceivedUtc = eventTime.IsEmpty ? ZDateTime.UtcNow : eventTime;
			newQuote.QU_Status = EPaymentStatusCodes.Quote.Received;
			return newQuote;
		}

		#endregion

		#region HandleIRJMessage

		void HandleIRJMessage(IXmlSessionTracker logger, UniversalEvent eventObject, AccEPaymentDeal deal)
		{
			if (!CheckForMissingOrEmptyContextFeilds(logger, eventObject, out var fieldValues) && CheckDealIsInRequestedStatus(logger, deal, out var paymentApprovalReference))
			{
				var messageType = EventParameters.GetEventParameter(EventReferenceParameters.Codes.MessageType, eventObject.EventParameters);
				var errorType = fieldValues[XUEFieldNames.ErrorType];
				if (errorType == IRJErrorTypes.Generic)
				{
					var errorOriginatedAt = fieldValues[XUEFieldNames.ErrorOriginatesAt];
					var errorMessage = fieldValues[XUEFieldNames.ErrorMessage];
					if (errorOriginatedAt == GEPErrorSource.Provider)
					{
						deal.AED_Status = EPaymentStatusCodes.Deal.Declined;
						if (deal.AED_ProviderCode == EPaymentProviderCodes.Codes.OFX && errorMessage.Contains((NoResString)"Our rates are updating, please try again", StringComparison.InvariantCultureIgnoreCase)) // OFX API message does not need to be translated.
						{
							errorMessage = (NoResString)"OFX Quote expired - please try again"; // Modified OFX API message does not need to be translated.
						}
						SetDealErrorDescription(deal, errorMessage);
						SetLastResponseReceivedUtc(eventObject, deal);
						var reference = $"E-Payment Deal declined by {messageType} with error: {errorMessage}"; // Log Reference does not need to be translated.
						EPaymentLogManager.SetEPaymentLogReference(deal, Events.InterchangeRejected, reference);
					}
					else if (new ZString[] { GEPErrorSource.XT, GEPErrorSource.UnhandledXTException, GEPErrorSource.Multiple }.Contains(errorOriginatedAt))
					{
						deal.AED_Status = EPaymentStatusCodes.Deal.SubmissionFailed;
						AccEPaymentHelper.InvalidateStaffTokenIfRefreshTokenInvalid(errorMessage, deal.AED_SystemCreateUser, deal.AED_GC_Company, logger);
						SetDealErrorDescription(deal, errorMessage);
						var reference = $"{messageType} E-Payment Error: {errorMessage}"; // Log Reference does not need to be translated.
						EPaymentLogManager.SetEPaymentLogReference(deal, Events.InterchangeRejected, reference);
					}
					else
					{
						LoggerWrapper.ReportAndLogError(logger, LogType.Error, Res.GetString("4da30067-7f37-4421-8e77-84add1f3251f", "Unexpected {0} [{1}] encountered.", "GEPErrorSource", errorOriginatedAt));
					}
				}
				else if (errorType == IRJErrorTypes.QuoteOutsideTolerance)
				{
					if (deal.Quote.QU_Status != EPaymentStatusCodes.Quote.Accepted)
					{
						logger.LogBoth(LogType.Warning, Res.GetString("73d69904-0643-4eb7-9ef2-1748d5ec1c3a", "Expected E-Quote Status is 'Accepted', but current Payment Approval {0} has E-Quote {1} with status '{2}'.", deal.Quote.QU_InternalReference, paymentApprovalReference, deal.Quote.QU_Status));
					}
					deal.Quote.QU_Status = EPaymentStatusCodes.Quote.Discarded;
					SetDealErrorDescription(deal, (NoResString)"Quote Expired. Please review the new e-quote."); // Error description does not need to be translated.
					AccEPaymentQuoteDataContextManager.SetQuoteErrorDescription(deal.Quote, QuoteExpiredMessage);
					CreateNewQuote(eventObject, deal.Quote.PaymentApproval, fieldValues, messageType, fieldValues[XUEFieldNames.QuoteProviderRef]);
					deal.AED_Status = EPaymentStatusCodes.Deal.Declined;
					SetLastResponseReceivedUtc(eventObject, deal);
					var reference = $"Quote {deal.Quote.QU_InternalReference} expired. New E-Quote received for approval."; // Log Reference does not need to be translated.
					EPaymentLogManager.SetEPaymentLogReference(deal, Events.InterchangeRejected, reference);
				}
				else
				{
					LoggerWrapper.ReportAndLogError(logger, LogType.Error, Res.GetString("6f2e5901-d421-4a18-af09-f01e162a1d0b", "Unexpected Error Type [{0}] encountered for IRJ event.", errorType));
				}
			}
		}

		void SetDealErrorDescription(AccEPaymentDeal deal, ZString errorMessage)
		{
			deal.AED_ErrorDescription = AccEPaymentHelper.GetErrorDescription(errorMessage, AccEPaymentDealSchema.AED_ErrorDescription.MaxLength);
		}

		#endregion

		#region Common Methods

		ZString QuoteExpiredMessage => (NoResString)"Quote Expired"; // Error description does not need to be translated.

		void SetLastResponseReceivedUtc(UniversalEvent eventObject, AccEPaymentDeal deal)
		{
			var eventTime = eventObject.EventTime.GetValueOrDefault();
			deal.AED_LastResponseReceivedUtc = eventTime.IsEmpty ? ZDateTime.UtcNow : eventTime.ToZDateTime();
		}

		bool CheckDealIsInRequestedStatus(IXmlSessionTracker logger, AccEPaymentDeal deal, out ZString paymentApprovalReference)
		{
			var dealIsInRequestedStatus = false;
			var quote = deal.Quote;
			paymentApprovalReference = ZString.Empty;

			if (quote == null)
			{
				LoggerWrapper.ReportAndLogError(logger, LogType.Error, Res.GetString("624679c1-fbe4-41c2-b7b8-47a977d753b3", "The Deal {0} is not attached to an existing Quote.", deal.AED_InternalReference));
			}
			else
			{
				var paymentApproval = quote.PaymentApproval;
				if (paymentApproval == null)
				{
					LoggerWrapper.ReportAndLogError(logger, LogType.Error, Res.GetString("1966fd5d-6810-446e-9139-4e2b77446f2c", "The Deal {0} is attached to Quote {1}, but Quote is not attached to an existing Payment Approval.", deal.AED_InternalReference, quote.QU_InternalReference));
				}
				else
				{
					paymentApprovalReference = paymentApproval.AV_PaymentApprovalReference;
					if (deal.AED_Status != EPaymentStatusCodes.Deal.Requested)
					{
						logger.LogBoth(LogType.Error, Res.GetString("59d6337b-c6cb-4d65-9508-b4a861c5e57b", "Unable to import incoming E-Payment Response related to Deal {0}, Payment Approval {1}. E-Payment Responses can be imported only when Deal is in 'Requested' status, but current status is '{2}'.", deal.AED_InternalReference, paymentApprovalReference, deal.AED_Status));
					}
					else
					{
						dealIsInRequestedStatus = true;
					}
				}
			}
			return dealIsInRequestedStatus;
		}

		bool CheckForMissingOrEmptyContextFeilds(IXmlSessionTracker logger, UniversalEvent eventObject, out Dictionary<ZString, ZString> fieldValues)
		{
			var isMissingFields = false;
			var eventType = eventObject.EventType.GetValueOrDefault();
			fieldValues = eventObject.ContextCollection.Where(x => x.Type.Type.HasValue && x.Value.HasValue && !x.Value.Value.IsEmpty).ToDictionary(x => x.Type.Type.Value, x => x.Value.Value);

			if (eventType == Events.InterchangeAcknowledgedCode)
			{
				var messageSubType = EventParameters.GetEventParameter(EventReferenceParameters.Codes.MessageSubType, eventObject.EventParameters) ?? ZString.Empty;
				var missingFieldsForIAKEvent = XUEFieldNames.GetRequiredFieldsForIAKEvent(messageSubType).Except(fieldValues.Keys);
				ReportMissingFieldsIfRequired(missingFieldsForIAKEvent);
			}
			else if (eventType == Events.InterchangeRejectedCode)
			{
				if (fieldValues.TryGetValue(XUEFieldNames.ErrorType, out var errorType))
				{
					var missingFieldsForIRJEvent = XUEFieldNames.GetRequiredFieldsForIRJEvent(errorType).Except(fieldValues.Keys);
					ReportMissingFieldsIfRequired(missingFieldsForIRJEvent);
				}
				else
				{
					ReportMissingFieldsIfRequired(new ZString[] { XUEFieldNames.ErrorType });
				}
			}
			return isMissingFields;

			void ReportMissingFieldsIfRequired(IEnumerable<ZString> missingFields)
			{
				if (missingFields.Any())
				{
					LoggerWrapper.ReportAndLogError(logger, LogType.Error, Res.GetString("c90b6d4c-5e75-48cb-80cd-5b29249b4930", @"The following Context fields are either empty or missing:
{0}", string.Join(System.Environment.NewLine, missingFields)));
					isMissingFields = true;
				}
			}
		}

		#endregion

		protected override ZQuery GetDataContextKeyMatchingQuery(IDataContextMatchingKey matchingValues, BusinessObjectFactory factory, IXmlImportLogger logger)
		{
			Argument.NotNull(matchingValues, nameof(matchingValues));
			Argument.NotNull(factory, nameof(factory));
			var dealNumber = matchingValues.Key;
			if (dealNumber.IsEmpty || !(matchingValues.DataObject is UniversalEvent universalEvent))
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

			var query = new ZQuery(AccEPaymentDealSchema.AED_InternalReference, dealNumber);
			query.AddToFilter(new ZQuery(AccEPaymentDealSchema.AED_GC_Company, company?.PK ?? ZGuid.Empty));
			query.AddToFilter(new ZQuery(AccEPaymentDealSchema.AED_ProviderCode, providerCode));

			return query;
		}

		protected override IEnumerable<KeyValuePair<TypeWithDescription, IZType>> GetEventContextValues()
		{
			return Enumerable.Empty<KeyValuePair<TypeWithDescription, IZType>>();
		}

		protected override EventParentFinder GetEventParentFinder(BusinessObjectFactory factory, IXmlImportLogger logger)
		{
			return new AccEPaymentDealEventParentFinder(factory, this, logger);
		}
	}
}
