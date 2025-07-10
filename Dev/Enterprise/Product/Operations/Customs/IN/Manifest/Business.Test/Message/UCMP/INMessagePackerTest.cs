using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.IN.Business;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.MessageProcessor.Testing;
using Enterprise.Messaging.Integration;
using NUnit.Framework;

namespace Enterprise.Customs.IN.Manifest.Business.Testing;

[TestedType(typeof(INMessagePacker))]
sealed class INMessagePackerTest : UniversalCustomsEDIMessagePackerTest<INMessagePacker>
{
	[TestDate(2024, 3, 25)]
	public void TestPack()
	{
		var messagePacker = new INMessagePacker();
		var testMessage = CreateMessage(messageBody: "TEST MESSAGE", isTest: true);
		const string senderEmailId = "abc@xyz.com";
		const string customsOfficeCode = "ABC123";
		const string customsOfficeEmailId = "lmn@pqr.com";
		const string copyToEmailId = "abccopy@pqr.com";

		SetupCustomsOfficeData(Factory, customsOfficeCode, customsOfficeEmailId);

		var credentails = CreateCredentials(testMessage);
		credentails.GP_MailBoxID = senderEmailId;
		var staff = StaffDataSetupTestHelper.CreateStaffWithMainEmail(Factory, copyToEmailId);
		credentails.GP_GS = staff.PK;
		credentails.NeedCopyOfEmails = true;
		var linkedObject = CreateLinkedObject(testMessage);
		linkedObject.AMA_CustomsOffice = customsOfficeCode;

		var logger = new LoggingInformation();
		var interchange = Factory.New<EDIInterchange>();
		var error = messagePacker.Pack(testMessage, interchange, logger);

		CombineAssertions(() =>
		{
			AssertNullOrEmpty("No error when packing valid message", error);
			AssertInterchange(interchange, testMessage, senderEmailId, customsOfficeEmailId, copyToEmailId);

			testMessage.EM_IsTestMessage = false;
			_ = messagePacker.Pack(testMessage, interchange, logger);
			AssertEquals("EI_To when PROD env", Constants.Messaging.IceGate.ProdName, interchange.EI_To);

			credentails.GP_MailBoxID = ZString.Empty;
			_ = messagePacker.Pack(testMessage, interchange, logger);
			AssertContains("SenderEmailId when Support User", ZString.Empty, interchange.EI_HeaderText);

			testMessage.UserWhoQueuedThisRecord.GS_LoginName = "Enterprise User";
			error = messagePacker.Pack(testMessage, interchange, logger);
			AssertEquals("Missing interchange sender", "Customs Interchange Sender email id is not set up.", error);

			credentails.GP_MailBoxID = senderEmailId;
			linkedObject.AMA_CustomsOffice = "XYZ789";
			error = messagePacker.Pack(testMessage, interchange, logger);
			AssertEquals("Missing interchange recipient", "Customs Interchange Recipient email id is not set up.", error);
		});
	}

	protected override string ApplicationCode => ApplicationCodeList.Codes.INCustoms;

	void AssertInterchange(EDIInterchange interchange, IN.Business.EDIMessage packedMessage, string senderEmailId, string recipientEmailId, string copyToEmailId)
	{
		var messageType = packedMessage.EM_MessageType;
		var messageNum = packedMessage.EM_MessageNum;

		AssertEquals("EI_TransportType", EDIInterchangeTransportTypeList.Codes.xT, interchange.EI_TransportType);
		AssertEquals("EI_InterchangeNum", ZString.Empty, interchange.EI_InterchangeNum);
		AssertEquals("EI_ReceiveTransmit", EDIInterchange.Direction.Transmit, interchange.EI_ReceiveTransmit);
		AssertEquals("EI_IsActive", expected: true, interchange.EI_IsActive);
		AssertEquals("EI_Status", EDIInterchangeStatusList.Codes.Queued, interchange.EI_Status);
		AssertEquals("EI_To", Constants.Messaging.IceGate.TestName, interchange.EI_To);
		AssertEquals("EI_From", GlbCompany.CurrentCompany.LicenceKeyIdentifier, interchange.EI_From);
		AssertEquals("EI_ApplicationCode", packedMessage.EM_ApplicationCode, interchange.EI_ApplicationCode);
		AssertEquals("EI_InterchangeType", messageType, interchange.EI_InterchangeType);
		AssertEquals("EI_BodyText", packedMessage.EM_MessageText, interchange.EI_BodyText);

		var subject = messageType + " " + messageNum;
		var fileName = messageNum + "." + messageType;
		var expectedHeader = $"{{\"custom.IN.FromMailBox\":\"{senderEmailId}\",\"custom.IN.DestinationMailBox\":\"{recipientEmailId}\",\"custom.IN.Subject\":\"{subject}\",\"custom.IN.FileName\":\"{fileName}\",\"custom.IN.CopyToMailBox\":\"{copyToEmailId}\"}}";
		AssertEquals("EI_HeaderText", expectedHeader, interchange.EI_HeaderText);
	}

	IN.Business.EDIMessage CreateMessage(string messageBody = "", bool isTest = true)
	{
		var message = Factory.New<IN.Business.EDIMessage>();

		message.EM_ApplicationCode = ApplicationCodeList.Codes.INCustoms;
		message.EM_MessageType = "XXX";
		message.EM_MessageSubType = "YYY";
		message.EM_MessageOwner = "ZZZ";
		message.EM_MessageText = messageBody;
		message.EM_IsTestMessage = isTest;

		return message;
	}

	GlbLoginPassword CreateCredentials(IN.Business.EDIMessage message)
	{
		var credentials = Factory.NewWithValidTestData<GlbLoginPassword>();
		message.EM_GP = credentials.PK;
		return credentials;
	}

	CGMAsycudaManifestHeader CreateLinkedObject(IN.Business.EDIMessage message)
	{
		var manifestHeader = Factory.NewWithValidTestData<CGMAsycudaManifestHeader>();
		message.EM_LinkedObject = manifestHeader;
		return manifestHeader;
	}

	void SetupCustomsOfficeData(BusinessObjectFactory factory, string officeCode, string officeEmailId)
	{
		var today = ZDateTime.Now;
		var yesterday = today.AddDays(-1);
		var tomorrow = today.AddDays(1);

		var helper = new UniversalReferenceTestDataHelper(factory);
		helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "India Customs EDI Location");
		var customsOffice = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.India, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, officeCode, yesterday, tomorrow);
		helper.CreateNewOrGetExistingCusCodeListAttribute(customsOffice.PK, Constants.RefCusCodeList.Attributes.EmailAddress, officeEmailId);
		factory.Save();
	}
}
