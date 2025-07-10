using System;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business.Testing
{
	public static class OrganisationsDataRegistryTestHelper
	{
		public static IDisposable SetTemporaryValueForAddressValidationWebServiceURIs(Action<AvsWebServiceUriRegistryBusinessObjectCollection> core, BusinessObjectFactory factory)
		{
			var temporaryValue = (AvsWebServiceUriRegistryBusinessObjectCollection)OrganisationsDataRegistry.Instance.AvsWebServiceURIs.Clone(new FallbackLevel(Guid.Empty, Guid.Empty, Guid.Empty), factory);
			core.Invoke(temporaryValue);
			return OrganisationsDataRegistry.Instance.AddressValidationWebServiceURIs.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, temporaryValue);
		}
	}
}
