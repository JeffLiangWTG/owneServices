using System;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
#if WINZOR
using CargoWise.Types;
#endif
using CargoWise.Windows.UI;
#if WINZOR
using Enterprise.ZArchitecture.Business;
#endif
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI
{
	sealed class ZAutoCompleteTextBoxTest : TestCaseWithFactory
	{
		[DeveloperOnlyTest]
		public static void TestCanOnlyPasteText()
		{
			using (var textbox = new ZAutoCompleteTextBox())
			{
				var action = new Action(() => SafeClipboard.SetData(DataFormats.Text, "Hello world"));
				action.Invoke();
				ClipboardTestHelper.RetryIfCopyOrCutFailed<object>(action, DataFormats.Text);

				Assert("Ctrl + V should be permitted when clipboard is text", !textbox.Hotkeys.ProcessCmdKey(textbox, Keys.Control | Keys.V));

				using (var bmp = new Bitmap(5, 5))
				{
					SafeClipboard.SetData(DataFormats.Bitmap, bmp);

					Assert("Ctrl + V should be blocked when clipboard is not text", textbox.Hotkeys.ProcessCmdKey(textbox, Keys.Control | Keys.V));
				}
			}
		}

		public static void ShowFruitsBox()
		{
			var form = new ZChildForm();
			form.Controls.Add(new ZAutoCompleteTextBox() { AutocompleteManager = new FruitsField(), Dock = DockStyle.Fill });

			form.Show();
		}

		public void TestBindingWorks()
		{
			using (var form = new ZChildForm())
			using (var autocomplete = new ZAutoCompleteTextBox { Dock = DockStyle.Top })
			using (var otherbox = new ZTextBox { Dock = DockStyle.Bottom, Height = 100, Multiline = true })
			{
				var dummy = Factory.NewWithValidTestData<DummyBusinessObject>();

				form.Controls.Add(autocomplete);
				form.Controls.Add(otherbox);

				autocomplete.SetDataBinding(dummy, nameof(dummy.Z0_VarBinaryMax));
				otherbox.SetDataBinding(dummy, nameof(dummy.Z0_VarCharMax));
				form.Show();

				autocomplete.Focus();
				Application.DoEvents();

				autocomplete.Text = "hello";
				Application.DoEvents();

				otherbox.Focus(); // Hopefully this should push the binding through
				Application.DoEvents();

				CombineAssertions(() =>
				{
					AssertContains("The binding manager should push datasource changes to the property", "hello", dummy.Z0_VarBinaryMax.ToUTF8());
					dummy.Z0_VarBinaryMax = Encoding.UTF8.GetBytes(@"{\rtf1\ansi world }");

					Application.DoEvents();

					AssertEquals("The binding member should pull data changes from the property", "world", autocomplete.Text.Trim());
				});
			}
		}

		public static void TestActiveColor()
		{
			using (var form = new ZForm())
			using (var textbox = new ZAutoCompleteTextBox())
			using (var anotherControl = new ZTextBox())
			{
				anotherControl.Dock = DockStyle.Top;
				textbox.Dock = DockStyle.Bottom;

				form.Controls.Add(anotherControl);
				form.Controls.Add(textbox);

				form.Show();
				Application.DoEvents();

				anotherControl.Focus();
				Application.DoEvents();

				AssertEquals("Before focus ZAutoCompleteTextBox should have its default backcolor", SystemColors.Window, textbox.BackColor);

				textbox.Focus();
				Application.DoEvents();

				AssertEquals("When focused ZAutoCompleteTextBox's backcolor should be the SelectedControlColor", EnterpriseFormLookStrategy.SelectedControlColor, textbox.BackColor);

				anotherControl.Focus();
				Application.DoEvents();

				AssertEquals("When unfocused ZAutoCompleteTextBox should revert to its default backcolor", SystemColors.Window, textbox.BackColor);
			}
		}

#if !WINZOR
		public static void TestAtTwiceDoesntReshowForm()
		{
			var fruits = new FruitsField();
			using (var form = new ZForm())
			{
				var autocomplete = new ZAutoCompleteTextBox() { AutocompleteManager = fruits };
				autocomplete.Dock = DockStyle.Fill;
				form.Controls.Add(autocomplete);
				form.Show();

				KeySender.SendKeyPress(autocomplete, '@');

				var original = autocomplete.DropForm_Exposed;
				AssertNotNull("PRE: Drop form exists", original);

				KeySender.SendKeyPress(autocomplete, '@');
				AssertNotNull("Form still open", autocomplete.DropForm_Exposed);
				Assert("Original should be no longer visible", original.IsDisposed);
			}
		}

		public void TestHittingEnterWhenThereIsNoItemThere()
		{
			var fruits = new FruitsField();
			using (var form = new ZForm())
			{
				var autocomplete = new ZAutoCompleteTextBox() { AutocompleteManager = fruits };
				form.Controls.Add(autocomplete);
				form.Show();

				var eventFired = false;
				autocomplete.ItemSelected += (o, e) => eventFired = true;

				autocomplete.Focus();
				KeyDownAndPress(autocomplete, '@');
				KeyDownAndPress(autocomplete, 'f');
				KeyDownAndPress(autocomplete, 'f');
				KeyDownAndPress(autocomplete, 'f');

				KeyDownAndPress(autocomplete, '\n');

				Application.DoEvents();

				Assert("There was nothing valid to select so the event should not fire", !eventFired);
			}
		}

		[ExpectNoExceptions]
		public void TestSelectAllWhenThereIsOnlyAtSymbol()
		{
			var fruits = new FruitsField();
			using (var form = new ZForm())
			{
				var autocomplete = new ZAutoCompleteTextBox() { AutocompleteManager = fruits };
				form.Controls.Add(autocomplete);
				form.Show();

				autocomplete.Focus();
				KeyDownAndPress(autocomplete, '@');
				KeySender.SendKeyDownToProcessCmdKey(autocomplete, (int)(Keys.Control | Keys.A));

				Application.DoEvents();
			}
		}

		public void TestSelectHighlightedOrFirstItem()
		{
			var fruits = new FruitsField();
			using (var form = new ZForm())
			{
				var autocomplete = new ZAutoCompleteTextBox() { AutocompleteManager = fruits, Dock = DockStyle.Fill };
				form.Controls.Add(autocomplete);
				form.Show();
				autocomplete.Focus();
				KeyDownAndPress(autocomplete, '@');

				AssertEquals("Pre-Condition", "@", autocomplete.Text.Trim());
				autocomplete.SelectHighlightedOrFirstItem();
				AssertEquals("Should select the first item", "@Apple", autocomplete.Text.Trim());
			}

			using (var form = new ZForm())
			{
				var autocomplete = new ZAutoCompleteTextBox() { AutocompleteManager = fruits };
				autocomplete.Dock = DockStyle.Fill;
				form.Controls.Add(autocomplete);
				form.Show();
				autocomplete.Focus();
				KeyDownAndPress(autocomplete, '@');

				var banana = (ICodeDescription)fruits.GetList("Banana")[0];
				AssertEquals("Pre-Condition", "@", autocomplete.Text.Trim());
				autocomplete.SelectHighlightedOrFirstItem(banana);
				AssertEquals("Should select the highlighted item", "@Banana", autocomplete.Text.Trim());
			}
		}

		public void TestCloseWhenFocusLost()
		{
			var fruits = new FruitsField();
			using (var form = new ZForm())
			{
				var autocomplete = new ZAutoCompleteTextBox() { AutocompleteManager = fruits };
				form.Controls.Add(autocomplete);

				var somethingElse = new ZTextBox();
				form.Controls.Add(somethingElse);
				form.Show();

				autocomplete.Focus();
				KeyDownAndPress(autocomplete, '@');
				Application.DoEvents();

				AssertNotNull("Form should be open", autocomplete.DropForm_Exposed.Visible);

				somethingElse.Focus();

				AssertNull("Form should be closed when we lose focus", autocomplete.DropForm_Exposed);
			}
		}

		public void TestArrowKeys()
		{
			var fruits = new FruitsField();
			using (var form = new ZForm())
			{
				var autocomplete = new ZAutoCompleteTextBox() { AutocompleteManager = fruits };
				autocomplete.Dock = DockStyle.Fill;
				form.Controls.Add(autocomplete);
				form.Show();

				KeyDownAndPress(autocomplete, '@');
				Assert("PRE: Drop form is shown", autocomplete.DropForm_Exposed.Visible);

				KeySender.SendKeyDown(autocomplete, autocomplete.Handle, Keys.Down);
				AssertEquals("Arrow should move selectedIndex", autocomplete.DropForm_Exposed.HighlightedItem_Exposed, 0);

				KeySender.SendKeyDown(autocomplete, autocomplete.Handle, Keys.Down);
				AssertEquals("Arrow should move selectedIndex", autocomplete.DropForm_Exposed.HighlightedItem_Exposed, 1);

				KeySender.SendKeyDown(autocomplete, autocomplete.Handle, Keys.Up);
				AssertEquals("Arrow should move selectedIndex up again", autocomplete.DropForm_Exposed.HighlightedItem_Exposed, 0);

				KeySender.SendKeyDown(autocomplete, autocomplete.Handle, Keys.Up);
				AssertEquals("Should not be able to move beyond zero", autocomplete.DropForm_Exposed.HighlightedItem_Exposed, 0);

				KeyDownAndPress(autocomplete, '\n');
				AssertNull("Should close the form", autocomplete.DropForm_Exposed);

				AssertEquals("Should have selected the text", "@Apple", autocomplete.Text);
			}
		}

		public void TestCloseWhenNavigating()
		{
			var keys = new[] { Keys.Home, Keys.End, Keys.Escape, Keys.Left, Keys.Right };
			foreach (var key in keys)
			{
				AssertFormGetsClosedWhenKeyPressed(key);
			}
		}

		void AssertFormGetsClosedWhenKeyPressed(Keys key)
		{
			var fruits = new FruitsField();
			using (var form = new ZForm())
			{
				var autocomplete = new ZAutoCompleteTextBox() { AutocompleteManager = fruits };
				autocomplete.Dock = DockStyle.Fill;
				form.Controls.Add(autocomplete);
				form.Show();

				KeyDownAndPress(autocomplete, '@');

				Assert("Form should be open", autocomplete.DropForm_Exposed.Visible);

				KeySender.SendKeyDown(autocomplete, autocomplete.Handle, key);

				AssertNull("Drop Form should be closed when we press " + key, autocomplete.DropForm_Exposed);
				AssertEquals("Should not have effected the text " + key, "@", autocomplete.Text);
			}
		}

		public void TestAutocompleteHasItems()
		{
			var fruits = new FruitsField();
			using (var form = new ZForm())
			{
				var autocomplete = new ZAutoCompleteTextBox() { AutocompleteManager = fruits };
				autocomplete.Dock = DockStyle.Fill;
				form.Controls.Add(autocomplete);

				form.Show();

				KeyDownAndPress(autocomplete, '@');
				AssertContainsExactElementsInAnyOrder(fruits.GetList(string.Empty), autocomplete.DropForm_Exposed.List_Exposed);

				KeyDownAndPress(autocomplete, 'a');
				AssertContainsExactElementsInAnyOrder(fruits.GetList("a"), autocomplete.DropForm_Exposed.List_Exposed);
			}
		}

		public void TestTwoAtSymbols()
		{
			var fruits = new FruitsField();
			using (var form = new ZForm())
			{
				var autocomplete = new ZAutoCompleteTextBox() { AutocompleteManager = fruits };
				autocomplete.Dock = DockStyle.Fill;
				form.Controls.Add(autocomplete);

				form.Show();

				KeyDownAndPress(autocomplete, '@');
				KeyDownAndPress(autocomplete, '@');
				AssertEquals("@@", autocomplete.Text);
			}
		}

		public void TestFormIsClosedOnItemSelected()
		{
			var fruits = new FruitsField();
			using (var form = new ZForm())
			{
				var autocomplete = new ZAutoCompleteTextBox() { AutocompleteManager = fruits };
				autocomplete.Dock = DockStyle.Fill;
				form.Controls.Add(autocomplete);

				form.Show();

				KeyDownAndPress(autocomplete, '@');

				KeyDownAndPress(autocomplete, '\n');

				AssertNull("Form should be closed on select", autocomplete.DropForm_Exposed);
				AssertEquals("We actually selected the value on our end", "@Apple", autocomplete.Text);
			}
		}

		public void TestBackSpace()
		{
			var fruits = new FruitsField();
			using (var form = new ZForm())
			{
				var autocomplete = new ZAutoCompleteTextBox() { AutocompleteManager = fruits };
				autocomplete.Dock = DockStyle.Fill;
				form.Controls.Add(autocomplete);

				form.Show();

				KeyDownAndPress(autocomplete, '@');
				KeyDownAndPress(autocomplete, 'a');
				KeyDownAndPress(autocomplete, 'p');

				AssertContainsExactElementsInAnyOrder(fruits.GetList("ap"), autocomplete.DropForm_Exposed.List_Exposed);

				const char backspace = (char)0x08;

				KeyDownAndPress(autocomplete, backspace);
				AssertContainsExactElementsInAnyOrder(fruits.GetList("a"), autocomplete.DropForm_Exposed.List_Exposed);

				KeyDownAndPress(autocomplete, backspace);
				AssertContainsExactElementsInAnyOrder(fruits.GetList(""), autocomplete.DropForm_Exposed.List_Exposed);

				KeyDownAndPress(autocomplete, backspace);
				AssertNull("Form should now be hidden, we backspaced the @ symbol", autocomplete.DropForm_Exposed);
			}
		}

		public void TestDoesNotAutoCompleteOnWhiteSpace()
		{
			var textToType = "I like @ap\t and @ba\n";
			var expectedOutput = "I like @Apple	 and @Banana";

			AssertAutocompletesTheRightText(textToType, expectedOutput);

			var textToType1 = "I like @ap  and @ba\n";
			var expectedOutput1 = "I like @ap  and @Banana";

			AssertAutocompletesTheRightText(textToType1, expectedOutput1);
		}

		public void TestAutocompleteDoesntDieOnItemNotInList()
		{
			var textToType = "Here is some @baaaaaad stuff";

			AssertAutocompletesTheRightText(textToType, textToType);
		}

		public void TestStartAndEnd()
		{
			var textToType = "@Ap\t and @ba\n";
			var expectedOutput = "@Apple	 and @Banana";

			AssertAutocompletesTheRightText(textToType, expectedOutput);
		}

		public void TestWeDontLoseFocusWhenPopupIsShown()
		{
			using (var form = new ZForm())
			{
				var autocomplete = new ZAutoCompleteTextBox() { AutocompleteManager = new FruitsField() };
				autocomplete.Dock = DockStyle.Fill;
				form.Controls.Add(autocomplete);

				form.Show();
				KeyDownAndPress(autocomplete, '@');

				Assert("Drop down shouldnt have focus", !autocomplete.DropForm_Exposed.ContainsFocus);
				Assert("Autocomplete should have focus", autocomplete.ContainsFocus);
			}
		}

		public void TestFormIsShownWhenAtSymbolIsPressed()
		{
			using (var form = new ZForm())
			{
				var autocomplete = new ZAutoCompleteTextBox() { AutocompleteManager = new FruitsField() };
				autocomplete.Dock = DockStyle.Fill;
				form.Controls.Add(autocomplete);

				form.Show();

				KeyDownAndPress(autocomplete, 'h');
				KeyDownAndPress(autocomplete, 'i');

				AssertNull("Form should not be visible", autocomplete.DropForm_Exposed);

				KeyDownAndPress(autocomplete, ' ');
				KeyDownAndPress(autocomplete, '@');

				Assert("Form should be visible", autocomplete.DropForm_Exposed.Visible);

				KeyDownAndPress(autocomplete, 'm');
				KeyDownAndPress(autocomplete, 'u');

				Assert("Form should still be visible", autocomplete.DropForm_Exposed.Visible);

				KeyDownAndPress(autocomplete, ' ');

				AssertNull("Form should no longer be visible", autocomplete.DropForm_Exposed);
			}
		}

		public void TestFormAutosizesProperly()
		{
			using (var form = new ZForm())
			{
				var fruits = new FruitsField();
				var autocomplete = new ZAutoCompleteTextBox() { AutocompleteManager = fruits };
				autocomplete.Dock = DockStyle.Fill;
				form.Controls.Add(autocomplete);

				form.Show();
				KeyDownAndPress(autocomplete, '@');
				var previousHeight = autocomplete.DropForm_Exposed.Height;

				KeyDownAndPress(autocomplete, 'a');

				Assert("Height should have shrunk because we have less items now", autocomplete.DropForm_Exposed.Height < previousHeight);
			}
		}

		public void TestDeletesTagProperlyAtEnd()
		{
			var textToType = "@ap\n \b\b";
			var expectedText = String.Empty;

			AssertAutocompletesTheRightText(textToType, expectedText);
		}
		public void TestDeletesTagProperlyInside()
		{
			var textToType = "@ap\n \b";
			var expectedText = " ";

			AssertAutocompletesTheRightText(textToType, expectedText, 3);
		}

		public void TestDoesNotDeleteTagJustBefore()
		{
			var textToType = "a@ap\n \b";
			var expectedText = "@Apple ";

			AssertAutocompletesTheRightText(textToType, expectedText, 1);
		}
		public void TestDeletesTagAmpersand()
		{
			var textToType = "a@ap\n \b";
			var expectedText = "a ";

			AssertAutocompletesTheRightText(textToType, expectedText, 2);
		}

		public void TestDeletesTagDeleteKeyAtStart()
		{
			var textToType = "a@ap\n \f";
			var expectedText = "a ";

			AssertAutocompletesTheRightText(textToType, expectedText, 1);
		}

		public void TestDoesNotDeleteTagDeleteKeyAtEnd()
		{
			var textToType = "a@ap\n ap\f";
			var expectedText = "a@Appleap";

			AssertAutocompletesTheRightText(textToType, expectedText, 7);
		}

		public void TestDeletesTagHighlightingInside()
		{
			var textToType = "a@ap\n \b";
			var expectedText = "a ";

			AssertAutocompletesTheRightText(textToType, expectedText, 3, 2);
		}

		public void TestDeletesTagHighlightingPartTextOnRight()
		{
			var textToType = "a@ap\n cp\b";
			var expectedText = "ap";

			AssertAutocompletesTheRightText(textToType, expectedText, 4, 5);
		}

		public void TestDeletesTagHighlightingPartTextOnLeft()
		{
			var textToType = "ac@ap\n p\b";
			var expectedText = "a p";

			AssertAutocompletesTheRightText(textToType, expectedText, 1, 4);
		}

		public void TestDeletesTagsHighlightingTwoTags()
		{
			var textToType = "ac@ap\n np@ba\n qr\b";
			var expectedText = "ac qr";

			AssertAutocompletesTheRightText(textToType, expectedText, 5, 9);
		}

		public void TestDoesNotDeleteTagsHighlightingBetweenTwoTags()
		{
			var textToType = "ac@ap\n np@ba\n qr\b";
			var expectedText = "ac@Apple@Banana qr";

			AssertAutocompletesTheRightText(textToType, expectedText, 8, 3);
		}

		void AssertAutocompletesTheRightText(string textToType, string expectedOutput, int selectionStartBeforeDelete = -1, int selectionLengthBeforeDelete = -1)
		{
			using (var form = new ZForm())
			{
				var autocomplete = new ZAutoCompleteTextBox() { AutocompleteManager = new FruitsField() };
				autocomplete.Dock = DockStyle.Fill;
				form.Controls.Add(autocomplete);

				form.Show();

				foreach (var character in textToType)
				{
					var key = GetKeyFromCharacter(character);
					if (key == Keys.Back || key == Keys.Delete)
					{
						if (selectionStartBeforeDelete != -1)
						{
							autocomplete.SelectionStart = selectionStartBeforeDelete;
							if (selectionLengthBeforeDelete != -1)
							{
								autocomplete.SelectionLength = selectionLengthBeforeDelete;
							}
						}
					}
					KeyDownAndPress(autocomplete, character);
				}
				AssertEquals(expectedOutput, autocomplete.Text);
			}
		}

		public void TestColorCorrectForTags()
		{
			using (var form = new ZForm())
			{
				var autocomplete = new ZAutoCompleteTextBox() { AutocompleteManager = new FruitsField() };
				autocomplete.Dock = DockStyle.Fill;
				form.Controls.Add(autocomplete);

				form.Show();

				GenerateText(autocomplete);

				autocomplete.SelectionStart = 0;
				autocomplete.SelectionLength = 2;
				AssertColorEquals(Color.Black, autocomplete.SelectionColor);

				autocomplete.SelectionStart = 2;
				autocomplete.SelectionLength = 6;
				AssertColorEquals(Color.Blue, autocomplete.SelectionColor);

				autocomplete.SelectionStart = 8;
				autocomplete.SelectionLength = 3;
				AssertColorEquals(Color.Black, autocomplete.SelectionColor);

				autocomplete.SelectionStart = 11;
				autocomplete.SelectionLength = 7;
				AssertColorEquals(Color.Blue, autocomplete.SelectionColor);

				autocomplete.SelectionStart = 18;
				autocomplete.SelectionLength = 3;
				AssertColorEquals(Color.Black, autocomplete.SelectionColor);
			}
		}

		[DeveloperOnlyTest]
		public void TestKeyDownChangingSelectionStart()
		{
			using (var form = new ZForm())
			{
				var autocomplete = new ZAutoCompleteTextBox() { AutocompleteManager = new FruitsField() };
				autocomplete.Dock = DockStyle.Fill;
				autocomplete.HideSelection = false;

				form.Controls.Add(autocomplete);

				form.Show();

				void SelectWordBackwards() => KSendKeys.SendWait("^+{left}", autocomplete);

				autocomplete.Text = "Here is some text so that we have something to select";
				autocomplete.SelectionStart = autocomplete.Text.IndexOf("something") - 1;
				autocomplete.SelectionLength = 0;

				SelectWordBackwards();
				AssertEquals("have", autocomplete.SelectedText);

				SelectWordBackwards();
				AssertEquals("we have", autocomplete.SelectedText);

				SelectWordBackwards();
				AssertEquals("that we have", autocomplete.SelectedText);
			}
		}

		public void TestDistinctTags()
		{
			using (var form = new ZForm())
			{
				var autocomplete = new ZAutoCompleteTextBox() { AutocompleteManager = new FruitsField() };
				autocomplete.Dock = DockStyle.Fill;
				form.Controls.Add(autocomplete);

				form.Show();

				GenerateText(autocomplete);
				GenerateAdditionalDuplicateText(autocomplete);

				var tags = autocomplete.DistinctTags;
				Assert(tags.Count(x => x.Code == "Apple") == 1);
				Assert(tags.Count(x => x.Code == "Banana") == 1);
			}
		}

		public void TestDistinctTagsAfterDelete()
		{
			using (var form = new ZForm())
			{
				var autocomplete = new ZAutoCompleteTextBox() { AutocompleteManager = new FruitsField() };
				autocomplete.Dock = DockStyle.Fill;
				form.Controls.Add(autocomplete);

				form.Show();

				GenerateText(autocomplete);
				GenerateAdditionalDuplicateText(autocomplete);

				autocomplete.SelectionStart = 14;

				KeyDownAndPress(autocomplete, (char)0x08);

				autocomplete.SelectionStart = 4;

				KeyDownAndPress(autocomplete, (char)0x08);

				var tags = autocomplete.DistinctTags;

				Assert(tags.Count(x => x.Code == "Apple") == 1);
				Assert(!tags.Any(x => x.Code == "Banana"));
			}
		}

		public void TestTags()
		{
			using (var form = new ZForm())
			{
				var autocomplete = new ZAutoCompleteTextBox() { AutocompleteManager = new FruitsField() };
				autocomplete.Dock = DockStyle.Fill;
				form.Controls.Add(autocomplete);

				form.Show();

				GenerateText(autocomplete);
				GenerateAdditionalDuplicateText(autocomplete);

				var tags = autocomplete.Tags;
				Assert(tags.Count(x => x.Code == "Apple") == 2);
				Assert(tags.Count(x => x.Code == "Banana") == 1);
			}
		}

		public void TestTagsAfterDelete()
		{
			using (var form = new ZForm())
			{
				var autocomplete = new ZAutoCompleteTextBox() { AutocompleteManager = new FruitsField() };
				autocomplete.Dock = DockStyle.Fill;
				form.Controls.Add(autocomplete);

				form.Show();

				GenerateText(autocomplete);
				GenerateAdditionalDuplicateText(autocomplete);

				autocomplete.SelectionStart = 14;

				KeyDownAndPress(autocomplete, (char)0x08);

				var tags = autocomplete.DistinctTags;

				Assert(tags.Count(x => x.Code == "Apple") == 1);
				Assert(!tags.Any(x => x.Code == "Banana"));
			}
		}

#else
		public void TestHtmlBinding()
		{
			using (var form = new ZForm())
			using (var autocomplete = new ZAutoCompleteTextBox { Dock = DockStyle.Top })
			{
				var note = Factory.New<StmNote>();
				form.Controls.Add(autocomplete);
				autocomplete.SetDataBinding(note, AutoStmNote.Schema.ST_NoteData);
				form.Show();

				AssertEquals(ZBlob.Empty, note.ST_NoteData);
				AssertEquals(ZBlob.Empty, note.ST_NoteData_HTML);

				note.ST_NoteData = ZBlob.FromUTF8(@"{\rtf1\ansi\ansicpg1252\deflang3081\nouicomp\uc0{\fonttbl{\f0\fnil Tahoma;}}{\colortbl}{{testing123 }{\f0\fs17 testing}\par}}");
				AssertEquals(@"<p>testing123 <span style=""font-family: Tahoma, sans-serif; font-size: 8.5pt;"">testing</span></p>", note.ST_NoteData_HTML.ToUTF8());
				AssertEquals(@"<p><span style=""font-family: Tahoma, sans-serif; font-size: 8pt;"">testing123 </span><span style=""font-family: Tahoma, sans-serif; font-size: 8.5pt;"">testing</span></p>", autocomplete.Html);
			}
		}
#endif

#if !WINZOR
		void GenerateText(Control autocomplete)
		{
			var characters = "ac@ap\n np@ba\n qr".ToCharArray();
			foreach (var character in characters)
			{
				KeyDownAndPress(autocomplete, character);
			}
		}

		void GenerateAdditionalDuplicateText(Control autocomplete)
		{
			var characters = " @ap\n ".ToCharArray();
			foreach (var character in characters)
			{
				KeyDownAndPress(autocomplete, character);
			}
		}

		void KeyDownAndPress(Control autocomplete, char character)
		{
			KeySender.SendKeyDown(autocomplete, autocomplete.Handle, GetKeyFromCharacter(character));
			KeySender.SendKeyPress(autocomplete, character);
		}

		Keys GetKeyFromCharacter(char character)
		{
			if (char.IsLetter(character))
			{
				return (Keys)char.ToUpper(character);
			}
			else
			{
				switch (character)
				{
					case ' ':
						return Keys.Space;
					case '@':
						return (Keys.Shift | Keys.D2);
					case '\b':
						return Keys.Back;
					case '\f':
						return Keys.Delete;
					case '\n':
						return Keys.Enter;
					case '\t':
						return Keys.Tab;
					default:
						return Keys.Escape;
				}
			}
		}
#endif
	}
}
