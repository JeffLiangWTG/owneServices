using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.CH.MessageContracts.Passar.Outgoing;
using Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.Customs.CH.NCTS.Business;

public class AdditionalSupplyChainActorDataProvider : IAdditionalSupplyChainActor
{
	public static IEnumerable<AdditionalSupplyChainActorDataProvider> NewCollection(ICusSupplyChainActorReferenceCollection<EU.NCTS.Business.CusSupplyChainActorReference> cusSupplyChainActors)
	{
		return cusSupplyChainActors?.Cast<CusSupplyChainActorReference>().Select((cusSupplyChainActor, index) => new AdditionalSupplyChainActorDataProvider(cusSupplyChainActor, index + 1));
	}

	AdditionalSupplyChainActorDataProvider(CusSupplyChainActorReference cusSupplyChainActor, int sequenceNumber)
	{
		this.cusSupplyChainActor = cusSupplyChainActor;
		this.sequenceNumber = sequenceNumber;
	}
	readonly CusSupplyChainActorReference cusSupplyChainActor;
	readonly int sequenceNumber;

	public int SequenceNumber => sequenceNumber;

	public string Role => cusSupplyChainActor.CFR_Code;

	public string IdentificationNumber => cusSupplyChainActor.CFR_Reference;
}
