using System.Collections.Generic;
using CargoWise.PAVE.Common.DTO;

namespace Enterprise.BufferManagement.Service.Shared
{
	public class TryUpdateNotesResult
	{
		public TryUpdateNotesResult(TaskNotesResponse taskNotesResponse)
		{
			TaskNotesResponse = taskNotesResponse;
		}

		public TryUpdateNotesResult(IEnumerable<string> validationErrorMessages)
		{
			Error = new PaveError(PaveErrorCode.ValidationError, validationErrorMessages);
		}

		public TaskNotesResponse TaskNotesResponse { get; }

		public PaveError Error { get; }

		public bool Success => Error == null;
	}
}
