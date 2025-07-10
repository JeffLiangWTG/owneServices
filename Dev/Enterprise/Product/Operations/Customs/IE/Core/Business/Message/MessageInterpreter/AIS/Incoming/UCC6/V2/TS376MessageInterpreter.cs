using System.Collections.Generic;
using System.Linq;
using Enterprise.Customs.IE.Messaging;

namespace Enterprise.Customs.IE.Business.AIS
{
	public class TS376MessageInterpreter : InboundMessageInterpreter<TS376Provider>
	{
		public TS376MessageInterpreter(Enterprise.Messaging.Business.EDIMessage message, TS376Provider provider) : base(message, provider)
		{
		}

		protected override string Summary => Res.GetString("2F382358-2A28-411F-800C-2EDA49995908", "A Goods Status Report Declaration (ND4) Rejection (TS376) message has been received for Job {0}.", relatedJob.JobNumber);

		protected override IEnumerable<(string Key, string Value)> GetMessageDetails()
		{
			yield return (CommonResStrings.MovementReferenceNumber, provider.MovementReferenceNumber);
			yield return (CommonResStrings.RejectionDate, provider.RejectionDate.ToShortDateString());
			yield return (CommonResStrings.RejectionMotivationText, provider.RejectionMotivationText);
		}

		protected override IEnumerable<(string summary, IEnumerable<(string key, string value)>)> GetAdditionalMessageDetails()
		{
			return provider.FunctionalErrors.Select(x => (string.Empty, x.GetMessageDetails(GetCodeAndDescription(x.ErrorCode, UniversalReferenceConstants.RefCusCodeListTypes.Codes.CL180))));
		}
	}
}
