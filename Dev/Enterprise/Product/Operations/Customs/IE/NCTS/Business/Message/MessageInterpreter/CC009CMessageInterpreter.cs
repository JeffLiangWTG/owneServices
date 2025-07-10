using System.Collections.Generic;
using Enterprise.Customs.IE.Business;
using Enterprise.Customs.IE.NCTS.Messaging;

namespace Enterprise.Customs.IE.NCTS.Business
{
	public class CC009CMessageInterpreter : InboundMessageInterpreter<CC009CProvider>
	{
		public CC009CMessageInterpreter(Enterprise.Messaging.Business.EDIMessage message, CC009CProvider provider) : base(message, provider)
		{
		}

		protected override string Summary => Res.GetString(
			"B140CFCE-5857-4EE5-A14D-D967D409358B",
			"An Invalidation Decision Message (IE009) has been received for Job {0}.",
			relatedJob.JobNumber
		);

		protected override IEnumerable<(string Key, string Value)> GetMessageDetails()
		{
			yield return (CommonResStrings.MovementReferenceNumber, provider.MovementReferenceNumber);
			yield return (Res.GetString("51E1B628-D4A9-4954-A38C-9E2A5082B77F", "Request Date & Time"), provider.InvalidationRequestDateAndTime.ToLongTimeString());
			yield return (Res.GetString("0E55AF52-3B40-432D-BDFD-8B49FCE1BD5D", "Decision Date & Time"), provider.InvalidationDecisionDateAndTime.ToLongTimeString());
			yield return (Res.GetString("55FBD434-E296-4CD3-92C5-DA3EDE60D1F2", "Decision"), InvalidationDecision());
			yield return (Res.GetString("7E24A830-B8D3-460D-84E6-780A5629CF3B", "Initiated by Customs"), provider.IsInitiatedByCustoms.ToString());
			yield return (Res.GetString("F19AE7DA-0DCD-443C-94FA-476E3F73C311", "Justification"), provider.Justification);
		}

		string InvalidationDecision()
		{
			return provider.IsInvalidated ?
				Res.GetString("9BB072EA-2FE2-4553-9864-4283F34789AB", "Invalidated") :
				Res.GetString("1487BA47-5EFB-4316-86BD-BDD1A490AEA0", "Not Invalidated");
		}
	}
}
