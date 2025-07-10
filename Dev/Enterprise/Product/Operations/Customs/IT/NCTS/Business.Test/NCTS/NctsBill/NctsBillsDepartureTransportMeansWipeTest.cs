using System;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.IT.NCTS.Business.Testing;

sealed class NctsBillsDepartureTransportMeansWipeTest : TestCaseWithFactory
{
	public void TestWipeDepartureTransportMeans_BM_TransportAtDepartureType()
	{
		SetupAndAssertWipeBillDepartureTransportFieldsOnValueChange((nctsMovementHeader) => nctsMovementHeader.BM_TransportAtDepartureType = "2");
		SetupAndAssertDoNotWipeBillDepartureTransportFieldsOnValueChange((nctsMovementHeader) => nctsMovementHeader.BM_TransportAtDepartureType = "");
	}

	public void TestWipeDepartureTransportMeans_BM_TransportAtDeparture()
	{
		SetupAndAssertWipeBillDepartureTransportFieldsOnValueChange((nctsMovementHeader) => nctsMovementHeader.BM_TransportAtDeparture = "20");
		SetupAndAssertDoNotWipeBillDepartureTransportFieldsOnValueChange((nctsMovementHeader) => nctsMovementHeader.BM_TransportAtDeparture = "");
	}

	public void TestWipeDepartureTransportMeans_BM_RN_NKTransportAtDepartureCountry()
	{
		SetupAndAssertWipeBillDepartureTransportFieldsOnValueChange((nctsMovementHeader) => nctsMovementHeader.BM_RN_NKTransportAtDepartureCountry = "IT");
		SetupAndAssertDoNotWipeBillDepartureTransportFieldsOnValueChange((nctsMovementHeader) => nctsMovementHeader.BM_RN_NKTransportAtDepartureCountry = "");
	}

	public void TestWipeDepartureTransportMeans_BM_AircraftIDAtDeparture()
	{
		SetupAndAssertWipeBillDepartureTransportFieldsOnValueChange((nctsMovementHeader) => nctsMovementHeader.BM_AircraftIDAtDeparture = "AID");
		SetupAndAssertDoNotWipeBillDepartureTransportFieldsOnValueChange((nctsMovementHeader) => nctsMovementHeader.BM_AircraftIDAtDeparture = "");
	}

	public void TestWipeDepartureTransportMeans_BM_RN_NKTransportAtDepartureTrailer1Nationality()
	{
		SetupAndAssertWipeBillDepartureTransportFieldsOnValueChange((nctsMovementHeader) => nctsMovementHeader.BM_RN_NKTransportAtDepartureTrailer1Nationality = "IT");
		SetupAndAssertDoNotWipeBillDepartureTransportFieldsOnValueChange((nctsMovementHeader) => nctsMovementHeader.BM_RN_NKTransportAtDepartureTrailer1Nationality = "");
	}

	public void TestWipeDepartureTransportMeans_BM_RN_NKTransportAtDepartureTrailer2Nationality()
	{
		SetupAndAssertWipeBillDepartureTransportFieldsOnValueChange((nctsMovementHeader) => nctsMovementHeader.BM_RN_NKTransportAtDepartureTrailer2Nationality = "DE");
		SetupAndAssertDoNotWipeBillDepartureTransportFieldsOnValueChange((nctsMovementHeader) => nctsMovementHeader.BM_RN_NKTransportAtDepartureTrailer2Nationality = "");
	}

	public void TestWipeDepartureTransportMeans_BM_TransportAtDepartureTrailer1RegNo()
	{
		SetupAndAssertWipeBillDepartureTransportFieldsOnValueChange((nctsMovementHeader) => nctsMovementHeader.BM_TransportAtDepartureTrailer1RegNo = "T1");
		SetupAndAssertDoNotWipeBillDepartureTransportFieldsOnValueChange((nctsMovementHeader) => nctsMovementHeader.BM_TransportAtDepartureTrailer1RegNo = "");
	}

	public void TestWipeDepartureTransportMeans_BM_TransportAtDepartureTrailer2RegNo()
	{
		SetupAndAssertWipeBillDepartureTransportFieldsOnValueChange((nctsMovementHeader) => nctsMovementHeader.BM_TransportAtDepartureTrailer2RegNo = "T2");
		SetupAndAssertDoNotWipeBillDepartureTransportFieldsOnValueChange((nctsMovementHeader) => nctsMovementHeader.BM_TransportAtDepartureTrailer2RegNo = "");
	}

	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("When movementHeader is null",
			() => new NctsBillsDepartureTransportMeansWiper(movementHeader: null));
	}

	public void TestEventsUnHookedOnDispose()
	{
		SetupAndAssertWipeBillDepartureTransportFieldsOnValueChange((nctsMovementHeader) => nctsMovementHeader.BM_TransportAtDepartureType = "2");

		var disposable = (IDisposable)nctsMovementHeader.BillsDepartureTransportMeansWiper;
		disposable.Dispose();

		SetupAndAssertDoNotWipeBillDepartureTransportFieldsOnValueChange((nctsMovementHeader) => nctsMovementHeader.BM_TransportAtDepartureType = "2");
	}

	protected override void SetUp()
	{
		base.SetUp();
		nctsHeader = Factory.NewDepartureNctsHeader();
		nctsMovementHeader = nctsHeader.MovementHeader;
		nctsBill = nctsHeader.Bills.AddNew();
	}

	NctsHeader nctsHeader;
	NctsDepartureMovementHeader nctsMovementHeader;
	NctsBill nctsBill;

	void SetupAndAssertWipeBillDepartureTransportFieldsOnValueChange(Action<NctsDepartureMovementHeader> action)
	{
		SetUpBillDepartureTransportFields();
		action(nctsMovementHeader);
		AssertEmptyBillDepartureTransportFields();
	}

	void AssertEmptyBillDepartureTransportFields()
	{
		CombineAssertions("Bill Departure Transport Fields Empty", () =>
		{
			AssertEquals("VesselNameAtDeparture", "", nctsBill.VesselNameAtDeparture);
			AssertEquals("VesselCountryAtDeparture", "", nctsBill.VesselCountryAtDeparture);
			AssertEquals("TransportTypeAtDeparture", "", nctsBill.TransportTypeAtDeparture);
			AssertEquals("TransportAtDeparture", "", nctsBill.TransportAtDeparture);
			AssertEquals("TransportCountryAtDeparture", "", nctsBill.TransportCountryAtDeparture);
			AssertEquals("Trailer1IDAtDeparture", "", nctsBill.Trailer1IDAtDeparture);
			AssertEquals("Trailer1NationalityAtDeparture", "", nctsBill.Trailer1NationalityAtDeparture);
			AssertEquals("Trailer2IDAtDeparture", "", nctsBill.Trailer2IDAtDeparture);
			AssertEquals("Trailer2NationalityAtDeparture", "", nctsBill.Trailer2NationalityAtDeparture);
			AssertEquals("AircraftIDAtDeparture", "", nctsBill.AircraftIDAtDeparture);
			AssertEquals("TransportDepartureAdditionalWagonNumbers Count", 0, nctsBill.TransportDepartureAdditionalWagonNumbers.Count);
		});
	}

	void SetupAndAssertDoNotWipeBillDepartureTransportFieldsOnValueChange(Action<NctsDepartureMovementHeader> action)
	{
		SetUpBillDepartureTransportFields();
		action(nctsMovementHeader);
		AssertFilledBillDepartureTransportFields();
	}

	void AssertFilledBillDepartureTransportFields()
	{
		CombineAssertions("Bill Departure Transport Fields Filled", () =>
		{
			AssertNotEquals("VesselNameAtDeparture", "", nctsBill.VesselNameAtDeparture);
			AssertNotEquals("VesselCountryAtDeparture", "", nctsBill.VesselCountryAtDeparture);
			AssertNotEquals("TransportTypeAtDeparture", "", nctsBill.TransportTypeAtDeparture);
			AssertNotEquals("TransportAtDeparture", "", nctsBill.TransportAtDeparture);
			AssertNotEquals("TransportCountryAtDeparture", "", nctsBill.TransportCountryAtDeparture);
			AssertNotEquals("Trailer1IDAtDeparture", "", nctsBill.Trailer1IDAtDeparture);
			AssertNotEquals("Trailer1NationalityAtDeparture", "", nctsBill.Trailer1NationalityAtDeparture);
			AssertNotEquals("Trailer2IDAtDeparture", "", nctsBill.Trailer2IDAtDeparture);
			AssertNotEquals("Trailer2NationalityAtDeparture", "", nctsBill.Trailer2NationalityAtDeparture);
			AssertNotEquals("AircraftIDAtDeparture", "", nctsBill.AircraftIDAtDeparture);
			AssertEquals("TransportDepartureAdditionalWagonNumbers Count", 1, nctsBill.TransportDepartureAdditionalWagonNumbers.Count);
		});
	}

	void SetUpBillDepartureTransportFields()
	{
		nctsBill.VesselNameAtDeparture = "VN";
		nctsBill.VesselCountryAtDeparture = "VC";
		nctsBill.TransportTypeAtDeparture = "TT";
		nctsBill.TransportAtDeparture = "TD";
		nctsBill.TransportCountryAtDeparture = "TC";
		nctsBill.Trailer1IDAtDeparture = "T1ID";
		nctsBill.Trailer1NationalityAtDeparture = "T1";
		nctsBill.Trailer2IDAtDeparture = "T2ID";
		nctsBill.Trailer2NationalityAtDeparture = "T2";
		nctsBill.AircraftIDAtDeparture = "AID";
		nctsBill.TransportDepartureAdditionalWagonNumbers.AddNew();
	}
}
