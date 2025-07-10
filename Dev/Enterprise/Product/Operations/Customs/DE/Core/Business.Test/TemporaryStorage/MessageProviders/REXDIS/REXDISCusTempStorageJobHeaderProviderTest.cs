using System;
using CargoWise.Customs.DE.MessageContracts.TemporaryStorage;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.DE.Business.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.DE.Business.CusTempStorage.Testing
{
	class REXDISCusTempStorageJobHeaderProviderTest : CusTempStorageJobHeaderProviderAbstractTest<REXDISCusTempStorageJobHeaderProvider>
	{
		public void TestFlightReferenceNumber()
		{
			tempStorageHeader.SJH_DepartureDate = new ZDate(2019, 2, 25);
			tempStorageHeader.SJH_TransportRegNo = "QF56A";

			Tuple<string, string>[] transportRegNumbersForTest =
			{
				Tuple.Create("M31", "M3 1 25.02.2019"),
				Tuple.Create("M31G", "M3 1 G 25.02.2019"),
				Tuple.Create("M312", "M3 12 25.02.2019"),
				Tuple.Create("M312H", "M3 12 H 25.02.2019"),
				Tuple.Create("M3125", "M3 125 25.02.2019"),
				Tuple.Create("M3125J", "M3 125 J 25.02.2019"),
				Tuple.Create("M31258", "M3 1258 25.02.2019"),
				Tuple.Create("M31258K", "M3 1258 K 25.02.2019"),

				Tuple.Create("SDO1", "SDO 1 25.02.2019"),
				Tuple.Create("SDO1G", "SDO 1 G 25.02.2019"),
				Tuple.Create("SDO12", "SDO 12 25.02.2019"),
				Tuple.Create("SDO12H", "SDO 12 H 25.02.2019"),
				Tuple.Create("SDO125", "SDO 125 25.02.2019"),
				Tuple.Create("SDO125J", "SDO 125 J 25.02.2019"),
				Tuple.Create("SDO1258", "SDO 1258 25.02.2019"),
				Tuple.Create("SDO1258K", "SDO 1258 K 25.02.2019"),
			};

			tempStorageHeader.SJH_DepartureDate = new ZDate(2019, 2, 25);
			foreach (var transportRegNo in transportRegNumbersForTest)
			{
				tempStorageHeader.SJH_TransportRegNo = transportRegNo.Item1;
				AssertEquals(transportRegNo.Item2, TempStorageHeaderWrapped.FlightReferenceNumber);
			}

			Tuple<ZDate, string>[] departureDatesForTest =
			{
				Tuple.Create(ZDate.Empty, "QF 1"),
				Tuple.Create(ZDate.Invalid, "QF 1"),
				Tuple.Create(new ZDate(2019, 4, 3), "QF 1 03.04.2019"),
			};

			tempStorageHeader.SJH_TransportRegNo = "QF1";
			foreach (var departureDate in departureDatesForTest)
			{
				tempStorageHeader.SJH_DepartureDate = departureDate.Item1;
				AssertEquals(departureDate.Item2, TempStorageHeaderWrapped.FlightReferenceNumber);
			}
		}

		public void TestCustomsAuthorisationNumber_NoCustodian()
		{
			AssertEquals(ZString.Empty, TempStorageHeaderWrapped.CustomsAuthorisationNumber);
		}

		public void TestCustomsAuthorisationNumber()
		{
			CombineAssertions(() =>
			{
				var presenter = Factory.New<OrgHeader>();
				tempStorageHeader.SJH_DepartureDate = ZDate.Today;
				tempStorageHeader.SJH_OA_Presenter = presenter.MainAddress.PK;
				AssertEquals("Has no AuthorisationNumber", ZString.Empty, TempStorageHeaderWrapped.CustomsAuthorisationNumber);
				presenter.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.ElectronicTransportDocument, "AUT123456");
				AssertEquals("Has AuthorisationNumber", "AUT123456", TempStorageHeaderWrapped.CustomsAuthorisationNumber);
				tempStorageHeader.SJH_DepartureDate = ZDate.Today.AddDays(-2);
				AssertEquals("Has AuthorisationNumber but DepartureDate out of range", ZString.Empty, TempStorageHeaderWrapped.CustomsAuthorisationNumber);
			});
		}

		protected override ITempStorageHeader GetTempStorageHeaderWrapped(CusTempStorageJobHeader tempStorageHeader) => new REXDISCusTempStorageJobHeaderProvider(tempStorageHeader);

		protected new IREXDISTempStorageHeader TempStorageHeaderWrapped => (IREXDISTempStorageHeader)base.TempStorageHeaderWrapped;
	}
}
