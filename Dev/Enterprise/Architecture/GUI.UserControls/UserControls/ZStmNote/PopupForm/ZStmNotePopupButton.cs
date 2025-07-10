using System;
using System.ComponentModel;
using System.Drawing.Design;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Internal;
using Res = Enterprise.ZArchitecture.GUI.UserControls.Res;

namespace Enterprise.ZArchitecture.GUI
{
	public class ZStmNotePopupButton : ZButton
	{
		public ZStmNotePopupButton() => Click += delegate { }; // basher expects all buttons to have a Click event hooked

		public event ZStmNotePopupForm.NoteHasChangesEventHandler NoteHasChangesChanged;

		#region Create New Note if Not Exists

		[Category(ZGUIConstants.DesignerCategory)]
		[DefaultValue(CreateNewNoteIfNotFoundDefault)]
		public bool CreateNewNoteIfNotFound { get; set; } = CreateNewNoteIfNotFoundDefault;

		const bool CreateNewNoteIfNotFoundDefault = true;

		#endregion

		#region No Note Exists Error

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public ZString NoNoteExistsError { get; set; } = NoNoteExistsErrorDefault;

		static string NoNoteExistsErrorDefault => Res.GetString("d59309fe-bb53-4d3e-8cbc-25226a103050", "The note could not be found.");

		#endregion

		#region NoteType

		[Category(ZGUIConstants.DesignerCategory)]
		[Editor(typeof(PredefinedNoteTypeEditor), typeof(UITypeEditor))]
		public string NoteType
		{
			get => fNoteType;
			set
			{
				if (value != null)
				{
					if (NoteTypeFromDescription(value) == null)
					{
						throw new Exception("Select an item from the list.");
					}
				}
				fNoteType = value;
			}
		}

		PredefinedNoteType NoteTypeFromDescription(string description) => PredefinedNoteTypes.Instance.NoteTypeByDescription(description);

		string fNoteType;

		#endregion

		#region Showing the Note Popup

		protected override void OnClick(EventArgs e)
		{
			base.OnClick(e);
			ShowNoteForm();
		}

		void ShowNoteForm()
		{
			var resetHasChangesIfNoteCancelled = !((BusinessObject)BizO).HasChanges;
			var deleteNoteIfNoteCancelled = false;

			var noteTypeToEdit = NoteTypeFromDescription(NoteType);
			var noteToEdit = CurrentNote;

			if (noteToEdit == null)
			{
				if (CreateNewNoteIfNotFound)
				{
					noteToEdit = BizO.Notes.AddNew();
					noteToEdit.ST_Description = noteTypeToEdit.Description;
					noteToEdit.ST_NoteType = noteTypeToEdit.DefaultVisibility.ToString();
					noteToEdit.ST_IsCustomDescription = false;
					deleteNoteIfNoteCancelled = true;
				}
				else
				{
					_ = Globals.Message.Show(NoNoteExistsError, Res.GetString("3d839a48-b3b6-451c-812a-884e46e708c2", "No Note Found"), MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
				}
			}

			if (noteToEdit != null)
			{
				var notePopupForm = new ZStmNotePopupForm(noteToEdit, BizO, resetHasChangesIfNoteCancelled, deleteNoteIfNoteCancelled);
				notePopupForm.NoteHasChangesChanged += new ZStmNotePopupForm.NoteHasChangesEventHandler(NotePopupForm_NoteHasChangesChanged);
				ZFormModaliser.Show(notePopupForm, FindForm());
			}
		}

		void NotePopupForm_NoteHasChangesChanged() => NoteHasChangesChanged?.Invoke();

		IStmNoteParent BizO
			=> FindForm() is ZForm parentForm
				? parentForm.BusinessEntity as IStmNoteParent
				: null;

		StmNote CurrentNote
			=> NoteType != null && BizO != null
				? BizO.Notes.FindByDescription(NoteType).FirstOrDefault(x => !x.IsNull && x.IsBelongingToCurrentLoginCompany)
				: null;

		#endregion
	}
}
