using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.CusTempStorage;
using Enterprise.Customs.Universal.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.Testing
{
	[TestedType(typeof(UOMDefaulter))]
	sealed class UOMDefaulterTest : TestCaseWithFactory
	{
		public void TestNoExceptionThrown()
		{
			AssertNoExceptionThrown(() => UOMDefaulter.DefaultUOMIfApplicable(null, null));
		}

		public void TestDefaultUOMIfApplicable()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var countryCode = Core.Constants.CountryCodes.Latvia;

			var expTariffType = helper.CreateTariffType(countryCode, "EXP");
			var dutyRateType = helper.CreateCusRateType(countryCode, "DTY");

			var tariff = helper.LoadOrCreateNewTariff(countryCode, expTariffType.PK, "12345678", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var rateCode = helper.LoadOrCreateNewCusRateCode(Factory, "DTY", dutyRateType.PK);
			var rateWithoutUnit = helper.CreateRate(tariff, rateCode.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, rateFormula: "0.5*VFD");
			var rateVolumeUnit = helper.CreateRate(tariff, rateCode.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, rateFormula: "0.5*[LTR]");
			var rateWeightUnit = helper.CreateRate(tariff, rateCode.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, rateFormula: "0.5*[KGM]");

			Factory.Save();

			var header = Factory.New<TemporaryStorageHeader>();
			var bill = header.Bills.AddNew();
			var packedItem = bill.PackedItems.AddNew();
			packedItem.API_CustomsUQ = "LTR";

			UOMDefaulter.DefaultUOMIfApplicable(null, packedItem);
			AssertEquals("API_CustomsUQ2: When no RateView, nothing change", ZString.Empty, packedItem.API_CustomsUQ2);
			AssertEquals("API_CustomsUQ3: When no RateView, nothing change", ZString.Empty, packedItem.API_CustomsUQ3);

			UOMDefaulter.DefaultUOMIfApplicable(rateWithoutUnit, packedItem);
			AssertEquals("API_CustomsUQ2: When no unit in Rate Formula, no unit set", ZString.Empty, packedItem.API_CustomsUQ2);
			AssertEquals("API_CustomsUQ3: When no unit in Rate Formula, no unit set", ZString.Empty, packedItem.API_CustomsUQ3);

			UOMDefaulter.DefaultUOMIfApplicable(rateVolumeUnit, packedItem);
			AssertEquals("API_CustomsUQ2: When unit is set in First, no unit set", ZString.Empty, packedItem.API_CustomsUQ2);
			AssertEquals("API_CustomsUQ3: When unit is set in First, no unit set", ZString.Empty, packedItem.API_CustomsUQ3);

			packedItem.API_CustomsUQ = ZString.Empty;
			UOMDefaulter.DefaultUOMIfApplicable(rateVolumeUnit, packedItem);
			AssertEquals("API_CustomsUQ2: When Second is empty, no unit set in Third", "LTR", packedItem.API_CustomsUQ2);
			AssertEquals("API_CustomsUQ3: When Second is empty, no unit set in Third", ZString.Empty, packedItem.API_CustomsUQ3);

			packedItem.API_CustomsUQ2 = "LPA";
			UOMDefaulter.DefaultUOMIfApplicable(rateVolumeUnit, packedItem);
			AssertEquals("API_CustomsUQ2: When unit is set in Second, unit is set in Third", "LPA", packedItem.API_CustomsUQ2);
			AssertEquals("API_CustomsUQ3: When unit is set in Second, unit is set in Third", "LTR", packedItem.API_CustomsUQ3);

			packedItem.API_CustomsUQ = "TNE";
			packedItem.API_CustomsUQ2 = ZString.Empty;
			packedItem.API_CustomsUQ3 = ZString.Empty;
			UOMDefaulter.DefaultUOMIfApplicable(rateWeightUnit, packedItem);
			AssertEquals("API_CustomsUQ2: When weight unit is set in First, no unit set", ZString.Empty, packedItem.API_CustomsUQ2);
			AssertEquals("API_CustomsUQ3: When weight unit is set in First, no unit set", ZString.Empty, packedItem.API_CustomsUQ3);
		}
	}
}
