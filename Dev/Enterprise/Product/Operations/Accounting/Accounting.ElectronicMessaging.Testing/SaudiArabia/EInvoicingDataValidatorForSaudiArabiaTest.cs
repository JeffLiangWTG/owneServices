using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.ElectronicMessaging.Common.DataValidation;
using Enterprise.Accounting.ElectronicMessaging.Common.DataValidation.Testing;
using Enterprise.Accounting.ElectronicMessaging.Common.Testing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.ElectronicMessaging.SaudiArabia.Testing
{
	public class EInvoicingDataValidatorForSaudiArabiaTest : BaseEInvoicingDataValidatorTest
	{
		#region Integration Tests

		public void TestValidCredential_HasNoErrors()
		{
			InitaliseStandardTestObjects();
			var credentialCreator = new TestEInvoicingCertificateCredentialCreator(SaudiArabiaCompany.FirstActiveBranch, ZDateTime.Today.AddDays(-10), ZDateTime.Today.AddDays(7));
			var certificateCredential = credentialCreator.CreateCertificateCredential();
			certificateCredential.GP_PasswordType = PasswordTypesList.Codes.EIM;

			Factory.Save();

			using (ConfigureErrorNotificationGroup())
			{
				GetEInvoicingDataValidatorForTest().Run(Logger);
			}

			AssertPivotHasNoErrors(Pivot, Batch);
			AssertEquals("No error level log messages should be logged", false, Logger.Logs.Any(x => x.Item1 == LogType.Error));
		}

		public void TestExpiredCredential_HasErrorOnPivot()
		{
			InitaliseStandardTestObjects();

			var credentialCreator = new TestEInvoicingCertificateCredentialCreator(SaudiArabiaCompany.FirstActiveBranch, ZDateTime.Today.AddDays(-10), ZDateTime.Today.AddDays(7));
			var certificateCredential = credentialCreator.CreateCertificateCredential();
			certificateCredential.GP_PasswordType = PasswordTypesList.Codes.EIM;
			certificateCredential.GP_ExpiryDate = ZDateTime.Today.AddDays(-8);

			Factory.Save();

			using (ConfigureErrorNotificationGroup())
			{
				GetEInvoicingDataValidatorForTest().Run(Logger);
			}

			AssertPivotHasError(Pivot, "Enable E-Reporting Functionality is set to Yes and Branch " + SaudiArabiaCompany.FirstActiveBranch.GB_Code + " has no valid E-Invoicing Credentials.");

			AssertEquals("One warning level log message should be logged for transaction with validation error", 1, Logger.Logs.Count(x => x.Item1 == LogType.Warning));
			Assert("Warning level log messages should contain the validation error", Logger.Logs.Any(x => x.Item1 == LogType.Warning && x.Item2.Contains(Pivot.AIP_ErrorDescription)));
		}

		public void TestInvoiceWithNegativeLines_HasErrorOnPivot()
		{
			InitaliseStandardTestObjects();

			var credentialCreator = new TestEInvoicingCertificateCredentialCreator(SaudiArabiaCompany.FirstActiveBranch, ZDateTime.Today.AddDays(-10), ZDateTime.Today.AddDays(7));
			var certificateCredential = credentialCreator.CreateCertificateCredential();
			certificateCredential.GP_PasswordType = PasswordTypesList.Codes.EIM;

			InvoiceLine.AL_OSExTaxAmount = -101m;

			Factory.Save();

			using (ConfigureErrorNotificationGroup())
			{
				GetEInvoicingDataValidatorForTest().Run(Logger);
			}

			AssertPivotHasError(Pivot, "Negative charges are not permitted in e-Invoicing transactions. Registry settings can prevent negative charges from being posted.");

			AssertEquals("One warning level log message should be logged for transaction with validation error", 1, Logger.Logs.Count(x => x.Item1 == LogType.Warning));
			Assert("Warning level log messages should contain the validation error", Logger.Logs.Any(x => x.Item1 == LogType.Warning && x.Item2.Contains(Pivot.AIP_ErrorDescription)));
		}

		public void TestCreditNoteWithNegativeLinesOnScreen_HasErrorOnPivot()
		{
			InitaliseStandardTestObjects(true);

			var credentialCreator = new TestEInvoicingCertificateCredentialCreator(SaudiArabiaCompany.FirstActiveBranch, ZDateTime.Today.AddDays(-10), ZDateTime.Today.AddDays(7));
			var branchCredential = credentialCreator.CreateCertificateCredential();
			branchCredential.GP_PasswordType = PasswordTypesList.Codes.EIM;

			CreditNoteLine.AL_OSExTaxAmount = -101m;

			Factory.Save();

			using (ConfigureErrorNotificationGroup())
			{
				GetEInvoicingDataValidatorForTest().Run(Logger);
			}

			AssertPivotHasError(Pivot, "Negative charges are not permitted in e-Invoicing transactions. Registry settings can prevent negative charges from being posted.");

			AssertEquals("One warning level log message should be logged for transaction with validation error", 1, Logger.Logs.Count(x => x.Item1 == LogType.Warning));
			Assert("Warning level log messages should contain the validation error", Logger.Logs.Any(x => x.Item1 == LogType.Warning && x.Item2.Contains(Pivot.AIP_ErrorDescription)));
		}

		public void TestCreditNoteWithPositiveLinesOnScreen_HasNoErrorOnPivot()
		{
			InitaliseStandardTestObjects(true);

			var credentialCreator = new TestEInvoicingCertificateCredentialCreator(SaudiArabiaCompany.FirstActiveBranch, ZDateTime.Today.AddDays(-10), ZDateTime.Today.AddDays(7));
			var certificateCredential = credentialCreator.CreateCertificateCredential();
			certificateCredential.GP_PasswordType = PasswordTypesList.Codes.EIM;

			Factory.Save();

			using (ConfigureErrorNotificationGroup())
			{
				GetEInvoicingDataValidatorForTest().Run(Logger);
			}

			AssertPivotHasNoErrors(Pivot, Batch);
			AssertEquals("No error level log messages should be logged", false, Logger.Logs.Any(x => x.Item1 == LogType.Error));
		}

		#endregion

		#region Implementation

		public override BaseEInvoicingDataValidator GetEInvoicingDataValidatorForTest()
			=> GetEInvoicingDataValidatorForTest(SaudiArabiaCompany);

		public BaseEInvoicingDataValidator GetEInvoicingDataValidatorForTest(GlbCompany company)
			=> new EInvoicingDataValidatorForSaudiArabia(company);

		static void AssertPivotHasNoErrors(AccEInvoicingTransactionPivot pivot, AccEInvoicingBatch batch)
		{
			AssertNullOrEmpty("No errors should be recorded when validation is successful.", pivot.AIP_ErrorDescription);
			AssertEquals("Pivot status should be unchanged as Batched when validation is successful", EInvoicingPivotState.Batched, pivot.AIP_Status);
			AssertEquals("Pivot should be part of the batch when validation is successful", batch.PK, pivot.AIP_AIB);
		}
		static void AssertPivotHasError(AccEInvoicingTransactionPivot pivot, string expectedError)
		{
			AssertEquals("Errors should be recorded when validation fails.", expectedError, pivot.AIP_ErrorDescription);
			AssertEquals("Pivot status should be Batched With Error when validation fails", EInvoicingPivotState.BatchedWithError, pivot.AIP_Status);
			AssertEquals("Pivot should not be of the batch when validation fails", ZGuid.Empty, pivot.AIP_AIB);
		}

		GlbCompany SaudiArabiaCompany;
		OrgHeader DebtorOrg;
		AccEInvoicingBatch Batch;
		AccEInvoicingTransactionPivot Pivot;
		ARInvoiceLine InvoiceLine;
		ZGuid NotificationGroupPK;
		DetailedLoggerForTest Logger;
		ARCreditNoteLine CreditNoteLine;

		(GlbCompany company, OrgHeader debtorOrg) CreateCompanyAndOrg()
		{
			var saudiArabiaCompany = TestObjectCreator.CreateCompanyAndBranch("SAUAR");
			var branchOrgProxy = TestObjectCreator.CreateOrgHeader("SA BRNPROXY", true, true, "SAUAR");
			saudiArabiaCompany.FirstActiveBranch.GB_OH_OrgProxy = branchOrgProxy.PK;
			AssertNotEquals("Precondition: different branch and company org proxies", saudiArabiaCompany.GC_OH_OrgProxy, saudiArabiaCompany.FirstActiveBranch.GB_OH_OrgProxy);

			var debtorOrg = TestObjectCreator.Debtor;
			debtorOrg.CustomsCodes.AddNew(OrgCusCode.CodeTypes.GSTCode, "87654321", CountryCodes.SaudiArabia);
			debtorOrg.MainAddress.City = "Test City";
			debtorOrg.MainAddress.State = "DL";
			debtorOrg.MainAddress.Postcode = "110011";
			debtorOrg.MainAddress.OA_RN_NKCountryCode = CountryCodes.SaudiArabia;

			return (saudiArabiaCompany, debtorOrg);
		}

		(ARInvoice, ARInvoiceLine, AccEInvoicingBatch, AccEInvoicingTransactionPivot) CreateTestInvoice(int number = 1, OrgHeader debtor = null, GlbCompany company = null)
		{
			var invoice = TestObjectCreator.CreateARInvoice<ARInvoice>("INV" + number.ToString("000"), TestObjectCreator.USD, 1m, debtor ?? DebtorOrg);
			invoice.IsManuallySetTransactionNumber_ForTestOnly = true;
			invoice.AH_GC = (company ?? SaudiArabiaCompany).PK;
			invoice.AH_GB = (company ?? SaudiArabiaCompany).FirstActiveBranch.PK;
			invoice.AH_ComplianceSubType = "ZZZ";
			invoice.AH_TransactionReference = "I12345";

			var invoiceLine = TestObjectCreator.CreateARInvoiceLine(invoice, null, TestObjectCreator.FRT, TestObjectCreator.USD, 1m, "desc", 100m + number);
			invoiceLine.AL_GC = (company ?? SaudiArabiaCompany).PK;
			invoiceLine.AL_GB = (company ?? SaudiArabiaCompany).FirstActiveBranch.PK;
			invoiceLine.AL_AT = TestObjectCreator.GST1.PK;

			var batch = TestObjectCreator.CreateEInvoicingBatch(100 + number, EInvoicingBatchState.Ready, company ?? SaudiArabiaCompany);
			var pivot = TestObjectCreator.CreateEInvoicingTransactionPivot(batch, invoice, EInvoicingPivotState.Batched);

			return (invoice, invoiceLine, batch, pivot);
		}

		(ARCreditNote, ARCreditNoteLine, AccEInvoicingBatch, AccEInvoicingTransactionPivot) CreateTestCreditNote(int number = 2, OrgHeader debtor = null, GlbCompany company = null)
		{
			var creditNote = TestObjectCreator.CreateARCreditNote("CRD" + number.ToString("000"), debtor ?? DebtorOrg, TestObjectCreator.USD, 1m, "desc");
			creditNote.IsManuallySetTransactionNumber_ForTestOnly = true;
			creditNote.AH_GC = (company ?? SaudiArabiaCompany).PK;
			creditNote.AH_GB = (company ?? SaudiArabiaCompany).FirstActiveBranch.PK;
			creditNote.AH_ComplianceSubType = "ZZZ";
			creditNote.AH_TransactionReference = "I12345";

			var creditNoteLine = TestObjectCreator.CreateARCreditNoteLine(creditNote, null, TestObjectCreator.FRT, 100m, TestObjectCreator.USD, 1m, "desc");
			creditNoteLine.AL_GC = (company ?? SaudiArabiaCompany).PK;
			creditNoteLine.AL_GB = (company ?? SaudiArabiaCompany).FirstActiveBranch.PK;
			creditNoteLine.AL_AT = TestObjectCreator.GST1.PK;

			var batch = TestObjectCreator.CreateEInvoicingBatch(100 + number, EInvoicingBatchState.Ready, company ?? SaudiArabiaCompany);
			var pivot = TestObjectCreator.CreateEInvoicingTransactionPivot(batch, creditNote, EInvoicingPivotState.Batched);

			return (creditNote, creditNoteLine, batch, pivot);
		}

		void InitaliseStandardTestObjects(bool isCreditNote = false)
		{
			NotificationGroupPK = new EInvoicingTestHelper(TestObjectCreator).CreateNotificationGroup("Test User", "company.user@abc.com");
			(SaudiArabiaCompany, DebtorOrg) = CreateCompanyAndOrg();
			if (isCreditNote)
			{
				(_, CreditNoteLine, Batch, Pivot) = CreateTestCreditNote();
			}
			else
			{
				(_, InvoiceLine, Batch, Pivot) = CreateTestInvoice();
			}
			Logger = new DetailedLoggerForTest();

			AssertEquals("Registry precondition: AllowSendingEInvoicingBatchWithError", false, AccountingConfigurationRegistry.Instance.AllowSendingEInvoicingBatchWithError.GetFallBackValueAtAllLevels(SaudiArabiaCompany.PK.ToGuid(), Guid.Empty, Guid.Empty));
		}

		IDisposable ConfigureErrorNotificationGroup(GlbCompany company = null)
		{
			return AccountingConfigurationRegistry.Instance.EInvoicingErrorNotificationGroup.SetTemporaryValue((company ?? SaudiArabiaCompany).PK.ToGuid(), Guid.Empty, Guid.Empty, NotificationGroupPK.ToGuid());
		}

		#endregion
	}
}
