using CargoWise.EntityFramework;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Services.OperationalActions.Module
{
	public class ShowEditNoteActionMethodApplicator : OperationalActionMethodApplicator, IObsoleteValidation
	{
		public ShowEditNoteActionMethodApplicator(ShowEditNoteActionMethodSettings settings, BusinessObjectFactory factory)
			: base(Res.GetString("f1810efd-91cd-4ad4-92a3-43d70ba57ea1", "View/Edit Note"), factory)
		{
			this.settings = settings;
		}

		readonly ShowEditNoteActionMethodSettings settings;

		protected override void ApplyCore(IOperationalActionSectionLog log, BusinessObject[] targets)
		{
			if (targets.Length == 0)
			{
				log.Notify(OperationalActionLogErrorLevel.Warning, Res.GetString("d1d9055d-7743-47e2-9ccb-fd7fd5b10894", "No records selected."));
			}
			else
			{
				foreach (var target in targets)
				{
					var notes = GetNotesFromTarget(log, target);
					if (notes.Length == 0)
					{
						log.Notify(OperationalActionLogErrorLevel.Warning, Res.GetString("d19a565c-6048-4c0c-90d6-d8fe873b4022", "No note found for {0}.", target.HumanReadableName));
					}
					else
					{
						foreach (var note in notes)
						{
							new PopupNoteEditor(note).Show();
						}
					}
				}
			}
		}

		StmNote[] GetNotesFromTarget(IOperationalActionSectionLog log, BusinessObject target)
		{
			var notesParent = target as IStmNoteParent;
			if (notesParent == null)
			{
				log.Notify(OperationalActionLogErrorLevel.Warning, Res.GetString("8be8e2f3-f72a-4b61-a7ec-b7aa2497df59", "{0} is not a notes parent.", target.HumanReadableName));
				return System.Array.Empty<StmNote>();
			}

			return notesParent.Notes.FindByDescription(settings.NoteDescription);
		}

		class PopupNoteEditor
		{
			public PopupNoteEditor(StmNote note)
			{
				this.note = note;
			}

			readonly StmNote note;
			ZStmNotePopupForm popupForm;

			public void Show()
			{
				popupForm = GetEditablePopupForm();
				popupForm.ConfirmCancelChanges = true;
				var noteMasterBizo = note.Master as BusinessObject;
				if (noteMasterBizo != null)
				{
					popupForm.FormHeadingPrefix = noteMasterBizo.HumanReadableName;
				}

#if DEBUG
				if (Globals.IsTest)
				{
					ShownPopup = true;
				}
				else
#endif
				{
					popupForm.ShowDialog();
				}

				popupForm.Dispose();
			}

			ZStmNotePopupForm GetEditablePopupForm()
			{
				return new ZStmNotePopupForm(note, false, false);
			}
		}

#if DEBUG
		static internal bool ShownPopup;
#endif
	}
}
