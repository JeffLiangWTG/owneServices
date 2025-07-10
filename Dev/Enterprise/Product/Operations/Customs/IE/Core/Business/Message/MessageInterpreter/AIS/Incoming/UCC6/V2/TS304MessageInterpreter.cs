using System.Collections.Generic;
using Enterprise.Customs.IE.Messaging;

namespace Enterprise.Customs.IE.Business.AIS
{
	public class TS304MessageInterpreter : InboundMessageInterpreter<TS304Provider>
	{
		public TS304MessageInterpreter(Enterprise.Messaging.Business.EDIMessage message, TS304Provider provider) : base(message, provider)
		{
		}

		protected override string Summary => Res.GetString("FBFF0506-CCA1-4DCE-AE4B-CDB554D862E7", "A Temporary Storage Declaration Amendment Request Registration (TS304) message has been received for Job {0}.", relatedJob.JobNumber);

		protected override IEnumerable<(string Key, string Value)> GetMessageDetails()
		{
			yield return (CommonResStrings.MovementReferenceNumber, provider.MovementReferenceNumber);
			yield return (CommonResStrings.AcceptanceDate, provider.DateOfAcceptance.ToLongTimeString());
			yield return (CommonResStrings.Remarks, provider.Remarks);
		}
	}
}
