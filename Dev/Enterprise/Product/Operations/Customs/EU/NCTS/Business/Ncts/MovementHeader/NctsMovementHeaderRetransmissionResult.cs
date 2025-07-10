
namespace Enterprise.Customs.EU.NCTS.Business
{
	public class NctsMovementHeaderRetransmissionResult
	{
		public bool IsUpdated { get; private set; }
		public string Message { get; private set; }

		public NctsMovementHeaderRetransmissionResult(bool isUpdated, string message)
		{
			IsUpdated = isUpdated;
			Message = message;
		}
	}
}
