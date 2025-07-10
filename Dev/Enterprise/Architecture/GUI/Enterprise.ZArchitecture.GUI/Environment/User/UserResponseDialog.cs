using System;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Core.Forms;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

#pragma warning disable WTG1001 // Do not use the 'private' keyword.

namespace Enterprise.Core.Environment
{
	public partial class UserResponseDialog : ZMessageBox
	{
		public UserResponseDialog(string message, string caption, MessageBoxButtons buttons, MessageBoxIcon icon, MessageBoxDefaultButton defaultButton, CodeDescriptionPairList answerList = null, string defaultResponse = "")
			: base(message, caption, buttons, icon, defaultButton)
		{
			InitializeForm(answerList, defaultResponse);
		}

		public UserResponseDialog(string message, string caption, MessageBoxButtons buttons, MessageBoxIcon icon, string button1Text, CodeDescriptionPairList answerList = null, string defaultResponse = "")
			: base(message, caption, buttons, icon, button1Text)
		{
			InitializeForm(answerList, defaultResponse);
		}

		public UserResponseDialog(MultilingualString message, string caption, MessageBoxButtons buttons, MessageBoxIcon icon, MessageBoxDefaultButton defaultButton, CodeDescriptionPairList answerList = null, string defaultResponse = "")
			: base(message, caption, buttons, icon, defaultButton)
		{
			InitializeForm(answerList, defaultResponse);
		}

		void InitializeForm(CodeDescriptionPairList answerList, string defaultResponse)
		{
			AnswerList = answerList;
			MinimumResponseLength = 20;
			UserResponse = defaultResponse;
			InitializeComponent();
			UserResponseTextBox.AcceptsReturn = true;
		}

		[List("AnswerList")]
		public ZString UserResponse { get; set; }

		public CodeDescriptionPairList AnswerList { get; private set; }

		public int MinimumResponseLength { get; set; }

		public int MaximumResponseLength { get; set; }

		public Char PasswordChar
		{
			get => UserResponseTextBox.PasswordChar;
			set => UserResponseTextBox.PasswordChar = value;
		}

		public CharacterCasing UserResponseTextBoxCharactersCasing
		{
			get { return UserResponseTextBox.CharacterCasing; }
			set { UserResponseTextBox.CharacterCasing = value; }
		}

		public CharacterCasing UserResponseDropEditCharactersCasing
		{
			get { return UserResponseDropEdit.CharacterCasing; }
			set { UserResponseDropEdit.CharacterCasing = value; }
		}

		public bool UserResponseDropEditOnlyShowCode { get; set; }

		#region Implementation

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			SetVisibilityAndDataBindings();
			SetOkEnableState();
			UpdateHeightWidthSettings();
		}
		internal void InternalOnLoad(EventArgs e) => OnLoad(e);

		protected void UpdateHeightWidthSettings()
		{
			if (Message != null)
			{
				SuspendLayout();
				var paddingY = ControlDpiScalingHelper.ScaleToCurrentDpiY(4);
				var paddingX = ControlDpiScalingHelper.ScaleToCurrentDpiX(4);
				var maxWidth = CachedScreenInfo.Instance.PrimaryScreenInfo.Width * 2 / 3;

				var originalTextBoxHeight = TextBox.Height;
				var textSize = TextBox.CreateGraphics().MeasureString(CorrectNewLines(TextBox.Text), TextBox.Font, maxWidth);
				ControlDpiScalingHelper.SetWidth(ref TextBox, (int)Utilities.Round(new decimal(textSize.Width), 0) + paddingX, false);
				ControlDpiScalingHelper.SetHeight(ref TextBox, (int)Utilities.Round(new decimal(textSize.Height), 0), false);

				var offsetY = TextBox.Height + paddingY - originalTextBoxHeight;
				if (offsetY > 0)
				{
					ControlDpiScalingHelper.SetHeight(this, Height + offsetY, false);
				}

				var originalFormWidth = Width;
				var minimumRequiredWidth = (TextBox.Width + (paddingX * 4)) + PictureBox.Width;
				if (Width < minimumRequiredWidth)
				{
					ControlDpiScalingHelper.SetWidth(this, minimumRequiredWidth, false);
				}

				var offsetX = Width - originalFormWidth;
				if (offsetX > 0)
				{
					ControlDpiScalingHelper.SetWidth(ResponseControl, ResponseControl.Width + offsetX, false);
				}

				var center = Width / 2;

				PictureBox.Location = ControlDpiScalingHelper.NewScaledPoint(paddingX, paddingY, false);

				var textboxVerticalLocation = paddingY;

				if (PictureBox.Height > TextBox.Height)
				{
					textboxVerticalLocation = paddingY + (PictureBox.Height - TextBox.Height) / 2;
				}

				TextBox.Location = ControlDpiScalingHelper.NewScaledPoint(PictureBox.Width + paddingX * 2, textboxVerticalLocation, false);
				Button1.Location = ControlDpiScalingHelper.NewScaledPoint(center - (paddingX + Button1.Width), ClientSize.Height - (Button1.Height + paddingY), false);
				Button2.Location = ControlDpiScalingHelper.NewScaledPoint(center + paddingX, ClientSize.Height - (Button1.Height + paddingY), false);
				ResponseControl.Location = ControlDpiScalingHelper.NewScaledPoint(center - (ResponseControl.Width / 2), Button1.Location.Y - ((paddingY * 2) + ResponseControl.Height), false);

				ResumeLayout(true);
			}
		}

		void SetVisibilityAndDataBindings()
		{
			if (AnswerList == null)
			{
				UserResponseTextBox.Visible = true;
				UserResponseTextBox.SetDataBinding(this, "UserResponse");
				if (MaximumResponseLength > 0)
				{
					UserResponseTextBox.MaxLength = MaximumResponseLength;
				}
				ResponseControl = UserResponseTextBox;
			}
			else
			{
				UserResponseDropEdit.Visible = true;
				UserResponseDropEdit.SetDataBinding(this, "UserResponse");
				if (UserResponseDropEditOnlyShowCode)
				{
					ControlDpiScalingHelper.SetWidth(UserResponseDropEdit.CodeBox, UserResponseDropEdit.Width, false);
					UserResponseDropEdit.ShowDescriptionBox = false;
					UserResponseDropEdit.ShowInDropDown = ZDropEdit.ShowInDropDownList.OnlyShowCode;
					UserResponseDropEdit.UseFullWidthForCodeBox = true;
				}
				if (MaximumResponseLength > 0)
				{
					UserResponseDropEdit.MaxLength = MaximumResponseLength;
				}
				ResponseControl = UserResponseDropEdit;
			}
			ActiveControl = ResponseControl;
		}

		private void SetOkEnableState()
		{
			if (UserResponseTextBox.Visible)
			{
				Button1.Enabled = UserResponseTextBox.Text.Length >= MinimumResponseLength;
			}
			else if (UserResponseDropEdit.Visible)
			{
				Button1.Enabled = UserResponseDropEdit.SelectedIndex >= 0;
			}
		}

		void UserResponseTextBox_TextChanged(object sender, EventArgs e)
		{
			SetOkEnableState();
		}

		void UserResponseDropEdit_SelectedIndexChanged(object sender, EventArgs e)
		{
			SetOkEnableState();
		}

		#endregion
	}
}
