using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.ES.Manifest.H7.Business.Testing
{
	public class CusGoodsLocationValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCGL_QualifierGivenDeclarationTypeIsA()
		{
			Setup();

			bill.ABL_ShipmentType = EU.Business.EntrySubStyleList.Codes.NormalDeclaration;

			cusGoodsLocation1.Validation.ValidateCGL_Qualifier();
			AssertHasMessageErrorContaining(cusGoodsLocation1.CGL_QualifierInfo, MandatoryValidation.YouHaveNotEntered);

			cusGoodsLocation2.Validation.ValidateCGL_Qualifier();
			AssertHasMessageErrorContaining(cusGoodsLocation2.CGL_QualifierInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCGL_QualifierGivenDeclarationTypeIsD()
		{
			Setup();

			bill.ABL_ShipmentType = EU.Business.EntrySubStyleList.Codes.PreliminaryDeclarationUnderCodeA;

			cusGoodsLocation1.CGL_Qualifier = "";
			cusGoodsLocation1.Validation.ValidateCGL_Qualifier();
			AssertNoMessageErrors(cusGoodsLocation1.CGL_QualifierInfo);

			cusGoodsLocation2.CGL_Qualifier = "";
			cusGoodsLocation2.Validation.ValidateCGL_Qualifier();
			AssertNoMessageErrors(cusGoodsLocation2.CGL_QualifierInfo);
		}

		public void TestCGL_TypeGivenDeclarationTypeIsA()
		{
			Setup();

			bill.ABL_ShipmentType = EU.Business.EntrySubStyleList.Codes.NormalDeclaration;

			cusGoodsLocation1.Validation.ValidateCGL_Type();
			AssertHasMessageErrorContaining(cusGoodsLocation1.CGL_TypeInfo, MandatoryValidation.YouHaveNotEntered);

			cusGoodsLocation2.Validation.ValidateCGL_Type();
			AssertHasMessageErrorContaining(cusGoodsLocation2.CGL_TypeInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCGL_TypeGivenDeclarationTypeIsD()
		{
			Setup();

			bill.ABL_ShipmentType = EU.Business.EntrySubStyleList.Codes.PreliminaryDeclarationUnderCodeA;

			cusGoodsLocation1.CGL_Type = "B";
			cusGoodsLocation1.Validation.ValidateCGL_Type();
			AssertHasMessageErrorContaining(cusGoodsLocation1.CGL_TypeInfo, notPopulatedErrorMessage);

			cusGoodsLocation1.CGL_Type = "";
			cusGoodsLocation1.Validation.ValidateCGL_Type();
			AssertNoMessageErrors(cusGoodsLocation1.CGL_TypeInfo);

			cusGoodsLocation2.CGL_Type = "B";
			cusGoodsLocation2.Validation.ValidateCGL_Type();
			AssertHasMessageErrorContaining(cusGoodsLocation2.CGL_TypeInfo, notPopulatedErrorMessage);

			cusGoodsLocation2.CGL_Type = "";
			cusGoodsLocation2.Validation.ValidateCGL_Type();
			AssertNoMessageErrors(cusGoodsLocation2.CGL_TypeInfo);
		}

		public void TestUnlocodeGivenDeclarationTypeIsA()
		{
			Setup();

			bill.ABL_ShipmentType = EU.Business.EntrySubStyleList.Codes.NormalDeclaration;

			cusGoodsLocation1.CGL_Qualifier = "U";
			cusGoodsLocation1.Validation.ValidateCGL_AdditionalIdentifier();
			AssertHasMessageErrorContaining(cusGoodsLocation1.UnlocodeInfo, "UNLOCODE required");

			cusGoodsLocation1.CGL_Qualifier = "";
			cusGoodsLocation1.CGL_AdditionalIdentifier = "";
			cusGoodsLocation1.Validation.ValidateCGL_AdditionalIdentifier();
			AssertNoMessageErrors(cusGoodsLocation1.UnlocodeInfo);

			cusGoodsLocation2.CGL_Qualifier = "U";
			cusGoodsLocation2.Validation.ValidateCGL_AdditionalIdentifier();
			AssertHasMessageErrorContaining(cusGoodsLocation2.UnlocodeInfo, "UNLOCODE required");

			cusGoodsLocation2.CGL_Qualifier = "";
			cusGoodsLocation2.CGL_AdditionalIdentifier = "";
			cusGoodsLocation2.Validation.ValidateCGL_AdditionalIdentifier();
			AssertNoMessageErrors(cusGoodsLocation2.UnlocodeInfo);
		}

		public void TestUnlocodeGivenDeclarationTypeIsD()
		{
			Setup();

			bill.ABL_ShipmentType = EU.Business.EntrySubStyleList.Codes.PreliminaryDeclarationUnderCodeA;

			cusGoodsLocation1.CGL_AdditionalIdentifier = "TEST";
			cusGoodsLocation1.Validation.ValidateCGL_AdditionalIdentifier();
			AssertHasMessageErrorContaining(cusGoodsLocation1.CGL_AdditionalIdentifierInfo, "UNLOCODE shall not be populated");

			cusGoodsLocation1.CGL_AdditionalIdentifier = "";
			cusGoodsLocation1.Validation.ValidateCGL_AdditionalIdentifier();
			AssertNoMessageErrors(cusGoodsLocation1.CGL_AdditionalIdentifierInfo);

			cusGoodsLocation2.CGL_AdditionalIdentifier = "TEST";
			cusGoodsLocation2.Validation.ValidateCGL_AdditionalIdentifier();
			AssertHasMessageErrorContaining(cusGoodsLocation2.CGL_AdditionalIdentifierInfo, "UNLOCODE shall not be populated");

			cusGoodsLocation2.CGL_AdditionalIdentifier = "";
			cusGoodsLocation2.Validation.ValidateCGL_AdditionalIdentifier();
			AssertNoMessageErrors(cusGoodsLocation2.CGL_AdditionalIdentifierInfo);
		}

		public void TestCheckCGL_Qualifier()
		{
			Setup();

			cusGoodsLocation1.CGL_Qualifier = "A";
			cusGoodsLocation1.Validation.ValidateCGL_Qualifier();
			AssertHasErrorContaining(cusGoodsLocation1.CGL_QualifierInfo, "Enter a valid Location of Goods: Qualifier.");
		}

		public void TestCheckCGL_Type()
		{
			Setup();

			cusGoodsLocation1.CGL_Type = "TE";
			cusGoodsLocation1.Validation.ValidateCGL_Type();
			AssertHasErrorContaining(cusGoodsLocation1.CGL_TypeInfo, "Enter a valid Location of Goods: Type.");
		}

		void Setup()
		{
			var setupManifest = Factory.New<AsycudaManifestHeader>();
			manifest = setupManifest;
			var setupBill = manifest.Bills.AddNew();
			bill = setupBill;

			var setupCusGoodsLocation1 = Factory.New<CusGoodsLocation>();
			setupCusGoodsLocation1.CGL_ParentID = bill.PK;
			setupCusGoodsLocation1.CGL_LocationUse = CusGoodsLocationUseList.Codes.Departure;
			setupCusGoodsLocation1.CGL_ParentTableCode = "ABL";
			cusGoodsLocation1 = setupCusGoodsLocation1;

			var setupCusGoodsLocation2 = Factory.New<CusGoodsLocation>();
			setupCusGoodsLocation2.CGL_ParentID = manifest.PK;
			setupCusGoodsLocation2.CGL_LocationUse = CusGoodsLocationUseList.Codes.Departure;
			setupCusGoodsLocation2.CGL_ParentTableCode = "AMA";
			cusGoodsLocation2 = setupCusGoodsLocation2;
		}

		AsycudaManifestHeader manifest;
		AsycudaBill bill;
		CusGoodsLocation cusGoodsLocation1;
		CusGoodsLocation cusGoodsLocation2;

		static readonly string notPopulatedErrorMessage = "Shall not be populated for additional declaration type 'D'.";
	}
}
