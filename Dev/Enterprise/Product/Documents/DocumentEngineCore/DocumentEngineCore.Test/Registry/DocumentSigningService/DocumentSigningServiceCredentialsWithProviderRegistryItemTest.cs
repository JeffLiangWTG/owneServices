using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentEngineCore.Registry.Business.Testing
{
	[TestedType(typeof(DocumentSigningServiceCredentialsWithProviderRegistryItem))]
	public class DocumentSigningServiceCredentialsWithProviderRegistryItemTest : StronglyTypedRegistryItemTestCase<DocumentSigningServiceCredentialsWithProviderConfiguration>
	{
		#region Implementation

		protected override StronglyTypedRegistryItem<DocumentSigningServiceCredentialsWithProviderConfiguration, DocumentSigningServiceCredentialsWithProviderConfiguration> GetNewRegistryItem()
		{
			return new DocumentSigningServiceCredentialsWithProviderRegistryItem("", null, null, null, RegistryStorageFlags.System, new DocumentSigningServiceCredentialsWithProviderConfiguration());
		}

		protected override DocumentSigningServiceCredentialsWithProviderConfiguration ValidValue
		{
			get
			{
				var result = new DocumentSigningServiceCredentialsWithProviderConfiguration();
				result.ProviderCode = "EMD";
				result.ClientID = "client";
				result.AccessKey = "key";
				result.KeyID = "id";

				return result;
			}
		}

		#endregion
	}
}
