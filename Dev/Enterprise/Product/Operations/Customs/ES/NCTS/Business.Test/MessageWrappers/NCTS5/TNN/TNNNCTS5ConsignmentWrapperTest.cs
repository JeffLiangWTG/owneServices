using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.ES.Business.Testing;
using Enterprise.Customs.ES.NCTS.Business.MessageWrappers;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.NCTS.Business.Testing
{
	public class TNNNCTS5ConsignmentWrapperTest : WrapperHelperTest<TNNNCTS5ConsignmentWrapper>
	{
		public void TestCommonConsignmentData()
		{
			CombineAssertions(() =>
			{
				var commonConsignmentData = wrapper.CommonConsignmentData;
				AssertNotNull("Expected filled CommonConsignmentData", commonConsignmentData);
				AssertSame("Cached CommonConsignmentData", wrapper.CommonConsignmentData, commonConsignmentData);
			});
		}

		public void TestCountryOfDestination()
		{
			CombineAssertions(() =>
			{
				departureMovement.BM_RL_NKDestinationPort = "ES";
				AssertEquals("Expected filled CountryOfDestination in Consignment when there are no goodsItems", "ES", wrapper.CommonConsignmentData.CountryOfDestination);

				var nctsBill = nctsHeader.Bills.AddNew();
				var goodsItem1 = nctsBill.GoodsItems.AddNew();
				goodsItem1.BY_RN_NKCountryOfDestination = "FR";
				wrapper = GetWrapper(nctsHeader);
				AssertEquals("Expected filled CountryOfDestination in Consignment when there is only one goodsItem", "ES", wrapper.CommonConsignmentData.CountryOfDestination);
				AssertEquals("Expected empty CountryOfDestination in ConsignmentItem when there is only one goodsItem", ZString.Empty, wrapper.HouseConsignment.First().ConsignmentItem.First().CountryOfDestination);

				var goodsItem2 = nctsBill.GoodsItems.AddNew();
				goodsItem2.BY_RN_NKCountryOfDestination = "DE";
				wrapper = GetWrapper(nctsHeader);
				AssertEquals("Expected empty CountryOfDestination in Consignment when there are multiple goodsItems with different codes", ZString.Empty, wrapper.CommonConsignmentData.CountryOfDestination);
				AssertContainsExactElementsInAnyOrder("Expected filled CountryOfDestination in ConsignmentItem when there are multiple goodsItems with different codes", new ZString[] { "FR", "DE" }, wrapper.HouseConsignment.First().ConsignmentItem.Select(x => x.CountryOfDestination).ToArray());

				goodsItem2.BY_RN_NKCountryOfDestination = "FR";
				wrapper = GetWrapper(nctsHeader);
				AssertEquals("Expected filled CountryOfDestination in Consignment when there are multiple goodsItems with same codes", "ES", wrapper.CommonConsignmentData.CountryOfDestination);
				AssertContainsExactElementsInAnyOrder("Expected filled CountryOfDestination in ConsignmentItem when there are multiple goodsItems with same codes", new ZString[] { ZString.Empty, ZString.Empty }, wrapper.HouseConsignment.First().ConsignmentItem.Select(x => x.CountryOfDestination).ToArray());
			});
		}

		public void TestNullConsignor()
		{
			AssertExceptionThrown<NullReferenceException>(() => wrapper.Consignor.ToString());
		}

		public void TestConsignor()
		{
			CombineAssertions(() =>
			{
				var orgAddress = Factory.New<OrgAddress>();
				var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
				orgAddress.OA_OH = orgHeader.PK;
				nctsHeader.Consignor.E2_OA_Address = orgAddress.PK;

				departureMovement.BM_TypeOfSecurity = "EXI";
				wrapper = GetWrapper(nctsHeader);
				AssertNull("Expected empty Consignor when securityType is EXI (not NON)", wrapper.Consignor);

				departureMovement.BM_TypeOfSecurity = "NON";
				wrapper = GetWrapper(nctsHeader);
				var consignor = wrapper.Consignor;
				AssertNotNull("Expected filled Consignor when securityType is NON", consignor);
				AssertSame("Cached Consignor", wrapper.Consignor, consignor);

				nctsHeader.Principal.E2_OA_Address = orgAddress.PK;
				wrapper = GetWrapper(nctsHeader);
				AssertNull("Expected empty Consignor when it is the same as the Principal", wrapper.Consignor);
			});
		}

		public void TestActiveBorderTransportMeans()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Expected empty ActiveBorderTransportMeans when no data declared", 0, wrapper.ActiveBorderTransportMeans.Count);

				departureMovement.BM_ExportTransportMode = ModeOfTransportList.Codes._1_SeaTransport;
				departureMovement.BM_ActiveBorderIdentificationType = "10";
				departureMovement.BM_TOLCarrierID = "Vessel";
				departureMovement.BM_RN_NKTOLCarrierNationality = "ES";
				departureMovement.BM_ConveyanceNumber = "Conveyance";

				wrapper = GetWrapper(nctsHeader);
				var activeBorderTransportMeans = wrapper.ActiveBorderTransportMeans;

				AssertEquals("Expected filled ActiveBorderTransportMeans with 1 element", 1, activeBorderTransportMeans.Count);
				AssertSame("Cached ActiveBorderTransportMeans", wrapper.ActiveBorderTransportMeans, activeBorderTransportMeans);

				var activeBorderTransportMeansElement = activeBorderTransportMeans.FirstOrDefault();
				AssertEquals("Expected filled ActiveBorderTransportMeans[0].SequenceNumber", "1", activeBorderTransportMeansElement.SequenceNumber);
				AssertEquals("Expected filled ActiveBorderTransportMeans[0].TransportMode", "10", activeBorderTransportMeansElement.TransportMode);
				AssertEquals("Expected filled ActiveBorderTransportMeans[0].TransportId", "VESSEL", activeBorderTransportMeansElement.TransportId);
				AssertEquals("Expected filled ActiveBorderTransportMeans[0].TransportNationality", "ES", activeBorderTransportMeansElement.TransportNationality);
				AssertEquals("Expected filled ActiveBorderTransportMeans[0].ConveyanceReferenceNumber", "Conveyance", activeBorderTransportMeansElement.ConveyanceReferenceNumber);
			});
		}

		public void TestHouseConsignment()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Expected empty HouseConsignment when no data declared", 0, wrapper.HouseConsignment.Count);

				nctsHeader.Bills.AddNew();
				nctsHeader.Bills.AddNew();
				wrapper = GetWrapper(nctsHeader);
				var houseConsignment = wrapper.HouseConsignment;
				AssertEquals("Expected filled HouseConsignment", 2, houseConsignment.Count);
				AssertSame("Cached HouseConsignment", wrapper.HouseConsignment, houseConsignment);
			});
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
		TNNNCTS5ConsignmentWrapper wrapper;

		TNNNCTS5ConsignmentWrapper GetWrapper(NctsHeader header) => new TNNNCTS5ConsignmentWrapper(header);

		protected override TNNNCTS5ConsignmentWrapper GetProvider() => wrapper;
	}
}
