using System.Collections.Generic;
using Enterprise.Customs.IE.Messaging.AES;

namespace Enterprise.Customs.IE.Business.AES
{
	public class CC529MessageInterpreter : InboundMessageInterpreter<CC529CProvider>
	{
		public CC529MessageInterpreter(AESInboundEDIMessage message, CC529CProvider provider) : base(message, provider) { }

		protected override string Summary => Res.GetString(
			"E79796C0-5F85-4D62-9789-081F58A4BB55",
			"A Release Response message has been received from Customs for Job {0} through the IE529 message.",
			relatedJob.JobNumber
		);

		protected override IEnumerable<(string Key, string Value)> GetMessageDetails()
		{
			yield return (CommonResStrings.Status, Res.GetString("C520C5D5-C97B-4FD3-8BE3-CF49A218C424", "Goods Released for Export"));
			yield return (CommonResStrings.LocalReferenceNumber, provider.LocalReferenceNumber);
			yield return (CommonResStrings.MovementReferenceNumber, provider.MovementReferenceNumber);
			yield return (CommonResStrings.ReleaseDate, provider.ReleaseDate.ToShortDateString());
		}
	}
}
