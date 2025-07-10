using System;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.Common.Testing;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using CargoWise.Windows.UI.Testing;
using Enterprise.Core.Forms;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Tools;
using Enterprise.ZArchitecture.Schema;
using Res = Enterprise.ZArchitecture.GUI.UserControls.Res;

namespace Enterprise.ZArchitecture.GUI.Internal
{
	[SuppressFormDesignerAnalysis]
	[SuppressFormsLocalizedTest]
	[DefaultDataSourceBindingMember(null)]
	[TypeDescriptionProvider(typeof(ZControlTypeDescriptionProvider))]
	internal partial class ZStmNoteRichTextBox : ZUserControl
	{
		public ZStmNoteRichTextBox()
		{
			InitializeComponent();
			OverrideValidationButton.Enabled = false;
			OverrideValidationButton.Visible = false;
			OverrideValidationButton.BringToFront();
			StmNoteUserInfoInserter.RegisterHotkeys(NoteTextBox);
			DisposableLeakListener.Instance.RegisterDisposable(this);
			RegisterEventHandlers();

			PopupButton.AllowOverlap(NoteRichTextBox);
		}

		protected override void OnLoad(EventArgs e)
		{
			SpellChecker.InitialiseSpellcheck(NoteTextBox, nameof(ZStmNoteRichTextBox) + nameof(NoteTextBox));
			SpellChecker.InitialiseSpellcheck(NoteRichTextBox, nameof(ZStmNoteRichTextBox) + nameof(NoteRichTextBox));

			base.OnLoad(e);
		}

		void RegisterEventHandlers()
		{
			NoteTextBox.MouseDown += new MouseEventHandler(TextBox_MouseDown);
			NoteRichTextBox.MouseDown += new MouseEventHandler(TextBox_MouseDown);
		}

		void TextBox_MouseDown(object sender, MouseEventArgs e)
		{
			base.OnMouseDown(e);
			CollateTextBoxFocusAndVisibility();
		}

		void CollateTextBoxFocusAndVisibility()
		{
			if (NoteTextBox.Focused && !NoteTextBox.Visible && NoteRichTextBox.Visible)
			{
				NoteRichTextBox.Focus();
			}
			else if (NoteRichTextBox.Focused && !NoteRichTextBox.Visible && NoteTextBox.Visible)
			{
				_ = NoteTextBox.Focus();
			}
		}

		#region Moving the Caret

		internal void SetCaretLocationAfterBindingHasFinished(int location)
		{
			CaretLocationToSetAfterBind = location;
			SetCaretLocation();
		}

		void SetCaretLocation()
		{
			NoteTextBox.SelectionStart = CaretLocationToSetAfterBind;
			#if !WINZOR
			NoteRichTextBox.SelectionStart = CaretLocationToSetAfterBind;
			#endif
			NoteTextBox.SelectionLength = 0;
			#if !WINZOR
			NoteRichTextBox.SelectionLength = 0;
			#endif
			if (NoteRichTextBox.Visible)
			{
				NoteRichTextBox.Focus();
			}
			else
			{
				_ = NoteTextBox.Focus();
			}
		}

		int CaretLocationToSetAfterBind;

		#endregion

		#region Control Size / Layout

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1091:Do Not Set CausesValidation to false", Justification = "temporary disable of validation")]
		internal void UpdateControlLayoutAfterBindingHasFinished(bool shouldChangeVisibility)
		{
#if DEBUG
			updateControlLayoutCalled = true;
#endif
			if (!IsDisposed && !NoteTextBox.IsDisposed)
			{
				var note = CurrentDataItem;
				if (note == null || note.IsDeleted)
				{
					return;
				}

				NoteRichTextBox.CausesValidation = false; // temporary disable of validation
				NoteTextBox.CausesValidation = false; // temporary disable of validation

				if (shouldChangeVisibility)
				{
					NoteRichTextBox.Visible = NoteRichTextBox.Enabled = !IsTextOnly;
					NoteTextBox.Visible = NoteTextBox.Enabled = IsTextOnly;

					TextOnlyLabel.Visible = IsTextOnly && IsDescriptionVisibleInTextMode;
					ModifyButton.Visible = IsModifyButtonVisible && IsPopupLog;
				}
				ModifyButton.ReadOnly = note.ReadOnly;

				#if WINZOR
				if (NoteTextBox.Visible)
				{
					NoteRichTextBox.RichTextBoxTop = 32;
				}
				else if (NoteRichTextBox.Visible)
				{
					NoteRichTextBox.RichTextBoxTop = 0;
				}
				#endif

				if (IsTextOnly)
				{
					if (IsDescriptionVisibleInTextMode)
					{
						ControlDpiScalingHelper.SetTop(ref NoteTextBox, NoteRichTextBox.RichTextBoxTop, false);
#if WINZOR
						ControlDpiScalingHelper.SetHeight(ref NoteTextBox, NoteRichTextBox.RichTextBoxHeight - NoteRichTextBox.RichTextBoxTop, false);
#else
						ControlDpiScalingHelper.SetHeight(ref NoteTextBox, NoteRichTextBox.RichTextBoxHeight, false);
#endif
					}
					else
					{
						ControlDpiScalingHelper.SetTop(ref NoteTextBox, NoteRichTextBox.Top, false);
						ControlDpiScalingHelper.SetHeight(ref NoteTextBox, NoteRichTextBox.Height, false);
					}
				}

				if (ContainsFocus)
				{
					if (IsTextOnly)
					{
						NoteTextBox.Focus();
					}
					else
					{
						NoteRichTextBox.Focus();
					}
				}

				NoteRichTextBox.CausesValidation = true;
				NoteTextBox.CausesValidation = true;

				var allowOverride = note != null && !note.IsDeleted && note.ST_Description == PredefinedNoteTypes.Instance.CountryRulesValidation.Description;
				OverrideValidationButton.Enabled = allowOverride;
				OverrideValidationButton.Visible = allowOverride;
			}
		}

#if DEBUG
		public bool updateControlLayoutCalled;
#endif

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);

			var note = CurrentDataItem;
			var allowOverride = note != null && !note.IsDeleted && note.ST_Description == PredefinedNoteTypes.Instance.CountryRulesValidation.Description;
			OverrideValidationButton.Enabled = allowOverride;
			OverrideValidationButton.Visible = allowOverride;

			if (CurrentDataItem == null)
			{
#if !WINZOR
				NoteRichTextBox.Rtf = string.Empty;
#else
				NoteRichTextBox.Html = string.Empty;
#endif
				NoteTextBox.Text = string.Empty;
			}
		}

#endregion

		#region IsModifyButtonVisible

		[Category(ZGUIConstants.DesignerCategory)]
		public bool IsModifyButtonVisible { get; set; }

		#endregion

		#region IsPopupButtonVisible

		[Category(ZGUIConstants.DesignerCategory)]
		[DefaultValue(true)]
		public bool IsPopupButtonVisible
		{
			get => PopupButton.Visible;
			set => PopupButton.Visible = value;
		}

		#endregion

		#region IsDescriptionVisibleInTextMode

		[Category(ZGUIConstants.DesignerCategory)]
		public bool IsDescriptionVisibleInTextMode { get; set; }

		#endregion

		#region IsTextOnly

		[Category(ZGUIConstants.DesignerCategory)]
		[DefaultValue(false)]
		public bool IsTextOnly
		{
			get => fIsTextOnly;
			set
			{
				if (fIsTextOnly != value)
				{
					fIsTextOnly = value;
				}
			}
		}

		// if we show a ZBool in the designer it will display "Y" / "N" (ZBool.ToString())
		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public ZBool IsTextOnlyForBinding
		{
			get => IsTextOnly;
			set
			{
				try
				{
					var valueHasChanged = false;
					if (IsTextOnly != value)
					{
						IsTextOnly = value;
						valueHasChanged = true;
						OnIsTextOnlyForBindingChanged();
					}

					if (!isUpdatingControlLayout)
					{
						isUpdatingControlLayout = true;
						UpdateControlLayoutAfterBindingHasFinished(valueHasChanged);
						isUpdatingControlLayout = false;
					}
				}
				catch (Exception)
				{
					//Rethrow the exception, this will be removed when we know more about issues 01024108.
					throw;
				}
			}
		}
		bool isUpdatingControlLayout;

		public event EventHandler IsTextOnlyForBindingChanged;

		public void OnIsTextOnlyForBindingChanged() => IsTextOnlyForBindingChanged?.Invoke(this, EventArgs.Empty);

		ZBool fIsTextOnly;

		#endregion

		#region IsPopupLog

		public ZBool IsPopupLog
		{
			get => isPopupLog;
			set
			{
				if (isPopupLog != value)
				{
					isPopupLog = value;
					UpdateControlLayoutAfterBindingHasFinished(true);
				}
			}
		}

		ZBool isPopupLog;

		#endregion

		#region OnDragDrop

		protected override void OnDragDrop(DragEventArgs dragEvent)
		{
			var parentIDLink = (CurrentDataItem != null) ? CurrentDataItem.PK.ToGuid() : Guid.Empty;
			using (var args = new RichEditDragEventArgs(StmNoteSchema.Constants.Prefix, parentIDLink, dragEvent))
			{
				base.OnDragDrop(args);
			}
		}

		#endregion

		#region Bind-To Properties

		[Category(ZGUIConstants.DesignerCategory)]
		public string BindToRtfNote
		{
			get => NoteRichTextBox.BindTo;
			set => NoteRichTextBox.BindTo = value;
		}

		[Category(ZGUIConstants.DesignerCategory)]
		public string BindToTextNote
		{
			get => NoteTextBox.BindTo;
			set => NoteTextBox.BindTo = value;
		}

		#endregion

		#region Modify / Popup Button Click

		void OverrideValidationButton_Click(object sender, EventArgs e)
		{
			var note = CurrentDataItem;
			if (note == null || note.IsDeleted)
			{
				return;
			}

			var rulePK = Guid.Empty;
			var ruleStart = note.ST_NoteText.IndexOf("<RulePK>", StringComparison.Ordinal) + "<RulePK>".Length;
			var ruleEnd = note.ST_NoteText.IndexOf("</RulePK>", StringComparison.Ordinal);
			if (!Guid.TryParse(note.ST_NoteText.SubstringSafe(ruleStart, ruleEnd - ruleStart), out rulePK))
			{
				return;
			}

			if (rulePK == Guid.Empty)
			{
				return;
			}

			var canOverride = false;
			if (Env.Security.CountriesOverrideCountryRuleValidation.IsAllowed)
			{
				canOverride = true;
			}
			else
			{
				var securityLogin = new SecurityLogin((s) => s.CountriesOverrideCountryRuleValidation)
				{
					HideApprovalRequestButton = true
				};
				var result = ZFormModaliser.ShowDialogAndDispose(new DocumentLoginForm(securityLogin));
				if (result == DialogResult.Yes)
				{ canOverride = true; }
			}

			if (canOverride)
			{
				note.Delete();

				if (note.OverrideValidationMasters.Any())
				{
					foreach (var master in note.OverrideValidationMasters)
					{
						_ = RefCountryRulesHelper.OverrideRule(master, (master as EnterpriseBusinessObject)?.Logs, rulePK, note.Factory);
					}
				}
				else
				{
					var master = (ParentForm as ZForm)?.BusinessEntity as IStmNoteParent;
					_ = RefCountryRulesHelper.OverrideRule(master, (master as EnterpriseBusinessObject)?.Logs, rulePK, note.Factory);
				}
			}
			else
			{
				Env.Security.CountriesOverrideCountryRuleValidation.ShowError();
			}
		}

		void ModifyButton_Click(object sender, EventArgs e)
		{
			if (CurrentDataItem != null)
			{
				ZFormModaliser.Show(GetEditablePopupForm(CurrentDataItem), FindForm());
			}
		}

		void PopupButton_Click(object sender, EventArgs e)
		{
			if (CurrentDataItem != null)
			{
				var form = IsPopupLog
					? new ZStmNotePopupViewForm(CurrentDataItem)
					: (Form)GetEditablePopupForm(CurrentDataItem, ParentForm);
				ZFormModaliser.Show(form, FindForm());
			}
		}

		new StmNote CurrentDataItem => base.CurrentDataItem as StmNote;

		ZStmNotePopupForm GetEditablePopupForm(StmNote note, Form parentForm = null) => new ZStmNotePopupForm(note, !((BusinessObject)note.Master).HasChanges, false, parentForm);

		#endregion

		#region classes InternalZRichTextBox + InternalZTextBox

		internal class InternalZRichTextBox : ZRichTextBox
		{
			public InternalZRichTextBox()
			{
#if WINZOR
				PopupButton.Visible = false;
				RichTextBoxTop = RichEdit.Top;
#endif
			}

#if !WINZOR

			public override void Paste()
			{
				// this hack is to resolve a bug that can be reproduced with the following steps:
				//   1. Create a text note. Notice that the text box changes to a TextBox.
				//   2. Click a field in the new row below the text note. Notice that the text box changes to a RichTextBox.
				//   3. Right-click the RichTextBox and select "Paste". It will try to paste into the rich text of the text note.
				if (!ParentNoteControl.IsTextOnly)
				{
					base.Paste();
				}
			}

#endif

			protected override void SetVisibleCore(bool value)
			{
#if !WINZOR
				SuppressSetRtf = !value;
#else
				SuppressSetHtml = !value;
#endif
				base.SetVisibleCore(value);
			}

			protected override void OnVisibleChanged(EventArgs e)
			{
				base.OnVisibleChanged(e);
#if !WINZOR
				SuppressSetRtf = !Visible;
#else
				SuppressSetHtml = !Visible;
#endif
			}

			public override bool IsPopupButtonVisible
			{
				get => false;
				set => ErrorReporter.ReportOnce("ZStmNoteRichTextBoxPopupButton", "The ZRichTextBox popup button is not used on the ZStmNoteRichTextBox (ZStmNoteRichTextBox has it's own popup button)");
			}

#if !WINZOR
			public int RichTextBoxTop => RichEdit.Top;
#else
			public int RichTextBoxTop;
#endif
			public int RichTextBoxHeight => RichEdit.Height;

#if !WINZOR
			ZStmNoteRichTextBox ParentNoteControl => (ZStmNoteRichTextBox)Parent;
#endif
		}

		internal class InternalZTextBox : ZTextBox
		{
			public InternalZTextBox() => Font = new Font("Lucida Console", DesignModeFinder.IsDesigning ? 10.25f : EnvProxy.Instance.Registry.TextOnlyNoteFontSize);

			protected override bool ProcessCmdKey(ref Message msg, Keys keyData) => ProcessCmdKey(keyData) || base.ProcessCmdKey(ref msg, keyData);

#if DEBUG
			internal
#endif
			bool ProcessCmdKey(Keys keyData)
			{
				if (keyData == Keys.Tab)
				{
					var message = Res.GetString("83e4016e-8301-4d21-bc91-8d0b184770c6", "Tabs are not available in text-only notes. Use the spacebar to add spaces instead.\r\n{0}", ZRichTextBox.TabTipText);
					UserNotificationProvider.ShowWarning(message, CannotUseTabsMessage);
				}
				else if (keyData == (Keys.Control | Keys.I)) // if we call base and Ctrl-I was pressed it will insert a tab, so don't do this
				{
					UserNotificationProvider.ShowWarning(Res.GetString("dce4c507-1101-440a-92cb-7175be0fc624", "Italic Text (Ctrl-I) is not available in text-only notes."), Res.GetString("bd972f59-5979-4ed9-9eec-19c06c8286cd", "Cannot Italicize Text"));
				}
				else if (keyData == (Keys.Control | Keys.B))
				{
					UserNotificationProvider.ShowWarning(Res.GetString("3dae589d-20c6-421f-8fb5-f78582d6e39a", "Bold Text (Ctrl-B) is not available in text-only notes."), Res.GetString("498451f2-16ad-448a-853b-f2965a6845b4", "Cannot Bold Text"));
				}
				else if (keyData == (Keys.Control | Keys.U))
				{
					UserNotificationProvider.ShowWarning(Res.GetString("af23d0f1-3f4f-4c7d-9a30-22c737537af6", "Underlined Text (Ctrl-U) is not available in text-only notes."), Res.GetString("84348c25-b23f-4f27-955d-50c094ca4773", "Cannot Underline Text"));
				}
				else if (keyData == (Keys.Control | Keys.A))
				{
					SelectAll();
				}
				else if (keyData == (Keys.Control | Keys.V))
				{
					prePastedText = Text;
					return false;
				}
				else
				{
					return false;
				}

				return true;
			}

			string prePastedText;

#if !WINZOR
			protected override void WndProc(ref Message msg)
			{
				// this hack is to resolve a bug that can be reproduced with the following steps:
				//   1. Create a text note. Change the text note to a rich note but don't commit yet
				//   2. Right click on bottom text box, context menu for text box should still be open
				//   3. Select 'paste' from the context menu, data will be pasted into invisible text box
				//   4. Exception will occur next time you click away
				if (msg.Msg == CargoWise.Interop.WindowsMessage.WM_LBUTTONDOWN || msg.Msg == CargoWise.Interop.WindowsMessage.WM_PASTE)
				{
					ParentNoteControl.UpdateControlLayoutAfterBindingHasFinished(true);
					if (!Visible)
					{
#if NETFRAMEWORK
						msg.Result = (IntPtr)0;
#else
						msg.Result = 0;
#endif
						return; //swallow LBUTTONDOWN and WM_PASTE if we are hidden
					}
				}

				base.WndProc(ref msg);

				if (msg.Msg == CargoWise.Interop.WindowsMessage.WM_PASTE)
				{
					HandleTextAdded(Text);
					prePastedText = null;
				}
			}
#endif

#if DEBUG
			internal
#endif
			void HandleTextAdded(string textboxContent)
			{
				if (textboxContent.Contains("\t"))
				{
					var messageLines = new[]
					{
						Res.GetString("9e311840-87ee-4e47-8ed0-7649c578e285", "The text you are trying to add contains tabs, which are not available"),
						Res.GetString("1d5f4d92-93a7-4218-a910-c5be17664b4a", "in text-only notes. Press Cancel to undo the operation, or enter the"),
						Res.GetString("085bf988-731b-47e0-8e2a-bef71f5f027b", "number of spaces to replace each tab with below and press OK.") };
					var message = string.Join(System.Environment.NewLine, messageLines);
					var args = new UserResponseArgument
					{
						Message = message,
						Caption = CannotUseTabsMessage,
						Buttons = ZMessageBoxButtons.OKCancel,
						Icon = ZMessageBoxIcon.Warning,
						DefaultButton = ZMessageBoxDefaultButton.Button1,
						MinimumResponseLength = 1,
						MaximumResponseLength = 2,
						DefaultAnswer = "4"
					};
					var numSpacesAsString = UserNotificationProvider.QueryUserResponse(args);
					if (!string.IsNullOrWhiteSpace(numSpacesAsString) && uint.TryParse(numSpacesAsString, out var numSpaces))
					{
						var spaces = string.Empty.PadRight((int)numSpaces, ' ');
						var replacedText = textboxContent.Replace("\t", spaces);
						if (replacedText.Length > MaxLength)
						{
							replacedText = replacedText.Substring(0, MaxLength);
						}
						Text = replacedText;
					}
					else
					{
						Text = prePastedText;
					}
				}
			}

			static string CannotUseTabsMessage => Res.GetString("b96a1fe6-71ef-4591-a7c1-62576bfc4f51", "Tabs not available");

			public override string SelectedText
			{
				get => base.SelectedText;
				set
				{
					base.SelectedText = value;
					HandleTextAdded(value);
				}
			}

			// unused
			//Icon NoteIcon => Enterprise.ZArchitecture.Core.Icons.GetIcon(Enterprise.ZArchitecture.Core.IconTypes.StmNote);

			internal IUserNotification UserNotificationProvider
			{
				get => userNotificationOverride ?? Globals.Message;
				set => userNotificationOverride = value;
			}
			IUserNotification userNotificationOverride;

#if !WINZOR
			ZStmNoteRichTextBox ParentNoteControl => (ZStmNoteRichTextBox)Parent;
#endif
		}

		#endregion

		#region IDataBoundControl Members

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			base.SetDataBinding(dataSource, dataMember);
			SetDataSourceBinding("IsTextOnlyForBinding", StmNote.Schema.ST_IsTextOnly);
			SetDataSourceBinding("IsPopupLog", StmNote.Schema.ST_IsPopupLog);
		}

		#endregion

		#region ReadOnly

		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool ReadOnly
		{
			get => NoteRichTextBox.ReadOnly && NoteTextBox.ReadOnly;
			set
			{
				NoteRichTextBox.ReadOnly = value;
				NoteTextBox.ReadOnly = value;
				ReadOnlyForBindingProperty.OnReadOnlyChanged();
			}
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("This property is intended for data binding only")]
		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool ReadOnlyForBinding
		{
			get => ReadOnlyForBindingProperty.ReadOnlyForBinding;
			set => ReadOnlyForBindingProperty.ReadOnlyForBinding = value;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("This property is intended for data binding only")]
		public event EventHandler ReadOnlyForBindingChanged
		{
			add { ReadOnlyForBindingProperty.ReadOnlyForBindingChanged += value; }
			remove { ReadOnlyForBindingProperty.ReadOnlyForBindingChanged -= value; }
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("This property is intended for data binding only")]
		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool ReadOnlyForBindingIsNull
		{
			get => false;
			set
			{
				if (value)
				{
					ReadOnlyForBinding = true;
				}
			}
		}

		ControlReadOnlyPropertyHelper ReadOnlyForBindingProperty
		{
			get
			{
				if (readOnlyForBindingProperty == null)
				{
					readOnlyForBindingProperty = new ControlReadOnlyPropertyHelper(
						this,
						delegate
						{ return ReadOnly; },
						delegate(bool value)
						{ ReadOnly = value; });
				}
				return readOnlyForBindingProperty;
			}
		}
		ControlReadOnlyPropertyHelper readOnlyForBindingProperty;

		#endregion

		#region GetPropertyDescriptors

		public static PropertyDescriptor[] GetPropertyDescriptors()
			=> new ControlPropertyDescriptorBuilder<ZStmNoteRichTextBox>()
				.Property("IsPopupLog", ZBool.True, false)
				.Property("IsTextOnlyForBinding", ZBool.False, true)
				.Result;

		#endregion
	}
}
