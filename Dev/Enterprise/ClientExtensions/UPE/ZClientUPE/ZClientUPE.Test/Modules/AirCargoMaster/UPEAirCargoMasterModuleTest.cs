using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Client.UPE.Module.Testing
{
	[TestedType(typeof(UPEAirCargoMasterModule))]
	public class UPEAirCargoMasterModuleTest : ZModuleBasherTest
	{
		protected override void SetUp()
		{
			UPEDataRegistry.Instance.EnableUPECustomisations = true;
			base.SetUp();
		}

		protected override string CountryCode
		{
			get
			{
				return Core.Constants.CountryCodes.Australia;
			}
		}

		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.Customs.AU.AirCargo;
		}
	}
}
