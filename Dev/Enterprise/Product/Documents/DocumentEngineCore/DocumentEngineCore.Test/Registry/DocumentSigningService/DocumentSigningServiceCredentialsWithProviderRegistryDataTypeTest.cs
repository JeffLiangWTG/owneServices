using System.Text;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Core.Encryption;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentEngineCore.Registry.Business.Testing
{
	[TestedType(typeof(DocumentSigningServiceCredentialsWithProviderRegistryDataType))]
	class DocumentSigningServiceCredentialsWithProviderRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<DocumentSigningServiceCredentialsWithProviderRegistryDataType>
	{
		#region Implementation

		protected override DocumentSigningServiceCredentialsWithProviderRegistryDataType GetNewDataType()
		{
			return new DocumentSigningServiceCredentialsWithProviderRegistryDataType();
		}

		protected override string ExpectedEditorName
		{
			get { return "DocumentSigningServiceCredentialsConfigurationRegistryItemEditor"; }
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var encoder = TwoWayEncoder.NewWithStandardInitialisationVector();

			var config1 = new DocumentSigningServiceCredentialsWithProviderConfiguration();
			config1.ProviderCode = "EMD";
			config1.ClientID = "client";
			config1.AccessKey = "key";
			config1.KeyID = "id";

			string xml1 = "<?xml version=\"1.0\" encoding=\"utf-16\"?>" +
$@"<DocumentSigningServiceCredentialsWithProviderConfiguration><ProviderCode>EMD</ProviderCode><ClientID>client</ClientID><AccessKey>{encoder.Encrypt("key")}</AccessKey><KeyID>{encoder.Encrypt("id")}</KeyID></DocumentSigningServiceCredentialsWithProviderConfiguration>";

			var config2 = new DocumentSigningServiceCredentialsWithProviderConfiguration();
			config2.ProviderCode = "DGS";
			config2.ClientID = "client2";
			config2.AccessKey = "key2";
			config2.KeyID = "id2";

			string xml2 = "<?xml version=\"1.0\" encoding=\"utf-16\"?>" +
$@"<DocumentSigningServiceCredentialsWithProviderConfiguration><ProviderCode>DGS</ProviderCode><ClientID>client2</ClientID><AccessKey>{encoder.Encrypt("key2")}</AccessKey><KeyID>{encoder.Encrypt("id2")}</KeyID></DocumentSigningServiceCredentialsWithProviderConfiguration>";

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(config1, Encoding.Unicode.GetBytes(xml1)),
				new ValidSampleAndBinaryValueInDB(config2, Encoding.Unicode.GetBytes(xml2))
			};
		}

		#endregion
	}
}
