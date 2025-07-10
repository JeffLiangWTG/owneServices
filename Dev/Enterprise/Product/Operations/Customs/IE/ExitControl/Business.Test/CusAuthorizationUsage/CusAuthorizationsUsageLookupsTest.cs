using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IE.ExitControl.Business.Testing
{
	sealed class CusAuthorizationsUsageLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestCodeList()
		{
			var provider = Customs.Business.CusAuthorisationHeaderProvider.GetByCountryCode(Core.Constants.CountryCodes.EuropeanUnion);
			AssertEquals(provider.GetAuthorisationTypeList(Factory).CodesAsString, ((CodeDescriptionPairList)lookups.CodeList).CodesAsString);
		}

		public void TestCodeList_Provider()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var euGrouping = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "European Union");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Ireland, "Ireland", euGrouping);
			helper.CreateNewOrGetExistingCusCodeType(EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_AUTH, "AUTH DESC");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_AUTH, "XXX", "XXX DESC", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_AUTH, "YYY", "YYY DESC", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();
			var lookups = CusAuthorizationUsageTest.GetNewBusinessObject(NewFactory()).authorizationUsage.Lookups;
			AssertEquals("XXX, YYY", ((CodeDescriptionPairList)lookups.CodeList).CodesAsString);
		}

		public void TestNumberList()
		{
			CombineAssertions(() =>
			{
				var numberList = lookups.NumberList;
				AssertType<EU.Business.CusAuthorisationHeaderCollectionFiltered>("Type", numberList);
				AssertEquals("Count is 0", 0, numberList.Count);

				var orgHeader = Factory.New<OrgHeader>();
				orgHeader.OH_Code = "ABC";

				var authorizationHeader = Factory.New<Customs.Business.CusAuthorisationHeader>();
				authorizationHeader.CPH_Number = "001";
				authorizationHeader.CPH_Type = "AAA";
				authorizationHeader.CPH_OH_PermitHolder = orgHeader.PK;

				authorizationUsage.AGC_Code = Customs.Business.CusAuthorizationHeaderTypeList.Codes.AuthorizedConsigneeTir;
				authorizationUsage.AGC_Number = "001";
				authorizationUsage.AGC_OH_Owner = orgHeader.PK;

				numberList = lookups.NumberList;
				AssertEquals("Count is 0", 0, numberList.Count);

				var authorizationHeader2 = Factory.New<Customs.Business.CusAuthorisationHeader>();
				authorizationHeader2.CPH_Number = "001";
				authorizationHeader2.CPH_Type = "ACT";
				authorizationHeader2.CPH_OH_PermitHolder = orgHeader.PK;

				numberList = lookups.NumberList;
				AssertContainsExactElementsInAnyOrder("Count is 1", new[] { authorizationHeader2.PK }, numberList.Select(x => x.PK));
			});
		}

		public void TestNumberList_FilterBusinessObjectDefaults()
		{
			var orgHeader = Factory.New<OrgHeader>();
			authorizationUsage.AGC_Code = Customs.Business.CusAuthorizationHeaderTypeList.Codes.AuthorizedConsigneeTir;
			authorizationUsage.AGC_Number = "001";
			authorizationUsage.AGC_OH_Owner = orgHeader.PK;
			var numberList = authorizationUsage.Lookups.NumberList;

			CombineAssertions(() =>
			{
				var authorisationTypeFilterBO = numberList.FilterBusinessObjectDefaults[Customs.Business.CusAuthorisationHeaderCollection.FilterConstants.AuthorisationType + ":Property"];
				AssertEquals("Value for AuthorisationType", Customs.Business.CusAuthorizationHeaderTypeList.Codes.AuthorizedConsigneeTir, authorisationTypeFilterBO.Value);
				AssertEquals("IsRemovablefor AuthorisationType", false, authorisationTypeFilterBO.IsRemovable);

				var authorisationNumberFilterBO = numberList.FilterBusinessObjectDefaults[Customs.Business.CusAuthorisationHeaderCollection.FilterConstants.AuthorisationNumber + ":Property"];
				AssertEquals("Value for AuthorisationNumber", "001", authorisationNumberFilterBO.Value);
				AssertEquals("IsRemovablefor AuthorisationNumber", true, authorisationNumberFilterBO.IsRemovable);

				var authorisationHolderFilterBO = numberList.FilterBusinessObjectDefaults[Customs.Business.CusAuthorisationHeaderCollection.FilterConstants.AuthorisationHolder + ":Property"];
				AssertEquals("Value for AuthorisationHolder", orgHeader.PK, authorisationHolderFilterBO.Value);
				AssertEquals("IsRemovablefor AuthorisationHolder", false, authorisationHolderFilterBO.IsRemovable);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			(_, _, authorizationUsage) = CusAuthorizationUsageTest.GetNewBusinessObject(Factory);
			lookups = authorizationUsage.Lookups;
		}
		CusAuthorizationUsage authorizationUsage;
		CusAuthorizationUsageLookups lookups;
	}
}
