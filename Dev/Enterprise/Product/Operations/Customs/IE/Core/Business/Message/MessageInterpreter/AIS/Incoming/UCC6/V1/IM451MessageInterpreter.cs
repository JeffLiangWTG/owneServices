using System.Collections.Generic;
using Enterprise.Customs.IE.Messaging;
using IM451Provider = Enterprise.Customs.IE.Messaging.UCC6.V1.IM451Provider;

namespace Enterprise.Customs.IE.Business.AIS.UCC6.V1
{
	public class IM451MessageInterpreter : InboundMessageInterpreter<IIM451Provider>
	{
		public IM451MessageInterpreter(AISInboundEDIMessage message, IM451Provider provider) : base(message, provider)
		{
			messageProvider = provider;
		}

		readonly IM451Provider messageProvider;

		protected override string Summary => Res.GetString("6ef6ac3b-0178-4f1d-9570-8d12bfcf24d3", "A Release Rejection (IM451) message has been received for Job {0}.", relatedJob.JobNumber);

		protected override IEnumerable<(string Key, string Value)> GetMessageDetails()
		{
			yield return (CommonResStrings.MovementReferenceNumber, messageProvider.MovementReferenceNumber);
			yield return (CommonResStrings.LocalReferenceNumber, messageProvider.LocalReferenceNumber);
			yield return (CommonResStrings.AdditionalDeclarationType, messageProvider.AdditionalDeclarationType);
			yield return (CommonResStrings.DecisionReason, messageProvider.DecisionReason);
			yield return (CommonResStrings.PreferredPaymentMethod, messageProvider.PreferredPaymentMethod);
			yield return (CommonResStrings.Remarks, messageProvider.Remarks);
		}

		protected override IEnumerable<(string summary, IEnumerable<(string key, string value)>)> GetAdditionalMessageDetails()
		{
			var controlResult = messageProvider.ControlResult;
			var details = new (string, string)[]
			{
				(CommonResStrings.ControlResultCode, controlResult.ControlResultCode),
				(CommonResStrings.ControlResultDate, controlResult.ControlDate.ToShortDateString()),
				(CommonResStrings.Remarks, controlResult.Remarks),
			};

			yield return (string.Empty, details);
		}
	}
}
