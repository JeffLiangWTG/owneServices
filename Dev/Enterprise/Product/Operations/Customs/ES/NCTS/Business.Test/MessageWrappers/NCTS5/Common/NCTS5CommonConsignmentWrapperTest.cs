using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.ES.Business.Testing;
using Enterprise.Customs.ES.NCTS.Business.MessageWrappers;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.ES.NCTS.Business.Testing
{
	public class NCTS5CommonConsignmentWrapperTest : WrapperHelperTest<NCTS5CommonConsignmentWrapper>
	{
		public void TestConstructor()
		{
			CombineAssertions(() =>
			{
				AssertExceptionThrown("Constructor Throws Exception if header is null", typeof(ArgumentNullException),
				ArgumentExceptionMessageHelper.GetArgumentExceptionMessage("Value cannot be null.", "header"), () => GetWrapper(null));

				AssertExceptionThrown("Constructor Throws Exception if MovementHeader is null when flag isArrivalDeclaration is false", typeof(ArgumentNullException),
				ArgumentExceptionMessageHelper.GetArgumentExceptionMessage("Value cannot be null.", "MovementHeader"), () => GetWrapper(Factory.New<NctsHeader>()));

				AssertExceptionThrown("Constructor Throws Exception if ArrivalMovementHeader is null when flag isArrivalDeclaration is true", typeof(ArgumentNullException),
				ArgumentExceptionMessageHelper.GetArgumentExceptionMessage("Value cannot be null.", "ArrivalMovementHeader"), () => GetWrapperArrival(Factory.New<NctsHeader>()));
			});
		}

		public void TestTransportEquipment()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Expected empty TransportEquipment when no data declared", 0, wrapper.TransportEquipment.Count);

				var cont1 = nctsHeader.DepartureHeaderContainers.AddNew();
				cont1.BC_Mode = "NCT";
				cont1.BC_ContainerNum = "CONT1";
				cont1.Seal1 = "Seal1";
				var cont2 = nctsHeader.DepartureHeaderContainers.AddNew();
				cont2.BC_Mode = "NCT";
				cont2.BC_ContainerNum = "CONT2";
				cont2.Seal1 = "Seal2";
				cont2.Seal2 = "Seal3";
				var cont3 = nctsHeader.DepartureHeaderContainers.AddNew();
				cont3.BC_Mode = "CNT";
				cont3.BC_ContainerNum = "CONT3";
				cont3.Seal1 = "Seal4";
				cont3.Seal2 = "Seal5";
				cont3.AdditionalSeals.AddNew().BK_SealNumber = "Seal6";
				var cont4 = nctsHeader.DepartureHeaderContainers.AddNew();
				cont4.BC_Mode = "CNT";
				cont4.BC_ContainerNum = "CONT4";

				cont1.BC_SequenceNumber = 8;
				cont2.BC_SequenceNumber = 5;
				cont3.BC_SequenceNumber = 10;
				cont4.BC_SequenceNumber = 6;

				AssertEquals("Prereq: cont1.BC_SequenceNumber", (ZShort)8, cont1.BC_SequenceNumber);
				AssertEquals("Prereq: cont2.BC_SequenceNumber", (ZShort)5, cont2.BC_SequenceNumber);
				AssertEquals("Prereq: cont3.BC_SequenceNumber", (ZShort)10, cont3.BC_SequenceNumber);
				AssertEquals("Prereq: cont4.BC_SequenceNumber", (ZShort)6, cont4.BC_SequenceNumber);

				wrapper = GetWrapper(nctsHeader);
				AssertEquals("Expected empty TransportEquipment when containers declared in header but not associated in goods items", 0, wrapper.TransportEquipment.Count);

				var nctsBill = nctsHeader.Bills.AddNew();
				var goodsItem1 = nctsBill.GoodsItems.AddNew();
				var goodsItem2 = nctsBill.GoodsItems.AddNew();
				var goodsItem3 = nctsBill.GoodsItems.AddNew();
				var goodsItem4 = nctsBill.GoodsItems.AddNew();

				AddContainerToGoodsItem(goodsItem1, cont1);
				AddContainerToGoodsItem(goodsItem2, cont2);
				AddContainerToGoodsItem(goodsItem3, cont3);
				AddContainerToGoodsItem(goodsItem4, cont4);

				wrapper = GetWrapper(nctsHeader);
				var transportEquipment = wrapper.TransportEquipment;

				AssertEquals("Expected filled TransportEquipment with 4 elements when all containers are associated to goods items", 4, transportEquipment.Count);
				AssertSame("Cached TransportEquipment", wrapper.TransportEquipment, transportEquipment);

				var transportEquipmentArray = transportEquipment.ToArray();
				AssertEquals("Expected filled TransportEquipment[0].SequenceNumber", "5", transportEquipmentArray[0].SequenceNumber);
				AssertEquals("Expected empty when NCT TransportEquipment[0].ContainerIdentificationNumber (cont2)", ZString.Empty, transportEquipmentArray[0].ContainerIdentificationNumber);
				AssertEquals("Expected filled TransportEquipment[0].NumberOfSeals", "2", transportEquipmentArray[0].NumberOfSeals);

				AssertEquals("Expected filled TransportEquipment[1].SequenceNumber", "6", transportEquipmentArray[1].SequenceNumber);
				AssertEquals("Expected filled TransportEquipment[1].ContainerIdentificationNumber", "CONT4", transportEquipmentArray[1].ContainerIdentificationNumber);
				AssertEquals("Expected filled TransportEquipment[1].NumberOfSeals", "0", transportEquipmentArray[1].NumberOfSeals);

				AssertEquals("Expected filled TransportEquipment[2].SequenceNumber", "8", transportEquipmentArray[2].SequenceNumber);
				AssertEquals("Expected empty when NCT TransportEquipment[2].ContainerIdentificationNumber (cont1)", ZString.Empty, transportEquipmentArray[2].ContainerIdentificationNumber);
				AssertEquals("Expected filled TransportEquipment[2].NumberOfSeals", "1", transportEquipmentArray[2].NumberOfSeals);

				AssertEquals("Expected filled TransportEquipment[3].SequenceNumber", "10", transportEquipmentArray[3].SequenceNumber);
				AssertEquals("Expected filled TransportEquipment[3].ContainerIdentificationNumber", "CONT3", transportEquipmentArray[3].ContainerIdentificationNumber);
				AssertEquals("Expected filled TransportEquipment[3].NumberOfSeals", "3", transportEquipmentArray[3].NumberOfSeals);
			});

			void AddContainerToGoodsItem(NctsDepartureCargoDesc goodsItem, NctsDepartureHeaderContainer container)
			{
				var package = goodsItem.Packages.AddNew();
				package.ContainersPivot.AddPivotFor(container);
			}
		}

		public void TestTransportEquipment_Arrival()
		{
			var (wrapperArrival, _) = SetUpForArrival();

			CombineAssertions(() =>
			{
				AssertEquals("Expected empty TransportEquipment when no data declared", 0, wrapperArrival.TransportEquipment.Count);

				var cont1 = nctsHeader.ArrivalHeaderContainers.AddNew();
				cont1.BC_Mode = "NCT";
				cont1.BC_ContainerNum = "CONT1";
				cont1.BC_UnloadedState = "DIF";
				var seal1 = cont1.Seals.AddNew();
				seal1.BK_SealNumber = "Seal1";
				seal1.BK_UnloadingState = "DIF";
				var seal2 = cont1.Seals.AddNew();
				seal2.BK_SealNumber = "Seal2";
				seal2.BK_UnloadingState = "MIS";

				var cont2 = nctsHeader.ArrivalHeaderContainers.AddNew();
				cont2.BC_Mode = "NCT";
				cont2.BC_ContainerNum = "CONT2";
				cont2.BC_UnloadedState = "MIS";
				var seal3 = cont2.Seals.AddNew();
				seal3.BK_SealNumber = "Seal3";
				seal3.BK_UnloadingState = "NEW";

				var cont3 = nctsHeader.ArrivalHeaderContainers.AddNew();
				cont3.BC_Mode = "CNT";
				cont3.BC_ContainerNum = "CONT3";
				cont3.BC_UnloadedState = "NEW";
				var seal4 = cont3.Seals.AddNew();
				seal4.BK_SealNumber = "Seal4";
				seal4.BK_UnloadingState = "NEW";
				var seal5 = cont3.Seals.AddNew();
				seal5.BK_SealNumber = "Seal5";
				seal5.BK_UnloadingState = "DIF";

				var cont4 = nctsHeader.ArrivalHeaderContainers.AddNew();
				cont4.BC_Mode = "CNT";
				cont4.BC_ContainerNum = "CONT4";
				cont4.BC_UnloadedState = "DEC";
				var seal6 = cont4.Seals.AddNew();
				seal6.BK_SealNumber = "Seal6";
				seal6.BK_UnloadingState = "MIS";

				var cont5 = nctsHeader.ArrivalHeaderContainers.AddNew();
				cont5.BC_Mode = "CNT";
				cont5.BC_ContainerNum = "CONT5";
				cont5.BC_UnloadedState = "DEC";

				var cont6 = nctsHeader.ArrivalHeaderContainers.AddNew();
				cont6.BC_Mode = "CNT";
				cont6.BC_ContainerNum = "CONT6";
				cont6.BC_UnloadedState = "NEW";

				var cont7 = nctsHeader.ArrivalHeaderContainers.AddNew();
				cont7.BC_Mode = "CNT";
				cont7.BC_ContainerNum = "CONT7";
				cont7.BC_UnloadedState = "DEC";
				var seal7 = cont7.Seals.AddNew();
				seal7.BK_SealNumber = "Seal7";
				seal7.BK_UnloadingState = "NEW";

				cont1.BC_SequenceNumber = 8;
				cont2.BC_SequenceNumber = 5;
				cont3.BC_SequenceNumber = 10;
				cont4.BC_SequenceNumber = 6;
				cont5.BC_SequenceNumber = 3;
				cont6.BC_SequenceNumber = 4;
				cont7.BC_SequenceNumber = 12;

				AssertEquals("Prereq: cont1.BC_SequenceNumber", (ZShort)8, cont1.BC_SequenceNumber);
				AssertEquals("Prereq: cont2.BC_SequenceNumber", (ZShort)5, cont2.BC_SequenceNumber);
				AssertEquals("Prereq: cont3.BC_SequenceNumber", (ZShort)10, cont3.BC_SequenceNumber);
				AssertEquals("Prereq: cont4.BC_SequenceNumber", (ZShort)6, cont4.BC_SequenceNumber);
				AssertEquals("Prereq: cont5.BC_SequenceNumber", (ZShort)3, cont5.BC_SequenceNumber);
				AssertEquals("Prereq: cont6.BC_SequenceNumber", (ZShort)4, cont6.BC_SequenceNumber);
				AssertEquals("Prereq: cont7.BC_SequenceNumber", (ZShort)12, cont7.BC_SequenceNumber);

				wrapperArrival = GetWrapperArrival(nctsHeader);
				var transportEquipment = wrapperArrival.TransportEquipment;
				AssertEquals("Expected filled TransportEquipment when containers declared in header but not associated in goods items but have status DIF or MIS (only those are declared)", 4, transportEquipment.Count);
				AssertSame("Cached TransportEquipment", wrapperArrival.TransportEquipment, transportEquipment);

				var transportEquipmentArray = transportEquipment.ToArray();
				AssertEquals("Expected filled TransportEquipment[0].SequenceNumber", "5", transportEquipmentArray[0].SequenceNumber);
				AssertEquals("Expected empty TransportEquipment[0].ContainerIdentificationNumber when unloaded state is MIS", ZString.Empty, transportEquipmentArray[0].ContainerIdentificationNumber);
				AssertEquals("Expected empty TransportEquipment[0].NumberOfSeals when unloaded state is MIS", ZString.Empty, transportEquipmentArray[0].NumberOfSeals);

				AssertEquals("Expected filled TransportEquipment[1].SequenceNumber", "6", transportEquipmentArray[1].SequenceNumber);
				AssertEquals("Expected empty TransportEquipment[1].ContainerIdentificationNumber when unloaded state is not NEW or DIF", ZString.Empty, transportEquipmentArray[1].ContainerIdentificationNumber);
				AssertEquals("Expected filled TransportEquipment[1].NumberOfSeals when unloaded state is not NEW or DIF", ZString.Empty, transportEquipmentArray[1].NumberOfSeals);

				AssertEquals("Expected filled TransportEquipment[2].SequenceNumber", "8", transportEquipmentArray[2].SequenceNumber);
				AssertEquals("Expected empty TransportEquipment[2].ContainerIdentificationNumber when NCT", ZString.Empty, transportEquipmentArray[2].ContainerIdentificationNumber);
				AssertEquals("Expected filled TransportEquipment[2].NumberOfSeals with one DIF and one MIS seal", "1", transportEquipmentArray[2].NumberOfSeals);

				AssertEquals("Expected filled TransportEquipment[3].SequenceNumber", "10", transportEquipmentArray[3].SequenceNumber);
				AssertEquals("Expected filled TransportEquipment[3].ContainerIdentificationNumber", "CONT3", transportEquipmentArray[3].ContainerIdentificationNumber);
				AssertEquals("Expected filled TransportEquipment[3].NumberOfSeals", "2", transportEquipmentArray[3].NumberOfSeals);

				var nctsBill = nctsHeader.Bills.AddNew();
				var goodsItem1 = nctsBill.ArrivalGoodsItems.AddNew();
				var goodsItem2 = nctsBill.ArrivalGoodsItems.AddNew();

				AddContainerToGoodsItem(goodsItem1, cont6);
				AddContainerToGoodsItem(goodsItem2, cont7);

				wrapperArrival = GetWrapperArrival(nctsHeader);
				transportEquipment = wrapperArrival.TransportEquipment;

				AssertEquals("Expected filled TransportEquipment with 6 elements when NEW containers are associated to goods items", 6, transportEquipment.Count);
				AssertSame("Cached TransportEquipment", wrapperArrival.TransportEquipment, transportEquipment);

				transportEquipmentArray = transportEquipment.ToArray();
				AssertEquals("Expected filled TransportEquipment[0].SequenceNumber", "4", transportEquipmentArray[0].SequenceNumber);
				AssertEquals("Expected filled TransportEquipment[0].ContainerIdentificationNumber", "CONT6", transportEquipmentArray[0].ContainerIdentificationNumber);
				AssertEquals("Expected filled TransportEquipment[0].NumberOfSeals", "0", transportEquipmentArray[0].NumberOfSeals);

				AssertEquals("Expected filled TransportEquipment[1].SequenceNumber", "5", transportEquipmentArray[1].SequenceNumber);
				AssertEquals("Expected empty TransportEquipment[1].ContainerIdentificationNumber when unloaded state is MIS", ZString.Empty, transportEquipmentArray[1].ContainerIdentificationNumber);
				AssertEquals("Expected empty TransportEquipment[1].NumberOfSeals when unloaded state is MIS", ZString.Empty, transportEquipmentArray[1].NumberOfSeals);

				AssertEquals("Expected filled TransportEquipment[2].SequenceNumber", "6", transportEquipmentArray[2].SequenceNumber);
				AssertEquals("Expected empty TransportEquipment[2].ContainerIdentificationNumber when unloaded state is not NEW or DIF", ZString.Empty, transportEquipmentArray[2].ContainerIdentificationNumber);
				AssertEquals("Expected filled TransportEquipment[2].NumberOfSeals when unloaded state is not NEW or DIF", ZString.Empty, transportEquipmentArray[2].NumberOfSeals);

				AssertEquals("Expected filled TransportEquipment[3].SequenceNumber", "8", transportEquipmentArray[3].SequenceNumber);
				AssertEquals("Expected empty TransportEquipment[3].ContainerIdentificationNumber when NCT", ZString.Empty, transportEquipmentArray[3].ContainerIdentificationNumber);
				AssertEquals("Expected filled TransportEquipment[3].NumberOfSeals with one DIF and one MIS seal", "1", transportEquipmentArray[3].NumberOfSeals);

				AssertEquals("Expected filled TransportEquipment[4].SequenceNumber", "10", transportEquipmentArray[4].SequenceNumber);
				AssertEquals("Expected filled TransportEquipment[4].ContainerIdentificationNumber", "CONT3", transportEquipmentArray[4].ContainerIdentificationNumber);
				AssertEquals("Expected filled TransportEquipment[4].NumberOfSeals", "2", transportEquipmentArray[4].NumberOfSeals);

				AssertEquals("Expected filled TransportEquipment[5].SequenceNumber", "12", transportEquipmentArray[5].SequenceNumber);
				AssertEquals("Expected empty TransportEquipment[5].ContainerIdentificationNumber when unloaded state is not NEW or DIF", ZString.Empty, transportEquipmentArray[5].ContainerIdentificationNumber);
				AssertEquals("Expected filled TransportEquipment[5].NumberOfSeals when unloaded state is not NEW or DIF", ZString.Empty, transportEquipmentArray[5].NumberOfSeals);
			});

			void AddContainerToGoodsItem(NctsArrivalCargoDesc goodsItem, NctsArrivalHeaderContainer container)
			{
				var package = goodsItem.Packages.AddNew();
				package.ContainersPivot.AddPivotFor(container);
			}
		}

		public void TestDepartureTransportMeans()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Expected empty DepartureTransportMeans when no data declared", 0, wrapper.DepartureTransportMeans.Count);

				departureMovement.InlandTransportModeAtDeparture = ModeOfTransportList.Codes._1_SeaTransport;
				departureMovement.TransportTypeAtDeparture = "11";
				departureMovement.VesselNameAtDeparture = "Vessel";
				departureMovement.VesselCountryAtDeparture = "ES";
				wrapper = GetWrapper(nctsHeader);
				var departureTransportMeans = wrapper.DepartureTransportMeans;

				AssertEquals("Expected filled DepartureTransportMeans with 1 element when inland mode is not 3", 1, departureTransportMeans.Count);
				AssertSame("Cached DepartureTransportMeans", wrapper.DepartureTransportMeans, departureTransportMeans);

				var departureTransportMeansElement = departureTransportMeans.FirstOrDefault();
				AssertEquals("Expected filled DepartureTransportMeans[0].SequenceNumber", "1", departureTransportMeansElement.SequenceNumber);
				AssertEquals("Expected filled DepartureTransportMeans[0].TransportMode", "11", departureTransportMeansElement.TransportMode);
				AssertEquals("Expected filled DepartureTransportMeans[0].TransportId", "Vessel", departureTransportMeansElement.TransportId);
				AssertEquals("Expected filled DepartureTransportMeans[0].TransportNationality", "ES", departureTransportMeansElement.TransportNationality);
			});
		}

		public void TestDepartureTransportMeans_MOT1()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Expected empty DepartureTransportMeans when no data declared", 0, wrapper.DepartureTransportMeans.Count);

				departureMovement.InlandTransportModeAtDeparture = ModeOfTransportList.Codes._1_SeaTransport;
				departureMovement.TransportTypeAtDeparture = "10";
				departureMovement.VesselNameAtDeparture = "Vessel";
				departureMovement.VesselCountryAtDeparture = "ES";
				wrapper = GetWrapper(nctsHeader);
				var departureTransportMeans = wrapper.DepartureTransportMeans;
				AssertEquals("Expected filled DepartureTransportMeans with 1 element", 1, departureTransportMeans.Count);
				AssertSame("Cached DepartureTransportMeans", wrapper.DepartureTransportMeans, departureTransportMeans);
				var departureTransportMeansElement = departureTransportMeans.FirstOrDefault();
				AssertEquals("Expected filled DepartureTransportMeans[0].SequenceNumber", "1", departureTransportMeansElement.SequenceNumber);
				AssertEquals("Expected filled DepartureTransportMeans[0].TransportMode", "10", departureTransportMeansElement.TransportMode);
				AssertEquals("Expected filled DepartureTransportMeans[0].TransportId", "Vessel", departureTransportMeansElement.TransportId);
				AssertEquals("Expected filled DepartureTransportMeans[0].TransportNationality", "ES", departureTransportMeansElement.TransportNationality);
			});
		}

		public void TestDepartureTransportMeans_MOT2()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Expected empty DepartureTransportMeans when no data declared", 0, wrapper.DepartureTransportMeans.Count);

				departureMovement.InlandTransportModeAtDeparture = ModeOfTransportList.Codes._2_RailTransport;
				departureMovement.TransportTypeAtDeparture = "20";
				departureMovement.TransportAtDeparture = "wagon";
				departureMovement.TransportCountryAtDeparture = "GB";
				wrapper = GetWrapper(nctsHeader);
				var departureTransportMeans = wrapper.DepartureTransportMeans;
				AssertEquals("Expected filled DepartureTransportMeans with 1 element", 1, departureTransportMeans.Count);
				AssertSame("Cached DepartureTransportMeans", wrapper.DepartureTransportMeans, departureTransportMeans);
				var departureTransportMeansElement = departureTransportMeans.FirstOrDefault();
				AssertEquals("Expected filled DepartureTransportMeans[0].SequenceNumber", "1", departureTransportMeansElement.SequenceNumber);
				AssertEquals("Expected filled DepartureTransportMeans[0].TransportMode", "20", departureTransportMeansElement.TransportMode);
				AssertEquals("Expected filled DepartureTransportMeans[0].TransportId", "wagon", departureTransportMeansElement.TransportId);
				AssertEquals("Expected filled DepartureTransportMeans[0].TransportNationality", "GB", departureTransportMeansElement.TransportNationality);
			});
		}

		public void TestDepartureTransportMeans_MOT3()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Expected empty DepartureTransportMeans when no data declared", 0, wrapper.DepartureTransportMeans.Count);

				departureMovement.InlandTransportModeAtDeparture = ModeOfTransportList.Codes._3_RoadTransport;
				departureMovement.Trailer2IDAtDeparture = "trailer2";
				departureMovement.Trailer2NationalityAtDeparture = "ES";
				wrapper = GetWrapper(nctsHeader);
				var departureTransportMeans = wrapper.DepartureTransportMeans;
				AssertEquals("Expected filled DepartureTransportMeans with 1 element", 1, departureTransportMeans.Count);
				AssertSame("Cached DepartureTransportMeans", wrapper.DepartureTransportMeans, departureTransportMeans);
				var departureTransportMeansElement = departureTransportMeans.FirstOrDefault();
				AssertEquals("When only trailer 2 ID declared expected filled DepartureTransportMeans[0].SequenceNumber", "1", departureTransportMeansElement.SequenceNumber);
				AssertEquals("When only trailer 2 ID declared expected filled DepartureTransportMeans[0].TransportMode", "31", departureTransportMeansElement.TransportMode);
				AssertEquals("When only trailer 2 ID declared expected filled DepartureTransportMeans[0].TransportId", "trailer2", departureTransportMeansElement.TransportId);
				AssertEquals("When only trailer 2 ID declared expected filled DepartureTransportMeans[0].TransportNationality", "ES", departureTransportMeansElement.TransportNationality);

				departureMovement.Trailer1IDAtDeparture = "trailer1";
				departureMovement.Trailer1NationalityAtDeparture = "FR";
				wrapper = GetWrapper(nctsHeader);
				departureTransportMeans = wrapper.DepartureTransportMeans;
				AssertEquals("Expected filled DepartureTransportMeans with 2 elements", 2, departureTransportMeans.Count);
				var departureTransportMeansArray = departureTransportMeans.ToArray();
				AssertEquals("When trailer 1 and trailer 2 ID declared expected filled DepartureTransportMeans[0].SequenceNumber", "1", departureTransportMeansArray[0].SequenceNumber);
				AssertEquals("When trailer 1 and trailer 2 ID declared expected filled DepartureTransportMeans[0].TransportMode", "31", departureTransportMeansArray[0].TransportMode);
				AssertEquals("When trailer 1 and trailer 2 ID declared expected filled with trailer 1 ID DepartureTransportMeans[0].TransportId", "trailer1", departureTransportMeansArray[0].TransportId);
				AssertEquals("When trailer 1 and trailer 2 ID declared expected filled DepartureTransportMeans[0].TransportNationality", "FR", departureTransportMeansArray[0].TransportNationality);

				AssertEquals("When trailer 1 and trailer 2 ID declared expected filled DepartureTransportMeans[1].SequenceNumber", "2", departureTransportMeansArray[1].SequenceNumber);
				AssertEquals("When trailer 1 and trailer 2 ID declared expected filled DepartureTransportMeans[1].TransportMode", "31", departureTransportMeansArray[1].TransportMode);
				AssertEquals("When trailer 1 and trailer 2 ID declared expected filled with trailer 2 ID DepartureTransportMeans[1].TransportId", "trailer2", departureTransportMeansArray[1].TransportId);
				AssertEquals("When trailer 1 and trailer 2 ID declared expected filled DepartureTransportMeans[1].TransportNationality", "ES", departureTransportMeansArray[1].TransportNationality);

				departureMovement.TransportAtDeparture = "transportID";
				departureMovement.TransportCountryAtDeparture = "GB";
				wrapper = GetWrapper(nctsHeader);
				departureTransportMeans = wrapper.DepartureTransportMeans;
				AssertEquals("Expected filled DepartureTransportMeans with 3 elements", 3, departureTransportMeans.Count);
				departureTransportMeansArray = departureTransportMeans.ToArray();
				AssertEquals("When transport ID, trailer 1 and trailer 2 ID declared expected filled DepartureTransportMeans[0].SequenceNumber", "1", departureTransportMeansArray[0].SequenceNumber);
				AssertEquals("When transport ID, trailer 1 and trailer 2 ID declared expected filled DepartureTransportMeans[0].TransportMode", "30", departureTransportMeansArray[0].TransportMode);
				AssertEquals("When transport ID, trailer 1 and trailer 2 ID declared expected filled with transport ID DepartureTransportMeans[0].TransportId", "transportID", departureTransportMeansArray[0].TransportId);
				AssertEquals("When transport ID, trailer 1 and trailer 2 ID declared expected filled DepartureTransportMeans[0].TransportNationality", "GB", departureTransportMeansArray[0].TransportNationality);

				AssertEquals("When transport ID, trailer 1 and trailer 2 ID declared expected filled DepartureTransportMeans[1].SequenceNumber", "2", departureTransportMeansArray[1].SequenceNumber);
				AssertEquals("When transport ID, trailer 1 and trailer 2 ID declared expected filled DepartureTransportMeans[1].TransportMode", "31", departureTransportMeansArray[1].TransportMode);
				AssertEquals("When transport ID, trailer 1 and trailer 2 ID declared expected filled with trailer 1 ID DepartureTransportMeans[1].TransportId", "trailer1", departureTransportMeansArray[1].TransportId);
				AssertEquals("When transport ID, trailer 1 and trailer 2 ID declared expected filled DepartureTransportMeans[1].TransportNationality", "FR", departureTransportMeansArray[1].TransportNationality);

				AssertEquals("When transport ID, trailer 1 and trailer 2 ID declared expected filled DepartureTransportMeans[2].SequenceNumber", "3", departureTransportMeansArray[2].SequenceNumber);
				AssertEquals("When transport ID, trailer 1 and trailer 2 ID declared expected filled DepartureTransportMeans[2].TransportMode", "31", departureTransportMeansArray[2].TransportMode);
				AssertEquals("When transport ID, trailer 1 and trailer 2 ID declared expected filled with trailer 2 ID DepartureTransportMeans[2].TransportId", "trailer2", departureTransportMeansArray[2].TransportId);
				AssertEquals("When transport ID, trailer 1 and trailer 2 ID declared expected filled DepartureTransportMeans[2].TransportNationality", "ES", departureTransportMeansArray[2].TransportNationality);
			});
		}

		public void TestDepartureTransportMeans_MOT4()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Expected empty DepartureTransportMeans when no data declared", 0, wrapper.DepartureTransportMeans.Count);

				departureMovement.InlandTransportModeAtDeparture = ModeOfTransportList.Codes._4_AirTransport;
				departureMovement.TransportTypeAtDeparture = "40";
				departureMovement.TransportAtDeparture = "aircraftID";
				departureMovement.TransportCountryAtDeparture = "ES";
				wrapper = GetWrapper(nctsHeader);
				var departureTransportMeans = wrapper.DepartureTransportMeans;
				AssertEquals("Expected filled DepartureTransportMeans with 1 element", 1, departureTransportMeans.Count);
				AssertSame("Cached DepartureTransportMeans", wrapper.DepartureTransportMeans, departureTransportMeans);
				var departureTransportMeansElement = departureTransportMeans.FirstOrDefault();
				AssertEquals("Expected filled DepartureTransportMeans[0].SequenceNumber", "1", departureTransportMeansElement.SequenceNumber);
				AssertEquals("Expected filled DepartureTransportMeans[0].TransportMode", "40", departureTransportMeansElement.TransportMode);
				AssertEquals("Expected filled DepartureTransportMeans[0].TransportId", "aircraftID", departureTransportMeansElement.TransportId);
				AssertEquals("Expected filled DepartureTransportMeans[0].TransportNationality", "ES", departureTransportMeansElement.TransportNationality);
			});
		}

		public void TestDepartureTransportMeans_MOT5()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Expected empty DepartureTransportMeans when no data declared", 0, wrapper.DepartureTransportMeans.Count);

				departureMovement.InlandTransportModeAtDeparture = ModeOfTransportList.Codes._5_PostalConsignment;
				wrapper = GetWrapper(nctsHeader);
				AssertEquals("Expected empty DepartureTransportMeans when MOT is 5", 0, wrapper.DepartureTransportMeans.Count);
			});
		}

		public void TestDepartureTransportMeans_MOT7()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Expected empty DepartureTransportMeans when no data declared", 0, wrapper.DepartureTransportMeans.Count);

				departureMovement.InlandTransportModeAtDeparture = ModeOfTransportList.Codes._7_FixedTransportInstallations;
				wrapper = GetWrapper(nctsHeader);
				AssertEquals("Expected empty DepartureTransportMeans when MOT is 7", 0, wrapper.DepartureTransportMeans.Count);
			});
		}

		public void TestDepartureTransportMeans_MOT8()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Expected empty DepartureTransportMeans when no data declared", 0, wrapper.DepartureTransportMeans.Count);

				departureMovement.InlandTransportModeAtDeparture = ModeOfTransportList.Codes._8_InlandWaterwayTransport;
				departureMovement.TransportTypeAtDeparture = "80";
				departureMovement.TransportAtDeparture = "Vessel";
				departureMovement.TransportCountryAtDeparture = "ES";
				wrapper = GetWrapper(nctsHeader);
				var departureTransportMeans = wrapper.DepartureTransportMeans;
				AssertEquals("Expected filled DepartureTransportMeans with 1 element", 1, departureTransportMeans.Count);
				AssertSame("Cached DepartureTransportMeans", wrapper.DepartureTransportMeans, departureTransportMeans);
				var departureTransportMeansElement = departureTransportMeans.FirstOrDefault();
				AssertEquals("When Vessel has no Lloyds/IMO number expected filled DepartureTransportMeans[0].SequenceNumber", "1", departureTransportMeansElement.SequenceNumber);
				AssertEquals("When Vessel has no Lloyds/IMO number expected filled DepartureTransportMeans[0].TransportMode", "80", departureTransportMeansElement.TransportMode);
				AssertEquals("When Vessel has no Lloyds/IMO number expected filled DepartureTransportMeans[0].TransportId", "Vessel", departureTransportMeansElement.TransportId);
				AssertEquals("When Vessel has no Lloyds/IMO number expected filled DepartureTransportMeans[0].TransportNationality", "ES", departureTransportMeansElement.TransportNationality);
			});
		}

		public void TestDepartureTransportMeans_MOT9()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Expected empty DepartureTransportMeans when no data declared", 0, wrapper.DepartureTransportMeans.Count);

				departureMovement.InlandTransportModeAtDeparture = ModeOfTransportList.Codes._9_OwnPropulsion;
				departureMovement.TransportTypeAtDeparture = "81";
				departureMovement.TransportAtDeparture = "transportID";
				departureMovement.TransportCountryAtDeparture = "ES";
				wrapper = GetWrapper(nctsHeader);
				var departureTransportMeans = wrapper.DepartureTransportMeans;
				AssertEquals("Expected filled DepartureTransportMeans with 1 element", 1, departureTransportMeans.Count);
				AssertSame("Cached DepartureTransportMeans", wrapper.DepartureTransportMeans, departureTransportMeans);
				var departureTransportMeansElement = departureTransportMeans.FirstOrDefault();
				AssertEquals("Expected filled DepartureTransportMeans[0].SequenceNumber", "1", departureTransportMeansElement.SequenceNumber);
				AssertEquals("Expected filled DepartureTransportMeans[0].TransportMode", "81", departureTransportMeansElement.TransportMode);
				AssertEquals("Expected filled DepartureTransportMeans[0].TransportId", "transportID", departureTransportMeansElement.TransportId);
				AssertEquals("Expected filled DepartureTransportMeans[0].TransportNationality", "ES", departureTransportMeansElement.TransportNationality);
			});
		}

		public void TestDepartureTransportMeans_Arrival()
		{
			var (wrapperArrival, arrivalMovementHeader) = SetUpForArrival();

			CombineAssertions(() =>
			{
				AssertEquals("Expected empty DepartureTransportMeans when no data declared", 0, wrapperArrival.DepartureTransportMeans.Count);

				var transportInfo1 = arrivalMovementHeader.ArrivalTransportInfos.AddNew();
				transportInfo1.TPM_SequenceNumber = 2;
				transportInfo1.TPM_TypeOfIdentification = "11";
				transportInfo1.TPM_IdentificationNumber = "Vessel";
				transportInfo1.TPM_RN_NKTransportNationality = "ES";
				transportInfo1.TPM_TransportState = "NEW";

				var transportInfo2 = arrivalMovementHeader.ArrivalTransportInfos.AddNew();
				transportInfo2.TPM_SequenceNumber = 3;
				transportInfo2.TPM_TypeOfIdentification = "20";
				transportInfo2.TPM_IdentificationNumber = "wagon";
				transportInfo2.TPM_RN_NKTransportNationality = "FR";
				transportInfo2.TPM_TransportState = "MIS";

				var transportInfo3 = arrivalMovementHeader.ArrivalTransportInfos.AddNew();
				transportInfo3.TPM_SequenceNumber = 4;
				transportInfo3.TPM_TypeOfIdentification = "30";
				transportInfo3.TPM_IdentificationNumber = "Vehicle";
				transportInfo3.TPM_RN_NKTransportNationality = "DE";
				transportInfo3.TPM_TransportState = "DIF";

				var transportInfo4 = arrivalMovementHeader.ArrivalTransportInfos.AddNew();
				transportInfo4.TPM_SequenceNumber = 5;
				transportInfo4.TPM_TypeOfIdentification = "40";
				transportInfo4.TPM_IdentificationNumber = "Flight";
				transportInfo4.TPM_RN_NKTransportNationality = "IT";
				transportInfo4.TPM_TransportState = "DEC";

				wrapperArrival = GetWrapperArrival(nctsHeader);
				var departureTransportMeans = wrapperArrival.DepartureTransportMeans;

				AssertEquals("Expected filled DepartureTransportMeans with 3 elements (only the ones with transport state MIS, DIF or NEW)", 3, departureTransportMeans.Count);
				AssertSame("Cached DepartureTransportMeans", wrapperArrival.DepartureTransportMeans, departureTransportMeans);

				var departureTransportMeansArray = departureTransportMeans.ToArray();
				AssertEquals("When Transport State is NEW, expected filled DepartureTransportMeans[0].SequenceNumber", "2", departureTransportMeansArray[0].SequenceNumber);
				AssertEquals("When Transport State is NEW, expected filled DepartureTransportMeans[0].TransportMode", "11", departureTransportMeansArray[0].TransportMode);
				AssertEquals("When Transport State is NEW, expected filled with transport ID DepartureTransportMeans[0].TransportId", "Vessel", departureTransportMeansArray[0].TransportId);
				AssertEquals("When Transport State is NEW, expected filled DepartureTransportMeans[0].TransportNationality", "ES", departureTransportMeansArray[0].TransportNationality);

				AssertEquals("When Transport State is MIS, expected filled DepartureTransportMeans[1].SequenceNumber", "3", departureTransportMeansArray[1].SequenceNumber);
				AssertEquals("When Transport State is MIS, expected empty DepartureTransportMeans[1].TransportMode", ZString.Empty, departureTransportMeansArray[1].TransportMode);
				AssertEquals("When Transport State is MIS, expected empty with trailer 1 ID DepartureTransportMeans[1].TransportId", ZString.Empty, departureTransportMeansArray[1].TransportId);
				AssertEquals("When Transport State is MIS, expected empty DepartureTransportMeans[1].TransportNationality", ZString.Empty, departureTransportMeansArray[1].TransportNationality);

				AssertEquals("When Transport State is DIF, expected filled DepartureTransportMeans[2].SequenceNumber", "4", departureTransportMeansArray[2].SequenceNumber);
				AssertEquals("When Transport State is DIF, expected filled DepartureTransportMeans[2].TransportMode", "30", departureTransportMeansArray[2].TransportMode);
				AssertEquals("When Transport State is DIF, expected filled with trailer 2 ID DepartureTransportMeans[2].TransportId", "Vehicle", departureTransportMeansArray[2].TransportId);
				AssertEquals("When Transport State is DIF, expected filled DepartureTransportMeans[2].TransportNationality", "DE", departureTransportMeansArray[2].TransportNationality);
			});
		}

		(NCTS5CommonConsignmentWrapper, NctsArrivalMovementHeader) SetUpForArrival()
		{
			nctsHeader.BH_HeaderType = NctsMovementType.Codes.Arrival;
			return (GetWrapperArrival(nctsHeader), nctsHeader.ArrivalMovementHeader);
		}

		protected override void SetUp()
		{
			base.SetUp();

			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			departureMovement = nctsHeader.MovementHeader;

			wrapper = GetWrapper(nctsHeader);
		}

		NctsHeader nctsHeader;
		NctsDepartureMovementHeader departureMovement;
		NCTS5CommonConsignmentWrapper wrapper;

		NCTS5CommonConsignmentWrapper GetWrapper(NctsHeader header) => new NCTS5CommonConsignmentWrapper(header);
		NCTS5CommonConsignmentWrapper GetWrapperArrival(NctsHeader header) => new NCTS5CommonConsignmentWrapper(header, isArrivalDeclaration: true);

		protected override NCTS5CommonConsignmentWrapper GetProvider() => wrapper;
	}
}
