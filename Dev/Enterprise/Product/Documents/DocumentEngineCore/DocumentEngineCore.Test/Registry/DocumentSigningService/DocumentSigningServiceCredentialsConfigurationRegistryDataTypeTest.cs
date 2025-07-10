using System.Text;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Core.Encryption;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentEngineCore.Registry.Business.Testing
{
	[TestedType(typeof(DocumentSigningServiceCredentialsConfigurationRegistryDataType))]
	class DocumentSigningServiceCredentialsConfigurationRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<DocumentSigningServiceCredentialsConfigurationRegistryDataType>
	{
		#region Implementation

		protected override DocumentSigningServiceCredentialsConfigurationRegistryDataType GetNewDataType()
		{
			return new DocumentSigningServiceCredentialsConfigurationRegistryDataType();
		}

		protected override string ExpectedEditorName
		{
			get { return "DocumentSigningServiceCredentialsConfigurationRegistryItemEditor"; }
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var encoder = TwoWayEncoder.NewWithStandardInitialisationVector();

			DocumentSigningServiceCredentialsConfiguration config1 = new DocumentSigningServiceCredentialsConfiguration();
			config1.ClientID = "client";
			config1.AccessKey = "key";
			config1.KeyID = "id";

			string xml1 =
				"<?xml version=\"1.0\" encoding=\"utf-16\"?>" +
				$@"<DocumentSigningServiceCredentialsConfiguration>
					<ClientID>client</ClientID>
					<AccessKey>{encoder.Encrypt("key")}</AccessKey>
					<KeyID>{encoder.Encrypt("id")}</KeyID>
				</DocumentSigningServiceCredentialsConfiguration>";

			DocumentSigningServiceCredentialsConfiguration config2 = new DocumentSigningServiceCredentialsConfiguration();
			config2.ClientID = "client2";
			config2.AccessKey = "key2";
			config2.KeyID = "id2";

			string xml2 =
				"<?xml version=\"1.0\" encoding=\"utf-16\"?>" +
				$@"<DocumentSigningServiceCredentialsConfiguration>
					<ClientID>client2</ClientID>
					<AccessKey>{encoder.Encrypt("key2")}</AccessKey>
					<KeyID>{encoder.Encrypt("id2")}</KeyID>
				</DocumentSigningServiceCredentialsConfiguration>";

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(config1, Encoding.Unicode.GetBytes(xml1)),
				new ValidSampleAndBinaryValueInDB(config2, Encoding.Unicode.GetBytes(xml2))
			};
		}

		#endregion
	}
}
