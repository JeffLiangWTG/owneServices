using System.Collections.Generic;
using CargoWise.Common;
using Enterprise.Customs.IE.Messaging.AES;

namespace Enterprise.Customs.IE.Business.AES
{
	public class CC557MessageInterpreter : InboundMessageInterpreter<CC557CProvider>
	{
		public CC557MessageInterpreter(Enterprise.Messaging.Business.EDIMessage message, CC557CProvider provider) : base(message, provider)
		{
		}

		protected override string Summary => Res.GetString(
			"33AADCFB-80A0-4AAB-9B33-78A3C41CBBBD",
			"A rejection message from office of Exit has been received from Customs for Job {0} through the IE557 message.",
			relatedJob.JobNumber
		);

		protected override IEnumerable<(string Key, string Value)> GetMessageDetails()
		{
			yield return (CommonResStrings.Status, CommonResStrings.RejRejected);
			yield return (CommonResStrings.LocalReferenceNumber, provider.LocalReferenceNumber);
			yield return (CommonResStrings.MovementReferenceNumber, provider.MovementReferenceNumber);
			yield return (CommonResStrings.BusinessRejectionType, GetCodeAndDescription(provider.BusinessRejectionType, UniversalReferenceConstants.RefCusCodeListTypes.Codes.CL570));
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
