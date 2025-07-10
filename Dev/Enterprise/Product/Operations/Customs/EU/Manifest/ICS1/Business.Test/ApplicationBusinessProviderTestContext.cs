using System;
using CargoWise.Application;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.ASYCUDA.Business.Testing;
using Enterprise.Customs.ManifestBase;

namespace Enterprise.Customs.EU.Manifest.Business.Testing
{
	sealed class ApplicationBusinessProviderTestContext : IDisposable
	{
		ApplicationBusinessProviderTestContext(params ApplicationBusinessProvider[] providers)
		{
			disposableContext = ObjectFactory.Substitute("GlobalManifestApplicationBusinessProvider", providers);
		}

		public static ApplicationBusinessProviderTestContext CreateDefaultIcsContext() => new(
			new EuManifestApplicationForTest(EUManifestTypes.Codes.ICS, Core.Constants.CountryCodes.Latvia)
				.WithApplicationCode(ApplicationCodeTypeList.Codes.ShippingLine));

		public void Dispose() => disposableContext.Dispose();

		readonly IDisposable disposableContext;
	}

	sealed class EuManifestApplicationForTest(string manifestType, params string[] countries) : DummyApplicationBusinessProvider(manifestType, countries)
	{
		public override Type AsycudaManifestHeaderType => typeof(AsycudaManifestHeader);
	}
}
