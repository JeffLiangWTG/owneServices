using CargoWise.Common;
using CargoWise.Customs.IE.MessageContracts.Interfaces;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.IE.Business.AIS.UCC5
{
	public class AdditionalSupplyChainActorProvider : IAdditionalSupplyChainActor
	{
		public AdditionalSupplyChainActorProvider(CusReference cusReference)
		{
			this.cusReference = Argument.NotNull(cusReference, nameof(cusReference));
		}
		readonly CusReference cusReference;

		public string Role => cusReference.CFR_Code;

		public string ID => cusReference.CFR_Reference;
	}
}
