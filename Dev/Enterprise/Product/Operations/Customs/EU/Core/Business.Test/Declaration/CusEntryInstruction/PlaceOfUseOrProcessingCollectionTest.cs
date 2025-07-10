using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Business.CodeDescriptionPairLists;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.Testing
{
	[TestedType(typeof(PlaceOfUseOrProcessingCollection))]
	sealed class PlaceOfUseOrProcessingCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest() => new PlaceOfUseOrProcessingCollection(Factory.New<CusEntryInstruction>());

		public void TestSetDefaultsForPlaceOfUseOrProcessingCollectionChild()
		{
			var collection = (PlaceOfUseOrProcessingCollection)GetCollectionToTest();
			var place = collection.AddNew();
			CombineAssertions(() =>
			{
				AssertEquals("CGL_LocationUse", PlaceOfUseOrProcessingLocationUseList.Codes.PlacesOfUseOrProcessing, place.CGL_LocationUse);
				AssertEquals("CGL_ParentTableCode", CusEntryInstructionSchema.Constants.Prefix, place.CGL_ParentTableCode);
				AssertEquals("HasChanges", false, place.HasChanges);
			});
		}
	}
}
