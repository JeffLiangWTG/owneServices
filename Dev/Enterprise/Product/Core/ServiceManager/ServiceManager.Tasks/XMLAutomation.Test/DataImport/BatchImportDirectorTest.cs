using System;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Billing.Integration;
using Enterprise.DataTransfer.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ServiceManager.Tasks.XMLAutomation
{
	sealed class BatchImportDirectorTest : TestCaseWithFactory
	{
		public void TestTasksListCount()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Australia);
			BatchImportDirector director = new BatchImportDirector(null);
			AssertEquals("There should be 23 tasks to be performed.", 23, director.ImportTasks.Count);

			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Albania);
			director = new BatchImportDirector(null);
			AssertEquals("There should be 21 tasks to be performed.", 21, director.ImportTasks.Count);

			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Germany);
			director = new BatchImportDirector(null);
			AssertEquals("There should be 23 tasks to be performed.", 21, director.ImportTasks.Count);
		}

		public void TestEligibleForProductImport()
		{
			BatchImportDirector director = new BatchImportDirector(null);
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Australia);
			AssertEquals("Company is eligible for product import", true, director.EligibleForCSVProductImport);

			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Angola);
			director = new BatchImportDirector(null);
			AssertEquals("Company is not eligible for product import", false, director.EligibleForCSVProductImport);

			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.NewZealand);
			director = new BatchImportDirector(null);
			AssertEquals("Company is eligible for product import", true, director.EligibleForCSVProductImport);

			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.UnitedStates);
			director = new BatchImportDirector(null);
			AssertEquals("Company is eligible for product import", true, director.EligibleForCSVProductImport);
		}

		public void TestBindUserContextChangingEvent()
		{
			var director = new BatchImportDirector(null);
			director.ImportTasks.Clear();
			director.ImportTasks.Add(new TestXmlImportTask(SystemDataRegistry.Instance.ShipmentDataImportDirectory, null, NotificationDataRegistry.Instance.ConsolShipmentImportNotificationGroup));
			director.Run();
			AssertEquals("Have user context changed logs", true, director.UserContextChangedLogsForTest.Any(log => log.StartsWith("User Context is changing, old branch is")));
		}

		public void TestUnbindUserContextChangingEventWithException()
		{
			var director = new BatchImportDirector(null);
			director.ImportTasks.Clear();
			director.ImportTasks.Add(new TestXmlImportTaskWithException(SystemDataRegistry.Instance.ShipmentDataImportDirectory, null, NotificationDataRegistry.Instance.ConsolShipmentImportNotificationGroup));
			AssertExceptionThrown<Exception>(() => { director.Run(); });

			var factory = new BusinessObjectFactory();
			var homeCompany = factory.NewWithValidTestData<GlbCompany>();
			homeCompany.CompanyName = "homeCompany";
			homeCompany.GC_Code = "HMC";
			var homeBranch = factory.NewWithValidTestData<GlbBranch>();
			homeBranch.GB_BranchName = "homeBranch";
			homeBranch.GB_Code = "HMB";
			homeBranch.GB_GC = homeCompany.PK;
			var staff = factory.NewWithValidTestData<GlbStaff>();
			staff.GS_FullName = "testStaff";
			staff.GS_Code = "STF";
			staff.GS_GB_HomeBranch = homeBranch.PK;

			using (Env.SetTemporaryUserContext(staff.PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentDepartmentPK))
			{ }

			AssertEquals("No user context changed logs", false, director.UserContextChangedLogsForTest.Any());
		}

		class TestXmlImportTask : XmlImportTask
		{
			public TestXmlImportTask(StringRegistryItem registryPath, INotifications notify, GuidRegistryItem notificationGroup)
				: base(registryPath, notify, notificationGroup, BillingInterfaceName.Test)
			{ }

			protected override void RunTask()
			{
				var factory = new BusinessObjectFactory();
				var homeCompany = factory.NewWithValidTestData<GlbCompany>();
				homeCompany.CompanyName = "homeCompany";
				homeCompany.GC_Code = "HMC";
				var homeBranch = factory.NewWithValidTestData<GlbBranch>();
				homeBranch.GB_BranchName = "homeBranch";
				homeBranch.GB_Code = "HMB";
				homeBranch.GB_GC = homeCompany.PK;
				var staff = factory.NewWithValidTestData<GlbStaff>();
				staff.GS_FullName = "testStaff";
				staff.GS_Code = "STF";
				staff.GS_GB_HomeBranch = homeBranch.PK;

				using (Env.SetTemporaryUserContext(staff.PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentDepartmentPK))
				{ }
			}

			protected override DataImporter NewImporter()
			{
				return new ShipmentDataImporter();
			}
		}

		class TestXmlImportTaskWithException : TestXmlImportTask
		{
			public TestXmlImportTaskWithException(StringRegistryItem registryPath, INotifications notify, GuidRegistryItem notificationGroup)
				: base(registryPath, notify, notificationGroup)
			{ }

			protected override void RunTask()
			{
				throw new Exception();
			}
		}
	}
}
