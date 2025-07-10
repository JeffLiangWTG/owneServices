using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IL.Manifest.Business.Testing
{
	sealed class AsycudaContainerLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestEmptyFullList()
		{
			var lookup = Factory.GetCachedValue<ILEmptyFullIndicatorList>();

			AssertEquals("Should contain 2 items", 2, lookup.Count);

			var expectedList = new CodeDescriptionPairList();
			expectedList.AddPair("A", "Empty");
			expectedList.AddPair("B", "Not Empty");
			AssertEquals("Elements", expectedList.ElementsAsString, lookup.ElementsAsString);

			var header = Factory.New<AsycudaManifestHeader>();
			var container = header.Containers.AddNew();

			AssertSame("EmptyFullList is cached", lookup, container.Lookups.EmptyFullList);
		}

		public void TestUnloadedStates()
		{
			var lookup = Factory.GetCachedValue<ILUnloadedStates>();

			AssertEquals("Should contain 5 items", 5, lookup.Count);
			AssertCodeDescriptionPairList(lookup, ("DAM", "Damaged"), ("DEC", "As Declared"), ("DIF", "Differences to Declared"), ("MIS", "Missing"), ("NEW", "New"));

			var header = Factory.New<AsycudaManifestHeader>();
			var container = header.Containers.AddNew();
			var lookups = container.Lookups;

			AssertSame("UnloadedStates is cached", lookup, lookups.UnloadedStates);
		}

		public void TestSealTypeListCore()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var container = header.Containers.AddNew();
			var lookups = container.Lookups;
			var lookup = lookups.SealTypeList;
			AssertEquals("Should contain 3 items", 3, lookup.Count);
			AssertCodeDescriptionPairList(lookup, ("E", "Electronic Seal"), ("M", "Mechanical Seal"), ("R", "RFID Seal"));
			AssertSame("SealTypeList is cached", lookup, lookups.SealTypeList);
		}
	}
}
