using System.Collections.Generic;
using Enterprise.Customs.IE.Messaging;

namespace Enterprise.Customs.IE.Business.AIS
{
	public class IM482MessageInterpreter : InboundMessageInterpreter<IM482Provider>
	{
		public IM482MessageInterpreter(Enterprise.Messaging.Business.EDIMessage message, IM482Provider provider) : base(message, provider)
		{
			messageProvider = provider;
		}

		readonly IM482Provider messageProvider;

		protected override string Summary => Res.GetString("7FC45EB8-FEFB-4AF4-97D5-06DD1839E3C3", "A Documents Request (IM482) message has been received for Job {0}.", relatedJob.JobNumber);

		protected override IEnumerable<(string Key, string Value)> GetMessageDetails()
		{
			yield return (CommonResStrings.MovementReferenceNumber, messageProvider.MRN);
			yield return (CommonResStrings.CaseId, messageProvider.CaseId);
			yield return (CommonResStrings.RequestDate, messageProvider.RequestDate.ToLongTimeString());
			yield return (CommonResStrings.DateLimit, messageProvider.DateLimit.ToLongTimeString());
		}

		protected override IEnumerable<(string summary, IEnumerable<(string key, string value)>)> GetAdditionalMessageDetails()
		{
			var sequenceNumber = 1;
			foreach (var additionalDocument in messageProvider.AdditionalInformations)
			{
				yield return (Res.GetString("EE107FD0-8473-4B24-A4CE-933A25E0EE85", "Document Additional Information: {0}", sequenceNumber++), new (string, string)[]
				{
					(Res.GetString("6C710D9F-C93E-478D-8D12-037D31CA65D4", "Type"), additionalDocument.DocumentType),
					(Res.GetString("2A0908C6-0D6E-40D1-BE81-207C8700A362", "Complementary Information"), additionalDocument.RequestInformation)
				});
			}
		}
	}
}
