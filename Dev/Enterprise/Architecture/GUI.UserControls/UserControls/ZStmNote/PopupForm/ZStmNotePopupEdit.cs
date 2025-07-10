using System;
using System.ComponentModel;
using System.Windows.Forms;
using CargoWise.Types;
using CargoWise.Windows.UI;
using CargoWise.Windows.UI.Testing;
using Enterprise.Core.Forms;
using Enterprise.ZArchitecture.Environment;

#if !WINZOR
using CargoWise.Interop;
#endif

namespace Enterprise.ZArchitecture.GUI
{
	[SuppressFormDesignerAnalysis]
	[DefaultDataSourceBindingMember(null)]
	[ToolboxItem(false)]
	public partial class ZStmNotePopupEdit : ZStmNotePopupBase
	{
		#region Constructors

		public ZStmNotePopupEdit() => InitializeComponent();

		#endregion

		#region ShowTextBox property

		[Category(ZGUIConstants.DesignerCategory)]
		[DefaultValue(true)]
		public bool ShowTextBox
		{
			get => fShowTextBox;
			set
			{
				fShowTextBox = value;
				TextBox.Visible = value;
				ControlDpiScalingHelper.SetWidth(this, ButtonWidth + (value ? ControlDpiScalingHelper.ScaleToCurrentDpiX(DefaultTextBoxWidth) : 0), false);
			}
		}
		bool fShowTextBox = true;

		#endregion

		#region Showing the Editor

		protected override void HookPopup(ZStmNotePopupForm notePopupForm)
		{
			notePopupForm.NoteHasChangesChanged += new ZStmNotePopupForm.NoteHasChangesEventHandler(NotePopupForm_NoteHasChangesChanged);
			base.HookPopup(notePopupForm);
		}

		void NotePopupForm_NoteHasChangesChanged()
		{
			TextBox.UpdateTextFromNote();
			TextBox.UpdateNoteBuffer();
		}

		#endregion

		#region IDataBoundControl Members

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			base.SetDataBinding(dataSource, dataMember);
			if (dataSource != null)
			{
				TextBox.UpdateTextFromNote();
			}
		}

		#endregion

		#region Implementation

		protected override void OnReadOnlyChanged(EventArgs e)
		{
			TextBox.ReadOnly = ReadOnly;
			base.OnReadOnlyChanged(e);
		}

		protected override void SetBoundsCore(int x, int y, int width, int height, BoundsSpecified specified)
			=> base.SetBoundsCore(x, y, ShowTextBox ? width : ButtonWidth, FixedHeight, specified);

		#endregion

		#region class ZNoteTextBox

		protected internal class ZNoteTextBox : ZTextBox, IPastableControl
		{
			public ZNoteTextBox(ZStmNotePopupEdit parentEditor)
			{
				ParentEditor = parentEditor;
				CharacterCasing = CharacterCasing.Normal;
			}

			readonly ZStmNotePopupEdit ParentEditor;

			protected override IControlExtensionCollection NewExtensionCollection()
			{
				var result = base.NewExtensionCollection();
				result.Remove<ILabelCaptionRenderer>();
				return result;
			}

			#region Update Text from Note / Note from Text

			protected override void OnEnter(EventArgs e)
			{
				base.OnEnter(e);
				UpdateNoteBuffer();
			}

			protected override void OnLeave(EventArgs e)
			{
				base.OnLeave(e);
				UpdateNoteFromText();
			}

			internal void UpdateTextFromNote()
			{
				if (ParentEditor != null && ParentEditor.NoteExists)
				{
					Text = ParentEditor.NoteText.SubstringSafe(0, MaxLength);
				}
			}

			void UpdateNoteFromText()
			{
				if (NoteBuffer.IsEmpty)
				{
					if (ParentEditor.NoteExists)
					{
						ParentEditor.Note.Delete();
					}
				}
				else
				{
					ParentEditor.CreateNewNoteIfNotExists();
					ParentEditor.NoteText = NoteBuffer;
				}
			}

			internal void UpdateNoteBuffer() => NoteBuffer = ParentEditor.NoteExists
					? ParentEditor.NoteText
					: (ZString)string.Empty;

			ZString NoteBuffer;

			#endregion

			#region Capturing key presses & synchronising the buffer

			#region Cut / Paste

#if !WINZOR

			void HandleCut()
			{
				if (!string.IsNullOrEmpty(SelectedText))
				{
					if (SafeClipboard.SetDataObject(SelectedText))
					{
						HandleDeleteKey();
					}
					else
					{
						Globals.Message.Show(SafeClipboard.ClipboardNotAccessibleWarning);
					}
				}
			}

			void HandlePaste()
			{
				var dataObject = SafeClipboard.GetDataObject();
				if (dataObject != null)
				{
					var pastedValue = (string)dataObject.GetData(typeof(string));
					if (pastedValue != null)
					{
						HandlePastedText(pastedValue);
					}
				}
			}

			void HandlePastedText(ZString pastedText)
			{
				var caretLocation = SelectionStart;

				NoteBuffer = NoteBuffer.Remove(SelectionStart, SelectionLength);
				NoteBuffer = NoteBuffer.Insert(SelectionStart, pastedText);

				ParentEditor.CreateNewNoteIfNotExists();

				ParentEditor.NoteText = NoteBuffer;
				UpdateTextFromNote();

				var caretLocationAtEndOfPastedText = caretLocation + pastedText.Length;
				if (NoteBuffer.Length > MaxLength)
				{
					ParentEditor.ShowEditorAndSetCaretLocation(caretLocationAtEndOfPastedText);
				}
				else
				{
					SelectionStart = caretLocationAtEndOfPastedText;
				}
			}

#endif

			#endregion

			protected override void OnKeyPress(KeyPressEventArgs e)
			{
				e.Handled = true;

				if (e.KeyChar != BackspaceKey)
				{
					if (!char.IsControl(e.KeyChar))
					{
						HandleCharacterKey(e.KeyChar);
					}
				}
				else if (SelectionLength > 0)
				{
					HandleDeleteKey(); // backspace was pressed but .NET treats this the same as pressing the delete key when text is *selected*
				}
				else if (SelectionStart > 0) // nothing to backspace otherwise
				{
					HandleBackSpaceKey();
				}

				base.OnKeyPress(e);
			}

			protected override bool ProcessDialogKey(Keys keyData)
			{
				bool handled;

				if (keyData == Keys.Delete && SelectionStart < TextLength)
				{
					HandleDeleteKey();
					handled = true;
				}
				else
				{
					handled = base.ProcessDialogKey(keyData);
				}

				return handled;
			}

			ZString TextFromNoteBuffer => NoteBuffer.SubstringSafe(0, MaxLength);

			void HandleCharacterKey(char c)
			{
				var maxLengthWasExceeded = SelectionLength == 0 && Text.Length == MaxLength;
				var caretLocation = SelectionStart;

				NoteBuffer = NoteBuffer.Remove(SelectionStart, SelectionLength);
				NoteBuffer = NoteBuffer.Insert(SelectionStart, c.ToString());
				Text = TextFromNoteBuffer;

				if (maxLengthWasExceeded)
				{
					UpdateNoteFromText(); // OnLeave will not be called so manually invoke the update here
					ParentEditor.ShowEditorAndSetCaretLocation(caretLocation + 1);
				}
				else
				{
					SelectionStart = caretLocation + 1;
					SelectionLength = 0;
				}
			}

			void HandleBackSpaceKey()
			{
				var caretLocation = SelectionStart;
				NoteBuffer = NoteBuffer.Remove(SelectionStart - 1, 1);
				Text = TextFromNoteBuffer;
				SelectionStart = caretLocation - 1;
			}

			void HandleDeleteKey()
			{
				var caretLocation = SelectionStart;
				var charsToRemove = (SelectionLength == 0) ? 1 : SelectionLength;
				NoteBuffer = NoteBuffer.Remove(SelectionStart, charsToRemove);
				Text = TextFromNoteBuffer;
				SelectionStart = caretLocation;
			}

			const uint BackspaceKey = 8;

#if !WINZOR

			protected override void WndProc(ref Message message)
			{
				var handled = false;

				switch (message.Msg) // these are here to handle the controls context menu
				{
					case WindowsMessage.WM_DELETE:
						HandleDeleteKey();
						handled = true;
						break;

					case WindowsMessage.WM_PASTE:
						HandlePaste();
						handled = true;
						break;

					case WindowsMessage.WM_CUT:
						HandleCut();
						handled = true;
						break;
				}

				if (!handled)
				{
					base.WndProc(ref message);
				}
			}

#endif

			#endregion

			#region MaxLength

			protected override void OnResize(EventArgs e)
			{
				base.OnResize(e);
				UpdateMaxLength();
			}

			void UpdateMaxLength()
			{
				if (!IsMaxLengthBound && MaxLength != ExpectedMaxLength)
				{
					MaxLength = ExpectedMaxLength;
					UpdateTextFromNote();
				}
			}

			int ExpectedMaxLength => (int)(Width / 5.5);

			bool IsMaxLengthBound => DataBindings["MaxLength"] != null;

			#endregion

			#region IPastableControl Members

#if !WINZOR

			bool IPastableControl.TryPaste()
			{
				HandlePaste();
				return true;
			}

#endif

			#endregion
		}

		#endregion
	}
}
