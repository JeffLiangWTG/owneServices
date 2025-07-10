using CargoWise.Common;
using CargoWise.Customs.DE.MessageContracts.EMCS;
using Enterprise.Customs.EU.EMCS.Business;

namespace Enterprise.Customs.DE.EMCS.Business
{
	public class ED810MessageHeaderProvider : MessageHeaderProvider<ED810HeaderProvider>, IEMCSMessageHeader
	{
		public ED810MessageHeaderProvider(EMCSJobDeclaration emcs, IEMCSCancellation cancellationOfEad)
		: base(emcs)
		{
			this.cancellationOfEad = Argument.NotNull(cancellationOfEad, nameof(cancellationOfEad));
		}
		readonly IEMCSCancellation cancellationOfEad;

		IEMCSHeader IEMCSMessageHeader.Header => header ?? (header = new ED810HeaderProvider(emcsJobDeclaration, cancellationOfEad));
	}
}
