using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.PaymentApproval;
using Enterprise.Accounting.ElectronicPayment.Universal;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Registry.Business;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.UniversalDataBuss.Management.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Accounting.Business.AccEPaymentDealLookups;
using static Enterprise.Accounting.ElectronicPayment.Universal.AccEPaymentDealMessageConstants;
using ProviderCodes = Enterprise.MasterFiles.Business.EPaymentProviderCodes.Codes;
using UniversalEventDataObject = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Accounting.ElectronicPayment.Testing.Universal
{
	[TestedType(typeof(AccEPaymentDealDataContextManager))]
	public class AccEPaymentDealDataContextManagerTest : DataContextManagerTestCase<AccEPaymentDealDataContextManager, AccEPaymentDeal>
	{
		#region IAK Message

		public void TestIAKMessage_NewQuoteWasWithinExchangeTolerance_RegistryIsChangedAfterPaymentProviderResponse_PaymentApprovalStatusRemainsApproved()
		{
			var deal1 = Factory.NewWithValidTestData<AccEPaymentDeal>();
			deal1.AED_InternalReference = "00001000";
			deal1.AED_Status = EPaymentStatusCodes.Deal.Requested;

			var paymentApproval = deal1.Quote.PaymentApproval;
			paymentApproval.AV_GC = GlbCompany.CurrentCompany.PK;
			paymentApproval.AV_PaymentApprovalReference = "00001000";
			paymentApproval.AV_Ledger = LedgerTypes.AccountsPayable;
			paymentApproval.AV_RX_NKPaymentCurrency = Core.Constants.CurrencyCodes.EuropeanUnion;
			paymentApproval.AV_PayExRate = 0.6166;
			paymentApproval.AV_Amount = 678.26;
			paymentApproval.AV_Status = PaymentApprovalStatus.FullyApproved;

			var oldQuote = deal1.Quote;
			oldQuote.QU_InternalReference = "00001000";
			oldQuote.QU_Status = EPaymentStatusCodes.Quote.Accepted;
			oldQuote.QU_LastResponseReceivedUtc = ZDateTime.UtcNow.AddHours(-3);
			oldQuote.QU_ExchangeRate = 0.6166;
			oldQuote.QU_ExchangeRateInverted = 1.6218;
			oldQuote.QU_FromAmount = 1100.00;
			oldQuote.QU_RX_NKFromCurrency = Core.Constants.CurrencyCodes.Australia;
			oldQuote.QU_ToAmount = 678.26;
			oldQuote.QU_RX_NKToCurrency = Core.Constants.CurrencyCodes.EuropeanUnion;
			oldQuote.QU_FeeAmount = 15.00;
			oldQuote.QU_RX_NKFeeCurrency = Core.Constants.CurrencyCodes.Australia;
			oldQuote.QU_ProviderReference = "apple";

			Factory.SaveForTesting();

			var exRateToleranceConfig = new ExchangeRateToleranceConfiguration();
			exRateToleranceConfig.ExchangeRateToleranceCollection.RemoveAndDeleteAll();
			var toleranceForEUR = new ExchangeRateTolerance
			{
				Currency = Core.Constants.CurrencyCodes.EuropeanUnion,
				ExchangeRateTolerancePercentage = 5
			};
			exRateToleranceConfig.ExchangeRateToleranceCollection.Add(toleranceForEUR);

			//base amount = 1100
			//tolerance amount = 1100 * 5% = 55
			//maximum amount = 1100 + 55 = 1155

			//new quote within tolerance, before registry was changed to 5%
			//new quote from amount = 1525.89
			//new exchange rate = 0.4445

			var paymentAuthSettingCollection = new PaymentAuthorisationSettingsCollection();
			var paymentAuthSetting1 = paymentAuthSettingCollection.AddNew();
			paymentAuthSetting1.Amount = 500m;
			paymentAuthSetting1.AuthorisationRequirement = AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.FirstApprovalRequiredOnly;
			paymentAuthSetting1.Range = AmountBasedMultiLevelAuthorisationRequirement.RangeCodes.UpTo;
			var paymentAuthSetting2 = paymentAuthSettingCollection.AddNew();
			paymentAuthSetting2.Amount = 500m;
			paymentAuthSetting2.AuthorisationRequirement = AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.SecondApprovalRequiredOnly;
			paymentAuthSetting2.Range = AmountBasedMultiLevelAuthorisationRequirement.RangeCodes.Above;

			Env.Security.APPaymentProcessingFirstApproval.IsAllowed = false;
			Env.Security.APPaymentProcessingSecondApproval.IsAllowed = false;

			using (AccountingConfigurationRegistry.Instance.PayableAuthorizationSettings.SetTemporaryValue(paymentApproval.AV_GC.ToGuid(), Guid.Empty, Guid.Empty, paymentAuthSettingCollection))
			using (AccountingConfigurationRegistry.Instance.ExchangeRateTolerance.SetTemporaryValue(paymentApproval.AV_GC.ToGuid(), Guid.Empty, Guid.Empty, exRateToleranceConfig))
			{
				var eventDataObject = GetNewIncomingIAKMessage(true, "apple||banana", "1525.89", "0.4445", "2.2497");
				eventDataObject.DataContext.AddDataTarget(DataContextType.AccEPaymentDeal, deal1.AED_InternalReference);

				var message = GetQueuedUniversalEventMessage(eventDataObject, UniversalXmlInfo.Namespace_2012_11);
				var serviceTaskLog = new ServiceTaskLogForTesting();
				var manager = new UniversalMessageProcessingManager(serviceTaskLog);
				var importResults = manager.Process(message).ImportResults;

				var expectedImportResult = $"Linked Event to {deal1.HumanReadableName}.";
				AssertEquals(expectedImportResult, importResults.Single().ToString());
				Assert(!serviceTaskLog.HasErrors());
				AssertEquals(EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);

				AssertEquals("Old quote is discarded", EPaymentStatusCodes.Quote.Discarded, oldQuote.QU_Status);
				var newQuote = Factory.LoadTop1<AccEPaymentQuote>(new ZQuery(AccEPaymentQuoteSchema.PK, SQLComparisonOperator.NotEqual, oldQuote.PK));

				AssertNotNull(newQuote);
				AssertEquals("orange", newQuote.QU_ProviderReference);
				AssertEquals(1525.89m, newQuote.QU_FromAmount);
				AssertEquals(Core.Constants.CurrencyCodes.Australia, newQuote.QU_RX_NKFromCurrency);
				AssertEquals(678.26m, newQuote.QU_ToAmount);
				AssertEquals(Core.Constants.CurrencyCodes.EuropeanUnion, newQuote.QU_RX_NKToCurrency);
				AssertEquals(15m, newQuote.QU_FeeAmount);
				AssertEquals(Core.Constants.CurrencyCodes.Australia, newQuote.QU_RX_NKFeeCurrency);
				AssertEquals(0.4445m, newQuote.QU_ExchangeRate);
				AssertEquals(2.2497m, newQuote.QU_ExchangeRateInverted);
				AssertEquals(EventTime, newQuote.QU_LastResponseReceivedUtc);

				AssertEquals("Deal is linked to new quote", deal1.Quote.PK, newQuote.PK);
				AssertEquals("New quote is accepted", EPaymentStatusCodes.Quote.Accepted, newQuote.QU_Status);

				AssertEquals(PaymentApprovalStatus.FullyApproved, paymentApproval.AV_Status);
				AssertEquals(0.4445m, paymentApproval.AV_PayExRate.Round(4));
				AssertEquals(678.26m, paymentApproval.AV_Amount);
				AssertEquals(Core.Constants.CurrencyCodes.EuropeanUnion, paymentApproval.AV_RX_NKPaymentCurrency);
				var paymentApprovalBase = Factory.LoadTop1<PaymentApprovalBase>(new ZQuery(AccPaymentApprovalSchema.PK, paymentApproval.PK));
				AssertEquals(1525.89m, paymentApprovalBase.AV_Calc_LocalAmount);
				AssertEquals(Core.Constants.CurrencyCodes.Australia, paymentApprovalBase.AV_Calc_LocalCurrency);

				AssertEquals(EPaymentStatusCodes.Deal.Accepted, deal1.AED_Status);
				AssertEquals("4acb60dc-83a1-467d-916f-dad748ed6294", deal1.AED_ProviderReference);
				AssertEquals(EventTime, deal1.AED_LastResponseReceivedUtc);
				AssertEquals(ZString.Empty, deal1.AED_ErrorDescription);

				var expectedLogReference = "E-Payment Deal 00001000 accepted by OFX.";
				var dealIAKLogs = deal1.Logs.Find(l => l.SL_SE_NKEvent == Events.InterchangeAcknowledgedCode && l.SL_Reference == expectedLogReference);
				AssertEquals(1, dealIAKLogs.Count());
				var ediMessageLinkedToDealIAKLog = dealIAKLogs.First().RelatedEDIMessage?.Message;
				AssertNotNull(ediMessageLinkedToDealIAKLog);

				var paymentApprovalIAKLogs = paymentApproval.Logs.Find(l => l.SL_SE_NKEvent == Events.InterchangeAcknowledgedCode && l.SL_Reference == expectedLogReference);
				AssertEquals(1, paymentApprovalIAKLogs.Count());
				var ediMessageLinkedToPaymentApprovalIAKLog = paymentApprovalIAKLogs.First().RelatedEDIMessage?.Message;
				AssertNotNull(ediMessageLinkedToPaymentApprovalIAKLog);
				AssertEquals(ediMessageLinkedToDealIAKLog.PK, ediMessageLinkedToPaymentApprovalIAKLog.PK);
			}
		}

		public void TestIAKMessageWithNewQuote_OldQuoteIsNotInACPStatus_OldQuoteIsNotInExpiredQuotesList()
		{
			TestIAKMessageWithNewQuote(false, false);
		}

		public void TestIAKMessageWithNewQuote_OldQuoteIsNotInACPStatus_OldQuoteIsInExpiredQuotesList()
		{
			TestIAKMessageWithNewQuote(false, true);
		}

		public void TestIAKMessageWithNewQuote_OldQuoteIsInACPStatus_OldQuoteIsNotInExpiredQuotesList()
		{
			TestIAKMessageWithNewQuote(true, false);
		}

		public void TestIAKMessageWithNewQuote_OldQuoteIsInACPStatus_OldQuoteIsInExpiredQuotesList()
		{
			TestIAKMessageWithNewQuote(true, true);
		}

		public void TestIAKMessage()
		{
			var deal1 = Factory.NewWithValidTestData<AccEPaymentDeal>();
			deal1.AED_InternalReference = "00001000";
			deal1.AED_Status = EPaymentStatusCodes.Deal.Requested;
			Factory.SaveForTesting();

			var paymentApproval = deal1.Quote.PaymentApproval;
			paymentApproval.AV_RX_NKPaymentCurrency = Core.Constants.CurrencyCodes.EuropeanUnion;
			paymentApproval.AV_PayExRate = 0.6166;
			paymentApproval.AV_Amount = 678.26;

			var quote = deal1.Quote;
			quote.QU_Status = EPaymentStatusCodes.Quote.Accepted;
			quote.QU_LastResponseReceivedUtc = ZDateTime.UtcNow.AddHours(-3);
			quote.QU_ExchangeRate = 0.6166;
			quote.QU_ExchangeRateInverted = 1.6218;
			quote.QU_FromAmount = 1100.00;
			quote.QU_RX_NKFromCurrency = Core.Constants.CurrencyCodes.Australia;
			quote.QU_ToAmount = 678.26;
			quote.QU_RX_NKToCurrency = Core.Constants.CurrencyCodes.EuropeanUnion;
			quote.QU_FeeAmount = 15.00;
			quote.QU_RX_NKFeeCurrency = Core.Constants.CurrencyCodes.Australia;
			quote.QU_ProviderReference = "orange";

			Factory.SaveForTesting();

			var eventDataObject = GetNewIncomingIAKMessage();
			eventDataObject.DataContext.AddDataTarget(DataContextType.AccEPaymentDeal, deal1.AED_InternalReference);

			var message = GetQueuedUniversalEventMessage(eventDataObject, UniversalXmlInfo.Namespace_2012_11);
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			var importResults = manager.Process(message).ImportResults;

			var expectedImportResult = FormattableString.Invariant(
$@"Linked Event to {deal1.HumanReadableName}.");

			AssertEquals(expectedImportResult, importResults.Single().ToString());
			Assert(!serviceTaskLog.HasErrors());
			AssertEquals(EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);

			AssertEquals(EPaymentStatusCodes.Deal.Accepted, deal1.AED_Status);
			AssertEquals(EventTime, deal1.AED_LastResponseReceivedUtc);
			AssertEquals(ZString.Empty, deal1.AED_ErrorDescription);

			var expectedLogReference = "E-Payment Deal 00001000 accepted by OFX.";
			var dealIAKLogs = deal1.Logs.Find(l => l.SL_SE_NKEvent == Events.InterchangeAcknowledgedCode && l.SL_Reference == expectedLogReference);
			AssertEquals(1, dealIAKLogs.Count());
			var ediMessageLinkedToDealIAKLog = dealIAKLogs.First().RelatedEDIMessage?.Message;
			AssertNotNull(ediMessageLinkedToDealIAKLog);

			var paymentApprovalIAKLogs = paymentApproval.Logs.Find(l => l.SL_SE_NKEvent == Events.InterchangeAcknowledgedCode && l.SL_Reference == expectedLogReference);
			AssertEquals(1, paymentApprovalIAKLogs.Count());
			var ediMessageLinkedToPaymentApprovalIAKLog = paymentApprovalIAKLogs.First().RelatedEDIMessage?.Message;
			AssertNotNull(ediMessageLinkedToPaymentApprovalIAKLog);
			AssertEquals(ediMessageLinkedToDealIAKLog.PK, ediMessageLinkedToPaymentApprovalIAKLog.PK);
		}

		public void TestIAKMessage_UpdateDealStatusWhenCancelledInCW1()
		{
			var deal1 = Factory.NewWithValidTestData<AccEPaymentDeal>();
			deal1.AED_InternalReference = "00001000";
			deal1.AED_Status = EPaymentStatusCodes.Deal.Cancelled;
			Factory.SaveForTesting();

			var paymentApproval = deal1.Quote.PaymentApproval;
			paymentApproval.AV_RX_NKPaymentCurrency = Core.Constants.CurrencyCodes.EuropeanUnion;
			paymentApproval.AV_PayExRate = 0.6166;
			paymentApproval.AV_Amount = 678.26;

			var quote = deal1.Quote;
			quote.QU_Status = EPaymentStatusCodes.Quote.Accepted;
			quote.QU_LastResponseReceivedUtc = ZDateTime.UtcNow.AddHours(-3);
			quote.QU_ExchangeRate = 0.6166;
			quote.QU_ExchangeRateInverted = 1.6218;
			quote.QU_FromAmount = 1100.00;
			quote.QU_RX_NKFromCurrency = Core.Constants.CurrencyCodes.Australia;
			quote.QU_ToAmount = 678.26;
			quote.QU_RX_NKToCurrency = Core.Constants.CurrencyCodes.EuropeanUnion;
			quote.QU_FeeAmount = 15.00;
			quote.QU_RX_NKFeeCurrency = Core.Constants.CurrencyCodes.Australia;
			quote.QU_ProviderReference = "orange";

			Factory.SaveForTesting();

			var eventDataObject = GetNewIncomingIAKMessageWithDealStatusUpdate();
			eventDataObject.DataContext.AddDataTarget(DataContextType.AccEPaymentDeal, deal1.AED_InternalReference);

			var message = GetQueuedUniversalEventMessage(eventDataObject, UniversalXmlInfo.Namespace_2012_11);
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			var importResults = manager.Process(message).ImportResults;

			var expectedError = "Cannot update deal status to 'PAI' due to deal has been canceled in CW1 already";

			var expectedImportResult = FormattableString.Invariant(
$@"Linked Event to {deal1.HumanReadableName}.
Error - {expectedError}");

			AssertEquals(expectedImportResult, importResults.Single().ToString());
			Assert(serviceTaskLog.HasErrors());
			AssertEquals(EDIMessageStatusList.Codes.Error, message.EM_Status);

			AssertEquals(EPaymentStatusCodes.Deal.Cancelled, deal1.AED_Status);

			var dealIAKLogs = deal1.Logs.Find(l => l.SL_SE_NKEvent == Events.InterchangeAcknowledgedCode);
			AssertEquals(1, dealIAKLogs.Count());
			var ediMessageLinkedToDealIAKLog = dealIAKLogs.First().RelatedEDIMessage?.Message;
			AssertNotNull(ediMessageLinkedToDealIAKLog);

			var paymentApprovalIAKLogs = paymentApproval.Logs.Find(l => l.SL_SE_NKEvent == Events.InterchangeAcknowledgedCode);
			AssertEquals(1, paymentApprovalIAKLogs.Count());
			var ediMessageLinkedToPaymentApprovalIAKLog = paymentApprovalIAKLogs.First().RelatedEDIMessage?.Message;
			AssertNotNull(ediMessageLinkedToPaymentApprovalIAKLog);
			AssertEquals(ediMessageLinkedToDealIAKLog.PK, ediMessageLinkedToPaymentApprovalIAKLog.PK);

			ErrorReporter.Instance.Clear();
		}

		public void TestIAKMessage_DealStatusIsNotREQ()
		{
			var deal1 = Factory.NewWithValidTestData<AccEPaymentDeal>();
			deal1.AED_InternalReference = "00001000";
			deal1.AED_Status = EPaymentStatusCodes.Deal.Cancelled;
			Factory.SaveForTesting();

			var eventDataObject = GetNewIncomingIAKMessage();
			eventDataObject.DataContext.AddDataTarget(DataContextType.AccEPaymentDeal, deal1.AED_InternalReference);

			var message = GetQueuedUniversalEventMessage(eventDataObject, UniversalXmlInfo.Namespace_2012_11);
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			var importResults = manager.Process(message).ImportResults;

			var expectedError = $"Unable to import incoming E-Payment Response related to Deal 00001000, Payment Approval {deal1.Quote.PaymentApproval.AV_PaymentApprovalReference}. E-Payment Responses can be imported only when Deal is in 'Requested' status, but current status is 'CAN'.";

			var expectedImportResult = FormattableString.Invariant(
$@"Linked Event to {deal1.HumanReadableName}.
Error - {expectedError}");

			AssertEquals(expectedImportResult, importResults.Single().ToString());
			Assert(serviceTaskLog.HasErrors());
			AssertEquals(EDIMessageStatusList.Codes.Error, message.EM_Status);

			AssertEquals(EPaymentStatusCodes.Deal.Cancelled, deal1.AED_Status);
			AssertEquals(ZDateTime.Empty, deal1.AED_LastResponseReceivedUtc);
			AssertEquals(ZString.Empty, deal1.AED_ErrorDescription);

			var dealIAKLogs = deal1.Logs.Find(l => l.SL_SE_NKEvent == Events.InterchangeAcknowledgedCode);
			AssertEquals(1, dealIAKLogs.Count());
			var ediMessageLinkedToDealIAKLog = dealIAKLogs.First().RelatedEDIMessage?.Message;
			AssertNotNull(ediMessageLinkedToDealIAKLog);

			var paymentApprovalIAKLogs = deal1.Quote.PaymentApproval.Logs.Find(l => l.SL_SE_NKEvent == Events.InterchangeAcknowledgedCode);
			AssertEquals(1, paymentApprovalIAKLogs.Count());
			var ediMessageLinkedToPaymentApprovalIAKLog = paymentApprovalIAKLogs.First().RelatedEDIMessage?.Message;
			AssertNotNull(ediMessageLinkedToPaymentApprovalIAKLog);
			AssertEquals(ediMessageLinkedToDealIAKLog.PK, ediMessageLinkedToPaymentApprovalIAKLog.PK);
		}

		public void TestIAKMessage_DealNotFound()
		{
			AssertEquals(0, Factory.Load<AccEPaymentDeal>(new ZQuery()).Length);

			var eventDataObject = GetNewIncomingIAKMessage();
			eventDataObject.DataContext.AddDataTarget(DataContextType.AccEPaymentDeal, "00001000");

			var message = GetQueuedUniversalEventMessage(eventDataObject, UniversalXmlInfo.Namespace_2012_11);
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);
			AssertEquals("Warning - No Module found a Business Entity to link this Universal Event to.", serviceTaskLog.ToString());
			AssertEquals(EDIMessageStatusList.Codes.Discarded, message.EM_Status);
		}

		public void TestIAKMessage_MissingContexts()
		{
			var deal1 = Factory.NewWithValidTestData<AccEPaymentDeal>();
			deal1.AED_InternalReference = "00001000";
			deal1.AED_Status = EPaymentStatusCodes.Deal.Requested;
			Factory.SaveForTesting();

			var eventDataObject = GetNewIncomingIAKMessage(false);
			eventDataObject.DataContext.AddDataTarget(DataContextType.AccEPaymentDeal, deal1.AED_InternalReference);

			var message = GetQueuedUniversalEventMessage(eventDataObject, UniversalXmlInfo.Namespace_2012_11);
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			var importResults = manager.Process(message).ImportResults;

			var expectedError = @"The following Context fields are either empty or missing:
DealId
Status
QuoteIdUsedForPayment
FromAmount
FromCurrency
ToAmount
ToCurrency
FeeAmount
FeeCurrency
ExchangeRate
ExchangeRateInverted";

			var expectedImportResult = FormattableString.Invariant(
$@"Linked Event to {deal1.HumanReadableName}.
Error - {expectedError}");

			AssertEquals(expectedError, ErrorReporter.LastMessageReported);
			AssertEquals(expectedImportResult, importResults.Single().ToString());
			Assert(serviceTaskLog.HasErrors());
			AssertEquals(EDIMessageStatusList.Codes.Error, message.EM_Status);

			AssertEquals(EPaymentStatusCodes.Deal.Requested, deal1.AED_Status);
			AssertEquals(ZDateTime.Empty, deal1.AED_LastResponseReceivedUtc);
			AssertEquals(ZString.Empty, deal1.AED_ErrorDescription);

			var dealIAKLogs = deal1.Logs.Find(l => l.SL_SE_NKEvent == Events.InterchangeAcknowledgedCode);
			AssertEquals(1, dealIAKLogs.Count());
			var ediMessageLinkedToDealIAKLog = dealIAKLogs.First().RelatedEDIMessage?.Message;
			AssertNotNull(ediMessageLinkedToDealIAKLog);

			var paymentApprovalIAKLogs = deal1.Quote.PaymentApproval.Logs.Find(l => l.SL_SE_NKEvent == Events.InterchangeAcknowledgedCode);
			AssertEquals(1, paymentApprovalIAKLogs.Count());
			var ediMessageLinkedToPaymentApprovalIAKLog = paymentApprovalIAKLogs.First().RelatedEDIMessage?.Message;
			AssertNotNull(ediMessageLinkedToPaymentApprovalIAKLog);
			AssertEquals(ediMessageLinkedToDealIAKLog.PK, ediMessageLinkedToPaymentApprovalIAKLog.PK);

			ErrorReporter.Instance.Clear();
		}

		public void TestIAKMessage_MissingQuote()
		{
			var deal1 = Factory.NewWithValidTestData<AccEPaymentDeal>();
			deal1.AED_InternalReference = "00001000";
			deal1.AED_Status = EPaymentStatusCodes.Deal.Requested;
			Factory.SaveForTesting();

			var eventDataObject = GetNewIncomingIAKMessage(true);
			eventDataObject.DataContext.AddDataTarget(DataContextType.AccEPaymentDeal, deal1.AED_InternalReference);

			var message = GetQueuedUniversalEventMessage(eventDataObject, UniversalXmlInfo.Namespace_2012_11);
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);

			deal1.AED_QU_Quote = ZGuid.BrettsGuid;
			var importResults = manager.Process(message).ImportResults;

			var expectedError = $"The Deal {deal1.AED_InternalReference} is not attached to an existing Quote.";

			var expectedImportResult = FormattableString.Invariant(
$@"Linked Event to {deal1.HumanReadableName}.
Error - {expectedError}");

			AssertEquals(expectedError, ErrorReporter.LastMessageReported);
			AssertEquals(expectedImportResult, importResults.Single().ToString());
			AssertEquals(1, deal1.Logs.Find(l => l.SL_SE_NKEvent == Events.InterchangeAcknowledgedCode).Count());
			Assert(serviceTaskLog.HasErrors());
			AssertEquals(EDIMessageStatusList.Codes.Error, message.EM_Status);

			AssertEquals(EPaymentStatusCodes.Deal.Requested, deal1.AED_Status);
			AssertEquals(ZDateTime.Empty, deal1.AED_LastResponseReceivedUtc);
			AssertEquals(ZString.Empty, deal1.AED_ErrorDescription);

			ErrorReporter.Instance.Clear();
		}

		public void TestIAKMessage_MissingPaymentApproval()
		{
			var deal1 = Factory.NewWithValidTestData<AccEPaymentDeal>();
			deal1.AED_InternalReference = "00001000";
			deal1.AED_Status = EPaymentStatusCodes.Deal.Requested;
			Factory.SaveForTesting();

			var eventDataObject = GetNewIncomingIAKMessage(true);
			eventDataObject.DataContext.AddDataTarget(DataContextType.AccEPaymentDeal, deal1.AED_InternalReference);

			var message = GetQueuedUniversalEventMessage(eventDataObject, UniversalXmlInfo.Namespace_2012_11);
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);

			deal1.Quote.QU_AV = ZGuid.BrettsGuid;
			var importResults = manager.Process(message).ImportResults;

			var expectedError = $"The Deal {deal1.AED_InternalReference} is attached to Quote {deal1.Quote.QU_InternalReference}, but Quote is not attached to an existing Payment Approval.";

			var expectedImportResult = FormattableString.Invariant(
$@"Linked Event to {deal1.HumanReadableName}.
Error - {expectedError}");

			AssertEquals(expectedError, ErrorReporter.LastMessageReported);
			AssertEquals(expectedImportResult, importResults.Single().ToString());
			AssertEquals(1, deal1.Logs.Find(l => l.SL_SE_NKEvent == Events.InterchangeAcknowledgedCode).Count());
			Assert(serviceTaskLog.HasErrors());
			AssertEquals(EDIMessageStatusList.Codes.Error, message.EM_Status);

			AssertEquals(EPaymentStatusCodes.Deal.Requested, deal1.AED_Status);
			AssertEquals(ZDateTime.Empty, deal1.AED_LastResponseReceivedUtc);
			AssertEquals(ZString.Empty, deal1.AED_ErrorDescription);

			ErrorReporter.Instance.Clear();
		}

		#endregion

		#region IRJ Message

		public void TestIRJMessageWithNewQuote_OldQuoteIsNotInACPStatus()
		{
			TestIRJMessageWithNewQuote(false);
		}

		public void TestIRJMessageWithNewQuote_OldQuoteIsInACPStatus()
		{
			TestIRJMessageWithNewQuote(true);
		}

		public void TestIRJMessage_ProviderDeclinedPaymentGeneric()
		{
			TestIRJMessage_ProviderDeclinedPayment();
		}

		public void TestIRJMessage_OFXDeclinedPaymentBecauseQuoteExpired()
		{
			TestIRJMessage_ProviderDeclinedPayment(recievedError: "DE:CreateDeal:0003: Our rates are updating, please try again", expectedError: "OFX Quote expired - please try again");
		}

		public void TestIRJMessage_StaffTokenInvalidatedWhenNoRefreshToken()
		{
			var objectFactory = new BusinessObjectFactory();
			var objectCreator = new TestObjectCreator(objectFactory);
			var testHelper = new EPaymentTestHelper(objectCreator);
			var creatingUser = objectCreator.CreateStaff("YOU");
			var ofxBankAccount = testHelper.CreateOFXPaymentProviderBankAccount(GlbCompany.CurrentCompany.PK);
			var staffToken = testHelper.CreateStaffToken(ofxBankAccount.PK, ZDateTime.UtcNow.AddDays(1), GlbCompany.CurrentCompany.PK, creatingUser.GS_Code, AccEPaymentStaffTokenLookups.StatusCodes.Authorised);
			staffToken.TK_AccountName = "This name is provided by OFX.";
			objectFactory.Save();

			AssertEquals(AccEPaymentStaffTokenLookups.StatusCodes.Authorised, staffToken.TK_Status);

			var deal1 = Factory.NewWithValidTestData<AccEPaymentDeal>();
			deal1.AED_InternalReference = "00001000";
			deal1.AED_Status = EPaymentStatusCodes.Deal.Requested;
			deal1.AED_SystemCreateUser = creatingUser.GS_Code;
			deal1.AED_GC_Company = staffToken.TK_GC;
			Factory.SaveForTesting();

			var eventDataObject = GetNewIncomingIRJMessage(GEPErrorSource.XT, true, true, "There is no active refresh token.");
			eventDataObject.DataContext.AddDataTarget(DataContextType.AccEPaymentDeal, deal1.AED_InternalReference);

			var message = GetQueuedUniversalEventMessage(eventDataObject, UniversalXmlInfo.Namespace_2012_11);
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			var importResults = manager.Process(message).ImportResults;

			var expectedError = "Please retry your last action or re-authorize your OFX user account from your Bank Account before proceeding.";
			var expectedImportResult = FormattableString.Invariant(
$@"Linked Event to {deal1.HumanReadableName}.");
			eventDataObject.DataContext.AddDataTarget(DataContextType.AccEPaymentBeneficiaryRequest, deal1.AED_InternalReference);

			AssertEquals(expectedImportResult, importResults.Single().ToString());
			Assert(!serviceTaskLog.HasErrors());
			AssertEquals(EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);

			AssertEquals(EPaymentStatusCodes.Deal.SubmissionFailed, deal1.AED_Status);
			AssertEquals(expectedError, deal1.AED_ErrorDescription);

			var updatedToken = staffToken.Factory.LoadTop1<AccEPaymentStaffToken>(new ZQuery(AccEPaymentStaffTokenSchema.PK, staffToken.PK));
			AssertEquals(AccEPaymentStaffTokenLookups.StatusCodes.Error, updatedToken.TK_Status);
			AssertEquals(ZDateTime.Empty, updatedToken.TK_ExpiryUtc);
		}

		void TestIRJMessage_ProviderDeclinedPayment(string recievedError = "Some weird error happened", string expectedError = "Some weird error happened")
		{
			var deal1 = Factory.NewWithValidTestData<EPaymentDeal>();
			deal1.AED_Status = EPaymentStatusCodes.Deal.Requested;
			Factory.SaveForTesting();

			var eventDataObject = GetNewIncomingIRJMessage(GEPErrorSource.Provider, reason: recievedError);
			eventDataObject.DataContext.AddDataTarget(DataContextType.AccEPaymentDeal, deal1.AED_InternalReference);

			var message = GetQueuedUniversalEventMessage(eventDataObject, UniversalXmlInfo.Namespace_2012_11);
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			var importResults = manager.Process(message).ImportResults;

			var expectedImportResult = FormattableString.Invariant(
$@"Linked Event to {deal1.HumanReadableName}.");

			AssertEquals(expectedImportResult, importResults.Single().ToString());
			Assert(!serviceTaskLog.HasErrors());
			AssertEquals(EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);

			AssertEquals(EPaymentStatusCodes.Deal.Declined, deal1.AED_Status);
			AssertEquals(EventTime, deal1.AED_LastResponseReceivedUtc);
			AssertEquals(expectedError, deal1.AED_ErrorDescription);

			var expectedLogReference = $"E-Payment Deal declined by OFX with error: {expectedError}";
			var dealIRJLogs = deal1.Logs.Find(l => l.SL_SE_NKEvent == Events.InterchangeRejectedCode && l.SL_Reference == expectedLogReference);
			AssertEquals(1, dealIRJLogs.Count());
			var ediMessageLinkedToDealIRJLog = dealIRJLogs.First().RelatedEDIMessage?.Message;
			AssertNotNull(ediMessageLinkedToDealIRJLog);

			var paymentApprovalIRJLogs = deal1.Quote.PaymentApproval.Logs.Find(l => l.SL_SE_NKEvent == Events.InterchangeRejectedCode && l.SL_Reference == expectedLogReference);
			AssertEquals(1, paymentApprovalIRJLogs.Count());
			var ediMessageLinkedToPaymentApprovalIRJLog = paymentApprovalIRJLogs.First().RelatedEDIMessage?.Message;
			AssertNotNull(ediMessageLinkedToPaymentApprovalIRJLog);
			AssertEquals(ediMessageLinkedToDealIRJLog.PK, ediMessageLinkedToPaymentApprovalIRJLog.PK);
		}

		public void TestIRJMessage_ValidationErrorHappenedInXT()
		{
			IRJMessage_UnhandledErrorHappenedInXT(GEPErrorSource.XT);
		}

		public void TestIRJMessage_UnhandledErrorHappenedInXT()
		{
			IRJMessage_UnhandledErrorHappenedInXT(GEPErrorSource.UnhandledXTException);
		}

		public void TestIRJMessage_MultipleErrorOccured()
		{
			IRJMessage_UnhandledErrorHappenedInXT(GEPErrorSource.Multiple);
		}

		void IRJMessage_UnhandledErrorHappenedInXT(string errorSource)
		{ 
			var deal1 = Factory.NewWithValidTestData<AccEPaymentDeal>();
			deal1.AED_InternalReference = "00001000";
			deal1.AED_Status = EPaymentStatusCodes.Deal.Requested;
			Factory.SaveForTesting();

			var eventDataObject = GetNewIncomingIRJMessage(errorSource);
			eventDataObject.DataContext.AddDataTarget(DataContextType.AccEPaymentDeal, deal1.AED_InternalReference);

			var message = GetQueuedUniversalEventMessage(eventDataObject, UniversalXmlInfo.Namespace_2012_11);
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			var importResults = manager.Process(message).ImportResults;

			var expectedError = "Some weird error happened";
			var expectedImportResult = FormattableString.Invariant(
$@"Linked Event to {deal1.HumanReadableName}.");

			AssertEquals(expectedImportResult, importResults.Single().ToString());
			Assert(!serviceTaskLog.HasErrors());
			AssertEquals(EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);

			AssertEquals(EPaymentStatusCodes.Deal.SubmissionFailed, deal1.AED_Status);
			AssertEquals(ZDateTime.Empty, deal1.AED_LastResponseReceivedUtc);
			AssertEquals(expectedError, deal1.AED_ErrorDescription);

			var expectedLogReference = $"OFX E-Payment Error: {expectedError}";
			var dealIRJLogs = deal1.Logs.Find(l => l.SL_SE_NKEvent == Events.InterchangeRejectedCode && l.SL_Reference == expectedLogReference);
			AssertEquals(1, dealIRJLogs.Count());
			var ediMessageLinkedToDealIRJLog = dealIRJLogs.First().RelatedEDIMessage?.Message;
			AssertNotNull(ediMessageLinkedToDealIRJLog);

			var paymentApprovalIRJLogs = deal1.Quote.PaymentApproval.Logs.Find(l => l.SL_SE_NKEvent == Events.InterchangeRejectedCode && l.SL_Reference == expectedLogReference);
			AssertEquals(1, paymentApprovalIRJLogs.Count());
			var ediMessageLinkedToPaymentApprovalIRJLog = paymentApprovalIRJLogs.First().RelatedEDIMessage?.Message;
			AssertNotNull(ediMessageLinkedToPaymentApprovalIRJLog);
			AssertEquals(ediMessageLinkedToDealIRJLog.PK, ediMessageLinkedToPaymentApprovalIRJLog.PK);
		}

		public void TestIRJMessage_MissingContexts_GenericError()
		{
			var deal1 = Factory.NewWithValidTestData<AccEPaymentDeal>();
			deal1.AED_InternalReference = "00001000";
			deal1.AED_Status = EPaymentStatusCodes.Deal.Requested;
			Factory.SaveForTesting();

			var eventDataObject = GetNewIncomingIRJMessage(GEPErrorSource.XT, populateErrorType: false, populateRequiredFields: false);
			eventDataObject.DataContext.AddDataTarget(DataContextType.AccEPaymentDeal, deal1.AED_InternalReference);

			var message = GetQueuedUniversalEventMessage(eventDataObject, UniversalXmlInfo.Namespace_2012_11);
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			var importResults = manager.Process(message).ImportResults;

			var expectedError = @"The following Context fields are either empty or missing:
ErrorType";

			var expectedImportResult = FormattableString.Invariant(
$@"Linked Event to {deal1.HumanReadableName}.
Error - {expectedError}");

			AssertEquals(expectedError, ErrorReporter.LastMessageReported);
			AssertEquals(expectedImportResult, importResults.Single().ToString());
			Assert(serviceTaskLog.HasErrors());
			AssertEquals(EDIMessageStatusList.Codes.Error, message.EM_Status);

			AssertEquals(EPaymentStatusCodes.Deal.Requested, deal1.AED_Status);
			AssertEquals(ZDateTime.Empty, deal1.AED_LastResponseReceivedUtc);
			AssertEquals(ZString.Empty, deal1.AED_ErrorDescription);

			var dealIRJLogs = deal1.Logs.Find(l => l.SL_SE_NKEvent == Events.InterchangeRejectedCode);
			AssertEquals(1, dealIRJLogs.Count());
			var ediMessageLinkedToDealIRJLog = dealIRJLogs.First().RelatedEDIMessage?.Message;
			AssertNotNull(ediMessageLinkedToDealIRJLog);

			var paymentApprovalIRJLogs = (deal1.Quote as IEPaymentLogParent).PaymentApprovalForLogging.Logs.Find(l => l.SL_SE_NKEvent == Events.InterchangeRejectedCode);
			AssertEquals(1, paymentApprovalIRJLogs.Count());
			var ediMessageLinkedToPaymentApprovalIRJLog = paymentApprovalIRJLogs.First().RelatedEDIMessage?.Message;
			AssertNotNull(ediMessageLinkedToPaymentApprovalIRJLog);
			AssertEquals(ediMessageLinkedToDealIRJLog.PK, ediMessageLinkedToPaymentApprovalIRJLog.PK);

			ErrorReporter.Clear();

			eventDataObject = GetNewIncomingIRJMessage(GEPErrorSource.XT, populateErrorType: true, populateRequiredFields: false);
			eventDataObject.DataContext.AddDataTarget(DataContextType.AccEPaymentDeal, deal1.AED_InternalReference);

			message = GetQueuedUniversalEventMessage(eventDataObject, UniversalXmlInfo.Namespace_2012_11);
			serviceTaskLog = new ServiceTaskLogForTesting();
			manager = new UniversalMessageProcessingManager(serviceTaskLog);
			importResults = manager.Process(message).ImportResults;

			expectedError = @"The following Context fields are either empty or missing:
ErrorOriginatesAt
ErrorMessage";

			expectedImportResult = FormattableString.Invariant(
$@"Linked Event to {deal1.HumanReadableName}.
Error - {expectedError}");

			AssertEquals(expectedError, ErrorReporter.LastMessageReported);
			AssertEquals(expectedImportResult, importResults.Single().ToString());
			Assert(serviceTaskLog.HasErrors());
			AssertEquals(EDIMessageStatusList.Codes.Error, message.EM_Status);

			AssertEquals(EPaymentStatusCodes.Deal.Requested, deal1.AED_Status);
			AssertEquals(ZDateTime.Empty, deal1.AED_LastResponseReceivedUtc);
			AssertEquals(ZString.Empty, deal1.AED_ErrorDescription);

			dealIRJLogs = deal1.Logs.Find(l => l.SL_SE_NKEvent == Events.InterchangeRejectedCode);
			AssertEquals(2, dealIRJLogs.Count());
			var ediMessagesLinkedToDealIRJLog = dealIRJLogs.Select(x => x.RelatedEDIMessage?.Message);
			AssertEquals(2, ediMessagesLinkedToDealIRJLog.Count());

			paymentApprovalIRJLogs = (deal1.Quote as IEPaymentLogParent).PaymentApprovalForLogging.Logs.Find(l => l.SL_SE_NKEvent == Events.InterchangeRejectedCode);
			AssertEquals(2, paymentApprovalIRJLogs.Count());
			var ediMessagesLinkedToPaymentApprovalIRJLog = paymentApprovalIRJLogs.Select(x => x.RelatedEDIMessage?.Message);
			AssertEquals(2, ediMessagesLinkedToPaymentApprovalIRJLog.Count());
			AssertContainsExactElementsInAnyOrder(ediMessagesLinkedToDealIRJLog.Select(m => m.PK), ediMessagesLinkedToPaymentApprovalIRJLog.Select(m => m.PK));

			ErrorReporter.Instance.Clear();
		}

		public void TestIRJMessage_MissingContexts_QuoteOutsideToleranceError()
		{
			var deal1 = Factory.NewWithValidTestData<AccEPaymentDeal>();
			deal1.AED_InternalReference = "00001000";
			deal1.AED_Status = EPaymentStatusCodes.Deal.Requested;
			Factory.SaveForTesting();

			var eventDataObject = GetNewIncomingIRJMessageWithNewQuoteDetails(populateErrorType: false, populateRequiredFields: false);
			eventDataObject.DataContext.AddDataTarget(DataContextType.AccEPaymentDeal, deal1.AED_InternalReference);

			var message = GetQueuedUniversalEventMessage(eventDataObject, UniversalXmlInfo.Namespace_2012_11);
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			var importResults = manager.Process(message).ImportResults;

			var expectedError = @"The following Context fields are either empty or missing:
ErrorType";

			var expectedImportResult = FormattableString.Invariant(
$@"Linked Event to {deal1.HumanReadableName}.
Error - {expectedError}");

			AssertEquals(expectedError, ErrorReporter.LastMessageReported);
			AssertEquals(expectedImportResult, importResults.Single().ToString());
			Assert(serviceTaskLog.HasErrors());
			AssertEquals(EDIMessageStatusList.Codes.Error, message.EM_Status);

			AssertEquals(EPaymentStatusCodes.Deal.Requested, deal1.AED_Status);
			AssertEquals(ZDateTime.Empty, deal1.AED_LastResponseReceivedUtc);
			AssertEquals(ZString.Empty, deal1.AED_ErrorDescription);

			var dealIRJLogs = deal1.Logs.Find(l => l.SL_SE_NKEvent == Events.InterchangeRejectedCode);
			AssertEquals(1, dealIRJLogs.Count());
			var ediMessageLinkedToDealIRJLog = dealIRJLogs.First().RelatedEDIMessage?.Message;
			AssertNotNull(ediMessageLinkedToDealIRJLog);

			var paymentApprovalIRJLogs = (deal1.Quote as IEPaymentLogParent).PaymentApprovalForLogging.Logs.Find(l => l.SL_SE_NKEvent == Events.InterchangeRejectedCode);
			AssertEquals(1, paymentApprovalIRJLogs.Count());
			var ediMessageLinkedToPaymentApprovalIRJLog = paymentApprovalIRJLogs.First().RelatedEDIMessage?.Message;
			AssertNotNull(ediMessageLinkedToPaymentApprovalIRJLog);
			AssertEquals(ediMessageLinkedToDealIRJLog.PK, ediMessageLinkedToPaymentApprovalIRJLog.PK);

			ErrorReporter.Clear();

			eventDataObject = GetNewIncomingIRJMessageWithNewQuoteDetails(populateErrorType: true, populateRequiredFields: false);
			eventDataObject.DataContext.AddDataTarget(DataContextType.AccEPaymentDeal, deal1.AED_InternalReference);

			message = GetQueuedUniversalEventMessage(eventDataObject, UniversalXmlInfo.Namespace_2012_11);
			serviceTaskLog = new ServiceTaskLogForTesting();
			manager = new UniversalMessageProcessingManager(serviceTaskLog);
			importResults = manager.Process(message).ImportResults;

			expectedError = @"The following Context fields are either empty or missing:
ProviderRef
FromAmount
FromCurrency
ToAmount
ToCurrency
FeeAmount
FeeCurrency
ExchangeRate
ExchangeRateInverted";

			expectedImportResult = FormattableString.Invariant(
$@"Linked Event to {deal1.HumanReadableName}.
Error - {expectedError}");

			AssertEquals(expectedError, ErrorReporter.LastMessageReported);
			AssertEquals(expectedImportResult, importResults.Single().ToString());
			Assert(serviceTaskLog.HasErrors());
			AssertEquals(EDIMessageStatusList.Codes.Error, message.EM_Status);

			AssertEquals(EPaymentStatusCodes.Deal.Requested, deal1.AED_Status);
			AssertEquals(ZDateTime.Empty, deal1.AED_LastResponseReceivedUtc);
			AssertEquals(ZString.Empty, deal1.AED_ErrorDescription);

			dealIRJLogs = deal1.Logs.Find(l => l.SL_SE_NKEvent == Events.InterchangeRejectedCode);
			AssertEquals(2, dealIRJLogs.Count());
			var ediMessagesLinkedToDealIRJLog = dealIRJLogs.Select(x => x.RelatedEDIMessage?.Message);
			AssertEquals(2, ediMessagesLinkedToDealIRJLog.Count());

			paymentApprovalIRJLogs = (deal1.Quote as IEPaymentLogParent).PaymentApprovalForLogging.Logs.Find(l => l.SL_SE_NKEvent == Events.InterchangeRejectedCode);
			AssertEquals(2, paymentApprovalIRJLogs.Count());
			var ediMessagesLinkedToPaymentApprovalIRJLog = paymentApprovalIRJLogs.Select(x => x.RelatedEDIMessage?.Message);
			AssertEquals(2, ediMessagesLinkedToPaymentApprovalIRJLog.Count());
			AssertContainsExactElementsInAnyOrder(ediMessagesLinkedToDealIRJLog.Select(m => m.PK), ediMessagesLinkedToPaymentApprovalIRJLog.Select(m => m.PK));

			ErrorReporter.Instance.Clear();
		}

		#endregion

		#region UST Message

		public void TestValidUSTMessage()
		{
			var validDeal = Factory.NewWithValidTestData<AccEPaymentDeal>();
			validDeal.AED_InternalReference = "00001000";
			validDeal.AED_Status = EPaymentStatusCodes.Deal.InProgress;
			validDeal.AED_LastResponseReceivedUtc = ZDateTime.UtcNow;
			validDeal.AED_ProviderReference = "21476757-3A23-4D43-9B1B-CBE600FEBD1E";

			var otherDeal = Factory.NewWithValidTestData<AccEPaymentDeal>();
			otherDeal.AED_InternalReference = "00001111";
			otherDeal.AED_Status = EPaymentStatusCodes.Deal.InProgress;
			otherDeal.AED_LastResponseReceivedUtc = ZDateTime.UtcNow;
			otherDeal.AED_ProviderReference = "A46B3F27-B80B-4B08-AE4A-21DAD4CBFFF7";

			var newCompany = TestObjectCreator.CreateNewCompany("NEW");
			var otherCompanyDeal = Factory.NewWithValidTestData<AccEPaymentDeal>();
			otherCompanyDeal.AED_InternalReference = validDeal.AED_InternalReference;
			otherCompanyDeal.AED_ProviderReference = "EEB4FD8E-DD16-490A-B140-1C219FB31BC4";
			otherCompanyDeal.AED_Status = EPaymentStatusCodes.Deal.InProgress;
			otherCompanyDeal.AED_LastResponseReceivedUtc = ZDateTime.UtcNow;
			otherCompanyDeal.AED_GC_Company = newCompany.PK;

			Factory.SaveForTesting();

			var eventDataObject = GetNewIncomingIAKMessageWithDealStatusUpdate();
			eventDataObject.DataContext.AddDataTarget(DataContextType.AccEPaymentDeal, validDeal.AED_InternalReference);

			var message = GetQueuedUniversalEventMessage(eventDataObject, UniversalXmlInfo.Namespace_2012_11);
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			var importResults = manager.Process(message).ImportResults;
			var expectedImportResult = FormattableString.Invariant(
$@"Linked Event to {validDeal.HumanReadableName}.");

			AssertEquals(expectedImportResult, importResults.Single().ToString());
			Assert(!serviceTaskLog.HasErrors());
			AssertEquals(EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);

			AssertEquals(EPaymentStatusCodes.Deal.Paid, validDeal.AED_Status);
			AssertEquals(EventTime, validDeal.AED_LastResponseReceivedUtc);
			AssertEquals(OFXDealStatus.Paid, validDeal.AED_ErrorDescription);

			var expectedLogReference = $"E-Payment Deal 00001000 status changed to {OFXDealStatus.Paid}.";
			var dealIAKLogs = validDeal.Logs.Find(l => l.SL_SE_NKEvent == Events.InterchangeAcknowledgedCode && l.SL_Reference == expectedLogReference);
			AssertEquals(1, dealIAKLogs.Count());
			var ediMessageLinkedToDealIAKLog = dealIAKLogs.First().RelatedEDIMessage?.Message;
			AssertNotNull(ediMessageLinkedToDealIAKLog);

			var paymentApprovalIAKLogs = validDeal.Quote.PaymentApproval.Logs.Find(l => l.SL_SE_NKEvent == Events.InterchangeAcknowledgedCode && l.SL_Reference == expectedLogReference);
			AssertEquals(1, paymentApprovalIAKLogs.Count());
			var ediMessageLinkedToPaymentApprovalIAKLog = paymentApprovalIAKLogs.First().RelatedEDIMessage?.Message;
			AssertNotNull(ediMessageLinkedToPaymentApprovalIAKLog);
			AssertEquals(ediMessageLinkedToDealIAKLog.PK, ediMessageLinkedToPaymentApprovalIAKLog.PK);
		}

		public void TestUSTMessageWithMissingContexts()
		{
			var incomingMessage = GetNewIncomingIAKMessageWithDealStatusUpdate(true, false);
			var validDeal = Factory.NewWithValidTestData<AccEPaymentDeal>();
			validDeal.AED_InternalReference = "00001000";
			validDeal.AED_Status = EPaymentStatusCodes.Deal.InProgress;
			var originalResponseReceived = ZDateTime.UtcNow;
			validDeal.AED_LastResponseReceivedUtc = originalResponseReceived;
			validDeal.AED_ProviderReference = "21476757-3A23-4D43-9B1B-CBE600FEBD1E";
			Factory.SaveForTesting();

			incomingMessage.DataContext.AddDataTarget(DataContextType.AccEPaymentDeal, validDeal.AED_InternalReference);
			var message = GetQueuedUniversalEventMessage(incomingMessage, UniversalXmlInfo.Namespace_2012_11);
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			var importResults = manager.Process(message).ImportResults;

			var expectedError = @"The following Context fields are either empty or missing:
StatusCode";

			var expectedImportResult = FormattableString.Invariant(
$@"Linked Event to {validDeal.HumanReadableName}.
Error - {expectedError}");

			AssertEquals(expectedError, ErrorReporter.LastMessageReported);
			AssertEquals(expectedImportResult, importResults.Single().ToString());
			Assert(serviceTaskLog.HasErrors());
			AssertEquals(EDIMessageStatusList.Codes.Error, message.EM_Status);

			AssertEquals("AED_LastResponseReceivedUtc should not be changed since message has missing context", originalResponseReceived, validDeal.AED_LastResponseReceivedUtc);
			AssertEquals(EPaymentStatusCodes.Deal.InProgress, validDeal.AED_Status);

			var dealIAKLogs = validDeal.Logs.Find(l => l.SL_SE_NKEvent == Events.InterchangeAcknowledgedCode);
			AssertEquals(1, dealIAKLogs.Count());
			var ediMessageLinkedToDealIAKLog = dealIAKLogs.First().RelatedEDIMessage?.Message;
			AssertNotNull(ediMessageLinkedToDealIAKLog);

			var paymentApprovalIAKLogs = validDeal.Quote.PaymentApproval.Logs.Find(l => l.SL_SE_NKEvent == Events.InterchangeAcknowledgedCode);
			AssertEquals(1, paymentApprovalIAKLogs.Count());
			var ediMessageLinkedToPaymentApprovalIAKLog = paymentApprovalIAKLogs.First().RelatedEDIMessage?.Message;
			AssertNotNull(ediMessageLinkedToPaymentApprovalIAKLog);
			AssertEquals(ediMessageLinkedToDealIAKLog.PK, ediMessageLinkedToPaymentApprovalIAKLog.PK);

			ErrorReporter.Instance.Clear();
		}

		#endregion

		void TestIAKMessageWithNewQuote(bool isOldQuoteInAcceptedStatus, bool isOldQuoteInExpiredQuotesList)
		{
			var deal1 = Factory.NewWithValidTestData<AccEPaymentDeal>();
			deal1.AED_InternalReference = "00001000";
			deal1.AED_Status = EPaymentStatusCodes.Deal.Requested;

			var paymentApproval = deal1.Quote.PaymentApproval;
			paymentApproval.AV_PaymentApprovalReference = "00001000";
			paymentApproval.AV_Ledger = LedgerTypes.AccountsPayable;
			paymentApproval.AV_RX_NKPaymentCurrency = Core.Constants.CurrencyCodes.EuropeanUnion;
			paymentApproval.AV_PayExRate = 0.6166;
			paymentApproval.AV_Amount = 678.26;

			var oldQuote = deal1.Quote;
			oldQuote.QU_InternalReference = "00001000";
			oldQuote.QU_Status = isOldQuoteInAcceptedStatus ? EPaymentStatusCodes.Quote.Accepted : EPaymentStatusCodes.Quote.Received;
			oldQuote.QU_LastResponseReceivedUtc = ZDateTime.UtcNow.AddHours(-3);
			oldQuote.QU_ExchangeRate = 0.6166;
			oldQuote.QU_ExchangeRateInverted = 1.6218;
			oldQuote.QU_FromAmount = 1100.00;
			oldQuote.QU_RX_NKFromCurrency = Core.Constants.CurrencyCodes.Australia;
			oldQuote.QU_ToAmount = 678.26;
			oldQuote.QU_RX_NKToCurrency = Core.Constants.CurrencyCodes.EuropeanUnion;
			oldQuote.QU_FeeAmount = 15.00;
			oldQuote.QU_RX_NKFeeCurrency = Core.Constants.CurrencyCodes.Australia;
			oldQuote.QU_ProviderReference = "apple";

			Factory.SaveForTesting();

			var exRateToleranceConfig = new ExchangeRateToleranceConfiguration();
			exRateToleranceConfig.ExchangeRateToleranceCollection.RemoveAndDeleteAll();
			var toleranceForEUR = new ExchangeRateTolerance
			{
				Currency = Core.Constants.CurrencyCodes.EuropeanUnion,
				ExchangeRateTolerancePercentage = 5
			};
			exRateToleranceConfig.ExchangeRateToleranceCollection.Add(toleranceForEUR);

			//base amount = 1100
			//tolerance amount = 1100 * 5% = 55
			//maximum amount = 1100 + 55 = 1155

			//new quote within tolerance
			//new quote from amount = 1005.57
			//new exchange rate = 0.6745

			using (AccountingConfigurationRegistry.Instance.ExchangeRateTolerance.SetTemporaryValue(paymentApproval.AV_GC.ToGuid(), Guid.Empty, Guid.Empty, exRateToleranceConfig))
			{
				var expiredQuoteIds = isOldQuoteInExpiredQuotesList ? "apple||banana" : "pear||banana";
				var eventDataObject = GetNewIncomingIAKMessage(true, expiredQuoteIds, "1005.57", "0.6745", "1.4826");
				eventDataObject.DataContext.AddDataTarget(DataContextType.AccEPaymentDeal, deal1.AED_InternalReference);

				var message = GetQueuedUniversalEventMessage(eventDataObject, UniversalXmlInfo.Namespace_2012_11);
				var serviceTaskLog = new ServiceTaskLogForTesting();
				var manager = new UniversalMessageProcessingManager(serviceTaskLog);
				var importResults = manager.Process(message).ImportResults;

				var expectedImportResultBuilder = new ZStringBuilder($"Linked Event to {deal1.HumanReadableName}.");
				if (!isOldQuoteInAcceptedStatus)
				{
					expectedImportResultBuilder.Append("Warning - Expected E-Quote Status is 'Accepted', but current Payment Approval 00001000 has E-Quote 00001000 with status 'RCV'.");
				}
				if (!isOldQuoteInExpiredQuotesList)
				{
					expectedImportResultBuilder.Append("Warning - Payment booked against new E-Quote compared to the accepted E-Quote, but the provider has not confirmed if the original E-Quote is expired.");
				}

				AssertEquals(expectedImportResultBuilder.ToStringWithNewLineBetweenAppends(), importResults.Single().ToString());
				Assert(!serviceTaskLog.HasErrors());
				if (isOldQuoteInAcceptedStatus && isOldQuoteInExpiredQuotesList)
				{
					AssertEquals(EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);
				}
				else
				{
					AssertEquals(EDIMessageStatusList.Codes.Warning, message.EM_Status);
				}

				AssertEquals(EPaymentStatusCodes.Quote.Discarded, oldQuote.QU_Status);
				AssertEquals(isOldQuoteInExpiredQuotesList ? "Quote Expired" : string.Empty, oldQuote.QU_ErrorDescription);

				var newQuote = Factory.LoadTop1<AccEPaymentQuote>(new ZQuery(AccEPaymentQuoteSchema.PK, SQLComparisonOperator.NotEqual, oldQuote.PK));
				AssertNotNull(newQuote);
				AssertEquals("orange", newQuote.QU_ProviderReference);
				AssertEquals(1005.57m, newQuote.QU_FromAmount);
				AssertEquals(Core.Constants.CurrencyCodes.Australia, newQuote.QU_RX_NKFromCurrency);
				AssertEquals(678.26m, newQuote.QU_ToAmount);
				AssertEquals(Core.Constants.CurrencyCodes.EuropeanUnion, newQuote.QU_RX_NKToCurrency);
				AssertEquals(15m, newQuote.QU_FeeAmount);
				AssertEquals(Core.Constants.CurrencyCodes.Australia, newQuote.QU_RX_NKFeeCurrency);
				AssertEquals(0.6745m, newQuote.QU_ExchangeRate);
				AssertEquals(1.4826m, newQuote.QU_ExchangeRateInverted);
				AssertEquals(EventTime, newQuote.QU_LastResponseReceivedUtc);

				AssertEquals("Deal is linked to new quote", deal1.Quote.PK, newQuote.PK);
				AssertEquals("New quote is accepted", EPaymentStatusCodes.Quote.Accepted, newQuote.QU_Status);

				AssertEquals(0.6745m, paymentApproval.AV_PayExRate.Round(4));
				AssertEquals(678.26m, paymentApproval.AV_Amount);
				AssertEquals(Core.Constants.CurrencyCodes.EuropeanUnion, paymentApproval.AV_RX_NKPaymentCurrency);
				var paymentApprovalBase = Factory.LoadTop1<PaymentApprovalBase>(new ZQuery(AccPaymentApprovalSchema.PK, paymentApproval.PK));
				AssertEquals(1005.57m, paymentApprovalBase.AV_Calc_LocalAmount);
				AssertEquals(Core.Constants.CurrencyCodes.Australia, paymentApprovalBase.AV_Calc_LocalCurrency);

				AssertEquals(EPaymentStatusCodes.Deal.Accepted, deal1.AED_Status);
				AssertEquals("4acb60dc-83a1-467d-916f-dad748ed6294", deal1.AED_ProviderReference);
				AssertEquals(EventTime, deal1.AED_LastResponseReceivedUtc);
				AssertEquals(ZString.Empty, deal1.AED_ErrorDescription);

				var expectedLogReference = "E-Payment Deal 00001000 accepted by OFX.";
				var dealIAKLogs = deal1.Logs.Find(l => l.SL_SE_NKEvent == Events.InterchangeAcknowledgedCode && l.SL_Reference == expectedLogReference);
				AssertEquals(1, dealIAKLogs.Count());
				var ediMessageLinkedToDealIAKLog = dealIAKLogs.First().RelatedEDIMessage?.Message;
				AssertNotNull(ediMessageLinkedToDealIAKLog);

				var paymentApprovalIAKLogs = paymentApproval.Logs.Find(l => l.SL_SE_NKEvent == Events.InterchangeAcknowledgedCode && l.SL_Reference == expectedLogReference);
				AssertEquals(1, paymentApprovalIAKLogs.Count());
				var ediMessageLinkedToPaymentApprovalIAKLog = paymentApprovalIAKLogs.First().RelatedEDIMessage?.Message;
				AssertNotNull(ediMessageLinkedToPaymentApprovalIAKLog);
				AssertEquals(ediMessageLinkedToDealIAKLog.PK, ediMessageLinkedToPaymentApprovalIAKLog.PK);
			}
		}

		void TestIRJMessageWithNewQuote(bool isOldQuoteInAcceptedStatus)
		{
			var deal1 = Factory.NewWithValidTestData<AccEPaymentDeal>();
			deal1.AED_InternalReference = "00001000";
			deal1.AED_Status = EPaymentStatusCodes.Deal.Requested;

			var paymentApproval = deal1.Quote.PaymentApproval;
			paymentApproval.AV_PaymentApprovalReference = "00001000";
			paymentApproval.AV_Ledger = LedgerTypes.AccountsPayable;
			paymentApproval.AV_RX_NKPaymentCurrency = Core.Constants.CurrencyCodes.EuropeanUnion;
			paymentApproval.AV_PayExRate = 0.6166;
			paymentApproval.AV_Amount = 678.26;

			var oldQuote = deal1.Quote;
			oldQuote.QU_InternalReference = "00001000";
			oldQuote.QU_Status = isOldQuoteInAcceptedStatus ? EPaymentStatusCodes.Quote.Accepted : EPaymentStatusCodes.Quote.Received;
			oldQuote.QU_LastResponseReceivedUtc = ZDateTime.UtcNow.AddHours(-3);
			oldQuote.QU_ExchangeRate = 0.6166;
			oldQuote.QU_ExchangeRateInverted = 1.6218;
			oldQuote.QU_FromAmount = 1100.00;
			oldQuote.QU_RX_NKFromCurrency = Core.Constants.CurrencyCodes.Australia;
			oldQuote.QU_ToAmount = 678.26;
			oldQuote.QU_RX_NKToCurrency = Core.Constants.CurrencyCodes.EuropeanUnion;
			oldQuote.QU_FeeAmount = 15.00;
			oldQuote.QU_RX_NKFeeCurrency = Core.Constants.CurrencyCodes.Australia;
			oldQuote.QU_ProviderReference = "apple";

			Factory.SaveForTesting();

			var exRateToleranceConfig = new ExchangeRateToleranceConfiguration();
			exRateToleranceConfig.ExchangeRateToleranceCollection.RemoveAndDeleteAll();
			var toleranceForEUR = new ExchangeRateTolerance
			{
				Currency = Core.Constants.CurrencyCodes.EuropeanUnion,
				ExchangeRateTolerancePercentage = 5
			};
			exRateToleranceConfig.ExchangeRateToleranceCollection.Add(toleranceForEUR);

			//base amount = 1100
			//tolerance amount = 1100 * 5% = 55
			//maximum amount = 1100 + 55 = 1155

			//new quote outside tolerance
			//new quote from amount = 1200.00
			//new exchange rate = 0.5652

			using (AccountingConfigurationRegistry.Instance.ExchangeRateTolerance.SetTemporaryValue(paymentApproval.AV_GC.ToGuid(), Guid.Empty, Guid.Empty, exRateToleranceConfig))
			{
				var eventDataObject = GetNewIncomingIRJMessageWithNewQuoteDetails();
				eventDataObject.DataContext.AddDataTarget(DataContextType.AccEPaymentDeal, deal1.AED_InternalReference);

				var message = GetQueuedUniversalEventMessage(eventDataObject, UniversalXmlInfo.Namespace_2012_11);
				var serviceTaskLog = new ServiceTaskLogForTesting();
				var manager = new UniversalMessageProcessingManager(serviceTaskLog);
				var importResults = manager.Process(message).ImportResults;

				var expectedImportResultBuilder = new ZStringBuilder($"Linked Event to {deal1.HumanReadableName}.");
				if (!isOldQuoteInAcceptedStatus)
				{
					expectedImportResultBuilder.Append("Warning - Expected E-Quote Status is 'Accepted', but current Payment Approval 00001000 has E-Quote 00001000 with status 'RCV'.");
				}

				AssertEquals(expectedImportResultBuilder.ToStringWithNewLineBetweenAppends(), importResults.Single().ToString());
				Assert(!serviceTaskLog.HasErrors());
				if (isOldQuoteInAcceptedStatus)
				{
					AssertEquals(EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);
				}

				AssertEquals(EPaymentStatusCodes.Quote.Discarded, oldQuote.QU_Status);
				AssertEquals("Quote Expired", oldQuote.QU_ErrorDescription);
				var newQuote = Factory.LoadTop1<AccEPaymentQuote>(new ZQuery(AccEPaymentQuoteSchema.PK, SQLComparisonOperator.NotEqual, oldQuote.PK));
				AssertNotNull(newQuote);

				AssertEquals("orange", newQuote.QU_ProviderReference);
				AssertEquals(1200.00m, newQuote.QU_FromAmount);
				AssertEquals(Core.Constants.CurrencyCodes.Australia, newQuote.QU_RX_NKFromCurrency);
				AssertEquals(678.26m, newQuote.QU_ToAmount);
				AssertEquals(Core.Constants.CurrencyCodes.EuropeanUnion, newQuote.QU_RX_NKToCurrency);
				AssertEquals(15m, newQuote.QU_FeeAmount);
				AssertEquals(Core.Constants.CurrencyCodes.Australia, newQuote.QU_RX_NKFeeCurrency);
				AssertEquals(0.5652m, newQuote.QU_ExchangeRate);
				AssertEquals(1.7693m, newQuote.QU_ExchangeRateInverted);
				AssertEquals(EventTime, newQuote.QU_LastResponseReceivedUtc);

				AssertNotEquals("Deal is not linked to new quote", deal1.Quote.PK, newQuote.PK);
				AssertEquals("Deal is still linked to old quote", deal1.Quote.PK, oldQuote.PK);
				AssertEquals("New quote is in received status", EPaymentStatusCodes.Quote.Received, newQuote.QU_Status);

				AssertEquals(0.6166m, paymentApproval.AV_PayExRate.Round(4));
				AssertEquals(678.26m, paymentApproval.AV_Amount);
				AssertEquals(Core.Constants.CurrencyCodes.EuropeanUnion, paymentApproval.AV_RX_NKPaymentCurrency);
				var paymentApprovalBase = Factory.LoadTop1<PaymentApprovalBase>(new ZQuery(AccPaymentApprovalSchema.PK, paymentApproval.PK));
				AssertEquals(1100.00m, paymentApprovalBase.AV_Calc_LocalAmount);
				AssertEquals(Core.Constants.CurrencyCodes.Australia, paymentApprovalBase.AV_Calc_LocalCurrency);

				AssertEquals(EPaymentStatusCodes.Deal.Declined, deal1.AED_Status);
				AssertEquals(ZString.Empty, deal1.AED_ProviderReference);
				AssertEquals(EventTime, deal1.AED_LastResponseReceivedUtc);
				AssertEquals("Quote Expired. Please review the new e-quote.", deal1.AED_ErrorDescription);

				var expectedLogReference = "Quote 00001000 expired. New E-Quote received for approval.";
				var dealIRJLogs = deal1.Logs.Find(l => l.SL_SE_NKEvent == Events.InterchangeRejectedCode && l.SL_Reference == expectedLogReference);
				AssertEquals(1, dealIRJLogs.Count());
				var ediMessageLinkedToDealIRJLog = dealIRJLogs.First().RelatedEDIMessage?.Message;
				AssertNotNull(ediMessageLinkedToDealIRJLog);

				var paymentApprovalIRJLogs = paymentApproval.Logs.Find(l => l.SL_SE_NKEvent == Events.InterchangeRejectedCode && l.SL_Reference == expectedLogReference);
				AssertEquals(1, paymentApprovalIRJLogs.Count());
				var ediMessageLinkedToPaymentApprovalIRJLog = paymentApprovalIRJLogs.First().RelatedEDIMessage?.Message;
				AssertNotNull(ediMessageLinkedToPaymentApprovalIRJLog);
				AssertEquals(ediMessageLinkedToDealIRJLog.PK, ediMessageLinkedToPaymentApprovalIRJLog.PK);
			}
		}

		public void TestInvalidEventType()
		{
			var incomingMessage = GetNewIncomingIAKMessageWithDealStatusUpdate();
			var validDeal = Factory.NewWithValidTestData<AccEPaymentDeal>();
			validDeal.AED_InternalReference = "00001000";
			validDeal.AED_Status = EPaymentStatusCodes.Deal.InProgress;
			var originalLastResponseReceived = ZDateTime.UtcNow;
			validDeal.AED_LastResponseReceivedUtc = originalLastResponseReceived;
			validDeal.AED_ProviderReference = "21476757-3A23-4D43-9B1B-CBE600FEBD1E";
			Factory.SaveForTesting();

			var eventDataObject = GetNewIncomingIAKMessageWithDealStatusUpdate();
			eventDataObject.DataContext.AddDataTarget(DataContextType.AccEPaymentDeal, validDeal.AED_InternalReference);
			eventDataObject.EventType = Events.InterchangeReceivedCode;

			var message = GetQueuedUniversalEventMessage(eventDataObject, UniversalXmlInfo.Namespace_2012_11);
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);

			var importResults = manager.Process(message).ImportResults;

			var expectedError = $"Unexpected Event Type encountered: {Events.InterchangeReceivedCode}.";
			var expectedImportResult = FormattableString.Invariant(
$@"Linked Event to {validDeal.HumanReadableName}.
Error - {expectedError}");

			AssertEquals(expectedError, ErrorReporter.LastMessageReported);
			AssertEquals(expectedImportResult, importResults.Single().ToString());
			Assert(serviceTaskLog.HasErrors());
			AssertEquals(EDIMessageStatusList.Codes.Error, message.EM_Status);

			AssertEquals("AED_LastResponseReceivedUtc should not be changed when event type is invalid", originalLastResponseReceived, validDeal.AED_LastResponseReceivedUtc);

			var dealIRCLogs = validDeal.Logs.Find(l => l.SL_SE_NKEvent == Events.InterchangeReceivedCode);
			AssertEquals(1, dealIRCLogs.Count());
			var ediMessageLinkedToDealIRCLog = dealIRCLogs.First().RelatedEDIMessage?.Message;
			AssertNotNull(ediMessageLinkedToDealIRCLog);

			var paymentApprovalIRCLogs = validDeal.Quote.PaymentApproval.Logs.Find(l => l.SL_SE_NKEvent == Events.InterchangeReceivedCode);
			AssertEquals(1, paymentApprovalIRCLogs.Count());
			var ediMessageLinkedToPaymentApprovalIRCLog = paymentApprovalIRCLogs.First().RelatedEDIMessage?.Message;
			AssertNotNull(ediMessageLinkedToPaymentApprovalIRCLog);
			AssertEquals(ediMessageLinkedToDealIRCLog.PK, ediMessageLinkedToPaymentApprovalIRCLog.PK);

			ErrorReporter.Instance.Clear();
		}

		public void TestInvalidXMLFormat()
		{
			var eventDataObject = GetNewIncomingIAKMessageWithDealStatusUpdate();
			eventDataObject.DataContext = new UniversalDataBuss.DataObjects.Universal._2011_11.DataContext();
			eventDataObject.DataContext.AddDataTarget(DataContextType.AccEPaymentDeal, "CD44BEF6-412C-4E6F-8F64-FB290BBCE66A");

			var message = GetQueuedUniversalEventMessage(eventDataObject, UniversalXmlInfo.Namespace_2011_11);
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);
			AssertEquals(
@"ERROR - Unsupported XML format encountered: Enterprise.UniversalDataBuss.DataObjects.Universal._2011_11.DataContext.
Warning - No Module found a Business Entity to link this Universal Event to.", serviceTaskLog.ToString());
			AssertEquals(EDIMessageStatusList.Codes.Discarded, message.EM_Status);
		}

		public void TestInvalidProviderCode()
		{
			var eventDataObject = GetNewIncomingIAKMessageWithDealStatusUpdate();
			eventDataObject.DataContext.AddDataTarget(DataContextType.AccEPaymentDeal, "58E79B36-5B5E-4793-A035-CB1ED90563FB");
			eventDataObject.EventParameters = new EventParameters();

			var message = GetQueuedUniversalEventMessage(eventDataObject, UniversalXmlInfo.Namespace_2012_11);
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);
			AssertEquals(
@"ERROR - Could not find a valid Provider Code in the Event's <MessageType> parameter. Value found: ''
Warning - No Module found a Business Entity to link this Universal Event to.", serviceTaskLog.ToString());
			AssertEquals(EDIMessageStatusList.Codes.Discarded, message.EM_Status);

			eventDataObject.EventParameters = new EventParameters { MessageType = ZString.Empty };
			message = GetQueuedUniversalEventMessage(eventDataObject, UniversalXmlInfo.Namespace_2012_11);
			serviceTaskLog = new ServiceTaskLogForTesting();
			manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);
			AssertEquals(
@"ERROR - Could not find a valid Provider Code in the Event's <MessageType> parameter. Value found: ''
Warning - No Module found a Business Entity to link this Universal Event to.", serviceTaskLog.ToString());
			AssertEquals(EDIMessageStatusList.Codes.Discarded, message.EM_Status);

			eventDataObject.EventParameters = new EventParameters { MessageType = "AAA" };
			message = GetQueuedUniversalEventMessage(eventDataObject, UniversalXmlInfo.Namespace_2012_11);
			serviceTaskLog = new ServiceTaskLogForTesting();
			manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);
			AssertEquals(
@"ERROR - Could not find a valid Provider Code in the Event's <MessageType> parameter. Value found: 'AAA'
Warning - No Module found a Business Entity to link this Universal Event to.", serviceTaskLog.ToString());
			AssertEquals(EDIMessageStatusList.Codes.Discarded, message.EM_Status);
		}

		public void TestInvalidMessageSubType()
		{
			var eventDataObject = GetNewIncomingIAKMessageWithDealStatusUpdate();
			eventDataObject.DataContext.AddDataTarget(DataContextType.AccEPaymentDeal, "58E79B36-5B5E-4793-A035-CB1ED90563FB");
			eventDataObject.EventParameters = new EventParameters { MessageType = ProviderCodes.OFX, MessageSubType = Events.InsuranceFinalisedCode };

			var message = GetQueuedUniversalEventMessage(eventDataObject, UniversalXmlInfo.Namespace_2012_11);
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);
			AssertEquals(
@"ERROR - Could not find a valid value in the Event's <MessageSubType> parameter. Value found: 'INF'
Warning - No Module found a Business Entity to link this Universal Event to.", serviceTaskLog.ToString());
			AssertEquals(EDIMessageStatusList.Codes.Discarded, message.EM_Status);
		}

		public void TestMissingDealReference()
		{
			var eventDataObject = GetNewIncomingIAKMessageWithDealStatusUpdate();
			eventDataObject.DataContext.AddDataTarget(DataContextType.AccEPaymentDeal, ZString.Empty);

			var message = GetQueuedUniversalEventMessage(eventDataObject, UniversalXmlInfo.Namespace_2012_11);
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);
			AssertEquals(
@"ERROR - Could not find a valid Deal Reference in the <DataTarget> Key parameter. Value found: ''
Warning - No Module found a Business Entity to link this Universal Event to.", serviceTaskLog.ToString());
			AssertEquals(EDIMessageStatusList.Codes.Discarded, message.EM_Status);
		}

		public void TestNoContextCollection()
		{
			var eventDataObject = GetNewIncomingIAKMessageWithDealStatusUpdate();
			eventDataObject.DataContext.AddDataTarget(DataContextType.AccEPaymentDeal, "F8AC0585-E128-43F3-A07F-269C7AB52006");
			eventDataObject.ContextCollection = null;

			var message = GetQueuedUniversalEventMessage(eventDataObject, UniversalXmlInfo.Namespace_2012_11);
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);
			AssertEquals(
@"ERROR - No Context Collection found.
Warning - No Module found a Business Entity to link this Universal Event to.", serviceTaskLog.ToString());
			AssertEquals(EDIMessageStatusList.Codes.Discarded, message.EM_Status);
		}

		public void TestDataContextTypeAndDataContextKey()
		{
			var deal = GetNewBusinessObjectForTesting();
			deal.AED_InternalReference = "61F49783-EA7D-4FA6-A5A2-3B44C9C7C7D6";

			AssertEquals(DataContextType.AccEPaymentDeal, deal.GetUniversalDataContextManager().DataContextType);
			AssertEquals("61F49783-EA7D-4FA6-A5A2-3B44C9C7C7D6", deal.GetUniversalDataContextManager().DataContextKey);
		}

		protected override void TestBusinessObjectImplementsIJobNumberCore()
		{
			Assert("No Job Number support", true);
		}

		UniversalEventDataObject GetNewIncomingIAKMessageWithDealStatusUpdate(bool populateCompanyCodeContext = true, bool populateStatusCodeContext = true)
		{
			var eventDataObject = new UniversalEventDataObject();
			eventDataObject.DataContext = new UniversalDataBuss.DataObjects.Universal._2012_11.DataContext();
			eventDataObject.EventType = Events.InterchangeAcknowledgedCode;
			eventDataObject.EventTime = EventTime.ToOffset();
			eventDataObject.EventParameters = new EventParameters { MessageType = ProviderCodes.OFX, MessageSubType = Events.StatusUpdatedCode };
			eventDataObject.ContextCollection = new List<Context>();
			if (populateCompanyCodeContext)
			{
				eventDataObject.ContextCollection.Add(new Context() { Type = new ContextType { Type = XUEFieldNames.CompanyCode }, Value = GlbCompany.CurrentCompany.GC_Code });
			}
			if (populateStatusCodeContext)
			{
				eventDataObject.ContextCollection.Add(new Context() { Type = new ContextType { Type = XUEFieldNames.StatusCode }, Value = OFXDealStatus.Paid });
			}
			return eventDataObject;
		}

		UniversalEventDataObject GetNewIncomingIAKMessage(bool populateRequiedFields = true, string expiredQuoteIds = null, string fromAmount = null, string exchangeRate = null, string exchangeRateInverted = null)
		{
			var eventDataObject = new UniversalEventDataObject();
			eventDataObject.DataContext = new UniversalDataBuss.DataObjects.Universal._2012_11.DataContext();
			eventDataObject.EventType = Events.InterchangeAcknowledgedCode;
			eventDataObject.EventTime = EventTime.ToOffset();
			eventDataObject.EventParameters = new EventParameters { MessageType = ProviderCodes.OFX, MessageSubType = MessageSubTypes.CreateADeal };
			eventDataObject.ContextCollection = new List<Context>();
			if (populateRequiedFields)
			{
				eventDataObject.ContextCollection.Add(new Context() { Type = new ContextType { Type = XUEFieldNames.DealId }, Value = "4acb60dc-83a1-467d-916f-dad748ed6294" });
				eventDataObject.ContextCollection.Add(new Context() { Type = new ContextType { Type = XUEFieldNames.Status }, Value = OFXDealStatus.Booked });
				eventDataObject.ContextCollection.Add(new Context() { Type = new ContextType { Type = XUEFieldNames.QuoteIdUsedForPayment }, Value = "orange" });
				eventDataObject.ContextCollection.Add(new Context() { Type = new ContextType { Type = XUEFieldNames.FromAmount }, Value = fromAmount ?? "1100" });
				eventDataObject.ContextCollection.Add(new Context() { Type = new ContextType { Type = XUEFieldNames.FromCurrency }, Value = Core.Constants.CurrencyCodes.Australia });
				eventDataObject.ContextCollection.Add(new Context() { Type = new ContextType { Type = XUEFieldNames.ToAmount }, Value = "678.26" });
				eventDataObject.ContextCollection.Add(new Context() { Type = new ContextType { Type = XUEFieldNames.ToCurrency }, Value = Core.Constants.CurrencyCodes.EuropeanUnion });
				eventDataObject.ContextCollection.Add(new Context() { Type = new ContextType { Type = XUEFieldNames.ExchaneRate }, Value = exchangeRate ?? "0.6166" });
				eventDataObject.ContextCollection.Add(new Context() { Type = new ContextType { Type = XUEFieldNames.FeeAmount }, Value = "15" });
				eventDataObject.ContextCollection.Add(new Context() { Type = new ContextType { Type = XUEFieldNames.FeeCurrency }, Value = Core.Constants.CurrencyCodes.Australia });
				eventDataObject.ContextCollection.Add(new Context() { Type = new ContextType { Type = XUEFieldNames.ExchaneRateInverted }, Value = exchangeRateInverted ?? "1.6218" });
			}
			eventDataObject.ContextCollection.Add(new Context() { Type = new ContextType { Type = XUEFieldNames.IsAwaitingFunds }, Value = "true" });
			eventDataObject.ContextCollection.Add(new Context() { Type = new ContextType { Type = XUEFieldNames.IsSettled }, Value = "false" });
			eventDataObject.ContextCollection.Add(new Context() { Type = new ContextType { Type = XUEFieldNames.PaymentDate }, Value = "13 Jun 2021 02:00:00 PM" });
			if (expiredQuoteIds != null)
			{
				eventDataObject.ContextCollection.Add(new Context() { Type = new ContextType { Type = XUEFieldNames.ExpiredQuoteIDs }, Value = expiredQuoteIds });
			}
			eventDataObject.ContextCollection.Add(new Context() { Type = new ContextType { Type = XUEFieldNames.CompanyCode }, Value = GlbCompany.CurrentCompany.GC_Code });
			eventDataObject.ContextCollection.Add(new Context() { Type = new ContextType { Type = XUEFieldNames.BranchCode }, Value = GlbBranch.CurrentBranch.GB_Code });
			eventDataObject.ContextCollection.Add(new Context() { Type = new ContextType { Type = XUEFieldNames.ResponseTextFromProvider }, Value = "" });
			return eventDataObject;
		}

		UniversalEventDataObject GetNewIncomingIRJMessage(string errorOriginatesdAt, bool populateErrorType = true, bool populateRequiredFields = true, string reason = "Some weird error happened")
		{
			var eventDataObject = new UniversalEventDataObject();
			eventDataObject.DataContext = new UniversalDataBuss.DataObjects.Universal._2012_11.DataContext();
			eventDataObject.EventType = Events.InterchangeRejectedCode;
			eventDataObject.EventTime = EventTime.ToOffset();
			eventDataObject.EventParameters = new EventParameters { MessageType = ProviderCodes.OFX, MessageSubType = MessageSubTypes.CreateADeal, Reason = reason };
			eventDataObject.ContextCollection = new List<Context>();
			if (populateErrorType)
			{
				eventDataObject.ContextCollection.Add(new Context() { Type = new ContextType { Type = XUEFieldNames.ErrorType }, Value = IRJErrorTypes.Generic });
			}
			if (populateRequiredFields)
			{
				eventDataObject.ContextCollection.Add(new Context() { Type = new ContextType { Type = XUEFieldNames.ErrorOriginatesAt }, Value = errorOriginatesdAt });
				eventDataObject.ContextCollection.Add(new Context() { Type = new ContextType { Type = XUEFieldNames.ErrorMessage }, Value = reason });
			}
			eventDataObject.ContextCollection.Add(new Context() { Type = new ContextType { Type = XUEFieldNames.CompanyCode }, Value = GlbCompany.CurrentCompany.GC_Code });
			eventDataObject.ContextCollection.Add(new Context() { Type = new ContextType { Type = XUEFieldNames.BranchCode }, Value = GlbBranch.CurrentBranch.GB_Code });
			eventDataObject.ContextCollection.Add(new Context() { Type = new ContextType { Type = XUEFieldNames.StreamContent }, Value = "U29tZSB3ZWlyZCBlcnJvciBoYXBwZW5lZA==" });
			return eventDataObject;
		}

		UniversalEventDataObject GetNewIncomingIRJMessageWithNewQuoteDetails(bool populateErrorType = true, bool populateRequiredFields = true)
		{
			var eventDataObject = new UniversalEventDataObject();
			eventDataObject.DataContext = new UniversalDataBuss.DataObjects.Universal._2012_11.DataContext();
			eventDataObject.EventType = Events.InterchangeRejectedCode;
			eventDataObject.EventTime = EventTime.ToOffset();
			eventDataObject.EventParameters = new EventParameters { MessageType = ProviderCodes.OFX, MessageSubType = MessageSubTypes.CreateADeal };
			eventDataObject.ContextCollection = new List<Context>();
			if (populateErrorType)
			{
				eventDataObject.ContextCollection.Add(new Context() { Type = new ContextType { Type = XUEFieldNames.ErrorType }, Value = IRJErrorTypes.QuoteOutsideTolerance });
			}
			if (populateRequiredFields)
			{
				eventDataObject.ContextCollection.Add(new Context() { Type = new ContextType { Type = XUEFieldNames.ErrorOriginatesAt }, Value = GEPErrorSource.XT });
				eventDataObject.ContextCollection.Add(new Context() { Type = new ContextType { Type = XUEFieldNames.QuoteProviderRef }, Value = "orange" });
				eventDataObject.ContextCollection.Add(new Context() { Type = new ContextType { Type = XUEFieldNames.FromAmount }, Value = "1200.00" });
				eventDataObject.ContextCollection.Add(new Context() { Type = new ContextType { Type = XUEFieldNames.FromCurrency }, Value = Core.Constants.CurrencyCodes.Australia });
				eventDataObject.ContextCollection.Add(new Context() { Type = new ContextType { Type = XUEFieldNames.ToAmount }, Value = "678.26" });
				eventDataObject.ContextCollection.Add(new Context() { Type = new ContextType { Type = XUEFieldNames.ToCurrency }, Value = Core.Constants.CurrencyCodes.EuropeanUnion });
				eventDataObject.ContextCollection.Add(new Context() { Type = new ContextType { Type = XUEFieldNames.FeeAmount }, Value = "15" });
				eventDataObject.ContextCollection.Add(new Context() { Type = new ContextType { Type = XUEFieldNames.FeeCurrency }, Value = Core.Constants.CurrencyCodes.Australia });
				eventDataObject.ContextCollection.Add(new Context() { Type = new ContextType { Type = XUEFieldNames.ExchaneRate }, Value = "0.5652" });
				eventDataObject.ContextCollection.Add(new Context() { Type = new ContextType { Type = XUEFieldNames.ExchaneRateInverted }, Value = "1.7693" });
			}
			eventDataObject.ContextCollection.Add(new Context() { Type = new ContextType { Type = XUEFieldNames.CompanyCode }, Value = GlbCompany.CurrentCompany.GC_Code });
			eventDataObject.ContextCollection.Add(new Context() { Type = new ContextType { Type = XUEFieldNames.BranchCode }, Value = GlbBranch.CurrentBranch.GB_Code });
			eventDataObject.ContextCollection.Add(new Context() { Type = new ContextType { Type = XUEFieldNames.StreamContent }, Value = "U29tZSB3ZWlyZCBlcnJvciBoYXBwZW5lZA==" });
			return eventDataObject;
		}

		protected override AccEPaymentDeal GetNewBusinessObjectForTesting()
		{
			var deal = Factory.NewWithValidTestData<AccEPaymentDeal>();
			Factory.SaveForTesting();
			return deal;
		}

		protected TestObjectCreator TestObjectCreator => testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory.BOFactory));
		TestObjectCreator testObjectCreator;
		readonly ZDateTime EventTime = ZDateTime.Now.AddHours(-1);
	}
}
