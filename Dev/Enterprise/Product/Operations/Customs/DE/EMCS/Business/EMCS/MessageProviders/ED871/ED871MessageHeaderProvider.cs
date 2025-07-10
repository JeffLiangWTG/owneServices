using CargoWise.Customs.DE.MessageContracts.EMCS;
using Enterprise.Customs.EU.EMCS.Business;

namespace Enterprise.Customs.DE.EMCS.Business
{
	public class ED871MessageHeaderProvider : MessageHeaderProvider<ED871HeaderProvider>, IED871MessageHeader
	{
		public ED871MessageHeaderProvider(EMCSJobDeclaration emcs, string reasonForShortage)
			: base(emcs)
		{
			this.reasonForShortage = reasonForShortage;
		}
		readonly string reasonForShortage;

		public bool IsDeclarantTypeConsignor => emcsJobDeclaration.JE_DeclarantType == EMCSEntryTypeList.Codes.Consignor;

		IEMCSHeader IEMCSMessageHeader.Header => header ?? (header = new ED871HeaderProvider(emcsJobDeclaration, reasonForShortage));
	}
}
