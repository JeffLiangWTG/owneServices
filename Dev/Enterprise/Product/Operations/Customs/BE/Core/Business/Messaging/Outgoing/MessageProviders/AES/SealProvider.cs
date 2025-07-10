using CargoWise.Common;
using CargoWise.Customs.BE.MessageContracts.Interfaces;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.BE.Business;

public class SealProvider : ISeal
{
	public SealProvider(CusSeal seal) : this(Argument.NotNull(seal, nameof(seal)).BK_SealNumber, seal.BK_SequenceNumber)
	{
	}

	public SealProvider(ZString identifier, ZInt sequenceNumber)
	{
		Identifier = Argument.NotNullOrEmpty(identifier, nameof(identifier));
		SequenceNumber = sequenceNumber;
	}

	public int SequenceNumber { get; }

	public string Identifier { get; }
}
