namespace Enterprise.BufferManagement.Service.Shared.Common
{
	public class UpdateContentWithHashRequest
	{
		public string PreviousHash { get; set; } = null!;

		public string NewContent { get; set; } = null!;
	}
}
