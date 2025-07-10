using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.JAS.Business.Testing
{
	class JASDataExporterBizOValidationTest : BusinessObjectValidationTestCase
	{
		public void TestAutoValidationType()
		{
			AssertEquals(ExpectedAutoValidationType, DataExporterBizO.Validation.GetType());
		}

		public void TestValidateAll()
		{
			DataExporterBizO.DeliveryMethod = "";
			DataExporterBizO.Validation.ValidateAll();
			AssertMandatoryValidationError(DataExporterBizO.DeliveryMethodInfo, true);
			DataExporterBizO.DeliveryMethod = JASDataExporterBizO.EmailDeliveryMethodCode;
			DataExporterBizO.IsIndividualEmailRecipient = true;
			DataExporterBizO.Validation.ValidateAll();
			AssertMandatoryValidationError(DataExporterBizO.DeliveryMethodInfo, false);
			AssertMandatoryValidationError(DataExporterBizO.EmailAddressInfo, true);
			AssertMandatoryValidationError(DataExporterBizO.EmailGroupPKInfo, false);
			AssertMandatoryValidationError(DataExporterBizO.ExportDirectoryInfo, false);
			DataExporterBizO.IsGroupEmailRecipient = true;
			DataExporterBizO.Validation.ValidateAll();
			AssertMandatoryValidationError(DataExporterBizO.DeliveryMethodInfo, false);
			AssertMandatoryValidationError(DataExporterBizO.EmailAddressInfo, false);
			AssertMandatoryValidationError(DataExporterBizO.EmailGroupPKInfo, true);
			AssertMandatoryValidationError(DataExporterBizO.ExportDirectoryInfo, false);
			DataExporterBizO.DeliveryMethod = JASDataExporterBizO.DirectoryDeliveryMethodCode;
			DataExporterBizO.Validation.ValidateAll();
			AssertMandatoryValidationError(DataExporterBizO.DeliveryMethodInfo, false);
			AssertMandatoryValidationError(DataExporterBizO.EmailAddressInfo, false);
			AssertMandatoryValidationError(DataExporterBizO.EmailGroupPKInfo, false);
			AssertMandatoryValidationError(DataExporterBizO.ExportDirectoryInfo, true);
		}

		public void TestValidateDeliveryMethod()
		{
			DataExporterBizO.DeliveryMethod = JASDataExporterBizO.DirectoryDeliveryMethodCode;
			AssertNoErrors("Valid Delivery Method", DataExporterBizO.DeliveryMethodInfo);
			DataExporterBizO.DeliveryMethod = "";
			AssertMandatoryValidationError(DataExporterBizO.DeliveryMethodInfo, true);
			AssertListValidationInvalidCodeError(DataExporterBizO.DeliveryMethodInfo, false);
			DataExporterBizO.DeliveryMethod = "sdk";
			AssertMandatoryValidationError(DataExporterBizO.DeliveryMethodInfo, false);
			AssertListValidationInvalidCodeError(DataExporterBizO.DeliveryMethodInfo, true);
		}

		public void TestValidateExportDirectory()
		{
			DataExporterBizO.DeliveryMethod = JASDataExporterBizO.DirectoryDeliveryMethodCode;
			DataExporterBizO.ExportDirectory = Env.TempPath;
			AssertNoErrors("Valid Export Directory", DataExporterBizO.ExportDirectoryInfo);
			DataExporterBizO.ExportDirectory = "";
			AssertMandatoryValidationError(DataExporterBizO.ExportDirectoryInfo, true);
			DataExporterBizO.ExportDirectory = @"\ThisIsA\Clients\JAS\TestDirectoryThatDoesNotExist_";
			string expectedNotification = "Directory \"\\ThisIsA\\Clients\\JAS\\TestDirectoryThatDoesNotExist_\" does not exist. Please select a different directory.";
			AssertHasError(DataExporterBizO.ExportDirectoryInfo, expectedNotification);
		}

		public void TestShouldNotValidateExportDirectorIfNotDirectoryDeliveryMethod()
		{
			DataExporterBizO.DeliveryMethod = JASDataExporterBizO.EmailDeliveryMethodCode;
			DataExporterBizO.ExportDirectory = Env.TempPath;
			AssertNoErrors("Should not validate if DeliveryMode is not Directory", DataExporterBizO.ExportDirectoryInfo);
			DataExporterBizO.ExportDirectory = "";
			AssertNoErrors("Should not validate if DeliveryMode is not Directory", DataExporterBizO.ExportDirectoryInfo);
		}

		public void TestValidateEmailAddress()
		{
			DataExporterBizO.DeliveryMethod = JASDataExporterBizO.EmailDeliveryMethodCode;
			DataExporterBizO.IsIndividualEmailRecipient = true;
			DataExporterBizO.EmailAddress = "CargowiseDevelopment@cargowise.com";
			AssertNoErrors("Valid email address", DataExporterBizO.EmailAddressInfo);
			DataExporterBizO.EmailAddress = "";
			AssertMandatoryValidationError(DataExporterBizO.EmailAddressInfo, true);
			DataExporterBizO.EmailAddress = "asdflkjsadf";
			AssertHasErrorContaining(DataExporterBizO.EmailAddressInfo, "Email Address is not valid");
		}

		public void TestValidateEmailAddressIfDeliveryMethodNotEmailAndRecipientTypeNotIndividual()
		{
			DataExporterBizO.DeliveryMethod = JASDataExporterBizO.DirectoryDeliveryMethodCode;
			DataExporterBizO.IsIndividualEmailRecipient = true;
			DataExporterBizO.EmailAddress = "";
			DataExporterBizO.Validation.ValidateEmailAddress();
			AssertNoErrors("Should not validate if DeliveryMode is not Email", DataExporterBizO.EmailAddressInfo);
			DataExporterBizO.DeliveryMethod = JASDataExporterBizO.EmailDeliveryMethodCode;
			DataExporterBizO.IsGroupEmailRecipient = true;
			DataExporterBizO.Validation.ValidateEmailAddress();
			AssertNoErrors("Should not validate if RecipientType is not Individual", DataExporterBizO.EmailAddressInfo);
		}

		public void TestValidateEmailGroup()
		{
			DataExporterBizO.EmailGroups.RemoveAll();
			GlbGroup group = DataExporterBizO.EmailGroups.AddNew();
			group.GG_Code = "MEH";
			DataExporterBizO.DeliveryMethod = JASDataExporterBizO.EmailDeliveryMethodCode;
			DataExporterBizO.IsGroupEmailRecipient = true;
			DataExporterBizO.EmailGroupPK = group.PK;
			AssertNoErrors("Valid email group", DataExporterBizO.EmailGroupPKInfo);
			DataExporterBizO.EmailGroupPK = ZGuid.Empty;
			AssertMandatoryValidationError(DataExporterBizO.EmailGroupPKInfo, true);
			DataExporterBizO.EmailGroupPK = ZGuid.NewZGuid();
			AssertListValidationInvalidCodeError(DataExporterBizO.EmailGroupPKInfo, true);
		}

		public void TestValidateEmailGroupIfDeliveryMethodNotEmailAndRecipientTypeNotGroup()
		{
			DataExporterBizO.EmailGroups.RemoveAll();
			GlbGroup group = DataExporterBizO.EmailGroups.AddNew();
			group.GG_Code = "MEH";
			DataExporterBizO.DeliveryMethod = JASDataExporterBizO.DirectoryDeliveryMethodCode;
			DataExporterBizO.IsGroupEmailRecipient = true;
			DataExporterBizO.EmailGroupPK = ZGuid.Empty;
			DataExporterBizO.Validation.ValidateEmailGroupPK();
			AssertNoErrors("Should not validate if DeliveryMode is not Email", DataExporterBizO.EmailGroupPKInfo);
			DataExporterBizO.DeliveryMethod = JASDataExporterBizO.EmailDeliveryMethodCode;
			DataExporterBizO.IsIndividualEmailRecipient = true;
			DataExporterBizO.Validation.ValidateEmailGroupPK();
			AssertNoErrors("Should not validate if RecipientType is not Group", DataExporterBizO.EmailGroupPKInfo);
		}

		protected JASDataExporterBizO DataExporterBizO
		{
			get
			{
				if (fDataExporterBizO == null)
				{
					fDataExporterBizO = GetNewJASDataExporterBizO();
				}

				return fDataExporterBizO;
			}
		}

		protected virtual JASDataExporterBizO GetNewJASDataExporterBizO()
		{
			return new JASDataExporterBizOForTest();
		}

		protected virtual Type ExpectedAutoValidationType
		{
			get
			{
				return typeof(JASDataExporterBizOValidation);
			}
		}

		JASDataExporterBizO fDataExporterBizO;
	}
}
