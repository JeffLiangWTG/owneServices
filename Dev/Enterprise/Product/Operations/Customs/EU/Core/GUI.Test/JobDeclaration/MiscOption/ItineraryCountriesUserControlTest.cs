using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.EU.GUI.Testing
{
	sealed class ItineraryCountriesUserControlTest : TestCaseWithFactory
	{
		public void TestItineraryCountriesGrid() => CombineAssertions(() =>
		{
			using (var control = new ItineraryCountriesUserControl())
			{
				var grid = control.ItineraryCountriesGrid;
				AssertEquals("Count", 2, grid.ColumnStyles.Count);
				AssertNotNull("CY_Order", grid.GetColumnStyle(nameof(ItineraryCountry.CY_Order)));
				AssertNotNull("CY_Code", grid.GetColumnStyle(nameof(ItineraryCountry.CY_Code)));
			}
		});
	}
}
