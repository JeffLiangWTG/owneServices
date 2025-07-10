using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.DE.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.Declaration.Testing
{
	[TestedType(typeof(InwardProcessingPlaceCollection))]
	public class InwardProcessingPlaceCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestSetDefaultsForNewChild()
		{
			var collection = (InwardProcessingPlaceCollection)GetCollectionToTest();
			var place = collection.AddNew();
			CombineAssertions(() =>
			{
				AssertEquals("E2_AddressType", DocAddressTypes.Codes.InwardProcessingPlace, place.E2_AddressType);
				AssertEquals("E2_ParentTableCode", CusEntryInstructionSchema.Constants.Prefix, place.E2_ParentTableCode);
				AssertEquals("HasChanges", false, place.HasChanges);
			});
		}

		public void TestFilterByAddressType()
		{
			var collection = (InwardProcessingPlaceCollection)GetCollectionToTest();
			var place = collection.AddNew();

			var mainAccountingAddress = place.Instruction.MainAccountingAddress;
			mainAccountingAddress.E2_OA_Address = Factory.New<OrgHeader>().MainAddress.PK;
			CombineAssertions(() =>
			{
				AssertEquals("DocAddressType.InwardProcessingPlace", true, place.MatchesFilter(collection.CompleteFilter));
				AssertEquals("DocAddressType.MainAccountingAddress", false, mainAccountingAddress.MatchesFilter(collection.CompleteFilter));
			});
		}

		public void TestMaxCount()
		{
			var collection = (InwardProcessingPlaceCollection)GetCollectionToTest();
			AssertEquals(999, collection.MaxCount);
		}

		protected override BusinessObjectCollection GetCollectionToTest() => new InwardProcessingPlaceCollection(Factory.CreateInwardProcessingInstruction());
	}
}
