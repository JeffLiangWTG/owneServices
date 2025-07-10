using System.Collections.Generic;
using System.Linq;
using Enterprise.Customs.IE.Messaging;

namespace Enterprise.Customs.IE.Business.AIS
{
	public class IM409MessageInterpreter : InboundMessageInterpreter<IIM409Provider>
	{
		public IM409MessageInterpreter(Enterprise.Messaging.Business.EDIMessage message, IM409Provider provider) : base(message, provider)
		{
			this.messageProvider = provider;
		}

		readonly IM409Provider messageProvider;

		protected override string Summary => provider.InvalidationDecision ? Res.GetString("7ECD2493-7707-482B-A03A-4A56AF2ED88E", "[IM409 - Invalidation Decision] has been received and linked to job {0}. Decision: Invalidation Rejected", relatedJob.JobNumber) : Res.GetString("03B7B296-7ECF-4601-822A-14B16D0C9FEA", "[IM409 - Invalidation Decision] has been received and linked to job {0}. Decision: Invalidation Accepted", relatedJob.JobNumber);

		protected override IEnumerable<(string Key, string Value)> GetMessageDetails()
		{
			yield return (CommonResStrings.MovementReferenceNumber, provider.MovementReferenceNumber);
			yield return (CommonResStrings.InvalidationDecision, provider.InvalidationDecision.ToString());
			yield return (CommonResStrings.InvalidationInitiatedByCustoms, provider.InvalidationInitiatedByCustoms.ToString());
			yield return (CommonResStrings.InvalidationJustification, provider.InvalidationJustification);
			yield return (CommonResStrings.DateOfInvalidationDecision, provider.DateOfInvalidationDecision.ToShortDateString());
			yield return (CommonResStrings.DateOfInvalidationRequest, provider.DateOfInvalidationRequest.ToShortDateString());
			yield return (CommonResStrings.DateOfInvalidation, provider.DateOfInvalidation.ToShortDateString());
		}

		protected override IEnumerable<(string summary, IEnumerable<(string key, string value)>)> GetAdditionalMessageDetails()
		{
			return messageProvider.FunctionalErrors.Select(x => (string.Empty, x.GetMessageDetails(GetCodeAndDescription(x.ErrorCode, UniversalReferenceConstants.RefCusCodeListTypes.Codes.CL180))));
		}
	}
}
