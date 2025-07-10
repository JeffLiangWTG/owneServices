using Enterprise.Customs.ES.NCTS.Business;
using Enterprise.Customs.EU.NCTS.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ES.NCTS.GUI.Testing
{
	[TestedType(typeof(TransportAndPackagingLayoutBuilder<NctsDepartureMovementHeader>))]
	sealed class TransportAndPackagingLayoutBuilderTest : ColumnLayoutBuilderAbstractTest<TransportAndPackagingLayoutBuilder<NctsDepartureMovementHeader>, NctsDepartureMovementHeader, TransportAndPackagingControlBag>
	{
		protected override TransportAndPackagingLayoutBuilder<NctsDepartureMovementHeader> GetColumnLayoutBuilderForTesting() => new TransportAndPackagingLayoutBuilder<NctsDepartureMovementHeader>();

		protected override int ExpectedMaxColumns => 2;
	}
}
