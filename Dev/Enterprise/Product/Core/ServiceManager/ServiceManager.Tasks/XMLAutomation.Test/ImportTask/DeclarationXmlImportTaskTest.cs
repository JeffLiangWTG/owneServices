using System;
using System.IO;
using CargoWise.ComponentModel;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.ServiceManager.Tasks.XMLAutomation.Testing
{
	sealed class DeclarationXmlImportTaskTest : XmlImportTaskTest
	{
		public void TestNotificationEmailIsSendAfterDeclarationsImported()
		{
			var postMasterGroup = Factory.Load<GlbGroup>(Core.Constants.Groups.PostMastersGroupPK);
			postMasterGroup.Staff[0].GS_EmailAddress = "a@b.c";
			Factory.Save();

			var testDirectory = Env.TempPath;
			SystemDataRegistry.Instance.EmailNotificationForErrorsOnly.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			SystemDataRegistry.Instance.CustomsDeclarationsDataImportDirectory.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, testDirectory);
			NotificationDataRegistry.Instance.CustomsDeclarationImportNotificationGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, postMasterGroup.PK.ToGuid());

			AssertEquals("Precondition: Saved Emails", 0, Env.OutgoingMailManager.EmailsCreated.Count);
			var testTempFile = Path.Combine(testDirectory, "TempFile.xml");
			try
			{
				CreateTestFileFromEmbeddedResource("Enterprise.ServiceManager.Tasks.XMLAutomation.Test.Testing.Declaration.xml", testTempFile);
				var task = new DeclarationXmlImportTaskForTest();
				task.Run();
				AssertEquals("One new email should have been created", 1, Env.OutgoingMailManager.EmailsCreated.Count);
				var item = Env.OutgoingMailManager.EmailsCreated[0];
				AssertEquals(0, item.Attachments.Count);
				AssertContains("Email Subject", task.TaskDescription + " Succeeded", item.Subject);
				Assert("Email Body", item.Body.Contains("Declaration (Master Bill='Masterbill' House Bill='Housebill') created"));

				CreateTestFileFromEmbeddedResource("Enterprise.ServiceManager.Tasks.XMLAutomation.Test.Testing.Declaration.xml", testTempFile);
				task.Run();

				AssertEquals("One new email should have been created", 2, Env.OutgoingMailManager.EmailsCreated.Count);
				item = Env.OutgoingMailManager.EmailsCreated[1];
				AssertEquals(1, item.Attachments.Count);
				AssertContains("Email Subject", task.TaskDescription + " Failed", item.Subject);
				Assert("Email Body", item.Body.Contains("Declaration B00001000 update rejected, update is not allowed"));

				SystemDataRegistry.Instance.AllowCustomsDeclarationUpdateItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
				CreateTestFileFromEmbeddedResource("Enterprise.ServiceManager.Tasks.XMLAutomation.Test.Testing.Declaration.xml", testTempFile);
				task.Run();

				AssertEquals("One new email should have been created", 3, Env.OutgoingMailManager.EmailsCreated.Count);
				item = Env.OutgoingMailManager.EmailsCreated[2];
				AssertEquals(0, item.Attachments.Count);
				AssertContains("Email Subject", task.TaskDescription + " Succeeded", item.Subject);
				Assert("Email Body", item.Body.Contains("Declaration B00001000 updated"));

				SystemDataRegistry.Instance.EmailNotificationForErrorsOnly.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

				CreateTestFileFromEmbeddedResource("Enterprise.ServiceManager.Tasks.XMLAutomation.Test.Testing.Declaration.xml", testTempFile);
				task.Run();

				AssertEquals("No new email should have been created", 3, Env.OutgoingMailManager.EmailsCreated.Count);
			}
			finally
			{
				DeleteIfExists(testTempFile);
			}
		}

		#region Implementation

		class DeclarationXmlImportTaskForTest : DeclarationXmlImportTask
		{
			public DeclarationXmlImportTaskForTest()
				: base(new NotificationBuffer(new NotificationBuffer()))
			{
			}

			public INotifications NotifyForTest
			{
				get { return base.Notify; }
			}
		}

		#endregion
	}
}
