using System.Collections.Generic;
using Enterprise.Customs.IE.Messaging.UCC5;

namespace Enterprise.Customs.IE.Business.AIS.UCC5
{
	public class IM429MessageInterpreter : InboundMessageInterpreter<IM429Provider>
	{
		public IM429MessageInterpreter(AISUCC5InboundEDIMessage message, IM429Provider provider) : base(message, provider)
		{
		}

		protected override string Summary => Res.GetString("0BEA346F-F111-4281-9E3A-40AE209468E3", "A Release Notification (IM429) message has been received for Job {0}.", relatedJob.JobNumber);

		protected override IEnumerable<(string Key, string Value)> GetMessageDetails()
		{
			yield return (CommonResStrings.DeclarationType, provider.DeclarationType);
			yield return (CommonResStrings.AdditionalDeclarationType, provider.AdditionalDeclarationType);
			yield return (CommonResStrings.LocalReferenceNumber, provider.LocalReferenceNumber);
			yield return (CommonResStrings.MovementReferenceNumber, provider.MovementReferenceNumber);
			yield return (CommonResStrings.DeclarationAcceptanceDate, provider.AcceptanceDate.ToShortDateString());
			yield return (CommonResStrings.ResponseDateLimit, provider.ResponseDateLimit.ToShortDateString());
			yield return (CommonResStrings.PreferredPaymentMethod, provider.PreferredPaymentMethod);
			yield return (CommonResStrings.Remarks, provider.Remarks);
		}
	}
}
