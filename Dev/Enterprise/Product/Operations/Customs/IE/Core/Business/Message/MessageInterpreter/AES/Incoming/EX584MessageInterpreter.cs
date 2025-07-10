using System.Collections.Generic;
using Enterprise.Customs.IE.Messaging.AES;

namespace Enterprise.Customs.IE.Business.AES
{
	public class EX584MessageInterpreter : InboundMessageInterpreter<EX584Provider>
	{
		public EX584MessageInterpreter(InboundEDIMessage message, EX584Provider provider) : base(message, provider)
		{
		}

		protected override string Summary => Res.GetString(
			"10882202-84B2-4CDE-85DB-C88539F44F1D",
			"A Document Request Presentation message has been received from Customs for Job {0} through the EX584 message stating that a list of following documents will need to be presented physically. ",
			relatedJob.JobNumber
		);

		protected override IEnumerable<(string Key, string Value)> GetMessageDetails()
		{
			yield return (CommonResStrings.LocalReferenceNumber, provider.LocalReferenceNumber);
			yield return (CommonResStrings.MovementReferenceNumber, provider.MovementReferenceNumber);
			yield return (CommonResStrings.RequestDate, provider.RequestDate.ToLongTimeString());
			yield return (CommonResStrings.DateLimit, provider.DateLimit.ToLongTimeString());

			var addInfos = provider.AdditionalInformation;
			foreach (var addInfo in addInfos)
			{
				yield return (CommonResStrings.DocumentType, addInfo.DocumentType);
				yield return (CommonResStrings.DocumentComplementaryInformation, addInfo.RequestInformation);
			}
		}
	}
}
