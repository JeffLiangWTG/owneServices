using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	sealed class NctsInvoicingSupporterTest : TestCaseWithFactory
	{
		public void TestIsNCTSPhase4()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			var invoicingSupporter = nctsHeader.InvoicingSupporter;
			AssertEquals(false, invoicingSupporter.IsNCTSPhase4);

			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			AssertEquals(true, invoicingSupporter.IsNCTSPhase4);
		}

		public void TestContainerMode()
		{
			var departureNctsHeader = Factory.New<NctsHeader>();
			departureNctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			departureNctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			var movementHeader = departureNctsHeader.MovementHeader;
			var invoicingSupporter = departureNctsHeader.InvoicingSupporter;

			movementHeader.BM_InlandTransportMode = ModeOfTransportList.Codes._2_RailTransport;
			AssertEquals("NCTS 5, Departure, BM_InlandTransportMode is not Sea, there is no Container line.", Core.Constants.ContainerModes.NonContainerised, invoicingSupporter.ContainerMode);

			movementHeader.BM_InlandTransportMode = ModeOfTransportList.Codes._1_SeaTransport;
			AssertEquals("NCTS 5, Departure, BM_InlandTransportMode is Sea, there is no Container line.", Core.Constants.ContainerModes.LCL, invoicingSupporter.ContainerMode);

			var container = departureNctsHeader.DepartureHeaderContainers.AddNew();
			AssertEquals("NCTS 5, Departure, BM_InlandTransportMode is Sea, there is no Container line with Container mode 'CNT'.", Core.Constants.ContainerModes.LCL, invoicingSupporter.ContainerMode);

			container.BC_Mode = Core.Constants.ContainerModes.Containerised;
			AssertEquals("NCTS 5, Departure, BM_InlandTransportMode is Sea, there exists a Container line with Container mode 'CNT'.", Core.Constants.ContainerModes.FCL, invoicingSupporter.ContainerMode);

			movementHeader.BM_InlandTransportMode = ModeOfTransportList.Codes._2_RailTransport;
			AssertEquals("NCTS 5, Departure, BM_InlandTransportMode is not Sea, there exists a Container line with Container mode 'CNT'.", Core.Constants.ContainerModes.Containerised, invoicingSupporter.ContainerMode);

			departureNctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			movementHeader.BM_InlandTransportMode = ModeOfTransportList.Codes._1_SeaTransport;
			AssertEquals("NCTS 4, Departure, BM_InlandTransportMode is Sea, there is no Container line.", Core.Constants.ContainerModes.Containerised, invoicingSupporter.ContainerMode);

			var arrivalNctsHeader = Factory.New<NctsHeader>();
			arrivalNctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			arrivalNctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
			invoicingSupporter = arrivalNctsHeader.InvoicingSupporter;
			AssertEquals("NCTS 5, Arrival", Core.Constants.ContainerModes.NonContainerised, invoicingSupporter.ContainerMode);
		}

		public void TestTransportMode()
		{
			var departureNctsHeader = Factory.New<NctsHeader>();
			departureNctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			departureNctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			var movementHeader = departureNctsHeader.MovementHeader;
			var invoicingSupporter = departureNctsHeader.InvoicingSupporter;

			movementHeader.BM_InlandTransportMode = ModeOfTransportList.Codes._1_SeaTransport;
			AssertEquals("NCTS 5, Departure, BM_InlandTransportMode is Sea", Core.Constants.TransportModes.Sea, invoicingSupporter.TransportMode);

			movementHeader.BM_InlandTransportMode = ModeOfTransportList.Codes._2_RailTransport;
			AssertEquals("NCTS 5, Departure, BM_InlandTransportMode is Rail", Core.Constants.TransportModes.Rail, invoicingSupporter.TransportMode);

			movementHeader.BM_InlandTransportMode = ModeOfTransportList.Codes._3_RoadTransport;
			AssertEquals("NCTS 5, Departure, BM_InlandTransportMode is Road", Core.Constants.TransportModes.Road, invoicingSupporter.TransportMode);

			movementHeader.BM_InlandTransportMode = ModeOfTransportList.Codes._4_AirTransport;
			AssertEquals("NCTS 5, Departure, BM_InlandTransportMode is Air", Core.Constants.TransportModes.Air, invoicingSupporter.TransportMode);

			movementHeader.BM_InlandTransportMode = ModeOfTransportList.Codes._5_PostalConsignment;
			AssertEquals("NCTS 5, Departure, BM_InlandTransportMode is Post", Core.Constants.TransportModes.Mail, invoicingSupporter.TransportMode);

			movementHeader.BM_InlandTransportMode = ModeOfTransportList.Codes._7_FixedTransportInstallations;
			AssertEquals("NCTS 5, Departure, BM_InlandTransportMode is Fix", Core.Constants.TransportModes.Unknown, invoicingSupporter.TransportMode);

			movementHeader.BM_InlandTransportMode = ZString.Empty;
			AssertEquals("NCTS 5, Departure, BM_InlandTransportMode is empty", Core.Constants.TransportModes.Unknown, invoicingSupporter.TransportMode);

			departureNctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			AssertEquals("NCTS 4, Departure, BM_ExportTransportMode is empty", ZString.Empty, invoicingSupporter.TransportMode);

			movementHeader.BM_ExportTransportMode = ModeOfTransportList.Codes._7_FixedTransportInstallations;
			AssertEquals("NCTS 4, Departure, BM_ExportTransportMode is Fix", Core.Constants.TransportModes.FixedTransportInstallations, invoicingSupporter.TransportMode);

			var arrivalNctsHeader = Factory.New<NctsHeader>();
			arrivalNctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			arrivalNctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
			invoicingSupporter = arrivalNctsHeader.InvoicingSupporter;
			AssertEquals(Core.Constants.TransportModes.Other, invoicingSupporter.TransportMode);

			arrivalNctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			AssertEquals("NCTS 5, Arrival", ZString.Empty, invoicingSupporter.TransportMode);
		}
	}
}
