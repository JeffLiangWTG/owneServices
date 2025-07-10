namespace Enterprise.BufferManagement.Service.Shared.Task.Dtos
{
	public class UpdateActualDurationRequest
	{
		public string PreviousHash { get; set; }

		public int? NewActualDuration { get; set; }
	}
}
