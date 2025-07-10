using CargoWise.Types;
using Enterprise.Customs.ES.Business.CusTempStorage;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ES.TemporaryStorage.Module.Testing
{
	[TestedType(typeof(TemporaryStorageRegisterModule))]
	class TemporaryStorageRegisterModuleTest : ZModuleBasherTest
	{
		[RequiresSTA]
		public override void TestModuleShowsAndCanSearch()
		{
			var header = Factory.NewWithValidTestData<CusTempStorageRegHeader>();
			header.SRH_AppCode = "ADT";
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
				AssertEquals("AllowEdit", expected: true, module.AllowEdit);
				AssertEquals("AllowDelete", expected: false, module.AllowDelete);
				AssertEquals("AllowView", expected: true, module.AllowView);
				AssertEquals("AllowUniversalCopy", expected: false, module.AllowUniversalCopy);
				AssertEquals("SecurtiyCheckPoint", Env.Security.CustomsTemporaryStorage, module.SecurityCheckpoint);
			});
		}

		public void TestGetNewController() => AssertType<TemporaryStorageRegisterController>(module.GetNewController());

		public void TestGetNewFilterBusinessObject() => AssertType<TemporaryStorageRegisterFilterBusinessObject>(module.FilterBusinessObject);

		public void TesGetNewFilterControl() => AssertType<TemporaryStorageRegisterFilterStripControl>(module.GetNewFilterControlForGrid());

		public void TestGetNewGridCollection() => AssertType<EFTA.TemporaryStorageRegister.Business.CusTempStorageRegHeaderCollection<CusTempStorageRegHeader>>(module.GetNewBusinessObjectCollection());

		protected override ModuleIdentifier GetModuleID() => ModuleIDs.Customs.EU.ES.TemporaryStorageRegister;

		protected override string CountryCode => Core.Constants.CountryCodes.Spain;

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
