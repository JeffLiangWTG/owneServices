using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.Invoicing.TaxFramework;
using Enterprise.Accounting.Business.Base.Interfaces;
using Enterprise.Accounting.Business.Base.Reversing.BadDebtWritingOff;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.Base.Transaction.Testing;
using Enterprise.Accounting.Business.ComplianceReport;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Business.Testing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Accounting.TaxFramework.Business;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Accounting.CriticalValidation;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Business.Testing.Accounting.Helpers;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Accounting.Business.AccountingConstants;
using static Enterprise.Core.Constants;
using static Enterprise.MasterFiles.Business.AccountingMasterFilesConstants;
using static Enterprise.Registry.Business.ComplianceReportConfigurationLookups;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	public abstract class InvoicingLineBaseValidationTest : DependentTransactionLineValidationTest
	{
		protected override (bool isNeedTestNoTaxMessage, bool isUseAPRegistry) GetIsNeedTestNoTaxMessageAndIsAP(TransactionHeaderWithLines header) => (true, header.AH_Ledger != LedgerTypes.AccountsReceivable);

		public void TestCheckAL_JHForAPInvoice()
		{
			var testObjectCreator = new TestObjectCreator(Factory);
			var consol = testObjectCreator.CreateConsol();
			var shipment = testObjectCreator.CreateShipment("S001", consol);
			var job = testObjectCreator.CreateJob(shipment, false);
			var invoice = testObjectCreator.CreateInvoice(typeof(APInvoice), "INV1", organisation: testObjectCreator.Creditor1);
			var line = (APInvoiceLine)testObjectCreator.CreateInvoiceLine(invoice, testObjectCreator.AUD, 1.0m, 10m, 10m, 0m);
			invoice.AH_PostDate = new ZDateTime(2009, 06, 15);
			invoice.SubmittedFromInvoicingForm = true;
			var charge = job.Charges.AddNew();
			charge.JR_AC = testObjectCreator.CC1.PK;
			charge.JR_OSSellAmt = 0M;
			line.AL_AC = testObjectCreator.CC1.PK;
			line.AL_JH = job.PK;
			line.AL_AT = testObjectCreator.CC1.GSTRate.PK;
			line.OriginalJobCharge = charge;
			charge.JR_AL_APLine = line.PK;
			charge.SetAmountsFromLinkedLinesForTests();
			testObjectCreator.CC1.AC_ChargeType = ChargeType.Disbursement;

			var lineValidation = new APInvoiceLineValidation(line);
			lineValidation.ValidateAL_JH();
			AssertNoErrors("AL_JH should not contain errors", line.AL_JHInfo);

			job.JH_Status = JobHeaderStatus.JobReadyForFinancialClosure.Code;
			var oldValue = Env.Security.AllowPostingChargesforFinancialClosureJob.IsAllowed;
			Env.Security.AllowPostingChargesforFinancialClosureJob.IsAllowed = false;

			using (AccountingConfigurationRegistry.Instance.AllowPostingAPInvoiceRelativeToExistingDSBChargeAgainstJFC.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				lineValidation.ValidateAL_JH();
				AssertHasErrorContaining(line.AL_JHInfo, "Cannot post this charge, because the job has Jobs Ready for Financial Closure status.");
			}

			using (new DisposableAction(() => Env.Security.AllowPostingChargesforFinancialClosureJob.IsAllowed = true, () => Env.Security.AllowPostingChargesforFinancialClosureJob.IsAllowed = oldValue))
			using (AccountingConfigurationRegistry.Instance.AllowPostingAPInvoiceRelativeToExistingDSBChargeAgainstJFC.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				lineValidation.ValidateAL_JH();
				AssertNoErrors("AL_JH should not contain errors", line.AL_JHInfo);
			}

			using (new DisposableAction(() => Env.Security.AllowPostingChargesforFinancialClosureJob.IsAllowed = false, () => Env.Security.AllowPostingChargesforFinancialClosureJob.IsAllowed = oldValue))
			using (AccountingConfigurationRegistry.Instance.AllowPostingAPInvoiceRelativeToExistingDSBChargeAgainstJFC.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				lineValidation.ValidateAL_JH();
				AssertHasWarning(line.AL_JHInfo, "You are posting Disbursement Charge when the job has Jobs Ready for Financial Closure status.");
				AssertNoErrors("AL_JH should not contain errors", line.AL_JHInfo);
			}

			using (new DisposableAction(() => Env.Security.AllowPostingChargesforFinancialClosureJob.IsAllowed = false, () => Env.Security.AllowPostingChargesforFinancialClosureJob.IsAllowed = oldValue))
			using (AccountingConfigurationRegistry.Instance.AllowPostingAPInvoiceRelativeToExistingDSBChargeAgainstJFC.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				line.OriginalJobCharge = null;
				line.ApportionmentChargeImportedFrom = Factory.NewWithValidTestData<ApportionSplitCharge>();
				lineValidation.ValidateAL_JH();
				AssertHasWarning(line.AL_JHInfo, "You are posting Disbursement Charge when the job has Jobs Ready for Financial Closure status.");
				AssertNoErrors("AL_JH should not contain errors", line.AL_JHInfo);
			}
		}

		public void TestCheckAL_JHForAPInvoiceForCreditNote()
		{
			var testObjectCreator = new TestObjectCreator(Factory);
			var consol = testObjectCreator.CreateConsol();
			var shipment = testObjectCreator.CreateShipment("S001", consol);
			var job = testObjectCreator.CreateJob(shipment, false);
			var invoice = testObjectCreator.CreateInvoice(typeof(APCreditNote), "INV1", organisation: testObjectCreator.Creditor1);
			var line = (APCreditNoteLine)testObjectCreator.CreateInvoiceLine(invoice, testObjectCreator.AUD, 1.0m, 10m, 10m, 0m);
			invoice.AH_PostDate = new ZDateTime(2009, 06, 15);
			invoice.SubmittedFromInvoicingForm = true;
			var charge = job.Charges.AddNew();
			charge.JR_AC = testObjectCreator.CC1.PK;
			charge.JR_OSSellAmt = 0M;
			line.AL_AC = testObjectCreator.CC1.PK;
			line.AL_JH = job.PK;
			line.AL_AT = testObjectCreator.CC1.GSTRate.PK;
			charge.JR_AL_APLine = line.PK;
			charge.SetAmountsFromLinkedLinesForTests();

			var lineValidation = new CreditNoteLineValidation(line);
			lineValidation.ValidateAL_JH();
			AssertNoErrors("AL_JH should not contain errors", line.AL_JHInfo);

			job.JH_Status = JobHeaderStatus.JobReadyForFinancialClosure.Code;
			var oldValue = Env.Security.AllowPostingChargesforFinancialClosureJob.IsAllowed;
			Env.Security.AllowPostingChargesforFinancialClosureJob.IsAllowed = false;

			using (AccountingConfigurationRegistry.Instance.AllowPostingAPInvoiceRelativeToExistingDSBChargeAgainstJFC.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				lineValidation.ValidateAL_JH();
				AssertHasErrorContaining(line.AL_JHInfo, "Cannot post this charge, because the job has Jobs Ready for Financial Closure status.");
			}

			using (new DisposableAction(() => Env.Security.AllowPostingChargesforFinancialClosureJob.IsAllowed = true, () => Env.Security.AllowPostingChargesforFinancialClosureJob.IsAllowed = oldValue))
			using (AccountingConfigurationRegistry.Instance.AllowPostingAPInvoiceRelativeToExistingDSBChargeAgainstJFC.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				lineValidation.ValidateAL_JH();
				AssertNoErrors("AL_JH should not contain errors", line.AL_JHInfo);
			}
		}

		public void TestCheckAL_ExchangeRateForInvoiceLinePopulatedFromApportionment()
		{
			ExchangeRateReader.GetReaderInstance().ClearCache();

			GlbCompany.CurrentCompany.AccExchangeRateConfigurations.SetExRate(JobInvoicingConsumerTypes.Shipment.Code, Constants.TransportModes.All, Constants.FreightShipmentDirection.Code.All, Constants.ExchangeRateTypes.Code.C01Rate, Constants.JobBillingExchangeRatePreference.Code.TodaysRate, 0, true);
			GlbCompany.CurrentCompany.AccExchangeRateConfigurations.SetExRate(JobInvoicingConsumerTypes.ForwardingConsol.Code, Constants.TransportModes.All, Constants.FreightShipmentDirection.Code.All, Constants.ExchangeRateTypes.Code.C02Rate, Constants.JobBillingExchangeRatePreference.Code.TodaysRate, 0, true);
			GlbCompany.CurrentCompany.Factory.Save();

			var invoiceDate = ZDateTime.Today.AddDays(2);

			var usdBaseRateForInvoiceDate = 1.340000m;
			var usdC01RateForInvoiceDate = 2.870000m;
			var usdC02RateForInvoiceDate = 3.560000m;
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.USD, Constants.ExchangeRateTypes.Code.BuyRate, usdBaseRateForInvoiceDate, invoiceDate, invoiceDate);
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.USD, Constants.ExchangeRateTypes.Code.SellRate, usdBaseRateForInvoiceDate, invoiceDate, invoiceDate);
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.USD, Constants.ExchangeRateTypes.Code.C01Rate, usdC01RateForInvoiceDate, invoiceDate, invoiceDate);
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.USD, Constants.ExchangeRateTypes.Code.C02Rate, usdC02RateForInvoiceDate, invoiceDate, invoiceDate);

			var consol = TestObjectCreator.CreateConsol();
			TestObjectCreator.CreateShipment("S1000", consol);
			var consolCost = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.CC1, 100m, TestObjectCreator.Creditor1);
			consolCost.E6_RX_NKCurrency = CurrencyCodes.UnitedStates;
			Factory.Save();

			var invoice = TestObjectCreator.CreateInvoice(InvoiceType, TestObjectCreator.USD, organisation: TestObjectCreator.Creditor1, invoiceDate: invoiceDate);
			invoice.AH_PostedToEFT = true;

			if (invoice.GetExRateLedger() == ExchangeRateValidLedgerEnum.AR)
			{
				PostingExRateRegistryAR.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, InvoicePostingExchangeRateOption.ExchangeRateBasedOnInvoiceDate.Code);
			}

			using (PostingExRateRegistryAP.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, InvoicePostingExchangeRateOption.ExchangeRateBasedOnInvoiceDate.Code))
			{
				assertInvoiceLineExchangeRateError(false);

				invoice.Lines.RemoveAll();
				AssertEquals(0, invoice.Lines.Count);

				assertInvoiceLineExchangeRateError(true);
			}

			ExchangeRateReader.GetReaderInstance().ClearCache();

			void assertInvoiceLineExchangeRateError(bool shouldChangeExRateManually)
			{
				var originatingCost = invoice.ConsolCosting.ConsolCosts.TryAddNewForConsol_ForTestOnly(consol);
				var newFactory = new BusinessObjectFactory();
				var importer = new InvoicingBaseConsolCostImporter(newFactory, originatingCost, invoice);
				importer.ImportCostsIntoCosting(new BusinessObject[] { consolCost });
				AssertEquals(1, invoice.ConsolCosting.ConsolCosts.Count);
				AssertEquals(usdC02RateForInvoiceDate, invoice.ConsolCosting.ConsolCosts[0].E6_ExchangeRate);

				var manualExchangeRate = 0.880000m;
				if (shouldChangeExRateManually)
				{
					invoice.ConsolCosting.ConsolCosts[0].E6_ExchangeRate = manualExchangeRate;
				}

				invoice.ImportAllApportionmentsFromCosting();
				AssertEquals(1, invoice.Lines.Count);

				var invoiceLine = invoice.Lines[0];
				AssertEquals(shouldChangeExRateManually ? manualExchangeRate : usdC02RateForInvoiceDate, invoiceLine.AL_ExchangeRate);
				Assert(invoiceLine.IsPopulatedFromImportedApportionment);
				invoiceLine.RunPreSaveValidation();
				if (shouldChangeExRateManually)
				{
					Assert("Exchange rate changed manually, error expected, use consol to create ExchangeRateConfigurationRateConsumer, not shipment job.", invoiceLine.AL_ExchangeRateInfo.Notifications.Contains(getExpectedValidationError(usdC02RateForInvoiceDate, manualExchangeRate, invoiceLine.IsAR())));
				}
				else
				{
					Assert("Exchange rate is defaulted correctly, no errors expected, use consol to create ExchangeRateConfigurationRateConsumer, not shipment job.", !invoiceLine.AL_ExchangeRateInfo.Notifications.Contains(getExpectedValidationError(usdC01RateForInvoiceDate, usdC02RateForInvoiceDate, invoiceLine.IsAR())));
				}
			}

			ZString getExpectedValidationError(ZDecimal expectedRate, ZDecimal enteredRate, bool isAR)
			{
				var ledger = isAR ? "AR" : "AP";
				return $@"The ""{ledger} Invoice Posting Exchange Rate Option"" has been set to ""Exchange Rate based on Invoice Date"".
With this option, the exchange rate should not be changed manually. System expected {expectedRate} rate but {enteredRate} was entered.";
			}
		}

		[TestDate(2018, 3, 1)]
		public void TestComplianceDocumentRelatedPropertiesShouldBeSameForSameDocumentNumber()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Taiwan))
			{
				var itemSet = (AccountingMasterFilesRegistry)Activator.CreateInstance(typeof(AccountingMasterFilesRegistry), true);
				var newlist = new ComplianceDocumentSupportingReasonCollection();
				newlist.Add(new ComplianceDocumentSupportingReason() { Code = "AAA", EnglishDescription = "test aaa" });
				newlist.Add(new ComplianceDocumentSupportingReason() { Code = "BBB", EnglishDescription = "test bbb" });
				itemSet.ComplianceDocumentSupportingDocumentType.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, newlist);

				var newReasonlist = new ComplianceDocumentSupportingReasonCollection();
				newReasonlist.Add(new ComplianceDocumentSupportingReason() { Code = "R1", EnglishDescription = "test reason a" });
				newReasonlist.Add(new ComplianceDocumentSupportingReason() { Code = "R2", EnglishDescription = "test reason b" });
				itemSet.ComplianceDocumentSupportingReasonsPayables.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, newReasonlist);
				AccountingMasterFilesRegistry.Instance.EnableComplianceDocumentModule.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
				Factory.Save();

				var invoice = Factory.New<APInvoice>();
				TestObjectCreator.AALSHI.CompanyData.OB_APCreateVATComplianceDocumentOnPosting = "PCD";
				invoice.AH_OH = TestObjectCreator.AALSHI.PK;

				var line1 = (APInvoiceLine)invoice.Lines.AddNew();
				var line2 = (APInvoiceLine)invoice.Lines.AddNew();
				line1.CreateComplianceDocumentRecordOnPosting = true;
				line1.ComplianceDocumentNumber = "DN123";
				line1.ComplianceDocumentOrganization = TestObjectCreator.AALSHI.PK;
				line1.ComplianceDocumentVATRegistrationNum = "10458574";
				line1.ComplianceDocumentDate = new ZDateTime(2018, 1, 1, 8, 50, 1);
				line1.ComplianceSupportingDocumentType = "AAA";
				line1.ComplianceDocumentSupportingReason = "R1";
				line1.ComplianceSupportingDocumentNumber = "1";
				line1.ComplianceDocumentReportingPeriod = 201803;
				line1.ComplianceSubType = "TXI";

				line2.CreateComplianceDocumentRecordOnPosting = true;
				line2.ComplianceDocumentNumber = "DN123";
				line2.ComplianceDocumentOrganization = TestObjectCreator.Creditor1.PK;
				line2.ComplianceDocumentVATRegistrationNum = "10458575";
				line2.ComplianceDocumentDate = new ZDateTime(2018, 2, 1, 8, 50, 1);
				line2.ComplianceSupportingDocumentType = "BBB";
				line2.ComplianceDocumentSupportingReason = "R2";
				line2.ComplianceSupportingDocumentNumber = "2";
				line2.ComplianceDocumentReportingPeriod = 201804;
				line2.ComplianceSubType = "NTI";

				((APInvoiceLineValidation)line2.Validation).ValidateComplianceDocumentOrganization();
				AssertHasError(line2.ComplianceDocumentOrganizationInfo, "For all transaction lines with the same document number, the Compliance Document Organization must be the same.");
				((APInvoiceLineValidation)line2.Validation).ValidateComplianceDocumentVATRegistrationNum();
				AssertHasError(line2.ComplianceDocumentVATRegistrationNumInfo, "For all transaction lines with the same document number, the VAT Registration Number must be the same.");
				((APInvoiceLineValidation)line2.Validation).ValidateComplianceDocumentDate();
				AssertHasError(line2.ComplianceDocumentDateInfo, "For all transaction lines with the same document number, the Document Date must be the same.");
				((APInvoiceLineValidation)line2.Validation).ValidateComplianceSupportingDocumentType();
				AssertHasError(line2.ComplianceSupportingDocumentTypeInfo, "For all transaction lines with the same document number, the Supporting Doc Type must be the same.");
				((APInvoiceLineValidation)line2.Validation).ValidateComplianceDocumentSupportingReason();
				AssertHasError(line2.ComplianceDocumentSupportingReasonInfo, "For all transaction lines with the same document number, the Supporting Reason must be the same.");
				((APInvoiceLineValidation)line2.Validation).ValidateComplianceSupportingDocumentNumber();
				AssertHasError(line2.ComplianceSupportingDocumentNumberInfo, "For all transaction lines with the same document number, the Supporting Doc Number must be the same.");
				((APInvoiceLineValidation)line2.Validation).ValidateComplianceDocumentReportingPeriod();
				AssertHasError(line2.ComplianceDocumentReportingPeriodInfo, "For all transaction lines with the same document number, the Reporting Period must be the same.");
				((APInvoiceLineValidation)line2.Validation).ValidateComplianceSubType();
				AssertHasError(line2.ComplianceSubTypeInfo, "For all transaction lines with the same document number, the Compliance Sub Type must be the same.");

				line2.ComplianceDocumentOrganization = TestObjectCreator.AALSHI.PK;
				line2.ComplianceDocumentVATRegistrationNum = "10458574";
				line2.ComplianceDocumentDate = new ZDateTime(2018, 1, 1, 8, 50, 1);
				line2.ComplianceSupportingDocumentType = "AAA";
				line2.ComplianceDocumentSupportingReason = "R1";
				line2.ComplianceSupportingDocumentNumber = "1";
				line2.ComplianceDocumentReportingPeriod = 201803;
				line2.ComplianceSubType = "TXI";

				((APInvoiceLineValidation)line2.Validation).ValidateComplianceDocumentOrganization();
				AssertNoError(line2.ComplianceDocumentOrganizationInfo, "For all transaction lines with the same document number, the Compliance Document Organization must be the same.");
				((APInvoiceLineValidation)line2.Validation).ValidateComplianceDocumentVATRegistrationNum();
				AssertNoError(line2.ComplianceDocumentVATRegistrationNumInfo, "For all transaction lines with the same document number, the VAT Registration Number must be the same.");
				((APInvoiceLineValidation)line2.Validation).ValidateComplianceDocumentDate();
				AssertNoError(line2.ComplianceDocumentDateInfo, "For all transaction lines with the same document number, the Document Date must be the same.");
				((APInvoiceLineValidation)line2.Validation).ValidateComplianceSupportingDocumentType();
				AssertNoError(line2.ComplianceSupportingDocumentTypeInfo, "For all transaction lines with the same document number, the Supporting Doc Type must be the same.");
				((APInvoiceLineValidation)line2.Validation).ValidateComplianceDocumentSupportingReason();
				AssertNoError(line2.ComplianceDocumentSupportingReasonInfo, "For all transaction lines with the same document number, the Supporting Reason must be the same.");
				((APInvoiceLineValidation)line2.Validation).ValidateComplianceSupportingDocumentNumber();
				AssertNoError(line2.ComplianceSupportingDocumentNumberInfo, "For all transaction lines with the same document number, the Supporting Doc Number must be the same.");
				((APInvoiceLineValidation)line2.Validation).ValidateComplianceDocumentReportingPeriod();
				AssertNoError(line2.ComplianceDocumentReportingPeriodInfo, "For all transaction lines with the same document number, the Reporting Period must be the same.");
				((APInvoiceLineValidation)line2.Validation).ValidateComplianceSubType();
				AssertNoError(line2.ComplianceSubTypeInfo, "For all transaction lines with the same document number, the Compliance Sub Type must be the same.");

				AccountingMasterFilesRegistry.Instance.EnableComplianceDocumentModule.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
				TestObjectCreator.AALSHI.CompanyData.OB_APCreateVATComplianceDocumentOnPosting = "PCD";

				invoice.AH_Ledger = LedgerTypes.AccountsPayable;
				invoice.AH_TransactionType = TransactionTypes.Invoice;
				invoice.AH_OH = TestObjectCreator.AALSHI.PK;

				line2.ComplianceDocumentOrganization = TestObjectCreator.Creditor1.PK;
				line2.ComplianceDocumentVATRegistrationNum = "10458575";
				line2.ComplianceDocumentDate = new ZDateTime(2018, 2, 1, 8, 50, 1);
				line2.ComplianceSupportingDocumentType = "BBB";
				line2.ComplianceDocumentSupportingReason = "R2";
				line2.ComplianceSupportingDocumentNumber = "2";
				line2.ComplianceDocumentReportingPeriod = 201804;
				line2.ComplianceSubType = "NTI";
				line2.ComplianceDocumentNumber = "DN123";

				AssertHasError(line2.ComplianceDocumentOrganizationInfo, "For all transaction lines with the same document number, the Compliance Document Organization must be the same.");
				AssertHasError(line2.ComplianceDocumentVATRegistrationNumInfo, "For all transaction lines with the same document number, the VAT Registration Number must be the same.");
				AssertHasError(line2.ComplianceDocumentDateInfo, "For all transaction lines with the same document number, the Document Date must be the same.");
				AssertHasError(line2.ComplianceSupportingDocumentTypeInfo, "For all transaction lines with the same document number, the Supporting Doc Type must be the same.");
				AssertHasError(line2.ComplianceDocumentSupportingReasonInfo, "For all transaction lines with the same document number, the Supporting Reason must be the same.");
				AssertHasError(line2.ComplianceSupportingDocumentNumberInfo, "For all transaction lines with the same document number, the Supporting Doc Number must be the same.");
				AssertHasError(line2.ComplianceDocumentReportingPeriodInfo, "For all transaction lines with the same document number, the Reporting Period must be the same.");
				AssertHasError(line2.ComplianceSubTypeInfo, "For all transaction lines with the same document number, the Compliance Sub Type must be the same.");
			}
		}

		public void TestCheckComplianceSupportingDocumentNumber()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Taiwan))
			{
				AccountingMasterFilesRegistry.Instance.EnableComplianceDocumentModule.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

				var invoice = Factory.New<APInvoice>();
				var line = (APInvoiceLine)invoice.Lines.AddNew();
				TestObjectCreator.AALSHI.CompanyData.OB_APCreateVATComplianceDocumentOnPosting = "PCD";
				invoice.AH_OH = TestObjectCreator.AALSHI.PK;
				line.CreateComplianceDocumentRecordOnPosting = true;

				line.ComplianceSupportingDocumentNumber = "123";
				((APInvoiceLineValidation)line.Validation).ValidateComplianceSupportingDocumentNumber();
				Assert(line.ComplianceSupportingDocumentNumberInfo.HasError("The Supporting Document Number should have a Supporting Document Type."));

				line.ComplianceSupportingDocumentType = "xx";
				((APInvoiceLineValidation)line.Validation).ValidateComplianceSupportingDocumentNumber();
				AssertNoErrors(line.ComplianceSupportingDocumentNumberInfo);
			}
		}

		public void TestCheckComplianceDocumentOrganization()
		{
			AccountingMasterFilesRegistry.Instance.EnableComplianceDocumentModule.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			var apInv = TestObjectCreator.CreateAPInvoice<APInvoice>("INV001", TestObjectCreator.TWD, 1m, 100m, 10m, 0m, 100m, 10m, 0m, TestObjectCreator.AALSHI);
			var apInvLine = (APInvoiceLine)apInv.Lines.AddNew();
			apInvLine.AL_AG = TestObjectCreator.GLHeader1.PK;
			var invoiceDocumentHeader = TestObjectCreator.CreateComplianceDocumentHeaderWithLine(LedgerTypes.AccountsPayable, "desc", "TX00010001", TaiwanComplianceInfo.ComplianceSubTypeCodes.NTI, "desc", apInvLine);
			invoiceDocumentHeader.ADH_OH_Organisation = TestObjectCreator.AALSHI.PK;
			invoiceDocumentHeader.ADH_ReportingPeriod = 201802;
			invoiceDocumentHeader.ADH_DocumentDate = new ZDateTime(2018, 2, 2);
			TestObjectCreator.AALSHI.CompanyData.OB_APCreateVATComplianceDocumentOnPosting = "PCD";

			var invoice = Factory.New<APInvoice>();
			invoice.AH_Ledger = LedgerTypes.AccountsPayable;
			invoice.AH_TransactionType = TransactionTypes.Invoice;
			invoice.AH_OH = TestObjectCreator.AALSHI.PK;
			var line = (APInvoiceLine)invoice.Lines.AddNew();
			line.ComplianceDocumentOrganization = ZGuid.NewZGuid();
			((APInvoiceLineValidation)line.Validation).ValidateComplianceDocumentOrganization();
			AssertHasError(line.ComplianceDocumentOrganizationInfo, "Enter a valid selection.");

			line.ComplianceDocumentOrganization = TestObjectCreator.AALSHI.PK;
			((APInvoiceLineValidation)line.Validation).ValidateComplianceDocumentOrganization();
			AssertNoErrors(line.ComplianceDocumentOrganizationInfo);
		}

		public void TestPeriodApportionmentDatesFieldsHaveValidPeriod()
		{
			TestCaseHelper.ClearTable(AccPeriodManagementSchema.Constants.TableName);
			var periodManagementTestHelper = new AccountingPeriodTestHelper();
			periodManagementTestHelper.PostPeriodsForEntireYear(2020, GlbCompany.CurrentCompany.PK);
			Factory.Save();

			var invoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), TestObjectCreator.AUD, 1m, TestObjectCreator.Creditor1);
			var invoiceLine = (InvoicingLineBase)invoice.Lines.AddNew();
			Assert("Precondition", invoiceLine.PeriodApportionmentLines.IsNullOrEmpty());
			invoiceLine.AL_AG = TestObjectCreator.GLHeader1.PK;
			invoiceLine.AL_ExchangeRate = 0.64m;
			invoiceLine.AL_OSExTaxAmount = 100m;
			invoiceLine.AL_RX_NKTransactionCurrency = "USD";
			invoiceLine.PeriodApportionmentMethod = PeriodApportionmentMethods.Codes.EquallyOverPeriods;

			TestObjectCreator.GLJournalClearingAccount.AG_AccountType = Core.Constants.AccountType.BalanceSheetAccount;
			invoiceLine.PeriodClearingGLAccountPK = TestObjectCreator.GLJournalClearingAccount.PK;
			invoiceLine.PeriodStartDate = new ZDate(2020, 2, 15);
			invoiceLine.PeriodEndDate = new ZDate(2021, 1, 29);

			var expectedError = @"No accounting period exists for the selected date.
An expense cannot be apportioned to a non - existent period.
Please amend the date or go to General Ledger > Period Management and create a Financial Year to cover the period of apportionment.";

			AssertNoErrors(invoiceLine.PeriodClearingGLAccountPKInfo);
			AssertNoErrors(invoiceLine.PeriodStartDateInfo);
			AssertHasError(invoiceLine.PeriodEndDateInfo, expectedError);

			invoiceLine.PeriodStartDate = new ZDate(2021, 2, 1);
			invoiceLine.PeriodEndDate = new ZDate(2021, 3, 31);

			AssertHasError(invoiceLine.PeriodStartDateInfo, expectedError);
			AssertHasError(invoiceLine.PeriodEndDateInfo, expectedError);
		}

		public void TestWhenPeriodApportionmentMethodIsDefault_PeriodApportionmentMethodFieldsValidationOccurs()
		{
			TestCaseHelper.ClearTable(AccPeriodManagementSchema.Constants.TableName);
			var periodManagementTestHelper = new AccountingPeriodTestHelper();
			periodManagementTestHelper.PostPeriodsForEntireYear(2020, GlbCompany.CurrentCompany.PK);
			Factory.Save();

			var invoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), TestObjectCreator.AUD, 1m, TestObjectCreator.Creditor1);
			var invoiceLine = (InvoicingLineBase)invoice.Lines.AddNew();
			Assert("Precondition", invoiceLine.PeriodApportionmentLines.IsNullOrEmpty());
			invoiceLine.AL_AG = TestObjectCreator.GLHeader1.PK;
			invoiceLine.AL_ExchangeRate = 0.64m;
			invoiceLine.AL_OSExTaxAmount = 100m;
			invoiceLine.AL_RX_NKTransactionCurrency = "USD";

			AssertEquals("DEF", invoiceLine.PeriodApportionmentMethod);
			Assert(invoiceLine.PeriodApportionmentLines.IsNullOrEmpty());

			invoiceLine.PeriodApportionmentMethod = "XXX";
			AssertHasError(invoiceLine.PeriodApportionmentMethodInfo, "Enter a valid Period Apportionment Method.");

			invoiceLine.PeriodApportionmentMethod = PeriodApportionmentMethods.Codes.Default;
			AssertNoErrors(invoiceLine.PeriodApportionmentMethodInfo);

			invoiceLine.PeriodApportionmentMethod = PeriodApportionmentMethods.Codes.EquallyOverPeriods;
			AssertNoErrors(invoiceLine.PeriodApportionmentMethodInfo);

			invoiceLine.AL_JH = TestObjectCreator.Job1.PK;
			AssertEquals("DEF", invoiceLine.PeriodApportionmentMethod);
			AssertNoErrors(invoiceLine.PeriodApportionmentMethodInfo);

			bool allApportionmentFieldsReadOnly = invoiceLine.PeriodApportionmentMethod_ReadOnly && invoiceLine.PeriodClearingGLAccountPK_ReadOnly && invoiceLine.PeriodStartDate_ReadOnly && invoiceLine.PeriodEndDate_ReadOnly;
			bool allNonApportionmentMethodFieldsBlank = invoiceLine.PeriodApportionmentMethod == PeriodApportionmentMethods.Codes.Default && invoiceLine.PeriodClearingGLAccountPK.IsEmpty && invoiceLine.PeriodStartDate.IsEmpty && invoiceLine.PeriodEndDate.IsEmpty;

			CombineAssertions(() =>
			{
				Assert("Not all Apportionment Fields are Read Only", allApportionmentFieldsReadOnly);
				Assert("Not all Non-Apportionment Method Fields are Blank", allNonApportionmentMethodFieldsBlank);
			});
		}

		public void TestWhenPeriodApportionmentMethodIsNotDefault_ClearingAccountAndServicePeriodIsMandatory()
		{
			TestCaseHelper.ClearTable(AccPeriodManagementSchema.Constants.TableName);
			var periodManagementTestHelper = new AccountingPeriodTestHelper();
			periodManagementTestHelper.PostPeriodsForEntireYear(2020, GlbCompany.CurrentCompany.PK);
			Factory.Save();

			var invoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), TestObjectCreator.AUD, 1m, TestObjectCreator.Creditor1);
			var invoiceLine = (InvoicingLineBase)invoice.Lines.AddNew();
			Assert("Precondition", invoiceLine.PeriodApportionmentLines.IsNullOrEmpty());
			invoiceLine.AL_AG = TestObjectCreator.GLHeader1.PK;
			invoiceLine.AL_ExchangeRate = 0.64m;
			invoiceLine.AL_OSExTaxAmount = 100m;
			invoiceLine.AL_RX_NKTransactionCurrency = "USD";
			Assert(invoiceLine.PeriodApportionmentLines.IsNullOrEmpty());

			invoiceLine.PeriodApportionmentMethod = PeriodApportionmentMethods.Codes.EquallyOverPeriods;

			invoiceLine.Validation.ValidateAll();

			CombineAssertions(() =>
			{
				AssertHasError(invoiceLine.PeriodClearingGLAccountPKInfo, "Please enter a Period Clearing GL Account.");
				AssertHasError(invoiceLine.PeriodStartDateInfo, "Please enter a Service Period Start Date.");
				AssertHasError(invoiceLine.PeriodEndDateInfo, "Please enter a Service Period End Date.");
			});

			TestObjectCreator.GLJournalClearingAccount.AG_AccountType = Core.Constants.AccountType.BalanceSheetAccount;
			invoiceLine.PeriodClearingGLAccountPK = TestObjectCreator.GLJournalClearingAccount.PK;
			invoiceLine.PeriodStartDate = new ZDate(2020, 2, 15);
			invoiceLine.PeriodEndDate = new ZDate(2020, 4, 29);

			CombineAssertions(() =>
			{
				AssertNoErrors(invoiceLine.PeriodClearingGLAccountPKInfo);
				AssertNoErrors(invoiceLine.PeriodStartDateInfo);
				AssertNoErrors(invoiceLine.PeriodEndDateInfo);
			});
		}

		public void TestWhenPeriodApportionmentMethodIsNotDefault_ClearingAccountTypeValidationOccurs()
		{
			TestCaseHelper.ClearTable(AccPeriodManagementSchema.Constants.TableName);
			var periodManagementTestHelper = new AccountingPeriodTestHelper();
			periodManagementTestHelper.PostPeriodsForEntireYear(2020, GlbCompany.CurrentCompany.PK);
			Factory.Save();

			var invoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), TestObjectCreator.AUD, 1m, TestObjectCreator.Creditor1);
			var invoiceLine = (InvoicingLineBase)invoice.Lines.AddNew();
			Assert("Precondition", invoiceLine.PeriodApportionmentLines.IsNullOrEmpty());
			invoiceLine.AL_AG = TestObjectCreator.GLHeader1.PK;
			invoiceLine.AL_ExchangeRate = 0.64m;
			invoiceLine.AL_OSExTaxAmount = 100m;
			invoiceLine.AL_RX_NKTransactionCurrency = "USD";
			Assert(invoiceLine.PeriodApportionmentLines.IsNullOrEmpty());

			invoiceLine.PeriodApportionmentMethod = PeriodApportionmentMethods.Codes.EquallyOverPeriods;
			TestObjectCreator.GLJournalClearingAccount.AG_AccountType = Core.Constants.AccountType.ProfitAndLossAccount;
			invoiceLine.PeriodClearingGLAccountPK = TestObjectCreator.GLJournalClearingAccount.PK;
			invoiceLine.PeriodStartDate = new ZDate(2020, 3, 15);
			invoiceLine.PeriodEndDate = new ZDate(2020, 4, 29);
			Assert(!invoiceLine.PeriodApportionmentLines.IsNullOrEmpty());

			AssertHasError(invoiceLine.PeriodClearingGLAccountPKInfo, "Period Apportionment Clearing Account must be a Balance Sheet Account.");

			TestObjectCreator.GLJournalClearingAccount.AG_AccountType = Core.Constants.AccountType.BalanceSheetAccount;
			invoiceLine.PeriodClearingGLAccountPK = ZGuid.Empty;
			invoiceLine.PeriodClearingGLAccountPK = TestObjectCreator.GLJournalClearingAccount.PK;
			AssertNoErrors(invoiceLine.PeriodClearingGLAccountPKInfo);

			TestObjectCreator.GLJournalClearingAccount.AG_DisallowDirectPosting = true;
			invoiceLine.PeriodClearingGLAccountPK = ZGuid.Empty;
			invoiceLine.PeriodClearingGLAccountPK = TestObjectCreator.GLJournalClearingAccount.PK;
			AssertHasError(invoiceLine.PeriodClearingGLAccountPKInfo, "Period Apportionment Clearing Account must allow Direct Posting.");

			AssertNoWarnings(invoiceLine.PeriodClearingGLAccountPKInfo);
			TestObjectCreator.CreateGLHeaderSubAccount(TestObjectCreator.GLJournalClearingAccount, OrgHeaderSchema.Constants.Prefix, true);
			TestObjectCreator.CreateGLHeaderSubAccount(TestObjectCreator.GLJournalClearingAccount, GlbStaffSchema.Constants.Prefix, true);
			invoiceLine.PeriodClearingGLAccountPK = ZGuid.Empty;
			invoiceLine.PeriodClearingGLAccountPK = TestObjectCreator.GLJournalClearingAccount.PK;
			AssertHasWarning(invoiceLine.PeriodClearingGLAccountPKInfo, "This Clearing Account has 2 sub account type(s). You can specify the sub accounts value by editing the GL Journals in Manage > General Ledger > Journals module on posting of the Expense/Revenue Apportionment.");
		}

		public void TestWhenPeriodApportionmentMethodIsNotDefault_ServicePeriodOrderValidationOccurs()
		{
			TestCaseHelper.ClearTable(AccPeriodManagementSchema.Constants.TableName);
			var periodManagementTestHelper = new AccountingPeriodTestHelper();
			periodManagementTestHelper.PostPeriodsForEntireYear(2020, GlbCompany.CurrentCompany.PK);
			Factory.Save();

			var invoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), TestObjectCreator.AUD, 1m, TestObjectCreator.Creditor1);
			var invoiceLine = (InvoicingLineBase)invoice.Lines.AddNew();
			Assert("Precondition", invoiceLine.PeriodApportionmentLines.IsNullOrEmpty());
			invoiceLine.AL_AG = TestObjectCreator.GLHeader1.PK;
			invoiceLine.AL_ExchangeRate = 0.64m;
			invoiceLine.AL_OSExTaxAmount = 100m;
			invoiceLine.AL_RX_NKTransactionCurrency = "USD";
			Assert(invoiceLine.PeriodApportionmentLines.IsNullOrEmpty());

			invoiceLine.PeriodApportionmentMethod = PeriodApportionmentMethods.Codes.EquallyOverPeriods;
			TestObjectCreator.GLJournalClearingAccount.AG_AccountType = Core.Constants.AccountType.BalanceSheetAccount;
			invoiceLine.PeriodClearingGLAccountPK = TestObjectCreator.GLJournalClearingAccount.PK;
			invoiceLine.PeriodStartDate = new ZDate(2020, 5, 15);
			invoiceLine.PeriodEndDate = new ZDate(2020, 4, 29);
			invoiceLine.Validation.ValidateAll();
			AssertHasError(invoiceLine.PeriodStartDateInfo, "Service Period Start Date cannot be after the Service Period End Date.");
			AssertHasError(invoiceLine.PeriodEndDateInfo, "Service Period Start Date cannot be after the Service Period End Date.");

			invoiceLine.PeriodStartDate = new ZDate(2020, 2, 15);
			invoiceLine.PeriodEndDate = new ZDate(2020, 4, 29);
			invoiceLine.Validation.ValidateAll();
			AssertNoErrors(invoiceLine.PeriodStartDateInfo);
			AssertNoErrors(invoiceLine.PeriodEndDateInfo);
		}

		public void TestWhenPeriodApportionmentMethodIsNotDefault_ServicePeriodClosedValidationOccurs()
		{
			TestCaseHelper.ClearTable(AccPeriodManagementSchema.Constants.TableName);
			var periodManagementTestHelper = new AccountingPeriodTestHelper();
			periodManagementTestHelper.SetupSinglePeriod(202001, new ZDate(2020, 1, 1), new ZDate(2020, 1, 31));
			periodManagementTestHelper.SetupSinglePeriod(202002, new ZDate(2020, 2, 1), new ZDate(2020, 2, 29));
			AccPeriodManagement period202001 = Factory.LoadTop1(typeof(AccPeriodManagement), new ZQuery(AccPeriodManagementSchema.AM_Period, 202001)) as AccPeriodManagement;
			AccPeriodManagement period202002 = Factory.LoadTop1(typeof(AccPeriodManagement), new ZQuery(AccPeriodManagementSchema.AM_Period, 202002)) as AccPeriodManagement;
			period202001.AM_IsGeneralLedgerClosed = ZBool.True;
			period202002.AM_IsGeneralLedgerClosed = ZBool.True;
			Factory.Save();

			var invoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), TestObjectCreator.AUD, 1m, TestObjectCreator.Creditor1);
			var invoiceLine = (InvoicingLineBase)invoice.Lines.AddNew();
			Assert("Precondition", invoiceLine.PeriodApportionmentLines.IsNullOrEmpty());
			invoiceLine.AL_AG = TestObjectCreator.GLHeader1.PK;
			invoiceLine.AL_ExchangeRate = 0.64m;
			invoiceLine.AL_OSExTaxAmount = 100m;
			invoiceLine.AL_RX_NKTransactionCurrency = "USD";
			Assert(invoiceLine.PeriodApportionmentLines.IsNullOrEmpty());

			invoiceLine.PeriodApportionmentMethod = PeriodApportionmentMethods.Codes.EquallyOverPeriods;
			TestObjectCreator.GLJournalClearingAccount.AG_AccountType = Core.Constants.AccountType.BalanceSheetAccount;
			invoiceLine.PeriodClearingGLAccountPK = TestObjectCreator.GLJournalClearingAccount.PK;
			invoiceLine.PeriodStartDate = new ZDate(2020, 1, 15);
			invoiceLine.PeriodEndDate = new ZDate(2020, 2, 20);
			invoiceLine.Validation.ValidateAll();

			AssertHasError(invoiceLine.PeriodStartDateInfo, "Service Period covers GL periods that are already closed. Unable to post period apportionment journals.");
			AssertHasError(invoiceLine.PeriodEndDateInfo, "Service Period covers GL periods that are already closed. Unable to post period apportionment journals.");

			period202001.AM_IsGeneralLedgerClosed = ZBool.False;
			period202002.AM_IsGeneralLedgerClosed = ZBool.False;

			invoiceLine.PeriodStartDate = new ZDate(2020, 1, 14);
			invoiceLine.PeriodEndDate = new ZDate(2020, 2, 21);
			invoiceLine.Validation.ValidateAll();
			AssertNoErrors(invoiceLine.PeriodStartDateInfo);
			AssertNoErrors(invoiceLine.PeriodEndDateInfo);
		}

		public void TestWhenPeriodApportionmentMethodIsNotDefault_GLAccountModuleHasAccountTypeBSHFilter()
		{
			var invoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), TestObjectCreator.AUD, 1m, TestObjectCreator.Creditor1);
			var invoiceLine = (InvoicingLineBase)invoice.Lines.AddNew();
			Assert("Precondition", invoiceLine.PeriodApportionmentLines.IsNullOrEmpty());
			invoiceLine.AL_ExchangeRate = 0.64m;
			invoiceLine.AL_OSExTaxAmount = 100m;
			invoiceLine.AL_RX_NKTransactionCurrency = "USD";
			Assert(invoiceLine.PeriodApportionmentLines.IsNullOrEmpty());

			invoiceLine.PeriodApportionmentMethod = PeriodApportionmentMethods.Codes.EquallyOverPeriods;

			var glAccount = invoiceLine.PeriodClearingGLAccountPK;
			AssertEquals(ZGuid.Empty, glAccount);

			var defaultList = invoiceLine.Lookups.BSHGLHeaders.FilterBusinessObjectDefaults.ToList<FilterBusinessObjectDefault>();

			CombineAssertions(() =>
			{
				AssertEquals(1, defaultList.Count);
				AssertEquals("Account Type", defaultList[0].FilterName);
				AssertEquals("Property", defaultList[0].PropertyName);
				AssertEquals(Constants.AccountType.BalanceSheetAccount, defaultList[0].Value);
			});
		}

		public void TestCheckComplianceDocumentNumber()
		{
			var helper = new AccountingPeriodTestHelper(Factory);
			helper.SetupSinglePeriod(201802, new ZDateTime(2018, 2, 1), new ZDateTime(2018, 2, 28));
			GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia);
			AccountingMasterFilesRegistry.Instance.EnableComplianceDocumentModule.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

			var apInv = TestObjectCreator.CreateAPInvoice<APInvoice>("INV001", TestObjectCreator.TWD, 1m, 100m, 10m, 0m, 100m, 10m, 0m, TestObjectCreator.AALSHI);
			var apInvLine = (APInvoiceLine)apInv.Lines.AddNew();
			apInvLine.AL_AG = TestObjectCreator.GLHeader1.PK;
			var invoiceDocumentHeader = TestObjectCreator.CreateComplianceDocumentHeaderWithLine(LedgerTypes.AccountsPayable, "desc", "TX00010002", TaiwanComplianceInfo.ComplianceSubTypeCodes.NTI, "desc", apInvLine);
			invoiceDocumentHeader.ADH_OH_Organisation = TestObjectCreator.AALSHI.PK;
			invoiceDocumentHeader.ADH_ReportingPeriod = 201802;
			invoiceDocumentHeader.ADH_DocumentDate = new ZDateTime(2018, 2, 2);

			var invoiceDocumentHeader2 = TestObjectCreator.CreateComplianceDocumentHeaderWithLine(LedgerTypes.AccountsPayable, "desc", "TX00010004", TaiwanComplianceInfo.ComplianceSubTypeCodes.NTI, "desc", apInvLine);
			invoiceDocumentHeader2.ADH_OH_Organisation = TestObjectCreator.AALSHI.PK;
			invoiceDocumentHeader2.ADH_ReportingPeriod = 201802;
			invoiceDocumentHeader2.ADH_DocumentDate = new ZDateTime(2018, 2, 2);
			TestObjectCreator.AALSHI.CompanyData.OB_APCreateVATComplianceDocumentOnPosting = "PCD";

			var apInv2 = TestObjectCreator.CreateAPInvoice<APInvoice>("INV002", TestObjectCreator.TWD, 1m, 100m, 10m, 0m, 100m, 10m, 0m, TestObjectCreator.AALSHI);
			var apInvLine2 = (APInvoiceLine)apInv2.Lines.AddNew();
			apInvLine2.AL_AG = TestObjectCreator.GLHeader1.PK;
			var invoiceDocumentHeader3 = TestObjectCreator.CreateComplianceDocumentHeaderWithLine(LedgerTypes.AccountsPayable, "desc", "TX00010005", TaiwanComplianceInfo.ComplianceSubTypeCodes.NTI, "desc", apInvLine2);
			invoiceDocumentHeader3.ADH_OH_Organisation = TestObjectCreator.ABIGAS.PK;
			invoiceDocumentHeader3.ADH_ReportingPeriod = 201802;
			invoiceDocumentHeader3.ADH_DocumentDate = new ZDateTime(2018, 2, 2);
			Factory.Save();

			var invoice = Factory.New<APInvoice>();
			invoice.AH_Ledger = LedgerTypes.AccountsPayable;
			invoice.AH_TransactionType = TransactionTypes.Invoice;
			invoice.AH_OH = TestObjectCreator.AALSHI.PK;
			var line = (APInvoiceLine)invoice.Lines.AddNew();
			line.CreateComplianceDocumentRecordOnPosting = true;
			line.ComplianceDocumentNumber = ZString.Empty;

			((APInvoiceLineValidation)line.Validation).ValidateComplianceDocumentNumber();
			AssertHasError(line.ComplianceDocumentNumberInfo, "Please enter a value.");

			line.ComplianceDocumentNumber = "xx";
			((APInvoiceLineValidation)line.Validation).ValidateComplianceDocumentNumber();
			AssertHasError(line.ComplianceDocumentNumberInfo, "The Tax ID should be entered before Compliance Document Number.");

			line.AL_AT = TestObjectCreator.GST1.PK;
			line.ComplianceDocumentNumber = "TX00010002";
			((APInvoiceLineValidation)line.Validation).ValidateComplianceDocumentNumber();
			AssertHasError(line.ComplianceDocumentNumberInfo, "This Compliance Document Number is already in use. Please enter another number.");

			line.ComplianceDocumentOrganization = TestObjectCreator.ABIGAS.PK;
			((APInvoiceLineValidation)line.Validation).ValidateComplianceDocumentNumber();
			AssertNoErrors(line.ComplianceDocumentNumberInfo);

			line.ComplianceDocumentOrganization = TestObjectCreator.AALSHI.PK;
			line.ComplianceDocumentNumber = "TX00010004";
			invoiceDocumentHeader2.ADH_DocumentStatus = "VOD";
			((APInvoiceLineValidation)line.Validation).ValidateComplianceDocumentNumber();
			AssertNoErrors(line.ComplianceDocumentNumberInfo);

			line.ComplianceDocumentNumber = "TX00010002";
			invoiceDocumentHeader.ADH_DocumentStatus = "SET";
			((APInvoiceLineValidation)line.Validation).ValidateComplianceDocumentNumber();
			AssertHasError(line.ComplianceDocumentNumberInfo, "This Compliance Document Number is already in use. Please enter another number.");

			line.ComplianceDocumentNumber = "TX00010003";
			((APInvoiceLineValidation)line.Validation).ValidateComplianceDocumentNumber();
			AssertNoErrors(line.ComplianceDocumentNumberInfo);

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Taiwan))
			{
				line.ComplianceDocumentNumber = "00";
				line.ComplianceSubType = "NTI";
				((APInvoiceLineValidation)line.Validation).ValidateComplianceDocumentNumber();
				AssertNoErrors(line.ComplianceDocumentNumberInfo);

				line.ComplianceSubType = "TXI";
				((APInvoiceLineValidation)line.Validation).ValidateComplianceDocumentNumber();
				AssertHasError(line.ComplianceDocumentNumberInfo, "Document Number must contains two alphabet prefix followed by eight numeric values. E.g. TX00001001");
				line.ComplianceDocumentNumber = "TX00001004";
				((APInvoiceLineValidation)line.Validation).ValidateComplianceDocumentNumber();
				AssertNoErrors(line.ComplianceDocumentNumberInfo);
			}
			line.Delete();
			invoice.Delete();

			var apCrd = TestObjectCreator.CreateAPCreditNote("CRD001", TestObjectCreator.AALSHI, TestObjectCreator.AUD, 1m, "desc");
			var apCrdLine = (APCreditNoteLine)apCrd.Lines.AddNew();
			apCrdLine.AL_AG = TestObjectCreator.GLHeader1.PK;

			var crdDocumentHeader = TestObjectCreator.CreateComplianceDocumentHeaderWithLine(LedgerTypes.AccountsPayable, "desc", "TX00010002", TaiwanComplianceInfo.ComplianceSubTypeCodes.NTI, "desc", apCrdLine);
			crdDocumentHeader.ADH_OH_Organisation = TestObjectCreator.AALSHI.PK;
			crdDocumentHeader.ADH_ReportingPeriod = 201802;
			crdDocumentHeader.ADH_DocumentDate = new ZDateTime(2018, 2, 2);
			Factory.Save();

			AccountingMasterFilesRegistry.Instance.CreditNoteComplianceDocumentConfiguration.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			var apCrd1 = TestObjectCreator.CreateAPCreditNote("CRD002", TestObjectCreator.AALSHI, TestObjectCreator.AUD, 1m, "desc");
			var apCrdLine1 = (APCreditNoteLine)apCrd1.Lines.AddNew();
			apCrdLine1.CreateComplianceDocumentRecordOnPosting = true;
			apCrdLine1.AL_AT = TestObjectCreator.GST1.PK;

			apCrdLine1.ComplianceDocumentNumber = "xx";
			((CreditNoteLineValidation)apCrdLine1.Validation).ValidateComplianceDocumentNumber();
			AssertHasError(apCrdLine1.ComplianceDocumentNumberInfo, "Document Number does not match any 'INV' Compliance Document recorded against the Creditor.");

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Taiwan))
			{
				apCrdLine1.ComplianceDocumentNumber = "TX00010005";
				((CreditNoteLineValidation)apCrdLine1.Validation).ValidateComplianceDocumentNumber();
				AssertNotEquals(invoiceDocumentHeader3.ADH_OH_Organisation, apCrd1.AH_OH);
				AssertHasError(apCrdLine1.ComplianceDocumentNumberInfo, "Document Number does not match any 'INV' Compliance Document recorded against the Creditor.");
			}

			apCrdLine1.ComplianceDocumentNumber = "TX00010002";
			((CreditNoteLineValidation)apCrdLine1.Validation).ValidateComplianceDocumentNumber();
			AssertEquals(invoiceDocumentHeader.ADH_OH_Organisation, apCrd1.AH_OH);
			AssertNoErrors(apCrdLine1.ComplianceDocumentNumberInfo);

			AccountingMasterFilesRegistry.Instance.CreditNoteComplianceDocumentConfiguration.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			apCrdLine1.ComplianceDocumentNumber = "TX00010002";
			((CreditNoteLineValidation)apCrdLine1.Validation).ValidateComplianceDocumentNumber();
			AssertHasError(apCrdLine1.ComplianceDocumentNumberInfo, "This Compliance Document Number is already in use. Please enter another number.");

			apCrdLine1.ComplianceDocumentOrganization = TestObjectCreator.ABIGAS.PK;
			((CreditNoteLineValidation)apCrdLine1.Validation).ValidateComplianceDocumentNumber();
			AssertNoErrors(apCrdLine1.ComplianceDocumentNumberInfo);

			AccountingMasterFilesRegistry.Instance.AllowNegativeComplianceDocumentLines.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			apCrdLine1.AL_OSExTaxAmount = -100;
			TestObjectCreator.AALSHI.CompanyData.OB_APCreateVATComplianceDocumentOnPosting = Constants.OrganisationCreateComplianceDocumentOnPostingTypes.PerComplianceDocumentNumber;
			apCrdLine1.AL_OH = TestObjectCreator.AALSHI.PK;
			((CreditNoteLineValidation)apCrdLine1.Validation).ValidateComplianceDocumentNumber();
			AssertHasError(apCrdLine1.ComplianceDocumentNumberInfo, ComplianceDocumentNegativeLinesMessage);

			AccountingMasterFilesRegistry.Instance.AllowNegativeComplianceDocumentLines.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			((CreditNoteLineValidation)apCrdLine1.Validation).ValidateComplianceDocumentNumber();
			AssertNoErrors(apCrdLine1.ComplianceDocumentNumberInfo);

			apCrd.AH_TransactionType = TransactionTypes.IncompleteCreditNote;
			AccountingMasterFilesRegistry.Instance.AllowNegativeComplianceDocumentLines.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			((CreditNoteLineValidation)apCrdLine1.Validation).ValidateComplianceDocumentNumber();
			AssertNoErrors(apCrdLine1.ComplianceDocumentNumberInfo);

			AccountingMasterFilesRegistry.Instance.AllowNegativeComplianceDocumentLines.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			((CreditNoteLineValidation)apCrdLine1.Validation).ValidateComplianceDocumentNumber();
			AssertHasError(apCrdLine1.ComplianceDocumentNumberInfo, ComplianceDocumentNegativeLinesMessage);

			apCrdLine1.AL_OSExTaxAmount = 100;
			apCrdLine1.CreateComplianceDocumentRecordOnPosting = false;
			((CreditNoteLineValidation)apCrdLine1.Validation).ValidateComplianceDocumentNumber();
			AssertNoErrors(apCrdLine1.ComplianceDocumentNumberInfo);
		}

		public void TestCheckRuleOfComplianceDocumentNumber()
		{
			var invoice = Factory.New<APInvoice>();
			invoice.AH_Ledger = LedgerTypes.AccountsPayable;
			invoice.AH_TransactionType = TransactionTypes.Invoice;
			invoice.AH_OH = TestObjectCreator.AALSHI.PK;
			var line = (APInvoiceLine)invoice.Lines.AddNew();
			line.CreateComplianceDocumentRecordOnPosting = true;
			line.ComplianceDocumentNumber = ZString.Empty;
			line.AL_AT = TestObjectCreator.GST1.PK;

			var errorMsg = "Document Number must contains two alphabet prefix followed by eight numeric values. E.g. TX00001001";
			var errorMsg1 = @"For compliance sub type TXE and TCE, the document number must be in one of the following formats:
1. 2 characters prefix followed by 8 numeric digits. E.g. TX00001001.
2. 2 characters 'BB' prefix followed by 8 alpha-numeric characters. E.g. BB12345678, BBTXIC2535.";
			var errorMsg2 = @"For compliance sub type TDC and TCD, the document number must be in one of the following formats:
1. 2 characters prefix followed by 8 numeric digits. E.g. TX00001001.
2. 10 alpha-numeric. E.g. A1G2345678, BDTXIC2535, 1234567890.";

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Taiwan))
			{
				var validation = (APInvoiceLineValidation)line.Validation;
				line.ComplianceDocumentNumber = "00";
				line.ComplianceSubType = "NTI";
				validation.ValidateComplianceDocumentNumber();
				AssertNoErrors(line.ComplianceDocumentNumberInfo);

				line.ComplianceSubType = "TXI";
				validation.ValidateComplianceDocumentNumber();
				AssertHasError(line.ComplianceDocumentNumberInfo, errorMsg);
				line.ComplianceDocumentNumber = "TX00001004";
				validation.ValidateComplianceDocumentNumber();
				AssertNoErrors(line.ComplianceDocumentNumberInfo);

				line.ComplianceSubType = TaiwanComplianceInfo.ComplianceSubTypeCodes.TXE;
				line.ComplianceDocumentNumber = "AAA0000001";
				validation.ValidateComplianceDocumentNumber();
				Assert(line.ComplianceDocumentNumberInfo.HasError(errorMsg1));

				line.ComplianceDocumentNumber = "AA12345678";
				validation.ValidateComplianceDocumentNumber();
				Assert(!line.ComplianceDocumentNumberInfo.HasError(errorMsg1));

				line.ComplianceDocumentNumber = "BBABCD1234";
				validation.ValidateComplianceDocumentNumber();
				Assert(!line.ComplianceDocumentNumberInfo.HasError(errorMsg1));

				line.ComplianceSubType = TaiwanComplianceInfo.ComplianceSubTypeCodes.TCE;
				line.ComplianceDocumentNumber = "AAA0000001";
				validation.ValidateComplianceDocumentNumber();
				Assert(line.ComplianceDocumentNumberInfo.HasError(errorMsg1));

				line.ComplianceDocumentNumber = "AA12345678";
				validation.ValidateComplianceDocumentNumber();
				Assert(!line.ComplianceDocumentNumberInfo.HasError(errorMsg1));

				line.ComplianceDocumentNumber = "BBABCD1234";
				validation.ValidateComplianceDocumentNumber();
				Assert(!line.ComplianceDocumentNumberInfo.HasError(errorMsg1));

				line.ComplianceSubType = TaiwanComplianceInfo.ComplianceSubTypeCodes.TDC;
				line.ComplianceDocumentNumber = "AAA00000011";
				validation.ValidateComplianceDocumentNumber();
				Assert(line.ComplianceDocumentNumberInfo.HasError(errorMsg2));

				line.ComplianceDocumentNumber = "A1B2C3D4E5";
				validation.ValidateComplianceDocumentNumber();
				Assert(!line.ComplianceDocumentNumberInfo.HasError(errorMsg2));

				line.ComplianceDocumentNumber = "AA12345678";
				validation.ValidateComplianceDocumentNumber();
				Assert(!line.ComplianceDocumentNumberInfo.HasError(errorMsg2));

				line.ComplianceSubType = TaiwanComplianceInfo.ComplianceSubTypeCodes.TCD;
				line.ComplianceDocumentNumber = "AAA00000011";
				validation.ValidateComplianceDocumentNumber();
				Assert(line.ComplianceDocumentNumberInfo.HasError(errorMsg2));

				line.ComplianceDocumentNumber = "A1B2C3D4E5";
				validation.ValidateComplianceDocumentNumber();
				Assert(!line.ComplianceDocumentNumberInfo.HasError(errorMsg2));

				line.ComplianceDocumentNumber = "AA12345678";
				validation.ValidateComplianceDocumentNumber();
				Assert(!line.ComplianceDocumentNumberInfo.HasError(errorMsg2));
			}
		}

		public void TestCheckComplianceSubType()
		{
			var invoice = Factory.New<APInvoice>();
			invoice.AH_Ledger = LedgerTypes.AccountsPayable;
			invoice.AH_TransactionType = TransactionTypes.Invoice;
			var line = (APInvoiceLine)invoice.Lines.AddNew();
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Taiwan))
			{
				AccountingMasterFilesRegistry.Instance.EnableComplianceDocumentModule.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
				TestObjectCreator.AALSHI.CompanyData.OB_APCreateVATComplianceDocumentOnPosting = "PCD";
				invoice.AH_OH = TestObjectCreator.AALSHI.PK;
				line.CreateComplianceDocumentRecordOnPosting = true;

				line.ComplianceSubType = ZString.Empty;
				((APInvoiceLineValidation)line.Validation).ValidateComplianceSubType();
				AssertHasError(line.ComplianceSubTypeInfo, "Please enter a value.");

				line.ComplianceSubType = "xx";
				((APInvoiceLineValidation)line.Validation).ValidateComplianceSubType();
				AssertHasError(line.ComplianceSubTypeInfo, "Enter a valid selection.");

				line.ComplianceSubType = "TXI";
				((APInvoiceLineValidation)line.Validation).ValidateComplianceSubType();
				AssertNoErrors(line.ComplianceSubTypeInfo);
			}
		}

		public void TestCheckComplianceDocumentVATRegistrationNum()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Taiwan))
			{
				AccountingMasterFilesRegistry.Instance.EnableComplianceDocumentModule.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
				var invoice = Factory.New<APInvoice>();
				TestObjectCreator.AALSHI.CompanyData.OB_APCreateVATComplianceDocumentOnPosting = "PCD";
				invoice.AH_OH = TestObjectCreator.AALSHI.PK;

				var line = (APInvoiceLine)invoice.Lines.AddNew();
				line.CreateComplianceDocumentRecordOnPosting = true;
				line.ComplianceDocumentVATRegistrationNum = ZString.Empty;
				((APInvoiceLineValidation)line.Validation).ValidateComplianceDocumentVATRegistrationNum();
				AssertHasError(line.ComplianceDocumentVATRegistrationNumInfo, "Please enter a VAT Registration Number.");

				line.ComplianceDocumentVATRegistrationNum = "1234567";
				((APInvoiceLineValidation)line.Validation).ValidateComplianceDocumentVATRegistrationNum();
				AssertHasError(line.ComplianceDocumentVATRegistrationNumInfo, "The Taiwan VAT number must be an 8-digit number.");

				line.ComplianceDocumentVATRegistrationNum = "123456ab";
				((APInvoiceLineValidation)line.Validation).ValidateComplianceDocumentVATRegistrationNum();
				AssertHasError(line.ComplianceDocumentVATRegistrationNumInfo, "The Taiwan VAT number must be an 8-digit number.");

				line.ComplianceDocumentVATRegistrationNum = "12345678";
				((APInvoiceLineValidation)line.Validation).ValidateComplianceDocumentVATRegistrationNum();
				AssertHasError(line.ComplianceDocumentVATRegistrationNumInfo, "The Taiwan VAT number is invalid.");

				line.ComplianceDocumentVATRegistrationNum = "87654321";
				((APInvoiceLineValidation)line.Validation).ValidateComplianceDocumentVATRegistrationNum();
				AssertHasError(line.ComplianceDocumentVATRegistrationNumInfo, "The Taiwan VAT number is invalid.");

				line.ComplianceDocumentVATRegistrationNum = "12345675";
				((APInvoiceLineValidation)line.Validation).ValidateComplianceDocumentVATRegistrationNum();
				AssertNoErrors(line.ComplianceDocumentVATRegistrationNumInfo);
			}
		}

		public void TestCheckComplianceDocumentDate()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Taiwan))
			{
				AccountingMasterFilesRegistry.Instance.EnableComplianceDocumentModule.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

				var invoice = Factory.New<APInvoice>();
				var line = (APInvoiceLine)invoice.Lines.AddNew();
				TestObjectCreator.AALSHI.CompanyData.OB_APCreateVATComplianceDocumentOnPosting = "PCD";
				invoice.AH_OH = TestObjectCreator.AALSHI.PK;
				line.CreateComplianceDocumentRecordOnPosting = true;

				line.ComplianceDocumentDate = ZDateTime.Empty;
				((APInvoiceLineValidation)line.Validation).ValidateComplianceDocumentDate();
				AssertHasError(line.ComplianceDocumentDateInfo, "Please enter a Document Date.");

				line.ComplianceDocumentDate = ZDateTime.Today.AddDays(1);
				((APInvoiceLineValidation)line.Validation).ValidateComplianceDocumentDate();
				AssertHasError(line.ComplianceDocumentDateInfo, "This date cannot be in the future.");

				line.ComplianceDocumentDate = ZDateTime.Today;
				((APInvoiceLineValidation)line.Validation).ValidateComplianceDocumentDate();
				AssertNoErrors(line.ComplianceDocumentDateInfo);
			}
		}

		public void TestCheckComplianceSupportingDocumentType()
		{
			var itemSet = (AccountingMasterFilesRegistry)Activator.CreateInstance(typeof(AccountingMasterFilesRegistry), true);
			var newlist = new ComplianceDocumentSupportingReasonCollection();
			newlist.Add(new ComplianceDocumentSupportingReason() { Code = "AAA", EnglishDescription = "test aaa" });
			itemSet.ComplianceDocumentSupportingDocumentType.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, newlist);
			Factory.Save();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Taiwan))
			{
				AccountingMasterFilesRegistry.Instance.EnableComplianceDocumentModule.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

				var invoice = Factory.New<APInvoice>();
				var line = (APInvoiceLine)invoice.Lines.AddNew();
				TestObjectCreator.AALSHI.CompanyData.OB_APCreateVATComplianceDocumentOnPosting = "PCD";
				invoice.AH_OH = TestObjectCreator.AALSHI.PK;
				line.CreateComplianceDocumentRecordOnPosting = true;

				line.ComplianceSupportingDocumentType = "xx";
				((APInvoiceLineValidation)line.Validation).ValidateComplianceSupportingDocumentType();
				AssertHasError(line.ComplianceSupportingDocumentTypeInfo, "Enter a valid Supporting Doc Type.");

				line.ComplianceSupportingDocumentType = "AAA";
				((APInvoiceLineValidation)line.Validation).ValidateComplianceSupportingDocumentType();
				AssertNoErrors(line.ComplianceSupportingDocumentTypeInfo);
			}
		}

		public void TestCheckComplianceDocumentSupportingReason()
		{
			var itemSet = (AccountingMasterFilesRegistry)Activator.CreateInstance(typeof(AccountingMasterFilesRegistry), true);
			var newReasonlist = new ComplianceDocumentSupportingReasonCollection();
			newReasonlist.Add(new ComplianceDocumentSupportingReason() { Code = "R1", EnglishDescription = "test reason a" });
			itemSet.ComplianceDocumentSupportingReasonsPayables.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, newReasonlist);
			Factory.Save();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Taiwan))
			{
				AccountingMasterFilesRegistry.Instance.EnableComplianceDocumentModule.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

				var invoice = Factory.New<APInvoice>();
				var line = (APInvoiceLine)invoice.Lines.AddNew();
				TestObjectCreator.AALSHI.CompanyData.OB_APCreateVATComplianceDocumentOnPosting = "PCD";
				invoice.AH_OH = TestObjectCreator.AALSHI.PK;
				line.CreateComplianceDocumentRecordOnPosting = true;

				line.ComplianceDocumentSupportingReason = "xx";
				((APInvoiceLineValidation)line.Validation).ValidateComplianceDocumentSupportingReason();
				AssertHasError(line.ComplianceDocumentSupportingReasonInfo, "Enter a valid Supporting Reason.");

				line.ComplianceDocumentSupportingReason = "R1";
				((APInvoiceLineValidation)line.Validation).ValidateComplianceDocumentSupportingReason();
				AssertNoErrors(line.ComplianceDocumentSupportingReasonInfo);
			}
		}

		[TestDate(2018, 3, 7)]
		public void TestCheckComplianceDocumentReportingPeriod()
		{
			var helper = new AccountingPeriodTestHelper(Factory);
			var period1 = helper.SetupSinglePeriod(201802, new ZDateTime(2018, 2, 1), new ZDateTime(2018, 2, 28));
			period1.AM_IsSubLedgerClosed = true;
			var period2 = helper.SetupSinglePeriod(201803, new ZDateTime(2018, 3, 1), new ZDateTime(2018, 3, 31));
			var period3 = helper.SetupSinglePeriod(201804, new ZDateTime(2018, 4, 1), new ZDateTime(2018, 4, 30));
			Factory.Save();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Taiwan))
			{
				var complianceConfig = AccountingMasterFilesRegistry.Instance.ComplianceReportConfiguration.Value;
				var reportConfig = complianceConfig.AddNew();
				reportConfig.ReportCode = "TST";
				reportConfig.ReportTitle = "Test Tax Report";
				reportConfig.ReportPeriodicity = ComplianceReportConfigurationLookups.ReportPeriodicityCodes.DateRange;
				reportConfig.Country = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
				reportConfig.TaxRegistrationType = "APC";
				reportConfig.ReportBaseTablePrefix = ReportBaseTablePrefixListCodes.ComplianceDocumentHeader;

				var setting1 = reportConfig.Settings.AddNew();
				setting1.ComplianceSubType = "TCR";
				setting1.LedgerType = LedgerTypes.AccountsPayable;

				AccountingMasterFilesRegistry.Instance.ComplianceReportConfiguration.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, complianceConfig);

				var report = Factory.New<AccComplianceReport>();
				report.ACR_ReportType = "TST";
				report.ACR_DateFrom = new ZDate(2018, 3, 1);
				report.ACR_DateTo = new ZDate(2018, 3, 31);
				Factory.Save();

				AccountingMasterFilesRegistry.Instance.EnableComplianceDocumentModule.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

				var invoice = Factory.New<APInvoice>();
				var line = (APInvoiceLine)invoice.Lines.AddNew();
				TestObjectCreator.AALSHI.CompanyData.OB_APCreateVATComplianceDocumentOnPosting = "PCD";
				invoice.AH_OH = TestObjectCreator.AALSHI.PK;
				line.CreateComplianceDocumentRecordOnPosting = true;

				report.ACR_Status = AccComplianceReport.Status.ReportFinalised;
				line.ComplianceSubType = "TCR";
				line.ComplianceDocumentReportingPeriod = 201803;
				((APInvoiceLineValidation)line.Validation).ValidateComplianceDocumentReportingPeriod();
				AssertHasError(line.ComplianceDocumentReportingPeriodInfo, "The reporting period falls in a compliance report that has been finalized.");

				report.ACR_Status = AccComplianceReport.Status.ReportCreated;
				line.ComplianceDocumentReportingPeriod = ZInt.Zero;
				((APInvoiceLineValidation)line.Validation).ValidateComplianceDocumentReportingPeriod();
				AssertHasError(line.ComplianceDocumentReportingPeriodInfo, "Please enter a Reporting Period.");

				line.ComplianceDocumentReportingPeriod = 201805;
				((APInvoiceLineValidation)line.Validation).ValidateComplianceDocumentReportingPeriod();
				AssertHasError(line.ComplianceDocumentReportingPeriodInfo, AccountingPeriodCalculator.GetInvalidPeriodValidationError(201805));

				line.ComplianceDocumentReportingPeriod = 201802;
				((APInvoiceLineValidation)line.Validation).ValidateComplianceDocumentReportingPeriod();
				AssertHasError(line.ComplianceDocumentReportingPeriodInfo, "This date falls into a period where the sub-ledger is closed");

				line.ComplianceDocumentReportingPeriod = 201804;
				((APInvoiceLineValidation)line.Validation).ValidateComplianceDocumentReportingPeriod();
				AssertHasError(line.ComplianceDocumentReportingPeriodInfo, "This period cannot be in the future.");

				line.ComplianceDocumentReportingPeriod = 201803;
				((APInvoiceLineValidation)line.Validation).ValidateComplianceDocumentReportingPeriod();
				AssertNoErrors(line.ComplianceDocumentReportingPeriodInfo);
			}
		}

		public void TestAL_TaxDateValidationReportJobOperationalDateMissingWhenDefaulting()
		{
			if (InvoiceType == typeof(APInvoice) || InvoiceType == typeof(APCreditNote))
			{
				var collection = new TaxDateDefaultingOptionCollection();
				var taxDateOption1 = collection.AddNew();
				taxDateOption1.JobType = "FCN";
				taxDateOption1.DirectionCode = "ALL";
				taxDateOption1.Mode = "ALL";
				taxDateOption1.Ledger = "AP";
				taxDateOption1.TaxDateOption = TaxDateDefaultingOption.Code.InvoiceDate;

				var taxDateOption2 = collection.AddNew();
				taxDateOption2.JobType = "SHP";
				taxDateOption2.DirectionCode = "ALL";
				taxDateOption2.Mode = "ALL";
				taxDateOption2.Ledger = "AP";
				taxDateOption2.TaxDateOption = TaxDateDefaultingOption.Code.ArrivalDate;

				using (AccountingConfigurationRegistry.Instance.TaxDateDefaultingOption.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, collection))
				{
					var shipment1 = TestObjectCreator.CreateShipment("S001001");
					var job1 = new Job.Loader(shipment1).TryCreateWithoutMutexForTestOnly();
					Factory.Save();

					var shipment2 = TestObjectCreator.CreateShipment("S001002");
					var job2 = new Job.Loader(shipment2).TryCreateWithoutMutexForTestOnly();
					shipment2.JS_E_ARV = ZDateTime.Today;
					Factory.Save();

					var expectedErrorMessage = @"Tax Date configuration for this job type requires Actual/Estimated Arrival Date date, which has not been entered on the job.
This date needs to be added to the job before costs to this job can be posted.";
					var chargeCode = TestObjectCreator.CreateChargeCode("ABC");
					var invoice = (InvoicingBase)Factory.New(InvoiceType);
					invoice.AH_OH = GSTRegisteredOrg.PK;
					var line = (InvoicingLineBase)invoice.Lines.AddNew();
					line.AL_AC = chargeCode.PK;
					line.AL_JH = job1.PK;
					AssertHasError("AL_TaxDate should report error.", line.AL_TaxDateInfo, expectedErrorMessage);

					line.AL_JH = job2.PK;
					AssertNoError("AL_TaxDate should NOT report error.", line.AL_TaxDateInfo, expectedErrorMessage);

					var invoice2 = (InvoicingBase)Factory.New(InvoiceType);
					var line2 = (InvoicingLineBase)invoice2.Lines.AddNew();
					var consol = TestObjectCreator.CreateConsol("AUSYD", "NZAKL", "C001");
					var shipment3 = TestObjectCreator.CreateShipment("S003", consol);
					shipment3.JS_E_ARV = ZDate.Empty;
					var job3 = new Job.Loader(shipment3).TryCreateWithoutMutexForTestOnly();
					var consolCost = TestObjectCreator.CreateConsolCost(invoice2, consol, TestObjectCreator.CC1, 10);
					invoice2.ImportSingleCost(consolCost, line2);
					Assert("Precondition: ConsolIDFromApportionedCharge is not empty", !line2.ConsolIDFromApportionedCharge.IsEmpty);
					invoice2.AH_OH = GSTRegisteredOrg.PK;
					line2.AL_TaxDate = ZDate.Empty;
					AssertNoError("AL_TaxDate should NOT report error since it's not linked to shipment", line2.AL_TaxDateInfo, expectedErrorMessage);
				}
			}
			else
			{
				Assert("Only applicable to AP Invoice, AP Credit Note", true);
			}
		}

		public void TestAL_TaxDateValidationIsNotPerformedForBadDebtWriteOffs()
		{
			var expectedErrorMessage = "Please enter a Tax Date.";
			var chargeCode = TestObjectCreator.CreateChargeCode("ABC");
			var invoice = (InvoicingBase)Factory.New(InvoiceType);
			invoice.AH_OH = GSTRegisteredOrg.PK;
			var line = (InvoicingLineBase)invoice.Lines.AddNew();
			line.AL_AC = chargeCode.PK;
			line.AL_AT = GST10TaxRate.PK;
			line.AL_OSExTaxAmount = 100m;
			line.AL_OSTaxAmount = 10m;

			Assert("Invoice is NOT a bad debt write off.", !invoice.IsBadDebtWritingOff);
			AssertNoError("AL_TaxDate has NO error.", line.AL_TaxDateInfo, expectedErrorMessage);
			line.AL_TaxDate = ZDate.Empty;
			AssertHasError("AL_TaxDate should have error, because invoice is NOT a bad debt write off.", line.AL_TaxDateInfo, expectedErrorMessage);

			if (invoice is IBadDebtWritingOff badDebt)
			{
				line.AL_TaxDate = ZDate.Today;
				TestObjectCreator.WriteOffBadDebt(badDebt, out string canNotReverseReason);
				Assert("Now, invoice is a bad debt write off.", invoice.IsBadDebtWritingOff);
				AssertNoError("AL_TaxDate has NO error.", line.AL_TaxDateInfo, expectedErrorMessage);
				line.AL_TaxDate = ZDate.Empty;
				AssertNoError("AL_TaxDate should NOT have error, because invoice is a bad debt write off.", line.AL_TaxDateInfo, expectedErrorMessage);
			}
		}

		public void TestValidateAL_DescWithEnableLocalChargeCodeDescriptionDefaultRegsitry()
		{
			if (InvoiceType == typeof(ARInvoice) || InvoiceType == typeof(ARCreditNote) || InvoiceType == typeof(ARAdjustmentNote))
			{
				var expectedWarningMessage = "Charge description was changed from default. This description will appear on AR Invoice without translation.";
				var chargeCode = TestObjectCreator.CreateChargeCode("ABC");
				chargeCode.AC_Desc = "My Test Charge Code";

				var invoice = (InvoicingBase)Factory.New(InvoiceType);
				var line = (InvoicingLineBase)invoice.Lines.AddNew();
				line.AL_AC = chargeCode.PK;

				using (AccountingConfigurationRegistry.Instance.EnableLocalChargeCodeDescriptionDefault.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
				{
					AssertEquals(chargeCode.AC_Desc, line.AL_Desc);
					Assert(line.AL_Desc.StartsWith(chargeCode.AC_Desc));
					AssertNoWarning(line.AL_DescInfo, expectedWarningMessage);

					line.AL_Desc = "My Test Charge Code and some appended text";
					Assert(line.AL_Desc.StartsWith(chargeCode.AC_Desc));
					AssertNoWarning(line.AL_DescInfo, expectedWarningMessage);

					line.AL_Desc = "This is a very different charge code description";
					Assert(!line.AL_Desc.StartsWith(chargeCode.AC_Desc));
					AssertHasWarning(line.AL_DescInfo, expectedWarningMessage);
				}

				using (AccountingConfigurationRegistry.Instance.EnableLocalChargeCodeDescriptionDefault.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				{
					line.AL_Desc = "My Test Charge Code";
					AssertEquals(chargeCode.AC_Desc, line.AL_Desc);
					Assert(line.AL_Desc.StartsWith(chargeCode.AC_Desc));
					AssertNoWarning(line.AL_DescInfo, expectedWarningMessage);

					line.AL_Desc = "My Test Charge Code and some appended text";
					Assert(line.AL_Desc.StartsWith(chargeCode.AC_Desc));
					AssertNoWarning(line.AL_DescInfo, expectedWarningMessage);

					line.AL_Desc = "This is a very different charge code description";
					Assert(!line.AL_Desc.StartsWith(chargeCode.AC_Desc));
					AssertNoWarning(line.AL_DescInfo, expectedWarningMessage);
				}
			}
			else
			{
				Assert("Only applicable to AR Invoice, AR Credit Note and AR Adjustment Note", true);
			}
		}

		public void TestCheckGenericCharge_ImportedXMLValues()
		{
			if (InvoiceType != typeof(APInvoice) && InvoiceType != typeof(APCreditNote))
			{
				Assert(true);
				return;
			}

			var isInvoice = InvoiceType == typeof(APInvoice);
			var multiplier = isInvoice ? 1 : -1;

			var orgAddress = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance);
			orgAddress.AddressType = nameof(DocAddressType.None);
			orgAddress.Address1 = "Street";
			orgAddress.OrganizationCode = "ZCreditor2";

			var universalTransaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			universalTransaction.Ledger = LedgerTypes.AccountsPayable;
			universalTransaction.OrganizationAddress = orgAddress;
			universalTransaction.SetPostingJournalCollection(() => new List<PostingJournal>());

			var chargeCode = new ChargeCode();
			var expectedChargeCode = "DDD";
			chargeCode.Code = expectedChargeCode;
			var universalLine1 = new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance);
			universalLine1.ChargeCode = chargeCode;
			universalTransaction.PostingJournalCollection.Add(universalLine1);

			var invoice = TestObjectCreator.CreateAndAllocateInvoiceWithUniversalTransactionInAllocationApprovalRequest(InvoiceType, universalTransaction, false);
			var line = (InvoicingLineBase)invoice.Lines.AddNew();
			line.IndexOfImportedUniversalTransactionLine = 0;
			AssertEquals("Precondition: ImportedChargeCodeXmlCode", expectedChargeCode, line.ImportedChargeCodeXmlCode);
			AssertEquals("Precondition: ImportedChargeCode", ZString.Empty, line.ImportedChargeCode);

			var validation = new InvoicingLineBaseValidation(line);
			validation.ValidateGenericCharge();
			AssertNoWarnings(line.GenericChargeInfo);

			line.GenericCharge = TestObjectCreator.CC1.PK;
			AssertHasWarning(line.GenericChargeInfo, "Charge code is not found for imported XML code 'DDD'. New matching rule will be created for foreign code 'DDD'.");

			var currentCompnay = GlbCompany.GetCurrentCompany(new BusinessObjectFactory());
			OrgPatternMatchOverride patternMatchOverride = TestObjectCreator.AddMatchingRuleForChargeCode(currentCompnay.OrgProxy, universalLine1.ChargeCode.Code.Value, TestObjectCreator.FRT);
			patternMatchOverride.Factory.Save();

			invoice.AllocationApprovalRequest.PostingDetails.SourceXML = " " + invoice.AllocationApprovalRequest.PostingDetails.SourceXML; //to run UpdateUniversalTransaction to remap xml codes 
			AssertEquals("Precondition: ImportedChargeCodeXmlCode", expectedChargeCode, line.ImportedChargeCodeXmlCode);
			AssertEquals("Precondition: ImportedChargeCode", "FRT", line.ImportedChargeCode);
			line.GenericCharge = TestObjectCreator.FRT.PK;
			AssertNoWarnings(line.GenericChargeInfo);
			line.GenericCharge = TestObjectCreator.CC1.PK;
			var ruleUpdateMessage = "Charge code value is different to a value matched by default. Charge code matching rule for foreign code 'DDD' will be updated.";
			AssertHasWarning(line.GenericChargeInfo, ruleUpdateMessage);

			universalLine1.ChargeCode = null;
			invoice.AllocationApprovalRequest.PostingDetails.SourceXML = universalTransaction.Serialize();
			AssertEquals("Precondition: ImportedChargeCodeXmlCode", ZString.Empty, line.ImportedChargeCodeXmlCode);
			AssertEquals("Precondition: ImportedChargeCode", ZString.Empty, line.ImportedChargeCode);
			validation.ValidateGenericCharge();
			AssertNoWarnings(line.GenericChargeInfo);

			var universalLine2 = new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance);
			universalLine2.ChargeCode = new ChargeCode { Code = "SSS" };
			universalLine1.ChargeCode = chargeCode;
			universalTransaction.PostingJournalCollection.Add(universalLine2);
			invoice.AllocationApprovalRequest.PostingDetails.SourceXML = universalTransaction.Serialize();
			var line2 = (InvoicingLineBase)invoice.Lines.AddNew();
			AssertEquals("Precondition: line 1 ImportedChargeCodeXmlCode", expectedChargeCode, line.ImportedChargeCodeXmlCode);
			AssertEquals("Precondition: line 1 ImportedChargeCode", "FRT", line.ImportedChargeCode);
			AssertEquals("Precondition: line 2 ImportedChargeCodeXmlCode", ZString.Empty, line2.ImportedChargeCodeXmlCode);
			AssertEquals("Precondition: line 2 ImportedChargeCode", "", line2.ImportedChargeCode);
			line2.GenericCharge = TestObjectCreator.FRT.PK;
			validation.ValidateGenericCharge();
			AssertHasWarning(line.GenericChargeInfo, ruleUpdateMessage);

			line2.IndexOfImportedUniversalTransactionLine = 1;
			AssertNotEquals("Precondition: line 2 ImportedChargeCodeXmlCode", ZString.Empty, line2.ImportedChargeCodeXmlCode);
			AssertNotEquals("Precondition: line 2 ImportedChargeCodeXmlCode", expectedChargeCode, line2.ImportedChargeCodeXmlCode);
			AssertEquals("Precondition: line 2 ImportedChargeCode", "", line2.ImportedChargeCode);
			validation.ValidateGenericCharge();
			AssertHasWarning(line.GenericChargeInfo, ruleUpdateMessage);

			universalLine2.ChargeCode.Code = chargeCode.Code;
			invoice.AllocationApprovalRequest.PostingDetails.SourceXML = universalTransaction.Serialize();
			AssertEquals("Precondition: line 1 ImportedChargeCodeXmlCode", expectedChargeCode, line.ImportedChargeCodeXmlCode);
			AssertEquals("Precondition: line 1 ImportedChargeCode", "FRT", line.ImportedChargeCode);
			AssertEquals("Precondition: line 2 ImportedChargeCodeXmlCode", expectedChargeCode, line2.ImportedChargeCodeXmlCode);
			AssertEquals("Precondition: line 2 ImportedChargeCode", "FRT", line2.ImportedChargeCode);
			validation.ValidateGenericCharge();
			var skipRuleUpdateMessage = $"Charge code value is different to a value matched by default. However it also different to values in lines with the same charge code value in the imported XML. Charge code matching rule for foreign code '{expectedChargeCode}' won’t be updated.";
			AssertHasWarning(line.GenericChargeInfo, skipRuleUpdateMessage);

			line2.GenericCharge = TestObjectCreator.CC1.PK;
			validation.ValidateGenericCharge();
			AssertHasWarning(line.GenericChargeInfo, ruleUpdateMessage);

			line2.GenericCharge = ZGuid.Empty;
			validation.ValidateGenericCharge();
			AssertHasWarning(line.GenericChargeInfo, ruleUpdateMessage);

			line2.GenericCharge = TestObjectCreator.FRT.PK;
			line.GenericCharge = ZGuid.Empty;
			AssertNoWarnings(line.GenericChargeInfo);

			line.GenericCharge = TestObjectCreator.CC1.PK;
			AssertHasWarning(line.GenericChargeInfo, skipRuleUpdateMessage);

			patternMatchOverride.Delete();
			patternMatchOverride.Factory.Save();
			invoice.AllocationApprovalRequest.PostingDetails.SourceXML = " " + universalTransaction.Serialize(); //to run UpdateUniversalTransaction to remap xml codes 
			AssertEquals("Precondition: line 1 ImportedChargeCodeXmlCode", expectedChargeCode, line.ImportedChargeCodeXmlCode);
			AssertEquals("Precondition: line 1 ImportedChargeCode", "", line.ImportedChargeCode);
			AssertEquals("Precondition: line 2 ImportedChargeCodeXmlCode", expectedChargeCode, line2.ImportedChargeCodeXmlCode);
			AssertEquals("Precondition: line 2 ImportedChargeCode", "", line2.ImportedChargeCode);
			invoice.RunPreSaveValidation();
			var skipRuleCreationMessage = $"Charge code is not found for imported XML code '{expectedChargeCode}'. However it also different to values in lines with the same charge code value in the imported XML. Charge code matching rule for foreign code '{expectedChargeCode}' won’t be created.";
			AssertHasWarning(line.GenericChargeInfo, skipRuleCreationMessage);
			AssertHasWarning(line2.GenericChargeInfo, skipRuleCreationMessage);
		}

		public void TestDepartmentValidation_MiscellaneousDepartment()
		{
			var miscDepartment = TestObjectCreator.MiscDepartment;
			var nonMiscDepartment = NonMiscDepartment;
			var shipment = TestObjectCreator.CreateShipment("S001001");
			var job = new Job.Loader(shipment).TryCreateWithoutMutexForTestOnly();
			job.JH_GE = NonMiscDepartment.PK;
			Factory.Save();

			InvoicingBase invoice = (InvoicingBase)Factory.NewWithValidTestData(InvoiceType);
			InvoicingLineBase line1 = (InvoicingLineBase)invoice.Lines.AddNew();
			line1.AL_JH = job.PK;
			line1.AL_GE = TestObjectCreator.MiscDepartment.PK;
			line1.Validation.ValidateAL_GE();
			Assert("Department should have error", line1.AL_GEInfo.HasErrors());
			Assert("Department Error", line1.AL_GEInfo.HasError("Cannot issue job charges for a miscellaneous department."));

			line1.OriginalJobCharge = Factory.NewWithValidTestData<JobCharge>();
			line1.Validation.ValidateAL_GE();
			Assert("Department should not have error", !line1.AL_GEInfo.HasErrors());

			line1.OriginalJobCharge = null;
			line1.AL_JH = ZGuid.Empty;
			line1.Validation.ValidateAL_GE();
			Assert("Department should not have error", !line1.AL_GEInfo.HasErrors());

			line1.AL_JH = job.PK;
			line1.AL_GE = NonMiscDepartment.PK;
			line1.Validation.ValidateAL_GE();
			Assert("Department should not have error", !line1.AL_GEInfo.HasErrors());

			line1.AL_JH = ZGuid.Empty;
			line1.AL_GE = TestObjectCreator.MiscDepartment.PK;
			line1.AL_JH = job.PK;
			line1.Validation.ValidateAL_GE();
			AssertNoErrors("Department should not have error", line1.AL_GEInfo);

			line1.AL_JH = ZGuid.Empty;
			line1.AL_GE = NonMiscDepartment.PK;
			line1.AL_JH = job.PK;
			line1.AL_GE = TestObjectCreator.MiscDepartment.PK;
			line1.Validation.ValidateAL_GE();
			Assert("Department should have error", line1.AL_GEInfo.HasErrors());
			Assert("Department Error", line1.AL_GEInfo.HasError("Cannot issue job charges for a miscellaneous department."));
		}

		public void TestDepartmentValidationBasedOnJobDepartment()
		{
			var shipment = TestObjectCreator.CreateShipment("S001001");
			Job testJob = Factory.NewJobWithValidTestDataForTesting<Job>();
			testJob.JH_ParentID = shipment.PK;
			testJob.JH_ParentTableCode = "JS";
			testJob.JH_GE = TestObjectCreator.MiscDepartment.PK;
			Factory.Save();

			GenericJob.GenericJob job = testJob.GenericJobView;

			InvoicingBase invoice = (InvoicingBase)Factory.NewWithValidTestData(InvoiceType);
			InvoicingLineBase line1 = (InvoicingLineBase)invoice.Lines.AddNew();
			line1.AL_AG = TestObjectCreator.GLHeader1.PK;
			line1.AL_JH = job.PK;
			line1.Validation.ValidateAL_GE();
			Assert("Department Error", line1.AL_GEInfo.HasError("Cannot issue job charges for a miscellaneous department."));

			line1.AL_JH = ZGuid.Empty;
			Factory.Save();
			line1.AL_JH = job.PK;
			line1.Validation.ValidateAL_GE();
			Assert("Department should not have error", !line1.AL_GEInfo.HasErrors());
		}

		public void TestCheckEmptyStates()
		{
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.India;
			var branchOrgProxy = Factory.NewWithValidTestData<OrgHeader>();
			var companyOrgProxy = Factory.NewWithValidTestData<OrgHeader>();

			branch.GB_GC = company.PK;
			company.GC_OH_OrgProxy = companyOrgProxy.PK;

			var branchAddress = branchOrgProxy.Addresses[0];
			branchAddress.AddressCapability.SetIsMainAddress(OrgConstants.AddressType.Office);
			branchAddress.State = "AAA";
			var companyAddress = companyOrgProxy.Addresses[0];
			companyAddress.State = "";
			companyOrgProxy.Addresses.Add(companyAddress);

			Factory.Save();

			using (Env.SetTemporaryUserContext(new UserContext(GlbStaff.CurrentUser.PK.ToGuid(), branch.PK.ToGuid(), Env.CurrentDepartment.PK)))
			using (AccountingMasterFilesRegistry.Instance.EnableBranchLevelTaxOverrideRuleConfigurations.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var shipment = TestObjectCreator.CreateShipment("S00001234");
				var job = TestObjectCreator.CreateJob(shipment, false);
				var testOrg = TestObjectCreator.CreateOrgHeader("TestOrg", true, true, true, true, true, true);
				Factory.Save();

				var invoice = (InvoicingBase)Factory.New(InvoiceType);
				invoice.AH_OH = testOrg.PK;
				var line = (InvoicingLineBase)invoice.Lines.AddNew();

				//Line Branch
				line.AL_JH = job.PK;
				line.AL_AC = TestObjectCreator.FRT.PK;
				line.AL_GB = branch.PK;
				AssertHasRowError("Line Branch org proxy has an empty state", line, IndiaCompanyEmptyStateErrorMessages.EmptyStateErrorMessageForLineBranch);
				branch.GB_OH_OrgProxy = branchOrgProxy.PK;
				line.Validation.ValidateAL_GB();
				AssertNoRowErrors(line);

				//Origin
				var origin = Factory.NewWithValidTestData<RefUNLOCO>();
				var state1 = Factory.NewWithValidTestData<RefCountryStates>();
				state1.RW_Code = "";
				origin.RL_RW = state1.PK;
				shipment.JS_RL_NKOrigin = origin.RL_Code;
				line.Validation.ValidateAL_GB();
				AssertHasRowWarning("Origin has an empty state", line, IndiaCompanyEmptyStateErrorMessages.EmptyStateWarningMessageForOrigin);
				shipment.Origin.CountryStates.RW_Code = "123";
				line.Validation.ValidateAL_GB();
				AssertNoRowWarnings(line);

				//Destination
				var destination = Factory.NewWithValidTestData<RefUNLOCO>();
				var state2 = Factory.NewWithValidTestData<RefCountryStates>();
				state2.RW_Code = "";
				destination.RL_RW = state2.PK;
				shipment.JS_RL_NKDestination = destination.RL_Code;
				line.Validation.ValidateAL_AC();
				AssertHasRowWarning("Destination has an empty state", line, IndiaCompanyEmptyStateErrorMessages.EmptyStateWarningMessageForDestination);
				shipment.Destination.CountryStates.RW_Code = "123";
				line.Validation.ValidateAL_AC();
				AssertNoRowWarnings(line);

				//Organization
				invoice.Header.MainAddress.OA_State = "";
				line.Validation.ValidateAL_JH();
				AssertHasRowWarning("Organization has an empty state", line, IndiaCompanyEmptyStateErrorMessages.EmptyStateWarningMessageForOrganization);
				invoice.Header.MainAddress.OA_State = "123";
				line.Validation.ValidateAL_JH();
				AssertNoRowWarnings(line);
			}
		}

		protected GlbDepartment NonMiscDepartment
		{
			get { return Factory.LoadTop1<GlbDepartment>(new ZQuery(GlbDepartmentSchema.GE_Misc, false)); }
		}

		[SuspendCriticalValidation]
		public void TestAL_SequenceIsUniqueInCollectionWhenNotInDatabase()
		{
			TestObjectCreator creator = new TestObjectCreator(Factory);
			AccChargeCode code = creator.CC1;
			Factory.Save();

			InvoicingBase invoice = (InvoicingBase)Factory.NewWithValidTestData(InvoiceType);

			invoice.AH_OH = creator.AALSHI.PK;
			InvoicingLineBase line1 = (InvoicingLineBase)invoice.Lines.AddNew();
			line1.GenericCharge = code.PK;
			line1.AL_OSExTaxAmount = 10m;
			line1.AL_Sequence = 1;

			InvoicingLineBase line2 = (InvoicingLineBase)invoice.Lines.AddNew();
			line2.GenericCharge = code.PK;
			line2.AL_OSExTaxAmount = 10m;
			line2.AL_Sequence = 1;

			line1.AL_JH = creator.Job1.PK;
			line1.Validation.ValidateAL_Sequence();
			line2.Validation.ValidateAL_Sequence();

			AssertEquals(false, line1.AL_SequenceInfo.HasErrors());
			AssertEquals(false, line2.AL_SequenceInfo.HasErrors());

			line1.AL_JH = ZGuid.Empty;
			line1.Validation.ValidateAL_Sequence();
			line2.Validation.ValidateAL_Sequence();

			AssertEquals(false, line1.AL_SequenceInfo.HasErrors());
			AssertEquals(false, line2.AL_SequenceInfo.HasErrors());

			invoice.InitializeDuplicateLinesSequenceLookup();
			line1.Validation.ValidateAL_Sequence();
			line2.Validation.ValidateAL_Sequence();

			AssertEquals(true, line1.AL_SequenceInfo.HasErrors());
			AssertEquals(true, line2.AL_SequenceInfo.HasErrors());

			Factory.Save();
			if (invoice.AH_Ledger == LedgerTypes.UnapprovedPayableTransactions)
			{
				invoice.AH_Ledger = LedgerTypes.AccountsPayable;
			}
			invoice.RunPreSaveValidation();
			if (!line1.AL_Sequence_ReadOnly)
			{
				Assert("Should be an error on AL_Sequence because it is editable", line1.AL_SequenceInfo.HasErrors());
				Assert("Should be an error on AL_Sequence because it is editable", line2.AL_SequenceInfo.HasErrors());
			}
			else
			{
				Assert("Should no longer be an error on AL_Sequence because line is in database and is ReadOnly", !line1.AL_SequenceInfo.HasErrors());
				Assert("Should no longer be an error on AL_Sequence because line is in database and is ReadOnly", !line2.AL_SequenceInfo.HasErrors());
			}
		}

		[SuspendCriticalValidation]
		public void TestAL_SequenceIsCantBeNegativeWhenNotInDatabase()
		{
			TestObjectCreator creator = new TestObjectCreator(Factory);
			AccChargeCode code = creator.CC1;
			Factory.Save();

			InvoicingBase invoice = (InvoicingBase)Factory.NewWithValidTestData(InvoiceType);

			invoice.AH_OH = creator.AALSHI.PK;
			InvoicingLineBase line1 = (InvoicingLineBase)invoice.Lines.AddNew();
			line1.GenericCharge = code.PK;
			line1.AL_OSExTaxAmount = 10m;
			line1.AL_Sequence = -1;
			Assert("Should be an error on AL_Sequence", line1.AL_SequenceInfo.HasErrors());

			Factory.Save();
			if (invoice.AH_Ledger == LedgerTypes.UnapprovedPayableTransactions)
			{
				invoice.AH_Ledger = LedgerTypes.AccountsPayable;
			}
			line1.Validation.ValidateAL_Sequence();
			if (!line1.AL_Sequence_ReadOnly)
			{
				Assert("Should be an error on AL_Sequence because it is editable", line1.AL_SequenceInfo.HasErrors());
			}
			else
			{
				Assert("Should no longer be an error on AL_Sequence because line is in database and is ReadOnly", !line1.AL_SequenceInfo.HasErrors());
			}
		}

		[TestDate(2018, 6, 20)]
		public void TestCheckAL_ExchangeRate()
		{
			InvoicingBase invoice = (InvoicingBase)Factory.New(InvoiceType);
			var postingExRateRegistry = invoice.GetExRateLedger() == ExchangeRateValidLedgerEnum.AR
				? AccountingConfigurationRegistry.Instance.InvoicePostingExchangeRateOptionAR
				: AccountingConfigurationRegistry.Instance.InvoicePostingExchangeRateOptionAP;

			InvoicingLineBase line = (InvoicingLineBase)invoice.Lines.AddNew();

			line.AL_ExchangeRate = 0m;
			AssertHasError(line.AL_ExchangeRateInfo, "Exchange Rate cannot be zero.");

			line.AL_ExchangeRate = 1m;
			AssertNoErrors(line.AL_ExchangeRateInfo);

			line.AL_ExchangeRate = -0.5m;
			AssertHasError(line.AL_ExchangeRateInfo, "Exchange Rate cannot be negative.");

			line.AL_ExchangeRate = 1m;
			AssertNoErrors(line.AL_ExchangeRateInfo);

			var ledgerStr = line.IsAR() ? "AR" : "AP";

			var noRateSetMessage = $@"The ""{ledgerStr} Invoice Posting Exchange Rate Option"" has been set to ""Today Exchange Rate"".
But the USD exchange rate is not set for the date 20-Jun-18. Please check your data and try again.";

			postingExRateRegistry.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "TOD");
			line.AL_RX_NKTransactionCurrency = CurrencyCodes.UnitedStates;
			AssertHasError("Error is shown when line currency is different to header", line.AL_ExchangeRateInfo, noRateSetMessage);

			invoice.AH_RX_NKTransactionCurrency = CurrencyCodes.UnitedStates;
			line.RunPreSaveValidation();
			AssertEquals("Pre-condition: currencies are equal", invoice.AH_RX_NKTransactionCurrency, line.AL_RX_NKTransactionCurrency);
			AssertNoErrorContaining("Error is not shown when line currency is same as header", line.AL_ExchangeRateInfo, noRateSetMessage);

			invoice.UseJobExchangeRate = true;
			line.RunPreSaveValidation();
			AssertEquals("Pre-condition: currencies are still equal", invoice.AH_RX_NKTransactionCurrency, line.AL_RX_NKTransactionCurrency);
			AssertHasError("Error is shown when UseJobExchangeRate is true", line.AL_ExchangeRateInfo, noRateSetMessage);

			var wrongRateSetMessage = $@"The ""{ledgerStr} Invoice Posting Exchange Rate Option"" has been set to ""Today Exchange Rate"".
With this option, the exchange rate should not be changed manually. System expected 0.750000 rate but 0.800000 was entered.";

			var creator = new TestObjectCreator(Factory);
			creator.CreateExchangeRate(creator.USD, ExchangeRateTypes.Code.SellRate, 0.75m);
			creator.CreateExchangeRate(creator.USD, ExchangeRateTypes.Code.BuyRate, 0.75m);

			invoice.AH_RX_NKTransactionCurrency = CurrencyCodes.Australia;
			line.AL_RX_NKTransactionCurrency = CurrencyCodes.UnitedStates;
			invoice.UseJobExchangeRate = false;
			line.AL_ExchangeRate = 0.8m;
			AssertHasError("Error is shown when line currency is different to header", line.AL_ExchangeRateInfo, wrongRateSetMessage);

			invoice.AH_RX_NKTransactionCurrency = CurrencyCodes.UnitedStates;
			line.RunPreSaveValidation();
			AssertEquals("Pre-condition: currencies are equal", invoice.AH_RX_NKTransactionCurrency, line.AL_RX_NKTransactionCurrency);
			AssertNoErrorContaining("Error is not shown when line currency is same as header", line.AL_ExchangeRateInfo, wrongRateSetMessage);

			invoice.UseJobExchangeRate = true;
			line.AL_ExchangeRate = 0.8m;
			AssertEquals("Pre-condition: currencies are still equal", invoice.AH_RX_NKTransactionCurrency, line.AL_RX_NKTransactionCurrency);
			AssertHasError("Error is shown when UseJobExchangeRate is true", line.AL_ExchangeRateInfo, wrongRateSetMessage);
		}

		[TestDate(2018, 6, 20)]
		public void TestCheckAL_ExchangeRate_WithCurrencyConfig()
		{
			TestCaseHelper.ClearTable(AccJobConfigPivotSchema.Constants.TableName);
			TestCaseHelper.ClearTable(AccExchangeRateConfigurationViewSchema.Constants.TableName);

			var masterTestObjectCreator = new AccountingTestObjectCreator(Factory);
			var configForAllCur = masterTestObjectCreator.CreateAccExchangeRateConfiguration(AccExRateConfigurationLevelEnum.System, ZGuid.Empty, ledger: string.Empty, TransportModes.All, FreightShipmentDirection.Code.All, TransportModes.All,
				currencyCodes: null);
			configForAllCur.GetCurrencyConfig(ZString.Empty, ZDate.Empty).JCT_ExRateType = ExchangeRateTypes.Code.C99Rate;
			var configForUsd = masterTestObjectCreator.CreateAccExchangeRateConfiguration(AccExRateConfigurationLevelEnum.System, ZGuid.Empty, ledger: string.Empty, TransportModes.All, FreightShipmentDirection.Code.All, TransportModes.All,
				currencyCodes: new[] { CurrencyCodes.UnitedStates });
			configForUsd.GetCurrencyConfig(CurrencyCodes.UnitedStates, ZDate.Empty).JCT_ExRateType = ExchangeRateTypes.Code.C01Rate;

			CombineAssertions("PreCondition", () => {
				var rateForUsd = Factory.Load<RefExchangeRate>(new ZQuery(RefExchangeRateSchema.RE_RX_NKExCurrency, CurrencyCodes.UnitedStates));
				AssertNotEquals(configForAllCur.GetCurrencyConfig(ZString.Empty, ZDate.Empty).JCT_ExRateType, configForUsd.GetCurrencyConfig(CurrencyCodes.UnitedStates, ZDate.Empty).JCT_ExRateType);
				AssertEquals(0, rateForUsd.Where(x => x.RE_ExRateType == configForAllCur.GetCurrencyConfig(ZString.Empty, ZDate.Empty).JCT_ExRateType).Count());
				AssertEquals(0, rateForUsd.Where(x => x.RE_ExRateType == configForUsd.GetCurrencyConfig(CurrencyCodes.UnitedStates, ZDate.Empty).JCT_ExRateType).Count());
			});

			var invoice = (InvoicingBase)Factory.New(InvoiceType);
			var postingExRateRegistry = invoice.GetExRateLedger() == ExchangeRateValidLedgerEnum.AR
				? AccountingConfigurationRegistry.Instance.InvoicePostingExchangeRateOptionAR
				: AccountingConfigurationRegistry.Instance.InvoicePostingExchangeRateOptionAP;
			postingExRateRegistry.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "TOD");

			var line = (InvoicingLineBase)invoice.Lines.AddNew();

			var ledgerStr = line.IsAR() ? "AR" : "AP";

			var wrongRateSetMessage = $@"The ""{ledgerStr} Invoice Posting Exchange Rate Option"" has been set to ""Today Exchange Rate"".
With this option, the exchange rate should not be changed manually. System expected 0.750000 rate but 0.800000 was entered.";

			var creator = new TestObjectCreator(Factory);
			creator.CreateExchangeRate(creator.USD, configForUsd.GetCurrencyConfig(CurrencyCodes.UnitedStates, ZDate.Empty).JCT_ExRateType, 0.75m);

			invoice.AH_RX_NKTransactionCurrency = CurrencyCodes.Australia;
			line.AL_RX_NKTransactionCurrency = CurrencyCodes.UnitedStates;
			invoice.UseJobExchangeRate = false;
			line.AL_ExchangeRate = 0.8m;
			AssertHasError("Error is shown when line currency is different to header", line.AL_ExchangeRateInfo, wrongRateSetMessage);

			invoice.AH_RX_NKTransactionCurrency = CurrencyCodes.UnitedStates;
			line.RunPreSaveValidation();
			AssertEquals("Pre-condition: currencies are equal", invoice.AH_RX_NKTransactionCurrency, line.AL_RX_NKTransactionCurrency);
			AssertNoErrorContaining("Error is not shown when line currency is same as header", line.AL_ExchangeRateInfo, wrongRateSetMessage);

			invoice.UseJobExchangeRate = true;
			line.AL_ExchangeRate = 0.8m;
			AssertEquals("Pre-condition: currencies are still equal", invoice.AH_RX_NKTransactionCurrency, line.AL_RX_NKTransactionCurrency);
			AssertHasError("Error is shown when UseJobExchangeRate is true", line.AL_ExchangeRateInfo, wrongRateSetMessage);
		}

		public void Test_AL_ExchangeRate_Overriding_WithSecurityRightsAndRegistryConfig_WhenHeaderIsForeign()
		{
			ExchangeRateReader.GetReaderInstance().ClearCache();

			TestObjectCreator.CreateExchangeRate(TestObjectCreator.USD, ExchangeRateTypes.Code.BuyRate, 2, ZDateTime.Today, ZDateTime.Today);

			var invoice = TestObjectCreator.CreateAPInvoice<APInvoice>("AP001", TestObjectCreator.USD, 2.000000m, 1000m, 100m, 0m, 2000m, 200m, 0m);
			invoice.AH_RX_NKTransactionCurrency = TestObjectCreator.USD.Code;
			invoice.UseJobExchangeRate = true;

			var postingExRateRegistry = AccountingConfigurationRegistry.Instance.InvoicePostingExchangeRateOptionAP;
			postingExRateRegistry.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, AccountingConstants.InvoicePostingExchangeRateOption.ExchangeRateBasedOnInvoiceDate.Code);

			var invoiceLine = invoice.Lines[0];
			invoiceLine.AL_ExchangeRate = 2.6;
			invoice.RunPreSaveValidation();

			AssertHasError(invoiceLine.AL_ExchangeRateInfo, $@"The ""AP Invoice Posting Exchange Rate Option"" has been set to ""Exchange Rate based on Invoice Date"".
With this option, the exchange rate should not be changed manually. System expected 2.000000 rate but 2.600000 was entered.");

			Env.Security.NewPayablesOverridePostingExchangeRateAllows.IsAllowed = true;
			invoice.AH_OverrideExchangeRate = true;

			invoice.RunPreSaveValidation();

			AssertNoErrors(invoiceLine.AL_ExchangeRateInfo);
			AssertNoWarnings(invoiceLine.AL_ExchangeRateInfo);

			Factory.Save();

			var logsWithMessageCount = invoice.Logs.GetAllLogs().Where(x => x.SL_Reference == "Line Exchange Rates Updated").Count();
			AssertEquals("Event log added for Line Exchange Rate Update", 1, logsWithMessageCount);

			ExchangeRateReader.GetReaderInstance().ClearCache();
		}

		public void Test_AL_ExchangeRate_Overriding_WithSecurityRightsAndRegistryConfig_WhenHeaderIsLocal()
		{
			ExchangeRateReader.GetReaderInstance().ClearCache();

			TestObjectCreator.CreateExchangeRate(TestObjectCreator.AUD, ExchangeRateTypes.Code.BuyRate, 1, ZDateTime.Today, ZDateTime.Today);
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.USD, ExchangeRateTypes.Code.BuyRate, 0.9m, ZDateTime.Today, ZDateTime.Today);

			var invoice = TestObjectCreator.CreateAPInvoice<APInvoice>("AP001", TestObjectCreator.AUD, 2.000000m, 1000m, 100m, 0m, 2000m, 200m, 0m);
			invoice.AH_RX_NKTransactionCurrency = TestObjectCreator.AUD.Code;

			var postingExRateRegistry = AccountingConfigurationRegistry.Instance.InvoicePostingExchangeRateOptionAP;
			postingExRateRegistry.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, AccountingConstants.InvoicePostingExchangeRateOption.ExchangeRateBasedOnInvoiceDate.Code);

			var invoiceLine = invoice.Lines[0];
			invoiceLine.AL_RX_NKTransactionCurrency = TestObjectCreator.USD.Code;
			invoiceLine.AL_ExchangeRate = 0.95;
			invoice.RunPreSaveValidation();

			AssertHasError(invoiceLine.AL_ExchangeRateInfo, $@"The ""AP Invoice Posting Exchange Rate Option"" has been set to ""Exchange Rate based on Invoice Date"".
With this option, the exchange rate should not be changed manually. System expected 0.900000 rate but 0.950000 was entered.");

			Env.Security.NewPayablesOverridePostingExchangeRateAllows.IsAllowed = true;
			invoice.AH_OverrideExchangeRate = true;

			invoice.RunPreSaveValidation();

			AssertNoErrors(invoiceLine.AL_ExchangeRateInfo);
			AssertNoWarnings(invoiceLine.AL_ExchangeRateInfo);

			Factory.Save();

			var logsWithMessageCount = invoice.Logs.GetAllLogs().Where(x => x.SL_Reference == "Line Exchange Rates Updated").Count();
			AssertEquals("Event log added for Line Exchange Rate Update", 1, logsWithMessageCount);

			ExchangeRateReader.GetReaderInstance().ClearCache();
		}

		#region TestCheckAL_OSTaxAmountAllowZeroForSPV

		public void TestCheckAL_OSTaxAmountAllowZeroForSPV_Italy()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Italy))
			{
				SetupAndAssertTaxAmountAllowZeroForSPV();
			}
		}

		public void TestCheckAL_OSTaxAmountAllowZeroForSPV_CostaRica()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.CostaRica))
			{
				SetupAndAssertTaxAmountAllowZeroForSPV();
			}
		}

		void SetupAndAssertTaxAmountAllowZeroForSPV()
		{
			GlbCompany.CurrentCompany.GC_IsGSTRegistered = true;
			InvoicingBase invoice = (InvoicingBase)Factory.New(InvoiceType);
			invoice.AH_OH = GSTRegisteredOrg.PK;

			InvoicingLineBase line = (InvoicingLineBase)invoice.Lines.AddNew();
			AccChargeCode chargeCode = CreateChargeCode("NCCC", "Non Comment Charge Code 1", Constants.ChargeType.Revenue, 80, null, null);

			line.AL_AC = chargeCode.PK;
			line.AL_OSExTaxAmount = 10m;
			line.AL_AT = SPV22TaxRate.PK;
			AssertEquals(0m, line.AL_OSTaxAmount);

			invoice.RunPreSaveValidation();
			AssertNoErrors(line.AL_OSTaxAmountInfo);
		}

		#endregion

		public void TestCheckAL_OSTaxAmount()
		{
			GlbCompany.CurrentCompany.GC_IsGSTRegistered = true;
			InvoicingBase invoice = (InvoicingBase)Factory.New(InvoiceType);
			invoice.AH_OH = GSTRegisteredOrg.PK;

			InvoicingLineBase line1 = (InvoicingLineBase)invoice.Lines.AddNew();
			AccChargeCode chargeCode = CreateChargeCode("NCCC", "Non Comment Charge Code 1", Constants.ChargeType.Revenue, 80, null, null);

			line1.AL_AC = chargeCode.PK;
			line1.AL_OSExTaxAmount = 10m;    // calculated tax amount will be 1
			line1.AL_AT = GST10TaxRate.PK;
			line1.AL_OSTaxAmount = 0m;

			invoice.RunPreSaveValidation();
			AssertEquals("OS Ex Tax Amt", 10.00M, line1.AL_OSExTaxAmount);
			AssertEquals("OS Total Amt ", 10.00M, line1.AL_OverseasTotal);
			AssertHasError(line1.AL_OSTaxAmountInfo, InvoicingLineBaseValidation.EnforceGSTAmountEntry_ForTestOnly);

			line1.AL_TaxRateNumerator = 0;
			invoice.RunPreSaveValidation();
			AssertEquals("OS Ex Tax Amt", 10.00M, line1.AL_OSExTaxAmount);
			AssertEquals("OS Total Amt ", 10.00M, line1.AL_OverseasTotal);
			AssertNoError(line1.AL_OSTaxAmountInfo, InvoicingLineBaseValidation.EnforceGSTAmountEntry_ForTestOnly);

			ARInvoiceLine line = Factory.NewWithValidTestData<ARInvoiceLine>();

			line.AL_OSExTaxAmount = -100m;
			line.AL_OSTaxAmount = 10m;
			AssertHasError(line.AL_OSTaxAmountInfo, TransactionLineValidation.AmountAndTaxAmountMustHaveSameSign_ForTestOnly);

			line.AL_OSExTaxAmount = 100m;
			line.AL_OSTaxAmount = 10m;
			AssertEquals(false, line.AL_OSTaxAmountInfo.HasErrors());

			line.AL_OSExTaxAmount = 100m;
			line.AL_OSTaxAmount = -10m;
			AssertHasError(line.AL_OSTaxAmountInfo, TransactionLineValidation.AmountAndTaxAmountMustHaveSameSign_ForTestOnly);
		}

		public void TestCheckAL_OSTaxAmountWhenCalculatedTaxIsZero()
		{
			GlbCompany.CurrentCompany.GC_IsGSTRegistered = true;
			InvoicingBase invoice = (InvoicingBase)Factory.New(InvoiceType);
			invoice.AH_OH = GSTRegisteredOrg.PK;

			InvoicingLineBase line = (InvoicingLineBase)invoice.Lines.AddNew();
			line.AL_AC = DisbursementChargeCode.PK;
			line.AL_OSExTaxAmount = 0.04m;
			line.AL_AT = GST10TaxRate.PK;

			invoice.RunPreSaveValidation();
			Assert("Tax amount should not have errors", !line.AL_OSTaxAmountInfo.HasErrors());
		}

		public void TestCheckAL_OSTaxAmountHasWarningForTaxAmountRange()
		{
			InvoicingBase invoice = (InvoicingBase)Factory.New(InvoiceType);
			invoice.AH_OH = GSTRegisteredOrg.PK;

			InvoicingLineBase line = (InvoicingLineBase)invoice.Lines.AddNew();
			AccChargeCode chargeCode = CreateChargeCode("NCCC", "Non Comment Charge Code 1", Constants.ChargeType.Revenue, 80, null, null);

			line.AL_AC = chargeCode.PK;
			line.AL_AT = GST10TaxRate.PK;

			line.AL_OSExTaxAmount = 10m;
			line.AL_OSTaxAmount = 10m;

			InvoicingLineBaseValidation validation = new InvoicingLineBaseValidation(invoice.Lines[0]);
			validation.ValidateAL_OSTaxAmount();
			AssertHasWarning(line.AL_OSTaxAmountInfo, InvoicingLineBaseValidation.TaxAmountRangeWarning_ForTestOnly);

			AccTaxRate gstAndQst = TestObjectCreator.CreateTaxRate("GSTNQST", "GSTANDQST", AccTaxRate.Types.Rated, 5, AccTaxRate.ExtraTypes.QuebecQST, 75, 10);
			AccTaxRate gstAndQst2 = TestObjectCreator.CreateTaxRate("GSTNQS2", "GSTANDQST", AccTaxRate.Types.Rated, 5, AccTaxRate.ExtraTypes.QuebecQST, 0, 10);

			line.AL_AT = gstAndQst.PK;
			line.AL_OSExTaxAmount = 100m;
			AssertEquals("GST Amount", 5m, (decimal)line.AL_OSGSTAmount);
			AssertEquals("QST Amount", 7.88m, (decimal)line.AL_OSExtraTaxAmount);
			AssertNoWarning(line.AL_OSTaxAmountInfo, InvoicingLineBaseValidation.TaxAmountRangeWarning_ForTestOnly);

			line.AL_AT = gstAndQst2.PK;
			line.AL_OSExTaxAmount = 100m;
			AssertEquals("GST Amount", 5m, line.AL_OSGSTAmount);
			AssertEquals("QST Amount", 0m, line.AL_OSExtraTaxAmount);
			AssertNoWarning(line.AL_OSTaxAmountInfo, InvoicingLineBaseValidation.TaxAmountRangeWarning_ForTestOnly);

			line.AL_OSTaxAmount = 100m;
			AssertHasWarning(line.AL_OSTaxAmountInfo, InvoicingLineBaseValidation.TaxAmountRangeWarning_ForTestOnly);

			AccTaxRate gstAndEdu = TestObjectCreator.CreateTaxRate("GSTNEDU", "GSTANDEDU", AccTaxRate.Types.Rated, 12, AccTaxRate.ExtraTypes.IndiaPrimaryAndSecondaryEducationTax, 3, 1);
			AccTaxRate gstAndEdu2 = TestObjectCreator.CreateTaxRate("GSTNED2", "GSTANDEDU", AccTaxRate.Types.Rated, 12, AccTaxRate.ExtraTypes.IndiaPrimaryAndSecondaryEducationTax, 0, 1);

			line.AL_AT = gstAndEdu.PK;
			line.AL_OSExTaxAmount = 100m;
			AssertEquals("GST Amount", 12m, (decimal)line.AL_OSGSTAmount);
			AssertEquals("EDU Amount", 0.36m, (decimal)line.AL_OSExtraTaxAmount);
			AssertNoWarning(line.AL_OSTaxAmountInfo, InvoicingLineBaseValidation.TaxAmountRangeWarning_ForTestOnly);

			line.AL_AT = gstAndEdu2.PK;
			line.AL_OSExTaxAmount = 100m;
			AssertEquals("GST Amount", 12m, line.AL_OSGSTAmount);
			AssertEquals("EDU Amount", 0m, line.AL_OSExtraTaxAmount);
			AssertNoWarning(line.AL_OSTaxAmountInfo, InvoicingLineBaseValidation.TaxAmountRangeWarning_ForTestOnly);

			line.AL_OSTaxAmount = 100m;
			AssertHasWarning(line.AL_OSTaxAmountInfo, InvoicingLineBaseValidation.TaxAmountRangeWarning_ForTestOnly);

			AccTaxRate ret = TestObjectCreator.CreateTaxRate("RET", "RET", AccTaxRate.Types.Rated, 16, AccTaxRate.ExtraTypes.VATRetention, 4, 1);
			AccTaxRate ret2 = TestObjectCreator.CreateTaxRate("RET2", "RET", AccTaxRate.Types.Rated, 16, AccTaxRate.ExtraTypes.VATRetention, 0, 1);

			line.AL_AT = ret.PK;
			line.AL_OSExTaxAmount = 100m;
			AssertEquals("GST Amount", 16m, (decimal)line.AL_OSGSTAmount);
			AssertEquals("RET Amount", -4m, (decimal)line.AL_OSExtraTaxAmount);
			AssertNoWarning(line.AL_OSTaxAmountInfo, InvoicingLineBaseValidation.TaxAmountRangeWarning_ForTestOnly);

			line.AL_AT = ret2.PK;
			line.AL_OSExTaxAmount = 100m;
			AssertEquals("GST Amount", 16m, line.AL_OSGSTAmount);
			AssertEquals("RET Amount", 0m, line.AL_OSExtraTaxAmount);
			AssertNoWarning(line.AL_OSTaxAmountInfo, InvoicingLineBaseValidation.TaxAmountRangeWarning_ForTestOnly);

			line.AL_OSTaxAmount = 100m;
			AssertHasWarning(line.AL_OSTaxAmountInfo, InvoicingLineBaseValidation.TaxAmountRangeWarning_ForTestOnly);

			AccTaxRate gstAndQstBasedOnQct = TestObjectCreator.CreateTaxRate("QCT", "QCT", AccTaxRate.Types.Rated, 5, AccTaxRate.ExtraTypes.QuebecQSTExcludingGSTInQSTBase, 9975, 1000);
			AccTaxRate gstAndQstBasedOnQct2 = TestObjectCreator.CreateTaxRate("QCT2", "QCT", AccTaxRate.Types.Rated, 5, AccTaxRate.ExtraTypes.QuebecQSTExcludingGSTInQSTBase, 0, 1000);

			line.AL_AT = gstAndQstBasedOnQct.PK;
			line.AL_OSExTaxAmount = 100m;
			AssertEquals("GST Amount", 5m, (decimal)line.AL_OSGSTAmount);
			AssertEquals("QST Amount", 9.98m, (decimal)line.AL_OSExtraTaxAmount);
			AssertNoWarning(line.AL_OSTaxAmountInfo, InvoicingLineBaseValidation.TaxAmountRangeWarning_ForTestOnly);

			line.AL_AT = gstAndQstBasedOnQct2.PK;
			line.AL_OSExTaxAmount = 100m;
			AssertEquals("GST Amount", 5m, line.AL_OSGSTAmount);
			AssertEquals("QST Amount", 0m, line.AL_OSExtraTaxAmount);
			AssertNoWarning(line.AL_OSTaxAmountInfo, InvoicingLineBaseValidation.TaxAmountRangeWarning_ForTestOnly);

			line.AL_OSTaxAmount = 100m;
			AssertHasWarning(line.AL_OSTaxAmountInfo, InvoicingLineBaseValidation.TaxAmountRangeWarning_ForTestOnly);

			AccTaxRate oto6 = TestObjectCreator.CreateTaxRate("OTO6", "OTO", AccTaxRate.Types.Rated, 0, AccTaxRate.ExtraTypes.ChinaInputVATOffsetAgainstOutputTax, 6, 1);
			AccTaxRate oto62 = TestObjectCreator.CreateTaxRate("OTO62", "OTO", AccTaxRate.Types.Rated, 0, AccTaxRate.ExtraTypes.ChinaInputVATOffsetAgainstOutputTax, 0, 1);

			line.AL_AT = oto6.PK;
			line.AL_OSExTaxAmount = 100m;
			AssertEquals("GST Amount", 0m, (decimal)line.AL_OSGSTAmount);
			AssertEquals("OTO Amount", 6m, (decimal)line.AL_OSExtraTaxAmount);
			AssertNoWarning(line.AL_OSTaxAmountInfo, InvoicingLineBaseValidation.TaxAmountRangeWarning_ForTestOnly);

			line.AL_AT = oto62.PK;
			line.AL_OSExTaxAmount = 100m;
			AssertEquals("GST Amount", 0m, line.AL_OSGSTAmount);
			AssertEquals("OTO Amount", 0m, line.AL_OSExtraTaxAmount);
			AssertNoWarning(line.AL_OSTaxAmountInfo, InvoicingLineBaseValidation.TaxAmountRangeWarning_ForTestOnly);

			line.AL_OSTaxAmount = 100m;
			AssertHasWarning(line.AL_OSTaxAmountInfo, InvoicingLineBaseValidation.TaxAmountRangeWarning_ForTestOnly);
		}

		public void TestCheckAL_OSTaxAmountHasErrorForTaxAmountRange()
		{
			InvoicingBase invoice = (InvoicingBase)Factory.New(InvoiceType);
			invoice.AH_OH = GSTRegisteredOrg.PK;

			InvoicingLineBase line = (InvoicingLineBase)invoice.Lines.AddNew();
			line.AL_AC = TestObjectCreator.CC1.PK;
			line.AL_AT = GST10TaxRate.PK;
			line.AL_OSExTaxAmount = 10m;
			line.AL_OSTaxAmount = 10m;
			line.TransactionHeader.AH_TransactionType = TransactionTypes.AdjustmentNote;

			InvoicingLineBaseValidation validation = new InvoicingLineBaseValidation(invoice.Lines[0]);
			validation.ValidateAL_OSTaxAmount();
			AssertHasWarning(line.AL_OSTaxAmountInfo, InvoicingLineBaseValidation.TaxAmountRangeWarning_ForTestOnly);
			AssertNoError(line.AL_OSTaxAmountInfo, InvoicingLineBaseValidation.TaxAmountRangeWarning_ForTestOnly);

			line.AL_OSTaxAmount = 0.99m;
			validation.ValidateAL_OSTaxAmount();
			AssertNoWarning(line.AL_OSTaxAmountInfo, InvoicingLineBaseValidation.TaxAmountRangeWarning_ForTestOnly);
			AssertNoError(line.AL_OSTaxAmountInfo, InvoicingLineBaseValidation.TaxAmountRangeWarning_ForTestOnly);

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Enterprise.Core.Constants.CountryCodes.Portugal))
			{
				invoice.AH_ComplianceSubType = PortugalComplianceInfo.ComplianceSubTypeCodes.TCR;
				invoice.AH_Ledger = LedgerTypes.AccountsReceivable;

				validation.ValidateAL_OSTaxAmount();
				AssertNoWarning(line.AL_OSTaxAmountInfo, InvoicingLineBaseValidation.TaxAmountRangeWarning_ForTestOnly);
				AssertNoError(line.AL_OSTaxAmountInfo, InvoicingLineBaseValidation.TaxAmountRangeWarning_ForTestOnly);

				line.TransactionHeader.AH_TransactionType = TransactionTypes.Invoice;
				validation.ValidateAL_OSTaxAmount();
				AssertNoWarning(line.AL_OSTaxAmountInfo, InvoicingLineBaseValidation.TaxAmountRangeWarning_ForTestOnly);
				AssertHasError(line.AL_OSTaxAmountInfo, InvoicingLineBaseValidation.TaxAmountRangeWarning_ForTestOnly);

				using (AccountingConfigurationRegistry.Instance.CalculateTaxAtHeaderLevel.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
				{
					validation.ValidateAL_OSTaxAmount();
					AssertNoWarning(line.AL_OSTaxAmountInfo, InvoicingLineBaseValidation.TaxAmountRangeWarning_ForTestOnly);
					AssertNoError(line.AL_OSTaxAmountInfo, InvoicingLineBaseValidation.TaxAmountRangeWarning_ForTestOnly);
				}

				invoice.AH_ComplianceSubType = ZString.Empty;
				validation.ValidateAL_OSTaxAmount();
				AssertNoWarning(line.AL_OSTaxAmountInfo, InvoicingLineBaseValidation.TaxAmountRangeWarning_ForTestOnly);
				AssertHasError(line.AL_OSTaxAmountInfo, InvoicingLineBaseValidation.TaxAmountRangeWarning_ForTestOnly);

				invoice.AH_ComplianceSubType = PortugalComplianceInfo.ComplianceSubTypeCodes.LCD;
				validation.ValidateAL_OSTaxAmount();
				AssertHasWarning(line.AL_OSTaxAmountInfo, InvoicingLineBaseValidation.TaxAmountRangeWarning_ForTestOnly);
				AssertNoError(line.AL_OSTaxAmountInfo, InvoicingLineBaseValidation.TaxAmountRangeWarning_ForTestOnly);

				invoice.AH_Ledger = LedgerTypes.AccountsPayable;
				validation.ValidateAL_OSTaxAmount();
				AssertNoWarning(line.AL_OSTaxAmountInfo, InvoicingLineBaseValidation.TaxAmountRangeWarning_ForTestOnly);
				AssertNoError(line.AL_OSTaxAmountInfo, InvoicingLineBaseValidation.TaxAmountRangeWarning_ForTestOnly);

				line.AL_OSTaxAmount = 10m;
				validation.ValidateAL_OSTaxAmount();
				AssertHasWarning(line.AL_OSTaxAmountInfo, InvoicingLineBaseValidation.TaxAmountRangeWarning_ForTestOnly);
				AssertNoError(line.AL_OSTaxAmountInfo, InvoicingLineBaseValidation.TaxAmountRangeWarning_ForTestOnly);
			}
		}

		public void TestCheckAL_OSTaxAmount_InterCompanyInvoiceImportError()
		{
			GlbCompany.CurrentCompany.GC_IsGSTRegistered = true;
			var invoice = (InvoicingBase)Factory.New(InvoiceType);
			invoice.AH_OH = GSTRegisteredOrg.PK;

			var line = (InvoicingLineBase)invoice.Lines.AddNew();

			line.AL_AC = TestObjectCreator.CC1.PK;
			line.AL_OSExTaxAmount = 10m;
			line.AL_AT = GST10TaxRate.PK;

			line.TaxAmountErrorDetailForInterCompanyInvoiceImport = ("AAA", line.AL_AT, line.AL_OSTaxAmount, line.AL_TaxRateCalc);
			AssertEquals("Precondition", GST10TaxRate.PK, line.TaxAmountErrorDetailForInterCompanyInvoiceImport.TaxRatePK);
			AssertEquals("Precondition", 1m, line.TaxAmountErrorDetailForInterCompanyInvoiceImport.OSTaxAmount);
			AssertEquals("Precondition", 10m, line.TaxAmountErrorDetailForInterCompanyInvoiceImport.TaxRateCalc);

			invoice.RunPreSaveValidation();
			AssertHasError(line.AL_OSTaxAmountInfo, "AAA");

			line.AL_OSTaxAmount = 1.1m;
			AssertNotEquals("Precondition", line.AL_OSTaxAmount, line.TaxAmountErrorDetailForInterCompanyInvoiceImport.OSTaxAmount);
			AssertEquals("Precondition", line.AL_AT, line.TaxAmountErrorDetailForInterCompanyInvoiceImport.TaxRatePK);
			AssertEquals("Precondition", line.AL_TaxRateCalc, line.TaxAmountErrorDetailForInterCompanyInvoiceImport.TaxRateCalc);
			invoice.RunPreSaveValidation();
			AssertNoError(line.AL_OSTaxAmountInfo, "AAA");

			line.AL_AT = TestObjectCreator.GST1.PK;
			line.AL_OSTaxAmount = 1m;
			AssertEquals("Precondition", line.AL_OSTaxAmount, line.TaxAmountErrorDetailForInterCompanyInvoiceImport.OSTaxAmount);
			AssertNotEquals("Precondition", line.AL_AT, line.TaxAmountErrorDetailForInterCompanyInvoiceImport.TaxRatePK);
			AssertEquals("Precondition", line.AL_TaxRateCalc, line.TaxAmountErrorDetailForInterCompanyInvoiceImport.TaxRateCalc);
			invoice.RunPreSaveValidation();
			AssertHasError(line.AL_OSTaxAmountInfo, "AAA");

			line.AL_AT = TestObjectCreator.GST2.PK;
			line.AL_OSTaxAmount = 1m;
			AssertEquals("Precondition", line.AL_OSTaxAmount, line.TaxAmountErrorDetailForInterCompanyInvoiceImport.OSTaxAmount);
			AssertNotEquals("Precondition", line.AL_AT, line.TaxAmountErrorDetailForInterCompanyInvoiceImport.TaxRatePK);
			AssertNotEquals("Precondition", line.AL_TaxRateCalc, line.TaxAmountErrorDetailForInterCompanyInvoiceImport.TaxRateCalc);
			invoice.RunPreSaveValidation();
			AssertNoError(line.AL_OSTaxAmountInfo, "AAA");
		}

		public void TestCheckGenericChargeHasErrorsIfEmpty()
		{
			AccChargeCode chargeCode = CreateChargeCode("TST", "Description", Constants.ChargeType.Margin, 100m, null, null);
			Factory.Save();

			APInvoice invoice = Factory.New<APInvoice>();
			invoice.AH_OH = GSTRegisteredOrg.PK;
			invoice.AH_TransactionNum = "5";
			InvoicingLineBase invoiceLine = (InvoicingLineBase)invoice.Lines.AddNew();
			invoiceLine.AL_AT = GST10TaxRate.PK;
			invoiceLine.AL_OSExTaxAmount = 100m;
			invoiceLine.AL_OSTaxAmount = 10m;

			GenericCharge.GenericCharge charge = Factory.Load<GenericCharge.GenericCharge>(chargeCode.PK);
			AssertNotNull("GenericCharge with ChargeCode must be loaded", charge);
			invoiceLine.GenericCharge = charge.PK;
			InvoicingLineBaseValidation validation = new InvoicingLineBaseValidation(invoiceLine);
			validation.ValidateGenericCharge();

			Assert("No errors expected.", !invoiceLine.GenericChargeInfo.HasErrors());
			chargeCode.AC_AG_CostAccount = ZGuid.Empty;
			validation.ValidateGenericCharge();

			Assert("Must Have the Error.", invoiceLine.GenericChargeInfo.HasError("Charge Code 'TST' must have CST GL Account entered."));

			chargeCode.AC_AG_CostAccount = TestObjectCreator.GLHeader1.PK;
			validation.ValidateGenericCharge();
			Assert("No errors expected.", !invoiceLine.GenericChargeInfo.HasErrors());

			chargeCode.AC_ChargeType = Constants.ChargeType.Comment;
			validation.ValidateGenericCharge();
			Assert("No errors expected.", !invoiceLine.GenericChargeInfo.HasErrors());

			invoiceLine.AL_AC = ZGuid.Empty;
			validation.ValidateGenericCharge();
			Assert("No errors expected.", !invoiceLine.GenericChargeInfo.HasErrors());

			invoiceLine = (InvoicingLineBase)invoice.Lines.AddNew();
			validation = new InvoicingLineBaseValidation(invoiceLine);

			GenericCharge.GenericCharge account = Factory.Load<GenericCharge.GenericCharge>(TestObjectCreator.GLHeader1.PK);
			AssertNotNull("GenericCharge with GL Account must be loaded", account);
			invoiceLine.GenericCharge = account.PK;
			validation.ValidateGenericCharge();

			Assert("No errors expected.", !invoiceLine.GenericChargeInfo.HasErrors());
			invoiceLine.AL_AG = ZGuid.Empty;
			validation.ValidateGenericCharge();

			Assert("Must Have the Error.", invoiceLine.GenericChargeInfo.HasError(TransactionLine.EmptyChargeCodeAndGLHeaderError));
		}

		public void TestValidationOfConsolWithDeletedSplitCharge()
		{
			InvoicingBase invoice = (InvoicingBase)Factory.NewWithValidTestData(InvoiceType);
			invoice.AH_OH = GSTRegisteredOrg.PK;
			InvoicingLineBase line = (InvoicingLineBase)invoice.Lines.AddNew();
			line.ApportionmentChargeImportedFrom = Factory.NewWithValidTestData<ApportionSplitCharge>();
			using (invoice.GetReportingDeletedApportionmentChargesSuspender())
			{
				line.ApportionmentChargeImportedFrom.Delete();
				AssertNoExceptionThrown(() => line.Validation.ValidateAll());
			}
		}

		public void TestCheckGenericChargeHasWarnings()
		{
			AccChargeCode charge = CreateChargeCode("TST", "Description", Constants.ChargeType.Margin, 100m, null, null);
			Factory.Save();

			InvoicingBase invoice = (InvoicingBase)Factory.New(InvoiceType);
			invoice.AH_OH = GSTRegisteredOrg.PK;
			InvoicingLineBase line = (InvoicingLineBase)invoice.Lines.AddNew();

			line.AL_AT = GST10TaxRate.PK;

			line.AL_OSExTaxAmount = 10m;
			line.AL_OSTaxAmount = 10m;

			GenericCharge.GenericCharge testGenericCharge = Factory.NewWithValidTestData(typeof(GenericCharge.GenericCharge)) as GenericCharge.GenericCharge;
			testGenericCharge.VC_IsGLAccount = false;
			line.GenericCharge = testGenericCharge.PK;
			line.AL_AC = charge.PK;
			line.InvoiceBase.SetIsReversing(true);

			InvoicingLineBaseValidation validation = new InvoicingLineBaseValidation(line);
			validation.ValidateGenericCharge();
			Assert(!line.GenericChargeInfo.HasErrors());
			Assert(line.GenericChargeInfo.HasWarnings());

			line.GenericChargeBizO.VC_IsGLAccount = true;
			validation.ValidateGenericCharge();
			AssertNoErrors("Should be no error for IsReversing invoice", line.GenericChargeInfo);
		}

		public void TestCheckGenericChargeOnAmendingLine()
		{
			var charge = CreateChargeCode("TST", "Description", Constants.ChargeType.Margin, 100m, null, null);
			Factory.Save();

			var shipment = TestObjectCreator.CreateShipment("S00001234");
			var job = TestObjectCreator.CreateJob(shipment, false);
			var invoice = (InvoicingBase)Factory.New(InvoiceType);
			invoice.AH_OH = GSTRegisteredOrg.PK;
			invoice.AH_JH = job.PK;
			var line = (InvoicingLineBase)invoice.Lines.AddNew();

			line.AL_AT = GST10TaxRate.PK;

			line.AL_OSExTaxAmount = 10m;
			line.AL_OSTaxAmount = 10m;

			var testGenericCharge = Factory.NewWithValidTestData(typeof(GenericCharge.GenericCharge)) as GenericCharge.GenericCharge;
			testGenericCharge.VC_IsGLAccount = false;
			line.GenericCharge = testGenericCharge.PK;
			line.AL_AC = charge.PK;

			if (invoice is IAmending && invoice.HasImplementedGenerateAmendingTransaction)
			{
				var invoiceType = invoice is APInvoice ? TransactionTypes.CreditNote : invoice.AH_TransactionType.ToString();
				var amending = ((IAmending)invoice).GenerateAmendingTransaction(invoiceType);
				AssertNotNull("Amending", amending);

				var amendingTransaction = (InvoicingBase)amending;
				Assert("Should be at least one line", amendingTransaction.Lines.Any());
				var amendingLine = amendingTransaction.Lines[0];
				AssertEquals("Amending Line has the same Charge Code as Original", line.AL_AC, amendingLine.AL_AC);

				var validation = new InvoicingLineBaseValidation(amendingLine);
				validation.ValidateGenericCharge();
				AssertNoErrors(amendingLine.GenericChargeInfo);

				amendingLine.GenericChargeBizO.VC_IsGLAccount = true;
				validation.ValidateGenericCharge();
				AssertHasError(amendingLine.GenericChargeInfo, "You cannot select a GL Account for an Amending Transaction.");
			}
			else
			{
				Assert("Not Applicable", true);
			}
		}

		public void TestCheckGenericChargeOnBadDebtLine()
		{
			// Test DB will not have the StmData base data, which contains the default value for bad debt for the registry.
			// So add bad debt into the registry for this test
			AccGLHeader badDebtAccount = Factory.LoadTop1<AccGLHeader>(new ZQuery(AccGLHeaderSchema.AG_AccountNum, "3980.00.00"));
			AccountingConfigurationRegistry.Instance.BadDebtWriteOffAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, badDebtAccount.PK.ToGuid());

			var invoice = (InvoicingBase)Factory.New(InvoiceType);
			invoice.AH_TransactionNum = "101";

			var line = (InvoicingLineBase)invoice.Lines.AddNew();
			line.AL_OSExTaxAmount = 100m;
			line.AL_AG = TestObjectCreator.GLHeader1.PK;

			Factory.Save();

			if (invoice is IBadDebtWritingOff)
			{
				IBadDebtWritingOff badDebt = invoice as IBadDebtWritingOff;
				AssertNotNull(badDebt);
				badDebt.IsWritingOff = true;

				var writingOff = new InvoicingBaseWritingOff(badDebt);
				writingOff.Reverse();

				var badDebtWriteOff = writingOff.ReverseTransaction as InvoicingBase;
				AssertNotNull("Bad debt write off transaction", badDebtWriteOff);
				AssertEquals("Count of lines", 1, badDebtWriteOff.Lines.Count);
				var badDebtLine = badDebtWriteOff.Lines[0];
				AssertNotNull("Generic charge on Bad debt line", badDebtLine.GenericChargeBizO);
				Assert("Generic charge on Bad debt line is a GL account", badDebtLine.GenericChargeBizO.VC_IsGLAccount);

				InvoicingLineBaseValidation validation = new InvoicingLineBaseValidation(badDebtLine);
				validation.ValidateGenericCharge();
				AssertNoErrors(badDebtLine.GenericChargeInfo);

				Factory.Save();

				var newFactory = new BusinessObjectFactory();
				var badDebtWriteOffInNewFactory = newFactory.Load<InvoicingBase>(badDebtWriteOff.PK);
				var badDebtLineInNewFactory = badDebtWriteOffInNewFactory.Lines[0];
				validation = new InvoicingLineBaseValidation(badDebtLineInNewFactory);
				validation.ValidateGenericCharge();
				AssertNoErrors(badDebtLineInNewFactory.GenericChargeInfo);
			}
			else
			{
				Assert("Not Applicable", true);
			}
		}
		public void TestCheckAL_JHAgainstGenericCharge()
		{
			AccChargeCode chargeREV = TestObjectCreator.InsertChargeCode(Constants.ChargeType.Revenue);
			chargeREV.AC_AG_CostAccount = TestObjectCreator.GLHeader1.PK;
			TestObjectCreator.CreateTestPeriods(ZDateTime.Today);
			Factory.Save();

			ForwardingShipment shipment = TestObjectCreator.CreateShipment("S00001234");
			var job = TestObjectCreator.CreateJob(shipment, false);
			var invoice = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "ARInv", TestObjectCreator.AUD, 1M, TestObjectCreator.Debtor);
			invoice.AH_JH = job.PK;
			var line = TestObjectCreator.CreateInvoiceLine(invoice, TestObjectCreator.AUD, 1M, 1500M, 0M, 0M, 1500M, 0M, 0M, TestObjectCreator.CC1.PK);
			line.AL_JH = job.PK;
			line.AL_GE = TestObjectCreator.FESDepartment.PK;
			TestObjectCreator.CreateJobCharge(line, job, TestObjectCreator.CC1);

			var amendingTransaction = ((IAmending)invoice).GenerateAmendingTransaction(TransactionTypes.CreditNote);
			var amendingTransactionAsInvoicingBase = amendingTransaction as InvoicingBase;
			Assert("Should be at least one line", amendingTransactionAsInvoicingBase.Lines.Any());

			var amendingLine = (InvoicingLineBase)amendingTransactionAsInvoicingBase.Lines.AddNew();
			amendingLine.GenericCharge = chargeREV.PK;
			amendingLine.AL_JH = ZGuid.Empty;
			amendingLine.Validation.ValidateAL_JH();
			AssertHasError("Expected Error : ", amendingLine.AL_JHInfo, "You must select a job for this charge code.");

			amendingTransactionAsInvoicingBase.SetContext(BusinessContext.SystemCreatedAmending);
			amendingLine.Validation.ValidateAL_JH();
			AssertNoErrors(amendingLine.AL_JHInfo);

			amendingLine.AL_JH = job.PK;
			amendingLine.Validation.ValidateAL_JH();
			AssertNoErrors(amendingLine.AL_JHInfo);
		}

		public void TestCheckGenericChargeAndCheckAL_JHAgainstGenericCharge()
		{
			AccGLHeader accountPL = TestObjectCreator.CreateGLHeader();
			accountPL.AG_AccountType = Constants.AccountType.ProfitAndLossAccount;

			AccGLHeader accountBSHControl = TestObjectCreator.CreateGLHeader();
			accountBSHControl.AG_AccountType = Constants.AccountType.BalanceSheetAccount;
			accountBSHControl.AG_ControlAccount = ZBool.True;

			AccGLHeader accountBSH = TestObjectCreator.CreateGLHeader();
			accountBSH.AG_AccountType = Constants.AccountType.BalanceSheetAccount;
			accountBSH.AG_ControlAccount = ZBool.False;

			AccGLHeader accountTTL = TestObjectCreator.CreateGLHeader();
			accountTTL.AG_AccountType = Constants.AccountType.Total;

			AccGLHeader accountHDR = TestObjectCreator.CreateGLHeader();
			accountHDR.AG_AccountType = Constants.AccountType.Header;

			AccGLHeader accountCLN = TestObjectCreator.CreateGLHeader();
			accountCLN.AG_AccountType = Constants.AccountType.Consolidation;

			AccGLHeader accountALT = TestObjectCreator.CreateGLHeader();
			accountALT.AG_AccountType = Constants.AccountType.Alternate;

			AccGLHeader accountPLNoDirectPosting = TestObjectCreator.CreateGLHeader();
			accountPLNoDirectPosting.AG_AccountType = Constants.AccountType.ProfitAndLossAccount;
			accountPLNoDirectPosting.AG_DisallowDirectPosting = ZBool.True;

			AccChargeCode chargeCMT = TestObjectCreator.InsertChargeCode(Constants.ChargeType.Comment);
			AccChargeCode chargeDSB = TestObjectCreator.InsertChargeCode(Constants.ChargeType.Disbursement);
			chargeDSB.AC_AG_CostAccount = TestObjectCreator.GLHeader1.PK;
			AccChargeCode chargeMRG = TestObjectCreator.InsertChargeCode(Constants.ChargeType.Margin);
			chargeMRG.AC_AG_CostAccount = TestObjectCreator.GLHeader1.PK;
			AccChargeCode chargeMJA = TestObjectCreator.InsertChargeCode(Constants.ChargeType.ManualJobAccrual);
			chargeMJA.AC_AG_CostAccount = TestObjectCreator.GLHeader1.PK;
			AccChargeCode chargeNON = TestObjectCreator.InsertChargeCode(Constants.ChargeType.NonAccrual);
			chargeNON.AC_AG_CostAccount = TestObjectCreator.GLHeader1.PK;
			AccChargeCode chargeOVR = TestObjectCreator.InsertChargeCode(Constants.ChargeType.Overhead);
			chargeOVR.AC_AG_CostAccount = TestObjectCreator.GLHeader1.PK;
			AccChargeCode chargeREV = TestObjectCreator.InsertChargeCode(Constants.ChargeType.Revenue);

			Factory.Save();

			APInvoice invoice = Factory.New<APInvoice>();
			InvoicingLineBase invoiceLine = (InvoicingLineBase)invoice.Lines.AddNew();
			InvoicingLineBaseValidation validation = (InvoicingLineBaseValidation)invoiceLine.Validation;
			const string validSelectionMessage = "Enter a valid selection.";

			invoiceLine.GenericCharge = accountPL.PK;
			AssertNoErrors("P&L account should not cause error.", invoiceLine.GenericChargeInfo);
			AssertNoErrors("P&L account should not cause error.", invoiceLine.AL_JHInfo);

			invoiceLine.GenericCharge = accountBSHControl.PK;
			AssertHasError("BSH control account should cause error", invoiceLine.GenericChargeInfo, validSelectionMessage);
			AssertNoErrors("BSH control account should not cause error.", invoiceLine.AL_JHInfo);

			invoiceLine.GenericCharge = accountBSH.PK;
			AssertNoErrors("BSH account should not cause error.", invoiceLine.GenericChargeInfo);
			AssertNoErrors("BSH account should not cause error.", invoiceLine.AL_JHInfo);

			invoiceLine.GenericCharge = accountTTL.PK;
			AssertHasError("TTL account should cause error.", invoiceLine.GenericChargeInfo, validSelectionMessage);
			AssertNoErrors("TTL account should not cause error.", invoiceLine.AL_JHInfo);

			invoiceLine.GenericCharge = accountHDR.PK;
			AssertHasError("HDR account should cause error.", invoiceLine.GenericChargeInfo, validSelectionMessage);
			AssertNoErrors("HDR account should not cause error.", invoiceLine.AL_JHInfo);

			invoiceLine.GenericCharge = accountCLN.PK;
			AssertHasError("CLN account should cause error", invoiceLine.GenericChargeInfo, validSelectionMessage);
			AssertNoErrors("CLN account should not cause error.", invoiceLine.AL_JHInfo);

			invoiceLine.GenericCharge = accountALT.PK;
			AssertHasError("ALT account should cause error.", invoiceLine.GenericChargeInfo, validSelectionMessage);
			AssertNoErrors("ALT account should not cause error.", invoiceLine.AL_JHInfo);

			invoiceLine.GenericCharge = accountPLNoDirectPosting.PK;
			AssertHasError("Non-direct posting P&L account should cause error.", invoiceLine.GenericChargeInfo, validSelectionMessage);
			AssertNoErrors("Non-direct posting P&L account should not cause error.", invoiceLine.AL_JHInfo);

			invoiceLine.GenericCharge = chargeCMT.PK;
			AssertNoErrors("CMT charge should not cause error.", invoiceLine.GenericChargeInfo);
			AssertNoErrors("CMT charge should not cause error.", invoiceLine.AL_JHInfo);

			invoiceLine.GenericCharge = chargeDSB.PK;
			AssertHasError("DSB charge should cause error.", invoiceLine.GenericChargeInfo, "This charge code cannot be chosen here.");
			AssertHasError("DSB charge should cause error.", invoiceLine.AL_JHInfo, "You must select a job for this charge code.");

			invoiceLine.GenericCharge = chargeMRG.PK;
			AssertHasError("MRG charge should cause error.", invoiceLine.GenericChargeInfo, "This charge code cannot be chosen here.");
			AssertHasError("MRG charge should cause error.", invoiceLine.AL_JHInfo, "You must select a job for this charge code.");

			invoiceLine.GenericCharge = chargeMJA.PK;
			AssertHasError("MJA charge should cause error.", invoiceLine.GenericChargeInfo, "This charge code cannot be chosen here.");
			AssertHasError("MJA charge should cause error.", invoiceLine.AL_JHInfo, "You must select a job for this charge code.");

			invoiceLine.GenericCharge = chargeNON.PK;
			AssertNoErrors("NON charge should not cause error.", invoiceLine.GenericChargeInfo);
			AssertNoErrors("NON charge should not cause error.", invoiceLine.AL_JHInfo);

			invoiceLine.GenericCharge = chargeOVR.PK;
			AssertNoErrors("OVR charge should not cause error.", invoiceLine.GenericChargeInfo);
			AssertNoErrors("OVR charge should not cause error.", invoiceLine.AL_JHInfo);

			invoiceLine.GenericCharge = chargeREV.PK;
			AssertHasError("REV charge should cause error.", invoiceLine.GenericChargeInfo, validSelectionMessage);
			AssertNoErrors("REV charge should not cause error.", invoiceLine.AL_JHInfo);

			invoiceLine.AL_JH = TestObjectCreator.Job1.PK;

			invoiceLine.GenericCharge = chargeCMT.PK;
			AssertNoErrors("CMT charge should not cause error.", invoiceLine.GenericChargeInfo);
			AssertNoErrors("CMT charge should not cause error.", invoiceLine.AL_JHInfo);

			invoiceLine.GenericCharge = chargeDSB.PK;
			AssertNoErrors("DSB charge should not cause error.", invoiceLine.GenericChargeInfo);
			AssertNoErrors("DSB charge should not cause error.", invoiceLine.AL_JHInfo);

			invoiceLine.GenericCharge = chargeMRG.PK;
			AssertNoErrors("MRG charge should not cause error.", invoiceLine.GenericChargeInfo);
			AssertNoErrors("MRG charge should not cause error.", invoiceLine.AL_JHInfo);

			invoiceLine.GenericCharge = chargeNON.PK;
			AssertNoErrors("NON charge should not cause error.", invoiceLine.GenericChargeInfo);
			AssertHasError("NON charge should cause error.", invoiceLine.AL_JHInfo, "You cannot select a job for a charge of type NON.");

			invoiceLine.GenericCharge = chargeOVR.PK;
			AssertNoErrors("OVR charge should not cause error.", invoiceLine.GenericChargeInfo);
			AssertHasError("OVR charge should cause error.", invoiceLine.AL_JHInfo, "You cannot select a job for a charge of type OVR.");

			invoiceLine.GenericCharge = chargeREV.PK;
			AssertHasError("REV charge should cause error.", invoiceLine.GenericChargeInfo, validSelectionMessage);
			AssertNoErrors("REV charge should not cause error.", invoiceLine.AL_JHInfo);
		}

		public void TestCheckAL_OSExAmount()
		{
			InvoicingBase invoice = (InvoicingBase)Factory.New(InvoiceType);
			InvoicingLineBase line = (InvoicingLineBase)invoice.Lines.AddNew();
			AccChargeCode chargeCode = CreateChargeCode("NCCC", "Non Comment Charge Code 1", Constants.ChargeType.Revenue, 80, null, null);
			line.AL_AC = chargeCode.PK;

			invoice.RunPreSaveValidation();

			AssertEquals("OS Ex Tax Amt", 0.00M, line.AL_OSExTaxAmount);
			AssertEquals("OS Total Amt ", 0.00M, line.AL_OverseasTotal);
			AssertHasErrors("OS Ex Tax Amt is only 0.00M for Comment type charge codes ", line.AL_OSExTaxAmountInfo);

			invoice = (InvoicingBase)Factory.New(InvoiceType);
			line = (InvoicingLineBase)invoice.Lines.AddNew();
			chargeCode = CreateChargeCode("CMT", "Comment", Constants.ChargeType.Comment, 0, null, null);
			line.AL_AC = chargeCode.PK;

			invoice.RunPreSaveValidation();

			AssertEquals("OS Ex Tax Amt", 0.00M, line.AL_OSExTaxAmount);
			AssertEquals("OS Total Amt ", 0.00M, line.AL_OverseasTotal);
			AssertNoErrors("OS Ex Tax Amt is only 0.00M for Comment type charge codes ", line.AL_OSExTaxAmountInfo);
		}

		public void TestCheckAL_OSExTaxAmountInMatchingContext()
		{
			InvoicingBase invoice = (InvoicingBase)Factory.New(InvoiceType);
			InvoicingLineBase line = (InvoicingLineBase)invoice.Lines.AddNew();
			line.AL_OSExTaxAmount = 100m;
			IMatchingCollection matchingBizOs = new IMatchingCollection(Factory);
			matchingBizOs.Add(invoice);
			invoice.RunPreSaveValidation();
			AssertEquals("OSExTax on header should be 100", 100m, invoice.AH_OSExTaxAmount);
		}

		public virtual void TestCheckAL_ATWhenRegisteredCompanyAndNonRegisteredDebtor()
		{
			bool oldPayableAllowUserToModifyGSTId = AccountingConfigurationRegistry.Instance.PayableAllowUserToModifyGSTId.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK);
			try
			{
				AccountingConfigurationRegistry.Instance.PayableAllowUserToModifyGSTId.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);

				GlbCompany.CurrentCompany.GC_IsGSTRegistered = ZBool.True;
				InvoicingBase invoice = (InvoicingBase)Factory.New(InvoiceType);
				invoice.Lines.AddNew();
				invoice.AH_OH = GSTRegisteredOrg.PK;

				invoice.Lines[0].AL_AT = ZGuid.Empty;
				invoice.Lines[0].Validation.ValidateAL_AT();
				Assert("Should validate with error", invoice.Lines[0].AL_ATInfo.HasErrors());

				invoice.Lines[0].AL_AT = GST10TaxRate.PK;
				invoice.Lines[0].Validation.ValidateAL_AT();
				Assert("Should not error with valid code", !invoice.Lines[0].AL_ATInfo.HasErrors());
			}
			finally
			{
				AccountingConfigurationRegistry.Instance.PayableAllowUserToModifyGSTId.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, oldPayableAllowUserToModifyGSTId);
			}
		}

		[SuspendGLAccountAndChargeCodeCriticalValidation]
		public void TestJobContainsUnpostedApportionmentsUsesNewFactory()
		{
			AssertNotNull(TestJob);
			Factory.Save();

			InvoicingBase invoice = (InvoicingBase)Factory.New(InvoiceType);
			invoice.AH_TransactionNum = "101";
			invoice.Lines.AddNew();

			InvoicingLineBaseValidation validation = invoice.Lines[0].Validation as InvoicingLineBaseValidation;
			Assert("Should not contain any unposted apportionments yet for either charge code", validation.JobContainsUnpostedApportionments(TestJob.PK, Margin100Code.PK) == null);
			Assert("Should not contain any unposted apportionments yet for either charge code", validation.JobContainsUnpostedApportionments(TestJob.PK, DisbursementChargeCode.PK) == null);

			AccTransactionLines postedApportionChargeAPLine = Factory.NewWithValidTestData<APInvoice>().Lines.AddNew();
			postedApportionChargeAPLine.AL_GE = GlbDepartment.CurrentDepartment.PK;
			postedApportionChargeAPLine.AL_GB = GlbBranch.CurrentBranch.PK;
			postedApportionChargeAPLine.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Cost;
			postedApportionChargeAPLine.AL_OSAmount = -10m;
			postedApportionChargeAPLine.AL_LineAmount = -10m;

			Charge postedApportionCharge = TestJob.Charges.AddNew();
			postedApportionCharge.JR_AL_APLine = postedApportionChargeAPLine.PK;
			postedApportionCharge.JR_AC = Margin100Code.PK;
			postedApportionCharge.JR_OSCostAmt = 10m;
			postedApportionCharge.JR_LocalCostAmt = 10m;

			AccTransactionLines postedApportionChargeARLine = Factory.NewWithValidTestData<ARInvoice>().Lines.AddNew();
			postedApportionChargeARLine.AL_GE = GlbDepartment.CurrentDepartment.PK;
			postedApportionChargeARLine.AL_GB = GlbBranch.CurrentBranch.PK;
			postedApportionChargeARLine.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Revenue;
			postedApportionChargeARLine.AL_OSAmount = 10m;
			postedApportionChargeARLine.AL_LineAmount = 10m;

			Charge unpostedApportionment = TestJob.Charges.AddNew(); // As yet this charge is not an apportionment, 
																	 //becomes one when the cost split group is set, i.e. Line 3078

			unpostedApportionment.JR_AC = Margin100Code.PK;

			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			JobConsolCost cost = consol.GetApportionments().CostsCollection.TryAddNew();
			cost.E6_AC_ChargeCode = DisbursementChargeCode.PK;
			cost.E6_GC = GlbCompany.CurrentCompany.PK;
			cost.E6_OSCostAmount = 10m;
			cost.E6_LocalCostAmount = 10m;
			cost.E6_AH_APInvoice = postedApportionChargeAPLine.AL_AH;
			cost.E6_AH_ARInvoice = postedApportionChargeARLine.AL_AH;

			postedApportionCharge.JR_AL_ARLine = postedApportionChargeARLine.PK;
			postedApportionCharge.JR_E6 = cost.PK;

			Charge unpostedStandardCharge = TestJob.Charges.AddNew();
			unpostedStandardCharge.JR_AC = DisbursementChargeCode.PK;

			Assert("margin charge code used for charge that is not yet an apportionment", validation.JobContainsUnpostedApportionments(TestJob.PK, Margin100Code.PK) == null);
			Assert("DSB charge not used for unposted apportionment charges for DSB", validation.JobContainsUnpostedApportionments(TestJob.PK, DisbursementChargeCode.PK) == null);

			JobConsolCost cost2 = consol.GetApportionments().CostsCollection.TryAddNew();
			cost2.E6_AC_ChargeCode = unpostedApportionment.JR_AC;
			cost2.E6_GC = GlbCompany.CurrentCompany.PK;
			cost2.E6_OSCostAmount = 10m;
			cost2.E6_LocalCostAmount = 10m;
			// This step should still return no unposted apportionment since it is not in DB yet.
			unpostedApportionment.JR_E6 = cost2.PK; // Make the charge an apportionment
			unpostedApportionment.JR_OSCostAmt = 10m;
			unpostedApportionment.JR_LocalCostAmt = 10m;
			Assert("margin charge code used for charge that is now an apportionment", validation.JobContainsUnpostedApportionments(TestJob.PK, Margin100Code.PK) == null);
			Assert("DSB charge not used for unposted apportionment charges for DSB", validation.JobContainsUnpostedApportionments(TestJob.PK, DisbursementChargeCode.PK) == null);

			Factory.Save();

			Assert("margin charge code used for charge that is now an apportionment", validation.JobContainsUnpostedApportionments(TestJob.PK, Margin100Code.PK) != null);
			Assert("DSB charge not used for unposted apportionment charges for DSB", validation.JobContainsUnpostedApportionments(TestJob.PK, DisbursementChargeCode.PK) == null);
		}

		public void TestCheckConsolIDFromApportionedChargWithUXml_ConsolErrorMessages()
		{
			var invoice = (InvoicingBase)Factory.New(InvoiceType);
			var line = (InvoicingLineBase)invoice.Lines.AddNew();
			line.GenericCharge = TestObjectCreator.FRT.PK;

			var validation = ((InvoicingLineBaseValidation)line.Validation);
			validation.ValidateConsolIDFromApportionedCharge();
			var expectedError = "Source XML errors: Some error";
			AssertNoWarning(line.ConsolIDFromApportionedChargeInfo, expectedError);

			line.UXml_ConsolErrorMessages = "Some error";
			Assert("Precondition: ConsolIDFromApportionedCharge is empty", line.ConsolIDFromApportionedCharge.IsEmpty);
			AssertHasWarning(line.ConsolIDFromApportionedChargeInfo, expectedError);

			var consol = TestObjectCreator.CreateConsol("AUSYD", "NZAKL", "C001");
			var shipment = TestObjectCreator.CreateShipment("S001", consol);
			var consolCost = TestObjectCreator.CreateConsolCost(invoice, consol, TestObjectCreator.CC1, 10);
			invoice.ImportSingleCost(consolCost, line);
			validation.ValidateAll();
			Assert("Precondition: ConsolIDFromApportionedCharge is not empty", !line.ConsolIDFromApportionedCharge.IsEmpty);
			AssertNoNotifications(line.ConsolIDFromApportionedChargeInfo);

			invoice.ReleaseAllMutexOnInvoice();
		}

		#region ValidateOriginalJobCharge

		[TestDate(2018, 03, 01)]
		public void TestValidateOriginalJobCharge()
		{
			if (InvoiceType == typeof(APInvoice) || InvoiceType == typeof(APCreditNote))
			{
				var postedInvoice = TestObjectCreator.CreateAPInvoice<APInvoice>("111", TestObjectCreator.AUD, 1M, 100M, 0M, 0M, 100M, 0M, 0M);

				var shipment = TestObjectCreator.CreateShipment("S001001", false);
				var job = TestObjectCreator.CreateJob(shipment, false);
				var jobCharge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, 100M, 0M);
				Factory.Save();

				var invoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), "234", TestObjectCreator.AUD, 1M);
				var line = (InvoicingLineBase)invoice.Lines.AddNew();
				line.AL_JH = job.PK;
				invoice.ImportJobChargesIntoInvoice(new[] { jobCharge }.ToList(), line);

				AssertNotNull("Original job charge is not null", line.OriginalJobCharge);
				Assert("Original job charge not posted yet", !line.OriginalJobCharge.IsCostPosted);

				var validation = ((InvoicingLineBaseValidation)line.Validation);
				validation.ValidateOriginalJobCharge();
				var expectedError = "The associated charge is already posted by another user. Please delete this row and try to post again.";
				AssertNoError("Should not have any error", line.OriginalJobChargePKInfo, expectedError);

				(line.OriginalJobCharge as BaseCharge).ReverseAccrual(ZDateTime.Today);
				line.OriginalJobCharge.JR_AL_APLine = postedInvoice.Lines[0].PK; //this is not realistic, simply forcefully making original job charge posted
				Assert("Original job charge is posted", line.OriginalJobCharge.IsCostPosted);

				validation.ValidateOriginalJobCharge();
				AssertHasError("Should have error now since job charge is linked to CST line", line.OriginalJobChargePKInfo, expectedError);
			}
			else
			{
				Assert("Not applicable", true);
			}
		}

		[TestDate(2018, 03, 01)]
		public void TestValidateOriginalJobChargeWhenOriginalJobChargeIsImportedToAnIncompleteInvoice()
		{
			if (InvoiceType == typeof(APInvoice) || InvoiceType == typeof(APCreditNote))
			{
				var shipment = TestObjectCreator.CreateShipment("S001001", false);
				var job = TestObjectCreator.CreateJob(shipment, false);
				var jobCharge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, 100M, 0M);
				Factory.Save();

				var invoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), "234", TestObjectCreator.AUD, 1M);
				var line = (InvoicingLineBase)invoice.Lines.AddNew();
				line.AL_JH = job.PK;
				invoice.ImportJobChargesIntoInvoice(new[] { jobCharge }.ToList(), line);
				AssertNotNull("Original job charge is not null", line.OriginalJobCharge);
				Assert("Original job charge not posted yet", !line.OriginalJobCharge.IsCostPosted);
				invoice.SaveAsIncomplete();

				var newFactory = new BusinessObjectFactory();
				var reloadedCharge = newFactory.Load<Charge>(jobCharge.PK);
				var postedInvoice = new TestObjectCreator(newFactory).CreateInvoice(typeof(APInvoice), "111", TestObjectCreator.AUD, 1M);
				var newline = (InvoicingLineBase)postedInvoice.Lines.AddNew();
				newline.AL_JH = job.PK;
				invoice.ImportJobChargesIntoInvoice(new[] { reloadedCharge }.ToList(), newline);

				var validation = ((InvoicingLineBaseValidation)newline.Validation);
				validation.ValidateOriginalJobCharge();
				var expectedError = "The associated charge is already used in an Incomplete Invoice 234 dated 01 Mar 2018. Please delete this row. If required, you can manually enter a new invoice line without importing the accrual.";
				AssertHasError("Should have an error", newline.OriginalJobChargePKInfo, expectedError);
			}
			else
			{
				Assert("Not applicable", true);
			}
		}

		[TestDate(2018, 03, 01)]
		public void TestValidateOriginalJobChargeWhenOriginalJobConsolCostIsImportedToAnIncompleteInvoice()
		{
			if (InvoiceType == typeof(APInvoice) || InvoiceType == typeof(APCreditNote))
			{
				var consol = TestObjectCreator.CreateConsol();
				var shipment = TestObjectCreator.CreateShipment("S0001", consol);
				_ = TestObjectCreator.CreateJob(shipment);
				var cost = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.CC1, 250M);
				Factory.Save();

				var incompleteInvoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), "00001");
				var importer = new InvoicingBaseBulkConsolCostImporter(incompleteInvoice.ConsolCosting);
				importer.LoadConsolsCollection();
				importer.Import();
				incompleteInvoice.ImportAllApportionmentsFromCosting();
				incompleteInvoice.SaveAsIncomplete();

				var newFactory = new BusinessObjectFactory();
				var invoice2 = new TestObjectCreator(newFactory).CreateInvoice(typeof(APInvoice), "111", TestObjectCreator.AUD, 1M);
				importer = new InvoicingBaseBulkConsolCostImporter(invoice2.ConsolCosting);
				importer.LoadConsolsCollection();
				importer.Import();
				invoice2.ImportAllApportionmentsFromCosting();

				var validation = ((InvoicingLineBaseValidation)invoice2.Lines[0].Validation);
				validation.ValidateOriginalJobCharge();
				var expectedError = "The associated charge is already used in an Incomplete Invoice 00001 dated 01 Mar 2018. Please delete this row. If required, you can manually enter a new invoice line without importing the accrual.";
				AssertHasError("Should have an error", invoice2.Lines[0].OriginalJobChargePKInfo, expectedError);
			}
			else
			{
				Assert("Not applicable", true);
			}
		}

		[TestDate(2018, 03, 01)]
		public void TestValidateOriginalJobChargeWhenIncompleteInvoiceIsReloaded()
		{
			if (InvoiceType == typeof(APInvoice) || InvoiceType == typeof(APCreditNote))
			{
				var shipment = TestObjectCreator.CreateShipment("S0001");
				var job = TestObjectCreator.CreateJob(shipment);
				var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, 200M, 200M);
				Factory.Save();

				var incompleteInvoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), "00001");
				var line = incompleteInvoice.Lines.AddNew() as InvoicingLineBase;
				incompleteInvoice.ImportJobChargesIntoInvoice(new[] { charge }, line, false);
				incompleteInvoice.SaveAsIncomplete();

				var newFactory = new BusinessObjectFactory();
				var apInvoice = newFactory.Load<APInvoice>(incompleteInvoice.PK);
				apInvoice.RestoreSavedData();
				var lines = apInvoice.Lines.OfType<InvoicingLineBase>().ToList();
				AssertEquals("Invoice Line Count", 1, lines.Count);

				var invLine = lines[0];
				AssertNotNull(invLine.OriginalJobCharge);
				var validation = ((InvoicingLineBaseValidation)invLine.Validation);
				validation.ValidateOriginalJobCharge();
				AssertNoErrors("Should have no error", invLine.OriginalJobChargePKInfo);

				apInvoice.MoveFromIncompleteToPayableLedger();
				validation = ((InvoicingLineBaseValidation)invLine.Validation);
				validation.ValidateOriginalJobCharge();
				AssertNoErrors("Should have no error", invLine.OriginalJobChargePKInfo);
			}
			else
			{
				Assert("Not applicable", true);
			}
		}

		#endregion

		#region CheckAL_JH

		public virtual void TestCheckAL_JHErrorMessage()
		{
			const string expectBaseError = "Job billing record for this job does not exist. To rectify, click on the billing tab of the job and save.";

			var invoice = (InvoicingBase)Factory.New(InvoiceType);
			var line = (InvoicingLineBase)invoice.Lines.AddNew();
			line.GenericCharge = TestObjectCreator.FRT.PK;

			line.AL_JH = ZGuid.Invalid;
			line.Validation.ValidateAL_JH();
			AssertHasError("Precondition: AL_JH error", line.AL_JHInfo, expectBaseError);

			var jobInOtherCompany = Factory.NewJobWithValidTestDataForTesting<Job>();
			jobInOtherCompany.JH_GC = TestObjectCreator.NonCurrentCompany.PK;
			line.AL_JH = jobInOtherCompany.PK;
			AssertHasError("Precondition: AL_JH error", line.AL_JHInfo, expectBaseError);

			line.AL_JH = Factory.NewJobWithValidTestDataForTesting<Job>().PK;
			AssertNoErrors(line.AL_JHInfo);
		}

		public virtual void TestCheckAL_JHWithUXml_JobErrorMessages()
		{
			var invoice = (InvoicingBase)Factory.New(InvoiceType);
			var line = (InvoicingLineBase)invoice.Lines.AddNew();
			line.GenericCharge = TestObjectCreator.FRT.PK;

			line.AL_JH = ZGuid.Invalid;
			line.Validation.ValidateAL_JH();
			var expectBaseError = "Job billing record for this job does not exist. To rectify, click on the billing tab of the job and save.";
			AssertHasError("Precondition: AL_JH error", line.AL_JHInfo, expectBaseError);
			var expectedError = "Source XML errors: Some error";
			AssertNoWarning(line.AL_JHInfo, expectedError);

			line.UXml_JobErrorMessages = "Some error";
			AssertHasError("Precondition: AL_JH error", line.AL_JHInfo, expectBaseError);
			AssertHasWarning(line.AL_JHInfo, expectedError);

			line.AL_JH = Factory.NewJobWithValidTestDataForTesting<Job>().PK;
			AssertNoNotifications(line.AL_JHInfo);
		}

		public virtual void TestCheckAL_JH_ReopenClosedJobDenied()
		{
			Job job = TestObjectCreator.CreateAndSaveTestShipmentJob(JobHeaderStatus.Closed.Code);
			InvoicingBase invoice = (InvoicingBase)Factory.New(InvoiceType);
			invoice.AH_OH = GSTRegisteredOrg.PK;

			InvoicingLineBase line = (InvoicingLineBase)invoice.Lines.AddNew();
			bool oldAllowReopenJob = Env.Security.ReopenJob.IsAllowed;

			try
			{
				Env.Security.ReopenJob.IsAllowed = false;
				line.AL_JH = job.PK;
				invoice.RunPreSaveValidation();

				var skipTypes = new[] {
					typeof(ARInvoice),
					typeof(ARCreditNote)
				};
				if (skipTypes.Contains(InvoiceType))
				{
					AssertEquals($"{InvoiceType.Name}, Should NOT have Warning", false, line.AL_JHInfo.HasWarning(InvoicingLineBaseValidation.ReopenClosedJobSecurityMessage));
				}
				else
				{
					invoice.RunPreSaveValidation();
					AssertNoError(line.AL_JHInfo, InvoicingLineBaseValidation.ReopenClosedJobSecurityMessage);
					AssertHasWarning(line.AL_JHInfo, InvoicingLineBaseValidation.ReopenClosedJobSecurityMessage);

					invoice.Factory.SetContext(BusinessContext.AllowReopenJobWhenImporting);
					invoice.RunPreSaveValidation();
					AssertNoError(line.AL_JHInfo, InvoicingLineBaseValidation.ReopenClosedJobSecurityMessage);
					AssertNoWarning(line.AL_JHInfo, InvoicingLineBaseValidation.ReopenClosedJobSecurityMessage);
				}
			}
			finally
			{
				Env.Security.ReopenJob.IsAllowed = oldAllowReopenJob;
			}
		}

		public void TestCheckAL_JH_ReopenClosedJobAllowed()
		{
			ForwardingShipment shipment = TestObjectCreator.CreateAndSaveTestForwardingShipmentJob(JobHeaderStatus.Closed.Code);

			using (Job job = TestObjectCreator.CreateJob(shipment))
			{
				InvoicingBase invoice = (InvoicingBase)Factory.New(InvoiceType);
				invoice.AH_OH = GSTRegisteredOrg.PK;

				InvoicingLineBase line = (InvoicingLineBase)invoice.Lines.AddNew();
				bool oldAllowReopenJob = Env.Security.ReopenJob.IsAllowed;

				try
				{
					Env.Security.ReopenJob.IsAllowed = true;
					line.AL_JH = job.PK;
					invoice.RunPreSaveValidation();

					if (InvoiceType == typeof(ARInvoice))
					{
						AssertEquals("AR Invoice, Should NOT have Warning", false, line.AL_JHInfo.HasWarning(InvoicingLineBaseValidation.ReopenClosedJobSecurityMessage));
					}
					else
					{
						AssertEquals("ReopenJob.IsAllowed = true, Should NOT have Warning", false, line.AL_JHInfo.HasWarning(InvoicingLineBaseValidation.ReopenClosedJobSecurityMessage));
					}
				}
				finally
				{
					Env.Security.ReopenJob.IsAllowed = oldAllowReopenJob;
				}
			}
		}

		public virtual void TestCheckAL_JHForSelectedGLTypeCharge()
		{
			ForwardingShipment shipment = TestObjectCreator.CreateShipment("S00001234");
			Job job = TestObjectCreator.CreateJob(shipment);
			Factory.Save();

			InvoicingBase invoice = (InvoicingBase)Factory.New(InvoiceType);
			InvoicingLineBase line = (InvoicingLineBase)invoice.Lines.AddNew();

			ZQuery testFilter = new ZQuery(ViewGenericChargeSchema.VC_Type, "P&L");
			testFilter.AddToFilter(ViewGenericChargeSchema.VC_DisallowDirectPosting, false);
			GenericCharge.GenericCharge testGenericCharge = Factory.LoadTop1(typeof(GenericCharge.GenericCharge), testFilter) as GenericCharge.GenericCharge;
			line.GenericCharge = testGenericCharge.PK;
			Assert("Precondition: AL_JH should be validated", line.ShouldValidateJob);
			((InvoicingLineBaseValidation)line.Validation).ValidateAL_JH();

			Assert("Should not have error: 'You cannot select a job for a GL Account charge code.", !line.AL_JHInfo.HasError("You cannot select a job for a GL Account charge code."));
			Assert("Should not have error: 'You must select a job for this charge code.'", !line.AL_JHInfo.HasError("You must select a job for this charge code."));

			line.AL_JH = job.PK;
			Assert("Precondition: AL_JH should be validated", line.ShouldValidateJob);
			((InvoicingLineBaseValidation)line.Validation).ValidateAL_JH();

			Assert("Should have error: 'You cannot select a job for a GL Account charge code.'", line.AL_JHInfo.HasError("You cannot select a job for a GL Account charge code."));
			Assert("Should not have error: 'You must select a job for this charge code.'", !line.AL_JHInfo.HasError("You must select a job for this charge code."));
		}

		public virtual void TestCheckAL_JHForSelectedCharge()
		{
			ForwardingShipment testShipment = TestObjectCreator.CreateShipment("S00001234");
			Job testJob = TestObjectCreator.CreateJob(testShipment);

			ZQuery testFilter = new ZQuery(AccChargeCodeSchema.AC_ChargeType, "MRG");
			AccChargeCode chargeCode = Factory.LoadTop1(typeof(AccChargeCode), testFilter) as AccChargeCode;
			chargeCode.AC_DepartmentFilterList = "ALL";

			Factory.Save();

			InvoicingBase invoice = (InvoicingBase)Factory.New(InvoiceType);
			InvoicingLineBase line = (InvoicingLineBase)invoice.Lines.AddNew();

			line.GenericCharge = chargeCode.PK;
			Assert("Precondition: AL_JH should be validated", line.ShouldValidateJob);
			((InvoicingLineBaseValidation)line.Validation).ValidateAL_JH();

			Assert("Should have error: 'You must select a job for this charge code.'", line.AL_JHInfo.HasError("You must select a job for this charge code."));
			// Assert("Should not have error: 'You cannot select a job for a GL Account charge code.'", !Line.AL_JHInfo.HasError("You cannot select a job for a GL Account charge code."));

			line.AL_JH = testJob.PK;
			Assert("Precondition: AL_JH should be validated", line.ShouldValidateJob);
			((InvoicingLineBaseValidation)line.Validation).ValidateAL_JH();

			Assert("Should not have error: 'You must select a job for this charge code.'", !line.AL_JHInfo.HasError("You must select a job for this charge code."));
			// Assert("Should not have error: 'You cannot select a job for a GL Account charge code.'", !Line.AL_JHInfo.HasError("You cannot select a job for a GL Account charge code."));
		}

		public void TestCheckAL_JH_ExceptionNullReference()
		{
			Env.Security.ReopenJob.IsAllowed = false;

			InvoicingLineBase invoiceLine = Factory.NewWithValidTestData<ARInvoiceLine>();
			invoiceLine.AL_JH = ZGuid.Empty;

			Job parentJob = Factory.NewJobWithValidTestDataForTesting<Job>();
			parentJob.JH_Status = JobHeaderStatus.Closed.Code;

			AssertNull(invoiceLine.InvoiceBase);
			AssertNoExceptionThrown("Should not return Object reference not set", () => invoiceLine.AL_JH = parentJob.PK);
		}

		public void TestCheckAL_JH_RevenueRecognitionDateWhenAPInvoiceIsReadOnlyAndIsConvertedFromARInvoice()
		{
			RevenueRecognitionCollection registryCollection = new RevenueRecognitionCollection();
			RevenueRecognition registryValue = registryCollection.AddNew();
			registryValue.JobType = JobInvoicingConsumerTypes.Shipment.Code;
			registryValue.DirectionCode = Constants.FreightShipmentDirection.Code.All;
			registryValue.Mode = RevenueRecognitionLookups.ModeAdditionalCodes.All;
			registryValue.RecognitionDateOptionCode = RevenueRecognitionLookups.RecognitionDateOptionCodes.ActualArrivalDate;
			registryValue.RunPreSaveValidation();

			AccountingConfigurationRegistry.Instance.RevenueRecognitionSetup.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, registryCollection);

			TestCaseHelper.ClearTable("AccPeriodManagement");
			AccountingPeriodTestHelper periodHelper = new AccountingPeriodTestHelper(Factory);
			periodHelper.SetupSinglePeriod(1, ZDateTime.Today.AddMonths(-1), ZDateTime.Today.AddMonths(1));

			Job testJob = TestObjectCreator.CreateJob("Z00001000", TestObjectCreator.LocalClient, 5M, TestObjectCreator.Agent, 10M);
			ForwardingShipment shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			testJob.Parent = shipment;
			TestObjectCreator.CreateCharge(testJob, TestObjectCreator.CC1, "Charge Code 1", TestObjectCreator.AUD, 100M, Creditor1,
			TestObjectCreator.AUD, 150M, TestObjectCreator.LocalClient);
			testJob.LocalChargesPK = ZGuid.Empty;
			Factory.Save();
			APInvoice invoice = Factory.New<APInvoice>();
			APInvoiceLine line1 = (APInvoiceLine)invoice.Lines.AddNew();

			AccountingConfigurationRegistry.Instance.CreateWIPOrAccrualWhenNoInvoicesPosted.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);
			line1.AL_JH = testJob.PK;
			line1.AL_AC = testJob.Charges[0].ChargeCode.PK;

			invoice.IsConvertedFromARInvoice = true;
			invoice.SetReadOnlyIncludingChildren(true);

			line1.Validation.ValidateAL_JH();
			AssertEquals("Line AL_JH should have error", true, line1.AL_JHInfo.HasErrors());
			AssertHasError("The error should be this:", line1.AL_JHInfo, "This invoice cannot be posted until the 'Actual/Estimated Arrival Date' for this job is recorded. This job and charge code combination requires this date for revenue recognition purposes.");
		}

		public void TestValidationErrorShownWhenRevenueRecognitionTypeNotSetForLineFromJobCharge()
		{
			if (InvoiceType == typeof(APInvoice) || InvoiceType == typeof(APCreditNote))
			{
				RevenueRecognitionCollection valuesForTest = new RevenueRecognitionCollection();
				RevenueRecognition setting = valuesForTest.AddNew();
				setting.JobType = JobInvoicingConsumerTypes.Shipment.Code;
				setting.DirectionCode = Constants.FreightShipmentDirection.Code.Import;
				setting.Mode = Core.Constants.TransportModes.Sea;
				setting.RecognitionDateOptionCode = RevenueRecognitionLookups.RecognitionDateOptionCodes.Immediate;
				setting.BrokerCode = RevenueRecognitionLookups.BrokerCodes.All;
				AccountingConfigurationRegistry.Instance.RevenueRecognitionSetup.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, valuesForTest);

				var factory1 = new BusinessObjectFactory();
				var testObjectCreator = new TestObjectCreator(factory1);
				var shipment = testObjectCreator.CreateShipment("S001001", "AUMEL", "NZAKL", transportMode: Constants.TransportModes.Air, saveIt: false);
				var job = testObjectCreator.CreateJob(shipment, false);
				var jobChargeInFactory1 = testObjectCreator.CreateCharge(job, TestObjectCreator.CC1, 100M, 0M);
				factory1.Save();

				var factory2 = new BusinessObjectFactory();
				var invoice = (InvoicingBase)factory2.New(InvoiceType);
				invoice.SubmittedFromInvoicingForm = true;
				invoice.AH_TransactionNum = "23433";
				var line = (InvoicingLineBase)invoice.Lines.AddNew();
				line.AL_JH = job.PK;
				var jobCharge = factory2.Load<JobCharge>(jobChargeInFactory1.PK);
				invoice.ImportJobChargesIntoInvoice(new[] { jobCharge }.ToList<Charge>(), line);
				Assert("Precondition: Job is applicable so should be validated", line.IsJobApplicable);
				Assert("Precondition: line imported from job charge", line.IsPopulatedFromImportedJobCharge);
				Assert("Precondition: AL_JH is readonly", line.AL_JHInfo.ReadOnly);
				Assert("Precondition: AL_JH should be validated", line.ShouldValidateJob);

				line.Validation.ValidateAL_JH();

				Assert("Line AL_JH should have error", line.AL_JHInfo.HasErrors());

				var expectedErrorMessage = @"You have not setup Revenue Recognition for this job type. Go to Registry -> Accounting -> Job Invoicing -> Revenue Recognition Setup to configure Revenue Recognition.";
				AssertHasError(line.AL_JHInfo, expectedErrorMessage);
			}
			else
			{
				Assert("Not applicable", true);
			}
		}

		public void TestCheckAL_JH_RevenueRecognitionDate()
		{
			RevenueRecognitionCollection registryCollection = new RevenueRecognitionCollection();
			RevenueRecognition registryValue = registryCollection.AddNew();
			registryValue.JobType = JobInvoicingConsumerTypes.Shipment.Code;
			registryValue.DirectionCode = Constants.FreightShipmentDirection.Code.All;
			registryValue.Mode = RevenueRecognitionLookups.ModeAdditionalCodes.All;
			registryValue.RecognitionDateOptionCode = RevenueRecognitionLookups.RecognitionDateOptionCodes.ActualArrivalDate;
			registryValue.RunPreSaveValidation();

			AccountingConfigurationRegistry.Instance.RevenueRecognitionSetup.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, registryCollection);

			TestCaseHelper.ClearTable("AccPeriodManagement");
			AccountingPeriodTestHelper periodHelper = new AccountingPeriodTestHelper(Factory);
			periodHelper.SetupSinglePeriod(1, ZDateTime.Today.AddMonths(-1), ZDateTime.Today.AddMonths(1));

			Job testJob = TestObjectCreator.CreateJob("Z00001000", TestObjectCreator.LocalClient, 5M, TestObjectCreator.Agent, 10M);
			ForwardingShipment shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			testJob.Parent = shipment;
			TestObjectCreator.CreateCharge(testJob, TestObjectCreator.CC1, "Charge Code 1", TestObjectCreator.AUD, 100M, Creditor1,
			TestObjectCreator.AUD, 150M, TestObjectCreator.LocalClient);
			testJob.LocalChargesPK = ZGuid.Empty;
			Factory.Save();
			APInvoice invoice = Factory.New<APInvoice>();
			APInvoiceLine line1 = (APInvoiceLine)invoice.Lines.AddNew();

			AccountingConfigurationRegistry.Instance.CreateWIPOrAccrualWhenNoInvoicesPosted.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);
			line1.AL_JH = testJob.PK;
			line1.AL_AC = testJob.Charges[0].ChargeCode.PK;
			line1.Validation.ValidateAL_JH();
			AssertEquals("Line AL_JH should have error", true, line1.AL_JHInfo.HasErrors());
			AssertHasError("The error should be this:", line1.AL_JHInfo, "This invoice cannot be posted until the 'Actual/Estimated Arrival Date' for this job is recorded. This job and charge code combination requires this date for revenue recognition purposes.");

			AccountingConfigurationRegistry.Instance.CreateWIPOrAccrualWhenNoInvoicesPosted.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			line1.Validation.ValidateAL_JH();
			AssertEquals("Line AL_JH should have error", true, line1.AL_JHInfo.HasErrors());
			AssertHasError("The error should be this:", line1.AL_JHInfo, "This invoice cannot be posted until the 'Actual/Estimated Arrival Date' for this job is recorded. This job and charge code combination requires this date for revenue recognition purposes.");

			RevenueRecognitionCollection valuesForTest = new RevenueRecognitionCollection();
			RevenueRecognition setting1 = valuesForTest.AddNew();
			setting1.JobType = JobInvoicingConsumerTypes.Shipment.Code;
			setting1.DirectionCode = Constants.FreightShipmentDirection.Code.All;
			setting1.Mode = Core.Constants.TransportModes.Air;
			setting1.RecognitionDateOptionCode = RevenueRecognitionLookups.RecognitionDateOptionCodes.PickupDate;
			AccountingConfigurationRegistry.Instance.RevenueRecognitionSetup.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, valuesForTest);
			testJob.ClearCacheOfRevenueRecognitionRegistryItems_ForTestsOnly();

			shipment.JS_RL_NKOrigin = "USLAX";
			shipment.JS_RL_NKDestination = "AUSYD";
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			line1.AL_AC = testJob.Charges[0].ChargeCode.PK;
			AssertEquals("Precondition: ", RevenueRecognitionLookups.RecognitionDateOptionCodes.PickupDate, line1.AL_RevRecognitionType);

			shipment.DocsAndCartage.JP_PickupCartageCompleted = ZDateTime.Now.AddDays(10);
			line1.Validation.ValidateAL_JH();
			AssertNoErrors("Line AL_JH should not have error", line1.AL_JHInfo);

			shipment.DocsAndCartage.JP_PickupCartageCompleted = ZDateTime.BrettsBirthday;
			line1.Validation.ValidateAL_JH();
			AssertHasErrorContaining(line1.AL_JHInfo, ZDateTime.BrettsBirthday.Date.ToShortDateString());

			TestObjectCreator.CreateJobChargeRevRecognition(testJob, "PIC", ZDateTime.Now);

			line1.Validation.ValidateAL_JH();
			AssertNoErrors("Line AL_JH should not have error", line1.AL_JHInfo);

			registryValue.RecognitionDateOptionCode = RevenueRecognitionLookups.RecognitionDateOptionCodes.PostDateOfFirstARTransaction;
			registryValue.RunPreSaveValidation();
			AccountingConfigurationRegistry.Instance.RevenueRecognitionSetup.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, registryCollection);
			testJob.ClearCacheOfRevenueRecognitionRegistryItems_ForTestsOnly();

			line1.AL_AC = testJob.Charges[0].ChargeCode.PK;
			AssertEquals("Precondition: ", RevenueRecognitionLookups.RecognitionDateOptionCodes.PostDateOfFirstARTransaction, line1.AL_RevRecognitionType);
			line1.Validation.ValidateAL_JH();
			AssertNoErrors("Line AL_JH should not have error", line1.AL_JHInfo);

			registryValue.RecognitionDateOptionCode = RevenueRecognitionLookups.RecognitionDateOptionCodes.CustomsClearanceDate;
			registryValue.RunPreSaveValidation();
			AccountingConfigurationRegistry.Instance.RevenueRecognitionSetup.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, registryCollection);
			testJob.ClearCacheOfRevenueRecognitionRegistryItems_ForTestsOnly();

			line1.AL_AC = testJob.Charges[0].ChargeCode.PK;
			AssertEquals("Precondition: ", RevenueRecognitionLookups.RecognitionDateOptionCodes.CustomsClearanceDate, line1.AL_RevRecognitionType);
			line1.Validation.ValidateAL_JH();
			AssertHasError("The error should be this:", line1.AL_JHInfo, "This invoice cannot be posted until the 'Customs Clearance Date' for this job is recorded. This job and charge code combination requires this date for revenue recognition purposes.");

			AccountingConfigurationRegistry.Instance.EnableDeferredRevenueRecognition.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			line1.Validation.ValidateAL_JH();
			AssertNoErrors("Line AL_JH should not have error", line1.AL_JHInfo);
		}

		public void TestCheckAL_JH_RevenueRecognitionDateBeforeFirstPeriod()
		{
			RevenueRecognitionCollection valuesForTest = new RevenueRecognitionCollection();
			RevenueRecognition setting1 = valuesForTest.AddNew();
			setting1.JobType = JobInvoicingConsumerTypes.Shipment.Code;
			setting1.DirectionCode = Constants.FreightShipmentDirection.Code.All;
			setting1.Mode = Core.Constants.TransportModes.Air;
			setting1.RecognitionDateOptionCode = RevenueRecognitionLookups.RecognitionDateOptionCodes.PickupDate;
			AccountingConfigurationRegistry.Instance.RevenueRecognitionSetup.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, valuesForTest);

			TestCaseHelper.ClearTable("AccPeriodManagement");
			TestObjectCreator.CreateTestPeriods(ZDateTime.Today);

			Job testJob = TestObjectCreator.CreateJob("Z00001000", TestObjectCreator.LocalClient, 5M, TestObjectCreator.Agent, 10M);
			ForwardingShipment shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			testJob.Parent = shipment;

			shipment.JS_RL_NKOrigin = "USLAX";
			shipment.JS_RL_NKDestination = "AUSYD";
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;

			shipment.DocsAndCartage.JP_PickupCartageCompleted = ZDateTime.BrettsBirthday;

			APInvoice invoice = Factory.New<APInvoice>();
			APInvoiceLine line1 = (APInvoiceLine)invoice.Lines.AddNew();
			line1.AL_AC = TestObjectCreator.CC1.PK;

			line1.AL_JH = testJob.PK;
			AssertHasErrors("Line AL_JH should have error", line1.AL_JHInfo);

			testJob.ShouldUseImmediateRevenueRecognisedDate += new EventHandler<UserQueryEventArgs>(testJob_ShouldUseImmediateRevenueRecognisedDate);
			testJob.AskShouldUseImmediateRevenueRecognisedDate(line1.ChargeCode);
			testJob.ShouldUseImmediateRevenueRecognisedDate -= new EventHandler<UserQueryEventArgs>(testJob_ShouldUseImmediateRevenueRecognisedDate);

			line1.Validation.ValidateAL_JH();
			AssertNoErrors("Line AL_JH should not have error", line1.AL_JHInfo);
		}

		void testJob_ShouldUseImmediateRevenueRecognisedDate(object sender, UserQueryEventArgs e)
		{
			e.Response = true;
		}

		public void TestCheckAL_JH_SpotQuoteTransaction()
		{
			var skipTypes = new[] {
				typeof(ARInvoice),
				typeof(ARCreditNote)
			};

			if (skipTypes.Contains(InvoiceType))
			{
				Assert(true);
			}
			else
			{
				var job = TestObjectCreator.Job1;
				job.JH_ParentTableCode = RatingHeaderSchema.Constants.Prefix;
				Factory.Save();

				InvoicingBase invoice = (InvoicingBase)Factory.New(InvoiceType);
				invoice.AH_JH = job.PK;
				InvoicingLineBase line = (InvoicingLineBase)invoice.Lines.AddNew();
				line.AL_JH = job.PK;
				AssertHasError(line.AL_JHInfo, "This type of job cannot be used.");
			}
		}

		public void TestCheckAmendingTransactionAL_JH_SkippedForManuallyAmendingCreditNote()
		{
			var shipment1 = TestObjectCreator.CreateShipment("S00001234");
			var job1 = TestObjectCreator.CreateJob(shipment1);

			var shipment2 = TestObjectCreator.CreateShipment("S00005678");
			var job2 = TestObjectCreator.CreateJob(shipment2);
			Factory.Save();

			var amendingPairs = new (InvoicingBase, InvoicingBase)[]
						{
								(Factory.NewWithValidTestData<ARInvoice>(), Factory.NewWithValidTestData<ARCreditNote>()),
								(Factory.NewWithValidTestData<APInvoice>(), Factory.NewWithValidTestData<APCreditNote>())
						};

			foreach (var pair in amendingPairs)
			{
				var invoiceParent = pair.Item1;
				invoiceParent.AH_JH = job2.PK;
				var creditNote = pair.Item2;
				creditNote.AH_OriginalTransactionNum = "123456";
				AssertNull(creditNote.OriginalTransaction);
				Assert("Should be Amending Transaction", (creditNote as IAmending).IsAmendingTransaction);
				Assert("Should be Amending Transaction via soft reference", (creditNote as IAmending).IsAmendingTransaction_SoftReference);
				var creditNoteLine = (InvoicingLineBase)creditNote.Lines.AddNew();
				Assert(!creditNoteLine.IsAmendingOriginalViaStrongReference);
				creditNoteLine.AL_JH = job1.PK;
				((InvoicingLineBaseValidation)creditNoteLine.Validation).ValidateAL_JH();
				AssertNoError(creditNoteLine.AL_JHInfo, "Job billing record for this job does not exist. To rectify, click on the billing tab of the job and save.");
				AssertNoError(creditNoteLine.AL_JHInfo, "Only Jobs from Original transaction can be used on Amending transaction.");

				creditNote.AH_TransactionBelongsToGroup = invoiceParent.PK;
				creditNote.OriginalTransaction = invoiceParent;
				Assert("Should be Amending Transaction via strong reference", creditNote.IsAmendingTransaction_StrongReference);
				creditNoteLine = (InvoicingLineBase)creditNote.Lines.AddNew();
				Assert(creditNoteLine.IsAmendingOriginalViaStrongReference);
				creditNoteLine.AL_JH = job1.PK;
				((InvoicingLineBaseValidation)creditNoteLine.Validation).ValidateAL_JH();
				AssertHasError(creditNoteLine.AL_JHInfo, "Job billing record for this job does not exist. To rectify, click on the billing tab of the job and save.");
				AssertHasError(creditNoteLine.AL_JHInfo, "Only Jobs from Original transaction can be used on Amending transaction.");
			}
		}

		public void TestCheckAL_JH_AmendingTransaction()
		{
			ForwardingShipment shipment = TestObjectCreator.CreateShipment("S00001234");
			Job job = TestObjectCreator.CreateJob(shipment);
			Factory.Save();

			InvoicingBase invoice = (InvoicingBase)Factory.New(InvoiceType);
			invoice.AH_JH = job.PK;
			InvoicingLineBase line = (InvoicingLineBase)invoice.Lines.AddNew();
			line.AL_JH = job.PK;
			invoice.Lines.RemoveAndDelete(invoice.Lines.AddNew()); // This is a trick to update Jobs collection
			Assert("Job should be in the Invoice Jobs", invoice.InvoiceDependentJobs.Any(a => ((InvoiceDependentJob)a).Job.PK == job.PK));
			Assert("Other job should not be in the Invoice Jobs", !invoice.InvoiceDependentJobs.Any(a => ((InvoiceDependentJob)a).Job.PK == TestObjectCreator.Job1.PK));

			if (invoice is IAmending && invoice.HasImplementedGenerateAmendingTransaction)
			{
				var invoiceType = invoice is APInvoice ? TransactionTypes.CreditNote : invoice.AH_TransactionType.ToString();
				IAmending amending = ((IAmending)invoice).GenerateAmendingTransaction(invoiceType);
				AssertNotNull("Amending", amending);

				InvoicingBase amendingTransaction = (InvoicingBase)amending;
				Assert("Should be at least one line", amendingTransaction.Lines.Any());
				InvoicingLineBase amendingLine = amendingTransaction.Lines[0];

				((InvoicingLineBaseValidation)amendingLine.Validation).ValidateAL_JH();
				AssertNoErrors(amendingLine.AL_JHInfo);

				amendingLine.AL_JH = TestObjectCreator.Job1.PK;
				((InvoicingLineBaseValidation)amendingLine.Validation).ValidateAL_JH();

				AssertHasErrorContaining(amendingLine.AL_JHInfo, "Only Jobs from Original transaction can be used on Amending transaction.");
			}
			else
			{
				Assert("Not Applicable", true);
			}
		}

		#endregion

		public override void TestCheckAL_GE()
		{
			base.TestCheckAL_GE();
			var feaChargeCodePK = FEADepartmentChargeCodeInDB.PK;

			InvoicingBase invoice = (InvoicingBase)Factory.New(InvoiceType);
			invoice.AH_TransactionNum = "101";
			InvoicingLineBase line = (InvoicingLineBase)invoice.Lines.AddNew();

			line.GenericCharge = ZGuid.Empty;
			line.AL_GE = Factory.LoadTop1(typeof(GlbDepartment), new ZQuery(GlbDepartmentSchema.GE_Code, "CIA")).PK;
			line.Validation.ValidateAL_GE();
			Assert("Department should not have error since no charge code is selected", !line.AL_GEInfo.HasErrors());

			line.GenericCharge = feaChargeCodePK;
			line.AL_GE = Factory.LoadTop1(typeof(GlbDepartment), new ZQuery(GlbDepartmentSchema.GE_Code, "CIA")).PK;
			line.Validation.ValidateAL_GE();
			AssertEquals("Department should have error since selected department is not in charge department filter list",
				"The department CIA is not contained in the department filter list for the entered charge." + System.Environment.NewLine + "The list is: FEA",
				line.AL_GEInfo.GetErrors().GetFirstMessage());

			TestObjectCreator.AttachJobToAPLine(line);
			TestObjectCreator.AttachChargeToAPLine(line);
			line.AL_GE = Factory.LoadTop1(typeof(GlbDepartment), new ZQuery(GlbDepartmentSchema.GE_Code, "FEA")).PK;
			line.Validation.ValidateAL_GE();
			Assert("Department should not have errors since FEA is in the department filter list", !line.AL_GEInfo.HasErrors());

			line.GenericCharge = ALLDepartmentChargeCodeInDB.PK;
			line.AL_GE = Factory.LoadTop1(typeof(GlbDepartment), new ZQuery(GlbDepartmentSchema.GE_Code, "FDS")).PK;
			line.Validation.ValidateAL_GE();
			Assert("Department should not have errors since the department filter list is ALL", !line.AL_GEInfo.HasErrors());
		}

		public void TestTaxRateValidationIfCommentChargeCode()
		{
			bool oldReceviablesAllowGSTModification = AccountingConfigurationRegistry.Instance.ReceivableAllowUserToModifyGSTId.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK);
			bool oldPayablesAllowGSTModification = AccountingConfigurationRegistry.Instance.PayableAllowUserToModifyGSTId.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK);
			try
			{
				AccountingConfigurationRegistry.Instance.ReceivableAllowUserToModifyGSTId.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
				AccountingConfigurationRegistry.Instance.PayableAllowUserToModifyGSTId.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);

				AccChargeCode chargeCode = SetCommentChargeCode();
				Factory.Save();
				InvoicingLineBase line = SetInvoiceLine(chargeCode);

				line.Validation.ValidateAL_AT();

				AssertNoErrors(line.AL_ATInfo);

				line.GenericCharge = TestObjectCreator.CC1.PK;
				line.AL_AT = ZGuid.Empty;
				line.Validation.ValidateAL_AT();

				AssertHasErrors(line.AL_ATInfo);
			}
			finally
			{
				AccountingConfigurationRegistry.Instance.ReceivableAllowUserToModifyGSTId.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, oldReceviablesAllowGSTModification);
				AccountingConfigurationRegistry.Instance.PayableAllowUserToModifyGSTId.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, oldPayablesAllowGSTModification);
			}
		}

		public void TestGSTMandatoryFlagNotSetIfCommentChargeCode()
		{
			AccChargeCode chargeCode = SetCommentChargeCode();
			Factory.Save();
			InvoicingLineBase line = SetInvoiceLine(chargeCode);

			AccountingConfigurationRegistry.Instance.ReceivableAllowUserToModifyGSTId.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			AccountingConfigurationRegistry.Instance.PayableAllowUserToModifyGSTId.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			GlbCompany.CurrentCompany.GC_IsGSTRegistered = ZBool.True;

			((InvoicingLineBaseValidation)line.Validation).ValidateAL_AT();
			AssertNoErrors(line.AL_ATInfo);
		}

		public void TestCheckAL_ATNotValidateIfLineIsPosted()
		{
			GlbCompany.CurrentCompany.GC_IsGSTRegistered = ZBool.True;
			AccountingConfigurationRegistry.Instance.ReceivableAllowUserToModifyGSTId.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			AccountingConfigurationRegistry.Instance.PayableAllowUserToModifyGSTId.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);

			var org = TestObjectCreator.AALSHI;
			org.CompanyData.OB_IsDebtor = false;
			org.CompanyData.OB_IsCreditor = false;
			Factory.Save();

			InvoicingBase invoicePosted = (InvoicingBase)Factory.NewWithValidTestData(InvoiceType);
			invoicePosted.AH_OH = org.PK;
			InvoicingLineBase linePosted = (InvoicingLineBase)invoicePosted.Lines.AddNew();
			linePosted.AL_AG = TestObjectCreator.GLHeader1.PK;
			Factory.Save();

			invoicePosted.RunPreSaveValidation();
			AssertNoErrors(linePosted.AL_ATInfo);
			AssertNoErrorContaining(linePosted.AL_ATInfo, "Please enter a Tax ID.");

			org.CompanyData.OB_IsDebtor = true;
			org.CompanyData.OB_IsCreditor = true;
			Factory.Save();

			invoicePosted.RunPreSaveValidation();
			AssertNoErrors(linePosted.AL_ATInfo);
			AssertNoErrorContaining(linePosted.AL_ATInfo, "Please enter a Tax ID.");

			InvoicingBase invoiceNotPosted = (InvoicingBase)Factory.NewWithValidTestData(InvoiceType);
			invoiceNotPosted.AH_OH = org.PK;
			InvoicingLineBase lineNotPosted = (InvoicingLineBase)invoiceNotPosted.Lines.AddNew();
			invoiceNotPosted.RunPreSaveValidation();
			AssertHasErrors(lineNotPosted.AL_ATInfo);
			AssertHasErrorContaining(lineNotPosted.AL_ATInfo, "Please enter a Tax ID.");
		}

		public void TestCheckAL_ATValidatesGSTMandatory()
		{
			bool originalGSTRegisterd = GlbCompany.CurrentCompany.GC_IsGSTRegistered;
			bool originalModifyGSTAR = AccountingConfigurationRegistry.Instance.ReceivableAllowUserToModifyGSTId.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK);
			bool originalModifyGSTAP = AccountingConfigurationRegistry.Instance.PayableAllowUserToModifyGSTId.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK);
			try
			{
				var accGLHeader = Factory.NewWithValidTestData<AccGLHeader>();
				Factory.Save();

				GlbCompany.CurrentCompany.GC_IsGSTRegistered = true;
				AccountingConfigurationRegistry.Instance.ReceivableAllowUserToModifyGSTId.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
				AccountingConfigurationRegistry.Instance.PayableAllowUserToModifyGSTId.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);

				InvoicingBase invoice = (InvoicingBase)Factory.NewWithValidTestData(InvoiceType);
				InvoicingLineBase line = (InvoicingLineBase)invoice.Lines.AddNew();
				((InvoicingLineBaseValidation)line.Validation).ValidateAL_AT();
				AssertNoErrorContaining(line.AL_ATInfo, "Please enter a Tax ID.");

				line.InvoiceBase.AH_OH = GSTRegisteredOrg.PK;
				((InvoicingLineBaseValidation)line.Validation).ValidateAL_AT();
				AssertHasError("Should be an error due to empty GST ID", line.AL_ATInfo, "Please enter a Tax ID.");

				AccountingConfigurationRegistry.Instance.ReceivableAllowUserToModifyGSTId.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);
				AccountingConfigurationRegistry.Instance.PayableAllowUserToModifyGSTId.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);

				InvoicingBase invoice1 = (InvoicingBase)Factory.NewWithValidTestData(InvoiceType);
				InvoicingLineBase line1 = (InvoicingLineBase)invoice.Lines.AddNew();
				line1.AL_AG = accGLHeader.PK;
				line1.GenericCharge = accGLHeader.PK;
				line1.InvoiceBase.AH_OH = GSTRegisteredOrg.PK;
				((InvoicingLineBaseValidation)line1.Validation).ValidateAL_AT();
				AssertHasError("Should be an error due to empty GST ID", line1.AL_ATInfo, "Please enter a Tax ID.");
			}
			finally
			{
				GlbCompany.CurrentCompany.GC_IsGSTRegistered = originalGSTRegisterd;
				AccountingConfigurationRegistry.Instance.ReceivableAllowUserToModifyGSTId.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, originalModifyGSTAR);
				AccountingConfigurationRegistry.Instance.PayableAllowUserToModifyGSTId.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, originalModifyGSTAP);
			}
		}

		public void TestCheckAL_ATValidatesInvalidTaxRate()
		{
			bool originalGSTRegisterd = GlbCompany.CurrentCompany.GC_IsGSTRegistered;
			try
			{
				AccTaxRate activeRate = TestObjectCreator.CreateTaxRate("TAX1", "", 10);
				AccTaxRate inactiveRate = TestObjectCreator.CreateTaxRate("TAX2", "", 10);
				inactiveRate.AT_IsActive = false;

				GlbCompany.CurrentCompany.GC_IsGSTRegistered = true;

				InvoicingBase invoice = (InvoicingBase)Factory.NewWithValidTestData(InvoiceType);
				InvoicingLineBase line = (InvoicingLineBase)invoice.Lines.AddNew();
				invoice.AH_OH = GSTRegisteredOrg.PK;
				line.AL_AT = activeRate.PK;
				((InvoicingLineBaseValidation)line.Validation).ValidateAL_AT();
				AssertNoErrorContaining(line.AL_ATInfo, "Enter a valid " + line.AL_ATInfo.Description + ".");

				line.AL_AT = inactiveRate.PK;
				((InvoicingLineBaseValidation)line.Validation).ValidateAL_AT();
				AssertHasError("Should be an error due to inactive GST ID", line.AL_ATInfo, "Enter a valid " + line.AL_ATInfo.Description + ".");

				invoice.SetReadOnlyIncludingChildren(true);
				((InvoicingLineBaseValidation)line.Validation).ValidateAL_AT();
				AssertNoErrorContaining(line.AL_ATInfo, "Enter a valid " + line.AL_ATInfo.Description + ".");

				invoice.SetReadOnlyIncludingChildren(false);
				((InvoicingLineBaseValidation)line.Validation).ValidateAL_AT();
				AssertHasError("Should be an error due to inactive GST ID", line.AL_ATInfo, "Enter a valid " + line.AL_ATInfo.Description + ".");
			}
			finally
			{
				GlbCompany.CurrentCompany.GC_IsGSTRegistered = originalGSTRegisterd;
			}
		}

		InvoicingLineBase SetInvoiceLine(AccChargeCode chargeCode)
		{
			InvoicingBase invoice = (InvoicingBase)Factory.New(InvoiceType);
			invoice.AH_OH = GSTRegisteredOrg.PK;
			InvoicingLineBase line = (InvoicingLineBase)invoice.Lines.AddNew();

			line.GenericCharge = chargeCode.PK;
			line.AL_AT = ZGuid.Empty;
			line.AL_AG = TestObjectCreator.GLHeader1.PK;

			return line;
		}

		AccChargeCode SetCommentChargeCode()
		{
			ZQuery testFilter = new ZQuery(AccChargeCodeSchema.AC_ChargeType, "MRG");
			AccChargeCode chargeCode = Factory.LoadTop1(typeof(AccChargeCode), testFilter) as AccChargeCode;
			chargeCode.AC_DepartmentFilterList = "ALL";
			chargeCode.AC_ChargeType = "CMT";
			return chargeCode;
		}

		public void TestCheckGSTInclusiveAmount()
		{
			InvoicingBase invoice = (InvoicingBase)Factory.New(InvoiceType);
			InvoicingLineBase line = (InvoicingLineBase)invoice.Lines.AddNew();
			invoice.GSTInclusiveAmounts = false;
			line.AL_AT = TestObjectCreator.CreateTaxRate("TestGST", "", 10).PK;
			line.AL_RX_NKTransactionCurrency = "AUD";
			line.AL_OSExTaxAmount = 120;
			line.GSTInclusiveAmount = 1;
			InvoicingLineBaseValidation testValidation = ((InvoicingLineBaseValidation)line.Validation);
			testValidation.ValidateGSTInclusiveAmount();
			Assert("GSTInclusiveAmount shouldn't have errors.", !line.GSTInclusiveAmountInfo.HasErrors());

			invoice.GSTInclusiveAmounts = true;
			line.GSTInclusiveAmount = 11;
			line.AL_OSExTaxAmount = 120;
			testValidation.ValidateGSTInclusiveAmount();
			Assert("GSTInclusiveAmount should have errors.", line.GSTInclusiveAmountInfo.HasErrors());

			line.AL_OSExTaxAmount = 10;
			testValidation.ValidateGSTInclusiveAmount();
			Assert("GSTInclusiveAmount shouldn't have errors.", !line.GSTInclusiveAmountInfo.HasErrors());

			line.GSTInclusiveAmount = 333;
			testValidation.ValidateGSTInclusiveAmount();
			Assert("GSTInclusiveAmount shouldn't have errors.", !line.GSTInclusiveAmountInfo.HasErrors());
		}

		public void TestValidateAllValidatesConsolCost_RowErrors()
		{
			if (!(InvoiceToValidate is APInvoice) && !(InvoiceToValidate is APCreditNote))
			{
				Assert(true);
			}
			else
			{
				var factory = new BusinessObjectFactory();
				var creator = new TestObjectCreator(factory);
				var consol = creator.CreateConsol("AUSYD", "NZAKL", "C0001003");
				var shipment = creator.CreateShipment("S00100191", "AUSYD", "NZAKL", consol);

				using (var job = creator.CreateJob(shipment))
				{
					creator.CreateCharge(job, creator.CC1, "desc", creator.AUD, 100M, creator.AALSHI, creator.AUD, 100M, creator.Debtor);

					var invoice = creator.CreateInvoice(InvoiceToValidate.GetType(), "INV1", creator.AUD, 1M);
					var apportionedCharge = creator.CreateConsolCost(invoice, consol, creator.CC1, 100M).ApportionmentCharges[0];
					apportionedCharge.JR_OSCostAmt = 100M;
					invoice.ImportAllApportionmentsFromCosting();

					AssertEquals("Precondition: invoice has copied the line", 1, invoice.Lines.Count);

					var branch = apportionedCharge.JR_GB;
					apportionedCharge.JR_GB = ZGuid.NewZGuid();

					invoice.ClearValidatedConsolCostPKList();

					var line = invoice.Lines[0];
					line.Validation.ValidateAll();

					var errorMessage = "The related consol cost is invalid, please fix the following errors in the consol cost from which this line was apportioned:\r\nBranch: Enter a valid Branch.\r\n";

					AssertHasErrors("Apportionment branch should have an error", apportionedCharge.JR_GBInfo);
					AssertNoErrors("No error on the line", line.AL_GBInfo);
					AssertHasRowError("Expected error even though line has no problems, the charge it was apportioned from does",
						line, errorMessage);

					apportionedCharge.JR_GB = branch;

					invoice.ClearValidatedConsolCostPKList();
					line.Validation.ValidateAll();

					AssertNoRowError(line, errorMessage);
				}
			}
		}

		public void TestValidateAllValidatesConsolCost_RowWarning()
		{
			if (!(InvoiceToValidate is APInvoice) && !(InvoiceToValidate is APCreditNote))
			{
				Assert(true);
			}
			else
			{
				var factory = new BusinessObjectFactory();
				var creator = new TestObjectCreator(factory);
				var consol = creator.CreateConsol("AUSYD", "NZAKL", "C0001003");
				var shipment = creator.CreateShipment("S00100191", "AUSYD", "NZAKL", consol);

				using (var job = creator.CreateJob(shipment))
				{
					var creditor = creator.AALSHI;
					creator.CreateCharge(job, creator.CC1, "desc", creator.AUD, 100M, creditor, creator.AUD, 100M, creator.Debtor);

					var invoice = creator.CreateInvoice(InvoiceToValidate.GetType(), "INV1", creator.AUD, 1M);
					var apportionedCharge = creator.CreateConsolCost(invoice, consol, creator.CC1, 100M).ApportionmentCharges[0];
					apportionedCharge.JR_OSCostAmt = 100M;

					invoice.ImportAllApportionmentsFromCosting();
					var line = invoice.Lines[0];

					AssertNotNull("Precondition: invoice has copied the line", line);
					AssertNoRowWarningContaining(line, InvoicingLineBaseValidation.ImportedFromApportionmentChargeWithWarningsMessage);

					job.JH_Status = JobHeaderStatus.Closed.Code;
					((ApportionSplitChargeValidation)apportionedCharge.Validation).ValidateJR_IsUsedForApportionment();

					invoice.ClearValidatedConsolCostPKList();
					line.Validation.ValidateAll();

					Assert("Precondition", apportionedCharge.HasWarnings);
					AssertHasRowWarning(line, InvoicingLineBaseValidation.ImportedFromApportionmentChargeWithWarningsMessage);
				}
			}
		}

		public void TestWritingOffOfBadDebtOfPeriodicInvoicesWithDuplicateDisplaySequences()
		{
			TestObjectCreator creator = new TestObjectCreator(Factory);
			ZString expectedError = "The Line Sequence Number must be unique.";
			ARInvoice invoice = creator.CreateARInvoice<ARInvoice>("1999", creator.AUD, 1.0M, creator.ABIGAS);
			ARInvoiceLine line1 = creator.CreateARInvoiceLine(invoice, null, creator.CC4, creator.AUD, 1.0M, "hello", 100);
			ARInvoiceLine line2 = creator.CreateARInvoiceLine(invoice, null, creator.CC4, creator.AUD, 1.0M, "hello", 100);

			line1.AL_Sequence = 1;
			line2.AL_Sequence = 1;

			invoice.RunPreSaveValidation();
			AssertHasError(invoice.Lines[0].AL_SequenceInfo, expectedError);
			AssertHasError(invoice.Lines[1].AL_SequenceInfo, expectedError);

			Factory.Save();

			((IReversing)invoice).GenerateReverseTransaction(false);
			invoice.ReverseInvoice.RunPreSaveValidation();
			AssertNoError(invoice.ReverseInvoice.Lines[0].AL_SequenceInfo, expectedError);
			AssertNoError(invoice.ReverseInvoice.Lines[1].AL_SequenceInfo, expectedError);
		}

		public void TestCheckAL_ATWithPostingGroups()
		{
			GlbCompany.CurrentCompany.GC_IsGSTRegistered = true;

			var tax1 = TestObjectCreator.CreateTaxRate("TAX1", "", 10);
			var tax2 = TestObjectCreator.CreateTaxRate("TAX2", "", 11);
			var tax3 = TestObjectCreator.CreateTaxRate("TAX3", "", 12);

			tax1.AT_PostingGroupId = 1;
			tax2.AT_PostingGroupId = 1;
			tax3.AT_PostingGroupId = 2;

			Factory.Save();

			var invoice = (InvoicingBase)Factory.NewWithValidTestData(InvoiceType);
			invoice.AH_OH = GSTRegisteredOrg.PK;

			var line1 = (InvoicingLineBase)invoice.Lines.AddNew();
			var line2 = (InvoicingLineBase)invoice.Lines.AddNew();
			var line3 = (InvoicingLineBase)invoice.Lines.AddNew();

			line1.AL_AT = tax1.PK;
			line2.AL_AT = tax2.PK;
			line3.AL_AT = tax3.PK;

			AssertEquals(new ZShort(1), line1.TaxRate.AT_PostingGroupId);
			AssertEquals(new ZShort(1), line2.TaxRate.AT_PostingGroupId);
			AssertEquals(new ZShort(2), line3.TaxRate.AT_PostingGroupId);

			invoice.RunPreSaveValidation();

			if (!invoice.IsReverseTransaction)
			{
				using (GlbCompany.CurrentCompany.TemporarilySetCountry("AU"))
				{
					invoice.RunPreSaveValidation();

					Assert(!AccTaxRate.IsPostingGroupsEnabled("AU"));

					AssertNoErrors(line1.AL_ATInfo);
					AssertNoWarnings(line1.AL_ATInfo);

					AssertNoErrors(line2.AL_ATInfo);
					AssertNoWarnings(line2.AL_ATInfo);

					AssertNoErrors(line3.AL_ATInfo);
					AssertNoWarnings(line3.AL_ATInfo);
				}

				using (GlbCompany.CurrentCompany.TemporarilySetCountry("VN"))
				{
					invoice.RunPreSaveValidation();

					Assert(AccTaxRate.IsPostingGroupsEnabled("VN"));

					AssertNoErrors(line1.AL_ATInfo);
					AssertHasWarning(line1.AL_ATInfo, @"You have prepared charges using a mix of Tax ID Posting Groups. 
This transaction line’s Tax ID Posting Group value is 1.");

					AssertNoErrors(line2.AL_ATInfo);
					AssertHasWarning(line2.AL_ATInfo, @"You have prepared charges using a mix of Tax ID Posting Groups. 
This transaction line’s Tax ID Posting Group value is 1.");

					AssertNoErrors(line3.AL_ATInfo);
					AssertHasWarning(line3.AL_ATInfo, @"You have prepared charges using a mix of Tax ID Posting Groups. 
This transaction line’s Tax ID Posting Group value is 2.");

					invoice.Lines.Remove(line3.PK);
					invoice.RunPreSaveValidation();

					AssertNoErrors(line1.AL_ATInfo);
					AssertNoWarnings(line1.AL_ATInfo);

					AssertNoErrors(line2.AL_ATInfo);
					AssertNoWarnings(line2.AL_ATInfo);
				}
			}
			else
			{
				AssertNoErrors(line1.AL_ATInfo);
				AssertNoWarnings(line1.AL_ATInfo);

				AssertNoErrors(line2.AL_ATInfo);
				AssertNoWarnings(line2.AL_ATInfo);

				AssertNoErrors(line3.AL_ATInfo);
				AssertNoWarnings(line3.AL_ATInfo);
			}
		}

		[TestDate(2015, 5, 1)]
		public void TestCheckAL_ExchangeRate_ARAPInvoicePostingExchangeRateOptionExcludeAmendingOrReversal_Job() => AssertCheckAL_ExchangeRate_ARAPInvoicePostingExchangeRateOptionExcludeAmendingOrReversal(true);

		[TestDate(2015, 5, 1)]
		public void TestCheckAL_ExchangeRate_ARAPInvoicePostingExchangeRateOptionExcludeAmendingOrReversal_NonJob() => AssertCheckAL_ExchangeRate_ARAPInvoicePostingExchangeRateOptionExcludeAmendingOrReversal(false);

		public void AssertCheckAL_ExchangeRate_ARAPInvoicePostingExchangeRateOptionExcludeAmendingOrReversal(bool attachJobToLine)
		{
			ExchangeRateReader.GetReaderInstance().ClearCache();

			TestObjectCreator.CreateExchangeRate(TestObjectCreator.USD, ExchangeRateTypes.Code.BuyRate, 2, ZDateTime.Today, ZDateTime.Today);
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.USD, ExchangeRateTypes.Code.SellRate, 2, ZDateTime.Today, ZDateTime.Today);
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.USD, ExchangeRateTypes.Code.C07Rate, 3, ZDateTime.Today, ZDateTime.Today);

			GlbCompany.CurrentCompany.AccExchangeRateConfigurations.RemoveAndDeleteAll();
			GlbCompany.CurrentCompany.AccExchangeRateConfigurations.SetExRate("ALL", "ALL", "ALL", ExchangeRateTypes.Code.C07Rate);
			GlbCompany.CurrentCompany.Factory.Save();

			var invoice = (InvoicingBase)Factory.NewWithValidTestData(InvoiceType);
			var invoiceLine = (InvoicingLineBase)invoice.Lines.AddNew();
			if (attachJobToLine)
			{
				var shipment = TestObjectCreator.CreateShipment("S001");
				var job = TestObjectCreator.CreateJob(shipment, false);
				invoiceLine.AL_JH = job.PK;
			}
			invoiceLine.AL_RX_NKTransactionCurrency = "USD";
			invoiceLine.AL_ExchangeRate = 3;
			invoiceLine.AL_LineAmount = 100;
			AssertEquals("default rate", 3m, invoiceLine.AL_ExchangeRate);

			invoiceLine.Company.AccExchangeRateConfigurations.Reload(true);
			AssertEquals("Pre-Condition: Config Rate Type to use is C07", ExchangeRateType.C07, AccExchangeRateConfigurationRateFinder.GetExchangeRateConfigurationRateType(invoice.GetExchangeRateConfigurationRateConsumer(invoiceLine.Job as Job), invoice.Header, ExchangeRateEnumsExtensions.GetLedgerFromCode(invoice.AH_Ledger), string.Empty));

			invoiceLine.AL_ExchangeRate = 2;
			invoiceLine.RunPreSaveValidation();
			AssertNoErrors(invoiceLine.AL_ExchangeRateInfo);

			var postingExRateRegistry = invoice.GetExRateLedger() == ExchangeRateValidLedgerEnum.AR
				? AccountingConfigurationRegistry.Instance.InvoicePostingExchangeRateOptionAR
				: AccountingConfigurationRegistry.Instance.InvoicePostingExchangeRateOptionAP;

			postingExRateRegistry.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, InvoicePostingExchangeRateOption.ExchangeRateBasedOnInvoiceDate.Code);
			invoiceLine.AL_ExchangeRate = 2;
			invoiceLine.RunPreSaveValidation();

			var ledgerStr = invoiceLine.IsAR() ? "AR" : "AP";
			AssertHasError("Validation should use Ex Rate generated from Config Rate Type when one exists", invoiceLine.AL_ExchangeRateInfo, $@"The ""{ledgerStr} Invoice Posting Exchange Rate Option"" has been set to ""Exchange Rate based on Invoice Date"".
With this option, the exchange rate should not be changed manually. System expected 3.000000 rate but 2.000000 was entered.");

			invoiceLine.AL_ExchangeRate = 3;
			invoiceLine.RunPreSaveValidation();
			AssertNoErrors(invoiceLine.AL_ExchangeRateInfo);

			GlbCompany.CurrentCompany.AccExchangeRateConfigurations.RemoveAndDeleteAll();
			GlbCompany.CurrentCompany.Factory.Save();
			AssertEquals(0, GlbCompany.CurrentCompany.AccExchangeRateConfigurations.Count);
			var rateConfig = AccExchangeRateConfigurationRateFinder.GetExchangeRateConfigurationRateType(invoice.GetExchangeRateConfigurationRateConsumer(invoice.Job as Job), invoice.Header, ExchangeRateEnumsExtensions.GetLedgerFromCode(invoice.AH_Ledger), "");
			AssertNotNull("Pre-Condition: when no config exists, it will use default system config", rateConfig);
			AssertEquals(ExchangeRateType.Buy, rateConfig.Value);

			invoiceLine.AL_ExchangeRate = 3;
			invoiceLine.RunPreSaveValidation();
			AssertHasError("Validation should use Ex Rate generated from the invoice's RateType when no Config exists", invoiceLine.AL_ExchangeRateInfo, $@"The ""{ledgerStr} Invoice Posting Exchange Rate Option"" has been set to ""Exchange Rate based on Invoice Date"".
With this option, the exchange rate should not be changed manually. System expected 2.000000 rate but 3.000000 was entered.");

			invoiceLine.AL_ExchangeRate = 2;
			invoiceLine.RunPreSaveValidation();
			AssertNoErrors(invoiceLine.AL_ExchangeRateInfo);

			if (invoice is IAmending amending && invoice.HasImplementedGenerateAmendingTransaction && !(invoice is APInvoice))
			{
				var invoiceParent = (InvoicingBase)Factory.NewWithValidTestData(InvoiceType);
				invoice.AH_TransactionBelongsToGroup = invoiceParent.PK;
				amending.FlagAsCreatedAmending();
				AssertEquals(true, invoice.IsAmendingOrReversal);
				invoiceLine.AL_ExchangeRate = 5;

				using (TestObjectCreator.SetupAmendingTransactionCopyExchangeRateRegistry(invoice.AH_Ledger,invoice.AH_GC, false))
				{
					invoiceLine.RunPreSaveValidation();
					AssertHasError(invoiceLine.AL_ExchangeRateInfo, $@"The ""{ledgerStr} Invoice Posting Exchange Rate Option"" has been set to ""Exchange Rate based on Invoice Date"".
With this option, the exchange rate should not be changed manually. System expected 2.000000 rate but 5.000000 was entered.");
				}

				using (TestObjectCreator.SetupAmendingTransactionCopyExchangeRateRegistry(invoice.AH_Ledger, invoice.AH_GC, true))
				{
					invoiceLine.RunPreSaveValidation();
					AssertNoErrors(invoiceLine.AL_ExchangeRateInfo);
				}
			}

			ExchangeRateReader.GetReaderInstance().ClearCache();
		}

		public void TestValidateLinkedChargeNotDeleted()
		{
			var consol = TestObjectCreator.CreateConsol();
			var shipment = TestObjectCreator.CreateShipment("SHP001", consol);
			TestObjectCreator.CreateJob(shipment, false);
			var consolCost = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.CC1, 258M, TestObjectCreator.AALSHI);
			Factory.Save();

			var apInvoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), TestObjectCreator.AUD);
			apInvoice.SubmittedFromInvoicingForm = true;
			apInvoice.AH_OH = TestObjectCreator.AALSHI.PK;

			var invoiceCost = TestObjectCreator.CreateConsolCost(apInvoice, consol, TestObjectCreator.CC1, 258M);
			apInvoice.ImportAllApportionmentsFromCosting();

			invoiceCost.ApportionmentCharges[0].Delete();

			var charge = apInvoice.Lines[0].ApportionmentChargeImportedFrom;
			var validation = ((InvoicingLineBaseValidation)apInvoice.Lines[0].Validation);
			validation.ValidateLinkedChargeNotDeleted();

			var expectedErrorMessage = "The charge linked to the line is deleted. Please try to import the cost again using 'Apportion to Consols' button.";
			AssertHasRowError(apInvoice.Lines[0], expectedErrorMessage);
			ErrorReporter.Clear();
		}

		public void TestAL_OSExTaxAmount_NegativeAmountsOnAccountReceivableTransactions()
		{
			var invoicingBaseTypes = new[] { typeof(ARInvoice), typeof(APInvoice), typeof(ARCreditNote), typeof(APCreditNote), typeof(ARAdjustmentNote), typeof(APAdjustmentNote) };
			foreach (var invoicingBaseType in invoicingBaseTypes)
			{
				var invoice = TestObjectCreator.CreateInvoice(invoicingBaseType);
				var invoiceLine = (InvoicingLineBase)invoice.Lines.AddNew();
				var isRevenueType = invoiceLine.AL_LineType == TransactionLineTypes.Revenue;

				var testDataItems = new[]
				{
						new { TransactionLineNegativeAmountAllowedOnAccountReceivableModes.Allowed.Code, OSExTaxAmount = 5, HasTransactionHeader = true, TransactionHeaderIsCancelled = false, Allowed = true },
						new { TransactionLineNegativeAmountAllowedOnAccountReceivableModes.Disallowed.Code, OSExTaxAmount = 5, HasTransactionHeader = true, TransactionHeaderIsCancelled = false, Allowed = true },
						new { TransactionLineNegativeAmountAllowedOnAccountReceivableModes.Allowed.Code, OSExTaxAmount = -5, HasTransactionHeader = true, TransactionHeaderIsCancelled = false, Allowed = true },
						new { TransactionLineNegativeAmountAllowedOnAccountReceivableModes.Disallowed.Code, OSExTaxAmount = -5, HasTransactionHeader = true, TransactionHeaderIsCancelled = false, Allowed = invoice.AH_Ledger != LedgerTypes.AccountsReceivable || !isRevenueType },
						new { TransactionLineNegativeAmountAllowedOnAccountReceivableModes.Allowed.Code, OSExTaxAmount = 5, HasTransactionHeader = true, TransactionHeaderIsCancelled = true, Allowed = true },
						new { TransactionLineNegativeAmountAllowedOnAccountReceivableModes.Disallowed.Code, OSExTaxAmount = 5, HasTransactionHeader = true, TransactionHeaderIsCancelled = true, Allowed = true },
						new { TransactionLineNegativeAmountAllowedOnAccountReceivableModes.Allowed.Code, OSExTaxAmount = -5, HasTransactionHeader = true, TransactionHeaderIsCancelled = true, Allowed = true },
						new { TransactionLineNegativeAmountAllowedOnAccountReceivableModes.Disallowed.Code, OSExTaxAmount = -5, HasTransactionHeader = true, TransactionHeaderIsCancelled = true, Allowed = true },
						new { TransactionLineNegativeAmountAllowedOnAccountReceivableModes.Allowed.Code, OSExTaxAmount = 5, HasTransactionHeader = false, TransactionHeaderIsCancelled = false, Allowed = true },
						new { TransactionLineNegativeAmountAllowedOnAccountReceivableModes.Disallowed.Code, OSExTaxAmount = 5, HasTransactionHeader = false, TransactionHeaderIsCancelled = false, Allowed = true },
						new { TransactionLineNegativeAmountAllowedOnAccountReceivableModes.Allowed.Code, OSExTaxAmount = -5, HasTransactionHeader = false, TransactionHeaderIsCancelled = false, Allowed = true },
						new { TransactionLineNegativeAmountAllowedOnAccountReceivableModes.Disallowed.Code, OSExTaxAmount = -5, HasTransactionHeader = false, TransactionHeaderIsCancelled = false, Allowed = true },
					};

				foreach (var codeGroup in testDataItems.GroupBy(testDataItem => testDataItem.Code))
				{
					using (AccountingMasterFilesRegistry.Instance.NegativeAmountAllowedOnAccountReceivableTransactions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, codeGroup.Key))
					{
						foreach (var testDataItem in codeGroup)
						{
							var testDataItemContext = FormattableString.Invariant($"Context: Code={testDataItem.Code}, Type={invoicingBaseType.Name}, Amount={testDataItem.OSExTaxAmount}, HasTransactionHeader={testDataItem.HasTransactionHeader}, TransactionHeaderIsCancelled={testDataItem.TransactionHeaderIsCancelled}, Allowed={testDataItem.Allowed}");
							invoiceLine.AL_AH = testDataItem.HasTransactionHeader ? invoice.PK : ZGuid.Empty;
							invoiceLine.AL_OSExTaxAmount = testDataItem.OSExTaxAmount;
							invoice.IsCancelled = testDataItem.TransactionHeaderIsCancelled;
							if (testDataItem.Allowed)
							{
								AssertNoErrors(testDataItemContext, invoiceLine.AL_OSExTaxAmountInfo);
							}
							else
							{
								AssertHasError(testDataItemContext, invoiceLine.AL_OSExTaxAmountInfo, "Negative revenue charges are not allowed. This is controlled by the registry setting at Accounting > Receivable defaults > Default Settings > Negative Charges on Accounts Receivable Transactions.");
							}
						}
					}
				}
			}
		}

		public void TestAL_OSExTaxAmount_NegativeAmountsOnAccountReceivableTransactions_NegativeChargesAllowed()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.CompanyData.OB_ARVATConfig = AccountingMasterFilesConstants.OrganisationTaxConfiguartionTypes.NotApplicable.Code;
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			org2.CompanyData.OB_ARVATConfig = AccountingMasterFilesConstants.OrganisationTaxConfiguartionTypes.Default.Code;
			Factory.Save();

			var invoicingBaseTypes = new[] { typeof(ARInvoice), typeof(APInvoice), typeof(ARCreditNote), typeof(APCreditNote), typeof(ARAdjustmentNote), typeof(APAdjustmentNote) };
			foreach (var invoicingBaseType in invoicingBaseTypes)
			{
				var invoice = TestObjectCreator.CreateInvoice(invoicingBaseType);
				var invoiceLine = (InvoicingLineBase)invoice.Lines.AddNew();
				var isRevenueType = invoiceLine.AL_LineType == TransactionLineTypes.Revenue;

				var testDataItems = new[]
				{
						new { TransactionLineNegativeAmountAllowedOnAccountReceivableModes.NegativeChargesAllowed.Code,	OSExTaxAmount = 5,	HasTransactionHeader = true,	TransactionHeaderIsCancelled = false,   IsDebtorVATIsNotApplicable = true,	Allowed = true },
						new { TransactionLineNegativeAmountAllowedOnAccountReceivableModes.NegativeChargesAllowed.Code,	OSExTaxAmount = -5,	HasTransactionHeader = true,	TransactionHeaderIsCancelled = false,	IsDebtorVATIsNotApplicable = true,	Allowed = true },
						new { TransactionLineNegativeAmountAllowedOnAccountReceivableModes.NegativeChargesAllowed.Code,	OSExTaxAmount = 5,	HasTransactionHeader = true,	TransactionHeaderIsCancelled = true,	IsDebtorVATIsNotApplicable = true,	Allowed = true },
						new { TransactionLineNegativeAmountAllowedOnAccountReceivableModes.NegativeChargesAllowed.Code,	OSExTaxAmount = -5,	HasTransactionHeader = true,	TransactionHeaderIsCancelled = true,	IsDebtorVATIsNotApplicable = true,	Allowed = true },
						new { TransactionLineNegativeAmountAllowedOnAccountReceivableModes.NegativeChargesAllowed.Code,	OSExTaxAmount = 5,  HasTransactionHeader = true,    TransactionHeaderIsCancelled = false,   IsDebtorVATIsNotApplicable = false,	Allowed = true },
						new { TransactionLineNegativeAmountAllowedOnAccountReceivableModes.NegativeChargesAllowed.Code,	OSExTaxAmount = -5, HasTransactionHeader = true,    TransactionHeaderIsCancelled = false,   IsDebtorVATIsNotApplicable = false,	Allowed = invoice.AH_Ledger != LedgerTypes.AccountsReceivable || !isRevenueType },
						new { TransactionLineNegativeAmountAllowedOnAccountReceivableModes.NegativeChargesAllowed.Code,	OSExTaxAmount = 5,  HasTransactionHeader = true,    TransactionHeaderIsCancelled = true,    IsDebtorVATIsNotApplicable = false,	Allowed = true },
						new { TransactionLineNegativeAmountAllowedOnAccountReceivableModes.NegativeChargesAllowed.Code,	OSExTaxAmount = -5, HasTransactionHeader = true,    TransactionHeaderIsCancelled = true,    IsDebtorVATIsNotApplicable = false,	Allowed = true },
						new { TransactionLineNegativeAmountAllowedOnAccountReceivableModes.NegativeChargesAllowed.Code,	OSExTaxAmount = 5,	HasTransactionHeader = false,	TransactionHeaderIsCancelled = false,	IsDebtorVATIsNotApplicable = false,	Allowed = true },
						new { TransactionLineNegativeAmountAllowedOnAccountReceivableModes.NegativeChargesAllowed.Code,	OSExTaxAmount = -5,	HasTransactionHeader = false,	TransactionHeaderIsCancelled = false,	IsDebtorVATIsNotApplicable = false,	Allowed = true },
				};

				foreach (var codeGroup in testDataItems.GroupBy(testDataItem => testDataItem.Code))
				{
					using (AccountingMasterFilesRegistry.Instance.NegativeAmountAllowedOnAccountReceivableTransactions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, codeGroup.Key))
					{
						foreach (var testDataItem in codeGroup)
						{
							var testDataItemContext = FormattableString.Invariant($"Context: Code={testDataItem.Code}, Type={invoicingBaseType.Name}, Amount={testDataItem.OSExTaxAmount}, HasTransactionHeader={testDataItem.HasTransactionHeader}, TransactionHeaderIsCancelled={testDataItem.TransactionHeaderIsCancelled}, Allowed={testDataItem.Allowed}");
							invoiceLine.AL_AH = testDataItem.HasTransactionHeader ? invoice.PK : ZGuid.Empty;
							invoice.AH_OH = testDataItem.IsDebtorVATIsNotApplicable ? org1.PK : org2.PK;
							invoiceLine.AL_OSExTaxAmount = testDataItem.OSExTaxAmount;
							invoice.IsCancelled = testDataItem.TransactionHeaderIsCancelled;
							if (testDataItem.Allowed)
							{
								AssertNoErrors(testDataItemContext, invoiceLine.AL_OSExTaxAmountInfo);
							}
							else
							{
								AssertHasError(testDataItemContext, invoiceLine.AL_OSExTaxAmountInfo, "Negative revenue charges are not allowed. This is controlled by the registry setting at Accounting > Receivable defaults > Default Settings > Negative Charges on Accounts Receivable Transactions.");
							}
						}
					}
				}
			}
		}

		public void TestAL_OSExTaxAmount_AmountCannotBeSet()
		{
			var invoicingBaseTypes = new[] { typeof(ARInvoice), typeof(APInvoice), typeof(ARCreditNote), typeof(APCreditNote), typeof(ARAdjustmentNote), typeof(APAdjustmentNote) };
			foreach (var invoicingBaseType in invoicingBaseTypes)
			{
				var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
				chargeCode.AC_ChargeType = Constants.ChargeType.Comment;

				var invoice = TestObjectCreator.CreateInvoice(invoicingBaseType);
				var invoiceLine = (InvoicingLineBase)invoice.Lines.AddNew();
				invoiceLine.AL_AC = chargeCode.PK;
				invoiceLine.AL_OSExTaxAmount = 100;
				AssertHasError(invoiceLine.AL_OSExTaxAmountInfo, "Line amount cannot be set if there is a comment charge entered");
			}
		}

		public void TestRevenueRecognitionTypeFromJobDuringPreSaveValidationErrorMessage()
		{
			if (InvoiceType == typeof(APInvoice) || InvoiceType == typeof(APCreditNote))
			{
				var shipment = TestObjectCreator.CreateShipment("S001001", false);
				var job = TestObjectCreator.CreateJob(shipment, false);
				var jobCharge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, 100M, 0M);
				Factory.Save();

				var invoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), "234", TestObjectCreator.AUD, 1M);
				var line = (InvoicingLineBase)invoice.Lines.AddNew();
				line.AL_JH = job.PK;
				invoice.ImportJobChargesIntoInvoice(new[] { jobCharge }.ToList(), line);

				AssertNotNull("Original job charge is not null", line.OriginalJobCharge);
				Assert("Original job charge not posted yet", !line.OriginalJobCharge.IsCostPosted);

				var validation = ((InvoicingLineBaseValidation)line.Validation);
				validation.ValidateAL_JH();
				AssertContains("Revenue Recognition Info", "\r\nRevenueRecognitionTypeFromJobDuringPreSaveValidation: There is no data collected for this PK.",
				CriticalValidationInfoCollectorService.GetOrCreateService(Factory).GetInfo(jobCharge.ChargeCode.PK, CriticalValidationInfoCollectorServiceKeyType.RevenueRecognitionTypeFromJobDuringPreSaveValidation));

				line.AL_RevRecognitionType = ZString.Empty;

				AssertEquals("Revenue Recognition Info", FormattableString.Invariant($@"
RevenueRecognitionTypeFromJobDuringPreSaveValidation:
ChargeCode PK: {jobCharge.ChargeCode.PK}
Job PK: {job.PK}

Revenue Recognition Type Details: 
Revenue Recognition Type: IMM 
JobType: SHP
Direction: OTH
Mode: SEA
Broker: 

GetRevenueRecognitionValidationError result: "),
				CriticalValidationInfoCollectorService.GetOrCreateService(Factory).GetInfo(jobCharge.ChargeCode.PK, CriticalValidationInfoCollectorServiceKeyType.RevenueRecognitionTypeFromJobDuringPreSaveValidation));
			}
			else
			{
				Assert("Not applicable", true);
			}
		}

		public void TestDescriptionMinimumLength_HasError_InIndiaCompanyWhenEligibleForEInvoicingOnly()
		{
			var isTestApplicable = InvoiceType == typeof(ARInvoice) || InvoiceType == typeof(ARCreditNote);
			if (!isTestApplicable)
			{
				Assert("Not applicable: validation only applies to AR Invoices & AR Credit Notes, as per ElectronicInvoicingEligibilityDecider", true);
				return;
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.India))
			{
				var invoice = (InvoicingBase)Factory.New(InvoiceType);
				var line = (InvoicingLineBase)invoice.Lines.AddNew();

				line.AL_Desc = "1";
				AssertHasError(line.AL_DescInfo, "Description has less than 3 characters.");

				line.AL_Desc = "123";
				AssertNoError(line.AL_DescInfo, "Description has less than 3 characters.");
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes._TemplateCountryName_))
			{
				var invoice = (InvoicingBase)Factory.New(InvoiceType);
				var line = (InvoicingLineBase)invoice.Lines.AddNew();

				line.AL_Desc = "1";
				AssertNoError(line.AL_DescInfo, "Description has less than 3 characters.");

				line.AL_Desc = "123";
				AssertNoError(line.AL_DescInfo, "Description has less than 3 characters.");
			}
		}

		public virtual void TestAllowTaxRecoveryLineWithDifferentBranchWhenBranchLevelPostingIsEnabled()
		{
			var testObjectCreator = new TestObjectCreator(Factory);

			var branch1 = testObjectCreator.CreateBranch("ABC", GlbCompany.CurrentCompany);
			var branch2 = testObjectCreator.CreateBranch("DEF", GlbCompany.CurrentCompany);

			var headerWithLines = (TransactionHeaderWithLines)Factory.New(GetExpectedParentBusinessObjectType());

			var expectedError = @"Please review the charge lines entered and ensure all charges have been entered belong to the same Posting Group. All charges posted in the one transaction must be in the same Posting Group.
Posting is prevented because charges have been entered using a mix of Posting Groups.";

			var branchLevelPostingConfiguration = new BranchLevelPostingConfiguration() { EnableBranchLevelPosting = true };
			headerWithLines.EnforceBranchLevelPostingRegistryItem.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, branchLevelPostingConfiguration);

			headerWithLines.Lines.AddNew();
			headerWithLines.Lines.AddNew();

			headerWithLines.Lines[0].AL_GB = branch1.PK; // Line 1
			headerWithLines.Lines[1].AL_GB = branch2.PK; // Line 2

			var taxRecordParent = TaxFrameworkObjectFactory.GetInvoicingBaseTaxRecordParent((InvoicingBase)headerWithLines) as ITaxRecordParent;
			taxRecordParent.AddTaxRecoveryLine(testObjectCreator.CC1.PK, headerWithLines.Lines[0].AL_JH, branch1.PK, headerWithLines.AH_GE, headerWithLines.AH_RX_NKTransactionCurrency, 50m, ZDate.Today, ZGuid.Empty, ZString.Empty); // Tax recovery line

			headerWithLines.RunPreSaveValidation();
			AssertHasError("Error status on line 1 when line 1 and line 2 have different branches", headerWithLines.Lines[0].AL_GBInfo, expectedError);
			AssertHasError("Error status on line 2 when line 1 and line 2 have different branches", headerWithLines.Lines[1].AL_GBInfo, expectedError);
			AssertNoError("Error status on tax recovery line when line 1 and line 2 have different branches (tax recovery line branch does not matter)", headerWithLines.Lines[2].AL_GBInfo, expectedError);

			headerWithLines.Lines[1].AL_GB = branch1.PK;
			headerWithLines.RunPreSaveValidation();
			AssertNoError("Error status on line 1 when line 1 and line 2 have different branches", headerWithLines.Lines[0].AL_GBInfo, expectedError);
			AssertNoError("Error status on line 2 when line 1 and line 2 have different branches", headerWithLines.Lines[1].AL_GBInfo, expectedError);
			AssertNoError("Error status on tax recovery line when line 1 and line 2 have different branches (tax recovery line branch does not matter)", headerWithLines.Lines[2].AL_GBInfo, expectedError);
		}

		#region Implementation

		protected abstract Type InvoiceLineType { get; }
		protected abstract Type InvoiceType { get; }

		protected InvoicingBase InvoiceToValidate
		{
			get { return invoiceToValidate ?? (invoiceToValidate = (InvoicingBase)Factory.New(InvoiceType)); }
		}
		InvoicingBase invoiceToValidate;

		protected AccountingPeriodTestHelper PeriodManagementTestHelper;
		protected bool PreTestCurrentCompanyGSTRegistered;
		protected TestObjectCreator TestObjectCreator;
		AccTaxRate fGST10TaxRate;
		OrgHeader fGSTRegisteredOrg;
		OrgHeader fNonGSTRegisteredOrg;
		OrgHeader fWHTRegisteredOrg;

		Job fTestJob;
		protected virtual Job TestJob
		{
			get
			{
				if (fTestJob == null)
				{
					fTestJob = TestObjectCreator.CreateJob(Creditor1, 0, null, 0);
				}

				return fTestJob;
			}
		}

		protected AccTaxRate SPV22TaxRate => spv22TaxRate ?? (spv22TaxRate = TestObjectCreator.CreateTaxRate("SPV22", "Test SPV Code", "RAT", 10, "SPV", 0, 1));
		AccTaxRate spv22TaxRate;

		protected AccTaxRate GST10TaxRate
		{
			get
			{
				if (fGST10TaxRate == null)
				{
					fGST10TaxRate = TestObjectCreator.CreateTaxRate("TSTGST", "Test GST Code", 10);
				}
				return fGST10TaxRate;
			}
		}

		protected OrgHeader GSTRegisteredOrg
		{
			get
			{
				if (fGSTRegisteredOrg == null)
				{
					fGSTRegisteredOrg = TestObjectCreator.CreateOrgHeader("TSTREGORG", true, true, true, false, true, false);
				}
				return fGSTRegisteredOrg;
			}
		}

		protected OrgHeader NonGSTRegisteredOrg
		{
			get
			{
				if (fNonGSTRegisteredOrg == null)
				{
					fNonGSTRegisteredOrg = TestObjectCreator.CreateOrgHeader("TSTNONREG", true, true, false, false, false, false);
				}
				return fNonGSTRegisteredOrg;
			}
		}

		protected OrgHeader WHTRegisteredOrg
		{
			get
			{
				if (fWHTRegisteredOrg == null)
				{
					fWHTRegisteredOrg = TestObjectCreator.CreateOrgHeader("TSTWHTREG", true, true, false, true, false, true);
				}
				return fWHTRegisteredOrg;
			}
		}

		protected OrgHeader fCreditor1;
		protected OrgHeader Creditor1
		{
			get
			{
				if (fCreditor1 == null)
				{
					fCreditor1 = TestObjectCreator.CreateOrgHeader("CREDITOR1", true, false, true, false, false, false);
				}

				return fCreditor1;
			}
		}

		AccChargeCode fMargin100Code;
		protected AccChargeCode Margin100Code
		{
			get
			{
				if (fMargin100Code == null)
				{
					fMargin100Code = TestObjectCreator.CreateChargeCode("MRG100", "Margin100%", Constants.ChargeType.Margin, 100, GST, WHT, "ALL");
				}

				return fMargin100Code;
			}
		}

		AccChargeCode fDisbursementChargeCode;
		protected AccChargeCode DisbursementChargeCode
		{
			get
			{
				if (fDisbursementChargeCode == null)
				{
					fDisbursementChargeCode = TestObjectCreator.CreateChargeCode("DSB", "Disbursement", Constants.ChargeType.Disbursement, 100, GST, WHT, "ALL");
				}

				return fDisbursementChargeCode;
			}
		}

		AccChargeCode fFEADepartmentChargeCode;
		protected AccChargeCode FEADepartmentChargeCodeInDB
		{
			get
			{
				if (fFEADepartmentChargeCode == null)
				{
					fFEADepartmentChargeCode = TestObjectCreator.CreateChargeCode("FEACHRG", "Charge for Department Test", Constants.ChargeType.Disbursement, 0M, GST, WHT, "FEA");
				}

				Factory.Save();
				return fFEADepartmentChargeCode;
			}
		}

		AccChargeCode fALLDepartmentChargeCode;
		protected AccChargeCode ALLDepartmentChargeCodeInDB
		{
			get
			{
				if (fALLDepartmentChargeCode == null)
				{
					fALLDepartmentChargeCode = TestObjectCreator.CreateChargeCode("ALLCHRG", "Charge for Department Test", Constants.ChargeType.Disbursement, 0M, GST, WHT, "ALL");
				}

				Factory.Save();
				return fALLDepartmentChargeCode;
			}
		}

		AccTaxRate fGST;
		protected AccTaxRate GST
		{
			get
			{
				if (fGST == null)
				{
					fGST = TestObjectCreator.CreateTaxRate("GST", "GSTRate", 10);
				}

				return fGST;
			}
		}

		AccWithholding fWHT;
		protected AccWithholding WHT
		{
			get
			{
				if (fWHT == null)
				{
					fWHT = TestObjectCreator.CreateOrLoadWithholdingTax("WHT", "WHTRate", 5);
				}

				return fWHT;
			}
		}

		protected override void SetUp()
		{
			base.SetUp();

			TestCaseHelper.ClearTable(AutoAccPeriodManagement.Schema.TableName);
			PeriodManagementTestHelper = new AccountingPeriodTestHelper();
			PeriodManagementTestHelper.SetupPeriods();
			PreTestCurrentCompanyGSTRegistered = GlbCompany.CurrentCompany.GC_IsGSTRegistered;
			TestObjectCreator = new TestObjectCreator(Factory);
		}

		protected override void TearDown()
		{
			base.TearDown();

			GlbCompany.CurrentCompany.GC_IsGSTRegistered = PreTestCurrentCompanyGSTRegistered;
		}

		AccChargeCode CreateChargeCode(string code, string description, string chargeType, decimal marginPercentage, AccTaxRate gST, AccWithholding wHT)
		{
			AccChargeCode chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode.AC_Code = code;
			chargeCode.AC_Desc = description;
			chargeCode.AC_ChargeType = chargeType;
			chargeCode.AC_MarginPercentage = marginPercentage;
			chargeCode.AC_AT_GSTRate = (gST == null) ? ZGuid.Empty : gST.PK;
			chargeCode.AC_AW_WithholdingTaxRate = (wHT == null) ? ZGuid.Empty : wHT.PK;
			chargeCode.AC_AG_CostAccount = TestObjectCreator.GLHeader1.PK;
			chargeCode.AC_AG_RevenueAccount = TestObjectCreator.GLHeader2.PK;

			return chargeCode;
		}

		protected void SetGenericCharge(InvoicingLineBase line, InvoicingBase invoice)
		{
			ZQuery testFilter = new ZQuery();

			testFilter.AddToFilter(ViewGenericChargeSchema.VC_IsActive, ZBool.True);
			if (invoice.AH_TransactionType == TransactionTypes.Invoice && invoice.AH_Ledger == LedgerTypes.AccountsReceivable)
			{
				testFilter.AddToFilter(ViewGenericChargeSchema.VC_IsGLAccount, ZBool.True);
				testFilter.AddToFilter(ViewGenericChargeSchema.VC_Type, "BSH");
				testFilter.AddToFilter(ViewGenericChargeSchema.VC_IsControlAccount, ZBool.False);
				testFilter.AddToFilter(ViewGenericChargeSchema.VC_DisallowDirectPosting, false);
			}
			else
			{
				testFilter.AddToFilter(ViewGenericChargeSchema.VC_Type, "OVR");
				testFilter.AddToFilter(ViewGenericChargeSchema.VC_GC, GlbCompany.CurrentCompany.PK);
			}

			GenericCharge.GenericCharge testGenericCharge = Factory.LoadTop1(typeof(GenericCharge.GenericCharge), testFilter) as GenericCharge.GenericCharge;
			line.GenericCharge = testGenericCharge.PK;
		}

		#endregion
	}
}
