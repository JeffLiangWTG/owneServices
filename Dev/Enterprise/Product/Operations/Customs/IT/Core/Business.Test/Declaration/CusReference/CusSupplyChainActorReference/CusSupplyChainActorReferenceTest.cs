using Enterprise.Customs.EU.Business.Declaration.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

[TestedType(typeof(CusSupplyChainActorReference))]
sealed class CusSupplyChainActorReferenceTest : CusSupplyChainActorReferenceAbstractTest<CusSupplyChainActorReference>
{
	public void TestValidation()
	{
		var supplyChainActorReference = Factory.NewWithValidTestData<CusSupplyChainActorReference>();
		AssertType<CusSupplyChainActorReferenceValidation>(nameof(supplyChainActorReference.Validation), supplyChainActorReference.Validation);
	}
}
