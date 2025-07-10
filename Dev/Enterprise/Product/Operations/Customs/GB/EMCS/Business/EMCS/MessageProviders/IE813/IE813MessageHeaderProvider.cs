using CargoWise.Customs.GB.MessageContracts.EMCS;

namespace Enterprise.Customs.GB.EMCS.Business
{
	public sealed class IE813MessageHeaderProvider : MessageHeaderProvider<IE813HeaderProvider>, IEMCSMessageHeader
	{
		public IE813MessageHeaderProvider(EMCSJobDeclaration emcsJobDeclaration) : base(emcsJobDeclaration) { }
	}
}
