using System.Collections.Generic;
using Enterprise.Customs.IE.Messaging.AES;

namespace Enterprise.Customs.IE.Business.AES
{
	class EX882MessageInterpreter : InboundMessageInterpreter<EX882Provider>
	{
		public EX882MessageInterpreter(InboundEDIMessage message, EX882Provider provider) : base(message, provider)
		{
		}

		protected override string Summary => Res.GetString(
			"09491E89-FBBC-4AB7-88E4-47DD6E356816",
			"A Document Upload Request Cancellation message has been received from Customs for Job {0} through the EX882 message stating that Revenue have now decided to cancel the uploading document request which was sent before through EX582 message.",
			relatedJob.JobNumber
		);

		protected override IEnumerable<(string Key, string Value)> GetMessageDetails()
		{
			yield return (CommonResStrings.MovementReferenceNumber, provider.MovementReferenceNumber);
			yield return (CommonResStrings.CaseId, provider.CaseId);
			yield return (Res.GetString("C451CE12-26F8-49E7-96B3-9B13CE5BC85F", "Document Upload Request Cancellation Reason"), provider.DocumentsUploadRequestCancellationReason);
		}
	}
}
