using CargoWise.Common;
using CargoWise.Customs.IE.MessageContracts.Interfaces;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.IE.Business
{
	public class AdditionalSupplyChainActorProvider : IAdditionalSupplyChainActor
	{
		readonly CusReference cusReference;

		public AdditionalSupplyChainActorProvider(CusReference cusReference)
		{
			this.cusReference = Argument.NotNull(cusReference, nameof(cusReference));
		}

		public string Role => cusReference.CFR_Code;

		public string ID => cusReference.CFR_Reference;
	}
}
