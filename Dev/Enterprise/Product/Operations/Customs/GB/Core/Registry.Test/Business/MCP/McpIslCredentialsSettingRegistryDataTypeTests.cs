using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.GB.Registry.Testing
{
	[TestedType(typeof(McpIslCredentialsSettingRegistryDataType))]
	class McpIslCredentialsSettingRegistryDataTypeNonPersistentTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<McpIslCredentialsSettingRegistryDataType>
	{
		protected override string ExpectedEditorName => "McpIslCredentialsSettingRegistryItemEditor";

		protected override McpIslCredentialsSettingRegistryDataType GetNewDataType()
		{
			return new McpIslCredentialsSettingRegistryDataType();
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var credentialsCollection1 = new McpIslCredentialsSettingCollection();
			var credential1 = credentialsCollection1.AddNew();
			credential1.McpIslCompanyCode = "WIS";
			credential1.McpIslUsername = "test";
			credential1.McpIslDevice = "CAW1";
			credential1.McpIslPassword = "abc";

			var credentialsCollection2 = new McpIslCredentialsSettingCollection();
			var credential2 = credentialsCollection2.AddNew();
			credential2.McpIslCompanyCode = "GLO";
			credential2.McpIslUsername = "test2";
			credential2.McpIslDevice = "CAW2";
			credential2.McpIslPassword = "abcd";

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(credentialsCollection1, new McpIslCredentialsSettingRegistryDataType().Serialise(credentialsCollection1)),
				new ValidSampleAndBinaryValueInDB(credentialsCollection2, new McpIslCredentialsSettingRegistryDataType().Serialise(credentialsCollection2))
			};
		}
	}
}
