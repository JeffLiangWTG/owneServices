using Enterprise.DataTransfer.Native.Integration;

namespace Enterprise.DataTransfer.Native.WebServiceDelivery.LocalService
{
	public class RequestMessageData : IRequestMessage
	{
		public string Message { get; set; }
		public string MessageEntity { get; set; }
		public string MessageID { get; set; }
		public string RecipientID { get; set; }
		public string RecipientType { get; set; }
		public string SenderID { get; set; }
		public string SenderType { get; set; }
		public string SenderUsername { get; set; }
	}
}
