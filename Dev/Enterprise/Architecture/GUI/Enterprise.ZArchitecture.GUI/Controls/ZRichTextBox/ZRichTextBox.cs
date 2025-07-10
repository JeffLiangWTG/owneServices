using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Common.Testing;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Interop;
using CargoWise.Interop.DataObjects;
using CargoWise.Schema;
using CargoWise.Types;
using CargoWise.Windows.UI;
using CargoWiseOne.ResourceStrings;
using Enterprise.Core.Forms;
using Enterprise.DocumentScanning.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.GUI.Notifications;
using Enterprise.ZArchitecture.GUI.RichEdit;
#if WINZOR
using System.Threading.Tasks;
using Enterprise.ZArchitecture.GUI.Testing;
using WinzorFramework.JSInterop;
#endif

#pragma warning disable WTG1001 // Do not use the 'private' keyword.

namespace Enterprise.ZArchitecture.GUI
{
#if !WINZOR
	[DefaultBindingProperty("RtfZBlob")]
#else
	[DefaultBindingProperty("HtmlZBlob")]
#endif
	[TypeDescriptionProvider(typeof(ZControlTypeDescriptionProvider))]
	public partial class ZRichTextBox : KUserControl, IPastableControl, IExtendedControl, IBindTo, IHotkeyProvider // this is an architecture control
	{
		#region Component Designer generated code

#if !WINZOR
		internal ZRichTextBoxToolBar RichTextToolBar;
#else
		protected internal ZButton PopupButton;
#endif
		protected Container components;

		void InitializeComponent()
		{
			this.RichEdit = new MyRichTextBox();
#if !WINZOR
			this.RichTextToolBar = new ZRichTextBoxToolBar();
#else
			this.PopupButton = new ZButton();
#endif
			((ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			//
			// RichEdit
			//
			this.RichEdit.AcceptsTab = true;
			this.RichEdit.AllowDrop = true;
			this.RichEdit.Anchor = (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right);

#if !WINZOR
			this.RichEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 32, true);
#else
			this.RichEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
#endif

			this.RichEdit.Name = "RichEdit";
#if !WINZOR
			this.RichEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(488, 328, true);
#else
			this.RichEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(488, 360, true);
#endif
			this.RichEdit.TabIndex = 0;
			this.RichEdit.Text = "";
#if WINZOR
			this.RichEdit.IsToolBarVisible = true;
#endif

#if !WINZOR
			//
			// RichTextToolBar
			//
			this.RichTextToolBar.Dock = System.Windows.Forms.DockStyle.Top;
			this.RichTextToolBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.RichTextToolBar.Name = "RichTextToolBar";
			this.RichTextToolBar.PopupButtonVisible = true;
			this.RichTextToolBar.RichTextBox = null;
			this.RichTextToolBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(488, 32, true);
			this.RichTextToolBar.TabIndex = 1;
			this.RichTextToolBar.TabStop = false;
#else
			//
			// PopupButton
			//
			this.PopupButton.Anchor = (System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right);
			this.PopupButton.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("ZRichTextBoxToolBar|Popup", "Popup");
			this.PopupButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(423, 0, true);
			this.PopupButton.Name = "PopupButton";
			this.PopupButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.PopupButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 23, true);
			this.PopupButton.TabIndex = 1;
			this.PopupButton.ToolTipCaption = null;
			this.PopupButton.Click += new EventHandler(this.PopupButton_Click);

			MissingResourceStringChecker.ExcludeFromTest(PopupButton);
#endif
			//
			// ZRichTextBox
			//
#if !WINZOR
			this.Controls.Add(this.RichTextToolBar);
#else
			this.Controls.Add(this.PopupButton);
#endif
			this.Controls.Add(this.RichEdit);
			this.Name = "ZRichTextBox";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(488, 360, true);
			((ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
		}
		#endregion

		#region Custom Adornment Layout

		class ZRichTextBoxAdornmentLayout : AdornmentLayout<ZRichTextBox>
		{
			public override IEnumerable<Control> GetBackroundAdornmentTargets(ZRichTextBox source)
			{
				yield return source.RichEdit;
			}

			public override IEnumerable<IIconLayout> GetIconAdornmentTargets(ZRichTextBox source)
			{
				yield return new IconLayout(source.RichEdit, IconAlignment.Right);
			}
		}

		#endregion

		#region Constructors

		static ZRichTextBox()
		{
			NotificationAdornmentFactory.RegisterCustomLayout(new ZRichTextBoxAdornmentLayout());
		}

		public ZRichTextBox()
		{
			InitializeComponent();
			InitializeControl();
			InitializeEventHandlers();
			InitializeDisposable();
			InitializeExtensions();

			RegisterHotkeys();

#if WINZOR
			this.PopupButton.AllowOverlap(this.RichEdit);
#endif
		}

		#endregion

		#region Initialization

		void InitializeControl()
		{
			Font = OFont.GetRichTextBoxFont();
			RichEdit.SelectionFont = OFont.GetRichTextBoxFont();

			RichEdit.Outer = this;
			RichEdit.Multiline = true;
			RichEdit.AcceptsTab = true;
			RichEdit.AllowDrop = true;

			fIsToolBarVisible = true;
			fIsPopupButtonVisible = true;

			isAttachButtonVisible = true;
			isInsertImageButtonVisible = true;
#if !WINZOR
			fRichEditOleLocator = ZComRichEditOleInterfaceLocator.GetInstance(RichEdit);
#else
			RichEdit.ImagesInserted += RichTextBox_ImagesInserted;
#endif
			fRichEditDataInserter = new ZRichEditDataInserter(this);

			contextMenuManager = new ZRichTextBoxContextMenuManager(this, RichEdit, new NoMacroBox());
			contextMenuManager.ContextMenuManagerContextMenuStripOpening += ContextMenuManager_ContextMenuManagerContextMenuStripOpening;
			contextMenuManager.ContextMenuStripOpened += new EventHandler(RichEdit_ContextMenuStripOpened);
#if !WINZOR
			RichTextToolBar.AllowOutsideOfParent();
#else
			PopupButton.AllowOutsideOfParent();
#endif
			RichEdit.AllowOutsideOfParent();
		}

		private void ContextMenuManager_ContextMenuManagerContextMenuStripOpening(object sender, EventArgs e)
		{
			Focus();
		}

		public event EventHandler ContextMenuManagerContextMenuStripOpening
		{
			add { contextMenuManager.ContextMenuManagerContextMenuStripOpening += value; }
			remove { contextMenuManager.ContextMenuManagerContextMenuStripOpening -= value; }
		}

		[SuppressMessage("Microsoft.Design", "CA1009:DeclareEventHandlersCorrectly")]
		public event EventHandler<int> OnAddExtraMenuItems
		{
			add { contextMenuManager.OnAddExtraMenuItems += value; }
			remove { contextMenuManager.OnAddExtraMenuItems -= value; }
		}

		void InitializeEventHandlers()
		{
			RichEdit.ReadOnlyChanged += RichEdit_ReadOnlyChanged;
			RichEdit.DragOver += RichEdit_DragOver;
			RichEdit.DragDrop += RichEdit_DragDrop;
			RichEdit.GotFocus += OnRichEdit_GotFocus;
			RichEdit.LostFocus += OnRichEdit_LostFocus;
		}

		void InitializeDisposable()
		{
			DisposableLeakListener.Instance.RegisterDisposable(this);
		}

		void InitializeExtensions()
		{
			Extensions = new DefaultControlExtensionCollection(this);
		}

		#endregion

		[DefaultValue(RichTextBoxScrollBars.Vertical)]
		public RichTextBoxScrollBars ScrollBars
		{
			get => RichEdit.ScrollBars;
			set => RichEdit.ScrollBars = value;
		}

		[DefaultValue(true)]
		public bool WordWrap
		{
			get => RichEdit.WordWrap;
			set => RichEdit.WordWrap = value;
		}

		[DefaultValue(null)]
		public Color? ForcedBackColor
		{
			get => RichEdit.ForcedBackColor;
			set => RichEdit.ForcedBackColor = value;
		}

		public void InsertObject(object data)
		{
			fRichEditDataInserter.InsertObject(data);
		}

		public new void Focus()
		{
			RichEdit.Focus();
		}

#if !WINZOR
		ZRichEditOleItems fOleItems;
		public ZRichEditOleItems OleItems
		{
			get
			{
				if (fOleItems == null)
				{
					fOleItems = new ZRichEditOleItems(RichEdit);
				}
				return fOleItems;
			}
		}
#endif

		#region IsToolBarVisible

		[Category(ZGUIConstants.DesignerCategory)]
		[DefaultValue(true)]
		public bool IsToolBarVisible
		{
			get { return fIsToolBarVisible; }
			set
			{
				fIsToolBarVisible = value;
#if !WINZOR
				this.RichTextToolBar.Visible = value;

				if (value)
				{
					RichEdit.SetBounds(0, RichTextToolBar.Height, Width, Height - RichTextToolBar.Height);
				}
				else
				{
					RichEdit.SetBounds(0, 0, Width, Height);
				}
#else
				this.RichEdit.IsToolBarVisible = value;
				PopupButton.Visible = value && fIsPopupButtonVisible;

				RichEdit.SetBounds(0, 0, Width, Height);
#endif
			}
		}

		bool fIsToolBarVisible;

		#endregion

		#region IsAttachButtonVisible

		[Category(ZGUIConstants.DesignerCategory)]
		[DefaultValue(true)]
		public bool IsAttachButtonVisible
		{
			get => isAttachButtonVisible;
			set
			{
				isAttachButtonVisible = value;
#if !WINZOR
				RichTextToolBar.ToggleAttachButton(value);
#endif
			}
		}

		bool isAttachButtonVisible;

		#endregion

		#region IsInsertImageButtonVisible

		[Category(ZGUIConstants.DesignerCategory)]
		[DefaultValue(true)]
		public bool IsInsertImageButtonVisible
		{
			get => isInsertImageButtonVisible;
			set
			{
				isInsertImageButtonVisible = value;
#if !WINZOR
				RichTextToolBar.ToggleInsertImageButton(value);
#endif
			}
		}

		bool isInsertImageButtonVisible;

		#endregion

		#region IsPopupButtonVisible, Showing the Popup Editor

		[Browsable(true), DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[DefaultValue(true)]
		public virtual bool IsPopupButtonVisible
		{
			get { return fIsPopupButtonVisible; }
			set
			{
				fIsPopupButtonVisible = value;
#if WINZOR
				PopupButton.Visible = value;
#endif
			}
		}

		public virtual void ShowPopupEditor()
		{
			var form = new ZRichTextBoxPopupForm(this);
			form.FinishedEditing += new ZRichTextBoxPopupFormClosed(Form_FinishedEditing);
			using (new RichTextBoxSelectionPreserver(RichEdit))
			{
				CopyRichTextBoxContent(form);

				BeforePopup?.Invoke(this, new RichTextPopupEventArgs(form));
				ZFormModaliser.Show(form, FindForm());
			}
		}

		public void ShowPopupEditorNonModal()
		{
			if (!ReadOnly || !ReadOnlyCascadeToPopup)
			{
				ErrorReporter.ReportOnce(FormattableString.Invariant($"The popup form should only ever be shown non-modally if the rich text box and its popup are readonly. Otherwise you should be using {nameof(ShowPopupEditor)}"));
				return;
			}

			if (nonModalPopupForm != null)
			{
				nonModalPopupForm.Focus();
				return;
			}

			nonModalPopupForm = new ZRichTextBoxPopupForm(this, PopupFormCaption);
			FindForm().FormClosed += CloseNonModalPopupForm;

#if !WINZOR
			RtfZBlobChanged += new EventHandler(CloseNonModalPopupForm);
#else
			HtmlZBlobChanged += new EventHandler(CloseNonModalPopupForm);
#endif

			nonModalPopupForm.FormClosed += (o, e) => nonModalPopupForm = null;
			CopyRichTextBoxContent(nonModalPopupForm);

			nonModalPopupForm.Show();
		}

		private void CloseNonModalPopupForm(object sender, EventArgs e)
		{
			if (nonModalPopupForm != null)
			{
				nonModalPopupForm.Close();
			}

#if !WINZOR
			RtfZBlobChanged -= new EventHandler(CloseNonModalPopupForm);
#else
			HtmlZBlobChanged -= new EventHandler(CloseNonModalPopupForm);
#endif
		}

		void CopyRichTextBoxContent(ZRichTextBoxPopupForm popupForm)
		{
#if !WINZOR
			popupForm.Rtf = Rtf;
#else
			popupForm.Html = Html;
#endif
		}

		ZRichTextBoxPopupForm nonModalPopupForm;

		[DefaultValue(false)]
		public bool ReadOnlyCascadeToPopup { get; set; }

		[DefaultValue("")]
		public ResourceStringData PopupFormCaption { get; set; }

		public event EventHandler<RichTextPopupEventArgs> BeforePopup;

		void Form_FinishedEditing(ZRichTextBoxPopupForm form)
		{
#if !WINZOR
			Rtf = form.Rtf;
#else
			Html = form.Html;
#endif

			RichEdit.Focus();
			form.FinishedEditing -= new ZRichTextBoxPopupFormClosed(Form_FinishedEditing);
			form.Dispose();
		}

#if WINZOR
		void PopupButton_Click(object sender, EventArgs e)
		{
			this.ShowPopupEditor();
		}
#endif

		protected bool fIsPopupButtonVisible;

		#endregion
#if !WINZOR
		#region Rtf / RtfZBlob

#if DEBUG
		internal
#endif
		bool skipForTest
		{ get; set; }

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public string Rtf
		{
			get { return (RichEdit.Rtf == ORtfTextUtil.EmptyRtf) ? "" : RichEdit.Rtf; }
			set
			{
				if (!IsDisposed)
				{
					var selStart = SelectionStart;
					if (string.IsNullOrEmpty(value))
					{
						RichEdit.Rtf = ORtfTextUtil.EmptyRtf;
					}
					else
					{
						if (!ORtfTextUtil.IsRtf(value) && new ZString(value).IsLettersAndNumbersAndPunctuationOnlyOrEmpty && !skipForTest)
						{
							RichEdit.Rtf = ORtfTextUtil.TextToRtf(value);
						}
						else
						{
							RichEdit.Rtf = value;
						}
					}
					if (selStart != -1)
					{
						SelectionStart = selStart;
					}
				}
			}
		}

		[BindingMetaDataProperty(MetaDataTypes.MaxLength, "MaxLength")]
		[BindingMetaDataProperty(MetaDataTypes.ReadOnly, "ReadOnlyForBinding")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public ZBlob RtfZBlob
		{
			get
			{
				//detect case where no actual changes occurred - just regeneration of rtf - and ignore
				//so that we don't have erroneous HasChanges
				if (CurrentRtfBlob.IsEmpty || (Rtf != RichEdit.newRtfSet || CurrentRtfBlob.ToUTF8() != RichEdit.oldRtfSet))
				{
					CurrentRtfBlob = ZBlob.FromUTF8(Rtf);
				}
				return CurrentRtfBlob;
			}
			set
			{
				if (!IsDisposed && !SuppressSetRtf)
				{
					Rtf = value.IsEmpty ? ZString.Empty : value.ToUTF8();

					CurrentRtfBlob = value;
				}
			}
		}

		public event EventHandler RtfZBlobChanged
		{
			add { RichEdit.TextChanged += value; }
			remove { RichEdit.TextChanged -= value; }
		}

		#endregion
#endif
		#region Font

		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public override Font Font
		{
			get { return base.Font; }
			set { base.Font = value; }
		}

		/// <summary>
		/// Get the font that is currently applied to the given selection and will be used when the user
		/// types next. This will never return null.
		/// </summary>
		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public Font ActiveRichTextFont
		{
			get
			{
				var result = RichEdit.SelectionFont;

				// if there is more than 1 font in the selection, just return RichTextBox.Font
				result ??= RichEdit.Font;
				return result;
			}
			set { RichEdit.SelectionFont = value; }
		}

		#endregion

		#region Max Length

		[DefaultValue(0)]
		public int MaxLength
		{
			get { return RichEdit.MaxLength; }
			set
			{
				if (value == -1 || value > 10000000)
				{
					value = 10000000; // TODO by Geoff: make this come from the business layer!!
				}

				RichEdit.MaxLength = value;
			}
		}

		#endregion

		#region Data Selection

		[DefaultValue(0)]
		public int Length
		{
			get { return RichEdit.Text.Length; }
		}

		[DefaultValue(0)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public int SelectionStart
		{
			get { return RichEdit.SelectionStart; }
			set { RichEdit.SelectionStart = value; }
		}

		[DefaultValue(0)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public int SelectionLength
		{
			get { return RichEdit.SelectionLength; }
			set { RichEdit.SelectionLength = value; }
		}

#if !WINZOR
		[DefaultValue(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool SelectionBullet
		{
			get { return RichEdit.SelectionBullet; }
			set { RichEdit.SelectionBullet = value; }
		}

		[DefaultValue(0)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public int SelectionIndent
		{
			get { return RichEdit.SelectionIndent; }
			set { RichEdit.SelectionIndent = value; }
		}

		[DefaultValue(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool SelectionNumberedList
		{
			get { return RichEdit.GetSelectionNumberedList(); }
			set { RichEdit.SetSelectionNumberedList(value); }
		}

		[DefaultValue(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		internal bool SelectionProtected
		{
			get { return RichEdit.SelectionProtected; }
			set { RichEdit.SelectionProtected = value; }
		}

		[DefaultValue("")]
		public string SelectedRtf
		{
			get { return RichEdit.SelectedRtf; }
			set { RichEdit.SelectedRtf = value; }
		}
#endif

		[DefaultValue(typeof(Color), "Black")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public Color SelectionColor
		{
			get { return RichEdit.SelectionColor; }
			set { RichEdit.SelectionColor = value; }
		}

		[DefaultValue("")]
		public string SelectedText
		{
			get { return RichEdit.SelectedText; }
			set { RichEdit.SelectedText = value; }
		}
		#endregion

		#region Undo, Cut, Copy, SelectAll

		public void Undo()
		{
			RichEdit.Undo();
		}

		public void Cut()
		{
			RichEdit.Cut();
		}

		public void Copy()
		{
			RichEdit.Copy();
		}

		public void SelectAll()
		{
			RichEdit.SelectAll();
		}

		#endregion

		#region Binding ReadOnly

		[DefaultValue(false)]
		public bool ReadOnly
		{
			get { return RichEdit.ReadOnly; }
			set
			{
				RichEdit.ReadOnly = value;
#if WINZOR
				PopupButton.ReadOnly = value;
#endif
			}
		}

		void RichEdit_ReadOnlyChanged(object sender, EventArgs e)
		{
			if (RichEdit.ForcedBackColor == null)
			{
				UpdateColor();
			}
			ReadOnlyForBindingProperty.OnReadOnlyChanged();
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("This property is intended for data binding only")]
		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool ReadOnlyForBinding
		{
			get { return ReadOnlyForBindingProperty.ReadOnlyForBinding; }
			set { ReadOnlyForBindingProperty.ReadOnlyForBinding = value; }
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
			get { return false; }
			set
			{
				if (value)
				{
					ReadOnlyForBinding = true;
				}
			}
		}

		protected virtual ControlReadOnlyPropertyHelper ReadOnlyForBindingProperty
		{
			get
			{
				if (readOnlyForBindingProperty == null)
				{
					readOnlyForBindingProperty = new ControlReadOnlyPropertyHelper(
						this,
						() => ReadOnly,
						(bool value) => { ReadOnly = value; });
				}
				return readOnlyForBindingProperty;
			}
		}
		ControlReadOnlyPropertyHelper readOnlyForBindingProperty;

		#endregion

		#region Events

		/// <summary>
		/// When the SelectionStart or SelectionLength has changed.
		/// </summary>
		public event EventHandler SelectionChanged
		{
			add { RichEdit.SelectionChanged += value; }
			remove { RichEdit.SelectionChanged -= value; }
		}

		/// <summary>
		/// Query whether or not to show the selection color. This is used by the toolbar to keep showing
		/// selection when the user drops a combo box.
		/// </summary>
		public event CancelEventHandler QueryAllowControlUnfocus;

		public event EventHandler ReadOnlyChanged
		{
			add { RichEdit.ReadOnlyChanged += value; }
			remove { RichEdit.ReadOnlyChanged -= value; }
		}

		#endregion

		#region Constants

		protected static readonly Guid PhotoEditorComGuid = new Guid("11943940-36de-11cf-953e-00c0a84029e9");

		#region SuppressResourceStringsCheckRegion

		[ThreadStatic]
		static string supportedHyperlinkProtocols;

		protected static string SupportedHyperlinkProtocols
		{
			get
			{
				return supportedHyperlinkProtocols ?? (supportedHyperlinkProtocols = string.Join("", new[]
				{
					"callto:",
					"file:",
					"ftp:",
					"gopher:",
					"http:",
					"https:",
					"mailto:",
					"news:",
					"notes:",
					"nntp:",
					"onenote:",
					"outlook:",
					"prospero:",
					"tel:",
					"telnet:",
					"wais:",
					"webcal:",
					Enterprise.URLHandler.EdiUrlPrefix.Value
				}));
			}
		}

		#endregion

		#endregion

		#region Processing Cmd Keys + Bold / Italic / Underline

		string IHotkeyProvider.TypeNameForDisplay => Res.GetString("6fc8a5fd-77cb-4515-abd0-986b44f8f911", "Text Box");
		public HotkeyRegister Hotkeys { get; } = new HotkeyRegister();

		void RegisterHotkeys()
		{
#if !WINZOR
			StmNoteUserInfoInserter.RegisterHotkeys(this);
			Hotkeys.RegisterHotKey(Keys.Control | Keys.B, ToggleFontStyle(FontStyle.Bold), Res.GetString("fb8546d8-d876-49ac-8dad-c48374a9e6b1", "Bold text"));
			Hotkeys.RegisterHotKey(Keys.Control | Keys.I, ToggleFontStyle(FontStyle.Italic), Res.GetString("30880e83-f13e-4210-8377-8b18f90c9e09", "Italicize text"));
			Hotkeys.RegisterHotKey(Keys.Control | Keys.U, ToggleFontStyle(FontStyle.Underline), Res.GetString("0264f085-c3b5-4a8d-98b7-ba2c90c7a11f", "Underline text"));
			Hotkeys.RegisterHotKey(Keys.Control | Keys.T, ToggleFontStyle(FontStyle.Strikeout), Res.GetString("cab1bddb-dd87-4ef8-a722-e16168104e5f", "Strikeout text"));
#else
			RichEdit.EnableStyleShortcuts = true;
			RichEdit.RegisterF5Inserter(StmNoteUserInfoInserter.GetUserText);
#endif
		}

		protected bool ProcessRichEditCmdKey(ref Message msg, Keys keyData)
		{
			switch (keyData)
			{
#if !WINZOR
				case Keys.Shift | Keys.Tab:
					PerformTab(false);
					return true;

				case Keys.Control | Keys.Tab:
					PerformTab(true);
					return true;

				case Keys.Control | Keys.V:
				case Keys.Shift | Keys.Insert:
					Paste();
					return true;

				case Keys.Control | Keys.Shift | Keys.V:
					PasteTextOnly();
					return true;
#endif
				default:
					return Hotkeys.ProcessCmdKey(this, keyData) || base.ProcessCmdKey(ref msg, keyData);
			}
		}

#if !WINZOR
		Action ToggleFontStyle(FontStyle style)
			=> () => RichTextToolBar.ApplyNewFontStyle(style, !ActiveRichTextFont.Style.HasFlag(style));
#endif

		#endregion

		#region Data Transfer and Clipboard Operations

		protected void DeleteSelection()
		{
#if !WINZOR
			RichEdit.SelectedRtf = "";
#else
			RichEdit.SelectedHtml = "";
#endif
		}

		#endregion

		#region Colors

		protected void OnRichEdit_GotFocus(object sender, EventArgs e)
		{
			if (RichEdit.ForcedBackColor == null)
			{
				UpdateColor();
			}
		}

		protected void OnRichEdit_LostFocus(object sender, EventArgs e)
		{
			if (RichEdit.ForcedBackColor == null)
			{
				UpdateColor();
			}
		}

		protected void UpdateColor()
		{
			var cancelEvent = new CancelEventArgs(false);

			QueryAllowControlUnfocus?.Invoke(this, cancelEvent);

			if ((RichEdit.Focused && !ReadOnly) || cancelEvent.Cancel)
			{
				RichEdit.BackColor = EnterpriseFormLookStrategy.SelectedControlColor;
			}
			else
			{
				RichEdit.BackColor = Color.White; // This is needed for the readonly colour to refresh - cant explain why but Note Security wont display correctly without it.
				RichEdit.BackColor = (ReadOnly) ? SystemColors.Control : SystemColors.Window;
			}
		}

		#endregion

		#region Ctrl-tab and Shift-tab

		protected void PerformTab(bool forward)
		{
			Control parentControl = this;
			IContainerControl parentContainer = null;
			do
			{
				parentContainer = GetParentContainer(parentControl);
				parentControl = (Control)parentContainer;
				if (parentContainer != null)
				{
					FindForm().SelectNextControlNonTabStopNonReadOnly(parentContainer.ActiveControl, forward, true, true);
				}
			}
			while (parentContainer != null && parentContainer.ActiveControl == this);
		}

		protected IContainerControl GetParentContainer(Control childControl)
		{
			var parentContainer = childControl.Parent as IContainerControl;

			if ((parentContainer == null) && (childControl.Parent != null))
			{
				parentContainer = GetParentContainer(childControl.Parent);
			}

			return parentContainer;
		}

		public static string TabTipText
		{
			get { return Res.GetString("e18c1419-02c0-4348-8141-33b0411b7ba7", "Tip: To move to the next field, press 'Ctrl-Tab'. To move to the previous field press 'Ctrl-Shift-Tab'."); }
		}

		#endregion

		#region Drag and Drop

		readonly ISynchronizeInvoke UIThreadSyncInvoke;

		ZForm fParentZForm;
		public ZForm ParentZForm
		{
			get
			{
				if (fParentZForm == null)
				{
					if (UIThreadSyncInvoke is Control)
					{
						fParentZForm = ((Control)UIThreadSyncInvoke).FindForm() as ZForm;
					}

					if (fParentZForm == null)
					{
						fParentZForm = (ZForm)FindForm();
					}
				}
				return fParentZForm;
			}
			set => fParentZForm = value;
		}

		protected internal bool IsEDocsAvailable
		{
			get
			{
				return EDocsPlugIn != null;
			}
		}

		internal IEDocsPlugIn EDocsPlugIn
		{
			get
			{
				if (!checkedForEDocsOnParentForm && ParentZForm != null)
				{
					eDocsPlugIn = ParentZForm.PlugIns.Instances.OfType<IEDocsPlugIn>().FirstOrDefault();
					checkedForEDocsOnParentForm = true;
				}
				return eDocsPlugIn;
			}
		}

		IEDocsPlugIn eDocsPlugIn;
		bool checkedForEDocsOnParentForm;

		protected void RichEdit_DragOver(object sender, DragEventArgs e)
		{
			var control = sender as Control;
			if (control != null)
			{
				DragDropManager.HandleDragOver(control, e);
			}
		}

		protected void RichEdit_DragDrop(object sender, DragEventArgs e)
		{
			var control = sender as Control;
			if (control != null)
			{
				DragDropManager.HandleDragDrop(control, e);
			}
		}

		object GetCurrentlyBoundObject()
		{
			var bindingManager = DataSource == null ? null : BindingContext[DataSource, new KBindingMemberInfo(DataMember).BindingPath];
			return (bindingManager == null || bindingManager.Position == -1) ? null : bindingManager.GetCurrent();
		}

		protected internal DataObjectPastedFileInfo[] WrapDataAndSendToParentForm(ZDataObject data)
		{
			var boundBusinessObject = GetCurrentlyBoundObject() as BusinessObject;
			if (boundBusinessObject != null)
			{
				data.ParentIDLink = boundBusinessObject.PK.ToGuid();
				data.ParentTableLink = ObjectFactory.Get<IApplicationSchemaResolver>().GetColumnNamePrefix(boundBusinessObject.TableName);
			}
			return ZFormPaster.PasteData(ParentZForm, data);
		}

		#endregion

		#region Context Menu

		void RichEdit_ContextMenuStripOpened(object sender, EventArgs e)
		{
			if (!Visible)
			{
				// To fix Issue 0003584
				// Changing from an RTF note to a text only note - then right-clicking in the note and clicking PASTE -
				// Pastes into the RTF rather than the text only
				BeginInvoke(new MethodInvoker(delegate
				{ ((ContextMenuStrip)sender).Visible = false; }));
			}
		}

		#endregion

		#region RichTextBox Sub-class

		[DesignerSerializer(typeof(CargoWise.Windows.UI.Design.ControlDpiScalingCodeDomSerializer), typeof(CodeDomSerializer))]
		protected internal partial class MyRichTextBox : RichTextBox
		{
			public MyRichTextBox()
			{
				devInfoPopupManager = new DevInfoPopupManager(this);
				DisposableLeakListener.Instance.RegisterDisposable(this);
			}

			readonly internal DevInfoPopupManager devInfoPopupManager;

			protected override void Dispose(bool isNotFinalizing)
			{
				if (isNotFinalizing)
				{
					DisposableLeakListener.Instance.UnRegisterDisposable(this);
				}
				if (devInfoPopupManager != null)
				{
					devInfoPopupManager.Dispose();
				}

				base.Dispose(isNotFinalizing);
			}

#if WINZOR
			protected override bool IsInputKey(Keys keyData)
			{
				return ((keyData & Keys.KeyCode) == Keys.Enter) || base.IsInputKey(keyData);
			}

			public override async Task OnInsertImagesAsync(BrowserFile[] images, int selectionStart, int selectionEnd)
			{
				var (validFiles, message) = AcceptableBrowserFileValidator.ValidateFiles(images);
				await base.OnInsertImagesAsync(validFiles, selectionStart, selectionEnd);
				if (!string.IsNullOrEmpty(message))
				{
					await InvokeWinzorDispatcherAsync(() => Globals.Message.ShowWarning(message));
				}
			}
#endif

			[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Exception Message")]
#if !WINZOR
			public new string Rtf
			{
				get { return base.Rtf; }
				set
				{
					try
					{
						//When we do this, differences in generator may cause non-visible changes to Rtf string content. Record change in value so we can detect
						//if any actual changes happened.
						oldRtfSet = value;
						base.Rtf = value;
						newRtfSet = base.Rtf;
					}
					catch (AccessViolationException ex)
					{
						ZRichTextBoxDiagnosticsCollector.Instance.ReportDeveloperErrorWithDiagnostics("AccessViolationInRichTextBoxWndProc", string.Format("There was a AccessViolationException in the setter for RichTextBox.Rtf which was unrecoverable: {0}", ex.Message));
						throw;
					}
					catch (Exception ex1) when (!ex1.IsCriticalException())
					{
						using (var stream = new MemoryStream(System.Text.Encoding.ASCII.GetBytes(value)))
						{
							base.LoadFile(stream, RichTextBoxStreamType.PlainText);
						}
					}
				}
			}

			internal string oldRtfSet;
			internal string newRtfSet;
#endif

			public ZRichTextBox Outer;
			protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
			{
				return Outer.ProcessRichEditCmdKey(ref msg, keyData);
			}

			protected override void OnHScroll(EventArgs e)
			{
				base.OnHScroll(e);
				Invalidate();
			}

			protected override void OnLinkClicked(LinkClickedEventArgs e)
			{
				base.OnLinkClicked(e);

				try
				{
					WebUrlLauncher.Launch(e.LinkText);
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					Globals.Message.ShowError(ex.Message, Res.GetString("4ef6dee0-e7d1-4e53-895d-8710247f02e1", "Could not follow link"));
				}
			}

			protected override void OnVScroll(EventArgs e)
			{
				base.OnVScroll(e);
				Invalidate();
			}

#if !WINZOR
			protected override void WndProc(ref Message m)
			{
				try
				{
					const int EM_AUTOURLDETECT = CargoWise.Interop.WindowsMessage.WM_USER + 91;

					if (m.Msg == CargoWise.Interop.WindowsMessage.WM_LBUTTONDOWN && !this.Visible)
					{
#if NETFRAMEWORK
						m.Result = (IntPtr)0;
#else
						m.Result = 0;
#endif
						return;
					}
					else if (m.Msg == EM_AUTOURLDETECT && m.WParam != IntPtr.Zero)
					{
						var strPtr = Marshal.StringToBSTR(SupportedHyperlinkProtocols);
						try
						{
							m.LParam = strPtr;
							base.WndProc(ref m);
						}
						finally
						{
							Marshal.FreeBSTR(strPtr);
						}
					}
					else if (m.Msg == 0x204e) //WM_REFLECT_NOTIFY. Following code adapted from RichTextBox.cs
					{
						var lParam = (NativeMethods.NMHDR)m.GetLParam(typeof(NativeMethods.NMHDR));
						var code = lParam.code;

						NativeMethods.ENLINK enlink;
						if (IntPtr.Size == 8)
						{
							enlink = UnsafeNativeMethods.ConvertFromENLINK64((NativeMethods.ENLINK64)m.GetLParam(typeof(NativeMethods.ENLINK64)));
						}
						else
						{
							enlink = (NativeMethods.ENLINK)m.GetLParam(typeof(NativeMethods.ENLINK));
						}

						if (code == 0x70b) //means we're about to enter RichTextBox.EnLinkMsgHandler
						{
							if (enlink.msg == 0x201) //means we clicked a link
							{
								var linkTextTemp = this.CharRangeToString(enlink.charrange);
								string linkText;
								do
								{
									linkText = linkTextTemp;
									enlink.charrange.cpMax++;
									linkTextTemp = this.CharRangeToString(enlink.charrange);
								}
								while (linkTextTemp.Length == linkText.Length + 1
									 && linkTextTemp.LastOrDefault() == '-');

								if (!string.IsNullOrEmpty(linkText))
								{
									this.OnLinkClicked(new LinkClickedEventArgs(linkText));
								}
								return;
							}
						}
						base.WndProc(ref m);
					}
					else if (m.Msg == WindowsMessage.WM_PASTE)
					{
						Outer.Paste();
					}
					else
					{
						base.WndProc(ref m);
					}

					if (m.Msg == CargoWise.Interop.WindowsMessage.WM_LBUTTONDOWN)
					{
						base.Focus(); //Hack: For some reason clicking on the richtextbox wouldn't guarantee focus - TST
					}
				}
				catch (AccessViolationException avex)
				{
					ErrorReporter.ReportOnce("AccessViolationRefInRTBWndProc", avex.Message + " on message number " + m.Msg, avex);
				}
				catch (NullReferenceException nrex)
				{
					ErrorReporter.ReportOnce("NullRefInRTBWndProc", nrex.Message + " on message number " + m.Msg, nrex);
				}

				if (m.Msg == WindowsMessage.WM_PAINT)
				{
					using (var g = CreateGraphics())
					{
						OnPaint(new PaintEventArgs(g, ClientRectangle));
					}
				}
			}

			//copied from RichTextBox.cs
			[SuppressMessage("Microsoft.Usage", "CA2201:DoNotRaiseReservedExceptionTypes")]
			private string CharRangeToString(NativeMethods.CHARRANGE c)
			{
				var lParam = new NativeMethods.TEXTRANGE();
				lParam.chrg = c;
				if (/*c.cpMax > this.Text.Length || */c.cpMax - c.cpMin <= 0) //Remove this check. In particular, cpMax can safely be past Text.Length because EM_GETTEXTRANGE seems to have nothing to do with Text.
				{
					return string.Empty;
				}

				var buffer = UnsafeNativeMethods.CharBuffer.CreateBuffer(c.cpMax - c.cpMin + 1);
				var ptr = buffer.AllocCoTaskMem();
				if (ptr == IntPtr.Zero)
				{
					throw new OutOfMemoryException("Out of memory."); //modified message since SR class is private
				}

				lParam.lpstrText = ptr;
				UnsafeNativeMethods.SendMessage(new HandleRef(this, this.Handle), 1099, IntPtr.Zero, lParam);
				buffer.PutCoTaskMem(ptr);
				if (lParam.lpstrText != IntPtr.Zero)
				{
					Marshal.FreeCoTaskMem(ptr);
				}

				return buffer.GetString();
			}

			[DllImport("kernel32.dll", CharSet = CharSet.Auto)]
			static extern IntPtr LoadLibrary(string lpFileName);

			private const string MSFTEDIT_CLASS = "RICHEDIT50W";

			protected override CreateParams CreateParams
			{
				get
				{
					var prams = base.CreateParams;
					if (!DesignModeFinder.IsDesigning && prams.ClassName != MSFTEDIT_CLASS && LoadLibrary("msftedit.dll") != IntPtr.Zero)
					{
						prams.ClassName = MSFTEDIT_CLASS;
					}
					return prams;
				}
			}
#endif

			public Color? ForcedBackColor
			{
				get;
				set;
			}

			public override Color BackColor
			{
				get
				{
					return ForcedBackColor ?? base.BackColor;
				}
				set => base.BackColor = value;
			}
		}

		#endregion

		#region IExtendedControl Members

		Control IExtendedControl.Host
		{
			get { return this; }
		}

		[Browsable(false)]
		public IControlExtensionCollection Extensions { get; private set; }

		#endregion

		#region IDataBoundControl Members

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			var dataBoundControl = DataBoundControl.GetDefaultImplementation(this);
#if !WINZOR
			dataBoundControl.SetDataBinding(dataSource, dataMember);
#else
			dataBoundControl.SetDataBinding(dataSource, dataMember + "_HTML");
			if (currencyManager != null)
			{
				currencyManager.CurrentChanged -= OnTextContextChanged;
			}

			if (dataSource != null)
			{
				var bindingMemberInfo = new KBindingMemberInfo(dataBoundControl.DataMember);
				currencyManager = this.BindingContext[dataBoundControl.DataSource, bindingMemberInfo.BindingPath];
				if (currencyManager != null)
				{
					currencyManager.CurrentChanged += OnTextContextChanged;
				}
			}
#endif
		}

		protected override object DataSourceCore
		{
			get { return DataBoundControl.GetDefaultImplementation(this).DataSource; }
		}

		protected override string DataMemberCore
		{
			get { return DataBoundControl.GetDefaultImplementation(this).DataMember; }
		}

		public override Type DataSourceType
		{
			get { return DataBoundControl.GetDefaultImplementation(this).DataSourceType; }
		}

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

		#region IPastableControl Members

#if !WINZOR
		public bool CanPaste()
		{
			return (Clipboard_GetDataObject() != null);
		}

		public virtual void Paste()
		{
			try
			{
				var nativeData = Clipboard_GetDataObject();
				if (nativeData != null)
				{
					PasteDataObject(nativeData);
				}
			}
			catch (ExternalException ex)
			{
				ErrorReporter.ReportOnce(ex.Message + " HResult=" + ex.ErrorCode, ex);
			}
		}

		public void PasteTextOnly()
		{
			IDataObject nativeData;
			try
			{
				nativeData = Clipboard_GetDataObject();
				if (!RichEdit.ReadOnly && nativeData != null)
				{
					var textData = nativeData.GetData(DataFormats.Text);
					if (textData != null)
					{
						var textToInsert = textData.ToString();
						RichEdit.SelectedText = textToInsert;
					}
				}
			}
			catch (ExternalException ex)
			{
				ErrorReporter.ReportOnce(ex.Message + " HResult=" + ex.ErrorCode, ex);
			}
		}
#endif

		static readonly Regex protectRegex = new Regex(@"(?<!\\)\\protect(?<!a-z)0{0,1}", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled); // non-semantic text
		void PasteDataObject(IDataObject dataObject)
		{
			var maximumLimitSizeInMB = SystemDataRegistry.Instance.eDocsMaximumFilesize.Value;
			using (var data = ZDataObject.FromDataWithMaximumLimitSizeInMB(dataObject, maximumLimitSizeInMB, StorageDocsHelper.GetMaximumLimitSizeNotifications(SystemDataRegistry.Instance.eDocsMaximumFilesize)))
			{
				if (!(data is ISupportInsertFromDragDropOnly))
				{
					if (!data.InfoNeedsToBeNotified.IsNullOrEmpty())
					{
						Globals.Message.ShowWarning(data.InfoNeedsToBeNotified);
					}

					var rtfData = new ZString(data.GetData(DataFormats.Rtf));
					if (protectRegex.IsMatch(rtfData))
					{
						data.SetData(DataFormats.Rtf, true, protectRegex.Replace(rtfData, string.Empty));
					}

					InsertObject(data);
				}
			}
		}

		#endregion

		#region Implementation

		#region Dispose

		[SuppressMessage("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed", MessageId = "contextMenuManager")]
		protected override void Dispose(bool isNotFinalizing)
		{
			if (isNotFinalizing)
			{
				Extensions.Dispose();

				DisposableLeakListener.Instance.UnRegisterDisposable(this);

				if (components != null)
				{
					components.Dispose();
				}

#if !WINZOR
				DisposeIfNotNull(fRichEditOleLocator);
				DisposeIfNotNull(fOleItems);
#else
				RichEdit.ImagesInserted -= RichTextBox_ImagesInserted;
#endif
				DisposeIfNotNull(fRichEditDataInserter);
				DisposeIfNotNull(contextMenuManager);
			}

			base.Dispose(isNotFinalizing);
		}

		void DisposeIfNotNull(IDisposable disposable)
		{
			if (disposable != null)
			{
				disposable.Dispose();
			}
		}

		#endregion
#if !WINZOR
		protected override void OnValidating(CancelEventArgs e)
		{
			this.SuppressSetRtf = true;
			base.OnValidating(e);

			if (e.Cancel)
			{
				this.SuppressSetRtf = false;
			}
		}

		protected override void OnValidated(EventArgs e)
		{
			try
			{
				this.SuppressSetRtf = false;
				base.OnValidated(e);
			}
			catch (InvalidOperationException)
			{
				// this sometimes happens while opening unusual attachments
			}
		}
#endif
		protected override void OnCreateControl()
		{
			base.OnCreateControl();
#if !WINZOR
			RichTextToolBar.RichTextBox = this;
#endif
		}

#if !WINZOR
		protected override void WndProc(ref Message m)
		{
			try
			{
				base.WndProc(ref m);
			}
			catch (NullReferenceException ex)
			{
				throw new ZException("NullReferenceException in WndProc of RichTextBox with message ID " + m.Msg, ex);
			}
		}
#endif

		protected bool SuppressSetRtf
		{
			get
			{
				return suppressSetRtf;
			}
			set
			{
				suppressSetRtf = value;
			}
		}
		bool suppressSetRtf;

#if !WINZOR
		ZComRichEditOleInterfaceLocator fRichEditOleLocator;
		ZBlob CurrentRtfBlob = ZBlob.Empty;
#endif
		ZRichEditDataInserter fRichEditDataInserter;
		protected internal MyRichTextBox RichEdit;
		internal ZRichTextBoxContextMenuManager contextMenuManager;

#if !WINZOR
		protected virtual IDataObject Clipboard_GetDataObject()
		{
			if (!SafeClipboard.Debounce())
			{
				return null;
			}
			return SafeClipboard.GetDataObject();
		}
#endif

		#endregion
#if !WINZOR
		#region PropertyDescriptors

		public static PropertyDescriptor[] GetPropertyDescriptors()
		{
			return new ControlPropertyDescriptorBuilder<ZRichTextBox>()
				.Property("RtfZBlob", ZBlob.Empty)
				.Property("ReadOnly", false, false)
				.Property("ReadOnlyForBinding", false, false)
				.Property("ReadOnlyForBindingIsNull", false, false)
				.Result;
		}

		#endregion
#endif

		protected override void SetVisibleCore(bool value)
		{
			base.SetVisibleCore(value);
			NotificationBroadcaster.Instance.BroadcastVisibilityChange(this);
		}

#if !WINZOR
		public bool TryPaste()
		{
			if (CanPaste())
			{
				Paste();
				return true;
			}
			return false;
		}

#endif

#if DEBUG
#if !WINZOR
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public ZBlob CurrentRtfBlob_Exposed => CurrentRtfBlob;
#endif

		public RichTextBox GetRichTextBoxForTest()
		{
			return RichEdit;
		}

		public void ContextMenuStripOpeningForTest()
		{
			contextMenuManager.InitializeContextMenu();
			RichEdit.ContextMenuStrip.Show();
		}

#endif
	}
}
