using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	[TestedType(typeof(TransportBorderLayoutBuilder<NctsDepartureMovementHeader>))]
	sealed class TransportBorderLayoutBuilderTest : ColumnLayoutBuilderAbstractTest<TransportBorderLayoutBuilder<NctsDepartureMovementHeader>, NctsDepartureMovementHeader, TransportBorderControlBag>
	{
		protected override int ExpectedMaxColumns => 2;

		protected override TransportBorderLayoutBuilder<NctsDepartureMovementHeader> GetColumnLayoutBuilderForTesting() => new TransportBorderLayoutBuilder<NctsDepartureMovementHeader>();
	}
}
