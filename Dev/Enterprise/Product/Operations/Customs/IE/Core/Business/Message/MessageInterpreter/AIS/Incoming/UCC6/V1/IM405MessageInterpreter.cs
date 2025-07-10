using System.Collections.Generic;
using Enterprise.Customs.IE.Messaging;
using IM405Provider = Enterprise.Customs.IE.Messaging.UCC6.V1.IM405Provider;

namespace Enterprise.Customs.IE.Business.AIS.UCC6.V1
{
	public class IM405MessageInterpreter : InboundMessageInterpreter<IIM405Provider>
	{
		public IM405MessageInterpreter(Enterprise.Messaging.Business.EDIMessage message, IM405Provider provider) : base(message, provider)
		{
			messageProvider = provider;
		}

		readonly IM405Provider messageProvider;

		protected override string Summary => Res.GetString("8a5f0809-00ec-4797-8994-7f5a617782e2", "An Amendment Request Rejection (IM405) message has been received from customs for Job {0}.", relatedJob.JobNumber);

		protected override IEnumerable<(string Key, string Value)> GetMessageDetails()
		{
			yield return (CommonResStrings.MovementReferenceNumber, messageProvider.MovementReferenceNumber);
			yield return (CommonResStrings.AmendmentRejectionDate, messageProvider.AmendmentRejectionDate.ToShortDateString());
			yield return (CommonResStrings.AmendmentRejectionMotivationText, messageProvider.AmendmentRejectionMotivationText);
			yield return (CommonResStrings.Remarks, messageProvider.Remarks);
		}

		protected override IEnumerable<(string summary, IEnumerable<(string key, string value)>)> GetAdditionalMessageDetails() =>
			messageProvider.FunctionalErrors.GetFunctionalErrorDetailsUCC5(MessageCreatedDate, factory);
	}
}
