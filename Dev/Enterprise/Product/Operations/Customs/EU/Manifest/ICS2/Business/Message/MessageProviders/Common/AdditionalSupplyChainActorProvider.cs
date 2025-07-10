using CargoWise.Customs.EU.MessageContracts.ICS2;
using CargoWise.Customs.Shared.MessageContracts;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business
{
	public class AdditionalSupplyChainActorProvider : IIdentifierTypePair
	{
		public AdditionalSupplyChainActorProvider(CusSupplyChainActorReference cusSupplyChainActorReference)
		{
			supplyChainActor = Argument.NotNull(cusSupplyChainActorReference, nameof(cusSupplyChainActorReference));
		}

		readonly CusSupplyChainActorReference supplyChainActor;

		public string Identifier => supplyChainActor.CFR_Reference;

		public string Type => supplyChainActor.CFR_Code;
	}
}
