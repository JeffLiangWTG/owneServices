using System;
using System.Collections.Generic;
using CargoWise.PAVE.Common.DTO;

namespace Enterprise.BufferManagement.Service.Shared
{
	public class AddNewTaskResponse
	{
		public Guid? NewTaskId { get; set; }
	}

	public class TryAddNewTaskResult
	{
		public TryAddNewTaskResult(Guid? taskId)
		{
			Response = new AddNewTaskResponse { NewTaskId = taskId };
		}

		public TryAddNewTaskResult(IEnumerable<string> validationErrorMessages)
		{
			Error = new PaveError(PaveErrorCode.ValidationError, validationErrorMessages);
		}

		public AddNewTaskResponse Response { get; }

		public PaveError Error { get; }

		public bool Success => Error == null;
	}
}
