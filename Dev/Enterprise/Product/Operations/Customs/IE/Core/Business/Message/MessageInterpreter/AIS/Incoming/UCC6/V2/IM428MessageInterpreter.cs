using System.Collections.Generic;
using Enterprise.Customs.IE.Messaging;

namespace Enterprise.Customs.IE.Business.AIS
{
	public class IM428MessageInterpreter : InboundMessageInterpreter<IIM428Provider>
	{
		public IM428MessageInterpreter(Enterprise.Messaging.Business.EDIMessage message, IM428Provider provider) : base(message, provider)
		{
			messageProvider = provider;
		}

		readonly IM428Provider messageProvider;

		protected override string Summary => Res.GetString("D4F9FEB8-0F8E-4A97-A635-9F1F53BD4D3E", "A Customs Declaration Acceptance (IM428) message has been received for Job {0}.", relatedJob.JobNumber);

		protected override IEnumerable<(string Key, string Value)> GetMessageDetails()
		{
			yield return (CommonResStrings.LocalReferenceNumber, messageProvider.LocalReferenceNumber);
			yield return (CommonResStrings.CustomsRegistrationNumber, messageProvider.CustomsRegistrationNumber);
			yield return (CommonResStrings.MovementReferenceNumber, messageProvider.MovementReferenceNumber);
			yield return (Res.GetString("3EC090A1-322A-4F58-9AFD-BF905C5992A7", "Declaration Acceptance Date and Time"), messageProvider.DeclarationAcceptanceDateAndTime.ToLongTimeString());
			yield return (CommonResStrings.DeclarationType, messageProvider.DeclarationType);
			yield return (CommonResStrings.AdditionalDeclarationType, messageProvider.AdditionalDeclarationType);
			yield return (CommonResStrings.ResponseDateLimit, messageProvider.ResponseDateLimit.ToShortDateString());
			yield return (CommonResStrings.PreferredPaymentMethod, messageProvider.PreferredPaymentMethod);
			yield return (CommonResStrings.Remarks, messageProvider.Remarks);
		}
	}
}

