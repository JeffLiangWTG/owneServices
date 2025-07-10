using CargoWise.Types;
using Enterprise.Customs.EFTA.TemporaryStorageRegister.Business;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;
using CusTempStorageRegHeader = Enterprise.Customs.EU.TemporaryStorage.Business.CusTempStorageRegHeader;

namespace Enterprise.Customs.DE.Module.Testing
{
	[TestedType(typeof(SumARegisterReadOnlyModule))]
	public class SumARegisterReadOnlyModuleTest : ZModuleBasherTest
	{
		public override void TestModuleShowsAndCanSearch()
		{
			var header = Factory.NewWithValidTestData<CusTempStorageRegHeader>();
			header.SRH_AppCode = "SUM";
			header.SRH_Reference = "Reference";
			header.SRH_SystemCreateTimeUtc = ZDateTime.Now;
			Factory.Save();
			base.TestModuleShowsAndCanSearch();
		}

		public void TestProperties()
		{
			CombineAssertions(() =>
			{
				AssertEquals("AllowNew", expected: false, module.AllowNew);
				AssertEquals("AllowEdit", expected: false, module.AllowEdit);
				AssertEquals("AllowDelete", expected: false, module.AllowDelete);
				AssertEquals("AllowView", expected: true, module.AllowView);
				AssertEquals("AllowUniversalCopy", expected: false, module.AllowUniversalCopy);
				AssertEquals("SecurtiyCheckPoint", Env.Security.CustomsTemporaryStorage, module.SecurityCheckpoint);
			});
		}

		public void TestGetNewController() => AssertType<SumARegisterReadOnlyController>(module.GetNewController());

		public void TestGetNewFilterBusinessObject() => AssertType<SumARegisterFilterBusinessObject>(module.FilterBusinessObject);

		public void TesGetNewFilterControl() => AssertType<SumARegisterFilterStripControl>(module.GetNewFilterControlForGrid());

		public void TestGetNewGridCollection() => AssertType<CusTempStorageRegHeaderCollection<Business.CusTempStorage.CusTempStorageRegHeader>>(module.GetNewBusinessObjectCollection());

		protected override ModuleIdentifier GetModuleID() => ModuleIDs.Customs.EU.DE.SumARegisterReadOnly;

		protected override string CountryCode => Core.Constants.CountryCodes.Germany;

		protected override void SetUp()
		{
			base.SetUp();
			module = GetModule();
		}

		protected override void TearDown()
		{
			base.TearDown();
			module.Dispose();
		}

		ZFilterModule module;
	}
}
