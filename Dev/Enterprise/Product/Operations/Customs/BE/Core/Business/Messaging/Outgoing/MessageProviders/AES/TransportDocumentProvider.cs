using CargoWise.Common;
using CargoWise.Customs.BE.MessageContracts.Interfaces;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.BE.Business;

public class TransportDocumentProvider : ITransportDocument
{
	readonly CusSupportingInfo supportingInfo;
	public TransportDocumentProvider(CusSupportingInfo supportingInfo, int sequence)
	{
		this.supportingInfo = Argument.NotNull(supportingInfo, nameof(supportingInfo));
		SequenceNumber = sequence;
	}

	public int SequenceNumber { get; }

	public string Type => supportingInfo.CSI_Code;

	public string ReferenceNumber => supportingInfo.CSI_ReferenceNumber;
}
