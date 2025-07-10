using System.Collections.Generic;
using Enterprise.Customs.IE.Messaging;

namespace Enterprise.Customs.IE.Business.AES
{
	public class EX582MessageInterpreter : InboundMessageInterpreter<EX582Provider>
	{
		public EX582MessageInterpreter(InboundEDIMessage message, EX582Provider provider) : base(message, provider)
		{
		}

		protected override string Summary => Res.GetString(
			"88E18F7F-866F-46B0-A5A7-0328ECCDBC50",
			"A Document Request message has been received from Customs for Job {0} through the EX582 message. ",
			relatedJob.JobNumber
		);

		protected override IEnumerable<(string Key, string Value)> GetMessageDetails()
		{
			yield return (CommonResStrings.LocalReferenceNumber, provider.LRN);
			yield return (CommonResStrings.MovementReferenceNumber, provider.MRN);
			yield return (Res.GetString("D8819C71-56A4-41C3-BCFF-BA4C6BEC7305", "Request Date"), provider.RequestDate.ToLongTimeString());
			yield return (Res.GetString("8D26FE3E-2913-4120-ABA0-BFE4F3FADC17", "Provide by Date"), provider.DateLimit.ToLongTimeString());

			var addInfos = provider.AdditionalInformations;
			foreach (var addInfo in addInfos)
			{
				yield return (CommonResStrings.DocumentType, addInfo.DocumentType);
				yield return (Res.GetString("578E8AA0-0EA3-4CAA-ACFE-BE044501B6B2", "Document Information"), addInfo.RequestInformation);
			}
		}
	}
}
