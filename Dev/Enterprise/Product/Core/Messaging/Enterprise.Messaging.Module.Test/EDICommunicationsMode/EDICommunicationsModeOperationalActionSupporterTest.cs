using CargoWise.Definitions;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Services.OperationalActions.Support.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Messaging.Module.Testing
{
	[TestedType(typeof(EDICommunicationsModeOperationalActionSupporter))]
	sealed class EDICommunicationsModeOperationalActionSupporterTest : OperationalActionSupporterTest<EDICommunicationsModeOperationalActionSupporter>
	{
		public void TestRootType()
		{
			var operationalActionSupporter = new EDICommunicationsModeOperationalActionSupporter();
			AssertEquals(nameof(EDICommunicationsModeOperationalActionSupporter.RootType), typeof(EDICommunicationsMode), operationalActionSupporter.RootType);
		}

		public void TestBusinessContext()
		{
			var operationalActionSupporter = new EDICommunicationsModeOperationalActionSupporter();
			AssertEquals(nameof(EDICommunicationsModeOperationalActionSupporter.BusinessContext), BusinessContext.EDIMessage, operationalActionSupporter.BusinessContext);
		}

		public void TestBaseCheckpoint()
		{
			var operationalActionSupporter = new EDICommunicationsModeOperationalActionSupporter();
			AssertEquals(nameof(EDICommunicationsModeOperationalActionSupporter.BaseCheckpoint), Env.Security.Organisation, operationalActionSupporter.BaseCheckpoint);
		}

		protected override ModuleIdentifier ModuleID => ModuleIDs.Messaging.EDICommunicationsMode;

		public override bool ShouldSupportDocuments => false;
		public new void TestSecurityCheckPoint()
		{
			//Unit Test failed. Is it necessary to create the new Security Checkpoints?
			//Message: The CustomizationSecurityCheckpoint needs to be a grandchild checkpoint of the module security checkpoint and needs to be named "Customize Actions".
			//Enterprise.Security.SecurityCheckpoint:
			//Maintain -> Reference Files -> Organization -> View -> * -> Customize Actions
			AssertEquals("CustomizationSecurityCheckpoint", Env.Security.Organisation, Supporter.CustomizationSecurityCheckpoint);
			AssertEquals("RunSecurityCheckpoint", Env.Security.Organisation, Supporter.RunSecurityCheckpoint);
		}
	}
}
