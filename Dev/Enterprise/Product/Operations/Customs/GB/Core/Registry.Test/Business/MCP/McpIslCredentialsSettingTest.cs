using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.GB.Registry.Testing
{
	[TestedType(typeof(McpIslCredentialsSetting))]
	public class McpIslCredentialsSettingTest : RegistryBusinessObjectTemplateTestCase<McpIslCredentialsSetting>
	{
		McpIslCredentialsSetting SetCredentialForTest(McpIslCredentialsSetting credential)
		{
			credential.McpIslCompanyCode = "WTG";
			credential.McpIslUsername = "JessicaJones";
			credential.McpIslDevice = "CAW3";
			credential.McpIslPassword = "KillGrave";
			return credential;
		}

		public void TestMcpIslCredentialsSetting()
		{
			var credential = GetBusinessObjectToSerialise();
			credential = SetCredentialForTest(credential);

			var dataType = new DummyNonPersistentBusinessObjectRegistryDataType(typeof(McpIslCredentialsSetting));
			var serialisedValue = dataType.Serialise(credential);
			var deserialisedBusinessObject = (McpIslCredentialsSetting)dataType.Deserialise(serialisedValue);

			AssertEquals("WTG", deserialisedBusinessObject.McpIslCompanyCode);
			AssertEquals("JessicaJones", deserialisedBusinessObject.McpIslUsername);
			AssertEquals("CAW3", deserialisedBusinessObject.McpIslDevice);
			AssertEquals("KillGrave", deserialisedBusinessObject.McpIslPassword);
		}

		public void TestValidationOfMcpIslCredentialsSetting()
		{
			var credential = new McpIslCredentialsSetting();
			credential.ValidateAll();
			AssertHasErrorContaining(credential.McpIslCompanyCodeInfo, "cannot be empty");
			AssertHasErrorContaining(credential.McpIslUsernameInfo, "cannot be empty");
			AssertHasErrorContaining(credential.McpIslDeviceInfo, "cannot be empty");
			AssertHasErrorContaining(credential.McpIslPasswordInfo, "cannot be empty");
			credential = SetCredentialForTest(credential);
			AssertNoErrorContaining(credential.McpIslCompanyCodeInfo, "cannot be empty");
			AssertNoErrorContaining(credential.McpIslUsernameInfo, "cannot be empty");
			AssertNoErrorContaining(credential.McpIslDeviceInfo, "cannot be empty");
			AssertNoErrorContaining(credential.McpIslPasswordInfo, "cannot be empty");
		}

		#region Implementation

		protected override McpIslCredentialsSetting GetBusinessObjectToClone()
		{
			return GetBusinessObjectToSerialise();
		}

		protected override McpIslCredentialsSetting GetBusinessObjectToSerialise()
		{
			return SetCredentialForTest(new McpIslCredentialsSetting());
		}

		protected override bool RequiresFactory => true;

		protected override bool RequiresFallbackLevel => true;

		#endregion
	}
}
