using System.Collections.Generic;
using Enterprise.Customs.IE.Messaging.UCC5;

namespace Enterprise.Customs.IE.Business.AIS.UCC5
{
	public class TS351MessageInterpreter : InboundMessageInterpreter<TS351Provider>
	{
		public TS351MessageInterpreter(AISUCC5InboundEDIMessage message, TS351Provider provider) : base(message, provider)
		{
		}

		protected override string Summary => Res.GetString("3250DFE2-1222-44DF-8142-C7DBB39AF395", "A Temporary Storage Refusal (TS351) message has been received for Job {0}.", relatedJob.JobNumber);

		protected override IEnumerable<(string Key, string Value)> GetMessageDetails()
		{
			yield return (CommonResStrings.LocalReferenceNumber, provider.LRN);
			yield return (CommonResStrings.MovementReferenceNumber, provider.MovementReferenceNumber);
			yield return (CommonResStrings.ControlResultDate, provider.ControlDate.ToShortDateString());
			yield return (CommonResStrings.ControlResultRemarks, provider.ControlResultRemarks);
			yield return (CommonResStrings.Remarks, provider.Remarks);
		}
	}
}
