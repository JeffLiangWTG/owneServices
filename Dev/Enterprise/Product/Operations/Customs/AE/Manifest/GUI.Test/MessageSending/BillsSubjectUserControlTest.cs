using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.AE.Manifest.Business;
using Enterprise.Customs.GUI.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AE.Manifest.GUI.Testing;

sealed class BillsSubjectUserControlTest : TestCaseWithFactory
{
	public void TestControls()
	{
		using var control = new BillsSubjectUserControl();
		CombineAssertions(() =>
		{
			control.AssertContainsControl<ZTextBox>("SubjectTextBox", x => x
				.WithBindTo(nameof(MessageChooserItem.Subject))
				.WithMultiline()
				.WithSizeScaled(260, 123)
			);
			control.AssertContainsControl<ZGroupBox>("SubjectGroupBox", x => x
				.WithCaption("Subject")
				.WithSizeScaled(266, 142)
			);
			control.AssertContainsControl<ZGroupBox>("SubjectGroupBox").AssertContainsControl("SubjectTextBox");
		});
	}
}
