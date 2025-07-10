using CargoWise.Types;
using Enterprise.Packing.Business;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	interface IPackingParentWrapper
	{
		AddressWrapper GetPickupAddress(PkgPackage package);
		AddressWrapper GetDeliveryAddress(PkgPackage package);

		ZDateTime GetDeliveryRequiredBy(PkgPackage package);

		ZString GetTransportReference(PkgPackage package);
		ZString GetOwnerReference(PkgPackage package);
		ZString GetCustomerReference(PkgPackage package);

		OrganisationWrapper GetTransportCompany(PkgPackage package);
		CarrierServiceLevelWrapper GetCarrierServiceLevel(PkgPackage package);
	}
}
