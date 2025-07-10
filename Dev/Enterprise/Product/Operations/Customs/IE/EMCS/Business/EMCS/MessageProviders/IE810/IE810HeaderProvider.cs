using CargoWise.Common;
using CargoWise.Customs.IE.MessageContracts.EMCS.Interfaces;
using Enterprise.Customs.EU.EMCS.Business;

namespace Enterprise.Customs.IE.EMCS.Business
{
	public class IE810HeaderProvider : HeaderProvider, IIE810Header
	{
		public IE810HeaderProvider(EMCSJobDeclaration emcsJobDeclaration, IEMCSCancellation cancellationOfEad)
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
