using System.Collections.Generic;
using Enterprise.Customs.IE.Business;
using Enterprise.Customs.IE.NCTS.Messaging;

namespace Enterprise.Customs.IE.NCTS.Business
{
	public class TR064CMessageInterpreter : InboundMessageInterpreter<TR064CProvider>
	{
		public TR064CMessageInterpreter(Enterprise.Messaging.Business.EDIMessage message, TR064CProvider provider) : base(message, provider)
		{
		}
		protected override string Summary => Res.GetString("C06427B3-4F4B-4E10-8391-2960611AE049", "A Request Declaration Invalidation (TR064) message has been received for Job {0}.", relatedJob.JobNumber);

		protected override IEnumerable<(string Key, string Value)> GetMessageDetails()
		{
			yield return (CommonResStrings.MovementReferenceNumber, provider.MRN);
			yield return (Res.GetString("E268942A-3A4E-4110-A487-CD22EAEBB463", "Case Id"), provider.CaseId);
			yield return (Res.GetString("69CC368A-E792-4869-B4A5-9F9B842CF3D4", "Remarks"), provider.Remarks);
		}
	}
}
