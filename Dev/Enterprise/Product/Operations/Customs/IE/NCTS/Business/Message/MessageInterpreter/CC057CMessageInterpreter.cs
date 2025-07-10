using System.Collections.Generic;
using CargoWise.Common;
using Enterprise.Customs.IE.Business;
using Enterprise.Customs.IE.NCTS.Messaging;

namespace Enterprise.Customs.IE.NCTS.Business
{
	public class CC057CMessageInterpreter : InboundMessageInterpreter<CC057CProvider>
	{
		public CC057CMessageInterpreter(Enterprise.Messaging.Business.EDIMessage message, CC057CProvider provider) : base(message, provider)
		{
		}

		protected override string Summary => Res.GetString(
			"ED5FD152-A2F0-40F8-8202-B4B5089993FC",
			"A Rejection from Office of Destination (IE057) message has been received for Job {0}.",
		relatedJob.JobNumber
		);

		protected override IEnumerable<(string Key, string Value)> GetMessageDetails()
		{
			yield return (CommonResStrings.MovementReferenceNumber, provider.MRN);
			yield return (CommonResStrings.RejectionCode, GetCodeAndDescription(provider.RejectionCode, UniversalReferenceConstants.RefCusCodeListTypes.Codes.CL227));
			yield return (CommonResStrings.RejectionReason, provider.RejectionReason);

			var errors = provider?.FunctionalErrors;
			if (!errors.IsNullOrEmpty())
			{
				foreach (var error in errors)
				{
					yield return (CommonResStrings.ErrorPointer, error.ErrorPointer);
					yield return (CommonResStrings.ErrorCode, GetCodeAndDescription(error.ErrorCode, IE.Business.UniversalReferenceConstants.RefCusCodeListTypes.Codes.CL180));
					yield return (CommonResStrings.ErrorReason, error.ErrorReason);
					yield return (Res.GetString("8ED0F770-198A-4634-9677-69A7C71DDF76", "Original Attribute Value"), error.OriginalAttributeValue);
				}
			}
		}
	}
}
