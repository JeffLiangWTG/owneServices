using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.BE.NCTS.Business.Testing;

[TestedType(typeof(CusGoodsLocation))]
sealed class CusGoodsLocationTest : EnterpriseBusinessObjectTestCase
{
	public void TestAddress()
	{
		AssertType<CusGoodsLocationAddress>(cusGoodsLocation.Address);
	}

	public void TestValidation()
	{
		AssertType<CusGoodsLocationValidation>(cusGoodsLocation.Validation);
	}

	public void TestLookups()
	{
		AssertType<CusGoodsLocationLookups>(cusGoodsLocation.Lookups);
	}

	public void TestIsParentIncidentPhase5Arrival()
	{
		CombineAssertions(() =>
		{
			AssertEquals("NCTS phase 5, Departure, Child of MovementHeader", false, cusGoodsLocation.IsParentIncidentPhase5Arrival);
			cusGoodsLocation.Header.BH_HeaderType = NctsMovementType.Codes.Arrival;
			AssertEquals("NCTS phase 5, Arrival, Child of MovementHeader", false, cusGoodsLocation.IsParentIncidentPhase5Arrival);

			var enRouteIncident = cusGoodsLocation.Header.EnRouteIncidents.AddNew();
			enRouteIncident.BN_Type = CusInBondEventTypes.Codes.Transshipment;
			var incidentGoodLocation = (CusGoodsLocation)enRouteIncident.GoodsLocation;
			cusGoodsLocation.Header.BH_HeaderType = NctsMovementType.Codes.Departure;

			AssertEquals("NCTS phase 5, Departure, Child of Incident", false, incidentGoodLocation.IsParentIncidentPhase5Arrival);
			cusGoodsLocation.Header.BH_HeaderType = NctsMovementType.Codes.Arrival;
			AssertEquals("NCTS phase 5, Arrival, Child of Icident", true, incidentGoodLocation.IsParentIncidentPhase5Arrival);
		});
	}

	public void TestSetDefaultValues()
	{
		var nctsHeader = Factory.NewWithValidTestData<NctsHeader>();
		nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Arrival);
		cusGoodsLocation = (CusGoodsLocation)nctsHeader.EnRouteIncidents.AddNew().GoodsLocation;

		AssertEquals("Type should be an empty string by default", ZString.Empty, cusGoodsLocation.CGL_Type);
	}

	public void TestUnlocode()
	{
		AssertEquals("Max length", 17, cusGoodsLocation.UnlocodeInfo.MaxLength);
	}

	protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => GetGoodsLocation(factory);

	protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewBusinessObject();

	protected override BusinessObject GetNewBusinessObject() => GetGoodsLocation(Factory);

	static CusGoodsLocation GetGoodsLocation(BusinessObjectFactory factory)
	{
		var nctsHeader = factory.NewWithValidTestData<NctsHeader>(TestBusinessObjectKind.MinimumRequiredToSave);
		nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
		return nctsHeader.ArrivalMovementHeader.GoodsLocation;
	}

	protected override void SetUp()
	{
		base.SetUp();
		var nctsHeader = Factory.NewWithValidTestData<NctsHeader>();
		nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
		cusGoodsLocation = nctsHeader.MovementHeader.GoodsLocation;
	}

	CusGoodsLocation cusGoodsLocation;
}
