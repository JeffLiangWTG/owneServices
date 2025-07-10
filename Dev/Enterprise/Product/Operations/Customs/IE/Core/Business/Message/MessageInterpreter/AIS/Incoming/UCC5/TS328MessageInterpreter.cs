using System.Collections.Generic;
using Enterprise.Customs.IE.Messaging.UCC5;

namespace Enterprise.Customs.IE.Business.AIS.UCC5
{
	public class TS328MessageInterpreter : InboundMessageInterpreter<TS328Provider>
	{
		public TS328MessageInterpreter(AISUCC5InboundEDIMessage message, TS328Provider provider) : base(message, provider)
		{
		}

		protected override string Summary => Res.GetString("88E8534C-5276-4060-B2A3-DD7B52072BA6", "A TSD Acceptance message (TS328) message has been received for Job {0}.", relatedJob.JobNumber);

		protected override IEnumerable<(string Key, string Value)> GetMessageDetails()
		{
			yield return (CommonResStrings.LocalReferenceNumber, provider.LRN);
			yield return (CommonResStrings.MovementReferenceNumber, provider.MovementReferenceNumber);
			yield return (CommonResStrings.AcceptanceDate, provider.AcceptanceDate.ToShortDateString());
			yield return (CommonResStrings.ResponseDateLimit, provider.ResponseDateLimit.ToShortDateString());
			yield return (CommonResStrings.Remarks, provider.Remarks);
		}
	}
}
