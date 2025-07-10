using CargoWise.EntityFramework;
using Enterprise.Customs.IE.ExitControl.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.ExitControl.Module.Testing
{
	[TestedType(typeof(ExitControlReportModule))]
	sealed class ExitControlReportModuleTest : ZModuleBasherTest
	{
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

		protected override string CountryCode => Core.Constants.CountryCodes.Ireland;
	}
}
