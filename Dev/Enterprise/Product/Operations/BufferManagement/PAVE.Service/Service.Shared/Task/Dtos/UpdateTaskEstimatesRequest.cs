namespace Enterprise.BufferManagement.Service.Shared
{
	public class UpdateTaskEstimatesRequest
	{
		public string PreviousHash { get; set; } = null!;

		public int? NewEstimateFactor { get; set; }

		public int? NewLowEstimate { get; set; }
	}
}
