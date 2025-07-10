using CargoWise.Common;
using CargoWise.Customs.DE.MessageContracts.EMCS;
using Enterprise.Customs.EU.EMCS.Business;

namespace Enterprise.Customs.DE.EMCS.Business
{
	public class ED810HeaderProvider : HeaderProvider, IED810Header
	{
		public ED810HeaderProvider(EMCSJobDeclaration emcsJobDeclaration, IEMCSCancellation cancellationOfEad)
		: base(emcsJobDeclaration)
		{
			this.cancellationOfEad = Argument.NotNull(cancellationOfEad, nameof(cancellationOfEad));
			helper = new Message810HeaderProviderHelper(emcsJobDeclaration, cancellationOfEad);
		}
		readonly IEMCSCancellation cancellationOfEad;
		readonly Message810HeaderProviderHelper helper;

		public int CancellationReasonCode => helper.CancellationReasonCode;

		public ITextAndLanguage ComplementaryInformation => GetComplementaryInformation(cancellationOfEad.Information);
	}
}
