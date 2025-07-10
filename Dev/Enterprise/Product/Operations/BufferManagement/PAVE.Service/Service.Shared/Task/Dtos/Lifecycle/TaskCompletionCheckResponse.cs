using System.Diagnostics.CodeAnalysis;
using CargoWise.PAVE.Common.DTO;

namespace Enterprise.BufferManagement.Service.Shared.Task.Dtos.Lifecycle
{
	public class TaskCompletionCheckResponse
	{
		public bool IsContainmentBarrierAnswerRequired { get; set; }

		[SuppressMessage("CargoWiseOne", "CW1050:PropertyNamingRule", Justification = "Property name is consistent with existing naming conventions.")]
		public int? ActualDurationInMinutes { get; set; }

		public bool IsActualDurationRequired { get; set; }

		public ContainmentBarrierRequiredDTO ContainmentBarrierDetails { get; set; }
	}
}
