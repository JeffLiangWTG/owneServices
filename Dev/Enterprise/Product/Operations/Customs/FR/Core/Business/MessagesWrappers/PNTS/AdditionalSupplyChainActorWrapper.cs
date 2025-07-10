using CargoWise.Common;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.PNTS
{
	public class AdditionalSupplyChainActorWrapper : CargoWise.Customs.FR.MessageDefinitions.PNTS.Interfaces.IAdditionalSupplyChainActor
	{
		AdditionalSupplyChainActorWrapper(CusReference cusReference)
		{
			this.cusReference = Argument.NotNull(cusReference, nameof(cusReference));
		}

		readonly CusReference cusReference;

		public string IdentificationNumber => identificationNumber ?? (identificationNumber = cusReference.CFR_Reference);
		string identificationNumber;

		public string Role => role ?? (role = cusReference.CFR_Code);
		string role;

		public static AdditionalSupplyChainActorWrapper New(CusReference cusReference) => cusReference == null ? null : new AdditionalSupplyChainActorWrapper(cusReference);
	}
}
