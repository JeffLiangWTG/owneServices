using System.Collections.Generic;
using Enterprise.Customs.IE.Business;
using Enterprise.Customs.IE.NCTS.Messaging;

namespace Enterprise.Customs.IE.NCTS.Business
{
	public class TR082CMessageInterpreter : InboundMessageInterpreter<TR082CProvider>
	{
		public TR082CMessageInterpreter(Enterprise.Messaging.Business.EDIMessage message, TR082CProvider provider) : base(message, provider)
		{
		}

		protected override string Summary => Res.GetString("A964305E-894A-4A60-AC17-910F6A3DA0C3", "A Documents Request (TR082) message has been received for Job {0}.", relatedJob.JobNumber);

		protected override IEnumerable<(string Key, string Value)> GetMessageDetails()
		{
			yield return (CommonResStrings.MovementReferenceNumber, provider.MRN);
			yield return (CommonResStrings.LocalReferenceNumber, provider.LRN);
			yield return (Res.GetString("108B0BAC-CAD3-4D38-95C2-6C1893AF7637", "Request Date"), provider.RequestDate.ToLongTimeString());
			yield return (Res.GetString("84F64BEB-DF44-4E03-96E4-C3CA0A426AB3", "Date Limit"), provider.DateLimit.ToLongTimeString());
		}

		protected override IEnumerable<(string summary, IEnumerable<(string key, string value)>)> GetAdditionalMessageDetails()
		{
			foreach (var additionalInformation in provider.AdditionalInformations)
			{
				yield return (string.Empty, new (string, string)[]
				{
					(Res.GetString("6BA6720B-9716-4F29-A2D0-8DDF2276D3F6", "Additional Information Document Type"), additionalInformation.DocumentType),
					(Res.GetString("7829E904-66AA-48FC-AB36-D571187D3DBE", "Additional Information Document Complementary Information"), additionalInformation.DocumentComplementaryInformation),
				});
			}
		}
	}
}
