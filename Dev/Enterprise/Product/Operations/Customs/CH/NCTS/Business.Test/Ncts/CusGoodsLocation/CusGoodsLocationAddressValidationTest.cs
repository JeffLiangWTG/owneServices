using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.CH.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CH.NCTS.Business.Testing;

[TestedType(typeof(CusGoodsLocationAddressValidation))]
sealed class CusGoodsLocationAddressValidationTest : BusinessObjectValidationTestCase
{
	public void TestCheckE2_GovRegNum() => CombineAssertions(() =>
	{
		var arrivalLocationAddress = CreateNctsHeader(EU.NCTS.Business.NctsMovementType.Codes.Arrival).ArrivalMovementHeader.GoodsLocation.Address;
		arrivalLocationAddress.IdentificationHolderPK = Representative.PK;

		arrivalLocationAddress.E2_GovRegNum = ZString.Empty;
		AssertHasMessageError("Arrival: Authorization Number is empty", arrivalLocationAddress.E2_GovRegNumInfo, MessageAuthorizationNoRequired);
		AssertNoMessageError("Arrival: Authorization Number is empty", arrivalLocationAddress.E2_GovRegNumInfo, MessageAuthorizationNoInvalid);

		arrivalLocationAddress.E2_GovRegNum = "002";
		AssertNoMessageError("Arrival: Invalid Authorisation Number", arrivalLocationAddress.E2_GovRegNumInfo, MessageAuthorizationNoRequired);
		AssertHasMessageError("Arrival: Invalid Authorization Number", arrivalLocationAddress.E2_GovRegNumInfo, MessageAuthorizationNoInvalid);

		arrivalLocationAddress.E2_GovRegNum = "001";
		AssertNoMessageError("Arrival: Valid Authorisation Number", arrivalLocationAddress.E2_GovRegNumInfo, MessageAuthorizationNoRequired);
		AssertNoMessageError("Arrival: Valid Authorisation Number", arrivalLocationAddress.E2_GovRegNumInfo, MessageAuthorizationNoInvalid);

		var departureLocationAddress = CreateNctsHeader(EU.NCTS.Business.NctsMovementType.Codes.Departure).MovementHeader.GoodsLocation.Address;
		departureLocationAddress.IdentificationHolderPK = Representative.PK;

		departureLocationAddress.E2_GovRegNum = ZString.Empty;
		AssertNoMessageError("Departure: Authorization Number is empty", departureLocationAddress.E2_GovRegNumInfo, MessageAuthorizationNoRequired);
		AssertNoMessageError("Departure: Authorization Number is empty", departureLocationAddress.E2_GovRegNumInfo, MessageAuthorizationNoInvalid);

		departureLocationAddress.E2_GovRegNum = "002";
		AssertNoMessageError("Departure: Invalid Authorisation Number", departureLocationAddress.E2_GovRegNumInfo, MessageAuthorizationNoRequired);
		AssertHasMessageError("Departure: Invalid Authorization Number", departureLocationAddress.E2_GovRegNumInfo, MessageAuthorizationNoInvalid);

		departureLocationAddress.E2_GovRegNum = "001";
		AssertNoMessageError("Departure: Valid Authorisation Number", departureLocationAddress.E2_GovRegNumInfo, MessageAuthorizationNoRequired);
		AssertNoMessageError("Departure: Valid Authorisation Number", departureLocationAddress.E2_GovRegNumInfo, MessageAuthorizationNoInvalid);
	});

	public void TestCheckE2_GovRegNum_Parent_NctsHeaderDepartureMessageSendingObject() => CombineAssertions(() =>
	{
		var sendingObject = new NctsHeaderDepartureMessageSendingObject(NctsHeader);
		var cusGoodsLocationAddress = sendingObject.GoodsLocation.Address;
		cusGoodsLocationAddress.IdentificationHolderPK = Representative.PK;

		cusGoodsLocationAddress.E2_GovRegNum = ZString.Empty;
		AssertHasError("Authorization Number is empty", cusGoodsLocationAddress.E2_GovRegNumInfo, MessageAuthorizationNoRequired);
		AssertNoMessageError("Authorization Number is empty", cusGoodsLocationAddress.E2_GovRegNumInfo, MessageAuthorizationNoInvalid);

		cusGoodsLocationAddress.E2_GovRegNum = "002";
		AssertNoError("Invalid Authorisation Number", cusGoodsLocationAddress.E2_GovRegNumInfo, MessageAuthorizationNoRequired);
		AssertHasMessageError("Invalid Authorization Number", cusGoodsLocationAddress.E2_GovRegNumInfo, MessageAuthorizationNoInvalid);

		cusGoodsLocationAddress.E2_GovRegNum = "001";
		AssertNoError("Valid Authorisation Number", cusGoodsLocationAddress.E2_GovRegNumInfo, MessageAuthorizationNoRequired);
		AssertNoMessageError("Valid Authorisation Number", cusGoodsLocationAddress.E2_GovRegNumInfo, MessageAuthorizationNoInvalid);
	});

	NctsHeader NctsHeader => nctsHeader ??= CreateNctsHeader();
	NctsHeader nctsHeader;

	NctsHeader CreateNctsHeader(string movementType = EU.NCTS.Business.NctsMovementType.Codes.Departure)
	{
		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.SetMovementType(movementType);
		return nctsHeader;
	}

	OrgHeader Representative => representative ??= CreateRepresentative();
	OrgHeader representative;

	OrgHeader CreateRepresentative()
	{
		var representative = Factory.NewWithValidTestData<OrgHeader>();
		CusAuthorisationHeaderTestHelper.CreateAuthorisationHeader(Factory, CusAuthorizationHeaderTypeList.Codes.AuthorizedLocationCodeForPassar, representative.PK, "001");
		CusAuthorisationHeaderTestHelper.CreateAuthorisationHeader(Factory, CusAuthorizationHeaderTypeList.Codes.AuthorizedConsigneeLocationsForEDec, representative.PK, "002");
		return representative;
	}

	const string MessageAuthorizationNoRequired = "[C0065] Location: Authorization No. required when qualifier is 'Y'.";
	const string MessageAuthorizationNoInvalid = "Approved Place Authorization Number is not valid.";
}
