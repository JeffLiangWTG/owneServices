using System.Collections.Generic;
using Enterprise.Customs.IE.Messaging.AES;

namespace Enterprise.Customs.IE.Business.AES
{
	public class CC571MessageInterpreter : InboundMessageInterpreter<CC571CProvider>
	{
		public CC571MessageInterpreter(AESInboundEDIMessage message, CC571CProvider provider) : base(message, provider)
		{
		}

		protected override string Summary => Res.GetString(
			"78D3F471-09EB-4D75-8BD0-5E091EFF20B8",
			"A Re-Export Notification Registration message has been received from Customs for Job {0} through the IE571 message.",
			relatedJob.JobNumber
		);

		protected override IEnumerable<(string Key, string Value)> GetMessageDetails()
		{
			yield return (CommonResStrings.Status, Res.GetString("E816C0A3-E56C-4FB8-B35E-31753AAD2129", "REL- Released for Export"));
			yield return (CommonResStrings.LocalReferenceNumber, provider.LocalReferenceNumber);
			yield return (CommonResStrings.MovementReferenceNumber, provider.MovementReferenceNumber);
			yield return (CommonResStrings.ReExportNotificationRegistrationDate, provider.ReExportNotificationRegistrationDate.ToShortDateString());
		}
	}
}
