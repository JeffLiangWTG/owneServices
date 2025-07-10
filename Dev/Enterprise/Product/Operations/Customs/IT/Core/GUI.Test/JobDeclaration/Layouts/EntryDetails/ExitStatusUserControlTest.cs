using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IT.GUI.Testing;

sealed class ExitStatusUserControlTest : EntryDetailsChildUserControlTest<ExitStatusUserControl>
{
	protected override IEnumerable<(string, Type)> ControlNames
	{
		get
		{
			yield return (ExitStatusTextBox, typeof(ZTextBox));
			yield return (ExitStatusDescriptionTextBox, typeof(ZTextBox));
		}
	}

	public void TestExitStatusTextBox_Width()
	{
		var childControl = Control.FindSingleOrDefault<Control>(ExitStatusTextBox);
		AssertNotNull(childControl);
		AssertEquals($"{ExitStatusTextBox} Width", 60, childControl.Size.Width);
	}

	public void TestExitStatusDescriptionTextBox_Width()
	{
		var childControl = Control.FindSingleOrDefault<Control>(ExitStatusDescriptionTextBox);
		AssertNotNull(childControl);
		AssertEquals($"{ExitStatusDescriptionTextBox} Width", 188, childControl.Size.Width);
	}

	public void TestExitStatusTextBoxCaption()
	{
		var sut = Control.ExitStatusTextBox;
		var labelCaptionVisible = new LabelCaptionRenderProvider().GetLabelCaptionVisible(sut);

		AssertNull("ExitStatusTextBox caption", sut.CaptionResourceString.Caption);
		AssertEquals("ExitStatusTextBox caption visible", expected: false, labelCaptionVisible);
	}

	public void TestExitStatusDescriptionTextBoxCaption()
	{
		var sut = Control.ExitStatusDescriptionTextBox;
		var labelCaptionVisible = new LabelCaptionRenderProvider().GetLabelCaptionVisible(sut);

		AssertNull("ExitStatusDescriptionTextBox caption", sut.CaptionResourceString.Caption);
		AssertEquals("ExitStatusDescriptionTextBox caption visible", expected: false, labelCaptionVisible);
	}

	public void TestImplementIExtendedControl()
	{
		AssertEquals("Implements IExtendedControl", expected: true, Control is IExtendedControl);
		AssertEquals("Host", Control, Control.Host);
		AssertNotNull("Extensions", Control.Extensions);
	}

	const string ExitStatusTextBox = "ExitStatusTextBox";

	const string ExitStatusDescriptionTextBox = "ExitStatusDescriptionTextBox";
}
