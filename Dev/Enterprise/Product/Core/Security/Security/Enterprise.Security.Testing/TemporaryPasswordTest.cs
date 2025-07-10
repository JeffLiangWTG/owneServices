using System;
using CargoWise.Data;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Security.Testing
{
	sealed class TemporaryPasswordTest : TestCaseWithFactory
	{
		public void TestCreateAndSendTemporaryPassword()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_LoginName = "bill";
			staff.StaffPlainTextPassword = "awesome";
			Factory.Save();

			//staff is not required to reset password
			AssertEquals(false, staff.LocalPasswordMustBeReset);
			var tempPassword = TemporaryPassword.CreateAndSendTemporaryPassword(staff);
			AssertNull("Not require to reset password", tempPassword);

			//staff does not have an email
			staff.LocalPasswordMustBeReset = true;
			staff.GS_EmailAddress = "";
			Factory.Save();
			AssertEquals(true, staff.LocalPasswordMustBeReset);
			tempPassword = TemporaryPassword.CreateAndSendTemporaryPassword(staff);
			AssertNull("No email", tempPassword);

			//staff has an email but not valid
			staff.GS_EmailAddress = "billgatesmicrosoftcom";
			Factory.Save();
			tempPassword = TemporaryPassword.CreateAndSendTemporaryPassword(staff);
			AssertNull("Invalid email", tempPassword);

			//staff has a valid email
			staff.GS_EmailAddress = "bill.gates@microsoft.com";
			Factory.Save();
			tempPassword = TemporaryPassword.CreateAndSendTemporaryPassword(staff);
			AssertNotNull("Valid email", tempPassword);

			AssertEquals("User", "bill", tempPassword.User.LoginName);
			AssertNotNullOrEmpty("Password", tempPassword.Password);
			AssertDateTimeWithinOneSecond("Time stamp", DateTime.UtcNow, tempPassword.CreatedOn.ToDateTime());
			AssertEquals("Attempts", 0, tempPassword.Attempt);

			//email no longer unique in the system
			var staff2 = Factory.NewWithValidTestData<GlbStaff>();
			staff2.GS_LoginName = "bill2";
			staff2.StaffPlainTextPassword = "awesome";
			staff2.GS_EmailAddress = "bill.gates@microsoft.com";
			Factory.Save();

			tempPassword = TemporaryPassword.CreateAndSendTemporaryPassword(staff);
			AssertNull("Email not valid", tempPassword);
		}

		public void TestCreateAndSendTemporaryPassword_EmailSentWithTheRightDetails()
		{
			Env.Registry.EmailDestinationOverride = "admin@cw1.com";

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_LoginName = "bill";
			staff.StaffPlainTextPassword = "awesome";
			Factory.Save();

			staff.LocalPasswordMustBeReset = true;
			staff.GS_EmailAddress = "bill.gates@microsoft.com";
			Factory.Save();
			var tempPassword = TemporaryPassword.CreateAndSendTemporaryPassword(staff);
			AssertNotNull("Valid email", tempPassword);

			AssertEquals("User", "bill", tempPassword.User.LoginName);
			AssertNotNullOrEmpty("Password", tempPassword.Password);
			AssertDateTimeWithinOneSecond("Time stamp", DateTime.UtcNow, tempPassword.CreatedOn.ToDateTime());
			AssertEquals("Attempts", 0, tempPassword.Attempt);

			AssertEquals(1, Env.OutgoingMailManager.EmailsCreated.Count);
			var email = Env.OutgoingMailManager.EmailsCreated[0];
			CombineAssertions(() =>
			{
				AssertEquals("Recipient count", 1, email.Recipients.Count);
				AssertEquals("Recipient", staff.GS_EmailAddress, email.Recipients[0]);
				AssertEquals("Recipient.IsForSystemCommunication", true, email.Recipients[0].IsForSystemCommunication);
				AssertEquals("Subject", $"Your {Enterprise.Core.Constants.ProductName} Temporary Password for system EDIDAT.", email.Subject);
				AssertStartsWith("Body", tempPassword.Password, email.Body);
				AssertEndsWith("Body", "This temporary password is only valid for the current login attempt for system EDIDAT.", email.Body);
				AssertEquals("ContentType", EmailContentTypes.PlainText, email.ContentType);

				AssertEquals("registry address should not overrride 2FA email.", 0, Db.Connection.ExecuteScalar("Select Count(*) from dbo.maildbrecipients where MR_RecipientMailAddress = 'admin@cw1.com'"));
				AssertEquals("registry address should not overrride 2FA email.", 1, Db.Connection.ExecuteScalar("Select Count(*) from dbo.maildbrecipients where MR_RecipientMailAddress = 'bill.gates@microsoft.com'"));
			});
		}

		public void TestValidate()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_LoginName = "bill";
			staff.StaffPlainTextPassword = "awesome";
			staff.GS_EmailAddress = "bill.gates@microsoft.com";
			Factory.Save();
			staff.LocalPasswordMustBeReset = true;
			Factory.Save();

			var tempPassword = TemporaryPassword.CreateAndSendTemporaryPassword(staff);
			AssertEquals("Invalid password", TemporaryPasswordValidationResult.Invalid, tempPassword.Validate(tempPassword.User.LoginName, "wrong"));
			AssertEquals("Attempt", 1, tempPassword.Attempt);

			AssertEquals("Invalid user", TemporaryPasswordValidationResult.Invalid, tempPassword.Validate("wrongname", tempPassword.Password));
			AssertEquals("Attempt", 2, tempPassword.Attempt);

			AssertEquals("Valid user and password", TemporaryPasswordValidationResult.OK, tempPassword.Validate(tempPassword.User.LoginName, tempPassword.Password));
			AssertEquals("Attempt", 3, tempPassword.Attempt);
		}

		[TestDate(2019, 09, 26, 17, 49, 37)]
		public void TestValidate_HasExpired()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_LoginName = "bill";
			staff.StaffPlainTextPassword = "awesome";
			staff.GS_EmailAddress = "bill.gates@microsoft.com";
			Factory.Save();
			staff.LocalPasswordMustBeReset = true;
			Factory.Save();

			var tempPassword = TemporaryPassword.CreateAndSendTemporaryPassword(staff);
			AssertEquals("Valid user and password", TemporaryPasswordValidationResult.OK, tempPassword.Validate(tempPassword.User.LoginName, tempPassword.Password));
			AssertEquals("Attempt", 1, tempPassword.Attempt);

			//Valid password but has expired
			TestDateAttribute.AddMinutes(11);
			AssertEquals("Expired", TemporaryPasswordValidationResult.Expired, tempPassword.Validate(tempPassword.User.LoginName, tempPassword.Password));
			AssertEquals("Attempt", 2, tempPassword.Attempt);

			//We travel back time
			TestDateAttribute.AddMinutes(-1);
			AssertEquals("Valid user and password", TemporaryPasswordValidationResult.OK, tempPassword.Validate(tempPassword.User.LoginName, tempPassword.Password));
			AssertEquals("Attempt", 3, tempPassword.Attempt);

			AssertEquals("Valid user and password, but attempt maxed", TemporaryPasswordValidationResult.Expired, tempPassword.Validate(tempPassword.User.LoginName, tempPassword.Password));
			AssertEquals("Attempt", 4, tempPassword.Attempt);
		}
	}
}
