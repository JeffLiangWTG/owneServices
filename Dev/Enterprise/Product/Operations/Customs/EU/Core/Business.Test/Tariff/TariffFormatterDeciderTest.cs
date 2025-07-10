using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.EU.Business.Testing
{
	public class TariffFormatterDeciderTest : TestCaseWithFactory
	{
		public void TestGetTariffFormatter()
		{
			var tariffFormatterDecider = new TariffFormatterDecider();
			AssertEquals("France should be TariffFormatterTen", typeof(TariffFormatterTen), tariffFormatterDecider.GetTariffFormatter(Core.Constants.CountryCodes.France).GetType());
			AssertEquals("Germany should be TariffFormatterEleven", typeof(TariffFormatterEleven), tariffFormatterDecider.GetTariffFormatter(Core.Constants.CountryCodes.Germany).GetType());
		}
	}
}
