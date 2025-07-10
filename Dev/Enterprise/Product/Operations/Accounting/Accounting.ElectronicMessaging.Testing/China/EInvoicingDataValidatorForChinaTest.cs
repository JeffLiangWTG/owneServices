using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.ElectronicMessaging.Common.DataValidation;
using Enterprise.Accounting.ElectronicMessaging.Common.DataValidation.Testing;
using Enterprise.Accounting.ElectronicMessaging.Common.Testing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CountryCompliance;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.ElectronicMessaging.China.Testing
{
	public class EInvoicingDataValidatorForChinaTest : BaseEInvoicingDataValidatorTest
	{
		public void TestErrorSendsEmailNotification()
		{
			TestObjectCreator.SetCurrentCompanyCountryCode(CountryCodes.China);
			SetUpForEligible();
			TestObjectCreator.Debtor.OH_RL_NKClosestPort = "CNBJS";
			TestObjectCreator.Debtor.CustomsCodes.RemoveAndDeleteAll();

			var notificationGroupPK = new EInvoicingTestHelper(TestObjectCreator).CreateNotificationGroup("Test User", "company.user@abc.com");
			var logger = new DetailedLoggerForTest();
			var pivot = CreateTransactionAndEInvoicingBatch().Pivot;

			Factory.Save();

			using (AccountingConfigurationRegistry.Instance.EInvoicingErrorNotificationGroup.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, notificationGroupPK.ToGuid()))
			{
				AssertEquals("Precondition", 0, Env.OutgoingMailManager.EmailsCreated.Count);
				GetEInvoicingDataValidatorForTest().Run(logger);
				AssertEquals("China should send email notifications on any errors", 1,
					Env.OutgoingMailManager.EmailsCreated.Count);
			}

			AssertEquals("Errors should be recorded when validation fails.",
				"Invoice Debtor must have a valid China VAT Registration Number.", pivot.AIP_ErrorDescription);
			AssertEquals("Pivot status should be Batched With Error when validation fails",
				EInvoicingPivotState.BatchedWithError, pivot.AIP_Status);
			AssertEquals("Pivot should not be of the batch when validation fails", ZGuid.Empty, pivot.AIP_AIB);
			AssertEquals("One Warning level log messages should be logged for transaction with validation error", 1, logger.Logs.Count(x => x.Item1 == LogType.Warning));
		}

		public void TestCheckInvoiceBranchProxyTaxCode()
		{
			var missTaxCodeMessage = "Invoice Branch Proxy's Organization must have a valid China VAT Registration Number.";
			var invalidTaxCodeMessage = "The China VAT Registration Number of the Invoice Branch Proxy's Organization must be 9 or 15-20 characters in length.";

			AssertCheckTaxCode(TestBranchOrgProxy, missTaxCodeMessage, invalidTaxCodeMessage, false);
		}

		public void TestCheckDebtorTaxCode()
		{
			var missTaxCodeMessage = "Invoice Debtor must have a valid China VAT Registration Number.";
			var invalidTaxCodeMessage = "The China VAT Registration Number of the Invoice Debtor must be 9 or 15-20 characters in length.";

			AssertCheckTaxCode(TestObjectCreator.Debtor, missTaxCodeMessage, invalidTaxCodeMessage, true);
		}

		void AssertCheckTaxCode(OrgHeader header, string missTaxCodeMessage, string invalidTaxCodeMessage, bool isDebtor)
		{
			IReadOnlyCollection<ZString> validationMessages;
			SetUpForEligible();

			TestObjectCreator.Debtor.OH_RL_NKClosestPort = "CNBJS";
			header.CustomsCodes.RemoveAll();
			var validator = GetEInvoicingDataValidatorForTest();
			var (invoice, line, batch, pivot) = CreateTransactionAndEInvoicingBatch();

			header.OH_Category = OrgConstants.Category.Government;
			validationMessages = validator.ValidateTransaction(invoice, pivot);

			AssertEquals(0, validationMessages.Count);

			header.OH_Category = OrgConstants.Category.Business;
			validationMessages = validator.ValidateTransaction(invoice, pivot);

			Assert(validationMessages.Contains(missTaxCodeMessage));

			header.CustomsCodes.AddNew(OrgCusCode.CodeTypes.VATCode, "1", CountryCodes.China);
			validationMessages = validator.ValidateTransaction(invoice, pivot);

			Assert(validationMessages.Contains(invalidTaxCodeMessage));

			header.CustomsCodes.RemoveAll();
			header.CustomsCodes.AddNew(OrgCusCode.CodeTypes.VATCode, "1234567890ABCDE", CountryCodes.China);
			validationMessages = validator.ValidateTransaction(invoice, pivot);

			AssertEquals(0, validationMessages.Count);

			header.CustomsCodes.RemoveAll();
			header.CustomsCodes.AddNew(OrgCusCode.CodeTypes.VATCode, "123456789", CountryCodes.China);
			validationMessages = validator.ValidateTransaction(invoice, pivot);

			AssertEquals(0, validationMessages.Count);

			header.CustomsCodes.RemoveAll();
			header.CustomsCodes.AddNew(OrgCusCode.CodeTypes.VATCode, "123456789012345", CountryCodes.China);
			validationMessages = validator.ValidateTransaction(invoice, pivot);

			AssertEquals(0, validationMessages.Count);

			header.CustomsCodes.RemoveAll();
			header.CustomsCodes.AddNew(OrgCusCode.CodeTypes.VATCode, "1234567890123456", CountryCodes.China);
			validationMessages = validator.ValidateTransaction(invoice, pivot);

			AssertEquals(0, validationMessages.Count);

			header.CustomsCodes.RemoveAll();
			header.CustomsCodes.AddNew(OrgCusCode.CodeTypes.VATCode, "12345678901234567", CountryCodes.China);
			validationMessages = validator.ValidateTransaction(invoice, pivot);

			AssertEquals(0, validationMessages.Count);

			header.CustomsCodes.RemoveAll();
			header.CustomsCodes.AddNew(OrgCusCode.CodeTypes.VATCode, "123456789012345678", CountryCodes.China);
			validationMessages = validator.ValidateTransaction(invoice, pivot);

			AssertEquals(0, validationMessages.Count);

			header.CustomsCodes.RemoveAll();
			header.CustomsCodes.AddNew(OrgCusCode.CodeTypes.VATCode, "1234567890123456789", CountryCodes.China);
			validationMessages = validator.ValidateTransaction(invoice, pivot);

			AssertEquals(0, validationMessages.Count);

			header.CustomsCodes.RemoveAll();
			header.CustomsCodes.AddNew(OrgCusCode.CodeTypes.VATCode, "12345678901234567890", CountryCodes.China);
			validationMessages = validator.ValidateTransaction(invoice, pivot);

			AssertEquals(0, validationMessages.Count);

			header.CustomsCodes.RemoveAll();
			header.CustomsCodes.AddNew(OrgCusCode.CodeTypes.VATCode, "123456789012345678901", CountryCodes.China);
			validationMessages = validator.ValidateTransaction(invoice, pivot);

			Assert(validationMessages.Contains(invalidTaxCodeMessage));

			header.CustomsCodes.RemoveAll();
			header.CustomsCodes.AddNew(OrgCusCode.CodeTypes.VATCode, "1234567890123456789+", CountryCodes.China);
			validationMessages = validator.ValidateTransaction(invoice, pivot);

			Assert(validationMessages.Contains(invalidTaxCodeMessage));

			if (isDebtor)
			{
				header.CustomsCodes.RemoveAll();
				header.CustomsCodes.AddNew(OrgCusCode.CodeTypes.VATCode, "", CountryCodes.China);

				header.OH_RL_NKClosestPort = "AUSYD";
				validationMessages = validator.ValidateTransaction(invoice, pivot);
				Assert(!validationMessages.Contains(invalidTaxCodeMessage));

				header.OH_RL_NKClosestPort = "CNBJS";
				validationMessages = validator.ValidateTransaction(invoice, pivot);
				Assert(validationMessages.Contains(invalidTaxCodeMessage));
			}

			header.CustomsCodes.RemoveAll();
			header.CustomsCodes.AddNew(OrgCusCode.CodeTypes.VATCode, "1234567890123456A", CountryCodes.China);
			validationMessages = validator.ValidateTransaction(invoice, pivot);

			AssertEquals(0, validationMessages.Count);

			header.CustomsCodes.RemoveAll();
			header.CustomsCodes.AddNew(OrgCusCode.CodeTypes.VATCode, "12345678A", CountryCodes.China);
			validationMessages = validator.ValidateTransaction(invoice, pivot);

			AssertEquals(0, validationMessages.Count);

			invoice.Branch.GB_OH_OrgProxy = ZGuid.Empty;
			validationMessages = validator.ValidateTransaction(invoice, pivot);

			Assert(!validationMessages.Contains(missTaxCodeMessage));
			Assert(!validationMessages.Contains(invalidTaxCodeMessage));
		}

		public void TestCheckDebtorMainARMailingAddress()
		{
			var missARMailingAddressMessage = "Invoice Debtor must have a Receivable Mailing Address recorded in 'ZH-CN' language.";
			var missCompanyNameMessage = "Invoice Debtor must have a Company Name recorded against it's Receivable Mailing Address in 'ZH-CN' language.";
			var missPhoneNumMessage = "Invoice Debtor must have a Phone Number recorded against it's Receivable Mailing Address in 'ZH-CN' language.";
			SetUpForEligible();
			TestObjectCreator.Debtor.Addresses.RemoveAll();
			TestObjectCreator.Debtor.OH_RL_NKClosestPort = "CNBJS";
			var validator = GetEInvoicingDataValidatorForTest();
			var (invoice, line, batch, pivot) = CreateTransactionAndEInvoicingBatch();

			AssertCheckDebtorMainARMailingAddress(null, missARMailingAddressMessage);

			var address = TestObjectCreator.Debtor.Addresses.AddNew();

			AssertCheckDebtorMainARMailingAddress(null, missARMailingAddressMessage);
			AssertCheckDebtorMainARMailingAddress(() => address.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.Receivables), missARMailingAddressMessage);
			AssertCheckDebtorMainARMailingAddress(() => address.AddressCapability.SetIsMainAddress(OrgAddressType.Receivables), missARMailingAddressMessage);
			AssertCheckDebtorMainARMailingAddress(() => address.Language = "ZH-CN", missARMailingAddressMessage);

			AssertCheckDebtorMainARMailingAddress(() =>
			{
				TestObjectCreator.Debtor.OH_RL_NKClosestPort = "AUSYD";
				TestObjectCreator.Debtor.OH_Category = OrgConstants.Category.Business;
				address.OA_State = ZString.Empty;
				address.OA_City = ZString.Empty;
				address.OA_Address1 = ZString.Empty;
				address.OA_Address2 = ZString.Empty;
				address.OA_CompanyNameOverride = "Test company name";
			}, missARMailingAddressMessage, false);
			AssertCheckDebtorMainARMailingAddress(() =>
			{
				TestObjectCreator.Debtor.OH_RL_NKClosestPort = "CNBJS";
				TestObjectCreator.Debtor.OH_Category = OrgConstants.Category.Business;
				address.OA_State = ZString.Empty;
				address.OA_City = ZString.Empty;
				address.OA_Address1 = ZString.Empty;
				address.OA_Address2 = ZString.Empty;
				address.OA_CompanyNameOverride = "Test company name";
			}, missARMailingAddressMessage);
			AssertCheckDebtorMainARMailingAddress(() =>
			{
				TestObjectCreator.Debtor.OH_Category = OrgConstants.Category.Business;
				address.OA_State = ZString.Empty;
				address.OA_City = ZString.Empty;
				address.OA_Address1 = ZString.Empty;
				address.OA_Address2 = ZString.Empty;
				address.OA_CompanyNameOverride = "Test company name";

				invoice.AH_ComplianceSubType = ChinaComplianceInfo.ComplianceSubTypeCodes.FDA;
			}, missARMailingAddressMessage, false);

			AssertCheckDebtorMainARMailingAddress(() =>
			{
				invoice.AH_ComplianceSubType = ChinaComplianceInfo.ComplianceSubTypeCodes.FDB;
			}, missARMailingAddressMessage, false);

			AssertCheckDebtorMainARMailingAddress(() =>
			{
				address.OA_State = "Test state";
				address.OA_Phone = ZString.Empty;
				invoice.AH_ComplianceSubType = ChinaComplianceInfo.ComplianceSubTypeCodes.FDA;
			}, missPhoneNumMessage, false);

			AssertCheckDebtorMainARMailingAddress(() =>
			{
				invoice.AH_ComplianceSubType = ChinaComplianceInfo.ComplianceSubTypeCodes.FDB;
			}, missPhoneNumMessage, false);

			address.OA_CompanyNameOverride = ZString.Empty;
			address.OA_Phone = ZString.Empty;
			invoice.AH_ComplianceSubType = ChinaComplianceInfo.ComplianceSubTypeCodes.TXA;

			AssertCheckDebtorMainARMailingAddress(() =>
			{
				TestObjectCreator.Debtor.OH_Category = OrgConstants.Category.Business;
				address.OA_State = "Test state";
				address.OA_City = ZString.Empty;
				address.OA_Address1 = ZString.Empty;
				address.OA_Address2 = ZString.Empty;
			}, missCompanyNameMessage);

			AssertCheckDebtorMainARMailingAddress(() =>
			{
				TestObjectCreator.Debtor.OH_Category = OrgConstants.Category.NaturalPersonIndividual;
				address.OA_State = "Test state";
				address.OA_City = ZString.Empty;
				address.OA_Address1 = ZString.Empty;
				address.OA_Address2 = ZString.Empty;
			}, missCompanyNameMessage);

			AssertCheckDebtorMainARMailingAddress(() =>
			{
				TestObjectCreator.Debtor.OH_Category = OrgConstants.Category.Business;
				address.OA_State = ZString.Empty;
				address.OA_City = "Test city";
				address.OA_Address1 = ZString.Empty;
				address.OA_Address2 = ZString.Empty;
			}, missCompanyNameMessage);

			AssertCheckDebtorMainARMailingAddress(() =>
			{
				TestObjectCreator.Debtor.OH_Category = OrgConstants.Category.NaturalPersonIndividual;
				address.OA_State = ZString.Empty;
				address.OA_City = "Test city";
				address.OA_Address1 = ZString.Empty;
				address.OA_Address2 = ZString.Empty;
			}, missCompanyNameMessage);

			AssertCheckDebtorMainARMailingAddress(() =>
			{
				TestObjectCreator.Debtor.OH_Category = OrgConstants.Category.Business;
				address.OA_State = ZString.Empty;
				address.OA_City = ZString.Empty;
				address.OA_Address1 = "Test address1";
				address.OA_Address2 = ZString.Empty;
			}, missCompanyNameMessage);

			AssertCheckDebtorMainARMailingAddress(() =>
			{
				TestObjectCreator.Debtor.OH_Category = OrgConstants.Category.NaturalPersonIndividual;
				address.OA_State = ZString.Empty;
				address.OA_City = ZString.Empty;
				address.OA_Address1 = "Test address1";
				address.OA_Address2 = ZString.Empty;
			}, missCompanyNameMessage);

			AssertCheckDebtorMainARMailingAddress(() =>
			{
				TestObjectCreator.Debtor.OH_Category = OrgConstants.Category.Business;
				address.OA_State = ZString.Empty;
				address.OA_City = ZString.Empty;
				address.OA_Address1 = ZString.Empty;
				address.OA_Address2 = "Test address2";
			}, missCompanyNameMessage);

			AssertCheckDebtorMainARMailingAddress(() =>
			{
				TestObjectCreator.Debtor.OH_Category = OrgConstants.Category.NaturalPersonIndividual;
				address.OA_State = ZString.Empty;
				address.OA_City = ZString.Empty;
				address.OA_Address1 = ZString.Empty;
				address.OA_Address2 = "Test address2";
			}, missCompanyNameMessage);

			AssertCheckDebtorMainARMailingAddress(() =>
			{
				TestObjectCreator.Debtor.OH_RL_NKClosestPort = "AUSYD";
				TestObjectCreator.Debtor.OH_Category = OrgConstants.Category.Business;
				address.OA_CompanyNameOverride = "Test company name";
			}, missPhoneNumMessage, false);

			AssertCheckDebtorMainARMailingAddress(() =>
			{
				TestObjectCreator.Debtor.OH_RL_NKClosestPort = "CNBJS";
				TestObjectCreator.Debtor.OH_Category = OrgConstants.Category.Business;
				address.OA_CompanyNameOverride = "Test company name";
			}, missPhoneNumMessage);

			AssertCheckDebtorMainARMailingAddress(() =>
			{
				TestObjectCreator.Debtor.OH_Category = OrgConstants.Category.NaturalPersonIndividual;
				address.OA_CompanyNameOverride = "Test company name";
			}, "", false);

			AssertCheckDebtorMainARMailingAddress(() =>
			{
				TestObjectCreator.Debtor.OH_Category = OrgConstants.Category.Business;
				address.OA_State = "123456789012345678901";
				address.OA_City = "12345678901234567890";
				address.OA_Address1 = "12345678901234567890";
				address.OA_Address2 = "12345678901234567890";
				address.OA_Phone = "12345678901234567890";
			}, "Receivable Mailing Address recorded in 'ZH-CN' language exceeds the limit 100.");

			AssertCheckDebtorMainARMailingAddress(() =>
			{
				TestObjectCreator.Debtor.OH_Category = OrgConstants.Category.NaturalPersonIndividual;
				address.OA_State = "123456789012345678901";
				address.OA_City = "12345678901234567890";
				address.OA_Address1 = "12345678901234567890";
				address.OA_Address2 = "12345678901234567890";
				address.OA_Phone = "12345678901234567890";
			}, "Receivable Mailing Address recorded in 'ZH-CN' language exceeds the limit 100.");

			AssertCheckDebtorMainARMailingAddress(() =>
			{
				TestObjectCreator.Debtor.OH_Category = OrgConstants.Category.Business;
				address.OA_State = "12345678901234567890";
				address.OA_City = "12345678901234567890";
				address.OA_Address1 = "12345678901234567890";
				address.OA_Address2 = "12345678901234567890";
				address.OA_Phone = "12345678901234567890";
			}, "", false);

			AssertCheckDebtorMainARMailingAddress(() =>
			{
				TestObjectCreator.Debtor.OH_RL_NKClosestPort = "AUSYD";
				TestObjectCreator.Debtor.OH_Category = OrgConstants.Category.Business;
				address.OA_State = "12345678901234567890";
				address.OA_City = "12345678901234567890";
				address.OA_Address1 = "12345678901234567890";
				address.OA_Address2 = "12345678901234567890";
				address.OA_Phone = "12345678901234567890";
			}, "", false);

			AssertCheckDebtorMainARMailingAddress(() => address.OA_Phone = "Test phone", "", false);

			void AssertCheckDebtorMainARMailingAddress(Action setValue, string expectedErrorMessage, bool hasErrors = true)
			{
				setValue?.Invoke();
				var validateMessages = validator.ValidateTransaction(invoice, pivot);
				if (hasErrors)
				{
					Assert(validateMessages.Contains(expectedErrorMessage));
				}
				else
				{
					AssertEquals(0, validateMessages.Count);
				}
			}
		}

		public void TestCheckDefaultTaxBankAccount()
		{
			var lackOfTaxBankMessage = "Invoice Debtor must have a default CN's TAX Bank Account in Invoiced Currency or Local Currency";
			SetUpForEligible();
			TestObjectCreator.Debtor.OH_RL_NKClosestPort = "CNBJS";
			var validator = GetEInvoicingDataValidatorForTest();
			var (invoice, line, batch, pivot) = CreateTransactionAndEInvoicingBatch();
			IReadOnlyCollection<ZString> validationMessages;

			AssertCheckDefaultTaxBankAccount(() => TestObjectCreator.Debtor.CompanyData.ARAccountDetailsCollection.RemoveAll(), invoice);
			AssertCheckDefaultTaxBankAccount(() => invoice.AH_ComplianceSubType = ChinaComplianceInfo.ComplianceSubTypeCodes.FDA, invoice, false);
			AssertCheckDefaultTaxBankAccount(() => invoice.AH_ComplianceSubType = ChinaComplianceInfo.ComplianceSubTypeCodes.FDB, invoice, false);

			invoice.AH_ComplianceSubType = ChinaComplianceInfo.ComplianceSubTypeCodes.TXA;
			var account = TestObjectCreator.Debtor.CompanyData.ARAccountDetailsCollection.AddNew();

			AssertCheckDefaultTaxBankAccount(null, invoice);
			AssertCheckDefaultTaxBankAccount(() => invoice.Header.OH_Category = OrgConstants.Category.NaturalPersonIndividual, invoice, false);
			AssertCheckDefaultTaxBankAccount(() => invoice.Header.OH_Category = OrgConstants.Category.Business, invoice);
			AssertCheckDefaultTaxBankAccount(() => TestObjectCreator.Debtor.OH_RL_NKClosestPort = "AUSYD", invoice, false);
			AssertCheckDefaultTaxBankAccount(() => TestObjectCreator.Debtor.OH_RL_NKClosestPort = "CNBJS", invoice);
			AssertCheckDefaultTaxBankAccount(() => account.A1_IsDefaultAccount = true, invoice);
			AssertCheckDefaultTaxBankAccount(() => account.A1_PaymentMethod = AccARAccountDetails.ARBankAccPayment, invoice);
			AssertCheckDefaultTaxBankAccount(() => account.A1_RN_NKCountryCode = CountryCodes.China, invoice);
			AssertCheckDefaultTaxBankAccount(() => account.A1_RX_NKAccountCurrency = CurrencyCodes.China, invoice);
			AssertCheckDefaultTaxBankAccount(() => account.A1_BankName = "Bank name", invoice);
			AssertCheckDefaultTaxBankAccount(() => account.A1_BankAccount = "Bank account", invoice, false);

			AssertCheckDefaultTaxBankAccount(() => account.A1_BankName = ZString.Empty, invoice);
			AssertCheckDefaultTaxBankAccount(() => invoice.AH_ComplianceSubType = ChinaComplianceInfo.ComplianceSubTypeCodes.FDA, invoice, false);
			AssertCheckDefaultTaxBankAccount(() => invoice.AH_ComplianceSubType = ChinaComplianceInfo.ComplianceSubTypeCodes.FDB, invoice, false);

			account.A1_BankName = "Bank name";
			account.A1_BankAccount = ZString.Empty;
			AssertCheckDefaultTaxBankAccount(() => invoice.AH_ComplianceSubType = ChinaComplianceInfo.ComplianceSubTypeCodes.TXA, invoice);
			AssertCheckDefaultTaxBankAccount(() => invoice.AH_ComplianceSubType = ChinaComplianceInfo.ComplianceSubTypeCodes.FDA, invoice, false);
			AssertCheckDefaultTaxBankAccount(() => invoice.AH_ComplianceSubType = ChinaComplianceInfo.ComplianceSubTypeCodes.FDB, invoice, false);

			account.A1_BankAccount = "Bank account";
			var invoice2 = CreateTransactionAndEInvoicingBatch(TestObjectCreator.USD).Invoice;

			AssertCheckDefaultTaxBankAccount(null, invoice2, false);
			AssertCheckDefaultTaxBankAccount(() => account.A1_RX_NKAccountCurrency = CurrencyCodes.UnitedStates, invoice2, false);
			AssertCheckDefaultTaxBankAccount(() => account.A1_RX_NKAccountCurrency = CurrencyCodes.Australia, invoice2);

			void AssertCheckDefaultTaxBankAccount(Action setValue, ARInvoice toValidateInvoice, bool hasErrors = true)
			{
				setValue?.Invoke();
				validationMessages = validator.ValidateTransaction(toValidateInvoice, pivot);
				if (hasErrors)
				{
					Assert(validationMessages.Contains(lackOfTaxBankMessage));
				}
				else
				{
					AssertEquals(0, validationMessages.Count);
				}
			}
		}

		public void TestCheckDiscountLineOrder()
		{
			SetUpForEligible();
			var validator = GetEInvoicingDataValidatorForTest();
			var (invoice, line, batch, pivot) = CreateTransactionAndEInvoicingBatch();
			var validationMessages = validator.ValidateTransaction(invoice, pivot);
			var job = TestObjectCreator.CreateJob(TestObjectCreator.CreateShipment("1111111"), TestObjectCreator.LocalClient, 0, TestObjectCreator.Agent, 0);

			AccountingMasterFilesRegistry.Instance.AlwaysTransmitNegativeChargesAsDiscount.SetValue(invoice.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

			var expectedErrorMessage = "The 'discount line' must be recorded immediately after the discounted line in the same invoice group.";

			AssertEquals(0, validationMessages.Count);

			TestObjectCreator.FRT.AC_ChargeType = ChargeType.Comment;

			var discountLine = TestObjectCreator.CreateARInvoiceLine(invoice, null, TestObjectCreator.MRG60, TestObjectCreator.CNY, 1m, "discount line", -10m);
			var anotherDiscountLine = TestObjectCreator.CreateARInvoiceLine(invoice, null, TestObjectCreator.MRG60, TestObjectCreator.CNY, 1m, "discount line 2", -20m);
			var normalLine = TestObjectCreator.CreateARInvoiceLine(invoice, null, TestObjectCreator.MRG60, TestObjectCreator.CNY, 1m, "normal line", 50m);
			var commentLine = TestObjectCreator.CreateARInvoiceLine(invoice, null, TestObjectCreator.FRT, TestObjectCreator.CNY, 1m, "comment line", 0m);
			var normalLineWithJob = TestObjectCreator.CreateARInvoiceLine(invoice, job, TestObjectCreator.MRG60, TestObjectCreator.CNY, 1m, "normal line", 50m);
			var discountLineWithJob = TestObjectCreator.CreateARInvoiceLine(invoice, job, TestObjectCreator.MRG60, TestObjectCreator.CNY, 1m, "discount line 3", -30m);

			normalLine.AL_Sequence = 1;
			discountLine.AL_Sequence = 2;
			invoice.Lines.RemoveAll();
			invoice.Lines.Add(normalLine);
			invoice.Lines.Add(discountLine);
			validationMessages = validator.ValidateTransaction(invoice, pivot);

			Assert(!validationMessages.Contains(expectedErrorMessage));

			normalLine.AL_Sequence = 1;
			commentLine.AL_Sequence = 2;
			discountLine.AL_Sequence = 3;
			invoice.Lines.RemoveAll();
			invoice.Lines.Add(normalLine);
			invoice.Lines.Add(commentLine);
			invoice.Lines.Add(discountLine);
			validationMessages = validator.ValidateTransaction(invoice, pivot);

			Assert(!validationMessages.Contains(expectedErrorMessage));

			normalLine.AL_Sequence = 1;
			normalLineWithJob.AL_Sequence = 1;
			discountLine.AL_Sequence = 2;
			discountLineWithJob.AL_Sequence = 2;
			invoice.Lines.RemoveAll();
			invoice.Lines.Add(normalLine);
			invoice.Lines.Add(normalLineWithJob);
			invoice.Lines.Add(discountLine);
			invoice.Lines.Add(discountLineWithJob);
			validationMessages = validator.ValidateTransaction(invoice, pivot);

			Assert(!validationMessages.Contains(expectedErrorMessage));

			discountLine.AL_Sequence = 1;
			normalLine.AL_Sequence = 2;
			invoice.Lines.RemoveAll();
			invoice.Lines.Add(discountLine);
			invoice.Lines.Add(normalLine);
			validationMessages = validator.ValidateTransaction(invoice, pivot);

			Assert(validationMessages.Contains(expectedErrorMessage));

			commentLine.AL_Sequence = 1;
			discountLine.AL_Sequence = 2;
			invoice.Lines.RemoveAll();
			invoice.Lines.Add(commentLine);
			invoice.Lines.Add(discountLine);
			validationMessages = validator.ValidateTransaction(invoice, pivot);

			Assert(validationMessages.Contains(expectedErrorMessage));

			discountLine.AL_Sequence = 1;
			anotherDiscountLine.AL_Sequence = 2;
			invoice.Lines.RemoveAll();
			invoice.Lines.Add(discountLine);
			invoice.Lines.Add(anotherDiscountLine);
			validationMessages = validator.ValidateTransaction(invoice, pivot);

			Assert(validationMessages.Contains(expectedErrorMessage));

			AccountingMasterFilesRegistry.Instance.AlwaysTransmitNegativeChargesAsDiscount.SetValue(invoice.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			validationMessages = validator.ValidateTransaction(invoice, pivot);
			Assert(!validationMessages.Contains(expectedErrorMessage));
		}

		public void TestDocumentActionTypePivotWontRunValidation()
		{
			SetUpForEligible();
			var validator = GetEInvoicingDataValidatorForTest();
			var (invoice, line, batch, pivot) = CreateTransactionAndEInvoicingBatch();

			var mainARMailingAddress = ChinaEInvoiceHelper.GetMainARMailingAddress(invoice.Header);
			mainARMailingAddress.OA_CompanyNameOverride = string.Empty;
			Factory.Save();

			Assert("Prerequisite", pivot.AIP_ActionType == EInvoicingPivotActionType.Submit);

			var expectedErrorMessage = "Invoice Debtor must have a Company Name recorded against it's Receivable Mailing Address in 'ZH-CN' language.";

			var validationMessages = validator.ValidateTransaction(invoice, pivot);
			Assert(validationMessages.Contains(expectedErrorMessage));

			batch = TestObjectCreator.CreateEInvoicingBatch(101, EInvoicingBatchState.Ready, GlbCompany.CurrentCompany);
			pivot = TestObjectCreator.CreateEInvoicingTransactionPivot(batch, invoice, EInvoicingPivotState.Batched, EInvoicingPivotActionType.DocumentAction);
			Factory.Save();

			validationMessages = validator.ValidateTransaction(invoice, pivot);
			Assert(validationMessages.IsNullOrEmpty());
		}

		public override BaseEInvoicingDataValidator GetEInvoicingDataValidatorForTest()
		{
			return new EInvoicingDataValidatorForChina(GlbCompany.CurrentCompany);
		}

		(ARInvoice Invoice, ARInvoiceLine Line, AccEInvoicingBatch Batch, AccEInvoicingTransactionPivot Pivot) CreateTransactionAndEInvoicingBatch(RefCurrency currency = null, bool useGLAccount = false)
		{
			var invoice = TestObjectCreator.CreateARInvoice<ARInvoice>("INV0001", currency ?? TestObjectCreator.CNY, 1m, TestObjectCreator.Debtor);
			invoice.AH_GB = TestBranch.PK;
			invoice.AH_ComplianceSubType = ChinaComplianceInfo.ComplianceSubTypeCodes.ETA;

			var invoiceLine = TestObjectCreator.CreateARInvoiceLine(invoice, null, TestObjectCreator.FRT, currency ?? TestObjectCreator.CNY, 1m, "desc", 100m);
			invoiceLine.AL_GB = TestBranch.PK;
			invoiceLine.AL_AT = TestObjectCreator.GST1.PK;
			if (useGLAccount)
			{
				invoiceLine.AL_AC = ZGuid.Empty;
				invoiceLine.AL_AG = TestObjectCreator.GLHeader1.PK;
			}

			var batch = TestObjectCreator.CreateEInvoicingBatch(100, EInvoicingBatchState.Ready, GlbCompany.CurrentCompany);
			var pivot = TestObjectCreator.CreateEInvoicingTransactionPivot(batch, invoice, EInvoicingPivotState.Batched);

			return (invoice, invoiceLine, batch, pivot);
		}

		void SetUpForEligible()
		{
			TestBranch = TestObjectCreator.CreateBranch("TBN", GlbCompany.CurrentCompany, TestBranchOrgProxy);
			TestObjectCreator.Debtor1.CustomsCodes.AddNew(OrgCusCode.CodeTypes.VATCode, "123456789012345", CountryCodes.China);

			TestObjectCreator.Debtor.CustomsCodes.AddNew(OrgCusCode.CodeTypes.VATCode, "123456789012345678", CountryCodes.China);
			var address = TestObjectCreator.CreateAddress(TestObjectCreator.Debtor, OrgAddressType.Receivables, true, "Address1", "Address2", "Test city", "Test state", "CN", "1", "Test phone", "Test email");
			address.Language = "ZH-CN";
			address.OA_CompanyNameOverride = "Test company name";

			var account = TestObjectCreator.Debtor.CompanyData.ARAccountDetailsCollection.AddNew();
			account.A1_IsDefaultAccount = true;
			account.A1_PaymentMethod = AccARAccountDetails.ARBankAccPayment;
			account.A1_RN_NKCountryCode = CountryCodes.China;
			account.A1_RX_NKAccountCurrency = CurrencyCodes.China;
			account.A1_BankName = "Bank name";
			account.A1_BankAccount = "Bank account";

			TestObjectCreator.FRT.AC_LocalLanguageDescription = "Local desc";

			Factory.Save();
		}

		GlbBranch TestBranch;
		OrgHeader TestBranchOrgProxy => TestObjectCreator.Debtor1;
	}
}
