using CargoWise.Common;
using CargoWise.Customs.FR.MessageDefinitions.DeltaIE.Interfaces;
using Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.DeltaIE
{
	public class AdditionalSupplyChainActorWrapper : IAdditionalSupplyChainActor
	{
		AdditionalSupplyChainActorWrapper(CusSupplyChainActorReference cusReference)
		{
			this.cusReference = Argument.NotNull(cusReference, nameof(cusReference));
		}

		readonly CusSupplyChainActorReference cusReference;

		public static AdditionalSupplyChainActorWrapper New(CusSupplyChainActorReference cusReference) => cusReference == null ? null : new AdditionalSupplyChainActorWrapper(cusReference);

		public string IdentificationNumber => identificationNumber ?? (identificationNumber = cusReference.CFR_Reference);
		string identificationNumber;

		public string Role => role ?? (role = cusReference.CFR_Code);
		string role;
	}
}
