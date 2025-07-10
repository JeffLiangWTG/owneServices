using System.Collections.Generic;
using CargoWise.Common;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.BufferManagement.Business
{
	class WorkflowAdditionalNoteProvider : IAdditionalNoteProvider
	{
		readonly IAdditionalNoteProvider baseProvider;
		readonly ProcessHeader processHeader;

		public WorkflowAdditionalNoteProvider(IAdditionalNoteProvider baseProvider, ProcessHeader processHeader)
		{
			this.baseProvider = baseProvider;
			this.processHeader = processHeader;
		}

		public IEnumerable<StmNote> AdditionalNotes
		{
			get
			{
				var workflowNote = WorkflowStmNote.GetForParent(processHeader);
				return workflowNote == null ? baseProvider.AdditionalNotes : baseProvider.AdditionalNotes.Append(workflowNote);
			}
		}
	}
}
