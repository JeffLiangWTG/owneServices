using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.IE.H7.Business.Testing
{
	public class CusGoodsLocationValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCGL_Type()
		{
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.GoodsOfLocationType, "location type");
			var mockTypeCodes = new List<string> { "A", "B", "C", "D" };
			mockTypeCodes.ForEach(li => helper.CreateCusCodeList(Core.Constants.CountryCodes.Ireland,
				Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.GoodsOfLocationType,
				li,
				"desc",
				ZDateTime.MinSmallDateTimeValue,
				ZDateTime.MaxSmallDateTime));
			Factory.Save();

			var manifest = Factory.New<AsycudaManifestHeader>();
			var bill = manifest.Bills.AddNew();
			var cusGoodsLocation = Factory.New<CusGoodsLocation>();
			cusGoodsLocation.CGL_ParentID = bill.PK;
			cusGoodsLocation.CGL_LocationUse = CusGoodsLocationUseList.Codes.Departure;
			cusGoodsLocation.CGL_ParentTableCode = "ABL";
			bill.ABL_ShipmentType = "A";

			cusGoodsLocation.CGL_Type = "E";
			cusGoodsLocation.Validation.ValidateCGL_Type();
			AssertHasErrorContaining(cusGoodsLocation.CGL_TypeInfo, invalidCodeMessageError);

			cusGoodsLocation.CGL_Type = "A";
			cusGoodsLocation.Validation.ValidateCGL_Type();
			AssertNoNotifications(cusGoodsLocation.CGL_TypeInfo);

			bill.ABL_ShipmentType = "D";
			cusGoodsLocation.CGL_Type = "E";
			cusGoodsLocation.Validation.ValidateCGL_Type();
			AssertHasErrorContaining(cusGoodsLocation.CGL_TypeInfo, invalidCodeErrorWhenDeclarationTypeIsD);
		}

		public void TestCGL_Qualifier()
		{
			var manifest = Factory.New<AsycudaManifestHeader>();
			var bill = manifest.Bills.AddNew();
			var cusGoodsLocation = Factory.New<CusGoodsLocation>();
			cusGoodsLocation.CGL_ParentID = bill.PK;
			cusGoodsLocation.CGL_LocationUse = CusGoodsLocationUseList.Codes.Departure;
			cusGoodsLocation.CGL_ParentTableCode = "ABL";

			bill.ABL_ShipmentType = EU.Business.EntrySubStyleList.Codes.NormalDeclaration;

			cusGoodsLocation.CGL_Qualifier = "W";
			cusGoodsLocation.Validation.ValidateCGL_Qualifier();
			AssertHasErrorContaining(cusGoodsLocation.CGL_QualifierInfo, invalidCodeMessageError);

			cusGoodsLocation.CGL_Qualifier = "U";
			cusGoodsLocation.Validation.ValidateCGL_Qualifier();
			AssertNoMessageErrors(cusGoodsLocation.CGL_QualifierInfo);

			bill.ABL_ShipmentType = EU.Business.EntrySubStyleList.Codes.PreliminaryDeclarationUnderCodeA;

			cusGoodsLocation.CGL_Qualifier = "W";
			cusGoodsLocation.Validation.ValidateCGL_Qualifier();
			AssertHasErrorContaining(cusGoodsLocation.CGL_QualifierInfo, invalidCodeErrorWhenDeclarationTypeIsD);
		}

		public void TestCheckUnlocode()
		{
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(IE.Business.UniversalReferenceConstants.RefCusCodeListTypes.Codes.GoodsLocation, "location type");
			var mockTypeCodes = new List<string> { "A", "B", "C", "D" };
			mockTypeCodes.ForEach(li => helper.CreateCusCodeList(Core.Constants.CountryCodes.Ireland,
				IE.Business.UniversalReferenceConstants.RefCusCodeListTypes.Codes.GoodsLocation,
				li,
				"desc",
				ZDateTime.MinSmallDateTimeValue,
				ZDateTime.MaxSmallDateTime));
			Factory.Save();

			var manifest = Factory.New<AsycudaManifestHeader>();
			var bill = manifest.Bills.AddNew();
			var cusGoodsLocation = Factory.New<CusGoodsLocation>();
			cusGoodsLocation.CGL_ParentID = bill.PK;
			cusGoodsLocation.CGL_LocationUse = CusGoodsLocationUseList.Codes.Departure;
			cusGoodsLocation.CGL_ParentTableCode = "ABL";

			bill.ABL_ShipmentType = "A";
			cusGoodsLocation.CGL_Qualifier = "U";

			cusGoodsLocation.CGL_CustomsOffice = "E";
			cusGoodsLocation.Validation.ValidateCGL_CustomsOffice();
			AssertHasMessageErrorContaining(cusGoodsLocation.UnlocodeInfo, invalidCodeMessageError);

			cusGoodsLocation.CGL_CustomsOffice = "A";
			cusGoodsLocation.Validation.ValidateCGL_CustomsOffice();
			AssertNoMessageErrors(cusGoodsLocation.UnlocodeInfo);

			bill.ABL_ShipmentType = "D";
			cusGoodsLocation.CGL_CustomsOffice = "E";
			cusGoodsLocation.Validation.ValidateCGL_CustomsOffice();
			AssertNoMessageErrorContaining(cusGoodsLocation.UnlocodeInfo, invalidCodeMessageError);
			AssertHasMessageErrorContaining(cusGoodsLocation.UnlocodeInfo, "UNLOCODE shall not be populated");
		}

		const string invalidCodeErrorWhenDeclarationTypeIsD = "The value entered is invalid and should be removed for additional declaration type 'D'.";
		const string invalidCodeMessageError = "[BR0020] The code you have selected is not in the list.";
	}
}
