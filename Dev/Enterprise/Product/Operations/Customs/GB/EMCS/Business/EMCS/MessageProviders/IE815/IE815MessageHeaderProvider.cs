using CargoWise.Customs.GB.MessageContracts.EMCS;

namespace Enterprise.Customs.GB.EMCS.Business
{
	public sealed class IE815MessageHeaderProvider : MessageHeaderProvider<IE815HeaderProvider>, IEMCSMessageHeader
	{
		public IE815MessageHeaderProvider(EMCSJobDeclaration emcsJobDeclaration) : base(emcsJobDeclaration) { }
	}
}
