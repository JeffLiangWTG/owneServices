using CargoWise.Customs.IE.MessageContracts.EMCS.Interfaces;

namespace Enterprise.Customs.IE.EMCS.Business
{
	public class IE871MessageHeaderProvider : MessageHeaderProvider<IE871HeaderProvider>, IEMCSMessageHeader
	{
		public IE871MessageHeaderProvider(EMCSJobDeclaration emcsJobDeclaration, string reasonForShortage) : base(emcsJobDeclaration)
		{
			this.reasonForShortage = reasonForShortage;
		}
		readonly string reasonForShortage;

		IEMCSHeader IEMCSMessageHeader.Header => header ?? (header = new IE871HeaderProvider(emcsJobDeclaration, reasonForShortage));
	}
}
