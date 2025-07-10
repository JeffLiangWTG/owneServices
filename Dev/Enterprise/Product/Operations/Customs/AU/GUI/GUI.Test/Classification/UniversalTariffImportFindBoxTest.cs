using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.Common;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.GUI.Testing
{
	sealed class UniversalTariffImportFindBoxTest : TestCase
	{
		public void TestTariffType()
		{
			using (var findBox = new UniversalTariffImportFindBoxForTest())
			{
				AssertEquals(Universal.Constants.TariffTypes.Import, findBox.TariffType);
				AssertType<AUImportTariffUniversalFormatter>(findBox.TariffFormatter);
			}
		}

		class UniversalTariffImportFindBoxForTest : UniversalTariffImportFindBox
		{
			public UniversalTariffImportFindBoxForTest()
			{ }

			public ITariffFormatter TariffFormatter => base.GetTariffFormatter();
		}
	}
}
