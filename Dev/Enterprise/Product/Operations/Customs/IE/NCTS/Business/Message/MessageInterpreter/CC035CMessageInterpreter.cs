using System.Collections.Generic;
using Enterprise.Customs.IE.Business;
using Enterprise.Customs.IE.NCTS.Messaging;

namespace Enterprise.Customs.IE.NCTS.Business
{
	public class CC035CMessageInterpreter : InboundMessageInterpreter<CC035CProvider>
	{
		public CC035CMessageInterpreter(Enterprise.Messaging.Business.EDIMessage message, CC035CProvider provider) : base(message, provider)
		{
		}

		protected override string Summary => Res.GetString(
			"73CFE966-5D9B-4133-8091-2E7F1BC46346",
			"Recovery Notification Message (IE035) has been received. A Competent Authority of Recovery has initiated Recovery of Transit Declaration for Job {0}.",
		relatedJob.JobNumber
		);

		protected override IEnumerable<(string Key, string Value)> GetMessageDetails()
		{
			yield return (CommonResStrings.MovementReferenceNumber, provider.MRN);
			yield return (NctsCommonResStrings.DeclarationAcceptedDate, provider.DeclarationAcceptanceDate.ToShortDateString());
			yield return (Res.GetString("86819849-C3E8-4F54-9BCF-CC20FB6DB8A6", "Recovery Notification Date"), provider.RecoveryNotificationDate.ToShortDateString());
			yield return (Res.GetString("95986C31-B045-4325-A8E2-044068537E1C", "Recovery Notification Text"), provider.RecoveryNotificationText);
			yield return (Res.GetString("CC5DF5F2-530B-4EE5-857A-8CECDD359515", "Amount Claimed"), provider.AmountClaimed);
			yield return (Res.GetString("240B2515-51DA-465F-9A96-ECDB9394AD45", "Customs Office of Recovery at Departure"), GetCodeAndDescription(provider.CustomsOfficeOfRecoveryAtDeparture, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice));
		}
	}
}
