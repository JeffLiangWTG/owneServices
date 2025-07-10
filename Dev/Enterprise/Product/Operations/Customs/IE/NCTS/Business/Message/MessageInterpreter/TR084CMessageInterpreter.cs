using System.Collections.Generic;
using Enterprise.Customs.IE.Business;
using Enterprise.Customs.IE.NCTS.Messaging;

namespace Enterprise.Customs.IE.NCTS.Business
{
	public class TR084CMessageInterpreter : InboundMessageInterpreter<TR084CProvider>
	{
		public TR084CMessageInterpreter(Enterprise.Messaging.Business.EDIMessage message, TR084CProvider provider) : base(message, provider)
		{
		}

		protected override string Summary => Res.GetString("8520C0BF-7615-474D-918B-1EF55FEA749C", "A Request Document Presentation (TR084) message has been received for Job {0}.", relatedJob.JobNumber);

		protected override IEnumerable<(string Key, string Value)> GetMessageDetails()
		{
			yield return (CommonResStrings.MovementReferenceNumber, provider.MRN);
			yield return (CommonResStrings.LocalReferenceNumber, provider.LRN);
			yield return (CommonResStrings.RequestDate, provider.RequestDate.ToLongTimeString());
			yield return (CommonResStrings.DateLimit, provider.DateLimit.ToLongTimeString());
		}

		protected override IEnumerable<(string summary, IEnumerable<(string key, string value)>)> GetAdditionalMessageDetails()
		{
			foreach (var addInfo in provider.AdditionalInformations)
			{
				yield return (Res.GetString("E9F40F8E-6EA1-41C1-9D08-9637FDB296E5", "Additional Information"), new (string, string)[]
				{
					(CommonResStrings.DocumentType, addInfo.DocumentType),
					(CommonResStrings.DocumentComplementaryInformation, addInfo.DocumentComplementaryInformation)
				});
			}
		}
	}
}
