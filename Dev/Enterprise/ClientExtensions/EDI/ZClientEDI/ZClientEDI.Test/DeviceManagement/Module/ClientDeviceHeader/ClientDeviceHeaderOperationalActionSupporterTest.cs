using Enterprise.Client.EDI.Modules;
using Enterprise.Security;
using Enterprise.Services.OperationalActions.Support.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Client.EDI.DeviceManagement.Module.Testing
{
	[TestedType(typeof(ClientDeviceHeaderOperationalActionSupporter))]
	sealed class ClientDeviceHeaderOperationalActionSupporterTest : OperationalActionSupporterTest<ClientDeviceHeaderOperationalActionSupporter>
	{
		protected override SecurityCheckpoint ExpectedCustomizationSecurityCheckpoint => EDISecurityCheckpoints.DevicesOperationalActionsCustomization;
		protected override SecurityCheckpoint ExpectedRunSecurityCheckpoint => EDISecurityCheckpoints.DevicesOperationalActionsExecution;
		protected override ModuleIdentifier ModuleID => ClientModuleRegistration.ClientDevice;
		public override bool ShouldSupportDocuments => false;
	}
}
