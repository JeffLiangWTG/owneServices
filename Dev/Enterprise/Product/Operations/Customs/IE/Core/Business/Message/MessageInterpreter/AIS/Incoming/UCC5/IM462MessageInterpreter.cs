using System.Collections.Generic;
using Enterprise.Customs.IE.Messaging.UCC5;

namespace Enterprise.Customs.IE.Business.AIS.UCC5
{
	public class IM462MessageInterpreter : InboundMessageInterpreter<IM462Provider>
	{
		public IM462MessageInterpreter(AISUCC5InboundEDIMessage message, IM462Provider provider) : base(message, provider)
		{
		}

		protected override string Summary => Res.GetString("E0D5BB4A-3E3C-4EEF-A46E-A001B649651C", "A Request Declaration Amendment message (IM462) message has been received for Job {0}.", relatedJob.JobNumber);

		protected override IEnumerable<(string Key, string Value)> GetMessageDetails()
		{
			yield return (CommonResStrings.MovementReferenceNumber, provider.MovementReferenceNumber);
			yield return (CommonResStrings.CaseId, provider.CaseId);
			yield return (Res.GetString("9DB75AB3-6A20-41AE-9502-C6AB9E48B8B5", "Amendment Reason"), provider.AmendReason);
		}
	}
}
