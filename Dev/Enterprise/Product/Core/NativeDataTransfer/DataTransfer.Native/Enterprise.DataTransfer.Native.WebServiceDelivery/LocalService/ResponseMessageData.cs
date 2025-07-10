using Enterprise.DataTransfer.Native.Integration;

namespace Enterprise.DataTransfer.Native.WebServiceDelivery.LocalService
{
	public class ResponseMessageData : IResponseMessage
	{
		public bool HasError { get; set; }
		public string MessageID { get; set; }
		public string ErrorMessage { get; set; }
		public string ResponseMessage { get; set; }
	}
}
