using System.Linq;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.Customs.IT.GUI.PlugIn;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IT.GUI.Testing;

[TestedType(typeof(EntryManualReleaseForm))]
sealed class EntryManualReleaseFormTest : ZFormBasherTest
{
	public void TestFormHeading()
	{
		using (var form = new EntryManualReleaseForm(manualRelease))
		{
			form.Show();
			AssertEquals("FormHeading", "Manual Release", form.FormHeading);
		}
	}

	public void TestCodeLabelCaption()
	{
		using (var form = new EntryManualReleaseForm(manualRelease))
		{
			form.Show();
			AssertEquals("Code Label  Caption", "Release code", form.ReleaseCodeTextBox.CaptionResourceString.Caption);
		}
	}

	public void TestDateLabelCaption()
	{
		using (var form = new EntryManualReleaseForm(manualRelease))
		{
			form.Show();
			AssertEquals("Date Label  Caption", "Release date", form.ReleaseDateEdit.CaptionResourceString.Caption);
		}
	}

	public void TestLayout()
	{
		using (var manualReleaseForm = new EntryManualReleaseForm(manualRelease))
		{
			manualReleaseForm.Show();
			var controls = manualReleaseForm.Controls;
			var releaseCodeTextBox = controls.Find("ReleaseCodeTextBox", true).Single() as ZTextBox;
			var releaseDateEdit = controls.Find("ReleaseDateEdit", true).Single() as ZDateEdit;
			var okButton = controls.Find("OkButton", true).Single() as ZButton;
			var cancelButton = controls.Find("AbortButton", true).Single() as ZButton;

			AssertEquals("ReleaseCodeTextBox Visible", true, releaseCodeTextBox.Visible);
			AssertEquals("ReleaseCodeTextBox CharacterCasing", CharacterCasing.Normal, releaseCodeTextBox.CharacterCasing);
			AssertEquals("ReleaseCodeTextBox BindTo", "ReleaseCode", releaseCodeTextBox.BindTo);

			AssertEquals("ReleaseDateEdit Visible", true, releaseDateEdit.Visible);
			AssertEquals("ReleaseDateEdit BindTo", "ReleaseDate", releaseDateEdit.BindTo);

			AssertEquals("OK button Visible", true, okButton.Visible);
			AssertEquals("OK button must be enabled", true, okButton.Enabled);
			AssertEquals("Cancel button Visible", true, cancelButton.Visible);
			AssertEquals("Cancel button must be enabled", true, cancelButton.Enabled);
		}
	}

	public void TestClickOkButtonWhenBusinessEntityHasErrors()
	{
		using (var form = new EntryManualReleaseForm(manualRelease))
		{
			form.Show();
			form.OkButton.PerformClick();

			CombineAssertions(() =>
			{
				AssertEquals("Form must be visible", true, form.Visible);
				AssertEquals("Form DialogResult", DialogResult.None, form.DialogResult);
				AssertEquals("Last message prompted to the user", "There are errors - can't save.", UnitTestUserNotification.Instance.LastMessage.Text);
			});
		}
	}

	public void TestClickOkButtonWhenBusinessEntityDoesNotHaveErrors()
	{
		var manualRelease = new EntryManualReleaseHandler(Factory.New<CusEntryHeader>());
		using (var form = new EntryManualReleaseForm(manualRelease))
		{
			form.Show();

			manualRelease.ReleaseCode = "123456";
			manualRelease.ReleaseDate = ZDate.Today;

			form.OkButton.PerformClick();

			CombineAssertions(() =>
			{
				AssertEquals("Form must be visible", false, form.Visible);
				AssertEquals("Form DialogResult", DialogResult.OK, form.DialogResult);
			});
		}
	}

	protected override void SetUp()
	{
		base.SetUp();

		manualRelease = new EntryManualReleaseHandler(Factory.New<CusEntryHeader>());
	}
	EntryManualReleaseHandler manualRelease;

	protected override Form GetFormToBashCore() => new EntryManualReleaseForm(manualRelease);
}
