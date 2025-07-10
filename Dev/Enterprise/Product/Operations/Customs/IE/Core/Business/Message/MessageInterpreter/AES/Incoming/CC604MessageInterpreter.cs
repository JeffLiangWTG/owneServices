using System.Collections.Generic;
using Enterprise.Customs.IE.Messaging.AES;

namespace Enterprise.Customs.IE.Business.AES
{
	public class CC604MessageInterpreter : InboundMessageInterpreter<CC604CProvider>
	{
		public CC604MessageInterpreter(AESInboundEDIMessage message, CC604CProvider provider) : base(message, provider)
		{
		}

		protected override string Summary => Res.GetString(
			"BC40B697-9384-45FD-9720-93D5971C1471",
			"An Exit Summary Declaration Amendment Acceptance message has been received from Customs for Job {0} through the IE604 message.",
			relatedJob.JobNumber
		);

		protected override IEnumerable<(string Key, string Value)> GetMessageDetails()
		{
			yield return (CommonResStrings.Status, Res.GetString("98B81B50-C420-4C47-AF38-92C0A8852072", "Amendment Accepted by Customs (ACC)"));
			yield return (CommonResStrings.MovementReferenceNumber, provider.MovementReferenceNumber);
			yield return (CommonResStrings.AmendmentSubmissionDateAndTime, provider.AmendmentSubmissionDateTime.ToLongTimeString());
			yield return (CommonResStrings.AmendmentAcceptanceDateAndTime, provider.AmendmentAcceptanceDateTime.ToLongTimeString());
		}
	}
}
