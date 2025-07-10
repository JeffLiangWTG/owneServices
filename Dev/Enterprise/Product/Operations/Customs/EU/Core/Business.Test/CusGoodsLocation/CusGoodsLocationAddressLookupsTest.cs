using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.EU.Business.Testing
{
	class CusGoodsLocationAddressLookupsTest : BusinessObjectLookupsTestCase
	{
		[ExpectNoExceptions]
		public void TestAuthorisationNumberList_FilterBusinessObjectDefaults()
		{
			var orgHeaderPK = Factory.New<OrgHeader>().PK;
			locationAddress.IdentificationHolderPK = orgHeaderPK;
			locationAddress.E2_GovRegNumType = CusAuthorizationHeaderTypeList.Codes.AuthorizedConsigneeTir;
			locationAddress.E2_GovRegNum = "001";

			CombineAssertions(() =>
			{
				var numberList = (CusAuthorisationHeaderCollection)lookups.AuthorisationNumberList;
				var authorisationTypeFilterBO = numberList.FilterBusinessObjectDefaults[CusAuthorisationHeaderCollection.FilterConstants.AuthorisationType + ":Property"];
				NUnit.Framework.Assert.That(authorisationTypeFilterBO.Value, NUnit.Framework.Is.EqualTo(CusAuthorizationHeaderTypeList.Codes.AuthorizedConsigneeTir).Using(CustomComparers.TypeComparison), "Value for AuthorisationType");
				NUnit.Framework.Assert.That(authorisationTypeFilterBO.IsRemovable, NUnit.Framework.Is.EqualTo(true), "IsRemovable for AuthorisationType");

				var authorisationNumberFilterBO = numberList.FilterBusinessObjectDefaults[CusAuthorisationHeaderCollection.FilterConstants.AuthorisationNumber + ":Property"];
				NUnit.Framework.Assert.That(authorisationNumberFilterBO.Value, NUnit.Framework.Is.EqualTo("001").Using(CustomComparers.TypeComparison), "Value for AuthorisationNumber");
				NUnit.Framework.Assert.That(authorisationNumberFilterBO.IsRemovable, NUnit.Framework.Is.EqualTo(true), "IsRemovable for AuthorisationNumber");

				var authorisationHolderFilterBO = numberList.FilterBusinessObjectDefaults[CusAuthorisationHeaderCollection.FilterConstants.AuthorisationHolder + ":Property"];
				NUnit.Framework.Assert.That(authorisationHolderFilterBO.Value, NUnit.Framework.Is.EqualTo(orgHeaderPK).Using(CustomComparers.TypeComparison), "Value for AuthorisationHolder");
				NUnit.Framework.Assert.That(authorisationHolderFilterBO.IsRemovable, NUnit.Framework.Is.EqualTo(false), "IsRemovable for AuthorisationHolder");
			});
		}

		[ExpectNoExceptions]
		public void TestOrganisationList()
		{
			NUnit.Framework.Assert.That(lookups.OrganisationList, NUnit.Framework.Is.TypeOf<OrgHeaderCollection>());
		}

		protected override void SetUp()
		{
			base.SetUp();
			var location = Factory.New<CusGoodsLocation>();
			locationAddress = location.Address;
			lookups = locationAddress.Lookups;
		}
		CusGoodsLocationAddress locationAddress;
		CusGoodsLocationAddressLookups lookups;
	}
}
