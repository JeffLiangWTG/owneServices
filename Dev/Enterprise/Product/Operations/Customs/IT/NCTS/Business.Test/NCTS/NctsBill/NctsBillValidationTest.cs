using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.Universal;
using RefDataGroupingCodes = Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes;
using ValidationCaptions = Enterprise.Customs.IT.Business.ValidationCaptions;

namespace Enterprise.Customs.IT.NCTS.Business.Testing;

sealed class NctsBillValidationTest : TestCaseWithFactory
{
	public void TestValidation()
	{
		AssertType<NctsBillValidation>("Validation Type", nctsBill.Validation);
	}

	public void TestCheckB0_BillStatus_ValidCode()
	{
		CombineAssertions( () =>
		{
			nctsBill.B0_BillStatus = "XXX";
			AssertHasErrorContaining(nctsBill.B0_BillStatusInfo, ListValidation.InvalidCodeError);
			nctsBill.B0_BillStatus = "NBD";
			AssertHasErrorContaining(nctsBill.B0_BillStatusInfo, ListValidation.InvalidCodeError);

			nctsBill.B0_BillStatus = "DEL";
			AssertNoErrors(nctsBill.B0_BillStatusInfo);
		});
	}

	public void TestCheckInlandTransportModeAtDeparture()
	{
		var expectedWarningMessage = ValidationCaptions.NctsBill.AllDepartureTransportMeansAreSame;
		var header = Factory.NewDepartureNctsHeaderPhase5();
		CombineAssertions(() =>
		{
			header.MovementHeader.InlandTransportModeAtDeparture = "2";
			header.MovementHeader.TransportTypeAtDeparture = ZString.Empty;
			var bill = header.Bills.AddNew();
			var bill2 = header.Bills.AddNew();

			using (SetNCTSTransitionPeriod(isActive: false))
			{
				bill.Validation.ValidateAll();
				AssertNoWarning("ValidateAll should trigger the check, No warning when AreAllDepartureTransportMeansFieldsEmpty is true", bill.InlandTransportModeAtDepartureInfo, expectedWarningMessage);

				header.MovementHeader.TransportTypeAtDeparture = "21";
				bill.Validation.ValidateInlandTransportModeAtDeparture();
				AssertNoWarning("No warning when IsTransportDepartureReadOnly is true", bill.InlandTransportModeAtDepartureInfo, expectedWarningMessage);
				header.MovementHeader.TransportTypeAtDeparture = ZString.Empty;

				bill.TransportTypeAtDeparture = "21";
				bill.TransportAtDeparture = "321";
				bill.TransportCountryAtDeparture = "IT";

				bill2.TransportTypeAtDeparture = "21";
				bill2.TransportAtDeparture = "321";
				bill2.TransportCountryAtDeparture = "IT";
				bill.Validation.ValidateInlandTransportModeAtDeparture();
				AssertHasWarning("Has warning when InlandTransportModeAtDeparture has value, TP OFF and AllBillsHaveSameDepartureTransportMeans is true", bill.InlandTransportModeAtDepartureInfo, expectedWarningMessage);

				bill2.TransportCountryAtDeparture = "US";
				bill.Validation.ValidateInlandTransportModeAtDeparture();
				AssertNoWarning("ValidateInlandTransportModeAtDeparture should trigger the check, no warning when AllBillsHaveSameDepartureTransportMeans is false", bill.InlandTransportModeAtDepartureInfo, expectedWarningMessage);
				bill2.TransportCountryAtDeparture = "IT";

				header.MovementHeader.InlandTransportModeAtDeparture = "";
				bill.Validation.ValidateInlandTransportModeAtDeparture();
				AssertNoWarning("No warning when InlandTransportModeAtDeparture is empty", bill.InlandTransportModeAtDepartureInfo, expectedWarningMessage);
				header.MovementHeader.InlandTransportModeAtDeparture = "2";
			}

			using (SetNCTSTransitionPeriod(isActive: true))
			{
				bill.Validation.ValidateInlandTransportModeAtDeparture();
				AssertNoWarning("No warning when TP ON", bill.InlandTransportModeAtDepartureInfo, expectedWarningMessage);
			}
		});
	}

	IDisposable SetNCTSTransitionPeriod(bool isActive) =>
		ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.NCTSTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, isActive);

	protected override void SetUp()
	{
		base.SetUp();
		nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
		nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		nctsBill = nctsHeader.Bills.AddNew();
	}

	NctsHeader nctsHeader;
	NctsBill nctsBill;
}
