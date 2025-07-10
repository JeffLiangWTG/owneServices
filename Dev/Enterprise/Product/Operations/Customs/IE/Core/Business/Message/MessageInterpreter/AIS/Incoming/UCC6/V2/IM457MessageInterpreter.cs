using System.Collections.Generic;
using Enterprise.Customs.IE.Messaging;

namespace Enterprise.Customs.IE.Business.AIS
{
	public class IM457MessageInterpreter : InboundMessageInterpreter<IM457Provider>
	{
		public IM457MessageInterpreter(Enterprise.Messaging.Business.EDIMessage message, IM457Provider provider) : base(message, provider)
		{
		}

		protected override string Summary => Res.GetString("0398B0C3-0776-4CBA-95A6-7CD6984B7F38", "A Presentation Notification Registration (IM457) message has been received for Job {0}.", relatedJob.JobNumber);

		protected override IEnumerable<(string Key, string Value)> GetMessageDetails()
		{
			yield return (CommonResStrings.LocalReferenceNumber, provider.LocalReferenceNumber);
			yield return (CommonResStrings.MovementReferenceNumber, provider.MovementReferenceNumber);
			yield return (Res.GetString("DCC806C8-8752-487F-A838-77E8524BD2AE", "Presentation Notification Registration Date and Time"), provider.PresentationNotificationRegistrationDateAndTime.ToLongTimeString());
		}
	}
}
