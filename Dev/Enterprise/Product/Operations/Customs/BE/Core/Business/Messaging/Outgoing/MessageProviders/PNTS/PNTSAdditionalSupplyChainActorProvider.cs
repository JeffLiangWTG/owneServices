using CargoWise.Common;
using CargoWise.Customs.BE.MessageContracts.Interfaces;
using Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.Customs.BE.Business;

public class PNTSAdditionalSupplyChainActorProvider : IPNTSAdditionalSupplyChainActor
{
	public PNTSAdditionalSupplyChainActorProvider(CusSupplyChainActorReference supplyChainActorReference)
	{
		this.supplyChainActorReference = Argument.NotNull(supplyChainActorReference, nameof(supplyChainActorReference));
	}
	readonly CusSupplyChainActorReference supplyChainActorReference;

	public string Role => supplyChainActorReference.CFR_Code;

	public string IdentificationNumber => supplyChainActorReference.CFR_Reference;
}
