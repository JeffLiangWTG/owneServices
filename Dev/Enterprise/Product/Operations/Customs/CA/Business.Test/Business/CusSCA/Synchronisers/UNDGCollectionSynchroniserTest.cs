using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business.Testing;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class UNDGCollectionSynchroniserTest : SynchroniserTestCase
	{
		public void TestUNDGCollectionSynchroniser()
		{
			var subsa = Factory.New<UNDGSubstance>();
			subsa.DG_Code = "A";
			subsa.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
			var subsb = Factory.New<UNDGSubstance>();
			subsb.DG_Code = "B";
			subsb.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
			var subsc = Factory.New<UNDGSubstance>();
			subsc.DG_Code = "C";
			subsc.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
			var subsd = Factory.New<UNDGSubstance>();
			subsd.DG_Code = "D";
			subsd.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
			var subse = Factory.New<UNDGSubstance>();
			subse.DG_Code = "E";
			subse.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;

			var source = Factory.New<PackLine>();
			var destination = Factory.New<CusSCAPivot>();
			var source1 = source.UNDGs.AddNew();
			source1.DI_DG = subsa.PK;
			source1.LinkDefault(subsa);
			source1.DI_DGFlashPoint = 1m;
			var contact1 = Factory.NewWithValidTestData<OrgContact>();
			source1.DI_OC_DGContact = contact1.PK;
			var source2 = source.UNDGs.AddNew();
			source2.DI_DG = subsb.PK;
			source2.LinkDefault(subsb);
			source2.DI_DGFlashPoint = 2m;
			var contact2 = Factory.NewWithValidTestData<OrgContact>();
			source2.DI_OC_DGContact = contact2.PK;
			var source3 = source.UNDGs.AddNew();
			source3.DI_DG = subsc.PK;
			source3.LinkDefault(subsc);
			source3.DI_DGFlashPoint = 3m;
			source3.DI_OC_DGContact = contact2.PK;
			var destination1 = destination.UNDGs.AddNew();
			destination1.DI_DG = subsa.PK;
			destination1.LinkDefault(subsa);
			destination1.DI_DGFlashPoint = 1m;
			destination1.DI_OC_DGContact = contact1.PK;
			var destination2 = destination.UNDGs.AddNew();
			destination2.DI_DG = subsb.PK;
			destination2.LinkDefault(subsb);
			destination2.DI_DGFlashPoint = 2m;
			destination2.DI_OC_DGContact = contact1.PK;

			var synchroniser = new UNDGCollectionSynchroniser(source, destination);
			synchroniser.Synchronise();

			AssertEquals("Destination should match source after synhcronise", 3, destination.UNDGs.Count);
			Assert(DestinationContains(destination.UNDGs, source1));
			Assert(DestinationContains(destination.UNDGs, source2));
			Assert(DestinationContains(destination.UNDGs, source3));
			Assert(!destination1.IsDeleted);
			Assert(destination2.IsDeleted);
			var source4 = destination.UNDGs.AddNew();
			source4.DI_DG = subsd.PK;
			source4.LinkDefault(subsd);
			source4.DI_DGFlashPoint = 4m;
			source4.DI_OC_DGContact = contact2.PK;
			AssertEquals("One element added", 4, destination.UNDGs.Count);
			var itemAddedToDestination = FindItemInDestination(destination.UNDGs, source4);
			AssertNotNull(itemAddedToDestination);
			source4.DI_DG = subse.PK;
			source4.LinkDefault(subse);
			AssertEquals("E", itemAddedToDestination.Substance.DG_Code);
			source4.DI_DGFlashPoint = 7m;
			AssertEquals(7m, itemAddedToDestination.DI_DGFlashPoint);
			source4.DI_OC_DGContact = contact1.PK;
			AssertEquals(contact1.PK, itemAddedToDestination.DI_OC_DGContact);
			source4.Delete();
			AssertEquals("One element removed", 3, destination.UNDGs.Count);
			Assert(itemAddedToDestination.IsDeleted);
		}

		bool DestinationContains(UNDGDataItemCollection destinationCollection, UNDGDataItem item)
		{
			return FindItemInDestination(destinationCollection, item) != null;
		}

		UNDGDataItem FindItemInDestination(UNDGDataItemCollection destinationCollection, UNDGDataItem item)
		{
			var query = new ZQuery(UNDGDataItemSchema.DI_DG, item.DI_DG);
			query.AddToFilter(UNDGDataItemSchema.DI_DGFlashPoint, item.DI_DGFlashPoint);
			query.AddToFilter(UNDGDataItemSchema.DI_OC_DGContact, item.DI_OC_DGContact);
			return destinationCollection.Find(query).FirstOrDefault();
		}
	}
}
