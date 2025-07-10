using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.Testing
{
	class CusExitDetailLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestCustomsOffices()
		{
			var yesterday = ZDateTime.Today.AddDays(-1);
			var tomorrow = ZDateTime.Today.AddDays(1);
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var eun = helper.CreateNewOrGetExistingDataGrouping(EconomicGroupList.Codes.EuropeanUnion, "European Union");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Germany, "Germany", eun);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Italy, "Italy", eun);
			helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "Customs Office");
			helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PackageTypes, "Package Types");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Italy, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PackageTypes, "BOX", yesterday, tomorrow);
			helper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.Italy, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "IT", "Italian Office", yesterday, tomorrow, RefCusCodeListAttributeTypes.Codes.ROLE, "EXT");
			helper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.Italy, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "DE", "German Office", yesterday, tomorrow, RefCusCodeListAttributeTypes.Codes.ROLE, "EIN");
			Factory.Save();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Spain))
			{
				var cusExitHeader = Factory.NewWithValidTestData<CusExitControlHeader>();
				var cusExitDetail = cusExitHeader.CusExitDetails.AddNew();

				var customsOffice = cusExitDetail.Lookups.CustomsOffices;
				customsOffice.Load();
				AssertContainsExactElementsInAnyOrder(new[] { "IT", "DE" }, customsOffice.Select(x => x.ZZD_Code));
			}
		}

		[ExpectNoExceptions]
		public void TestCustomsOffices_HeaderNull()
		{
			var cusExitDetail = Factory.New<CusExitDetail>();
			NUnit.Framework.Assert.That(cusExitDetail.Lookups.CustomsOffices, NUnit.Framework.Is.Not.EqualTo(default(Customs.Business.CustomsOfficeCodeCollection)), "CustomsOffices when Header is null - should not be [null]");
		}

		[ExpectNoExceptions]
		public void TestOrganizationsFindBoxList()
		{
			var orgHeader = Factory.New<OrgHeader>();
			var cusExitHeader = Factory.NewWithValidTestData<CusExitControlHeader>();
			var cusExitDetail = cusExitHeader.CusExitDetails.AddNew();
			var list = cusExitDetail.Lookups.OrganizationsFindBoxList;
			NUnit.Framework.Assert.That(orgHeader.MatchesFilter(list.CompleteFilter), NUnit.Framework.Is.EqualTo(true), "orgHeader is in the list");
		}

		[ExpectNoExceptions]
		public void TestOrganizationsFindBoxList_HeaderNull()
		{
			var cusExitDetail = Factory.New<CusExitDetail>();
			NUnit.Framework.Assert.That(cusExitDetail.Lookups.OrganizationsFindBoxList, NUnit.Framework.Is.Not.EqualTo(default(OrganisationsFindBoxCollection)), "OrganizationsFindBoxList when Header is null - should not be [null]");
		}

		[ExpectNoExceptions]
		public void TestStatusList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var eun = helper.CreateNewOrGetExistingDataGrouping(EconomicGroupList.Codes.EuropeanUnion, "European Union");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Germany, "Germany", eun);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Spain, "Spain", eun);
			helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ExitCustomsStatus, "Exit Customs Status");
			helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PackageTypes, "Package Types");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ExitCustomsStatus, "Invalid Country", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Spain, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ExitCustomsStatus, "ES001", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Spain, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ExitCustomsStatus, "ES002", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Spain, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PackageTypes, "Invalid Type", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			Factory.Save();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Spain))
			{
				var cusExitDetail = Factory.New<CusExitDetail>();
				var statusCodes = cusExitDetail.Lookups.StatusList;
				CombineAssertions(() =>
				{
					NUnit.Framework.Assert.That(statusCodes.GetAllCodes(), NUnit.Framework.Is.EquivalentTo(new[] { "ES001", "ES002" }), "List");
					NUnit.Framework.Assert.That(cusExitDetail.Lookups.StatusList, NUnit.Framework.Is.SameAs(statusCodes), "Cached");
				});
			}
		}
	}
}
