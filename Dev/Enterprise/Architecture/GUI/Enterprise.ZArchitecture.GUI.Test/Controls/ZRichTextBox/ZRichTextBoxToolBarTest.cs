using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.PlugIn.Testing;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.RichEdit.Testing
{
	class ZRichTextBoxToolBarTest : TestCaseWithFactory
	{
#if !WINZOR
		public void TestPreventToolBarImageListLoading()
		{
			try
			{
				ZRichTextBoxToolBar.PreventToolBarImageListLoading = true;
				using (var richToolbar = new TestZRichTextBoxToolBarForTest())
				{
					AssertEquals("Bo", richToolbar.BoldButton.Text);
					AssertEquals("It", richToolbar.ItalicButton.Text);
					AssertEquals("St", richToolbar.StrikeoutButton.Text);
					AssertEquals("Bu", richToolbar.BulletsButton.Text);
					AssertEquals("Nu", richToolbar.NumberedListButton.Text);
					AssertEquals("At", richToolbar.AttachButton.Text);
					AssertEquals("In", richToolbar.InsertImageButton.Text);
					AssertEquals("Fo", richToolbar.FormatPainterButton.Text);
				}
			}
			finally
			{
				ZRichTextBoxToolBar.PreventToolBarImageListLoading = false;
			}
		}

		public void TestToolbarHiddenButtonsAreActuallyHidden()
		{
			using (var richToolbar = new ZRichTextBoxToolBar())
			{
				var toolbar = richToolbar.ToolBar;
				var overlapControls = richToolbar.Controls.Cast<Control>().Except(toolbar).Where(c => c != toolbar.Parent && DoControlOverlap(c, toolbar));

				CombineAssertions("If you're using buttons to provide space to other controls on top, please disable the buttons to stop any hover events.", () =>
				{
					foreach (var child in overlapControls)
					{
						var overlappingButtons = toolbar.Buttons.Cast<ToolBarButton>().Where(b => b.Enabled && DoControlOverlap(child, toolbar, b.Rectangle)).ToList();

						Assert(child.Name + " overlaps with " + string.Join(", ", overlappingButtons.Select(b => b.Name)), !overlappingButtons.Any());
					}
				});
			}
		}

		static bool DoControlOverlap(Control a, Control b, Rectangle bBounds = default)
		{
			//Check if controls overlap in screen coordinates (works with controls on different parents)

			var aShift = a.Parent.PointToScreen(a.Location);
			var aBounds = new Rectangle(aShift.X, aShift.Y, a.Width, a.Height);

			if (bBounds.Width == 0)
			{
				bBounds = b.Bounds;
			}
			var bShift = b.Parent.PointToScreen(bBounds.Location);
			bBounds = new Rectangle(bShift.X, bShift.Y, bBounds.Width, bBounds.Height);

			return aBounds.IntersectsWith(bBounds);
		}

		public void TestToolBarButtons()
		{
			Form.Controls.Add(RichTextBox);
			Form.Controls.Add(Toolbar);
			Toolbar.RichTextBox = RichTextBox;

			Form.Show();
			Application.DoEvents();
			RichTextBox.Focus();

			Assert("FontFamilyComboBox", Toolbar.FontFamilyComboBox.Items.Count > 5);
			Assert("FontSizeComboBox", Toolbar.FontFamilyComboBox.Items.Count > 5);

			RichTextBox.RichEdit.Text = "xxx";

			AssertEquals("Bold", false, Toolbar.BoldButton.Pushed);
			AssertEquals("Bold", false, RichTextBox.Font.Bold);
			AssertEquals("Italic", false, Toolbar.ItalicButton.Pushed);
			AssertEquals("Italic", false, RichTextBox.Font.Italic);
			AssertEquals("Underline", false, Toolbar.UnderlineButton.Pushed);
			AssertEquals("Underline", false, RichTextBox.Font.Underline);
			AssertEquals("Strikeout", false, Toolbar.StrikeoutButton.Pushed);
			AssertEquals("Strikeout", false, RichTextBox.Font.Strikeout);
			AssertEquals("Bullets", false, Toolbar.BulletsButton.Pushed);
			AssertEquals("Bullets", false, RichTextBox.SelectionBullet);
			AssertEquals("NumberedList", false, Toolbar.NumberedListButton.Pushed);
			AssertEquals("NumberedList", false, RichTextBox.SelectionNumberedList);

			Toolbar.PerformToolbarClick(Toolbar.BoldButton);
			AssertEquals("Bold", true, Toolbar.BoldButton.Pushed);
			AssertEquals("Bold", true, RichTextBox.ActiveRichTextFont.Bold);

			Toolbar.PerformToolbarClick(Toolbar.ItalicButton);
			AssertEquals("Italic", true, Toolbar.ItalicButton.Pushed);
			AssertEquals("Italic", true, RichTextBox.ActiveRichTextFont.Italic);

			Toolbar.PerformToolbarClick(Toolbar.UnderlineButton);
			AssertEquals("Underline", true, Toolbar.UnderlineButton.Pushed);
			AssertEquals("Underline", true, RichTextBox.ActiveRichTextFont.Underline);

			Toolbar.PerformToolbarClick(Toolbar.StrikeoutButton);
			AssertEquals("Strikeout", true, Toolbar.StrikeoutButton.Pushed);
			AssertEquals("Strikeout", true, RichTextBox.ActiveRichTextFont.Strikeout);

			Toolbar.PerformToolbarClick(Toolbar.BulletsButton);
			AssertEquals("Bullets", true, Toolbar.BulletsButton.Pushed);
			AssertEquals("Bullets", true, RichTextBox.SelectionBullet);

			Toolbar.PerformToolbarClick(Toolbar.NumberedListButton);
			AssertEquals("NumberedList", true, Toolbar.NumberedListButton.Pushed);
			AssertEquals("NumberedList", true, RichTextBox.SelectionNumberedList);

			RichTextBox.SelectionStart = 1;
			RichTextBox.SelectionLength = 1;
			Application.DoEvents();

			AssertEquals("Bold", false, Toolbar.BoldButton.Pushed);
			AssertEquals("Bold", false, RichTextBox.ActiveRichTextFont.Bold);
			AssertEquals("Italic", false, Toolbar.ItalicButton.Pushed);
			AssertEquals("Italic", false, RichTextBox.ActiveRichTextFont.Italic);
			AssertEquals("Underline", false, Toolbar.UnderlineButton.Pushed);
			AssertEquals("Underline", false, RichTextBox.ActiveRichTextFont.Underline);
			AssertEquals("StrikeoutButton", false, Toolbar.StrikeoutButton.Pushed);
			AssertEquals("StrikeoutButton", false, RichTextBox.ActiveRichTextFont.Strikeout);
			AssertEquals("Bullets", false, Toolbar.BulletsButton.Pushed);
			AssertEquals("Bullets", false, RichTextBox.SelectionBullet);
			AssertEquals("NumberedList", true, Toolbar.NumberedListButton.Pushed);
			AssertEquals("NumberedList", true, RichTextBox.SelectionNumberedList);
		}

		public void TestSelectingTextDoesntUpdateFont()
		{
			const string rtf = @"{\rtf1\ansi\ansicpg1252\deff0\deflang3081{\fonttbl{\f0\fswiss\fcharset0 Arial;}}
{\*\generator Msftedit 5.41.21.2508;}\viewkind4\uc1\pard\f0\fs20 Normal text\par
\b Bold text\b0\par
\par
}";
			Form.Controls.Add(RichTextBox);
			Form.Controls.Add(Toolbar);
			Toolbar.RichTextBox = RichTextBox;
			Form.Show();
			Application.DoEvents();

			RichTextBox.Rtf = rtf;
			RichTextBox.SelectAll();
			AssertEquals("Rtf shouldn't change after selecting text", true, RichTextBox.Rtf.Contains(@"\b Bold text\b"));
		}

		public void TestApplyStyleOnSelection_EndOfTextBug()
		{
			Form.Controls.Add(RichTextBox);
			Form.Controls.Add(Toolbar);
			Toolbar.RichTextBox = RichTextBox;
			RichTextBox.Rtf = "{\\rtf1\\ansi\\ansicpg1252\\deff0\\nouicompat\\deflang3081{\\fonttbl{\\f0\\fnil Microsoft Sans Serif;}{\\f1\\fnil\\fcharset0 Microsoft Sans Serif;}}\r\n{\\colortbl ;\\red0\\green0\\blue255;}\r\n{\\*\\generator Riched20 10.0.22621}\\viewkind4\\uc1 \r\n\\pard {\\f0\\fs24{\\field{\\*\\fldinst{HYPERLINK \"edient:Command=ShowEditForm&ControllerID=WorkItem&BusinessEntityPK=3ae43063-b9e5-4fa6-9fd7-b28b48a9cd3f&VersionNumber=25.4.4.58&Domain=wtg.zone&Instance=ediProd&ServerName=ediprod.db.wtg.zone&DatabaseName=EDIPROD&Hash=%2bJ4KWFNdMYU%2fPfnoLUqswIxoRGfhje%2fKA\"}}{\\fldrslt{\\ul\\cf1\\cf1\\ul WI00893926 - CS01921263 - WISGLOSYD - Bug when formatting a selection that ends with newline}}}}\\f0\\fs20\\par\r\n\\f1\\lang3081\\par\r\naafghfg\\par\r\nbfghgfh fghgfhfg dfgdf h\\par\r\nc\\f0\\lang1033\\par\r\n}\r\n";

			var length = RichTextBox.RichEdit.TextLength;
			RichTextBox.RichEdit.Select(371, 25);
			AssertEquals("bfghgfh fghgfhfg dfgdf h\n", RichTextBox.RichEdit.SelectedText);
			Toolbar.PerformToolbarClick(Toolbar.BoldButton);
			AssertEquals(length, RichTextBox.RichEdit.TextLength);
		}

		public void TestApplyStyleOnSelectionWithDifferentStylesApplied()
		{
			Form.Controls.Add(RichTextBox);
			Form.Controls.Add(Toolbar);
			Toolbar.RichTextBox = RichTextBox;
			const string text = "Test text";
			RichTextBox.Rtf = text;

			RichTextBox.RichEdit.Select(0, 4);
			Toolbar.PerformToolbarClick(Toolbar.BoldButton);
			Assert("Text 'Test' should be bold now", RichTextBox.ActiveRichTextFont.Bold);

			RichTextBox.RichEdit.Select(0, text.Length);
			Toolbar.PerformToolbarClick(Toolbar.ItalicButton);
			Assert("Entire text should be Italic now", RichTextBox.ActiveRichTextFont.Italic);

			RichTextBox.RichEdit.Select(0, text.Length);
			Toolbar.PerformToolbarClick(Toolbar.UnderlineButton);
			Assert("Entire text should be Underline now", RichTextBox.ActiveRichTextFont.Underline);
			Assert("Italic should still be active", RichTextBox.ActiveRichTextFont.Italic);

			Assert("Italic button should be pushed", Toolbar.ItalicButton.Pushed);
			Assert("Underlined button should be pushed", Toolbar.UnderlineButton.Pushed);
			Assert("Bold button shouldn't be pushed", !Toolbar.BoldButton.Pushed);

			RichTextBox.RichEdit.Select(0, 4);
			Assert("Italic button should be pushed", Toolbar.ItalicButton.Pushed);
			Assert("Underlined button should be pushed", Toolbar.UnderlineButton.Pushed);
			Assert("Bold button should be pushed", Toolbar.BoldButton.Pushed);

			RichTextBox.RichEdit.Select(0, text.Length);
			Toolbar.PerformToolbarClick(Toolbar.StrikeoutButton);
			Assert("Entire text should be striked out now", RichTextBox.ActiveRichTextFont.Strikeout);
		}

		public void TestFontSizeComboBoxSelectedIndexChangedDoesntUpdateFontStyle()
		{
			Form.Controls.Add(RichTextBox);
			Form.Controls.Add(Toolbar);
			Toolbar.RichTextBox = RichTextBox;
			Form.Show();
			Application.DoEvents();

			Toolbar.FontSizeComboBox.SelectedIndex = 0;

			const string text = "textBoldAndItalic";
			RichTextBox.Rtf = text;

			RichTextBox.RichEdit.Select(4, 4);
			Toolbar.PerformToolbarClick(Toolbar.BoldButton);
			Assert("Text 'Bold' should be bold now", RichTextBox.ActiveRichTextFont.Bold);

			RichTextBox.RichEdit.Select(11, text.Length - 11);
			Toolbar.PerformToolbarClick(Toolbar.ItalicButton);
			Assert("Text 'Italic' should be Italic now", RichTextBox.ActiveRichTextFont.Italic);

			RichTextBox.SelectAll();
			Assert("Bold shouldn't be active", !RichTextBox.ActiveRichTextFont.Bold);
			Assert("Italic shouldn't be active", !RichTextBox.ActiveRichTextFont.Italic);
			AssertEquals("Entire text should be regulr now", FontStyle.Regular, RichTextBox.ActiveRichTextFont.Style);

			Toolbar.FontSizeComboBox.SelectedIndex = 1;

			RichTextBox.RichEdit.Select(4, 4);
			Assert("Text 'Bold' should still be bold", RichTextBox.ActiveRichTextFont.Bold);

			RichTextBox.RichEdit.Select(11, text.Length - 11);
			Assert("Text 'Italic' should still be Italic", RichTextBox.ActiveRichTextFont.Italic);
		}

		public void TestRemoveStylesFromRtf()
		{
			Form.Controls.Add(RichTextBox);
			Form.Controls.Add(Toolbar);
			Toolbar.RichTextBox = RichTextBox;
			const string text = @"Bitch where you when I was walkin'?
Now I run the game, got the whole world talkin', King Kunta
Everybody wanna cut the legs off him";
			RichTextBox.Rtf = text;

			RichTextBox.RichEdit.Select(0, text.Length);
			Toolbar.PerformToolbarClick(Toolbar.BoldButton);
			Assert("Entire text should be bold now", RichTextBox.ActiveRichTextFont.Bold);

			var quarterText = Math.Abs(text.Length / 4);
			RichTextBox.RichEdit.Select(quarterText, quarterText);
			Assert("Selected text should be bold now", RichTextBox.ActiveRichTextFont.Bold);
			Toolbar.PerformToolbarClick(Toolbar.BoldButton);
			Assert("Selected text shouldn't be bold", !RichTextBox.ActiveRichTextFont.Bold);
			Assert("Selected text shouldn't be bold", !RichTextBox.RichEdit.SelectedRtf.Contains("\\b"));

			RichTextBox.RichEdit.Select(quarterText * 3, quarterText);
			Assert("Selected text should be bold now", RichTextBox.ActiveRichTextFont.Bold);
			Toolbar.PerformToolbarClick(Toolbar.BoldButton);
			Assert("Selected text shouldn't be bold", !RichTextBox.ActiveRichTextFont.Bold);
			Assert("Selected text shouldn't be bold", !RichTextBox.RichEdit.SelectedRtf.Contains("\\b"));

			RichTextBox.RichEdit.Select(0, text.Length);
			Assert("Entire text shouldn't be bold", !RichTextBox.ActiveRichTextFont.Bold);
			AssertEquals("Selected text shouldn't be bold - Start Tags (with end tags)", 4, RichTextBox.RichEdit.SelectedRtf.CountMatches("\\b"));
			AssertEquals("Selected text shouldn't be bold - End tags", 2, RichTextBox.RichEdit.SelectedRtf.CountMatches("\\b0"));
			Assert("Selected text shouldn't be bold", RichTextBox.RichEdit.SelectedRtf.Contains("\\pard\\b"));
		}

		[ExpectNoExceptions]
		public void TestUnsupportedFont()
		{
			Form.Controls.Add(Toolbar);
			Form.Show();
			Application.DoEvents();
			Toolbar.CreateNewFont("Monotype Corsiva", 12, FontStyle.Regular);
		}

		[ExpectException(typeof(ArgumentOutOfRangeException))]
		public void TestUnsupportedSizeZero()
		{
			Form.Controls.Add(Toolbar);
			Form.Show();
			Application.DoEvents();
			Toolbar.CreateNewFont(FontFamily.GenericSansSerif, 0, FontStyle.Regular);
		}

		[ExpectException(typeof(ArgumentOutOfRangeException))]
		public void TestUnsupportedSizeNegative()
		{
			Form.Controls.Add(Toolbar);
			Form.Show();
			Application.DoEvents();
			Toolbar.CreateNewFont(FontFamily.GenericSansSerif, -0.01f, FontStyle.Regular);
		}

		public void TestPopupButtonFont()
		{
			AssertEquals("Popup button font should not be big", OFont.GetFont(), Toolbar.PopupButton.Font);
		}

		public void TestComboBoxesPopulated()
		{
			Form.Controls.Add(Toolbar);
			Form.Show();
			Application.DoEvents();

			Assert(Toolbar.FontFamilyComboBox.Items.Count > 0);
			Assert(Toolbar.FontSizeComboBox.Items.Count > 0);
		}

		public void TestComboBoxesAreNotSelectedWhenInitiallyShown()
		{
			Form.Controls.Add(RichTextBox);
			Form.Controls.Add(Toolbar);
			Toolbar.RichTextBox = RichTextBox;
			Form.Show();
			Application.DoEvents();

			AssertEquals("ComboBox text should not be selected initially", 0, Toolbar.FontFamilyComboBox.SelectionLength);
			AssertEquals("ComboBox text should not be selected initially", 0, Toolbar.FontSizeComboBox.SelectionLength);
		}

		public void TestComboBoxesAreBoundInitially()
		{
			using (var zForm = new ZForm())
			using (var zRichTextBox = new ZRichTextBox())
			using (var dummyTestZRichTextBoxToolBar = new DummyTestZRichTextBoxToolBar())
			{
				zForm.Controls.Add(zRichTextBox);
				dummyTestZRichTextBoxToolBar.RichTextBox = zRichTextBox;

				zForm.Controls.Add(dummyTestZRichTextBoxToolBar);
				zForm.Show();
				Application.DoEvents();

				AssertEquals("FontFamilyComboBox text should not be selected initially", true, Toolbar.FontFamilyComboBox.Text.Length > 0);
				AssertEquals("FontSizeComboBox text should not be selected initially", true, Toolbar.FontSizeComboBox.Text.Length > 0);
			}
		}

		public void TestComboBoxesAreScaledProperly()
		{
			using (ControlDpiScalingHelper.OverrideDPI_ForTesting(ControlDpiScalingHelper.BaseDpiX, ControlDpiScalingHelper.BaseDpiY))
			{
				Form.Controls.Add(RichTextBox);
				Toolbar.RichTextBox = RichTextBox;
				Form.Controls.Add(Toolbar);
				Form.Show();
				Application.DoEvents();
			}

			using (ControlDpiScalingHelper.OverrideDPI_ForTesting(ControlDpiScalingHelper.BaseDpiX * 150 / 100, ControlDpiScalingHelper.BaseDpiY * 150 / 100))
			using (var newForm = new ZForm())
			using (var newRichTextBox = new ZRichTextBox())
			using (var newToolbar = new TestZRichTextBoxToolBarForTest())
			{
				newForm.Controls.Add(newRichTextBox);
				newToolbar.RichTextBox = newRichTextBox;
				newForm.Controls.Add(newToolbar);
				newForm.Show();
				Application.DoEvents();

				AssertEquals(Toolbar.FontFamilyComboBox.Width * 150 / 100, newToolbar.FontFamilyComboBox.Width);
				AssertEquals(Toolbar.FontSizeComboBox.Width * 150 / 100, newToolbar.FontSizeComboBox.Width);
			}
		}

		public void TestNameIsRetainedWhenInsertingImage()
		{
			var org = Factory.LoadTop1<IOrgHeader>(new ZQuery());

			using (var tempFile = TempFile.NewWithExtension(".tif"))
			using (var form = new ZPlugInsTest.TestFormWithTabControl(org))
			{
				File.WriteAllBytes(tempFile.Filename, new byte[] { 1, 2, 3 });
				form.PlugIns.Add(Modules.ControllerIDs.eDocsPlugIn);
				form.Controls.Add(RichTextBox);
				Toolbar.RichTextBox = RichTextBox;
				form.Controls.Add(Toolbar);
				form.Show();
				Application.DoEvents();

				Toolbar.FileToImport = tempFile.ToString();
				Toolbar.QueryImageFileFromUserDialogResult = DialogResult.OK;

				Toolbar.PerformToolbarClick(Toolbar.InsertImageButton);

				Assert("Rtf contents should change - note should be inserted to say filename was added to edocs", RichTextBox.RichEdit.Text.Contains(Path.GetFileNameWithoutExtension(tempFile.ToString())));
			}
		}

		public void TestClickFormatPainterButtonTwice()
		{
			Form.Controls.Add(RichTextBox);
			Form.Controls.Add(Toolbar);
			Toolbar.RichTextBox = RichTextBox;
			Form.Show();
			RichTextBox.Focus();
			RichTextBox.RichEdit.Text = "here sum words";
			RichTextBox.RichEdit.Select(0, 4);
			Toolbar.PerformToolbarClick(Toolbar.FormatPainterButton);
			Toolbar.PerformToolbarClick(Toolbar.FormatPainterButton);
			AssertEquals("Format painter button should not be pushed", false, Toolbar.FormatPainterButton.Pushed);
		}

		public void TestFormatPainterWhenSelectedTextIsInconsistent()
		{
			Form.Controls.Add(RichTextBox);
			Form.Controls.Add(Toolbar);
			Toolbar.RichTextBox = RichTextBox;
			Form.Show();
			RichTextBox.Focus();
			RichTextBox.Text = "here sum words";
			RichTextBox.RichEdit.Text = "here sum words";
			RichTextBox.RichEdit.Select(0, 4);
			RichTextBox.RichEdit.SelectionFont = OFont.GetFontBold();

			RichTextBox.RichEdit.Select(RichTextBox.Text.IndexOf("sum"), 3);
			RichTextBox.RichEdit.Select(RichTextBox.Text.IndexOf("sum", StringComparison.Ordinal), 3);
			AssertNotEquals("Testing different font families", RichTextBox.RichEdit.SelectionFont.FontFamily, OFont.GetFontBold().FontFamily);

			RichTextBox.RichEdit.Select(0, 7);
			Toolbar.PerformToolbarClick(Toolbar.FormatPainterButton);

			AssertEquals("First word should be the same as third", OFont.GetFontBold().FontFamily, Toolbar.SavedFormatProperties?.Font.FontFamily);
		}

		public void TestFormatPainterWhenNothingSelected()
		{
			Form.Controls.Add(RichTextBox);
			Form.Controls.Add(Toolbar);
			Toolbar.RichTextBox = RichTextBox;
			Form.Show();
			RichTextBox.Focus();

			RichTextBox.RichEdit.SelectionLength = 0;
			Toolbar.PerformToolbarClick(Toolbar.FormatPainterButton);
			AssertEquals("The format painter is not pushed", false, Toolbar.SavedFormatProperties.HasValue);
		}

		public void TestFormatIsConvertedAfterFormatPainterButton()
		{
			Form.Controls.Add(RichTextBox);
			Form.Controls.Add(Toolbar);
			Toolbar.RichTextBox = RichTextBox;
			Form.Show();
			RichTextBox.Focus();
			RichTextBox.RichEdit.Text = "xxx yyy";

			RichTextBox.RichEdit.Select(0, 3);
			Toolbar.PerformToolbarClick(Toolbar.BoldButton);
			RichTextBox.RichEdit.Select(0, 3);
			Toolbar.PerformToolbarClick(Toolbar.FormatPainterButton);
			RichTextBox.RichEdit.Select(4, 3);
			RichTextBox.RichEdit.SelectionLength = 0;
			Toolbar.ApplyStyleAndExitMode(RichTextBox.RichEdit, EventArgs.Empty);
			Assert("Text 'Test' should be bold now", RichTextBox.ActiveRichTextFont.Bold);
		}

		public void TestToggleAttachButton()
		{
			using (var zForm = new ZForm())
			using (var zRichTextBox = new ZRichTextBox())
			{
				zForm.Controls.Add(zRichTextBox);
				zForm.Show();
				Application.DoEvents();

				Assert(zRichTextBox.RichTextToolBar.AttachButton.Visible);

				zRichTextBox.RichTextToolBar.ToggleAttachButton(false);
				Assert(!zRichTextBox.RichTextToolBar.AttachButton.Visible);

				zRichTextBox.RichTextToolBar.ToggleAttachButton(true);
				Assert(zRichTextBox.RichTextToolBar.AttachButton.Visible);
			}
		}

		public void TestToggleInsertImageButton()
		{
			using (var zForm = new ZForm())
			using (var zRichTextBox = new ZRichTextBox())
			{
				zForm.Controls.Add(zRichTextBox);
				zForm.Show();
				Application.DoEvents();

				Assert(zRichTextBox.RichTextToolBar.InsertImageButton.Visible);

				zRichTextBox.RichTextToolBar.ToggleInsertImageButton(false);
				Assert(!zRichTextBox.RichTextToolBar.InsertImageButton.Visible);

				zRichTextBox.RichTextToolBar.ToggleInsertImageButton(true);
				Assert(zRichTextBox.RichTextToolBar.InsertImageButton.Visible);
			}
		}

		#region Test Classes

		class TestZRichTextBoxToolBarForTest : ZRichTextBoxToolBar
		{
			public new SavedFormatPropertiesStruct? SavedFormatProperties => base.SavedFormatProperties;

			public new ToolBarButton BoldButton
			{
				get { return base.BoldButton; }
			}

			public new ToolBarButton ItalicButton
			{
				get { return base.ItalicButton; }
			}

			public new ToolBarButton UnderlineButton
			{
				get { return base.UnderlineButton; }
			}

			public new ToolBarButton StrikeoutButton
			{
				get { return base.StrikeoutButton; }
			}

			public new ToolBarButton BulletsButton
			{
				get { return base.BulletsButton; }
			}

			public new ToolBarButton InsertImageButton
			{
				get { return base.InsertImageButton; }
			}

			public new ToolBarButton NumberedListButton
			{
				get { return base.NumberedListButton; }
			}

			public new ToolBarButton FormatPainterButton
			{
				get { return base.FormatPainterButton; }
			}

			public new ComboBox FontFamilyComboBox
			{
				get { return base.FontFamilyComboBox; }
			}

			public new ComboBox FontSizeComboBox
			{
				get { return base.FontSizeComboBox; }
			}

			public void PerformToolbarClick(ToolBarButton button)
			{
				button.Pushed = !button.Pushed;
				base.ToolBar_ButtonClick(this, new ToolBarButtonClickEventArgs(button));
			}

			public new Font CreateNewFont(FontFamily family, float size, FontStyle style)
			{
				return base.CreateNewFont(family, size, style);
			}

			public new Font CreateNewFont(string family, float size, FontStyle style)
			{
				return base.CreateNewFont(family, size, style);
			}

			protected override DialogResult QueryImageFileFromUser(out string imageFile)
			{
				imageFile = FileToImport;
				return QueryImageFileFromUserDialogResult;
			}
			public DialogResult QueryImageFileFromUserDialogResult;
			public string FileToImport;
		}

		class DummyTestZRichTextBoxToolBar : ZRichTextBoxToolBar
		{
			internal override FontFamily[] GetFontFamilies()
			{
				return new[] { new FontFamily("Arial") };
			}
		}

		#endregion

		#region Implementation

		ZForm Form => form ?? (form = new ZForm());
		ZForm form;

		ZRichTextBox RichTextBox => richTextBox ?? (richTextBox = new ZRichTextBox());
		ZRichTextBox richTextBox;

		TestZRichTextBoxToolBarForTest Toolbar => toolbar ?? (toolbar = new TestZRichTextBoxToolBarForTest());
		TestZRichTextBoxToolBarForTest toolbar;

		protected override void TearDown()
		{
			base.TearDown();

			form?.Dispose();
			richTextBox?.Dispose();
			toolbar?.Dispose();
		}

		#endregion
#endif
	}
}
