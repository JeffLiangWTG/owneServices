using Enterprise.Customs.KR.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.GUI.Testing
{
	[TestedType(typeof(MethodTwoToSixDocumentLayoutBuilder<JobDeclaration>))]
	sealed class MethodTwoToSixDocumentLayoutBuilderTest : ColumnLayoutBuilderAbstractTest<MethodTwoToSixDocumentLayoutBuilder<JobDeclaration>, JobDeclaration, MethodTwoToSixControlBag>
	{
		public void TestControlsVisibility()
		{
			var declaration = Factory.New<JobDeclaration>();
			var layout = ((IPanelLayoutProvider)new MethodTwoToSixDocumentLayout()).Layout;

			AssertEquals(true, layout.IsVisible(MethodTwoToSixControlBag.InstanceForDeclaration.ExpectedCustomsValueCalcEdit, declaration));
			AssertEquals(true, layout.IsVisible(MethodTwoToSixControlBag.InstanceForDeclaration.SupportingDocument1TextBox, declaration));
			AssertEquals(true, layout.IsVisible(MethodTwoToSixControlBag.InstanceForDeclaration.SupportingDocument2TextBox, declaration));
		}

		protected override int ExpectedMaxColumns => 1;

		protected override ColumnLayoutBuilderCaptionWidthSize ExpectedCaptionWidth => ColumnLayoutBuilderCaptionWidthSize.Long;

		protected override MethodTwoToSixDocumentLayoutBuilder<JobDeclaration> GetColumnLayoutBuilderForTesting() => new MethodTwoToSixDocumentLayoutBuilder<JobDeclaration>(MethodTwoToSixControlBag.InstanceForDeclaration);
	}
}
