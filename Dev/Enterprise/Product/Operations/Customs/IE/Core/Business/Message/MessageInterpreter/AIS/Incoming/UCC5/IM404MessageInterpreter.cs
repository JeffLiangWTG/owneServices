using System.Collections.Generic;
using Enterprise.Customs.IE.Messaging.UCC5;

namespace Enterprise.Customs.IE.Business.AIS.UCC5
{
	public class IM404MessageInterpreter : InboundMessageInterpreter<IM404Provider>
	{
		public IM404MessageInterpreter(Enterprise.Messaging.Business.EDIMessage message, IM404Provider provider) : base(message, provider)
		{
		}

		protected override string Summary => Res.GetString("3AED1CC7-690F-426E-8929-863590B745D3", "An Amendment Request Registration (IM404) message has been received from customs for Job {0}.", relatedJob.JobNumber);

		protected override IEnumerable<(string Key, string Value)> GetMessageDetails()
		{
			yield return (CommonResStrings.MovementReferenceNumber, provider.MovementReferenceNumber);
			yield return (Res.GetString("16200E74-3367-4ADE-A081-BF4B9D4D2B48", "Amendment Acceptance Date"), provider.AmendmentAcceptanceDate.ToShortDateString());
			yield return (CommonResStrings.PreferredPaymentMethod, provider.PreferredPaymentMethod);
			yield return (CommonResStrings.Remarks, provider.Remarks);
		}
	}
}
