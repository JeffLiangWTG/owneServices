using CargoWise.Customs.DE.MessageContracts;
using Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.Customs.DE.Business
{
	public class SupplyChainActorProvider : ISupplyChainActor
	{
		public static SupplyChainActorProvider NewOrNull(CusSupplyChainActorReference cusReference) => cusReference == null ? null : new SupplyChainActorProvider(cusReference);

		SupplyChainActorProvider(CusSupplyChainActorReference cusReference)
		{
			this.cusReference = cusReference;
		}
		readonly CusSupplyChainActorReference cusReference;

		public string Role => cusReference.CFR_Code;

		public string IdentificationNumber => cusReference.CFR_Reference;
	}
}
