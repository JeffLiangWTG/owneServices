using CargoWise.Common;
using CargoWise.Customs.FR.MessageDefinitions.DeltaIE.Interfaces;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.DeltaIE
{
	public class SellerWrapper : ISeller
	{
		SellerWrapper(OrgAddress sellerAddress)
		{
			this.sellerAddress = Argument.NotNull(sellerAddress, nameof(sellerAddress));
		}

		readonly OrgAddress sellerAddress;

		public static SellerWrapper New(OrgAddress sellerAddress) => sellerAddress == null ? null : new SellerWrapper(sellerAddress);

		public IAddress Address => address ?? (address = sellerAddress != null && IdentificationNumber.IsNullOrEmpty() ? OrganisationAddressWrapper.New(sellerAddress) : null);
		IAddress address;

		public string IdentificationNumber => identificationNumber ?? (identificationNumber = sellerAddress.GetEuIdentificationNumber());
		string identificationNumber;

		public string Name => name ?? (IdentificationNumber.IsNullOrEmpty() ? name = sellerAddress.Header?.OH_FullName : null);
		string name;
	}
}
