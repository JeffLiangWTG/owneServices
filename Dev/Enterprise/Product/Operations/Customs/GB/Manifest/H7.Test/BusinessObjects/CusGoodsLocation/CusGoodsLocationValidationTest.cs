using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.GB.H7.Business.Testing
{
	sealed class CusGoodsLocationValidationTest : BusinessObjectValidationTestCase
	{
		#region CGL_Qualifier

		public void TestCheckCGL_Qualifier_WhenDeclarationTypeIsAAndQualifierIsValid_ShouldNotShowError()
		{
			var cusGoodsLocation = CreateCusGoodsLocation(EU.Business.EntrySubStyleList.Codes.NormalDeclaration);
			cusGoodsLocation.CGL_Qualifier = "U";
			cusGoodsLocation.Validation.ValidateCGL_Qualifier();
			AssertNoMessageErrors(cusGoodsLocation.CGL_QualifierInfo);
		}

		public void TestCheckCGL_Qualifier_WhenDeclarationTypeIsAAndQualifierIsEmpty_ShouldShowError()
		{
			var cusGoodsLocation = CreateCusGoodsLocation(EU.Business.EntrySubStyleList.Codes.NormalDeclaration);
			cusGoodsLocation.CGL_Qualifier = string.Empty;
			cusGoodsLocation.Validation.ValidateCGL_Qualifier();
			AssertHasMessageErrorContaining(cusGoodsLocation.CGL_QualifierInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckCGL_Qualifier_WhenDeclarationTypeIsAAndQualifierIsInvalid_ShouldShowError()
		{
			var cusGoodsLocation = CreateCusGoodsLocation(EU.Business.EntrySubStyleList.Codes.NormalDeclaration);
			cusGoodsLocation.CGL_Qualifier = "Z";
			cusGoodsLocation.Validation.ValidateCGL_Qualifier();
			AssertHasErrorContaining(cusGoodsLocation.CGL_QualifierInfo, "Enter a valid Location of Goods: Qualifier");
		}

		public void TestCheckCGL_Qualifier_WhenDeclarationTypeIsDAndQualifierIsValid_ShouldNotShowError()
		{
			var cusGoodsLocation = CreateCusGoodsLocation(EU.Business.EntrySubStyleList.Codes.PreliminaryDeclarationUnderCodeA);
			cusGoodsLocation.CGL_Qualifier = "U";
			cusGoodsLocation.Validation.ValidateCGL_Qualifier();
			AssertNoMessageErrors(cusGoodsLocation.CGL_QualifierInfo);
		}

		public void TestCheckCGL_Qualifier_WhenDeclarationTypeIsDAndQualifierIsEmpty_ShouldShowError()
		{
			var cusGoodsLocation = CreateCusGoodsLocation(EU.Business.EntrySubStyleList.Codes.PreliminaryDeclarationUnderCodeA);
			cusGoodsLocation.CGL_Qualifier = string.Empty;
			cusGoodsLocation.Validation.ValidateCGL_Qualifier();
			AssertHasMessageErrorContaining(cusGoodsLocation.CGL_QualifierInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckCGL_Qualifier_WhenDeclarationTypeIsDAndQualifierIsInvalid_ShouldShowError()
		{
			var cusGoodsLocation = CreateCusGoodsLocation(EU.Business.EntrySubStyleList.Codes.PreliminaryDeclarationUnderCodeA);
			cusGoodsLocation.CGL_Qualifier = "Z";
			cusGoodsLocation.Validation.ValidateCGL_Qualifier();
			AssertHasErrorContaining(cusGoodsLocation.CGL_QualifierInfo, "Enter a valid Location of Goods: Qualifier");
		}

		#endregion

		#region CGL_Type

		public void TestCheckCGL_Type_WhenDeclarationTypeIsAAndTypeIsValid_ShouldNotShowError()
		{
			var cusGoodsLocation = CreateCusGoodsLocation(EU.Business.EntrySubStyleList.Codes.NormalDeclaration);
			cusGoodsLocation.CGL_Type = "B";
			cusGoodsLocation.Validation.ValidateCGL_Type();
			AssertNoMessageErrors(cusGoodsLocation.CGL_TypeInfo);
		}

		public void TestCheckCGL_Type_WhenDeclarationTypeIsAAndTypeIsEmpty_ShouldShowError()
		{
			var cusGoodsLocation = CreateCusGoodsLocation(EU.Business.EntrySubStyleList.Codes.NormalDeclaration);
			cusGoodsLocation.CGL_Type = string.Empty;
			cusGoodsLocation.Validation.ValidateCGL_Type();
			AssertHasMessageErrorContaining(cusGoodsLocation.CGL_TypeInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckCGL_Type_WhenDeclarationTypeIsAAndTypeIsInvalid_ShouldShowError()
		{
			var cusGoodsLocation = CreateCusGoodsLocation(EU.Business.EntrySubStyleList.Codes.NormalDeclaration);
			cusGoodsLocation.CGL_Type = "Z";
			cusGoodsLocation.Validation.ValidateCGL_Type();
			AssertHasErrorContaining(cusGoodsLocation.CGL_TypeInfo, "Enter a valid Location of Goods: Type.");
		}

		public void TestCheckCGL_Type_WhenDeclarationTypeIsDAndTypeIsValid_ShouldNotShowError()
		{
			var cusGoodsLocation = CreateCusGoodsLocation(EU.Business.EntrySubStyleList.Codes.PreliminaryDeclarationUnderCodeA);
			cusGoodsLocation.CGL_Type = "B";
			cusGoodsLocation.Validation.ValidateCGL_Type();
			AssertNoMessageErrors(cusGoodsLocation.CGL_TypeInfo);
		}

		public void TestCheckCGL_Type_WhenDeclarationTypeIsDAndTypeIsEmpty_ShouldShowError()
		{
			var cusGoodsLocation = CreateCusGoodsLocation(EU.Business.EntrySubStyleList.Codes.PreliminaryDeclarationUnderCodeA);
			cusGoodsLocation.CGL_Type = string.Empty;
			cusGoodsLocation.Validation.ValidateCGL_Type();
			AssertHasMessageErrorContaining(cusGoodsLocation.CGL_TypeInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckCGL_Type_WhenDeclarationTypeIsDAndTypeIsInvalid_ShouldShowError()
		{
			var cusGoodsLocation = CreateCusGoodsLocation(EU.Business.EntrySubStyleList.Codes.PreliminaryDeclarationUnderCodeA);
			cusGoodsLocation.CGL_Type = "Z";
			cusGoodsLocation.Validation.ValidateCGL_Type();
			AssertHasErrorContaining(cusGoodsLocation.CGL_TypeInfo, "Enter a valid Location of Goods: Type.");
		}

		#endregion

		#region CGL_AdditionalIdentifier

		public void TestCheckCGL_AdditionalIdentifier_WhenDeclarationTypeIsAAndQualifierIsEmpty_ShouldNotShowError()
		{
			var cusGoodsLocation = CreateCusGoodsLocation(EU.Business.EntrySubStyleList.Codes.NormalDeclaration);
			cusGoodsLocation.CGL_Qualifier = string.Empty;
			cusGoodsLocation.Unlocode = string.Empty;
			cusGoodsLocation.Validation.ValidateCGL_AdditionalIdentifier();
			AssertNoMessageErrors(cusGoodsLocation.UnlocodeInfo);
		}

		public void TestCheckCGL_AdditionalIdentifier_WhenDeclarationTypeIsAAndQualifierIsUAndUnlocodeIsEmpty_ShouldShowError()
		{
			var cusGoodsLocation = CreateCusGoodsLocation(EU.Business.EntrySubStyleList.Codes.NormalDeclaration);
			cusGoodsLocation.CGL_Qualifier = "U";
			cusGoodsLocation.Unlocode = string.Empty;
			cusGoodsLocation.Validation.ValidateCGL_AdditionalIdentifier();
			AssertHasMessageErrorContaining(cusGoodsLocation.UnlocodeInfo, "UNLOCODE required");
		}

		public void TestCheckCGL_AdditionalIdentifier_WhenDeclarationTypeIsAAndUnlocodeIsInvalid_ShouldShowError()
		{
			var cusGoodsLocation = CreateCusGoodsLocation(EU.Business.EntrySubStyleList.Codes.NormalDeclaration);
			cusGoodsLocation.CGL_Qualifier = "U";
			cusGoodsLocation.Unlocode = "ADZZZ";
			cusGoodsLocation.Validation.ValidateCGL_AdditionalIdentifier();
			AssertHasMessageErrorContaining(cusGoodsLocation.UnlocodeInfo, "The code you have selected is not in the list");
		}

		public void TestCheckCGL_AdditionalIdentifier_WhenDeclarationTypeIsAAndUnlocodeIsValid_ShouldNotShowError()
		{
			var unlocode = "ADZZZ";
			CreateCusCode(code: unlocode);

			var cusGoodsLocation = CreateCusGoodsLocation(EU.Business.EntrySubStyleList.Codes.NormalDeclaration);
			cusGoodsLocation.CGL_Qualifier = "U";
			cusGoodsLocation.Unlocode = unlocode;
			cusGoodsLocation.Validation.ValidateCGL_AdditionalIdentifier();
			AssertNoMessageErrors(cusGoodsLocation.UnlocodeInfo);
		}

		public void TestCheckCGL_AdditionalIdentifier_WhenDeclarationTypeIsDAndQualifierIsEmpty_ShouldNotShowError()
		{
			var cusGoodsLocation = CreateCusGoodsLocation(EU.Business.EntrySubStyleList.Codes.PreliminaryDeclarationUnderCodeA);
			cusGoodsLocation.CGL_Qualifier = string.Empty;
			cusGoodsLocation.Unlocode = string.Empty;
			cusGoodsLocation.Validation.ValidateCGL_AdditionalIdentifier();
			AssertNoMessageErrors(cusGoodsLocation.UnlocodeInfo);
		}

		public void TestCheckCGL_AdditionalIdentifier_WhenDeclarationTypeIsDAndQualifierIsUAndUnlocodeIsEmpty_ShouldShowError()
		{
			var cusGoodsLocation = CreateCusGoodsLocation(EU.Business.EntrySubStyleList.Codes.PreliminaryDeclarationUnderCodeA);
			cusGoodsLocation.CGL_Qualifier = "U";
			cusGoodsLocation.Unlocode = string.Empty;
			cusGoodsLocation.Validation.ValidateCGL_AdditionalIdentifier();
			AssertHasMessageErrorContaining(cusGoodsLocation.UnlocodeInfo, "UNLOCODE required");
		}

		public void TestCheckCGL_AdditionalIdentifier_WhenDeclarationTypeIsDAndUnlocodeIsInvalid_ShouldShowError()
		{
			var cusGoodsLocation = CreateCusGoodsLocation(EU.Business.EntrySubStyleList.Codes.PreliminaryDeclarationUnderCodeA);
			cusGoodsLocation.CGL_Qualifier = "U";
			cusGoodsLocation.Unlocode = "ADZZZ";
			cusGoodsLocation.Validation.ValidateCGL_AdditionalIdentifier();
			AssertHasMessageErrorContaining(cusGoodsLocation.UnlocodeInfo, "The code you have selected is not in the list");
		}

		public void TestCheckCGL_AdditionalIdentifier_WhenDeclarationTypeIsDAndUnlocodeIsValid_ShouldNotShowError()
		{
			var unlocode = "ADZZZ";
			CreateCusCode(code: unlocode);

			var cusGoodsLocation = CreateCusGoodsLocation(EU.Business.EntrySubStyleList.Codes.PreliminaryDeclarationUnderCodeA);
			cusGoodsLocation.CGL_Qualifier = "U";
			cusGoodsLocation.Unlocode = unlocode;
			cusGoodsLocation.Validation.ValidateCGL_AdditionalIdentifier();
			AssertNoMessageErrors(cusGoodsLocation.UnlocodeInfo);
		}

		#endregion

		void CreateCusCode(string dataGroupingCode = "CDS", string codeType = "PORT", string code = "ADZZZ")
		{
			var today = ZDateTime.Now;
			var yesterday = today.AddDays(-1);
			var tomorrow = today.AddDays(1);

			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusCodeList(dataGroupingCode, codeType, code, yesterday, tomorrow);

			Factory.Save();
		}

		CusGoodsLocation CreateCusGoodsLocation(string declarationType)
		{
			var manifest = Factory.New<AsycudaManifestHeader>();
			var bill = manifest.Bills.AddNew();
			bill.ABL_ShipmentType = declarationType;

			var cusGoodsLocation = Factory.New<CusGoodsLocation>();
			cusGoodsLocation.CGL_ParentID = bill.PK;
			cusGoodsLocation.CGL_LocationUse = CusGoodsLocationUseList.Codes.Departure;
			cusGoodsLocation.CGL_ParentTableCode = "ABL";

			return cusGoodsLocation;
		}
	}
}
