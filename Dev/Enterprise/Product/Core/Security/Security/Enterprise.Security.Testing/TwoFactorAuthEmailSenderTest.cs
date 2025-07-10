using System;
using System.Text.RegularExpressions;
using CargoWise.Data;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Security.Testing
{
	sealed class TwoFactorAuthEmailSenderTest : TestCaseWithFactory
	{
		public void TestGenerateTwoFactorAuthenticationCode()
		{
			var twoFactorAuthEmailSender = new TwoFactorAuthEmailSender();
			var codeRegex = new Regex(@"^\d{6}$");
			var code1 = twoFactorAuthEmailSender.GenerateTwoFactorAuthenticationCode();
			var code2 = twoFactorAuthEmailSender.GenerateTwoFactorAuthenticationCode();

			AssertEquals("Code should be 6 numeric characters.", true, codeRegex.IsMatch(code1));
			AssertEquals("Code should be 6 numeric characters.", true, codeRegex.IsMatch(code2));
			AssertNotEquals("Code should be random", code1, code2);
		}

		public void TestSendTwoFactorAuthenticationCode()
		{
			Env.Registry.EmailDestinationOverride = "admin@cw1.com";
			Env.Registry.SMTPDefaultReturnEmailAddress = "donotReply@cw1.com";
			SystemDataRegistry.Instance.TwoFactorAuthenticationTypes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "Email");

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_LoginName = "bill";
			staff.GS_EmailAddress = "bill.murray@cw1.com";
			staff.StaffPlainTextPassword = "awesome";
			staff.IsTwoFactorAuthenticationEnabled = true;
			Factory.Save();

			using (Env.SetTemporaryUserContext(staff.PK.ToGuid(), GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			using (SystemDataRegistry.Instance.MakeTwoFactorAuthenticationEmailHTML.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var code = new TwoFactorAuthEmailSender().SendTwoFactorAuthenticationCode(staff);

				AssertEquals(1, Env.OutgoingMailManager.EmailsCreated.Count);
				var email = Env.OutgoingMailManager.EmailsCreated[0];
				CombineAssertions(() =>
				{
					AssertEquals("Recipient count", 1, email.Recipients.Count);
					AssertEquals("Recipient.IsForSystemCommunication", true, email.Recipients[0].IsForSystemCommunication);
					AssertEquals("Recipient", staff.GS_EmailAddress, email.Recipients[0]);
					AssertEquals("Subject", string.Format("Your {0} Login Code", Enterprise.Core.Constants.ProductName), email.Subject);
					AssertEquals("Body", code + "\r\n\r\n" + "Please note that this code is only valid for the current login attempt", email.Body);
					AssertEquals("ContentType", EmailContentTypes.PlainText, email.ContentType);
					AssertEquals("donotReply@cw1.com", email.FromAddress);

					AssertEquals("registry address should not overrride 2FA email.", 0, Db.Connection.ExecuteScalar("Select Count(*) from dbo.maildbrecipients where MR_RecipientMailAddress = 'admin@cw1.com'"));
					AssertEquals("registry address should not overrride 2FA email.", 1, Db.Connection.ExecuteScalar("Select Count(*) from dbo.maildbrecipients where MR_RecipientMailAddress = 'bill.murray@cw1.com'"));
				});
			}
		}

		public void TestSendTwoFactorAuthenticationCodeAsHTML()
		{
			Env.Registry.EmailDestinationOverride = "admin@cw1.com";
			Env.Registry.SMTPDefaultReturnEmailAddress = "donotReply@cw1.com";
			SystemDataRegistry.Instance.TwoFactorAuthenticationTypes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "Email");

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_LoginName = "bill";
			staff.GS_EmailAddress = "bill.murray@cw1.com";
			staff.StaffPlainTextPassword = "awesome";
			staff.IsTwoFactorAuthenticationEnabled = true;
			Factory.Save();

			using (Env.SetTemporaryUserContext(staff.PK.ToGuid(), GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			using (SystemDataRegistry.Instance.MakeTwoFactorAuthenticationEmailHTML.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var code = new TwoFactorAuthEmailSender().SendTwoFactorAuthenticationCode(staff);

				AssertEquals(1, Env.OutgoingMailManager.EmailsCreated.Count);
				var email = Env.OutgoingMailManager.EmailsCreated[0];
				CombineAssertions(() =>
				{
					AssertEquals("Recipient count", 1, email.Recipients.Count);
					AssertEquals("Recipient.IsForSystemCommunication", true, email.Recipients[0].IsForSystemCommunication);
					AssertEquals("Recipient", staff.GS_EmailAddress, email.Recipients[0]);
					AssertEquals("Subject", string.Format("Your {0} Login Code", Enterprise.Core.Constants.ProductName), email.Subject);
					AssertContains("Body", code + "\r\n\r\n" + "Please note that this code is only valid for the current login attempt", email.Body);
					AssertContains("Body", "<!--Call Stack:    at Enterprise.Security.TwoFactorAuthEmailSender.SendTwoFactorAuthenticationCode(IUser user)", email.Body);
					AssertEquals("ContentType", EmailContentTypes.HTML, email.ContentType);
					AssertEquals("donotReply@cw1.com", email.FromAddress);
				});
			}
		}
	}
}
