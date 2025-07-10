using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.DE.Business.Declaration.Testing
{
	class CusSupplyChainActorReferenceProviderTest : TestCaseWithFactory
	{
		public void TestGetByDataGroupingCode()
		{
			var provider = EU.Business.Declaration.CusSupplyChainActorReferenceProvider.GetByDataGroupingCode(Core.Constants.CountryCodes.Germany);
			CombineAssertions(() =>
			{
				AssertType<CusSupplyChainActorReferenceProvider>("Type", provider);
				AssertEquals("DataGroupingCode", Core.Constants.CountryCodes.Germany, provider.DataGroupingCode);
			});
		}

		public void TestOverwrittenReferenceColumnCaption()
		{
			AssertEquals("Identification (TCUI/EORI)", EU.Business.Declaration.CusSupplyChainActorReferenceProvider.GetByDataGroupingCode(Core.Constants.CountryCodes.Germany).OverwrittenReferenceColumnCaption);
		}
	}
}
