using CargoWise.Customs.GB.MessageContracts.EMCS;

namespace Enterprise.Customs.GB.EMCS.Business
{
	public sealed class IE825MessageHeaderProvider : MessageHeaderProvider<IE825HeaderProvider>, IEMCSMessageHeader
	{
		public IE825MessageHeaderProvider(EMCSJobDeclaration emcsJobDeclaration) : base(emcsJobDeclaration) { }

		IEMCSHeader IEMCSMessageHeader.Header => header ?? (header = new IE825HeaderProvider(emcsJobDeclaration));
	}
}
