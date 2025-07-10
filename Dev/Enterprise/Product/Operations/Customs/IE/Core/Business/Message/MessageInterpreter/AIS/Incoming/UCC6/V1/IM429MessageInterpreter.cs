using System.Collections.Generic;
using Enterprise.Customs.IE.Messaging;
using IM429Provider = Enterprise.Customs.IE.Messaging.UCC6.V1.IM429Provider;

namespace Enterprise.Customs.IE.Business.AIS.UCC6.V1
{
	public class IM429MessageInterpreter : InboundMessageInterpreter<IIM429Provider>
	{
		public IM429MessageInterpreter(AISInboundEDIMessage message, IM429Provider provider) : base(message, provider)
		{
			messageProvider = provider;
		}

		readonly IM429Provider messageProvider;

		protected override string Summary => Res.GetString("b8736854-9cb4-48e6-b039-96ec0d981c2f", "A Customs Declaration (IM429) message has been received for Job {0}.", relatedJob.JobNumber);

		protected override IEnumerable<(string Key, string Value)> GetMessageDetails()
		{
			yield return (CommonResStrings.LocalReferenceNumber, messageProvider.LocalReferenceNumber);
			yield return (CommonResStrings.MovementReferenceNumber, messageProvider.MovementReferenceNumber);
			yield return (CommonResStrings.AdditionalDeclarationType, messageProvider.AdditionalDeclarationType);
			yield return (CommonResStrings.PreferredPaymentMethod, messageProvider.PreferredPaymentMethod);
			yield return (CommonResStrings.Remarks, messageProvider.Remarks);
		}
	}
}
