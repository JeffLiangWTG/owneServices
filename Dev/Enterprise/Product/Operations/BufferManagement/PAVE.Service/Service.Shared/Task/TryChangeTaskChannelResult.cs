using System.Collections.Generic;
using CargoWise.PAVE.Common.DTO;

namespace Enterprise.BufferManagement.Service.Shared.Task
{
	public class TryChangeTaskChannelResult
	{
		public TryChangeTaskChannelResult()
		{
		}

		public TryChangeTaskChannelResult(IEnumerable<string> validationErrorMessages)
		{
			Error = new PaveError(PaveErrorCode.ValidationError, validationErrorMessages);
		}

		public TryChangeTaskChannelResult(ContainmentBarrierRequiredDTO containmentBarrierRequired, IEnumerable<string> errorMessages)
		{
			ContainmentBarrierRequired = containmentBarrierRequired;
			Error = new PaveError(PaveErrorCode.IterationIsRequired, errorMessages);
		}

		public TryChangeTaskChannelResult(IEnumerable<ChangeTaskChannelAction> actions)
		{
			Actions = actions;
		}

		public PaveError Error { get; }

		public ContainmentBarrierRequiredDTO ContainmentBarrierRequired { get; }

		public bool Success => Error == null;

		public IEnumerable<ChangeTaskChannelAction> Actions { get; set; }
	}

	public class ChangeTaskChannelAction
	{
		public string Text { get; set; }
		public string Tooltip { get; set; }
		public TaskChangeChannelMethod Method { get; set; }
		public bool HideFormBeforeFireAction { get; set; }
	}
}
