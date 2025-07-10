using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.Business.Declaration.Testing
{
	class ESGuaranteeLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestHolderIdentificationList()
		{
			AssertEquals(ZString.Empty, guarantee.Lookups.HolderIdentificationList.CodesAsString);

			var declaration = Factory.New<JobDeclaration>();
			guarantee = declaration.Guarantees.AddNew();

			AssertEquals(ZString.Empty, guarantee.Lookups.HolderIdentificationList.CodesAsString);

			var importer = CreateOrgHeader("IMP11111111", OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "FR");
			var declarant = CreateOrgHeader("DEC22222222", OrgCusCode.SpainCodeTypes.NIF, "ES");
			var representative = CreateOrgHeader("REP22222222", OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "ES");
			var buyer = CreateOrgHeader("BUY22222222", OrgCusCode.SpainCodeTypes.NIF, "ES");

			declaration.JE_OH_Importer = importer.PK;
			declaration.Declarant.OA_OH = declarant.PK;
			declaration.RepresentativeDocAddress.OrganisationPK = representative.PK;
			declaration.JE_OH_Buyer = buyer.PK;

			AssertEquals("ESDEC22222222, FRIMP11111111", guarantee.Lookups.HolderIdentificationList.CodesAsString);
		}

		OrgHeader CreateOrgHeader(string code, string type, string country)
		{
			var org = Factory.New<OrgHeader>();
			var cusCode = org.CustomsCodes.AddNew(type, code, country);
			cusCode.OK_SystemCreateTimeUtc = new ZDateTime(2022, 11, 11);
			if (type == OrgCusCode.EuropeanUnionSharedCodeTypes.Eori)
			{
				var cusCode2 = org.CustomsCodes.AddNew(type, "XI" + code, "GB");
				cusCode2.OK_SystemCreateTimeUtc = new ZDateTime(2022, 11, 09);
			}
			return org;
		}

		protected override void SetUp()
		{
			base.SetUp();
			guarantee = Factory.New<ESGuarantee>();
		}
		ESGuarantee guarantee;
	}
}
