using System.Collections.Generic;
using Enterprise.Customs.IE.Messaging;

namespace Enterprise.Customs.IE.Business.AIS
{
	public class IM404MessageInterpreter : InboundMessageInterpreter<IIM404Provider>
	{
		public IM404MessageInterpreter(Enterprise.Messaging.Business.EDIMessage message, IM404Provider provider) : base(message, provider)
		{
			this.messageProvider = provider;
		}

		readonly IM404Provider messageProvider;

		protected override string Summary => Res.GetString("74394363-1527-4E16-B1AD-5CEE577CC343", "An Amendment Request Registration (IM404) message has been received for Job {0}.", relatedJob.JobNumber);

		protected override IEnumerable<(string Key, string Value)> GetMessageDetails()
		{
			yield return (CommonResStrings.LocalReferenceNumber, messageProvider.LocalReferenceNumber);
			yield return (CommonResStrings.CustomsRegistrationNumber, messageProvider.CustomsRegistrationNumber);
			yield return (CommonResStrings.MovementReferenceNumber, messageProvider.MovementReferenceNumber);
			yield return (Res.GetString("FBB8788B-D029-4B36-B00E-4CA6AB6079E3", "Amendment Date and Time"), messageProvider.AmendmentDateAndTime.ToLongTimeString());
			yield return (Res.GetString("7A2A594D-EA5F-40C2-AEE1-244EEBC3297C", "Amendment Acceptance Date and Time"), messageProvider.AmendmentAcceptanceDateAndTime.ToLongTimeString());
			yield return (CommonResStrings.PreferredPaymentMethod, messageProvider.PreferredPaymentMethod);
			yield return (CommonResStrings.Remarks, messageProvider.Remarks);
		}
	}
}
