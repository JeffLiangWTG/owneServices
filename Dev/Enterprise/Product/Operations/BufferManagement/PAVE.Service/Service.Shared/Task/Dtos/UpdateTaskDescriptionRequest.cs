namespace Enterprise.BufferManagement.Service.Shared.Task.Dtos
{
	public class UpdateTaskDescriptionRequest
	{
		public string PreviousHash { get; set; } = null!;

		public string NewDescription { get; set; } = null!;
	}
}
