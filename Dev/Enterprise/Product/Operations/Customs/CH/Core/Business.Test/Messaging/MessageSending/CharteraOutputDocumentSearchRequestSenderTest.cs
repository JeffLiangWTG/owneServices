using System;
using System.Runtime.CompilerServices;
using CargoWise.Customs.CH.MessageDefinitions.Chartera.documentsearchrequest_v1;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Customs.Business.CusPollingTransaction;
using static Enterprise.Customs.CH.Business.CompanyPollingTransaction;
using static Enterprise.Customs.CH.Business.MessagingConstants;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(CharteraOutputDocumentSearchRequestSender))]
sealed class CharteraOutputDocumentSearchRequestSenderTest : TestCaseWithFactory
{
	[TestDate(2020, 8, 15, 12, 0, 0)]
	[TestUtcOffset(3, 0, 0)]
	public void TestNoPendingTransactions() => CombineAssertions(() =>
	{
		_ = Company;
		Factory.Save();

		var (transaction, message, interchange, request) = SendRequestAndGetResult();
		AssertEquals($"From (UtcNow={ZDateTime.UtcNow})", ZDateTime.UtcNow.AddHours(-1).ToDateTime(), request.CurrentQuery.From);
		AssertEquals($"To (UtcNow={ZDateTime.UtcNow})", ZDateTime.UtcNow.ToDateTime(), request.CurrentQuery.To);
		AssertTransaction(transaction, request);
		AssertEDIMessage(message, transaction);
		AssertInterchange(interchange, message);
	});

	[TestDate(2020, 8, 15, 12, 0, 0)]
	[TestUtcOffset(3, 0, 0)]
	public void TestMaxEarliestAttemptLessThan5Minutes() => CombineAssertions(() =>
	{
		var testHelper = new CustomsMessageProcessorTestHelper(Factory);
		testHelper.AddPollingTransaction(Company, ApplicationCodes.CHCustomsCharteraOutput, TransactionTypes.DocumentSearchRequest, StatusCodes.AwaitingResponse, earliestTimeOfNextAttemptUtc: new DateTime(2020, 8, 15, 11, 58, 0));
		Factory.Save();

		var (transaction, message, interchange, request) = SendRequestAndGetResult();
		AssertNull("CusPollingTransaction", transaction);
		AssertNull("EDIMessage", message);
	});

	[TestDate(2020, 8, 15, 12, 0, 0)]
	[TestUtcOffset(3, 0, 0)]
	public void TestMaxEarliestAttemptBetween5And60Minutes() => CombineAssertions(() =>
	{
		var testHelper = new CustomsMessageProcessorTestHelper(Factory);
		testHelper.AddPollingTransaction(Company, ApplicationCodes.CHCustomsCharteraOutput, TransactionTypes.DocumentSearchRequest, StatusCodes.AwaitingResponse, earliestTimeOfNextAttemptUtc: new DateTime(2020, 8, 15, 10, 54, 00));
		testHelper.AddPollingTransaction(Company, ApplicationCodes.CHCustomsCharteraOutput, TransactionTypes.DocumentSearchRequest, StatusCodes.AwaitingResponse, earliestTimeOfNextAttemptUtc: new DateTime(2020, 8, 15, 11, 54, 00));
		Factory.Save();

		var (transaction, message, interchange, request) = SendRequestAndGetResult();
		AssertEquals($"From (UtcNow={ZDateTime.UtcNow})", new DateTime(2020, 8, 15, 11, 54, 00), request.CurrentQuery.From);
		AssertEquals($"To (UtcNow={ZDateTime.UtcNow})", ZDateTime.UtcNow.ToDateTime(), request.CurrentQuery.To);
		AssertTransaction(transaction, request);
		AssertEDIMessage(message, transaction);
		AssertInterchange(interchange, message);
	});

	[TestDate(2020, 8, 15, 12, 0, 0)]
	[TestUtcOffset(3, 0, 0)]
	public void TestMaxEarliestAttemptMoreThan60Minutes() => CombineAssertions(() =>
	{
		var testHelper = new CustomsMessageProcessorTestHelper(Factory);
		testHelper.AddPollingTransaction(Company, ApplicationCodes.CHCustomsCharteraOutput, TransactionTypes.DocumentSearchRequest, StatusCodes.AwaitingResponse, earliestTimeOfNextAttemptUtc: new DateTime(2020, 8, 15, 10, 54, 00));
		testHelper.AddPollingTransaction(Company, ApplicationCodes.CHCustomsCharteraOutput, TransactionTypes.DocumentSearchRequest, StatusCodes.AwaitingResponse, earliestTimeOfNextAttemptUtc: new DateTime(2020, 8, 15, 10, 59, 00));
		Factory.Save();

		var (transaction, message, interchange, request) = SendRequestAndGetResult();
		AssertEquals($"From (UtcNow={ZDateTime.UtcNow})", new DateTime(2020, 8, 15, 10, 59, 00), request.CurrentQuery.From);
		AssertEquals($"To (UtcNow={ZDateTime.UtcNow})", new DateTime(2020, 8, 15, 11, 59, 00), request.CurrentQuery.To);
		AssertTransaction(transaction, request);
		AssertEDIMessage(message, transaction);
		AssertInterchange(interchange, message);
	});

	[TestDate(2020, 8, 15, 12, 0, 0)]
	[TestUtcOffset(3, 0, 0)]
	public void TestEM_MessageNum()
	{
		var testHelper = new CustomsMessageProcessorTestHelper(Factory);
		testHelper.AddPollingTransaction(Company, ApplicationCodes.CHCustomsCharteraOutput, TransactionTypes.DocumentSearchRequest, StatusCodes.AwaitingResponse, earliestTimeOfNextAttemptUtc: new DateTime(2020, 8, 15, 11, 0, 0));
		Factory.Save();

		var (_, message1, _, _) = SendRequestAndGetResult();

		TestDateAttribute.AddHours(2);
		var (_, message2, _, _) = SendRequestAndGetResult();

		AssertNotEquals(message1.EM_MessageNum, message2.EM_MessageNum);
	}

	[TestDate(2020, 1, 1, 20, 0, 0, 0)]
	public void TestTimeLimit() => CombineAssertions(() =>
	{
		var testHelper = new CustomsMessageProcessorTestHelper(Factory);
		var transaction = testHelper.AddPollingTransaction(Company, ApplicationCodes.CHCustomsCharteraOutput, TransactionTypes.DocumentSearchRequest, StatusCodes.AwaitingResponse);

		var config = new PassarSearchRequestConfig();
		config.TimeLimit = 5;
		using (CHCustomsDataRegistry.Instance.PassarSearchRequestConfig.SetTemporaryValue(Company.PK.ToGuid(), Guid.Empty, Guid.Empty, config))
		{
			AssertSender(7, ZDateTime.UtcNow.AddHours(-4).AddMinutes(SenderDelay));
			AssertSender(6, ZDateTime.UtcNow.AddHours(-4).AddMinutes(SenderDelay));
			AssertSender(5, ZDateTime.UtcNow.AddHours(-4).AddMinutes(SenderDelay));
			AssertSender(4, ZDateTime.UtcNow.AddHours(-3));
			AssertSender(3, ZDateTime.UtcNow.AddHours(-2));
		}

		void AssertSender(int ageOfLatestMessage, ZDateTime expectedEarliestTimeOfNextAttemptUtc, [CallerLineNumber] int line = 0)
		{
			transaction.CPT_EarliestTimeOfNextAttemptUtc = ZDateTime.UtcNow.AddHours(-ageOfLatestMessage);
			Factory.Save();
			var (newTransaction, message, _, _) = SendRequestAndGetResult();
			AssertEquals($"[{line}] ageOfLatestMessage={ageOfLatestMessage}h UtcNow={ZDateTime.UtcNow} - CPT_EarliestTimeOfNextAttemptUtc:", expectedEarliestTimeOfNextAttemptUtc, newTransaction.CPT_EarliestTimeOfNextAttemptUtc);
			newTransaction.Delete();
		}
	});

	(CusPollingTransaction transaction, CHEDIMessage message, CHEDIInterchange interchange, DocumentSearchRequest request) SendRequestAndGetResult()
	{
		TestDateAttribute.AddMinutes(SenderDelay);
		using (DisposableEnvironment.ForCompany(Company.GC_Code))
		{
			new CharteraOutputDocumentSearchRequestSender(new LoggingInformationForTesting()).Send();
		}
		var transaction = Factory.LoadTop1<CusPollingTransaction>(new ZQuery(CusPollingTransactionSchema.CPT_SystemCreateTimeUtc, SQLComparisonOperator.GreaterThanOrEqualTo, ZDateTime.UtcNow));
		var message = Factory.LoadTop1<CHEDIMessage>(new ZQuery(EDIMessageSchema.EM_SystemCreateTimeUtc, SQLComparisonOperator.GreaterThanOrEqualTo, ZDateTime.UtcNow));
		var interchange = Factory.LoadTop1<CHEDIInterchange>(new ZQuery(EDIInterchangeSchema.EI_SystemCreateTimeUtc, SQLComparisonOperator.GreaterThanOrEqualTo, ZDateTime.UtcNow));
		var request = message == null ? null : XmlObjectSerializer.Deserialize<DocumentSearchRequest>(message.EM_MessageText);
		return (transaction, message, interchange, request);
	}

	const int SenderDelay = 1;

	void AssertTransaction(CusPollingTransaction transaction, DocumentSearchRequest request)
	{
		AssertEquals("CPT_ApplicationCode", ApplicationCodes.CHCustomsCharteraOutput, transaction.CPT_ApplicationCode);
		AssertEquals("CPT_Type", TransactionTypes.DocumentSearchRequest, transaction.CPT_Type);
		AssertEquals("CPT_TransactionID", request.CurrentQuery.ProcessId, transaction.CPT_TransactionID);
		AssertEquals("CPT_ParentTableCode", GlbCompanySchema.Constants.Prefix, transaction.CPT_ParentTableCode);
		AssertEquals("CPT_ParentID", Company.PK, transaction.CPT_ParentID);
		AssertEquals("CPT_Status", StatusCodes.AwaitingResponse, transaction.CPT_Status);
		AssertEquals("CPT_StatusTimeUtc", ZDateTime.UtcNow, transaction.CPT_StatusTimeUtc);
		AssertEquals("CPT_NumberOfAttempts", new ZByte(1), transaction.CPT_NumberOfAttempts);
		AssertEquals("CPT_EarliestTimeOfNextAttemptUtc", request.CurrentQuery.To, transaction.CPT_EarliestTimeOfNextAttemptUtc);
	}

	void AssertEDIMessage(CHEDIMessage ediMessage, CusPollingTransaction transaction)
	{
		AssertEquals("EM_ApplicationCode", ApplicationCodes.CHCustomsCharteraOutput, ediMessage.EM_ApplicationCode);
		AssertEquals("EM_MessageType", MessageTypeCodeList.Codes.REQ, ediMessage.EM_MessageType);
		AssertEquals("EM_MessageSubType", MessageSubTypeCodeList.Codes.CharteraOutputDocumentSearchRequest, ediMessage.EM_MessageSubType);
		AssertEquals("EM_ReceiveTransmit", ReceiveTransmitList.Codes.Transmit, ediMessage.EM_ReceiveTransmit);
		AssertNotEquals("EM_MessageNum", ZString.Empty, ediMessage.EM_MessageNum);
		AssertEquals("EM_Status", EDIMessageStatusList.Codes.Sent, ediMessage.EM_Status);
		AssertEquals("EM_LinkTable", CusPollingTransactionSchema.Constants.TableName, ediMessage.EM_LinkTable);
		AssertEquals("EM_LinkUniqueID", transaction.PK, ediMessage.EM_LinkUniqueID);
		AssertEquals("EM_GP", GlbCompanyWrapper.GetWrapper<GlbCompanyWrapper>(Company).TokenCredentials.PK, ediMessage.EM_GP);
		AssertEquals("EM_ApplicationReference", transaction.CPT_TransactionID, ediMessage.EM_ApplicationReference);
	}

	void AssertInterchange(CHEDIInterchange interchange, CHEDIMessage message)
	{
		AssertEquals("EI_ApplicationCode", ApplicationCodes.CHCustomsCharteraOutput, interchange.EI_ApplicationCode);
		AssertEquals("EI_InterchangeType", MessageTypeCodeList.Codes.REQ, interchange.EI_InterchangeType);
		AssertEquals("EI_IsActive", true, interchange.EI_IsActive);
		AssertEquals("EI_BodyText", message.EM_MessageText, interchange.EI_BodyText);
		AssertEquals("EI_ReceiveTransmit", EDIInterchange.Direction.Transmit, interchange.EI_ReceiveTransmit);
		AssertEquals("EI_Status", EDIInterchange.Status.Queued, interchange.EI_Status);
		AssertEquals("EI_To", CustomsDestinationCodes.CustomsCharteraOutput, interchange.EI_To);
	}

	GlbCompany Company => company ??= MessageProcessorTestHelper.CreateCompany(Factory, tokenCredential: true);
	GlbCompany company;
}
