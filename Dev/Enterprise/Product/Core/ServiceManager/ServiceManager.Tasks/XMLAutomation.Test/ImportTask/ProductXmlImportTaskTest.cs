using System;
using System.IO;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.ServiceManager.Tasks.XMLAutomation.Testing
{
	sealed class ProductXmlImportTaskTest : XmlImportTaskTest
	{
		public void TestNotificationEmailIsSendAfterProductImported()
		{
			var postMasterGroup = Factory.Load<GlbGroup>(Core.Constants.Groups.PostMastersGroupPK);
			postMasterGroup.Staff[0].GS_EmailAddress = "a@b.c";
			Factory.Save();

			var testDirectory = Env.TempPath;
			SystemDataRegistry.Instance.EmailNotificationForErrorsOnly.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			SystemDataRegistry.Instance.ProductsXMLDataImportDirectory.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, testDirectory);
			NotificationDataRegistry.Instance.ProductImportNotificationGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, postMasterGroup.PK.ToGuid());

			AssertEquals("Precondition: Saved Emails", 0, Env.OutgoingMailManager.EmailsCreated.Count);
			var testTempFile = Path.Combine(testDirectory, "TempFile.xml");
			try
			{
				CreateTestFileFromEmbeddedResource("Enterprise.ServiceManager.Tasks.XMLAutomation.Test.Testing.ProductXML.xml", testTempFile);
				var task = new ProductXmlImportTaskForTest();
				task.Run();
				AssertEquals("One new email should have been created", 1, Env.OutgoingMailManager.EmailsCreated.Count);
				var item = Env.OutgoingMailManager.EmailsCreated[0];
				AssertEquals(0, item.Attachments.Count);
				AssertContains("Email Subject", Res.GetString("a41f3f48-a635-4d9b-8aea-54593582c3d5", "Product XML Import") + " Succeeded", item.Subject);
				AssertContains("Email Body", "Part PNH created", item.Body);

				SystemDataRegistry.Instance.UpdateProductsDuringAutomaticImport.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
				CreateTestFileFromEmbeddedResource("Enterprise.ServiceManager.Tasks.XMLAutomation.Test.Testing.ProductXML.xml", testTempFile);
				task.Run();

				AssertEquals("Another one new email should have been created", 2, Env.OutgoingMailManager.EmailsCreated.Count);
				item = Env.OutgoingMailManager.EmailsCreated[1];
				AssertContains("Email Body", "Part PNH updated", item.Body);
				var parts = Factory.Load<OrgSupplierPart>(new ZQuery());
				AssertEquals("One Product exists", 1, parts.Length);
			}
			finally
			{
				DeleteIfExists(testTempFile);
			}
		}

		#region Implementation

		class ProductXmlImportTaskForTest : ProductXmlImportTask
		{
			public ProductXmlImportTaskForTest()
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
