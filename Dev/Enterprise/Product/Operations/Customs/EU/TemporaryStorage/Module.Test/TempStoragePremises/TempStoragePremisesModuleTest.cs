using Enterprise.Customs.EU.TemporaryStorage.Module;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Module.TemporaryStorage.Testing
{
	[TestedType(typeof(TempStoragePremisesModule))]
	sealed class TempStoragePremisesModuleTest : ZModuleBasherTest
	{
		public void TestProperties()
		{
			using (var module = (TempStoragePremisesModule)GetModule())
			{
				CombineAssertions(() =>
				{
					AssertEquals("AllowNew", expected: true, module.AllowNew);
					AssertEquals("AllowEdit", expected: true, module.AllowEdit);
					AssertEquals("AllowDelete", expected: true, module.AllowDelete);
					AssertEquals("AllowView", expected: false, module.AllowView);
					AssertEquals("AllowUniversalCopy", expected: false, module.AllowUniversalCopy);
					AssertEquals("AllowToggleFilterVisibilityMenuItem", expected: true, module.AllowToggleFilterVisibilityMenuItem);
				});
			}
		}

		public void TestGetNewController()
		{
			using (var module = (TempStoragePremisesModule)GetModule())
			{
				AssertType<TempStoragePremisesController>(module.GetNewController());
			}
		}

		public void TestGetNewFilterBusinessObject()
		{
			using (var module = (TempStoragePremisesModule)GetModule())
			{
				AssertType<TempStoragePremisesFilterBusinessObject>(module.FilterBusinessObject);
			}
		}

		[RequiresSTA]
		public void TestGetNewFilterControl()
		{
			using (var module = (TempStoragePremisesModule)GetModule())
			{
				using (var filterControl = module.GetNewFilterControlForGrid())
				{
					AssertType<TempStoragePremisesFilterStripControl>(filterControl);
				}
			}
		}

		public void TestLicenceCheckPoint()
		{
			using (var module = (TempStoragePremisesModule)GetModule())
			{
				AssertEquals(Env.Licence.Core, module.LicenceCheckPoint);
			}
		}

		public void TestSecurityCheckpoint()
		{
			using (var module = (TempStoragePremisesModule)GetModule())
			{
				AssertEquals(Env.Security.EUTempStoragePremises, module.SecurityCheckpoint);
			}
		}

		protected override ModuleIdentifier GetModuleID() => ModuleIDs.Customs.EU.TempStoragePremises;

		protected override string CountryCode => Core.Constants.CountryCodes.Latvia;
	}
}
