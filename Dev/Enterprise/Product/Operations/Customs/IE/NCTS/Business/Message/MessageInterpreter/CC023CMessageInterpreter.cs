using System.Collections.Generic;
using Enterprise.Customs.IE.Business;
using Enterprise.Customs.IE.NCTS.Messaging;

namespace Enterprise.Customs.IE.NCTS.Business
{
	public class CC023CMessageInterpreter : InboundMessageInterpreter<CC023CProvider>
	{
		public CC023CMessageInterpreter(Enterprise.Messaging.Business.EDIMessage message, CC023CProvider provider) : base(message, provider)
		{
		}

		protected override string Summary => Res.GetString(
			"09144D34-AA6B-4E4B-AEE1-03CE9D2C53F3",
			"A Guarantor Notification (IE023) message has been received for Job {0}.",
			relatedJob.JobNumber
		);

		protected override IEnumerable<(string Key, string Value)> GetMessageDetails()
		{
			yield return (CommonResStrings.MovementReferenceNumber, provider.MRN);
			yield return (NctsCommonResStrings.DeclarationAcceptedDate, provider.DeclarationAcceptanceDate.ToLongTimeString());
			yield return (Res.GetString("A108108A-7BB2-403B-9BBA-93592E7F7120", "Customs Office Of Recovery At Departure"), provider.CustomsOfficeOfRecoveryAtdeparture);
			yield return (Res.GetString("5C17D252-89BA-4403-A8D1-10795C73C154", "Guarantor's Name"), provider.NameOfGuarantor);
			yield return (Res.GetString("929A9576-6E82-4BD9-AAF7-1E53FDD450ED", "Guarantor's Address"), provider.AddressOfGuarantor);
			yield return (Res.GetString("33A30DFE-DD19-47CE-97F7-686C20BA0FB8", "Guarantor Notification Date"), provider.GuarantorNotificationDate.ToLongTimeString());
			yield return (Res.GetString("70D8BC81-FE30-4244-BB9E-7DF4D666A34E", "Guarantor Notification Text"), provider.GuarantorNotificationText);
		}
	}
}
