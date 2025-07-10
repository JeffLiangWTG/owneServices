using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.Universal.Testing;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;
using static Enterprise.Core.Constants.Customs.Universal;

namespace Enterprise.Customs.ES.NCTS.Business.Testing
{
	[TestedType(typeof(NctsCusGoodsLocation))]
	public class NctsCusGoodsLocationTest : EnterpriseBusinessObjectTestCase
	{
		public void TestLookups()
		{
			AssertType<NctsCusGoodsLocationLookups>(cusGoodsLocation.Lookups);
		}

		public void TestValidation()
		{
			AssertType<NctsCusGoodsLocationValidation>(cusGoodsLocation.Validation);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => GetNewBusinessObject();

		protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewBusinessObject();

		protected override BusinessObject GetNewBusinessObject() => cusGoodsLocation;

		public void TestGoodsLocationDefaultsForNew()
		{
			CombineAssertions(() =>
			{
				AssertEquals("New GoodsLocation has CGL_Type='B'", CusGoodsLocationTypeList.Codes.AuthorizedPlace, cusGoodsLocation.CGL_Type);
				AssertEquals("New GoodsLocation has CGL_Qualifier='Y'", CusGoodsLocationQualifierList.Codes.AuthorizationNumber, cusGoodsLocation.CGL_Qualifier);

				var indicentCusGoodsLocation = nctsHeader.EnRouteIncidents.AddNew().GoodsLocation;
				AssertEquals("New GoodsLocation has CGL_Type = empty", ZString.Empty, indicentCusGoodsLocation.CGL_Type);
				AssertEquals("New GoodsLocation has CGL_Qualifier = empty", ZString.Empty, indicentCusGoodsLocation.CGL_Qualifier);
			});
		}

		public void TestGoodsLocationReadOnlyIfPhaseTNNForArrival()
		{
			CombineAssertions(() =>
			{
				nctsHeader.ArrivalMovementHeader.BM_Phase = ESNctsMovementHeaderTransactionStatusList.Codes.Annexes;
				AssertEquals("CGL_AdditionalIdentifier is not readOnly when internal environment and BM_Phase is not TNN (Arrival)", false, cusGoodsLocation.CGL_AdditionalIdentifierInfo.ReadOnly);
				nctsHeader.ArrivalMovementHeader.BM_Phase = ESNctsMovementHeaderTransactionStatusList.Codes.OriginalDepartureData;
				AssertEquals("CGL_AdditionalIdentifier is not readOnly when internal environment but BM_Phase is TNN (Arrival and NCTS5)", false, cusGoodsLocation.CGL_AdditionalIdentifierInfo.ReadOnly);
				nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
				AssertEquals("CGL_AdditionalIdentifier is not readOnly when internal environment but BM_Phase is TNN (Arrival and NCTS4)", false, cusGoodsLocation.CGL_AdditionalIdentifierInfo.ReadOnly);
			});
		}

		public void TestGoodsLocationReadOnlyIfPhaseTNNForDeparture()
		{
			nctsHeader.BH_HeaderType = EU.NCTS.Business.NctsMovementType.Codes.Departure;
			var cusGoodsLocationDeparture = nctsHeader.MovementHeader.GoodsLocation;
			CombineAssertions(() =>
			{
				nctsHeader.MovementHeader.BM_Phase = ESNctsMovementHeaderTransactionStatusList.Codes.Annexes;
				AssertEquals("CGL_AdditionalIdentifier is not readOnly when internal environment and BM_Phase is not TNN (Departure)", false, cusGoodsLocationDeparture.CGL_AdditionalIdentifierInfo.ReadOnly);
				nctsHeader.MovementHeader.BM_Phase = ESNctsMovementHeaderTransactionStatusList.Codes.OriginalDepartureData;
				AssertEquals("CGL_AdditionalIdentifier is readOnly when internal environment but BM_Phase is TNN (Departure and NCTS5)", true, cusGoodsLocationDeparture.CGL_AdditionalIdentifierInfo.ReadOnly);
				nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
				AssertEquals("CGL_AdditionalIdentifier is not readOnly when internal environment but BM_Phase is TNN (Departure and NCTS4)", false, cusGoodsLocationDeparture.CGL_AdditionalIdentifierInfo.ReadOnly);
			});
		}

		public void TestGoodsLocationChangedWithTNNCopyFieldToGoodsLocation()
		{
			var location1 = "aaaa";
			var location2 = "bbbb";
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Arrival);
			var arrivalMovement = header.ArrivalMovementHeader;
			var goodsLocation = arrivalMovement.GoodsLocation;
			goodsLocation.CGL_AdditionalIdentifier = location1;
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			header.ArrivalMrnFromUser = "ES123456";
			header.ESNctsHeader.CEN_TNNArrival = false;

			var tnnDataCodeInfo = TnnDataCodeInfo.LoadNew(header);
			tnnDataCodeInfo.AcceptanceDate = ZDateTime.Now;
			tnnDataCodeInfo.ClearanceDate = ZDateTime.Now.AddDays(-1);

			arrivalMovement.GenerateTNNDeparture(tnnDataCodeInfo);
			Factory.Save();

			CombineAssertions(() =>
			{
				AssertEquals("GoodsLocation in arrivalMovement is aaaa", location1, goodsLocation.CGL_AdditionalIdentifier);
				AssertEquals("GoodsLocation in arrivalMovement HeaderTNN is aaaa (GenerateTNNDeparture)", location1, arrivalMovement.HeaderTNN.MovementHeader.GoodsLocation.CGL_AdditionalIdentifier);

				goodsLocation.CGL_AdditionalIdentifier = location2;
				AssertEquals("GoodsLocation in arrivalMovement is bbbb", location2, goodsLocation.CGL_AdditionalIdentifier);
				AssertEquals("GoodsLocation in arrivalMovement HeaderTNN bbbb", location2, arrivalMovement.HeaderTNN.MovementHeader.GoodsLocation.CGL_AdditionalIdentifier);

				goodsLocation.CGL_AdditionalIdentifier = ZString.Empty;
				AssertEquals("GoodsLocation in arrivalMovement is empty (Change with value to empty)", ZString.Empty, goodsLocation.CGL_AdditionalIdentifier);
				AssertEquals("GoodsLocation in arrivalMovement HeaderTNN empty (Change with value to empty)", ZString.Empty, arrivalMovement.HeaderTNN.MovementHeader.GoodsLocation.CGL_AdditionalIdentifier);
			});
		}

		public void TestCGL_AdditionalIdentifier_MaxLegnth()
		{
			AssertEquals(17, cusGoodsLocation.CGL_AdditionalIdentifierInfo.MaxLength);
		}

		public void TestIsParentIncidentPhase5Arrival()
		{
			CombineAssertions(() =>
			{
				AssertEquals("NCTS phase 5, Departure, Child of MovementHeader", false, cusGoodsLocation.IsParentIncidentPhase5Arrival);
				cusGoodsLocation.Header.BH_HeaderType = EU.NCTS.Business.NctsMovementType.Codes.Arrival;
				AssertEquals("NCTS phase 5, Arrival, Child of MovementHeader", false, cusGoodsLocation.IsParentIncidentPhase5Arrival);
				cusGoodsLocation.Header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
				AssertEquals("NCTS phase 4, Arrival, Child of MovementHeader", false, cusGoodsLocation.IsParentIncidentPhase5Arrival);

				var enRouteIncident = cusGoodsLocation.Header.EnRouteIncidents.AddNew();
				enRouteIncident.BN_Type = CusInBondEventTypes.Codes.Transshipment;
				var incidentGoodLocation = (NctsCusGoodsLocation)enRouteIncident.GoodsLocation;
				cusGoodsLocation.Header.BH_HeaderType = EU.NCTS.Business.NctsMovementType.Codes.Departure;
				cusGoodsLocation.Header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;

				AssertEquals("NCTS phase 5, Departure, Child of Incident", false, incidentGoodLocation.IsParentIncidentPhase5Arrival);
				cusGoodsLocation.Header.BH_HeaderType = EU.NCTS.Business.NctsMovementType.Codes.Arrival;
				AssertEquals("NCTS phase 5, Arrival, Child of Icident", true, incidentGoodLocation.IsParentIncidentPhase5Arrival);
				cusGoodsLocation.Header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
				AssertEquals("NCTS phase 4, Arrival, Child of Incident", false, incidentGoodLocation.IsParentIncidentPhase5Arrival);
			});
		}

		public void TestAdditionalIdentifierDescription()
		{
			const string description = "Identifier Description";
			ZString code = "ID1234";
			var helper = new UniversalReferenceTestDataHelper(Factory);

			helper.CreateNewOrGetExistingCusCodeType(RefCusCodeListTypes.Codes.GoodsOfLocationType, "Location Codes");
			var grouping = helper.CreateNewOrGetExistingDataGrouping("EUN");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Spain, parent: grouping);
			helper.CreateCusCodeList("ES", RefCusCodeListTypes.Codes.GoodsOfLocationType, code, description, ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2));
			Factory.Save();
			cusGoodsLocation.CGL_AdditionalIdentifier = code;

			CombineAssertions(() =>
			{
				AssertEquals("Description matches on valid additional identifier", description, cusGoodsLocation.AdditionalIdentifierDescription.ToString());
				cusGoodsLocation.CGL_AdditionalIdentifier = "InvalidCode";
				AssertEquals("Description is empty on invalid additional identifier", ZString.Empty, cusGoodsLocation.AdditionalIdentifierDescription);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			nctsHeader = Factory.NewWithValidTestData<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Arrival);
			cusGoodsLocation = nctsHeader.ArrivalMovementHeader.GoodsLocation;
		}

		NctsCusGoodsLocation cusGoodsLocation;
		NctsHeader nctsHeader;
	}
}
