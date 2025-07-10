using System.Collections.Generic;
using System.Linq;
using Enterprise.Customs.IE.Messaging;

namespace Enterprise.Customs.IE.Business.AIS
{
	public class IM416MessageInterpreter : InboundMessageInterpreter<IIM416Provider>
	{
		public IM416MessageInterpreter(Enterprise.Messaging.Business.EDIMessage message, IM416Provider provider) : base(message, provider)
		{
			messageProvider = provider;
		}

		readonly IM416Provider messageProvider;

		protected override string Summary => CommonResStrings.GetIM416MessageInterpreterSummary(relatedJob.JobNumber);

		protected override IEnumerable<(string Key, string Value)> GetMessageDetails()
		{
			yield return (CommonResStrings.AdditionalDeclarationType, messageProvider.AdditionalDeclarationType);
			yield return (CommonResStrings.LocalReferenceNumber, messageProvider.LocalReferenceNumber);
			yield return (CommonResStrings.RejectionDate, messageProvider.RejectionDate.ToShortDateString());
			yield return (CommonResStrings.RejectionMotivationText, messageProvider.RejectionMotivationText);
		}

		protected override IEnumerable<(string summary, IEnumerable<(string key, string value)>)> GetAdditionalMessageDetails() =>
			messageProvider.FunctionalErrors.Select(x => (string.Empty, x.GetMessageDetails(GetCodeAndDescription(x.ErrorCode, UniversalReferenceConstants.RefCusCodeListTypes.Codes.CL180))));
	}
}
