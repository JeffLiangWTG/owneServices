using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(CACExportTariff.Loader))]
	sealed class CACExportTariffLoaderTest : LoaderTestCase
	{
		public void TestLoadFromCode()
		{
			CACExportTariff tariff = Factory.New<CACExportTariff>();
			tariff.CE_Code = "0000000000";
			tariff.CE_Description = "Short description";

			CACExportTariff.Loader loader = new CACExportTariff.Loader(Factory);
			CACExportTariff tariff2 = loader.LoadFromCode("0000000000");
			AssertEquals(tariff2, tariff);
		}

		protected override BusinessObject.Loader GetNewLoaderToTest()
		{
			return new CACExportTariff.Loader(Factory);
		}
	}
}
