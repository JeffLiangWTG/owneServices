using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CH.NCTS.Business.Testing;

[TestedType(typeof(CusGoodsLocationAddress))]
class CusGoodsLocationAddressTest : EnterpriseBusinessObjectTestCase
{
	public void TestLookups()
	{
		AssertType<CusGoodsLocationAddressLookups>(NctsHeader.MovementHeader.GoodsLocation.Address.Lookups);
	}

	public void TestValidation()
	{
		AssertType<CusGoodsLocationAddressValidation>(NctsHeader.MovementHeader.GoodsLocation.Address.Validation);
	}

	public void TestGoodsLocationDefaultAuthorizationNumberSingleALPAuth()
	{
		var representative = Factory.NewWithValidTestData<OrgHeader>();
		NctsHeader.MovementHeader.Representative.OrganisationPK = representative.PK;
		var cusAuthorizationHeader = CusAuthorisationHeaderTestHelper.CreateAuthorisationHeader(Factory, CH.Business.CusAuthorizationHeaderTypeList.Codes.AuthorizedLocationCodeForPassar, representative.PK, "001");
		CusAuthorisationHeaderTestHelper.CreateAuthorisationHeader(Factory, CH.Business.CusAuthorizationHeaderTypeList.Codes.AuthorizedConsigneeLocationsForEDec, representative.PK, "002");
		var cusGoodsLocation = NctsHeader.MovementHeader.GoodsLocation;
		CombineAssertions(() =>
		{
			AssertEquals("When selected Organization has only one Auth. of type 'ALP', Auth Number should be set as default", cusAuthorizationHeader.CPH_Number, cusGoodsLocation.Address.AuthorisationNumber);
		});
	}

	public void TestGoodsLocationDefaultAuthorizationNumberMultiALPAuth()
	{
		var representative = Factory.NewWithValidTestData<OrgHeader>();
		NctsHeader.MovementHeader.Representative.OrganisationPK = representative.PK;
		CusAuthorisationHeaderTestHelper.CreateAuthorisationHeader(Factory, CH.Business.CusAuthorizationHeaderTypeList.Codes.AuthorizedLocationCodeForPassar, representative.PK, "001");
		CusAuthorisationHeaderTestHelper.CreateAuthorisationHeader(Factory, CH.Business.CusAuthorizationHeaderTypeList.Codes.AuthorizedLocationCodeForPassar, representative.PK, "002");
		var cusGoodsLocation = NctsHeader.MovementHeader.GoodsLocation;
		CombineAssertions(() =>
		{
			AssertEquals("When selected Organization has more than one Auth. of type 'ALP', Auth Number should be empty", ZString.Empty, cusGoodsLocation.Address.AuthorisationNumber);
		});
	}

	public void TestGoodsLocationDefaultAuthorizationNumberInactive()
	{
		var representative = Factory.NewWithValidTestData<OrgHeader>();
		NctsHeader.MovementHeader.Representative.OrganisationPK = representative.PK;
		var cusAuthorizationHeader = CusAuthorisationHeaderTestHelper.CreateAuthorisationHeader(Factory, CH.Business.CusAuthorizationHeaderTypeList.Codes.AuthorizedLocationCodeForPassar, representative.PK, "001");
		cusAuthorizationHeader.CPH_IsActive = false;
		CusAuthorisationHeaderTestHelper.CreateAuthorisationHeader(Factory, CH.Business.CusAuthorizationHeaderTypeList.Codes.AuthorizedConsigneeLocationsForEDec, representative.PK, "002");
		Factory.Save();

		var cusGoodsLocation = NctsHeader.MovementHeader.GoodsLocation;
		CombineAssertions(() =>
		{
			AssertEquals("When selected Auth. of type 'ALP' is not active, Auth Number should not be set as default", ZString.Empty, cusGoodsLocation.Address.AuthorisationNumber);
		});
	}

	protected override BusinessObject GetNewBusinessObject() => NctsHeader.MovementHeader.GoodsLocation.Address;

	NctsHeader NctsHeader => nctsHeader ?? (nctsHeader = CreateNctsHeader());
	NctsHeader nctsHeader;

	NctsHeader CreateNctsHeader()
	{
		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
		return nctsHeader;
	}
}
