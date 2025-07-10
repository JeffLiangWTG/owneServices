using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.IT.Business;
using Enterprise.Customs.IT.Business.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IT.TemporaryStorage.Business.Testing;

sealed class CusGoodsLocationLookupsTest : TestCaseWithFactory
{
	public void TestAdditionalIdentifierList()
	{
		var holderPk = SetuUpAuthorisationHeadersAndRules();
		var location = Factory.New<CusGoodsLocation>();
		location.Address.IdentificationHolderPK = holderPk;
		AssertEquals("Authorisation Type TST", "90808F, 90815M", location.Lookups.AdditionalIdentifierList.CodesAsString);
	}

	public void TestQualifierList()
	{
		AssertEquals("Y", Lookups.QualifierList.CodesAsString);
	}

	public void TestTypeList()
	{
		AssertEquals("C", Lookups.TypeList.CodesAsString);
	}

	ZGuid SetuUpAuthorisationHeadersAndRules()
	{
		var holder = Factory.New<OrgHeader>();
		var auth1 = CusAuthorisationHeaderTestHelper.CreateAuthorisationHeader(Factory, CusAuthorizationHeaderTypeList.Codes.TemporaryStorage, holder.PK, "TST001")
			.AddLocRule("90808F", "IT014199")
			.AddLocRule("90815M", "IT017199");

		var auth2 = CusAuthorisationHeaderTestHelper.CreateAuthorisationHeader(Factory, CusAuthorizationHeaderTypeList.Codes.ApprovedLocationForImport, holder.PK, "ALI001")
			.AddLocRule("100970K", "IT025000");

		return holder.PK;
	}

	CusGoodsLocationLookups Lookups
	{
		get
		{
			if (fLookups == null)
			{
				var location = Factory.New<CusGoodsLocation>();
				fLookups = location.Lookups;
			}

			return fLookups;
		}
	}
	CusGoodsLocationLookups fLookups;
}
