using System.Collections.Generic;
using Enterprise.Customs.IE.Messaging.AES;

namespace Enterprise.Customs.IE.Business.AES
{
	public class CC509MessageInterpreter : InboundMessageInterpreter<CC509CProvider>
	{
		public CC509MessageInterpreter(AESInboundEDIMessage message, CC509CProvider provider) : base(message, provider)
		{
		}

		protected override string Summary => Res.GetString(
			"4E0A8690-F26B-48E5-922F-50F2A43B320A",
			"An Invalidation Decision message has been received from Customs for Job {0} through the IE509 message.",
			relatedJob.JobNumber);

		protected override IEnumerable<(string Key, string Value)> GetMessageDetails()
		{
			yield return (CommonResStrings.Status, Res.GetString("D2BE214B-F314-4BF0-B73B-175B1A337BE5", "Canceled (CAN)"));
			yield return (CommonResStrings.LocalReferenceNumber, provider.LocalReferenceNumber);
			yield return (CommonResStrings.MovementReferenceNumber, provider.MovementReferenceNumber);
			yield return (Res.GetString("67352F01-CC8B-4BC9-AADC-EC5A744AE050", "Invalidation Decision Date and Time"), provider.InvalidationDecisionDateAndTime.ToLongTimeString());
			yield return (Res.GetString("67352F01-CC8B-4BC9-AADC-EC5A744AE051", "Invalidation Request Date and Time"), provider.InvalidationRequestDateAndTime.ToLongTimeString());
			yield return (Res.GetString("67352F01-CC8B-4BC9-AADC-EC5A744AE052", "Initiated by Customs"), provider.IsInvalidationInitiatedByCustoms.ToString());
			yield return (CommonResStrings.InvalidationJustification, provider.InvalidationJustification);
		}
	}
}
