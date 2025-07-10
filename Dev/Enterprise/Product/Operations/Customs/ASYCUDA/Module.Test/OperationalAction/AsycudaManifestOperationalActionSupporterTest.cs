using CargoWise.Definitions;
using Enterprise.Environment;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.Services.OperationalActions.Support.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.ASYCUDA.Module.Testing
{
	[TestedType(typeof(AsycudaManifestOperationalActionSupporter))]
	sealed class AsycudaManifestOperationalActionSupporterTest : OperationalActionSupporterTest<AsycudaManifestOperationalActionSupporter>
	{
		public void TestRootType()
		{
			AssertEquals(typeof(Business.AsycudaManifestHeader), Supporter.RootType);
		}

		public void TestBusinessContext()
		{
			AssertEquals(BusinessContext.AsycudaManifest, Supporter.BusinessContext);
		}

		public void TestBaseCheckpoint()
		{
			AssertEquals(Env.Security.AsycudaManifestReporting, Supporter.BaseCheckpoint);
		}

		public void TestPopulateMethods()
		{
			AssertCollectionContains(ActionMethodProviderIDs.INManifest, Supporter.Methods.GetAllIds());
		}

		protected override ModuleIdentifier ModuleID => ModuleIDs.Customs.ASYCUDA.Manifest;
	}
}
