using CargoWise.Common;
using CargoWise.Customs.BE.MessageContracts.Interfaces;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.BE.Business;

public class CodeProvider : ICode
{
	public CodeProvider(SupplementaryCode supplementaryCode, int sequenceNumber)
	{
		this.supplementaryCode = Argument.NotNull(supplementaryCode, nameof(supplementaryCode));
		this.sequenceNumber = sequenceNumber;
	}

	readonly SupplementaryCode supplementaryCode;
	readonly int sequenceNumber;

	public string SequenceNumber => sequenceNumber.ToString();

	public string Code => supplementaryCode.CY_Code;
}
