using System.Collections.Generic;
using Enterprise.Customs.IE.Business;
using Enterprise.Customs.IE.NCTS.Messaging;

namespace Enterprise.Customs.IE.NCTS.Business
{
	public class TR062CMessageInterpreter : InboundMessageInterpreter<TR062CProvider>
	{
		public TR062CMessageInterpreter(Enterprise.Messaging.Business.EDIMessage message, TR062CProvider provider) : base(message, provider)
		{
		}

		protected override string Summary => Res.GetString(
			"921EB653-7A6D-474A-A17F-1B91C97E914E",
			"A Request Declaration Amendment (TR062) message has been received for Job {0}.",
		relatedJob.JobNumber
		);

		protected override IEnumerable<(string Key, string Value)> GetMessageDetails()
		{
			yield return (CommonResStrings.MovementReferenceNumber, provider.MRN);
			yield return (CommonResStrings.CaseId, provider.CaseID);
			yield return (CommonResStrings.Remarks, provider.Remarks);
		}
	}
}
