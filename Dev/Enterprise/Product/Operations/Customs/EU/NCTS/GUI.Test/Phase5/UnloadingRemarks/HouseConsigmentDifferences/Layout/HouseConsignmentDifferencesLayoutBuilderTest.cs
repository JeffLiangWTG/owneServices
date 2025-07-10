using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	[TestedType(typeof(HouseConsignmentDifferencesLayoutBuilder<NctsBill>))]
	sealed class HouseConsignmentDifferencesLayoutBuilderTest : ColumnLayoutBuilderAbstractTest<HouseConsignmentDifferencesLayoutBuilder<NctsBill>, NctsBill, HouseConsignmentDifferencesControlBag>
	{
		protected override int ExpectedMaxColumns => 3;

		protected override HouseConsignmentDifferencesLayoutBuilder<NctsBill> GetColumnLayoutBuilderForTesting() => new HouseConsignmentDifferencesLayoutBuilder<NctsBill>();
	}
}
