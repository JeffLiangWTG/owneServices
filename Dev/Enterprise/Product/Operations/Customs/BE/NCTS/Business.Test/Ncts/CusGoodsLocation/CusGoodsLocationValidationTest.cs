using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.BE.NCTS.Business.Testing;

sealed class CusGoodsLocationValidationTest : BusinessObjectValidationTestCase
{
	public void TestIsRuleNR0053Violated() => CombineAssertions(() =>
	{
		var goodsLocation = Factory.New<CusGoodsLocation>();
		goodsLocation.CGL_LocationUse = "ARR";
		var nctsHeader = Factory.NewWithValidTestData<NctsHeader>();
		nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
		goodsLocation.Parent = nctsHeader.MovementHeader;
		var movementHeader = (NctsDepartureMovementHeader)goodsLocation.Parent;
		movementHeader.BM_AdditionalDeclarationType = "A";

		goodsLocation.CGL_Type = ZString.Empty;
		goodsLocation.CGL_Qualifier = ZString.Empty;
		AssertEquals("both empty", false, goodsLocation.Validation.IsRuleNR0053Violated());

		goodsLocation.CGL_Type = "C";
		goodsLocation.CGL_Qualifier = ZString.Empty;
		AssertEquals("type filled, qualifier not U", true, goodsLocation.Validation.IsRuleNR0053Violated());

		goodsLocation.CGL_Type = ZString.Empty;
		goodsLocation.CGL_Qualifier = "U";
		AssertEquals("qualifier filled, type not C", true, goodsLocation.Validation.IsRuleNR0053Violated());
	});

	public void TestCheckCGL_Type_Phase5ArrivalIncident()
	{
		var nctsHeader = Factory.NewWithValidTestData<NctsHeader>();
		nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
		nctsHeader.ArrivalMovementHeader.AuthorizationCode = NctsArrivalAuthorizationCodeList.Codes.AuthorizationForTheStatusOfAuthorizedConsigneeForUnionTransit;
		var cusGoodsLocation = (CusGoodsLocation)nctsHeader.EnRouteIncidents.AddNew().GoodsLocation;

		CombineAssertions(() =>
		{
			cusGoodsLocation.CGL_Type = ZString.Empty;
			AssertNoMessageErrors("No validation when type is empty", cusGoodsLocation.CGL_TypeInfo);
		});
	}

	public void TestCheckCGL_AdditionalIdentifier_V()
	{
		const string message = "The customs office of the location of goods, must be a Belgian Customs Office that is 8 positions long.";
		arrivalMovementHeader.CustomsOffices.AddNew(OfficeCodes_NCTS.Codes.NCTSOfficeOfDestination);
		goodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.CustomsOfficeIdentifier;

		goodsLocation.CGL_AdditionalIdentifier = "DE123456";
		AssertNoMessageError("NCTS arrival", goodsLocation.CGL_AdditionalIdentifierInfo, message);

		nctsHeader.BH_HeaderType = NctsMovementType.Codes.Departure;
		goodsLocation = nctsHeader.MovementHeader.GoodsLocation;
		goodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.CustomsOfficeIdentifier;

		CombineAssertions(() =>
		{
			goodsLocation.CGL_AdditionalIdentifier = "DE123456";
			AssertNoMessageError("No customs office 'DEP'", goodsLocation.CGL_AdditionalIdentifierInfo, message);

			var depCustomsOffice = nctsHeader.MovementHeader.CustomsOfficesForDeparture.AddNew(OfficeCodes_NCTS.Codes.NCTSOfficeOfDeparture, "DE1");
			goodsLocation.Validation.ValidateCGL_AdditionalIdentifier();
			AssertNoMessageError("Customs office 'DEP' doesn't start with 'BE'", goodsLocation.CGL_AdditionalIdentifierInfo, message);

			depCustomsOffice.CY_Data = "BE1";
			goodsLocation.Validation.ValidateCGL_AdditionalIdentifier();
			AssertHasMessageError("Customs office 'DEP' starts with 'BE', location customs office doesn't start with 'BE'", goodsLocation.CGL_AdditionalIdentifierInfo, message);

			goodsLocation.CGL_AdditionalIdentifier = "BE123456";
			AssertNoMessageError("Customs office 'DEP' starts with 'BE', location customs office starts with 'BE'", goodsLocation.CGL_AdditionalIdentifierInfo, message);

			goodsLocation.CGL_AdditionalIdentifier = ZString.Empty;
			AssertHasMessageError("Customs office 'DEP' starts with 'BE', location customs office empty", goodsLocation.CGL_AdditionalIdentifierInfo, message);

			goodsLocation.CGL_AdditionalIdentifier = "BE12345";
			AssertHasMessageError("Customs office 'DEP' starts with 'BE', location customs office is not 8 chars long", goodsLocation.CGL_AdditionalIdentifierInfo, message);

			goodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.UnLocode;
			goodsLocation.Validation.ValidateCGL_AdditionalIdentifier();
			AssertNoMessageError("CGL_Qualifier <> 'V'", goodsLocation.CGL_AdditionalIdentifierInfo, message);
		});
	}

	public void TestCheckCGL_CustomsOffice()
	{
		const string message = "[C0394] Customs Office required when qualifier is 'V'.";
		goodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.CustomsOfficeIdentifier;
		goodsLocation.CGL_AdditionalIdentifier = "BE12345678";
		goodsLocation.Validation.ValidateCGL_CustomsOffice();
		AssertHasMessageError("When CGL_Qualifier = V and there is no customs office", goodsLocation.CGL_CustomsOfficeInfo, message);

		goodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.UnLocode;
		goodsLocation.Validation.ValidateCGL_CustomsOffice();
		AssertNoMessageErrors(goodsLocation.CGL_CustomsOfficeInfo);
	}

	protected override void SetUp()
	{
		base.SetUp();
		nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
		arrivalMovementHeader = nctsHeader.ArrivalMovementHeader;
		goodsLocation = arrivalMovementHeader.GoodsLocation;
	}
	NctsHeader nctsHeader;
	NctsArrivalMovementHeader arrivalMovementHeader;
	CusGoodsLocation goodsLocation;
}
