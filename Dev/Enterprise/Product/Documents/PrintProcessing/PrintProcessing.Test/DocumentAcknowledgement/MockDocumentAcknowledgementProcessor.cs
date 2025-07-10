using CargoWise.Types;

namespace Enterprise.PrintProcessing.DocumentAcknowledgement
{
	class MockDocumentAcknowledgementProcessor : IDocumentAcknowledgementProcessor
	{
		public void Process(ZGuid documentSentLogPK, DocumentAcknowledgementStatus status, string message)
		{
			HasProcessed = true;
			LastPK = documentSentLogPK;
			LastStatus = status;
			LastMessage = message;
		}

		public void Process(ZGuid documentSentLogPK, DocumentAcknowledgementStatus status)
		{
			Process(documentSentLogPK, status, null);
		}

		public void Reset()
		{
			HasProcessed = new bool();
			LastPK = new ZGuid();
			LastStatus = new DocumentAcknowledgementStatus();
			LastMessage = null;
		}

		public bool HasProcessed;
		public ZGuid LastPK;
		public DocumentAcknowledgementStatus LastStatus;
		public string LastMessage;
	}
}
