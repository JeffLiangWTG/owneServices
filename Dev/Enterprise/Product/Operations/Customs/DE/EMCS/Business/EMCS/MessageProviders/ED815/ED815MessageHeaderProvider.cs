using CargoWise.Customs.DE.MessageContracts.EMCS;

namespace Enterprise.Customs.DE.EMCS.Business
{
	public class ED815MessageHeaderProvider : MessageHeaderProvider<ED815HeaderProvider>, IEMCSMessageHeader
	{
		public ED815MessageHeaderProvider(EMCSJobDeclaration emcsJobDeclaration)
			: base(emcsJobDeclaration)
		{
		}
	}
}
