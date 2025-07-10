using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using CargoWise.Common.Testing;
using CargoWise.ComponentModel;
using CargoWise.Interop;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.GUI.Notifications;

namespace Enterprise.ZArchitecture
{
	public enum OTransportType
	{
		Air, Other
	}

	#region OMasterBillNumber

	[DefaultBindingProperty("FormattedMasterBill")]
	public abstract partial class OMasterBillNumber : ZUserControl
	{
		#region Auto-Generated

		readonly Container components;

		#endregion

		public event EventHandler FormattedMasterBillChanged;

		protected OMasterBillNumber()
		{
			InitializeComponent();

			MasterBill1TextBox.MasterBill2TextBox = MasterBill2TextBox;
			MasterBill2TextBox.MasterBill1TextBox = MasterBill1TextBox;

			// for binding
			MasterBill1TextBox.TextChanged += new EventHandler(MasterBillTextBox_TextChanged);
			MasterBill2TextBox.TextChanged += new EventHandler(MasterBillTextBox_TextChanged);
			MasterBillOtherTextBox.TextChanged += new EventHandler(MasterBillTextBox_TextChanged);

			// for auto-formatting of spaces
			MasterBill1TextBox.Enter += new EventHandler(MasterBillTextBox_Enter);
			MasterBill2TextBox.Enter += new EventHandler(MasterBillTextBox_Enter);
			MasterBill1TextBox.Leave += new EventHandler(MasterBillTextBox_Leave);
			MasterBill2TextBox.Leave += new EventHandler(MasterBillTextBox_Leave);

			// for the non-Air MasterBill box
			MasterBillOtherTextBox.KeyPress += new KeyPressEventHandler(MasterBillOtherTextBox_KeyPress);

			UserEventTracker.Instance.AddUserEventToControl(this);
			UserEventTracker.Instance.AddUserEventToControl(MasterBill1TextBox);
			UserEventTracker.Instance.AddUserEventToControl(MasterBill2TextBox);

			MasterBill1TextBox.AllowOverlap(DummyTextBox);
			MasterBill2TextBox.AllowOverlap(DummyTextBox);
			SeparatorLabel.AllowOverlap(DummyTextBox);
			MasterBill1TextBox.AllowOverlap(SeparatorLabel);
		}

		/// <summary>
		/// Set to true to allow MAWB number prefix (MAWP) to be alphanumeric, e.g. LHR-12345678. Otherwise allows only 125-12345678. No effect on MAWN.
		/// </summary>
		public bool AllowAlphaInMAWP
		{
			get { return allowAlphaInMAWP; }
			set
			{
				allowAlphaInMAWP = value;
				if (MasterBill1TextBox != null)
				{
					MasterBill1TextBox.AllowAlphaInMAWP = value;
				}
			}
		}
		bool allowAlphaInMAWP;

		#region Property (MasterBillText)

		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public object MasterBillText
		{
			get { return (TransportType == OTransportType.Air) ? MasterBill1TextBox.Text + MasterBill2TextBox.Text : MasterBillOtherTextBox.Text; }
			set
			{
				try
				{
					suspendRaiseTextChanged = true; // don't fire text changed multiple times

					if (value == DBNull.Value || string.IsNullOrEmpty(value.ToString()))
					{
						MasterBill1TextBox.Text = "";
						MasterBill2TextBox.Text = "";
						MasterBillOtherTextBox.Text = "";
					}
					else
					{
						var masterBill = value.ToString();

						if (TransportType == OTransportType.Air)
						{
							if (masterBill.Length <= MasterBill1TextBox.MaxLength)
							{
								MasterBill1TextBox.Text = masterBill;
								MasterBill2TextBox.Text = "";
							}
							else
							{
								MasterBill1TextBox.Text = masterBill.Substring(0, MasterBill1TextBox.MaxLength);
								MasterBill2TextBox.Text = masterBill.Substring(MasterBill1TextBox.MaxLength, masterBill.Length - MasterBill1TextBox.MaxLength);
							}
						}
						else
						{
							MasterBillOtherTextBox.Text = masterBill;
						}
					}
				}
				finally
				{
					suspendRaiseTextChanged = false;
				}
			}
		}

		[BindingMetaDataProperty(MetaDataTypes.ReadOnly, "ReadOnly")]
		[BindingOptions(UseTypeConverters = true)]
		public object FormattedMasterBill
		{
			get { return (TransportType == OTransportType.Air) ? GetFormattedMasterBill(MasterBill1TextBox.Text + MasterBill2TextBox.Text) : MasterBillOtherTextBox.Text; }
			set
			{
				var beforeMasterBillText = MasterBillText;
				var newMasterBillText = (TransportType == OTransportType.Air) ? GetFormattedMasterBillWithSpaceOnly(value.ToString()) : value;

				if (!beforeMasterBillText.Equals(newMasterBillText))
				{
					MasterBillText = newMasterBillText;
					OnFormattedMasterBillChanged();
				}
			}
		}

		string GetUnformattedMasterBill(string value)
		{
			var result = "";
			foreach (var c in value)
			{
				if ((AllowAlphaInMAWP && char.IsLetterOrDigit(c)) || char.IsDigit(c))
				{
					result += c;
				}
			}
			return result;
		}

		string GetFormattedMasterBill(string value)
		{
			var result = GetUnformattedMasterBill(value);

			if (result.Length > 3)
			{
				result = result.Insert(3, "-");
			}

			if (result.Length > 8)
			{
				result = result.Insert(8, " ");
			}

			return result;
		}

		string GetFormattedMasterBillWithSpaceOnly(string value)
		{
			var result = GetUnformattedMasterBill(value);

			if (result.Length > 7)
			{
				result = result.Insert(7, " ");
			}

			return result;
		}

		#endregion

		#region Property (TransportType)

		/// <summary>
		/// Gets or sets the TransportType associated with the MasterBill Number.
		/// </summary>
		[Browsable(true), DesignerSerializationVisibility(DesignerSerializationVisibility.Visible), DefaultValue(OTransportType.Air)]
		public OTransportType TransportType
		{
			get { return transportType; }
			set
			{
				transportType = value;

				var showAirTextBoxes = (transportType == OTransportType.Air);

				DummyTextBox.Visible = showAirTextBoxes;
				DummyTextBox.SendToBack();
				MasterBill1TextBox.Visible = showAirTextBoxes;
				MasterBill2TextBox.Visible = showAirTextBoxes;
				SeparatorLabel.Visible = showAirTextBoxes;

				MasterBillOtherTextBox.Visible = !showAirTextBoxes;
			}
		}

		OTransportType transportType = OTransportType.Air;

		#endregion

		#region Auto-Generated

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}

		#endregion

		#region Implementation

		bool suspendRaiseTextChanged;

		// Fires the FormattedMasterBillChanged event.
		protected virtual void OnFormattedMasterBillChanged()
		{
			if (FormattedMasterBillChanged != null)
			{
				FormattedMasterBillChanged(this, new EventArgs());
			}
		}

		// Fire the OnTextChanged event for binding.
		void MasterBillTextBox_TextChanged(object sender, EventArgs e)
		{
			if (!suspendRaiseTextChanged)
			{
				OnFormattedMasterBillChanged();
			}
		}

		#region Auto-Formatting

		public abstract bool ReadOnly { get; set; }

		// Auto-remove space.
		void MasterBillTextBox_Enter(object sender, EventArgs e)
		{
			if (!ReadOnly)
			{
				FormatBox2WithSpaces = false;
			}
		}

		// Auto-add space.
		void MasterBillTextBox_Leave(object sender, EventArgs e)
		{
			if (!ReadOnly)
			{
				FormatBox2WithSpaces = true;
			}
		}

		// Sets a value indicating that whether box 2 should display a space after the first 4 characters.
		bool FormatBox2WithSpaces
		{
			set
			{
				suspendRaiseTextChanged = true;
				try
				{
					if (value)
					{
						const int InsertSpaceAt = 4;

						if (MasterBill2TextBox.Text.Length > InsertSpaceAt)
						{
							MasterBill2TextBox.MaxLength = OMasterBill2TextBox.InitialMaxLength + 1;
							MasterBill2TextBox.Text = MasterBill2TextBox.Text.Insert(InsertSpaceAt, " ");
						}
					}
					else
					{
						MasterBill2TextBox.MaxLength = OMasterBill2TextBox.InitialMaxLength;
						MasterBill2TextBox.Text = MasterBill2TextBox.Text.Replace(" ", "");
					}
				}
				finally
				{
					suspendRaiseTextChanged = false;
				}
			}
		}

		#endregion

		void MasterBillOtherTextBox_KeyPress(object sender, KeyPressEventArgs e)
		{
			if (!MasterBillOtherTextBox.IsKeyAlphaNumericOrControl(e.KeyChar))
			{
				// don't allow invalid values
				e.Handled = true;
			}
		}

		#endregion
	}

	#region OMasterBillTextBox

	[ToolboxItem(false)]
	public class OMasterBillTextBox : ZTextBox.Bare
	{
		#region Helper Methods

		// Returns true if the textbox is full.
		public bool IsFull()
		{
			return Text.Length == MaxLength;
		}

		// Returns true if the textbox is empty.
		public bool IsEmpty()
		{
			return Text.Length == 0;
		}

		// Returns true if the textbox is full and the caret is beyond the last character.
		public bool IsCaretAtEnd()
		{
			return Text.Length == SelectionStart && Text.Length == MaxLength;
		}

		// Returns true if the caret is before the first character in the textbox.
		public bool IsCaretAtStart()
		{
			return SelectionStart == 0;
		}

		// Sets the caret to a specified position in the textbox.
		public void SetCaretPositionAt(int caretPosition)
		{
			Focus();
			SelectionLength = 0;
			SelectionStart = caretPosition;
		}

		#endregion

		#region KeyPress Comparisons

		// Returns true if the character is a control key (ie Backspace).
		public bool IsControlKey(char key)
		{
			return char.IsControl(key);
		}

		// Returns true if the character is a digit or control key (ie Backspace).
		public bool IsKeyDigitOrControl(char key)
		{
			return char.IsDigit(key) || IsControlKey(key);
		}

		// Returns true if the character is a digit, alpha or control key (ie Backspace).
		public bool IsKeyAlphaNumericOrControl(char key)
		{
			return char.IsLetterOrDigit(key) || IsControlKey(key);
		}

		#endregion
	}

	#endregion

	#region OMasterBill1TextBox

	[ToolboxItem(false)]
	public class OMasterBill1TextBox : OMasterBillTextBox
	{
		public const int InitialMaxLength = 3;

		public OMasterBill1TextBox()
		{
			MaxLength = InitialMaxLength;
		}

		public bool AllowAlphaInMAWP { get; set; }

		#region Box 2

		public OMasterBill2TextBox MasterBill2TextBox
		{
			get { return masterBill2TextBox; }
			set { masterBill2TextBox = value; }
		}

		OMasterBill2TextBox masterBill2TextBox;

		#endregion

#if !WINZOR

		protected override void WndProc(ref Message m)
		{
			var handled = false;

			switch (m.Msg) // these are here to handle the controls context menu
			{
				case WindowsMessage.WM_DELETE:
					HandleDeleteKeyDown();
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
				base.WndProc(ref m);
			}
		}

		void HandlePaste()
		{
			var dataObject = SafeClipboard.GetDataObject();
			if (dataObject != null)
			{
				var value = (string)dataObject.GetData(typeof(string));
				if (value != null)
				{
					foreach (var c in value)
					{
						HandleCharPress(c);
					}
				}
			}
		}

		void HandleCut()
		{
			if (SafeClipboard.SetDataObject(SelectedText))
			{
				HandleDeleteKeyDown();
			}
			else
			{
				Globals.Message.Show(SafeClipboard.ClipboardNotAccessibleWarning);
			}
		}

#endif

		#region KeyDown Event

		protected override void OnKeyDown(KeyEventArgs e)
		{
			base.OnKeyDown(e);

			// handle right key
			if (IsCaretAtEnd() && e.KeyCode == Keys.Right)
			{
				HandleRightKeyDown();
				e.Handled = true;
			}
			// handle delete key
			else if (e.KeyCode == Keys.Delete)
			{
				HandleDeleteKeyDown();
				e.Handled = true;
			}
			// handle end key
			else if (e.KeyCode == Keys.End && IsFull())
			{
				HandleEndKeyDown();
				e.Handled = true;
			}
		}

		void HandleRightKeyDown()
		{
			// move caret to box 2
			masterBill2TextBox.SetCaretPositionAt(0);
		}

		void HandleDeleteKeyDown()
		{
			if (SelectionLength > 0 && !masterBill2TextBox.IsEmpty())
			{
				// delete the selection and bubble chars from box 2 back to box 1
				var caretPosition = SelectionStart;
				var charsToBubbleBack = (SelectionLength > masterBill2TextBox.Text.Length) ? masterBill2TextBox.Text.Length : SelectionLength;

				Text = Text.Remove(SelectionStart, SelectionLength);
				Text += masterBill2TextBox.Text.Substring(0, charsToBubbleBack);
				masterBill2TextBox.Text = masterBill2TextBox.Text.Remove(0, charsToBubbleBack);

				SetCaretPositionAt(caretPosition);
			}
			else if (IsCaretAtEnd())
			{
				// move focus to box 2 and then delete the first char
				if (masterBill2TextBox.Text.Length > 0)
				{
					masterBill2TextBox.SetCaretPositionAt(0);
					masterBill2TextBox.Text = masterBill2TextBox.Text.Remove(0, 1);
				}
			}
			else if (!masterBill2TextBox.IsEmpty())
			{
				// bubble first char from box 2 back to box 1
				var deletePosition = SelectionStart;
				if (deletePosition + 1 <= Text.Length)
				{
					Text = Text.Remove(deletePosition, 1);
				}
				while (Text.Length < 3)
				{
					Text += masterBill2TextBox.Text[0];
					masterBill2TextBox.Text = masterBill2TextBox.Text.Remove(0, 1);
					SetCaretPositionAt(deletePosition);
				}
			}
			else
			{
				// do a normal delete
				if (SelectionLength == 0)
				{
					SelectionLength = 1; // in case no selection
				}
				var deletePosition = SelectionStart;

				Text = Text.Remove(deletePosition, SelectionLength);
				SetCaretPositionAt(deletePosition);
			}
		}

		void HandleEndKeyDown()
		{
			// move to end of box 2
			masterBill2TextBox.SetCaretPositionAt(masterBill2TextBox.Text.Length);
		}

		#endregion

		#region KeyPress Event

		void HandlePushCharactersIntoBox2(char key)
		{
			var insertPostion = SelectionStart;

			if (insertPostion == MaxLength)
			{
				// insert at start of box 2
				masterBill2TextBox.Text = key + masterBill2TextBox.Text;
				masterBill2TextBox.SetCaretPositionAt(1);
			}
			else
			{
				// insert into box 1, bubble last char across to box 2
				var text = Text.Insert(insertPostion, key.ToString());

				Text = text.Substring(0, 3);
				masterBill2TextBox.Text = text[text.Length - 1] + masterBill2TextBox.Text;
				SetCaretPositionAt(insertPostion + 1);
			}
		}

		void HandleBackSpaceKeyPress()
		{
			if (SelectionLength > 0 && !masterBill2TextBox.IsEmpty())
			{
				// delete the selection and bubble chars from box 2 back to box 1
				var caretPosition = SelectionStart;
				var charsToBubbleBack = SelectionLength;

				if (SelectionStart >= 0)
				{
					Text = Text.Remove(SelectionStart, charsToBubbleBack);
					Text += masterBill2TextBox.Text.Substring(0, Math.Min(masterBill2TextBox.Text.Length, charsToBubbleBack));
					masterBill2TextBox.Text = masterBill2TextBox.Text.Remove(0, Math.Min(masterBill2TextBox.Text.Length, charsToBubbleBack));
					SetCaretPositionAt(caretPosition);
				}
			}
			else if (SelectionStart > 0 && !masterBill2TextBox.IsEmpty())
			{
				// bubble first char from box 2 back to box 1
				var deletePosition = SelectionStart - 1; // -1 to delete prev. char

				if (deletePosition >= 0)
				{
					Text = Text.Remove(deletePosition, 1);
					Text += masterBill2TextBox.Text[0];
					masterBill2TextBox.Text = masterBill2TextBox.Text.Remove(0, 1);
					SetCaretPositionAt(deletePosition);
				}
			}
			else
			{
				// do a normal backspace (selected chars)
				if (SelectionLength > 0)
				{
					var deletePosition = SelectionStart;
					if (deletePosition >= 0)
					{
						Text = Text.Remove(deletePosition, SelectionLength);
						SetCaretPositionAt(deletePosition);
					}
				}
				// do a normal backspace (nothing selected)
				else if (SelectionStart > 0)
				{
					var deletePosition = SelectionStart - 1;
					if (deletePosition >= 0)
					{
						Text = Text.Remove(deletePosition, 1);
						SetCaretPositionAt(deletePosition);
					}
				}
			}
		}

		protected override void OnKeyPress(KeyPressEventArgs e)
		{
			base.OnKeyPress(e);
			e.Handled = HandleCharPress(e.KeyChar);
		}

		bool HandleCharPress(char c)
		{
			var handled = false;
			if ((!AllowAlphaInMAWP && !IsKeyDigitOrControl(c)) || (AllowAlphaInMAWP && !IsKeyAlphaNumericOrControl(c)))
			{
				// don't allow invalid values
				handled = true;
			}
			else if (!IsControlKey(c) && IsFull() && !masterBill2TextBox.IsFull())
			{
				HandlePushCharactersIntoBox2(c);
			}
			else if (c == (char)8) // BackSpace
			{
				HandleBackSpaceKeyPress();
				handled = true;
			}

			return handled;
		}

		#endregion

		#region TextChanged Event

		// Once box 1 is full (and the caret is at the end), move focus to box 2.
		protected override void OnTextChanged(EventArgs e)
		{
			base.OnTextChanged(e);
			if (IsFull() && SelectionStart == MaxLength)
			{
				masterBill2TextBox.SetCaretPositionAt(0);
			}
		}

		#endregion

	}

	#endregion

	#region OMasterBill2TextBox

	[ToolboxItem(false)]
	public class OMasterBill2TextBox : OMasterBillTextBox
	{
		public OMasterBill2TextBox()
		{
			MaxLength = InitialMaxLength;
		}

		public const int InitialMaxLength = 8;

		public override bool ShouldAggressivelyTruncateText
		{
			get
			{
				return false;
			}
		}

		#region Box 1

		public OMasterBill1TextBox MasterBill1TextBox
		{
			get { return masterBill1TextBox; }
			set { masterBill1TextBox = value; }
		}

		OMasterBill1TextBox masterBill1TextBox;

		#endregion

		#region KeyDown Event

		void HandleLeftKeyDown()
		{
			// move caret to box 1
			masterBill1TextBox.SetCaretPositionAt(masterBill1TextBox.Text.Length);
		}

		void HandleHomeKeyDown()
		{
			// move to start of box 1
			masterBill1TextBox.SetCaretPositionAt(0);
		}

		protected override void OnKeyDown(KeyEventArgs e)
		{
			base.OnKeyDown(e);

			// handle left key
			if (IsCaretAtStart() && e.KeyCode == Keys.Left)
			{
				HandleLeftKeyDown();
				e.Handled = true;
			}
			// handle home key
			else if (e.KeyCode == Keys.Home)
			{
				HandleHomeKeyDown();
				e.Handled = true;
			}
		}

		#endregion

		#region KeyPress Event

		void HandleBackSpaceKeyPress()
		{
			if (IsEmpty())
			{
				// move focus to box 1 and delete the last character
				if (masterBill1TextBox.Text.Length > 0)
				{
					masterBill1TextBox.Text = masterBill1TextBox.Text.Remove(masterBill1TextBox.Text.Length - 1, 1);
				}
				masterBill1TextBox.SetCaretPositionAt(masterBill1TextBox.Text.Length);
			}
			else
			{
				// bubble the first char from box 2 back to box 1
				if (masterBill1TextBox.Text.Length > 0)
				{
					masterBill1TextBox.Text = masterBill1TextBox.Text.Remove(masterBill1TextBox.Text.Length - 1, 1);
				}
				masterBill1TextBox.Text += Text[0];
				Text = Text.Remove(0, 1);
				masterBill1TextBox.SetCaretPositionAt(Math.Max(masterBill1TextBox.Text.Length - 1, 0));
			}
		}

		protected override void OnKeyPress(KeyPressEventArgs e)
		{
			base.OnKeyPress(e);

			// handle invalid values
			if (!IsKeyDigitOrControl(e.KeyChar))
			{
				e.Handled = true;
			}
			// handle backspace key
			else if (e.KeyChar == (char)8 && IsCaretAtStart() && SelectionLength == 0)
			{
				HandleBackSpaceKeyPress();
				e.Handled = true;
			}
		}

		#endregion

		#region Enter / Leave Events

		// Don't let the user click on box 2 unless box 1 is full.
		protected override void OnEnter(EventArgs e)
		{
			base.OnEnter(e);
			if (!MasterBill1TextBox.IsFull())
			{
				OnLeave(e);
				MasterBill1TextBox.SetCaretPositionAt(MasterBill1TextBox.Text.Length);
			}
		}

		#endregion
	}

	#endregion

	#endregion

	[DefaultDataSourceBindingMember(null)]
	[TypeDescriptionProvider(typeof(ZControlTypeDescriptionProvider))]
	[Designer(typeof(Designer))]
	[ToolboxItem(true)]
	[FrontMostControlProvider(typeof(ZMasterBillControlFrontMostControlProvider))]
	public class ZMasterBillControl : OMasterBillNumber, IExtendedControl, IBindTo
	{
		#region Custom Adornment Layout

		class ZMasterBillControlAdornmentLayout : AdornmentLayout<ZMasterBillControl>
		{
			public override IEnumerable<Control> GetBackroundAdornmentTargets(ZMasterBillControl source)
			{
				yield return source.MasterBill1TextBox;
				yield return source.MasterBill2TextBox;
				yield return source.MasterBillOtherTextBox;
			}

			public override IEnumerable<IIconLayout> GetIconAdornmentTargets(ZMasterBillControl source)
			{
				yield return new IconLayout(source.MasterBill2TextBox, IconAlignment.Default);
			}
		}

		#endregion

		#region Designer Support

		internal sealed class Designer : GenericControlDesignerWithTextBoxSnapLine<ZMasterBillControl>
		{
			protected override TextBox GetTextBox(ZMasterBillControl userControl)
			{
				return userControl.MasterBillOtherTextBox;
			}
		}

		#endregion

		#region Constructors

		static ZMasterBillControl()
		{
			NotificationAdornmentFactory.RegisterCustomLayout(new ZMasterBillControlAdornmentLayout());
		}

		public ZMasterBillControl()
		{
			InitializeDisposable();
			InitializeExtensions();
		}

		#endregion

		#region Initialization

		void InitializeExtensions()
		{
			Extensions = new DefaultControlExtensionCollection(this);
		}

		void InitializeDisposable()
		{
			DisposableLeakListener.Instance.RegisterDisposable(this);
		}

		#endregion

		#region ReadOnly

		[DefaultValue(false)]
		public override bool ReadOnly
		{
			get { return MasterBill1TextBox.ReadOnly; }
			set
			{
				if (ReadOnly != value)
				{
					MasterBill1TextBox.ReadOnly = value;
					MasterBill2TextBox.ReadOnly = value;
					SeparatorLabel.BackColor = MasterBill1TextBox.BackColor;
					MasterBillOtherTextBox.ReadOnly = value;
					TabStop = !value;

					if (ReadOnlyChanged != null)
					{
						ReadOnlyChanged(this, EventArgs.Empty);
					}
				}
			}
		}

		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool ReadOnlyIsNull
		{
			get { return false; }
			set { if (value) { ReadOnly = true; } }
		}

		public event EventHandler ReadOnlyChanged;

		#endregion

		#region IExtendedControl Members

		Control IExtendedControl.Host
		{
			get { return this; }
		}

		[Browsable(false)]
		public IControlExtensionCollection Extensions { get; private set; }

		#endregion

		#region IDataBoundControl

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			DataBoundControl.GetDefaultImplementation(this).SetDataBinding(dataSource, dataMember);
			this.dataSource = dataSource;
			this.dataMember = dataMember;
		}

		protected override object DataSourceCore
		{
			get { return dataSource; }
		}
		object dataSource;

		protected override string DataMemberCore
		{
			get { return dataMember; }
		}
		string dataMember;

		#endregion

		#region IBindTo Members

		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public string BindTo
		{
			get { return BindingMemberHelper.BindingMember; }
			set { BindingMemberHelper.BindingMember = value; }
		}

		ControlBindingMemberHelper BindingMemberHelper
		{
			get { return bindingMemberHelper ?? (bindingMemberHelper = ControlBindingMemberHelper.Get(this)); }
		}
		ControlBindingMemberHelper bindingMemberHelper;

		protected Type BindingSourceDataSourceType
		{
			get { return KBindingSource.GetBindingSource(this) == null ? null : KBindingSource.GetBindingSource(this).DataSourceType; }
		}

		#endregion

		#region PropertyDescriptors

		public static PropertyDescriptor[] GetPropertyDescriptors()
		{
			return new ControlPropertyDescriptorBuilder<ZMasterBillControl>()
				.Property<object>("FormattedMasterBill", "")
				.Property("ReadOnly", true)
				.Property("IsVisibleForBinding", ZBool.True)
				.Result;
		}

		#endregion

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
			if (disposing)
			{
				Extensions.Dispose();
				DisposableLeakListener.Instance.UnRegisterDisposable(this);
			}
		}

		protected override void SetVisibleCore(bool value)
		{
			base.SetVisibleCore(value);
			NotificationBroadcaster.Instance.BroadcastVisibilityChange(this);
		}

#if !WINZOR

		protected override void WndProc(ref Message m)
		{
			var handled = false;

			switch (m.Msg) // these are here to handle the controls context menu
			{
				case WindowsMessage.WM_COPY:
				case WindowsMessage.WM_PASTE:
				case WindowsMessage.WM_CUT:
					SendMessageToActiveChildControl(ActiveControl, m.Msg);
					handled = true;
					break;
			}

			if (!handled)
			{
				base.WndProc(ref m);
			}
		}

		void SendMessageToActiveChildControl(Control activeChildControl, int message)
		{
			UnsafeNativeMethods.PostMessage(new HandleRef(activeChildControl, activeChildControl.Handle), message, IntPtr.Zero, IntPtr.Zero);
		}

#endif
	}

	public class ZMasterBillControlFrontMostControlProvider : DefaultFrontMostControlProvider
	{
		public override Control GetFrontMostControl(Control control)
		{
			return control;
		}
	}
}

