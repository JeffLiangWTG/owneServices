using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentEngineCore.Registry.Business.Testing
{
	[TestedType(typeof(DocumentSigningServiceCredentialsRegistryItem))]
	public class DocumentSigningServiceCredentialsRegistryItemTest : StronglyTypedRegistryItemTestCase<DocumentSigningServiceCredentialsConfiguration>
	{
		#region Implementation

		protected override StronglyTypedRegistryItem<DocumentSigningServiceCredentialsConfiguration, DocumentSigningServiceCredentialsConfiguration> GetNewRegistryItem()
		{
			return new DocumentSigningServiceCredentialsRegistryItem("", null, null, null, RegistryStorageFlags.System);
		}

		protected override DocumentSigningServiceCredentialsConfiguration ValidValue
		{
			get
			{
				DocumentSigningServiceCredentialsConfiguration result = new DocumentSigningServiceCredentialsConfiguration();
				result.ClientID = "client";
				result.AccessKey = "key";
				result.KeyID = "id";

				return result;
			}
		}

		#endregion
	}
}
