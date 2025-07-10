using System.Collections.Generic;
using Enterprise.Customs.IE.Messaging.AES;

namespace Enterprise.Customs.IE.Business.AES
{
	public class EX562MessageInterpreter : InboundMessageInterpreter<EX562Provider>
	{
		public EX562MessageInterpreter(InboundEDIMessage message, EX562Provider provider) : base(message, provider)
		{
		}

		protected override string Summary => Res.GetString(
			"1BD80F1A-5287-4119-AF84-3D6262CAD1F1",
			"An Amendment Request message has been received from Customs for Job {0} through the EX562 message.",
			relatedJob.JobNumber);

		protected override IEnumerable<(string Key, string Value)> GetMessageDetails()
		{
			yield return (CommonResStrings.MovementReferenceNumber, provider.MovementReferenceNumber);
			yield return (CommonResStrings.CaseId, provider.CaseId);
			yield return (CommonResStrings.Remarks, provider.Remarks);
		}
	}
}
