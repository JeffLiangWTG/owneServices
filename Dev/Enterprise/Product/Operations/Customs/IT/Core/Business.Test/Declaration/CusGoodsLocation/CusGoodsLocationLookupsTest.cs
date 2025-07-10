using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

sealed class CusGoodsLocationLookupsTest : TestCaseWithFactory
{
	public void TestQualifierList()
	{
		AssertEquals("QualifierList", "V, Y, Z", goodsLocationLookups.QualifierList.CodesAsString);
	}

	public void TestOrganisationList()
	{
		AssertType("Type", typeof(OrganisationsFindBoxCollection), goodsLocationLookups.OrganisationList);
	}

	public void TestAdditionalIdentifierList()
	{
		AssertType("Type", typeof(CodeDescriptionPairList), goodsLocationLookups.AdditionalIdentifierList);
		AssertNotNull(goodsLocationLookups.AdditionalIdentifierList);
	}

	protected override void SetUp()
	{
		base.SetUp();

		declaration = Factory.New<JobDeclaration>();
		goodsLocation = (CusGoodsLocation)declaration.GoodsLocation;
		goodsLocationLookups = goodsLocation.Lookups;
	}

	JobDeclaration declaration;
	CusGoodsLocation goodsLocation;
	CusGoodsLocationLookups goodsLocationLookups;
}
