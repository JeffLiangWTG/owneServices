using System.Collections.Generic;
using Enterprise.Customs.IE.Business;
using Enterprise.Customs.IE.NCTS.Messaging;

namespace Enterprise.Customs.IE.NCTS.Business
{
	public class CC229CMessageInterpreter : InboundMessageInterpreter<CC229CProvider>
	{
		public CC229CMessageInterpreter(Enterprise.Messaging.Business.EDIMessage message, CC229CProvider provider) : base(message, provider)
		{
		}

		protected override string Summary => Res.GetString(
			"E04F76EE-3E6C-443E-9ED6-CE2C4EAE76C9",
			"An Individual Guarantee voucher revocation Notification (IE229) message has been received for GRN {0}.",
			provider.GRN
		);

		protected override IEnumerable<(string Key, string Value)> GetMessageDetails()
		{
			yield return (Res.GetString("5C17D252-89BA-4403-A8D1-10795C73C154", "Guarantor's Name"), provider.GuarantorName);
			yield return (Res.GetString("929A9576-6E82-4BD9-AAF7-1E53FDD450ED", "Guarantor's Address"), provider.GuarantorAddress);
			yield return (NctsCommonResStrings.GRN, provider.GRN);
			yield return (NctsCommonResStrings.InvalidityDate, provider.InvalidityDate.ToShortDateString());
			yield return (NctsCommonResStrings.CustomsOfficeOfGuarantee, provider.CustomsOfficeOfGuarantee);
		}
	}
}
