using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentWrappers.GenericWrappers;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Transactions.Business;

namespace Enterprise.DocumentWrappers
{
	static class IJobWithTransportCompanyExtensions
	{
		public static ZString GetCarrierName(this IJobWithTransportCompany jobWithTransportCompany)
		{
			return jobWithTransportCompany.TransportCoDocAddress.Organisation?.OH_FullName ?? ZString.Empty;
		}

		public static AddressWrapper GetTransportCoAddress(this IJobWithTransportCompany jobWithTransportCompany, BusinessObjectFactory factory)
		{
			return jobWithTransportCompany != null
				? new AddressWrapper(jobWithTransportCompany.TransportCoDocAddress, factory)
				: AddressWrapper.Empty(factory);
		}

		public static DocDocAddress GetTransportCoAddressLegacy(this IJobWithTransportCompany jobWithTransportCompany, BusinessObjectFactory factory)
		{
			return DocDocAddress.New(jobWithTransportCompany.TransportCoDocAddress, factory);
		}

		public static OrganisationWrapper GetTransportCompany(this IJobWithTransportCompany jobWithTransportCompany, BusinessObjectFactory factory)
		{
			return new OrganisationWrapper(OrganisationUsageType.TransportCo, jobWithTransportCompany.GetTransportCo(), ContactType.TransportServices, factory);
		}

		public static DocOrganisation GetTransportCompanyLegacy(this IJobWithTransportCompany jobWithTransportCompany, BusinessObjectFactory factory)
		{
			return DocOrganisation.New(jobWithTransportCompany.TransportCoDocAddress, factory);
		}
	}
}
