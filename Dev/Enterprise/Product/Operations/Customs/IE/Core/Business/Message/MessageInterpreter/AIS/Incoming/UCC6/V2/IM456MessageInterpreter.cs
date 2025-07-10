using System.Collections.Generic;
using System.Linq;
using Enterprise.Customs.IE.Messaging;

namespace Enterprise.Customs.IE.Business.AIS
{
	public class IM456MessageInterpreter : InboundMessageInterpreter<IM456Provider>
	{
		public IM456MessageInterpreter(Enterprise.Messaging.Business.EDIMessage message, IM456Provider provider) : base(message, provider)
		{
		}

		protected override string Summary => Res.GetString("3DAA7396-C764-4810-AB38-5732AA07A32B", "A Rejection from SCI (IM456) message has been received for Job {0}.", relatedJob.JobNumber);

		protected override IEnumerable<(string Key, string Value)> GetMessageDetails()
		{
			yield return (CommonResStrings.LocalReferenceNumber, provider.LocalReferenceNumber);
			yield return (CommonResStrings.MovementReferenceNumber, provider.MovementReferenceNumber);
			yield return (CommonResStrings.CustomsRegistrationNumber, provider.CustomsRegistrationNumber);
			yield return (CommonResStrings.BusinessRejectionType, provider.BusinessRejectionType);
			yield return (CommonResStrings.RejectionDateAndTime, provider.RejectionDateAndTime.ToLongTimeString());
			yield return (CommonResStrings.RejectionCode, provider.RejectionCode);
			yield return (CommonResStrings.RejectionReason, provider.RejectionReason);
		}

		protected override IEnumerable<(string summary, IEnumerable<(string key, string value)>)> GetAdditionalMessageDetails()
		{
			return provider.FunctionalErrors.Select(x => (string.Empty, x.GetMessageDetails(GetCodeAndDescription(x.ErrorCode, UniversalReferenceConstants.RefCusCodeListTypes.Codes.CL180)))).ToArray();
		}
	}
}
