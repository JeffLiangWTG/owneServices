using Enterprise.Customs.EU.TemporaryStorage.Business;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.TemporaryStorage.Module.Testing
{
	[TestedType(typeof(TempStorageRegisterLinesModule))]
	class TempStorageRegisterLinesModuleTest : ZModuleBasherTest
	{
		public void TestModuleType()
		{
			using var module = GetModule();
			AssertType<TempStorageRegisterLinesModule>(module);
		}

		public void TestGetNewController()
		{
			using (var module = (TempStorageRegisterLinesModule)GetModule())
			{
				AssertNull(module.GetNewController());
			}
		}

		public void TestGetNewFilterBusinessObject()
		{
			using (var module = (TempStorageRegisterLinesModule)GetModule())
			{
				AssertType<TempStorageRegisterLinesFilterBusinessObject>(module.FilterBusinessObject);
			}
		}

		[RequiresSTA]
		public void TestGetNewFilterControl()
		{
			using (var module = (TempStorageRegisterLinesModule)GetModule())
			{
				using (var filterControl = module.GetNewFilterControlForGrid())
				{
					AssertType<TempStorageRegLinesFilterStripControl>(filterControl);
				}
			}
		}

		public void TestGetNewGridCollection()
		{
			using (var module = (TempStorageRegisterLinesModule)GetModule())
			{
				var collection = module.GridCollection;
				AssertType<CusTempStorageRegLineSelCollection>("Type", collection);
			}
		}

		public void TestLicenceCheckPoint()
		{
			using (var module = (TempStorageRegisterLinesModule)GetModule())
			{
				AssertEquals(Env.Licence.Core, module.LicenceCheckPoint);
			}
		}

		public void TestSecurityCheckpoint()
		{
			using (var module = (TempStorageRegisterLinesModule)GetModule())
			{
				AssertEquals(Env.Security.EUTempStorageRegister, module.SecurityCheckpoint);
			}
		}

		public void TestAllowNew()
		{
			using (var module = (TempStorageRegisterLinesModule)GetModule())
			{
				AssertEquals("User should not be able to create new register, the system does instead", false, module.AllowNew);
			}
		}

		protected override ModuleIdentifier GetModuleID() => ModuleIDs.Customs.EU.TempStorageRegisterLines;

		protected override string CountryCode => Core.Constants.CountryCodes.Latvia;

		protected override bool HasController() => false;
	}
}
