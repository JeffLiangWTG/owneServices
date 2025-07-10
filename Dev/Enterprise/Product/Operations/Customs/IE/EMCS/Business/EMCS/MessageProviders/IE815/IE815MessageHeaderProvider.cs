using CargoWise.Customs.IE.MessageContracts.EMCS.Interfaces;

namespace Enterprise.Customs.IE.EMCS.Business
{
	public class IE815MessageHeaderProvider : MessageHeaderProvider<IE815HeaderProvider>, IEMCSMessageHeader
	{
		public IE815MessageHeaderProvider(EMCSJobDeclaration emcsJobDeclaration) : base(emcsJobDeclaration)
		{
		}
	}
}
