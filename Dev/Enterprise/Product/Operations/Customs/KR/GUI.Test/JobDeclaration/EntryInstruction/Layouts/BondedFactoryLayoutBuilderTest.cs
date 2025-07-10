using Enterprise.Customs.Common.KR;
using Enterprise.Customs.KR.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.GUI.Testing
{
	[TestedType(typeof(BondedFactoryLayoutBuilder))]
	sealed class BondedFactoryLayoutBuilderTest : ColumnLayoutBuilderAbstractTest<BondedFactoryLayoutBuilder, JobDeclaration, EntryInstructionLayoutControlBag>
	{
		public void TestControlsVisibility()
		{
			var declaration = Factory.New<JobDeclaration>();
			var layout = ((IPanelLayoutProvider)new EntryDetailsLayout()).Layout;

			CombineAssertions(() =>
			{
				declaration.JE_MessageType = KRJobMessageTypeList.Codes.Import;
				AssertEquals(true, layout.IsVisible(EntryInstructionLayoutControlBag.Instance.UseTypeDropEdit, declaration));
				AssertEquals(true, layout.IsVisible(EntryInstructionLayoutControlBag.Instance.UseDateEdit, declaration));
			});
		}

		protected override int ExpectedMaxColumns => 1;

		protected override ColumnLayoutBuilderCaptionWidthSize ExpectedCaptionWidth => ColumnLayoutBuilderCaptionWidthSize.Medium;

		protected override BondedFactoryLayoutBuilder GetColumnLayoutBuilderForTesting()
		{
			var builder = new BondedFactoryLayoutBuilder();
			builder.AddControlBag(EntryInstructionLayoutControlBag.Instance);
			return builder;
		}
	}
}
