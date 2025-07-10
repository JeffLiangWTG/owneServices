using Enterprise.Security;
using Enterprise.Services.OperationalActions.Support.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Billing.Module.BillingPrices
{
	[TestedType(typeof(BillingPricesSupporter))]
	internal sealed class BillingPricesSupporterTest : OperationalActionSupporterTest<BillingPricesSupporter>
	{
		protected override SecurityCheckpoint ExpectedCustomizationSecurityCheckpoint
		{
			get
			{
				return EDISecurityCheckpoints.OrgLicenceBilling;
			}
		}

		public new void TestSecurityCheckPoint()
		{
			//Unit Test failed. Is it necessary to create the new Security Checkpoints?
			//Message: The CustomizationSecurityCheckpoint needs to be a grandchild checkpoint of the module security checkpoint and needs to be named "Customize Actions".
			//Enterprise.Security.SecurityCheckpoint:
			//Maintain -> Reference Files -> Organization -> View -> * -> Customize Actions
			AssertEquals("CustomizationSecurityCheckpoint", ExpectedCustomizationSecurityCheckpoint, Supporter.CustomizationSecurityCheckpoint);
			AssertEquals("RunSecurityCheckpoint", ExpectedRunSecurityCheckpoint, Supporter.RunSecurityCheckpoint);
		}

		protected override ModuleIdentifier ModuleID
		{
			get
			{
				return Modules.ClientModuleRegistration.BillingPrices;
			}
		}

		public override bool ShouldSupportDocuments
		{
			get
			{
				return false;
			}
		}
	}
}
