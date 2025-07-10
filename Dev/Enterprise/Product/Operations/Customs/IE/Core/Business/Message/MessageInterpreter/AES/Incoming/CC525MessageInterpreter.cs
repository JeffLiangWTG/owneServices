using System.Collections.Generic;
using Enterprise.Customs.IE.Messaging.AES;

namespace Enterprise.Customs.IE.Business.AES
{
	public class CC525MessageInterpreter : InboundMessageInterpreter<CC525CProvider>
	{
		public CC525MessageInterpreter(Enterprise.Messaging.Business.EDIMessage message, CC525CProvider provider) : base(message, provider)
		{
		}

		protected override string Summary => Res.GetString(
			"7752C121-E806-4A0F-8173-378C2DAB3B4C",
			"An Exit Release notification message has been received from Customs for Job {0} through the IE525 message.",
			relatedJob.JobNumber
		);

		protected override IEnumerable<(string Key, string Value)> GetMessageDetails()
		{
			yield return (CommonResStrings.Status, CommonResStrings.ReleasedForExit);
			yield return (CommonResStrings.MovementReferenceNumber, provider.MovementReferenceNumber);
			yield return (CommonResStrings.ReleaseDate, provider.ReleaseDate.ToShortDateString());
			yield return (CommonResStrings.StoringFlag, provider.IsStored.ToString());
		}
	}
}
