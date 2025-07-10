using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.IN.Business;
using Enterprise.Customs.IN.Manifest.Business;
using Enterprise.Customs.IN.Manifest.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Services.OperationalActions.Support.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.IN.Manifest.Module.Testing;

[TestedType(typeof(MessageSendingOperationalActionMethodApplicator))]
sealed class MessageSendingOperationalActionMethodApplicatorTest : OperationalActionMethodApplicatorTest
{
	public void TestName()
	{
		AssertEquals("Sending message", Applicator.Name);
	}

	public void TestCheckIsOKToSend()
	{
		const string dscTokenDetailsMissingMessage = "DSC Token Profile is not setup for current user.";
		const string selectValidTransactionMessage = "Please select at least one India Forwarder Manifest.";

		RefDataSetupTestHelper.SetupCertificateTokenData(Factory);
		var (inHeader, zaHeader) = GetSampleManifests();
		UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
		var log = SimulateRun(Array.Empty<BusinessObject>(), false);
		AssertUserNotificationAndErrorLog("When records not selected", selectValidTransactionMessage);

		UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
		log = SimulateRun(new[] { zaHeader }, false);
		AssertUserNotificationAndErrorLog("When Non IN record selected", selectValidTransactionMessage);

		UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
		log = SimulateRun(new[] { zaHeader, inHeader }, false);
		AssertNullOrEmpty("For Support user, Bulk message sending validation - user notification", UnitTestUserNotification.Instance.LastMessage.Text);
		AssertContains("For Support user, Bulk message sending validation - log", "WARNING: [HL IN123] Skipped. Errors & Warning observed.", log.MessagesString());

		CombineAssertions("For normal user, Bulk message sending validation", () =>
		{
			GlbStaff.CurrentUser.GS_LoginName = "Enterprise User";
			log = SimulateRun(new[] { zaHeader, inHeader }, false);
			AssertUserNotificationAndErrorLog("Icegate details not setup", "ICEGATE Login ID and Email ID and DSC Token Profile is not defined for current user under Staff & Resources Credentials.");

			var loginPassword = IN.Business.GlbStaffWrapper.GetWrapperForCurrentUser().LoginPassword;
			loginPassword.GP_MailBoxID = "user@test.in";
			log = SimulateRun(new[] { zaHeader, inHeader }, false);
			AssertUserNotificationAndErrorLog("User Id missing", "ICEGATE Login ID is not defined for current user.");

			loginPassword.GP_UserID = "iceuser1";
			loginPassword.GP_MailBoxID = ZString.Empty;
			log = SimulateRun(new[] { zaHeader, inHeader }, false);
			AssertUserNotificationAndErrorLog("Email missing", "ICEGATE Email ID is not defined for current user.");

			loginPassword.GP_MailBoxID = "user@test.in";
			log = SimulateRun(new[] { zaHeader, inHeader }, false);
			AssertUserNotificationAndErrorLog("CertificatePassword not setup", dscTokenDetailsMissingMessage);

			var certificatePassword = IN.Business.GlbStaffWrapper.GetWrapperForCurrentUser().CertificatePassword;
			certificatePassword.GP_CertificateSerialNumber = "123";
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			log = SimulateRun(new[] { zaHeader, inHeader }, false);
			AssertUserNotificationAndErrorLog("Chipset missing", dscTokenDetailsMissingMessage);

			certificatePassword.GP_Name = "XYZ";
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			log = SimulateRun(new[] { zaHeader, inHeader }, false);
			AssertUserNotificationAndErrorLog("Chipset invalid", dscTokenDetailsMissingMessage);

			certificatePassword.GP_Name = "WatchData";
			certificatePassword.GP_CertificateSerialNumber = ZString.Empty;
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			log = SimulateRun(new[] { zaHeader, inHeader }, false);
			AssertUserNotificationAndErrorLog("Serial number missing", dscTokenDetailsMissingMessage);

			certificatePassword.GP_CertificateSerialNumber = "1234";
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			log = SimulateRun(new[] { zaHeader, inHeader }, false);
			AssertContains("Message sending canceled.", UnitTestUserNotification.Instance.LastMessage.Text);
			AssertNullOrEmpty(log.MessagesString());
		});

		void AssertUserNotificationAndErrorLog(string message, string notificationText)
		{
			AssertEquals($"{message} - user notification", notificationText, UnitTestUserNotification.Instance.LastMessage.Text);
			AssertContains($"{message} - log", $"ERROR: {notificationText}", log.MessagesString());
		}
	}

	public void TestSendMultipleHeaderMessage()
	{
		RefDataSetupTestHelper.SetupCertificateTokenData(Factory);
		var certificatePassword = IN.Business.GlbStaffWrapper.GetWrapperForCurrentUser().CertificatePassword;
		certificatePassword.GP_Name = "WatchData";
		certificatePassword.GP_CertificateSerialNumber = "1234";

		GlbStaff.CurrentUser.GS_LoginName = "Enterprise User";
		var password = IN.Business.GlbStaffWrapper.GetWrapperForCurrentUser()?.LoginPassword;
		password.GP_GS = GlbStaff.CurrentUser.PK;
		password.GP_UserID = "test";
		password.GP_MailBoxID = "test@test.com";

		var (inHeader, zaHeader) = GetSampleManifests();
		var header = (CGMAsycudaManifestHeader)inHeader;
		header.AMA_JobReference = "IN123";
		header.AMA_CustomsOffice = "INMUM";
		Factory.Save();

		CombineAssertions(() =>
		{
			var log = SimulateRun(new[] { zaHeader, inHeader }, true);
			AssertNotContains("WARNING: [HL IN123] Skipped. Errors & Warning observed.", log.MessagesString());

			var tokenPinStore = CertificateTokenPinStore.Instance as ITokenPinStore;
			tokenPinStore.ResetPin();
			tokenPinStore.SetPin("1234");

			log = SimulateRun(new[] { zaHeader, inHeader }, true);
			AssertContains("WARNING: [HL IN123] Skipped. Errors & Warning observed.", log.MessagesString());
		});
	}

	(BusinessObject, BusinessObject) GetSampleManifests()
	{
		var inHeader = (BusinessObject)Factory.New<Integration.Customs.ASYCUDA.INManifest.ICGMAsycudaManifestHeader>();
		var zaHeader = (BusinessObject)Factory.New<Integration.Customs.ASYCUDA.ZAManifest.IAsycudaManifestHeader>();
		((CGMAsycudaManifestHeader)inHeader).AMA_JobReference = "IN123";
		Factory.Save();
		return (inHeader, zaHeader);
	}
}
