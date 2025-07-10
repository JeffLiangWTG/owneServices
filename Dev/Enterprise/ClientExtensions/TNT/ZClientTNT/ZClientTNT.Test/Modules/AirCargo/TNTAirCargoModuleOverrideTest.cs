using Enterprise.Customs.AU.Module.AirCargo;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Client.TNT.AirCargo.Testing
{
	[TestedType(typeof(TNTAirCargoModuleOverride))]
	public class TNTAirCargoModuleOverrideTest : ZModuleBasherTest
	{
		public void TestPOImportMenuItem()
		{
			using (AUCustomsAirCargoModule module = (AUCustomsAirCargoModule)ZModuleFactory.Instance.Create(ModuleIDs.Customs.AU.AirCargo))
			{
				MenuAssertion.AssertHasMenu("Should find the Import IQDown tool bar button", module.FormActionMenu, "&Actions", "D&ata Transfer", "Import I&QDown");
				MenuAssertion.AssertHasMenu("Should find the Import XXX tool bar button", module.FormActionMenu, "&Actions", "D&ata Transfer", "Import &XXX");
			}
		}

		protected override string CountryCode
		{
			get
			{
				return "AU";
			}
		}

		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.Customs.AU.AirCargo;
		}
	}
}
