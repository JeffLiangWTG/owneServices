using Enterprise.ZArchitecture.Business;

namespace Enterprise.BufferManagement.Business
{
	public class WorkflowStmNoteValidation : StmNoteValidation
	{
		public WorkflowStmNoteValidation(WorkflowStmNote parent)
			: base(parent)
		{
		}

		protected override void CheckST_NoteData()
		{
			// We allow empty notes - the record is delete in ProcessHeader.OnSaving
		}
	}
}
