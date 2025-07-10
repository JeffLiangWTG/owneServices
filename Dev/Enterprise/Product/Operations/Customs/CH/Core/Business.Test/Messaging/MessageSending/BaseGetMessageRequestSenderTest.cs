using System;
using System.Collections.Generic;
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
using static Enterprise.Customs.CH.Business.MessagingConstants;

namespace Enterprise.Customs.CH.Business.Testing;

abstract class BaseGetMessageRequestSenderTest : TestCaseWithFactory
{
	protected abstract string ApplicationCode { get; }

	protected abstract string ExpectedDestination { get; }

	protected abstract BasePassarCompanyMessageSender GetInstanceForTest(LoggingInformationForTesting loggingInformation);

	public void TestEDIMessageCreated() => CombineAssertions(() =>
	{
		var newTransactionID = CreateCusPollingTransaction(CompanyPollingTransaction.StatusCodes.New);
		var rejTransactionID = CreateCusPollingTransaction(CompanyPollingTransaction.StatusCodes.Rejected);
		Factory.Save();

		ProcessTransactions();

		AssertEDIMessage(newTransactionID);
		AssertEDIMessage(rejTransactionID);

		void AssertEDIMessage(ZString transactionID)
		{
			var ediMessage = LoadEDIMessage(transactionID);
			AssertNotNull("EDIMessage created", ediMessage);
			if (ediMessage != null)
			{
				AssertEquals("EM_ApplicationCode", ApplicationCode, ediMessage.EM_ApplicationCode);
				AssertEquals("EM_MessageType", MessageTypeCodeList.Codes.MSG, ediMessage.EM_MessageType);
				AssertEquals("EM_ReceiveTransmit", ReceiveTransmitList.Codes.Transmit, ediMessage.EM_ReceiveTransmit);
				AssertEquals("EM_Status", EDIMessage.Status.Sent, ediMessage.EM_Status);
				AssertEquals("EM_LinkTable", GlbCompanySchema.Constants.TableName, ediMessage.EM_LinkTable);
				AssertEquals("EM_LinkUniqueID", Company.PK, ediMessage.EM_LinkUniqueID);
				AssertEquals("EM_GP", GlbCompanyWrapper.GetWrapper<GlbCompanyWrapper>(Company).TokenCredentials.PK, ediMessage.EM_GP);

				var expected = new Dictionary<string, string>();
				expected[CustomMsgAttributes.BpId] = CompanyBID;
				expected[CustomMsgAttributes.MessageID] = transactionID;
				JsonTestHelper.AssertEquals("EI_HeaderText", expected, ediMessage.EM_MessageText);
			}
		}
	});

	public void TestEDIInterchangeCreated()
	{
		var transactionID1 = CreateCusPollingTransaction();
		var transactionID2 = CreateCusPollingTransaction();

		Factory.Save();
		ProcessTransactions();

		var ediMessage1 = LoadEDIMessage(transactionID1);
		var ediMessage2 = LoadEDIMessage(transactionID2);
		var ediInterchange1 = LoadEDIInterchange(transactionID1);
		var ediInterchange2 = LoadEDIInterchange(transactionID2);

		AssertNotNull("EDIInterchange 1 created", ediInterchange1);
		AssertNotNull("EDIInterchange 2 created", ediInterchange2);

		CombineAssertions(() =>
		{
			AssertEDIInterchange("EDIInterchange 1", ediInterchange1, ediMessage1);
			AssertEDIInterchange("EDIInterchange 2", ediInterchange2, ediMessage2);
		});

		void AssertEDIInterchange(string assertionMessage, EDIInterchange ediInterchange, EDIMessage ediMessage)
		{
			AssertEquals($"{assertionMessage}: EI_ApplicationCode", ediMessage.EM_ApplicationCode, ediInterchange.EI_ApplicationCode);
			AssertEquals($"{assertionMessage}: EI_InterchangeType", ediMessage.EM_MessageType, ediInterchange.EI_InterchangeType);
			AssertEquals($"{assertionMessage}: EI_ReceiveTransmit", EDIInterchange.Direction.Transmit, ediInterchange.EI_ReceiveTransmit);
			AssertEquals($"{assertionMessage}: EI_From", Company.LicenceKeyIdentifier, ediInterchange.EI_From);
			AssertEquals($"{assertionMessage}: EI_To", ExpectedDestination, ediInterchange.EI_To);
			AssertEquals($"{assertionMessage}: EI_TransportType", EDIInterchange.TransportType.xT, ediInterchange.EI_TransportType);
			AssertEquals($"{assertionMessage}: EI_Status", EDIInterchange.Status.Queued, ediInterchange.EI_Status);
			AssertEquals($"{assertionMessage}: EI_Priority", EDIInterchangePriorityList.Codes.High, ediInterchange.EI_Priority);
			AssertEquals($"{assertionMessage}: EI_InterchangeNum", ediMessage.EM_InterchangeNumber, ediInterchange.EI_InterchangeNum);
			AssertEquals($"{assertionMessage}: EI_IsActive", ZBool.True, ediInterchange.EI_IsActive);
			AssertEquals($"{assertionMessage}: EI_GP", ediMessage.EM_GP, ediInterchange.EI_GP);
			AssertEquals($"{assertionMessage}: EI_BodyText", ZString.Empty, ediInterchange.EI_BodyText);
			AssertNotEquals($"{assertionMessage}: EI_HeaderText", ZString.Empty, ediInterchange.EI_HeaderText);
		}
	}

	public void TestEI_HeaderText()
	{
		var transactionID = CreateCusPollingTransaction();
		Factory.Save();

		ProcessTransactions();

		var ediInterchange = LoadEDIInterchange(transactionID);

		var expected = new Dictionary<string, string>();
		expected[CustomMsgAttributes.BpId] = CompanyBID;
		expected[CustomMsgAttributes.MessageID] = transactionID;
		JsonTestHelper.AssertEquals("EI_HeaderText", expected, ediInterchange.EI_HeaderText);
	}

	[TestDate(2023, 4, 2, 20, 15, 40)]
	[TestUtcOffset(2, 0, 0)]
	public void TestCusPollingTransactionUpdated()
	{
		var transactionID = CreateCusPollingTransaction(numberOfAttempts: 4);
		Factory.Save();

		TestDateAttribute.AddMinutes(4);
		ProcessTransactions();

		var transaction = Factory.LoadTop1<CusPollingTransaction>(new ZQuery(CusPollingTransactionSchema.CPT_TransactionID, transactionID));

		CombineAssertions(() =>
		{
			AssertEquals("CPT_Status", CompanyPollingTransaction.StatusCodes.AwaitingResponse, transaction.CPT_Status);
			AssertEquals("CPT_StatusTimeUtc", new ZDateTime(2023, 4, 2, 20, 19, 40), transaction.CPT_StatusTimeUtc);
			AssertEquals("CPT_NumberOfAttempts", new ZByte(5), transaction.CPT_NumberOfAttempts);
		});
	}

	public void TestCusPollingTransactionQuery() => CombineAssertions(() =>
	{
		var transactionIDWrongStatus = CreateCusPollingTransaction(status: CompanyPollingTransaction.StatusCodes.AwaitingResponse);
		var transactionIDWrongType = CreateCusPollingTransaction(type: "LMI");
		var transactionIDWrongParentTableCode = CreateCusPollingTransaction(parentTypeCode: "BH");
		var transactionIDWrongCompany = CreateCusPollingTransaction(company: OtherCompany);
		var transactionIDOk = CreateCusPollingTransaction();
		Factory.Save();

		ProcessTransactions();

		AssertNull("Status", LoadEDIMessage(transactionIDWrongStatus));
		AssertNull("Type", LoadEDIMessage(transactionIDWrongType));
		AssertNull("ParentTableCode", LoadEDIMessage(transactionIDWrongParentTableCode));
		AssertNull("Company", LoadEDIMessage(transactionIDWrongCompany));
		AssertNotNull("Ok", LoadEDIMessage(transactionIDOk));
	});

	[TestDate(2000, 1, 1, 0, 0, 0)]
	public void TestOldestTransactionProcessedFirst()
	{
		var transactionID1 = CreateCusPollingTransaction();
		Factory.Save();
		TestDateAttribute.AddMinutes(1);
		var transactionID2 = CreateCusPollingTransaction();
		Factory.Save();

		ProcessTransactions();

		var ediMessage1 = LoadEDIInterchange(transactionID1);
		var ediMessage2 = LoadEDIInterchange(transactionID2);
		AssertLessThan(ediMessage1.EI_InterchangeNum, ediMessage2.EI_InterchangeNum);
	}

	public void TestMoreThanMaxMessagesPerSaveMessages()
	{
		var transactionIDs = new List<string>();

		for (var i = 0; i < BaseGetMessageRequestSender.MaxMessagesPerSave + 1; i++)
		{
			transactionIDs.Add(CreateCusPollingTransaction());
		}
		Factory.Save();

		ProcessTransactions();
		foreach (var transactionID in transactionIDs)
		{
			AssertNotNull(LoadEDIInterchange(transactionID));
		}
	}

	LoggingInformationForTesting ProcessTransactions()
	{
		var loggingInformation = new LoggingInformationForTesting();
		using (DisposableEnvironment.ForCompany(Company.GC_Code))
		{
			GetInstanceForTest(loggingInformation).Send();
		}
		return loggingInformation;
	}

	EDIMessage LoadEDIMessage(string transactionID)
	{
		return Factory.LoadTop1<EDIMessage>(new ZQuery(EDIMessageSchema.EM_ApplicationReference, transactionID));
	}

	EDIInterchange LoadEDIInterchange(string transactionID)
	{
		return Factory.Load<EDIInterchange>(LoadEDIMessage(transactionID)?.EM_EI ?? ZGuid.Empty);
	}

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

	ZString CreateCusPollingTransaction(string status = CompanyPollingTransaction.StatusCodes.New, string applicationCode = null, string type = CompanyPollingTransaction.TransactionTypes.MessageId, string parentTypeCode = GlbCompanySchema.Constants.Prefix, GlbCompany company = null, int numberOfAttempts = 1)
	{
		var transaction = Factory.New<CusPollingTransaction>();
		transaction.CPT_ApplicationCode = applicationCode ?? ApplicationCode;
		transaction.CPT_Type = type;
		transaction.CPT_ParentTableCode = parentTypeCode;
		transaction.CPT_ParentID = company == null ? Company.PK : company.PK;
		transaction.CPT_Status = status;
		transaction.CPT_TransactionID = Guid.NewGuid().ToString();
		transaction.CPT_NumberOfAttempts = new ZByte((byte)numberOfAttempts);

		Factory.Save();

		return transaction.CPT_TransactionID;
	}

	const string CompanyBID = "C234567890";
	const string AccessToken = "the-access-token";
}
