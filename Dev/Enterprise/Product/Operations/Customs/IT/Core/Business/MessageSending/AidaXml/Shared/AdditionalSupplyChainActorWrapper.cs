using CargoWise.Common;
using CargoWise.Customs.IT.MessageContracts;
using Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.Customs.IT.Business.MessageSending.AidaXml.Shared;

public class AdditionalSupplyChainActorWrapper : IAdditionalSupplyChainActor
{
	public AdditionalSupplyChainActorWrapper(CusSupplyChainActorReference supplyChainActor)
	{
		this.supplyChainActor = Argument.NotNull(supplyChainActor, nameof(supplyChainActor));
	}

	readonly CusSupplyChainActorReference supplyChainActor;

	string IAdditionalSupplyChainActor.Role => supplyChainActor.CFR_Code;

	string IAdditionalSupplyChainActor.Reference => supplyChainActor.CFR_Reference;
}
