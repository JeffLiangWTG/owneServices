using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using CargoWise.Interop;
using CargoWise.Windows.UI;
using CargoWise.Windows.UI.Interop;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

#pragma warning disable WTG1001 // Do not use the 'private' keyword.

namespace Enterprise.ZArchitecture.GUI
{
	[UserEventDiagnosticReference("Caption = \"{Text}\", Message = \"{Message}\"")]
	public partial class ZMessageBox : KForm, IDynamicSizedDialog, ICaptionRenderingSupport
	{
		ZMessageBox()
		{
			InitializeComponent();

			TextBox.EnsureCaretIsNotShown(this);

			Button1.TabStop = true;
			Button2.TabStop = true;
			Button3.TabStop = true;

			this.Font = OFont.GetFont();

			textBoxTranslationFeedbackManager = new TranslationFeedbackManager(TextBox, TranslationFeedbackManager.ClickMode.None);
			TextBox.Click += TextBox_Click;
		}

		void TextBox_Click(object sender, EventArgs e)
		{
			if (TranslationFeedbackManager.InTranslationFeedbackMode())
			{
				TranslationFeedbackManager.OpenFeedbackForm(TextBox, MessageMultilingual);
			}
		}

		public ZMessageBox(string message, string caption, MessageBoxButtons buttons, MessageBoxIcon icon)
			: this(message, caption, buttons, icon, MessageBoxDefaultButton.Button1)
		{ }

		public ZMessageBox(string message, string caption, MessageBoxButtons buttons, MessageBoxIcon icon, string button1Text)
			: this(message, caption, buttons, icon, MessageBoxDefaultButton.Button1)
		{
			Button1.Text = button1Text;
		}

		public ZMessageBox(string message, string caption, MessageBoxButtons buttons, MessageBoxIcon icon, string button1Text, string button2Text)
			: this(message, caption, buttons, icon, button1Text)
		{
			Button2.Text = button2Text;
		}

		public ZMessageBox(string message, string caption, MessageBoxButtons buttons, MessageBoxIcon icon, MessageBoxDefaultButton defaultButton)
			: this()
		{
			this.Message = message;
			InitializeForm(caption, buttons, icon, defaultButton);
		}

		public ZMessageBox(MultilingualString message, string caption, MessageBoxButtons buttons, MessageBoxIcon icon)
			: this(message, caption, buttons, icon, MessageBoxDefaultButton.Button1)
		{ }

		public ZMessageBox(MultilingualString message, string caption, MessageBoxButtons buttons, MessageBoxIcon icon, MessageBoxDefaultButton defaultButton)
			: this()
		{
			this.MessageMultilingual = message;
			InitializeForm(caption, buttons, icon, defaultButton);
		}

		void InitializeForm(string caption, MessageBoxButtons buttons, MessageBoxIcon icon, MessageBoxDefaultButton defaultButton)
		{
			Text = caption;
			SetHeightWidthSettings();
			SetIcon(icon);
			InitialiseButtons(buttons, defaultButton);
		}

		public string Message
		{
			get { return TextBox?.Text; }
			set
			{
				messageMultilingual = (NoResString)value;
				TextBox.Text = CorrectNewLines(value);
			}
		}

		public MultilingualString MessageMultilingual
		{
			get { return messageMultilingual; }
			set
			{
				messageMultilingual = value;
				TextBox.Text = CorrectNewLines(value);
			}
		}
		MultilingualString messageMultilingual;

		public void SetCustomIcon(Icon icon)
		{
			IconImage = icon;
		}

		#region Implementation

		protected Container components;
		protected ZPictureBox PictureBox;
		internal protected ZButton Button1;
		internal protected ZButton Button2;
		protected ZButton Button3;
		protected internal KTextBox TextBox;
		bool HasDisabledAllForms;

		Icon IconImage
		{
			set => Image = value.ToBitmap();
		}

		protected Image Image
		{
			set => PictureBox.Image = value;
		}

#if DEBUG
		internal
#endif
 protected MessageBoxButtons Buttons;

#if DEBUG
		internal
#endif
 DialogResult DefaultButton;

		protected override void OnLoad(EventArgs e)
		{
			DisableAllForms();
			HasDisabledAllForms = true;
			base.OnLoad(e);
		}

#if WINZOR
		const int MarginBetweenTextBoxAndButtons = 4;
#pragma warning disable CW1017 // Non DPI-aware code has been detected
		protected override void SetClientSizeCore(int width, int height)
		{
			if (resizedFromClient)
			{
				this.TextBox.Width -= ClientSize.Width - width;
			}
			base.SetClientSizeCore(width, height);
			var tbHeightBefore = TextBox.Height;
			TextBox.Height = Button1.Top - TextBox.Top - MarginBetweenTextBoxAndButtons;
			var tbHeightAfter = TextBox.Height;
			if (tbHeightAfter > tbHeightBefore)
			{
				TextBox.Height -= (tbHeightAfter - tbHeightBefore);
			}
		}
#pragma warning restore CW1017 // Non DPI-aware code has been detected
#endif

		internal string CorrectNewLines(string message)
		{
			var regex = new Regex("(?<!\r)\n");
			message = regex.Replace(message, System.Environment.NewLine);

			return message;
		}

		protected virtual void SetHeightWidthSettings()
		{
			this.EnsureDialogTextVisible();
		}

		///<remarks>returned Width and Height are scaled</remarks>
		internal Rectangle CurrentScreenInfo => IDynamicSizedDialogExtensions.CurrentScreenInfo;

		[DpiState(DpiState.ScaleX)]
		internal protected int MaxWidth => IDynamicSizedDialogExtensions.DefaultDialogMaxWidth;

		[DpiState(DpiState.ScaleY)]
		protected int MaxHeight => IDynamicSizedDialogExtensions.DefaultDialogMaxHeight;

		void SetIcon(MessageBoxIcon icon)
		{
			switch (icon)
			{
				case MessageBoxIcon.Error:
					IconImage = SystemIcons.Error;
					break;
				case MessageBoxIcon.Information:
					IconImage = SystemIcons.Information;
					break;
				case MessageBoxIcon.Question:
					IconImage = SystemIcons.Question;
					break;
				case MessageBoxIcon.Warning:
					IconImage = SystemIcons.Warning;
					break;
			}
		}

		#region Initialise Buttons

		void InitialiseButtons(MessageBoxButtons buttons, MessageBoxDefaultButton defaultButton)
		{
			this.Buttons = buttons;
			switch (buttons)
			{
				case MessageBoxButtons.AbortRetryIgnore:
					InitialiseThreeButtons(DialogResult.Abort, DialogResult.Retry, DialogResult.Ignore, defaultButton, MessageBoxDefaultButton.Button1);
					SetCloseResult(DialogResult.Abort);
					break;
				case MessageBoxButtons.OK:
					InitialiseOneButton(DialogResult.OK);
					SetCloseResult(DialogResult.OK);
					break;
				case MessageBoxButtons.OKCancel:
					InitialiseTwoButtons(DialogResult.OK, DialogResult.Cancel, defaultButton);
					SetCloseResult(DialogResult.Cancel);
					break;
				case MessageBoxButtons.RetryCancel:
					InitialiseTwoButtons(DialogResult.Retry, DialogResult.Cancel, defaultButton);
					SetCloseResult(DialogResult.Cancel);
					break;
				case MessageBoxButtons.YesNo:
					InitialiseTwoButtons(DialogResult.Yes, DialogResult.No, defaultButton);
					SetCloseResult(DialogResult.No);
					break;
				case MessageBoxButtons.YesNoCancel:
					InitialiseThreeButtons(DialogResult.Yes, DialogResult.No, DialogResult.Cancel, defaultButton, MessageBoxDefaultButton.Button3);
					SetCloseResult(DialogResult.Cancel);
					break;
			}

			switch (defaultButton)
			{
				case MessageBoxDefaultButton.Button1:
					this.DefaultButton = Button1.DialogResult;
					break;
				case MessageBoxDefaultButton.Button2:
					this.DefaultButton = Button2.DialogResult;
					break;
				case MessageBoxDefaultButton.Button3:
					this.DefaultButton = Button3.DialogResult;
					break;
			}
		}

		void InitialiseOneButton(DialogResult button1Result)
		{
			if (ClientRectangle.Width < Button1.Width + ControlDpiScalingHelper.ScaleToCurrentDpiX(20))
			{
				ControlDpiScalingHelper.SetWidth(this, ControlDpiScalingHelper.ScaleToCurrentDpiX(20) + Button1.Width + (Width - ClientRectangle.Width), false);
			}

			ControlDpiScalingHelper.SetLeft(ref Button1, (ClientRectangle.Width - Button1.Width) / 2, false);
			Button1.Text = DialogResultCaptions.GetCaptionForDialogResult(button1Result);
			Button1.Name = button1Result.ToString() + "Button";
			Button1.DialogResult = button1Result;
			Button2.Visible = false;
			Button3.Visible = false;

			AcceptButton = Button1;
			ActiveControl = (Control)AcceptButton;
		}

		internal void InitialiseTwoButtons(DialogResult button1Result, DialogResult button2Result, MessageBoxDefaultButton defaultButton)
		{
			if (ClientRectangle.Width < Button1.Width * 2 + ControlDpiScalingHelper.ScaleToCurrentDpiX(26))
			{
				ControlDpiScalingHelper.SetWidth(this, Button1.Width * 2 + ControlDpiScalingHelper.ScaleToCurrentDpiX(26) + (Width - ClientRectangle.Width), false);
			}

			ControlDpiScalingHelper.SetLeft(Button1, (ClientRectangle.Width - (Button1.Width * 2 + ControlDpiScalingHelper.ScaleToCurrentDpiX(6))) / 2, false);
			Button1.Text = DialogResultCaptions.GetCaptionForDialogResult(button1Result);
			Button1.DialogResult = button1Result;
			Button1.Name = button1Result.ToString() + "Button";
			ControlDpiScalingHelper.SetLeft(Button2, Button1.Right + ControlDpiScalingHelper.ScaleToCurrentDpiX(6), false);
			Button2.Text = DialogResultCaptions.GetCaptionForDialogResult(button2Result);
			Button2.Name = button2Result.ToString() + "Button";
			Button2.DialogResult = button2Result;
			Button2.Visible = true;
			Button3.Visible = false;

			switch (defaultButton)
			{
				case MessageBoxDefaultButton.Button1:
					AcceptButton = Button1;
					break;
				case MessageBoxDefaultButton.Button2:
					AcceptButton = Button2;
					break;
				case MessageBoxDefaultButton.Button3:
					throw new Exception("Cannot set default button to Button3 on a two button MessageBox!");
			}

			CancelButton = Button2;
			ActiveControl = (Control)AcceptButton;
		}

		void InitialiseThreeButtons(DialogResult button1Result, DialogResult button2Result, DialogResult button3Result, MessageBoxDefaultButton defaultAcceptButton, MessageBoxDefaultButton defaultCancelButton)
		{
			if (ClientRectangle.Width < Button1.Width * 3 + ControlDpiScalingHelper.ScaleToCurrentDpiX(32))
			{
				ControlDpiScalingHelper.SetWidth(this, ControlDpiScalingHelper.ScaleToCurrentDpiX(32) + Button1.Width * 3 + (Width - ClientRectangle.Width), false);
			}

			ControlDpiScalingHelper.SetLeft(Button1, (ClientRectangle.Width - (Button1.Width * 3 + ControlDpiScalingHelper.ScaleToCurrentDpiX(12))) / 2, false);
			Button1.Text = DialogResultCaptions.GetCaptionForDialogResult(button1Result);
			Button1.Name = button1Result.ToString() + "Button";
			Button1.DialogResult = button1Result;
			ControlDpiScalingHelper.SetLeft(Button2, Button1.Right + ControlDpiScalingHelper.ScaleToCurrentDpiX(6), false);
			Button2.Text = DialogResultCaptions.GetCaptionForDialogResult(button2Result);
			Button2.Name = button2Result.ToString() + "Button";
			Button2.DialogResult = button2Result;
			ControlDpiScalingHelper.SetLeft(Button3, Button2.Right + ControlDpiScalingHelper.ScaleToCurrentDpiX(6), false);
			Button3.Text = DialogResultCaptions.GetCaptionForDialogResult(button3Result);
			Button3.Name = button3Result.ToString() + "Button";
			Button3.DialogResult = button3Result;

			switch (defaultAcceptButton)
			{
				case MessageBoxDefaultButton.Button1:
					AcceptButton = Button1;
					break;
				case MessageBoxDefaultButton.Button2:
					AcceptButton = Button2;
					break;
				case MessageBoxDefaultButton.Button3:
					AcceptButton = Button3;
					break;
			}

			switch (defaultCancelButton)
			{
				case MessageBoxDefaultButton.Button1:
					CancelButton = Button1;
					break;
				case MessageBoxDefaultButton.Button2:
					CancelButton = Button2;
					break;
				case MessageBoxDefaultButton.Button3:
					CancelButton = Button3;
					break;
			}

			ActiveControl = (Control)AcceptButton;
		}

		void SetCloseResult(DialogResult result)
		{
			CloseResult = result;
		}

		DialogResult CloseResult;

		#endregion

		protected override void OnClosing(CancelEventArgs e)
		{
			if (HasDisabledAllForms)
			{
				ReEnableAllForms();
				HasDisabledAllForms = false;
			}

			base.OnClosing(e);

			// closing without choosing a button (ie alt+f4 or [x]) will set dialog result to cancel
			// it always should be cancel unless cancel is not an available option
			if (CloseResult != DialogResult.Cancel && DialogResult == DialogResult.Cancel)
			{
				DialogResult = CloseResult;
			}
		}

#if !WINZOR

		protected override void WndProc(ref Message m)
		{
			switch (m.Msg)
			{
				case WindowsMessage.WM_NCMOUSEMOVE:
					unchecked
					{
						if (m.WParam.ToInt32() == HitTestCodes.HTCAPTION && TranslationFeedbackManager.InTranslationFeedbackMode())
						{
							CaptionHighlightForm.Show(this);
						}
					}
					break;
			}
			base.WndProc(ref m);
		}

#endif

		#endregion

		#region IDisposable Members

		protected override void Dispose(bool isNotFinalizing)
		{
			if (isNotFinalizing)
			{
				if (textBoxTranslationFeedbackManager != null)
				{
					textBoxTranslationFeedbackManager.Dispose();
				}

				if (components != null)
				{
					components.Dispose();
				}

				if (HasDisabledAllForms)
				{
					ReEnableAllForms();
					HasDisabledAllForms = false;
				}
			}

			base.Dispose(isNotFinalizing);
		}

		#endregion

		#region IDynamicSizedDialog Members

		KTextBox IDynamicSizedDialog.DialogTextField => TextBox;

		int IDynamicSizedDialog.MaxHeight => MaxHeight;

		int IDynamicSizedDialog.MaxWidth => MaxWidth;

		int IDynamicSizedDialog.VerticalPadding => 18;

		#endregion

		#region ICaptionRenderingSupport Members

		[Category(ZGUIConstants.DesignerCategory)]
		[Browsable(true), DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		public bool? CaptionRenderingEnabled
		{
			get { return captionRenderingEnabled; }
			set
			{
				if (captionRenderingEnabled != value)
				{
					captionRenderingEnabled = value;
					OnCaptionRenderingEnabledChanged(EventArgs.Empty);
				}
			}
		}
		bool? captionRenderingEnabled;

		public event EventHandler CaptionRenderingEnabledChanged;

		void OnCaptionRenderingEnabledChanged(EventArgs e)
		{
			if (CaptionRenderingEnabledChanged != null)
			{
				CaptionRenderingEnabledChanged(this, e);
			}
		}

		#endregion

		#region Disable All Forms

#if !WINZOR

		private void DisableAllForms()
		{
			globalDisabledCount++;

			var activeMsgBoxParent = ZFormModaliser.GetActiveMessageBoxParentForm();
			foreach (var form in ZApplication.GetOpenForms().ToList())
			{
				if (!DisabledForms.Contains(form) && (form != activeMsgBoxParent) && !(form is ZMessageBox))
				{
					DisabledForms.Add(form);
				}
			}

			DisableMouseEvents();
		}

		private void ReEnableAllForms()
		{
			if (globalDisabledCount <= 0)
			{
				Globals.Message.ShowDeveloperErrorAlways("Cannot call ReEnableAllForms without a corresponding DisableAllForms.", "");
			}
			else
			{
				globalDisabledCount--;

				foreach (var form in ZApplication.GetOpenForms())
				{
					DisabledForms.Remove(form);
				}

				EnableMouseEvents();
			}
		}

		static List<Form> DisabledForms
		{
			get { return disabledForms ?? (disabledForms = new List<Form>()); }
		}

		[ThreadStatic]
		static List<Form> disabledForms;

		int globalDisabledCount;
		MouseHook mouseHook;

		private void DisableMouseEvents()
		{
			mouseHook = new MouseHook();
			mouseHook.Install();
			mouseHook.MouseDown += mouseHook_AllExceptMove;
			mouseHook.MouseMove += mouseHook_MouseMove;
			mouseHook.MouseDoubleClick += mouseHook_AllExceptMove;
		}

		public void EnableMouseEvents()
		{
			if (mouseHook != null)
			{
				mouseHook.Uninstall();
				mouseHook.MouseDown -= mouseHook_AllExceptMove;
				mouseHook.MouseMove -= mouseHook_MouseMove;
				mouseHook.MouseDoubleClick -= mouseHook_AllExceptMove;
				mouseHook = null;
			}
		}

		void mouseHook_AllExceptMove(object sender, MouseHookEventArgs e)
		{
			if (e.Control != null && DisabledForms.Contains(e.Control.FindForm()))
			{
				e.DisableMessage = true;
			}
		}

		void mouseHook_MouseMove(object sender, MouseHookEventArgs e)
		{
			Form form;
			if (e.Control != null && DisabledForms.Contains(form = e.Control.FindForm()))
			{
				if (form != null)
				{
					form.Cursor = Cursors.Default;
				}
				e.DisableMessage = true;
			}
		}

#endif

#if WINZOR

		private void DisableAllForms()
		{
			var activeMsgBoxParent = ZFormModaliser.GetActiveMessageBoxParentForm();
			foreach (var form in ZApplication.GetOpenForms().ToList())
			{
				if (form is not ZMessageBox && form != activeMsgBoxParent)
				{
					form.Enabled = true;

					foreach (var ownedForm in form.OwnedForms)
					{
						ownedForm.Enabled = false;
					}
				}
			}
		}

		private void ReEnableAllForms()
		{
			var activeMsgBoxParent = ZFormModaliser.GetActiveMessageBoxParentForm();
			foreach (var form in ZApplication.GetOpenForms().ToList())
			{
				if (form is not ZMessageBox && form != activeMsgBoxParent)
				{
					form.Enabled = true;

					foreach (var ownedForm in form.OwnedForms)
					{
						ownedForm.Enabled = true;
					}
				}
			}
		}

#endif

		#endregion

		readonly TranslationFeedbackManager textBoxTranslationFeedbackManager;
	}
}
