using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(TemporaryManifestHolder))]
	class TemporaryManifestHolderTest : NonPersistentBusinessObjectTestCase
	{
		public void TestManifests()
		{
			AssertNotNull(holder.Manifests);
			AssertEquals(0, holder.Manifests.Count);
		}

		public void TestManifestsCollectionDoesntHitDB()
		{
			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			ExportCustomsManifestHeader header = factory2.New<ExportCustomsManifestHeader>();
			factory2.Save();

			AssertEquals(0, holder.Manifests.Count);
		}

		public void TestProperties()
		{
			AssertEquals("precondition", ZString.Empty, holder.VesselName);
			AssertEquals("precondition", ZString.Empty, holder.VoyageNumber);

			holder.VesselName = "TestVessel";
			holder.VoyageNumber = "123";

			AssertEquals("VesselName", "TestVessel", holder.VesselName);
			AssertEquals("VoyageNumber", "123", holder.VoyageNumber);
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();

			holder = new TemporaryManifestHolder(Factory);
		}

		TemporaryManifestHolder holder;

		#endregion
	}
}
