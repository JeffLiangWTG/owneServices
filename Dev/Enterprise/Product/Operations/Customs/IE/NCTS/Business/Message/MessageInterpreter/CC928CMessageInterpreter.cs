using System.Collections.Generic;
using Enterprise.Customs.IE.Business;
using Enterprise.Customs.IE.NCTS.Messaging;

namespace Enterprise.Customs.IE.NCTS.Business
{
	public class CC928CMessageInterpreter : InboundMessageInterpreter<CC928CProvider>
	{
		public CC928CMessageInterpreter(NCTSInboundEDIMessage message, CC928CProvider provider) : base(message, provider)
		{
		}

		protected override string Summary => Res.GetString(
			"24F2BB1C-26FB-4D91-930D-249837E5EA14",
			"A Positive Acknowledgement message has been received from Customs for Job {0} through the IE928 message.",
		relatedJob.JobNumber
		);

		protected override IEnumerable<(string Key, string Value)> GetMessageDetails()
		{
			yield return (CommonResStrings.LocalReferenceNumber, provider.LocalReferenceNumber);
			yield return (CommonResStrings.ReferenceNumber, provider.ReferenceNumber);
		}
	}
}
