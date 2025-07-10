using System.Collections.Generic;
using System.Linq;
using Enterprise.Customs.IE.Messaging;

namespace Enterprise.Customs.IE.Business.AIS
{
	public class IM405MessageInterpreter : InboundMessageInterpreter<IIM405Provider>
	{
		public IM405MessageInterpreter(Enterprise.Messaging.Business.EDIMessage message, IM405Provider provider) : base(message, provider)
		{
			this.messageProvider = provider;
		}

		readonly IM405Provider messageProvider;

		protected override string Summary => Res.GetString("17B3F90A-B531-48A1-8C39-A3875BDE4B21", "An Amendment Request Rejection (IM405) message has been received for Job {0}.", relatedJob.JobNumber);

		protected override IEnumerable<(string Key, string Value)> GetMessageDetails()
		{
			yield return (CommonResStrings.MovementReferenceNumber, provider.MovementReferenceNumber);
			yield return (CommonResStrings.AmendmentRejectionDate, provider.AmendmentRejectionDate.ToShortDateString());
			yield return (CommonResStrings.AmendmentRejectionMotivationText, provider.AmendmentRejectionMotivationText);
			yield return (CommonResStrings.Remarks, provider.Remarks);
		}

		protected override IEnumerable<(string summary, IEnumerable<(string key, string value)>)> GetAdditionalMessageDetails()
		{
			return messageProvider.FunctionalErrors.Select(x => (string.Empty, x.GetMessageDetails(GetCodeAndDescription(x.ErrorCode, UniversalReferenceConstants.RefCusCodeListTypes.Codes.CL180))));
		}
	}
}
