using System;
using System.IO;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ServiceManager.Tasks.XMLAutomation.Testing
{
	sealed class XMLTaskTestCase : TestCaseWithFactory
	{
		public void TestIsEnvironmentDataValid()
		{
			Assert("IsEnvironmentDataValid true by default", new XMLTaskForTest(null).IsEnvironmentDataValid);
		}

		public void TestSendEmailToNotificationGroup()
		{
			NotificationBuffer buffer = new NotificationBuffer();
			XMLTaskForTest task = new XMLTaskForTest(buffer);

			using (TempFile file = TempFile.New())
			{
				group.Staff[0].GS_EmailAddress = "";
				Factory.Save();

				Assert(task.SendEmailToGroup(group.PK.ToGuid(), new FileInfo(file.Filename), "Test Message"));
				AssertEquals("One email should have been created", 1, Env.OutgoingMailManager.EmailsCreated.Count);

				EmailDef email = Env.OutgoingMailManager.EmailsCreated[0];
				AssertEquals("Subject", "Test - " + Path.GetFileName(file.Filename), email.Subject);
				Assert("There are no attachments", email.Attachments.Count > 0);
				AssertEquals("Wrong attachment", Path.GetFileName(file.Filename), email.Attachments[0].DisplayName);
				Assert(email.Recipients.Contains("postmaster@email.com"));

				group.Staff[0].GS_EmailAddress = "email@email.com";
				Factory.Save();

				Env.OutgoingMailManager.EmailsCreated.Clear();
				using (FileStream fs = File.Open(file.Filename, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
				{
					Assert(!task.SendEmailToGroup(group.PK.ToGuid(), new FileInfo(file.Filename), "Test Message"));
					AssertEquals(0, Env.OutgoingMailManager.EmailsCreated.Count);
					AssertContains("The file is locked.", buffer.AsString);
				}
				Assert(task.SendEmailToGroup(group.PK.ToGuid(), new FileInfo(file.Filename), "Test Message"));
				AssertEquals(1, Env.OutgoingMailManager.EmailsCreated.Count);
				Env.OutgoingMailManager.EmailsCreated.Clear();

				task.ThrowExceptionOnEmailCreate = true;
				Assert(!task.SendEmailToGroup(group.PK.ToGuid(), new FileInfo(file.Filename), "Test Message"));
				AssertEquals(0, Env.OutgoingMailManager.EmailsCreated.Count);
				AssertContains("test error message", buffer.AsString);
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			GlbGroup postmasterGroup = Factory.Load<GlbGroup>(Core.Constants.Groups.PostMastersGroupPK);
			GlbStaff postmasterStaff = Factory.NewWithValidTestData<GlbStaff>();
			postmasterGroup.Staff.Add(postmasterStaff);
			postmasterStaff.GS_Code = "ZAC";
			postmasterStaff.GS_FullName = "Potmaster";
			postmasterStaff.GS_EmailAddress = "postmaster@email.com";

			group = Factory.NewWithValidTestData<GlbGroup>();
			GlbStaff staff = group.Staff.AddNew();
			staff.GS_FullName = "Name";
			staff.GS_Code = "ZA2";
			staff.GS_EmailAddress = "email@email.com";

			Factory.Save();
		}

		GlbGroup group;

		class XMLTaskForTest : XMLTask
		{
			public XMLTaskForTest(INotifications notify) : base(notify) { }

			public new bool IsEnvironmentDataValid
			{
				get { return base.IsEnvironmentDataValid(); }
			}

			public bool SendEmailToGroup(Guid notificationGroup, FileInfo dataFile, ZString message)
			{
				return base.SendEmailToNotificationGroup(notificationGroup, Env.Registry.RawRegistry.NotificationGroup, dataFile, message);
			}

			protected override string NotificationEmailSubject
			{
				get { return "Test"; }
			}

			protected override void RunTask()
			{
			}

			protected override EmailDef CreateEmailDef(ZString message, FileInfo dataFile)
			{
				if (ThrowExceptionOnEmailCreate)
				{
					throw new ArgumentException("test error message");
				}

				return base.CreateEmailDef(message, dataFile);
			}

			public override ZString UniqueIdentifier
			{
				get
				{
					return new ZString("A not very unique UniqueIdentifier...");
				}
			}

			public bool ThrowExceptionOnEmailCreate { get; set; }
		}
	}
}
