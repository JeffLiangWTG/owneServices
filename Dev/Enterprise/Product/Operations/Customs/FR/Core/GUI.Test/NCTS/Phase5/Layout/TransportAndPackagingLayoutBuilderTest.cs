using Enterprise.Customs.FR.Business.NCTS;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.FR.GUI.NCTS.Testing
{
	[TestedType(typeof(TransportAndPackagingLayoutBuilder<NctsDepartureMovementHeader>))]
	sealed class TransportAndPackagingLayoutBuilderTest : ColumnLayoutBuilderAbstractTest<TransportAndPackagingLayoutBuilder<NctsDepartureMovementHeader>, NctsDepartureMovementHeader, TransportAndPackagingControlBag>
	{
		protected override int ExpectedMaxColumns => 1;

		protected override TransportAndPackagingLayoutBuilder<NctsDepartureMovementHeader> GetColumnLayoutBuilderForTesting() => new TransportAndPackagingLayoutBuilder<NctsDepartureMovementHeader>();
	}
}
