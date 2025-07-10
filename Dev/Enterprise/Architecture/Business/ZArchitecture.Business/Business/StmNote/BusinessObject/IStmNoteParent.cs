using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.ZArchitecture.Business
{
	public interface IStmNoteParent
	{
		bool IsInDatabase { get; }
		bool IsDeleted { get; }
		Notes Notes { get; }
		ZGuid NotesParentPK { get; }
		string NotesParentTableName { get; }
		BusinessObjectFactory NotesFactory { get; }
		GetValueDelegate<NoteTypeCollection> CustomNoteTypesDelegate { get; set; }

		bool SupportsNotes { get; }
		BusinessObject[] BusinessObjectsWithRelatedNotes { get; }
		StmNoteContexts NoteContextsForRelatedNotes { get; }
		NoteTypeCollection NoteTypes { get; }
	}

	public interface IStmNoteParentWithSystemNote : IStmNoteParent
	{
		bool IsSystemNote(StmNote note);
	}
}
