using System.Collections.Generic;
using System.Linq;
using Enterprise.Customs.IE.Business;
using Enterprise.Customs.IE.EMCS.Messaging;

namespace Enterprise.Customs.IE.EMCS.Business
{
	public sealed class IE704MessageInterpreter : InboundMessageInterpreter<IIE704>
	{
		public IE704MessageInterpreter(EMCSInboundEDIMessage message, IIE704 provider) : base(message, provider)
		{
		}

		protected override string Summary => Res.GetString("A13AE47C-2C8F-42EE-8E9A-F8498A05483C", "A Generic Refusal (IE704) message has been received for Job {0}.", relatedJob.JobNumber);

		protected override IEnumerable<(string Key, string Value)> GetMessageDetails()
		{
			yield return (CommonResStrings.AdministrativeReferenceCode, provider.AdministrativeReferenceCode);
			yield return (CommonResStrings.LocalReferenceNumber, provider.LocalReferenceNumber);
		}

		protected override IEnumerable<(string summary, IEnumerable<(string key, string value)>)> GetAdditionalMessageDetails()
		{
			return provider.Errors.Select((error, index) => ($"{CommonResStrings.FunctionalError} {index + 1}", new (string, string)[]
			{
				(CommonResStrings.ErrorType, error.ErrorType),
				(CommonResStrings.ErrorReason, error.ErrorReason),
				(CommonResStrings.ErrorLocation, error.ErrorLocation),
				(CommonResStrings.OriginalAttributeValue, error.OriginalAttributeValue)
			}.AsEnumerable()));
		}
	}
}
