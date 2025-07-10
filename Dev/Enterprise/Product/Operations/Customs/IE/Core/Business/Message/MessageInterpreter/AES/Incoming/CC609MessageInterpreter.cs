using System.Collections.Generic;
using Enterprise.Customs.IE.Messaging.AES;

namespace Enterprise.Customs.IE.Business.AES
{
	public class CC609MessageInterpreter : InboundMessageInterpreter<CC609CProvider>
	{
		public CC609MessageInterpreter(AESInboundEDIMessage message, CC609CProvider provider) : base(message, provider)
		{
		}

		protected override string Summary => Res.GetString(
			"BD1A0DE8-5DFD-4092-8AA3-0E83F18FEDD7",
			"An EXS/REN Invalidation Decision message has been received from Customs for Job {0} through the IE609 message.",
			relatedJob.JobNumber
		);

		protected override IEnumerable<(string Key, string Value)> GetMessageDetails()
		{
			yield return (CommonResStrings.MovementReferenceNumber, provider.MovementReferenceNumber);
			yield return (CommonResStrings.InvalidationDecisionDateAndTime, provider.InvalidationDecisionDateTime.ToLongTimeString());
			yield return (CommonResStrings.InvalidationRequestDateAndTime, provider.InvalidationRequestDateTime.ToLongTimeString());
		}
	}
}
