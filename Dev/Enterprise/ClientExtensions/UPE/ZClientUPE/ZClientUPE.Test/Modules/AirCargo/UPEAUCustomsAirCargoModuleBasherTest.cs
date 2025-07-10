using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Client.UPE.Module.Testing
{
	[TestedType(typeof(UPEAirCargoModule))]
	internal class UPEAUCustomsAirCargoModuleBasherTest : ZModuleBasherTest
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
				return "AU";
			}
		}

		protected override ModuleIdentifier GetModuleID()
		{
			return ClientModuleRegistration.HouseAirCargo;
		}
	}
}
