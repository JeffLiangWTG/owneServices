using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.Common.Testing;
using CargoWise.Interop.DataObjects;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

#pragma warning disable WTG1001 // Do not use the 'private' keyword.

namespace Enterprise.ZArchitecture.GUI
{
	[ToolboxItem(false)]
	public partial class ZRichTextBoxToolBar : ZUserControl // This is an architecture control
	{
		public ZRichTextBoxToolBar()
		{
			InitializeComponent();
			InitializeToolBarImageList();
			SetupCaptions();
			DisposableLeakListener.Instance.RegisterDisposable(this);
			PopupButton.Font = OFont.GetFont();
		}

#if DEBUG
		[ThreadStatic]
		internal static bool PreventToolBarImageListLoading;
#endif

		void InitializeToolBarImageList()
		{
			// 
			// ToolBarImageList
			//
			for (var tries = 0; tries < 3; ++tries)
			{
				try
				{
#if DEBUG
					if (PreventToolBarImageListLoading)
					{
						throw new InvalidOperationException("Loading of the ImageList did not succeed.");
					}
#endif

					var resources = new ComponentResourceManager(typeof(ZRichTextBoxToolBar));
					ZImageListDpiScalingHelper.SetScaledImagesFromImageListStreamer(this.ToolBarImageList, (ImageListStreamer)resources.GetObject("ToolBarImageList.ImageStream"));
					this.ToolBarImageList.TransparentColor = System.Drawing.Color.Transparent;
					this.ToolBarImageList.Images.SetKeyName(0, "");
					this.ToolBarImageList.Images.SetKeyName(1, "");
					this.ToolBarImageList.Images.SetKeyName(2, "");
					this.ToolBarImageList.Images.SetKeyName(3, "");
					this.ToolBarImageList.Images.SetKeyName(4, "");
					this.ToolBarImageList.Images.SetKeyName(5, "");
					this.ToolBarImageList.Images.SetKeyName(6, "");
					this.ToolBarImageList.Images.SetKeyName(7, "");
					this.ToolBarImageList.Images.SetKeyName(8, "");
					this.ToolBarImageList.Images.SetKeyName(9, "");
					this.ToolBarImageList.Images.SetKeyName(10, "");
					this.ToolBarImageList.Images.SetKeyName(11, "");
					this.ToolBarImageList.Images.SetKeyName(12, "");
					this.ToolBarImageList.Images.SetKeyName(13, "");
					this.ToolBarImageList.Images.SetKeyName(14, "");
					this.ToolBarImageList.Images.SetKeyName(15, "LegitWorksThisTimeIcon.png");

					for (var i = 0; i < this.ToolBarImageList.Images.Count; i++)
					{
						this.ToolBarImageList.Images.SetKeyName(i, "");
					}

					return;
				}
				catch (InvalidOperationException)
				{
					//Loading of the ImageList did not succeed.
				}
			}
		}

		void SetupCaptions()
		{
			this.BoldButton.ToolTipText = Res.GetString("a74704a1-640a-4bff-9b72-33cb2a818fb3", "Bold");
			this.ItalicButton.ToolTipText = Res.GetString("7eec38bf-3007-4641-93b6-ea55ae44abcb", "Italic");
			this.UnderlineButton.ToolTipText = Res.GetString("c9943bfb-4acd-40d8-8308-4f8d815fb5a2", "Underline");
			this.StrikeoutButton.ToolTipText = Res.GetString("fa5dc0be-d36c-4ea4-b675-811d6e82e4dd", "Strikeout");
			this.BulletsButton.ToolTipText = Res.GetString("8b105a8d-321c-4272-84f4-ede57a9a4391", "Bullets");
			this.NumberedListButton.ToolTipText = Res.GetString("96ec8bd6-32b7-4356-862c-0f050f329e89", "Numbering");
			this.AttachButton.ToolTipText = Res.GetString("0395ea4e-b182-4a8d-9d06-156970d71286", "Attach a file");
			this.InsertImageButton.ToolTipText = Res.GetString("2349db67-259a-4e39-a950-2c0d40236798", "Insert Image");
			this.FormatPainterButton.ToolTipText = Res.GetString("64F83DA0-8901-4635-954B-1F0A986E38E9", "Format Painter - Copy Selected Formatting");

			//backup plan when loading images fails
			if (this.ToolBarImageList.ImageStream == null)
			{
				foreach (ToolBarButton button in this.ToolBar.Buttons)
				{
					button.Text = button.ToolTipText.Substring(0, Math.Min(2, button.ToolTipText.Length));
					if (string.IsNullOrEmpty(button.Text))
					{
						button.Text = button.Name.Substring(0, Math.Min(2, button.Name.Length));
					}
				}
			}
		}

#region RichTextBox

		public ZRichTextBox RichTextBox
		{
			get { return richTextBox; }
			set
			{
				if (richTextBox != null)
				{
					richTextBox.SelectionChanged -= new EventHandler(RichTextBox_SelectionChanged);
					richTextBox.TextChanged -= new EventHandler(RichTextBox_SelectionChanged);
					richTextBox.ReadOnlyChanged -= new EventHandler(RichTextBox_ReadOnlyChanged);
					richTextBox.QueryAllowControlUnfocus -= new CancelEventHandler(RichTextBox_QueryAllowControlUnfocus);
				}
				richTextBox = value;
				if (richTextBox != null)
				{
					richTextBox.SelectionChanged += new EventHandler(RichTextBox_SelectionChanged);
					richTextBox.TextChanged += new EventHandler(RichTextBox_SelectionChanged);
					richTextBox.ReadOnlyChanged += new EventHandler(RichTextBox_ReadOnlyChanged);
					richTextBox.QueryAllowControlUnfocus += new CancelEventHandler(RichTextBox_QueryAllowControlUnfocus);

					RichTextBox_ReadOnlyChanged(this, EventArgs.Empty);
					RichTextBox_SelectionChanged(this, EventArgs.Empty);

					PopupButton.Visible = RichTextBox.IsPopupButtonVisible;
				}
			}
		}

#endregion

#region PopupButtonVisible / ApplyNewFontStyle

		[Browsable(true), DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		public bool PopupButtonVisible
		{
			get { return this.PopupButton.Visible; }
			set { this.PopupButton.Visible = value; }
		}

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Rtf tag can't be translated")]
		const string boldTag = "b";
		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Rtf tag can't be translated")]
		const string italicTag = "i";
		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Rtf tag can't be translated")]
		const string strikeOutTag = "strike";
		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Rtf tag can't be translated")]
		const string underlineTag = "ul";
		static string GetTagName(FontStyle f)
		{
			switch (f)
			{
				case FontStyle.Bold:
					return boldTag;
				case FontStyle.Italic:
					return italicTag;
				case FontStyle.Underline:
					return underlineTag;
				case FontStyle.Strikeout:
					return strikeOutTag;
				default:
					throw new ArgumentException("Unknown tag: " + f);
			}
		}

		public void ApplyNewFontStyle(FontStyle changeStyle, bool value)
		{
			var selectionStart = RichTextBox.RichEdit.SelectionStart;
			var selectionLength = RichTextBox.RichEdit.SelectionLength;
			if (!RichTextBox.RichEdit.SelectedRtf.Contains("\\pard"))
			{
				RichTextBox.ActiveRichTextFont = ChangeFontStyle(RichTextBox.ActiveRichTextFont, changeStyle, value);
			}
			else
			{
				//var isEndOfEntireText = RichTextBox.RichEdit.Text.Length <= (selectionStart + selectionLength);
				var isEndOfEntireText = RichTextBox.RichEdit.Rtf.Length <= (selectionStart + selectionLength);
				AddOrRemoveFontStyleOnSelection(GetTagName(changeStyle), value, isEndOfEntireText);
			}

			RichTextBox.RichEdit.Select(selectionStart, selectionLength);
		}

		void AddOrRemoveFontStyleOnSelection(string tagName, bool isAdd, bool isEndOfEntireText)
		{
			var rtfText = Regex.Replace(richTextBox.RichEdit.SelectedRtf, FormattableString.Invariant($@"\\{tagName}0? ?\b"), "");
			if (isAdd)
			{
				rtfText = Regex.Replace(rtfText, @"\\pard\b", "\\pard\\" + tagName);
				if (isEndOfEntireText)
				{
					rtfText = RemoveEmptyParagraphs(rtfText);
				}
			}

			richTextBox.RichEdit.SelectedRtf = rtfText;
		}

		static string RemoveEmptyParagraphs(string rtf) => rtf.Replace("\\par\r\n}\r\n", "}");

		Font ChangeFontStyle(Font f, FontStyle changeStyle, bool enable) => CreateNewFont(f.FontFamily, f.Size, enable ? (f.Style | changeStyle) : (f.Style & ~changeStyle));

		protected Font CreateNewFont(FontFamily family, float size, FontStyle style)
		{
			if (size <= 0)
			{
				throw new ArgumentOutOfRangeException(nameof(size), size, "Size must be greater than 0.");
			}

			Font result;
			try
			{
				result = new Font(family, size, style);
			}
			catch (ArgumentException ex)
			{
				if (RichTextBox != null)
				{
					Globals.Message.ShowError(ex.Message, Res.GetString("1e7a4727-1a9f-4a2b-af88-261060a8721f", "Error setting font"));
				}
				result = new Font(FontFamily.GenericMonospace, size, style);
			}
			return result;
		}

		protected Font CreateNewFont(string family, float size, FontStyle style)
		{
			var newFamily = ParseFamily(family);
			return CreateNewFont(newFamily, size, style);
		}

#endregion

#region Toolbar Buttons

		protected void ToolBar_ButtonClick(object sender, ToolBarButtonClickEventArgs e)
		{
			if (!updateActiveFontSuspended)
			{
				if (e.Button == BoldButton)
				{
					ApplyNewFontStyle(FontStyle.Bold, e.Button.Pushed);
				}

				if (e.Button == ItalicButton)
				{
					ApplyNewFontStyle(FontStyle.Italic, e.Button.Pushed);
				}

				if (e.Button == UnderlineButton)
				{
					ApplyNewFontStyle(FontStyle.Underline, e.Button.Pushed);
				}

				if (e.Button == StrikeoutButton)
				{
					ApplyNewFontStyle(FontStyle.Strikeout, e.Button.Pushed);
				}

				if (e.Button == BulletsButton && (RichTextBox.SelectionBullet = e.Button.Pushed))
				{
					NumberedListButton.Pushed = false;
				}

				if (e.Button == NumberedListButton && (RichTextBox.SelectionNumberedList = e.Button.Pushed))
				{
					BulletsButton.Pushed = false;
				}

				if (e.Button == LeftIndentButton)
				{
					RichTextBox.SelectionIndent -= ControlDpiScalingHelper.ScaleToCurrentDpiX(10);
				}

				if (e.Button == RightIndentButton)
				{
					RichTextBox.SelectionIndent += ControlDpiScalingHelper.ScaleToCurrentDpiX(10);
				}
			}
			if (e.Button == AttachButton)
			{
				AttachFile();
			}

			if (e.Button == InsertImageButton)
			{
				InsertImage();
			}

			if (e.Button == ColorButton)
			{
				ChangeColor();
			}

			if (e.Button == FormatPainterButton)
			{
				FormatPainterSave();
			}
		}

		void InsertImage()
		{
			if (QueryImageFileFromUser(out var imageFile) == DialogResult.OK)
			{
				try
				{
					using (var data = ZDataObject.FromData(DataFormats.FileDrop, new string[] { imageFile }))
					{
						RichTextBox.InsertObject(data);
					}
				}
				catch (Exception f) when (!f.IsCriticalException())
				{
					Globals.Message.Show(f.Message);
				}
			}
		}

		void AttachFile()
		{
			if (QueryAttachmentFileFromUser(out var attachFile) == DialogResult.OK)
			{
				using (var data = ZDataObject.FromData(DataFormats.FileDrop, new string[] { attachFile }))
				{
					RichTextBox.InsertObject(data);
				}
			}
		}

		void ChangeColor()
		{
			var dialog = new ColorDialog();
			dialog.Color = this.RichTextBox.SelectionColor;
			dialog.ShowDialog();
			RichTextBox.SelectionColor = dialog.Color;
		}

		public void ToggleAttachButton(bool isVisible)
		{
			AttachButton.Visible = isVisible;
		}

		public void ToggleInsertImageButton(bool isVisible)
		{
			InsertImageButton.Visible = isVisible;
		}

#region Format Painter
		internal SavedFormatPropertiesStruct? SavedFormatProperties;
		internal struct SavedFormatPropertiesStruct
		{
			public SavedFormatPropertiesStruct(Font pFont, Color pColor, int pIndent, bool pBulletPoint)
			{
				Font = pFont;
				Color = pColor;
				Indent = pIndent;
				BulletPoint = pBulletPoint;
			}

			internal readonly Font Font;
			internal Color Color;
			internal int Indent;
			internal bool BulletPoint;
		}

		void EnterFormatPainterMode()
		{
			RichTextBox.RichEdit.SelectionLength = 1;

			SavedFormatProperties = new SavedFormatPropertiesStruct(
				RichTextBox.RichEdit.SelectionFont,
				RichTextBox.RichEdit.SelectionColor,
				RichTextBox.RichEdit.SelectionIndent,
				RichTextBox.RichEdit.SelectionBullet
			);

			RichTextBox.RichEdit.SelectionLength = 0;

			FormatPainterButton.Pushed = true;
			RichTextBox.RichEdit.Cursor = Cursors.Hand;
			RichTextBox.RichEdit.MouseUp += ApplyStyleAndExitMode;
		}

		void ExitFormatPainterMode()
		{
			RichTextBox.RichEdit.Cursor = Cursors.IBeam;
			RichTextBox.RichEdit.SelectionLength = 0;
			RichTextBox.RichEdit.MouseUp -= ApplyStyleAndExitMode;
			FormatPainterButton.Pushed = false;

			SavedFormatProperties = null;
		}

		internal void ApplyStyleAndExitMode(object sender, EventArgs e)
		{
			ApplySavedFormatStyle();
			ExitFormatPainterMode();
		}

		void ApplySavedFormatStyle()
		{
			if (SavedFormatProperties.HasValue)
			{
				RichTextBox.RichEdit.SelectionFont = SavedFormatProperties.Value.Font;
				RichTextBox.RichEdit.SelectionColor = SavedFormatProperties.Value.Color;
				RichTextBox.RichEdit.SelectionIndent = SavedFormatProperties.Value.Indent;
				RichTextBox.RichEdit.SelectionBullet = SavedFormatProperties.Value.BulletPoint;
			}
		}

		void FormatPainterSave()
		{
			if (RichTextBox.RichEdit.SelectionLength > 0 && !SavedFormatProperties.HasValue)
			{
				EnterFormatPainterMode();
			}
		}

#endregion

#region Querying Attachments from user

		DialogResult QueryAttachmentFileFromUser(out string file)
		{
			file = "";
			if (openFileDialog != null)
			{
				openFileDialog.Dispose();
			}
			openFileDialog = new ZOpenFileDialog();
			var result = openFileDialog.ShowDialog(this);
			if (result == DialogResult.OK)
			{
				file = openFileDialog.ForceLocalFile();
			}
			return result;
		}

		[SuppressMessage("Microsoft.Design", "CA1021")]
		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "File extensions")]
		protected virtual DialogResult QueryImageFileFromUser(out string file)
		{
			file = "";
			if (openFileDialog != null)
			{
				openFileDialog.Dispose();
			}
			openFileDialog = new ZOpenFileDialog();
			openFileDialog.Filter = Res.GetString("5fcd6f70-2836-43f9-af23-0f39776b0f79", "Image Files")
				+ " (*.gif, *.bmp, *.jpg, *.tif, *.png)|*.gif;*.bmp;*.jpg;*.tif;*.png";
			var result = openFileDialog.ShowDialog(this);
			if (result == DialogResult.OK)
			{
				file = openFileDialog.ForceLocalFile();
			}
			return result;
		}

		ZOpenFileDialog openFileDialog;

#endregion

#region RichTextBox Events

		void RichTextBox_SelectionChanged(object sender, EventArgs e)
		{
			if (!updateFontControlsSuspended)
			{
				using (SuspendUpdateActiveFont())
				{
					UpdateFontControlsFromSelection();
				}
			}
		}

		void RichTextBox_ReadOnlyChanged(object sender, EventArgs e)
		{
			Enabled = !RichTextBox.ReadOnly;
		}

		void RichTextBox_QueryAllowControlUnfocus(object sender, CancelEventArgs e)
		{
			if (FontFamilyComboBox.Focused || FontSizeComboBox.Focused)
			{
				e.Cancel = true;
			}
		}

#endregion

#region UpdateFontControlsFromSelection

		void UpdateFontControlsFromSelection()
		{
			if (RichTextBox.ActiveRichTextFont != null || RichTextBox.RichEdit.SelectedRtf.Contains("\\pard"))
			{
				BulletsButton.Pushed = RichTextBox.SelectionBullet;
				NumberedListButton.Pushed = RichTextBox.SelectionNumberedList;
				BoldButton.Pushed = IsStyleActive(FontStyle.Bold);
				ItalicButton.Pushed = IsStyleActive(FontStyle.Italic);
				UnderlineButton.Pushed = IsStyleActive(FontStyle.Underline);
				StrikeoutButton.Pushed = IsStyleActive(FontStyle.Strikeout);
				FontFamilyComboBox.Text = RichTextBox.ActiveRichTextFont.FontFamily.Name;
				FontSizeComboBox.Text = RichTextBox.ActiveRichTextFont.Size.ToString();
			}
		}

		bool IsStyleActive(FontStyle style)
			=> (RichTextBox.RichEdit.SelectionLength == 0) ?
					RichTextBox.ActiveRichTextFont.Style.HasFlag(style) :
					IsStyleActiveForSelection(style);

		bool IsStyleActiveForSelection(FontStyle style)
			=> Regex.IsMatch(RichTextBox.RichEdit.SelectedRtf, FormattableString.Invariant($@"\\pard[\\\w]*\\{GetTagName(style)}\b")) && !RichTextBox.RichEdit.SelectedRtf.Contains("\\" + GetTagName(style) + "0");

	#endregion

		#region ComboBoxes

		bool updateActiveFontSuspended;
		bool updateFontControlsSuspended;

		internal virtual FontFamily[] GetFontFamilies()
		{
			return FontFamily.Families;
		}

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Property name")]
		void SetupComboBoxes()
		{
			if (!DesignModeFinder.IsDesigning)
			{
				FontSizeComboBox.DataSource = FontSizeList;
				FontFamilyComboBox.DataSource = GetFontFamilies();
				FontFamilyComboBox.DisplayMember = "Name";
			}

			FontFamilyComboBox.SelectedIndexChanged += FontFamilyComboBox_SelectedIndexChanged;
			FontFamilyComboBox.KeyDown += FontFamilyComboBox_KeyDown;

			FontSizeComboBox.SelectedIndexChanged += FontSizeComboBox_SelectedIndexChanged;
			FontSizeComboBox.KeyDown += FontSizeComboBox_KeyDown;
		}

		void FontSizeComboBox_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Enter)
			{
				FontSizeComboBox_SelectedIndexChanged(sender, e);
			}
		}

		void FontFamilyComboBox_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Enter)
			{
				FontFamilyComboBox_SelectedIndexChanged(sender, e);
			}
		}

		void FontSizeComboBox_SelectedIndexChanged(object sender, EventArgs e)
		{
			using (SuspendUpdateFontControls())
			{
				if (!updateActiveFontSuspended && RichTextBox != null)
				{
					float newSize;
					float.TryParse(((ComboBox)sender).Text, out newSize);
					if (newSize > 0)
					{
						RichTextBox.RichEdit.SetSelectionFontSize(newSize);
					}
					RichTextBox.Focus();
				}
			}
		}

		void FontFamilyComboBox_SelectedIndexChanged(object sender, EventArgs e)
		{
			using (SuspendUpdateFontControls())
			{
				if (!updateActiveFontSuspended && RichTextBox != null)
				{
					var currentFont = RichTextBox.ActiveRichTextFont;
					var newFamily = ParseFamily(((ComboBox)sender).Text) ?? (FontFamily)((ComboBox)sender).SelectedItem;
					if (newFamily != null)
					{
						RichTextBox.ActiveRichTextFont = CreateNewFont(
								newFamily, currentFont.Size, currentFont.Style);
					}
					RichTextBox.Focus();
				}
			}
		}

		static FontFamily ParseFamily(string familyName)
		{
			foreach (var family in FontFamily.Families)
			{
				if (family.Name == familyName)
				{
					return family;
				}
			}
			return null;
		}

		IDisposable SuspendUpdateFontControls()
		{
			updateFontControlsSuspended = true;
			return new DisposableAction(delegate { updateFontControlsSuspended = false; });
		}

		IDisposable SuspendUpdateActiveFont()
		{
			updateActiveFontSuspended = true;
			return new DisposableAction(delegate { updateActiveFontSuspended = false; });
		}

		#endregion

		#region Dispose

		protected override void Dispose(bool disposing)
		{
			if (openFileDialog != null)
			{
				openFileDialog.Dispose();
			}
			if (disposing)
			{
				DisposableLeakListener.Instance.UnRegisterDisposable(this);
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}

		#endregion

		#region Implementation

		ZRichTextBox richTextBox;

		protected override void OnCreateControl()
		{
			base.OnCreateControl();

			PopupButton.AllowOverlap(PanelButtons);

			// no child control should be a tab stop
			TabStop = false;
			foreach (Control control in this.Controls)
			{
				control.TabStop = false;
			}
			SetupComboBoxes();

			if (RichTextBox != null)
			{
				PopupButton.Visible = RichTextBox.IsPopupButtonVisible;
			}
		}

		void PopupButton_Click(object sender, EventArgs e)
		{
			RichTextBox.ShowPopupEditor();
		}

		int[] FontSizeList
		{
			get
			{
				if (fontSizeList == null)
				{
					var list = new List<int>();
					for (var i = 1; i <= 12; i++)
					{
						list.Add(i);
					}
					for (var i = 14; i <= 24; i += 2)
					{
						list.Add(i);
					}
					list.Add(32);
					list.Add(48);
					list.Add(96);
					fontSizeList = list.ToArray();
				}
				return fontSizeList;
			}
		}

		int[] fontSizeList;

		protected class FontDetailsComboBox : KComboBox
		{
			protected override void OnEnabledChanged(EventArgs e)
			{
				base.OnEnabledChanged(e);
				SelectionStart = 0;
				SelectionLength = 0;
			}
		}

		#endregion
	}
}
#endregion
