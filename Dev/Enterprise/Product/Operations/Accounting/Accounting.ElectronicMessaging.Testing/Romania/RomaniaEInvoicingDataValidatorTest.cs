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
using Enterprise.MasterFiles.Business.Accounting.Helpers;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.ElectronicMessaging.Romania.Testing
{
	public class RomaniaEInvoicingDataValidatorTest : BaseEInvoicingDataValidatorTest
	{
		public void TestPendingCredential_IsApplying_HasError()
		{
			InitaliseStandardTestObjects();
			SetupCredential(5, PasswordStatusList.Codes.Pending);

			ValidateAndAssertWarning();
		}

		public void TestPendingCredential_IsRefreshing_HasNoError()
		{
			InitaliseStandardTestObjects();
			var credential = SetupCredential(5, PasswordStatusList.Codes.Pending);
			credential.GP_StatusReason = OAuthHelper.CredentialStatusReasonRefreshingToken;

			Factory.Save();

			ValidateAndAssertNoError();
		}

		public void TestValidCredential_HasNoError()
		{
			InitaliseStandardTestObjects();
			SetupCredential(5);

			ValidateAndAssertNoError();
		}

		public void TestExpiredCredential_HasNoError()
		{
			InitaliseStandardTestObjects();
			SetupCredential(-5);

			ValidateAndAssertNoError();
		}

		public void TestNoCredential_HasError()
		{
			InitaliseStandardTestObjects();
			Factory.Save();

			ValidateAndAssertWarning();
		}

		public void TestNoCredential_ButOtherROCOmpanyHasPendingRefreshToken_HasError()
		{
			InitaliseStandardTestObjects();
			var otherRomaniaCompany = TestObjectCreator.CreateCompanyAndBranch("RCOO");
			otherRomaniaCompany.GC_RN_NKCountryCode = CountryCodes.Romania;
			var wrapper = GlbCompanyWrapper.GetWrapper<GlbCompanyWrapper>(otherRomaniaCompany);
			var credential = wrapper.GetGlbExternalPasswordOrCreateNew<GlbCompanyEInvoicingCertificateCredential>(PasswordTypesList.Codes.EIM);
			credential.GP_PasswordStatus = PasswordStatusList.Codes.Pending;
			credential.GP_StatusReason = OAuthHelper.CredentialStatusReasonRefreshingToken;
			credential.GP_ExpiryDate = DateTime.UtcNow.AddDays(5);

			Factory.Save();

			ValidateAndAssertWarning();
		}

		public override BaseEInvoicingDataValidator GetEInvoicingDataValidatorForTest()	=> new RomaniaEInvoicingDataValidator(RomaniaCompany);

		#region Implementation

		void ValidateAndAssertWarning()
		{
			using (ConfigureErrorNotificationGroup())
			{
				GetEInvoicingDataValidatorForTest().Run(Logger);
			}

			AssertPivotHasError(Pivot, "Enable E-Reporting Functionality is set to Yes and Company " + RomaniaCompany.GC_Code + " has no valid E-Invoicing Credentials.");

			AssertEquals("One warning level log message should be logged for transaction with validation error", 1, Logger.Logs.Count(x => x.Item1 == LogType.Warning));
			Assert("Warning level log messages should contain the validation error", Logger.Logs.Any(x => x.Item1 == LogType.Warning && x.Item2.Contains(Pivot.AIP_ErrorDescription)));
		}

		void ValidateAndAssertNoError()
		{
			using (ConfigureErrorNotificationGroup())
			{
				GetEInvoicingDataValidatorForTest().Run(Logger);
			}

			AssertPivotHasNoErrors(Pivot, Batch);
			AssertEquals("No error level log messages should be logged", false, Logger.Logs.Any(x => x.Item1 == LogType.Error));
		}

		void AssertPivotHasNoErrors(AccEInvoicingTransactionPivot pivot, AccEInvoicingBatch batch)
		{
			AssertNullOrEmpty("No errors should be recorded when validation is successful.", pivot.AIP_ErrorDescription);
			AssertEquals("Pivot status should be unchanged as Batched when validation is successful", EInvoicingPivotState.Batched, pivot.AIP_Status);
			AssertEquals("Pivot should be part of the batch when validation is successful", batch.PK, pivot.AIP_AIB);
		}

		void AssertPivotHasError(AccEInvoicingTransactionPivot pivot, string expectedError)
		{
			AssertEquals("Errors should be recorded when validation fails.", expectedError, pivot.AIP_ErrorDescription);
			AssertEquals("Pivot status should be Batched With Error when validation fails", EInvoicingPivotState.BatchedWithError, pivot.AIP_Status);
			AssertEquals("Pivot should not be of the batch when validation fails", ZGuid.Empty, pivot.AIP_AIB);
		}

		(GlbCompany company, OrgHeader debtorOrg) CreateCompanyAndOrg()
		{
			var romaniaCompany = TestObjectCreator.CreateCompanyAndBranch("ROCO");
			var branchOrgProxy = TestObjectCreator.CreateOrgHeader("RO BRNPROXY", true, true, "ROCO");
			romaniaCompany.FirstActiveBranch.GB_OH_OrgProxy = branchOrgProxy.PK;

			var debtorOrg = TestObjectCreator.Debtor;
			debtorOrg.CustomsCodes.AddNew(OrgCusCode.CodeTypes.GSTCode, "87654321", CountryCodes.Romania);
			debtorOrg.MainAddress.City = "Test City";
			debtorOrg.MainAddress.State = "DL";
			debtorOrg.MainAddress.Postcode = "110011";
			debtorOrg.MainAddress.OA_RN_NKCountryCode = CountryCodes.Romania;

			return (romaniaCompany, debtorOrg);
		}

		(AccEInvoicingBatch, AccEInvoicingTransactionPivot) CreateTestInvoice(int number = 1, OrgHeader debtor = null, GlbCompany company = null)
		{
			var invoice = TestObjectCreator.CreateARInvoice<ARInvoice>("INV" + number.ToString("000"), TestObjectCreator.USD, 1m, debtor ?? DebtorOrg);
			invoice.IsManuallySetTransactionNumber_ForTestOnly = true;
			invoice.AH_GC = (company ?? RomaniaCompany).PK;
			invoice.AH_GB = (company ?? RomaniaCompany).FirstActiveBranch.PK;
			invoice.AH_ComplianceSubType = "ZZZ";
			invoice.AH_TransactionReference = "I12345";

			var batch = TestObjectCreator.CreateEInvoicingBatch(100 + number, EInvoicingBatchState.Ready, company ?? RomaniaCompany);
			var pivot = TestObjectCreator.CreateEInvoicingTransactionPivot(batch, invoice, EInvoicingPivotState.Batched);

			return (batch, pivot);
		}

		void InitaliseStandardTestObjects()
		{
			NotificationGroupPK = new EInvoicingTestHelper(TestObjectCreator).CreateNotificationGroup("Test User", "company.user@abc.com");
			(RomaniaCompany, DebtorOrg) = CreateCompanyAndOrg();
			(Batch, Pivot) = CreateTestInvoice();
			Logger = new DetailedLoggerForTest();

			AssertEquals("Registry precondition: AllowSendingEInvoicingBatchWithError", false, AccountingConfigurationRegistry.Instance.AllowSendingEInvoicingBatchWithError.GetFallBackValueAtAllLevels(RomaniaCompany.PK.ToGuid(), Guid.Empty, Guid.Empty));
		}

		GlbCompanyEInvoicingCertificateCredential SetupCredential(int expiryDay, string status = PasswordStatusList.Codes.Valid)
		{
			var wrapper = GlbCompanyWrapper.GetWrapper<GlbCompanyWrapper>(RomaniaCompany);
			var credential = wrapper.GetGlbExternalPasswordOrCreateNew<GlbCompanyEInvoicingCertificateCredential>(PasswordTypesList.Codes.EIM);
			credential.GP_PasswordStatus = status;
			credential.GP_ExpiryDate = DateTime.UtcNow.AddDays(expiryDay);

			Factory.Save();

			return credential;
		}

		IDisposable ConfigureErrorNotificationGroup(GlbCompany company = null)
		{
			return AccountingConfigurationRegistry.Instance.EInvoicingErrorNotificationGroup.SetTemporaryValue((company ?? RomaniaCompany).PK.ToGuid(), Guid.Empty, Guid.Empty, NotificationGroupPK.ToGuid());
		}

		GlbCompany RomaniaCompany;
		OrgHeader DebtorOrg;
		AccEInvoicingBatch Batch;
		AccEInvoicingTransactionPivot Pivot;
		ZGuid NotificationGroupPK;
		DetailedLoggerForTest Logger;

		#endregion
	}
}
