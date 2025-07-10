using Enterprise.Customs.ES.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ES.GUI.Testing;

[TestedType(typeof(VehicleDetailsLayoutBuilder))]
internal class VehicleDetailsLayoutBuilderTest : ColumnLayoutBuilderAbstractTest<VehicleDetailsLayoutBuilder, CusVehicle, VehicleDetailsControlBag>
{
	protected override int ExpectedMaxColumns => 1;
	protected override VehicleDetailsLayoutBuilder GetColumnLayoutBuilderForTesting() => new VehicleDetailsLayoutBuilder();
}
