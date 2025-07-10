using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	[TestedType(typeof(SupplyChainActorLayoutBuilder<CusSupplyChainActorReference>))]
	class SupplyChainActorLayoutBuilderTest : ColumnLayoutBuilderAbstractTest<SupplyChainActorLayoutBuilder<CusSupplyChainActorReference>, CusSupplyChainActorReference, SupplyChainActorControlBag>
	{
		protected override SupplyChainActorLayoutBuilder<CusSupplyChainActorReference> GetColumnLayoutBuilderForTesting() => new SupplyChainActorLayoutBuilder<CusSupplyChainActorReference>();

		protected override int ExpectedMaxColumns => 1;
	}
}
