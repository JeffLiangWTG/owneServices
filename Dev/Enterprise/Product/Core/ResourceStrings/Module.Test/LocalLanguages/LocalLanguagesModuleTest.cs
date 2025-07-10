using Enterprise.Environment;
using Enterprise.ResourceStrings.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.ResourceStrings.Module.Testing
{
	[TestedType(typeof(LocalLanguagesModule))]
	public class LocalLanguagesModuleTest : ZModuleBasherTest
	{
		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.LocalLanguages;
		}

		public void TestCheckpoints()
		{
			using (var module = new LocalLanguagesModule())
			{
				AssertEquals("SecurityCheckpoint", Env.Security.LocalLanguages, module.SecurityCheckpoint);
				AssertEquals("LicenseCheckPoint", Env.Licence.LocalLanguages, module.LicenceCheckPoint);
			}
		}

		#region Properties

		public void TestGetNewFilterControl()
		{
			using (var module = new LocalLanguagesModuleForTest())
			using (var filterControl = module.NewFilterControl)
			{
				Assert("Invalid type", filterControl is LocalLanguagesFilterControl);
			}
		}

		public void TestGetNewGridCollection()
		{
			using (var module = new LocalLanguagesModuleForTest())
			{
				var countriesCollection = module.NewGridCollection;
				Assert("Invalid type", countriesCollection is RefLocalLanguageCollection);
			}
		}

		public void TestGetNewFilterBusinessObject()
		{
			using (var module = new LocalLanguagesModuleForTest())
			{
				var filterBusinessObject = module.NewFilterBusinessObject;
				Assert("Invalid type", filterBusinessObject is LocalLanguagesFilterBusinessObject);
			}
		}

		public void TestAllowUniversalCopy()
		{
			using (var module = new LocalLanguagesModuleForTest())
			{
				Assert("Should not allow universalcopy", !module.AllowUniversalCopy);
			}
		}

		#endregion
	}
}
