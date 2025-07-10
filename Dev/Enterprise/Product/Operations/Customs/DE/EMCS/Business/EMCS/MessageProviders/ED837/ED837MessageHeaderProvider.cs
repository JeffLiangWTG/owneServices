using CargoWise.Customs.DE.MessageContracts.EMCS;
using Enterprise.Customs.EU.EMCS.Business;

namespace Enterprise.Customs.DE.EMCS.Business
{
	public class ED837MessageHeaderProvider : MessageHeaderProvider<ED837HeaderProvider>, IEMCSMessageHeader
	{
		readonly IExplanationOnDelay explanationOnDelay;

		public ED837MessageHeaderProvider(EMCSJobDeclaration emcsJobDeclaration, IExplanationOnDelay explanationOnDelay) : base(emcsJobDeclaration)
		{
			this.explanationOnDelay = explanationOnDelay;
		}

		IEMCSHeader IEMCSMessageHeader.Header => header ?? (header = new ED837HeaderProvider(emcsJobDeclaration, explanationOnDelay));
	}
}
