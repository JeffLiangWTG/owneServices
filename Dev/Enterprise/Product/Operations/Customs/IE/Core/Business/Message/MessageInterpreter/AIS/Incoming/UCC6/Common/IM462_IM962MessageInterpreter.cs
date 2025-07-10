using System.Collections.Generic;
using Enterprise.Customs.IE.Messaging;

namespace Enterprise.Customs.IE.Business.AIS
{
	public class IM462_IM962MessageInterpreter : InboundMessageInterpreter<IM462_IM962Provider>
	{
		public IM462_IM962MessageInterpreter(Enterprise.Messaging.Business.EDIMessage message, IM462_IM962Provider provider) : base(message, provider)
		{
		}

		protected override string Summary => Res.GetString("a1560c01-0d46-44cb-b820-b2e17f01e328", "An Amendment Request ({0}) message has been received for Job {1}.", "IM" + message.EM_MessageType, relatedJob.JobNumber);

		protected override IEnumerable<(string Key, string Value)> GetMessageDetails()
		{
			yield return (CommonResStrings.MovementReferenceNumber, provider.MovementReferenceNumber);
			yield return (CommonResStrings.CaseId, provider.CaseId);
			yield return (Res.GetString("402C5F12-5BA0-4724-9FB0-7E2DAF8D3022", "Amend Reason"), provider.AmendReason);
		}
	}
}
