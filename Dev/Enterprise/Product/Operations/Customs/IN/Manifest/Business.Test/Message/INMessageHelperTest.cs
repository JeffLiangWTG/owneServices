using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.IN.Business;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.IN.Manifest.Business.Testing;

[TestedType(typeof(INMessageHelper))]
sealed class INMessageHelperTest : TestCaseWithFactory
{
	public void TestGetRecipient()
	{
		CombineAssertions(() =>
		{
			var message = Factory.New<EDIMessage>();
			message.EM_IsTestMessage = true;
			AssertEquals("Test Env", Constants.Messaging.IceGate.TestName, INMessageHelper.GetMessageRecipient(message));

			message.EM_IsTestMessage = false;
			AssertEquals("Prod Env", Constants.Messaging.IceGate.ProdName, INMessageHelper.GetMessageRecipient(message));
		});
	}

	public void TestGetSenderEmailId()
	{
		CombineAssertions(() =>
		{
			var user = Factory.New<GlbStaff>();
			user.GS_LoginName = "Enterprise User";
			user.GS_Code = "AAA";

			var message = Factory.New<EDIMessage>();
			AssertEquals("EM_SystemCreateUser not set", ZString.Empty, INMessageHelper.GetMessageSenderEmailId(message));

			message.EM_SystemCreateUser = user.GS_Code;
			AssertEquals("LoginPassword not set and when message created by non-CWSupport user", ZString.Empty, INMessageHelper.GetMessageSenderEmailId(message));

			var loginPassword = IN.Business.GlbStaffWrapper.Get(user).LoginPassword;
			message.EM_GP = loginPassword.PK;
			AssertEquals("GP_MailBoxID not set when message created by non-CWSupport user", ZString.Empty, INMessageHelper.GetMessageSenderEmailId(message));

			loginPassword.GP_MailBoxID = "ABC";
			AssertEquals("GP_MailBoxID set when message created by non-CWSupport user", "ABC", INMessageHelper.GetMessageSenderEmailId(message));

			var supporter = Factory.Load<GlbStaff>(GlbStaff.CurrentUser.PK);
			if (!supporter.IsSupportUser)
			{
				supporter = Factory.New<GlbStaff>();
				supporter.GS_LoginName = User.SupportUserName;
				supporter.GS_Code = "E";
			}

			message = Factory.New<EDIMessage>();
			message.EM_SystemCreateUser = supporter.GS_Code;
			AssertEquals("LoginPassword not set and when message created by CWSupport user", ZString.Empty, INMessageHelper.GetMessageSenderEmailId(message));

			loginPassword = IN.Business.GlbStaffWrapper.Get(supporter).LoginPassword;
			message.EM_GP = loginPassword.PK;
			AssertEquals("GP_MailBoxID not set when message created by CWSupport user", ZString.Empty, INMessageHelper.GetMessageSenderEmailId(message));

			loginPassword.GP_MailBoxID = "BCD";
			AssertEquals("GP_MailBoxID set when message created by CWSupport user", "BCD", INMessageHelper.GetMessageSenderEmailId(message));
		});
	}

	public void TestGetRecipientEmailId()
	{
		var today = ZDateTime.Now;
		var yesterday = today.AddDays(-1);
		var tomorrow = today.AddDays(1);

		const string officeEmailId = "abc@xyz.com";
		var helper = new UniversalReferenceTestDataHelper(Factory);
		helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "India Customs EDI Location");
		var customsOffice = helper.CreateCusCodeList(Core.Constants.CountryCodes.India, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "ABC123", yesterday, tomorrow);
		helper.CreateCusCodeListAttribute(customsOffice.PK, Constants.RefCusCodeList.Attributes.EmailAddress, officeEmailId);
		Factory.Save();

		var message = Factory.New<EDIMessage>();
		CombineAssertions(() =>
		{
			AssertNullOrEmpty("Message object is not linked", INMessageHelper.GetMessageRecipientEmailId(message));

			var manifestHeader = Factory.New<CGMAsycudaManifestHeader>();

			manifestHeader.AMA_CustomsOffice = "ABC123";
			message.EM_LinkedObject = manifestHeader;
			AssertEquals("Recipient EmailId is set", officeEmailId, INMessageHelper.GetMessageRecipientEmailId(message));

			manifestHeader.AMA_CustomsOffice = "XYZ789";
			AssertNullOrEmpty("Recipient EmailId is not set", INMessageHelper.GetMessageRecipientEmailId(message));
		});
	}

	public void TestGetMessageCopyToEmailId()
	{
		var loginPassword = Factory.New<GlbLoginPassword>();
		var message = Factory.New<EDIMessage>();

		CombineAssertions(() =>
		{
			AssertEquals("When Login password missing", ZString.Empty, INMessageHelper.GetMessageCopyToEmailId(message));

			message.EM_GP = loginPassword.PK;
			AssertEquals("When LoginPassword set and empty Main email in Staff Details", ZString.Empty, INMessageHelper.GetMessageCopyToEmailId(message));

			var staff = StaffDataSetupTestHelper.CreateStaffWithMainEmail(Factory, "abc@wtg.in");
			loginPassword.GP_GS = staff.PK;
			loginPassword.NeedCopyOfEmails = true;
			AssertEquals("When LoginPassword set and Main email entered in Staff Details", "abc@wtg.in", INMessageHelper.GetMessageCopyToEmailId(message));
		});
	}
}
