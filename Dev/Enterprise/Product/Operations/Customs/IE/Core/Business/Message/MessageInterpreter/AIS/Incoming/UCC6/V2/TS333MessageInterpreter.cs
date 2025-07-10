using System.Collections.Generic;
using System.Linq;
using Enterprise.Customs.IE.Messaging;

namespace Enterprise.Customs.IE.Business.AIS
{
	public class TS333MessageInterpreter : InboundMessageInterpreter<TS333Provider>
	{
		public TS333MessageInterpreter(Enterprise.Messaging.Business.EDIMessage message, TS333Provider provider) : base(message, provider)
		{
		}

		protected override string Summary => Res.GetString("86D0B191-C3EA-4B0A-A491-3FE8481BB519", "A Presentation Notification (G3) Rejection (TS333) message has been received for Job {0}.", relatedJob.JobNumber);

		protected override IEnumerable<(string Key, string Value)> GetMessageDetails()
		{
			yield return (CommonResStrings.MovementReferenceNumber, provider.MovementReferenceNumber);
			yield return (CommonResStrings.RejectionDate, provider.RejectionDate.ToShortDateString());
			yield return (CommonResStrings.RejectionReason, provider.RejectionReason);
		}

		protected override IEnumerable<(string summary, IEnumerable<(string key, string value)>)> GetAdditionalMessageDetails()
		{
			return provider.FunctionalErrors.Select(x => (string.Empty, x.GetMessageDetails(GetCodeAndDescription(x.ErrorCode, UniversalReferenceConstants.RefCusCodeListTypes.Codes.CL180))));
		}
	}
}
