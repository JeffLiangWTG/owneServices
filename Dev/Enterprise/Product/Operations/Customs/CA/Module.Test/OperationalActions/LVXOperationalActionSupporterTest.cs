using CargoWise.Definitions;
using Enterprise.Environment;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.Services.OperationalActions.Support.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Module.Testing
{
	[TestedType(typeof(LVXOperationalActionSupporter))]
	sealed class LVXOperationalActionSupporterTest : OperationalActionSupporterTest<LVXOperationalActionSupporter>
	{
		public void TestPopulateMethods()
		{
			var supporter = new LVXOperationalActionSupporter();
			var allIds = supporter.Methods.GetAllIds();
			AssertCollectionContains(ActionMethodProviderIDs.CAJobDeclaration, allIds);
		}

		public new void TestBusinessContextIsConsistent()
		{
			AssertEquals("The supporter's business context is set to CALVX to show the special operational action menus for Courier LVS Declarations module.", BusinessContext.CALVX, Supporter.BusinessContext);
		}

		public new void TestSecurityCheckPoint()
		{
			//Unit Test failed. Is it necessary to create the new Security Checkpoints?
			//Message: The CustomizationSecurityCheckpoint needs to be a grandchild checkpoint of the module security checkpoint and needs to be named "Customize Actions".
			//Enterprise.Security.SecurityCheckpoint:
			//Operate -> Customs -> Courier LVS Declarations -> Customize Actions

			AssertEquals("CustomizationSecurityCheckpoint", ExpectedCustomizationSecurityCheckpoint, Supporter.CustomizationSecurityCheckpoint);
			AssertEquals("RunSecurityCheckpoint", ExpectedRunSecurityCheckpoint, Supporter.RunSecurityCheckpoint);
			AssertEquals("SecurityCheckpoint", Env.Security.CALVXJobs, Module.SecurityCheckpoint);
		}

		public void TestDocumentBusinessContext()
		{
			AssertEquals("DocumentBusinessContext", BusinessContext.Customs, Supporter.DocumentBusinessContext);
		}

		protected override ModuleIdentifier ModuleID => ModuleIDs.Customs.CA.CALVXJobs;
	}
}
