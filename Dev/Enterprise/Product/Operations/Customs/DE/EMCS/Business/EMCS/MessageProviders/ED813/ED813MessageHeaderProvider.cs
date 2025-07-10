using CargoWise.Customs.DE.MessageContracts.EMCS;

namespace Enterprise.Customs.DE.EMCS.Business
{
	public class ED813MessageHeaderProvider : MessageHeaderProvider<ED813HeaderProvider>, IEMCSMessageHeader
	{
		public ED813MessageHeaderProvider(EMCSJobDeclaration emcs)
			: base(emcs)
		{
		}
	}
}
