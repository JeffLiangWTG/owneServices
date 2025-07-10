using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using CusEntryInstruction = Enterprise.Customs.EU.Business.Declaration.CusEntryInstruction;

namespace Enterprise.Customs.EU.Business.Testing
{
	sealed class PlaceOfUseOrProcessingValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckDuplicates()
		{
			var expectedMessageErrorMessage = "Place must be unique, no duplicate allowed.";
			var instruction = Factory.New<CusEntryInstruction>();
			var place1 = instruction.PlaceOfUseOrProcessingCollection.AddNew();
			var place2 = instruction.PlaceOfUseOrProcessingCollection.AddNew();
			var place3 = instruction.PlaceOfUseOrProcessingCollection.AddNew();

			place1.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.GnssCoordinates;
			place1.CGL_Type = CusGoodsLocationTypeList.Codes.DesignatedLocation;
			place1.CGL_AdditionalIdentifier = "123";
			place2.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.EoriNumber;
			place2.CGL_Type = CusGoodsLocationTypeList.Codes.ApprovedPlace;
			place2.CGL_AdditionalIdentifier = "456";

			CombineAssertions(() =>
			{
				place1.Validation.ValidateAll();
				AssertNoRowMessageError("Single first entry", place1, expectedMessageErrorMessage);

				place3.CGL_Qualifier = place1.CGL_Qualifier;
				place3.CGL_Type = place1.CGL_Type;
				place3.CGL_AdditionalIdentifier = place1.CGL_AdditionalIdentifier;

				place1.Validation.ValidateAll();
				AssertHasRowMessageError("Duplicate with third", place1, expectedMessageErrorMessage);
			});
		}

		public void TestRule061()
		{
			var instruction = Factory.NewWithValidTestData<CusEntryInstruction>();

			var goodslocation = instruction.FirstPlaceOfUseOrProcessing;
			var message = "[C0061] Customs Office required when qualifier is 'U'.";

			goodslocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.UnLocode;
			goodslocation.Validation.ValidateCGL_CustomsOffice();
			AssertHasMessageErrorContaining("No UNLOCO: " + message, goodslocation.CGL_CustomsOfficeInfo, message);

			goodslocation.Unlocode = "unloco";
			AssertNoMessageErrorContaining("There is an unloco: " + message, goodslocation.CGL_CustomsOfficeInfo, message);
		}

		public void TestRule062()
		{
			var instruction = Factory.NewWithValidTestData<CusEntryInstruction>();

			var goodslocation = instruction.FirstPlaceOfUseOrProcessing;
			var message = "[C0062] Customs Office required when qualifier is 'V'.";

			goodslocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.CustomsOfficeIdentifier;
			goodslocation.Validation.ValidateCGL_CustomsOffice();
			AssertHasMessageErrorContaining("No customs office: " + message, goodslocation.CGL_CustomsOfficeInfo, message);

			goodslocation.CGL_CustomsOffice = "office";
			AssertNoMessageErrorContaining("There is a customs office: " + message, goodslocation.CGL_CustomsOfficeInfo, message);
		}
	}
}
