using System.Collections.Generic;
using CargoWise.Common;
using Enterprise.Customs.IE.Business;
using Enterprise.Customs.IE.NCTS.Messaging;

namespace Enterprise.Customs.IE.NCTS.Business
{
	public class CC056CMessageInterpreter : InboundMessageInterpreter<CC056CProvider>
	{
		public CC056CMessageInterpreter(Enterprise.Messaging.Business.EDIMessage message, CC056CProvider provider) : base(message, provider)
		{
		}

		protected override string Summary => Res.GetString(
			"622B9141-DB5A-45BD-BC60-1CFE7F48067E",
			"A Rejection From Office of Departure (IE056) message has been received for Job {0}.",
			relatedJob.JobNumber
		);

		protected override IEnumerable<(string Key, string Value)> GetMessageDetails()
		{
			yield return (CommonResStrings.MovementReferenceNumber, provider.MRN);
			yield return (CommonResStrings.RejectionDateAndTime, provider.RejectionDateAndTime.ToLongTimeString());
			yield return (CommonResStrings.RejectionCode, GetCodeAndDescription(provider.RejectionCode, UniversalReferenceConstants.RefCusCodeListTypes.Codes.CL226));
			yield return (CommonResStrings.RejectionReason, provider.RejectionReason);

			var errors = provider?.FunctionalErrors;
			if (!errors.IsNullOrEmpty())
			{
				foreach (var error in errors)
				{
					yield return (CommonResStrings.ErrorPointer, error.ErrorPointer);
					yield return (CommonResStrings.ErrorCode, GetCodeAndDescription(error.ErrorCode, UniversalReferenceConstants.RefCusCodeListTypes.Codes.CL180));
					yield return (CommonResStrings.ErrorReason, error.ErrorReason);
				}
			}
		}
	}
}
