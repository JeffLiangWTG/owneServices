using System.Collections.Generic;
using CargoWise.PAVE.Common.DTO;
using Enterprise.BufferManagement.Service.Shared.Task.Dtos;

namespace Enterprise.BufferManagement.Service.Shared
{
	public class TryUpdateTaskDescriptionResult
	{
		public TryUpdateTaskDescriptionResult(TaskDescriptionResponse response)
		{
			TaskDescriptionResponse = response;	
		}

		public TryUpdateTaskDescriptionResult(IEnumerable<string> validationErrorMessages)
		{
			Error = new PaveError(PaveErrorCode.ValidationError, validationErrorMessages);
		}

		public TaskDescriptionResponse TaskDescriptionResponse { get; }

		public PaveError Error { get; }

		public bool Success => Error == null;
	}
}
