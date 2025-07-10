using CargoWise.Common;
using CargoWise.Customs.IE.MessageContracts.EMCS.Interfaces;
using Enterprise.Customs.EU.EMCS.Business;

namespace Enterprise.Customs.IE.EMCS.Business
{
	public class IE810MessageHeaderProvider : MessageHeaderProvider<IE810HeaderProvider>, IEMCSMessageHeader
	{
		public IE810MessageHeaderProvider(EMCSJobDeclaration emcs, IEMCSCancellation cancellationOfEad)
		: base(emcs)
		{
			this.cancellationOfEad = Argument.NotNull(cancellationOfEad, nameof(cancellationOfEad));
		}
		readonly IEMCSCancellation cancellationOfEad;

		IEMCSHeader IEMCSMessageHeader.Header => header ?? (header = new IE810HeaderProvider(emcsJobDeclaration, cancellationOfEad));
	}
}
