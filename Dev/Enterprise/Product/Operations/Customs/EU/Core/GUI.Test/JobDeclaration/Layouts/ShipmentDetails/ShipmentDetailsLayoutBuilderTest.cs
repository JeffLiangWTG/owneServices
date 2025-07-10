using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.GUI.Testing
{
	[TestedType(typeof(ShipmentDetailsLayoutBuilder<JobDeclaration>))]
	class ShipmentDetailsLayoutBuilderTest : ColumnLayoutBuilderAbstractTest<ShipmentDetailsLayoutBuilder<JobDeclaration>, JobDeclaration, Customs.GUI.ShipmentDetailsControlBag>
	{
		protected override int ExpectedMaxColumns => 1;

		protected override ShipmentDetailsLayoutBuilder<JobDeclaration> GetColumnLayoutBuilderForTesting() => new ShipmentDetailsLayoutBuilder<JobDeclaration>();
	}
}
