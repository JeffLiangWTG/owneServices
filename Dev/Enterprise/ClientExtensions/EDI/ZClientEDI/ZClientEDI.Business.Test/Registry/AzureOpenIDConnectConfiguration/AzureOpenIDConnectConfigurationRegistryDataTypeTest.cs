using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Registry.Business
{
	[TestedType(typeof(AzureOpenIDConnectConfigurationRegistryDataType))]
	public class AzureOpenIDConnectConfigurationRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<AzureOpenIDConnectConfigurationRegistryDataType>
	{
		protected override AzureOpenIDConnectConfigurationRegistryDataType GetNewDataType()
		{
			return new AzureOpenIDConnectConfigurationRegistryDataType();
		}

		protected override string ExpectedEditorName
		{
			get { return "AzureOpenIDConnectConfigurationRegistryItemEditor"; }
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var azureApplicationManagementCollection1 = new AzureOpenIDConnectConfigurationCollection
			{
				new AzureOpenIDConnectConfiguration
				{
					Code = "PRD",
					AuthorityUrl = "https://www.example.com",
					ClientID = "Test1",
				}
			};

			var azureApplicationManagementCollection2 = new AzureOpenIDConnectConfigurationCollection
			{
				new AzureOpenIDConnectConfiguration
				{
					Code = "PRD",
					AuthorityUrl = "https://www.example.com",
					ClientID = "Test2"
				}
			};

			return new ValidSampleAndBinaryValueInDB[] {
				new ValidSampleAndBinaryValueInDB(azureApplicationManagementCollection1, DataType.Serialise(azureApplicationManagementCollection1)),
				new ValidSampleAndBinaryValueInDB(azureApplicationManagementCollection2, DataType.Serialise(azureApplicationManagementCollection2))
			};
		}
	}
}
