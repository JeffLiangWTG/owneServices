using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.ZArchitecture.Modules.Testing
{
	public class ZModuleTest : TestCaseWithFactory
	{
		public void TestLicenceCheckPointOverride()
		{
			using (var module = new DummyModule())
			{
				AssertEquals((LicenceCheckpoint)EnvProxy.Instance.Licence.Core, module.LicenceCheckPoint);
				module.LicenceCheckPointOverride = (LicenceCheckpoint)EnvProxy.Instance.Licence.Booking;
				AssertEquals((LicenceCheckpoint)EnvProxy.Instance.Licence.Booking, module.LicenceCheckPoint);
			}
		}

		public void TestZModule()
		{
			var moduleInfo = new ModuleList()[DummyModuleIDs.Dummy, Enterprise.Core.Constants.CountryCodes._TemplateCountryName_];
			AssertNotEquals("PreCondition", Enterprise.Core.Constants.CountryCodes._TemplateCountryName_, EnvProxy.Instance.CurrentCompany.Country.Code);
			using (var module = new DummyModule())
			{
				AssertEquals("Name", DummyModuleIDs.Dummy.Description, module.Description);
				AssertNotEquals("Name", moduleInfo.Description, module.Description);
				AssertEquals("ID", DummyModuleIDs.Dummy, module.ID);
				AssertNull("BusinessContexts", module.BusinessContexts);
			}

			StaticCurrentFetcher.Instance.CurrentCompany.SetCountry(Enterprise.Core.Constants.CountryCodes._TemplateCountryName_);
			using (var module = new DummyModule())
			{
				AssertEquals("Name", moduleInfo.Description, module.Description);
				AssertEquals("ID", DummyModuleIDs.Dummy, module.ID);
				AssertNull("BusinessContexts", module.BusinessContexts);
			}
		}

		public void TestIsDisposed()
		{
			DummyModule module;
			using (module = new DummyModule())
			{
				AssertEquals("IsDisposed false when not yet disposed", false, module.IsDisposed);
			}
			AssertEquals("IsDisposed true when disposed", true, module.IsDisposed);
		}

		public void TestDefaultSecurityCheckpointForPopups()
		{
			using (var module = new DummyModule())
			{
				AssertNotNull(module.SecurityCheckpoint);
				var moduleAccess = Env.Security.FindOrCreateAccessModuleCheckPoint(module.SecurityCheckpoint);
				AssertEquals(moduleAccess, module.GetSecurityCheckpointForPopups()[0]);
			}
		}

		public void TestSetReadOnlyModule()
		{
			using (var module = new DummyModule())
			{
				CombineAssertions("Precondition: Default values are set to true", () =>
				{
					Assert(module.AllowNew);
					Assert(module.AllowEdit);
					Assert(module.AllowDelete);
				});

				((IZModuleInternals)module).SetReadOnly(true);

				CombineAssertions("AllowNew, AllowEdit, AllowDelete disabled when module should be read only", () =>
				{
					Assert(!module.AllowNew);
					Assert(!module.AllowEdit);
					Assert(!module.AllowDelete);
				});
			}
		}
	}
}
