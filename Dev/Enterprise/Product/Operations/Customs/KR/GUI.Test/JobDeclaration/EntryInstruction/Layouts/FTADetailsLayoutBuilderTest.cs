using Enterprise.Customs.Common.KR;
using Enterprise.Customs.KR.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.GUI.Testing
{
	[TestedType(typeof(FTADetailsLayoutBuilder))]
	sealed class FTADetailsLayoutBuilderTest : ColumnLayoutBuilderAbstractTest<FTADetailsLayoutBuilder, JobDeclaration, FTADetailsControlBag>
	{
		public void TestControlsVisibility()
		{
			var declaration = Factory.New<JobDeclaration>();
			var layout = ((IPanelLayoutProvider)new EntryDetailsLayout()).Layout;

			CombineAssertions(() =>
			{
				declaration.JE_MessageType = KRJobMessageTypeList.Codes.Import;
				AssertEquals(true, layout.IsVisible(FTADetailsControlBag.Instance.LawCodeDropEdit, declaration));
				AssertEquals(true, layout.IsVisible(FTADetailsControlBag.Instance.CustomsDisbursementBillDropEdit, declaration));
			});
		}

		protected override int ExpectedMaxColumns => 1;

		protected override ColumnLayoutBuilderCaptionWidthSize ExpectedCaptionWidth => ColumnLayoutBuilderCaptionWidthSize.Long;

		protected override FTADetailsLayoutBuilder GetColumnLayoutBuilderForTesting()
		{
			var builder = new FTADetailsLayoutBuilder();
			builder.AddControlBag(FTADetailsControlBag.Instance);
			return builder;
		}
	}
}
