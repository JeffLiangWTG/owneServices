using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentEngineCore.Registry.Testing
{
	[TestedType(typeof(DocumentSigningServiceCredentialsConfiguration))]
	public class DocumentSigningServiceCredentialsConfigurationTest : RegistryBusinessObjectTemplateTestCase<DocumentSigningServiceCredentialsConfiguration>
	{
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
			BizObj.ClientID = "client";
			BizObj.AccessKey = "key";
			BizObj.KeyID = "id";

			DocumentsDataRegistry.Instance.DocumentSigningServicePartnerCredentialsAccessToken.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, BizObj);

			var loaded = DocumentsDataRegistry.Instance.DocumentSigningServicePartnerCredentialsAccessToken.Value;

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

		protected override DocumentSigningServiceCredentialsConfiguration GetBusinessObjectToClone()
		{
			var result = new DocumentSigningServiceCredentialsConfiguration();

			result.ClientID = "client";
			result.AccessKey = "key";
			result.KeyID = "id";

			return result;
		}

		protected override DocumentSigningServiceCredentialsConfiguration GetBusinessObjectToSerialise()
		{
			return GetBusinessObjectToClone();
		}

		DocumentSigningServiceCredentialsConfiguration Config
		{
			get
			{
				if (fConfig == null)
				{
					fConfig = (DocumentSigningServiceCredentialsConfiguration)GetNewBusinessObject();
				}
				return fConfig;
			}
		}

		DocumentSigningServiceCredentialsConfiguration fConfig;

		#endregion
	}
}
