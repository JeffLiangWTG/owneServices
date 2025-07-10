using System.Collections.Generic;
using Enterprise.Customs.IE.Messaging;

namespace Enterprise.Customs.IE.Business.AIS
{
	public class IM438MessageInterpreter : InboundMessageInterpreter<IM438Provider>
	{
		public IM438MessageInterpreter(Enterprise.Messaging.Business.EDIMessage message, IM438Provider provider) : base(message, provider)
		{
		}

		protected override string Summary => Res.GetString("CCDD052A-31A0-49BF-B9EB-CC81F2761FEA", "A Reminder for Providing Additional Documents (IM438) message has been received for Job {0}.", relatedJob.JobNumber);

		protected override IEnumerable<(string Key, string Value)> GetMessageDetails()
		{
			yield return (CommonResStrings.LocalReferenceNumber, provider.LocalReferenceNumber);
			yield return (CommonResStrings.MovementReferenceNumber, provider.MovementReferenceNumber);
			yield return (CommonResStrings.RequestDate, provider.RequestDate.ToShortDateString());
			yield return (CommonResStrings.ExpirationDate, provider.ExpirationDate.ToShortDateString());
		}
	}
}
