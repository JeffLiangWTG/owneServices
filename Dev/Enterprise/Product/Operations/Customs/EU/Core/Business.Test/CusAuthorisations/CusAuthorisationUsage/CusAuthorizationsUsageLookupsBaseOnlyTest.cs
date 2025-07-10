using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.EU.Business.Testing
{
	sealed class CusAuthorizationsUsageLookupsBaseOnlyTest : BusinessObjectLookupsTestCase
	{
		[ExpectNoExceptions]
		public void TestNumberList()
		{
			CombineAssertions(() =>
			{
				var numberList = lookups.NumberList;
				NUnit.Framework.Assert.That(numberList, NUnit.Framework.Is.TypeOf<CusAuthorisationHeaderCollectionFiltered>(), "Type");
				NUnit.Framework.Assert.That(numberList.Count, NUnit.Framework.Is.EqualTo(0), "Count is 0");

				var orgHeader = Factory.New<OrgHeader>();
				orgHeader.OH_Code = "ABC";

				var authorizationHeader = Factory.New<CusAuthorisationHeader>();
				authorizationHeader.CPH_Number = "001";
				authorizationHeader.CPH_Type = "AAA";
				authorizationHeader.CPH_OH_PermitHolder = orgHeader.PK;

				cusAuthorizationUsage.AGC_Code = CusAuthorizationHeaderTypeList.Codes.AuthorizedConsigneeTir;
				cusAuthorizationUsage.AGC_Number = "001";
				cusAuthorizationUsage.AGC_OH_Owner = orgHeader.PK;

				numberList = cusAuthorizationUsage.Lookups.NumberList;
				NUnit.Framework.Assert.That(numberList.Count, NUnit.Framework.Is.EqualTo(0), "Count is 0");

				var authorizationHeader2 = Factory.New<CusAuthorisationHeader>();
				authorizationHeader2.CPH_Number = "001";
				authorizationHeader2.CPH_Type = "ACT";
				authorizationHeader2.CPH_OH_PermitHolder = orgHeader.PK;

				numberList = cusAuthorizationUsage.Lookups.NumberList;
				NUnit.Framework.Assert.That(numberList.Select(x => x.PK), NUnit.Framework.Is.EquivalentTo(new[] { authorizationHeader2.PK }), "Count is 1");
			});
		}

		[ExpectNoExceptions]
		public void TestNumberList_FilterBusinessObjectDefaults()
		{
			var orgHeader = Factory.New<OrgHeader>();
			cusAuthorizationUsage.AGC_Code = CusAuthorizationHeaderTypeList.Codes.AuthorizedConsigneeTir;
			cusAuthorizationUsage.AGC_Number = "001";
			cusAuthorizationUsage.AGC_OH_Owner = orgHeader.PK;
			var numberList = cusAuthorizationUsage.Lookups.NumberList;

			CombineAssertions(() =>
			{
				var authorisationTypeFilterBO = numberList.FilterBusinessObjectDefaults[CusAuthorisationHeaderCollection.FilterConstants.AuthorisationType + ":Property"];
				NUnit.Framework.Assert.That(authorisationTypeFilterBO.Value, NUnit.Framework.Is.EqualTo(CusAuthorizationHeaderTypeList.Codes.AuthorizedConsigneeTir).Using(CustomComparers.TypeComparison), "Value for AuthorisationType");
				NUnit.Framework.Assert.That(authorisationTypeFilterBO.IsRemovable, NUnit.Framework.Is.EqualTo(false), "IsRemovablefor AuthorisationType");

				var authorisationNumberFilterBO = numberList.FilterBusinessObjectDefaults[CusAuthorisationHeaderCollection.FilterConstants.AuthorisationNumber + ":Property"];
				NUnit.Framework.Assert.That(authorisationNumberFilterBO.Value, NUnit.Framework.Is.EqualTo("001").Using(CustomComparers.TypeComparison), "Value for AuthorisationNumber");
				NUnit.Framework.Assert.That(authorisationNumberFilterBO.IsRemovable, NUnit.Framework.Is.EqualTo(true), "IsRemovablefor AuthorisationNumber");

				var authorisationHolderFilterBO = numberList.FilterBusinessObjectDefaults[CusAuthorisationHeaderCollection.FilterConstants.AuthorisationHolder + ":Property"];
				NUnit.Framework.Assert.That(authorisationHolderFilterBO.Value, NUnit.Framework.Is.EqualTo(orgHeader.PK).Using(CustomComparers.TypeComparison), "Value for AuthorisationHolder");
				NUnit.Framework.Assert.That(authorisationHolderFilterBO.IsRemovable, NUnit.Framework.Is.EqualTo(false), "IsRemovablefor AuthorisationHolder");
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			cusAuthorizationUsage = Factory.New<CusAuthorizationUsage>();
			lookups = cusAuthorizationUsage.Lookups;
		}

		CusAuthorizationUsage cusAuthorizationUsage;
		CusAuthorizationUsageLookups lookups;
	}
}
