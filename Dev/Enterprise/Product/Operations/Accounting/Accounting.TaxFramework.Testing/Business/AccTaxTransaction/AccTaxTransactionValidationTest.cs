using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Accounting.Helpers;
using Moq;
using NUnit.Framework;
using static Enterprise.MasterFiles.Business.AccountingMasterFilesTaxFrameworkConstants;

namespace Enterprise.Accounting.TaxFramework.Business.Testing
{
	public class AccTaxTransactionValidationTest : BusinessObjectValidationTestCase
	{
		public void TestValidateDataRefreshBusChanges()
		{
			var dataRefreshBusUpdateDeciderMock = new Mock<IDataRefreshBusUpdateActionDecider>();
			BusinessObject passedObject = null;
			dataRefreshBusUpdateDeciderMock
				.Setup(x => x.ValidateDataRefreshBusChanges(It.IsAny<AccTaxTransaction>()))
				.Callback<BusinessObject>(x => passedObject = x);
			ObjectFactory.Substitute(dataRefreshBusUpdateDeciderMock.Object);

			var validationTotTest = new AccTaxTransactionValidation(taxTransaction);
			validationTotTest.ValidateAll();
			dataRefreshBusUpdateDeciderMock.Verify(x => x.ValidateDataRefreshBusChanges(It.IsAny<AccTaxTransaction>()));
			AssertEquals("Correct parameter passed into ValidateDataRefreshBusChanges method.", taxTransaction, passedObject);
		}

		public void TestValidateATT_AH()
		{
			var invoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), TestObjectCreator.AUD, 1M, TestObjectCreator.AALSHI);

			taxTransaction.RunPreSaveValidation();
			AssertHasError(taxTransaction.ATT_AHInfo, "Please enter a value.");

			Assert(!invoice.IsCancelled);
			taxTransaction.ATT_AH = invoice.PK;
			taxTransaction.RunPreSaveValidation();
			AssertNoErrors(taxTransaction.ATT_AHInfo);

			invoice.IsCancelled = true;
			taxTransaction.ATT_AH = invoice.PK;
			taxTransaction.RunPreSaveValidation();
			AssertNoErrors(taxTransaction.ATT_AHInfo);
		}

		public void TestValidateATT_Rate()
		{
			Factory.SetContext(BusinessContext.SystemGeneratedTaxRecordCreationInProgress);
			taxTransaction.ATT_RateNumerator = 0;
			AssertNull("Precondition: ", taxTransaction.GetSystemCalculatedValuesIfAvailable());

			taxTransaction.RunPreSaveValidation();
			AssertError(taxTransaction.ATT_RateInfo, "Rate cannot be zero.");

			SetTaxTransactionsSystemCalculatedValues_ForTests(taxTransaction);
			Factory.RemoveContext(BusinessContext.SystemGeneratedTaxRecordCreationInProgress);

			AssertNotNull("Precondition: ", taxTransaction.GetSystemCalculatedValuesIfAvailable());
			taxTransaction.RunPreSaveValidation();
			AssertError(taxTransaction.ATT_RateInfo, "No valid tax rate found for tax ID ''. Please check the Rate Source of the tax ID and make sure there are valid tax rate for the Rate Source.");

			taxTransaction.ATT_RateNumerator = 1;
			AssertNoErrors(taxTransaction.ATT_RateInfo);

			taxTransaction.ATT_RateNumerator = 0;
			var taxId = TestObjectCreator.CreateTaxRateWithoutZZ("GS1", "Other tax", 10);
			taxTransaction.ATT_AT_TaxID = taxId.PK;
			var expectedError = "No valid tax rate found for tax ID 'GS1'. Please check the Rate Source of the tax ID and make sure there are valid tax rate for the Rate Source.";
			AssertError(taxTransaction.ATT_RateInfo, expectedError);

			taxTransaction.ATT_RateNumerator = 5;
			AssertNoErrors(taxTransaction.ATT_RateInfo);

			taxTransaction.ATT_RateDenominator = 0;
			AssertError(taxTransaction.ATT_RateInfo, expectedError);

			taxTransaction.ATT_RateDenominator = 1;
			AssertNoErrors(taxTransaction.ATT_RateInfo);

			using (taxTransaction.GetValidationSuspender())
			{
				Factory.SetContext(BusinessContext.SystemGeneratedTaxRecordCreationInProgress);
				taxTransaction.ATT_RateNumerator = 1;
				SetTaxTransactionsSystemCalculatedValues_ForTests(taxTransaction);
				Factory.RemoveContext(BusinessContext.SystemGeneratedTaxRecordCreationInProgress);
			}
			AssertNoErrors(taxTransaction.ATT_RateInfo);

			taxTransaction.ATT_Ledger = TaxConfigurationLedgers.AccountsPayable.Code;
			taxTransaction.ATT_TaxSuperType = TaxSuperTypeList.Perceptions.Code;

			Assert("Precondition", !taxTransaction.ATT_RateInfo.ReadOnly);

			taxTransaction.ATT_Rate = 0;
			AssertError(taxTransaction.ATT_RateInfo, "Rate cannot be zero.");

			taxTransaction.ATT_Rate = 100;
			AssertNoErrors(taxTransaction.ATT_RateInfo);

			taxTransaction.ATT_Rate = 101;
			AssertError(taxTransaction.ATT_RateInfo, "Rate must be less than 100%");

			taxTransaction.ATT_Rate = 1;
			AssertNoErrors(taxTransaction.ATT_RateInfo);

			taxTransaction.ATT_Rate = -1;
			AssertError(taxTransaction.ATT_RateInfo, "Rate must be positive.");
		}

		[TestDate(2020, 09, 10)]
		public void TestValidateATT_TaxDate()
		{
			taxTransaction.RunPreSaveValidation();
			AssertHasError(taxTransaction.ATT_TaxDateInfo, "Please enter a Tax Date.");

			taxTransaction.ATT_TaxDate = ZDate.Today;
			AssertNoErrors(taxTransaction.ATT_TaxDateInfo);
		}

		public void TestValidateATT_OSTaxAmount()
		{
			var taxFrameworkTestObjectCreator = new TaxFrameworkTestObjectCreator(Factory);
			var taxConfig = taxFrameworkTestObjectCreator.CreateTaxConfiguration(TaxConfigurationLedgers.AccountsPayable.Code);
			taxTransaction.ATT_ETC = taxConfig.PK;
			taxTransaction.ATT_GC = GlbCompany.CurrentCompany.PK;

			Factory.SetContext(BusinessContext.SystemGeneratedTaxRecordCreationInProgress);
			taxTransaction.ATT_OSTaxAmount = -10M;

			AssertNull("Precondition: ", taxTransaction.GetSystemCalculatedValuesIfAvailable());
			taxTransaction.RunPreSaveValidation();
			AssertNoErrors(taxTransaction.ATT_OSTaxAmountInfo);

			SetTaxTransactionsSystemCalculatedValues_ForTests(taxTransaction);
			Factory.RemoveContext(BusinessContext.SystemGeneratedTaxRecordCreationInProgress);

			AssertNotNull("Precondition: ", taxTransaction.GetSystemCalculatedValuesIfAvailable());
			taxTransaction.RunPreSaveValidation();
			AssertNoErrors(taxTransaction.ATT_OSTaxAmountInfo);

			taxTransaction.ATT_OSTaxAmount = 10M;
			AssertError(taxTransaction.ATT_OSTaxAmountInfo, "Please do not change sign for Tax Amount.");

			taxTransaction.ATT_OSTaxAmount = -20M;
			AssertNoErrors(taxTransaction.ATT_OSTaxAmountInfo);
		}

		public void TestValidateATT_OSTaxBaseAmount()
		{
			Factory.SetContext(BusinessContext.SystemGeneratedTaxRecordCreationInProgress);
			taxTransaction.ATT_OSTaxBaseAmount = -100M;

			AssertNull("Precondition: ", taxTransaction.GetSystemCalculatedValuesIfAvailable());
			taxTransaction.RunPreSaveValidation();
			AssertNoErrors(taxTransaction.ATT_OSTaxBaseAmountInfo);

			SetTaxTransactionsSystemCalculatedValues_ForTests(taxTransaction);
			Factory.RemoveContext(BusinessContext.SystemGeneratedTaxRecordCreationInProgress);

			AssertNotNull("Precondition: ", taxTransaction.GetSystemCalculatedValuesIfAvailable());
			taxTransaction.RunPreSaveValidation();
			AssertNoErrors(taxTransaction.ATT_OSTaxBaseAmountInfo);

			taxTransaction.ATT_OSTaxBaseAmount = 100M;
			AssertError(taxTransaction.ATT_OSTaxBaseAmountInfo, "Please do not change sign for Tax Base Amount.");

			taxTransaction.ATT_OSTaxBaseAmount = -200M;
			AssertNoErrors(taxTransaction.ATT_OSTaxBaseAmountInfo);
		}

		public void TestATT_TaxAuthorityServiceCodeAndDescriptionMandatory()
		{
			ZString mandatoryErrorMessage = "Please check the Tax Authority Service Code and Tax Authority Service Code Description entered against this Tax Transaction Record. Please enter a value in both columns or ensure both columns are empty";
			taxTransaction.ATT_TaxAuthorityServiceCode = "54.9887";
			taxTransaction.ATT_TaxAuthorityServiceCodeDescription = "";
			AssertError(taxTransaction.ATT_TaxAuthorityServiceCodeDescriptionInfo, mandatoryErrorMessage);

			taxTransaction.ATT_TaxAuthorityServiceCodeDescription = "Legislation 5.2020(809)";
			taxTransaction.ATT_TaxAuthorityServiceCode = "";
			AssertError(taxTransaction.ATT_TaxAuthorityServiceCodeInfo, mandatoryErrorMessage);

			taxTransaction.ATT_TaxAuthorityServiceCodeDescription = "Legislation 5.2020(809)";
			taxTransaction.ATT_TaxAuthorityServiceCode = "54.9887";
			AssertNoErrors(taxTransaction.ATT_TaxAuthorityServiceCodeDescriptionInfo);
			AssertNoErrors(taxTransaction.ATT_TaxAuthorityServiceCodeInfo);
		}

		protected virtual void AssertError(ZPropertyInfo propertyInfo, string message)
		{
			AssertHasError(propertyInfo, message);
		}

		protected void SetTaxTransactionsSystemCalculatedValues_ForTests(AccTaxTransaction taxRecord)
		{
			taxRecord.SetTaxTransactionsSystemCalculatedValues(
				taxRecord.ATT_OSTaxBaseAmount,
				taxRecord.ATT_OSTaxAmount,
				taxRecord.ATT_RateNumerator,
				taxRecord.ATT_RateDenominator,
				taxRecord.ATT_TaxDate,
				taxRecord.ATT_TaxAuthorityServiceCode,
				taxRecord.ATT_TaxAuthorityServiceCodeDescription);
		}

		protected AccTaxTransaction taxTransaction;

		protected override void SetUp()
		{
			base.SetUp();
			taxTransaction = CreateTaxTransaction();
		}

		protected virtual AccTaxTransaction CreateTaxTransaction()
		{
			var taxTransaction = Factory.New<AccTaxTransaction>();
			return taxTransaction;
		}

		TestObjectCreator TestObjectCreator => testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory));
		TestObjectCreator testObjectCreator;
	}
}
