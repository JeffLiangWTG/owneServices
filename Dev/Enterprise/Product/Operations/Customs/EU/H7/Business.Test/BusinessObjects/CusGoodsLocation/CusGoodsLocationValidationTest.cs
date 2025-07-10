using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.EU.H7.Business.Testing
{
	sealed class CusGoodsLocationValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCGL_QualifierGivenDeclarationTypeIsA()
		{
			var manifest = Factory.New<AsycudaManifestHeader>();
			var bill = manifest.Bills.AddNew();
			var cusGoodsLocation = Factory.New<CusGoodsLocation>();
			cusGoodsLocation.CGL_ParentID = bill.PK;
			cusGoodsLocation.CGL_LocationUse = CusGoodsLocationUseList.Codes.Departure;
			cusGoodsLocation.CGL_ParentTableCode = "ABL";

			bill.ABL_ShipmentType = EU.Business.EntrySubStyleList.Codes.NormalDeclaration;

			cusGoodsLocation.Validation.ValidateCGL_Qualifier();
			AssertHasMessageErrorContaining(cusGoodsLocation.CGL_QualifierInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCGL_QualifierGivenDeclarationTypeIsD()
		{
			var manifest = Factory.New<AsycudaManifestHeader>();
			var bill = manifest.Bills.AddNew();
			var cusGoodsLocation = Factory.New<CusGoodsLocation>();
			cusGoodsLocation.CGL_ParentID = bill.PK;
			cusGoodsLocation.CGL_LocationUse = CusGoodsLocationUseList.Codes.Departure;
			cusGoodsLocation.CGL_ParentTableCode = "ABL";

			bill.ABL_ShipmentType = EU.Business.EntrySubStyleList.Codes.PreliminaryDeclarationUnderCodeA;

			cusGoodsLocation.CGL_Qualifier = "U";
			cusGoodsLocation.Validation.ValidateCGL_Qualifier();
			AssertHasMessageErrorContaining(cusGoodsLocation.CGL_QualifierInfo, notPopulatedErrorMessage);

			cusGoodsLocation.CGL_Qualifier = "";
			cusGoodsLocation.Validation.ValidateCGL_Qualifier();
			AssertNoMessageErrors(cusGoodsLocation.CGL_QualifierInfo);
		}

		public void TestCGL_TypeGivenDeclarationTypeIsA()
		{
			var manifest = Factory.New<AsycudaManifestHeader>();
			var bill = manifest.Bills.AddNew();
			var cusGoodsLocation = Factory.New<CusGoodsLocation>();
			cusGoodsLocation.CGL_ParentID = bill.PK;
			cusGoodsLocation.CGL_LocationUse = CusGoodsLocationUseList.Codes.Departure;
			cusGoodsLocation.CGL_ParentTableCode = "ABL";

			bill.ABL_ShipmentType = EU.Business.EntrySubStyleList.Codes.NormalDeclaration;

			cusGoodsLocation.Validation.ValidateCGL_Type();
			AssertHasMessageErrorContaining(cusGoodsLocation.CGL_TypeInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCGL_TypeGivenDeclarationTypeIsD()
		{
			var manifest = Factory.New<AsycudaManifestHeader>();
			var bill = manifest.Bills.AddNew();
			var cusGoodsLocation = Factory.New<CusGoodsLocation>();
			cusGoodsLocation.CGL_ParentID = bill.PK;
			cusGoodsLocation.CGL_LocationUse = CusGoodsLocationUseList.Codes.Departure;
			cusGoodsLocation.CGL_ParentTableCode = "ABL";

			bill.ABL_ShipmentType = EU.Business.EntrySubStyleList.Codes.PreliminaryDeclarationUnderCodeA;

			cusGoodsLocation.CGL_Type = "B";
			cusGoodsLocation.Validation.ValidateCGL_Type();
			AssertHasMessageErrorContaining(cusGoodsLocation.CGL_TypeInfo, notPopulatedErrorMessage);

			cusGoodsLocation.CGL_Type = "";
			cusGoodsLocation.Validation.ValidateCGL_Type();
			AssertNoMessageErrors(cusGoodsLocation.CGL_TypeInfo);
		}

		public void TestUnlocodeGivenDeclarationTypeIsA()
		{
			var manifest = Factory.New<AsycudaManifestHeader>();
			var bill = manifest.Bills.AddNew();
			var cusGoodsLocation = Factory.New<CusGoodsLocation>();
			cusGoodsLocation.CGL_ParentID = bill.PK;
			cusGoodsLocation.CGL_LocationUse = CusGoodsLocationUseList.Codes.Departure;
			cusGoodsLocation.CGL_ParentTableCode = "ABL";

			bill.ABL_ShipmentType = EU.Business.EntrySubStyleList.Codes.NormalDeclaration;

			cusGoodsLocation.CGL_Qualifier = "U";
			cusGoodsLocation.Validation.ValidateCGL_AdditionalIdentifier();
			AssertHasMessageErrorContaining(cusGoodsLocation.UnlocodeInfo, "UNLOCODE required");

			cusGoodsLocation.CGL_Qualifier = "";
			cusGoodsLocation.CGL_AdditionalIdentifier = "";
			cusGoodsLocation.Validation.ValidateCGL_AdditionalIdentifier();
			AssertNoMessageErrors(cusGoodsLocation.UnlocodeInfo);
		}

		public void TestUnlocodeGivenDeclarationTypeIsD()
		{
			var manifest = Factory.New<AsycudaManifestHeader>();
			var bill = manifest.Bills.AddNew();
			var cusGoodsLocation = Factory.New<CusGoodsLocation>();
			cusGoodsLocation.CGL_ParentID = bill.PK;
			cusGoodsLocation.CGL_LocationUse = CusGoodsLocationUseList.Codes.Departure;
			cusGoodsLocation.CGL_ParentTableCode = "ABL";

			bill.ABL_ShipmentType = EU.Business.EntrySubStyleList.Codes.PreliminaryDeclarationUnderCodeA;

			cusGoodsLocation.CGL_AdditionalIdentifier = "TEST";
			cusGoodsLocation.Validation.ValidateCGL_AdditionalIdentifier();
			AssertHasMessageErrorContaining(cusGoodsLocation.CGL_AdditionalIdentifierInfo, "UNLOCODE shall not be populated");

			cusGoodsLocation.CGL_AdditionalIdentifier = "";
			cusGoodsLocation.Validation.ValidateCGL_AdditionalIdentifier();
			AssertNoMessageErrors(cusGoodsLocation.CGL_AdditionalIdentifierInfo);
		}

		static readonly string notPopulatedErrorMessage = "Shall not be populated for additional declaration type 'D'.";
	}
}
