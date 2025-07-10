using Enterprise.Rating.Business;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	sealed class PricingPageTableLclCellFormatterTest : PricingPageTableCellFormatterBaseTest<PricingPageTableLclCellFormatter>
	{
		public void TestMinimumOrPerUnit()
		{
			SetMinimumOrPerUnit(TariffLine, 0, 0);
			AssertSuccess(TariffLine, "0.00", "0.00");

			SetMinimumOrPerUnit(TariffLine, 100, 0);
			AssertSuccess(TariffLine, "0.00", "100.00");

			SetMinimumOrPerUnit(TariffLine, 0, 50);
			AssertSuccess(TariffLine, "50.00", "0.00");

			SetMinimumOrPerUnit(TariffLine, 100, 50);
			AssertSuccess(TariffLine, "50.00", "100.00");
		}

		#region Implementation

		protected override PricingPageTableLclCellFormatter NewFormatter()
		{
			return new PricingPageTableLclCellFormatter(RatingConstants.Units.CN);
		}

		#endregion
	}
}
