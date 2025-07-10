using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(CACClass.Loader))]
	sealed class CACClassTestLoaderTest : LoaderTestCase
	{
		public void TestLoadFromCode()
		{
			CACClass tariff = Factory.New<CACClass>();
			tariff.CT_Tariff = "0000000000";
			tariff.CT_LongDescription = "Long description";

			CACClass.Loader loader = new CACClass.Loader(Factory);
			CACClass tariff2 = loader.LoadFromCode("0000000000");
			AssertEquals(tariff2, tariff);
		}

		protected override BusinessObject.Loader GetNewLoaderToTest()
		{
			return new CACClass.Loader(Factory);
		}
	}
}
