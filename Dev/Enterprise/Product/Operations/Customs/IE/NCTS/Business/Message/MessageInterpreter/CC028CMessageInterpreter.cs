using System.Collections.Generic;
using Enterprise.Customs.IE.Business;
using Enterprise.Customs.IE.NCTS.Messaging;

namespace Enterprise.Customs.IE.NCTS.Business
{
	public class CC028CMessageInterpreter : InboundMessageInterpreter<CC028CProvider>
	{
		public CC028CMessageInterpreter(Enterprise.Messaging.Business.EDIMessage message, CC028CProvider provider) : base(message, provider)
		{
		}

		protected override string Summary => Res.GetString(
			"2E52C72E-F21D-4ECF-AF6A-E6DD1CEBB404",
			"A MRN Allocated (IE028) message has been received for Job {0}.",
		relatedJob.JobNumber
		);

		protected override IEnumerable<(string Key, string Value)> GetMessageDetails()
		{
			yield return (CommonResStrings.MovementReferenceNumber, provider.MRN);
			yield return (NctsCommonResStrings.DeclarationAcceptedDate, provider.DeclarationAcceptanceDate.ToShortDateString());
		}
	}
}
