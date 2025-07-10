using System;
using System.Collections.Generic;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.IT.GUI.Testing;

sealed class MessageStatusUserControlTest : EntryDetailsChildUserControlTest<MessageStatusUserControl>
{
	protected override IEnumerable<(string, Type)> ControlNames
	{
		get
		{
			yield return ("MessageStatusTextBox", typeof(ZTextBox));
			yield return ("MessageStatusDescriptionTextBox", typeof(ZTextBox));
		}
	}

	public void TestMessageStatusTextBoxCaption()
	{
		var sut = Control.MessageStatusTextBox;
		var labelCaptionVisible = new LabelCaptionRenderProvider().GetLabelCaptionVisible(sut);

		AssertNull("MessageStatusTextBox caption", sut.CaptionResourceString.Caption);
		AssertEquals("MessageStatusTextBox caption visible", expected: false, labelCaptionVisible);
	}

	public void TestMessageStatusDescriptionTextBoxCaption()
	{
		var sut = Control.MessageStatusDescriptionTextBox;
		var labelCaptionVisible = new LabelCaptionRenderProvider().GetLabelCaptionVisible(sut);

		AssertNull("MessageStatusDescriptionTextBox caption", sut.CaptionResourceString.Caption);
		AssertEquals("MessageStatusDescriptionTextBox caption visible", expected: false, labelCaptionVisible);
	}

	public void TestImplementIExtendedControl()
	{
		AssertEquals("Implements IExtendedControl", expected: true, Control is IExtendedControl);
		AssertEquals("Host", Control, Control.Host);
		AssertNotNull("Extensions", Control.Extensions);
	}
}
