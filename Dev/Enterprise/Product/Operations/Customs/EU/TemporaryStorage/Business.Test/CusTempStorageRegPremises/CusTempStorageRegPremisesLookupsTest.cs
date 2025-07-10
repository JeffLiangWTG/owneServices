using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using NUnit.Framework;

namespace Enterprise.Customs.EU.TemporaryStorage.Business.Testing
{
	[TestedType(typeof(CusTempStorageRegPremisesLookups))]
	sealed class CusTempStorageRegPremisesLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestAuthorizationNumberList()
		{
			premises.SRP_Code = "X";
			premises.SRP_Description = "DESC";
			var orgHeader = Factory.New<MasterFiles.Business.OrgHeader>();
			orgHeader.OH_Code = "AAA";
			var orgAddress = Factory.New<MasterFiles.Business.OrgAddress>();
			orgAddress.OA_OH = orgHeader.PK;
			orgAddress.OA_Address1 = "Address";
			premises.SRP_OA_PremisesAddress = orgAddress.PK;

			var orgHeaderPK = Factory.NewWithValidTestData<MasterFiles.Business.OrgHeader>().PK;
			premises.AuthorizationOwner = orgHeaderPK;
			premises.AuthorizationNumber = "OH";

			Factory.Save();

			CombineAssertions("Filtered authorisation number list", () =>
			{
				var numberList = lookups.AuthorizationNumberList;
				AssertType<CusAuthorisationHeaderCollectionFiltered>(numberList);
				var authorisationTypeFilterBO = numberList.FilterBusinessObjectDefaults[CusAuthorisationHeaderCollection.FilterConstants.AuthorisationType + ":Property"];
				AssertEquals("Value for AuthorisationType", AuthorizationTypeList.Codes.TST, authorisationTypeFilterBO.Value);
				AssertEquals("IsRemovable for AuthorisationType", false, authorisationTypeFilterBO.IsRemovable);

				var authorisationHolderFilterBO = numberList.FilterBusinessObjectDefaults[CusAuthorisationHeaderCollection.FilterConstants.AuthorisationHolder + ":Property"];
				AssertEquals("Value for AuthorisationHolder", orgHeaderPK, authorisationHolderFilterBO.Value);
				AssertEquals("IsRemovable for AuthorisationHolder", false, authorisationHolderFilterBO.IsRemovable);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			premises = Factory.New<CusTempStorageRegPremises>();
			lookups = premises.Lookups;
		}
		CusTempStorageRegPremisesLookups lookups;
		CusTempStorageRegPremises premises;
	}
}
