using System;
using System.ComponentModel;
using System.Drawing.Design;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI.Internal;

namespace Enterprise.ZArchitecture.GUI
{
	[ToolboxItem(false)]
	[TypeDescriptionProvider(typeof(ZControlTypeDescriptionProvider))]
	[DefaultBindingProperty("DataSource")]
	public partial class ZStmNotePopupBase : ZUserControl, IExtendedControl
	{
		protected const int FixedHeight = 22;
		protected const int DefaultWidth = 200;
		protected const int DefaultButtonWidth = 52;
		protected const int DefaultTextBoxWidth = DefaultWidth - DefaultButtonWidth;

		#region Constructors

		public ZStmNotePopupBase()
		{
			InitializeComponent();
			InitializeControl();
#if DEBUG
			TypeDescriptor.AddAttributes(popupButton, new SuppressFormsLocalizedTestAttribute());
#endif
		}

		#endregion

		#region Initialization

		void InitializeControl()
		{
			ControlDpiScalingHelper.SetWidth(ref popupButton, DefaultButtonWidth, true);
			ControlDpiScalingHelper.SetWidth(this, DefaultWidth, true);
			Extensions = new DefaultControlExtensionCollection(this);
			popupButton.AllowOutsideOfParent();
		}

		#endregion

		#region Properties

		#region Button Text

		[Category(ZGUIConstants.DesignerCategory)]
		[DefaultValue("")]
		public string ButtonText
		{
			get => popupButton.Text;
			set => popupButton.Text = value;
		}

		#endregion

		#region Button Width

		[Category(ZGUIConstants.DesignerCategory)]
		public int ButtonWidth
		{
			get => popupButton.Width;
			set
			{
				if (value > Width)
				{
					ControlDpiScalingHelper.SetWidth(this, value, false);
				}
				ControlDpiScalingHelper.SetWidth(ref popupButton, value, false);
			}
		}

		#endregion

		#region NoteType

		[Category(ZGUIConstants.DesignerCategory)]
		[Editor(typeof(PredefinedNoteTypeEditor), typeof(UITypeEditor))]
		[Description("Select a Predefined Note Type from the available list of TextOnly notes.")]
		public string NoteTypeDescription
		{
			get => (NoteType != null) ? NoteType.Description : "";
			set
			{
				if (value != null) // when dropping the list in the designer and cancelling, this value will be null (nfi why, not worth investigating :P)
				{
					NoteType = PredefinedNoteTypes.Instance.NoteTypeByDescription(value);

					if (NoteType == null)
					{
						throw new ArgumentException("Please select a Predefined Note Type from the available list.");
					}
				}
			}
		}

		PredefinedNoteType NoteType;

		#endregion

		#endregion

		#region PerformButtonClick

		public void PerformButtonClick() => popupButton.PerformClick();

		#endregion

		#region Note

		protected ZString NoteText
		{
			get => Note.ST_NoteDataAsText;
			set => Note.ST_NoteDataAsText = value;
		}

		protected bool NoteExists => Note != null;

		protected StmNote Note
		{
			get
			{
				StmNote result = null;

				if (NoteType != null && BusinessObject != null)
				{
					//#warning remove call to GetAllNotes() once Notes.Find() bug is fixed
					_ = BusinessObject.Notes.GetAllNotes();
					var notes = BusinessObject.Notes.FindByDescription(NoteType.Description);
					if (notes.Length > 0)
					{
						result = notes[0];
					}
				}

				return result;
			}
		}

		protected void CreateNewNoteIfNotExists()
		{
			if (!NoteExists)
			{
				var newNote = BusinessObject.Notes.AddNew();
				newNote.ST_Description = NoteType.Description;
				newNote.ST_NoteType = NoteType.DefaultVisibility.ToString();
				newNote.ST_IsCustomDescription = false;
				newNote.NoteTextMaxLength = MaximumNoteLength ?? -1;
			}
		}

		public int? MaximumNoteLength
		{
			get => maximumNoteLength;
			set
			{
				maximumNoteLength = value;
				SetNoteTextMaxLength();
			}
		}
		int? maximumNoteLength;

		void SetNoteTextMaxLength()
		{
			if (MaximumNoteLength.HasValue)
			{
				var note = Note;
				if (note != null)
				{
					note.NoteTextMaxLength = MaximumNoteLength.Value;
				}
			}
		}

		#endregion

		#region Showing the Editor

		void PopupButton_Click(object sender, EventArgs e) => _ = ShowEditor();

		protected override bool ProcessDialogKey(Keys keyData)
		{
			bool result;

			if (keyData == Keys.F3)
			{
				_ = ShowEditor();

				result = true;
			}
			else
			{
				result = base.ProcessDialogKey(keyData);
			}

			return result;
		}

		protected ZStmNotePopupForm ShowEditor()
		{
			var notePopupForm = GetNewPopupFormWithNote();
			HookPopup(notePopupForm);
			ZFormModaliser.Show(notePopupForm, FindForm());
			notePopupForm.Focus();

#if DEBUG
			LastShownPopup = notePopupForm;
#endif

			return notePopupForm;
		}

#if DEBUG

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public ZStmNotePopupForm LastShownPopup
		{
			[System.Diagnostics.DebuggerStepThrough]
			get;
			[System.Diagnostics.DebuggerStepThrough]
			set;
		}

#endif

		protected void ShowEditorAndSetCaretLocation(int location)
		{
			var notePopupForm = ShowEditor();
			notePopupForm.SetCaretLocationAfterBindingHasFinished(location);
		}

		protected virtual void HookPopup(ZStmNotePopupForm notePopupForm)
		{
		}

		ZStmNotePopupForm GetNewPopupFormWithNote()
		{
			var resetHasChangesOnCancelEdit = !((BusinessObject)BusinessObject)?.HasChanges ?? false;
			var deleteNoteOnCancelEdit = false;

			if (!NoteExists)
			{
				CreateNewNoteIfNotExists();
				deleteNoteOnCancelEdit = true;
			}

			return new ZStmNotePopupForm(Note, BusinessObject, resetHasChangesOnCancelEdit, deleteNoteOnCancelEdit);
		}

		#endregion

		#region SetBoundsCore

		protected override void SetBoundsCore(int x, int y, int width, int height, BoundsSpecified specified) => base.SetBoundsCore(x, y, width, ControlDpiScalingHelper.ScaleToCurrentDpiY(FixedHeight), specified);

		#endregion

		#region Binding ReadOnly

		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool ReadOnly
		{
			get => readOnly;
			set
			{
				readOnly = value;
				OnReadOnlyChanged(EventArgs.Empty);
			}
		}
		bool readOnly;

		protected virtual void OnReadOnlyChanged(EventArgs e) => ReadOnlyForBindingProperty.OnReadOnlyChanged();

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

		#region PropertyDescriptors

		public static PropertyDescriptor[] GetPropertyDescriptors()
			=> new ControlPropertyDescriptorBuilder<ZStmNotePopupBase>()
				.Property("ReadOnly", false, false)
				.Property("ReadOnlyForBinding", false, false)
				.Property("ReadOnlyForBindingIsNull", false, false)
				.Property("DataSource", (object)null, false)
				.Result;

		#endregion

		#region IDataBoundControl Members

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			this.dataSource = dataSource;
			this.dataMember = dataMember;
			DataBoundControl.SetDataBindingForMetadataProperties(this, dataSource, dataMember);

			SetNoteTextMaxLength();
		}

		[BindingMetaDataProperty(MetaDataTypes.ReadOnly, "ReadOnlyForBinding")]
		public new object DataSource => base.DataSource;

		protected override object DataSourceCore => dataSource;
		object dataSource;

		protected override string DataMemberCore => dataMember;
		string dataMember;

		IStmNoteParent BusinessObject
		{
			get
			{
				var bindingManager = DataSource == null ? null : BindingContext[DataSource, new KBindingMemberInfo(DataMember).BindingPath];
				return (bindingManager == null || bindingManager.Position == -1) ? null : bindingManager.GetCurrent() as IStmNoteParent;
			}
		}

		#endregion

		#region IExtendedControl Members

		Control IExtendedControl.Host => this;

		[Browsable(false)]
		public IControlExtensionCollection Extensions { get; private set; }

		#endregion

		#region Overrides

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				Extensions.Dispose();
			}
			base.Dispose(disposing);
		}

		#endregion
	}
}
