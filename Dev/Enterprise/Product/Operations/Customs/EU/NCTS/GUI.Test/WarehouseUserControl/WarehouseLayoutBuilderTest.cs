using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	[TestedType(typeof(WarehouseLayoutBuilder<NctsDepartureCargoDesc>))]
	sealed class WarehouseLayoutBuilderTest : ColumnLayoutBuilderAbstractTest<WarehouseLayoutBuilder<NctsDepartureCargoDesc>, NctsDepartureCargoDesc, WarehouseControlBag>
	{
		protected override WarehouseLayoutBuilder<NctsDepartureCargoDesc> GetColumnLayoutBuilderForTesting() => new WarehouseLayoutBuilder<NctsDepartureCargoDesc>();

		protected override int ExpectedMaxColumns => 1;
	}
}
