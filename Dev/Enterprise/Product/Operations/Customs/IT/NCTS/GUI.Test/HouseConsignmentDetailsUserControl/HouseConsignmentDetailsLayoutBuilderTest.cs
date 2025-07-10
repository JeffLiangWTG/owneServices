using Enterprise.Customs.IT.NCTS.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IT.NCTS.GUI.Testing;

[TestedType(typeof(HouseConsignmentDetailsLayoutBuilder<NctsBill>))]
sealed class HouseConsignmentDetailsLayoutBuilderTest : ColumnLayoutBuilderAbstractTest<HouseConsignmentDetailsLayoutBuilder<NctsBill>, NctsBill, HouseConsignmentDetailsControlBag>
{
	protected override int ExpectedMaxColumns => 3;

	protected override HouseConsignmentDetailsLayoutBuilder<NctsBill> GetColumnLayoutBuilderForTesting() => new HouseConsignmentDetailsLayoutBuilder<NctsBill>();
}
