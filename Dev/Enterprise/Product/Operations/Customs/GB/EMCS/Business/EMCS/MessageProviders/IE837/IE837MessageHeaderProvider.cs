using CargoWise.Customs.GB.MessageContracts.EMCS;
using Enterprise.Customs.EU.EMCS.Business;

namespace Enterprise.Customs.GB.EMCS.Business
{
	public sealed class IE837MessageHeaderProvider : MessageHeaderProvider<IE837HeaderProvider>, IEMCSMessageHeader
	{
		readonly IExplanationOnDelay explanationOnDelay;

		public IE837MessageHeaderProvider(EMCSJobDeclaration emcsJobDeclaration, IExplanationOnDelay explanationOnDelay) : base(emcsJobDeclaration)
		{
			this.explanationOnDelay = explanationOnDelay;
		}

		IEMCSHeader IEMCSMessageHeader.Header => header ?? (header = new IE837HeaderProvider(emcsJobDeclaration, explanationOnDelay));
	}
}
