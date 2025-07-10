using Enterprise.Customs.EU.NCTS.Business.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using ESCusSupplyChainActorReferenceProvider = Enterprise.Customs.ES.Business.Declaration.CusSupplyChainActorReferenceProvider;
using EUCusSupplyChainActorReferenceProvider = Enterprise.Customs.EU.Business.Declaration.CusSupplyChainActorReferenceProvider;

namespace Enterprise.Customs.ES.NCTS.Business.Testing
{
	[TestedType(typeof(CusSupplyChainActorReference))]
	sealed class CusSupplyChainActorReferenceTest : CusSupplyChainActorReferenceAbstractTest<CusSupplyChainActorReference>
	{
		public void TestDataGroupingCode()
		{
			CombineAssertions(() =>
			{
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Spain))
				{
					var supplyChainActorReference = Factory.New<CusSupplyChainActorReference>();
					AssertEquals("For Spain DataGroupingCode", Core.Constants.CountryCodes.Spain, supplyChainActorReference.Provider.DataGroupingCode);
					AssertType<ESCusSupplyChainActorReferenceProvider>(supplyChainActorReference.Provider);
				}
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Latvia))
				{
					var supplyChainActorReference = Factory.New<CusSupplyChainActorReference>();
					AssertEquals("For Latvia DataGroupingCode", Core.Constants.CountryCodes.Latvia, supplyChainActorReference.Provider.DataGroupingCode);
					AssertType<EUCusSupplyChainActorReferenceProvider>(supplyChainActorReference.Provider);
				}
			});
		}
	}
}
