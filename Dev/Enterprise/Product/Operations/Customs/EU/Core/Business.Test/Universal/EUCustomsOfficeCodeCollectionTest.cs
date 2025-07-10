using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.Testing
{
	[TestedType(typeof(EUCustomsOfficeCodeCollection))]
	class EUCustomsOfficeCodeCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestAllEuropeanUnionCustomsOfficesWithRequiredRoles()
		{
			Factory.Save();
			var collection = EUCustomsOfficeCodeCollection.AllEuropeanUnionCustomsOfficesWithRequiredRoles(Factory);
			collection.Load();
			var expectedOfficeCodes = new ZString[] { "DE004323", "DE003478", "IT008734", "IT009278", "XI005342" };
			AssertContainsExactElementsInAnyOrder(expectedOfficeCodes, collection.Cast<ZZRefCusCodeListCombined>().Select(x => x.ZZD_Code));
		}

		public void TestAllEuropeanUnionAndOtherCountriesCustomsOfficesWithRequiredRoles()
		{
			AddEuropeanAdditionalRequiredCustomsOffices();
			var collection = EUCustomsOfficeCodeCollection.AllEuropeanUnionAndOtherCountriesCustomsOfficesWithRequiredRoles(Factory, [Core.Constants.CountryCodes.Norway, Core.Constants.CountryCodes.Switzerland]);
			collection.Load();
			var expectedOfficeCodes = new ZString[] { "DE004323", "DE003478", "IT008734", "IT009278", "XI005342", "NO11014A", "CH004633" };
			AssertContainsExactElementsInAnyOrder(expectedOfficeCodes, collection.Cast<ZZRefCusCodeListCombined>().Select(x => x.ZZD_Code));
		}

		public void TestAllEuropeanUnionCustomsOfficesWithRequiredRolesMatchingAttributes()
		{
			Factory.Save();
			var collection = EUCustomsOfficeCodeCollection.AllEuropeanUnionCustomsOfficesWithRequiredRoles(Factory, CustomsOfficeCodeTestHelper.CompetentAuthorityOfEnquiryCodeType, CustomsOfficeCodeTestHelper.EoriRegistrationAuthoritiesCodeType);
			collection.Load();
			var expectedOfficeCodes = new ZString[] { "DE004323", "IT009278" };
			AssertContainsExactElementsInAnyOrder(expectedOfficeCodes, collection.Cast<ZZRefCusCodeListCombined>().Select(x => x.ZZD_Code));
		}

		public void TestAllEuropeanUnionCustomsOfficesWithRequiredRolesExceptLocal()
		{
			Factory.Save();
			var collection = EUCustomsOfficeCodeCollection.AllEuropeanUnionCustomsOfficesWithRequiredRolesExceptLocal(Factory, Core.Constants.CountryCodes.Italy);
			collection.Load();
			var expectedOfficeCodes = new ZString[] { "DE004323", "DE003478", "XI005342" };
			AssertContainsExactElementsInAnyOrder(expectedOfficeCodes, collection.Cast<ZZRefCusCodeListCombined>().Select(x => x.ZZD_Code));
		}

		public void TestAllEuropeanUnionAndCtCountriesCustomsOfficesWithRequiredRoles()
		{
			var collection = EUCustomsOfficeCodeCollection.AllEuropeanUnionAndCtCountriesCustomsOfficesWithRequiredRoles(Factory);
			collection.Load();
			var expectedOfficeCodes = new ZString[] { "DE004323", "DE003478", "IT008734", "IT009278", "GB003478", "XI005342" };
			AssertContainsExactElementsInAnyOrder(expectedOfficeCodes, collection.Cast<ZZRefCusCodeListCombined>().Select(x => x.ZZD_Code));
		}

		public void TestCustomsOfficesWithRequiredRoles()
		{
			var collection = EUCustomsOfficeCodeCollection.CustomsOfficesWithRequiredRoles(Factory, new ZString[] { Core.Constants.CountryCodes.Italy, Core.Constants.CountryCodes.NorthernIreland_ForUseOnlyByEuInCertainScopes });
			collection.Load();
			var expectedOfficeCodes = new ZString[] { "IT008734", "IT009278", "XI005342" };
			AssertContainsExactElementsInAnyOrder(expectedOfficeCodes, collection.Cast<ZZRefCusCodeListCombined>().Select(x => x.ZZD_Code));
		}

		public void TestAllEuropeanUnionAndCtCountriesCustomsOfficesWithRequiredRolesExceptLocal()
		{
			var collection = EUCustomsOfficeCodeCollection.AllEuropeanUnionAndCtCountriesCustomsOfficesWithRequiredRolesExceptLocal(Factory, Core.Constants.CountryCodes.Italy, System.Array.Empty<ZString>());
			collection.Load();
			var expectedOfficeCodes = new ZString[] { "DE004323", "DE003478", "GB003478", "XI005342" };
			AssertContainsExactElementsInAnyOrder(expectedOfficeCodes, collection.Cast<ZZRefCusCodeListCombined>().Select(x => x.ZZD_Code));
		}

		protected override BusinessObjectCollection GetCollectionToTest() => EUCustomsOfficeCodeCollection.AllEuropeanUnionAndCtCountriesCustomsOfficesWithRequiredRoles(Factory);

		protected override void SetUp()
		{
			base.SetUp();
			var helper = new UniversalReferenceTestDataHelper(Factory);
			new CustomsOfficeCodeTestHelper(helper);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.UnitedKingdom);
			helper.CreateCusCodeList(Core.Constants.CountryCodes.UnitedKingdom, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "GB003478", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();
		}

		void AddEuropeanAdditionalRequiredCustomsOffices()
		{
			var dateMin = ZDateTime.MinSmallDateTimeValue;
			var dateMax = ZDateTime.MaxSmallDateTimeValue;
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var wco = helper.CreateNewOrGetExistingDataGrouping("WCO", "World Trade Organization");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Norway, "Norway", wco);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Switzerland, "Switzerland");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Norway, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "NO11014A", "NO11014A DESC", dateMin, dateMax);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Switzerland, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CH004633", "CH004633 DESC", dateMin, dateMax);
			Factory.Save();
		}
	}
}
