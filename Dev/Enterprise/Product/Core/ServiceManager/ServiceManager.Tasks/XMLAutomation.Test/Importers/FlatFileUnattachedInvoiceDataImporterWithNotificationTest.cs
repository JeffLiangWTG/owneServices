using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ServiceManager.Tasks.XMLAutomation.Testing
{
	sealed class FlatFileUnattachedInvoiceDataImporterWithNotificationTest : TestCaseWithFactory
	{
		public void TestImportSuccessSendsEmail()
		{
			NotificationDataRegistry.Instance.CommercialInvoiceImportNotificationGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Constants.Groups.AllPK);
			Factory.Save();

			AssertEquals("Precondition: Saved Emails", 0, Env.OutgoingMailManager.EmailsCreated.Count);
			var importer = new ImporterToTestSuccess(CommercialInvoiceCsvPath);

			importer.Import();

			AssertEquals("One new email should have been created", 1, Env.OutgoingMailManager.EmailsCreated.Count);

			EmailDef eMail = Env.OutgoingMailManager.EmailsCreated[0];
			Assert("Email Subject", eMail.Subject.Contains("Commercial Invoice Import Succeeded"));
			AssertEquals(0, eMail.Attachments.Count);
		}

		public void TestImportFailureSendsEmail()
		{
			NotificationDataRegistry.Instance.CommercialInvoiceImportNotificationGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Constants.Groups.AllPK);
			Factory.Save();

			AssertEquals("Precondition: Saved Emails", 0, Env.OutgoingMailManager.EmailsCreated.Count);

			var importer = new ImporterToTestFailure(CommercialInvoiceCsvPath);

			importer.Import();

			AssertEquals("One new email should have been created", 1, Env.OutgoingMailManager.EmailsCreated.Count);

			EmailDef eMail = Env.OutgoingMailManager.EmailsCreated[0];
			Assert("Email Subject", eMail.Subject.Contains("Commercial Invoice Import Failed"));
			AssertEquals(1, eMail.Attachments.Count);
		}

		protected override void SetUp()
		{
			base.SetUp();

			GlbGroup group = Factory.Load<GlbGroup>(Constants.Groups.AllPK);
			GlbStaff member = group.Staff.AddNew();
			member.GS_EmailAddress = "test@test.com";
			member.GS_Code = "ZAC";
		}

		protected override void TearDown()
		{
			base.TearDown();
			if (resourceRetriever.IsValueCreated)
			{
				resourceRetriever.Value.Dispose();
			}
		}

		readonly Lazy<EmbeddedResourceRetriever> resourceRetriever = new Lazy<EmbeddedResourceRetriever>(() => new EmbeddedResourceRetriever());

		string commercialInvoiceCsvPath;
		string CommercialInvoiceCsvPath
		{
			get
			{
				if (string.IsNullOrEmpty(commercialInvoiceCsvPath))
				{
					commercialInvoiceCsvPath = resourceRetriever.Value.SaveResourceToFile("Enterprise.ServiceManager.Tasks.XMLAutomation.Test.Testing.CommercialInvoice.csv", "CommercialInvoice.csv");
				}
				return commercialInvoiceCsvPath;
			}
		}

		sealed class ImporterToTestSuccess : FlatFileUnattachedInvoiceDataImporterWithNotification
		{
			public ImporterToTestSuccess(string fileName)
				: base(new NotificationBuffer(), Core.Constants.Groups.AllPK, fileName)
			{
			}

			public override void Import()
			{
				AfterImport(true);
			}
		}

		sealed class ImporterToTestFailure : FlatFileUnattachedInvoiceDataImporterWithNotification
		{
			public ImporterToTestFailure(string fileName)
				: base(new NotificationBuffer(), Core.Constants.Groups.AllPK, fileName)
			{
			}

			public override void Import()
			{
				AfterImport(false);
			}
		}
	}
}
