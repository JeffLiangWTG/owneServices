using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.ElectronicPayment.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.UniversalDataBuss.Management.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Accounting.ElectronicPayment.Universal.AccEPaymentQuoteMessageConstants;
using static Enterprise.Core.Constants;
using ProviderCodes = Enterprise.MasterFiles.Business.EPaymentProviderCodes.Codes;
using StatusCodes = Enterprise.MasterFiles.Business.EPaymentStatusCodes.Quote;
using UniversalEventDataObject = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Accounting.ElectronicPayment.Testing.Universal
{
	[TestedType(typeof(AccEPaymentQuoteDataContextManager))]
	public class AccEPaymentQuoteDataContextManagerTest : DataContextManagerTestCase<AccEPaymentQuoteDataContextManager, AccEPaymentQuote>
	{
		public void TestGAQ_ValidIAKResponse_WithFee_GAQ() => AssertValidIAKResponse(true, MessageModes.GetQuote);
		public void TestGAQ_ValidIAKResponse_WithFee_GRT() => AssertValidIAKResponse(true, MessageModes.GetRate);
		public void TestGAQ_ValidIAKResponse_NoFee_GAQ() => AssertValidIAKResponse(false, MessageModes.GetQuote);
		public void TestGAQ_ValidIAKResponse_NoFee_GRT() => AssertValidIAKResponse(false, MessageModes.GetRate);

		void AssertValidIAKResponse(bool addTransactionFee, string messageMode)
		{
			var paymentApproval = TestObjectCreator.CreatePaymentApproval(ReceiptTypes.Cheque, TestObjectCreator.AUDBankAccount, TestObjectCreator.AUDChequeBook);
			paymentApproval.AV_RX_NKPaymentCurrency = CurrencyCodes.UnitedStates;
			paymentApproval.AV_Amount = 1000m;

			var validQuote = TestObjectCreator.CreateEPaymentQuote(paymentApproval, ProviderCodes.OFX);
			validQuote.QU_InternalReference = "00001000";
			validQuote.QU_Status = StatusCodes.Requested;

			var otherQuote = Factory.NewWithValidTestData<AccEPaymentQuote>();
			otherQuote.QU_InternalReference = "00001111";

			var newCompany = TestObjectCreator.CreateNewCompany("NEW");
			var otherCompanyQuote = Factory.NewWithValidTestData<AccEPaymentQuote>();
			otherCompanyQuote.QU_InternalReference = validQuote.QU_InternalReference;
			otherCompanyQuote.QU_GC = newCompany.PK;

			Factory.SaveForTesting();
			Assert(validQuote.QU_LastResponseReceivedUtc.IsEmpty);
			Assert(validQuote.QU_ProviderReference.IsEmpty);
			AssertEquals(0m, validQuote.QU_FromAmount);
			AssertEquals(1000m, validQuote.QU_ToAmount);
			AssertEquals(0m, validQuote.QU_ExchangeRate);
			AssertEquals(0m, validQuote.QU_ExchangeRateInverted);
			AssertEquals(0m, validQuote.QU_FeeAmount);
			Assert(validQuote.QU_RX_NKFeeCurrency.IsEmpty);

			var eventDataObject = GetNewIncomingMessage(messageMode);
			eventDataObject.DataContext.AddDataTarget(DataContextType.AccEPaymentQuote, validQuote.QU_InternalReference);
			eventDataObject.ContextCollection.Add(new Context() { Type = new ContextType { Type = XUEFieldNames.ProviderRef }, Value = "12345" });
			eventDataObject.ContextCollection.Add(new Context() { Type = new ContextType { Type = XUEFieldNames.LedgerType }, Value = LedgerTypes.AccountsPayable });
			eventDataObject.ContextCollection.Add(new Context() { Type = new ContextType { Type = XUEFieldNames.FromCurrency }, Value = CurrencyCodes.Australia });
			eventDataObject.ContextCollection.Add(new Context() { Type = new ContextType { Type = XUEFieldNames.FromAmount }, Value = "200" });
			eventDataObject.ContextCollection.Add(new Context() { Type = new ContextType { Type = XUEFieldNames.ToCurrency }, Value = CurrencyCodes.UnitedStates });
			eventDataObject.ContextCollection.Add(new Context() { Type = new ContextType { Type = XUEFieldNames.ToAmount }, Value = "1000" });
			eventDataObject.ContextCollection.Add(new Context() { Type = new ContextType { Type = XUEFieldNames.ExchangeRate }, Value = "5" });
			eventDataObject.ContextCollection.Add(new Context() { Type = new ContextType { Type = XUEFieldNames.ExchangeRateInverted }, Value = "0.2" });
			if (addTransactionFee)
			{
				eventDataObject.ContextCollection.Add(new Context() { Type = new ContextType { Type = XUEFieldNames.FeeAmount }, Value = "10" });
				eventDataObject.ContextCollection.Add(new Context() { Type = new ContextType { Type = XUEFieldNames.FeeCurrency }, Value = CurrencyCodes.Australia });
			}

			var message = GetQueuedUniversalEventMessage(eventDataObject, UniversalXmlInfo.Namespace_2012_11);
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			var importResults = manager.Process(message).ImportResults;
			var expectedImportResult = FormattableString.Invariant(
$@"Linked Event to {validQuote.HumanReadableName}.");

			AssertEquals(expectedImportResult, importResults.Single().ToString());
			Assert(!serviceTaskLog.HasErrors());
			AssertEquals(EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);

			AssertEquals(EventTime, validQuote.QU_LastResponseReceivedUtc);
			AssertEquals(StatusCodes.Received, validQuote.QU_Status);
			AssertEquals(messageMode == MessageModes.GetQuote ? "12345" : string.Empty, validQuote.QU_ProviderReference);
			AssertEquals(200m, validQuote.QU_FromAmount);
			AssertEquals(1000m, validQuote.QU_ToAmount);
			AssertEquals(5m, validQuote.QU_ExchangeRate);
			AssertEquals(0.2m, validQuote.QU_ExchangeRateInverted);
			if (addTransactionFee && messageMode == MessageModes.GetQuote)
			{
				AssertEquals(10m, validQuote.QU_FeeAmount);
				AssertEquals(CurrencyCodes.Australia, validQuote.QU_RX_NKFeeCurrency);
			}
			else
			{
				AssertEquals(0m, validQuote.QU_FeeAmount);
				Assert(validQuote.QU_RX_NKFeeCurrency.IsEmpty);
			}

			var expectedLogReference = messageMode == MessageModes.GetQuote ? "E-Quote Received from OFX." : "Indicative Rate Received from OFX.";
			var quoteIAKLogs = validQuote.Logs.Find(l => l.SL_SE_NKEvent == Events.InterchangeAcknowledgedCode && l.SL_Reference == expectedLogReference);
			AssertEquals(1, quoteIAKLogs.Count());
			var ediMessageLinkedToQuoteIAKLog = quoteIAKLogs.First().RelatedEDIMessage?.Message;
			AssertNotNull(ediMessageLinkedToQuoteIAKLog);

			var paymentApprovalIAKLogs = paymentApproval.Logs.Find(l => l.SL_SE_NKEvent == Events.InterchangeAcknowledgedCode && l.SL_Reference == expectedLogReference);
			AssertEquals(1, paymentApprovalIAKLogs.Count());
			var ediMessageLinkedToPaymentApprovalIAKLog = paymentApprovalIAKLogs.First().RelatedEDIMessage?.Message;
			AssertNotNull(ediMessageLinkedToPaymentApprovalIAKLog);
			AssertEquals(ediMessageLinkedToQuoteIAKLog.PK, ediMessageLinkedToPaymentApprovalIAKLog.PK);
		}

		public void TestIAKResponseWithMissingContexts_NoTags_GAQ() => AssertIAKResponseWithMissingContexts_NoTags(MessageModes.GetQuote);
		public void TestIAKResponseWithMissingContexts_NoTags_GRT() => AssertIAKResponseWithMissingContexts_NoTags(MessageModes.GetRate);

		void AssertIAKResponseWithMissingContexts_NoTags(string messageMode)
		{
			var eventDataObject = GetNewIncomingMessage(messageMode);
			AssertIAKResponseWithMissingContexts(eventDataObject, messageMode);
		}

		public void TestIAKResponseWithMissingContexts_HasTags_GAQ() => AssertIAKResponseWithMissingContexts_HasTags(MessageModes.GetQuote);
		public void TestIAKResponseWithMissingContexts_HasTags_GRT() => AssertIAKResponseWithMissingContexts_HasTags(MessageModes.GetRate);

		void AssertIAKResponseWithMissingContexts_HasTags(string messageMode)
		{
			var eventDataObject = GetNewIncomingMessage(messageMode);
			XUEFieldNames.GetRequiredFields(messageMode).ForEach(x => eventDataObject.ContextCollection.Add(new Context() { Type = new ContextType { Type = x }, Value = ZString.Empty }));
			AssertIAKResponseWithMissingContexts(eventDataObject, messageMode);
		}

		void AssertIAKResponseWithMissingContexts(UniversalEventDataObject incomingMessage, string messageMode)
		{
			var validQuote = Factory.NewWithValidTestData<AccEPaymentQuote>();
			validQuote.QU_InternalReference = "00001000";

			var otherQuote = Factory.NewWithValidTestData<AccEPaymentQuote>();
			otherQuote.QU_InternalReference = "00001111";

			var newCompany = TestObjectCreator.CreateNewCompany("NEW");
			var otherCompanyQuote = Factory.NewWithValidTestData<AccEPaymentQuote>();
			otherCompanyQuote.QU_InternalReference = validQuote.QU_InternalReference;
			otherCompanyQuote.QU_GC = newCompany.PK;

			Factory.SaveForTesting();
			Assert(validQuote.QU_LastResponseReceivedUtc.IsEmpty);

			incomingMessage.DataContext.AddDataTarget(DataContextType.AccEPaymentQuote, validQuote.QU_InternalReference);
			var message = GetQueuedUniversalEventMessage(incomingMessage, UniversalXmlInfo.Namespace_2012_11);
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			var importResults = manager.Process(message).ImportResults;

			var expectedError = @"The following Context fields are either empty or missing:
LedgerType
FromCurrency
FromAmount
ToCurrency
ToAmount
ExchangeRate
ExchangeRateInverted";
			if (messageMode == MessageModes.GetQuote)
			{
				expectedError += System.Environment.NewLine + "ProviderRef";
			}

			var expectedImportResult = FormattableString.Invariant(
$@"Linked Event to {validQuote.HumanReadableName}.
Error - {expectedError}");

			AssertEquals(expectedError, ErrorReporter.LastMessageReported);
			AssertEquals(expectedImportResult, importResults.Single().ToString());
			Assert(serviceTaskLog.HasErrors());
			AssertEquals(EDIMessageStatusList.Codes.Error, message.EM_Status);

			AssertEquals(EventTime, validQuote.QU_LastResponseReceivedUtc);
			AssertEquals(StatusCodes.Queued, validQuote.QU_Status);

			var quoteIAKLogs = validQuote.Logs.Find(l => l.SL_SE_NKEvent == Events.InterchangeAcknowledgedCode);
			AssertEquals(1, quoteIAKLogs.Count());
			var ediMessageLinkedToQuoteIAKLog = quoteIAKLogs.First().RelatedEDIMessage?.Message;
			AssertNotNull(ediMessageLinkedToQuoteIAKLog);

			var paymentApprovalIAKLogs = validQuote.PaymentApproval.Logs.Find(l => l.SL_SE_NKEvent == Events.InterchangeAcknowledgedCode);
			AssertEquals(1, paymentApprovalIAKLogs.Count());
			var ediMessageLinkedToPaymentApprovalIAKLog = paymentApprovalIAKLogs.First().RelatedEDIMessage?.Message;
			AssertNotNull(ediMessageLinkedToPaymentApprovalIAKLog);
			AssertEquals(ediMessageLinkedToQuoteIAKLog.PK, ediMessageLinkedToPaymentApprovalIAKLog.PK);

			ErrorReporter.Instance.Clear();
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

			var validQuote = Factory.NewWithValidTestData<AccEPaymentQuote>();
			validQuote.QU_InternalReference = "00001100";
			validQuote.QU_SystemCreateUser = creatingUser.GS_Code;
			validQuote.QU_GC = staffToken.TK_GC;

			Factory.SaveForTesting();
			Assert(validQuote.QU_LastResponseReceivedUtc.IsEmpty);
			Assert(validQuote.QU_ErrorDescription.IsEmpty);
			AssertNotEquals(StatusCodes.Error, validQuote.QU_Status);

			var eventDataObject = GetNewIncomingMessage();
			eventDataObject.DataContext.AddDataTarget(DataContextType.AccEPaymentQuote, validQuote.QU_InternalReference);
			eventDataObject.EventType = Events.InterchangeRejectedCode;
			eventDataObject.ContextCollection.Add(new Context() { Type = new ContextType { Type = XUEFieldNames.ErrorMessage }, Value = "There is no active refresh token." });

			var message = GetQueuedUniversalEventMessage(eventDataObject, UniversalXmlInfo.Namespace_2012_11);
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			var importResults = manager.Process(message).ImportResults;

			var expectedError = "Please retry your last action or re-authorize your OFX user account from your Bank Account before proceeding.";
			var expectedImportResult = FormattableString.Invariant(
$@"Linked Event to {validQuote.HumanReadableName}.");

			AssertEquals(expectedImportResult, importResults.Single().ToString());
			Assert(!serviceTaskLog.HasErrors());
			AssertEquals(EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);

			AssertEquals(EPaymentStatusCodes.Quote.Error, validQuote.QU_Status);
			AssertEquals(expectedError, validQuote.QU_ErrorDescription);

			var updatedToken = staffToken.Factory.LoadTop1<AccEPaymentStaffToken>(new ZQuery(AccEPaymentStaffTokenSchema.PK, staffToken.PK));
			AssertEquals(AccEPaymentStaffTokenLookups.StatusCodes.Error, updatedToken.TK_Status);
			AssertEquals(ZDateTime.Empty, updatedToken.TK_ExpiryUtc);
		}

		public void TestValidIRJResponse_WithMessage() => AssertValidIRJResponse("Here is an error");
		public void TestValidIRJResponse_NoMessage() => AssertValidIRJResponse(ZString.Empty);
		public void TestValidIRJResponse_NoMessageTag() => AssertValidIRJResponse(null);

		void AssertValidIRJResponse(ZString? errorMessage)
		{
			var validQuote = Factory.NewWithValidTestData<AccEPaymentQuote>();
			validQuote.QU_InternalReference = "00001000";

			var otherQuote = Factory.NewWithValidTestData<AccEPaymentQuote>();
			otherQuote.QU_InternalReference = "00001111";

			var newCompany = TestObjectCreator.CreateNewCompany("NEW");
			var otherCompanyQuote = Factory.NewWithValidTestData<AccEPaymentQuote>();
			otherCompanyQuote.QU_InternalReference = validQuote.QU_InternalReference;
			otherCompanyQuote.QU_GC = newCompany.PK;

			Factory.SaveForTesting();
			Assert(validQuote.QU_LastResponseReceivedUtc.IsEmpty);
			Assert(validQuote.QU_ErrorDescription.IsEmpty);
			AssertNotEquals(StatusCodes.Error, validQuote.QU_Status);

			var eventDataObject = GetNewIncomingMessage();
			eventDataObject.DataContext.AddDataTarget(DataContextType.AccEPaymentQuote, validQuote.QU_InternalReference);
			eventDataObject.EventType = Events.InterchangeRejectedCode;
			if (errorMessage.HasValue)
			{
				eventDataObject.ContextCollection.Add(new Context() { Type = new ContextType { Type = XUEFieldNames.ErrorMessage }, Value = errorMessage });
			}

			var message = GetQueuedUniversalEventMessage(eventDataObject, UniversalXmlInfo.Namespace_2012_11);
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			var importResults = manager.Process(message).ImportResults;
			var expectedImportResult = FormattableString.Invariant(
$@"Linked Event to {validQuote.HumanReadableName}.");

			AssertEquals(expectedImportResult, importResults.Single().ToString());
			Assert(!serviceTaskLog.HasErrors());
			AssertEquals(EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);

			AssertEquals(EventTime, validQuote.QU_LastResponseReceivedUtc);
			AssertEquals(errorMessage ?? ZString.Empty, validQuote.QU_ErrorDescription);
			AssertEquals(StatusCodes.Error, validQuote.QU_Status);

			var expectedLogReference = errorMessage.HasValue && !errorMessage.Value.IsEmpty ? $"OFX E-Quote Error: {errorMessage}" : "OFX E-Quote Error:";
			var quoteIRJLogs = validQuote.Logs.Find(l => l.SL_SE_NKEvent == Events.InterchangeRejectedCode && l.SL_Reference == expectedLogReference);
			AssertEquals(1, quoteIRJLogs.Count());
			var ediMessageLinkedToQuoteIRJLog = quoteIRJLogs.First().RelatedEDIMessage?.Message;
			AssertNotNull(ediMessageLinkedToQuoteIRJLog);

			var paymentApprovalIRJLogs = validQuote.PaymentApproval.Logs.Find(l => l.SL_SE_NKEvent == Events.InterchangeRejectedCode && l.SL_Reference == expectedLogReference);
			AssertEquals(1, paymentApprovalIRJLogs.Count());
			var ediMessageLinkedToPaymentApprovalIRJLog = paymentApprovalIRJLogs.First().RelatedEDIMessage?.Message;
			AssertNotNull(ediMessageLinkedToPaymentApprovalIRJLog);
			AssertEquals(ediMessageLinkedToQuoteIRJLog.PK, ediMessageLinkedToPaymentApprovalIRJLog.PK);
		}

		public void TestInvalidMessageMode()
		{
			var paymentApproval = TestObjectCreator.CreatePaymentApproval(ReceiptTypes.Cheque, TestObjectCreator.AUDBankAccount, TestObjectCreator.AUDChequeBook);
			paymentApproval.AV_RX_NKPaymentCurrency = CurrencyCodes.UnitedStates;
			paymentApproval.AV_Amount = 1000m;

			var validQuote = TestObjectCreator.CreateEPaymentQuote(paymentApproval, ProviderCodes.OFX);
			validQuote.QU_InternalReference = "00001000";
			Factory.SaveForTesting();

			var eventDataObject = GetNewIncomingMessage("AAA");
			eventDataObject.DataContext.AddDataTarget(DataContextType.AccEPaymentQuote, validQuote.QU_InternalReference);

			var message = GetQueuedUniversalEventMessage(eventDataObject, UniversalXmlInfo.Namespace_2012_11);
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			var importResults = manager.Process(message).ImportResults;

			var expectedError = "Message mode Could not find a valid Message Mode in the Event's <MessageSubType> parameter. Value found: 'AAA'";
			var expectedImportResult = FormattableString.Invariant(
$@"Linked Event to {validQuote.HumanReadableName}.
Error - {expectedError}");

			AssertEquals(expectedError, ErrorReporter.LastMessageReported);
			AssertEquals(expectedImportResult, importResults.Single().ToString());
			Assert(serviceTaskLog.HasErrors());
			AssertEquals(EDIMessageStatusList.Codes.Error, message.EM_Status);

			AssertEquals(EventTime, validQuote.QU_LastResponseReceivedUtc);
			AssertEquals(StatusCodes.Queued, validQuote.QU_Status);

			var quoteIAKLogs = validQuote.Logs.Find(l => l.SL_SE_NKEvent == Events.InterchangeAcknowledgedCode);
			AssertEquals(1, quoteIAKLogs.Count());
			var ediMessageLinkedToQuoteIAKLog = quoteIAKLogs.First().RelatedEDIMessage?.Message;
			AssertNotNull(ediMessageLinkedToQuoteIAKLog);

			var paymentApprovalIAKLogs = paymentApproval.Logs.Find(l => l.SL_SE_NKEvent == Events.InterchangeAcknowledgedCode);
			AssertEquals(1, paymentApprovalIAKLogs.Count());
			var ediMessageLinkedToPaymentApprovalIAKLog = paymentApprovalIAKLogs.First().RelatedEDIMessage?.Message;
			AssertNotNull(ediMessageLinkedToPaymentApprovalIAKLog);
			AssertEquals(ediMessageLinkedToQuoteIAKLog.PK, ediMessageLinkedToPaymentApprovalIAKLog.PK);

			ErrorReporter.Instance.Clear();
		}

		public void TestStatusNotRequested_Discarded()
		{
			var paymentApproval = TestObjectCreator.CreatePaymentApproval(ReceiptTypes.Cheque, TestObjectCreator.AUDBankAccount, TestObjectCreator.AUDChequeBook);
			paymentApproval.AV_RX_NKPaymentCurrency = CurrencyCodes.UnitedStates;
			paymentApproval.AV_Amount = 1000m;

			var validQuote = TestObjectCreator.CreateEPaymentQuote(paymentApproval, ProviderCodes.OFX);
			validQuote.QU_InternalReference = "00001000";
			validQuote.QU_Status = StatusCodes.Discarded;
			Factory.SaveForTesting();

			var eventDataObject = GetNewIncomingMessage();
			eventDataObject.DataContext.AddDataTarget(DataContextType.AccEPaymentQuote, validQuote.QU_InternalReference);
			eventDataObject.ContextCollection.Add(new Context() { Type = new ContextType { Type = XUEFieldNames.ProviderRef }, Value = "12345" });
			eventDataObject.ContextCollection.Add(new Context() { Type = new ContextType { Type = XUEFieldNames.LedgerType }, Value = LedgerTypes.AccountsPayable });
			eventDataObject.ContextCollection.Add(new Context() { Type = new ContextType { Type = XUEFieldNames.ToCurrency }, Value = CurrencyCodes.UnitedStates });
			eventDataObject.ContextCollection.Add(new Context() { Type = new ContextType { Type = XUEFieldNames.ToAmount }, Value = "1000" });
			eventDataObject.ContextCollection.Add(new Context() { Type = new ContextType { Type = XUEFieldNames.FromCurrency }, Value = CurrencyCodes.Australia });
			eventDataObject.ContextCollection.Add(new Context() { Type = new ContextType { Type = XUEFieldNames.FromAmount }, Value = "200" });
			eventDataObject.ContextCollection.Add(new Context() { Type = new ContextType { Type = XUEFieldNames.ExchangeRate }, Value = "5" });
			eventDataObject.ContextCollection.Add(new Context() { Type = new ContextType { Type = XUEFieldNames.ExchangeRateInverted }, Value = "0.2" });

			var message = GetQueuedUniversalEventMessage(eventDataObject, UniversalXmlInfo.Namespace_2012_11);
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			var importResults = manager.Process(message).ImportResults;

			var expectedImportResult = FormattableString.Invariant(
$@"Linked Event to {validQuote.HumanReadableName}.
Warning - Incoming E-Quote Message relates to Payment Quote 00001000, but this Quote has been discarded since it was requested.");

			AssertEquals(0, ErrorReporter.TotalErrorCount);
			AssertEquals(expectedImportResult, importResults.Single().ToString());
			Assert(!serviceTaskLog.HasErrors());
			AssertEquals(EDIMessageStatusList.Codes.Warning, message.EM_Status);

			AssertEquals(EventTime, validQuote.QU_LastResponseReceivedUtc);
			AssertEquals(StatusCodes.Discarded, validQuote.QU_Status);

			var quoteIAKLogs = validQuote.Logs.Find(l => l.SL_SE_NKEvent == Events.InterchangeAcknowledgedCode);
			AssertEquals(1, quoteIAKLogs.Count());
			var ediMessageLinkedToQuoteIAKLog = quoteIAKLogs.First().RelatedEDIMessage?.Message;
			AssertNotNull(ediMessageLinkedToQuoteIAKLog);

			var paymentApprovalIAKLogs = paymentApproval.Logs.Find(l => l.SL_SE_NKEvent == Events.InterchangeAcknowledgedCode);
			AssertEquals(1, paymentApprovalIAKLogs.Count());
			var ediMessageLinkedToPaymentApprovalIAKLog = paymentApprovalIAKLogs.First().RelatedEDIMessage?.Message;
			AssertNotNull(ediMessageLinkedToPaymentApprovalIAKLog);
			AssertEquals(ediMessageLinkedToQuoteIAKLog.PK, ediMessageLinkedToPaymentApprovalIAKLog.PK);
		}

		public void TestStatusNotRequested()
		{
			var paymentApproval = TestObjectCreator.CreatePaymentApproval(ReceiptTypes.Cheque, TestObjectCreator.AUDBankAccount, TestObjectCreator.AUDChequeBook);
			paymentApproval.AV_RX_NKPaymentCurrency = CurrencyCodes.UnitedStates;
			paymentApproval.AV_Amount = 1000m;

			var validQuote = TestObjectCreator.CreateEPaymentQuote(paymentApproval, ProviderCodes.OFX);
			validQuote.QU_InternalReference = "00001000";
			validQuote.QU_Status = StatusCodes.Queued;
			Factory.SaveForTesting();

			var eventDataObject = GetNewIncomingMessage();
			eventDataObject.DataContext.AddDataTarget(DataContextType.AccEPaymentQuote, validQuote.QU_InternalReference);
			eventDataObject.ContextCollection.Add(new Context() { Type = new ContextType { Type = XUEFieldNames.ProviderRef }, Value = "12345" });
			eventDataObject.ContextCollection.Add(new Context() { Type = new ContextType { Type = XUEFieldNames.LedgerType }, Value = LedgerTypes.AccountsPayable });
			eventDataObject.ContextCollection.Add(new Context() { Type = new ContextType { Type = XUEFieldNames.ToCurrency }, Value = CurrencyCodes.UnitedStates });
			eventDataObject.ContextCollection.Add(new Context() { Type = new ContextType { Type = XUEFieldNames.ToAmount }, Value = "1000" });
			eventDataObject.ContextCollection.Add(new Context() { Type = new ContextType { Type = XUEFieldNames.FromCurrency }, Value = CurrencyCodes.Australia });
			eventDataObject.ContextCollection.Add(new Context() { Type = new ContextType { Type = XUEFieldNames.FromAmount }, Value = "200" });
			eventDataObject.ContextCollection.Add(new Context() { Type = new ContextType { Type = XUEFieldNames.ExchangeRate }, Value = "5" });
			eventDataObject.ContextCollection.Add(new Context() { Type = new ContextType { Type = XUEFieldNames.ExchangeRateInverted }, Value = "0.2" });

			var message = GetQueuedUniversalEventMessage(eventDataObject, UniversalXmlInfo.Namespace_2012_11);
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			var importResults = manager.Process(message).ImportResults;

			var statusDesc = validQuote.Lookups.StatusCodeList.GetDescriptionFromCode(StatusCodes.Queued);
			var expectedImportResult = FormattableString.Invariant(
$@"Linked Event to {validQuote.HumanReadableName}.
Warning - Incoming E-Quote Message relates to Payment Quote 00001000, but this Quote is {statusDesc}. E-Quotes can be imported only when Payment Quote is in Requested status.");

			AssertEquals(0, ErrorReporter.TotalErrorCount);
			AssertEquals(expectedImportResult, importResults.Single().ToString());
			Assert(!serviceTaskLog.HasErrors());
			AssertEquals(EDIMessageStatusList.Codes.Warning, message.EM_Status);

			AssertEquals(EventTime, validQuote.QU_LastResponseReceivedUtc);
			AssertEquals(StatusCodes.Error, validQuote.QU_Status);

			var quoteIAKLogs = validQuote.Logs.Find(l => l.SL_SE_NKEvent == Events.InterchangeAcknowledgedCode);
			AssertEquals(1, quoteIAKLogs.Count());
			var ediMessageLinkedToQuoteIAKLog = quoteIAKLogs.First().RelatedEDIMessage?.Message;
			AssertNotNull(ediMessageLinkedToQuoteIAKLog);

			var paymentApprovalIAKLogs = paymentApproval.Logs.Find(l => l.SL_SE_NKEvent == Events.InterchangeAcknowledgedCode);
			AssertEquals(1, paymentApprovalIAKLogs.Count());
			var ediMessageLinkedToPaymentApprovalIAKLog = paymentApprovalIAKLogs.First().RelatedEDIMessage?.Message;
			AssertNotNull(ediMessageLinkedToPaymentApprovalIAKLog);
			AssertEquals(ediMessageLinkedToQuoteIAKLog.PK, ediMessageLinkedToPaymentApprovalIAKLog.PK);
		}

		public void TestDetailsChangedOnQuote_FromCurrency() => AssertDetailsChangedOnQuote((quote) => AlterQuoteDetails(quote.PK, quote.QU_ToAmount, quote.QU_RX_NKToCurrency, CurrencyCodes.Ireland));
		public void TestDetailsChangedOnQuote_ToCurrency() => AssertDetailsChangedOnQuote((quote) => AlterQuoteDetails(quote.PK, quote.QU_ToAmount, CurrencyCodes.Ireland, quote.QU_RX_NKFromCurrency));
		public void TestDetailsChangedOnQuote_ToAmount() => AssertDetailsChangedOnQuote((quote) => AlterQuoteDetails(quote.PK, 2000, quote.QU_RX_NKToCurrency, quote.QU_RX_NKFromCurrency));

		void AssertDetailsChangedOnQuote(Action<AccEPaymentQuote> alterQuote)
		{
			var paymentApproval = TestObjectCreator.CreatePaymentApproval(ReceiptTypes.Cheque, TestObjectCreator.AUDBankAccount, TestObjectCreator.AUDChequeBook);
			paymentApproval.AV_RX_NKPaymentCurrency = CurrencyCodes.UnitedStates;
			paymentApproval.AV_Amount = 1000m;

			var validQuote = TestObjectCreator.CreateEPaymentQuote(paymentApproval, ProviderCodes.OFX);
			validQuote.QU_InternalReference = "00001000";
			validQuote.QU_Status = StatusCodes.Requested;
			Factory.SaveForTesting();

			var eventDataObject = GetNewIncomingMessage();
			eventDataObject.DataContext.AddDataTarget(DataContextType.AccEPaymentQuote, validQuote.QU_InternalReference);
			eventDataObject.ContextCollection.Add(new Context() { Type = new ContextType { Type = XUEFieldNames.FromCurrency }, Value = CurrencyCodes.Australia });
			eventDataObject.ContextCollection.Add(new Context() { Type = new ContextType { Type = XUEFieldNames.ToCurrency }, Value = CurrencyCodes.UnitedStates });
			eventDataObject.ContextCollection.Add(new Context() { Type = new ContextType { Type = XUEFieldNames.ToAmount }, Value = "1000" });

			eventDataObject.ContextCollection.Add(new Context() { Type = new ContextType { Type = XUEFieldNames.ProviderRef }, Value = "12345" });
			eventDataObject.ContextCollection.Add(new Context() { Type = new ContextType { Type = XUEFieldNames.LedgerType }, Value = LedgerTypes.AccountsPayable });
			eventDataObject.ContextCollection.Add(new Context() { Type = new ContextType { Type = XUEFieldNames.FromAmount }, Value = "200" });
			eventDataObject.ContextCollection.Add(new Context() { Type = new ContextType { Type = XUEFieldNames.ExchangeRate }, Value = "5" });
			eventDataObject.ContextCollection.Add(new Context() { Type = new ContextType { Type = XUEFieldNames.ExchangeRateInverted }, Value = "0.2" });

			var message = GetQueuedUniversalEventMessage(eventDataObject, UniversalXmlInfo.Namespace_2012_11);

			alterQuote(validQuote);
			validQuote.Reload();

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			var importResults = manager.Process(message).ImportResults;

			var expectedError = "Quote details should not change after being requested.";

			var expectedImportResult = FormattableString.Invariant(
$@"Linked Event to {validQuote.HumanReadableName}.
Warning - {expectedError}");

			AssertEquals(expectedError, ErrorReporter.LastMessageReported);
			AssertEquals(expectedImportResult, importResults.Single().ToString());
			Assert(!serviceTaskLog.HasErrors());
			AssertEquals(EDIMessageStatusList.Codes.Warning, message.EM_Status);

			AssertEquals(EventTime, validQuote.QU_LastResponseReceivedUtc);
			AssertEquals(StatusCodes.Error, validQuote.QU_Status);

			var quoteIAKLogs = validQuote.Logs.Find(l => l.SL_SE_NKEvent == Events.InterchangeAcknowledgedCode);
			AssertEquals(1, quoteIAKLogs.Count());
			var ediMessageLinkedToQuoteIAKLog = quoteIAKLogs.First().RelatedEDIMessage?.Message;
			AssertNotNull(ediMessageLinkedToQuoteIAKLog);

			var paymentApprovalIAKLogs = paymentApproval.Logs.Find(l => l.SL_SE_NKEvent == Events.InterchangeAcknowledgedCode);
			AssertEquals(1, paymentApprovalIAKLogs.Count());
			var ediMessageLinkedToPaymentApprovalIAKLog = paymentApprovalIAKLogs.First().RelatedEDIMessage?.Message;
			AssertNotNull(ediMessageLinkedToPaymentApprovalIAKLog);
			AssertEquals(ediMessageLinkedToQuoteIAKLog.PK, ediMessageLinkedToPaymentApprovalIAKLog.PK);

			ErrorReporter.Instance.Clear();
		}

		void AlterQuoteDetails(ZGuid quotePK, ZDecimal toAmount, ZString toCurrency, ZString fromCurrency)
		{
			TestConnection.ExecuteNonQuery(FormattableString.Invariant($@"UPDATE {AccEPaymentQuoteSchema.Constants.SqlSchemaName}.{AccEPaymentQuoteSchema.Constants.TableName}
																		SET {AccEPaymentQuoteSchema.Constants.QU_ToAmount} = {toAmount}
																		, {AccEPaymentQuoteSchema.Constants.QU_RX_NKToCurrency} = '{toCurrency}'
																		, {AccEPaymentQuoteSchema.Constants.QU_RX_NKFromCurrency} = '{fromCurrency}'
																		, {AccEPaymentQuoteSchema.Constants.QU_SystemLastEditTimeUtc} = GETUTCDATE()
																		, {AccEPaymentQuoteSchema.Constants.QU_SystemLastEditUser} = 'TST'
																		WHERE {AccEPaymentQuoteSchema.Constants.PK} = '{quotePK}'"));
		}

		public void TestDetailsChangedOnPayment_PaymentCurrency() => AssertDetailsChangedOnPayment((payment) => AlterPaymentDetails(payment.PK, payment.AV_Amount, CurrencyCodes.Ireland));
		public void TestDetailsChangedOnPayment_PaymentAmount() => AssertDetailsChangedOnPayment((payment) => AlterPaymentDetails(payment.PK, 2000, payment.AV_RX_NKPaymentCurrency));

		void AssertDetailsChangedOnPayment(Action<AccPaymentApproval> alterPayment)
		{
			var paymentApproval = TestObjectCreator.CreatePaymentApproval(ReceiptTypes.Cheque, TestObjectCreator.AUDBankAccount, TestObjectCreator.AUDChequeBook);
			paymentApproval.AV_RX_NKPaymentCurrency = CurrencyCodes.UnitedStates;
			paymentApproval.AV_Amount = 1000m;

			var validQuote = TestObjectCreator.CreateEPaymentQuote(paymentApproval, ProviderCodes.OFX);
			validQuote.QU_InternalReference = "00001000";
			validQuote.QU_Status = StatusCodes.Requested;
			Factory.SaveForTesting();

			var eventDataObject = GetNewIncomingMessage();
			eventDataObject.DataContext.AddDataTarget(DataContextType.AccEPaymentQuote, validQuote.QU_InternalReference);
			eventDataObject.ContextCollection.Add(new Context() { Type = new ContextType { Type = XUEFieldNames.FromCurrency }, Value = CurrencyCodes.Australia });
			eventDataObject.ContextCollection.Add(new Context() { Type = new ContextType { Type = XUEFieldNames.ToCurrency }, Value = CurrencyCodes.UnitedStates });
			eventDataObject.ContextCollection.Add(new Context() { Type = new ContextType { Type = XUEFieldNames.ToAmount }, Value = "1000" });

			eventDataObject.ContextCollection.Add(new Context() { Type = new ContextType { Type = XUEFieldNames.ProviderRef }, Value = "12345" });
			eventDataObject.ContextCollection.Add(new Context() { Type = new ContextType { Type = XUEFieldNames.LedgerType }, Value = LedgerTypes.AccountsPayable });
			eventDataObject.ContextCollection.Add(new Context() { Type = new ContextType { Type = XUEFieldNames.FromAmount }, Value = "200" });
			eventDataObject.ContextCollection.Add(new Context() { Type = new ContextType { Type = XUEFieldNames.ExchangeRate }, Value = "5" });
			eventDataObject.ContextCollection.Add(new Context() { Type = new ContextType { Type = XUEFieldNames.ExchangeRateInverted }, Value = "0.2" });

			var message = GetQueuedUniversalEventMessage(eventDataObject, UniversalXmlInfo.Namespace_2012_11);

			alterPayment(validQuote.PaymentApproval);
			validQuote.PaymentApproval.Reload();

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			var importResults = manager.Process(message).ImportResults;

			var expectedError = "Quote should be discarded after payment details are changed.";

			var expectedImportResult = FormattableString.Invariant(
$@"Linked Event to {validQuote.HumanReadableName}.
Warning - {expectedError}");

			AssertEquals(expectedError, ErrorReporter.LastMessageReported);
			AssertEquals(expectedImportResult, importResults.Single().ToString());
			Assert(!serviceTaskLog.HasErrors());
			AssertEquals(EDIMessageStatusList.Codes.Warning, message.EM_Status);

			AssertEquals(EventTime, validQuote.QU_LastResponseReceivedUtc);
			AssertEquals(StatusCodes.Error, validQuote.QU_Status);

			var quoteIAKLogs = validQuote.Logs.Find(l => l.SL_SE_NKEvent == Events.InterchangeAcknowledgedCode);
			AssertEquals(1, quoteIAKLogs.Count());
			var ediMessageLinkedToQuoteIAKLog = quoteIAKLogs.First().RelatedEDIMessage?.Message;
			AssertNotNull(ediMessageLinkedToQuoteIAKLog);

			var paymentApprovalIAKLogs = paymentApproval.Logs.Find(l => l.SL_SE_NKEvent == Events.InterchangeAcknowledgedCode);
			AssertEquals(1, paymentApprovalIAKLogs.Count());
			var ediMessageLinkedToPaymentApprovalIAKLog = paymentApprovalIAKLogs.First().RelatedEDIMessage?.Message;
			AssertNotNull(ediMessageLinkedToPaymentApprovalIAKLog);
			AssertEquals(ediMessageLinkedToQuoteIAKLog.PK, ediMessageLinkedToPaymentApprovalIAKLog.PK);

			ErrorReporter.Instance.Clear();
		}

		void AlterPaymentDetails(ZGuid paymentApprovalPK, ZDecimal toAmount, ZString toCurrency)
		{
			TestConnection.ExecuteNonQuery(FormattableString.Invariant($@"UPDATE {AccPaymentApprovalSchema.Constants.SqlSchemaName}.{AccPaymentApprovalSchema.Constants.TableName}
																		SET {AccPaymentApprovalSchema.Constants.AV_Amount} = {toAmount}
																		, {AccPaymentApprovalSchema.Constants.AV_RX_NKPaymentCurrency} = '{toCurrency}'
																		, {AccPaymentApprovalSchema.Constants.AV_SystemLastEditTimeUtc} = GETUTCDATE()
																		, {AccPaymentApprovalSchema.Constants.AV_SystemLastEditUser} = 'TST'
																		WHERE {AccPaymentApprovalSchema.Constants.PK} = '{paymentApprovalPK}'"));
		}

		public void TestInvalidEventType()
		{
			var validQuote = Factory.NewWithValidTestData<AccEPaymentQuote>();
			validQuote.QU_InternalReference = "00001000";

			var otherQuote = Factory.NewWithValidTestData<AccEPaymentQuote>();
			otherQuote.QU_InternalReference = "00001111";

			var newCompany = TestObjectCreator.CreateNewCompany("NEW");
			var otherCompanyQuote = Factory.NewWithValidTestData<AccEPaymentQuote>();
			otherCompanyQuote.QU_InternalReference = validQuote.QU_InternalReference;
			otherCompanyQuote.QU_GC = newCompany.PK;

			Factory.SaveForTesting();
			Assert(validQuote.QU_LastResponseReceivedUtc.IsEmpty);

			var eventDataObject = GetNewIncomingMessage();
			eventDataObject.DataContext.AddDataTarget(DataContextType.AccEPaymentQuote, validQuote.QU_InternalReference);
			eventDataObject.EventType = Events.InterchangeReceivedCode;

			var message = GetQueuedUniversalEventMessage(eventDataObject, UniversalXmlInfo.Namespace_2012_11);
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);

			var importResults = manager.Process(message).ImportResults;

			var expectedError = $"Unexpected Event Type encountered: {Events.InterchangeReceivedCode}.";
			var expectedImportResult = FormattableString.Invariant(
$@"Linked Event to {validQuote.HumanReadableName}.
Error - {expectedError}");

			AssertEquals(expectedError, ErrorReporter.LastMessageReported);
			AssertEquals(expectedImportResult, importResults.Single().ToString());
			AssertEquals(1, validQuote.Logs.Find(l => l.SL_SE_NKEvent == Events.InterchangeReceivedCode).Count());
			AssertEquals(1, validQuote.PaymentApproval.Logs.Find(l => l.SL_SE_NKEvent == Events.InterchangeReceivedCode).Count());
			Assert(serviceTaskLog.HasErrors());
			AssertEquals(EDIMessageStatusList.Codes.Error, message.EM_Status);

			AssertEquals(EventTime, validQuote.QU_LastResponseReceivedUtc);

			var quoteIAKLogs = validQuote.Logs.Find(l => l.SL_SE_NKEvent == Events.InterchangeReceivedCode);
			AssertEquals(1, quoteIAKLogs.Count());
			var ediMessageLinkedToQuoteIAKLog = quoteIAKLogs.First().RelatedEDIMessage?.Message;
			AssertNotNull(ediMessageLinkedToQuoteIAKLog);

			var paymentApprovalIAKLogs = validQuote.PaymentApproval.Logs.Find(l => l.SL_SE_NKEvent == Events.InterchangeReceivedCode);
			AssertEquals(1, paymentApprovalIAKLogs.Count());
			var ediMessageLinkedToPaymentApprovalIAKLog = paymentApprovalIAKLogs.First().RelatedEDIMessage?.Message;
			AssertNotNull(ediMessageLinkedToPaymentApprovalIAKLog);
			AssertEquals(ediMessageLinkedToQuoteIAKLog.PK, ediMessageLinkedToPaymentApprovalIAKLog.PK);

			ErrorReporter.Instance.Clear();
		}

		public void TestInvalidXMLFormat()
		{
			var eventDataObject = GetNewIncomingMessage();
			eventDataObject.DataContext = new UniversalDataBuss.DataObjects.Universal._2011_11.DataContext();
			eventDataObject.DataContext.AddDataTarget(DataContextType.AccEPaymentQuote, "00001000");

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
			var eventDataObject = GetNewIncomingMessage();
			eventDataObject.DataContext.AddDataTarget(DataContextType.AccEPaymentQuote, "00001000");
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

		public void TestMissingQuoteNumber()
		{
			var eventDataObject = GetNewIncomingMessage();
			eventDataObject.DataContext.AddDataTarget(DataContextType.AccEPaymentQuote, ZString.Empty);

			var message = GetQueuedUniversalEventMessage(eventDataObject, UniversalXmlInfo.Namespace_2012_11);
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);
			AssertEquals(
@"ERROR - Could not find a valid Quote Number in the <DataTarget> Key parameter. Value found: ''
Warning - No Module found a Business Entity to link this Universal Event to.", serviceTaskLog.ToString());
			AssertEquals(EDIMessageStatusList.Codes.Discarded, message.EM_Status);
		}

		public void TestNoContextCollection()
		{
			var eventDataObject = GetNewIncomingMessage();
			eventDataObject.DataContext.AddDataTarget(DataContextType.AccEPaymentQuote, "00001000");
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
			var quote = GetNewBusinessObjectForTesting();
			quote.QU_InternalReference = "00001000";

			AssertEquals(DataContextType.AccEPaymentQuote, quote.GetUniversalDataContextManager().DataContextType);
			AssertEquals("00001000", quote.GetUniversalDataContextManager().DataContextKey);
		}

		protected override void TestBusinessObjectImplementsIJobNumberCore()
		{
			Assert("No Job Number support", true);
		}

		UniversalEventDataObject GetNewIncomingMessage(string messageMode = "GAQ")
		{
			var eventDataObject = new UniversalEventDataObject();
			eventDataObject.DataContext = new UniversalDataBuss.DataObjects.Universal._2012_11.DataContext();
			eventDataObject.EventType = Events.InterchangeAcknowledgedCode;
			eventDataObject.EventTime = EventTime.ToOffset();
			eventDataObject.EventParameters = new EventParameters { MessageType = ProviderCodes.OFX, MessageSubType = messageMode };
			eventDataObject.ContextCollection = new List<Context>();
			eventDataObject.ContextCollection.Add(new Context() { Type = new ContextType { Type = XUEFieldNames.CompanyCode }, Value = GlbCompany.CurrentCompany.GC_Code });
			return eventDataObject;
		}

		protected override AccEPaymentQuote GetNewBusinessObjectForTesting()
		{
			var quote = Factory.NewWithValidTestData<AccEPaymentQuote>();
			Factory.SaveForTesting();
			return quote;
		}

		protected TestObjectCreator TestObjectCreator => testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory.BOFactory));
		TestObjectCreator testObjectCreator;
		readonly ZDateTime EventTime = ZDateTime.Now.AddHours(-1);
	}
}
