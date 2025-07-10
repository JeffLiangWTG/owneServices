using System.Collections.Generic;
using Enterprise.Customs.IE.Business;
using Enterprise.Customs.IE.NCTS.Messaging;

namespace Enterprise.Customs.IE.NCTS.Business
{
	public class CC045CMessageInterpreter : InboundMessageInterpreter<CC045CProvider>
	{
		public CC045CMessageInterpreter(Enterprise.Messaging.Business.EDIMessage message, CC045CProvider provider) : base(message, provider)
		{
		}

		protected override string Summary => Res.GetString(
			"1C7F7C9F-4718-459D-A51B-BB34CCCD324A",
			"A Write-Off Notification (IE045) message has been received for Job {0}.",
		relatedJob.JobNumber
		);

		protected override IEnumerable<(string Key, string Value)> GetMessageDetails()
		{
			yield return (CommonResStrings.MovementReferenceNumber, provider.MRN);
			yield return (Res.GetString("3995A832-29DE-4E8D-A195-D9ED563BDBB5", "Write-off Date"), provider.WriteOffDate.ToShortDateString());
		}
	}
}
