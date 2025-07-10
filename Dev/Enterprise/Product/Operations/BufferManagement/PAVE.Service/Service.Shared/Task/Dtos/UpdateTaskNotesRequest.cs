namespace Enterprise.BufferManagement.Service.Shared
{
	public class UpdateTaskNotesRequest
	{
		public string PreviousHash { get; set; } = null!;

		public string NewNotes { get; set; }
	}
}
