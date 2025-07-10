using Enterprise.Security;
using Enterprise.Services.OperationalActions.Support.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Client.EDI.LicenceKeyBuilder.Module.Testing
{
	[TestedType(typeof(LicenceKeyOpAccSupporter))]
	internal sealed class LicenceKeyOpAccSupporterTest : OperationalActionSupporterTest<LicenceKeyOpAccSupporter>
	{
		protected override SecurityCheckpoint ExpectedCustomizationSecurityCheckpoint => EDISecurityCheckpoints.OrgLicenceModify;
		protected override SecurityCheckpoint ExpectedRunSecurityCheckpoint => EDISecurityCheckpoints.OrgLicenceModify;
		protected override ModuleIdentifier ModuleID => Modules.ClientModuleRegistration.LicenseKey;
		public override bool ShouldSupportDocuments => false;
		public new void TestSecurityCheckPoint()
		{
			//Unit Test failed. Is it necessary to create the new Security Checkpoints?
			//Message: The CustomizationSecurityCheckpoint needs to be a grandchild checkpoint of the module security checkpoint and needs to be named "Customize Actions".
			//Enterprise.Security.SecurityCheckpoint:
			//Maintain -> Reference Files -> Organization -> View -> * -> Customize Actions
			AssertEquals("CustomizationSecurityCheckpoint", ExpectedCustomizationSecurityCheckpoint, Supporter.CustomizationSecurityCheckpoint);
			AssertEquals("RunSecurityCheckpoint", ExpectedRunSecurityCheckpoint, Supporter.RunSecurityCheckpoint);
		}
	}
}
