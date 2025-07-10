using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Registry.Business.Test
{
	[TestedType(typeof(AzureOpenIDConnectConfiguration))]
	public class AzureOpenIDConnectConfigurationTest : RegistryBusinessObjectTemplateTestCase<AzureOpenIDConnectConfiguration>
	{
		protected override bool RequiresFactory => false;

		protected override bool RequiresFallbackLevel => false;

		protected override AzureOpenIDConnectConfiguration GetBusinessObjectToClone()
		{
			return new AzureOpenIDConnectConfiguration();
		}

		protected override AzureOpenIDConnectConfiguration GetBusinessObjectToSerialise()
		{
			return GetBusinessObjectToClone();
		}

		#region Test
		public void TestValidateCode()
		{
			var azureApplicationManagement = new AzureOpenIDConnectConfiguration();
			AssertNoErrors(azureApplicationManagement.CodeInfo);

			azureApplicationManagement.ValidateCode();
			AssertHasError(azureApplicationManagement.CodeInfo, "Please enter a value.");

			azureApplicationManagement.Code = "Test";
			AssertHasError(azureApplicationManagement.CodeInfo, "Enter a valid selection.");

			azureApplicationManagement.Code = AzureB2CEnvironmentCodeDescriptionList.Codes.PRD;
			AssertNoErrors(azureApplicationManagement.CodeInfo);
		}

		public void TestValidateUrl()
		{
			var azureApplicationManagement = new AzureOpenIDConnectConfiguration();
			AssertNoErrors(azureApplicationManagement.AuthorityUrlInfo);

			azureApplicationManagement.ValidateUrl();
			AssertHasError(azureApplicationManagement.AuthorityUrlInfo, "Please enter a value.");

			azureApplicationManagement.AuthorityUrl = "Test";
			azureApplicationManagement.ValidateUrl();
			AssertHasError(azureApplicationManagement.AuthorityUrlInfo, "Please enter the correct URL.");

			azureApplicationManagement.AuthorityUrl = "https://www.example.com";
			azureApplicationManagement.ValidateUrl();
			AssertNoErrors(azureApplicationManagement.AuthorityUrlInfo);
		}
	
		public void TestRunPreSaveValidationCore()
		{
			var azureApplicationManagement = new AzureOpenIDConnectConfiguration();
			azureApplicationManagement.Code = AzureB2CEnvironmentCodeDescriptionList.Codes.PRD;

			azureApplicationManagement.ClearAllNotifications();
			azureApplicationManagement.RunPreSaveValidation();

			AssertHasError(azureApplicationManagement.AuthorityUrlInfo, "Please enter a value.");
			AssertHasError(azureApplicationManagement.ClientIDInfo, "Please enter a value.");

			azureApplicationManagement.Code = AzureB2CEnvironmentCodeDescriptionList.Codes.PRD;
			azureApplicationManagement.ClientID = "Test";
			azureApplicationManagement.AuthorityUrl = "http://test.com";
			azureApplicationManagement.RunPreSaveValidation();

			AssertNoErrors(azureApplicationManagement.CodeInfo);
			AssertNoErrors(azureApplicationManagement.AuthorityUrlInfo);
			AssertNoErrors(azureApplicationManagement.ClientIDInfo);
		}

		#endregion
	}
}
