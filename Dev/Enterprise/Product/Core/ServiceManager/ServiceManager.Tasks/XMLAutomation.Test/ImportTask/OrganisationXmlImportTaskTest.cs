using System;
using System.IO;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
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
	sealed class OrganisationXmlImportTaskTest : TestCaseWithFactory
	{
		public void TestEmailIsSentOnImportSuccess()
		{
			Factory.Save();

			var organisationsCount = Factory.GetDatabaseCount(typeof(OrgHeader));
			AssertEquals("Precondition: Saved Emails", 0, Env.OutgoingMailManager.EmailsCreated.Count);
			var testTempFile = Path.Combine(TestDirectory, "TempFile.xml");
			try
			{
				var task = new OrganisationXmlImportTask(SystemDataRegistry.Instance.OrganisationDataImportDirectory, new NotificationBuffer(new NotificationBuffer()), NotificationDataRegistry.Instance.OrganisationImportNotificationGroup);

				using (var resourceRetriever = new EmbeddedResourceRetriever())
				using (var sourceStream = resourceRetriever.GetStream("Enterprise.ServiceManager.Tasks.XMLAutomation.Test.Testing.Organisation.xml"))
				using (var file = File.Create(testTempFile))
				{
					sourceStream.CopyTo(file);
				}
				task.Run();

				AssertEquals("One new email should have been created", 1, Env.OutgoingMailManager.EmailsCreated.Count);
				var item = Env.OutgoingMailManager.EmailsCreated[0];
				AssertEquals(0, item.Attachments.Count);
				AssertContains("Email subject", task.TaskDescription + " Succeeded", item.Subject);
				AssertEquals(organisationsCount + 1, Factory.GetDatabaseCount(typeof(OrgHeader)));
			}
			finally
			{
				DeleteIfExists(testTempFile);
			}
		}

		public void TestEmailIsSentOnImportFailure()
		{
			Factory.Save();

			int organisationsCount = Factory.GetDatabaseCount(typeof(OrgHeader));

			AssertEquals("Precondition: Saved Emails", 0, Env.OutgoingMailManager.EmailsCreated.Count);
			try
			{
				using (StreamWriter writer = new StreamWriter(Path.Combine(TestDirectory, "TempFile.xml")))
				{
					writer.Write("Sample text");
				}

				OrganisationXmlImportTask task = new OrganisationXmlImportTask(SystemDataRegistry.Instance.OrganisationDataImportDirectory, new NotificationBuffer(new NotificationBuffer()), NotificationDataRegistry.Instance.OrganisationImportNotificationGroup);

				task.Run();

				AssertEquals("One new email should have been created", 1, Env.OutgoingMailManager.EmailsCreated.Count);
				EmailDef item = Env.OutgoingMailManager.EmailsCreated[0];
				AssertEquals(1, item.Attachments.Count);
				AssertContains("Email Subject", task.TaskDescription + " Failed", item.Subject);
				AssertEquals(organisationsCount, Factory.GetDatabaseCount(typeof(OrgHeader)));
			}
			finally
			{
				DeleteIfExists(TestDirectory + @"\TempFile.xml");
			}
		}

		public void TestImporterRefreshDisabled()
		{
			OrganisationXmlImportTaskForTesting task = new OrganisationXmlImportTaskForTesting(SystemDataRegistry.Instance.OrganisationDataImportDirectory, new NotificationBuffer(new NotificationBuffer()), NotificationDataRegistry.Instance.OrganisationImportNotificationGroup);
			BusinessObjectFactoryProvider provider = task.GetFactoryProvider_Exposed();
			Assert("RefreshEnabled should be off", !provider.Current.RefreshEnabled);
		}

		class OrganisationXmlImportTaskForTesting : OrganisationXmlImportTask
		{
			public OrganisationXmlImportTaskForTesting(StringRegistryItem registryPath, INotifications notify, GuidRegistryItem notificationGroup)
				: base(registryPath, notify, notificationGroup)
			{
			}

			public BusinessObjectFactoryProvider GetFactoryProvider_Exposed()
			{
				return base.GetFactoryProvider();
			}
		}

		string TestDirectory
		{
			get { return Env.TempPath; }
		}

		string TestingCountry
		{
			get { return Core.Constants.CountryCodes.Australia; }
		}

		protected override void SetUp()
		{
			base.SetUp();
			if (!string.IsNullOrEmpty(TestingCountry))
			{
				StoredCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
				GlbCompany.CurrentCompany.SetCountry(TestingCountry);
			}

			SystemDataRegistry.Instance.OrganisationDataImportDirectory.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, TestDirectory);

			NotificationDataRegistry.Instance.OrganisationImportNotificationGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Constants.Groups.AllPK);
			GlbGroup group = Factory.Load<GlbGroup>(Constants.Groups.AllPK);
			var staff = group.Staff.AddNew();
			staff.GS_EmailAddress = "test@test.com";
			staff.GS_Code = "ZAC";
		}
		string StoredCountry;

		protected override void TearDown()
		{
			if (!string.IsNullOrEmpty(StoredCountry) && StoredCountry != GlbCompany.CurrentCompany.GC_RN_NKCountryCode)
			{
				GlbCompany.CurrentCompany.SetCountry(StoredCountry);
			}

			base.TearDown();
		}
	}
}
