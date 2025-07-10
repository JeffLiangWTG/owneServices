using System.Collections.Generic;
using Enterprise.Customs.IE.Messaging;

namespace Enterprise.Customs.IE.Business.AIS
{
	public class IM464MessageInterpreter : InboundMessageInterpreter<IM464Provider>
	{
		public IM464MessageInterpreter(Enterprise.Messaging.Business.EDIMessage message, IM464Provider provider) : base(message, provider)
		{
			this.messageProvider = provider;
		}

		readonly IM464Provider messageProvider;

		protected override string Summary => Res.GetString("84DFE8AD-2FEE-43F6-8D78-231D3004702F", "A Request Declaration Invalidation (IM464) message has been received for Job {0}.", relatedJob.JobNumber);

		protected override IEnumerable<(string Key, string Value)> GetMessageDetails()
		{
			yield return (CommonResStrings.MovementReferenceNumber, messageProvider.MovementReferenceNumber);
			yield return (CommonResStrings.CaseId, messageProvider.CaseId);
			yield return (CommonResStrings.Remarks, messageProvider.Remarks);
		}
	}
}
