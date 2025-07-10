using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.ES.Business.Declaration.Testing
{
	public class CusSupplyChainActorReferenceProviderTest : TestCaseWithFactory
	{
		public void TestGetByDataGroupingCode()
		{
			var provider = EU.Business.Declaration.CusSupplyChainActorReferenceProvider.GetByDataGroupingCode(Core.Constants.CountryCodes.Spain);
			CombineAssertions(() =>
			{
				AssertType<CusSupplyChainActorReferenceProvider>("Type", provider);
				AssertEquals("DataGroupingCode", Core.Constants.CountryCodes.Spain, provider.DataGroupingCode);
			});
		}

		public void TestGetReferenceFromOwnerCore()
		{
			AssertEquals("Identification (TCUI/EORI)", EU.Business.Declaration.CusSupplyChainActorReferenceProvider.GetByDataGroupingCode(Core.Constants.CountryCodes.Germany).OverwrittenReferenceColumnCaption);
		}
	}
}
