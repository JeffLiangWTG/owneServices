using CargoWise.EntityFramework;

namespace Enterprise.ZArchitecture.Business.Testing
{
	sealed class NoteForTest : StmNote
	{
		readonly bool _saved;
		readonly bool _deleted;
		readonly bool _hasChanges;
		public NoteForTest(StmNote factoryNote, bool isSaved, bool isDeleted = false, bool hasChanges = false) : base(factoryNote.Factory, ((IBusinessObjectInternals)factoryNote).Row)
		{
			_saved = isSaved;
			_deleted = isDeleted;
			_hasChanges = hasChanges;
		}

		public override bool HasChanges { get => _hasChanges; set => base.HasChanges = value; }
		public override bool IsInDatabase { get => _saved; }
		public override bool IsDeleted { get => _deleted; }
	}
}
