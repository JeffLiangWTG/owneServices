using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.Common;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.GUI.Testing
{
	sealed class UniversalTariffExportFindBoxTest : TestCase
	{
		public void TestTariffType()
		{
			using (var findBox = new UniversalTariffExportFindBoxForTest())
			{
				AssertEquals(Universal.Constants.TariffTypes.Export, findBox.TariffType);
				AssertType<AUExportTariffUniversalFormatter>(findBox.TariffFormatter);
			}
		}

		class UniversalTariffExportFindBoxForTest : UniversalTariffExportFindBox
		{
			public UniversalTariffExportFindBoxForTest()
			{ }

			public ITariffFormatter TariffFormatter => base.GetTariffFormatter();
		}
	}
}
