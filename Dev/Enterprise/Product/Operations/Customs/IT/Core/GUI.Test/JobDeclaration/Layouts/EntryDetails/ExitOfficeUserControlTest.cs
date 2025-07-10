using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IT.GUI.Testing;

sealed class ExitOfficeUserControlTest : EntryDetailsChildUserControlTest<ExitOfficeUserControl>
{
	protected override IEnumerable<(string, Type)> ControlNames
	{
		get
		{
			yield return (ExitOfficeTextBox, typeof(ZTextBox));
			yield return (ExitOfficeDescriptionTextBox, typeof(ZTextBox));
		}
	}

	public void TestExitStatusTextBox_Width()
	{
		var childControl = Control.FindSingleOrDefault<Control>(ExitOfficeTextBox);
		AssertNotNull(childControl);
		AssertEquals($"{ExitOfficeTextBox} Width", 60, childControl.Size.Width);
	}

	public void TestExitStatusDescriptionTextBox_Width()
	{
		var childControl = Control.FindSingleOrDefault<Control>(ExitOfficeDescriptionTextBox);
		AssertNotNull(childControl);
		AssertEquals($"{ExitOfficeDescriptionTextBox} Width", 188, childControl.Size.Width);
	}

	public void TestExitOfficeTextBoxCaption()
	{
		var sut = Control.ExitOfficeTextBox;
		var labelCaptionVisible = new LabelCaptionRenderProvider().GetLabelCaptionVisible(sut);

		AssertNull("ExitOfficeTextBox caption", sut.CaptionResourceString.Caption);
		AssertEquals("ExitOfficeTextBox caption visible", expected: false, labelCaptionVisible);
	}

	public void TestExitOfficeDescriptionTextBoxCaption()
	{
		var sut = Control.ExitOfficeDescriptionTextBox;
		var labelCaptionVisible = new LabelCaptionRenderProvider().GetLabelCaptionVisible(sut);

		AssertNull("ExitOfficeDescriptionTextBox caption", sut.CaptionResourceString.Caption);
		AssertEquals("ExitOfficeDescriptionTextBox caption visible", expected: false, labelCaptionVisible);
	}

	public void TestImplementIExtendedControl()
	{
		AssertEquals("Implements IExtendedControl", expected: true, Control is IExtendedControl);
		AssertEquals("Host", Control, Control.Host);
		AssertNotNull("Extensions", Control.Extensions);
	}

	const string ExitOfficeTextBox = "ExitOfficeTextBox";

	const string ExitOfficeDescriptionTextBox = "ExitOfficeDescriptionTextBox";
}
