using System.Collections.Generic;
using Enterprise.Customs.IE.Messaging.AES;

namespace Enterprise.Customs.IE.Business.AES
{
	public class EX564MessageInterpreter : InboundMessageInterpreter<EX564Provider>
	{
		public EX564MessageInterpreter(InboundEDIMessage message, EX564Provider provider) : base(message, provider) { }

		protected override string Summary => Res.GetString(
			"30075F01-F939-48D3-A85C-EF1B4371EB23",
			"A Request Declaration Cancellation message has been received from Customs for Job {0} through the EX564 message.",
			relatedJob.JobNumber
		);

		protected override IEnumerable<(string Key, string Value)> GetMessageDetails()
		{
			yield return (CommonResStrings.MovementReferenceNumber, provider.MovementReferenceNumber);
			yield return (CommonResStrings.CaseId, provider.CaseId);
			yield return (CommonResStrings.Remarks, provider.Remarks);
		}
	}
}
