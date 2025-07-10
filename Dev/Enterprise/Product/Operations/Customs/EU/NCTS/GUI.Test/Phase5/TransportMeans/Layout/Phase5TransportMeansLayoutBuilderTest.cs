using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	[TestedType(typeof(Phase5TransportMeansLayoutBuilder<CusInBondEvent>))]
	class Phase5TransportMeansLayoutBuilderTest : ColumnLayoutBuilderAbstractTest<Phase5TransportMeansLayoutBuilder<CusInBondEvent>, CusInBondEvent, Phase5TransportMeansControlBag>
	{
		protected override int ExpectedMaxColumns => 1;

		protected override Phase5TransportMeansLayoutBuilder<CusInBondEvent> GetColumnLayoutBuilderForTesting() => new Phase5TransportMeansLayoutBuilder<CusInBondEvent>();
	}
}
