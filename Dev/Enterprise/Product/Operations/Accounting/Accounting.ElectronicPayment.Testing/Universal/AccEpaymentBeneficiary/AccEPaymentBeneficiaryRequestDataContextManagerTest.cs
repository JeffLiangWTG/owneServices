using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.ElectronicPayment.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.UniversalDataBuss.Management.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Accounting.ElectronicPayment.Universal.AccEPaymentBeneficiaryRequestMessageConstants;
using ProviderCodes = Enterprise.MasterFiles.Business.EPaymentProviderCodes.Codes;
using UniversalEventDataObject = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Accounting.ElectronicPayment.Testing.Universal
{
	[TestedType(typeof(AccEPaymentBeneficiaryRequestDataContextManager))]
	public class AccEPaymentBeneficiaryRequestDataContextManagerTest : DataContextManagerTestCase<AccEPaymentBeneficiaryRequestDataContextManager, AccEPaymentBeneficiaryRequest>
	{
		public void TestIAKMessage_Complete()
		{
			var request = Factory.NewWithValidTestData<AccEPaymentBeneficiaryRequest>();
			request.ABR_InternalReference = "00001000";
			request.ABR_Status = EPaymentStatusCodes.BeneficiaryRequest.Requested;
			Factory.SaveForTesting();

			var eventDataObject = GetNewIncomingIAKMessage(true, () => ConvertToBase64(beneficiarySearchResult1));
			eventDataObject.DataContext.AddDataTarget(DataContextType.AccEPaymentBeneficiaryRequest, request.ABR_InternalReference);

			var message = GetQueuedUniversalEventMessage(eventDataObject, UniversalXmlInfo.Namespace_2012_11);
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			var importResults = manager.Process(message).ImportResults;

			var expectedImportResult = FormattableString.Invariant(
$@"Linked Event to {request.HumanReadableName}.");

			AssertEquals(expectedImportResult, importResults.Single().ToString());
			Assert(!serviceTaskLog.HasErrors());
			AssertEquals(EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);

			AssertEquals(EPaymentStatusCodes.BeneficiaryRequest.Received, request.ABR_Status);
			AssertEquals(EventTime, request.ABR_LastResponseReceivedUtc);
			AssertEquals(ZString.Empty, request.ABR_ErrorDescription);

			var expectedLogReference = "All beneficiaries are returned successfully by OFX against beneficiary search request: 00001000";
			var requestIAKLogs = request.Logs.Find(l => l.SL_SE_NKEvent == Events.InterchangeAcknowledgedCode && l.SL_Reference == expectedLogReference);
			AssertEquals(1, requestIAKLogs.Count());
			var ediMessageLinkedToBeneficiaryRequestIAKLog = requestIAKLogs.First().RelatedEDIMessage?.Message;
			AssertNotNull(ediMessageLinkedToBeneficiaryRequestIAKLog);

			var bens = Factory.Load<AccEPaymentBeneficiary>(new ZQuery()).OrderBy(p => p.ABF_BeneficiaryNickName).ToArray();
			AssertEquals(2, bens.Length);
			AssertFirstAccEPaymentBeneficiary(bens[0]);
			AssertSecondAccEPaymentBeneficiary(bens[1]);
		}

		public void TestIAKMessage_NotComplete()
		{
			var messages = Factory.Load<EDIMessage>(new ZQuery(EDIMessageSchema.EM_ApplicationCode, "GEP"));
			AssertEquals(0, messages.Length);

			var objectFactory = new BusinessObjectFactory();
			var objectCreator = new TestObjectCreator(objectFactory);
			var testHelper = new EPaymentTestHelper(objectCreator);
			var beneficiaryRequestCreatingUser = objectCreator.CreateStaff("YOU");
			var ofxBankAccount = testHelper.CreateOFXPaymentProviderBankAccount(GlbCompany.CurrentCompany.PK);
			var staffToken = testHelper.CreateStaffToken(ofxBankAccount.PK, ZDateTime.UtcNow.AddDays(1), GlbCompany.CurrentCompany.PK, beneficiaryRequestCreatingUser.GS_Code, AccEPaymentStaffTokenLookups.StatusCodes.Authorised);
			staffToken.TK_AccountName = "This name is provided by OFX.";
			objectFactory.Save();

			var request = Factory.NewWithValidTestData<AccEPaymentBeneficiaryRequest>();
			request.ABR_InternalReference = "00001000";
			request.ABR_Status = EPaymentStatusCodes.BeneficiaryRequest.Requested;
			request.ABR_SystemCreateUser = beneficiaryRequestCreatingUser.GS_Code;
			Factory.SaveForTesting();

			var eventDataObject = GetNewIncomingIAKMessage(true, () => ConvertToBase64(beneficiarySearchResult2));
			eventDataObject.DataContext.AddDataTarget(DataContextType.AccEPaymentBeneficiaryRequest, request.ABR_InternalReference);

			var message = GetQueuedUniversalEventMessage(eventDataObject, UniversalXmlInfo.Namespace_2012_11);
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			var importResults = manager.Process(message).ImportResults;

			var expectedImportResult = FormattableString.Invariant(
$@"Linked Event to {request.HumanReadableName}.");

			AssertEquals(expectedImportResult, importResults.Single().ToString());
			Assert(!serviceTaskLog.HasErrors());
			AssertEquals(EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);

			AssertEquals(EPaymentStatusCodes.BeneficiaryRequest.Partial, request.ABR_Status);
			AssertEquals(EventTime, request.ABR_LastResponseReceivedUtc);
			AssertEquals(ZString.Empty, request.ABR_ErrorDescription);

			var expectedLogReference = "Received information of 2 beneficiaries from OFX against beneficiary search request: 00001000. This is a partial list of beneficiaries.";
			var requestIAKLogs = request.Logs.Find(l => l.SL_SE_NKEvent == Events.InterchangeAcknowledgedCode && l.SL_Reference == expectedLogReference);
			AssertEquals(1, requestIAKLogs.Count());
			var ediMessageLinkedToBeneficiaryRequestIAKLog = requestIAKLogs.First().RelatedEDIMessage?.Message;
			AssertNotNull(ediMessageLinkedToBeneficiaryRequestIAKLog);

			var bens = Factory.Load<AccEPaymentBeneficiary>(new ZQuery()).OrderBy(p => p.ABF_BeneficiaryNickName).ToArray();
			AssertEquals(2, bens.Length);
			AssertFirstAccEPaymentBeneficiary(bens[0]);
			AssertSecondAccEPaymentBeneficiary(bens[1]);

			messages = Factory.Load<EDIMessage>(new ZQuery(EDIMessageSchema.EM_ApplicationCode, "GEP"));
			AssertEquals(1, messages.Length);
		}

		public void TestIAKMessage_UpdateExisingAPAccounts()
		{
			var benDetails = Factory.NewWithValidTestData<AccEPaymentBeneficiary>();
			benDetails.ABF_ProviderCode = "OFX";
			benDetails.ABF_ProviderReference = "55fe2c0f-a867-49fe-b9f3-76a33fff5e89";
			Factory.SaveForTesting();

			var apAccountDetails = Factory.NewWithValidTestData<AccAPAccountDetails>();
			apAccountDetails.A1_EPaymentBeneficiaryId = benDetails.PK;
			apAccountDetails.A1_PaymentMethod = "EPO";
			Factory.SaveForTesting();

			var objectFactory = new BusinessObjectFactory();
			var objectCreator = new TestObjectCreator(objectFactory);
			var testHelper = new EPaymentTestHelper(objectCreator);
			var beneficiaryRequestCreatingUser = objectCreator.CreateStaff("YOU");
			var ofxBankAccount = testHelper.CreateOFXPaymentProviderBankAccount(GlbCompany.CurrentCompany.PK);
			var staffToken = testHelper.CreateStaffToken(ofxBankAccount.PK, ZDateTime.UtcNow.AddDays(1), GlbCompany.CurrentCompany.PK, beneficiaryRequestCreatingUser.GS_Code, AccEPaymentStaffTokenLookups.StatusCodes.Authorised);
			staffToken.TK_AccountName = "This name is provided by OFX.";
			objectFactory.Save();

			var request = Factory.NewWithValidTestData<AccEPaymentBeneficiaryRequest>();
			request.ABR_InternalReference = "00001000";
			request.ABR_Status = EPaymentStatusCodes.BeneficiaryRequest.Requested;
			request.ABR_SystemCreateUser = beneficiaryRequestCreatingUser.GS_Code;
			Factory.SaveForTesting();

			var eventDataObject = GetNewIncomingIAKMessage(true, () => ConvertToBase64(beneficiarySearchResult2));
			eventDataObject.DataContext.AddDataTarget(DataContextType.AccEPaymentBeneficiaryRequest, request.ABR_InternalReference);

			var message = GetQueuedUniversalEventMessage(eventDataObject, UniversalXmlInfo.Namespace_2012_11);
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			var importResults = manager.Process(message).ImportResults;

			var expectedImportResult = FormattableString.Invariant(
$@"Linked Event to {request.HumanReadableName}.");

			AssertEquals(expectedImportResult, importResults.Single().ToString());
			Assert(!serviceTaskLog.HasErrors());
			AssertEquals(EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);

			AssertEquals(EPaymentStatusCodes.BeneficiaryRequest.Partial, request.ABR_Status);
			AssertEquals(EventTime, request.ABR_LastResponseReceivedUtc);
			AssertEquals(ZString.Empty, request.ABR_ErrorDescription);

			var expectedLogReference = "Received information of 2 beneficiaries from OFX against beneficiary search request: 00001000. This is a partial list of beneficiaries.";
			var requestIAKLogs = request.Logs.Find(l => l.SL_SE_NKEvent == Events.InterchangeAcknowledgedCode && l.SL_Reference == expectedLogReference);
			AssertEquals(1, requestIAKLogs.Count());
			var ediMessageLinkedToBeneficiaryRequestIAKLog = requestIAKLogs.First().RelatedEDIMessage?.Message;
			AssertNotNull(ediMessageLinkedToBeneficiaryRequestIAKLog);

			var bens = Factory.Load<AccEPaymentBeneficiary>(new ZQuery()).OrderBy(p => p.ABF_BeneficiaryNickName).ToArray();
			AssertEquals(2, bens.Length);
			AssertFirstAccEPaymentBeneficiary(bens[0]);
			AssertSecondAccEPaymentBeneficiary(bens[1]);

			AssertAccAPAccountDetails(apAccountDetails);
		}

		public void TestIAKMessage_NoBeneficiaryInfo()
		{
			var request = Factory.NewWithValidTestData<AccEPaymentBeneficiaryRequest>();
			request.ABR_InternalReference = "00001000";
			request.ABR_Status = EPaymentStatusCodes.BeneficiaryRequest.Requested;
			Factory.SaveForTesting();

			var eventDataObject = GetNewIncomingIAKMessage(true, () => ConvertToBase64(beneficiarySearchResult3));
			eventDataObject.DataContext.AddDataTarget(DataContextType.AccEPaymentBeneficiaryRequest, request.ABR_InternalReference);

			var message = GetQueuedUniversalEventMessage(eventDataObject, UniversalXmlInfo.Namespace_2012_11);
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			var importResults = manager.Process(message).ImportResults;

			var expectedImportResult = FormattableString.Invariant(
$@"Linked Event to {request.HumanReadableName}.");

			AssertEquals(expectedImportResult, importResults.Single().ToString());
			Assert(!serviceTaskLog.HasErrors());
			AssertEquals(EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);

			AssertEquals(EPaymentStatusCodes.BeneficiaryRequest.Received, request.ABR_Status);
			AssertEquals(EventTime, request.ABR_LastResponseReceivedUtc);
			AssertEquals(ZString.Empty, request.ABR_ErrorDescription);

			var expectedLogReference = "No Beneficiary is returned by OFX";
			var requestIAKLogs = request.Logs.Find(l => l.SL_SE_NKEvent == Events.InterchangeAcknowledgedCode && l.SL_Reference == expectedLogReference);
			AssertEquals(1, requestIAKLogs.Count());
			var ediMessageLinkedToBeneficiaryRequestIAKLog = requestIAKLogs.First().RelatedEDIMessage?.Message;
			AssertNotNull(ediMessageLinkedToBeneficiaryRequestIAKLog);
		}

		public void TestIAKMessage_MissingContexts()
		{
			var request = Factory.NewWithValidTestData<AccEPaymentBeneficiaryRequest>();
			request.ABR_InternalReference = "00001000";
			request.ABR_Status = EPaymentStatusCodes.BeneficiaryRequest.Requested;
			Factory.SaveForTesting();

			var eventDataObject = GetNewIncomingIAKMessage(false, () => string.Empty);
			eventDataObject.DataContext.AddDataTarget(DataContextType.AccEPaymentBeneficiaryRequest, request.ABR_InternalReference);

			var message = GetQueuedUniversalEventMessage(eventDataObject, UniversalXmlInfo.Namespace_2012_11);
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			var importResults = manager.Process(message).ImportResults;

			var expectedError = @"The following Context fields are either empty or missing:
BeneficiarySearchResult";

			var expectedImportResult = FormattableString.Invariant(
$@"Linked Event to {request.HumanReadableName}.
Error - {expectedError}");

			AssertEquals(expectedError, ErrorReporter.LastMessageReported);
			AssertEquals(expectedImportResult, importResults.Single().ToString());
			Assert(serviceTaskLog.HasErrors());
			AssertEquals(EDIMessageStatusList.Codes.Error, message.EM_Status);

			AssertEquals(EPaymentStatusCodes.BeneficiaryRequest.Requested, request.ABR_Status);
			AssertEquals(ZDateTime.Empty, request.ABR_LastResponseReceivedUtc);
			AssertEquals(ZString.Empty, request.ABR_ErrorDescription);

			var requestIAKLogs = request.Logs.Find(l => l.SL_SE_NKEvent == Events.InterchangeAcknowledgedCode);
			AssertEquals(1, requestIAKLogs.Count());
			var ediMessageLinkedToDealIAKLog = requestIAKLogs.First().RelatedEDIMessage?.Message;
			AssertNotNull(ediMessageLinkedToDealIAKLog);

			ErrorReporter.Instance.Clear();
		}

		public void TestIRJMessage()
		{
			var request = Factory.NewWithValidTestData<AccEPaymentBeneficiaryRequest>();
			request.ABR_InternalReference = "00001000";
			request.ABR_Status = EPaymentStatusCodes.BeneficiaryRequest.Requested;
			Factory.SaveForTesting();

			var eventDataObject = GetNewIncomingIRJMessage();
			eventDataObject.DataContext.AddDataTarget(DataContextType.AccEPaymentBeneficiaryRequest, request.ABR_InternalReference);

			var message = GetQueuedUniversalEventMessage(eventDataObject, UniversalXmlInfo.Namespace_2012_11);
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			var importResults = manager.Process(message).ImportResults;

			var expectedError = "Some weird error happened";
			var expectedImportResult = FormattableString.Invariant(
$@"Linked Event to {request.HumanReadableName}.");

			AssertEquals(expectedImportResult, importResults.Single().ToString());
			Assert(!serviceTaskLog.HasErrors());
			AssertEquals(EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);

			AssertEquals(EPaymentStatusCodes.BeneficiaryRequest.Error, request.ABR_Status);
			AssertEquals(EventTime, request.ABR_LastResponseReceivedUtc);
			AssertEquals(expectedError, request.ABR_ErrorDescription);

			var expectedLogReference = $"OFX E-Payment Error: {expectedError}";
			var requestIRJLogs = request.Logs.Find(l => l.SL_SE_NKEvent == Events.InterchangeRejectedCode && l.SL_Reference == expectedLogReference);
			AssertEquals(1, requestIRJLogs.Count());
			var ediMessageLinkedToRequestIRJLog = requestIRJLogs.First().RelatedEDIMessage?.Message;
			AssertNotNull(ediMessageLinkedToRequestIRJLog);
		}

		public void TestIRJMessage_StaffTokenInvalidatedWhenNoRefreshToken()
		{
			var messages = Factory.Load<EDIMessage>(new ZQuery(EDIMessageSchema.EM_ApplicationCode, "GEP"));
			AssertEquals(0, messages.Length);

			var objectFactory = new BusinessObjectFactory();
			var objectCreator = new TestObjectCreator(objectFactory);
			var testHelper = new EPaymentTestHelper(objectCreator);
			var beneficiaryRequestCreatingUser = objectCreator.CreateStaff("YOU");
			var ofxBankAccount = testHelper.CreateOFXPaymentProviderBankAccount(GlbCompany.CurrentCompany.PK);
			var staffToken = testHelper.CreateStaffToken(ofxBankAccount.PK, ZDateTime.UtcNow.AddDays(1), GlbCompany.CurrentCompany.PK, beneficiaryRequestCreatingUser.GS_Code, AccEPaymentStaffTokenLookups.StatusCodes.Authorised);
			staffToken.TK_AccountName = "This name is provided by OFX.";
			objectFactory.Save();

			AssertEquals(AccEPaymentStaffTokenLookups.StatusCodes.Authorised, staffToken.TK_Status);

			var request = Factory.NewWithValidTestData<AccEPaymentBeneficiaryRequest>();
			request.ABR_InternalReference = "00001000";
			request.ABR_Status = EPaymentStatusCodes.BeneficiaryRequest.Requested;
			request.ABR_SystemCreateUser = beneficiaryRequestCreatingUser.GS_Code;
			Factory.SaveForTesting();

			var eventDataObject = GetNewIncomingIRJMessage("There is no active refresh token.");
			eventDataObject.DataContext.AddDataTarget(DataContextType.AccEPaymentBeneficiaryRequest, request.ABR_InternalReference);

			var message = GetQueuedUniversalEventMessage(eventDataObject, UniversalXmlInfo.Namespace_2012_11);
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			var importResults = manager.Process(message).ImportResults;

			var expectedError = "Please retry your last action or re-authorize your OFX user account from your Bank Account before proceeding.";
			var expectedImportResult = FormattableString.Invariant(
$@"Linked Event to {request.HumanReadableName}.");

			AssertEquals(expectedImportResult, importResults.Single().ToString());
			Assert(!serviceTaskLog.HasErrors());
			AssertEquals(EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);

			AssertEquals(EPaymentStatusCodes.BeneficiaryRequest.Error, request.ABR_Status);
			AssertEquals(EventTime, request.ABR_LastResponseReceivedUtc);
			AssertEquals(expectedError, request.ABR_ErrorDescription);

			var updatedToken = staffToken.Factory.LoadTop1<AccEPaymentStaffToken>(new ZQuery(AccEPaymentStaffTokenSchema.PK, staffToken.PK));
			AssertEquals(AccEPaymentStaffTokenLookups.StatusCodes.Error, updatedToken.TK_Status);
			AssertEquals(ZDateTime.Empty, updatedToken.TK_ExpiryUtc);
		}

		protected override void TestBusinessObjectImplementsIJobNumberCore()
		{
			Assert("No Job Number support", true);
		}

		UniversalEventDataObject GetNewIncomingIAKMessage(bool populateRequiedFields, Func<string> getBeneficiarySearchResult)
		{
			var eventDataObject = new UniversalEventDataObject();
			eventDataObject.DataContext = new UniversalDataBuss.DataObjects.Universal._2012_11.DataContext();
			eventDataObject.EventType = Events.InterchangeAcknowledgedCode;
			eventDataObject.EventTime = EventTime.ToOffset();
			eventDataObject.DataContext.AddDataTarget(DataContextType.AccEPaymentBeneficiaryRequest, "00001000");
			eventDataObject.EventParameters = new EventParameters { MessageType = ProviderCodes.OFX, MessageSubType = MessageSubTypes.SearchBeneficiary };
			eventDataObject.ContextCollection = new List<Context>();
			if (populateRequiedFields)
			{
				eventDataObject.ContextCollection.Add(new Context() { Type = new ContextType { Type = XUEFieldNames.BeneficiarySearchResult }, Value = getBeneficiarySearchResult?.Invoke() });
			}
			eventDataObject.ContextCollection.Add(new Context() { Type = new ContextType { Type = XUEFieldNames.CompanyCode }, Value = GlbCompany.CurrentCompany.GC_Code });
			eventDataObject.ContextCollection.Add(new Context() { Type = new ContextType { Type = XUEFieldNames.BranchCode }, Value = GlbBranch.CurrentBranch.GB_Code });
			return eventDataObject;
		}

		UniversalEventDataObject GetNewIncomingIRJMessage(string reason = "Some weird error happened")
		{
			var eventDataObject = new UniversalEventDataObject();
			eventDataObject.DataContext = new UniversalDataBuss.DataObjects.Universal._2012_11.DataContext();
			eventDataObject.EventType = Events.InterchangeRejectedCode;
			eventDataObject.EventTime = EventTime.ToOffset();
			eventDataObject.EventParameters = new EventParameters { MessageType = ProviderCodes.OFX, MessageSubType = AccEPaymentBeneficiaryRequestMessageConstants.MessageSubTypes.SearchBeneficiary, Reason = "Some weird error happened" };
			eventDataObject.ContextCollection = new List<Context>();
			eventDataObject.ContextCollection.Add(new Context() { Type = new ContextType { Type = XUEFieldNames.ErrorType }, Value = "Generic" });
			eventDataObject.ContextCollection.Add(new Context() { Type = new ContextType { Type = XUEFieldNames.ErrorOriginatesAt }, Value = "Electronic Payment Service Provider" });
			eventDataObject.ContextCollection.Add(new Context() { Type = new ContextType { Type = XUEFieldNames.ErrorMessage }, Value = reason });
			eventDataObject.ContextCollection.Add(new Context() { Type = new ContextType { Type = XUEFieldNames.CompanyCode }, Value = GlbCompany.CurrentCompany.GC_Code });
			eventDataObject.ContextCollection.Add(new Context() { Type = new ContextType { Type = XUEFieldNames.BranchCode }, Value = GlbBranch.CurrentBranch.GB_Code });
			eventDataObject.ContextCollection.Add(new Context() { Type = new ContextType { Type = "StreamContent" }, Value = "U29tZSB3ZWlyZCBlcnJvciBoYXBwZW5lZA==" });
			return eventDataObject;
		}

		protected override AccEPaymentBeneficiaryRequest GetNewBusinessObjectForTesting()
		{
			var beneficiaryRequest = Factory.NewWithValidTestData<AccEPaymentBeneficiaryRequest>();
			Factory.SaveForTesting();
			return beneficiaryRequest;
		}

		protected TestObjectCreator TestObjectCreator => testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory.BOFactory));
		TestObjectCreator testObjectCreator;

		readonly ZDateTime EventTime = ZDateTime.Now.AddHours(-1);

		string ConvertToBase64(string plainText) => Convert.ToBase64String(new UTF8Encoding(false).GetBytes(plainText));

		void AssertFirstAccEPaymentBeneficiary(AccEPaymentBeneficiary firstBeneficiary)
		{
			AssertEquals(nameof(firstBeneficiary.ABF_BankAccount), "45663", firstBeneficiary.ABF_BankAccount);
			AssertEquals(nameof(firstBeneficiary.ABF_BankAddress1), "1, vaChatswood Branch, Sydney, New South Wales 20", firstBeneficiary.ABF_BankAddress1);
			AssertEquals(nameof(firstBeneficiary.ABF_BankBranchName), "C", firstBeneficiary.ABF_BankBranchName);
			AssertEquals(nameof(firstBeneficiary.ABF_BankBsb), "234434", firstBeneficiary.ABF_BankBsb);
			AssertEquals(nameof(firstBeneficiary.ABF_BankName), "Westpac AU", firstBeneficiary.ABF_BankName);
			AssertEquals(nameof(firstBeneficiary.ABF_BankSwift), "EDRFAU22", firstBeneficiary.ABF_BankSwift);
			AssertEquals(nameof(firstBeneficiary.ABF_BeneficiaryFullName), "J", firstBeneficiary.ABF_BeneficiaryFullName);
			AssertEquals(nameof(firstBeneficiary.ABF_BeneficiaryNickName), "J", firstBeneficiary.ABF_BeneficiaryNickName);
			AssertEquals(nameof(firstBeneficiary.ABF_EmailAddress), string.Empty, firstBeneficiary.ABF_EmailAddress);
			AssertEquals(nameof(firstBeneficiary.ABF_ProviderClassification), "Own Account", firstBeneficiary.ABF_ProviderClassification);
			AssertEquals(nameof(firstBeneficiary.ABF_ProviderCode), "OFX", firstBeneficiary.ABF_ProviderCode);
			AssertEquals(nameof(firstBeneficiary.ABF_ProviderReference), "55fe2c0f-a867-49fe-b9f3-76a33fff5e89", firstBeneficiary.ABF_ProviderReference);
			AssertEquals(nameof(firstBeneficiary.ABF_RN_NKCountryCode), "AU", firstBeneficiary.ABF_RN_NKCountryCode);
			AssertEquals(nameof(firstBeneficiary.ABF_RX_NKAccountCurrency), "USD", firstBeneficiary.ABF_RX_NKAccountCurrency);
		}

		void AssertSecondAccEPaymentBeneficiary(AccEPaymentBeneficiary secondBeneficiary)
		{
			AssertEquals(nameof(secondBeneficiary.ABF_BankAccount), "111589", secondBeneficiary.ABF_BankAccount);
			AssertEquals(nameof(secondBeneficiary.ABF_BankAddress1), "1 Market St, Sydney, New South Wales 2060, AU", secondBeneficiary.ABF_BankAddress1);
			AssertEquals(nameof(secondBeneficiary.ABF_BankBranchName), "Z", secondBeneficiary.ABF_BankBranchName);
			AssertEquals(nameof(secondBeneficiary.ABF_BankBsb), "234434", secondBeneficiary.ABF_BankBsb);
			AssertEquals(nameof(secondBeneficiary.ABF_BankName), "Commonwealth AU", secondBeneficiary.ABF_BankName);
			AssertEquals(nameof(secondBeneficiary.ABF_BankSwift), "EDRFAU234", secondBeneficiary.ABF_BankSwift);
			AssertEquals(nameof(secondBeneficiary.ABF_BeneficiaryFullName), "M", secondBeneficiary.ABF_BeneficiaryFullName);
			AssertEquals(nameof(secondBeneficiary.ABF_BeneficiaryNickName), "M", secondBeneficiary.ABF_BeneficiaryNickName);
			AssertEquals(nameof(secondBeneficiary.ABF_EmailAddress), string.Empty, secondBeneficiary.ABF_EmailAddress);
			AssertEquals(nameof(secondBeneficiary.ABF_ProviderClassification), "High Risk", secondBeneficiary.ABF_ProviderClassification);
			AssertEquals(nameof(secondBeneficiary.ABF_ProviderCode), "OFX", secondBeneficiary.ABF_ProviderCode);
			AssertEquals(nameof(secondBeneficiary.ABF_ProviderReference), "da0b0c15-ae2d-4b7e-84c9-45e3274979c3", secondBeneficiary.ABF_ProviderReference);
			AssertEquals(nameof(secondBeneficiary.ABF_RN_NKCountryCode), "AU", secondBeneficiary.ABF_RN_NKCountryCode);
			AssertEquals(nameof(secondBeneficiary.ABF_RX_NKAccountCurrency), "EUR", secondBeneficiary.ABF_RX_NKAccountCurrency);
		}

		void AssertAccAPAccountDetails(AccAPAccountDetails accAPAccount)
		{
			AssertEquals(nameof(accAPAccount.A1_AccountName), "J", accAPAccount.A1_AccountName);
			AssertEquals(nameof(accAPAccount.A1_BankAccount), "45663", accAPAccount.A1_BankAccount);
			AssertEquals(nameof(accAPAccount.A1_BankAddress1), "1, vaChatswood Branch, Sydney, New", accAPAccount.A1_BankAddress1);
			AssertEquals(nameof(accAPAccount.A1_BankBranchName), "C", accAPAccount.A1_BankBranchName);
			AssertEquals(nameof(accAPAccount.A1_BankBsb), "234434", accAPAccount.A1_BankBsb);
			AssertEquals(nameof(accAPAccount.A1_BankName), "Westpac AU", accAPAccount.A1_BankName);
			AssertEquals(nameof(accAPAccount.A1_BankSwift), "EDRFAU22", accAPAccount.A1_BankSwift);
			AssertEquals(nameof(accAPAccount.A1_RX_NKAccountCurrency), "USD", accAPAccount.A1_RX_NKAccountCurrency);
			AssertEquals(nameof(accAPAccount.A1_RN_NKCountryCode), "AU", accAPAccount.A1_RN_NKCountryCode);
		}

		const string beneficiarySearchResult1 = @"<?xml version='1.0' encoding='utf-8'?>
<BeneficiarySearchResult>
  <Beneficiaries>
    <BeneficiaryDetails>
      <BeneficiaryFullName>J</BeneficiaryFullName>
      <BeneficiaryNickName>J</BeneficiaryNickName>
      <ProviderCode>OFX</ProviderCode>
      <ProviderReference>55fe2c0f-a867-49fe-b9f3-76a33fff5e89</ProviderReference>
      <ProviderClassification>Own Account</ProviderClassification>
      <Currency>USD</Currency>
      <Address>9/11, vaBridge Street, Sydney, New South Wales 2000, AU</Address>
      <Country>AU</Country>
      <EmailAddress />
      <BankName>Westpac AU</BankName>
      <BankBranchName>C</BankBranchName>
      <BankAddress>1, vaChatswood Branch, Sydney, New South Wales 2060, AU</BankAddress>
      <BankAccount>45663</BankAccount>
      <BankAccountSuffix>String</BankAccountSuffix>
      <BankCode />
      <BranchCode>234434</BranchCode>
      <BankSwift>EDRFAU22</BankSwift>
      <PaymentReason>Automated Payment Reason</PaymentReason>
      <BusinessNumber />
    </BeneficiaryDetails>
    <BeneficiaryDetails>
      <BeneficiaryFullName>M</BeneficiaryFullName>
      <BeneficiaryNickName>M</BeneficiaryNickName>
      <ProviderCode>OFX</ProviderCode>
      <ProviderReference>da0b0c15-ae2d-4b7e-84c9-45e3274979c3</ProviderReference>
      <ProviderClassification>High Risk</ProviderClassification>
      <Currency>EUR</Currency>
      <Address>9/10, Pitt Street, Sydney, New South Wales 2000, AU</Address>
      <Country>AU</Country>
      <EmailAddress />
      <BankName>Commonwealth AU</BankName>
      <BankBranchName>Z</BankBranchName>
      <BankAddress>1 Market St, Sydney, New South Wales 2060, AU</BankAddress>
      <BankAccount>111589</BankAccount>
      <BankAccountSuffix>DUM</BankAccountSuffix>
      <BankCode />
      <BranchCode>234434</BranchCode>
      <BankSwift>EDRFAU234</BankSwift>
      <PaymentReason>Sample Payment Reason</PaymentReason>
      <BusinessNumber />
    </BeneficiaryDetails>
  </Beneficiaries>
  <IsComplete>true</IsComplete>
  <StartPageNumber>1</StartPageNumber>
  <EndPageNumber>1</EndPageNumber>
</BeneficiarySearchResult>";

		const string beneficiarySearchResult2 = @"<?xml version='1.0' encoding='utf-8'?>
<BeneficiarySearchResult>
  <Beneficiaries>
    <BeneficiaryDetails>
      <BeneficiaryFullName>J</BeneficiaryFullName>
      <BeneficiaryNickName>J</BeneficiaryNickName>
      <ProviderCode>OFX</ProviderCode>
      <ProviderReference>55fe2c0f-a867-49fe-b9f3-76a33fff5e89</ProviderReference>
      <ProviderClassification>Own Account</ProviderClassification>
      <Currency>USD</Currency>
      <Address>9/11, vaBridge Street, Sydney, New South Wales 2000, AU</Address>
      <Country>AU</Country>
      <EmailAddress />
      <BankName>Westpac AU</BankName>
      <BankBranchName>C</BankBranchName>
      <BankAddress>1, vaChatswood Branch, Sydney, New South Wales 2060, AU</BankAddress>
      <BankAccount>45663</BankAccount>
      <BankAccountSuffix>String</BankAccountSuffix>
      <BankCode />
      <BranchCode>234434</BranchCode>
      <BankSwift>EDRFAU22</BankSwift>
      <PaymentReason>Automated Payment Reason</PaymentReason>
      <BusinessNumber />
    </BeneficiaryDetails>
    <BeneficiaryDetails>
      <BeneficiaryFullName>M</BeneficiaryFullName>
      <BeneficiaryNickName>M</BeneficiaryNickName>
      <ProviderCode>OFX</ProviderCode>
      <ProviderReference>da0b0c15-ae2d-4b7e-84c9-45e3274979c3</ProviderReference>
      <ProviderClassification>High Risk</ProviderClassification>
      <Currency>EUR</Currency>
      <Address>9/10, Pitt Street, Sydney, New South Wales 2000, AU</Address>
      <Country>AU</Country>
      <EmailAddress />
      <BankName>Commonwealth AU</BankName>
      <BankBranchName>Z</BankBranchName>
      <BankAddress>1 Market St, Sydney, New South Wales 2060, AU</BankAddress>
      <BankAccount>111589</BankAccount>
      <BankAccountSuffix>DUM</BankAccountSuffix>
      <BankCode />
      <BranchCode>234434</BranchCode>
      <BankSwift>EDRFAU234</BankSwift>
      <PaymentReason>Sample Payment Reason</PaymentReason>
      <BusinessNumber />
    </BeneficiaryDetails>
  </Beneficiaries>
  <IsComplete>false</IsComplete>
  <StartPageNumber>1</StartPageNumber>
  <EndPageNumber>1</EndPageNumber>
</BeneficiarySearchResult>";

		const string beneficiarySearchResult3 = @"<?xml version='1.0' encoding='utf-8'?>
<BeneficiarySearchResult>
  <IsComplete>true</IsComplete>
  <StartPageNumber>1</StartPageNumber>
  <EndPageNumber>1</EndPageNumber>
</BeneficiarySearchResult>";
	}
}
