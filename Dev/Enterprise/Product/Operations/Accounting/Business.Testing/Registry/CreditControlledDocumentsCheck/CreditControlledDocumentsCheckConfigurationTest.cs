using CargoWise.ComponentModel;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Organisation.Registry;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	[TestedType(typeof(CreditControlledDocumentsCheckConfiguration))]
	public class CreditControlledDocumentsCheckConfigurationTest : RegistryBusinessObjectTemplateTestCase
	{
		public void TestInvoiceTypeList()
		{
			AssertEquals("InvoiceTypeList should have 3 elements", 3, BizObj.InvoiceTypeList.Count);
			AssertEquals("The InvoiceTypeList should contain All", true, BizObj.InvoiceTypeList.ContainsCode(CreditControlledDocumentsCheckConfigurationInvoiceTypes.All));
			AssertEquals("The InvoiceTypeList should contain DSB", true, BizObj.InvoiceTypeList.ContainsCode(CreditControlledDocumentsCheckConfigurationInvoiceTypes.DSB));
			AssertEquals("The InvoiceTypeList should contain NDB", true, BizObj.InvoiceTypeList.ContainsCode(CreditControlledDocumentsCheckConfigurationInvoiceTypes.NDB));
		}

		public void TestValidateNumberOfDaysOverdue()
		{
			var config = new CreditControlledDocumentsCheckConfiguration();
			config.NumberOfDaysOverdue = 0;
			config.RunPreSaveValidation();
			AssertHasError(config.NumberOfDaysOverdueInfo, "Please enter a value.");
			config.NumberOfDaysOverdue = 366;
			AssertHasError(config.NumberOfDaysOverdueInfo, "Number of Days Overdue must be between 1 and 365");
			config.NumberOfDaysOverdue = 1;
			AssertNoErrors(config.NumberOfDaysOverdueInfo);
			config.NumberOfDaysOverdue = 365;
			AssertNoErrors(config.NumberOfDaysOverdueInfo);
		}

		public void TestValidateNumberOfDaysOverdueHigherAmountRecordsMustHaveHigherNumberOfDays()
		{
			var collection = new CreditControlledDocumentsCheckConfigurationCollection();

			var config1 = collection.AddNew();
			config1.InvoiceType = CreditControlledDocumentsCheckConfigurationInvoiceTypes.DSB.Code;
			config1.Amount = 10M;
			config1.Range = AmountBasedMultiLevelAuthorisationRequirement.RangeCodes.UpTo;
			config1.AuthorisationRequirement = AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.NoApprovalRequired;
			config1.NumberOfDaysOverdue = 5;

			var config2 = collection.AddNew();
			config2.InvoiceType = CreditControlledDocumentsCheckConfigurationInvoiceTypes.DSB.Code;
			config2.Amount = 20M;
			config2.Range = AmountBasedMultiLevelAuthorisationRequirement.RangeCodes.UpTo;
			config2.AuthorisationRequirement = AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.FirstApprovalRequiredOnly;
			config2.NumberOfDaysOverdue = 15;

			var config3 = collection.AddNew();
			config3.InvoiceType = CreditControlledDocumentsCheckConfigurationInvoiceTypes.DSB.Code;
			config3.Amount = 20M;
			config3.Range = AmountBasedMultiLevelAuthorisationRequirement.RangeCodes.Above;
			config3.AuthorisationRequirement = AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.ThirdApprovalRequiredOnly;
			config3.NumberOfDaysOverdue = 15;

			collection.RunPreSaveValidation();

			Assert("No errors", !collection.HasErrors());

			config1.NumberOfDaysOverdue = 20;
			var expectedErrorMessage = "Higher amounts must have 'Number Of Days Overdue' higher than lower amounts.";
			AssertHasError(config1.NumberOfDaysOverdueInfo, expectedErrorMessage);

			config1.NumberOfDaysOverdue = 10;
			AssertNoErrors(config1.NumberOfDaysOverdueInfo);

			expectedErrorMessage = "The Above Line's 'Number Of Days Overdue' must be 15.";
			config3.NumberOfDaysOverdue = 10;
			AssertHasError(config3.NumberOfDaysOverdueInfo, expectedErrorMessage);
		}

		public void TestValidateInvoiceType()
		{
			var collection = new CreditControlledDocumentsCheckConfigurationCollection();
			var config = collection.AddNew();
			config.InvoiceType = "";
			config.RunPreSaveValidation();
			AssertHasError(config.InvoiceTypeInfo, "Please enter a value.");
			config.InvoiceType = OrgARTermsLookups.InvoiceTypes.All.Code;
			AssertNoErrors(config.InvoiceTypeInfo);
			config.InvoiceType = "XYZ";
			AssertHasError(config.InvoiceTypeInfo, "Enter a valid selection.");
		}

		public void TestSameOrConflictingInvoiceType()
		{
			var collection = new CreditControlledDocumentsCheckConfigurationCollection();

			var config1 = collection.AddNew();
			config1.InvoiceType = CreditControlledDocumentsCheckConfigurationInvoiceTypes.DSB.Code;
			config1.Amount = 10M;
			config1.Range = AmountBasedMultiLevelAuthorisationRequirement.RangeCodes.UpTo;
			config1.AuthorisationRequirement = AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.NoApprovalRequired;
			config1.NumberOfDaysOverdue = 5;

			var config2 = collection.AddNew();
			config2.InvoiceType = CreditControlledDocumentsCheckConfigurationInvoiceTypes.DSB.Code;
			config2.Amount = 20M;
			config2.Range = AmountBasedMultiLevelAuthorisationRequirement.RangeCodes.UpTo;
			config2.AuthorisationRequirement = AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.FirstApprovalRequiredOnly;
			config2.NumberOfDaysOverdue = 15;

			var config3 = collection.AddNew();
			config3.InvoiceType = CreditControlledDocumentsCheckConfigurationInvoiceTypes.DSB.Code;
			config3.Amount = 20M;
			config3.Range = AmountBasedMultiLevelAuthorisationRequirement.RangeCodes.Above;
			config3.AuthorisationRequirement = AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.ThirdApprovalRequiredOnly;
			config3.NumberOfDaysOverdue = 15;

			collection.RunPreSaveValidation();

			Assert("No errors", !collection.HasErrors());

			var configAll = collection.AddNew();
			configAll.InvoiceType = CreditControlledDocumentsCheckConfigurationInvoiceTypes.All.Code;
			configAll.Amount = 10M;
			configAll.Range = AmountBasedMultiLevelAuthorisationRequirement.RangeCodes.UpTo;
			configAll.AuthorisationRequirement = AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.NoApprovalRequired;
			configAll.NumberOfDaysOverdue = 5;

			collection.RunPreSaveValidation();

			Assert("Registry now has errors", collection.HasErrors());

			var expectedErrorMessage = "This setting already exists.";
			AssertHasError(config1.InvoiceTypeInfo, expectedErrorMessage);
			AssertHasError(configAll.InvoiceTypeInfo, expectedErrorMessage);

			config1.InvoiceTypeInfo.ClearAllNotifications();
			configAll.InvoiceTypeInfo.ClearAllNotifications();

			configAll = collection.AddNew();
			configAll.InvoiceType = CreditControlledDocumentsCheckConfigurationInvoiceTypes.All.Code;
			configAll.Amount = 10M;
			configAll.Range = AmountBasedMultiLevelAuthorisationRequirement.RangeCodes.UpTo;
			configAll.AuthorisationRequirement = AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.NoApprovalRequired;
			configAll.NumberOfDaysOverdue = 15;

			collection.RunPreSaveValidation();

			Assert("Registry now has errors", collection.HasErrors());

			expectedErrorMessage = "This setting conflicts with another setting.";
			AssertHasError(config1.InvoiceTypeInfo, expectedErrorMessage);
			AssertHasError(configAll.InvoiceTypeInfo, expectedErrorMessage);
		}

		#region Implementation

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			var result = new CreditControlledDocumentsCheckConfiguration();
			result.Amount = 10m;
			result.NumberOfDaysOverdue = 1;
			result.InvoiceType = OrgARTermsLookups.InvoiceTypes.All.Code;
			result.Range = AmountBasedMultiLevelAuthorisationRequirement.RangeCodes.Above;
			result.AuthorisationRequirement = AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.NoApprovalRequired;

			return result;
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise()
		{
			return GetBusinessObjectToClone();
		}

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		protected new CreditControlledDocumentsCheckConfiguration BizObj
		{
			get { return (CreditControlledDocumentsCheckConfiguration)base.BizObj; }
		}

		protected virtual CreditControlledDocumentsCheckConfigurationCollection GetAuthorisationSettingsCollection()
		{
			return new CreditControlledDocumentsCheckConfigurationCollection();
		}

		#endregion
	}
}
