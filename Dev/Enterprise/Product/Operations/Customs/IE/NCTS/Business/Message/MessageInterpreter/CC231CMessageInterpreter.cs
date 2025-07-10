using System.Collections.Generic;
using Enterprise.Customs.IE.Business;
using Enterprise.Customs.IE.NCTS.Messaging;

namespace Enterprise.Customs.IE.NCTS.Business
{
	public class CC231CMessageInterpreter : InboundMessageInterpreter<CC231CProvider>
	{
		public CC231CMessageInterpreter(EDIMessage message, CC231CProvider provider) : base(message, provider)
		{
		}

		protected override string Summary => Res.GetString(
			"29FE8065-07E8-46F6-B6A6-BDAAB6CD24C0",
			"A Comprehensive Guarantee Cancellation Notification (IE231) message has been received for GRN {0}.",
			provider.GRN
		);

		protected override IEnumerable<(string Key, string Value)> GetMessageDetails()
		{
			yield return (Res.GetString("4838B4DF-74DA-4916-8612-8CC6FE869840", "Holder of Transit Procedure's Identification Number"), provider.HolderIdentificationNumber);
			yield return (NctsCommonResStrings.GRN, provider.GRN);
			yield return (NctsCommonResStrings.InvalidityDate, provider.InvalidityDate.ToShortDateString());
			yield return (NctsCommonResStrings.InvalidityReasonCode, provider.InvalidityReasonCode);
			yield return (NctsCommonResStrings.InvalidityReasonText, provider.InvalidityReasonText);
			yield return (NctsCommonResStrings.CustomsOfficeOfGuarantee, provider.CustomsOfficeOfGuarantee);
		}
	}
}
