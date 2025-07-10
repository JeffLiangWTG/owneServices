using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.ES.Business.Testing;
using Enterprise.Customs.ES.NCTS.Business.MessageWrappers;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.ES.NCTS.Business.Testing
{
	public class NCTS5CommonDepartureAndNotifConsignmentWrapperTest : WrapperHelperTest<NCTS5CommonDepartureAndNotifConsignmentWrapper>
	{
		public void TestLocationOfGoods()
		{
			CombineAssertions(() =>
			{
				AssertNull("Expected null LocationOfGoods when departureMovement.GoodsLocation.CGL_AdditionalIdentifier is empty", wrapper.LocationOfGoods);

				departureMovement.GoodsLocation.CGL_AdditionalIdentifier = "reference";
				wrapper = GetWrapper(nctsHeader);
				var locationOfGoods = wrapper.LocationOfGoods;
				AssertNotNull("Expected filled LocationOfGoods when departureMovement.GoodsLocation.CGL_AdditionalIdentifier is not empty", locationOfGoods);
				AssertSame("Cached LocationOfGoods", wrapper.LocationOfGoods, locationOfGoods);
			});
		}

		public void TestActiveBorderTransportMeans()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Expected empty ActiveBorderTransportMeans when no data declared", 0, wrapper.ActiveBorderTransportMeans.Count);

				departureMovement.BM_ExportTransportMode = ModeOfTransportList.Codes._1_SeaTransport;
				departureMovement.BM_CustomsOfficeAtBorder = "ES009999";
				departureMovement.BM_ActiveBorderIdentificationType = "10";
				departureMovement.BM_TOLCarrierID = "Vessel";
				departureMovement.BM_RN_NKTOLCarrierNationality = "ES";
				departureMovement.BM_ConveyanceNumber = "Conveyance";

				wrapper = GetWrapper(nctsHeader);
				var activeBorderTransportMeans = wrapper.ActiveBorderTransportMeans;

				AssertEquals("Expected filled ActiveBorderTransportMeans with 1 element when border mode is not 5", 1, activeBorderTransportMeans.Count);
				AssertSame("Cached ActiveBorderTransportMeans", wrapper.ActiveBorderTransportMeans, activeBorderTransportMeans);

				var activeBorderTransportMeansElement = activeBorderTransportMeans.FirstOrDefault();
				AssertEquals("Expected filled ActiveBorderTransportMeans[0].SequenceNumber", "1", activeBorderTransportMeansElement.SequenceNumber);
				AssertEquals("Expected filled ActiveBorderTransportMeans[0].CustomsOfficeAtBorderReferenceNumber", "ES009999", activeBorderTransportMeansElement.CustomsOfficeAtBorderReferenceNumber);
				AssertEquals("Expected filled ActiveBorderTransportMeans[0].TransportMode", "10", activeBorderTransportMeansElement.TransportMode);
				AssertEquals("Expected filled ActiveBorderTransportMeans[0].TransportId", "VESSEL", activeBorderTransportMeansElement.TransportId);
				AssertEquals("Expected filled ActiveBorderTransportMeans[0].TransportNationality", "ES", activeBorderTransportMeansElement.TransportNationality);
				AssertEquals("Expected filled ActiveBorderTransportMeans[0].ConveyanceReferenceNumber", "Conveyance", activeBorderTransportMeansElement.ConveyanceReferenceNumber);

				departureMovement.BM_ExportTransportMode = ModeOfTransportList.Codes._5_PostalConsignment;
				departureMovement.BM_CustomsOfficeAtBorder = "ES009999";
				departureMovement.BM_ActiveBorderIdentificationType = "10";
				wrapper = GetWrapper(nctsHeader);
				AssertEquals("Expected empty ActiveBorderTransportMeans border mode is 5", 0, wrapper.ActiveBorderTransportMeans.Count);
			});
		}

		public void TestPlaceOfLoading()
		{
			CombineAssertions(() =>
			{
				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality("NC5TP", "EUN", ZDate.Today, value: true))
				{
					AssertNull("TransitionalPeriod: Expected empty PlaceOfLoading when not declared", wrapper.PlaceOfLoading);

					departureMovement.BM_PlaceOfLoading = "ESMAD";
					departureMovement.BM_TypeOfSecurity = "EXI";
					wrapper = GetWrapper(nctsHeader);
					AssertNotNull("TransitionalPeriod: Expected filled PlaceOfLoading when securityType is not NON (is EXI)", wrapper.PlaceOfLoading);

					departureMovement.BM_TypeOfSecurity = "NON";
					wrapper = GetWrapper(nctsHeader);
					AssertNull("TransitionalPeriod: Expected empty PlaceOfLoading when securityType is NON", wrapper.PlaceOfLoading);
				}

				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality("NC5TP", "EUN", ZDate.Today, value: false))
				{
					departureMovement.BM_PlaceOfLoading = ZString.Empty;
					AssertNull("FinalPeriod: Expected empty PlaceOfLoading when not declared", wrapper.PlaceOfLoading);

					departureMovement.BM_PlaceOfLoading = "ESMAD";
					wrapper = GetWrapper(nctsHeader);
					AssertNotNull("FinalPeriod: Expected PlaceOfLoading if declared", wrapper.PlaceOfLoading);
				}
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
		NCTS5CommonDepartureAndNotifConsignmentWrapper wrapper;

		NCTS5CommonDepartureAndNotifConsignmentWrapper GetWrapper(NctsHeader header) => new NCTS5CommonDepartureAndNotifConsignmentWrapper(header);

		protected override NCTS5CommonDepartureAndNotifConsignmentWrapper GetProvider() => wrapper;
	}
}
