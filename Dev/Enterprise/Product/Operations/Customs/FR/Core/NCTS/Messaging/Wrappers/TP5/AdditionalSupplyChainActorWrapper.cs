using CargoWise.Common;
using CargoWise.Customs.FR.MessageDefinitions.TP5.Interfaces;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.FR.NCTS.Messaging.TP5
{
	public class AdditionalSupplyChainActorWrapper : IAdditionalSupplyChainActor
	{
		AdditionalSupplyChainActorWrapper(CusReference cusReference)
		{
			this.cusReference = Argument.NotNull(cusReference, nameof(cusReference));
		}

		readonly CusReference cusReference;

		public static AdditionalSupplyChainActorWrapper New(CusReference cusReference) => cusReference == null ? null : new AdditionalSupplyChainActorWrapper(cusReference);

		public string IdentificationNumber => identificationNumber ?? (identificationNumber = cusReference.CFR_Reference);
		string identificationNumber;

		public string Role => role ?? (role = cusReference.CFR_Code);
		string role;
	}
}
