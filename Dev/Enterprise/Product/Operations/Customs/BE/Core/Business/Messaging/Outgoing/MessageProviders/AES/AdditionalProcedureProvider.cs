using CargoWise.Common;
using CargoWise.Customs.BE.MessageContracts.Interfaces;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.BE.Business;

public class AdditionalProcedureProvider : IAdditionalProcedure
{
	public AdditionalProcedureProvider(AdditionalProcedureCode procedureCode, int sequence)
	{
		this.procedureCode = Argument.NotNull(procedureCode, nameof(procedureCode));
		this.sequence = sequence;
	}

	readonly AdditionalProcedureCode procedureCode;
	readonly int sequence;

	public string AdditionalProcedure => procedureCode.CY_Code;

	public string SequenceNumber => sequence.ToString();
}
