using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI.SearchBox;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Testing;

class KeySenderTest : TestCase
{
	public void TestSendKeyDownTextBoxTypingBehavior()
	{
		using var form = new ZForm();
		var textBox = new ZTextBox();
		var keyDownRaised = false;
		var keyPressRaised = false;
		var keyUpRaised = false;
		textBox.KeyDown += (sender, e) => keyDownRaised = true;
		textBox.KeyPress += (sender, e) => keyPressRaised = true;
		textBox.KeyUp += (sender, e) => keyUpRaised = true;
		form.Controls.Add(textBox);
		form.Show();
		CombineAssertions(() =>
		{
			Assert("Precondition: KeyDown should not have been raised", !keyDownRaised);
			Assert("Precondition: KeyPress should not have been raised", !keyPressRaised);
			Assert("Precondition: KeyUp should not have been raised", !keyUpRaised);
			AssertEquals("Precondition: Text should be empty", string.Empty, textBox.Text);
			AssertEquals("Precondition: Cursor should be at start of textbox", 0, textBox.SelectionStart);
			AssertEquals("Precondition: Selection should be empty", 0, textBox.SelectionLength);
		});
		KeySender.SendKeyDown(textBox, textBox.Handle, Keys.A);
		CombineAssertions(() =>
		{
			Assert("KeyDown should have been raised", keyDownRaised);
			Assert("KeyPress should not have been raised", !keyPressRaised);
			Assert("KeyUp should not have been raised", !keyUpRaised);
			AssertEquals("Text should be empty", string.Empty, textBox.Text);
			AssertEquals("Cursor should be at start of textbox", 0, textBox.SelectionStart);
			AssertEquals("Selection should be empty", 0, textBox.SelectionLength);
		});
	}

	public void TestSendKeyDownToProcessCmdKeyTextBoxTypingBehavior()
	{
		using var form = new ZForm();
		var textBox = new ZTextBox();
		var keyDownRaised = false;
		var keyPressRaised = false;
		var keyUpRaised = false;
		textBox.KeyDown += (sender, e) => keyDownRaised = true;
		textBox.KeyPress += (sender, e) => keyPressRaised = true;
		textBox.KeyUp += (sender, e) => keyUpRaised = true;
		form.Controls.Add(textBox);
		form.Show();
		CombineAssertions(() =>
		{
			Assert("Precondition: KeyDown should not have been raised", !keyDownRaised);
			Assert("Precondition: KeyPress should not have been raised", !keyPressRaised);
			Assert("Precondition: KeyUp should not have been raised", !keyUpRaised);
			AssertEquals("Precondition: Text should be empty", string.Empty, textBox.Text);
			AssertEquals("Precondition: Cursor should be at start of textbox", 0, textBox.SelectionStart);
			AssertEquals("Precondition: Selection should be empty", 0, textBox.SelectionLength);
		});
		KeySender.SendKeyDownToProcessCmdKey(textBox, Keys.A);
		CombineAssertions(() =>
		{
			Assert("KeyDown should not have been raised", !keyDownRaised);
			Assert("KeyPress should not have been raised", !keyPressRaised);
			Assert("KeyUp should not have been raised", !keyUpRaised);
			AssertEquals("Text should be empty", string.Empty, textBox.Text);
			AssertEquals("Cursor should be at the start of the textbox", 0, textBox.SelectionStart);
			AssertEquals("Selection should be empty", 0, textBox.SelectionLength);
		});
	}

	public void TestSendKeyPressTextBoxTypingBehavior()
	{
		using var form = new ZForm();
		var textBox = new ZTextBox();
		textBox.CharacterCasing = CharacterCasing.Normal;
		var keyDownRaised = false;
		var keyPressRaised = false;
		var keyUpRaised = false;
		textBox.KeyDown += (sender, e) => keyDownRaised = true;
		textBox.KeyPress += (sender, e) => keyPressRaised = true;
		textBox.KeyUp += (sender, e) => keyUpRaised = true;
		form.Controls.Add(textBox);
		form.Show();
		CombineAssertions(() =>
		{
			Assert("Precondition: KeyDown should not have been raised", !keyDownRaised);
			Assert("Precondition: KeyPress should not have been raised", !keyPressRaised);
			Assert("Precondition: KeyUp should not have been raised", !keyUpRaised);
			AssertEquals("Precondition: Text should be empty", string.Empty, textBox.Text);
			AssertEquals("Precondition: Cursor should be at start of textbox", 0, textBox.SelectionStart);
			AssertEquals("Precondition: Selection should be empty", 0, textBox.SelectionLength);
		});
		KeySender.SendKeyPress(textBox, textBox.Handle, Keys.A);
		CombineAssertions(() =>
		{
			Assert("KeyDown should not have been raised", !keyDownRaised);
			Assert("KeyPress should have been raised", keyPressRaised);
			Assert("KeyUp should not have been raised", !keyUpRaised);
			AssertEquals("Text should be updated", "A", textBox.Text);
			AssertEquals("Cursor should be updated", 1, textBox.SelectionStart);
			AssertEquals("Selection should be empty", 0, textBox.SelectionLength);
		});
	}
	public void TestSendKeyPressTextBoxTabBehavior()
	{
		using var form = new ZForm();
		var textBox = new ZTextBox();
		var otherControl = new ZTextBox();
		form.Controls.Add(textBox);
		form.Controls.Add(otherControl);
		form.Show();
		CombineAssertions(() =>
		{
			Assert("Precondition: Textbox should be focused", textBox.Focused);
			Assert("Precondition: Other control should not be focused", !otherControl.Focused);
		});
		KeySender.SendKeyPress(textBox, textBox.Handle, Keys.Tab);
		CombineAssertions(() =>
		{
			Assert("Textbox should be focused", textBox.Focused);
			Assert("Other control should not be focused", !otherControl.Focused);
		});
	}
	public void TestSendKeyUpTextBoxTypingBehavior()
	{
		using var form = new ZForm();
		var textBox = new ZTextBox();
		var keyDownRaised = false;
		var keyPressRaised = false;
		var keyUpRaised = false;
		textBox.KeyDown += (sender, e) => keyDownRaised = true;
		textBox.KeyPress += (sender, e) => keyPressRaised = true;
		textBox.KeyUp += (sender, e) => keyUpRaised = true;
		form.Controls.Add(textBox);
		form.Show();
		CombineAssertions(() =>
		{
			Assert("Precondition: KeyDown should not have been raised", !keyDownRaised);
			Assert("Precondition: KeyPress should not have been raised", !keyPressRaised);
			Assert("Precondition: KeyUp should not have been raised", !keyUpRaised);
			AssertEquals("Precondition: Text should be empty", string.Empty, textBox.Text);
			AssertEquals("Precondition: Cursor should be at start of textbox", 0, textBox.SelectionStart);
			AssertEquals("Precondition: Selection should be empty", 0, textBox.SelectionLength);
		});
		KeySender.SendKeyUp(textBox, textBox.Handle, Keys.A);
		CombineAssertions(() =>
		{
			Assert("KeyDown should not have been raised", !keyDownRaised);
			Assert("KeyPress should not have been raised", !keyPressRaised);
			Assert("KeyUp should have been raised", keyUpRaised);
			AssertEquals("Text should be empty", string.Empty, textBox.Text);
			AssertEquals("Cursor should be at start of textbox", 0, textBox.SelectionStart);
			AssertEquals("Selection should be empty", 0, textBox.SelectionLength);
		});
	}
	public void TestSendKeyUpTextBoxTabBehavior()
	{
		using var form = new ZForm();
		var textBox = new ZTextBox();
		var otherControl = new ZTextBox();
		form.Controls.Add(textBox);
		form.Controls.Add(otherControl);
		form.Show();
		CombineAssertions(() =>
		{
			Assert("Precondition: Textbox should be focused", textBox.Focused);
			Assert("Precondition: Other control should not be focused", !otherControl.Focused);
		});
		KeySender.SendKeyUp(textBox, textBox.Handle, Keys.Tab);
		CombineAssertions(() =>
		{
			Assert("Textbox should be focused", textBox.Focused);
			Assert("Other control should not be focused", !otherControl.Focused);
		});
	}

	public void Test_SendKeyPress_ToTargetControlTest()
	{
		using var form = new Form();
		var targetTriggered = false;
		var secondaryTriggered = false;

		var targetListBox = new ZSearchListBox();
		targetListBox.DataSource = new List<IDisplayItem>
			{
				DisplayItemFactory.CreateSearchItem(() => targetTriggered = true, "Item 33", "Order 66")
			};
		targetListBox.SelectedIndex = 0;

		var secondaryListBox = new ZSearchListBox();
		secondaryListBox.DataSource = new List<IDisplayItem>
			{
				DisplayItemFactory.CreateSearchItem(() => secondaryTriggered = true, "Item 33", "Order 66")
			};
		secondaryListBox.SelectedIndex = 0;

		form.Controls.Add(targetListBox);
		form.Controls.Add(secondaryListBox);
		form.Show();

		CombineAssertions(() =>
		{
			KeySender.SendKeyPress(targetListBox, targetListBox.Handle, Keys.Enter);
			Assert("The target list box's key press has not been triggered.", targetTriggered);
			Assert("The secondary list box's key press has been triggered.", !secondaryTriggered);
			KeySender.SendKeyPress(secondaryListBox, secondaryListBox.Handle, Keys.Enter);
			Assert("The secondary list box's key press has not been triggered.", secondaryTriggered);
		});
	}

	public void Test_SendKeyDown_ShouldTriggerKeyDownEvent()
	{
		using var form = new Form();
		var textBox = new TextBox();
		var keyDownEventTriggered = false;
		var receivedKey = Keys.None;

		textBox.KeyDown += (sender, e) =>
		{
			keyDownEventTriggered = true;
			receivedKey = e.KeyCode;
		};
		form.Controls.Add(textBox);
		form.Show();

		KeySender.SendKeyDown(textBox, textBox.Handle, 'A');
		Assert("KeyDown event has not been triggered.", keyDownEventTriggered);
		AssertEquals(receivedKey, Keys.A);

		keyDownEventTriggered = false;
		receivedKey = Keys.None;
		KeySender.SendKeyDown(textBox, textBox.Handle, Keys.B);
		Assert("KeyDown event has not been triggered.", keyDownEventTriggered);
		AssertEquals(receivedKey, Keys.B);
	}

	public void Test_SendKeyUp_ShouldTriggerKeyUpEvent()
	{
		using var form = new Form();
		var textBox = new TextBox();
		var keyUpEventTriggered = false;
		var receivedKey = Keys.None;

		textBox.KeyUp += (sender, e) =>
		{
			keyUpEventTriggered = true;
			receivedKey = e.KeyCode;
		};
		form.Controls.Add(textBox);
		form.Show();

		KeySender.SendKeyUp(textBox, textBox.Handle, Keys.A);
		Assert("KeyUp event has not been triggered.", keyUpEventTriggered);
		AssertEquals(receivedKey, Keys.A);
	}

	public void Test_PostKeyDown_ShouldTriggerKeyDownEvent()
	{
		using var form = new Form();
		var textBox = new TextBox();
		var keyDownEventTriggered = false;
		var receivedKey = Keys.None;

		textBox.KeyDown += (sender, e) =>
		{
			keyDownEventTriggered = true;
			receivedKey = e.KeyCode;
		};
		form.Controls.Add(textBox);
		form.Show();

		KeySender.PostKeyDown(textBox, textBox.Handle, Keys.A);
		Application.DoEvents();

		Assert("KeyDown event has not been triggered.", keyDownEventTriggered);
		Assert(receivedKey == Keys.A);

		keyDownEventTriggered = false;
		receivedKey = Keys.None;
		KeySender.PostKeyDown(textBox, Keys.B);
		Application.DoEvents();

		Assert("KeyDown event has not been triggered.", keyDownEventTriggered);
		AssertEquals(receivedKey, Keys.B);
	}

	public void TestPostKeyUp_ShouldTriggerKeyUpEvent()
	{
		using var form = new Form();
		var textBox = new TextBox();
		var keyUpEventTriggered = false;
		var receivedKey = Keys.None;
		textBox.KeyUp += (sender, e) =>
		{
			keyUpEventTriggered = true;
			receivedKey = e.KeyCode;
		};
		form.Controls.Add(textBox);
		form.Show();

		KeySender.PostKeyUp(textBox, textBox.Handle, Keys.A);
		Application.DoEvents();

		Assert("KeyUp event has not been triggered.", keyUpEventTriggered);
		AssertEquals(receivedKey, Keys.A);
	}

	public void TestPostKeyDownTextBox_ShouldTriggerKeyDown_AndUpdateText()
	{
		using var form = new Form();
		var textBox = new TextBox();
		textBox.CharacterCasing = CharacterCasing.Normal;
		var keyDownRaised = false;
		var keyPressRaised = false;
		var keyUpRaised = false;

		textBox.KeyDown += (sender, e) => keyDownRaised = true;
		textBox.KeyPress += (sender, e) => keyPressRaised = true;
		textBox.KeyUp += (sender, e) => keyUpRaised = true;

		form.Controls.Add(textBox);
		form.Show();

		KeySender.PostKeyDown(textBox, textBox.Handle, Keys.A);
		Application.DoEvents();

		CombineAssertions(() =>
		{
			Assert("KeyDown should have been raised", keyDownRaised);
			Assert("KeyPress may or may not have been raised depending on TranslateMessage", keyPressRaised);
			Assert("KeyUp should not have been raised", !keyUpRaised);
			AssertEquals("Text should be changed", "a", textBox.Text);
		});
	}

	public void TestSendKeyPressTextBox()
	{
		var testCases = new[]
		{
			new { Key = (object)'a', ExpectedText = "a" },
			new { Key = (object)'A', ExpectedText = "A" },
			new { Key = (object)Keys.A, ExpectedText = "A" },
			new { Key = (object)Keys.Oemplus, ExpectedText = "»" },
			new { Key = (object)'=', ExpectedText = "=" },
			new { Key = (object)Keys.D1, ExpectedText = "1" },
			new { Key = (object)(Keys.D1 | Keys.Shift), ExpectedText = "1" }
		};

		CombineAssertions(() =>
		{
			foreach (var testCase in testCases)
			{
				TestTextBoxCore(textBox =>
				{
					textBox.CharacterCasing = CharacterCasing.Normal;
					if (testCase.Key is Keys key)
					{
						KeySender.SendKeyPress(textBox, textBox.Handle, key);
					}
					else if (testCase.Key is char ch)
					{
						KeySender.SendKeyPress(textBox, textBox.Handle, ch);
					}
				}, testCase.ExpectedText);
			}
		});
	}

	public void TestSendKeyPress_SpecialCharacterInput_ByKey()
	{
		TestTextBoxCore(textBox =>
		{
			var receivedKeyChar = '\0';
			textBox.KeyPress += (sender, e) =>
			{
				receivedKeyChar = e.KeyChar;
			};
			KeySender.SendKeyPress(textBox, Keys.Oemcomma);
			AssertEquals('¼', receivedKeyChar);
		}, "¼");
	}

	public void TestSendKeyPress_SpecialCharacterInput_ByChar()
	{
		TestTextBoxCore(textBox =>
		{
			var receivedKeyChar = '\0';
			textBox.KeyPress += (sender, e) =>
			{
				receivedKeyChar = e.KeyChar;
			};
			KeySender.SendKeyPress(textBox, ',');
			AssertEquals(',', receivedKeyChar);
		}, ",");
	}

	public void TestPostKeyDownTextBox()
	{
		var testCases = new[]
		{
			new { Key = Keys.Oemplus, ExpectedText = "=" },
			new { Key = Keys.D1, ExpectedText = "1" },
			new { Key = Keys.D1 | Keys.Shift, ExpectedText = "1" },
			new { Key = Keys.A, ExpectedText = "a" }
		};

		CombineAssertions(() =>
		{
			foreach (var testCase in testCases)
			{
				TestTextBoxCore(textBox =>
				{
					textBox.CharacterCasing = CharacterCasing.Normal;
					KeySender.PostKeyDown(textBox, textBox.Handle, testCase.Key);
					Application.DoEvents();
				}, testCase.ExpectedText);
			}
		});
	}

	public void TestPostKeyDownTextBox_SeparateShiftAndD1()
	{
		TestTextBoxCore(textBox =>
		{
			KeySender.PostKeyDown(textBox, textBox.Handle, Keys.Shift);
			KeySender.PostKeyDown(textBox, textBox.Handle, Keys.D1);
			Application.DoEvents();
		}, "1");
	}

	public void TestPostKeyDownTextBox_RespectsMaxLength()
	{
		TestTextBoxCore(textBox =>
		{
			textBox.MaxLength = 3;

			for (var i = 0; i < 3; ++i)
			{
				KeySender.PostKeyDown(textBox, Keys.Add);
				KeySender.PostKeyDown(textBox, Keys.D1);
				KeySender.PostKeyDown(textBox, Keys.Subtract);
			}
			Application.DoEvents();
		}, "+1-");
	}

	public void TestPostKeyDownTextBox_Selection_ShouldRemain_WhenKeyHandled()
	{
		TestTextBoxCore(textBox =>
		{
			textBox.Text = "111";
			textBox.SelectionStart = 0;
			textBox.SelectionLength = 3;

			textBox.KeyPress += (sender, e) =>
			{
				if (e.KeyChar == '2')
				{
					e.Handled = true;
				}
			};

			KeySender.PostKeyDown(textBox, Keys.D2);
			Application.DoEvents();

			AssertEquals(0, textBox.SelectionStart);
			AssertEquals(3, textBox.SelectionLength);
		}, "111");
	}

	public void TestPostKeyDownTextBox_TextOverride_ShouldResetSelectionToEnd()
	{
		TestTextBoxCore(textBox =>
		{
			textBox.Text = "111";
			textBox.CharacterCasing = CharacterCasing.Normal;
			textBox.SelectionStart = 3;
			textBox.SelectionLength = 0;

			textBox.KeyPress += (sender, e) =>
			{
				if (e.KeyChar == '2')
				{
					textBox.Text = "override";
					e.Handled = true;
				}
			};

			KeySender.PostKeyDown(textBox, Keys.D2);
			Application.DoEvents();

			AssertEquals(0, textBox.SelectionStart);
			AssertEquals(0, textBox.SelectionLength);
		}, "override");
	}

	#region Implementation

	void TestTextBoxCore(Action<ZTextBox> keyAction, string expectedText)
	{
		using var form = new ZForm();
		var textBox = new ZTextBox();
		form.Controls.Add(textBox);
		form.Show();

		keyAction(textBox);

		AssertEquals($"Text should be updated to '{expectedText}'", expectedText, textBox.Text);
	}

	#endregion
}
