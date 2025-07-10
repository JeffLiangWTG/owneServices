using CargoWise.Common;
using CargoWise.Customs.FR.MessageDefinitions.DeltaIE.Interfaces;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.DeltaIE
{
	public class BuyerWrapper : IBuyer
	{
		BuyerWrapper(OrgAddress buyerAddress)
		{
			this.buyerAddress = Argument.NotNull(buyerAddress, nameof(buyerAddress));
		}

		readonly OrgAddress buyerAddress;

		public static BuyerWrapper New(OrgAddress buyerAddress) => buyerAddress == null ? null : new BuyerWrapper(buyerAddress);

		public IAddress Address => address ?? (address = buyerAddress != null && IdentificationNumber.IsNullOrEmpty() ? OrganisationAddressWrapper.New(buyerAddress) : null);
		IAddress address;

		public string IdentificationNumber => identificationNumber ?? (identificationNumber = buyerAddress.GetEuIdentificationNumber());
		string identificationNumber;

		public string Name => name ?? (IdentificationNumber.IsNullOrEmpty() ? name = buyerAddress.Header?.OH_FullName : null);
		string name;
	}
}
