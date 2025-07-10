using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.Testing
{
	class ReimportCountryCodeLookupsTest : BusinessObjectLookupsTestCase
	{
		[ExpectNoExceptions]
		public void TestCY_CodeList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var eUGroup = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Germany, parent: eUGroup);

			helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.DEReimportCountry, "I0809 CodeList  - Country list (re-import)");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.DEReimportCountry, "BE", "Belgien", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.DEReimportCountry, "BG", "Bulgarien", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			Factory.Save();

			var cusCode = Factory.CreateReimportCountryCode();
			var codeList = cusCode.Lookups.CY_CodeList;
			NUnit.Framework.Assert.Multiple(() =>
			{
				NUnit.Framework.Assert.That(codeList.CodesAsString, NUnit.Framework.Is.EqualTo("BE, BG"), "Codes");
				NUnit.Framework.Assert.That(Factory.GetCachedValue("ReimportCountryCodeLookups.CY_CodeList", () => new CodeDescriptionPairList()), NUnit.Framework.Is.SameAs(codeList), "Cached CY_CodeList");
			});
		}
	}
}
