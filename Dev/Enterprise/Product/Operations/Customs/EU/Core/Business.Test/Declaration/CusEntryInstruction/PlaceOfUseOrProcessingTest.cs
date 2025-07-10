using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Business.CodeDescriptionPairLists;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.Testing
{
	[TestedType(typeof(PlaceOfUseOrProcessing))]
	sealed class PlaceOfUseOrProcessingTest : EnterpriseBusinessObjectTestCase
	{
		public void TestCGL_LocationUse()
		{
			AssertEquals("CGL_LocationUse should be FPU", PlaceOfUseOrProcessingLocationUseList.Codes.FirstPlaceOfUseOrProcessing, place.CGL_LocationUse);
		}

		public void TestLookups()
		{
			AssertType<PlaceOfUseOrProcessingLookups>(place.Lookups);
		}

		public void TestOwnersOfGoodsJobDocAddressValidation()
		{
			AssertType<PlaceOfUseOrProcessingValidation>(place.Validation);
		}

		public void TestDisplayText_Caption()
		{
			this.AssertDataBoundResourceStringsWithMultipleResourceKey(
				place.DisplayTextInfo,
				multipleResourceKey: JobDeclaration.CaptionKeyImportUCC6,
				caption: "Goods Location",
				fullDescription: "[4/9] Place(s) of Use or Processing"
			);
			this.AssertDataBoundResourceStringsWithMultipleResourceKey(
				place.DisplayTextInfo,
				multipleResourceKey: string.Empty,
				caption: "Goods Location",
				fullDescription: string.Empty
			);
		}

		public void TestUnlocode()
		{
			var cusGoodsLocation = Factory.New<PlaceOfUseOrProcessing>();
			cusGoodsLocation.CGL_CustomsOffice = "IEDUB100";
			AssertEquals("Unlocode mapping", "IEDUB100", cusGoodsLocation.Unlocode);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var place = instruction.FirstPlaceOfUseOrProcessing;

			return place;
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => GetNewBusinessObject();

		protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewBusinessObject();

		protected override void SetUp()
		{
			base.SetUp();
			place = (PlaceOfUseOrProcessing)GetNewBusinessObject();
		}
		PlaceOfUseOrProcessing place;
	}
}
