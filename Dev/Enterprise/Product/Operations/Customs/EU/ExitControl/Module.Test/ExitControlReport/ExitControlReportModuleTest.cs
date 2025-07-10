using CargoWise.EntityFramework;
using Enterprise.Customs.EU.ExitControl.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.ExitControl.Module.Testing
{
	[TestedType(typeof(ExitControlReportModule))]
	sealed class ExitControlReportModuleTest : ZModuleBasherTest
	{
		public void TestGetNewBusinessObjectCollectionFilterCurrentCompany()
		{
			var company1 = Factory.New<GlbCompany>();
			company1.GC_Code = "TS1";
			company1.GC_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			company1.GC_Name = "Test Company 1";

			var branch1 = company1.Branches.AddNew();
			branch1.GB_Code = "BR1";
			branch1.GB_RL_NKHomePort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			branch1.GB_BranchName = "Test Branch 1";

			var header1 = Factory.NewWithValidTestData<CusExitHeader>();
			header1.CXH_JobReference = header1.PK.ToString().Substring(0, 35);
			header1.CXH_GC_Company = company1.PK;
			header1.CXH_GB_Branch = branch1.PK;
			var consignment1 = header1.CusExitConsignments.AddNew();

			var report1 = Factory.New<CusExitReport>();
			report1.CER_CXC_Consignment = consignment1.PK;
			report1.CER_OfficeOfExit = "DE001";
			report1.CER_CXH_Header = header1.PK;

			var header2 = Factory.NewWithValidTestData<CusExitHeader>();
			header2.CXH_JobReference = header2.PK.ToString().Substring(0, 35);
			var consignment2 = header2.CusExitConsignments.AddNew();

			var report2 = Factory.New<CusExitReport>();
			report2.CER_CXC_Consignment = consignment2.PK;
			report2.CER_OfficeOfExit = "DE002";
			report2.CER_CXH_Header = header2.PK;

			Factory.Save();

			ZQuery filter;
			using (var module = GetModule())
			{
				filter = module.GetNewBusinessObjectCollection().CompleteFilter;
			}

			AssertEquals(false, report1.MatchesFilter(filter));
			AssertEquals(true, report2.MatchesFilter(filter));
		}

		public void TestGetNewController()
		{
			AssertType<ExitControlReportController>("Controller must be of type ExitControlReportController", module.GetNewController());
		}

		public void TestHasActions()
		{
			AssertEquals("HasActions must be true", true, module.HasActions);
		}

		public void TestGetNewFilterControl()
		{
			using (var filterControl = module.GetNewFilterControlForGrid())
			{
				AssertType<ExitControlReportFilterStripControl>("FilterControl Type", filterControl);
			}
		}

		public void TestAllowNew()
		{
			AssertEquals("AllowNew must be false", false, module.AllowNew);
		}

		public void TestAllowDelete()
		{
			AssertEquals("AllowDelete must be false", false, module.AllowDelete);
		}

		public void TestAllowCopyFilterGridHyperlinkToClipboard()
		{
			AssertEquals("module.AllowCopyFilterGridHyperlinkToClipboard", false, module.AllowCopyFilterGridHyperlinkToClipboard);
		}

		public void TestFilterBusinessObject()
		{
			AssertType<ExitControlReportFilterBusinessObject>("FilterBusinessObject Type", module.FilterBusinessObject);
		}

		protected override void AddTestObjects(IBusinessObjectCollection collection)
		{
			var header1 = Factory.NewWithValidTestData<CusExitHeader>();
			header1.CXH_JobReference = header1.PK.ToString().Substring(0, 35);

			var report1 = Factory.NewWithValidTestData<CusExitReport>();
			report1.CER_CXH_Header = header1.PK;

			Factory.Save();

			base.AddTestObjects(collection);
		}

		protected override void SetUp()
		{
			base.SetUp();
			module = new ExitControlReportModule();
		}

		protected override void TearDown()
		{
			module.Dispose();
			base.TearDown();
		}
		ExitControlReportModule module;

		protected override ModuleIdentifier GetModuleID() => ModuleIDs.Customs.EU.ExitControlReport;
	}
}
