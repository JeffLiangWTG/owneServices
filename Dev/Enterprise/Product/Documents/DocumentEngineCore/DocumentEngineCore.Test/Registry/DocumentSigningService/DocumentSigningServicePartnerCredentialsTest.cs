using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentEngineCore.Registry.Testing
{
	[TestedType(typeof(DocumentSigningServicePartnerCredentials))]
	public class DocumentSigningServicePartnerCredentialsTest : RegistryBusinessObjectTemplateTestCase<DocumentSigningServicePartnerCredentials>
	{
		public void TestValidations()
		{
			AssertMandatoryValidation(Config.PartnerIDInfo);
			AssertMandatoryValidation(Config.PartnerAccessKeyInfo);
		}

		void AssertMandatoryValidation(ZPropertyInfo propertyToTestForMandatoryValidation)
		{
			propertyToTestForMandatoryValidation.Value = (ZString)"wii";
			AssertNoError(propertyToTestForMandatoryValidation, $"Please enter a {propertyToTestForMandatoryValidation.HumanReadableName}.");

			propertyToTestForMandatoryValidation.Value = ZString.Empty;
			AssertHasError(propertyToTestForMandatoryValidation, $"Please enter a {propertyToTestForMandatoryValidation.HumanReadableName}.");

			// ensures notifications are cleared
			propertyToTestForMandatoryValidation.Value = (ZString)"wii";
			AssertNoError(propertyToTestForMandatoryValidation, $"Please enter a {propertyToTestForMandatoryValidation.HumanReadableName}.");
		}

		public void TestSaveLoad()
		{
			BizObj.PartnerID = "client";
			BizObj.PartnerAccessKey = "key";

			DocumentsDataRegistry.Instance.DocumentSigningServicePartnerCredentials.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, BizObj);

			var loaded = DocumentsDataRegistry.Instance.DocumentSigningServicePartnerCredentials.Value;

			AssertEquals("client", loaded.PartnerID);
			AssertEquals("key", loaded.PartnerAccessKey);
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

		protected override DocumentSigningServicePartnerCredentials GetBusinessObjectToClone()
		{
			DocumentSigningServicePartnerCredentials result = new DocumentSigningServicePartnerCredentials();

			result.PartnerID = "client";
			result.PartnerAccessKey = "key";
			return result;
		}

		protected override DocumentSigningServicePartnerCredentials GetBusinessObjectToSerialise()
		{
			return GetBusinessObjectToClone();
		}

		DocumentSigningServicePartnerCredentials Config
		{
			get
			{
				if (fConfig == null)
				{
					fConfig = (DocumentSigningServicePartnerCredentials)GetNewBusinessObject();
				}
				return fConfig;
			}
		}

		DocumentSigningServicePartnerCredentials fConfig;

		#endregion
	}
}
