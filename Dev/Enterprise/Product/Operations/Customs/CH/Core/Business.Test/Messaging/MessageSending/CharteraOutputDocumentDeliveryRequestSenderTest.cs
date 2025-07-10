using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.Customs.CH.MessageDefinitions.Chartera.documentdeliveryrequest_v1;
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
using static Enterprise.Customs.CH.Business.CompanyPollingTransaction;
using static Enterprise.Customs.CH.Business.MessagingConstants;
using static Enterprise.Messaging.Business.EDIInterchange;

namespace Enterprise.Customs.CH.Business.Testing;

class CharteraOutputDocumentDeliveryRequestSenderTest : TestCaseWithFactory
{
	public void TestMessage() => CombineAssertions(() =>
	{
		var transactionId1 = CreateCusPollingTransaction(sequenceNumber: 1);
		var transactionId2 = CreateCusPollingTransaction(sequenceNumber: 2);
		Factory.Save();

		AssertPreConditionNoEdiMessages();
		ProcessTransactions();
		var ediMessage = LoadEDIMessage();

		AssertEquals("EM_ApplicationCode", EDIInterchange.ApplicationCodes.CHCustomsCharteraOutput, ediMessage.EM_ApplicationCode);
		AssertEquals("EM_MessageType", MessageTypeCodeList.Codes.REQ, ediMessage.EM_MessageType);
		AssertEquals("EM_MessageSubType", MessageSubTypeCodeList.Codes.CharteraOutputDocumentDeliveryRequest, ediMessage.EM_MessageSubType);
		AssertEquals("EM_ReceiveTransmit", ReceiveTransmitList.Codes.Transmit, ediMessage.EM_ReceiveTransmit);
		AssertNotEquals("EM_MessageNum", ZString.Empty, ediMessage.EM_MessageNum);
		AssertEquals("EM_Status", EDIMessage.Status.Sent, ediMessage.EM_Status);
		AssertEquals("EM_LinkTable", GlbCompanySchema.Constants.TableName, ediMessage.EM_LinkTable);
		AssertEquals("EM_LinkUniqueID", Company.PK, ediMessage.EM_LinkUniqueID);
		AssertEquals("EM_GP", GlbCompanyWrapper.GetWrapper<GlbCompanyWrapper>(Company).TokenCredentials.PK, ediMessage.EM_GP);
		AssertNotEquals("EM_ApplicationReference", ZString.Empty, ediMessage.EM_ApplicationReference);

		var xml = XmlObjectSerializer.Deserialize<DocumentDeliveryRequest>(ediMessage.EM_MessageText);
		AssertEquals("EM_MessageText: ProcessId", ediMessage.EM_ApplicationReference, xml.ProcessId);
		AssertEquals("EM_MessageText: DocumentIdsCount", 2, xml.DocumentIds.Count);
		AssertEquals("EM_MessageText: DocumentIds[0]", transactionId1, xml.DocumentIds[0]);
		AssertEquals("EM_MessageText: DocumentIds[1]", transactionId2, xml.DocumentIds[1]);

		var ediInterchange = ediMessage.Interchange;
		AssertEquals("EI_ApplicationCode", EDIInterchange.ApplicationCodes.CHCustomsCharteraOutput, ediInterchange.EI_ApplicationCode);
		AssertEquals("EI_InterchangeType", MessageTypeCodeList.Codes.REQ, ediInterchange.EI_InterchangeType);
		AssertEquals("EI_ReceiveTransmit", ReceiveTransmitList.Codes.Transmit, ediInterchange.EI_ReceiveTransmit);
		AssertEquals("EI_From", Company.LicenceKeyIdentifier, ediInterchange.EI_From);
		AssertEquals("EI_To", CustomsDestinationCodes.CustomsCharteraOutput, ediInterchange.EI_To);
		AssertEquals("EI_TransportType", TransportType.xT, ediInterchange.EI_TransportType);
		AssertEquals("EI_Status", EDIInterchange.Status.Queued, ediInterchange.EI_Status);
		AssertNotEquals("EI_InterchangeNum", ZString.Empty, ediInterchange.EI_InterchangeNum);
		AssertNotEquals("EI_SessionGUID", ZGuid.Empty, ediInterchange.EI_SessionGUID);
		AssertEquals("EI_IsActive", ZBool.True, ediInterchange.EI_IsActive);
		AssertEquals("EI_GP", ediInterchange.EI_GP, ediInterchange.EI_GP);
		AssertEquals("EI_BodyText", ediMessage.EM_MessageText, ediInterchange.EI_BodyText);

		var match = Regex.Match(ediInterchange.EI_HeaderText, $@"""{Regex.Escape(CustomMsgAttributes.MessageID)}"":""([^""]*)""");
		var messageId = match.Success ? match.Groups[1].Value : null;
		AssertNotNullOrEmpty("EI_HeaderText: messageId", messageId);
		var expected = new Dictionary<string, string>();
		expected[CustomMsgAttributes.BpId] = CompanyBID;
		expected[CustomMsgAttributes.MessageType] = "DocumentDeliveryRequest";
		expected[CustomMsgAttributes.MessageID] = messageId;
		JsonTestHelper.AssertEquals("EI_HeaderText", expected, ediInterchange.EI_HeaderText);

		CreateCusPollingTransaction();
		Factory.Save();
		ProcessTransactions();
		var ediMessage2 = LoadEDIMessage(new ZQuery(EDIMessageSchema.PK, SQLComparisonOperator.NotEqual, ediMessage.PK));
		AssertNotEquals("Unique EM_MessageNum", ediMessage.EM_MessageNum, ediMessage2.EM_MessageNum);
		var ediInterchange2 = ediMessage2.Interchange;
		AssertNotEquals("Unique EI_InterchangeNum", ediInterchange.EI_InterchangeNum, ediInterchange2.EI_InterchangeNum);
		AssertNotEquals("Unique EI_SessionGUID", ediInterchange.EI_SessionGUID, ediInterchange2.EI_SessionGUID);
	});

	[TestDate(2023, 10, 1, 12, 30, 0)]
	[TestUtcOffset(2, 0, 0)]
	public void TestCusPollingTransactionUpdated() => CombineAssertions(() =>
	{
		var transactionId = CreateCusPollingTransaction(numberOfAttempts: 1);
		Factory.Save();

		TestDateAttribute.AddMinutes(1);
		ProcessTransactions();

		var transaction = LoadTransaction(transactionId);
		AssertEquals("CPT_Status", StatusCodes.AwaitingResponse, transaction.CPT_Status);
		AssertEquals("CPT_StatusTimeUtc", ZDateTime.UtcNow, transaction.CPT_StatusTimeUtc);
		AssertEquals("CPT_NumberOfAttempts", new ZByte(2), transaction.CPT_NumberOfAttempts);
	});

	[TestDate(2023, 10, 1, 12, 30, 0)]
	[TestUtcOffset(2, 0, 0)]
	public void TestCusPollingTransactionUpdated_maxRetriesExceeded() => CombineAssertions(() =>
	{
		var transactionId = CreateCusPollingTransaction(numberOfAttempts: 3);
		Factory.Save();

		using (CHCustomsDataRegistry.Instance.MaxNumberOfDownloadAttempts.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 3))
		{
			ProcessTransactions();
		}

		var transaction = LoadTransaction(transactionId);
		AssertEquals("CPT_Status", StatusCodes.Skip, transaction.CPT_Status);
		AssertEquals("CPT_StatusTimeUtc", ZDateTime.UtcNow, transaction.CPT_StatusTimeUtc);
		AssertEquals("CPT_NumberOfAttempts", new ZByte(3), transaction.CPT_NumberOfAttempts);
	});

	[TestDate(2023, 10, 1, 12, 30, 0)]
	public void TestCusPollingTransactionUpdated_maxByteValueReached() => CombineAssertions(() =>
	{
		using (CHCustomsDataRegistry.Instance.MaxNumberOfDownloadAttempts.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 255))
		{
			var transactionId = CreateCusPollingTransaction(numberOfAttempts: 253);
			Factory.Save();

			ProcessTransactions();

			var transaction = LoadTransaction(transactionId);
			AssertEquals("3rd last run - CPT_Status", StatusCodes.AwaitingResponse, transaction.CPT_Status);
			AssertEquals("3rd last run -  - CPT_NumberOfAttempts", new ZByte(254), transaction.CPT_NumberOfAttempts);

			TestDateAttribute.AddHours(1);
			ProcessTransactions();

			Factory.ReloadAll<CusPollingTransaction>();
			AssertEquals("2nd last run - CPT_Status", StatusCodes.AwaitingResponse, transaction.CPT_Status);
			AssertEquals("2nd last run - CPT_NumberOfAttempts", new ZByte(255), transaction.CPT_NumberOfAttempts);

			TestDateAttribute.AddHours(1);
			ProcessTransactions();

			Factory.ReloadAll<CusPollingTransaction>();
			AssertEquals("Last run - CPT_Status", StatusCodes.Skip, transaction.CPT_Status);
			AssertEquals("Last run - CPT_NumberOfAttempts", new ZByte(255), transaction.CPT_NumberOfAttempts);
		}
	});

	[TestDate(2023, 10, 1, 12, 30, 0)]
	[TestUtcOffset(2, 0, 0)]
	public void TestCusPollingTransactionUpdated_nextAttemptUTC() => CombineAssertions(() =>
	{
		var transactionId = CreateCusPollingTransaction();
		Factory.Save();

		using (CHCustomsDataRegistry.Instance.MaxNumberOfDownloadAttempts.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 99))
		{
			ProcessTransactions();
			var transaction = LoadTransaction(transactionId);
			AssertEquals("CPT_EarliestTimeOfNextAttemptUtc (1)", ZDateTime.UtcNow.AddMinutes(5), transaction.CPT_EarliestTimeOfNextAttemptUtc);

			TestDateAttribute.AddMinutes(6);
			ProcessTransactions();
			transaction = LoadTransaction(transactionId);
			AssertEquals("CPT_EarliestTimeOfNextAttemptUtc (2)", ZDateTime.UtcNow.AddMinutes(10), transaction.CPT_EarliestTimeOfNextAttemptUtc);

			TestDateAttribute.AddMinutes(11);
			ProcessTransactions();
			transaction = LoadTransaction(transactionId);
			AssertEquals("CPT_EarliestTimeOfNextAttemptUtc (3)", ZDateTime.UtcNow.AddMinutes(30), transaction.CPT_EarliestTimeOfNextAttemptUtc);

			TestDateAttribute.AddMinutes(31);
			ProcessTransactions();
			transaction = LoadTransaction(transactionId);
			AssertEquals("CPT_EarliestTimeOfNextAttemptUtc (4)", ZDateTime.UtcNow.AddMinutes(60), transaction.CPT_EarliestTimeOfNextAttemptUtc);

			TestDateAttribute.AddMinutes(61);
			ProcessTransactions();
			transaction = LoadTransaction(transactionId);
			AssertEquals("CPT_EarliestTimeOfNextAttemptUtc (5)", ZDateTime.UtcNow.AddMinutes(60), transaction.CPT_EarliestTimeOfNextAttemptUtc);
		}
	});

	[TestDate(2023, 10, 1, 12, 30, 0)]
	[TestUtcOffset(2, 0, 0)]
	public void TestCusPollingTransactionQuery() => CombineAssertions(() =>
	{
		DeleteAllTransactions();
		CreateCusPollingTransaction(applicationCode: ApplicationCodeList.Codes.CHCustomsPassar, type: CompanyPollingTransaction.TransactionTypes.MessageId);
		AssertEquals("Wrong applicationCode (or type)", 0, ProcessTransactionsAndCount());

		DeleteAllTransactions();
		CreateCusPollingTransaction(type: CompanyPollingTransaction.TransactionTypes.MessageId);
		AssertEquals("Wrong type", 0, ProcessTransactionsAndCount());

		DeleteAllTransactions();
		CreateCusPollingTransaction(parentTypeCode: CusInBondHeaderSchema.Constants.Prefix);
		AssertEquals("Wrong parent type", 0, ProcessTransactionsAndCount());

		DeleteAllTransactions();
		CreateCusPollingTransaction(company: OtherCompany);
		AssertEquals("Wrong company", 0, ProcessTransactionsAndCount());

		DeleteAllTransactions();
		CreateCusPollingTransaction(status: StatusCodes.Skip);
		AssertEquals("Wrong status", 0, ProcessTransactionsAndCount());

		DeleteAllTransactions();
		CreateCusPollingTransaction(earliestTimeOfNextAttemptUtc: new ZDateTime(2024, 10, 1, 10, 31, 0));
		AssertEquals($"too early", 0, ProcessTransactionsAndCount());

		DeleteAllTransactions();
		CreateCusPollingTransaction(earliestTimeOfNextAttemptUtc: new ZDateTime(2022, 10, 1, 10, 29, 0));
		AssertEquals($"not too early", 1, ProcessTransactionsAndCount());

		DeleteAllTransactions();
		CreateCusPollingTransaction();
		AssertEquals("valid transaction", 1, ProcessTransactionsAndCount());

		int ProcessTransactionsAndCount()
		{
			Factory.Save();
			var count = CountEDIMessages();
			ProcessTransactions();
			return CountEDIMessages() - count;
		}
	});

	[TestDate(2000, 1, 1, 0, 0, 0)]
	public void TestCusPollingTransactionQuery_Order() => CombineAssertions(() =>
	{
		var transactionId1 = CreateCusPollingTransaction(sequenceNumber: 2);
		Factory.Save();
		TestDateAttribute.AddMinutes(1);
		var transactionId2 = CreateCusPollingTransaction(sequenceNumber: 1);
		Factory.Save();

		AssertPreConditionNoEdiMessages();
		ProcessTransactions();
		var ediMessage = LoadEDIMessage();

		var xml = XmlObjectSerializer.Deserialize<DocumentDeliveryRequest>(ediMessage.EM_MessageText);
		AssertEquals("DocumentIds[0]", transactionId1, xml.DocumentIds[0]);
		AssertEquals("DocumentIds[1]", transactionId2, xml.DocumentIds[1]);
	});

	public void TestCusPollingTransactionQuery_Limit() => CombineAssertions(() =>
	{
		using (CHCustomsDataRegistry.Instance.MaxNumberOfSimultaneousDownloads.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 3))
		{
			CreateCusPollingTransaction();
			CreateCusPollingTransaction();
			CreateCusPollingTransaction();
			CreateCusPollingTransaction();
			CreateCusPollingTransaction();
			Factory.Save();

			AssertPreConditionNoEdiMessages();
			ProcessTransactions();

			var ediMessage = LoadEDIMessage();
			var xml = XmlObjectSerializer.Deserialize<DocumentDeliveryRequest>(ediMessage.EM_MessageText);
			AssertEquals("DocumentIdsCount", 3, xml.DocumentIds.Count);
			AssertEquals(2, Factory.Load<CusPollingTransaction>(new ZQuery(CusPollingTransactionSchema.CPT_Status, StatusCodes.New)).Length);
		}
	});

	public void TestLogMessage() => CombineAssertions(() =>
	{
		_ = Company;
		Factory.Save();

		var logger = ProcessTransactions();
		AssertEquals("No transactions", $"0 {FriendlyName}(s) for company {Company.GC_Code} has been processed.", logger.Logs.Last().ToString());

		CreateCusPollingTransaction();
		CreateCusPollingTransaction();
		Factory.Save();
		logger = ProcessTransactions();
		AssertEquals("Transactions processed", $"1 {FriendlyName}(s) for company {Company.GC_Code} has been processed.", logger.Logs.Last().ToString());
	});

	LoggingInformationForTesting ProcessTransactions()
	{
		var loggingInformation = new LoggingInformationForTesting();
		using (DisposableEnvironment.ForCompany(Company.GC_Code))
		{
			new CharteraOutputDocumentDeliveryRequestSender(loggingInformation).Send();
		}
		return loggingInformation;
	}

	void AssertPreConditionNoEdiMessages()
	{
		AssertEquals("Pre-condition: No EDIMessages", 0, CountEDIMessages());
	}

	CusPollingTransaction LoadTransaction(ZString transactionId)
	{
		return Factory.LoadTop1<CusPollingTransaction>(new ZQuery(CusPollingTransactionSchema.CPT_TransactionID, transactionId));
	}

	EDIMessage LoadEDIMessage(ZQuery query = null)
	{
		query = query ?? new ZQuery();
		query.AddToFilter(EDIMessageSchema.EM_LinkUniqueID, Company.PK);
		return Factory.LoadTop1<EDIMessage>(query);
	}

	void DeleteAllTransactions()
	{
		foreach (var transaction in Factory.Load<CusPollingTransaction>(new ZQuery(CusPollingTransactionSchema.CPT_ParentID, Company.PK)))
		{
			transaction.Delete();
		}
		Factory.Save();
	}

	int CountEDIMessages() => Factory.Load<EDIMessage>(new ZQuery(EDIMessageSchema.EM_LinkUniqueID, Company.PK)).Length;

	GlbCompany Company => company ?? (company = CreateCompany("C01", CompanyBID));
	GlbCompany company;

	GlbCompany OtherCompany => otherCompany ?? (otherCompany = CreateCompany("C02"));
	GlbCompany otherCompany;

	GlbCompany CreateCompany(string code, string bid = null)
	{
		var company = Factory.New<GlbCompany>();
		company.GC_Code = code;
		company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Switzerland;
		company.GC_IsActive = ZBool.True;
		company.GC_OH_OrgProxy = CreateOrgHeader(code, bid).PK;
		var companyWrapper = GlbCompanyWrapper.GetWrapper<GlbCompanyWrapper>(company);
		companyWrapper.TokenCredentialsEnabled = true;
		companyWrapper.TokenCredentials.GP_CertificateText = AccessToken;

		var branch = company.Branches.AddNew();
		branch.GB_Code = code;
		branch.GB_IsActive = ZBool.True;

		return company;
	}

	OrgHeader CreateOrgHeader(string code, string bid)
	{
		var orgHeader = Factory.New<OrgHeader>();
		orgHeader.OH_Code = code;
		if (bid != null)
		{
			orgHeader.CustomsCodes.AddNew(OrgCusCode.SwissCodeTypes.BID, bid);
		}
		return orgHeader;
	}

	ZString CreateCusPollingTransaction(string status = CompanyPollingTransaction.StatusCodes.New, string applicationCode = CusPollingTransaction.ApplicationCodes.CHCustomsCharteraOutput, string type = CompanyPollingTransaction.TransactionTypes.DocumentDelivery, string parentTypeCode = GlbCompanySchema.Constants.Prefix, GlbCompany company = null, int numberOfAttempts = 0, int sequenceNumber = 0, ZDateTime? earliestTimeOfNextAttemptUtc = null)
	{
		var transaction = Factory.New<CusPollingTransaction>();
		transaction.CPT_ApplicationCode = applicationCode;
		transaction.CPT_Type = type;
		transaction.CPT_ParentTableCode = parentTypeCode;
		transaction.CPT_ParentID = company?.PK ?? Company.PK;
		transaction.CPT_Status = status;
		transaction.CPT_TransactionID = Guid.NewGuid().ToString();
		transaction.CPT_NumberOfAttempts = new ZByte((byte)numberOfAttempts);
		transaction.CPT_SequenceNumber = new ZShort((short)sequenceNumber);
		transaction.CPT_EarliestTimeOfNextAttemptUtc = earliestTimeOfNextAttemptUtc ?? ZDateTime.Empty;
		return transaction.CPT_TransactionID;
	}

	const string CompanyBID = "C234567890";
	const string AccessToken = "the-access-token";
	const string FriendlyName = "Chartera Output Document Delivery Request";
}
