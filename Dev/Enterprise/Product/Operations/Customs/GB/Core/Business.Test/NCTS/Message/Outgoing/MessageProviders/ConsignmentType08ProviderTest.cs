using System;
using System.Linq;
using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.GB.Business.NCTS.Testing
{
	sealed class ConsignmentType08ProviderTest : DataProviderTestCase<ConsignmentType08Provider>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new ConsignmentType08Provider(null));
		}

		public void TestContainerIndicator() => CombineAssertions(() =>
		{
			AssertEquals("No HeaderContainers", false, Provider.ContainerIndicator);

			header.DepartureHeaderContainers.AddNew();
			AssertEquals("Has HeaderContainers", true, Provider.ContainerIndicator);
		});

		public void TestInlandModeOfTransport()
		{
			header.MovementHeader.BM_InlandTransportMode = "";
			AssertEquals(null, Provider.InlandModeOfTransport);
			header.MovementHeader.BM_InlandTransportMode = "0";
			AssertEquals(null, Provider.InlandModeOfTransport);
			header.MovementHeader.BM_InlandTransportMode = "1";
			AssertEquals("1", Provider.InlandModeOfTransport);
		}

		public void TestModeOfTransportAtTheBorder()
		{
			header.MovementHeader.BM_ExportTransportMode = "";
			AssertEquals(null, Provider.ModeOfTransportAtTheBorder);
			header.MovementHeader.BM_ExportTransportMode = "0";
			AssertEquals(null, Provider.ModeOfTransportAtTheBorder);
			header.MovementHeader.BM_ExportTransportMode = "2";
			AssertEquals("2", Provider.ModeOfTransportAtTheBorder);
		}

		public void TestTransportEquipments()
		{
			header.DepartureHeaderContainers.AddNew();
			header.DepartureHeaderContainers.AddNew();
			AssertArrayEqualsByElements(new[] { 1, 2 }, Provider.TransportEquipments.Select(x => x.SequenceNumber).ToArray());
		}

		public void TestLocationOfGoods()
		{
			AssertNotNull(Provider.LocationOfGoods);
		}

		public void TestDepartureTransportMeans()
		{
			header.MovementHeader.BM_TransportAtDeparture = "1";
			header.MovementHeader.BM_TransportAtDepartureTrailer1RegNo = "2";
			header.MovementHeader.BM_TransportAtDepartureTrailer2RegNo = "3";
			header.MovementHeader.BM_AircraftIDAtDeparture = "4";
			AssertEquals(4, Provider.DepartureTransportMeans.Count);
		}

		public void TestActiveBorderTransportMeans() => CombineAssertions(() =>
		{
			AssertType<ActiveBorderTransportMeansCollectionProvider>(Provider.ActiveBorderTransportMeans);
			AssertEquals("Mandatory fields are empty", 0, Provider.ActiveBorderTransportMeans.Count);

			header.MovementHeader.BM_ActiveBorderIdentificationType = "A";
			header.MovementHeader.BM_TOLCarrierID = "ABC123";
			header.MovementHeader.BM_RN_NKTOLCarrierNationality = "ES";
			AssertEquals("Mandatory fields populated", 1, GetProvider().ActiveBorderTransportMeans.Count);
		});

		public void TestPlaceOfLoading()
		{
			header.MovementHeader.BM_PortOfPresentationCode = "GBDVR";
			AssertNotNull(Provider.PlaceOfLoading.UnLocode);
		}

		public void TestHouseConsignments()
		{
			header.Bills.AddNew();
			header.Bills.AddNew();
			AssertArrayEqualsByElements(new[] { 1, 2 }, Provider.HouseConsignments.Select(x => x.SequenceNumber).ToArray());
		}

		protected override ConsignmentType08Provider GetProvider() => new ConsignmentType08Provider(header);

		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.New<NctsHeader>();
			header.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
		}
		NctsHeader header;
	}
}
