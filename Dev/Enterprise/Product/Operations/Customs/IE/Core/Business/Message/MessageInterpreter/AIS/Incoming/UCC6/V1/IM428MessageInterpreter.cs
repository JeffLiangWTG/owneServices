using System.Collections.Generic;
using Enterprise.Customs.IE.Messaging;
using IM428Provider = Enterprise.Customs.IE.Messaging.UCC6.V1.IM428Provider;

namespace Enterprise.Customs.IE.Business.AIS.UCC6.V1
{
	public class IM428MessageInterpreter : InboundMessageInterpreter<IIM428Provider>
	{
		public IM428MessageInterpreter(AISInboundEDIMessage message, IM428Provider provider) : base(message, provider)
		{
			messageProvider = provider;
		}

		readonly IM428Provider messageProvider;

		protected override string Summary => Res.GetString(
			"236e8612-1cdc-48fd-b9f4-ce2f5bf19c42",
			"A Customs Declaration Acceptance or Goods Deemed to be Placed under Customs Warehousing Procedure (IM428) message has been received from customs for Job {0}.",
			relatedJob.JobNumber
		);

		protected override IEnumerable<(string Key, string Value)> GetMessageDetails()
		{
			yield return (CommonResStrings.LocalReferenceNumber, messageProvider.LocalReferenceNumber);
			yield return (CommonResStrings.MovementReferenceNumber, messageProvider.MovementReferenceNumber);
			yield return (CommonResStrings.AdditionalDeclarationType, messageProvider.AdditionalDeclarationType);
			yield return (CommonResStrings.DeclarationAcceptanceDate, messageProvider.DeclarationAcceptanceDateAndTime.ToShortDateString());
			yield return (CommonResStrings.PreferredPaymentMethod, messageProvider.PreferredPaymentMethod);
			yield return (CommonResStrings.Remarks, messageProvider.Remarks);
		}
	}
}
