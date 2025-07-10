using CargoWise.Customs.IE.MessageContracts.EMCS.Interfaces;

namespace Enterprise.Customs.IE.EMCS.Business
{
	public class IE813MessageHeaderProvider : MessageHeaderProvider<IE813HeaderProvider>, IEMCSMessageHeader
	{
		public IE813MessageHeaderProvider(EMCSJobDeclaration emcs)
			: base(emcs)
		{
		}
	}
}
