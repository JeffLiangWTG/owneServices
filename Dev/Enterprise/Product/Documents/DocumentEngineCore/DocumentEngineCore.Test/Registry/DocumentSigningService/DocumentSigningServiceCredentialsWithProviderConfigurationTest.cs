using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;
using static Enterprise.DocumentEngineCore.Registry.DocumentSigningRegistryConstants;

namespace Enterprise.DocumentEngineCore.Registry.Testing
{
	[TestedType(typeof(DocumentSigningServiceCredentialsWithProviderConfiguration))]
	public class DocumentSigningServiceCredentialsWithProviderConfigurationTest : RegistryBusinessObjectTemplateTestCase<DocumentSigningServiceCredentialsWithProviderConfiguration>
	{
		public void TestProviderCodeHasChanges()
		{
			Config.ProviderCode = PdfSigningOptionCodes.EmudhraV1;
			AssertEquals(true, Config.HasChanges);
		}

		public void TestClientIDHasChanges()
		{
			Config.ClientID = "1";
			AssertEquals(true, Config.HasChanges);
		}

		public void TestAccessKeyHasChanges()
		{
			Config.AccessKey = "1";
			AssertEquals(true, Config.HasChanges);
		}
		public void TestKeyIDHasChanges()
		{
			Config.KeyID = "1";
			AssertEquals(true, Config.HasChanges);
		}

		public void TestProviderCodeValidation()
		{
			AssertMandatoryValidation(Config.ProviderCodeInfo);
		}

		public void TestClientIDValidation()
		{
			AssertMandatoryValidation(Config.ClientIDInfo);
		}

		public void TestAccessKeyValidation()
		{
			AssertMandatoryValidation(Config.AccessKeyInfo);
		}

		public void TestKeyIDValidation()
		{
			AssertMandatoryValidation(Config.KeyIDInfo);
		}

		void AssertMandatoryValidation(ZPropertyInfo propertyToTestForMandatoryValidation)
		{
			propertyToTestForMandatoryValidation.Value = (ZString)"wii";
			AssertNoError(propertyToTestForMandatoryValidation, "Please enter a value.");

			propertyToTestForMandatoryValidation.Value = ZString.Empty;
			AssertHasError(propertyToTestForMandatoryValidation, "Please enter a value.");

			// ensures notifications are cleared
			propertyToTestForMandatoryValidation.Value = (ZString)"wii";
			AssertNoError(propertyToTestForMandatoryValidation, "Please enter a value.");
		}

		public void TestSaveLoad()
		{
			BizObj.ProviderCode = PdfSigningOptionCodes.EmudhraV1;
			BizObj.ClientID = "client";
			BizObj.AccessKey = "key";
			BizObj.KeyID = "id";

			DocumentsDataRegistry.Instance.DocumentSigningServiceCredentials.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, BizObj);

			var loaded = DocumentsDataRegistry.Instance.DocumentSigningServiceCredentials.Value;

			AssertEquals("EMD", loaded.ProviderCode);
			AssertEquals("client", loaded.ClientID);
			AssertEquals("key", loaded.AccessKey);
			AssertEquals("id", loaded.KeyID);
		}

		#region Implementation

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		protected override DocumentSigningServiceCredentialsWithProviderConfiguration GetBusinessObjectToClone()
		{
			var result = new DocumentSigningServiceCredentialsWithProviderConfiguration();

			result.ProviderCode = PdfSigningOptionCodes.EmudhraV1;
			result.ClientID = "client";
			result.AccessKey = "key";
			result.KeyID = "id";

			return result;
		}

		protected override DocumentSigningServiceCredentialsWithProviderConfiguration GetBusinessObjectToSerialise()
		{
			return GetBusinessObjectToClone();
		}

		DocumentSigningServiceCredentialsWithProviderConfiguration Config
		{
			get
			{
				if (fConfig == null)
				{
					fConfig = (DocumentSigningServiceCredentialsWithProviderConfiguration)GetNewBusinessObject();
				}
				return fConfig;
			}
		}

		DocumentSigningServiceCredentialsWithProviderConfiguration fConfig;

		#endregion
	}
}
