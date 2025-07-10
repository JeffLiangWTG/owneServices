using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentEngineCore.Registry.Business.Testing
{
	[TestedType(typeof(DocumentSigningServicePartnerCredentialsRegistryDataType))]
	class DocumentSigningServicePartnerCredentialsRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<DocumentSigningServicePartnerCredentialsRegistryDataType>
	{
		#region Implementation

		protected override DocumentSigningServicePartnerCredentialsRegistryDataType GetNewDataType()
		{
			return new DocumentSigningServicePartnerCredentialsRegistryDataType();
		}

		protected override string ExpectedEditorName
		{
			get { return "DocumentSigningServicePartnerCredentialsRegistryItemEditor"; }
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var config1 = new DocumentSigningServicePartnerCredentials();
			config1.PartnerID = "client";
			config1.PartnerAccessKey = "key";

			var config2 = new DocumentSigningServicePartnerCredentials();
			config2.PartnerID = "client2";
			config2.PartnerAccessKey = "key2";

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(config1, new DocumentSigningServicePartnerCredentialsRegistryDataType().Serialise(config1)),
				new ValidSampleAndBinaryValueInDB(config2, new DocumentSigningServicePartnerCredentialsRegistryDataType().Serialise(config2))
			};
		}

		#endregion
	}
}
