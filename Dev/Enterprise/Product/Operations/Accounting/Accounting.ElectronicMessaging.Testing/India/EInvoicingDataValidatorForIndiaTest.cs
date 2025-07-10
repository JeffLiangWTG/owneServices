using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.ElectronicMessaging.Common.DataValidation;
using Enterprise.Accounting.ElectronicMessaging.Common.DataValidation.Testing;
using Enterprise.Accounting.ElectronicMessaging.Common.Testing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.ElectronicMessaging.India.Testing
{
	public class EInvoicingDataValidatorForIndiaTest : BaseEInvoicingDataValidatorTest
	{
		#region Integration Tests

		public void TestValidTransaction_HasNoErrors()
		{
			InitaliseStandardTestObjects();
			Factory.Save();

			using (ConfigureErrorNotificationGroup())
			{
				GetEInvoicingDataValidatorForTest().Run(Logger);
			}

			AssertPivotHasNoErrors(Pivot, Batch);
			AssertBatchHasNoError(Batch);
			AssertEquals("No error level log messages should be logged", false, Logger.Logs.Any(x => x.Item1 == LogType.Error));
		}

		public void TestTwoValidTransactions_HaveNoErrors()
		{
			InitaliseStandardTestObjects();
			var (_, _, batch2, pivot2) = CreateTestTransaction(number: 2);
			Factory.Save();

			using (ConfigureErrorNotificationGroup())
			{
				GetEInvoicingDataValidatorForTest().Run(Logger);
			}

			AssertPivotHasNoErrors(Pivot, Batch);
			AssertBatchHasNoError(Batch);
			AssertPivotHasNoErrors(pivot2, batch2);
			AssertBatchHasNoError(batch2);

			AssertEquals("No error level log messages should be logged", false, Logger.Logs.Any(x => x.Item1 == LogType.Error));
		}

		public void TestInvalidTransaction_HasErrorOnPivot()
		{
			InitaliseStandardTestObjects();
			DebtorOrg.MainAddress.State = ZString.Empty;
			Factory.Save();

			using (ConfigureErrorNotificationGroup())
			{
				GetEInvoicingDataValidatorForTest().Run(Logger);
			}

			AssertPivotHasError(Pivot, "Could not determine buyer's state code.");
			AssertBatchHasError(Batch);

			AssertEquals("One warning level log message should be logged for transaction with validation error", 1, Logger.Logs.Count(x => x.Item1 == LogType.Warning));
			Assert("Warning level log messages should contain the validation error", Logger.Logs.Any(x => x.Item1 == LogType.Warning && x.Item2.Contains(Pivot.AIP_ErrorDescription)));
		}

		public void TestTwoInvalidTransactions_HaveErrorOnPivots()
		{
			InitaliseStandardTestObjects();
			var (invoice2, _, batch2, pivot2) = CreateTestTransaction(number: 2);
			Invoice.AH_TransactionReference = ZString.Empty;
			invoice2.AH_ComplianceSubType = ZString.Empty;
			Factory.Save();

			var complianceNumbersApplyFrom = ZDate.Today.ToDateTime();
			using (AccountingConfigurationRegistry.Instance.IndiaUseComplianceNumbersInsteadOfTransactionNumbersFrom.SetTemporaryValue(IndiaCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, complianceNumbersApplyFrom))
			using (ConfigureErrorNotificationGroup())
			{
				GetEInvoicingDataValidatorForTest().Run(Logger);
			}

			AssertPivotHasError(Pivot, "Compliance number is missing.");
			AssertBatchHasError(Batch);
			AssertPivotHasError(pivot2, "Compliance sub-type is missing.");
			AssertBatchHasError(batch2);

			AssertEquals("One warning level log message should be logged for each transaction with validation error", 2, Logger.Logs.Count(x => x.Item1 == LogType.Warning));
			Assert("Warning level log messages should contain the validation error", Logger.Logs.Any(x => x.Item1 == LogType.Warning && x.Item2.Contains(Pivot.AIP_ErrorDescription)));
			Assert("Warning level log messages should contain the validation error", Logger.Logs.Any(x => x.Item1 == LogType.Warning && x.Item2.Contains(pivot2.AIP_ErrorDescription)));
		}

		public void TestMixedTransactions_HaveErrorsOnInvalidOnly()
		{
			InitaliseStandardTestObjects();
			var (invoice2, _, batch2, pivot2) = CreateTestTransaction(number: 2);
			invoice2.AH_ComplianceSubType = ZString.Empty;
			Factory.Save();

			using (ConfigureErrorNotificationGroup())
			{
				GetEInvoicingDataValidatorForTest().Run(Logger);
			}

			AssertPivotHasNoErrors(Pivot, Batch);
			AssertBatchHasNoError(Batch);
			AssertPivotHasError(pivot2, "Compliance sub-type is missing.");
			AssertBatchHasError(batch2);

			AssertEquals("One warning level log messages should be logged for transaction with validation error", 1, Logger.Logs.Count(x => x.Item1 == LogType.Warning));
			Assert("Warning level log messages should contain the validation error", Logger.Logs.Any(x => x.Item1 == LogType.Warning && x.Item2.Contains(pivot2.AIP_ErrorDescription)));
		}

		public void TestManyErrorsOnOneTransaction()
		{
			InitaliseStandardTestObjects();
			SellerOrg.MainAddress.Postcode = ZString.Empty;
			DebtorOrg.MainAddress.State = ZString.Empty;
			DebtorOrg.MainAddress.City = ZString.Empty;
			Factory.Save();

			using (ConfigureErrorNotificationGroup())
			{
				GetEInvoicingDataValidatorForTest().Run(Logger);
			}

			var expectedErrors = new[]
			{
				"Could not determine seller's post code.",
				"Could not determine buyer's state code.",
				"Could not determine buyer's city.",
			};
			AssertPivotHasError(Pivot, string.Join(" ", expectedErrors));
			AssertBatchHasError(Batch);

			AssertEquals("One warning level log message should be logged for transaction with validation error", 1, Logger.Logs.Count(x => x.Item1 == LogType.Warning));
			foreach (var error in expectedErrors)
			{
				Assert("Warning level log messages should contain the validation error", Logger.Logs.Any(x => x.Item1 == LogType.Warning && x.Item2.Contains(error)));
			}
		}

		public void TestValidationRunsForOneCompanyOnly()
		{
			InitaliseStandardTestObjects();
			var otherCompany = TestObjectCreator.CreateCompanyAndBranch("AUSYD");
			var sellerOrg = otherCompany.OrgProxy;
			otherCompany.OrgProxy.CustomsCodes.AddNew(OrgCusCode.CodeTypes.GSTCode, "111222333", CountryCodes.India);
			otherCompany.OrgProxy.MainAddress.City = "Sydney";
			otherCompany.OrgProxy.MainAddress.State = "NSW";
			otherCompany.OrgProxy.MainAddress.Postcode = "2001";

			var (invoice2, _, batch2, pivot2) = CreateTestTransaction(number: 2, company: otherCompany);
			Invoice.AH_TransactionReference = ZString.Empty;
			invoice2.AH_ComplianceSubType = ZString.Empty;
			Factory.Save();

			var complianceNumbersApplyFrom = ZDate.Today.ToDateTime();
			using (AccountingConfigurationRegistry.Instance.IndiaUseComplianceNumbersInsteadOfTransactionNumbersFrom.SetTemporaryValue(IndiaCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, complianceNumbersApplyFrom))
			using (ConfigureErrorNotificationGroup(company: IndiaCompany))
			{
				GetEInvoicingDataValidatorForTest(IndiaCompany).Run(Logger);
			}

			// Validation runs for default company.
			AssertPivotHasError(Pivot, "Compliance number is missing.");
			AssertBatchHasError(Batch);
			// Validation does not run for second company.
			AssertPivotHasNoErrors(pivot2, batch2);
			AssertBatchHasNoError(batch2);

			AssertEquals("One warning level log message should be logged for transaction with validation error", 1, Logger.Logs.Count(x => x.Item1 == LogType.Warning));
			Assert("Warning level log messages should contain the validation error", Logger.Logs.Any(x => x.Item1 == LogType.Warning && x.Item2.Contains(Pivot.AIP_ErrorDescription)));

			using (AccountingConfigurationRegistry.Instance.IndiaUseComplianceNumbersInsteadOfTransactionNumbersFrom.SetTemporaryValue(IndiaCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, complianceNumbersApplyFrom))
			using (ConfigureErrorNotificationGroup(company: otherCompany))
			{
				GetEInvoicingDataValidatorForTest(otherCompany).Run(Logger);
			}

			AssertPivotHasError(pivot2, "Compliance sub-type is missing.");
			AssertBatchHasError(batch2);
		}

		public void TestValidationError_WhenAllowSendingBatchWithErrors_IsOn()
		{
			InitaliseStandardTestObjects();
			DebtorOrg.MainAddress.State = ZString.Empty;
			Factory.Save();

			using (AccountingConfigurationRegistry.Instance.AllowSendingEInvoicingBatchWithError.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (ConfigureErrorNotificationGroup())
			{
				GetEInvoicingDataValidatorForTest().Run(Logger);
			}

			AssertPivotHasErrorButStillInABatch(Pivot, Batch, "Could not determine buyer's state code.");
			AssertBatchHasNoError(Batch);

			AssertEquals("One warning level log message should be logged for transaction with validation error", 1, Logger.Logs.Count(x => x.Item1 == LogType.Warning));
			Assert("Warning level log messages should contain the validation error", Logger.Logs.Any(x => x.Item1 == LogType.Warning && x.Item2.Contains(Pivot.AIP_ErrorDescription)));
		}

		public void TestErrorSendsEmailNotification()
		{
			InitaliseStandardTestObjects();
			DebtorOrg.MainAddress.State = ZString.Empty;
			Factory.Save();

			using (ConfigureErrorNotificationGroup())
			{
				AssertEquals("Precondition", 0, Env.OutgoingMailManager.EmailsCreated.Count);
				GetEInvoicingDataValidatorForTest().Run(Logger);
				AssertEquals("India should send email notifications on any errors", 1, Env.OutgoingMailManager.EmailsCreated.Count);
			}

			AssertPivotHasError(Pivot, "Could not determine buyer's state code.");
			AssertEquals("One warning level log messages should be logged for transaction with validation error", 1, Logger.Logs.Count(x => x.Item1 == LogType.Warning));
		}

		#endregion

		#region Validation Rule Tests

		public void TestValidationRule_MissingTransactionNumber()
		{
			InitaliseStandardTestObjects();
			Invoice.AH_TransactionNum = ZString.Empty;

			var validationMessages = GetEInvoicingDataValidatorForTest().ValidateTransaction(Invoice, null);

			AssertContainsExactElementsInAnyOrder(new[] { "Transaction number is missing." }, validationMessages);
		}

		public void TestValidationRule_MissingTransactionDate()
		{
			InitaliseStandardTestObjects();
			Invoice.AH_InvoiceDate = ZDate.Empty;

			var validationMessages = GetEInvoicingDataValidatorForTest().ValidateTransaction(Invoice, null);

			AssertContainsExactElementsInAnyOrder(new[] { "Transaction date is missing." }, validationMessages);
		}

		public void TestValidationRule_MissingSellerGSTNumber()
		{
			InitaliseStandardTestObjects();
			SellerOrg.CustomsCodes.RemoveAll();

			var validationMessages = GetEInvoicingDataValidatorForTest().ValidateTransaction(Invoice, null);

			AssertContainsExactElementsInAnyOrder(new[] { "Seller does not have GSTIN." }, validationMessages);
		}

		public void TestValidationRule_MissingSellerState()
		{
			InitaliseStandardTestObjects();
			SellerOrg.MainAddress.OA_State = ZString.Empty;

			var validationMessages = GetEInvoicingDataValidatorForTest().ValidateTransaction(Invoice, null);

			AssertContainsExactElementsInAnyOrder(new[] { "Could not determine seller's state code." }, validationMessages);
		}

		public void TestValidationRule_MissingSellerCity()
		{
			InitaliseStandardTestObjects();
			SellerOrg.MainAddress.OA_City = ZString.Empty;

			var validationMessages = GetEInvoicingDataValidatorForTest().ValidateTransaction(Invoice, null);

			AssertContainsExactElementsInAnyOrder(new[] { "Could not determine seller's city." }, validationMessages);
		}

		public void TestValidationRule_MissingSellerPostCode()
		{
			InitaliseStandardTestObjects();
			SellerOrg.MainAddress.OA_PostCode = ZString.Empty;

			var validationMessages = GetEInvoicingDataValidatorForTest().ValidateTransaction(Invoice, null);

			AssertContainsExactElementsInAnyOrder(new[] { "Could not determine seller's post code." }, validationMessages);
		}

		public void TestValidationRule_MissingDebtorState()
		{
			InitaliseStandardTestObjects();
			DebtorOrg.MainAddress.OA_RN_NKCountryCode = CountryCodes.India;
			DebtorOrg.MainAddress.State = ZString.Empty;

			var validationMessages = GetEInvoicingDataValidatorForTest().ValidateTransaction(Invoice, null);

			AssertContainsExactElementsInAnyOrder("Buyer state is required when in India", new[] { "Could not determine buyer's state code." }, validationMessages);
		}

		public void TestValidationRule_MissingDebtorState_DoesNotApplyOutsideIndia()
		{
			InitaliseStandardTestObjects();
			DebtorOrg.MainAddress.OA_RN_NKCountryCode = CountryCodes.Iceland;
			DebtorOrg.MainAddress.State = ZString.Empty;

			var validationMessages = GetEInvoicingDataValidatorForTest().ValidateTransaction(Invoice, null);

			AssertContainsExactElementsInAnyOrder("Buyer state is not required when outside of India", Array.Empty<string>(), validationMessages);
		}

		public void TestValidationRule_MissingDebtorCity()
		{
			InitaliseStandardTestObjects();
			DebtorOrg.MainAddress.City = ZString.Empty;

			var validationMessages = GetEInvoicingDataValidatorForTest().ValidateTransaction(Invoice, null);

			AssertContainsExactElementsInAnyOrder(new[] { "Could not determine buyer's city." }, validationMessages);
		}

		public void TestValidationRule_MissingDebtorPostCode()
		{
			InitaliseStandardTestObjects();
			DebtorOrg.MainAddress.OA_RN_NKCountryCode = CountryCodes.India;
			DebtorOrg.MainAddress.Postcode = ZString.Empty;

			var validationMessages = GetEInvoicingDataValidatorForTest().ValidateTransaction(Invoice, null);

			AssertContainsExactElementsInAnyOrder("Buyer postcode is required when in India", new[] { "Could not determine buyer's post code." }, validationMessages);
		}

		public void TestValidationRule_MissingDebtorPostCode_DoesNotApplyOutsideIndia()
		{
			InitaliseStandardTestObjects();
			DebtorOrg.MainAddress.OA_RN_NKCountryCode = CountryCodes.Iceland;
			DebtorOrg.MainAddress.Postcode = ZString.Empty;

			var validationMessages = GetEInvoicingDataValidatorForTest().ValidateTransaction(Invoice, null);

			AssertContainsExactElementsInAnyOrder("Buyer postcode is not required when outside India", Array.Empty<string>(), validationMessages);
		}

		public void TestValidationRule_MissingComplianceNumber()
		{
			InitaliseStandardTestObjects();
			Invoice.AH_TransactionReference = ZString.Empty;

			var complianceNumbersApplyFrom = ZDate.Today.ToDateTime();
			using (AccountingConfigurationRegistry.Instance.IndiaUseComplianceNumbersInsteadOfTransactionNumbersFrom.SetTemporaryValue(IndiaCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, complianceNumbersApplyFrom))
			{
				var validationMessages = GetEInvoicingDataValidatorForTest().ValidateTransaction(Invoice, null);

				AssertContainsExactElementsInAnyOrder(new[] { "Compliance number is missing." }, validationMessages);
			}

			complianceNumbersApplyFrom = ZDate.Today.AddDays(1).ToDateTime();
			using (AccountingConfigurationRegistry.Instance.IndiaUseComplianceNumbersInsteadOfTransactionNumbersFrom.SetTemporaryValue(IndiaCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, complianceNumbersApplyFrom))
			{
				var validationMessages = GetEInvoicingDataValidatorForTest().ValidateTransaction(Invoice, null);

				AssertSequencesEqual("Compliance number validation should not be triggered when compliance number is not mapped", Array.Empty<ZString>(), validationMessages);
			}
		}

		public void TestValidationRule_MissingComplianceSubType()
		{
			InitaliseStandardTestObjects();
			Invoice.AH_ComplianceSubType = ZString.Empty;

			var validationMessages = GetEInvoicingDataValidatorForTest().ValidateTransaction(Invoice, null);

			AssertContainsExactElementsInAnyOrder(new[] { "Compliance sub-type is missing." }, validationMessages);
		}

		public void TestValidationRule_ShortTransactionLineDescription()
		{
			InitaliseStandardTestObjects();
			InvoiceLine.AL_Desc = "12";

			var validationMessages = GetEInvoicingDataValidatorForTest().ValidateTransaction(Invoice, null);

			AssertContainsExactElementsInAnyOrder(new[] { "Transaction line 1 with Charge Code 'FRT' has less then 3 characters." }, validationMessages);
		}

		public void TestValidationRule_ShortTransactionLineDescriptionWithMultipleLines()
		{
			InitaliseStandardTestObjects();
			InvoiceLine.AL_Desc = "12";
			TestObjectCreator.CreateARInvoiceLine(Invoice, null, TestObjectCreator.CC1, TestObjectCreator.USD, 1m, "1", 100m);
			var glAccountLine = (InvoicingLineBase)Invoice.Lines.AddNew();
			glAccountLine.AL_LineType = TransactionLineTypes.Revenue;
			glAccountLine.AL_Desc = "1";
			glAccountLine.AL_AG = TestObjectCreator.CreateGLHeader("1234.5678").PK;

			var validationMessages = GetEInvoicingDataValidatorForTest().ValidateTransaction(Invoice, null);
			var expectedMessages = new[]
			{
				"Transaction line 1 with Charge Code 'FRT' has less then 3 characters.",
				"Transaction line 2 with Charge Code 'ZZCC1' has less then 3 characters.",
				"Transaction line 3 with GL Post To Account '1234.5678' has less then 3 characters.",
			};
			AssertContainsExactElementsInAnyOrder("Multiple lines should have multiple messages", expectedMessages, validationMessages);
		}

		const string InvalidCharactersForTransactionNumber = "0/-,.\"':;";
		public void TestValidationRule_InvalidTransactionNumberStart()
		{
			InitaliseStandardTestObjects();

			foreach (var c in InvalidCharactersForTransactionNumber)
			{
				Invoice.AH_TransactionNum = c + "123";
				var validationMessages = GetEInvoicingDataValidatorForTest().ValidateTransaction(Invoice, null);
				AssertContainsExactElementsInAnyOrder(new[] { $"Generated Transaction Number '{c}123' cannot start with '{c}'. Please check your Transaction Number sequence configuration." }, validationMessages);
			}
			Invoice.AH_TransactionNum = "INV123";

			foreach (var c in InvalidCharactersForTransactionNumber)
			{
				Invoice.AH_TransactionReference = c + "123";
				var validationMessages = GetEInvoicingDataValidatorForTest().ValidateTransaction(Invoice, null);
				AssertEquals("As registry indicates transaction numbers are in use, this rule should not apply to compliance numbers.", 0, validationMessages.Count);
			}
		}

		public void TestValidationRule_InvalidComplianceNumberStart()
		{
			InitaliseStandardTestObjects();
			var complianceNumbersApplyFrom = ZDate.Today.ToDateTime();
			using (AccountingConfigurationRegistry.Instance.IndiaUseComplianceNumbersInsteadOfTransactionNumbersFrom.SetTemporaryValue(IndiaCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, complianceNumbersApplyFrom))
			{
				foreach (var c in InvalidCharactersForTransactionNumber)
				{
					Invoice.AH_TransactionReference = c + "123";
					var validationMessages = GetEInvoicingDataValidatorForTest().ValidateTransaction(Invoice, null);
					AssertContainsExactElementsInAnyOrder(new[] { $"Generated Compliance Number '{c}123' cannot start with '{c}'. Please check your Compliance Number sequence configuration." }, validationMessages);
				}
				Invoice.AH_TransactionReference = "INV123";

				foreach (var c in InvalidCharactersForTransactionNumber)
				{
					Invoice.AH_TransactionNum = c + "123";
					var validationMessages = GetEInvoicingDataValidatorForTest().ValidateTransaction(Invoice, null);
					AssertEquals("As registry indicates compliance numbers are in use, this rule should not apply to transaction numbers.", 0, validationMessages.Count);
				}
			}
		}

		#endregion

		#region Implementation

		public override BaseEInvoicingDataValidator GetEInvoicingDataValidatorForTest()
			=> GetEInvoicingDataValidatorForTest(IndiaCompany);

		public BaseEInvoicingDataValidator GetEInvoicingDataValidatorForTest(GlbCompany company)
			=> new EInvoicingDataValidatorForIndia(company);

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
		static void AssertPivotHasErrorButStillInABatch(AccEInvoicingTransactionPivot pivot, AccEInvoicingBatch batch, string expectedError)
		{
			AssertEquals("Errors should be recorded when validation fails.", expectedError, pivot.AIP_ErrorDescription);
			AssertEquals("Pivot status should be Batched With Error when validation fails", EInvoicingPivotState.BatchedWithError, pivot.AIP_Status);
			AssertEquals("Pivot should remain part of the batch even when validation fails", batch.PK, pivot.AIP_AIB);
		}

		static void AssertBatchHasError(AccEInvoicingBatch batch)
		{
			AssertEquals("Batch is discarded when all related transaction have error", EInvoicingBatchState.Discarded, batch.AIB_Status);
		}
		static void AssertBatchHasNoError(AccEInvoicingBatch batch)
		{
			AssertEquals("Batch remains ready when all related transactions are successful", EInvoicingBatchState.Ready, batch.AIB_Status);
		}

		GlbCompany IndiaCompany;
		OrgHeader SellerOrg;
		OrgHeader DebtorOrg;
		AccEInvoicingBatch Batch;
		AccEInvoicingTransactionPivot Pivot;
		ARInvoice Invoice;
		ARInvoiceLine InvoiceLine;
		ZGuid NotificationGroupPK;
		DetailedLoggerForTest Logger;

		(GlbCompany company, OrgHeader sellerOrg, OrgHeader debtorOrg) CreateCompanyAndOrgs()
		{
			var indiaCompany = TestObjectCreator.CreateCompanyAndBranch("INDEL");
			var branchOrgProxy = TestObjectCreator.CreateOrgHeader("IN BRNPROXY", true, true, "INDEL");
			indiaCompany.FirstActiveBranch.GB_OH_OrgProxy = branchOrgProxy.PK;
			AssertNotEquals("Precondition: different branch and company org proxies", indiaCompany.GC_OH_OrgProxy, indiaCompany.FirstActiveBranch.GB_OH_OrgProxy);

			var sellerOrg = branchOrgProxy;
			sellerOrg.CustomsCodes.AddNew(OrgCusCode.CodeTypes.GSTCode, "12345678", CountryCodes.India);
			sellerOrg.MainAddress.City = "New Delhi";
			sellerOrg.MainAddress.State = "DL";
			sellerOrg.MainAddress.Postcode = "110011";
			sellerOrg.MainAddress.OA_RN_NKCountryCode = CountryCodes.India;

			var debtorOrg = TestObjectCreator.Debtor;
			debtorOrg.CustomsCodes.AddNew(OrgCusCode.CodeTypes.GSTCode, "87654321", CountryCodes.India);
			debtorOrg.MainAddress.City = "New Delhi";
			debtorOrg.MainAddress.State = "DL";
			debtorOrg.MainAddress.Postcode = "110011";
			debtorOrg.MainAddress.OA_RN_NKCountryCode = CountryCodes.India;

			return (indiaCompany, sellerOrg, debtorOrg);
		}

		(ARInvoice, ARInvoiceLine, AccEInvoicingBatch, AccEInvoicingTransactionPivot) CreateTestTransaction(int number = 1, OrgHeader debtor = null, GlbCompany company = null)
		{
			var invoice = TestObjectCreator.CreateARInvoice<ARInvoice>("INV" + number.ToString("000"), TestObjectCreator.USD, 1m, debtor ?? DebtorOrg);
			invoice.IsManuallySetTransactionNumber_ForTestOnly = true;
			invoice.AH_GC = (company ?? IndiaCompany).PK;
			invoice.AH_GB = (company ?? IndiaCompany).FirstActiveBranch.PK;
			invoice.AH_ComplianceSubType = "ZZZ";
			invoice.AH_TransactionReference = "I12345";

			var invoiceLine = TestObjectCreator.CreateARInvoiceLine(invoice, null, TestObjectCreator.FRT, TestObjectCreator.USD, 1m, "desc", 100m + number);
			invoiceLine.AL_GC = (company ?? IndiaCompany).PK;
			invoiceLine.AL_GB = (company ?? IndiaCompany).FirstActiveBranch.PK;
			invoiceLine.AL_AT = TestObjectCreator.GST1.PK;

			var batch = TestObjectCreator.CreateEInvoicingBatch(100 + number, EInvoicingBatchState.Ready, company ?? IndiaCompany);
			var pivot = TestObjectCreator.CreateEInvoicingTransactionPivot(batch, invoice, EInvoicingPivotState.Batched);

			return (invoice, invoiceLine, batch, pivot);
		}

		void InitaliseStandardTestObjects()
		{
			NotificationGroupPK = new EInvoicingTestHelper(TestObjectCreator).CreateNotificationGroup("Test User", "company.user@abc.com");
			(IndiaCompany, SellerOrg, DebtorOrg) = CreateCompanyAndOrgs();
			(Invoice, InvoiceLine, Batch, Pivot) = CreateTestTransaction();
			Logger = new DetailedLoggerForTest();

			AssertEquals("Registry precondition: AllowSendingEInvoicingBatchWithError", false, AccountingConfigurationRegistry.Instance.AllowSendingEInvoicingBatchWithError.GetFallBackValueAtAllLevels(IndiaCompany.PK.ToGuid(), Guid.Empty, Guid.Empty));
			AssertEquals("Registry precondition: IndiaUseComplianceNumbersInsteadOfTransactionNumbersFrom", DateTime.MinValue, AccountingConfigurationRegistry.Instance.IndiaUseComplianceNumbersInsteadOfTransactionNumbersFrom.GetFallBackValueAtAllLevels(IndiaCompany.PK.ToGuid(), Guid.Empty, Guid.Empty));
		}

		IDisposable ConfigureErrorNotificationGroup(GlbCompany company = null)
		{
			return AccountingConfigurationRegistry.Instance.EInvoicingErrorNotificationGroup.SetTemporaryValue((company ?? IndiaCompany).PK.ToGuid(), Guid.Empty, Guid.Empty, NotificationGroupPK.ToGuid());
		}

		#endregion
	}
}
