using CargoWise.Common;
using CargoWise.Customs.GB.MessageContracts.EMCS;
using Enterprise.Customs.EU.EMCS.Business;

namespace Enterprise.Customs.GB.EMCS.Business
{
	public sealed class IE810MessageHeaderProvider : MessageHeaderProvider<IE810HeaderProvider>, IEMCSMessageHeader
	{
		public IE810MessageHeaderProvider(EMCSJobDeclaration emcsJobDeclaration, IEMCSCancellation cancellationOfEad) : base(emcsJobDeclaration)
		{
			this.cancellationOfEad = Argument.NotNull(cancellationOfEad, nameof(cancellationOfEad));
		}
		readonly IEMCSCancellation cancellationOfEad;

		IEMCSHeader IEMCSMessageHeader.Header => header ?? (header = new IE810HeaderProvider(emcsJobDeclaration, cancellationOfEad));
	}
}
