using System;
using System.Reflection;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Environment;
using Enterprise.MailManager;
using Enterprise.MailManager.Business;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Mail.Business.Test
{
	[TestedType(typeof(EmailLoopDetector))]
	public class EmailLoopDetectorTest : TestCaseWithFactory
	{
		//MailItem mail, string application, string objectNumber
		readonly MethodInfo getRepliedEmailsCountInPeriod = typeof(EmailLoopDetector).GetMethod("GetReceivedEmailsCountInPeriod", BindingFlags.Static | BindingFlags.NonPublic);

		const string application = "CSV";

		void SetupRegistry()
		{
			EDIDataRegistry.Instance.EmailLoopDetectingWindowDuration.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 10);
			EDIDataRegistry.Instance.EmailLoopMaximumEmailCount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 5);
		}

		public void TestGetRepliedEmailsCountInPeriod()
		{
			SetupRegistry();
			var number = "CS00001234";

			var incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.IM_IncidentNumber = number;

			Factory.Save();

			Env.OutgoingMailManager.EmailsCreated.Clear();

			var time = ZDateTime.UtcNow;
			for (var i = 0; i < 3; i++)
			{
				var email = Factory.New<MailItem>();
				email.MI_Status = MailStatus.Processed;
				email.MI_Direction = MailDirection.Receive;
				email.MI_Application = application;
				email.MI_ReceivedDateTime = ZDateTime.UtcNow.AddMinutes(-2);
				email.MI_From = "User <123@123.com>";
				email.MI_Subject = $"{i}_XXXXXX {number} XXXXXXXX";
				email.MI_Body = $"Test{i}";
			}

			for (var i = 0; i < 3; i++)
			{
				var email = Factory.New<MailItem>();
				email.MI_Status = MailStatus.Processed;
				email.MI_Direction = MailDirection.Receive;
				email.MI_Application = application;
				email.MI_ReceivedDateTime = ZDateTime.UtcNow.AddMinutes(-(EDIDataRegistry.Instance.EmailLoopDetectingWindowDuration.Value + 10));
				email.MI_From = "User <123@123.com>";
				email.MI_Subject = $"False_{i}_XXXXXX {number} XXXXXXXX";
				email.MI_Body = $"Test{i}";
			}

			Factory.Save();

			var newEmail = Factory.New<MailItem>();
			newEmail.MI_Status = MailStatus.Processed;
			newEmail.MI_Direction = MailDirection.Receive;
			newEmail.MI_ReceivedDateTime = ZDateTime.UtcNow;
			newEmail.MI_From = "User <123@123.com>";
			newEmail.MI_Subject = $"XXXXXX {number} XXXXXXXX";
			newEmail.MI_Body = "Test";

			var resultWithinDuration = (int)getRepliedEmailsCountInPeriod.Invoke(null, [newEmail, application, number]);
			AssertEquals(3, resultWithinDuration);
		}

		public void TestIsReceivedEmailsCountOutOfLimit()
		{
			SetupRegistry();
			var number = "CS00001234";
			var processor = new EDIEmailPreocessorForLoopTest();
			var incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.IM_IncidentNumber = number;

			Factory.Save();
			Env.OutgoingMailManager.EmailsCreated.Clear();

			for (var i = 0; i < 4; i++)
			{
				var email = Factory.New<MailItem>();
				email.MI_Status = MailStatus.Processed;
				email.MI_Direction = MailDirection.Receive;
				email.MI_Application = processor.MailApplicationCode;
				email.MI_ReceivedDateTime = ZDateTime.UtcNow.AddMinutes(-2);
				email.MI_From = "User <123@123.com>";
				email.MI_Subject = $"{i}_XXXXXX {number} XXXXXXXX";
				email.MI_Body = $"Test{i}";
			}

			Factory.Save();

			var newEmail = Factory.New<MailItem>();
			newEmail.MI_Status = MailStatus.Processed;
			newEmail.MI_Direction = MailDirection.Receive;
			newEmail.MI_ReceivedDateTime = ZDateTime.UtcNow;
			newEmail.MI_Application = processor.MailApplicationCode;
			newEmail.MI_From = "User <123@123.com>";
			newEmail.MI_Subject = $"XXXXXX {number} XXXXXXXX";
			newEmail.MI_Body = "Test";

			using (EDIDataRegistry.Instance.EnableEmailLoopDetection.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				Assert(!processor.IsReceivedEmailsCountOutOfLimit(newEmail));
			}
			Factory.Save();

			var newEmail2 = Factory.New<MailItem>();
			newEmail2.MI_Status = MailStatus.Processed;
			newEmail2.MI_Direction = MailDirection.Receive;
			newEmail2.MI_ReceivedDateTime = ZDateTime.UtcNow;
			newEmail2.MI_Application = processor.MailApplicationCode;
			newEmail2.MI_From = "User <123@123.com>";
			newEmail2.MI_Subject = $"XXXXXX {number} XXXXXXXX";
			newEmail2.MI_Body = "Test";

			using (EDIDataRegistry.Instance.EnableEmailLoopDetection.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				Assert(processor.IsReceivedEmailsCountOutOfLimit(newEmail2));
			}

			using (EDIDataRegistry.Instance.EnableEmailLoopDetection.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				Assert("Should be false when registry is false", !processor.IsReceivedEmailsCountOutOfLimit(newEmail2));
			}
		}

		class EDIEmailPreocessorForLoopTest : EDIBusinessObjectEmailProcessor<SupportIncident>
		{
			public override string EmailTypeName => "LDT";

			public override string MailApplicationCode => "LDT";

			protected override string DocType => "LDT";

			protected override SupportIncident LoadFromIdentifier(BusinessObjectFactory factory, string identifier)
			{
				throw new NotImplementedException();
			}

			protected override bool ShouldAttachEmailAndSave(MailItem mailItem, SupportIncident bizO)
			{
				throw new NotImplementedException();
			}

			internal override string GetBusinessObjectIdentifierFromSubject(string subject)
			{
				return subject;
			}
		}
	}
}
