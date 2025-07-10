using System.Collections.Generic;
using System.Linq;
using Enterprise.Customs.IE.Messaging;
using IM416Provider = Enterprise.Customs.IE.Messaging.UCC6.V1.IM416Provider;

namespace Enterprise.Customs.IE.Business.AIS.UCC6.V1
{
	public class IM416MessageInterpreter : InboundMessageInterpreter<IIM416Provider>
	{
		public IM416MessageInterpreter(AISInboundEDIMessage message, IM416Provider provider) : base(message, provider)
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
			messageProvider.FunctionalErrors.Select(x => (string.Empty, x.GetFunctionalErrorDetails(GetCodeAndDescription(x.ErrorType, UniversalReferenceConstants.RefCusCodeListTypes.Codes.CL180))));
	}
}
