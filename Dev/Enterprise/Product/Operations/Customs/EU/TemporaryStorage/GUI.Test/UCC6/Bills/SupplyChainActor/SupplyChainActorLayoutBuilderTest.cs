using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.TemporaryStorage.GUI.Testing
{
	[TestedType(typeof(SupplyChainActorLayoutBuilder))]
	sealed class SupplyChainActorLayoutBuilderTest : ColumnLayoutBuilderAbstractTest<SupplyChainActorLayoutBuilder, CusSupplyChainActorReference, SupplyChainActorControlBag>
	{
		protected override SupplyChainActorLayoutBuilder GetColumnLayoutBuilderForTesting() => new SupplyChainActorLayoutBuilder();
		protected override int ExpectedMaxColumns => 1;
	}
}
