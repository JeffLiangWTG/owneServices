using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.Rating.Business.DocumentPrinting.DocAmount;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	sealed class PricingPageTableCellFormatterTest : PricingPageTableCellFormatterBaseTest<PricingPageTableCellFormatter>
	{
		public void TestUnit()
		{
			SetUnit(TariffLine, 0);
			AssertSuccess(TariffLine, "0.00");

			SetUnit(TariffLine, 100);
			AssertSuccess(TariffLine, "100.00");
		}

		public void TestFlat()
		{
			SetFlat(TariffLine, 0);
			AssertSuccess("No need to say 'Flat' if zero.", TariffLine, "0.00");

			SetFlat(TariffLine, 100);
			AssertSuccess("Say 'Flat' if not per-unit.", TariffLine, "100.00 Flat");

			SetFlat(TariffLine, 12345.6789);
			var message = "Amount should be comma separated and have 2 decimal place based on the Australian currency";
			AssertSuccess(message, TariffLine, "12,345.68 Flat");

			TariffLine.TL_RX_NKCurrency = Core.Constants.CurrencyCodes.Japan;
			message = "Amount should have no decimal places due to Japanese Currency";
			AssertSuccess(message, TariffLine, "12,346 Flat");

			var originalCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			try
			{
				GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Germany);
				AssertSuccess(message, TariffLine, "12.346 Flat");

				TariffLine.TL_RX_NKCurrency = Core.Constants.CurrencyCodes.Oman;
				AssertSuccess(message, TariffLine, "12.345,679 Flat");
			}
			finally
			{
				GlbCompany.CurrentCompany.SetCountry(originalCountry);
			}
		}

		public void TestFlatPlusPerUnit()
		{
			SetFlatPlusPerUnit(TariffLine, 0, 0);
			AssertSuccess(TariffLine, "0.00");

			SetFlatPlusPerUnit(TariffLine, 0, 50);
			AssertSuccess("Only unit portion specified.", TariffLine, "50.00");

			SetFlatPlusPerUnit(TariffLine, 100, 0);
			AssertSuccess("Only flat portion specified.", TariffLine, "100.00 Flat");

			SetFlatPlusPerUnit(TariffLine, 100, 50);
			AssertFailure("Too complex to display in one cell", TariffLine);
		}

		public void TestMinimumOrPerUnit()
		{
			SetMinimumOrPerUnit(TariffLine, 0, 0);
			AssertSuccess(TariffLine, "0.00");

			// not sure if this behaves like the MinimumCalculator or the FlatCalculator, assuming it's the former.
			SetMinimumOrPerUnit(TariffLine, 100, 0);
			AssertFailure("Too ambigious to display in one cell.", TariffLine);

			SetMinimumOrPerUnit(TariffLine, 0, 50);
			AssertSuccess(TariffLine, "50.00");

			SetMinimumOrPerUnit(TariffLine, 100, 50);
			AssertFailure("Too complex to display in one cell.", TariffLine);
		}

		public void TestFirstPlusAdditional()
		{
			SetFirstPlusAdditional(TariffLine, 0, 0);
			AssertSuccess(TariffLine, "0.00");

			SetFirstPlusAdditional(TariffLine, 100, 0);
			AssertSuccess("Behaves like the FlatCalculator when Additional = 0", TariffLine, "100.00 Flat");

			SetFirstPlusAdditional(TariffLine, 0, 50);
			AssertFailure("Too complex to display in one cell.", TariffLine);

			SetFirstPlusAdditional(TariffLine, 100, 50);
			AssertFailure("Too complex to display in one cell.", TariffLine);

			SetFirstPlusAdditional(TariffLine, 100, 100);
			AssertSuccess("Behaves like the UnitCalculator if First == Aditional", TariffLine, "100.00");
		}

		[ExpectException(typeof(DeveloperNotificationException))]
		public void TestCompanyTariffOrCostBased()
		{
			SetCompanyTariffBased(RateLine, 0, 0, 50, 0);
			Formatter.TryFormat(RateLine, DocAmount.Create(count: Formatter.RequiredColumnCount));
		}

		public void TestCurrencyDecimals()
		{
			SetUnit(TariffLine, 100);

			TariffLine.TL_RX_NKCurrency = "JPY";
			AssertSuccess("JPY = 0 decimal places", TariffLine, "100");

			TariffLine.TL_RX_NKCurrency = "AUD";
			AssertSuccess("AUD = 2 decimal places", TariffLine, "100.00");

			TariffLine.TL_RX_NKCurrency = "KWD";
			AssertSuccess("KWD = 3 decimal places", TariffLine, "100.000");
		}

		public void TestMultiples()
		{
			SetUnit(TariffLine, 100);

			TariffLine.TL_WeightVolumeMultiple = 0;
			AssertSuccess("Zero implies no multiple (go figgure).", TariffLine, "100.00");

			TariffLine.TL_WeightVolumeMultiple = 1;
			AssertSuccess("1 is the logical value for no multiple.", TariffLine, "100.00");

			TariffLine.TL_WeightVolumeMultiple = 10;
			AssertFailure("Not enough room to express multiples in the grid.", TariffLine);
		}

		public void TestIncompatableUnit()
		{
			TariffLine.TL_WeightVolume = RatingConstants.Units.M3;

			SetUnit(TariffLine, 0);
			AssertSuccess("Zero can safely ignore units.", TariffLine, "0.00");

			SetUnit(TariffLine, 50);
			AssertFailure("Incompatable unit.", TariffLine);

			SetFlat(TariffLine, 0);
			AssertSuccess("Zero and flat amounts can safely ignore units.", TariffLine, "0.00");

			SetFlat(TariffLine, 50);
			AssertSuccess("Flat amounts ignore units.", TariffLine, "50.00 Flat");
		}

		#region Implementation

		protected override PricingPageTableCellFormatter NewFormatter()
		{
			return new PricingPageTableCellFormatter(RatingConstants.Units.CN);
		}

		#endregion
	}
}
