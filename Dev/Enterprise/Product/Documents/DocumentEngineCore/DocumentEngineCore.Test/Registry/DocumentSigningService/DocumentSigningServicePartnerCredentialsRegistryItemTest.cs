using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentEngineCore.Registry.Business.Testing
{
	[TestedType(typeof(DocumentSigningServicePartnerCredentialsRegistryItem))]
	public class DocumentSigningServicePartnerCredentialsRegistryItemTest : StronglyTypedRegistryItemTestCase<DocumentSigningServicePartnerCredentials>
	{
		#region Implementation

		protected override StronglyTypedRegistryItem<DocumentSigningServicePartnerCredentials, DocumentSigningServicePartnerCredentials> GetNewRegistryItem()
		{
			return new DocumentSigningServicePartnerCredentialsRegistryItem("", null, null, null, RegistryStorageFlags.System, RegistryOptions.Default, new DocumentSigningServicePartnerCredentials() { PartnerID = "id", PartnerAccessKey = "key" });
		}

		protected override DocumentSigningServicePartnerCredentials ValidValue
		{
			get
			{
				DocumentSigningServicePartnerCredentials result = new DocumentSigningServicePartnerCredentials();
				result.PartnerID = "client";
				result.PartnerAccessKey = "key";
				return result;
			}
		}

		#endregion
	}
}
