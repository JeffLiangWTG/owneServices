using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Customs.CH.Business.MessagingConstants;

namespace Enterprise.Customs.CH.Business.Testing;

public abstract class BaseMessageListRequestSenderTest : TestCaseWithFactory
{
	protected abstract string ApplicationCode { get; }

	protected abstract string FriendlyName { get; }
	protected abstract string CustomsDestinationCode { get; }

	protected abstract BaseMessageListRequestSender CreateMessageListRequestSender(LoggingInformation logger);

	protected virtual string GetExpectedHeaderText(GlbCompany company, CusPollingTransaction transaction = null)
	{
		var bpid = company.OrgProxy.CustomsCodes.GetCustomsRegNo(OrgCusCode.SwissCodeTypes.BID);
		var lastMessageId = transaction?.CPT_TransactionID;
		return $@"{{""{CustomMsgAttributes.BpId}"":""{bpid}"",""{CustomMsgAttributes.LastMessageID}"":""{lastMessageId}""}}";
	}

	public void TestConstructor() => AssertExceptionThrown<ArgumentNullException>(() => CreateMessageListRequestSender(null));

	public void TestSendRequest_NoTokenCredentials()
	{
		var company = CreateCompany(Factory);
		Factory.Save();

		var logger = SendMessageListRequest(company);
		AssertEquals($"Debug - No {FriendlyName} for company {company.GC_Code} has been processed.", logger.Logs.First().ToString());

		var ediMessages = LoadMessages(company);
		AssertEquals("No EDIMessages expected at this stage", 0, ediMessages.Length);
	}

	public void TestSendResponse_WithoutLastMessageId()
	{
		var company = CreateCompany(Factory);
		CreateGlbCompanyTokenCredentials(company);
		var expectedRequest = GetExpectedHeaderText(company);

		Factory.Save();

		CombineAssertions(() =>
		{
			var logger = SendMessageListRequest(company);

			var ediMessages = LoadMessages(company);
			AssertEquals("One EDIMessage for MessageList Request", 1, ediMessages.Length);

			var ediMessage = ediMessages.FirstOrDefault();
			var ediInterchange = Factory.Load<EDIInterchange>(ediMessage.EM_EI);
			AssertNotNull("One EDIInterchange for MessageList Request", ediInterchange);

			AssertEdiMessage(ediMessage, company, expectedRequest);
			AssertEdiInterchange(ediInterchange, ediMessage, company, expectedRequest);
			AssertEquals($"1 {FriendlyName}(s) for company {company.GC_Code} has been processed.", logger.Logs.First().ToString());
		});
	}

	public void TestSendResponse_WithLastMessageId()
	{
		var lastMessageId = "LastMessageId";

		var company = CreateCompany(Factory);
		CreateGlbCompanyTokenCredentials(company);
		var transaction = company.CreateLastMessageIdTransaction(ApplicationCode);
		transaction.CPT_TransactionID = lastMessageId;
		var expectedRequest = GetExpectedHeaderText(company, transaction);

		Factory.Save();

		CombineAssertions(() =>
		{
			var logger = SendMessageListRequest(company);

			var ediMessages = LoadMessages(company);
			AssertEquals("One EDIMessage for MessageList Request", 1, ediMessages.Length);

			var ediMessage = ediMessages.FirstOrDefault();
			var ediInterchange = Factory.Load<EDIInterchange>(ediMessage.EM_EI);
			AssertNotNull("One EDIInterchange for MessageList Request", ediInterchange);

			AssertEdiMessage(ediMessage, company, expectedRequest);
			AssertEdiInterchange(ediInterchange, ediMessage, company, expectedRequest);
			AssertEquals($"1 {FriendlyName}(s) for company {company.GC_Code} has been processed.", logger.Logs.First().ToString());
		});
	}

	void AssertEdiMessage(CHEDIMessage ediMessage, GlbCompany company, string expectedRequest)
	{
		var externalPasswordPK = GlbCompanyWrapper.GetWrapper<GlbCompanyWrapper>(company)?.TokenCredentials?.PK ?? ZGuid.Empty;
		AssertEquals("EM_ApplicationCode", ApplicationCode, ediMessage.EM_ApplicationCode);
		AssertEquals("EM_MessageType", MessageTypeCodeList.Codes.MSL, ediMessage.EM_MessageType);
		AssertEquals("EM_ReceiveTransmit", ReceiveTransmitList.Codes.Transmit, ediMessage.EM_ReceiveTransmit);
		Assert("EM_MessageNum", !ediMessage.EM_MessageNum.IsEmpty);
		AssertEquals("EM_Status", EDIMessage.Status.Sent, ediMessage.EM_Status);
		JsonTestHelper.AssertEquals("EM_MessageText", expectedRequest, ediMessage.EM_MessageText);
		AssertEquals("EM_LinkTable", GlbCompanySchema.Constants.TableName, ediMessage.EM_LinkTable);
		AssertEquals("EM_LinkUniqueID", company.PK, ediMessage.EM_LinkUniqueID);
		AssertEquals("EM_GP", externalPasswordPK, ediMessage.EM_GP);
	}

	LoggingInformationForTesting SendMessageListRequest(GlbCompany company)
	{
		var loggingInformation = new LoggingInformationForTesting();
		var messageListRequestSender = CreateMessageListRequestSender(loggingInformation);
		using (DisposableEnvironment.ForCompany(company.GC_Code))
		{
			messageListRequestSender.Send();
		}
		return loggingInformation;
	}

	CHEDIMessage[] LoadMessages(GlbCompany company)
	{
		var query = new ZQuery(EDIMessageSchema.EM_LinkUniqueID, company.PK);
		query.AddToFilter(EDIMessageSchema.EM_ApplicationCode, ApplicationCode);
		query.AddToFilter(EDIMessageSchema.EM_MessageType, MessageTypeCodeList.Codes.MSL);
		return Factory.Load<CHEDIMessage>(query);
	}

	void AssertEdiInterchange(EDIInterchange ediInterchange, EDIMessage ediMessage, GlbCompany company, string expectedHeaderText)
	{
		AssertEquals("EI_ApplicationCode", ApplicationCode, ediInterchange.EI_ApplicationCode);
		AssertEquals("EI_InterchangeType", MessageTypeCodeList.Codes.MSL, ediInterchange.EI_InterchangeType);
		AssertEquals("EI_ReceiveTransmit", ReceiveTransmitList.Codes.Transmit, ediInterchange.EI_ReceiveTransmit);
		AssertEquals("EI_From", company.LicenceKeyIdentifier, ediInterchange.EI_From);
		AssertEquals("EI_To", CustomsDestinationCode, ediInterchange.EI_To);
		AssertEquals("EI_TransportType", EDIInterchange.TransportType.xT, ediInterchange.EI_TransportType);
		AssertEquals("EI_Status", EDIMessage.Status.Queued, ediInterchange.EI_Status);
		Assert("EI_InterchangeNum", !ediInterchange.EI_InterchangeNum.IsEmpty);
		Assert("EI_SessionGUID", !ediInterchange.EI_SessionGUID.IsEmpty);
		AssertEquals("EI_IsActive", true, ediInterchange.EI_IsActive);
		AssertEquals("EI_GP", ediMessage.EM_GP, ediInterchange.EI_GP);
		JsonTestHelper.AssertEquals("EI_HeaderText", expectedHeaderText, ediInterchange.EI_HeaderText);
		AssertEquals("EI_BodyText", ZString.Empty, ediInterchange.EI_BodyText);
	}

	static GlbCompany CreateCompany(BusinessObjectFactory factory)
	{
		var company = factory.NewWithValidTestData<GlbCompany>();
		company.GC_Code = "CHC";
		company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Switzerland;
		company.GC_OH_OrgProxy = factory.NewWithValidTestData<OrgHeader>().PK;

		var branch = factory.NewWithValidTestData<GlbBranch>();
		branch.GB_Code = "CHB";
		branch.GB_GC = company.PK;
		branch.GB_RL_NKHomePort = "AUSYD";

		company.OrgProxy.CustomsCodes.AddNew(OrgCusCode.SwissCodeTypes.BID, "234567");

		return company;
	}

	static GlbCompanyTokenCredentials CreateGlbCompanyTokenCredentials(GlbCompany parent)
	{
		var companyWrapper = GlbCompanyWrapper.GetWrapper<GlbCompanyWrapper>(parent);
		companyWrapper.TokenCredentialsEnabled = true;
		var token = companyWrapper.TokenCredentials;

		token.GP_Certificate = X509Certificate2TestHelper.ValidCertificate;
		token.GP_PasswordStatus = PasswordStatusList.Codes.Valid;

		token.GP_UserID = clientId;
		token.CurrentDecryptedPassword = clientSecret;

		token.RefreshTokenText = refreshToken;
		token.GP_CertificateText = accessToken;
		return token;
	}

	const string clientSecret = "12345679";
	const string refreshToken = "_03ca28a0-c74e-3b6d-9453-259a4deb4257";
	const string clientId = "fgLj92vLh6bdB0W7XmHXq_T19kMa";
	const string accessToken = "the-access-token";
}

