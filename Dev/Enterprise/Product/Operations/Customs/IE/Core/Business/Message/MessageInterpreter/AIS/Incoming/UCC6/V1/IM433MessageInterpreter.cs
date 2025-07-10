using System.Collections.Generic;
using Enterprise.Customs.IE.Messaging;
using IM433Provider = Enterprise.Customs.IE.Messaging.UCC6.V1.IM433Provider;

namespace Enterprise.Customs.IE.Business.AIS.UCC6.V1
{
	public class IM433MessageInterpreter : InboundMessageInterpreter<IIM933Provider>
	{
		public IM433MessageInterpreter(AISInboundEDIMessage message, IM433Provider provider) : base(message, provider)
		{
			messageProvider = provider;
		}

		readonly IM433Provider messageProvider;

		protected override string Summary => Res.GetString("aea6534b-e40b-4c3d-93af-af6ab80e7de1", "A Presentation Notification Rejection (IM433) message has been received for Job {0}.", relatedJob.JobNumber);

		protected override IEnumerable<(string Key, string Value)> GetMessageDetails()
		{
			yield return (CommonResStrings.MovementReferenceNumber, messageProvider.MovementReferenceNumber);
			yield return (CommonResStrings.RejectionDate, messageProvider.NotificationRejectionDate.ToShortDateString());
			yield return (CommonResStrings.RejectionReason, messageProvider.NotificationRejectionReason);
		}

		protected override IEnumerable<(string summary, IEnumerable<(string key, string value)>)> GetAdditionalMessageDetails() =>
			messageProvider.FunctionalErrors.GetFunctionalErrorDetailsUCC5(MessageCreatedDate, factory);
	}
}
