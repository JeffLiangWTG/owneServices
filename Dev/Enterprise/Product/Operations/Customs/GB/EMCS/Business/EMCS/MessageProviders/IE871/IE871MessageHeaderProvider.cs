using CargoWise.Customs.GB.MessageContracts.EMCS;
using Enterprise.Customs.EU.EMCS.Business;

namespace Enterprise.Customs.GB.EMCS.Business
{
	public sealed class IE871MessageHeaderProvider : MessageHeaderProvider<IE871HeaderProvider>, IIE871MessageHeader
	{
		public IE871MessageHeaderProvider(EMCSJobDeclaration emcsJobDeclaration, string reasonForShortage) : base(emcsJobDeclaration)
		{
			this.reasonForShortage = reasonForShortage;
		}
		readonly string reasonForShortage;

		IEMCSHeader IEMCSMessageHeader.Header => header ?? (header = new IE871HeaderProvider(emcsJobDeclaration, reasonForShortage));

		public bool IsDeclarantTypeConsignor => emcsJobDeclaration.JE_DeclarantType == EMCSEntryTypeList.Codes.Consignor;
	}
}
