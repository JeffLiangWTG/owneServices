using System.Collections.Specialized;
using System.IO;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MailManager;
using Enterprise.MailManager.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.JAS.Business.Testing
{
	public class JASMailSenderTest : TestCaseWithFactory
	{
		public void TestMailManagerEmailSender()
		{
			AssertEquals(typeof(OutgoingMailCreator), MailSender.BaseOutgoingMailManager.GetType());
		}

		public void TestSendEmail()
		{
			GlbGroup group = Factory.New<GlbGroup>();
			GlbStaff newStaff = Factory.New<GlbStaff>();
			newStaff.GS_EmailAddress = "test@test.edi.com.au";
			newStaff.Groups.Add(group);
			newStaff.GS_Code = "ZAC";
			Factory.Save();
			StringCollection emails = new EmailGroupUtility().GetGroupEmailCollection(group.PK.ToGuid(), false);
			AssertEquals("Pre-condition", 1, emails.Count);
			AssertEquals("Pre-condition", "test@test.edi.com.au", emails[0]);
			MailSender.SendEmail(group.PK, Env.Registry.RawRegistry.NotificationGroup, "Test", "this is the body", null);
			AssertNotNull(MailSender.TestOutgoingMailCreator.LastEmailDefSent);
			AssertEquals("this is the body", MailSender.TestOutgoingMailCreator.LastEmailDefSent.Body);
			AssertEquals("Test", MailSender.TestOutgoingMailCreator.LastEmailDefSent.Subject);
			AssertEquals(1, MailSender.TestOutgoingMailCreator.LastEmailDefSent.Recipients.Count);
			Assert(MailSender.TestOutgoingMailCreator.LastEmailDefSent.Recipients.Contains("test@test.edi.com.au"));
			AssertEquals(0, MailSender.TestOutgoingMailCreator.LastEmailDefSent.Attachments.Count);
			string filePath = Path.Combine(Env.TempPath, "test12345file.txt");
			try
			{
				using (FileStream testFile = File.Create(filePath))
				{
					byte[] expectedData = new byte[] { 12, 99, 0, 32 };
					testFile.Write(expectedData, 0, expectedData.Length);
					testFile.Close();
					MailSender.SendEmail(group.PK, Env.Registry.RawRegistry.NotificationGroup, "Test", "this is the body", filePath);
					AssertEquals(1, MailSender.TestOutgoingMailCreator.LastEmailDefSent.Attachments.Count);
					AssertEquals(expectedData, MailSender.TestOutgoingMailCreator.LastEmailDefSent.Attachments[0].Data);
				}
			}
			finally
			{
				File.Delete(filePath);
			}
		}

		public void TestSendEmail_NotificationGroupPKNotSpecified()
		{
			MailSender.SendEmail(ZGuid.Empty, Env.Registry.RawRegistry.NotificationGroup, "Test", "this is the body", null);
			AssertNull("no email if no recipients", MailSender.TestOutgoingMailCreator.LastEmailDefSent);
		}

		#region Implementation
		JASMailSenderForTest MailSender
		{
			get
			{
				if (fMailSender == null)
				{
					fMailSender = new JASMailSenderForTest();
				}

				return fMailSender;
			}
		}

		JASMailSenderForTest fMailSender;
		class JASMailSenderForTest : JASMailSender
		{
			public IOutgoingMailManager BaseOutgoingMailManager
			{
				get
				{
					return base.OutgoingMailManager;
				}
			}

			protected override IOutgoingMailManager OutgoingMailManager
			{
				get
				{
					if (fOutgoingMailManager == null)
					{
						fOutgoingMailManager = new OutgoingMailCreatorForTest();
					}

					return fOutgoingMailManager;
				}
			}

			public OutgoingMailCreatorForTest TestOutgoingMailCreator
			{
				get
				{
					return (OutgoingMailCreatorForTest)OutgoingMailManager;
				}
			}

			IOutgoingMailManager fOutgoingMailManager;
		}
		#endregion
	}
}
