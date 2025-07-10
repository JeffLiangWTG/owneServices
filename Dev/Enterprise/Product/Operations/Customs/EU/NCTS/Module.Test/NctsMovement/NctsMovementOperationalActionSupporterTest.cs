using CargoWise.Definitions;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.Module.Testing
{
	[TestedType(typeof(NctsMovementOperationalActionSupporter))]
	sealed class NctsMovementOperationalActionSupporterTest : Services.OperationalActions.Support.Testing.OperationalActionSupporterTest<NctsMovementOperationalActionSupporter>
	{
		public void TestRootType()
		{
			var operationalActionSupporter = new NctsMovementOperationalActionSupporter();
			AssertEquals(nameof(NctsMovementOperationalActionSupporter.RootType), typeof(NctsHeader), operationalActionSupporter.RootType);
		}

		public void TestBusinessContext()
		{
			var operationalActionSupporter = new NctsMovementOperationalActionSupporter();
			AssertEquals(nameof(NctsMovementOperationalActionSupporter.BusinessContext), BusinessContext.CusInBondHeader, operationalActionSupporter.BusinessContext);
		}

		public void TestBaseCheckpoint()
		{
			var operationalActionSupporter = new NctsMovementOperationalActionSupporter();
			AssertEquals(nameof(NctsMovementOperationalActionSupporter.BaseCheckpoint), Env.Security.EuNctsMovement, operationalActionSupporter.BaseCheckpoint);
		}

		protected override ModuleIdentifier ModuleID => ModuleIDs.Customs.EU.NctsMovementModule;
	}
}
