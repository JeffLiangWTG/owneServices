using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.IL.Manifest.Business.Testing
{
	sealed class CusSealLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestSealType()
		{
			var lookup = lookups.SealTypeList;

			AssertEquals("Should contain 3 items", 3, lookup.Count);
			AssertCodeDescriptionPairList(lookup, ("E", "Electronic Seal"), ("M", "Mechanical Seal"), ("R", "RFID Seal"));
			AssertSame("SealTypeList is cached", lookup, lookups.SealTypeList);
		}

		public void TestUnloadedStates()
		{
			AssertType<ILUnloadedStates>(lookups.UnloadedStates);
			var unloadedStates = lookups.UnloadedStates;

			AssertEquals("Should contain 5 items", 5, unloadedStates.Count);
			AssertCodeDescriptionPairList(unloadedStates, ("DAM", "Damaged"), ("DEC", "As Declared"), ("DIF", "Differences to Declared"), ("MIS", "Missing"), ("NEW", "New"));

			var factory = Factory;
			var header = factory.New<AsycudaManifestHeader>();
			var container = header.Containers.AddNew();
			var cusSeal = container.AdditionalSeals.AddNew();
			AssertSame("Condition list is cached", unloadedStates, cusSeal.Lookups.UnloadedStates);
		}

		public void TestSealingPartyList_WhenParentIsAsycudaContainer_ReturnsContainerSealingPartyList()
		{
			var factory = Factory;
			var header = factory.New<AsycudaManifestHeader>();
			var container = header.Containers.AddNew();
			var expectedList = container.Lookups.SealingPartyList;

			var cusSeal = container.AdditionalSeals.AddNew();
			var lookups = cusSeal.Lookups;

			AssertSame("Should return container's SealingPartyList", expectedList, lookups.SealingPartyList);
		}

		public void TestSealingPartyList_WhenParentIsNotAsycudaContainer_ReturnsEmptyList()
		{
			var cusSeal = Factory.New<CusSeal>();
			var lookups = cusSeal.Lookups;

			var sealingPartyList = lookups.SealingPartyList;
			AssertNotNull("Should return List if Parent.Parent is not AsycudaContainer", sealingPartyList);
			AssertEquals("Should return empty list", 0, sealingPartyList.Count);
		}
		protected override void SetUp()
		{
			base.SetUp();
			var factory = Factory;
			var header = factory.New<AsycudaManifestHeader>();
			var container = header.Containers.AddNew();
			var cusSeal = container.AdditionalSeals.AddNew();
			cusSeal.BK_SealNumber = "123";
			lookups = cusSeal.Lookups;
		}

		CusSealLookups lookups;
	}
}
