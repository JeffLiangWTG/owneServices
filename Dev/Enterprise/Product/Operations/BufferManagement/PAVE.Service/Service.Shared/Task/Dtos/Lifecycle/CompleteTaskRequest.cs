using CargoWise.PAVE.Common.DTO;

namespace Enterprise.BufferManagement.Service.Shared.Task.Dtos.Lifecycle
{
	public class CompleteTaskRequest
	{
		//TODO: create a ContainmentBarrierAnswerDto here and remove ContainmentBarrierRequestDTO from shared
		public ContainmentBarrierRequestDTO ContainmentBarrierAnswer { get; set; }

		public int? ActualDurationInMinutes { get; set; }
	}
}
