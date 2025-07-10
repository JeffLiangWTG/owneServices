using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	[TestedType(typeof(DeclarationDetailsLayoutBuilder<NctsDepartureMovementHeader>))]
	sealed class DeclarationDetailsLayoutBuilderTest : ColumnLayoutBuilderAbstractTest<DeclarationDetailsLayoutBuilder<NctsDepartureMovementHeader>, NctsDepartureMovementHeader, DeclarationDetailsControlBag>
	{
		protected override int ExpectedMaxColumns => 2;

		protected override DeclarationDetailsLayoutBuilder<NctsDepartureMovementHeader> GetColumnLayoutBuilderForTesting() => new DeclarationDetailsLayoutBuilder<NctsDepartureMovementHeader>();
	}
}
