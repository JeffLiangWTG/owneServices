using System.Collections.Generic;
using System.Linq;
using Enterprise.Customs.IE.Messaging;

namespace Enterprise.Customs.IE.Business.AIS
{
	public class TS309MessageInterpreter : InboundMessageInterpreter<TS309Provider>
	{
		public TS309MessageInterpreter(AISInboundEDIMessage message, TS309Provider provider) : base(message, provider)
		{
		}

		protected override string Summary => provider.InvalidationDecision
			? Res.GetString("44494783-25FB-4B2E-9BE7-8F10512B3DB8", "Temporary Storage Declaration Invalidation Decision (TS309) has been received and linked to job {0}. Decision: Invalidation Request Accepted.", relatedJob.JobNumber)
			: Res.GetString("2BC2CC22-0B70-410E-BF78-540E69D5AFC0", "Temporary Storage Declaration Invalidation Decision (TS309) has been received and linked to job {0}. Decision: Invalidation Request Rejected.", relatedJob.JobNumber);

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
			var functionalErrors = provider.FunctionalErrors.ToArray();
			for (var i = 0; i < functionalErrors.Length; i++)
			{
				yield return GetSummaryForFunctionalError(functionalErrors[i], i + 1);
			}
		}

		(string summary, IEnumerable<(string key, string value)>) GetSummaryForFunctionalError(MFunctionalError01Provider functionalErrorProvider, int sequenceNumber)
		{
			var details = new (string, string)[]
			{
				(CommonResStrings.ErrorPointer, functionalErrorProvider.ErrorPointer),
				(CommonResStrings.ErrorCode, functionalErrorProvider.ErrorCode),
				(CommonResStrings.ErrorCodeDescription, GetDescriptionFromErrorType(functionalErrorProvider.ErrorCode)),
				(CommonResStrings.ErrorReason, functionalErrorProvider.ErrorReason),
				(CommonResStrings.Remarks, functionalErrorProvider.Remarks),
				(CommonResStrings.OriginalAttributeValue, functionalErrorProvider.OriginalAttributeValue)
			};

			return (MessageInterpreterHelper.GetFunctionalSummary(sequenceNumber), details);
		}

		string GetDescriptionFromErrorType(string errorType) => MessageInterpreterHelper.GetDescriptionFromCode(factory, errorType, UniversalReferenceConstants.RefCusCodeListTypes.Codes.CL180, MessageCreatedDate);
	}
}
