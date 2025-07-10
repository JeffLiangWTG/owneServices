using CargoWise.Definitions;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.Business.Testing
{
	sealed class HiddenTextNoteExposed : HiddenTextNote
	{
		public HiddenTextNoteExposed(IStmNoteParent parent)
			: base(parent)
		{
		}

		public ZQuery FilterExposed
		{
			get
			{
				ZQuery result = new ZQuery();

				result.AddToFilter(StmNoteSchema.ST_ParentID, Parent.NotesParentPK);
				result.AddToFilter(StmNoteSchema.ST_Table, Parent.NotesParentTableName);
				result.AddToFilter(StmNoteSchema.ST_Description, Description);
				result.AddToFilter(StmNoteSchema.ST_NoteType, nameof(StmNoteVisibility.DOC));

				return result;
			}
		}
	}
}
