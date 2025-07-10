using Enterprise.Customs.GUI;
using Enterprise.Customs.KR.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.GUI.Testing
{
	[TestedType(typeof(MiscOptionsLayoutBuilder))]
	sealed class MiscOptionsLayoutBuilderTest : ColumnLayoutBuilderAbstractTest<MiscOptionsLayoutBuilder, JobDeclaration, CommonMiscOptionsControlBag>
	{
		public void TestMergeByDropEditVisibility()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.KR.KRJobMessageTypeList.Codes.Export;

			var layout = ((IPanelLayoutProvider)new LocalExportMiscOptionsLayout()).Layout;
			CombineAssertions(() =>
			{
				AssertEquals(true, layout.IsVisible(CommonMiscOptionsControlBag.Instance.MergeByDropEdit, declaration));

				declaration.JE_MessageType = Common.KR.KRJobMessageTypeList.Codes.LocalExport;
				AssertEquals(false, layout.IsVisible(CommonMiscOptionsControlBag.Instance.MergeByDropEdit, declaration));

				declaration.JE_MessageType = Common.KR.KRJobMessageTypeList.Codes.Import;
				AssertEquals(true, layout.IsVisible(CommonMiscOptionsControlBag.Instance.MergeByDropEdit, declaration));
			});
		}

		protected override MiscOptionsLayoutBuilder GetColumnLayoutBuilderForTesting() => new MiscOptionsLayoutBuilder();

		protected override ColumnLayoutBuilderCaptionWidthSize ExpectedCaptionWidth => ColumnLayoutBuilderCaptionWidthSize.Long;

		protected override int ExpectedMaxColumns => 1;
	}
}
