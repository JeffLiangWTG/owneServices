using System.Collections.Generic;
using Enterprise.Customs.IE.Messaging.AES;

namespace Enterprise.Customs.IE.Business.AES
{
	public class CC522MessageInterpreter : InboundMessageInterpreter<CC522CProvider>
	{
		public CC522MessageInterpreter(Enterprise.Messaging.Business.EDIMessage message, CC522CProvider provider) : base(message, provider)
		{
		}

		protected override string Summary => Res.GetString(
			"901CF7E2-D62B-4893-A62C-D177A0C90497",
			"An Exit Release Rejection message has been received from Customs for Job {0} through the IE522 message.",
			relatedJob.JobNumber
		);

		protected override IEnumerable<(string Key, string Value)> GetMessageDetails()
		{
			yield return (CommonResStrings.MovementReferenceNumber, provider.MovementReferenceNumber);
			yield return (Res.GetString("E81A6525-B535-4A21-A6F4-F41CFEE345C4", "Exit rejection motivation code"), provider.ExitRejectionMotivationCode);
			yield return (Res.GetString("E81A6525-B535-4A21-A6F4-F41CFEE345C5", "Exit rejection motivation"), provider.ExitRejectionMotivation);
		}
	}
}
