using System.Collections.Generic;
using System.Linq;
using Enterprise.Customs.IE.Messaging;

namespace Enterprise.Customs.IE.Business.AIS
{
	public class IM484MessageInterpreter : InboundMessageInterpreter<IIM484Provider>
	{
		public IM484MessageInterpreter(InboundEDIMessage message, IM484Provider provider) : base(message, provider)
		{
			this.messageProvider = provider;
		}

		readonly IM484Provider messageProvider;

		protected override string Summary => Res.GetString("11212725-00E9-4E3C-A943-3A7DE4DC9234", "A Document Presentation Request (IM484) message has been received for Job {0}.", relatedJob.JobNumber);

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
