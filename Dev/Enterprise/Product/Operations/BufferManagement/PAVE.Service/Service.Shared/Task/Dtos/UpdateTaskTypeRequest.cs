namespace Enterprise.BufferManagement.Service.Shared.Task.Dtos
{
	public class UpdateTaskTypeRequest
	{
		public string PreviousType { get; set; } = null!;

		public string NewType { get; set; }
	}
}
