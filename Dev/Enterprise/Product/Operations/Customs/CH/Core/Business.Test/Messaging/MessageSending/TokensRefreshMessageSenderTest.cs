using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Messaging.Business.EDIMessage;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(TokensRefreshMessageSender))]
sealed class TokensRefreshMessageSenderTest : TestCaseWithFactory
{
	public void TestConstructor() => AssertExceptionThrown<ArgumentNullException>(() => new TokensRefreshMessageSender(null));

	[TestDate(2024, 3, 8)]
	public void TestRefreshTokens()
	{
		var ediMessages = Factory.Load<CHEDIMessage>(new ZQuery(EDIMessageSchema.EM_LinkUniqueID, company.PK));
		AssertEquals("Precondition: No EDIMessages expected at this stage", 0, ediMessages.Length);

		token.GP_ExpiryDate = ZDateTime.UtcNow.AddHours(2);
		Factory.Save();

		RunMessageSender();

		ediMessages = Factory.Load<CHEDIMessage>(new ZQuery(EDIMessageSchema.EM_LinkUniqueID, company.PK));
		AssertEquals("No EDIMessages generated, token not expired", 0, ediMessages.Length);

		token.GP_ExpiryDate = ZDateTime.UtcNow.AddHours(1);
		Factory.Save();

		RunMessageSender();

		ediMessages = Factory.Load<CHEDIMessage>(new ZQuery(EDIMessageSchema.EM_LinkUniqueID, company.PK));
		AssertEquals("One EDIMessage for refresh token", 1, ediMessages.Length);

		AssertEDIMessage(ediMessages[0]);

		ediMessages[0].EM_Status = EDIMessage.Status.Sent;
		Factory.Save();

		RunMessageSender();

		ediMessages = Factory.Load<CHEDIMessage>(new ZQuery(EDIMessageSchema.EM_LinkUniqueID, company.PK));
		AssertEquals("No EDIMessages generated, status of token is QUE", 1, ediMessages.Length);

		TestDateAttribute.AddMinutes(11);

		RunMessageSender();

		ediMessages = Factory.Load<CHEDIMessage>(new ZQuery(EDIMessageSchema.EM_LinkUniqueID, company.PK).AddToFilter(EDIMessageSchema.PK, SQLComparisonOperator.NotEqual, ediMessages[0].PK));
		AssertEquals("A new EDIMessage generated for token status QUE", 1, ediMessages.Length);
		AssertEDIMessage(ediMessages[0]);

		void RunMessageSender()
		{
			using (DisposableEnvironment.ForCompany(company.GC_Code))
			{
				messageSending.Send();
			}
		}

		void AssertEDIMessage(EDIMessage ediMessage) => CombineAssertions(() =>
		{
			AssertEquals("EM_ApplicationCode", ApplicationCodes.CHCustomsPassar, ediMessage.EM_ApplicationCode);
			AssertEquals("EM_MessageType", MessageTypeCodeList.Codes.TRE, ediMessage.EM_MessageType);
			AssertEquals("EM_ReceiveTransmit", ReceiveTransmitList.Codes.Transmit, ediMessage.EM_ReceiveTransmit);
			AssertEquals("EM_Status", EDIMessage.Status.Queued, ediMessage.EM_Status);
			AssertEquals("EM_MessageText", CredentialsTestHelper.ExpectedRequestBody, ediMessage.EM_MessageText);
			AssertEquals("EM_LinkTable", "GlbCompany", ediMessage.EM_LinkTable);
			AssertEquals("EM_LinkUniqueID", company.PK, ediMessage.EM_LinkUniqueID);

			AssertEquals("Token Status after processing", "QUE", token.GP_PasswordStatus);
			AssertEquals("Tokens Refresh Event", "Requested", company.Logs.MostRecentLogByEventTime(Events.CommunicationTokensRefresh).SL_Reference);
			Assert("Logger should log Tokens Refresh information", serviceLogger.ContainsLogEntry($"Token Refresh message for company {company.GC_Code} has been processed."));
		});
	}

	protected override void SetUp()
	{
		base.SetUp();
		company = Factory.NewWithValidTestData<GlbCompany>();
		company.GC_Code = "CHC";
		company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Switzerland;
		var branch = Factory.NewWithValidTestData<GlbBranch>();
		branch.GB_Code = "CHB";
		branch.GB_GC = company.PK;
		branch.GB_RL_NKHomePort = "AUSYD";

		token = CredentialsTestHelper.CreateCompanyTokenCredential(company);

		serviceLogger = new LoggingInformationForTesting();
		messageSending = new TokensRefreshMessageSender(serviceLogger);
		Factory.Save();
	}

	GlbCompanyTokenCredentials token;
	GlbCompany company;
	TokensRefreshMessageSender messageSending;
	LoggingInformationForTesting serviceLogger;
}
