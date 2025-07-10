using System.ComponentModel;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.Core.Forms;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.GUI
{
	[ToolboxItem(false)]
	[TypeDescriptionProvider(typeof(ZControlTypeDescriptionProvider))]
	public partial class ZStmNoteUserControl : ZUserControl, IStmNoteControl
	{
		public ZStmNoteUserControl()
		{
			InitializeComponent();

			Dock = DockStyle.Fill;
			NoteGrid.BindTo = BindToNotes;
			ShowRelatedNotesCheckBox.BindTo = BindToShowRelatedNotes;
			NoteRichTextBox.SetBindingMember(BindToNotes);
			ShowNotesForAllCompaniesCheckBox.BindTo = BindToShowNotesForAllCompanies;
			ShowNotesForAllCompaniesCheckBox.GotFocus += ShowNotesForAllCompaniesCheckBox_Click;
			UserEventTracker.Instance.AddUserEventToControl(this);

			ShowRelatedNotesCheckBox.AllowOverlap(NoteRichTextBox);
			ShowNotesForAllCompaniesCheckBox.AllowOverlap(NoteRichTextBox);
			ShowNotesForAllCompaniesCheckBox.AllowOutsideOfParent();
		}

		#region ShowNotesForAllCompanies

		void ShowNotesForAllCompaniesCheckBox_Click(object sender, System.EventArgs e)
		{
			if (ShowNotesForAllCompaniesCheckBox.ReadOnly && ParentForm is ZForm parentForm)
			{
				parentForm.MessageStatusBarPanel.Text = Enterprise.ZArchitecture.GUI.UserControls.Res.GetString("8B388743-1024-4C57-985E-AEEBE6E5694A", "You don't have permission to view notes for all companies.");
			}
		}

		#endregion

		#region OnLoad

		protected override void OnLoad(System.EventArgs e) => ShowNotesForAllCompaniesCheckBox.Checked = false;

		#endregion

		#region Description Column

		public void RemoveDescriptionColumn()
		{
			ZGridColumnInfo descriptionColumnInfo = null;

			foreach (ZGridColumnInfo columnInfo in NoteGrid.ColumnStyles)
			{
				if (columnInfo is ZNoteDescriptionColumnStyleInfo)
				{
					descriptionColumnInfo = columnInfo;
					break;
				}
			}
			NoteGrid.ColumnStyles.Remove(descriptionColumnInfo);
		}

		#endregion

		#region Note Context

		internal void RemoveNoteContext()
		{
			RemoveNoteContextColumn("ST_NoteContextModuleCaption");
			RemoveNoteContextColumn("ST_NoteContextDirectionCaption");
			RemoveNoteContextColumn("ST_NoteContextFreightModeCaption");
		}

		void RemoveNoteContextColumn(string columnName)
		{
			var columnStyle = NoteGrid.GetColumnStyle(columnName);
			if (columnStyle != null)
			{
				NoteGrid.ColumnStyles.Remove(columnStyle);
			}
		}

		#endregion

		#region Selecting Tab and Note

		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public StmNote SelectedNote
		{
			get => (StmNote)NoteGrid.ListManager?.GetCurrent();
			set
			{
				if (NoteGrid.ListManager != null)
				{
					for (var i = 0; i < NoteGrid.ListManager.Count; i++)
					{
						if (((StmNote)NoteGrid.ListManager.List[i]).PK == value.PK)
						{
							NoteGrid.ListManager.Position = i;
							break;
						}
					}
				}
			}
		}

		public void FocusOnTabPage()
		{
			Control control = this;
			while (control != null)
			{
				if (control is ZTabPage tab && control.Parent != null)
				{
					((ZTabControl)control.Parent).SelectedTab = tab;
				}
				control = control.Parent;
			}
			_ = Focus();
		}

		#endregion

		#region Parent Tab

		internal ZStmNoteTabPage ParentStmTab => Parent as ZStmNoteTabPage;

		#endregion

		#region Notes Icon

		void Notes_CountChanged(object sender, CollectionCountChangedEventArgs e) => ParentStmTab?.UpdateNoteImage();

		#endregion

		#region ReadOnly

		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool ReadOnly
		{
			get => NoteGrid.ReadOnly && NoteRichTextBox.ReadOnly;
			set
			{
				NoteGrid.ReadOnly = value;
				NoteRichTextBox.ReadOnly = value;
			}
		}

		#endregion

		#region PropertyDescriptors

		public static PropertyDescriptor[] GetPropertyDescriptors() => new ControlPropertyDescriptorBuilder<ZStmNoteUserControl>().Result;

		#endregion

		#region IDataBoundControl Members

		protected override void OnCurrentDataItemChanging(System.EventArgs e)
		{
			base.OnCurrentDataItemChanging(e);
			if (CurrentDataItem is BusinessObject businessObject && !businessObject.IsDeleted)
			{
				businessObject.GetNotes().VisibleNotes.CountChanged -= Notes_CountChanged;
			}
		}

		protected override void OnCurrentDataItemChanged(System.EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);
			if (CurrentDataItem is BusinessObject businessObject)
			{
				if (ParentStmTab != null)
				{
					businessObject.GetNotes().VisibleNotes.CountChanged += Notes_CountChanged;
				}
				if (((IStmNoteParent)businessObject).NoteTypes.Count > 0)
				{
					AddIsCustomDescriptionCheckBox();
				}
				NoteGrid.RemoveAction = businessObject.GetNotes().HasSecurityToDeleteNotes ? RemoveAction.RemoveAndDelete : RemoveAction.NoRemovePossible;
				SecurityPanel.Visible = !businessObject.GetNotes().HasAllNoteSecurity;
			}
		}

		void AddIsCustomDescriptionCheckBox()
		{
			var info = new ZCheckBoxColumnStyleInfo(StmNoteSchema.Constants.ST_IsCustomDescription, 70)
			{
				IsMandatory = true
			};

			NoteGrid.ColumnStyles.Insert(1, info);
		}

		public const string BindToNotes = "Notes+VisibleNotes";
		public const string BindToShowRelatedNotes = "Notes+ShowRelatedNotes";
		public const string BindToShowNotesForAllCompanies = "Notes+ShowNotesForAllCompanies";

		#endregion

		#region IStmNoteControl Members

		public void ShowMessage(string caption, string message) => Globals.Message.ShowWarning(message, caption);

		#endregion
	}
}
