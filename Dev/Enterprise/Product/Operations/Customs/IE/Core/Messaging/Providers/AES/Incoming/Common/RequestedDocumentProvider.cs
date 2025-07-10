using CargoWise.Types;

namespace Enterprise.Customs.IE.Messaging.AES
{
	public class RequestedDocumentProvider
	{
		public ZString SequenceNumber { get; set; }

		public ZString DocumentType { get; set; }

		public ZString Description { get; set; }
	}
}
