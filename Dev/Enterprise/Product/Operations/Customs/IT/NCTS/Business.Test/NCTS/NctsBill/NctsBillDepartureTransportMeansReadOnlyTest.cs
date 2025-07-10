using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using RefDataGroupingCodes = Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes;

namespace Enterprise.Customs.IT.NCTS.Business.Testing;

sealed class NctsBillDepartureTransportMeansReadOnlyTest : TestCaseWithFactory
{
	public void TestReadOnlyBaseOn_TransitionPeriodActive()
	{
		using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(
			Constants.FunctionalityTypes.NCTSTransitionPeriod,
			RefDataGroupingCodes.EuropeanUnionEUN,
			ZDate.Today,
			value: true))
		{
			AssertEmptyBillDepartureTransportFields(expectedReadOnly: true);
		}
	}

	public void TestReadOnlyBasedOn_BM_TransportAtDepartureType()
	{
		AssertReadOnlyBillDepartureTransportFieldsOnValueChange((nctsMovementHeader) => nctsMovementHeader.BM_TransportAtDepartureType = "2");
		AssertNotReadOnlyBillDepartureTransportFieldsOnValueChange((nctsMovementHeader) => nctsMovementHeader.BM_TransportAtDepartureType = "");
	}

	public void TestReadOnlyBasedOn_BM_TransportAtDeparture()
	{
		AssertReadOnlyBillDepartureTransportFieldsOnValueChange((nctsMovementHeader) => nctsMovementHeader.BM_TransportAtDeparture = "20");
		AssertNotReadOnlyBillDepartureTransportFieldsOnValueChange((nctsMovementHeader) => nctsMovementHeader.BM_TransportAtDeparture = "");
	}

	public void TestReadOnlyBasedOn_BM_RN_NKTransportAtDepartureCountry()
	{
		AssertReadOnlyBillDepartureTransportFieldsOnValueChange((nctsMovementHeader) => nctsMovementHeader.BM_RN_NKTransportAtDepartureCountry = "IT");
		AssertNotReadOnlyBillDepartureTransportFieldsOnValueChange((nctsMovementHeader) => nctsMovementHeader.BM_RN_NKTransportAtDepartureCountry = "");
	}

	public void TestReadOnlyBasedOn_BM_AircraftIDAtDeparture()
	{
		AssertReadOnlyBillDepartureTransportFieldsOnValueChange((nctsMovementHeader) => nctsMovementHeader.BM_AircraftIDAtDeparture = "AID");
		AssertNotReadOnlyBillDepartureTransportFieldsOnValueChange((nctsMovementHeader) => nctsMovementHeader.BM_AircraftIDAtDeparture = "");
	}

	public void TestReadOnlyBasedOn_BM_RN_NKTransportAtDepartureTrailer1Nationality()
	{
		AssertReadOnlyBillDepartureTransportFieldsOnValueChange((nctsMovementHeader) => nctsMovementHeader.BM_RN_NKTransportAtDepartureTrailer1Nationality = "IT");
		AssertNotReadOnlyBillDepartureTransportFieldsOnValueChange((nctsMovementHeader) => nctsMovementHeader.BM_RN_NKTransportAtDepartureTrailer1Nationality = "");
	}

	public void TestReadOnlyBasedOn_BM_RN_NKTransportAtDepartureTrailer2Nationality()
	{
		AssertReadOnlyBillDepartureTransportFieldsOnValueChange((nctsMovementHeader) => nctsMovementHeader.BM_RN_NKTransportAtDepartureTrailer2Nationality = "DE");
		AssertNotReadOnlyBillDepartureTransportFieldsOnValueChange((nctsMovementHeader) => nctsMovementHeader.BM_RN_NKTransportAtDepartureTrailer2Nationality = "");
	}

	public void TestReadOnlyBasedOn_BM_TransportAtDepartureTrailer1RegNo()
	{
		AssertReadOnlyBillDepartureTransportFieldsOnValueChange((nctsMovementHeader) => nctsMovementHeader.BM_TransportAtDepartureTrailer1RegNo = "T1");
		AssertNotReadOnlyBillDepartureTransportFieldsOnValueChange((nctsMovementHeader) => nctsMovementHeader.BM_TransportAtDepartureTrailer1RegNo = "");
	}

	public void TestReadOnlyBasedOn_BM_TransportAtDepartureTrailer2RegNo()
	{
		AssertReadOnlyBillDepartureTransportFieldsOnValueChange((nctsMovementHeader) => nctsMovementHeader.BM_TransportAtDepartureTrailer2RegNo = "T2");
		AssertNotReadOnlyBillDepartureTransportFieldsOnValueChange((nctsMovementHeader) => nctsMovementHeader.BM_TransportAtDepartureTrailer2RegNo = "");
	}

	protected override void SetUp()
	{
		base.SetUp();
		nctsHeader = Factory.NewDepartureNctsHeader();
		nctsHeader.BH_ApplicationCode = "NC5";
		nctsMovementHeader = nctsHeader.MovementHeader;
		nctsBill = nctsHeader.Bills.AddNew();
	}

	NctsHeader nctsHeader;
	NctsDepartureMovementHeader nctsMovementHeader;
	NctsBill nctsBill;

	void AssertReadOnlyBillDepartureTransportFieldsOnValueChange(Action<NctsDepartureMovementHeader> action)
	{
		action(nctsMovementHeader);
		AssertEmptyBillDepartureTransportFields(expectedReadOnly: true);
	}

	void AssertNotReadOnlyBillDepartureTransportFieldsOnValueChange(Action<NctsDepartureMovementHeader> action)
	{
		action(nctsMovementHeader);
		AssertEmptyBillDepartureTransportFields(expectedReadOnly: false);
	}

	void AssertEmptyBillDepartureTransportFields(bool expectedReadOnly)
	{
		CombineAssertions("Bill Departure Transport Fields ReadOnly", () =>
		{
			AssertEquals("VesselNameAtDeparture ReadOnly", expectedReadOnly, nctsBill.VesselNameAtDepartureInfo.ReadOnly);
			AssertEquals("VesselCountryAtDeparture ReadOnly", expectedReadOnly, nctsBill.VesselCountryAtDepartureInfo.ReadOnly);
			AssertEquals("TransportTypeAtDeparture ReadOnly", expectedReadOnly, nctsBill.TransportTypeAtDepartureInfo.ReadOnly);
			AssertEquals("TransportAtDeparture ReadOnly", expectedReadOnly, nctsBill.TransportAtDepartureInfo.ReadOnly);
			AssertEquals("TransportCountryAtDeparture ReadOnly", expectedReadOnly, nctsBill.TransportCountryAtDepartureInfo.ReadOnly);
			AssertEquals("Trailer1IDAtDeparture ReadOnly", expectedReadOnly, nctsBill.Trailer1IDAtDepartureInfo.ReadOnly);
			AssertEquals("Trailer1NationalityAtDeparture ReadOnly", expectedReadOnly, nctsBill.Trailer1NationalityAtDepartureInfo.ReadOnly);
			AssertEquals("Trailer2IDAtDeparture ReadOnly", expectedReadOnly, nctsBill.Trailer2IDAtDepartureInfo.ReadOnly);
			AssertEquals("Trailer2NationalityAtDeparture ReadOnly", expectedReadOnly, nctsBill.Trailer2NationalityAtDepartureInfo.ReadOnly);
			AssertEquals("AircraftIDAtDeparture ReadOnly", expectedReadOnly, nctsBill.AircraftIDAtDepartureInfo.ReadOnly);
		});
	}
}
