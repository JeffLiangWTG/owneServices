using System.Collections.Generic;
using Enterprise.Customs.IE.Messaging.AES;

namespace Enterprise.Customs.IE.Business.AES
{
	public class CC582MessageInterpreter : InboundMessageInterpreter<CC582CProvider>
	{
		public CC582MessageInterpreter(InboundEDIMessage message, CC582CProvider provider) : base(message, provider)
		{
		}

		protected override string Summary => Res.GetString(
			"651C298C-83EC-492F-BE3C-4215892291ED",
			"A Request on Non-exited Export message has been received from Customs for Job {0} through the IE582 message.",
			relatedJob.JobNumber
		);

		protected override IEnumerable<(string Key, string Value)> GetMessageDetails()
		{
			yield return (CommonResStrings.MovementReferenceNumber, provider.MovementReferenceNumber);
			yield return (Res.GetString("142E6D58-B258-42CC-B28E-8DF2B5614A24", "Limit for Response Date"), provider.ResponseDateLimit.ToShortDateString());
			yield return (Res.GetString("6970ECCA-8A12-438A-9B94-5D04FEC7CA64", "Request on Non-Exited Export Date"), provider.NonExitedExportRequestDate.ToShortDateString());
		}
	}
}
