using Enterprise.Customs.AE.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AE.GUI.Testing;

[TestedType(typeof(ShipmentDetailsLayoutBuilder))]
sealed class ShipmentDetailsLayoutBuilderTest : ColumnLayoutBuilderAbstractTest<ShipmentDetailsLayoutBuilder, JobDeclaration, Customs.GUI.ShipmentDetailsControlBag>
{
	protected override ShipmentDetailsLayoutBuilder GetColumnLayoutBuilderForTesting() => new ShipmentDetailsLayoutBuilder();

	protected override int ExpectedMaxColumns => 2;
}
