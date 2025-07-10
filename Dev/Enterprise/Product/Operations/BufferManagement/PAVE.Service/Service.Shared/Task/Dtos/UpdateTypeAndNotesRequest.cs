namespace Enterprise.BufferManagement.Service.Shared
{
	public class UpdateTypeAndNotesRequest
	{
		public string PreviousType { get; set; } = null!;

		public string NewType { get; set; }

		public string Notes { get; set; } = null!;
	}
}
