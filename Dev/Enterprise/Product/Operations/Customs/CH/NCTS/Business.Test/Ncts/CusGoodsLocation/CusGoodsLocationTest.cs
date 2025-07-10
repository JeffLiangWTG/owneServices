using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CH.NCTS.Business.Testing;

[TestedType(typeof(CusGoodsLocation))]
sealed class CusGoodsLocationTest : EnterpriseBusinessObjectTestCase
{
	protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => GetGoodsLocation(factory);

	protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewBusinessObject();

	protected override BusinessObject GetNewBusinessObject() => GetGoodsLocation(Factory);

	static CusGoodsLocation GetGoodsLocation(BusinessObjectFactory factory)
	{
		var nctsHeader = factory.NewWithValidTestData<NctsHeader>(TestBusinessObjectKind.MinimumRequiredToSave);
		nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
		return nctsHeader.ArrivalMovementHeader.GoodsLocation;
	}

	public void TestAddressType()
	{
		AssertType<CusGoodsLocationAddress>(Factory.New<CusGoodsLocation>().Address);
	}

	public void TestArrivalQualifier_ReadOnly()
	{
		NctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
		AssertQualifier_ReadOnly(NctsHeader.ArrivalMovementHeader.GoodsLocation);
	}

	public void TestDepartureQualifier_ReadOnly()
	{
		NctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
		AssertQualifier_ReadOnly(NctsHeader.MovementHeader.GoodsLocation);
	}

	void AssertQualifier_ReadOnly(CusGoodsLocation cusGoodsLocation)
	{
		AssertEquals("CGL_Qualifier.ReadOnly", true, cusGoodsLocation.CGL_QualifierInfo.ReadOnly);
	}

	public void TestArrivalType_ReadOnly()
	{
		NctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
		AssertType_ReadOnly(NctsHeader.ArrivalMovementHeader.GoodsLocation);
	}

	public void TestDepartureType_ReadOnly()
	{
		NctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
		AssertType_ReadOnly(NctsHeader.MovementHeader.GoodsLocation);
	}

	void AssertType_ReadOnly(CusGoodsLocation cusGoodsLocation)
	{
		AssertEquals("CGL_Type.ReadOnly", true, cusGoodsLocation.CGL_TypeInfo.ReadOnly);
	}

	public void TestGoodsLocationDefaultValues_Departure()
	{
		NctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
		AssertGoodsLocationDefaultValues(NctsHeader.MovementHeader.GoodsLocation);
	}

	public void TestGoodsLocationDefaultValues_Arrival()
	{
		NctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
		AssertGoodsLocationDefaultValues(NctsHeader.ArrivalMovementHeader.GoodsLocation);
	}

	public void TestParentIsIncident()
	{
		NctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
		Assert(!NctsHeader.ArrivalMovementHeader.GoodsLocation.ParentIsIncident);
		Assert((NctsHeader.EnRouteIncidents.AddNew().GoodsLocation as CusGoodsLocation).ParentIsIncident);
	}

	void AssertGoodsLocationDefaultValues(CusGoodsLocation cusGoodsLocation)
	{
		CombineAssertions(() =>
		{
			AssertEquals("New GoodsLocation has CGL_Type='C'", CusGoodsLocationTypeList.Codes.ApprovedPlace, cusGoodsLocation.CGL_Type);
			AssertEquals("New GoodsLocation has CGL_Qualifier='Y'", CusGoodsLocationQualifierList.Codes.AuthorizationNumber, cusGoodsLocation.CGL_Qualifier);
			AssertEquals("HasChanges should not be set", false, cusGoodsLocation.HasChanges);
		});
	}

	public void TestGoodsLocationDefaultPrincipalOrganization()
	{
		NctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
		var principal = Factory.NewWithValidTestData<OrgHeader>();
		NctsHeader.MovementHeader.Representative.OrganisationPK = ZGuid.Empty;
		NctsHeader.Principal.OrganisationPK = principal.PK;
		var cusGoodsLocation = NctsHeader.MovementHeader.GoodsLocation;
		AssertEquals("When Representative is empty, Default Org. should be Principal address", principal.PK, cusGoodsLocation.Address.IdentificationHolderPK);
		AssertEquals("HasChanges should not be set", false, cusGoodsLocation.HasChanges);
	}

	public void TestGoodsLocationDefaultRepresentativeOrganization()
	{
		NctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
		var principal = Factory.NewWithValidTestData<OrgHeader>();
		var representative = Factory.NewWithValidTestData<OrgHeader>();
		NctsHeader.Principal.OrganisationPK = principal.PK;
		NctsHeader.MovementHeader.Representative.OrganisationPK = representative.PK;
		var cusGoodsLocation = NctsHeader.MovementHeader.GoodsLocation;
		AssertEquals("When Representative is defined, Default Org. should be Representative address", representative.PK, cusGoodsLocation.Address.IdentificationHolderPK);
		AssertEquals("HasChanges should not be set", false, cusGoodsLocation.HasChanges);
	}

	public void TestGoodsLocationDefaultDestinationTrader()
	{
		NctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
		var destinationTrader = Factory.NewWithValidTestData<OrgHeader>();
		NctsHeader.DestinationTrader.OrganisationPK = destinationTrader.PK;
		var cusGoodsLocation = NctsHeader.ArrivalMovementHeader.GoodsLocation;
		AssertEquals("When DestinationTrader is defined, Default Org. should be Destination Trader address", destinationTrader.PK, cusGoodsLocation.Address.IdentificationHolderPK);
		AssertEquals("HasChanges should not be set", false, cusGoodsLocation.HasChanges);
	}

	NctsHeader NctsHeader => nctsHeader ?? (nctsHeader = Factory.New<NctsHeader>());
	NctsHeader nctsHeader;
}
