using System.Collections.Generic;
using Enterprise.Customs.IE.Messaging;
using IM404Provider = Enterprise.Customs.IE.Messaging.UCC6.V1.IM404Provider;

namespace Enterprise.Customs.IE.Business.AIS.UCC6.V1
{
	public class IM404MessageInterpreter : InboundMessageInterpreter<IIM404Provider>
	{
		public IM404MessageInterpreter(Enterprise.Messaging.Business.EDIMessage message, IM404Provider provider) : base(message, provider)
		{
			messageProvider = provider;
		}

		readonly IM404Provider messageProvider;

		protected override string Summary => Res.GetString("64204b4c-77ee-4fa4-b6ea-9242e9454715", "An Amendment Request Registration (IM404) message has been received from customs for Job {0}.", relatedJob.JobNumber);

		protected override IEnumerable<(string Key, string Value)> GetMessageDetails()
		{
			yield return (CommonResStrings.MovementReferenceNumber, messageProvider.MovementReferenceNumber);
			yield return (CommonResStrings.AmendmentAcceptanceDate, messageProvider.AmendmentAcceptanceDateAndTime.ToShortDateString());
			yield return (CommonResStrings.PreferredPaymentMethod, messageProvider.PreferredPaymentMethod);
			yield return (CommonResStrings.Remarks, messageProvider.Remarks);
		}
	}
}
