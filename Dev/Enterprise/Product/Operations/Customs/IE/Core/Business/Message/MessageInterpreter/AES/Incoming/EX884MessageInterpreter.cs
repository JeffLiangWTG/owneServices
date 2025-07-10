using System.Collections.Generic;
using Enterprise.Customs.IE.Messaging.AES;

namespace Enterprise.Customs.IE.Business.AES
{
	class EX884MessageInterpreter : InboundMessageInterpreter<EX884Provider>
	{
		public EX884MessageInterpreter(InboundEDIMessage message, EX884Provider provider) : base(message, provider)
		{
		}

		protected override string Summary => Res.GetString(
			"B1B4CC8C-0688-4D9E-9B42-3EE14DA74ED4",
			"A Document Presentation Request Cancellation message has been received from Customs for Job {0} via the EX884 message canceling a previous request for presentation of documents.",
			relatedJob.JobNumber
		);

		protected override IEnumerable<(string Key, string Value)> GetMessageDetails()
		{
			yield return (CommonResStrings.MovementReferenceNumber, provider.MovementReferenceNumber);
			yield return (CommonResStrings.CaseId, provider.CaseId);
			yield return (Res.GetString("6C0D7767-54DF-4AD5-934C-9AE5294D4909", "Document Presentation Request Cancellation Reason"), provider.DocumentsPresentRequestCancellationReason);
		}
	}
}
