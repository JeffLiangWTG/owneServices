using Enterprise.Customs.AU.Module.AirCargo.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Client.UPE.Module.Testing
{
	[TestedType(typeof(UPEAirCargoConsolReportingModule))]
	sealed class UPEAirCargoConsolReportingModuleTest : AUCustomsAirCargoModuleTest
	{
		protected override ModuleIdentifier GetModuleID() => ClientModuleRegistration.AirCargo;

		protected override void SetUp()
		{
			UPEDataRegistry.Instance.EnableUPECustomisations = true;
			base.SetUp();
		}
	}
}
