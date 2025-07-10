using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Accounting.Business.WIPAccrual;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.QuotedBookings.Business;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static System.FormattableString;
using static Enterprise.Accounting.Business.AccountingConstants;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business.JobInvoicing.Testing
{
	public abstract class BaseChargeValidationTest : JobChargeValidationTest
	{
		#region Cash Advance

		public void TestCanNotChangeCreditorWhenActiveCashAdvanceRequestExistAndCashAdvanceFunctionalityIsEnabled()
		{
			AssertCanNotChangeCriticalChargeDetailsWhenActiveCashAdvanceRequestExist(JobChargeSchema.Constants.JR_OH_CostAccount, TestObjectCreator.Creditor1.PK, true);
		}

		public void TestCanNotChangeCreditorWhenActiveCashAdvanceRequestExistAndCashAdvanceFunctionalityIsDisabled()
		{
			AssertCanNotChangeCriticalChargeDetailsWhenActiveCashAdvanceRequestExist(JobChargeSchema.Constants.JR_OH_CostAccount, TestObjectCreator.Creditor1.PK, false);
		}

		public void TestCanNotChangeCostCurrencyWhenActiveCashAdvanceRequestExistAndCashAdvanceFunctionalityIsEnabled()
		{
			AssertCanNotChangeCriticalChargeDetailsWhenActiveCashAdvanceRequestExist(JobChargeSchema.Constants.JR_RX_NKCostCurrency, TestObjectCreator.USD.RX_Code, true);
		}

		public void TestCanNotChangeCostCurrencyWhenActiveCashAdvanceRequestExistAndCashAdvanceFunctionalityIsDisabled()
		{
			AssertCanNotChangeCriticalChargeDetailsWhenActiveCashAdvanceRequestExist(JobChargeSchema.Constants.JR_RX_NKCostCurrency, TestObjectCreator.USD.RX_Code, false);
		}

		public void TestCanNotChangeDebtorWhenActiveCashAdvanceRequestExistAndCashAdvanceFunctionalityIsEnabled()
		{
			AssertCanNotChangeCriticalChargeDetailsWhenActiveCashAdvanceRequestExist(JobChargeSchema.Constants.JR_OH_SellAccount, TestObjectCreator.Debtor.PK, true);
		}

		public void TestCanNotChangeDebtorWhenActiveCashAdvanceRequestExistAndCashAdvanceFunctionalityIsDisabled()
		{
			AssertCanNotChangeCriticalChargeDetailsWhenActiveCashAdvanceRequestExist(JobChargeSchema.Constants.JR_OH_SellAccount, TestObjectCreator.Debtor.PK, false);
		}

		public void TestCanNotChangeSellCurrencyWhenActiveCashAdvanceRequestExistAndCashAdvanceFunctionalityIsEnabled()
		{
			AssertCanNotChangeCriticalChargeDetailsWhenActiveCashAdvanceRequestExist(JobChargeSchema.Constants.JR_RX_NKSellCurrency, TestObjectCreator.USD.RX_Code, true);
		}

		public void TestCanNotChangeSellCurrencyWhenActiveCashAdvanceRequestExistAndCashAdvanceFunctionalityIsDisabled()
		{
			AssertCanNotChangeCriticalChargeDetailsWhenActiveCashAdvanceRequestExist(JobChargeSchema.Constants.JR_RX_NKSellCurrency, TestObjectCreator.USD.RX_Code, false);
		}

		public void TestCanNotChangeDebtorToNoValueWhenActiveCashAdvanceRequestExistAndCashAdvanceFunctionalityIsDisabled()
		{
			AssertCanNotChangeCriticalChargeDetailsWhenActiveCashAdvanceRequestExist(JobChargeSchema.Constants.JR_OH_SellAccount, ZString.Empty, false);
		}

		public void TestCanNotChangeDebtorToNoValueWhenActiveCashAdvanceRequestExistAndCashAdvanceFunctionalityIsEnabled()
		{
			AssertCanNotChangeCriticalChargeDetailsWhenActiveCashAdvanceRequestExist(JobChargeSchema.Constants.JR_OH_SellAccount, ZString.Empty, true);
		}

		void AssertCanNotChangeCriticalChargeDetailsWhenActiveCashAdvanceRequestExist(string propertyName, IZType newValueToSet, bool isCashAdvanceFunctionalityEnabled)
		{
			bool isARCashAdvance = propertyName == JobChargeSchema.Constants.JR_RX_NKSellCurrency || propertyName == JobChargeSchema.Constants.JR_OH_SellAccount;
			var expectedErrorMessage = (propertyName == JobChargeSchema.Constants.JR_RX_NKSellCurrency ? "This charge has an active AR Advance Payment with Currency AUD. Please change the currency to match the Advance Payment, or cancel the Advance Payment to continue." :
				(propertyName == JobChargeSchema.Constants.JR_RX_NKCostCurrency ? "This charge has an active AP Advance Payment with Currency AUD. Please change the currency to match the Advance Payment, or cancel the Advance Payment to continue." :
				(propertyName == JobChargeSchema.Constants.JR_OH_SellAccount ? "This charge has an active AR Advance Payment with Organization ABIGAS. Please change the Organization to match the Advance Payment, or cancel the Advance Payment to continue." :
				"This charge has an active AP Advance Payment with Organization AALSHI. Please change the Organization to match the Advance Payment, or cancel the Advance Payment to continue.")));

			using (AccountingConfigurationRegistry.Instance.EnableReceivablesCashAdvanceFunctionality.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, isCashAdvanceFunctionalityEnabled))
			{
				var shipment = TestObjectCreator.CreateShipment("S00001011");
				var job = new Job.Loader(shipment).TryLoadOrCreateWithoutMutexForTestOnly();
				var charge1 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "", TestObjectCreator.AUD, 100m, TestObjectCreator.AALSHI, TestObjectCreator.AUD, 100m, TestObjectCreator.ABIGAS);
				Factory.Save();

				var propertyInfoToTest = charge1.FindPropertyInfo(propertyName);
				var originalValue = charge1[propertyName];
				charge1[propertyName] = newValueToSet;
				charge1.RunPreSaveValidation();
				Assert("Advance Payment Request is not created yet.", !propertyInfoToTest.HasError(expectedErrorMessage));
				charge1[propertyName] = originalValue;

				if (isARCashAdvance)
				{
					charge1.JR_IsARCashAdvance = true;
				}
				else
				{
					charge1.JR_IsAPCashAdvance = true;
				}
				Factory.Save();

				charge1[propertyName] = newValueToSet;
				charge1.RunPreSaveValidation();
				Assert("Advance Payment Request is not created yet.", !propertyInfoToTest.HasError(expectedErrorMessage));
				charge1[propertyName] = originalValue;

				var ledger = isARCashAdvance ? LedgerTypes.AccountsReceivable : LedgerTypes.AccountsPayable;
				var organization = isARCashAdvance ? TestObjectCreator.ABIGAS : TestObjectCreator.AALSHI;
				var cashAdvanceHeader = TestObjectCreator.CreateCashAdvanceRequestHeader(job.PK, organization.PK, ledger, 100m, 100m, TestObjectCreator.AUD.RX_Code, CashAdvanceStatusCodes.RequestHeader.Requested);
				var cashAdvanceLine1 = TestObjectCreator.CreateCashAdvanceRequestLine(cashAdvanceHeader.PK, 100m, 100m, CashAdvanceStatusCodes.RequestLine.Requested);
				if (isARCashAdvance)
				{
					charge1.JR_CAL_ARLine = cashAdvanceLine1.PK;
				}
				else
				{
					charge1.JR_CAL_APLine = cashAdvanceLine1.PK;
				}
				Factory.Save();

				charge1[propertyName] = newValueToSet;
				charge1.RunPreSaveValidation();

				if (isCashAdvanceFunctionalityEnabled)
				{
					AssertHasError("cashAdvanceLine1 is in REQ status.", propertyInfoToTest, expectedErrorMessage);
				}
				else
				{
					Assert(!propertyInfoToTest.HasError(expectedErrorMessage));
				}
				charge1[propertyName] = originalValue;

				cashAdvanceLine1.CAL_Status = CashAdvanceStatusCodes.RequestLine.Paid;
				cashAdvanceLine1.CAL_LocalPaidAmount = cashAdvanceLine1.CAL_OSPaidAmount = 100m;
				cashAdvanceHeader.CAH_Status = CashAdvanceStatusCodes.RequestHeader.Paid;
				cashAdvanceHeader.CAH_LocalPaidAmount = cashAdvanceHeader.CAH_OSPaidAmount = 100m;
				Factory.Save();

				charge1[propertyName] = newValueToSet;
				charge1.RunPreSaveValidation();
				if (isCashAdvanceFunctionalityEnabled)
				{
					AssertHasError("cashAdvanceLine1 is in PAI status.", propertyInfoToTest, expectedErrorMessage);
				}
				else
				{
					Assert(!propertyInfoToTest.HasError(expectedErrorMessage));
				}
				charge1[propertyName] = originalValue;

				cashAdvanceLine1.CAL_Status = CashAdvanceStatusCodes.RequestLine.Invoiced;
				cashAdvanceHeader.CAH_Status = CashAdvanceStatusCodes.RequestHeader.Invoiced;
				Factory.Save();

				charge1[propertyName] = newValueToSet;
				charge1.RunPreSaveValidation();
				if (isCashAdvanceFunctionalityEnabled)
				{
					AssertHasError("cashAdvanceLine1 is in INV status.", propertyInfoToTest, expectedErrorMessage);
				}
				else
				{
					Assert(!propertyInfoToTest.HasError(expectedErrorMessage));
				}
				charge1[propertyName] = originalValue;

				cashAdvanceLine1.CAL_Status = CashAdvanceStatusCodes.RequestLine.Cancelled;
				cashAdvanceLine1.CAL_LocalPaidAmount = cashAdvanceLine1.CAL_OSPaidAmount = 0m;
				cashAdvanceHeader.CAH_Status = CashAdvanceStatusCodes.RequestHeader.Cancelled;
				cashAdvanceHeader.CAH_LocalPaidAmount = cashAdvanceHeader.CAH_OSPaidAmount = 0m;
				Factory.Save();

				charge1[propertyName] = newValueToSet;
				charge1.RunPreSaveValidation();
				Assert("cashAdvanceLine1 is in CAN status.", !propertyInfoToTest.HasError(expectedErrorMessage));
			}
		}

		#endregion

		#region Cost & Sell Tax Branch

		public void TestCheckJR_GB_CostTaxBranch()
		{
			var nonCurrentCompany = Factory.NewWithValidTestData<GlbCompany>();
			var nonCurrentCompanyBranch = Factory.NewWithValidTestData<GlbBranch>();
			nonCurrentCompanyBranch.GB_GC = nonCurrentCompany.PK;

			var currentCompanyUnActiveBranch = Factory.NewWithValidTestData<GlbBranch>();
			currentCompanyUnActiveBranch.GB_GC = GlbCompany.CurrentCompany.PK;
			currentCompanyUnActiveBranch.GB_IsActive = false;

			var shipment = TestObjectCreator.CreateShipment("S00000001", null);
			var shipmentJob = new Job.Loader(shipment).TryLoadOrCreateWithoutMutexForTestOnly();
			TestObjectCreator.Creditor1.CompanyData.OB_APVATConfig = AccountingMasterFilesConstants.OrganisationTaxConfiguartionTypes.Default.Code;
			Factory.Save();

			AssertCheckJR_GB_CostTaxBranch(true);
			AssertCheckJR_GB_CostTaxBranch(false);

			void AssertCheckJR_GB_CostTaxBranch(bool enableTaxBranchReporting)
			{
				using (AccountingMasterFilesRegistry.Instance.EnableTaxBranchReporting.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, enableTaxBranchReporting))
				{
					var charge = shipmentJob.Charges.AddNew();
					charge.JR_AC = TestObjectCreator.CC1.PK;
					charge.JR_OH_CostAccount = TestObjectCreator.Creditor1.PK;
					AssertEquals("Precondition", false, charge.IsCostPosted);
					AssertEquals("Precondition", !enableTaxBranchReporting, charge.JR_GB_CostTaxBranchInfo.ReadOnly);

					charge.Validation.ValidateJR_GB_CostTaxBranch();
					AssertEquals(enableTaxBranchReporting, charge.JR_GB_CostTaxBranchInfo.HasError("Please enter a Cost Tax Branch."));

					charge.JR_GB_CostTaxBranch = nonCurrentCompanyBranch.PK;
					AssertEquals(enableTaxBranchReporting, charge.JR_GB_CostTaxBranchInfo.HasError("Enter a valid Cost Tax Branch."));

					charge.JR_GB_CostTaxBranch = currentCompanyUnActiveBranch.PK;
					AssertEquals(enableTaxBranchReporting, charge.JR_GB_CostTaxBranchInfo.HasError("This Cost Tax Branch is inactive - it may not be used."));

					charge.JR_GB_CostTaxBranch = GlbBranch.CurrentBranch.PK;
					AssertEquals(!enableTaxBranchReporting, charge.JR_GB_CostTaxBranchInfo.HasError(@"Tax branch values on unposted charges in the billing tab conflict with at least one of the following settings:
- 'Enabled Tax Branch Reporting' registry value
- Debtor / Creditor 'Tax is Applicable' flag
- Current Login Company 'VAT Registered' flag
One possible way to resolve this is to go into the ""Job Invoicing"" menu and click the ""Reset Unposted lines Tax Default"" option."));

					charge.JR_GB_CostTaxBranch = ZGuid.Invalid;
					AssertEquals(true, charge.JR_GB_CostTaxBranchInfo.HasError("Enter a valid Cost Tax Branch."));

					var line = Factory.NewWithValidTestData<AccTransactionLines>();
					line.AL_LineType = TransactionLineTypes.Cost;
					charge.JR_AL_APLine = line.PK;
					AssertEquals("Precondition", true, charge.IsCostPosted);
					AssertEquals("Precondition", true, charge.JR_GB_CostTaxBranchInfo.ReadOnly);
					charge.JR_GB_CostTaxBranch = ZGuid.Empty;
					AssertEquals(false, charge.JR_GB_CostTaxBranchInfo.HasErrors());

					line.Delete();
					charge.JR_AL_APLine = ZGuid.Empty;
					charge.JR_GB_CostTaxBranch = nonCurrentCompanyBranch.PK;
					Factory.Save();
					charge.Validation.ValidateJR_GB_CostTaxBranch();
					AssertEquals(enableTaxBranchReporting, charge.JR_GB_CostTaxBranchInfo.HasError("Enter a valid Cost Tax Branch."));
				}
			}
		}

		public void TestCheckJR_GB_CostTaxBranch_ShouldHaveSameTaxBranch()
		{
			var shipment = TestObjectCreator.CreateShipment("S00000001", null);
			var shipmentJob = new Job.Loader(shipment).TryLoadOrCreateWithoutMutexForTestOnly();
			TestObjectCreator.Creditor1.CompanyData.OB_APVATConfig = AccountingMasterFilesConstants.OrganisationTaxConfiguartionTypes.Default.Code;
			TestObjectCreator.Creditor2.CompanyData.OB_APVATConfig = AccountingMasterFilesConstants.OrganisationTaxConfiguartionTypes.Default.Code;
			Factory.Save();

			AssertCheckJR_GB_CostTaxBranch_ShouldHaveSameTaxBranch(true);
			AssertCheckJR_GB_CostTaxBranch_ShouldHaveSameTaxBranch(false);

			void AssertCheckJR_GB_CostTaxBranch_ShouldHaveSameTaxBranch(bool enableTaxBranchReporting)
			{
				var error = "All cost lines with the same Creditor and AP Invoice Number must have the same Cost Tax Branch.";
				using (AccountingMasterFilesRegistry.Instance.EnableTaxBranchReporting.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, enableTaxBranchReporting))
				{
					//Tax Branches are different
					var charge1 = PrepareCharge(TestObjectCreator.Creditor1.PK, "Test001", TestObjectCreator.NonCurrentBranch.PK, false);
					var charge2 = PrepareCharge(TestObjectCreator.Creditor1.PK, "Test001", GlbBranch.CurrentBranch.PK, false);
					AssertEquals(enableTaxBranchReporting, charge1.JR_GB_CostTaxBranchInfo.HasError(error));
					AssertEquals(enableTaxBranchReporting, charge2.JR_GB_CostTaxBranchInfo.HasError(error));

					//Creditors & InvoiceNums & Tax Branch are all equal 
					charge1 = PrepareCharge(TestObjectCreator.Creditor1.PK, "Test002", TestObjectCreator.NonCurrentBranch.PK, false);
					charge2 = PrepareCharge(TestObjectCreator.Creditor1.PK, "Test002", TestObjectCreator.NonCurrentBranch.PK, false);
					AssertEquals(false, charge1.JR_GB_CostTaxBranchInfo.HasError(error));
					AssertEquals(false, charge2.JR_GB_CostTaxBranchInfo.HasError(error));

					//InvoiceNums & Tax Branch are different
					charge1 = PrepareCharge(TestObjectCreator.Creditor1.PK, "Test003", TestObjectCreator.NonCurrentBranch.PK, false);
					charge2 = PrepareCharge(TestObjectCreator.Creditor1.PK, "Test003_1", GlbBranch.CurrentBranch.PK, false);
					AssertEquals(false, charge1.JR_GB_CostTaxBranchInfo.HasError(error));
					AssertEquals(false, charge2.JR_GB_CostTaxBranchInfo.HasError(error));

					//Creditors & Tax Branch are different
					charge1 = PrepareCharge(TestObjectCreator.Creditor1.PK, "Test004", TestObjectCreator.NonCurrentBranch.PK, false);
					charge2 = PrepareCharge(TestObjectCreator.Creditor2.PK, "Test004", GlbBranch.CurrentBranch.PK, false);
					AssertEquals(false, charge1.JR_GB_CostTaxBranchInfo.HasError(error));
					AssertEquals(false, charge2.JR_GB_CostTaxBranchInfo.HasError(error));

					//Invoice is Posted & Tax Branch are different
					charge1 = PrepareCharge(TestObjectCreator.Creditor1.PK, "Test006", TestObjectCreator.NonCurrentBranch.PK, false);
					charge2 = PrepareCharge(TestObjectCreator.Creditor1.PK, "Test006", GlbBranch.CurrentBranch.PK, true);
					AssertEquals(false, charge1.JR_GB_CostTaxBranchInfo.HasError(error));
					AssertEquals(false, charge2.JR_GB_CostTaxBranchInfo.HasError(error));

					//Invalid Tax Branch PK
					charge1 = PrepareCharge(TestObjectCreator.Creditor1.PK, "Test007", ZGuid.Invalid, false);
					charge2 = PrepareCharge(TestObjectCreator.Creditor1.PK, "Test007", GlbBranch.CurrentBranch.PK, true);
					AssertEquals(false, charge1.JR_GB_CostTaxBranchInfo.HasError(error));
					AssertEquals(false, charge2.JR_GB_CostTaxBranchInfo.HasError(error));

					//InvoiceNum is empty
					charge1 = PrepareCharge(TestObjectCreator.Creditor1.PK, "", TestObjectCreator.NonCurrentBranch.PK, false);
					charge2 = PrepareCharge(TestObjectCreator.Creditor1.PK, "", GlbBranch.CurrentBranch.PK, true);
					AssertEquals(false, charge1.JR_GB_CostTaxBranchInfo.HasError(error));
					AssertEquals(false, charge2.JR_GB_CostTaxBranchInfo.HasError(error));

					//Invalid Creditor PK
					charge1 = PrepareCharge(ZGuid.Invalid, "Test007", TestObjectCreator.NonCurrentBranch.PK, false);
					charge2 = PrepareCharge(TestObjectCreator.Creditor1.PK, "Test007", GlbBranch.CurrentBranch.PK, true);
					AssertEquals(false, charge1.JR_GB_CostTaxBranchInfo.HasError(error));
					AssertEquals(false, charge2.JR_GB_CostTaxBranchInfo.HasError(error));
				}
			}

			Charge PrepareCharge(ZGuid creditor, ZString invoiceNum, ZGuid taxBranch, bool isPosted)
			{
				var charge = shipmentJob.Charges.AddNew();
				charge.JR_AC = TestObjectCreator.CC1.PK;
				charge.JR_OH_CostAccount = creditor;
				charge.JR_APInvoiceNum = invoiceNum;
				charge.JR_GB_CostTaxBranch = taxBranch;

				if (isPosted)
				{
					var line = Factory.NewWithValidTestData<AccTransactionLines>();
					line.AL_LineType = TransactionLineTypes.Cost;
					charge.JR_AL_APLine = line.PK;
				}
				AssertEquals("Precondition", isPosted, charge.IsCostPosted);

				shipmentJob.RunPreSaveValidation();

				return charge;
			}
		}

		public void TestCheckJR_GB_SellTaxBranch()
		{
			var nonCurrentCompany = Factory.NewWithValidTestData<GlbCompany>();
			var nonCurrentCompanyBranch = Factory.NewWithValidTestData<GlbBranch>();
			nonCurrentCompanyBranch.GB_GC = nonCurrentCompany.PK;

			var currentCompanyUnActiveBranch = Factory.NewWithValidTestData<GlbBranch>();
			currentCompanyUnActiveBranch.GB_GC = GlbCompany.CurrentCompany.PK;
			currentCompanyUnActiveBranch.GB_IsActive = false;

			var shipment = TestObjectCreator.CreateShipment("S00000001", null);
			var shipmentJob = new Job.Loader(shipment).TryLoadOrCreateWithoutMutexForTestOnly();
			TestObjectCreator.Debtor1.CompanyData.OB_ARVATConfig = AccountingMasterFilesConstants.OrganisationTaxConfiguartionTypes.Default.Code;
			Factory.Save();

			AssertCheckJR_GB_SellTaxBranch(true);
			AssertCheckJR_GB_SellTaxBranch(false);

			void AssertCheckJR_GB_SellTaxBranch(bool enableTaxBranchReporting)
			{
				using (AccountingMasterFilesRegistry.Instance.EnableTaxBranchReporting.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, enableTaxBranchReporting))
				{
					var charge = shipmentJob.Charges.AddNew();
					charge.JR_AC = TestObjectCreator.CC1.PK;
					charge.JR_OH_SellAccount = TestObjectCreator.Debtor1.PK;
					AssertEquals("Precondition", false, charge.IsRevenuePosted);
					AssertEquals("Precondition", !enableTaxBranchReporting, charge.JR_GB_SellTaxBranchInfo.ReadOnly);

					charge.Validation.ValidateJR_GB_SellTaxBranch();
					AssertEquals(enableTaxBranchReporting, charge.JR_GB_SellTaxBranchInfo.HasError("Please enter a Sell Tax Branch."));

					charge.JR_GB_SellTaxBranch = nonCurrentCompanyBranch.PK;
					AssertEquals(enableTaxBranchReporting, charge.JR_GB_SellTaxBranchInfo.HasError("Enter a valid Sell Tax Branch."));

					charge.JR_GB_SellTaxBranch = currentCompanyUnActiveBranch.PK;
					AssertEquals(enableTaxBranchReporting, charge.JR_GB_SellTaxBranchInfo.HasError("This Sell Tax Branch is inactive - it may not be used."));

					charge.JR_GB_SellTaxBranch = GlbBranch.CurrentBranch.PK;
					AssertEquals(!enableTaxBranchReporting, charge.JR_GB_SellTaxBranchInfo.HasError(@"Tax branch values on unposted charges in the billing tab conflict with at least one of the following settings:
- 'Enabled Tax Branch Reporting' registry value
- Debtor / Creditor 'Tax is Applicable' flag
- Current Login Company 'VAT Registered' flag
One possible way to resolve this is to go into the ""Job Invoicing"" menu and click the ""Reset Unposted lines Tax Default"" option."));

					charge.JR_GB_SellTaxBranch = ZGuid.Invalid;
					AssertEquals(true, charge.JR_GB_SellTaxBranchInfo.HasError("Enter a valid Sell Tax Branch."));

					var line = Factory.NewWithValidTestData<AccTransactionLines>();
					line.AL_LineType = TransactionLineTypes.Revenue;
					charge.JR_AL_ARLine = line.PK;
					AssertEquals("Precondition", true, charge.IsRevenuePosted);
					AssertEquals("Precondition", true, charge.JR_GB_SellTaxBranchInfo.ReadOnly);
					charge.JR_GB_SellTaxBranch = ZGuid.Empty;
					AssertEquals(false, charge.JR_GB_SellTaxBranchInfo.HasErrors());

					line.Delete();
					charge.JR_AL_ARLine = ZGuid.Empty;
					charge.JR_GB_SellTaxBranch = nonCurrentCompanyBranch.PK;
					Factory.Save();
					charge.Validation.ValidateJR_GB_SellTaxBranch();
					AssertEquals(enableTaxBranchReporting, charge.JR_GB_SellTaxBranchInfo.HasError("Enter a valid Sell Tax Branch."));
				}
			}
		}

		#endregion

		#region Cost & Sell Supply Type

		public void TestCheckJR_CostSupplyType()
		{
			AssertJR_CostSupplyType(true);
			AssertJR_CostSupplyType(false);
			void AssertJR_CostSupplyType(bool enableSupplyTypeClassificationCodes)
			{
				var validValue = AccountingMasterFilesConstants.SupplyTypeClassificationCodes.LOC;
				using (AccountingMasterFilesRegistry.Instance.EnableSupplyTypeClassificationCodes.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, enableSupplyTypeClassificationCodes))
				{
					AssertSupplyType("JR_CostSupplyType", enableSupplyTypeClassificationCodes, true, validValue, "ERR", "Enter a valid Cost Supply Type.");
					AssertSupplyType("JR_CostSupplyType", enableSupplyTypeClassificationCodes, false, validValue, string.Empty, "The Cost Supply Type is not specified. Please check if a supply type is needed before posting.");
				}
			}
		}

		public void TestCheckJR_SellSupplyType()
		{
			AssertJR_SellSupplyType(true);
			AssertJR_SellSupplyType(false);
			void AssertJR_SellSupplyType(bool enableSupplyTypeClassificationCodes)
			{
				var validValue = AccountingMasterFilesConstants.SupplyTypeClassificationCodes.LOC;
				using (AccountingMasterFilesRegistry.Instance.EnableSupplyTypeClassificationCodes.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, enableSupplyTypeClassificationCodes))
				{
					AssertSupplyType("JR_SellSupplyType", enableSupplyTypeClassificationCodes, true, validValue, "ERR", "Enter a valid Sell Supply Type.");
					AssertSupplyType("JR_SellSupplyType", enableSupplyTypeClassificationCodes, false, validValue, string.Empty, "The Sell Supply Type is not specified. Please check if a supply type is needed before posting.");
				}
			}
		}

		void AssertSupplyType(string propertyName, bool shouldContainExpectedErrorOrWarning, bool testForError, ZString validValue, ZString inValidValue, string expectedErrorOrWarning)
		{
			var charge1 = Factory.NewWithValidTestData<Charge>();
			var charge2 = Factory.NewWithValidTestData<Charge>();
			var charge3 = Factory.NewWithValidTestData<Charge>();
			var line = Factory.NewWithValidTestData<AccTransactionLines>();
			if (propertyName.Contains("Sell"))
			{
				line.AL_LineType = TransactionLineTypes.Revenue;
				charge3.JR_AL_ARLine = line.PK;
				AssertEquals("Precondition", false, charge1.IsRevenuePosted);
				AssertEquals("Precondition", false, charge2.IsRevenuePosted);
				AssertEquals("Precondition", true, charge3.IsRevenuePosted);
			}
			else if (propertyName.Contains("Cost"))
			{
				line.AL_LineType = TransactionLineTypes.Cost;
				charge3.JR_AL_APLine = line.PK;
				AssertEquals("Precondition", false, charge1.IsCostPosted);
				AssertEquals("Precondition", false, charge2.IsCostPosted);
				AssertEquals("Precondition", true, charge3.IsCostPosted);
			}

			var notificationType = testForError ? CargoWise.EntityFramework.NotificationType.Error : CargoWise.EntityFramework.NotificationType.Warning;
			AssertEquals(false, SetValueAndGetNotifications(charge1, validValue).Contains(expectedErrorOrWarning));
			AssertEquals(shouldContainExpectedErrorOrWarning, SetValueAndGetNotifications(charge2, inValidValue).Contains(expectedErrorOrWarning));
			AssertEquals(false, SetValueAndGetNotifications(charge3, inValidValue).Contains(expectedErrorOrWarning));

			IEnumerable<INotification> SetValueAndGetNotifications(Charge charge, ZString value)
			{
				var propertyInfo = charge.FindPropertyInfo(propertyName);
				propertyInfo.Value = value;
				charge.Validation.ValidateAll();
				return propertyInfo.Notifications.Where(n => n.Type == notificationType);
			}
		}

		#endregion

		#region TaxMessageIsEmptyTest

		public void TestTaxMessageWarning()
		{
			AccountingMasterFilesRegistry.Instance.EnableNewTurkeyARComplianceFeaturesFrom.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, DateTime.Now.AddDays(-30));

			var nil = ZGuid.Empty;
			var msg = TestObjectCreator.CreateTaxMsg("MSG1", "MSG1", "english msg1", "local msg1").PK;

			void asserter()
			{
				//change country hits db so better do it in bulk
				foreach (var country in new[] { "PT", "AU", "US", "JP" })
				{
					using (GlbCompany.CurrentCompany.TemporarilySetCountry(country))
					{
						//when message is present, no notification
						RunTaxMessageIsEmptyTest("NOT", 0m, msg, false, NotificationTypes.None);
						RunTaxMessageIsEmptyTest("REQ", 0m, msg, false, NotificationTypes.None);
						RunTaxMessageIsEmptyTest("RTZ", 0m, msg, false, NotificationTypes.None);
						RunTaxMessageIsEmptyTest("RET", 0m, msg, false, NotificationTypes.None);
						RunTaxMessageIsEmptyTest("REZ", 7m, msg, false, NotificationTypes.None, 70m);
						RunTaxMessageIsEmptyTest("NOT", 7m, msg, true, NotificationTypes.None);
						RunTaxMessageIsEmptyTest("RTZ", 7m, msg, true, NotificationTypes.None);
						RunTaxMessageIsEmptyTest("REQ", 7m, msg, true, NotificationTypes.None);
						RunTaxMessageIsEmptyTest("RET", 7m, msg, true, NotificationTypes.None);
						RunTaxMessageIsEmptyTest("REZ", 0m, msg, true, NotificationTypes.None, 0m, true);

						////when taxID is blank, no notification
						RunTaxMessageIsEmptyTest("NOT", 7m, nil, true, NotificationTypes.None);
						RunTaxMessageIsEmptyTest("RTZ", 7m, nil, true, NotificationTypes.None);
						RunTaxMessageIsEmptyTest("REQ", 7m, nil, true, NotificationTypes.None);
						RunTaxMessageIsEmptyTest("RET", 7m, nil, true, NotificationTypes.None);
						RunTaxMessageIsEmptyTest("RET", 0m, nil, false, NotificationTypes.None);
						RunTaxMessageIsEmptyTest("REZ", 7m, nil, true, NotificationTypes.None);

						//Nothing when NOT
						RunTaxMessageIsEmptyTest("NOT", 0m, nil, false, NotificationTypes.None);
						RunTaxMessageIsEmptyTest("NOT", 7m, nil, false, NotificationTypes.None);

						//Nothing when tax is entered and RTZ
						RunTaxMessageIsEmptyTest("RTZ", 7m, nil, false, NotificationTypes.None);

						//Errors						
						RunTaxMessageIsEmptyTest("RTZ", 0m, nil, false, NotificationTypes.Error, 70m, false, true);
						RunTaxMessageIsEmptyTest("REQ", 0m, nil, false, NotificationTypes.Error);
						RunTaxMessageIsEmptyTest("RET", 7m, nil, false, NotificationTypes.Error);
						RunTaxMessageIsEmptyTest("REZ", 7m, nil, false, NotificationTypes.Error, 0m);
						RunTaxMessageIsEmptyTest("REZ", 7m, nil, false, NotificationTypes.Error, 70m);
					}
				}
			}

			CombineAssertions("", asserter);
		}

		void RunTaxMessageIsEmptyTest(string registry, ZDecimal taxAmount, ZGuid messagePK, bool taxIDEmpty, NotificationTypes notificationType, decimal amount = decimal.Zero, bool isExtraVAT = false, bool isGSTFree = false)
		{
			var notificationMessage = registry == Constants.TaxMessageMandatoryOptionConstants.RequiredAlways
				? "Tax Message is Required."
				: registry == Constants.TaxMessageMandatoryOptionConstants.RequiredWhenTaxIsZero
				? "Tax Amount is zero, please enter a Tax Message."
				: "Please enter a valid Tax Message when using this Tax ID.";

			var taxID = taxIDEmpty ? ZGuid.Empty : isGSTFree ? TestObjectCreator.GSTFREE1.PK : TestObjectCreator.GST1.PK;
			if (registry == Constants.TaxMessageMandatoryOptionConstants.RequiredWhenExtraTaxIsNotZero)
			{
				taxID = taxIDEmpty ? ZGuid.Empty : TestObjectCreator.KDV20W5.PK;
			}

			if (registry == Constants.TaxMessageMandatoryOptionConstants.RequiredWhenTaxHasExtraElementOrZeroTax)
			{
				if (!isExtraVAT)
				{
					taxID = taxIDEmpty ? ZGuid.Empty : TestObjectCreator.KDV20W5.PK;
				}
				else
				{
					taxID = taxIDEmpty ? ZGuid.Empty : TestObjectCreator.GSTFREE1.PK;
				}
			}

			var shipment = TestObjectCreator.CreateShipment("S0001");
			var job = TestObjectCreator.CreateJob(shipment, false);

			var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "Shipment Job Charge", TestObjectCreator.AUD, 0M, null, TestObjectCreator.AUD, 0M, null);
			charge.JR_OSSellAmt = taxAmount * 10;
			charge.JR_OSCostAmt = taxAmount * 10;
			if (amount != decimal.Zero)
			{
				charge.JR_OSSellAmt = amount;
				charge.JR_OSCostAmt = amount;
			}
			charge.JR_AT_SellGSTRate = taxID;
			charge.JR_AT_CostGSTRate = taxID;

			AssertEquals(taxID.IsEmpty ? 0 : taxAmount, charge.JR_OSSellGSTAmt_Calc);
			AssertEquals(taxID.IsEmpty ? 0 : taxAmount, charge.JR_OSCostGSTAmt_Calc);

			charge.JR_A9_CostVATClass = messagePK;
			charge.JR_A9_SellVATClass = messagePK;

			AccountingConfigurationRegistry.Instance.TaxMessageIsMandatoryPayables.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registry);
			AccountingConfigurationRegistry.Instance.TaxMessageIsMandatoryReceivables.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registry);

			charge.Validation.ValidateJR_A9_CostVATClass();
			charge.Validation.ValidateJR_A9_SellVATClass();

			string getFailMessage()
			{
				return Invariant($@"Country: {GlbCompany.CurrentCompany.Country.Code}
registrySetting: {registry}
taxAmount: {taxAmount}
mesageIsEmpty: {messagePK.IsEmpty}
taxIdIsEmpty: {taxIDEmpty}
costPosted: {charge.IsCostPosted}
sellPosted: {charge.IsRevenuePosted}");
			}

			void postSellAndRevalidate()
			{
				var line = Factory.NewWithValidTestData<ARInvoiceLine>();
				line.AL_LineType = TransactionLineTypes.Revenue;
				charge.JR_AL_ARLine = line.PK;
				Assert("revenue posted sanity check", charge.IsRevenuePosted);

				charge.Validation.ValidateJR_A9_CostVATClass();
				charge.Validation.ValidateJR_A9_SellVATClass();
			}

			void postCostAndRevalidate()
			{
				var line = Factory.NewWithValidTestData<APInvoiceLine>();
				line.AL_LineType = TransactionLineTypes.Cost;
				charge.JR_AL_APLine = line.PK;
				Assert("cost posted sanity check", charge.IsCostPosted);

				charge.Validation.ValidateJR_A9_CostVATClass();
				charge.Validation.ValidateJR_A9_SellVATClass();
			}

			switch (notificationType)
			{
				case NotificationTypes.Error:
					AssertHasError(getFailMessage(), charge.JR_A9_CostVATClassInfo, notificationMessage);
					AssertHasError(getFailMessage(), charge.JR_A9_SellVATClassInfo, notificationMessage);
					AssertNoWarnings(getFailMessage(), charge.JR_A9_CostVATClassInfo);
					AssertNoWarnings(getFailMessage(), charge.JR_A9_SellVATClassInfo);

					postSellAndRevalidate();
					AssertNoWarnings(getFailMessage(), charge.JR_A9_SellVATClassInfo);
					AssertNoErrors(getFailMessage(), charge.JR_A9_SellVATClassInfo);
					AssertHasError(getFailMessage(), charge.JR_A9_CostVATClassInfo, notificationMessage);

					postCostAndRevalidate();
					AssertNoWarnings(getFailMessage(), charge.JR_A9_CostVATClassInfo);
					AssertNoErrors(getFailMessage(), charge.JR_A9_CostVATClassInfo);

					break;
				case NotificationTypes.Warning:
					AssertHasWarning(getFailMessage(), charge.JR_A9_CostVATClassInfo, notificationMessage);
					AssertHasWarning(getFailMessage(), charge.JR_A9_SellVATClassInfo, notificationMessage);
					AssertNoErrors(getFailMessage(), charge.JR_A9_CostVATClassInfo);
					AssertNoErrors(getFailMessage(), charge.JR_A9_SellVATClassInfo);

					postSellAndRevalidate();
					AssertNoWarnings(getFailMessage(), charge.JR_A9_SellVATClassInfo);
					AssertNoErrors(getFailMessage(), charge.JR_A9_SellVATClassInfo);
					AssertHasWarning(getFailMessage(), charge.JR_A9_CostVATClassInfo, notificationMessage);

					postCostAndRevalidate();
					AssertNoErrors(getFailMessage(), charge.JR_A9_CostVATClassInfo);
					AssertNoWarnings(getFailMessage(), charge.JR_A9_CostVATClassInfo);

					break;
				case NotificationTypes.None:
					AssertNoWarnings(getFailMessage(), charge.JR_A9_CostVATClassInfo);
					AssertNoWarnings(getFailMessage(), charge.JR_A9_SellVATClassInfo);
					AssertNoErrors(getFailMessage(), charge.JR_A9_CostVATClassInfo);
					AssertNoErrors(getFailMessage(), charge.JR_A9_SellVATClassInfo);
					break;
			}
		}

		#endregion

		public void TestCheckJR_PaymentType_EPayment()
		{
			var expectedError = "To process E-Payments, please create a Payment or Payment Batch in the Payables Transactions module or a Payment Approval in the Payment Processing module.";

			var charge = Factory.NewWithValidTestData<Charge>();
			charge.JR_PaymentType = ReceiptTypes.Cheque;
			AssertNoErrorContaining(charge.JR_PaymentTypeInfo, expectedError);

			charge.JR_PaymentType = ReceiptTypes.EPayment;
			AssertHasError(charge.JR_PaymentTypeInfo, expectedError);
		}

		public void TestCheckJR_PaymentType_Cash()
		{
			var cashAccount = Factory.NewWithValidTestData<AccBankAccount>();
			cashAccount.AB_AccountType = ReceiptTypes.Cash;

			var charge = Factory.NewWithValidTestData<Charge>();
			charge.JR_AB = cashAccount.PK;

			var expectedError = $"For Cash Account, please select CSH - Cash {charge.JR_PaymentTypeInfo.HumanReadableName}.";
			AssertNoErrorContaining(charge.JR_PaymentTypeInfo, expectedError);

			charge.JR_PaymentType = ReceiptTypes.Cheque;
			AssertHasError(charge.JR_PaymentTypeInfo, expectedError);
		}

		public void TestInvoiceTargetNotValidatedOncePosted()
		{
			TestObjectCreator.SetupDebtorDefaultingRegistry(("ALL", "ALL", "ORG", "PPD", "SHP", "SGT", "PSA"));

			var creator = new TestObjectCreator(Factory);
			var sisterOrgProxy = creator.DebtorSisterOrgProxy;

			//			gC0001		gC0002		C0003		C0004
			//	AUBNE	-	AUSYD	-	SGSIN	-	HKHKG	-	USLAX
			//												 \
			//													C0005
			//														\
			//															USNYC
			//				|-	-	-	-	-	S0001	-	-	-|
			//	|-	-	-	-	-	-	-	S0002	-	-	-	-	-	-|
			//	|-	-	-	S0003	-	-|
			//
			var setup = creator.CreateGatewayConsolsAndShipments();
			var acc = creator.CreateChargeCode("GTB");
			acc.AC_ChargeGroup = ChargeCodeGroupList.Codes.Origin;
			Factory.Save();

			using (var consolJobHeader = creator.CreateJob(setup.gC0002))
			{
				var taxRate = creator.CreateTaxRate("TAX1", "desc", 5);

				var charge = consolJobHeader.Charges.AddNew();
				charge.JR_AC = acc.PK;
				charge.JR_OSSellAmt = 600m;
				charge.JR_RX_NKSellCurrency = "AUD";
				charge.JR_RX_NKCostCurrency = "AUD";
				charge.JR_OH_SellAccount = sisterOrgProxy.PK;
				charge.JR_SellRatingOverride = false;

				Assert(!charge.IsRevenuePosted);

				charge.JR_OH_SellAccount = sisterOrgProxy.PK;
				charge.JR_Calc_RelatedJobNumber = "S0003";

				Assert(!charge.JR_Calc_InvoiceTargetInfo.ReadOnly);
				Assert(!charge.JR_Calc_RelatedJobNumberInfo.ReadOnly);
				charge.JR_Calc_InvoiceTarget = "C0001";

				charge.Validation.ValidateJR_Calc_InvoiceTarget();
				charge.Validation.ValidateJR_Calc_RelatedJobNumber();
				Assert(!charge.JR_Calc_InvoiceTarget.IsEmpty);
				Assert(charge.Lookups.InvoiceTargetJobNumbers.ContainsCode("C0001"));
				Assert(charge.Lookups.RelatedJobNumbers.ContainsCode("S0003"));
				AssertNoNotifications(charge.JR_Calc_InvoiceTargetInfo);
				AssertNoNotifications(charge.JR_Calc_RelatedJobNumberInfo);

				var invoice = Factory.NewWithValidTestData<ARInvoice>();
				var transactionLine = (AccTransactionLines)invoice.Lines.AddNew();
				transactionLine.AL_JH = consolJobHeader.PK;
				transactionLine.AL_LineType = TransactionLineTypes.Revenue;
				transactionLine.AL_AT = charge.JR_AT_SellGSTRate;
				transactionLine.AL_LineAmount = 600;
				transactionLine.AL_OSAmount = 600;
				transactionLine.AL_RX_NKTransactionCurrency = "AUD";
				transactionLine.AL_AG = TestObjectCreator.GLHeader1.PK;
				charge.JR_AL_ARLine = transactionLine.PK;

				Factory.Save();
				charge.Validation.ValidateJR_Calc_InvoiceTarget();
				charge.Validation.ValidateJR_Calc_RelatedJobNumber();

				Assert(charge.IsRevenuePosted);
				Assert(charge.JR_Calc_InvoiceTargetInfo.ReadOnly);
				Assert(charge.JR_Calc_RelatedJobNumberInfo.ReadOnly);
				AssertEquals("C0001", charge.JR_Calc_InvoiceTarget);
				AssertEquals("S0003", charge.JR_Calc_RelatedJobNumber);
				Assert(!charge.Lookups.InvoiceTargetJobNumbers.ContainsCode("C0001"));
				Assert(charge.Lookups.RelatedJobNumbers.ContainsCode("S0003"));
				AssertNoNotifications(charge.JR_Calc_InvoiceTargetInfo);
				AssertNoNotifications(charge.JR_Calc_RelatedJobNumberInfo);

				setup.gC0002.Shipments.Remove(setup.s0003);
				Factory.Save();
				charge.Validation.ValidateJR_Calc_InvoiceTarget();
				charge.Validation.ValidateJR_Calc_RelatedJobNumber();

				Assert(charge.JR_Calc_InvoiceTargetInfo.ReadOnly);
				Assert(charge.JR_Calc_RelatedJobNumberInfo.ReadOnly);
				AssertEquals("C0001", charge.JR_Calc_InvoiceTarget);
				AssertEquals("S0003", charge.JR_Calc_RelatedJobNumber);
				Assert(!charge.Lookups.InvoiceTargetJobNumbers.ContainsCode("C0001"));
				Assert(!charge.Lookups.RelatedJobNumbers.ContainsCode("S0003"));
				AssertNoNotifications(charge.JR_Calc_InvoiceTargetInfo);
				AssertNoNotifications(charge.JR_Calc_RelatedJobNumberInfo);

				setup.gC0001.Shipments.Remove(setup.s0003);
				setup.s0003.Consols.RemoveAll();

				AssertExceptionThrown<CannotDeleteException>("As we save a link to shipment, it's good it's not possible to delete it as this would leave JR_Calc_RelatedJobNumber empty", setup.s0003.Delete);

				setup.gC0001.Shipments.RemoveAll();
				setup.gC0001.Delete();
				Factory.Save();
				charge.Validation.ValidateJR_Calc_InvoiceTarget();
				charge.Validation.ValidateJR_Calc_RelatedJobNumber();

				Assert(charge.JR_Calc_InvoiceTargetInfo.ReadOnly);
				Assert(charge.JR_Calc_RelatedJobNumberInfo.ReadOnly);
				AssertEquals("It's possible to delete Consol so all we can do is show empty invoice target", "", charge.JR_Calc_InvoiceTarget);
				AssertEquals("S0003", charge.JR_Calc_RelatedJobNumber);
				Assert(!charge.Lookups.InvoiceTargetJobNumbers.ContainsCode("C0001"));
				Assert(!charge.Lookups.RelatedJobNumbers.ContainsCode("S0003"));
				AssertNoNotifications(charge.JR_Calc_InvoiceTargetInfo);
				AssertNoNotifications(charge.JR_Calc_RelatedJobNumberInfo);
			}
		}

		public void TestInvoiceTargetJobNumber()
		{
			void assertErrors((string jobNumber, bool isValid)[] expected, BaseCharge charge)
			{
				CombineAssertions(() =>
				{
					foreach (var testPair in expected)
					{
						charge.JR_Calc_InvoiceTarget = testPair.jobNumber;
						var message = $"{charge.JR_Calc_RelatedJobNumber} - {testPair.jobNumber}";

						if (testPair.isValid)
						{
							AssertNoNotifications(message, charge.JR_Calc_InvoiceTargetInfo);
						}
						else
						{
							AssertHasError(message, charge.JR_Calc_InvoiceTargetInfo, "Enter a valid Intercompany Invoice Target Job.");
						}
					}
				});
			}

			var creator = new TestObjectCreator(Factory);
			var cSS = creator.CreateGatewayConsol("AUSYD", "SGSIN", "C00SS", receivingGatewayCompany: GlbCompany.CurrentCompany);
			var shipment1 = creator.CreateShipment("S0001", "AUSYD", "USLAX", cSS);
			var shipment2 = creator.CreateShipment("S0002", "AUBNE", "USNYC", cSS);
			var shipment3 = creator.CreateShipment("S0003", "AUBNE", "SGSIN", cSS);
			var shipment4 = creator.CreateShipment("S0004", "AUMEL", "USLAX");

			//			cBS			cSS			cSH			cHL		
			//	AUBNE	-	AUSYD	-	SGSIN	-	HKHKG	-	USLAX
			//												 \
			//													cHN
			//														\
			//															USNYC
			//				|-	-	-	-	Shipment1	-	-	-|
			//	|-	-	-	-	-	-	-	Shipment2	-	-	-	-	-|
			//	|-	-	Shipment3	-	-|
			//			
			ForwardingConsol cHL = creator.CreateConsol("HKHKG", "USLAX", "C00HL");
			ForwardingConsol cBS = creator.CreateConsol("AUBNE", "AUSYD", "C00BS");
			ForwardingConsol cSH = creator.CreateConsol("SGSIN", "HKHKG", "C00SH");
			ForwardingConsol cHN = creator.CreateConsol("HKHKG", "USNYC", "C00HN");

			shipment1.Consols.Add(cHL);
			shipment1.Consols.Add(cSH);
			shipment2.Consols.Add(cHN);
			shipment2.Consols.Add(cSH);
			shipment2.Consols.Add(cBS);
			shipment3.Consols.Add(cBS);

			Factory.Save();

			using (var gatewayJobHeader = creator.CreateJob(cSS))
			{
				var charge = gatewayJobHeader.Charges.AddNew();
				charge.JR_OH_SellAccount = creator.DebtorSisterOrgProxy.PK;

				charge.JR_Calc_RelatedJobNumber = "S0001";
				var expected = new[]
				{
					("S0001", true),
					("C00SS", true),
					("C00SH", true),
					("C00HL", true),
					("S0002", false),
					("C00BS", false),
					("C00HN", false),
					("S0003", false),
					("C00BS", false),
					("S0004", false),
					("S0005", false)
				};

				assertErrors(expected, charge);

				charge.JR_Calc_RelatedJobNumber = "S0002";
				expected = new[]
				{
					("S0002", true),
					("C00BS", true),
					("C00SS", true),
					("C00SH", true),
					("C00HN", true),
					("S0001", false),
					("C00HL", false),
					("S0003", false),
					("S0004", false),
					("S0005", false)
				};

				assertErrors(expected, charge);

				charge.JR_Calc_RelatedJobNumber = "S0003";
				expected = new[]
				{
					("S0003", true),
					("C00BS", true),
					("C00SS", true),
					("S0001", false),
					("C00SH", false),
					("C00HL", false),
					("S0002", false),
					("C00HN", false),
					("S0004", false),
					("S0005", false)
				};

				assertErrors(expected, charge);

				charge.JR_Calc_RelatedJobNumber = "S0004";
				expected = new[]
				{
					("S0001", false),
					("C00SS", false),
					("C00SH", false),
					("C00HL", false),
					("S0002", false),
					("C00BS", false),
					("C00HN", false),
					("S0003", false),
					("S0004", false),
					("S0005", false)
				};

				assertErrors(expected, charge);

				charge.JR_Calc_RelatedJobNumber = "S0005";
				expected = new[]
				{
					("S0001", false),
					("C00SS", false),
					("C00SH", false),
					("C00HL", false),
					("S0002", false),
					("C00BS", false),
					("C00HN", false),
					("S0003", false),
					("S0004", false),
					("S0005", false)
				};

				assertErrors(expected, charge);
			}
		}

		public void TestJR_ACValidationWhenRestrictedByJobStatus()
		{
			var shipment = TestObjectCreator.CreateShipment("S0001");
			var job = TestObjectCreator.CreateJob(shipment, false);
			var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "Shipment Job Charge", TestObjectCreator.AUD, 0M, null, TestObjectCreator.AUD, 0M, null);

			job.JH_Status = JobHeaderStatus.JobReadyForRevenuePosting.Code;
			TestObjectCreator.CC1.AC_ChargeType = Core.Constants.ChargeType.ManualJobAccrual;
			charge.Validation.ValidateJR_AC();
			AssertNoErrors("MJA allowed on Job Ready for Revenue Posting", charge.JR_ACInfo);

			TestObjectCreator.CC1.AC_ChargeType = Core.Constants.ChargeType.Revenue;
			charge.Validation.ValidateJR_AC();
			AssertHasError("Revenue not allowed when status is 'ready for revenue posting'", charge.JR_ACInfo, "Only charge codes of type 'MJA' can be used when the job status is 'Job Ready for Revenue Posting'");

			job.JH_Status = JobHeaderStatus.JobReadyForRevenueAndCostPosting.Code;
			TestObjectCreator.CC1.AC_ChargeType = Core.Constants.ChargeType.ManualJobAccrual;
			charge.Validation.ValidateJR_AC();
			AssertNoErrors("MJA allowed on Job Ready for Revenue Posting", charge.JR_ACInfo);

			TestObjectCreator.CC1.AC_ChargeType = Core.Constants.ChargeType.Revenue;
			charge.Validation.ValidateJR_AC();
			AssertHasError("Revenue not allowed when status is 'ready for revenue posting'", charge.JR_ACInfo, "Only charge codes of type 'MJA' can be used when the job status is 'Job Ready for Revenue and Cost Posting'");

			job.JH_Status = JobHeaderStatus.JobReadyForCostPosting.Code;
			TestObjectCreator.CC1.AC_ChargeType = Core.Constants.ChargeType.ManualJobAccrual;
			charge.Validation.ValidateJR_AC();
			AssertNoErrors("MJA allowed on Job Ready for Cost Posting", charge.JR_ACInfo);

			TestObjectCreator.CC1.AC_ChargeType = Core.Constants.ChargeType.Revenue;
			charge.Validation.ValidateJR_AC();
			AssertNoErrors("REV allowed on Job Ready for Cost Posting", charge.JR_ACInfo);

			TestObjectCreator.CC1.AC_ChargeType = Core.Constants.ChargeType.Margin;
			charge.Validation.ValidateJR_AC();
			AssertHasError("Margin not allowed when status is 'ready for revenue posting'", charge.JR_ACInfo, "Only charge codes of type 'MJA' and 'REV' can be used when the job status is 'Job Ready for Cost Posting'");

			Factory.Save();
			Assert("Charge should now be in database", charge.IsInDatabase);
			Assert("Charge Code field should now be readonly", charge.JR_ACInfo.ReadOnly);

			TestObjectCreator.CC1.AC_ChargeType = Core.Constants.ChargeType.Revenue;
			charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "Shipment Job Charge", TestObjectCreator.AUD, 0M, null, TestObjectCreator.AUD, 0M, null);
			charge.Validation.ValidateJR_AC();
			AssertNoErrors("REV allowed on Job Ready for Cost Posting, for charges not in database", charge.JR_ACInfo);

			TestObjectCreator.CC1.AC_ChargeType = Core.Constants.ChargeType.Revenue;
			charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "Shipment Job Charge", TestObjectCreator.AUD, 0M, null, TestObjectCreator.AUD, 0M, null);
			charge.JR_E6 = ZGuid.NewZGuid();
			charge.Validation.ValidateJR_AC();
			AssertNoErrors("REV allowed on Job Ready for Cost Posting, for charges not in database and when attached to consol cost", charge.JR_ACInfo);
		}

		[TestDate(2006, 11, 23, 9, 26, 47)]
		public void TestGetChargeForSameInvoiceWithUnequalColumn_DateTimeColumn()
		{
			Charge charge1 = Factory.New<Charge>();
			charge1.JR_APInvoiceNum = "1234";
			charge1.JR_OH_CostAccount = ZGuid.NewZGuid();
			charge1.JR_APInvoiceDate = ZDateTime.Now;
			charge1.JR_PaymentDate = ZDateTime.Now.AddMinutes(1);
			AssertEquals("HasErrors", false, charge1.JR_APInvoiceDateInfo.HasErrors());
			AssertEquals("HasErrors", false, charge1.JR_PaymentDateInfo.HasErrors());

			Charge charge2 = Factory.New<Charge>();
			charge2.JR_APInvoiceNum = charge1.JR_APInvoiceNum;
			charge2.JR_OH_CostAccount = charge1.JR_OH_CostAccount;
			charge2.JR_APInvoiceDate = ZDateTime.Now.AddDays(1);
			charge2.JR_PaymentDate = ZDateTime.Now.AddDays(1).AddMinutes(1);
			AssertEquals("HasErrors", true, charge2.JR_APInvoiceDateInfo.HasErrors());
			AssertEquals("HasErrors", true, charge2.JR_PaymentDateInfo.HasErrors());

			charge2.JR_APInvoiceDate = ZDateTime.Now.AddHours(2);
			charge2.JR_PaymentDate = ZDateTime.Now.AddHours(2).AddMinutes(1);
			AssertEquals("HasErrors", false, charge2.JR_APInvoiceDateInfo.HasErrors());
			AssertEquals("HasErrors", false, charge2.JR_PaymentDateInfo.HasErrors());

			charge2.JR_APInvoiceDate = ZDateTime.Empty;
			charge2.JR_PaymentDate = ZDateTime.Empty;
			AssertEquals("HasErrors", true, charge2.JR_APInvoiceDateInfo.HasErrors());
			AssertEquals("HasErrors", true, charge2.JR_PaymentDateInfo.HasErrors());
		}

		[TestDate(2006, 11, 23, 9, 26, 47)]
		public void TestGetChargeForSameInvoiceWithUnequalColumn_DateTimeColumnExcludesCommentCharge()
		{
			AccountingConfigurationRegistry.Instance.AllowForwardDatingofAPInvoiceDate.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

			Charge charge1 = Factory.New<Charge>();
			charge1.JR_APInvoiceNum = "1234";
			charge1.JR_OH_CostAccount = ZGuid.NewZGuid();
			charge1.JR_APInvoiceDate = ZDateTime.Now;
			charge1.JR_PaymentDate = ZDateTime.Now.AddMinutes(1);
			AssertEquals("HasErrors", false, charge1.JR_APInvoiceDateInfo.HasErrors());
			AssertEquals("HasErrors", false, charge1.JR_PaymentDateInfo.HasErrors());

			var commentChargeCode = Factory.New<AccChargeCode>();
			commentChargeCode.AC_ChargeType = Core.Constants.ChargeType.Comment;
			Charge charge2 = Factory.New<Charge>();
			charge2.JR_APInvoiceNum = charge1.JR_APInvoiceNum;
			charge2.JR_OH_CostAccount = charge1.JR_OH_CostAccount;
			charge2.JR_APInvoiceDate = ZDateTime.Now.AddDays(1);
			charge2.JR_PaymentDate = ZDateTime.Now.AddDays(1).AddMinutes(1);
			AssertEquals("HasErrors", true, charge2.JR_APInvoiceDateInfo.HasErrors());
			AssertEquals("HasErrors", true, charge2.JR_PaymentDateInfo.HasErrors());

			charge2.JR_AC = commentChargeCode.PK;
			charge2.JR_APInvoiceDate = charge2.JR_APInvoiceDate.AddDays(1);
			charge2.JR_PaymentDate = charge2.JR_PaymentDate.AddDays(1).AddMinutes(1);
			AssertEquals("HasErrors for comment charges", false, charge2.JR_APInvoiceDateInfo.HasErrors());
			AssertEquals("HasErrors for comment charges", false, charge2.JR_PaymentDateInfo.HasErrors());

			charge1.JR_APInvoiceDate = ZDateTime.Now;
			charge1.JR_PaymentDate = ZDateTime.Now.AddMinutes(1);
			AssertEquals("HasErrors", false, charge1.JR_APInvoiceDateInfo.HasErrors());
			AssertEquals("HasErrors", false, charge1.JR_PaymentDateInfo.HasErrors());
		}

		public void TestCreditorOnAccrualIsMandatory()
		{
			var shipment = TestObjectCreator.CreateShipment("S0001");
			var job = TestObjectCreator.CreateJob(shipment, false);
			var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "Shipment Job Charge", TestObjectCreator.AUD, 0M, null, TestObjectCreator.AUD, 0M, null);

			charge.Validation.ValidateJR_OH_CostAccount();
			AssertNoErrors(charge.JR_OH_CostAccountInfo);
			charge.JR_LocalCostAmt = 100m;
			charge.Validation.ValidateJR_OH_CostAccount();
			AssertNoErrors(charge.JR_OH_CostAccountInfo);

			AccountingConfigurationRegistry.Instance.AccrualMustHaveCreditorCode.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

			charge.Validation.ValidateJR_OH_CostAccount();
			AssertHasErrors(charge.JR_OH_CostAccountInfo);
			string expectedError = "You must enter a creditor. Your system has been configured so that the 'creditor' is mandatory when entering an unposted cost of non zero value.\r\n\r\n" +
							"The registry setting that governs this rule is Accounting > Job Costing > Accrual Must Have Creditor Code";
			AssertHasError(charge.JR_OH_CostAccountInfo, expectedError);
			charge.JR_LocalCostAmt = 0m;
			charge.Validation.ValidateJR_OH_CostAccount();
			AssertNoErrors(charge.JR_OH_CostAccountInfo);

			charge.JR_LocalCostAmt = 100m;
			var oldValue = Globals.IsWeb;
			Globals.IsWeb = true;
			charge.Validation.ValidateJR_OH_CostAccount();
			AssertNoErrors(charge.JR_OH_CostAccountInfo);
			Globals.IsWeb = oldValue;
		}

		public void TestCreditorOnAccrualIsNotMandatoryForSpotQuotes()
		{
			AccountingConfigurationRegistry.Instance.AccrualMustHaveCreditorCode.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

			var shipment = TestObjectCreator.CreateShipment("S0001");
			var jobForShipment = TestObjectCreator.CreateJob(shipment, false);
			var chargeForShipment = TestObjectCreator.CreateCharge(jobForShipment, TestObjectCreator.CC1, "Shipment Job Charge", TestObjectCreator.AUD, 10M, null, TestObjectCreator.AUD, 0M, TestObjectCreator.ABIGAS);

			AssertEquals(true, jobForShipment.ConsumerTypeShouldCreateAccrual(chargeForShipment.JR_InvoiceType));
			chargeForShipment.Validation.ValidateJR_OH_CostAccount();
			AssertHasErrors(chargeForShipment.JR_OH_CostAccountInfo);
			var expectedError = "You must enter a creditor. Your system has been configured so that the 'creditor' is mandatory when entering an unposted cost of non zero value.\r\n\r\n" +
							"The registry setting that governs this rule is Accounting > Job Costing > Accrual Must Have Creditor Code";
			AssertHasError(chargeForShipment.JR_OH_CostAccountInfo, expectedError);

			var spotQuote = QuotedBooking.New(Freight.Integration.QuoteBookingType.SpotQuote, Factory);
			var jobForSpotQuote = TestObjectCreator.CreateJob(spotQuote, false);
			var chargeForSpotQuote = TestObjectCreator.CreateCharge(jobForSpotQuote, TestObjectCreator.CC1, "Spot Quote Job Charge", TestObjectCreator.AUD, 10M, null, TestObjectCreator.AUD, 0M, TestObjectCreator.ABIGAS);

			AssertEquals(false, jobForSpotQuote.ConsumerTypeShouldCreateAccrual(chargeForSpotQuote.JR_InvoiceType));
			chargeForSpotQuote.Validation.ValidateJR_OH_CostAccount();
			AssertNoErrors(chargeForSpotQuote.JR_OH_CostAccountInfo);
		}

		public void TestCreditorValidationOnBookings()
		{
			var expectedError = "You must enter a creditor. Your system has been configured so that the 'creditor' is mandatory when entering an unposted cost of non zero value.\r\n\r\n" +
								"The registry setting that governs this rule is Accounting > Job Costing > Accrual Must Have Creditor Code";
			var quotedBooking = QuotedBooking.New(Freight.Integration.QuoteBookingType.BookingWithQuote, Factory);
			var jobForQuotedBooking = TestObjectCreator.CreateJob(quotedBooking, false);
			var chargeForQuotedBooking = TestObjectCreator.CreateCharge(jobForQuotedBooking, TestObjectCreator.CC1, "Quoted Booking Job Charge", TestObjectCreator.AUD, 10M, null, TestObjectCreator.AUD, 0M, TestObjectCreator.ABIGAS);

			var quickBooking = QuotedBooking.New(Freight.Integration.QuoteBookingType.QuickBooking, Factory);
			var jobForQuickBooking = TestObjectCreator.CreateJob(quickBooking, false);
			var chargeForQuickBooking = TestObjectCreator.CreateCharge(jobForQuickBooking, TestObjectCreator.CC1, "Quick Booking Job Charge", TestObjectCreator.AUD, 10M, null, TestObjectCreator.AUD, 0M, TestObjectCreator.ABIGAS);
			chargeForQuickBooking.JR_OH_CostAccount = ZGuid.Empty;

			using (AccountingConfigurationRegistry.Instance.AccrualMustHaveCreditorCode.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			{
				chargeForQuotedBooking.Validation.ValidateJR_OH_CostAccount();
				AssertNoErrors(chargeForQuotedBooking.JR_OH_CostAccountInfo);

				chargeForQuickBooking.Validation.ValidateJR_OH_CostAccount();
				AssertNoErrors(chargeForQuickBooking.JR_OH_CostAccountInfo);
			}

			using (AccountingConfigurationRegistry.Instance.AccrualMustHaveCreditorCode.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				chargeForQuotedBooking.Validation.ValidateJR_OH_CostAccount();
				AssertNoErrors(chargeForQuotedBooking.JR_OH_CostAccountInfo);

				chargeForQuickBooking.Validation.ValidateJR_OH_CostAccount();
				AssertHasError(chargeForQuickBooking.JR_OH_CostAccountInfo, expectedError);
			}
		}

		public void TestRelatedJobNumber()
		{
			var creator = new TestObjectCreator(Factory);
			var gatewayConsol = creator.CreateGatewayConsol("AUSYD", "SGSIN", "C0001", receivingGatewayCompany: GlbCompany.CurrentCompany);

			using (var consolJobHeader = creator.CreateJob(gatewayConsol))
			{
				creator.CreateShipment("S0001", "AUMEL", "USLAX");
				creator.CreateShipment("S0002", "AUSYD", "USLAX", gatewayConsol);
				creator.CreateShipment("S0003", "AUBNE", "USNYC", gatewayConsol);
				Factory.Save();
				creator.CreateShipment("S0004", "AUSYD", "SGSIN", gatewayConsol);

				var charge = creator.CreateCharge(consolJobHeader, creator.CreateChargeCode("AAA"), 100, 100);
				charge.Validation.ValidateAll();
				AssertNoErrors(charge.JR_Calc_RelatedJobNumberInfo);

				charge.JR_Calc_RelatedJobNumber = "S0001";
				charge.Validation.ValidateAll();
				AssertHasErrors(charge.JR_Calc_RelatedJobNumberInfo);

				charge.JR_Calc_RelatedJobNumber = "S0002";
				charge.Validation.ValidateAll();
				AssertNoErrors(charge.JR_Calc_RelatedJobNumberInfo);

				charge.JR_Calc_RelatedJobNumber = "S0004";
				charge.Validation.ValidateAll();
				AssertHasErrors(charge.JR_Calc_RelatedJobNumberInfo);

				charge.JR_Calc_RelatedJobNumber = "S0003";
				charge.Validation.ValidateAll();
				AssertNoErrors(charge.JR_Calc_RelatedJobNumberInfo);

				Factory.Save();
				charge.JR_Calc_RelatedJobNumber = "S0004";
				charge.Validation.ValidateAll();
				AssertNoErrors(charge.JR_Calc_RelatedJobNumberInfo);
			}
		}

		public void TestDebtorOnWIPIsMandatory()
		{
			var shipment = TestObjectCreator.CreateShipment("S0001");
			var job = TestObjectCreator.CreateJob(shipment, false);
			var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "Shipment Job Charge", TestObjectCreator.AUD, 0M, null, TestObjectCreator.AUD, 0M, null);

			charge.Validation.ValidateJR_OH_SellAccount();
			AssertNoErrors(charge.JR_OH_SellAccountInfo);
			charge.JR_LocalSellAmt = 100m;
			charge.Validation.ValidateJR_OH_SellAccount();
			AssertNoErrors(charge.JR_OH_SellAccountInfo);

			AccountingConfigurationRegistry.Instance.WIPMustHaveDebtorCode.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

			charge.Validation.ValidateJR_OH_SellAccount();
			AssertHasErrors(charge.JR_OH_SellAccountInfo);
			string expectedError = @"You must enter a debtor. Your system has been configured so that the 'debtor' is mandatory when entering an unposted sell of non zero value.

The registry setting that governs this rule is Accounting > Job Costing > WIP Must Have Debtor Code";

			AssertHasError(charge.JR_OH_SellAccountInfo, expectedError);
			charge.JR_LocalSellAmt = 0m;
			charge.Validation.ValidateJR_OH_SellAccount();
			AssertNoErrors(charge.JR_OH_SellAccountInfo);

			charge.JR_LocalSellAmt = 10m;
			charge.Validation.ValidateJR_OH_SellAccount();
			AssertHasError(charge.JR_OH_SellAccountInfo, expectedError);

			charge.JR_AL_ARLine = Factory.New<ARInvoiceLine>().PK;
			AssertEquals("Precondition: charge.IsRevenuePosted", true, charge.IsRevenuePosted);
			charge.Validation.ValidateJR_OH_SellAccount();
			AssertNoErrors(charge.JR_OH_SellAccountInfo);
		}

		public void TestInternalFields_CasesForIsExcludedFromAutoJRJ()
		{
			AutoJRJRegistryStatusHelper.SetAutoJRJWithTaxRegistrationNumberEnabled_ForTestOnly();

			var currentCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			var registeredNumberCodeType = OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber;
			AssertEquals("PreCondition", CountryCodes.Australia, currentCountryCode);

			const string registeredNumberGroup1 = "21 003 980 130";
			const string registeredNumberGroup2 = "21 003 980 130 123";

			var branchForCharge = GlbBranch.CurrentBranch;
			branchForCharge.GB_OH_OrgProxy = TestObjectCreator.ABIGAS.PK;
			TestObjectCreator.SetCustomsCodeForOrgHeader(TestObjectCreator.ABIGAS, registeredNumberCodeType, currentCountryCode, registeredNumberGroup1);

			var branchForDifferentRegisteredNumber = TestObjectCreator.NonCurrentBranch;
			branchForDifferentRegisteredNumber.GB_OH_OrgProxy = TestObjectCreator.AALSHI.PK;
			TestObjectCreator.SetCustomsCodeForOrgHeader(TestObjectCreator.AALSHI, registeredNumberCodeType, currentCountryCode, registeredNumberGroup2);

			var shipment = TestObjectCreator.CreateShipment("S00001", "AUBNE", "JPTYO");
			var job = TestObjectCreator.CreateJob(shipment);
			job.PlugInData = shipment;

			GlbBranch.CurrentBranch.Factory.Save();
			Factory.Save();

			var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.FRT, "Charge 1");
			charge.JR_GB = branchForCharge.PK;
			charge.JR_GE = TestObjectCreator.FEADepartment.PK;

			AssertEquals("Precondition", branchForCharge.PK, charge.JR_GB);

			AssertInternalFieldsAreEmpty_NotExcludedFromAutoJRJ();
			AssertInternalFieldsAreEmpty_IsExcludedFromAutoJRJ();
			AssertInternalFieldsHaveValues_NotExcludedFromAutoJRJ();
			AssertInternalFieldsHaveValues_IsExcludedFromAutoJRJ();

			void AssertInternalFieldsAreEmpty_NotExcludedFromAutoJRJ()
			{
				charge.JR_OH_CostAccount = branchForCharge.GB_OH_OrgProxy;
				using (charge.SetDefaultValuesForAutoJobRevenueJournalsSuspender.GetSuspender())
				{
					charge.JR_JH_InternalJob = ZGuid.Empty;
					charge.JR_GB_InternalBranch = ZGuid.Empty;
					charge.JR_GE_InternalDept = ZGuid.Empty;
				}
				AssertEquals("Precondition", false, charge.IsInternalJobInfoDisabled);
				AssertEquals("Precondition", false, charge.IsExcludedFromAutoJRJ(charge.CostAccount));

				CombineAssertions("Case For internal fields are empty and charge AutoJRJ.", () => {
					charge.Validation.ValidateJR_JH_InternalJob();
					AssertNoErrors(charge.JR_JH_InternalJobInfo);
					AssertNoWarnings(charge.JR_JH_InternalJobInfo);

					charge.Validation.ValidateJR_GB_InternalBranch();
					AssertNoErrors(charge.JR_GB_InternalBranchInfo);
					AssertHasWarning(charge.JR_GB_InternalBranchInfo, "Job Revenue Journal is posted upon Save only when Internal Branch is specified.");

					charge.Validation.ValidateJR_GE_InternalDept();
					AssertNoErrors(charge.JR_GE_InternalDeptInfo);
					AssertHasWarning(charge.JR_GE_InternalDeptInfo, "Job Revenue Journal is posted upon Save only when Internal Department is specified.");
				});
			}

			void AssertInternalFieldsAreEmpty_IsExcludedFromAutoJRJ()
			{
				charge.JR_OH_CostAccount = branchForDifferentRegisteredNumber.GB_OH_OrgProxy;
				using (charge.SetDefaultValuesForAutoJobRevenueJournalsSuspender.GetSuspender())
				{
					charge.JR_JH_InternalJob = ZGuid.Empty;
					charge.JR_GB_InternalBranch = ZGuid.Empty;
					charge.JR_GE_InternalDept = ZGuid.Empty;
				}
				AssertEquals("Precondition", true, charge.IsInternalJobInfoDisabled);
				AssertEquals("Precondition", true, charge.IsExcludedFromAutoJRJ(charge.CostAccount));

				CombineAssertions("Case for internal fields are empty and charge is excluded from AutoJRJ.", () => {
					charge.Validation.ValidateJR_JH_InternalJob();
					AssertNoErrors(charge.JR_JH_InternalJobInfo);
					AssertNoWarnings(charge.JR_JH_InternalJobInfo);

					charge.Validation.ValidateJR_GB_InternalBranch();
					AssertNoErrors(charge.JR_GB_InternalBranchInfo);
					AssertNoWarnings(charge.JR_GB_InternalBranchInfo);

					charge.Validation.ValidateJR_GE_InternalDept();
					AssertNoErrors(charge.JR_GE_InternalDeptInfo);
					AssertNoWarnings(charge.JR_GE_InternalDeptInfo);
				});
			}

			void AssertInternalFieldsHaveValues_NotExcludedFromAutoJRJ()
			{
				charge.JR_OH_CostAccount = branchForCharge.GB_OH_OrgProxy;
				using (charge.SetDefaultValuesForAutoJobRevenueJournalsSuspender.GetSuspender())
				{
					charge.JR_JH_InternalJob = job.PK;
					charge.JR_GB_InternalBranch = branchForCharge.PK;
					charge.JR_GE_InternalDept = charge.JR_GE;
				}
				AssertEquals("Precondition", false, charge.IsInternalJobInfoDisabled);
				AssertEquals("Precondition", false, charge.IsExcludedFromAutoJRJ(charge.CostAccount));

				CombineAssertions("Case for internal fields are not empty and charge is AutoJRJ.", () => {
					charge.Validation.ValidateJR_JH_InternalJob();
					AssertNoErrors(charge.JR_JH_InternalJobInfo);
					AssertHasWarning(charge.JR_JH_InternalJobInfo, "Job Revenue Journal is posted upon Save only when Internal Job/-Branch/-Department is different to Charge Job/-Branch/-Department.");

					charge.Validation.ValidateJR_GB_InternalBranch();
					AssertNoErrors(charge.JR_GB_InternalBranchInfo);
					AssertNoWarnings(charge.JR_GB_InternalBranchInfo);

					charge.Validation.ValidateJR_GE_InternalDept();
					AssertNoErrors(charge.JR_GE_InternalDeptInfo);
					AssertNoWarnings(charge.JR_GE_InternalDeptInfo);
				});
			}

			void AssertInternalFieldsHaveValues_IsExcludedFromAutoJRJ()
			{
				charge.JR_OH_CostAccount = branchForDifferentRegisteredNumber.GB_OH_OrgProxy;
				using (charge.SetDefaultValuesForAutoJobRevenueJournalsSuspender.GetSuspender())
				{
					charge.JR_JH_InternalJob = job.PK;
					charge.JR_GB_InternalBranch = branchForDifferentRegisteredNumber.PK;
					charge.JR_GE_InternalDept = charge.JR_GE;
				}
				AssertEquals("Precondition", true, charge.IsInternalJobInfoDisabled);
				AssertEquals("Precondition", true, charge.IsExcludedFromAutoJRJ(charge.CostAccount));

				CombineAssertions("Case for internal fields are not empty and charge is excluded from AutoJRJ.", () => {
					charge.Validation.ValidateJR_JH_InternalJob();
					AssertHasError(charge.JR_JH_InternalJobInfo, "You cannot set the internal job when the Cost or Sell charge is not eligible to post auto Job Revenue Journal.");
					AssertNoWarnings(charge.JR_JH_InternalJobInfo);

					charge.Validation.ValidateJR_GB_InternalBranch();
					AssertHasError(charge.JR_GB_InternalBranchInfo, "You cannot set the internal branch when the Cost or Sell charge is not eligible to post auto Job Revenue Journal.");
					AssertNoWarnings(charge.JR_GB_InternalBranchInfo);

					charge.Validation.ValidateJR_GE_InternalDept();
					AssertHasError(charge.JR_GE_InternalDeptInfo, "You cannot set the internal department when the Cost or Sell charge is not eligible to post auto Job Revenue Journal.");
					AssertNoWarnings(charge.JR_GE_InternalDeptInfo);
				});
			}
		}

		public void TestInternalFields_EmptyWhenTaxRegistrationNumbersAreTheSame()
		{
			TestObjectCreator.PrepareAutoJRJTestEnvironment(true);
			var shipment = TestObjectCreator.CreateShipment("S00000001");
			var job = TestObjectCreator.CreateJob(shipment);
			Factory.Save();

			var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, string.Empty, TestObjectCreator.AUD, 190m, TestObjectCreator.AutoJRJCreditorOrDebtor, TestObjectCreator.AUD, 0m, null);
			AssertEquals("Precondition", GlbBranch.CurrentBranch.PK, charge.JR_GB);

			charge.JR_JH_InternalJob = ZGuid.Empty;
			charge.JR_GB_InternalBranch = ZGuid.Empty;
			charge.JR_GE_InternalDept = ZGuid.Empty;

			AssertEquals("Precondition", false, charge.IsExcludedFromAutoJRJ(charge.CostAccount));

			charge.Validation.ValidateJR_JH_InternalJob();
			charge.Validation.ValidateJR_GB_InternalBranch();
			charge.Validation.ValidateJR_GE_InternalDept();

			AssertNoErrors(charge.JR_JH_InternalJobInfo);
			AssertNoErrors(charge.JR_GB_InternalBranchInfo);
			AssertNoErrors(charge.JR_GE_InternalDeptInfo);
			AssertNoWarnings(charge.JR_JH_InternalJobInfo);
			AssertHasWarning(charge.JR_GB_InternalBranchInfo, "Job Revenue Journal is posted upon Save only when Internal Branch is specified.");
			AssertHasWarning(charge.JR_GE_InternalDeptInfo, "Job Revenue Journal is posted upon Save only when Internal Department is specified.");
		}

		public void TestInternalFields_EmptyWhenTaxRegistrationNumbersAreDifferent()
		{
			TestObjectCreator.PrepareAutoJRJTestEnvironment(false);
			var shipment = TestObjectCreator.CreateShipment("S00000001");
			var job = TestObjectCreator.CreateJob(shipment);
			Factory.Save();

			var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, string.Empty, TestObjectCreator.AUD, 190m, TestObjectCreator.AutoJRJCreditorOrDebtor, TestObjectCreator.AUD, 0m, null);
			AssertEquals("Precondition", GlbBranch.CurrentBranch.PK, charge.JR_GB);

			charge.JR_JH_InternalJob = ZGuid.Empty;
			charge.JR_GB_InternalBranch = ZGuid.Empty;
			charge.JR_GE_InternalDept = ZGuid.Empty;

			AssertEquals("Precondition", true, charge.IsExcludedFromAutoJRJ(charge.CostAccount));

			charge.Validation.ValidateJR_JH_InternalJob();
			charge.Validation.ValidateJR_GB_InternalBranch();
			charge.Validation.ValidateJR_GE_InternalDept();

			AssertNoErrors(charge.JR_JH_InternalJobInfo);
			AssertNoErrors(charge.JR_GB_InternalBranchInfo);
			AssertNoErrors(charge.JR_GE_InternalDeptInfo);
			AssertNoWarnings(charge.JR_JH_InternalJobInfo);
			AssertNoWarnings(charge.JR_GB_InternalBranchInfo);
			AssertNoWarnings(charge.JR_GE_InternalDeptInfo);
		}

		public void TestInternalFields_NonEmptyWhenTaxRegistrationNumbersAreDifferent()
		{
			TestObjectCreator.PrepareAutoJRJTestEnvironment(false);
			var shipment = TestObjectCreator.CreateShipment("S00000001");
			var job = TestObjectCreator.CreateJob(shipment);
			Factory.Save();

			var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, string.Empty, TestObjectCreator.AUD, 190m, TestObjectCreator.AutoJRJCreditorOrDebtor, TestObjectCreator.AUD, 0m, null);
			AssertEquals("Precondition", GlbBranch.CurrentBranch.PK, charge.JR_GB);

			using (AutoJRJRegistryStatusHelper.SetAutoJRJDisabled_ForTestOnly())
			{
				charge.JR_JH_InternalJob = job.PK;
				charge.JR_GB_InternalBranch = GlbBranch.CurrentBranch.PK;
				charge.JR_GE_InternalDept = GlbDepartment.CurrentDepartment.PK;
			}

			AssertEquals("Precondition", true, charge.IsExcludedFromAutoJRJ(charge.CostAccount));

			charge.Validation.ValidateJR_JH_InternalJob();
			charge.Validation.ValidateJR_GB_InternalBranch();
			charge.Validation.ValidateJR_GE_InternalDept();

			AssertHasError(charge.JR_JH_InternalJobInfo, "You cannot set the internal job when the Cost or Sell charge is not eligible to post auto Job Revenue Journal.");
			AssertHasError(charge.JR_GB_InternalBranchInfo, "You cannot set the internal branch when the Cost or Sell charge is not eligible to post auto Job Revenue Journal.");
			AssertHasError(charge.JR_GE_InternalDeptInfo, "You cannot set the internal department when the Cost or Sell charge is not eligible to post auto Job Revenue Journal.");
			AssertNoWarnings(charge.JR_JH_InternalJobInfo);
			AssertNoWarnings(charge.JR_GB_InternalBranchInfo);
			AssertNoWarnings(charge.JR_GE_InternalDeptInfo);
		}

		public void TestCheckJR_A9_SellVATClass_RTZ_SellAmountIsZero()
		{
			AccountingConfigurationRegistry.Instance.TaxMessageIsMandatoryReceivables.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Constants.TaxMessageMandatoryOptionConstants.RequiredWhenTaxIsZero);

			var consol = TestObjectCreator.CreateConsol("AUSYD", "NZAKL", "C0001");
			var shipment1 = TestObjectCreator.CreateShipment("S00001004", "AUSYD", "NZAKL", consol);
			var job1 = TestObjectCreator.CreateJob(shipment1, false);
			job1.JH_OA_AgentCollectAddr = TestObjectCreator.Debtor.MainAddress.PK;
			var shipment2 = TestObjectCreator.CreateShipment("S00001005", "AUSYD", "NZAKL", consol);
			var job2 = TestObjectCreator.CreateJob(shipment2, false);
			job2.JH_OA_AgentCollectAddr = TestObjectCreator.Debtor.MainAddress.PK;
			Factory.Save();

			var listing = consol.GetApportionments();
			var consolCost = listing.CostsCollection.TryAddNew();

			Assert("Pre-condition: JR_OSSellAmt is 0", consolCost.ApportionmentCharges.Cast<BaseCharge>().All(x => x.JR_OSSellAmt == 0));

			consolCost.E6_AC_ChargeCode = TestObjectCreator.FRT.PK;

			Assert(!consolCost.ApportionmentCharges.Cast<BaseCharge>().All(x => x.JR_A9_SellVATClassInfo.HasError("Tax Amount is zero, please enter a Tax Message.")));

			consol.RunPreSaveValidation();
			Assert(!consolCost.ApportionmentCharges.Cast<BaseCharge>().All(x => x.JR_A9_SellVATClassInfo.HasError("Tax Amount is zero, please enter a Tax Message.")));
		}

		public void TestJR_JH_InternalJobValidation_NotAllChargeHasJRJ_EnableAutoJobRevenueJournals()
		{
			const string expectedError = "Please ensure the Internal Job/Branch/Department is different to Charge Job/Branch/Department.";

			AutoJRJRegistryStatusHelper.SetAutoJRJWithTaxRegistrationNumberEnabled_ForTestOnly(GlbCompany.CurrentCompany.PK.ToGuid());

			var currentCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			var registeredNumberCodeType = OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber;
			AssertEquals("PreCondition", CountryCodes.Australia, currentCountryCode);

			const string registeredNumberGroup1 = "21 003 980 130";
			const string registeredNumberGroup2 = "21 003 980 130 123";

			var branchForRegisteredNumberGroup1 = TestObjectCreator.CreateBranch("ABC", GlbCompany.CurrentCompany);
			branchForRegisteredNumberGroup1.GB_OH_OrgProxy = TestObjectCreator.ABIGAS.PK;
			TestObjectCreator.SetCustomsCodeForOrgHeader(TestObjectCreator.ABIGAS, registeredNumberCodeType, currentCountryCode, registeredNumberGroup1);

			var branchForRegisteredNumberGroup2 = TestObjectCreator.CreateBranch("DEF", GlbCompany.CurrentCompany);
			branchForRegisteredNumberGroup2.GB_OH_OrgProxy = TestObjectCreator.AALSHI.PK;
			TestObjectCreator.SetCustomsCodeForOrgHeader(TestObjectCreator.AALSHI, registeredNumberCodeType, currentCountryCode, registeredNumberGroup2);

			var consol = Factory.New<ForwardingConsol>();
			var shipment1 = consol.Shipments.AddNew();
			var shipment2 = consol.Shipments.AddNew();
			Factory.Save();

			var apps = new ApportionmentListing(Factory, consol);
			var consolCost = apps.CostsCollection.TryAddNew();
			consolCost.E6_AC_ChargeCode = TestObjectCreator.DSBChargeCode.PK;
			consolCost.E6_OSCostAmount = 5163.96;
			consolCost.E6_OH_Creditor = branchForRegisteredNumberGroup1.OrgProxy.PK;

			AssertEquals("PreCondition", consolCost.ApportionmentCharges.Count, 2);
			var splitCharge1 = consolCost.ApportionmentCharges[0];
			splitCharge1.JR_GE = TestObjectCreator.FEADepartment.PK;
			var splitCharge2 = consolCost.ApportionmentCharges[1];

			AssertEquals("Precondition, all charges are consol costing apportionment charge.", true, consolCost.ApportionmentCharges.Cast<ApportionSplitCharge>().All(x => !x.IsGatewaySellApportionmentCharge));
			try
			{
				AssertWhenAllChargeAreEligible_ErrorForNonJRJ();
				AssertWhenAllChargeAreEligible_NoErrorSinceAllChargeAreNonJRJ();
				AssertDifferentTaxRegistrationNumber();
				AssertDifferentTaxRegistrationNumber_DisableAJLForBranchWithSameTaxRegistration();
			}
			finally
			{
				apps.ReleaseMutexes();
			}

			void AssertWhenAllChargeAreEligible_ErrorForNonJRJ()
			{
				splitCharge1.JR_GB = branchForRegisteredNumberGroup1.PK;
				splitCharge2.JR_GB = branchForRegisteredNumberGroup1.PK;

				splitCharge1.JR_JH_InternalJob = splitCharge1.JR_JH;
				splitCharge1.JR_GB_InternalBranch = splitCharge1.JR_GB;
				splitCharge1.JR_GE_InternalDept = TestObjectCreator.FISDepartment.PK;
				AssertEquals("PreCondition, it is AutoJRJ charge since internal department is different from charge's department.", true, splitCharge1.ShouldCreateCostJRJ);

				AssertEquals("PreCondition, we have no other apporitonment charge in use.", true, consolCost.ApportionmentCharges.Cast<ApportionSplitCharge>().All(x => x.JR_IsUsedForApportionment));
				AssertEquals("PreCondition, all charges are eligible.", true, consolCost.ApportionmentCharges.Cast<ApportionSplitCharge>().All(x => !x.IsInternalJobInfoDisabled));
				CombineAssertions("Do validation for all charges are eligible. It should be error for NonJRJ charge since have one charge need to create AutoJRJ.", () => {
					splitCharge2.JR_JH_InternalJob = splitCharge2.JR_JH;
					splitCharge2.JR_GB_InternalBranch = splitCharge2.JR_GB;
					splitCharge2.JR_GE_InternalDept = ZGuid.Empty;
					AssertEquals("It is nonJRJ charge since missing one of JRJ criterias.", false, splitCharge2.ShouldCreateCostJRJ);
					splitCharge1.Validation.ValidateJR_JH_InternalJob();
					AssertNoError(splitCharge1.JR_JH_InternalJobInfo, expectedError);

					splitCharge1.JR_IsUsedForApportionment = false;
					splitCharge2.JR_IsUsedForApportionment = true;
					splitCharge2.Validation.ValidateJR_JH_InternalJob();
					AssertNoError("Should not have error, since we only consider apportionment charges in use.", splitCharge2.JR_JH_InternalJobInfo, expectedError);

					splitCharge1.JR_IsUsedForApportionment = true;
					splitCharge2.JR_IsUsedForApportionment = false;
					splitCharge2.Validation.ValidateJR_JH_InternalJob();
					AssertNoError("Should not have error, since charge is not used.", splitCharge2.JR_JH_InternalJobInfo, expectedError);

					splitCharge1.JR_IsUsedForApportionment = true;
					splitCharge2.JR_IsUsedForApportionment = true;
					splitCharge2.Validation.ValidateJR_JH_InternalJob();
					AssertHasError("Should have error, since another charge need to do JRJ but current charge is not AutoJRJ eligible.", splitCharge2.JR_JH_InternalJobInfo, expectedError);

					splitCharge2.JR_JH_InternalJob = splitCharge2.JR_JH;
					splitCharge2.JR_GB_InternalBranch = splitCharge2.JR_GB;
					splitCharge2.JR_GE_InternalDept = splitCharge2.JR_GE;
					AssertEquals("It is nonJRJ charge since JRJ criterias are matched with charge's value.", false, splitCharge2.ShouldCreateCostJRJ);
					splitCharge1.Validation.ValidateJR_JH_InternalJob();
					AssertNoError(splitCharge1.JR_JH_InternalJobInfo, expectedError);

					splitCharge1.JR_IsUsedForApportionment = false;
					splitCharge2.Validation.ValidateJR_JH_InternalJob();
					AssertNoError("Should not have error, since we only consider apportionment charges in use.", splitCharge2.JR_JH_InternalJobInfo, expectedError);

					splitCharge1.JR_IsUsedForApportionment = true;
					splitCharge2.Validation.ValidateJR_JH_InternalJob();
					AssertHasError("Should have error, since another charge need to do JRJ but current charge is not AutoJRJ eligible", splitCharge2.JR_JH_InternalJobInfo, expectedError);
				});
			}

			void AssertWhenAllChargeAreEligible_NoErrorSinceAllChargeAreNonJRJ()
			{
				splitCharge1.JR_GB = branchForRegisteredNumberGroup1.PK;
				splitCharge2.JR_GB = branchForRegisteredNumberGroup1.PK;

				splitCharge1.JR_JH_InternalJob = splitCharge1.JR_JH;
				splitCharge1.JR_GB_InternalBranch = splitCharge1.JR_GB;
				splitCharge1.JR_GE_InternalDept = TestObjectCreator.FEADepartment.PK;
				AssertEquals("PreCondition, it is nonJRJ charge since JRJ criterias are matched with charge's value.", false, splitCharge1.ShouldCreateCostJRJ);

				AssertEquals("PreCondition", true, consolCost.ApportionmentCharges.Cast<ApportionSplitCharge>().All(x => x.JR_IsUsedForApportionment));
				AssertEquals("PreCondition, all charges are eligible.", true, consolCost.ApportionmentCharges.Cast<ApportionSplitCharge>().All(x => !x.IsInternalJobInfoDisabled));
				CombineAssertions("Do validation for all charges are eligible. It should be no error since all charge do not need to create AutoJRJ.", () => {
					splitCharge2.JR_JH_InternalJob = splitCharge2.JR_JH;
					splitCharge2.JR_GB_InternalBranch = splitCharge2.JR_GB;
					splitCharge2.JR_GE_InternalDept = ZGuid.Empty;
					AssertEquals("It is nonJRJ charge since missing one of JRJ criterias.", false, splitCharge2.ShouldCreateCostJRJ);
					splitCharge1.Validation.ValidateJR_JH_InternalJob();
					AssertNoError(splitCharge1.JR_JH_InternalJobInfo, expectedError);
					splitCharge2.Validation.ValidateJR_JH_InternalJob();
					AssertNoError(splitCharge2.JR_JH_InternalJobInfo, expectedError);

					splitCharge2.JR_JH_InternalJob = splitCharge2.JR_JH;
					splitCharge2.JR_GB_InternalBranch = splitCharge2.JR_GB;
					splitCharge2.JR_GE_InternalDept = splitCharge2.JR_GE;
					AssertEquals("It is nonJRJ charge since JRJ criterias are matched with charge's value.", false, splitCharge2.ShouldCreateCostJRJ);
					splitCharge1.Validation.ValidateJR_JH_InternalJob();
					AssertNoError(splitCharge1.JR_JH_InternalJobInfo, expectedError);
					splitCharge2.Validation.ValidateJR_JH_InternalJob();
					AssertNoError(splitCharge2.JR_JH_InternalJobInfo, expectedError);
				});
			}

			void AssertDifferentTaxRegistrationNumber()
			{
				splitCharge1.JR_GB = branchForRegisteredNumberGroup1.PK;
				splitCharge2.JR_GB = branchForRegisteredNumberGroup2.PK;

				splitCharge1.JR_JH_InternalJob = splitCharge1.JR_JH;
				splitCharge1.JR_GB_InternalBranch = splitCharge1.JR_GB;
				splitCharge1.JR_GE_InternalDept = TestObjectCreator.FISDepartment.PK;
				AssertEquals("PreCondition, it is AutoJRJ charge since internal department is different from charge's department.", true, splitCharge1.ShouldCreateCostJRJ);
				AssertEquals(false, splitCharge1.IsInternalJobInfoDisabled);

				AssertEquals("PreCondition", true, consolCost.ApportionmentCharges.Cast<ApportionSplitCharge>().All(x => x.JR_IsUsedForApportionment));
				CombineAssertions("It is not eligible since creditor have different tax registrion number to charge branch's tax registration number", () => {
					AssertEquals(true, splitCharge2.IsInternalJobInfoDisabled);
					AssertEquals(ZGuid.Empty, splitCharge2.JR_JH_InternalJob);
					AssertEquals(ZGuid.Empty, splitCharge2.JR_GB_InternalBranch);
					AssertEquals(ZGuid.Empty, splitCharge2.JR_GE_InternalDept);
				});

				var dummyGuidForGatewaySellHeader = new ZGuid("C7236D15-9B6F-4C45-90CB-0D98DBBD9136");
				splitCharge1.JR_E6_GatewaySellHeader = dummyGuidForGatewaySellHeader;
				splitCharge2.JR_E6_GatewaySellHeader = dummyGuidForGatewaySellHeader;
				CombineAssertions("Do original validation for gateway sell apportionment.", () => {
					AssertEquals(true, splitCharge1.IsGatewaySellApportionmentCharge);
					AssertEquals(true, splitCharge2.IsGatewaySellApportionmentCharge);

					splitCharge1.Validation.ValidateJR_JH_InternalJob();
					AssertNoError(splitCharge1.JR_JH_InternalJobInfo, expectedError);
					splitCharge2.Validation.ValidateJR_JH_InternalJob();
					AssertNoError("splitCharge2 is NOT eligible for auto JRJ. Hence should skip relative validation.", splitCharge2.JR_JH_InternalJobInfo, expectedError);
				});

				splitCharge1.JR_E6_GatewaySellHeader = ZGuid.Empty;
				splitCharge2.JR_E6_GatewaySellHeader = ZGuid.Empty;
				CombineAssertions("Skip original validation for consol costing, since we have alternative validation at ApportionSplitChargeValidation.CheckJR_GB.", () => {
					AssertEquals(false, splitCharge1.IsGatewaySellApportionmentCharge);
					AssertEquals(false, splitCharge2.IsGatewaySellApportionmentCharge);

					splitCharge1.Validation.ValidateJR_JH_InternalJob();
					AssertNoError(splitCharge1.JR_JH_InternalJobInfo, expectedError);
					splitCharge2.Validation.ValidateJR_JH_InternalJob();
					AssertNoError(splitCharge2.JR_JH_InternalJobInfo, expectedError);
				});
			}

			void AssertDifferentTaxRegistrationNumber_DisableAJLForBranchWithSameTaxRegistration()
			{
				using (AutoJRJRegistryStatusHelper.SetAutoJRJEnabled_ForTestOnly(GlbCompany.CurrentCompany.PK.ToGuid()))
				{
					splitCharge1.JR_GB = branchForRegisteredNumberGroup1.PK;
					splitCharge2.JR_GB = branchForRegisteredNumberGroup2.PK;

					splitCharge1.JR_JH_InternalJob = splitCharge1.JR_JH;
					splitCharge1.JR_GB_InternalBranch = splitCharge1.JR_GB;
					splitCharge1.JR_GE_InternalDept = TestObjectCreator.FISDepartment.PK;
					AssertEquals("PreCondition, it is AutoJRJ charge since internal department is different from charge's department.", true, splitCharge1.ShouldCreateCostJRJ);

					AssertEquals("PreCondition", true, consolCost.ApportionmentCharges.Cast<ApportionSplitCharge>().All(x => x.JR_IsUsedForApportionment));
					AssertEquals("PreCondition, all charges are eligible.", true, consolCost.ApportionmentCharges.Cast<ApportionSplitCharge>().All(x => !x.IsInternalJobInfoDisabled));
					CombineAssertions("Do validation for all charges are eligible. It should be error for NonJRJ charge since have one charge need to create AutoJRJ.", () => {
						splitCharge2.JR_JH_InternalJob = splitCharge2.JR_JH;
						splitCharge2.JR_GB_InternalBranch = splitCharge2.JR_GB;
						splitCharge2.JR_GE_InternalDept = ZGuid.Empty;
						AssertEquals("It is nonJRJ charge since missing one of JRJ criterias.", false, splitCharge2.ShouldCreateCostJRJ);
						splitCharge1.Validation.ValidateJR_JH_InternalJob();
						AssertNoError(splitCharge1.JR_JH_InternalJobInfo, expectedError);
						splitCharge2.Validation.ValidateJR_JH_InternalJob();
						AssertHasError(splitCharge2.JR_JH_InternalJobInfo, expectedError);

						splitCharge2.JR_JH_InternalJob = splitCharge2.JR_JH;
						splitCharge2.JR_GB_InternalBranch = splitCharge2.JR_GB;
						splitCharge2.JR_GE_InternalDept = splitCharge2.JR_GE;
						AssertEquals("It is nonJRJ charge since JRJ criterias are matched with charge's value.", false, splitCharge2.ShouldCreateCostJRJ);
						splitCharge1.Validation.ValidateJR_JH_InternalJob();
						AssertNoError(splitCharge1.JR_JH_InternalJobInfo, expectedError);
						splitCharge2.Validation.ValidateJR_JH_InternalJob();
						AssertHasError(splitCharge2.JR_JH_InternalJobInfo, expectedError);
					});
				}
			}
		}

		public void TestShouldNotReportErrorWhenConsumerTypeNotCreateCostOrSellJRJ()
		{
			AccountingConfigurationRegistry.Instance.WIPMustHaveDebtorCode.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

			var spotQuote = QuotedBooking.New(Freight.Integration.QuoteBookingType.SpotQuote, Factory);
			var jobForSpotQuote = TestObjectCreator.CreateJob(spotQuote, false);
			var chargeForSpotQuote = TestObjectCreator.CreateCharge(jobForSpotQuote, TestObjectCreator.CC1, "Spot Quote Job Charge", TestObjectCreator.AUD, 10M, TestObjectCreator.ABIGAS, TestObjectCreator.AUD, 10M, null);

			Assert(!jobForSpotQuote.IsConsumerTypeShouldCreateCostJRJ);
			Assert(!jobForSpotQuote.IsConsumerTypeShouldCreateSellJRJ);

			chargeForSpotQuote.Validation.ValidateJR_GB_InternalBranch();
			AssertNoErrors(chargeForSpotQuote.JR_GB_InternalBranchInfo);
			AssertNoWarnings(chargeForSpotQuote.JR_GB_InternalBranchInfo);

			chargeForSpotQuote.Validation.ValidateJR_GE_InternalDept();
			AssertNoErrors(chargeForSpotQuote.JR_GE_InternalDeptInfo);
			AssertNoWarnings(chargeForSpotQuote.JR_GE_InternalDeptInfo);

			chargeForSpotQuote.Validation.ValidateJR_JH_InternalJob();
			AssertNoErrors(chargeForSpotQuote.JR_JH_InternalJobInfo);
			AssertNoWarnings(chargeForSpotQuote.JR_JH_InternalJobInfo);
		}

		public void TestInternalFieldsNotValidatedWhenCostIsPosted()
		{
			AssertInternalFieldsNotValidatedWhenPosted(
				(Charge charge) => charge.JR_OH_CostAccount = GlbCompany.CurrentCompany.OrgProxy.PK,
				(Charge charge) =>
				{
					var line = Factory.NewWithValidTestData<APInvoiceLine>();
					line.AL_LineType = TransactionLineTypes.Cost;
					charge.JR_AL_APLine = line.PK;
				});
		}

		public void TestInternalFieldsNotValidatedWhenRevenueIsPosted()
		{
			AssertInternalFieldsNotValidatedWhenPosted(
				(Charge charge) => charge.JR_OH_SellAccount = GlbCompany.CurrentCompany.OrgProxy.PK,
				(Charge charge) =>
				{
					var line = Factory.NewWithValidTestData<ARInvoiceLine>();
					line.AL_LineType = TransactionLineTypes.Revenue;
					charge.JR_AL_ARLine = line.PK;
				});
		}

		void AssertInternalFieldsNotValidatedWhenPosted(Action<Charge> setAccount, Action<Charge> postCharge)
		{
			AutoJRJRegistryStatusHelper.SetAutoJRJEnabled_ForTestOnly();
			var charge = TestObjectCreator.Job1.Charges.AddNew();
			setAccount(charge);

			charge.JR_JH_InternalJob = ZGuid.NewZGuid();
			charge.JR_GB_InternalBranch = ZGuid.NewZGuid();
			charge.JR_GE_InternalDept = ZGuid.NewZGuid();

			AssertHasErrors(charge.JR_JH_InternalJobInfo);
			AssertHasErrors(charge.JR_GB_InternalBranchInfo);
			AssertHasErrors(charge.JR_GE_InternalDeptInfo);

			postCharge(charge);

			charge.Validation.ValidateJR_JH_InternalJob();
			charge.Validation.ValidateJR_GB_InternalBranch();
			charge.Validation.ValidateJR_GE_InternalDept();

			AssertNoErrors(charge.JR_JH_InternalJobInfo);
			AssertNoErrors(charge.JR_GB_InternalBranchInfo);
			AssertNoErrors(charge.JR_GE_InternalDeptInfo);
		}

		public void TestDebtorOnWIPIsNotMandatoryForSpotQuotes()
		{
			AccountingConfigurationRegistry.Instance.WIPMustHaveDebtorCode.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

			var shipment = TestObjectCreator.CreateShipment("S0001");
			var jobForShipment = TestObjectCreator.CreateJob(shipment, false);
			var chargeForShipment = TestObjectCreator.CreateCharge(jobForShipment, TestObjectCreator.CC1, "Shipment Job Charge", TestObjectCreator.AUD, 10M, TestObjectCreator.ABIGAS, TestObjectCreator.AUD, 10M, null);

			AssertEquals(true, jobForShipment.ConsumerTypeShouldCreateWIP(chargeForShipment.JR_InvoiceType));
			chargeForShipment.Validation.ValidateJR_OH_SellAccount();
			AssertHasErrors(chargeForShipment.JR_OH_SellAccountInfo);
			string expectedError = @"You must enter a debtor. Your system has been configured so that the 'debtor' is mandatory when entering an unposted sell of non zero value.

The registry setting that governs this rule is Accounting > Job Costing > WIP Must Have Debtor Code";

			AssertHasError(chargeForShipment.JR_OH_SellAccountInfo, expectedError);

			var spotQuote = QuotedBooking.New(Freight.Integration.QuoteBookingType.SpotQuote, Factory);
			var jobForSpotQuote = TestObjectCreator.CreateJob(spotQuote, false);
			var chargeForSpotQuote = TestObjectCreator.CreateCharge(jobForSpotQuote, TestObjectCreator.CC1, "Spot Quote Job Charge", TestObjectCreator.AUD, 10M, TestObjectCreator.ABIGAS, TestObjectCreator.AUD, 10M, null);

			AssertEquals(false, jobForSpotQuote.ConsumerTypeShouldCreateWIP(chargeForSpotQuote.JR_InvoiceType));
			chargeForSpotQuote.Validation.ValidateJR_OH_SellAccount();
			AssertNoErrors(chargeForSpotQuote.JR_OH_SellAccountInfo);
		}

		public void TestJR_CostRatingOverrideCommentValidation()
		{
			Charge charge1 = Factory.NewWithValidTestData<Charge>();

			charge1.JR_CostRatingOverride = false;
			charge1.RunPreSaveValidation();
			AssertNoNotifications(charge1.JR_CostRatingOverrideCommentInfo);

			charge1.JR_CostRatingOverride = true;
			charge1.RunPreSaveValidation();
			AssertNoErrors(charge1.JR_CostRatingOverrideCommentInfo);
			AssertHasWarnings(charge1.JR_CostRatingOverrideCommentInfo);

			charge1.JR_CostRatingOverrideComment = "Something";
			AssertNoNotifications(charge1.JR_CostRatingOverrideCommentInfo);
		}

		public void TestJR_SellRatingOverrideCommentValidation()
		{
			Charge charge1 = Factory.NewWithValidTestData<Charge>();

			charge1.JR_SellRatingOverride = false;
			charge1.RunPreSaveValidation();
			AssertNoNotifications(charge1.JR_SellRatingOverrideCommentInfo);

			charge1.JR_SellRatingOverride = true;
			charge1.RunPreSaveValidation();
			AssertNoErrors(charge1.JR_SellRatingOverrideCommentInfo);
			AssertHasWarnings(charge1.JR_SellRatingOverrideCommentInfo);

			charge1.JR_SellRatingOverrideComment = "Something";
			AssertNoNotifications(charge1.JR_SellRatingOverrideCommentInfo);
		}

		public virtual void TestValidateJR_AB()
		{
			var bankAccount = Factory.New<AccBankAccount>();
			bankAccount.AB_AccountType = AccountTypeCodeDescriptionPairList.Codes.BNK;
			var creditor = TestObjectCreator.CreateOrgHeader("ABC", true, false);
			var charge1 = Factory.NewWithValidTestData<Charge>();
			charge1.JR_OH_CostAccount = creditor.PK;
			charge1.JR_APInvoiceNum = "111";
			charge1.JR_PaymentType = ReceiptTypes.Cash;

			charge1.JR_AB = ZGuid.Invalid;
			charge1.RunPreSaveValidation();
			AssertHasErrors(charge1.JR_ABInfo);

			charge1.BankAccounts.Add(bankAccount);
			charge1.JR_AB = bankAccount.PK;
			charge1.RunPreSaveValidation();
			AssertNoNotifications(charge1.JR_ABInfo);

			charge1.JR_AB = ZGuid.NewZGuid();
			charge1.RunPreSaveValidation();
			AssertHasErrors(charge1.JR_ABInfo);

			var costLine = Factory.NewWithValidTestData<APInvoiceLine>();
			charge1.JR_AL_APLine = costLine.PK;
			charge1.JR_AB = ZGuid.NewZGuid();
			charge1.RunPreSaveValidation();
			AssertNoNotifications(charge1.JR_ABInfo);

			var ofxAccount = TestObjectCreator.CreateBankAccount("OFX", "Test Bank", TestObjectCreator.AUD, TestObjectCreator.GLHeader1);
			ofxAccount.AB_AccountType = AccountTypeCodeDescriptionPairList.Codes.EPA;
			ofxAccount.AB_PaymentProvider = EPaymentProviderCodes.Codes.OFX;

			var expectedError1 = "This bank account is an E-Payment Account. Please set Payment Type to EPA - E-Payment.";
			var expectedError2 = "Bank Account is not an E-Payment Account.";

			charge1.JR_PaymentType = ReceiptTypes.Cheque;
			charge1.JR_AB = bankAccount.PK;
			AssertNoErrorContaining(charge1.JR_ABInfo, expectedError1);
			AssertNoErrorContaining(charge1.JR_ABInfo, expectedError2);

			charge1.JR_AB = ofxAccount.PK;
			AssertHasError(charge1.JR_ABInfo, expectedError1);
			AssertNoErrorContaining(charge1.JR_ABInfo, expectedError2);

			charge1.JR_PaymentType = ReceiptTypes.EPayment;
			charge1.JR_AB = bankAccount.PK;
			AssertNoErrorContaining(charge1.JR_ABInfo, expectedError1);
			AssertHasError(charge1.JR_ABInfo, expectedError2);

			charge1.JR_AB = ofxAccount.PK;
			AssertNoErrorContaining(charge1.JR_ABInfo, expectedError1);
			AssertNoErrorContaining(charge1.JR_ABInfo, expectedError2);
		}

		public virtual void TestValidateJR_AK()
		{
			AccBankAccount bankAccount = Factory.New<AccBankAccount>();
			AccChargeCode chargeCode = Factory.New<AccChargeCode>();
			AccChequeBook chequeBook = Factory.New<AccChequeBook>();
			chequeBook.AK_Code = "PPP";
			chequeBook.AK_AB = bankAccount.PK;
			Charge charge1 = Factory.NewWithValidTestData<Charge>();

			charge1.BankAccounts.Add(bankAccount);
			charge1.JR_AB = bankAccount.PK;
			charge1.JR_AC = chargeCode.PK;
			charge1.JR_PaymentType = ZArchitecture.Core.ReceiptTypes.Cheque;
			charge1.JR_AK = ZGuid.Invalid;
			charge1.RunPreSaveValidation();
			AssertHasErrors(charge1.JR_AKInfo);

			charge1.ChequeBooks.Add(chequeBook);
			charge1.JR_AK = chequeBook.PK;
			charge1.RunPreSaveValidation();
			AssertNoNotifications(charge1.JR_AKInfo);

			charge1.JR_AK = ZGuid.NewZGuid();
			charge1.RunPreSaveValidation();
			AssertHasErrors(charge1.JR_AKInfo);

			APInvoiceLine costLine = Factory.NewWithValidTestData<APInvoiceLine>();
			charge1.JR_AL_APLine = costLine.PK;
			charge1.JR_AK = ZGuid.NewZGuid();
			charge1.RunPreSaveValidation();
			AssertNoNotifications(charge1.JR_AKInfo);
		}

		public virtual void TestValidateJR_OH_CostAccount()
		{
			Charge charge1 = Factory.NewWithValidTestData<Charge>();
			charge1.JR_APInvoiceDate = ZDateTime.Now;
			charge1.JR_PaymentDate = ZDateTime.Now;
			charge1.JR_RX_NKCostCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			charge1.JR_OSCostAmt = 100M;
			charge1.JR_AB = Factory.New<AccBankAccount>().PK;
			charge1.Factory.RemoveContext(BusinessContext.InvoicingPlugInGUI);

			OrgHeader creditor = TestObjectCreator.CreateOrgHeader("CRE", true, false);
			AccountingConfigurationRegistry.Instance.AccrualMustHaveCreditorCode.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

			charge1.JR_OH_CostAccount = ZGuid.Empty;
			charge1.RunPreSaveValidation();
			AssertHasErrors(charge1.JR_OH_CostAccountInfo);

			charge1.Creditors.Add(creditor);
			charge1.JR_OH_CostAccount = creditor.PK;
			charge1.RunPreSaveValidation();
			AssertNoNotifications(charge1.JR_OH_CostAccountInfo);

			charge1.JR_OH_CostAccount = ZGuid.Empty;
			charge1.RunPreSaveValidation();
			AssertHasErrors(charge1.JR_OH_CostAccountInfo);

			APInvoiceLine costLine = Factory.NewWithValidTestData<APInvoiceLine>();
			charge1.JR_AL_APLine = costLine.PK;
			charge1.JR_OH_CostAccount = ZGuid.Empty;
			charge1.RunPreSaveValidation();
			AssertNoNotifications(charge1.JR_OH_CostAccountInfo);

			var charge2 = Factory.NewWithValidTestData<Charge>();
			charge2.JR_APInvoiceDate = ZDateTime.Now;
			charge2.JR_PaymentDate = ZDateTime.Now;
			charge2.JR_RX_NKCostCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			charge2.JR_OSCostAmt = 100M;
			charge2.JR_AB = Factory.New<AccBankAccount>().PK;
			AccountingConfigurationRegistry.Instance.AccrualMustHaveCreditorCode.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

			Factory.SetContext(Enterprise.Integration.Accounting.BusinessContext.ApportionRevenueToShipment);
			charge2.JR_OH_CostAccount = ZGuid.Empty;
			charge2.RunPreSaveValidation();
			AssertNoErrors(charge2.JR_OH_CostAccountInfo);
		}

		[DisableZeroExchangeRateOverriding]
		public void TestCheckJR_LocalCostAmt_CreateAccrual()
		{
			AssertLocalAmountRecognizeProfitValidation(
				charge => charge.JR_LocalCostAmtInfo,
				charge => charge.Validation.ValidateJR_LocalCostAmt(),
				charge =>
				{
					APInvoiceLine costLine = Factory.NewWithValidTestData<APInvoiceLine>();
					charge.JR_AL_APLine = costLine.PK;
				});
		}

		[DisableZeroExchangeRateOverriding]
		public void TestCheckJR_LocalSellAmt_CreateWIP()
		{
			AssertLocalAmountRecognizeProfitValidation(
				charge => charge.JR_LocalSellAmtInfo,
				charge => charge.Validation.ValidateJR_LocalSellAmt(),
				charge =>
				{
					var line = Factory.NewWithValidTestData<ARInvoiceLine>();
					charge.JR_AL_ARLine = line.PK;
				});
		}

		[DisableZeroExchangeRateOverriding]
		public void TestCheckJR_LocalCostAmt_CreateCostJRJ()
		{
			AssertLocalAmountRecognizeProfitValidation(
				charge => charge.JR_LocalCostAmtInfo,
				charge => charge.Validation.ValidateJR_LocalCostAmt(),
				charge =>
				{
					APInvoiceLine costLine = Factory.NewWithValidTestData<APInvoiceLine>();
					charge.JR_AL_APLine = costLine.PK;
				},
				false);
		}

		[DisableZeroExchangeRateOverriding]
		public void TestCheckJR_LocalSellAmt_CreateSellJRJ()
		{
			AssertLocalAmountRecognizeProfitValidation(
				charge => charge.JR_LocalSellAmtInfo,
				charge => charge.Validation.ValidateJR_LocalSellAmt(),
				charge =>
				{
					var line = Factory.NewWithValidTestData<ARInvoiceLine>();
					charge.JR_AL_ARLine = line.PK;
				},
				false);
		}

		public void TestErrorMessageWhenSingleChargeAmountExceedsMaxValue()
		{
			AccountingConfigurationRegistry.Instance.JobInvoiceMaximumValueOfSplittingRules.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 450m);
			using (Job job = Job.CreateWithMutex(Factory, TestObjectCreator.CreateShipment("S00012345")))
			{
				BaseCharge charge = GetNewParentBusinessObject();
				charge.JR_AC = TestObjectCreator.CC1.PK;
				charge.JR_JH = job.PK;
				charge.JR_OH_SellAccount = TestObjectCreator.LocalClient.PK;
				charge.JR_LocalSellAmt = 500m;
				var expectedErrorMessage = "The Charge has exceeded the maximum value (450) as defined in the following registry Accounting -> Job Invoicing -> Invoice Splitting Rules -> Maximum value. Please split the charge to multiple lines with value less than the maximum value.";
				AssertHasError(charge.JR_LocalSellAmtInfo, expectedErrorMessage);

				charge.JR_LocalSellAmt = 400m;
				AssertNoError(charge.JR_LocalSellAmtInfo, expectedErrorMessage);
			}
		}

		public void TestValidationNotificationsRemovedBeforeValidateAll()
		{
			BaseCharge testCharge = Factory.NewWithValidTestData<BaseCharge>();

			WIP relatedLine = Factory.NewWithValidTestData<WIP>();
			testCharge.JR_AL_ARLine = relatedLine.PK;
			relatedLine.AL_JH = testCharge.JR_JH;

			testCharge.ReverseWIP(ZDateTime.Today);

			testCharge.Validation.ValidateAll();

			string expectedError = "expectedError";
			testCharge.AddRowError(expectedError);
			AssertHasRowErrorContaining(testCharge, expectedError);

			testCharge.Validation.ValidateAll();
			AssertNoRowErrors(testCharge);
		}

		public void TestValidationJR_AC()
		{
			var shipment = TestObjectCreator.CreateShipment("S00001234");
			var testJob = TestObjectCreator.CreateJob(shipment, false);
			testJob.JH_Status = JobHeaderStatus.JobReadyForFinancialClosure.Code;

			var testCharge = testJob.Charges.AddNew();
			testCharge.JR_AC = TestObjectCreator.CC1.PK;
			testCharge.JR_RX_NKCostCurrency = Constants.CurrencyCodes.UnitedStates;
			testCharge.JR_OSCostAmt = 200;
			testCharge.JR_InvoiceType = InvoiceTypesList.Codes.ForeignCurrencyInvoice;
			testCharge.JR_OH_CostAccount = TestObjectCreator.AALSHI.PK;
			testJob.ExchangeRates[0].JF_BaseRate = 0.5;

			var cacheValue = Env.Security.ModifyChargesforFinancialClosureJob.IsAllowed;

			using (new DisposableAction(() => Env.Security.ModifyChargesforFinancialClosureJob.IsAllowed = false, () => Env.Security.ModifyChargesforFinancialClosureJob.IsAllowed = cacheValue))
			{
				testCharge.Validation.ValidateJR_AC();
				AssertHasError(testCharge.JR_ACInfo, JobIsReadyForFinancialClosureWithoutModifySecurityErrorMessage);
			}

			using (new DisposableAction(() => Env.Security.ModifyChargesforFinancialClosureJob.IsAllowed = true, () => Env.Security.ModifyChargesforFinancialClosureJob.IsAllowed = cacheValue))
			{
				testCharge.Validation.ValidateJR_AC();
				AssertNoError(testCharge.JR_ACInfo, JobIsReadyForFinancialClosureWithoutModifySecurityErrorMessage);
			}
		}

		public void TestCheckJR_OSCostExRate()
		{
			var shipment = TestObjectCreator.CreateShipment("S00001234");
			var testJob = TestObjectCreator.CreateJob(shipment, false);
			var testCharge = testJob.Charges.AddNew();
			testCharge.JR_AC = TestObjectCreator.CC1.PK;
			testCharge.JR_RX_NKCostCurrency = Constants.CurrencyCodes.UnitedStates;
			testCharge.JR_OSCostAmt = 200;
			testCharge.JR_InvoiceType = InvoiceTypesList.Codes.ForeignCurrencyInvoice;
			testCharge.JR_OH_CostAccount = TestObjectCreator.AALSHI.PK;
			testJob.ExchangeRates[0].JF_BaseRate = 0.5;

			Factory.Save();

			var expectedError = "Cost Exchange Rate for Currency USD must be greater than 0.";

			var jobCharge = testJob.Charges[0];
			jobCharge.Validation.ValidateJR_OSCostExRate();

			AssertNoError(jobCharge.JR_OSCostExRateInfo, expectedError);

			var exchangeRate = testJob.ExchangeRates[1];
			exchangeRate.JF_BaseRate = -0.5;
			jobCharge.Validation.ValidateJR_OSCostExRate();

			AssertHasError(jobCharge.JR_OSCostExRateInfo, expectedError);

			exchangeRate.JF_BaseRate = 2;
			jobCharge.Validation.ValidateJR_OSCostExRate();

			AssertNoError(jobCharge.JR_OSCostExRateInfo, expectedError);

			exchangeRate.JF_BaseRate = 0;
			jobCharge.Validation.ValidateJR_OSCostExRate();

			AssertHasError(jobCharge.JR_OSCostExRateInfo, expectedError);

			testCharge.JR_OSCostAmt = 0m;
			jobCharge.Validation.ValidateJR_OSSellExRate();

			AssertHasError("Although cost amount is 0 ,still should be validated", jobCharge.JR_OSCostExRateInfo, expectedError);
		}

		public void TestCheckJR_OSSellExRate()
		{
			ExchangeRateReader.GetReaderInstance().ClearCache();

			var shipment = TestObjectCreator.CreateShipment("S00001234");
			var testJob = TestObjectCreator.CreateJob(shipment, false);
			var testCharge = TestObjectCreator.CreateCharge(testJob, TestObjectCreator.CC1, 200m, 200m);
			testCharge.JR_RX_NKSellCurrency = Constants.CurrencyCodes.UnitedStates;
			testCharge.JR_OSSellAmt = 200;
			testCharge.JR_InvoiceType = InvoiceTypesList.Codes.ForeignCurrencyInvoice;

			var testOrgHeader = TestObjectCreator.AALSHI;
			testCharge.JR_OH_SellAccount = testOrgHeader.PK;

			var testExRate = testJob.ExchangeRates[0];
			testExRate.JF_BaseRate = 0.5;

			Factory.Save();

			var expectedError = "Sell Exchange Rate for Currency USD must be greater than 0.";

			var jobCharge = testJob.Charges[0];
			jobCharge.Validation.ValidateJR_OSSellExRate();

			AssertNoError(jobCharge.JR_OSCostExRateInfo, expectedError);

			testExRate.JF_BaseRate = -0.5;
			jobCharge.Validation.ValidateJR_OSSellExRate();

			AssertHasError(jobCharge.JR_OSSellExRateInfo, expectedError);

			testExRate.JF_BaseRate = 2;
			AssertNoError(jobCharge.JR_OSCostExRateInfo, expectedError);

			testExRate.JF_BaseRate = 0;
			jobCharge.Validation.ValidateJR_OSSellExRate();

			AssertHasError(jobCharge.JR_OSSellExRateInfo, expectedError);

			testCharge.JR_OSSellAmt = 0m;
			jobCharge.Validation.ValidateJR_OSSellExRate();

			AssertHasError("Although sell amount is 0 ,still should be validated", jobCharge.JR_OSSellExRateInfo, expectedError);
		}

		public void TestCheckCommentChargeLineValidation()
		{
			using (var job = TestObjectCreator.CreateJob(TestObjectCreator.LocalClient, 0, TestObjectCreator.Agent, 0))
			{
				var charge = job.Charges.AddNew();
				charge.JR_AC = TestObjectCreator.CC1.PK;
				charge.JR_LocalSellAmt = 100;
				charge.JR_JH_InternalJob = job.PK;
				charge.JR_GE_InternalDept = charge.JR_GE;
				charge.JR_GB_InternalBranch = charge.JR_GB;
				charge.Validation.ValidateRow();
				AssertNoRowErrors(charge);

				var commentCharge = job.Charges.AddNew();
				commentCharge.JR_AC = TestObjectCreator.CommentChargeCode.PK;
				commentCharge.JR_LocalSellAmt = 0;
				commentCharge.JR_JH_InternalJob = job.PK;
				commentCharge.JR_GE_InternalDept = charge.JR_GE;
				commentCharge.JR_GB_InternalBranch = charge.JR_GB;
				Assert("Revenue Posted", !charge.IsRevenuePosted);

				using (AccountingConfigurationRegistry.Instance.CommentChargeLineARInvoiceWarning.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, CommentChargeLineARInvoiceWarningOptions.NoAction))
				{
					charge.Validation.ValidateAll();
					AssertNoWarning(charge.JR_ACInfo, CommentChargeLineValidationMessage);
					AssertNoError(charge.JR_ACInfo, CommentChargeLineValidationMessage);
					commentCharge.Validation.ValidateAll();
					AssertNoWarning(commentCharge.JR_ACInfo, CommentChargeLineValidationMessage);
					AssertNoError(commentCharge.JR_ACInfo, CommentChargeLineValidationMessage);
				}

				using (AccountingConfigurationRegistry.Instance.CommentChargeLineARInvoiceWarning.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, CommentChargeLineARInvoiceWarningOptions.WarningValidation))
				{
					charge.Validation.ValidateAll();
					AssertNoWarning(charge.JR_ACInfo, CommentChargeLineValidationMessage);
					AssertNoError(charge.JR_ACInfo, CommentChargeLineValidationMessage);
					commentCharge.Validation.ValidateAll();
					AssertHasWarning(commentCharge.JR_ACInfo, CommentChargeLineValidationMessage);
					AssertNoError(commentCharge.JR_ACInfo, CommentChargeLineValidationMessage);
				}

				using (AccountingConfigurationRegistry.Instance.CommentChargeLineARInvoiceWarning.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, CommentChargeLineARInvoiceWarningOptions.ErrorValidation))
				{
					charge.Validation.ValidateAll();
					AssertNoWarning(charge.JR_ACInfo, CommentChargeLineValidationMessage);
					AssertNoError(charge.JR_ACInfo, CommentChargeLineValidationMessage);
					commentCharge.Validation.ValidateAll();
					AssertNoWarning(commentCharge.JR_ACInfo, CommentChargeLineValidationMessage);
					AssertHasError(commentCharge.JR_ACInfo, CommentChargeLineValidationMessage);

					var transactionLine = Factory.New<AccTransactionLines>();
					charge.JR_AL_ARLine = transactionLine.PK;
					transactionLine.AL_LineType = TransactionLineTypes.Revenue;
					Assert("Revenue Posted", charge.IsRevenuePosted);
					charge.Validation.ValidateAll();
					AssertNoWarning(charge.JR_ACInfo, CommentChargeLineValidationMessage);
					AssertNoError(charge.JR_ACInfo, CommentChargeLineValidationMessage);
					commentCharge.Validation.ValidateAll();
					AssertNoWarning(commentCharge.JR_ACInfo, CommentChargeLineValidationMessage);
					AssertHasError(commentCharge.JR_ACInfo, CommentChargeLineValidationMessage);
				}
			}
		}

		public void TestCostTaxIdAndTaxMessageValidation()
		{
			var taxRate = Factory.NewWithValidTestData<AccTaxRate>();
			taxRate.AT_Type = AccTaxRate.Types.Rated;
			taxRate.AT_Code = "TaxRate01";
			var taxMsg1 = Factory.NewWithValidTestData<AccInvMsg>();
			taxMsg1.A9_Code = "TaxMsg01";
			var taxMsg2 = Factory.NewWithValidTestData<AccInvMsg>();
			taxMsg2.A9_Code = "TaxMsg02";
			Factory.Save();

			var config = TestObjectCreator.CreateTaxIdAndTaxMessageCombinationRulesConfiguration((TransactionLineTypes.Cost, taxRate, taxMsg1));
			AccountingMasterFilesRegistry.Instance.TaxIdAndTaxMessageCombinationRules.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, config);

			var charge = Factory.New<JobCharge>();
			charge.JR_AT_CostGSTRate = taxRate.PK;
			charge.JR_A9_CostVATClass = taxMsg2.PK;

			charge.Validation.ValidateJR_A9_CostVATClass();
			var expectedMsg = @"Transaction cannot be saved/posted due to invalid Tax ID and Tax Message combination.
Line Type=CST, Tax ID=TaxRate01, Tax Message=TaxMsg02";
			AssertHasErrors(expectedMsg, charge.JR_A9_CostVATClassInfo);

			charge.JR_A9_CostVATClass = taxMsg1.PK;
			charge.Validation.ValidateJR_A9_CostVATClass();
			AssertNoErrors(charge.JR_A9_CostVATClassInfo);

			charge.JR_AT_CostGSTRate = ZGuid.Empty;
			charge.Validation.ValidateJR_A9_CostVATClass();
			AssertNoErrors(charge.JR_A9_CostVATClassInfo);

			charge.JR_AT_CostGSTRate = taxRate.PK;
			charge.JR_A9_CostVATClass = ZGuid.Empty;
			charge.Validation.ValidateJR_A9_CostVATClass();
			expectedMsg = @"Transaction cannot be saved/posted due to invalid Tax ID and Tax Message combination.
Line Type=CST, Tax ID=TaxRate01, Tax Message=";
			AssertHasErrors(expectedMsg, charge.JR_A9_CostVATClassInfo);

			charge.JR_AT_CostGSTRate = ZGuid.Empty;
			charge.JR_A9_CostVATClass = ZGuid.Empty;
			charge.Validation.ValidateJR_A9_CostVATClass();
			AssertNoErrors(charge.JR_A9_CostVATClassInfo);
		}

		public void TestSellTaxIdAndTaxMessageValidation()
		{
			var taxRate = Factory.NewWithValidTestData<AccTaxRate>();
			taxRate.AT_Type = AccTaxRate.Types.Rated;
			taxRate.AT_Code = "TaxRate01";
			var taxMsg1 = Factory.NewWithValidTestData<AccInvMsg>();
			taxMsg1.A9_Code = "TaxMsg01";
			var taxMsg2 = Factory.NewWithValidTestData<AccInvMsg>();
			taxMsg2.A9_Code = "TaxMsg02";
			Factory.Save();

			var config = TestObjectCreator.CreateTaxIdAndTaxMessageCombinationRulesConfiguration((TransactionLineTypes.Revenue, taxRate, taxMsg1));
			AccountingMasterFilesRegistry.Instance.TaxIdAndTaxMessageCombinationRules.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, config);

			var charge = Factory.New<JobCharge>();
			charge.JR_AT_SellGSTRate = taxRate.PK;
			charge.JR_A9_SellVATClass = taxMsg2.PK;

			charge.Validation.ValidateJR_A9_SellVATClass();
			var expectedMsg = @"Transaction cannot be saved/posted due to invalid Tax ID and Tax Message combination.
Line Type=REV, Tax ID=TaxRate01, Tax Message=TaxMsg02";
			AssertHasErrors(expectedMsg, charge.JR_A9_SellVATClassInfo);

			charge.JR_A9_SellVATClass = taxMsg1.PK;
			charge.Validation.ValidateJR_A9_SellVATClass();
			AssertNoErrors(charge.JR_A9_SellVATClassInfo);

			charge.JR_AT_SellGSTRate = ZGuid.Empty;
			charge.Validation.ValidateJR_A9_SellVATClass();
			AssertNoErrors(charge.JR_A9_SellVATClassInfo);

			charge.JR_AT_SellGSTRate = taxRate.PK;
			charge.JR_A9_SellVATClass = ZGuid.Empty;
			charge.Validation.ValidateJR_A9_SellVATClass();
			expectedMsg = @"Transaction cannot be saved/posted due to invalid Tax ID and Tax Message combination.
Line Type=REV, Tax ID=TaxRate01, Tax Message=";
			AssertHasErrors(expectedMsg, charge.JR_A9_SellVATClassInfo);

			charge.JR_AT_SellGSTRate = ZGuid.Empty;
			charge.JR_A9_SellVATClass = ZGuid.Empty;
			charge.Validation.ValidateJR_A9_SellVATClass();
			AssertNoErrors(charge.JR_A9_SellVATClassInfo);
		}

		public void TestSellTaxIdAndTaxMessageValidation_ForApportionmentCharges()
		{
			var taxRate = Factory.NewWithValidTestData<AccTaxRate>();
			taxRate.AT_Type = AccTaxRate.Types.Rated;
			taxRate.AT_Code = "TaxRate01";
			var taxMsg1 = Factory.NewWithValidTestData<AccInvMsg>();
			taxMsg1.A9_Code = "TaxMsg01";
			TestObjectCreator.CC1.AC_AT_GSTRate = taxRate.PK;
			Factory.Save();

			var config = TestObjectCreator.CreateTaxIdAndTaxMessageCombinationRulesConfiguration((TransactionLineTypes.Revenue, taxRate, taxMsg1));
			AccountingMasterFilesRegistry.Instance.TaxIdAndTaxMessageCombinationRules.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, config);

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			var shipment = consol.Shipments.AddNew();
			var job = TestObjectCreator.CreateJob(shipment, false);
			job.LocalChargesPK = TestObjectCreator.Debtor1.PK;
			var consolCost = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.CC1, 100m);
			consolCost.E6_OH_Creditor = TestObjectCreator.Creditor1.PK;
			AssertEquals(1, consolCost.ApportionmentCharges.Count);

			var charge = consolCost.ApportionmentCharges[0];
			var expectedMsg = @"Transaction cannot be saved/posted due to invalid Tax ID and Tax Message combination.
Line Type=REV, Tax ID=TaxRate01, Tax Message=";

			AssertEquals("Precondition", true, charge.JR_IsApportioned);
			AssertEquals("Precondition", false, charge.IsInDatabase);
			charge.Validation.ValidateJR_A9_SellVATClass();
			AssertHasWarning(charge.JR_A9_SellVATClassInfo, expectedMsg);
			AssertNoError(charge.JR_A9_SellVATClassInfo, expectedMsg);

			Factory.Save();

			charge = consolCost.ApportionmentCharges[0];
			AssertEquals("Precondition", true, charge.JR_IsApportioned);
			AssertEquals("Precondition", true, charge.IsInDatabase);
			charge.Validation.ValidateJR_A9_SellVATClass();
			AssertNoWarning(charge.JR_A9_SellVATClassInfo, expectedMsg);
			AssertHasErrors(expectedMsg, charge.JR_A9_SellVATClassInfo);
		}

		#region TestCheckProperties

		DisposableList PrepareChargeForProperty(ZPropertyInfo property, Charge charge, TestObjectCreator creator)
		{
			if (property.Name == JobChargeSchema.Constants.JR_CostSupplyType || property.Name == JobChargeSchema.Constants.JR_SellSupplyType)
			{
				return new DisposableList(new IDisposable[]
				{
					AccountingMasterFilesRegistry.Instance.EnableSupplyTypeClassificationCodes.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true)
				});
			}
			else if (property.Name == JobChargeSchema.Constants.JR_GB_CostTaxBranch)
			{
				return new DisposableList(new IDisposable[]
				{
					AccountingMasterFilesRegistry.Instance.EnableTaxBranchReporting.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true),
					new DisposableAction(() => { Env.Security.GetInvoicingSecurityCheckPoint(Env.Security.MaintainShipmentJobInvoicing, SecurityCore.AllowOverrideCostTaxBranch).IsAllowed = true; },
										() => { Env.Security.GetInvoicingSecurityCheckPoint(Env.Security.MaintainShipmentJobInvoicing, SecurityCore.AllowOverrideCostTaxBranch).IsAllowed = false; }),
					new DisposableAction(() => { charge.JR_OH_CostAccount = TestObjectCreator.TestOrganisation.PK; }, () => { charge.JR_OH_CostAccount = (ZGuid)charge.JR_OH_CostAccountInfo.OriginalValue; }),
				});
			}
			else if (property.Name == JobChargeSchema.Constants.JR_GB_SellTaxBranch)
			{
				return new DisposableList(new IDisposable[]
				{
					AccountingMasterFilesRegistry.Instance.EnableTaxBranchReporting.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true),
					new DisposableAction(() => { Env.Security.GetInvoicingSecurityCheckPoint(Env.Security.MaintainShipmentJobInvoicing, SecurityCore.AllowOverrideSellTaxBranch).IsAllowed = true; },
										() => { Env.Security.GetInvoicingSecurityCheckPoint(Env.Security.MaintainShipmentJobInvoicing, SecurityCore.AllowOverrideSellTaxBranch).IsAllowed = false; }),
					new DisposableAction(() => { charge.JR_OH_SellAccount = TestObjectCreator.TestOrganisation.PK; }, () => { charge.JR_OH_SellAccount = (ZGuid)charge.JR_OH_SellAccountInfo.OriginalValue; }),
				});
			}
			else
			{
				return new DisposableList(0);
			}
		}

		public void TestCheckProperties()
		{
			GlbStaff testUser = Factory.NewWithValidTestData<GlbStaff>();
			testUser.GS_IsController = false;
			Factory.Save();

			using (Env.SetTemporaryUserContext(testUser.GS_LoginName, Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				string[] excludedProperties = {
						JobChargeSchema.Constants.JR_APNumberOfSupportingDocuments,
						JobChargeSchema.Constants.JR_ARNumberOfSupportingDocuments,
						JobChargeSchema.Constants.JR_ProFormaCost,
						JobChargeSchema.Constants.JR_ProFormaRevenue,
						JobChargeSchema.Constants.JR_A9_CostVATClass,
						JobChargeSchema.Constants.JR_A9_SellVATClass,
						JobChargeSchema.Constants.JR_OA_SellInvoiceAddress,
						JobChargeSchema.Constants.JR_OC_SellInvoiceContact,
						JobChargeSchema.Constants.JR_AL_APLine,
						JobChargeSchema.Constants.JR_AL_ARLine,
						JobChargeSchema.Constants.JR_AL_CFXLine,
						JobChargeSchema.Constants.JR_APLinePostingStatus,
						JobChargeSchema.Constants.JR_ARLinePostingStatus,
						JobChargeSchema.Constants.JR_GB,
						JobChargeSchema.Constants.JR_GC,
						JobChargeSchema.Constants.JR_GE,
						JobChargeSchema.Constants.JR_OSSellExRate,
						JobChargeSchema.Constants.JR_CostRated,
						JobChargeSchema.Constants.JR_SellRated,
						JobChargeSchema.Constants.JR_E6,
						JobChargeSchema.Constants.JR_E6_GatewaySellHeader,
						JobChargeSchema.Constants.JR_OSCostExRate,
						JobChargeSchema.Constants.JR_APInvoiceNum,
						JobChargeSchema.Constants.JR_CostReference,
						JobChargeSchema.Constants.JR_GB_InternalBranch,
						JobChargeSchema.Constants.JR_GE_InternalDept,
						JobChargeSchema.Constants.JR_JH_InternalJob,
						JobChargeSchema.Constants.JR_OSCostGSTAmt, //JR_OSCostGSTAmt_Calc should be validated instead
						JobChargeSchema.Constants.JR_DisplaySequence,		// No longer validated. See WI00249266.
						JobChargeSchema.Constants.JR_SystemCreateTimeUtc,
						JobChargeSchema.Constants.JR_SystemCreateUser,
						JobChargeSchema.Constants.JR_SystemLastEditTimeUtc,
						JobChargeSchema.Constants.JR_SystemLastEditUser,
						JobChargeSchema.Constants.JR_IsAPCashAdvance,
						JobChargeSchema.Constants.JR_IsARCashAdvance,
						JobChargeSchema.Constants.JR_CAL_APLine,
						JobChargeSchema.Constants.JR_CAL_ARLine,
						JobChargeSchema.Constants.JR_IsSpotCost
					};

				string[] includedNonPersitentProperties =
				{
					BaseCharge.Schema.JR_OSCostGSTAmt_Calc
				};

				CombineAssertions(delegate
					{
						SecurityCore security = new UserLoginController().GetSecurityForUser(Env.CurrentUser.LoginName, Env.CurrentBranch.PK, Env.CurrentDepartment.PK);
						BusinessObjectFactory securityFactory = new BusinessObjectFactory();

						GlbSecurity loginSecurity = securityFactory.New<GlbSecurity>();
						loginSecurity.GU_GB = Env.CurrentBranch.PK;
						loginSecurity.GU_GE = Env.CurrentDepartment.PK;
						loginSecurity.GU_GS = Env.CurrentUser.PK;
						loginSecurity.GU_SecurityRight = security.Login.Code;

						GlbSecurity invoicingSecurity = securityFactory.New<GlbSecurity>();
						invoicingSecurity.GU_GB = Env.CurrentBranch.PK;
						invoicingSecurity.GU_GE = Env.CurrentDepartment.PK;
						invoicingSecurity.GU_GS = Env.CurrentUser.PK;
						invoicingSecurity.GU_SecurityRight = security.GetInvoicingSecurityCheckPoint(Env.Security.MaintainShipmentJobInvoicing, SecurityCore.AllowEnterModifyCharges).Code;

						TestObjectCreator creator = new TestObjectCreator(Factory);
						ForwardingShipment testShipment = creator.CreateShipment("S00001234");
						Job testJob = creator.CreateJob(creator.LocalClient, 0, creator.Agent, 0);
						testJob.PlugInData = testShipment;
						testJob.JH_ParentID = testShipment.PK;
						testJob.JH_ParentTableCode = "JS";
						Factory.Save();

						security.Login.IsAllowed = true;
						loginSecurity.GU_SecurityItemIsAllowed = true;
						securityFactory.Save();

						Charge testCharge = GetJobChargeInNewFactory(testJob, creator.CC1);
						testCharge.StopGSTAmountOfUnApportionedChargeFromBeingOverridden.GetSuspender();
						testCharge.JR_IsCostTaxAmountOverridden = true;

						foreach (ZPropertyInfo info in testCharge.ZPropertyInfoHash)
						{
							using (PrepareChargeForProperty(info, testCharge, creator))
							{
								if (!info.HasSetter)
								{
									continue;
								}

								if (info is ZWrappedPropertyInfo)
								{
									continue;
								}

								if (!info.IsPersistent && !includedNonPersitentProperties.Contains(info.Name))
								{
									continue;
								}

								if (excludedProperties.Contains(info.Name))
								{
									continue;
								}

								IZType oldValue = info.Value;
								IZType expectedOriginalValue = GetExpectedOriginalValue(info);

								security.GetInvoicingSecurityCheckPoint(Env.Security.MaintainShipmentJobInvoicing, SecurityCore.AllowEnterModifyCharges).IsAllowed = true;
								invoicingSecurity.GU_SecurityItemIsAllowed = true;
								securityFactory.Save();

								info.Value = GetValue(info, 1);
								AssertNoError(info, string.Format("You cannot create a new charge / modify / delete charge against a branch for which you do not have login permission. You must reset the value to its previous value {0}", expectedOriginalValue));

								security.GetInvoicingSecurityCheckPoint(Env.Security.MaintainShipmentJobInvoicing, SecurityCore.AllowEnterModifyCharges).IsAllowed = false;
								invoicingSecurity.GU_SecurityItemIsAllowed = false;
								securityFactory.Save();

								info.Value = GetValue(info, 2);
								AssertNoError(info, string.Format("You cannot create a new charge / modify / delete charge against a branch for which you do not have login permission. You must reset the value to its previous value {0}", expectedOriginalValue));

								info.Value = oldValue;
								AssertNoError(info, string.Format("You cannot create a new charge / modify / delete charge against a branch for which you do not have login permission. You must reset the value to its previous value {0}", expectedOriginalValue));
							}
						}

						security.Login.IsAllowed = false;
						loginSecurity.GU_SecurityItemIsAllowed = false;
						securityFactory.Save();

						testCharge.Delete();
						testCharge.Factory.Save();

						testCharge = GetJobChargeInNewFactory(testJob, creator.CC1);
						testCharge.StopGSTAmountOfUnApportionedChargeFromBeingOverridden.GetSuspender();
						testCharge.JR_IsCostTaxAmountOverridden = true;

						using (Env.SetTemporaryUserContext(testUser.GS_LoginName, TestObjectCreator.NonCurrentBranch.PK.ToGuid(), TestObjectCreator.NonCurrentDepartment.PK.ToGuid()))
						{
							foreach (ZPropertyInfo info in testCharge.ZPropertyInfoHash)
							{
								using (PrepareChargeForProperty(info, testCharge, creator))
								{
									if (!info.HasSetter)
									{
										continue;
									}

									if (info is ZWrappedPropertyInfo)
									{
										continue;
									}

									if (!info.IsPersistent && !includedNonPersitentProperties.Contains(info.Name))
									{
										continue;
									}

									if (excludedProperties.Contains(info.Name))
									{
										continue;
									}

									var oldValue = info.OriginalValue;
									IZType expectedOriginalValue = GetExpectedOriginalValue(info);

									security.GetInvoicingSecurityCheckPoint(Env.Security.MaintainShipmentJobInvoicing, SecurityCore.AllowEnterModifyCharges).IsAllowed = true;
									invoicingSecurity.GU_SecurityItemIsAllowed = true;
									securityFactory.Save();

									info.Value = GetValue(info, 1);
									if (info.Name != "JR_JH")
									{
										AssertNoError(info, string.Format("You cannot create a new charge / modify / delete charge against a branch for which you do not have login permission. You must reset the value to its previous value {0}", expectedOriginalValue));
									}
									else
									{
										AssertHasError("That will be always the error on JR_JH when it HasChanges", info, string.Format("You cannot create a new charge / modify / delete charge against a branch for which you do not have login permission. You must reset the value to its previous value {0}", expectedOriginalValue));
									}

									security.GetInvoicingSecurityCheckPoint(Env.Security.MaintainShipmentJobInvoicing, SecurityCore.AllowEnterModifyCharges).IsAllowed = false;
									invoicingSecurity.GU_SecurityItemIsAllowed = false;
									securityFactory.Save();

									info.Value = GetValue(info, 2);
									AssertHasError(info, string.Format("You cannot create a new charge / modify / delete charge against a branch for which you do not have login permission. You must reset the value to its previous value {0}", expectedOriginalValue));

									info.Value = oldValue;
									AssertEquals("Info.HasChanges should be false", false, info.HasChanges);
									AssertNoError(info, string.Format("You cannot create a new charge / modify / delete charge against a branch for which you do not have login permission. You must reset the value to its previous value {0}", expectedOriginalValue));
								}
							}
						}
					});
			}
		}

		Charge GetJobChargeInNewFactory(Job parentJob, AccChargeCode chargeCode)
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			Charge result = factory.New<Charge>();
			result.JR_JH = parentJob.PK;
			result.JR_GB = parentJob.JH_GB;
			result.JR_AC = chargeCode.PK;
			result.FillWithValidTestData();

			factory.Save();
			result.Job.Parent = parentJob.Parent;
			factory.Save();

			return result;
		}

		IZType GetExpectedOriginalValue(ZPropertyInfo info)
		{
			IZType result = info.OriginalValue;
			if (result is ZGuid)
			{
				if (result.IsEmpty || !result.IsValid)
				{
					result = new ZString("empty");
				}
				else
				{
					result = RelatedBusinessObjectAttribute.GetCodeForGuid(info);
				}
			}

			return result;
		}

		IZType GetValue(ZPropertyInfo info, int seed)
		{
			if (info == null)
			{
				throw new ArgumentNullException(nameof(info));
			}

			if (info.PropertyType == typeof(ZString))
			{
				char c = (char)(seed + 'a');
				return new ZString(c, Math.Min(info.MaxLength, 100));
			}
			else if (info.PropertyType == typeof(ZInt))
			{
				return (ZInt)seed;
			}
			else if (info.PropertyType == typeof(ZDecimal))
			{
				return new ZDecimal(seed);
			}
			else if (info.PropertyType == typeof(ZBool))
			{
				if (seed % 2 == 0)
				{
					return new ZBool(true);
				}
				else
				{
					return new ZBool(false);
				}
			}
			else if (info.PropertyType == typeof(ZBlob))
			{
				return new ZBlob(new byte[] { (byte)seed });
			}
			else if (info.PropertyType == typeof(ZByte))
			{
				return (ZByte)seed;
			}
			else if (info.PropertyType == typeof(ZDate))
			{
				return ZDate.Today.AddDays(-seed);
			}
			else if (info.PropertyType == typeof(ZDateTime))
			{
				return ZDateTime.Today.AddDays(-seed);
			}
			else if (info.PropertyType == typeof(ZShort))
			{
				return (ZShort)seed;
			}
			else if (info.PropertyType == typeof(ZGuid))
			{
				return ZGuid.NewZGuid();
			}
			else
			{
				throw new NotSupportedException("and what am i supposed to do with a '" + info.PropertyType.FullName + "'?");
			}
		}

		#endregion

		public void TestCheckEmptyRevenueRecognitionTypes()
		{
			string expectedErrorMessageForCost = "Cost revenue recognition type can't be empty. To fix it use menu item 'Recognize Revenue' form Job Invoicing menu.";
			string expectedErrorMessageForSell = "Sell revenue recognition type can't be empty. To fix it use menu item 'Recognize Revenue' form Job Invoicing menu.";
			var charge = GetNewParentBusinessObject();
			var validationForTest = charge.Validation;
			charge.JR_JH = ZGuid.Empty;
			AssertEquals("Precondition: CostRecognition", "", charge.CostRecognition);
			AssertEquals("Precondition: SellRecognition", "", charge.SellRecognition);
			validationForTest.ValidateAll();
			AssertNoRowError(charge, expectedErrorMessageForCost);
			AssertNoRowError(charge, expectedErrorMessageForSell);
			AssertEquals(false, validationForTest.CheckEmptyRevenueRecognitionTypesForCost());
			AssertEquals(false, validationForTest.CheckEmptyRevenueRecognitionTypesForSell());

			charge.FillWithValidTestData();
			AssertNotNull("Precondition: InvoicingJob", charge.InvoicingJob);
			AssertEquals("Precondition: CostRecognition", RevenueRecognitionLookups.RecognitionDateOptionCodes.Immediate, charge.CostRecognition);
			AssertEquals("Precondition: CostRecognition", RevenueRecognitionLookups.RecognitionDateOptionCodes.Immediate, charge.SellRecognition);
			validationForTest.ValidateAll();
			AssertNoRowError(charge, expectedErrorMessageForCost);
			AssertNoRowError(charge, expectedErrorMessageForSell);
			AssertEquals(false, validationForTest.CheckEmptyRevenueRecognitionTypesForCost());
			AssertEquals(false, validationForTest.CheckEmptyRevenueRecognitionTypesForSell());

			var accrual = TestObjectCreator.CreateAccrual(charge);
			AssertEquals("Precondition: CostRecognition", RevenueRecognitionLookups.RecognitionDateOptionCodes.Immediate, charge.CostRecognition);
			validationForTest.ValidateAll();
			AssertNoRowError(charge, expectedErrorMessageForCost);
			AssertNoRowError(charge, expectedErrorMessageForSell);
			AssertEquals(false, validationForTest.CheckEmptyRevenueRecognitionTypesForCost());
			AssertEquals(false, validationForTest.CheckEmptyRevenueRecognitionTypesForSell());

			accrual.AL_RevRecognitionType = "";
			AssertEquals("Precondition: CostRecognition", "", charge.CostRecognition);
			validationForTest.ValidateAll();
			AssertHasRowError(charge, expectedErrorMessageForCost);
			AssertNoRowError(charge, expectedErrorMessageForSell);
			AssertEquals(true, validationForTest.CheckEmptyRevenueRecognitionTypesForCost());
			AssertEquals(false, validationForTest.CheckEmptyRevenueRecognitionTypesForSell());

			var apLine = TestObjectCreator.CreateCostLine(charge, Factory.New<APInvoice>().PK);
			AssertEquals("Precondition: CostRecognition", RevenueRecognitionLookups.RecognitionDateOptionCodes.Immediate, charge.CostRecognition);
			validationForTest.ValidateAll();
			AssertNoRowError(charge, expectedErrorMessageForCost);
			AssertNoRowError(charge, expectedErrorMessageForSell);
			AssertEquals(false, validationForTest.CheckEmptyRevenueRecognitionTypesForCost());
			AssertEquals(false, validationForTest.CheckEmptyRevenueRecognitionTypesForSell());

			apLine.AL_RevRecognitionType = "";
			AssertEquals("Precondition: CostRecognition", "", charge.CostRecognition);
			validationForTest.ValidateAll();
			AssertHasRowError(charge, expectedErrorMessageForCost);
			AssertNoRowError(charge, expectedErrorMessageForSell);
			AssertEquals(true, validationForTest.CheckEmptyRevenueRecognitionTypesForCost());
			AssertEquals(false, validationForTest.CheckEmptyRevenueRecognitionTypesForSell());

			charge.ClearCostLink();
			AssertEquals("Precondition: CostRecognition", RevenueRecognitionLookups.RecognitionDateOptionCodes.Immediate, charge.CostRecognition);
			validationForTest.ValidateAll();
			AssertNoRowError(charge, expectedErrorMessageForCost);
			AssertNoRowError(charge, expectedErrorMessageForSell);
			AssertEquals(false, validationForTest.CheckEmptyRevenueRecognitionTypesForCost());
			AssertEquals(false, validationForTest.CheckEmptyRevenueRecognitionTypesForSell());

			var wip = TestObjectCreator.CreateWIP(charge);
			charge.ReverseWIP(ZDateTime.Now);
			charge.JR_AL_ARLine = wip.PK;
			if (ExceptionReporterTestListener.Instance.Count == 1 && ExceptionReporterTestListener.Instance[0].InnerException.Message.Contains("The new WIP has been reversed"))
			{
				ExceptionReporterTestListener.Instance.Clear();
			}
			AssertEquals("Precondition: SellRecognition", RevenueRecognitionLookups.RecognitionDateOptionCodes.Immediate, charge.SellRecognition);
			validationForTest.ValidateAll();
			AssertNoRowError(charge, expectedErrorMessageForCost);
			AssertNoRowError(charge, expectedErrorMessageForSell);
			AssertEquals(false, validationForTest.CheckEmptyRevenueRecognitionTypesForCost());
			AssertEquals(false, validationForTest.CheckEmptyRevenueRecognitionTypesForSell());

			wip.AL_RevRecognitionType = "";
			AssertEquals("Precondition: SellRecognition", "", charge.SellRecognition);
			validationForTest.ValidateAll();
			AssertNoRowError(charge, expectedErrorMessageForCost);
			AssertHasRowError(charge, expectedErrorMessageForSell);
			AssertEquals(false, validationForTest.CheckEmptyRevenueRecognitionTypesForCost());
			AssertEquals(true, validationForTest.CheckEmptyRevenueRecognitionTypesForSell());

			var arLine = TestObjectCreator.CreateRevenueLine(charge, Factory.New<ARInvoice>().PK);
			AssertEquals("Precondition: SellRecognition", RevenueRecognitionLookups.RecognitionDateOptionCodes.Immediate, charge.SellRecognition);
			validationForTest.ValidateAll();
			AssertNoRowError(charge, expectedErrorMessageForCost);
			AssertNoRowError(charge, expectedErrorMessageForSell);
			AssertEquals(false, validationForTest.CheckEmptyRevenueRecognitionTypesForCost());
			AssertEquals(false, validationForTest.CheckEmptyRevenueRecognitionTypesForSell());

			arLine.AL_RevRecognitionType = "";
			AssertEquals("Precondition: SellRecognition", "", charge.SellRecognition);
			validationForTest.ValidateAll();
			AssertNoRowError(charge, expectedErrorMessageForCost);
			AssertHasRowError(charge, expectedErrorMessageForSell);
			AssertEquals(false, validationForTest.CheckEmptyRevenueRecognitionTypesForCost());
			AssertEquals(true, validationForTest.CheckEmptyRevenueRecognitionTypesForSell());

			charge.ClearRevenueLink();
			AssertEquals("Precondition: SellRecognition", RevenueRecognitionLookups.RecognitionDateOptionCodes.Immediate, charge.SellRecognition);
			validationForTest.ValidateAll();
			AssertNoRowError(charge, expectedErrorMessageForCost);
			AssertNoRowError(charge, expectedErrorMessageForSell);
			AssertEquals(false, validationForTest.CheckEmptyRevenueRecognitionTypesForCost());
			AssertEquals(false, validationForTest.CheckEmptyRevenueRecognitionTypesForSell());

			charge.JR_AL_APLine = apLine.PK;
			charge.JR_AL_ARLine = arLine.PK;
			validationForTest.ValidateAll();
			AssertHasRowError(charge, expectedErrorMessageForCost);
			AssertHasRowError(charge, expectedErrorMessageForSell);
			AssertEquals(true, validationForTest.CheckEmptyRevenueRecognitionTypesForCost());
			AssertEquals(true, validationForTest.CheckEmptyRevenueRecognitionTypesForSell());
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
			using (CreditChecker.GetCreditLimitValidationSuspender(Factory))
			{
				var shipment = TestObjectCreator.CreateShipment("S00001234");
				var job = TestObjectCreator.CreateJob(shipment, false);
				var testOrg = TestObjectCreator.CreateOrgHeader("TestOrg", true, true, true, true, true, true);
				var charge = job.Charges.AddNew();
				job.LocalChargesPK = testOrg.PK;
				charge.JR_OH_SellAccount = charge.JR_OH_CostAccount = testOrg.PK;
				charge.JR_OSCostExRate = 1m;
				charge.JR_OSSellExRate = 1m;
				charge.JR_GB = branch.PK;
				charge.JR_GE = TestObjectCreator.FESDepartment.PK;

				//Charge Branch
				charge.JR_AC = TestObjectCreator.FRT.PK;
				AssertHasRowError("Charge Branch org proxy has an empty state", charge, IndiaCompanyEmptyStateErrorMessages.EmptyStateErrorMessageForChargeBranch);
				branch.GB_OH_OrgProxy = branchOrgProxy.PK;
				charge.Validation.ValidateJR_GB();
				AssertNoRowErrors(charge);

				//Origin
				var origin = Factory.NewWithValidTestData<RefUNLOCO>();
				var state1 = Factory.NewWithValidTestData<RefCountryStates>();
				state1.RW_Code = "";
				origin.RL_RW = state1.PK;
				shipment.JS_RL_NKOrigin = origin.RL_Code;
				charge.Validation.ValidateJR_GB();
				AssertHasRowWarning("Origin has an empty state", charge, IndiaCompanyEmptyStateErrorMessages.EmptyStateWarningMessageForOrigin);
				shipment.Origin.CountryStates.RW_Code = "123";
				charge.Validation.ValidateJR_GB();
				AssertNoRowWarnings(charge);

				//Destination
				var destination = Factory.NewWithValidTestData<RefUNLOCO>();
				var state2 = Factory.NewWithValidTestData<RefCountryStates>();
				state2.RW_Code = "";
				destination.RL_RW = state2.PK;
				shipment.JS_RL_NKDestination = destination.RL_Code;
				charge.Validation.ValidateJR_AC();
				AssertHasRowWarning("Destination has an empty state", charge, IndiaCompanyEmptyStateErrorMessages.EmptyStateWarningMessageForDestination);
				shipment.Destination.CountryStates.RW_Code = "123";
				charge.Validation.ValidateJR_AC();
				AssertNoRowWarnings(charge);

				//Sell Account & Cost Account
				testOrg.MainAddress.OA_State = "";
				charge.Validation.ValidateJR_AC();
				AssertHasRowWarning("Sell Account has an empty state", charge, IndiaCompanyEmptyStateErrorMessages.EmptyStateWarningMessageForSellAccount);
				AssertHasRowWarning("Cost Account has an empty state", charge, IndiaCompanyEmptyStateErrorMessages.EmptyStateWarningMessageForCostAccount);
				testOrg.MainAddress.OA_State = "123";
				charge.Validation.ValidateJR_AC();
				AssertNoRowWarnings(charge);
			}
		}

		public void TestCheckEmptyStateForDestination_NullReference()
		{
			using (GlbCompany.TemporaryLoginInNewCompanyForCountry(Core.Constants.CountryCodes.India))
			using (AccountingMasterFilesRegistry.Instance.EnableBranchLevelTaxOverrideRuleConfigurations.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var job = Factory.NewJobWithValidTestDataForTesting<Job>();
				var charge = job.Charges.AddNew();
				AssertNoExceptionThrown("Should not throw NullReferenceException", charge.Validation.ValidateJR_AC);
			}
		}

		public void TestNotifiesUserInCaseOfLocalSellAmountRecalculated()
		{
			var charge = Factory.New<MockParentBaseCharge>();

			charge.SetSellAmountRecalculatedAlertOn(11.45m);

			charge.Validation.ValidateJR_LocalSellAmt();

			AssertHasWarning(charge.JR_LocalSellAmtInfo, "Local Sell Amount was recalculated based on the current configuration. Please check that all values on this charge are as expected. Previous value was: 11.45");
		}

		public void TestCheckExporterExemptionForCostAccount()
		{
			var registry = AccountingMasterFilesRegistry.Instance;

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Italy))
			{
				var requiredDocument = SetupDataForExporterExemption(TestObjectCreator.Creditor1, JobRequiredDocument.DocUsage.Creditor);

				var shipment = TestObjectCreator.CreateShipment("S0001");
				var job = TestObjectCreator.CreateJob(shipment, false);
				var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "Shipment Job Charge", TestObjectCreator.AUD, 0M, null, TestObjectCreator.AUD, 0M, null);
				var validationForTest = charge.Validation;

				using (registry.ValidateTaxIDApplicationForExporterExemptionItaly.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
				{
					charge.JR_OH_CostAccount = TestObjectCreator.Creditor1.PK;
					validationForTest.ValidateJR_OH_CostAccount();

					AssertNoWarnings(charge.JR_OH_CostAccountInfo);

					requiredDocument.EQ_ValidToDate = ZDate.Today.AddDays(30);
					validationForTest.ValidateJR_OH_CostAccount();
					var expectedWarningMessage1 = $"The Exporter Exemption Certificate Number {requiredDocument.EQ_DocNumber} is expiring on {requiredDocument.EQ_ValidToDate.ToShortDateString()}.";
					AssertEquals("Expected certificate expired warning for Payables", true, charge.JR_OH_CostAccountInfo.Notifications.GetWarnings().Contains(expectedWarningMessage1));

					AccountingConfigurationRegistry.Instance.ExporterExemptionCellingLimitThreshold.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, 50);
					var ceilingLimitAttribute = requiredDocument.Attributes.AddNew();
					ceilingLimitAttribute.D0_AttribName = JobRequiredDocAttribTypeList.Codes.CeilingLimit;
					ceilingLimitAttribute.D0_AttribValue = "1000";

					validationForTest.ValidateJR_OH_CostAccount();
					var expectedWarningMessage2 = @"The Exporter Exemption Ceiling Limit Threshold of 50% has exceeded.
The Total Certificate Ceiling Limit is EUR 1000.00.
Total Posted Transactions Amount using the exporter exemption certificates is EUR 700.00.";
					AssertEquals("Expected exemption ceiling limit threshold warning for Payables", true, charge.JR_OH_CostAccountInfo.Notifications.GetWarnings().Contains(ZString.Format("{0}\r\n{1}", expectedWarningMessage1, expectedWarningMessage2)));
				}

				charge.JR_OH_CostAccount = ZGuid.Empty;
				requiredDocument.EQ_ValidToDate = ZDate.Today.AddDays(-30);

				using (registry.ValidateTaxIDApplicationForExporterExemptionItaly.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
				{
					var errorMessage = @"There are errors that need to be corrected before saving the current Accounts Receivable Invoice.
TAX ID: DICH.INT, based on the Registry [Accounting -> Receivable Defaults -> Default Settings -> Validate Tax ID Application for Exporter Exemption (Italy)] it is not possible to proceed with the post because you are posting a transaction on a debtor that has one or more EXV-VAT/GST Exporter Exemption document with CEILING LIMIT.
The sum of the transactions that contain DICH.INT Tax ID including this one you are posting, exceeds the CEILING LIMIT by EUR 500.00.
To proceed with the post please fix Tax ID Code or save a new EXV-VAT/GST Exporter Exemption in the Debtor Organization eDocs, with an higher CEILING LIMIT or disable the Registry.";

					charge.JR_OH_CostAccount = TestObjectCreator.Creditor1.PK;
					validationForTest.ValidateJR_OH_CostAccount();

					AssertNoWarnings(charge.JR_OH_CostAccountInfo);

					requiredDocument.EQ_ValidToDate = ZDate.Today.AddDays(30);
					validationForTest.ValidateJR_OH_CostAccount();
					var expectedWarningMessage1 = $"The Exporter Exemption Certificate Number {requiredDocument.EQ_DocNumber} is expiring on {requiredDocument.EQ_ValidToDate.ToShortDateString()}.";
					AssertEquals("Expected certificate expired warning", true, charge.JR_OH_CostAccountInfo.Notifications.GetWarnings().Contains(expectedWarningMessage1));

					AccountingConfigurationRegistry.Instance.ExporterExemptionCellingLimitThreshold.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, 50);
					var ceilingLimitAttribute = requiredDocument.Attributes.AddNew();
					ceilingLimitAttribute.D0_AttribName = JobRequiredDocAttribTypeList.Codes.CeilingLimit;
					ceilingLimitAttribute.D0_AttribValue = "1000";

					validationForTest.ValidateJR_OH_CostAccount();
					AssertEquals("Exemption ceiling limit threshold error not raised for Payables", false, charge.JR_OH_CostAccountInfo.Notifications.GetErrors().Contains(errorMessage));
				}
			}
		}

		public void TestCheckExporterExemptionForSellAccount()
		{
			var registry = AccountingMasterFilesRegistry.Instance;

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Italy))
			{
				var requiredDocument = SetupDataForExporterExemption(TestObjectCreator.Debtor, JobRequiredDocument.DocUsage.Debtor);

				var shipment = TestObjectCreator.CreateShipment("S0001");
				var job = TestObjectCreator.CreateJob(shipment, false);
				var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "Shipment Job Charge", TestObjectCreator.EUR, 0M, null, TestObjectCreator.EUR, 0M, null);
				var validationForTest = charge.Validation;

				using (registry.ValidateTaxIDApplicationForExporterExemptionItaly.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
				{
					requiredDocument.EQ_ValidToDate = ZDate.Today.AddDays(30);
					charge.JR_OH_SellAccount = TestObjectCreator.Debtor.PK;
					validationForTest.ValidateJR_OH_SellAccount();
					var expectedWarningMessage1 = $"The Exporter Exemption Certificate Number {requiredDocument.EQ_DocNumber} is expiring on {requiredDocument.EQ_ValidToDate.ToShortDateString()}.";
					AssertHasWarning("Expect certificate expired warning", charge.JR_OH_SellAccountInfo, expectedWarningMessage1);

					AccountingConfigurationRegistry.Instance.ExporterExemptionCellingLimitThreshold.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, 50);
					var ceilingLimitAttribute = requiredDocument.Attributes.AddNew();
					ceilingLimitAttribute.D0_AttribName = JobRequiredDocAttribTypeList.Codes.CeilingLimit;
					ceilingLimitAttribute.D0_AttribValue = "1000";

					validationForTest.ValidateJR_OH_SellAccount();
					var expectedWarningMessage2 = @"The Exporter Exemption Ceiling Limit Threshold of 50% has exceeded.
The Total Certificate Ceiling Limit is EUR 1000.00.
Total Posted Transactions Amount using the exporter exemption certificates is EUR 700.00.";
					AssertHasWarning("Expect exemption ceiling limit threshold warning", charge.JR_OH_SellAccountInfo, ZString.Format("{0}\r\n{1}", expectedWarningMessage1, expectedWarningMessage2));
				}

				charge.JR_OH_SellAccount = ZGuid.Empty;
				requiredDocument.EQ_ValidToDate = ZDate.Today.AddDays(-30);

				using (registry.ValidateTaxIDApplicationForExporterExemptionItaly.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
				{
					var errorMessage = @"There are errors that need to be corrected before saving the current Accounts Receivable Invoice.
TAX ID: DICH.INT, based on the Registry [Accounting -> Receivable Defaults -> Default Settings -> Validate Tax ID Application for Exporter Exemption (Italy)] it is not possible to proceed with the post because you are posting a transaction on a debtor that has one or more EXV-VAT/GST Exporter Exemption document with CEILING LIMIT.
The sum of the transactions that contain DICH.INT Tax ID including this one you are posting, exceeds the CEILING LIMIT by EUR 500.00.
To proceed with the post please fix Tax ID Code or save a new EXV-VAT/GST Exporter Exemption in the Debtor Organization eDocs, with an higher CEILING LIMIT or disable the Registry.";

					requiredDocument.EQ_ValidToDate = ZDate.Today.AddDays(30);
					charge.JR_OH_SellAccount = TestObjectCreator.Debtor.PK;
					validationForTest.ValidateJR_OH_SellAccount();
					var expectedWarningMessage1 = $"The Exporter Exemption Certificate Number {requiredDocument.EQ_DocNumber} is expiring on {requiredDocument.EQ_ValidToDate.ToShortDateString()}.";
					AssertHasWarning("Expect certificate expired warning", charge.JR_OH_SellAccountInfo, expectedWarningMessage1);

					AccountingConfigurationRegistry.Instance.ExporterExemptionCellingLimitThreshold.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, 50);
					var ceilingLimitAttribute = requiredDocument.Attributes.AddNew();
					ceilingLimitAttribute.D0_AttribName = JobRequiredDocAttribTypeList.Codes.CeilingLimit;
					ceilingLimitAttribute.D0_AttribValue = "1000";

					validationForTest.ValidateJR_OH_SellAccount();
					AssertHasWarning("Expect certificate expired warning", charge.JR_OH_SellAccountInfo, expectedWarningMessage1);
					AssertEquals("Exemption ceiling limit threshold error not raised for Creditors", false, charge.JR_OH_SellAccountInfo.Notifications.GetErrors().Contains(errorMessage));
				}
			}
		}

		JobRequiredDocument SetupDataForExporterExemption(OrgHeader orgHeader, string docUsage)
		{
			var stampDutyRecharge = new StampDutyRecharge
			{
				StampDutyRechargeOrganizationType = StampDutyRechargeOrganizationType.NotRecharging,
				StampDutyRechargeTransactionType = StampDutyRechargeTransactionType.All
			};
			AccountingConfigurationRegistry.Instance.StampDutyRecharge.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, stampDutyRecharge);

			var query = new ZQuery(AccTaxRateSchema.AT_IsActive, true);
			query.AddToFilter(AccTaxRateSchema.AT_RN_NKCountry, CountryCodes.Italy);
			query.AddToFilter(AccTaxRateSchema.AT_Code, "DICH.INT");
			var taxRate = Factory.LoadTop1<AccTaxRate>(query);

			var invoiceType = docUsage == JobRequiredDocument.DocUsage.Debtor ? typeof(ARInvoice) : typeof(APInvoice);
			var dichInvoice = TestObjectCreator.CreateInvoice(invoiceType, "INV001", TestObjectCreator.EUR, 1M, orgHeader);
			var line = TestObjectCreator.CreateInvoiceLine(dichInvoice, TestObjectCreator.EUR, 1M, 700M, 70M, 0M);
			line.AL_AT = taxRate.PK;

			var requiredDocument = dichInvoice.Header.RequiredDocuments.AddNew();
			requiredDocument.EQ_DocCategory = ReferenceTypes.ClientSupplierRelationship;
			requiredDocument.EQ_DocType = RefDocTypes.VATExporterExemption;
			requiredDocument.EQ_DocUsage = docUsage;
			requiredDocument.EQ_RN_NKRelatedCountry = CountryCodes.Italy;
			requiredDocument.EQ_DocNumber = "2019-10";
			requiredDocument.EQ_DateReceived = ZDateTimeOffset.Today.AddDays(-1);
			requiredDocument.EQ_ValidToDate = ZDate.Today.AddDays(31);

			Factory.Save();

			return requiredDocument;
		}

		public void TestDisplaySequenceCanBeModifiedByUsersWithoutBranchEditSecurity_WI00249266()
		{
			var shipment = TestObjectCreator.CreateShipment("S0001");
			var job = TestObjectCreator.CreateJob(shipment, false);
			var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "Shipment Job Charge", TestObjectCreator.AUD, 0M, null, TestObjectCreator.AUD, 0M, null);
			Factory.Save();
			AssertNoErrors("Precondition: no validation errors on Charge.", charge);

			var branch = TestObjectCreator.CreateBranch("ABC", GlbCompany.CurrentCompany);
			var testUser = Factory.NewWithValidTestData<GlbStaff>();
			testUser.GS_IsController = false;
			Factory.Save();

			var securityFactory = new BusinessObjectFactory();

			var security = new UserLoginController().GetSecurityForUser(Env.CurrentUser.LoginName, Env.CurrentBranch.PK, Env.CurrentDepartment.PK);
			var loginSecurityCurrentBranch = securityFactory.New<GlbSecurity>();
			loginSecurityCurrentBranch.GU_GB = Env.CurrentBranch.PK;
			loginSecurityCurrentBranch.GU_GE = Env.CurrentDepartment.PK;
			loginSecurityCurrentBranch.GU_GS = testUser.PK;
			loginSecurityCurrentBranch.GU_SecurityRight = security.Login.Code;
			loginSecurityCurrentBranch.GU_SecurityItemIsAllowed = false;
			var loginSecurityABCBranch = securityFactory.New<GlbSecurity>();
			loginSecurityABCBranch.GU_GB = branch.PK;
			loginSecurityABCBranch.GU_GE = Env.CurrentDepartment.PK;
			loginSecurityABCBranch.GU_GS = testUser.PK;
			loginSecurityABCBranch.GU_SecurityRight = security.Login.Code;
			loginSecurityABCBranch.GU_SecurityItemIsAllowed = true;

			var chargeSecurity = securityFactory.New<GlbSecurity>();
			chargeSecurity.GU_GB = branch.PK;
			chargeSecurity.GU_GE = Env.CurrentDepartment.PK;
			chargeSecurity.GU_GS = testUser.PK;
			chargeSecurity.GU_SecurityRight = security.GetInvoicingSecurityCheckPoint(Env.Security.MaintainShipmentJobInvoicing, SecurityCore.AllowEnterModifyCharges).Code;
			chargeSecurity.GU_SecurityItemIsAllowed = false;
			securityFactory.Save();

			using (Env.SetTemporaryUserContext(testUser.GS_LoginName, branch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				Assert("Precondition: user is not allowed to modify charge", !job.IsAllowedToModifyChargesFromOtherBranchesOrDepartments);

				var oldDesc = charge.JR_Desc;
				charge.JR_Desc = "CHANGED Shipment Job Charge";
				AssertHasError("Users without access to Job Branch may NOT change other Charge fields.", charge.JR_DescInfo, "You cannot create a new charge / modify / delete charge against a branch for which you do not have login permission. You must reset the value to its previous value Shipment Job Charge");
				charge.JR_Desc = oldDesc;

				AssertNoErrors("Precondition: no validation errors.", charge);
				charge.JR_DisplaySequence = 2;
				AssertNoErrors("Users without access to Job Branch may change Charge.JR_DisplaySequence, SO THAT related jobs with automatically reordered duplicate sequence numbers don't cause validation errors when the user hasn't changed anything.", charge.JR_DisplaySequenceInfo);
				AssertNoErrors("Users without access to Job Branch may change Charge.JR_DisplaySequence, and not trigger other validation messages.", charge);
			}
		}

		public void TestCheckAllowedToChangeForJobIsReadyForFinancialClosureWithoutModifySecurity()
		{
			var shipment = TestObjectCreator.CreateShipment("S0001");
			var job = TestObjectCreator.CreateJob(shipment, false);
			var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "Shipment Job Charge", TestObjectCreator.EUR, 0M, null, TestObjectCreator.EUR, 0M, null);
			var validationForTest = charge.Validation;
			Factory.Save();

			job.JH_Status = "JFC";
			charge.JR_LocalCostAmt = 1000M;

			Assert("Procondition", charge.IsInDatabase);
			Assert("Procondition", charge.JR_LocalCostAmtInfo.HasChanges);
			AssertNotNull("Procondition", charge.Job);
			Env.Security.ModifyChargesforFinancialClosureJob.IsAllowed = false;

			validationForTest.CheckAllowedToChange(charge.JR_LocalCostAmtInfo);
			AssertHasError(charge.JR_LocalCostAmtInfo, JobIsReadyForFinancialClosureWithoutModifySecurityErrorMessage);

			charge.JR_LocalCostAmtInfo.ClearAllNotifications();
			Factory.SetContext(BusinessContext.PostManagerCreatingTransaction);
			validationForTest.CheckAllowedToChange(charge.JR_LocalCostAmtInfo);
			AssertNoError(charge.JR_LocalCostAmtInfo, JobIsReadyForFinancialClosureWithoutModifySecurityErrorMessage);

			Env.Security.ModifyChargesforFinancialClosureJob.IsAllowed = true;

			charge.JR_LocalCostAmtInfo.ClearAllNotifications();
			validationForTest.CheckAllowedToChange(charge.JR_LocalCostAmtInfo);
			AssertNoError(charge.JR_LocalCostAmtInfo, JobIsReadyForFinancialClosureWithoutModifySecurityErrorMessage);

			ErrorReporter.Clear();
		}

		public void TestJobIsReadyForFinancialClosureWithoutModifySecurity_ChargeNotUsed()
		{
			var consol = TestObjectCreator.CreateConsol();
			var shipment1 = TestObjectCreator.CreateShipment("S0001", consol);
			var shipment2 = TestObjectCreator.CreateShipment("S0002", consol);
			var job1 = TestObjectCreator.CreateJob(shipment1, true);
			var job2 = TestObjectCreator.CreateJob(shipment2, true);
			Factory.Save();

			job1.JH_Status = JobHeaderStatus.Codes.JobReadyForFinancialClosure;
			job2.JH_Status = JobHeaderStatus.Codes.JobReadyForFinancialClosure;

			var consolCost = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.FRT, 100);
			AssertEquals("Precondition: ApportionmentCharges.Count", 2, consolCost.ApportionmentCharges.Count);

			var charge1 = consolCost.ApportionmentCharges.Cast<ApportionSplitCharge>().First(x => x.Job.JH_JobNum == "S0001");
			var charge2 = consolCost.ApportionmentCharges.Cast<ApportionSplitCharge>().First(x => x.Job.JH_JobNum == "S0002");
			charge2.JR_IsUsedForApportionment = false;
			Assert("Precondition: charge 1 JR_IsUsedForApportionment", charge1.JR_IsUsedForApportionment);
			Assert("Precondition: charge 2 JR_IsUsedForApportionment", !charge2.JR_IsUsedForApportionment);

			Env.Security.ModifyChargesforFinancialClosureJob.IsAllowed = false;
			charge1.Validation.ValidateJR_AC();
			charge2.Validation.ValidateJR_AC();
			AssertHasError(charge1.JR_ACInfo, JobIsReadyForFinancialClosureWithoutModifySecurityErrorMessage);
			AssertNoError(charge2.JR_ACInfo, JobIsReadyForFinancialClosureWithoutModifySecurityErrorMessage);
		}

		public void TestJobIsReadyForFinancialClosureWithoutModifySecurity_InAPInvoiceOrCreditNoteForm()
		{
			var shipment = TestObjectCreator.CreateShipment("S0001");
			var job = TestObjectCreator.CreateJob(shipment, false);
			Factory.Save();

			var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "Shipment Job Charge", TestObjectCreator.EUR, 0M, null, TestObjectCreator.EUR, 0M, null);
			job.JH_Status = JobHeaderStatus.Codes.JobReadyForFinancialClosure;
			Env.Security.ModifyChargesforFinancialClosureJob.IsAllowed = false;
			charge.Validation.ValidateJR_AC();
			AssertHasError(charge.JR_ACInfo, JobIsReadyForFinancialClosureWithoutModifySecurityErrorMessage);

			using (charge.Factory.SetTempContext(BusinessContext.APInvoiceForm))
			{
				charge.Validation.ValidateJR_AC();
				AssertNoError(charge.JR_ACInfo, JobIsReadyForFinancialClosureWithoutModifySecurityErrorMessage);
			}

			using (charge.Factory.SetTempContext(BusinessContext.APCreditNoteForm))
			{
				charge.Validation.ValidateJR_AC();
				AssertNoError(charge.JR_ACInfo, JobIsReadyForFinancialClosureWithoutModifySecurityErrorMessage);
			}
		}

		#region JR_OH_SellAccount

		public void TestCheckJR_OH_SellAccount()
		{
			using (AutoJRJRegistryStatusHelper.SetAutoJRJEnabled_ForTestOnly())
			{
				var charge = PrepareChargeWithOrgProxies(out OrgHeader proxyOrg);

				string expectedError = "You cannot set both the Cost Account and Sell Account to be the organization proxies.";
				charge.Validation.ValidateJR_OH_SellAccount();
				AssertHasError(charge.JR_OH_SellAccountInfo, expectedError);

				charge.JR_OH_SellAccount = ZGuid.Empty;

				charge.Validation.ValidateJR_OH_SellAccount();
				AssertNoErrors(charge.JR_OH_SellAccountInfo);

				Factory.Save();

				AssertEquals(true, charge.IsCostPosted);
				charge.JR_OH_SellAccount = proxyOrg.PK;
				charge.Validation.ValidateJR_OH_SellAccount();
				AssertHasError(charge.JR_OH_SellAccountInfo, expectedError);

				using (charge.Factory.SetTempContext(BusinessContext.AutoJobRevenueJournal))
				{
					AssertEquals(proxyOrg.PK, charge.JR_OH_SellAccount);
					charge.Validation.ValidateJR_OH_SellAccount();
					AssertNoErrors(charge.JR_OH_SellAccountInfo);
				}
				Factory.Save();

				AssertEquals(true, charge.IsRevenuePosted);
				charge.Validation.ValidateJR_OH_SellAccount();
				AssertNoErrors(charge.JR_OH_SellAccountInfo);
			}
		}
		#endregion

		#region JR_OH_CostAccount

		public void TestBaseChargeValidationCheckJR_OH_CostAccount()
		{
			using (AutoJRJRegistryStatusHelper.SetAutoJRJEnabled_ForTestOnly())
			{
				var charge = PrepareChargeWithOrgProxies(out OrgHeader proxyOrg);

				string expectedError = "You cannot set both the Cost Account and Sell Account to be the organization proxies.";
				charge.Validation.ValidateJR_OH_CostAccount();
				AssertHasError(charge.JR_OH_CostAccountInfo, expectedError);

				charge.JR_OH_CostAccount = ZGuid.Empty;

				charge.Validation.ValidateJR_OH_CostAccount();
				AssertNoErrors(charge.JR_OH_CostAccountInfo);

				Factory.Save();

				AssertEquals(true, charge.IsRevenuePosted);
				charge.JR_OH_CostAccount = charge.InternalBranch.GB_OH_OrgProxy;
				charge.Validation.ValidateJR_OH_CostAccount();
				AssertHasError(charge.JR_OH_CostAccountInfo, expectedError);

				using (charge.Factory.SetTempContext(BusinessContext.AutoJobRevenueJournal))
				{
					AssertEquals(charge.InternalBranch.GB_OH_OrgProxy, charge.JR_OH_CostAccount);
					charge.Validation.ValidateJR_OH_CostAccount();
					AssertNoErrors(charge.JR_OH_CostAccountInfo);
				}
				Factory.Save();

				AssertEquals(true, charge.IsCostPosted);
				charge.Validation.ValidateJR_OH_CostAccount();
				AssertNoErrors(charge.JR_OH_CostAccountInfo);
			}
		}

		#endregion

		Charge PrepareChargeWithOrgProxies(out OrgHeader proxyOrg)
		{
			var saSyd = TestObjectCreator.CreateOrgHeader("SA_SYD", true, true, "AUSYD");
			var raBne = TestObjectCreator.CreateOrgHeader("RA_BNE", true, true, "AUBNE");

			var bneBranch = Factory.LoadTop1<GlbBranch>(new ZQuery(GlbBranchSchema.GB_Code, "BNE"));
			bneBranch.GB_OH_OrgProxy = raBne.PK;

			var sydBranch = Factory.LoadTop1<GlbBranch>(new ZQuery(GlbBranchSchema.GB_Code, "SYD"));
			sydBranch.GB_OH_OrgProxy = saSyd.PK;

			Factory.Save();
			proxyOrg = raBne;

			var s2cnr = TestObjectCreator.CreateOrgHeader("S2CNR", false, true, "AUSYD");
			var s2cne = TestObjectCreator.CreateOrgHeader("S2CNE", false, true, "CNSHA");
			var shipment = TestObjectCreator.CreateShipment("S0002", "AUSYD", "CNSHA", null, incoTerm: "FOB", housebill: "S0002");
			shipment.ConsignorPK = s2cnr.PK;
			shipment.ConsigneePK = s2cne.PK;

			Factory.Save();

			var shipmentJob = new Job.Loader(shipment).TryLoadOrCreateWithoutMutexForTestOnly();
			var charge = shipmentJob.Charges.AddNew();
			charge.JR_AC = TestObjectCreator.FRT.PK;
			charge.JR_OH_SellAccount = proxyOrg.PK;
			charge.JR_OSSellAmt = 200m;
			charge.JR_RX_NKSellCurrency = "AUD";
			charge.JR_OH_CostAccount = charge.InternalBranch.GB_OH_OrgProxy;
			return charge;
		}

		#region Implementation

		class MockParentBaseCharge : BaseCharge
		{
			public MockParentBaseCharge(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public void SetSellAmountRecalculatedAlertOn(ZDecimal prevSellAmtValue)
			{
				LocalSellAmtOverridenAndUserNeedToCheck = true;
				LocalSellAmtPreviousValue = prevSellAmtValue;
			}

			protected internal override ZDecimal CalculateCFXAmt()
			{
				throw new NotImplementedException();
			}
		}

		protected abstract BaseCharge GetNewParentBusinessObject(BusinessObjectFactory factory);

		protected BaseCharge GetNewParentBusinessObject()
		{
			return GetNewParentBusinessObject(Factory);
		}

		TestObjectCreator fTestObjectCreator;
		protected TestObjectCreator TestObjectCreator
		{
			get { return fTestObjectCreator ?? (fTestObjectCreator = new TestObjectCreator(Factory)); }
		}

		#region Assert Local Amounts

		delegate ZPropertyInfo GetChargeLocalAmountInfoDelegate(BaseCharge charge);
		delegate void ValidateLocalAmountDelegate(BaseCharge charge);
		delegate void PostRelatedPartDelegate(BaseCharge charge);

		void AssertLocalAmountRecognizeProfitValidation(GetChargeLocalAmountInfoDelegate getChargeLocalAmountInfo,
			ValidateLocalAmountDelegate validateLocalAmount, PostRelatedPartDelegate postRelatedPart, bool createWipOrAccrual = true)
		{
			ZString errorMessageWithArrivalDate = "You cannot save non-zero value charges until the 'Actual/Estimated Arrival Date' is set.";
			ZString errorMessageWithDepartureDate = "You cannot save non-zero value charges until the 'Actual/Estimated Departure Date' is set.";
			ZString errorMessageWithPickupDate = "You cannot save non-zero value charges until the 'Pickup Date' is set.";
			ZString errorMessageWithDeliveryDate = "You cannot save non-zero value charges until the 'Delivery Date' is set.";

			RevenueRecognitionCollection registryCollection = new RevenueRecognitionCollection();
			RevenueRecognition registryValue = registryCollection.AddNew();
			registryValue.JobType = "SHP";
			registryValue.DirectionCode = Constants.FreightShipmentDirection.Code.All;
			registryValue.Mode = RevenueRecognitionLookups.ModeAdditionalCodes.All;
			registryValue.RecognitionDateOptionCode = RevenueRecognitionLookups.RecognitionDateOptionCodes.ActualArrivalDate;
			AccountingConfigurationRegistry.Instance.RevenueRecognitionSetup.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, registryCollection);

			AccountingConfigurationRegistry.Instance.RecognizeProfitOnWIPsAccrualsBeforePosting.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			Job internalJob = null;

			using (Job job = Job.CreateWithMutex(Factory, TestObjectCreator.CreateShipment("S00012345")))
			{
				BaseCharge charge = GetNewParentBusinessObject();
				charge.JR_AC = TestObjectCreator.CC1.PK;
				charge.JR_JH = job.PK;

				AssertEquals("Precondition", ZDateTime.Empty, job.GetRevenueRecognitionDate(charge.CostRecognition));
				AssertEquals("Precondition", ZDateTime.Empty, job.GetRevenueRecognitionDate(charge.SellRecognition));

				ZPropertyInfo chargeLocalAmountInfo = getChargeLocalAmountInfo(charge);

				if (createWipOrAccrual)
				{
					AccountingConfigurationRegistry.Instance.EnableNegativeAccrualBehaviors.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
					chargeLocalAmountInfo.Value = (ZDecimal)(-10M);
					validateLocalAmount(charge);
					AssertHasError(chargeLocalAmountInfo, errorMessageWithArrivalDate);
					AccountingConfigurationRegistry.Instance.EnableNegativeAccrualBehaviors.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
					validateLocalAmount(charge);
					AssertNoErrors(chargeLocalAmountInfo);

					chargeLocalAmountInfo.Value = (ZDecimal)10M;
					validateLocalAmount(charge);
					AssertHasError(chargeLocalAmountInfo, errorMessageWithArrivalDate);

					AccountingConfigurationRegistry.Instance.EnableDeferredRevenueRecognition.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
					validateLocalAmount(charge);
					AssertHasError(chargeLocalAmountInfo, errorMessageWithArrivalDate);
					AccountingConfigurationRegistry.Instance.EnableDeferredRevenueRecognition.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

					chargeLocalAmountInfo.Value = (ZDecimal)0M;
					validateLocalAmount(charge);
					AssertNoErrors(chargeLocalAmountInfo);

					AccountingConfigurationRegistry.Instance.CreateWIPOrAccrualWhenNoInvoicesPosted.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);
					chargeLocalAmountInfo.Value = (ZDecimal)20M;
					validateLocalAmount(charge);
					AssertNoErrors(chargeLocalAmountInfo);

					AccountingConfigurationRegistry.Instance.CreateWIPOrAccrualWhenNoInvoicesPosted.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
					validateLocalAmount(charge);
					AssertHasError(chargeLocalAmountInfo, errorMessageWithArrivalDate);

					AccountingConfigurationRegistry.Instance.RecognizeProfitOnWIPsAccrualsBeforePosting.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
					validateLocalAmount(charge);
					AssertNoErrors(chargeLocalAmountInfo);

					AccountingConfigurationRegistry.Instance.RecognizeProfitOnWIPsAccrualsBeforePosting.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
					validateLocalAmount(charge);
					AssertHasError(chargeLocalAmountInfo, errorMessageWithArrivalDate);
				}
				else    //Auto Job Revenue Journal
				{
					AutoJRJRegistryStatusHelper.SetAutoJRJEnabled_ForTestOnly(GlbCompany.CurrentCompany.PK.ToGuid());
					var branch = Factory.Load<GlbBranch>(Env.CurrentBranch.PK);
					branch.GB_OH_OrgProxy = TestObjectCreator.TestOrganisation.PK;

					internalJob = TestObjectCreator.CreateJob(TestObjectCreator.CreateShipment("S00067890"), false);
					charge.JR_OH_SellAccount = charge.JR_OH_CostAccount = TestObjectCreator.TestOrganisation.PK;
					charge.JR_LocalSellAmt = charge.JR_LocalCostAmt = 10M;
					charge.JR_JH_InternalJob = internalJob.PK;
					charge.JR_GB_InternalBranch = GlbBranch.CurrentBranch.PK;
					charge.JR_GE_InternalDept = GlbDepartment.CurrentDepartment.PK;

					validateLocalAmount(charge);
					AssertHasError(chargeLocalAmountInfo, errorMessageWithArrivalDate);
				}

				TestObjectCreator.CreateTestPeriods(ZDateTime.Now.AddMonths(-1));

				((ForwardingShipment)job.PlugInData).JS_E_ARV = ZDateTime.Now;
				validateLocalAmount(charge);
				AssertNoErrors(chargeLocalAmountInfo);

				((ForwardingShipment)job.PlugInData).JS_E_ARV = ZDateTime.Empty;
				validateLocalAmount(charge);
				AssertHasError(chargeLocalAmountInfo, errorMessageWithArrivalDate);

				TestObjectCreator.CreateJobChargeRevRecognition(job, RevenueRecognitionLookups.RecognitionDateOptionCodes.ActualArrivalDate, ZDateTime.BrettsBirthday);
				validateLocalAmount(charge);
				AssertNoErrors(chargeLocalAmountInfo);

				registryValue.RecognitionDateOptionCode = RevenueRecognitionLookups.RecognitionDateOptionCodes.ActualDepartureDate;
				AccountingConfigurationRegistry.Instance.RevenueRecognitionSetup.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, registryCollection);
				job.ClearCacheOfRevenueRecognitionRegistryItems_ForTestsOnly();
				validateLocalAmount(charge);
				AssertHasError(chargeLocalAmountInfo, errorMessageWithDepartureDate);

				registryValue.RecognitionDateOptionCode = RevenueRecognitionLookups.RecognitionDateOptionCodes.DeliveryDate;
				AccountingConfigurationRegistry.Instance.RevenueRecognitionSetup.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, registryCollection);
				job.ClearCacheOfRevenueRecognitionRegistryItems_ForTestsOnly();
				validateLocalAmount(charge);
				AssertHasError(chargeLocalAmountInfo, errorMessageWithDeliveryDate);

				registryValue.RecognitionDateOptionCode = RevenueRecognitionLookups.RecognitionDateOptionCodes.Immediate;
				AccountingConfigurationRegistry.Instance.RevenueRecognitionSetup.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, registryCollection);
				job.ClearCacheOfRevenueRecognitionRegistryItems_ForTestsOnly();
				validateLocalAmount(charge);
				AssertNoErrors(chargeLocalAmountInfo);

				registryValue.RecognitionDateOptionCode = RevenueRecognitionLookups.RecognitionDateOptionCodes.JobClosure;
				AccountingConfigurationRegistry.Instance.RevenueRecognitionSetup.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, registryCollection);
				job.ClearCacheOfRevenueRecognitionRegistryItems_ForTestsOnly();
				validateLocalAmount(charge);
				AssertNoErrors(chargeLocalAmountInfo);

				registryValue.RecognitionDateOptionCode = RevenueRecognitionLookups.RecognitionDateOptionCodes.JobOpenDate;
				AccountingConfigurationRegistry.Instance.RevenueRecognitionSetup.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, registryCollection);
				job.ClearCacheOfRevenueRecognitionRegistryItems_ForTestsOnly();
				validateLocalAmount(charge);
				AssertNoErrors(chargeLocalAmountInfo);

				registryValue.RecognitionDateOptionCode = RevenueRecognitionLookups.RecognitionDateOptionCodes.PickupDate;
				AccountingConfigurationRegistry.Instance.RevenueRecognitionSetup.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, registryCollection);
				job.ClearCacheOfRevenueRecognitionRegistryItems_ForTestsOnly();
				validateLocalAmount(charge);
				AssertHasError(chargeLocalAmountInfo, errorMessageWithPickupDate);

				if (postRelatedPart != null)
				{
					postRelatedPart(charge);
					validateLocalAmount(charge);
					AssertNoErrors(chargeLocalAmountInfo);
				}
			}
		}

		#endregion

		#endregion
	}
}
