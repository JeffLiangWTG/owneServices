using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	[TestedType(typeof(CusGoodsLocation))]
	sealed class CusGoodsLocationTest : EnterpriseBusinessObjectTestCase
	{
		public void TestLookups()
		{
			AssertType<CusGoodsLocationLookups>(cusGoodsLocation.Lookups);
		}

		public void TestValidation()
		{
			AssertType<CusGoodsLocationValidation>(cusGoodsLocation.Validation);
		}

		public void TestAddress()
		{
			AssertType<CusGoodsLocationAddress>(cusGoodsLocation.Address);
		}

		public void TestTypeDecider()
		{
			AssertType<CusGoodsLocationTypeDecider>(CusGoodsLocation.TypeDecider);
		}

		public void TestHeader()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Arrival);

			CombineAssertions(() =>
			{
				var incident = header.EnRouteIncidents.AddNew();
				var goodslocation = (CusGoodsLocation)incident.GoodsLocation;

				AssertSame("When CGL_ParentTableCode = 'BN'", header, goodslocation.Header);
				AssertEquals("When CGL_ParentTableCode = 'BN'", goodslocation.GetType().GetProperty("IsParentTableCodeMoveHeader", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(goodslocation), false);

				var movementHeader = header.ArrivalMovementHeader;
				goodslocation = movementHeader.GoodsLocation;
				AssertSame("When CGL_ParentTableCode = 'BM'", header, goodslocation.Header);
				AssertEquals("When CGL_ParentTableCode = 'BM'", goodslocation.GetType().GetProperty("IsParentTableCodeMoveHeader", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(goodslocation), true);

				goodslocation = Factory.New<CusGoodsLocation>();
				AssertNull("Other cases", goodslocation.Header);
				AssertEquals("Other cases", goodslocation.GetType().GetProperty("IsParentTableCodeMoveHeader", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(goodslocation), false);
			});
		}

		public void TestMovementHeaderDeparture()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Arrival);
			var goodslocation = header.ArrivalMovementHeader.GoodsLocation;

			CombineAssertions(() =>
			{
				var incident = header.EnRouteIncidents.AddNew();
				AssertNull("When CGL_ParentTableCode = 'BN'", ((CusGoodsLocation)incident.GoodsLocation).DepartureMovementHeader);

				AssertNull("When CGL_ParentTableCode = 'BM' and Arrival", goodslocation.DepartureMovementHeader);

				header = Factory.New<NctsHeader>();
				header.SetMovementType(NctsMovementType.Codes.Departure);
				var movementHeader = header.MovementHeader;
				goodslocation = movementHeader.GoodsLocation;
				AssertSame("When CGL_ParentTableCode = 'BM' and Departure", movementHeader, goodslocation.DepartureMovementHeader);

				goodslocation = Factory.New<CusGoodsLocation>();
				AssertNull("Other cases", goodslocation.DepartureMovementHeader);
			});
		}

		public void TestMovementHeaderArrival()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
			var goodslocation = header.MovementHeader.GoodsLocation;

			CombineAssertions(() =>
			{
				var incident = header.EnRouteIncidents.AddNew();
				AssertNull("When CGL_ParentTableCode = 'BN'", ((CusGoodsLocation)incident.GoodsLocation).ArrivalMovementHeader);

				AssertNull("When CGL_ParentTableCode = 'BM' and Departure", goodslocation.ArrivalMovementHeader);

				header = Factory.New<NctsHeader>();
				header.SetMovementType(NctsMovementType.Codes.Arrival);
				var arrivalMovementHeader = header.ArrivalMovementHeader;
				goodslocation = arrivalMovementHeader.GoodsLocation;
				AssertSame("When CGL_ParentTableCode = 'BM' and Arrival", arrivalMovementHeader, goodslocation.ArrivalMovementHeader);

				goodslocation = Factory.New<CusGoodsLocation>();
				AssertNull("Other cases", goodslocation.ArrivalMovementHeader);
			});
		}

		public void TestIsParentIncidentPhase5Arrival()
		{
			CombineAssertions(() =>
			{
				AssertEquals("NCTS phase 5, Departure, Child of MovementHeader", false, cusGoodsLocation.IsParentIncidentPhase5Arrival);
				cusGoodsLocation.Header.BH_HeaderType = NctsMovementType.Codes.Arrival;
				AssertEquals("NCTS phase 5, Arrival, Child of MovementHeader", false, cusGoodsLocation.IsParentIncidentPhase5Arrival);
				cusGoodsLocation.Header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
				AssertEquals("NCTS phase 4, Arrival, Child of MovementHeader", false, cusGoodsLocation.IsParentIncidentPhase5Arrival);

				var enRouteIncident = cusGoodsLocation.Header.EnRouteIncidents.AddNew();
				enRouteIncident.BN_Type = CusInBondEventTypes.Codes.Transshipment;
				var incidentGoodLocation = (CusGoodsLocation)enRouteIncident.GoodsLocation;
				cusGoodsLocation.Header.BH_HeaderType = NctsMovementType.Codes.Departure;
				cusGoodsLocation.Header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;

				AssertEquals("NCTS phase 5, Departure, Child of Incident", false, incidentGoodLocation.IsParentIncidentPhase5Arrival);
				cusGoodsLocation.Header.BH_HeaderType = NctsMovementType.Codes.Arrival;
				AssertEquals("NCTS phase 5, Arrival, Child of Icident", true, incidentGoodLocation.IsParentIncidentPhase5Arrival);
				cusGoodsLocation.Header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
				AssertEquals("NCTS phase 4, Arrival, Child of Incident", false, incidentGoodLocation.IsParentIncidentPhase5Arrival);
			});
		}

		public void TestCGL_AdditionalIdentifier_Caption()
		{
			var captionData = DataBoundResourceStrings.GetDataForProperty(cusGoodsLocation.CGL_AdditionalIdentifierInfo);
			AssertEquals("Additional Identifier", captionData.Caption);
			AssertEquals("Additional Ident.", captionData.MediumCaption);
			AssertEquals("Add. Ident.", captionData.ShortCaption);
		}

		public void TestAdditionalIdentifierDescription()
		{
			AssertNullOrEmpty(cusGoodsLocation.AdditionalIdentifierDescription);
		}

		public void TestAdditionalIdentifier_MaxLength()
		{
			AssertEquals(17, cusGoodsLocation.AdditionalIdentifierInfo.MaxLength);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => GetNewBusinessObject();

		protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewBusinessObject();

		protected override BusinessObject GetNewBusinessObject()
		{
			return cusGoodsLocation;
		}

		protected override void SetUp()
		{
			base.SetUp();

			cusGoodsLocation = Factory.New<CusGoodsLocation>();
			cusGoodsLocation.CGL_LocationUse = "ARR";
			var nctsHeader = Factory.NewWithValidTestData<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
			var movementHeader = nctsHeader.ArrivalMovementHeader;
			cusGoodsLocation.Parent = movementHeader;
		}

		CusGoodsLocation cusGoodsLocation;
	}
}
