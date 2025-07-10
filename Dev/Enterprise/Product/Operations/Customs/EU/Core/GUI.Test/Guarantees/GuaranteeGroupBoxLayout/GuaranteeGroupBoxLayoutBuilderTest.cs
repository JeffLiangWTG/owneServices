using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.GUI.Testing
{
	[TestedType(typeof(GuaranteeGroupBoxLayoutBuilder<CommonGuarantee>))]
	sealed class GuaranteeGroupBoxLayoutBuilderTest : ColumnLayoutBuilderAbstractTest<GuaranteeGroupBoxLayoutBuilder<CommonGuarantee>, CommonGuarantee, GuaranteeGroupBoxControlBag>
	{
		protected override GuaranteeGroupBoxLayoutBuilder<CommonGuarantee> GetColumnLayoutBuilderForTesting() => new GuaranteeGroupBoxLayoutBuilder<CommonGuarantee>();

		protected override int ExpectedMaxColumns => 1;
	}
}
