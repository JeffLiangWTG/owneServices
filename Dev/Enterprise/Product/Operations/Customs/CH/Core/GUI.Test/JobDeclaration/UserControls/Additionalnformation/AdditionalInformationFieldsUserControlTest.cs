using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CH.GUI.Testing;

public sealed class AdditionalInformationFieldsUserControlTest : TestCaseWithFactory
{
	public void TestCodeDropEdit()
	{
		using (control)
		{
			var codeDropEditBox = control.CodeDropEdit;
			AssertNotNull(nameof(codeDropEditBox), codeDropEditBox);
			AssertType<ZDropEdit>(control.CodeDropEdit);
			AssertEquals(nameof(codeDropEditBox.Visible), true, codeDropEditBox.Visible);
		}
	}

	public void TestReferenceNumberDropEdit()
	{
		using (control)
		{
			var referenceNumberDropEditBox = control.ReferenceNumberDropEdit;
			AssertNotNull(nameof(referenceNumberDropEditBox), referenceNumberDropEditBox);
			AssertType<ZDropEdit>(control.ReferenceNumberDropEdit);
			AssertEquals(nameof(referenceNumberDropEditBox.Visible), true, referenceNumberDropEditBox.Visible);
		}
	}

	public void TestDescriptionTextBox()
	{
		using (control)
		{
			var descriptionTextBox = control.DescriptionTextBox;
			AssertNotNull(nameof(descriptionTextBox), descriptionTextBox);
			AssertType<ZDropEdit>(control.ReferenceNumberDropEdit);
			AssertEquals(nameof(descriptionTextBox.Visible), true, descriptionTextBox.Visible);
		}
	}

	protected override void SetUp()
	{
		base.SetUp();
		control = new AdditionalInformationFieldsUserControl();
	}

	protected override void TearDown()
	{
		base.TearDown();
		control.Dispose();
	}
	AdditionalInformationFieldsUserControl control;
}
