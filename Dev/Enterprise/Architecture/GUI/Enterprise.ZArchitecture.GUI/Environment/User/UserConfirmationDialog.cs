using System;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.Core.Forms;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Core.Environment
{
	public partial class UserConfirmationDialog : ZMessageBox
	{
		internal KTextBox ConfirmationStringTextBox;
		internal KLabel ConfirmationPromptLabel;
		internal UserConfirmationStringLabel ExpectedStringLabel;
		KCheckBox DontAskMeAgainInThisSessionCheckBox;

		public UserConfirmationDialog(string message, string caption, MessageBoxButtons buttons, MessageBoxIcon icon, MessageBoxDefaultButton defaultButton, ConfirmationMessageLayout layout = ConfirmationMessageLayout.AllInOneLine, bool showRepeatableConfirmationOption = false)
			: base(message, caption, buttons, icon, defaultButton)
		{
			InitializeForm(caption, showRepeatableConfirmationOption);
			this.layout = layout;
		}

		public UserConfirmationDialog(MultilingualString message, string caption, MessageBoxButtons buttons, MessageBoxIcon icon, MessageBoxDefaultButton defaultButton, ConfirmationMessageLayout layout = ConfirmationMessageLayout.AllInOneLine, bool showRepeatableConfirmationOption = false)
			: base(message, caption, buttons, icon, defaultButton)
		{
			InitializeForm(caption, showRepeatableConfirmationOption);
			this.layout = layout;
		}

		void InitializeForm(string caption, bool showRepeatableConfirmationOption)
		{
			InitializeComponent();
			this.ActiveControl = this.ConfirmationStringTextBox;
			AcceptButton = Button1;
			Button1.Enabled = false;
			this.DontAskMeAgainInThisSessionCheckBox.Visible = showRepeatableConfirmationOption;
			SetHeightWidthSettings();
			this.Text = caption;
		}

		public string ExpectedString
		{
			get { return ExpectedStringLabel.Text; }
			set { ExpectedStringLabel.Text = value; }
		}

		public string ConfirmationPromptLabelText
		{
			get { return ConfirmationPromptLabel.Text; }
			set { ConfirmationPromptLabel.Text = value; }
		}

		readonly ConfirmationMessageLayout layout;

		public bool UseThisAnswerForAllConfirmationsOfThisTypeInThisSession
		{
			get { return (this.DontAskMeAgainInThisSessionCheckBox.Checked); }
			set { this.DontAskMeAgainInThisSessionCheckBox.Checked = value; }
		}

		#region Implementation

		internal int padding = 4;

		void ResizeConfirmationPromptLabel()
		{
			if (string.IsNullOrEmpty(ConfirmationPromptLabelText))
			{
				ControlDpiScalingHelper.SetWidth(ref ConfirmationPromptLabel, 0, true);
				ControlDpiScalingHelper.SetHeight(ref ConfirmationPromptLabel, 0, true);
			}
			else
			{
				ConfirmationPromptLabel.Size = TextRenderer.MeasureText(TextBox.CreateGraphics(), ConfirmationPromptLabel.Text, ConfirmationPromptLabel.Font, ControlDpiScalingHelper.NewScaledSize(MaxWidth, 0, false), TextFormatFlags.NoClipping);
			}
		}

		void ResizeExpectedStringLabel()
		{
			ExpectedStringLabel.Size = TextRenderer.MeasureText(TextBox.CreateGraphics(), ExpectedStringLabel.Text, ExpectedStringLabel.Font, ControlDpiScalingHelper.NewScaledSize(MaxWidth, 0, false), TextFormatFlags.NoClipping);
		}

		void ResizeConfirmationStringTextBox()
		{
			var newSize = TextRenderer.MeasureText(TextBox.CreateGraphics(), ExpectedStringLabel.Text, ExpectedStringLabel.Font, ControlDpiScalingHelper.NewScaledSize(MaxWidth, 0, false), TextFormatFlags.NoClipping);
			if (newSize.Width > ConfirmationStringTextBox.Width)
			{
				ControlDpiScalingHelper.SetWidth(ref ConfirmationStringTextBox, newSize.Width, false);
			}
			ControlDpiScalingHelper.SetHeight(ref ConfirmationStringTextBox, newSize.Height, false);
		}

		string LongestLineInMessage
		{
			get
			{
				var longestLine = "";
				foreach (var textLine in Message.Split("\r\n".ToCharArray()))
				{
					if (longestLine.Length < textLine.Length)
					{
						longestLine = textLine;
					}
				}
				if (ConfirmationPromptLabel.Text.Length > longestLine.Length)
				{
					longestLine = ConfirmationPromptLabel.Text;
				}
				return longestLine;
			}
		}

		protected internal void UpdateHeightWidthSettings()
		{
			if (ExpectedStringLabel != null)
			{
				ResizeConfirmationPromptLabel();
				ResizeExpectedStringLabel();
				ResizeConfirmationStringTextBox();

				var offsetX = TextBox.Left;
				var offsetY = ConfirmationPromptLabel.Height + ControlDpiScalingHelper.ScaleToCurrentDpiY(padding);

				//old way
				//var newHeight = this.Height + offsetY + ControlDpiScalingHelper.ScaleToCurrentDpiY(padding);
				//new way
				var newHeight = ((IDynamicSizedDialog)this).DialogTextField.Bottom
					+ ConfirmationPromptLabel.Height
					+ Button1.Height
					+ ControlDpiScalingHelper.ScaleToCurrentDpiY(padding) * 4
					+ this.Height - this.ClientSize.Height;

				ControlDpiScalingHelper.SetHeight(this, newHeight, false);
				UpdateWidth(offsetX);
				ControlDpiScalingHelper.SetWidth(ref TextBox, Width - (offsetX * 2 + ControlDpiScalingHelper.ScaleToCurrentDpiX(padding)), false);
				SetConfirmationLayout(offsetY);

				var center = Width / 2;
				Button1.Location = ControlDpiScalingHelper.NewScaledPoint(center - (ControlDpiScalingHelper.ScaleToCurrentDpiX(padding) + Button1.Width),
					ClientSize.Height - Button1.Height - ControlDpiScalingHelper.ScaleToCurrentDpiY(padding), false);
				Button2.Location = ControlDpiScalingHelper.NewScaledPoint(center + ControlDpiScalingHelper.ScaleToCurrentDpiX(padding),
					ClientSize.Height - Button1.Height - ControlDpiScalingHelper.ScaleToCurrentDpiY(padding), false);
			}

			if (DontAskMeAgainInThisSessionCheckBox.Visible)
			{
				DontAskMeAgainInThisSessionCheckBox.Location = ControlDpiScalingHelper.NewScaledPoint(Button2.Location.X + Button2.Width + ControlDpiScalingHelper.ScaleToCurrentDpiX(20), Button2.Location.Y, false);
			}
		}

		void UpdateWidth(int offsetX)
		{
			var widthRequired = GetRealMaxWidth() + offsetX * 2 + ControlDpiScalingHelper.ScaleToCurrentDpiX(padding);

			if (Width < widthRequired)
			{
				ControlDpiScalingHelper.SetWidth(this, widthRequired, false);
				if (Width > CachedScreenInfo.Instance.PrimaryScreenInfo.Width)
				{
					ControlDpiScalingHelper.SetWidth(this, CachedScreenInfo.Instance.PrimaryScreenInfo.Width - ControlDpiScalingHelper.ScaleToCurrentDpiX(1), false);
				}
			}
		}

		internal int GetRealMaxWidth()
		{
			var messageBoxWidth = TextRenderer.MeasureText(TextBox.CreateGraphics(), LongestLineInMessage, TextBox.Font, ControlDpiScalingHelper.NewScaledSize(MaxWidth, 0, false), TextFormatFlags.WordBreak);

			var widthRequired = 0;
			switch (layout)
			{
				case ConfirmationMessageLayout.AllInOneLine:
					widthRequired = ConfirmationPromptLabel.Width + ExpectedStringLabel.Width + ConfirmationStringTextBox.Width;
					break;
				case ConfirmationMessageLayout.LineBreakAfterConfirmationPrompt:
					widthRequired = Math.Max(ConfirmationPromptLabel.Width, ExpectedStringLabel.Width + ConfirmationStringTextBox.Width);
					break;
				case ConfirmationMessageLayout.LineBreakAfterEachPart:
					widthRequired = Math.Max(Math.Max(ConfirmationPromptLabel.Width, ExpectedStringLabel.Width), ConfirmationStringTextBox.Width);
					break;
			}
			return Math.Max(widthRequired, messageBoxWidth.Width);
		}

		void SetConfirmationLayout(int offsetY)
		{
			var scaledPaddingY = ControlDpiScalingHelper.ScaleToCurrentDpiY(padding);
			var scaledPaddingX = ControlDpiScalingHelper.ScaleToCurrentDpiX(padding);
			var startLocation = ControlDpiScalingHelper.NewScaledPoint(TextBox.Location.X, ClientSize.Height - (offsetY + (scaledPaddingY * 2) + Button1.Height), false);
			var textBoxVerticalMiddleAlignmentAdjustment = (ConfirmationStringTextBox.Height - ExpectedStringLabel.Height) / 2;

			ConfirmationPromptLabel.Location = startLocation;

			if (layout == ConfirmationMessageLayout.AllInOneLine)
			{
				ControlDpiScalingHelper.SetHeight(this, Height + padding, false);

				ExpectedStringLabel.Location = ControlDpiScalingHelper.NewScaledPoint(startLocation.X + ConfirmationPromptLabel.Width, startLocation.Y, false);

				ConfirmationStringTextBox.Location = ControlDpiScalingHelper.NewScaledPoint(
					startLocation.X + ConfirmationPromptLabel.Width + ExpectedStringLabel.Width + scaledPaddingX,
					startLocation.Y - textBoxVerticalMiddleAlignmentAdjustment,
					false);
			}
			else if (layout == ConfirmationMessageLayout.LineBreakAfterConfirmationPrompt)
			{
				ControlDpiScalingHelper.SetHeight(this, Height + Math.Max(ConfirmationStringTextBox.Height, ExpectedStringLabel.Height) + scaledPaddingY, false);

				ExpectedStringLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(
					startLocation.X,
					ConfirmationPromptLabel.Location.Y + ConfirmationPromptLabel.Height + scaledPaddingY,
					false);

				ConfirmationStringTextBox.Location = ControlDpiScalingHelper.NewScaledPoint(
					ExpectedStringLabel.Location.X + ExpectedStringLabel.Width + scaledPaddingX,
					ExpectedStringLabel.Location.Y - textBoxVerticalMiddleAlignmentAdjustment,
					false);
			}
			else if (layout == ConfirmationMessageLayout.LineBreakAfterEachPart)
			{
				ControlDpiScalingHelper.SetHeight(this, Height + ConfirmationStringTextBox.Height + ExpectedStringLabel.Height + scaledPaddingY * 2, false);

				ExpectedStringLabel.Location = ControlDpiScalingHelper.NewScaledPoint(
					startLocation.X,
					ConfirmationPromptLabel.Location.Y + ConfirmationPromptLabel.Height + scaledPaddingY,
					false);

				ConfirmationStringTextBox.Location = ControlDpiScalingHelper.NewScaledPoint(
					startLocation.X,
					ExpectedStringLabel.Location.Y + ExpectedStringLabel.Height + scaledPaddingY,
					false);
			}
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			UpdateHeightWidthSettings();
		}

		protected override void Dispose(bool isNotFinalizing)
		{
			if (isNotFinalizing)
			{
				if (components != null)
				{
					components.Dispose();
				}
			}

			base.Dispose(isNotFinalizing);
		}

		void ConfirmationStringTextBox_TextChanged(object sender, EventArgs e)
		{
			Button1.Enabled = ExpectedString.ToUpper() == ConfirmationStringTextBox.Text.ToUpper();
			ExpectedStringLabel.UpdateInput(ConfirmationStringTextBox.Text);
		}

		void ConfirmationStringTextBox_KeyPress(object sender, KeyPressEventArgs e)
		{
			if (ConfirmationStringTextBox.SelectionStart < ExpectedString.Length)
			{
				var expectedChar = ExpectedString[ConfirmationStringTextBox.SelectionStart];
				char charToInsert;
				var startPosition = ConfirmationStringTextBox.SelectionStart;
				if (char.IsLetter(e.KeyChar) && char.IsLetter(expectedChar))
				{
					if (ConfirmationStringTextBox.SelectionLength > 0)
					{
						ConfirmationStringTextBox.Text = ConfirmationStringTextBox.Text.Remove(ConfirmationStringTextBox.SelectionStart, ConfirmationStringTextBox.SelectionLength);
					}

					charToInsert = char.IsUpper(expectedChar) ? char.ToUpper(e.KeyChar) : char.ToLower(e.KeyChar);
					ConfirmationStringTextBox.Text = ConfirmationStringTextBox.Text.Insert(startPosition, charToInsert.ToString());
					ConfirmationStringTextBox.SelectionStart = startPosition + 1;
					e.Handled = true;
				}
			}
		}

		#endregion
	}
}
