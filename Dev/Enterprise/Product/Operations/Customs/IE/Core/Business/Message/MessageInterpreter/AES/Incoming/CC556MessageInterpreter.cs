using System.Collections.Generic;
using CargoWise.Common;
using Enterprise.Customs.IE.Messaging.AES;

namespace Enterprise.Customs.IE.Business.AES
{
	public class CC556MessageInterpreter : InboundMessageInterpreter<CC556CProvider>
	{
		public CC556MessageInterpreter(AESInboundEDIMessage message, CC556CProvider provider) : base(message, provider)
		{
		}

		protected override string Summary => Res.GetString(
			"EC5F1211-3824-413D-9A3F-1ABEFE6F5BFD",
			"A Rejection message has been received from Customs for Job {0} through the IE556 message.",
			relatedJob.JobNumber
		);

		protected override IEnumerable<(string Key, string Value)> GetMessageDetails()
		{
			yield return (CommonResStrings.LocalReferenceNumber, provider.LocalReferenceNumber);
			yield return (CommonResStrings.MovementReferenceNumber, provider.MovementReferenceNumber);
			yield return (CommonResStrings.BusinessRejectionType, GetCodeAndDescription(provider.BusinessRejectionType, UniversalReferenceConstants.RefCusCodeListTypes.Codes.CL560));
			yield return (CommonResStrings.RejectionDateAndTime, provider.RejectionDateAndTime.ToLongTimeString());
			yield return (CommonResStrings.RejectionCode, provider.RejectionCode);
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
