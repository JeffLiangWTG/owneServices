using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.Customs.ES.Business.MessageWrappers
{
	public class AdditionalSupplyChainActorCommonWrapper : IAdditionalSupplyChainActorCommon
	{
		public AdditionalSupplyChainActorCommonWrapper(CusSupplyChainActorReference supplyChainActorRef)
		{
			supplyChainActor = Argument.NotNull(supplyChainActorRef, nameof(supplyChainActorRef));
		}
		readonly CusSupplyChainActorReference supplyChainActor;

		public ZString Role => supplyChainActor.CFR_Code;

		public ZString Id => supplyChainActor.CFR_Reference;
	}
}
