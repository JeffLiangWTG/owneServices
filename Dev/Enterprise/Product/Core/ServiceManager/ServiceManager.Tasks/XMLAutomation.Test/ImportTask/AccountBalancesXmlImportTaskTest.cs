using System;
using System.IO;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Journal;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.ServiceManager.Tasks.XMLAutomation.Testing
{
	sealed class AccountBalancesXmlImportTaskTest : TestCaseWithFactory
	{
		[TestDate(2012, 04, 05)]
		public void TestEmailIsSentOnImportSuccess()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "POLFRASYD";

			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_OH_OrgProxy = org.PK;

			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_Code = "ZZZ";
			branch.GB_GC = company.PK;

			var companyDataForCurrentOrg = org.GetCompanyDataForGlbCompany(company);
			companyDataForCurrentOrg.OB_IsDebtor = true;
			companyDataForCurrentOrg.OB_IsCreditor = true;

			Factory.Save();

			var periodTestHelper = new AccountingPeriodTestHelper();
			periodTestHelper.PostPeriodsForEntireYear(ZDateTime.Now.Year, company.PK);

			Factory.Save();

			var journal = Factory.LoadTop1<AccTransactionHeader>(new ZQuery());
			AssertNull("Precondition: expected no journals to be saved by default", journal);
			AssertEquals("Precondition: Saved Emails", 0, Env.OutgoingMailManager.EmailsCreated.Count);
			var testTempFile = Path.Combine(TestDirectory, "TempFile.xml");
			try
			{
				var buffer = new NotificationBuffer(new NotificationBuffer());
				var task = new AccountBalancesXmlImportTask(SystemDataRegistry.Instance.ARAPBalancesUpdateImportDirectoryItem,
					buffer,
					NotificationDataRegistry.Instance.UpdateAPARAccountBalancesProcessNotificationGroupItem);

				using (var resourceRetriever = new EmbeddedResourceRetriever())
				using (var stream = resourceRetriever.GetStream("Enterprise.ServiceManager.Tasks.XMLAutomation.Test.Testing.Balance.xml"))
				{
					using (var file = File.Create(testTempFile))
					{
						stream.CopyTo(file);
					}
					task.Run();

					Assert("Expected no error in the buffer", !buffer.ContainsNotificationType(ErrorType.Error));
					AssertEquals("One new email should have been created", 1, Env.OutgoingMailManager.EmailsCreated.Count);

					var item = Env.OutgoingMailManager.EmailsCreated[0];
					AssertEquals(0, item.Attachments.Count);
					Assert("Email subject", item.Subject.Contains("Account Balances Update Import Succeeded"));

					journal = Factory.LoadTop1<AccTransactionHeader>(new ZQuery());
					AssertNotNull("Expected journal to be created from file", journal);
					AssertEquals(journal.AH_Ledger, "AP");
				}
			}
			finally
			{
				DeleteIfExists(testTempFile);
			}
		}

		public void TestEmailIsSentOnImportFailure()
		{
			Factory.Save();

			int organisationsCount = Factory.GetDatabaseCount(typeof(Journal));

			AssertEquals("Precondition: Saved Emails", 0, Env.OutgoingMailManager.EmailsCreated.Count);
			try
			{
				using (StreamWriter writer = new StreamWriter(Path.Combine(TestDirectory, "TempFile.xml")))
				{
					writer.Write("Sample text");
				}

				AccountBalancesXmlImportTask task = new AccountBalancesXmlImportTask(SystemDataRegistry.Instance.ARAPBalancesUpdateImportDirectoryItem, new NotificationBuffer(new NotificationBuffer()), NotificationDataRegistry.Instance.UpdateAPARAccountBalancesProcessNotificationGroupItem);

				task.Run();

				AssertEquals("One new email should have been created", 1, Env.OutgoingMailManager.EmailsCreated.Count);
				EmailDef item = Env.OutgoingMailManager.EmailsCreated[0];
				AssertEquals(1, item.Attachments.Count);
				Assert("Email Subject", item.Subject.Contains("Account Balances Update Import Failed"));
				AssertEquals(organisationsCount, Factory.GetDatabaseCount(typeof(Journal)));
			}
			finally
			{
				DeleteIfExists(TestDirectory + @"\TempFile.xml");
			}
		}

		string TestDirectory
		{
			get { return Env.TempPath; }
		}

		protected override void SetUp()
		{
			base.SetUp();

			SystemDataRegistry.Instance.ARAPBalancesUpdateImportDirectoryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, TestDirectory);
			SystemDataRegistry.Instance.OrganisationMatchnigTypeItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "ENT");
			NotificationDataRegistry.Instance.UpdateAPARAccountBalancesProcessNotificationGroupItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Constants.Groups.AllPK);
			GlbGroup group = Factory.Load<GlbGroup>(Constants.Groups.AllPK);
			var staff = group.Staff.AddNew();
			staff.GS_EmailAddress = "test@test.com";
			staff.GS_Code = "TST";
		}
	}
}
