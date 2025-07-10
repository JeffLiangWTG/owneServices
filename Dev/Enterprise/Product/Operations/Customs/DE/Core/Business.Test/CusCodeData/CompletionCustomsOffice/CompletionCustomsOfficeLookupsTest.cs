using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.DE.Business.Testing
{
	public class CompletionCustomsOfficeLookupsTest : BusinessObjectLookupsTestCase
	{
		[ExpectNoExceptions]
		public void TestCY_CodeList()
		{
			NUnit.Framework.Assert.Multiple(() =>
			{
				var cy_CodeList = lookups.CY_CodeList;
				NUnit.Framework.Assert.That(cy_CodeList.CodesAsString, Is.EqualTo("CUSOF"), "Valid Codes");
				NUnit.Framework.Assert.That(lookups.CY_CodeList, Is.SameAs(cy_CodeList), "Cached");
			});
		}

		public void TestOfficeCodeList()
		{
			var officeCodeList = lookups.OfficeCodeList;
			officeCodeList.Load();
			NUnit.Framework.Assert.Multiple(() =>
			{
				AssertContainsExactElementsInAnyOrder(new[] { "DE000001" }, officeCodeList.Cast<ZZRefCusCodeListCombined>().Select(x => x.ZZD_Code));
				NUnit.Framework.Assert.That(officeCodeList.FilterBusinessObjectDefaults["Country/Region or Grouping:Property"].Value, Is.EqualTo(Core.Constants.CountryCodes.Germany).Using(CustomComparers.TypeComparison), "German is the default for Country/Region or Grouping");
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var eunZZZ = helper.CreateNewOrGetExistingDataGrouping("EUN");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Germany, parent: eunZZZ);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "Customs Office");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "DE000001", "DE000001", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			lookups = new CompletionCustomsOfficeLookups(Factory.New<CompletionCustomsOffice>());
		}
		CompletionCustomsOfficeLookups lookups;
	}
}
