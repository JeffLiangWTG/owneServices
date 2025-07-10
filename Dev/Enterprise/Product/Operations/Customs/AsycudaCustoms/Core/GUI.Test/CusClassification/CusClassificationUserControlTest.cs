using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AsycudaCustoms.GUI.Testing
{
	class CusClassificationUserControlTest : TestCaseWithFactory
	{
		public void TestTariffFindBox()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var parentDataGrouping = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.CommonDataGrouping, "ZZ");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Botswana, "Botswana", parentDataGrouping);
			Factory.Save();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Bangladesh))
			using (var control = new DummyCusClassificationUserControl())
			{
				AssertEquals("HSN", control.GetUniversalTariffType());
				AssertEquals(Core.Constants.CountryCodes.Bangladesh, control.GetCustomsCountryCodeForTesting());
			}

			using (var control2 = new DummyCusClassificationUserControl())
			{
				AssertEquals("1P1", control2.GetUniversalTariffType());
				AssertEquals(Core.Constants.CountryCodes.Botswana, control2.GetCustomsCountryCodeForTesting());
				AssertEquals(Core.Constants.Customs.Universal.RefDataGrouping.Codes.CommonDataGrouping, RefDataGrouping.GetParentDataGrouping(Factory, control2.GetCustomsCountryCodeForTesting()).ZZZ_DataGrouping);
			}
		}
	}

	class DummyCusClassificationUserControl : CusClassificationUserControl
	{
		public ZString GetUniversalTariffType() => UniversalTariffType;
		public string GetCustomsCountryCodeForTesting() => GetCustomsCountryCode();
	}
}
