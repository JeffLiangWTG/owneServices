using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.CA.Registry;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class B3SendingNotificationHelperTest : TestCaseWithFactory
	{
		public void TestSendMail()
		{
			var staff1 = Factory.New<GlbStaff>();
			staff1.GS_Code = "T1";
			staff1.GS_LoginName = "STAFF1";
			staff1.GS_EmailAddress = "staff1@wisetechglobal.com";
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
			declaration.JE_DeclarationReference = "B00000001";
			declaration.JE_SystemCreateUser = staff1.GS_Code;

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			var helper = new B3SendingNotificationHelper(declaration);
			helper.SendEmail("B3 Sending Notification", "Body");

			var email1 = Env.OutgoingCustomsMailManager.EmailsCreated.Find(emailToMatched => emailToMatched.Subject == "B3 Sending Notification");
			AssertNotNull("B3 Sending Notification should be created", email1);
			AssertEquals("Email.Recipients", 1, email1.Recipients.Count);
			Assert("Email.Recipients", email1.Recipients.Contains(staff1.GS_EmailAddress));

			var staff2 = Factory.New<GlbStaff>();
			staff2.GS_Code = "T2";
			staff2.GS_LoginName = "STAFF2";
			staff2.GS_EmailAddress = "staff2@wisetechglobal.com";
			var staff3 = Factory.New<GlbStaff>();
			staff3.GS_Code = "T3";
			staff3.GS_LoginName = "STAFF3";
			staff3.GS_EmailAddress = "staff3@wisetechglobal.com";
			GlbGroup group = Factory.New<GlbGroup>();
			group.Staff.Add(staff2);
			group.Staff.Add(staff3);

			CACustomsDataRegistry.Instance.SendDeclarationMessageErrors.SetTemporaryValue(declaration.Branch.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, Core.Constants.EmailTo.StaffMember);
			CACustomsDataRegistry.Instance.SendDeclarationMessageErrorsToGroup.SetTemporaryValue(declaration.Branch.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, group.PK.ToGuid());

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			helper.SendEmail("B3 Sending Notification", "Body");

			var email2 = Env.OutgoingCustomsMailManager.EmailsCreated.Find(emailToMatched => emailToMatched.Subject == "B3 Sending Notification");
			AssertNotNull("B3 Sending Notification should be created", email2);
			AssertEquals("Email.Recipients", 2, email2.Recipients.Count);
			Assert("Email.Recipients", !email2.Recipients.Contains(staff1.GS_EmailAddress));
			Assert("Email.Recipients", email2.Recipients.Contains(staff2.GS_EmailAddress));
			Assert("Email.Recipients", email2.Recipients.Contains(staff3.GS_EmailAddress));

			var staff4 = Factory.New<GlbStaff>();
			staff4.GS_Code = "T4";
			staff4.GS_LoginName = "STAFF4";
			staff4.GS_EmailAddress = "staff4@wisetechglobal.com";

			var message = declaration.Messages.AddNew();
			message.EM_SystemCreateUser = staff4.GS_Code;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message.EM_SystemCreateTimeUtc = ZDateTime.Now;

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			helper.SendEmail("B3 Sending Notification", "Body");

			var email3 = Env.OutgoingCustomsMailManager.EmailsCreated.Find(emailToMatched => emailToMatched.Subject == "B3 Sending Notification");
			AssertNotNull("B3 Sending Notification should be created", email3);
			AssertEquals("Email.Recipients", 1, email3.Recipients.Count);
			Assert("Email.Recipients", !email3.Recipients.Contains(staff1.GS_EmailAddress));
			Assert("Email.Recipients", !email3.Recipients.Contains(staff2.GS_EmailAddress));
			Assert("Email.Recipients", !email3.Recipients.Contains(staff3.GS_EmailAddress));
			Assert("Email.Recipients", email3.Recipients.Contains(staff4.GS_EmailAddress));
		}
	}
}
