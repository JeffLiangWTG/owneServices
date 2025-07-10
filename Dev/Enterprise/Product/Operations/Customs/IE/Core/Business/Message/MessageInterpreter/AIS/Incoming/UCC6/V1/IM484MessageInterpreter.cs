using System.Collections.Generic;
using System.Linq;
using Enterprise.Customs.IE.Messaging;
using IM484Provider = Enterprise.Customs.IE.Messaging.UCC6.V1.IM484Provider;

namespace Enterprise.Customs.IE.Business.AIS.UCC6.V1
{
	public class IM484MessageInterpreter : InboundMessageInterpreter<IIM484Provider>
	{
		public IM484MessageInterpreter(Enterprise.Messaging.Business.EDIMessage message, IM484Provider provider) : base(message, provider)
		{
			this.messageProvider = provider;
		}

		readonly IM484Provider messageProvider;

		protected override string Summary => Res.GetString("a8698582-c7a0-469a-a226-150090ee997f", "A Document Presentation Request (IM484) message has been received for Job {0}.", relatedJob.JobNumber);

		protected override IEnumerable<(string Key, string Value)> GetMessageDetails()
		{
			yield return (CommonResStrings.MovementReferenceNumber, messageProvider.MovementReferenceNumber);
			yield return (CommonResStrings.LocalReferenceNumber, messageProvider.LocalReferenceNumber);
			yield return (CommonResStrings.RequestDate, messageProvider.RequestDate.ToShortDateString());
			yield return (CommonResStrings.DateLimit, messageProvider.DateLimit.ToShortDateString());
		}

		protected override IEnumerable<(string summary, IEnumerable<(string key, string value)>)> GetAdditionalMessageDetails()
		{
			var additionalInformations = messageProvider.AdditionalInformations.ToArray();
			for (var index = 0; index < additionalInformations.Length; index++)
			{
				yield return ($"Shipment Additional Information {index + 1}", new (string, string)[]
				{
					(CommonResStrings.DocumentType, additionalInformations[index].DocumentType),
					(CommonResStrings.DocumentComplementaryInformation, additionalInformations[index].RequestInformation)
				});
			}
		}
	}
}
