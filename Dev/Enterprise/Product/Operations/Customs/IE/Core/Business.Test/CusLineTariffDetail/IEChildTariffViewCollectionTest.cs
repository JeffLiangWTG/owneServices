using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.Testing
{
	[TestedType(typeof(IEChildTariffViewCollection))]
	sealed class IEChildTariffViewCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestIEChildTariffViewCollection_LoadsCorrectly()
		{
			var tariffTypes = new ZString[] { "X2" };
			var ieChildTariffViewCollection = new IEChildTariffViewCollection(Factory, Core.Constants.CountryCodes.Ireland, tariffTypes, ZDateTime.Today);
			ieChildTariffViewCollection.Load();
			AssertContainsExactElementsInAnyOrder(new ZString[] { "X203" }, ieChildTariffViewCollection.Cast<TariffView>().Select(x => x.ZZ1_TariffCode));
		}

		public void TestIEChildTariffViewCollection_Empty()
		{
			var ieChildTariffViewCollection = new IEChildTariffViewCollection(Factory, Core.Constants.CountryCodes.Ireland, null, ZDateTime.Today);
			ieChildTariffViewCollection.Load();
			AssertContainsExactElementsInAnyOrder(new ZString[] { "X203", "X110" }, ieChildTariffViewCollection.Cast<TariffView>().Select(x => x.ZZ1_TariffCode));
		}

		protected override void SetUp()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var startDate = ZDateTime.MinSmallDateTimeValue;
			var endDate = ZDateTime.MaxSmallDateTimeValue;

			CreateTariffData(helper, Core.Constants.CountryCodes.Ireland, "X2", "X203", "Cigarettes containing tobaco (a)", "402.32 * [MIL] + 0.0873 + [RSP]", "MIL", "RSP", "2402209000");
			CreateTariffData(helper, Core.Constants.CountryCodes.Ireland, "X1", "X110", "Liquefied Petroleum Gas - used as propellant", "63.95 * [KLT]", "VCT", null, "1234567890");
		}

		void CreateTariffData(UniversalReferenceTestDataHelper helper, string countryCode, string tariffTypeCode, string tariffCode, string description, string rateFormula, string rateUOM1, string rateUOM2, string tariffRelationshipCode)
		{
			var euGrouping = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.EuropeanUnion, "European Union");
			helper.CreateNewOrGetExistingDataGrouping(countryCode, "Ireland", euGrouping);
			var tariffType = helper.CreateNewOrGetExistingTariffType(countryCode, tariffTypeCode);
			var tariff = helper.LoadOrCreateNewTariff(countryCode, tariffType.PK, tariffCode, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, description);
			var rateType = helper.CreateCusRateType(countryCode, tariffTypeCode, description: $"{description} Tax");
			var rateCode = helper.CreateCusRateCode(Factory, "EXC", rateType.PK, description: "Excise", countryCode: countryCode);
			var rate = helper.CreateRate(tariff, rateCode.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, rateFormula, dataGrouping: countryCode);
			helper.CreateRateUOM(rate.PK, rateUOM1);
			if (rateUOM2 != null)
			{
				helper.CreateRateUOM(rate.PK, rateUOM2);
			}
			helper.CreateTariffRelationship(tariff.PK, tariffType.PK, tariffRelationshipCode);
		}

		protected override BusinessObjectCollection GetCollectionToTest() => new IEChildTariffViewCollection(Factory, Core.Constants.CountryCodes.Ireland, null, ZDateTime.Today);
	}
}
