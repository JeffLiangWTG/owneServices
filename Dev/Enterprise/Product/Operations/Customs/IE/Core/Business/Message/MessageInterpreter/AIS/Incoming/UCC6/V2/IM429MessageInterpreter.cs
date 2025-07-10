using System.Collections.Generic;
using Enterprise.Customs.IE.Messaging;

namespace Enterprise.Customs.IE.Business.AIS
{
	public class IM429MessageInterpreter : InboundMessageInterpreter<IIM429Provider>
	{
		public IM429MessageInterpreter(Enterprise.Messaging.Business.EDIMessage message, IM429Provider provider) : base(message, provider)
		{
			messageProvider = provider;
		}

		readonly IM429Provider messageProvider;

		protected override string Summary => Res.GetString("16BD8053-7D5D-4972-8BCB-784A85D75528", "A Release for Import (IM429) message has been received for Job {0}.", relatedJob.JobNumber);

		protected override IEnumerable<(string Key, string Value)> GetMessageDetails()
		{
			yield return (CommonResStrings.LocalReferenceNumber, messageProvider.LocalReferenceNumber);
			yield return (CommonResStrings.MovementReferenceNumber, messageProvider.MovementReferenceNumber);
			yield return (CommonResStrings.DeclarationType, messageProvider.DeclarationType);
			yield return (CommonResStrings.AdditionalDeclarationType, messageProvider.AdditionalDeclarationType);
			yield return (CommonResStrings.DeclarationAcceptanceDate, messageProvider.DeclarationAcceptanceDate.ToShortDateString());
			yield return (CommonResStrings.ReleaseDate, messageProvider.ReleaseDate.ToShortDateString());
			yield return (CommonResStrings.ResponseDateLimit, messageProvider.ResponseDateLimit.ToShortDateString());
			yield return (CommonResStrings.PreferredPaymentMethod, messageProvider.PreferredPaymentMethod);
			yield return (CommonResStrings.Remarks, messageProvider.Remarks);
		}
	}
}
