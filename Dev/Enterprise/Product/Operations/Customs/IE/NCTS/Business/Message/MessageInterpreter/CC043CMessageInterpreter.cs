using System.Collections.Generic;
using Enterprise.Customs.IE.Business;
using Enterprise.Customs.IE.NCTS.Messaging;

namespace Enterprise.Customs.IE.NCTS.Business
{
	public class CC043CMessageInterpreter : InboundMessageInterpreter<CC043CProvider>
	{
		public CC043CMessageInterpreter(Enterprise.Messaging.Business.EDIMessage message, CC043CProvider provider) : base(message, provider)
		{
		}

		protected override string Summary => Res.GetString(
			"46BDBA92-93EC-48DB-89E3-42978BD2506C",
			"An Unloading Permission (IE043) message has been received for Job {0}.",
		relatedJob.JobNumber
		);

		protected override IEnumerable<(string Key, string Value)> GetMessageDetails()
		{
			yield return (CommonResStrings.MovementReferenceNumber, provider.MRN);
			yield return (Res.GetString("C7BCBABB-91D4-4C21-B2F5-AE671E828F6B", "Customs Office of Destination (Actual)"), provider.CustomsOfficeOfDestinationActual);
			yield return (Res.GetString("0075B281-C624-4903-AA94-B0DB9E7DCDAF", "Trader at Destination "), provider.TraderAtDestination);
			yield return (Res.GetString("F1CAD863-2728-4510-8BA7-BBCA5D8848BA", "Address"), provider.Address);
		}
	}
}
