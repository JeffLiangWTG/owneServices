using System.Collections.Generic;
using Enterprise.Customs.IE.Messaging.AES;

namespace Enterprise.Customs.IE.Business.AES
{
	public class CC599MessageInterpreter : InboundMessageInterpreter<CC599CProvider>
	{
		public CC599MessageInterpreter(AESInboundEDIMessage message, CC599CProvider provider) : base(message, provider)
		{
		}

		protected override string Summary => Res.GetString(
			"7778441B-7089-4C4E-A638-5B62A2584993",
			"An Export Notification message has been received from Customs for Job {0} through the IE599 message.",
			relatedJob.JobNumber
		);

		protected override IEnumerable<(string Key, string Value)> GetMessageDetails()
		{
			yield return (CommonResStrings.Status, provider.Status);
			yield return (CommonResStrings.LocalReferenceNumber, provider.LocalReferenceNumber);
			yield return (CommonResStrings.MovementReferenceNumber, provider.MovementReferenceNumber);
			yield return (Res.GetString("937475DA-98C4-489F-AAE6-F5EEB0D5D4A5", "Exit Control Code"), provider.ExitResultCode);
			yield return (Res.GetString("6C347F3F-EBC7-4395-9283-C2DB48BAEE78", "Exit Date"), provider.ExitDate.ToShortDateString());
			yield return (Res.GetString("840ACB72-B220-41BB-A0E7-F2F7F0A95DCF", "Exit Stopped Date"), provider?.ExitStoppedDate.ToShortDateString());
			yield return (Res.GetString("614E74D8-F2EA-45D8-9CC7-8F52F144742C", "State of Seals"), provider.StateofSeals);
		}
	}
}
