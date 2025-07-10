using CargoWise.Common;
using CargoWise.Customs.BE.MessageContracts.Interfaces;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.BE.Business;

public class AdditionalSupplyChainActorProvider : IAdditionalSupplyChainActor
{
	public AdditionalSupplyChainActorProvider(CusReference cusReference, int sequenceNumber)
	{
		this.cusReference = Argument.NotNull(cusReference, nameof(cusReference));
		this.sequenceNumber = sequenceNumber;
	}

	readonly CusReference cusReference;
	readonly int sequenceNumber;

	public int SequenceNumber => sequenceNumber;

	public string Role => cusReference.CFR_Code;

	public string IdentificationNumber => cusReference.CFR_Reference;
}
