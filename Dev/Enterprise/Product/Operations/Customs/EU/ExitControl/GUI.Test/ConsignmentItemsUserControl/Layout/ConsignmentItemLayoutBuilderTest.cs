using Enterprise.Customs.EU.ExitControl.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.ExitControl.GUI.Testing
{
	[TestedType(typeof(ConsignmentItemLayoutBuilder<CusExitConsignmentItem>))]
	sealed class ConsignmentItemLayoutBuilderTest : ColumnLayoutBuilderAbstractTest<ConsignmentItemLayoutBuilder<CusExitConsignmentItem>, CusExitConsignmentItem, ConsignmentItemControlBag>
	{
		protected override int ExpectedMaxColumns => 1;

		protected override ConsignmentItemLayoutBuilder<CusExitConsignmentItem> GetColumnLayoutBuilderForTesting() => new ConsignmentItemLayoutBuilder<CusExitConsignmentItem>();
	}
}
