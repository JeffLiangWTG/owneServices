using Enterprise.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Manifest.H7.Module.Testing
{
	[TestedType(typeof(G3DeclarationModule))]
	sealed class G3DeclarationModuleTest : ZModuleBasherTest
	{
		public void TestModuleID()
		{
			using (var module = (G3DeclarationModule)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				AssertEquals(ModuleIDs.Customs.EU.ES.G3Declaration, module.ID);
			}
		}

		public void TestSecurityCheckPoint()
		{
			using (var module = (G3DeclarationModule)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				AssertEquals(Env.Security.G3Declaration, module.SecurityCheckpoint);
			}
		}

		public void TestLicenceCheckPointCore()
		{
			using (var module = (G3DeclarationModule)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				AssertEquals(Env.Licence.Core, module.LicenceCheckPoint);
			}
		}

		public void TestFilterControl()
		{
			using (var module = new G3DeclarationModuleForTest())
			using (var controlForTest = module.GetNewFilterControlForTest())
			{
				AssertType<G3DeclarationFilterControl>(controlForTest);
			}
		}

		public void TestFilterBusinessObject()
		{
			using (var module = new G3DeclarationModule())
			{
				AssertEquals(typeof(G3DeclarationFilterBusinessObject), module.FilterBusinessObject.GetType());
			}
		}

		public void TestNotAllowNew()
		{
			using (var module = new G3DeclarationModule())
			{
				AssertEquals(false, module.AllowNew);
			}
		}

		public void TestNotAllowEdit()
		{
			using (var module = new G3DeclarationModule())
			{
				AssertEquals(false, module.AllowEdit);
			}
		}

		protected override ModuleIdentifier GetModuleID() => ModuleIDs.Customs.EU.ES.G3Declaration;

		protected override string CountryCode => Core.Constants.CountryCodes.Spain;

		class G3DeclarationModuleForTest : G3DeclarationModule
		{
			public G3DeclarationFilterControl GetNewFilterControlForTest()
			{
				return (G3DeclarationFilterControl)GetNewFilterControl();
			}
		}
	}
}
