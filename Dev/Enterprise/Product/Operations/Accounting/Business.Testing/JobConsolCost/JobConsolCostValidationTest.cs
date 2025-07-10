using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.Journal;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using static Enterprise.Core.Constants;
using PrepaidCollectCodes = Enterprise.Accounting.Integration.PrepaidCollectFreightForwardingList.Codes;

namespace Enterprise.Accounting.Business.ConsolCosting
{
	public class JobConsolCostValidationTest : BusinessObjectValidationTestCase
	{
		protected virtual JobConsolCost GetConsolCost()
		{
			JobConsolCost cost = Factory.New<JobConsolCost>();
			return cost;
		}

		protected override void SetUp()
		{
			base.SetUp();
			ObjectCreator = new TestObjectCreator(Factory);
		}

		protected TestObjectCreator ObjectCreator;

		#region Tax Branch

		public void TestCheckE6_GB_CostTaxBranch()
		{
			var testObjectCreator = new TestObjectCreator(Factory);
			var nonCurrentCompany = Factory.NewWithValidTestData<GlbCompany>();
			var nonCurrentCompanyBranch = Factory.NewWithValidTestData<GlbBranch>();
			nonCurrentCompanyBranch.GB_GC = nonCurrentCompany.PK;

			var currentCompanyUnActiveBranch = Factory.NewWithValidTestData<GlbBranch>();
			currentCompanyUnActiveBranch.GB_GC = GlbCompany.CurrentCompany.PK;
			currentCompanyUnActiveBranch.GB_IsActive = false;
			testObjectCreator.Creditor1.CompanyData.OB_APVATConfig = AccountingMasterFilesConstants.OrganisationTaxConfiguartionTypes.Default.Code;
			Factory.Save();

			var cost = GetConsolCost();
			AssertCheckE6_GB_CostTaxBranch(true);
			AssertCheckE6_GB_CostTaxBranch(false);

			void AssertCheckE6_GB_CostTaxBranch(bool enableTaxBranchReporting)
			{
				using (testObjectCreator.SetUpTaxBranchRegistry(enableTaxBranchReporting))
				{
					testObjectCreator.ResetSecurityCore();

					cost.E6_OH_Creditor = testObjectCreator.Creditor1.PK;
					cost.E6_GB_CostTaxBranch = ZGuid.Empty;
					AssertEquals("Precondition", false, cost.IsPosted);
					AssertEquals("Precondition", !enableTaxBranchReporting, cost.E6_GB_CostTaxBranchInfo.ReadOnly);

					cost.Validation.ValidateE6_GB_CostTaxBranch();
					AssertEquals(enableTaxBranchReporting, cost.E6_GB_CostTaxBranchInfo.HasError("Please enter a Tax Branch."));

					cost.E6_GB_CostTaxBranch = nonCurrentCompanyBranch.PK;
					AssertEquals(enableTaxBranchReporting, cost.E6_GB_CostTaxBranchInfo.HasError("Enter a valid Tax Branch."));

					cost.E6_GB_CostTaxBranch = currentCompanyUnActiveBranch.PK;
					AssertEquals(enableTaxBranchReporting, cost.E6_GB_CostTaxBranchInfo.HasError("Enter a valid Tax Branch."));

					cost.E6_GB_CostTaxBranch = GlbBranch.CurrentBranch.PK;
					if (enableTaxBranchReporting)
					{
						AssertEquals(false, cost.E6_GB_CostTaxBranchInfo.HasErrors());
					}
					else
					{
						AssertEquals(true, cost.E6_GB_CostTaxBranchInfo.HasError(@"Tax branch values on unposted charges in the billing tab conflict with at least one of the following settings:
- 'Enabled Tax Branch Reporting' registry value
- Debtor / Creditor 'Tax is Applicable' flag
- Current Login Company 'VAT Registered' flag
One possible way to resolve this is to go into the ""Job Invoicing"" menu and click the ""Reset Unposted lines Tax Default"" option."));
					}

					cost.E6_GB_CostTaxBranch = ZGuid.Invalid;
					AssertEquals(true, cost.E6_GB_CostTaxBranchInfo.HasError("Enter a valid Tax Branch."));
				}
			}
		}

		public void TestCheckE6_GB_CostTaxBranch_ShouldHaveSameTaxBranch()
		{
			var testObjectCreator = new TestObjectCreator(Factory);
			var consol = testObjectCreator.CreateConsol("AUSYD", "USLAX");
			var shipment1 = testObjectCreator.CreateShipment("S001", consol);
			var shipment2 = testObjectCreator.CreateShipment("S002", consol);
			var shipmentJob1 = new Job.Loader(shipment1).TryLoadOrCreateWithoutMutexForTestOnly();
			var shipmentJob2 = new Job.Loader(shipment2).TryLoadOrCreateWithoutMutexForTestOnly();
			testObjectCreator.Creditor1.CompanyData.OB_APVATConfig = AccountingMasterFilesConstants.OrganisationTaxConfiguartionTypes.Default.Code;
			testObjectCreator.Creditor2.CompanyData.OB_APVATConfig = AccountingMasterFilesConstants.OrganisationTaxConfiguartionTypes.Default.Code;
			Factory.Save();

			AssertCheckE6_GB_CostTaxBranch_ShouldHaveSameTaxBranch(true);
			AssertCheckE6_GB_CostTaxBranch_ShouldHaveSameTaxBranch(false);

			void AssertCheckE6_GB_CostTaxBranch_ShouldHaveSameTaxBranch(bool enableTaxBranchReporting)
			{
				using (testObjectCreator.SetUpTaxBranchRegistry(enableTaxBranchReporting))
				{
					testObjectCreator.ResetSecurityCore();

					var error = "All cost lines with the same Creditor and AP Invoice Number must have the same Cost Tax Branch.";

					//Tax Branches are different
					var cost1 = PrepareCost(testObjectCreator.Creditor1.PK, "Test001", testObjectCreator.NonCurrentBranch.PK, false);
					var cost2 = PrepareCost(testObjectCreator.Creditor1.PK, "Test001", GlbBranch.CurrentBranch.PK, false);
					AssertEquals(enableTaxBranchReporting, cost1.E6_GB_CostTaxBranchInfo.HasError(error));
					AssertEquals(enableTaxBranchReporting, cost2.E6_GB_CostTaxBranchInfo.HasError(error));

					//Creditors & InvoiceNums & Tax Branch are all equal 
					cost1 = PrepareCost(testObjectCreator.Creditor1.PK, "Test002", testObjectCreator.NonCurrentBranch.PK, false);
					cost2 = PrepareCost(testObjectCreator.Creditor1.PK, "Test002", testObjectCreator.NonCurrentBranch.PK, false);
					AssertEquals(false, cost1.E6_GB_CostTaxBranchInfo.HasError(error));
					AssertEquals(false, cost2.E6_GB_CostTaxBranchInfo.HasError(error));

					//InvoiceNums & Tax Branch are different
					cost1 = PrepareCost(testObjectCreator.Creditor1.PK, "Test003", testObjectCreator.NonCurrentBranch.PK, false);
					cost2 = PrepareCost(testObjectCreator.Creditor1.PK, "Test003_1", GlbBranch.CurrentBranch.PK, false);
					AssertEquals(false, cost1.E6_GB_CostTaxBranchInfo.HasError(error));
					AssertEquals(false, cost2.E6_GB_CostTaxBranchInfo.HasError(error));

					//Creditors & Tax Branch are different
					cost1 = PrepareCost(testObjectCreator.Creditor1.PK, "Test004", testObjectCreator.NonCurrentBranch.PK, false);
					cost2 = PrepareCost(testObjectCreator.Creditor2.PK, "Test004", GlbBranch.CurrentBranch.PK, false);
					AssertEquals(false, cost1.E6_GB_CostTaxBranchInfo.HasError(error));
					AssertEquals(false, cost2.E6_GB_CostTaxBranchInfo.HasError(error));

					//Invoice is Posted & Tax Branch are different
					cost1 = PrepareCost(testObjectCreator.Creditor1.PK, "Test006", testObjectCreator.NonCurrentBranch.PK, false);
					cost2 = PrepareCost(testObjectCreator.Creditor1.PK, "Test006", GlbBranch.CurrentBranch.PK, true);
					AssertEquals(false, cost1.E6_GB_CostTaxBranchInfo.HasError(error));
					AssertEquals(false, cost2.E6_GB_CostTaxBranchInfo.HasError(error));

					//Invalid Tax Branch PK
					cost1 = PrepareCost(testObjectCreator.Creditor1.PK, "Test007", ZGuid.Invalid, false);
					cost2 = PrepareCost(testObjectCreator.Creditor1.PK, "Test007", GlbBranch.CurrentBranch.PK, true);
					AssertEquals(false, cost1.E6_GB_CostTaxBranchInfo.HasError(error));
					AssertEquals(false, cost2.E6_GB_CostTaxBranchInfo.HasError(error));

					//InvoiceNum is empty
					cost1 = PrepareCost(testObjectCreator.Creditor1.PK, "", testObjectCreator.NonCurrentBranch.PK, false);
					cost2 = PrepareCost(testObjectCreator.Creditor1.PK, "", GlbBranch.CurrentBranch.PK, true);
					AssertEquals(false, cost1.E6_GB_CostTaxBranchInfo.HasError(error));
					AssertEquals(false, cost2.E6_GB_CostTaxBranchInfo.HasError(error));

					//Invalid Creditor PK
					cost1 = PrepareCost(ZGuid.Invalid, "Test007", testObjectCreator.NonCurrentBranch.PK, false);
					cost2 = PrepareCost(testObjectCreator.Creditor1.PK, "Test007", GlbBranch.CurrentBranch.PK, true);
					AssertEquals(false, cost1.E6_GB_CostTaxBranchInfo.HasError(error));
					AssertEquals(false, cost2.E6_GB_CostTaxBranchInfo.HasError(error));

					error = "The Cost Tax Branch value of all apportioned charges must be the same. Please re-enter the Consol Cost's Tax Branch to update the apportioned charges.";
					cost1 = PrepareCost(testObjectCreator.Creditor1.PK, "Test007", testObjectCreator.NonCurrentBranch.PK, false);
					cost1.ApportionmentCharges[0].JR_GB_CostTaxBranch = testObjectCreator.NonCurrentBranch.PK;
					cost1.ApportionmentCharges[1].JR_GB_CostTaxBranch = GlbBranch.CurrentBranch.PK;
					cost1.RunPreSaveValidation();
					AssertEquals(enableTaxBranchReporting, cost1.E6_GB_CostTaxBranchInfo.HasError(error));
				}
			}

			JobConsolCost PrepareCost(ZGuid creditor, ZString invoiceNum, ZGuid taxBranch, bool isPosted)
			{
				var cost = testObjectCreator.CreateConsolCost(consol, testObjectCreator.CC1, 100, testObjectCreator.Creditor1);
				cost.E6_AC_ChargeCode = testObjectCreator.CC1.PK;
				cost.E6_OH_Creditor = creditor;
				cost.E6_InvoiceNum = invoiceNum;
				cost.E6_GB_CostTaxBranch = taxBranch;

				if (isPosted)
				{
					var header = Factory.NewWithValidTestData<AccTransactionHeader>();
					cost.E6_AH_APInvoice = header.PK;
				}
				AssertEquals("Precondition", isPosted, cost.IsPosted);

				consol.GetApportionments().CostsCollection.RunPreSaveValidation();

				return cost;
			}
		}

		public void TestCheckE6_GB_CostTaxBranch_OrgTaxApplicable()
		{
			var org = ObjectCreator.CreateOrgHeader("TST", true, false);
			org.CompanyData.OB_APVATConfig = AccountingMasterFilesConstants.OrganisationTaxConfiguartionTypes.Default.Code;
			Factory.Save();

			var consol = ObjectCreator.CreateConsol(saveIt: true);
			var consolCost = ObjectCreator.CreateConsolCost(consol, ObjectCreator.CC1, creditor: org);
			consolCost.E6_AT_TaxRate = ObjectCreator.GST1.PK;
			consolCost.E6_GB_CostTaxBranch = GlbBranch.CurrentBranch.PK;
			var apportionCharge = consolCost.ApportionmentCharges.AddNew();
			apportionCharge.JR_OSCostAmt = 100m;

			AccountingMasterFilesRegistry.Instance.EnableTaxBranchReporting.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			consolCost.Validation.ValidateE6_GB_CostTaxBranch();
			AssertNoErrors("Pre-condition", consolCost.E6_GB_CostTaxBranchInfo);

			consolCost.E6_GB_CostTaxBranch = ZGuid.Empty;
			consolCost.Validation.ValidateE6_GB_CostTaxBranch();
			AssertHasError("Tax Branch should be filled", consolCost.E6_GB_CostTaxBranchInfo, @"Tax branch values on unposted charges in the billing tab conflict with at least one of the following settings:
- 'Enabled Tax Branch Reporting' registry value
- Debtor / Creditor 'Tax is Applicable' flag
- Current Login Company 'VAT Registered' flag
One possible way to resolve this is to go into the ""Job Invoicing"" menu and click the ""Reset Unposted lines Tax Default"" option.");

			AccountingMasterFilesRegistry.Instance.EnableTaxBranchReporting.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			consolCost.Validation.ValidateE6_GB_CostTaxBranch();
			AssertNoErrors("Tax Branch reporting registry is off", consolCost.E6_GB_CostTaxBranchInfo);

			consolCost.E6_GB_CostTaxBranch = ZGuid.Empty;
			var invoice = ObjectCreator.CreateInvoice(typeof(APInvoice));
			consolCost.E6_AH_APInvoice = invoice.PK;
			AssertEquals(true, consolCost.IsPosted);
			AssertNoErrors("ConsolCost is posted", consolCost.E6_GB_CostTaxBranchInfo);
		}

		#endregion

		#region E6_SupplyType

		public void TestCheckE6_SupplyType()
		{
			var cost = GetConsolCost();
			AssertCheckE6_SupplyType(true);
			AssertCheckE6_SupplyType(false);

			void AssertCheckE6_SupplyType(bool enableSupplyTypeClassificationCodes)
			{
				using (AccountingMasterFilesRegistry.Instance.EnableSupplyTypeClassificationCodes.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, enableSupplyTypeClassificationCodes))
				{
					cost.E6_AH_APInvoice = ZGuid.Empty;
					AssertEquals("Precondition", false, cost.IsPosted);

					var expectedError = "Enter a valid Cost Supply Type.";
					cost.E6_SupplyType = "ERR";
					AssertEquals(enableSupplyTypeClassificationCodes, cost.E6_SupplyTypeInfo.HasError(expectedError));

					cost.E6_SupplyType = AccountingMasterFilesConstants.SupplyTypeClassificationList[0].Code;
					AssertEquals(false, cost.E6_SupplyTypeInfo.HasError(expectedError));

					var expectedWarning = "The Cost Supply Type is not specified. Please check if a supply type is needed before posting.";
					cost.E6_SupplyType = string.Empty;
					AssertEquals(enableSupplyTypeClassificationCodes, cost.E6_SupplyTypeInfo.HasWarning(expectedWarning));

					cost.E6_SupplyType = AccountingMasterFilesConstants.SupplyTypeClassificationList[0].Code;
					AssertEquals(false, cost.E6_SupplyTypeInfo.HasWarning(expectedWarning));

					cost.E6_AH_APInvoice = Factory.NewWithValidTestData<APInvoice>().PK;
					AssertEquals("Precondition", true, cost.IsPosted);

					cost.E6_SupplyType = "ERR";
					AssertEquals(false, cost.E6_SupplyTypeInfo.HasError(expectedError));

					cost.E6_SupplyType = string.Empty;
					AssertEquals(false, cost.E6_SupplyTypeInfo.HasWarning(expectedWarning));
				}
			}
		}

		public void TestCheckE6_SupplyTypeWhenHasContextAPInvoiceApportionToConsol()
		{
			var expectedLackOfCodeErrorMessage = "Please enter a Cost Supply Type.";
			var expectedLackOfCodeWarningMessage = "The Cost Supply Type is not specified. Please check if a supply type is needed before posting.";
			var expectedInvalidCodeErrorMessage = "Enter a valid Cost Supply Type.";

			Factory.SetContext(BusinessContext.APInvoiceApportionToConsol);

			using (AccountingMasterFilesRegistry.Instance.EnableSupplyTypeClassificationCodes.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true))
			using (AccountingMasterFilesRegistry.Instance.SupplyTypeClassificationCodesIsMandatory.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, false))
			{
				var consol = ObjectCreator.CreateConsol("AUD", "LAX", "C0001");
				var consolCost = ObjectCreator.CreateConsolCost(consol, ObjectCreator.CC1);
				consolCost.E6_SupplyType = AccountingMasterFilesConstants.SupplyTypeClassificationCodes.INA;
				consolCost.E6_SupplyType = ZString.Empty;
				AssertHasWarning(consolCost.E6_SupplyTypeInfo, expectedLackOfCodeWarningMessage);
				consolCost.E6_SupplyType = "111";
				AssertHasError(consolCost.E6_SupplyTypeInfo, expectedInvalidCodeErrorMessage);
			}
			using (AccountingMasterFilesRegistry.Instance.EnableSupplyTypeClassificationCodes.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true))
			using (AccountingMasterFilesRegistry.Instance.SupplyTypeClassificationCodesIsMandatory.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true))
			{
				var consol = ObjectCreator.CreateConsol("AUD", "LAX", "C0002");
				var consolCost = ObjectCreator.CreateConsolCost(consol, ObjectCreator.CC1);
				consolCost.E6_SupplyType = AccountingMasterFilesConstants.SupplyTypeClassificationCodes.INA;
				consolCost.E6_SupplyType = ZString.Empty;
				AssertHasError(consolCost.E6_SupplyTypeInfo, expectedLackOfCodeErrorMessage);
				consolCost.E6_SupplyType = "111";
				AssertHasError(consolCost.E6_SupplyTypeInfo, expectedInvalidCodeErrorMessage);
			}
		}

		public void TestCheckE6_SupplyType_ForApportionmentCharges()
		{
			var expectedError = "The Cost Supply Type value of all apportioned charges must be the same. Please re-enter the Consol Cost's Cost Supply Type to update the apportioned charges.";
			var testObjectCreator = new TestObjectCreator(Factory);
			var consol = testObjectCreator.CreateConsol();
			consol.Shipments.AddNew();
			consol.Shipments.AddNew();

			var cost = testObjectCreator.CreateConsolCost(consol, testObjectCreator.CC1, testObjectCreator.AUD, 1M, 500M);
			Factory.Save();

			AssertCheckE6_SupplyType_ForApportionmentCharges(true);
			AssertCheckE6_SupplyType_ForApportionmentCharges(false);

			void AssertCheckE6_SupplyType_ForApportionmentCharges(bool enableSupplyTypeClassificationCodes)
			{
				using (AccountingMasterFilesRegistry.Instance.EnableSupplyTypeClassificationCodes.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, enableSupplyTypeClassificationCodes))
				{
					cost.E6_SupplyType = AccountingMasterFilesConstants.SupplyTypeClassificationCodes.LOC;
					AssertEquals(true, cost.ApportionmentCharges.OfType<ApportionSplitCharge>().All(x => x.JR_CostSupplyType == AccountingMasterFilesConstants.SupplyTypeClassificationCodes.LOC));
					AssertEquals(false, cost.E6_SupplyTypeInfo.HasError(expectedError));

					cost.ApportionmentCharges[0].JR_CostSupplyType = AccountingMasterFilesConstants.SupplyTypeClassificationCodes.LOA;
					cost.Validation.ValidateE6_SupplyType();
					AssertEquals(enableSupplyTypeClassificationCodes, cost.E6_SupplyTypeInfo.HasError(expectedError));

					cost.E6_SupplyType = AccountingMasterFilesConstants.SupplyTypeClassificationCodes.INX;
					AssertEquals(true, cost.ApportionmentCharges.OfType<ApportionSplitCharge>().All(x => x.JR_CostSupplyType == AccountingMasterFilesConstants.SupplyTypeClassificationCodes.INX));
					AssertEquals(false, cost.E6_SupplyTypeInfo.HasError(expectedError));
				}
			}
		}

		#endregion

		public void TestTaxIdAndTaxMessageValidation()
		{
			var taxRate = Factory.NewWithValidTestData<AccTaxRate>();
			taxRate.AT_Type = AccTaxRate.Types.Rated;
			taxRate.AT_Code = "TaxRate01";
			var taxMsg1 = Factory.NewWithValidTestData<AccInvMsg>();
			taxMsg1.A9_Code = "TaxMsg01";
			var taxMsg2 = Factory.NewWithValidTestData<AccInvMsg>();
			taxMsg2.A9_Code = "TaxMsg02";
			Factory.Save();

			var config = ObjectCreator.CreateTaxIdAndTaxMessageCombinationRulesConfiguration((TransactionLineTypes.Cost, taxRate, taxMsg1));
			AccountingMasterFilesRegistry.Instance.TaxIdAndTaxMessageCombinationRules.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, config);

			var consol = ObjectCreator.CreateConsol("AUSYD", "NZAKL", "C001001");
			var consolCost = ObjectCreator.CreateConsolCost(consol, ObjectCreator.CC1, 100m);
			consolCost.E6_AT_TaxRate = taxRate.PK;
			consolCost.E6_A9_VATClass = taxMsg2.PK;
			consolCost.Validation.ValidateE6_A9_VATClass();

			var expectedMsg = @"Transaction cannot be saved/posted due to invalid Tax ID and Tax Message combination.
Line Type=CST, Tax ID=TaxRate01, Tax Message=TaxMsg02";
			AssertHasErrors(expectedMsg, consolCost.E6_A9_VATClassInfo);

			consolCost.E6_A9_VATClass = taxMsg1.PK;
			consolCost.Validation.ValidateE6_A9_VATClass();
			AssertNoErrors(consolCost.E6_A9_VATClassInfo);

			consolCost.E6_AT_TaxRate = ZGuid.Empty;
			consolCost.Validation.ValidateE6_A9_VATClass();
			AssertNoErrors(consolCost.E6_A9_VATClassInfo);

			consolCost.E6_AT_TaxRate = taxRate.PK;
			consolCost.E6_A9_VATClass = ZGuid.Empty;
			consolCost.Validation.ValidateE6_A9_VATClass();
			expectedMsg = @"Transaction cannot be saved/posted due to invalid Tax ID and Tax Message combination.
Line Type=CST, Tax ID=TaxRate01, Tax Message=";
			AssertHasErrors(expectedMsg, consolCost.E6_A9_VATClassInfo);

			consolCost.E6_AT_TaxRate = ZGuid.Empty;
			consolCost.E6_A9_VATClass = ZGuid.Empty;
			consolCost.Validation.ValidateE6_A9_VATClass();
			AssertNoErrors(consolCost.E6_A9_VATClassInfo);
		}

		public void TestTaxIdAndTaxMessageValidationWithPostedJobConsolCost()
		{
			var taxRate1 = Factory.NewWithValidTestData<AccTaxRate>();
			taxRate1.AT_Type = AccTaxRate.Types.Rated;
			taxRate1.AT_Code = "TaxRate01";
			var taxMsg1 = Factory.NewWithValidTestData<AccInvMsg>();
			taxMsg1.A9_Code = "TaxMsg01";
			Factory.Save();

			var config = ObjectCreator.CreateTaxIdAndTaxMessageCombinationRulesConfiguration((TransactionLineTypes.Cost, taxRate1, taxMsg1));
			AccountingMasterFilesRegistry.Instance.TaxIdAndTaxMessageCombinationRules.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, config);

			var consol = ObjectCreator.CreateConsol("AUSYD", "NZAKL", "C001001");
			var consolCost = ObjectCreator.CreateConsolCost(consol, ObjectCreator.CC1, 100m);

			consolCost.E6_AT_TaxRate = taxRate1.PK;
			consolCost.E6_A9_VATClass = ZGuid.Empty;
			AssertEquals("Precondition", false, consolCost.IsPosted);

			consolCost.Validation.ValidateE6_A9_VATClass();
			var expectedMsg = @"Transaction cannot be saved/posted due to invalid Tax ID and Tax Message combination.
Line Type=CST, Tax ID=TaxRate01, Tax Message=";
			AssertHasErrors(expectedMsg, consolCost.E6_A9_VATClassInfo);

			consolCost.E6_AH_APInvoice = ZGuid.NewZGuid();
			AssertEquals("Precondition", true, consolCost.IsPosted);

			consolCost.Validation.ValidateE6_A9_VATClass();
			AssertNoErrors(expectedMsg, consolCost.E6_A9_VATClassInfo);
		}

		public void TestCheckE6_PaymentType()
		{
			var expectedError = "To process E-Payments, please create a Payment or Payment Batch in the Payables Transactions module or a Payment Approval in the Payment Processing module.";

			var cost = GetConsolCost();
			cost.E6_PaymentType = ReceiptTypes.Cheque;
			AssertNoErrorContaining(cost.E6_PaymentTypeInfo, expectedError);

			cost.E6_PaymentType = ReceiptTypes.EPayment;
			AssertHasError(cost.E6_PaymentTypeInfo, expectedError);
		}

		public void TestCheckE6_AB_BankAccount()
		{
			var bankAccount = ObjectCreator.CreateBankAccount("ANZ", "Test Bank", ObjectCreator.AUD, ObjectCreator.GLHeader1);
			bankAccount.AB_AccountType = AccountTypeCodeDescriptionPairList.Codes.BNK;
			var ofxAccount = ObjectCreator.CreateBankAccount("OFX", "Test Bank", ObjectCreator.AUD, ObjectCreator.GLHeader2);
			ofxAccount.AB_AccountType = AccountTypeCodeDescriptionPairList.Codes.EPA;
			ofxAccount.AB_PaymentProvider = EPaymentProviderCodes.Codes.OFX;

			var expectedError1 = "This bank account is an E-Payment Account. Please set Payment Type to EPA - E-Payment.";
			var expectedError2 = "Bank Account is not an E-Payment Account.";

			var cost = GetConsolCost();
			cost.E6_PaymentType = ReceiptTypes.Cheque;
			cost.E6_AB_BankAccount = bankAccount.PK;
			cost.RunPreSaveValidation();
			AssertNoErrorContaining(cost.E6_AB_BankAccountInfo, expectedError1);
			AssertNoErrorContaining(cost.E6_AB_BankAccountInfo, expectedError2);

			cost.E6_AB_BankAccount = ofxAccount.PK;
			cost.RunPreSaveValidation();
			AssertHasError(cost.E6_AB_BankAccountInfo, expectedError1);
			AssertNoErrorContaining(cost.E6_AB_BankAccountInfo, expectedError2);

			cost.E6_PaymentType = ReceiptTypes.EPayment;
			cost.E6_AB_BankAccount = bankAccount.PK;
			cost.RunPreSaveValidation();
			AssertNoErrorContaining(cost.E6_AB_BankAccountInfo, expectedError1);
			AssertHasError(cost.E6_AB_BankAccountInfo, expectedError2);

			cost.E6_AB_BankAccount = ofxAccount.PK;
			cost.RunPreSaveValidation();
			AssertNoErrorContaining(cost.E6_AB_BankAccountInfo, expectedError1);
			AssertNoErrorContaining(cost.E6_AB_BankAccountInfo, expectedError2);
		}

		public void TestValidateE6_RX_NKCurrency_CheckForEmpty()
		{
			JobConsolCost cost = GetConsolCost();
			cost.E6_RX_NKCurrency = ObjectCreator.USD.Code;
			cost.E6_RX_NKCurrency = string.Empty;
			AssertHasError(cost.E6_RX_NKCurrencyInfo, "Please enter a Currency.");
			cost.E6_RX_NKCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			AssertNoError(cost.E6_RX_NKCurrencyInfo, "Please enter a Currency.");
		}

		public void TestValidateE6_RX_NKCurrency_CheckForNotValidOnList()
		{
			JobConsolCost cost = GetConsolCost();
			cost.E6_RX_NKCurrency = "1.2";
			AssertHasError(cost.E6_RX_NKCurrencyInfo, "Enter a valid Currency.");
			cost.E6_RX_NKCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			AssertNoError(cost.E6_RX_NKCurrencyInfo, "Enter a valid Currency.");
		}

		public void TestCheckPlaceOfSupply()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.India))
			{
				var cost = GetConsolCost();
				AssertNoErrors(cost.E6_PlaceOfSupplyInfo);

				cost.E6_PlaceOfSupply = "XX";
				AssertHasError(cost.E6_PlaceOfSupplyInfo, "Enter a valid Fixed Place of Supply.");

				cost.E6_PlaceOfSupplyType = PlaceOfSupplyTypes.State.Code;
				cost.E6_PlaceOfSupply = "";
				AssertNoErrors(cost.E6_PlaceOfSupplyInfo);
				Assert("Type must be reset to empty string when Place is set to empty", cost.E6_PlaceOfSupplyType.IsEmpty);
				AssertNoErrors(cost.E6_PlaceOfSupplyTypeInfo);
			}
		}

		public void TestCheckPlaceOfSupplyType()
		{
			using (PlaceOfSupplyHelper.SetPOSTypesEnabled_ForTestOnly(PlaceOfSupplyTypes.State.Code, PlaceOfSupplyTypes.PredefinedRule.Code))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.India))
			{
				var cost = GetConsolCost();
				AssertNoErrors(cost.E6_PlaceOfSupplyTypeInfo);

				cost.E6_PlaceOfSupplyType = "XXX";
				AssertHasError(cost.E6_PlaceOfSupplyTypeInfo, "Enter a valid selection.");

				cost.E6_PlaceOfSupply = "DL";
				AssertEquals(PlaceOfSupplyTypes.State.Code, cost.E6_PlaceOfSupplyType);
				AssertNoErrors("STA is a valid PlaceOfSupplyTypes", cost.E6_PlaceOfSupplyTypeInfo);

				cost.E6_PlaceOfSupplyType = "";
				AssertHasError(cost.E6_PlaceOfSupplyTypeInfo, "Please enter a value.");
				AssertEquals("Place is not reset on resetting Type", "DL", cost.E6_PlaceOfSupply);
				AssertNoErrors(cost.E6_PlaceOfSupplyInfo);
			}
		}

		public void TestAppMethodValidation()
		{
			string warningMessage = @"Apportionment by Capacity Per Container method cannot be performed as no calculation data exist for this cost. This may be because:
 - The billing line was entered manually.
 - The charge code is not FRT.";
			var cost = GetConsolCost();
			cost.E6_ApportionmentMethod = "ZZZ";
			AssertHasError(cost.E6_ApportionmentMethodInfo, "Enter a valid " + cost.E6_ApportionmentMethodInfo.Description + ".");
			cost.E6_ApportionmentMethod = "";
			AssertHasError(cost.E6_ApportionmentMethodInfo, "Please enter an " + cost.E6_ApportionmentMethodInfo.Description + ".");
			cost.E6_ApportionmentMethod = AllocationMethod.Shipment;
			AssertNoError(cost.E6_ApportionmentMethodInfo, "Please enter an " + cost.E6_ApportionmentMethodInfo.Description + ".");
			cost.E6_ApportionmentMethod = AllocationMethod.CapacityPerContainer;
			AssertHasWarning(cost.E6_ApportionmentMethodInfo, warningMessage);
			var paymentBasis = cost.PaymentBases.AddNew();
			paymentBasis.PBS_PerUnitRate = 2.0m;
			paymentBasis.PBS_ChargeableUnit = "20GP";
			paymentBasis.PBS_ChargeableAmount = 1;
			paymentBasis.PBS_ChargeableDescription = "CONT001";
			paymentBasis.PBS_RateUnit = QuantityUnit.CN;
			cost.E6_ApportionmentMethod = AllocationMethod.Shipment;
			cost.E6_ApportionmentMethod = AllocationMethod.CapacityPerContainer;
			AssertNoWarning(cost.E6_ApportionmentMethodInfo, warningMessage);
		}

		public virtual void TestValidateE6_IsForCollectInvoice()
		{
			JobConsolCost cost = GetConsolCost();
			cost.E6_IsForCollectInvoice = true;
			AssertNoErrors(cost.E6_IsForCollectInvoiceInfo);
			cost.E6_IsForCollectInvoice = false;
			AssertNoErrors(cost.E6_IsForCollectInvoiceInfo);
		}

		public void TestCheckE6_AK_ChequeBook()
		{
			string warningSamePrinterMessage = AccChequeBook.WarningChequeBookWithSamePrinterMessageStart + "'Book1'" + AccChequeBook.WarningSamePrinterMessageEnd;
			BusinessObject printer = (BusinessObject)Factory.New<Enterprise.Integration.DocumentEngine.IStmPrintQueue>();
			AccBankAccount testBank = Factory.NewWithValidTestData<AccBankAccount>();
			AccChequeBook chequeBook = Factory.New(typeof(AccChequeBook)) as AccChequeBook;
			chequeBook.AK_AB = testBank.PK;
			chequeBook.AK_GB = Factory.LoadTop1(typeof(GlbBranch), new ZQuery()).PK;
			chequeBook.AK_Code = "Book1";
			chequeBook.AK_AutoPrintCheque = ZBool.True;
			chequeBook.AK_SQ = printer.PK;
			AccChequeBook book2 = Factory.New(typeof(AccChequeBook)) as AccChequeBook;
			book2.AK_AB = testBank.PK;
			book2.AK_GB = chequeBook.AK_GB;
			book2.AK_Code = "Book2";
			book2.AK_AutoPrintCheque = ZBool.True;
			book2.AK_SQ = printer.PK;
			OrgHeader creditor = Factory.NewWithValidTestData<OrgHeader>();
			JobConsolCost testCost = GetConsolCost();
			testCost.E6_AB_BankAccount = book2.AK_AB;
			testCost.E6_AK_ChequeBook = book2.PK;
			Assert("Should have warning about another Cheque Book with the same printer", testCost.E6_AK_ChequeBookInfo.HasWarning(warningSamePrinterMessage));
		}

		public void TestCheckIsFinal()
		{
			bool oldSecurity = Env.Security.AllowPayablesInvoiceFinalFlag.IsAllowed;
			try
			{
				TestObjectCreator creator = new TestObjectCreator(Factory);
				APInvoice invoice = creator.CreateAPInvoice<APInvoice>("00001", creator.AUD, 1m, 100m, 0m, 0m, 100m, 0m, 0m);
				JobConsolCost consolCost = invoice.ConsolCosting.ConsolCosts.AddNew();
				Assert("IsFinal is ticked as default", consolCost.IsFinal);
				JobConsolCostValidation validation = consolCost.Validation;
				Env.Security.AllowPayablesInvoiceFinalFlag.IsAllowed = true;
				validation.ValidateIsFinal();
				AssertNoError(consolCost.IsFinalInfo, @"You do not have appropriate security rights to tick this flag.
Please contact your system administrator for the following security right: Manage > Payables > Payables Transactions > New Transactions > Invoice > Allow Tick Final Flag on Payables Invoice");
				Env.Security.AllowPayablesInvoiceFinalFlag.IsAllowed = false;
				validation.ValidateIsFinal();
				AssertHasError(consolCost.IsFinalInfo, @"You do not have appropriate security rights to tick this flag.
Please contact your system administrator for the following security right: Manage > Payables > Payables Transactions > New Transactions > Invoice > Allow Tick Final Flag on Payables Invoice");
			}
			finally
			{
				Env.Security.ReopenJob.IsAllowed = oldSecurity;
			}
		}

		public void TestCheckGSTInclusiveAmount()
		{
			APInvoice invoice = Factory.New<APInvoice>();
			JobConsolCost testCost = invoice.ConsolCosting.ConsolCosts.AddNew();
			invoice.GSTInclusiveAmounts = false;
			testCost.E6_AT_TaxRate = ObjectCreator.CreateTaxRate("TestGST", "", 10).PK;
			testCost.E6_RX_NKCurrency = ObjectCreator.AUD.RX_Code;
			testCost.E6_OSCostAmount = 120;
			testCost.GSTInclusiveAmount = 1;
			JobConsolCostValidation testValidation = testCost.Validation;
			testValidation.ValidateGSTInclusiveAmount();
			Assert("GSTInclusiveAmount shouldn't have errors.", !testCost.GSTInclusiveAmountInfo.HasErrors());
			invoice.GSTInclusiveAmounts = true;
			testCost.GSTInclusiveAmount = 11;
			testCost.E6_OSCostAmount = 120;
			testValidation.ValidateGSTInclusiveAmount();
			Assert("GSTInclusiveAmount should have errors.", testCost.GSTInclusiveAmountInfo.HasErrors());
			testCost.E6_OSCostAmount = 10;
			testValidation.ValidateGSTInclusiveAmount();
			Assert("GSTInclusiveAmount shouldn't have errors.", !testCost.GSTInclusiveAmountInfo.HasErrors());
			testCost.GSTInclusiveAmount = 333;
			testValidation.ValidateGSTInclusiveAmount();
			Assert("GSTInclusiveAmount shouldn't have errors.", !testCost.GSTInclusiveAmountInfo.HasErrors());
		}

		public void TestCriticalValidationDoesNotCheckAmountsOnValidateAll()
		{
			JobConsolCost testCost = Factory.NewWithValidTestData<JobConsolCost>();
			ApportionSplitCharge charge1 = testCost.ApportionmentCharges.AddNew();
			ApportionSplitCharge charge2 = testCost.ApportionmentCharges.AddNew();
			charge1.JR_OSCostAmt = 100;
			charge2.JR_OSCostAmt = -500;
			AssertEquals("JobConsolCost OSCostAmount should not match the sum of apportionment", 0m, testCost.E6_OSCostAmount);
			testCost.Validation.ValidateAll();
			AssertNoRowErrors(testCost);
		}

		public void TestInvalidDecimalPlacesShouldBeCaughtByValidationRatherThanCriticalValidation()
		{
			ErrorReporter.Clear();
			var testCost = Factory.NewWithValidTestData<JobConsolCost>();
			var charge1 = testCost.ApportionmentCharges.AddNew();
			testCost.E6_RX_NKCurrency = "USD";
			charge1.JR_OSCostAmt = 100m;
			charge1.JR_LocalCostAmt = 100m;
			testCost.E6_LocalCostAmount = 100.001m;
			testCost.E6_OSCostAmount = 100m;
			testCost.Validation.ValidateAll();
			if (!testCost.E6_LocalCostAmountInfo.HasErrors())
			{
				Factory.Save();
			}

			AssertEquals("No Developer Error Expected when invalid decimal places", true, string.IsNullOrEmpty(ErrorReporter.LastMessageReported));
		}

		public void TestInvalidLocalCostDecimalPlaces()
		{
			var originalLocalCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			try
			{
				GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency = "AUD";
				var testCost = Factory.NewWithValidTestData<JobConsolCost>();
				var charge1 = testCost.ApportionmentCharges.AddNew();
				testCost.E6_RX_NKCurrency = "USD";
				charge1.JR_OSCostAmt = 100m;
				charge1.JR_LocalCostAmt = 100m;
				testCost.E6_LocalCostAmount = 100.001m;
				testCost.E6_OSCostAmount = 100m;
				AssertHasErrorContaining(testCost.E6_LocalCostAmountInfo, "You have entered 3 decimal places for the local cost amount. The AUD currency only allows entering amounts up to 2 decimal places.");
			}
			finally
			{
				GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency = originalLocalCurrency;
			}
		}

		public void TestNoRowErrorIfConsolDoesNotHaveShipmentForAllApportionedCharges()
		{
			ForwardingConsol consol = Factory.NewWithValidTestData<ForwardingConsol>();
			Factory.Save();
			Job job = Factory.NewJobWithValidTestDataForTesting<Job>();
			ForwardingShipment shipment = consol.Shipments.AddNew();
			job.JH_ParentID = shipment.PK;
			job.JH_ParentTableCode = "JS";
			AssertNotNull("Shipment should have a Job", shipment.Job);
			JobConsolCost cost = consol.GetApportionments().CostsCollection.TryAddNew();
			cost.FillWithValidTestData();
			ApportionSplitCharge charge = cost.ApportionmentCharges.AddNew();
			charge.JR_JH = job.PK;
			charge.JR_OSCostAmt = 100m;
			charge.JR_OSCostExRate = 1;
			AssertNotNull("Charge should have a Job", charge.Job);
			cost.UpdateShipmentInfosOnCharges();
			AssertNotNull("Charge should have a ShipmentInfo", charge.ShipmentInfo);
			cost.Validation.ValidateAll();
			AssertNoRowErrors("Cost should have no errors", cost);
			shipment.Job.Delete();
			consol.Shipments.Remove(shipment);
			charge.SetShipmentInfo(null);
			cost.UpdateShipmentInfosOnCharges();
			AssertNull("No ShipmentInfo should be assigened after update", charge.ShipmentInfo);
			cost.Validation.ValidateAll();
			AssertNoRowErrors("Cost should have no errors", cost);
		}

		public void TestCheckUnApportionedAmount()
		{
			var consol = ObjectCreator.CreateConsol("AUD", "LAX", "C0001");
			var shipment1 = ObjectCreator.CreateShipment("S0001", consol);
			var job1 = ObjectCreator.CreateJob(shipment1, false);
			var consolCost = ObjectCreator.CreateConsolCost(consol, ObjectCreator.CC12, ObjectCreator.AALSHI);
			consolCost.E6_RX_NKCurrency = "AUD";
			consolCost.E6_OSCostAmount = 100M;
			consolCost.E6_AT_TaxRate = ObjectCreator.GST1.PK;
			consolCost.E6_ApportionmentMethod = AllocationMethod.Shipment;
			consolCost.E6_InvoiceNum = "ABC123";
			consolCost.E6_InvoiceDate = ZDateTime.Now;
			consolCost.E6_PaymentType = ReceiptTypes.Cheque;
			consolCost.E6_AB_BankAccount = ObjectCreator.AUDBankAccount.PK;
			consolCost.E6_AK_ChequeBook = ObjectCreator.AUDChequeBook.PK;
			AssertEquals("precondition", consolCost.ApportionmentCharges[0].JR_OSCostAmt, 100M);

			consolCost.RunPreSaveValidation();
			AssertNoErrors(consolCost);
			AssertNoErrors(consolCost.UnApportionedAmountInfo);

			consolCost.ApportionmentCharges[0].JR_OSCostAmt = 90M;
			consolCost.RunPreSaveValidation();
			AssertHasErrors(consolCost.UnApportionedAmountInfo);
			AssertEquals("UnApportionedAmount must have 1 error only.", 1, consolCost.UnApportionedAmountInfo.Notifications.Count());
			AssertHasError(consolCost.UnApportionedAmountInfo, "Please ensure that this Cost Amount is fully apportioned.");

			var consolAPInvoice = Factory.New<APInvoice>();
			consolAPInvoice.AH_TransactionNum = "1111";
			consolCost.E6_AH_APInvoice = consolAPInvoice.PK;
			consolCost.RunPreSaveValidation();
			AssertEquals("UnApportionedAmount must have 2 errors only.", 2, consolCost.UnApportionedAmountInfo.Notifications.Count());
			AssertHasError(consolCost.UnApportionedAmountInfo, "Please ensure that this Cost Amount is fully apportioned.");
			AssertHasError(consolCost.UnApportionedAmountInfo, "Please close the form and try again.");
		}

		public void TestCheckUnApportionedAmount_DifferentSign()
		{
			var consol = Factory.New<ForwardingConsol>();
			for (int i = 0; i < 2; i++)
			{
				consol.Shipments.AddNew();
			}

			Factory.Save();
			var listing = new ApportionmentListing(Factory, consol);
			try
			{
				var consolCost = listing.CostsCollection.TryAddNew();
				consolCost.E6_AC_ChargeCode = Env.Registry.FreightChargeCode;
				consolCost.E6_OSCostAmount = 10m;
				consolCost.CostExchangeRate.Currency = Enterprise.Core.Constants.CurrencyCodes.UnitedStates;
				consolCost.CostExchangeRate.Rate = 1m;
				//Manual
				consolCost.E6_ApportionmentMethod = AllocationMethod.Manual;
				consolCost.ApportionmentCharges[0].JR_OSCostAmt = 20m;
				consolCost.ApportionmentCharges[1].JR_OSCostAmt = -10m;
				consolCost.RunPreSaveValidation();
				AssertNoErrors("Shouldn't have any error while using Manual method.", consolCost.UnApportionedAmountInfo);
				//Shipment
				consolCost.E6_ApportionmentMethod = AllocationMethod.Shipment;
				consolCost.ApportionmentCharges[0].JR_OSCostAmt = 20m;
				consolCost.ApportionmentCharges[1].JR_OSCostAmt = -10m;
				consolCost.RunPreSaveValidation();
				AssertHasError("Should have an error while NOT using Manual method.", consolCost.UnApportionedAmountInfo, "Using different signs on Amounts is allowed only for MAN apportionment method.");
				using (AccountingConfigurationRegistry.Instance.CheckDifferentSignsWhenApportionConsolCost.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, false))
				{
					consolCost.RunPreSaveValidation();
					AssertNoErrors("Shouldn't have any error while registry set to No.", consolCost.UnApportionedAmountInfo);
				}
			}
			finally
			{
				listing.ReleaseMutexes();
			}
		}

		public void TestCheckUnApportionedAmount_DifferentSignWithPostedConsolCost()
		{
			var consol = Factory.New<ForwardingConsol>();
			for (int i = 0; i < 2; i++)
			{
				consol.Shipments.AddNew();
			}

			var apInvoice = Factory.NewWithValidTestData<APInvoice>();
			Factory.Save();
			var listing = new ApportionmentListing(Factory, consol);
			try
			{
				var consolCost = listing.CostsCollection.TryAddNew();
				consolCost.E6_AC_ChargeCode = Env.Registry.FreightChargeCode;
				consolCost.E6_OSCostAmount = 10m;
				consolCost.CostExchangeRate.Currency = Enterprise.Core.Constants.CurrencyCodes.UnitedStates;
				consolCost.CostExchangeRate.Rate = 1m;
				//Manual
				consolCost.E6_ApportionmentMethod = AllocationMethod.Manual;
				consolCost.ApportionmentCharges[0].JR_OSCostAmt = 20m;
				consolCost.ApportionmentCharges[1].JR_OSCostAmt = -10m;
				consolCost.E6_AH_APInvoice = ZGuid.Empty;
				Assert(!consolCost.IsPosted);
				consolCost.MarkAsNeedingValidationIncludingChildren();
				consolCost.RunPreSaveValidation();
				AssertNoErrors(consolCost.UnApportionedAmountInfo);
				consolCost.E6_AH_APInvoice = apInvoice.PK;
				Assert(consolCost.IsPosted);
				consolCost.MarkAsNeedingValidationIncludingChildren();
				consolCost.RunPreSaveValidation();
				AssertNoErrors(consolCost.UnApportionedAmountInfo);
				//Shipment
				consolCost.E6_ApportionmentMethod = AllocationMethod.Shipment;
				consolCost.ApportionmentCharges[0].JR_OSCostAmt = 20m;
				consolCost.ApportionmentCharges[1].JR_OSCostAmt = -10m;
				consolCost.E6_AH_APInvoice = ZGuid.Empty;
				Assert(!consolCost.IsPosted);
				consolCost.MarkAsNeedingValidationIncludingChildren();
				consolCost.RunPreSaveValidation();
				AssertHasError(consolCost.UnApportionedAmountInfo, "Using different signs on Amounts is allowed only for MAN apportionment method.");
				consolCost.E6_AH_APInvoice = apInvoice.PK;
				Assert(consolCost.IsPosted);
				consolCost.MarkAsNeedingValidationIncludingChildren();
				consolCost.RunPreSaveValidation();
				AssertNoErrors("should not validate when posted", consolCost.UnApportionedAmountInfo);
				using (AccountingConfigurationRegistry.Instance.CheckDifferentSignsWhenApportionConsolCost.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, false))
				{
					consolCost.E6_AH_APInvoice = ZGuid.Empty;
					Assert(!consolCost.IsPosted);
					consolCost.MarkAsNeedingValidationIncludingChildren();
					consolCost.RunPreSaveValidation();
					AssertNoErrors("Shouldn't have any error while registry set to No.", consolCost.UnApportionedAmountInfo);
				}
			}
			finally
			{
				listing.ReleaseMutexes();
			}
		}

		public void TestCheckE6_AC_ChargeCode_ChargeCodeIsNotConsolLevelCharge_AddWarning()
		{
			var chargeCode = Factory.New<AccChargeCode>();
			chargeCode.AC_Code = "SSS";
			var consolCost = Factory.New<JobConsolCost>();
			consolCost.E6_AC_ChargeCode = chargeCode.PK;
			chargeCode.AC_IsGroupageCharge = true;
			consolCost.RunPreSaveValidation();
			AssertNoWarnings(consolCost.E6_AC_ChargeCodeInfo);
			chargeCode.AC_IsGroupageCharge = false;
			consolCost.RunPreSaveValidation();
			AssertHasWarning(consolCost.E6_AC_ChargeCodeInfo, "The SSS code is not a Consol Level Charge. Creditor only defaults for Consol Level charges.");
		}

		public void TestCheckE6_PaymentType_WithMatchedJournal()
		{
			var journal = Factory.NewWithValidTestData<APJournal>();
			journal.AH_TransactionCategory = TransactionCategory.Codes.TransactionNotFound;
			journal.AH_OH = ObjectCreator.AALSHI.PK;
			journal.AH_ChequeOrReference = "T001";
			journal.AH_InvoiceAmount = 200;
			journal.AH_OutstandingAmount = 200;
			journal.AH_OSTotal = 200;
			Factory.Save();
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			var shipment = consol.Shipments.AddNew();
			var job = ObjectCreator.CreateJob(shipment, false);
			Factory.Save();
			var cost = ObjectCreator.CreateConsolCost(consol, ObjectCreator.CC1, ObjectCreator.AALSHI);
			cost.E6_OSCostAmount = 250m;
			cost.ApportionmentCharges[0].JR_OSCostAmt = 250m;
			cost.E6_InvoiceNum = "T001";
			cost.E6_PaymentType = ZArchitecture.Core.ReceiptTypes.Cash;
			var expectedErrorMessage = string.Format("Payment details cannot be entered as this AP Invoice Number matches an unpaid �Carried Forward� journal�s payment reference. When this invoice is posted, it will automatically be matched against the journal, up to the value of the invoice. The unpaid journal transaction number is [{0}].", cost.MatchedWithTNFJournalNum);
			AssertHasError("should found the matched journal", cost.E6_PaymentTypeInfo, expectedErrorMessage);
			cost.E6_InvoiceNum = "INV100";
			AssertNoErrors("should not found the matched journal", cost.E6_PaymentTypeInfo);
		}

		public void TestValidateE6_CostGovtChargeCode()
		{
			foreach (var enableGovtChargeCode in new[] { true, false })
			{
				using (AccountingMasterFilesRegistry.Instance.EnableGovernmentChargeCode.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, enableGovtChargeCode))
				{
					var expectedErrorMessage = "Please enter a value.";
					var expectedWarningMessage = "Cost Government Charge Code is empty.";
					var govtChargeCode = "GVTCC1";
					JobConsolCost cost = Factory.NewWithValidTestData<JobConsolCost>();
					//set up cost
					cost.E6_AC_ChargeCode = ObjectCreator.CC1.PK;
					cost.E6_AT_TaxRate = ObjectCreator.GST1.PK;
					cost.E6_CostGovtChargeCode = govtChargeCode;
					AssertEquals(govtChargeCode, cost.E6_CostGovtChargeCode);
					AssertNoErrors(cost.E6_CostGovtChargeCodeInfo);
					cost.E6_CostGovtChargeCode = "";
					if (enableGovtChargeCode)
					{
						AssertHasError(cost.E6_CostGovtChargeCodeInfo, expectedErrorMessage);
						var apInvoice = Factory.NewWithValidTestData<APInvoice>();
						cost.E6_AH_APInvoice = apInvoice.PK;
						Assert("Precondition: IsPosted is true", cost.IsPosted);
						cost.Validation.ValidateE6_CostGovtChargeCode();
						AssertNoError("Field is optional when cost posted", cost.E6_CostGovtChargeCodeInfo, expectedErrorMessage);
						cost.E6_AH_APInvoice = ZGuid.Empty; //revert to previous condition
						cost.Validation.ValidateE6_CostGovtChargeCode();
						AssertHasError(cost.E6_CostGovtChargeCodeInfo, expectedErrorMessage);
						var previousTaxId = cost.E6_AT_TaxRate;
						cost.E6_AT_TaxRate = ZGuid.Empty;
						AssertNoError("Field is optional when no tax is associated", cost.E6_CostGovtChargeCodeInfo, expectedErrorMessage);
						AssertHasWarning(cost.E6_CostGovtChargeCodeInfo, expectedWarningMessage);
						cost.E6_AT_TaxRate = previousTaxId; //revert to previous condition
						AssertHasError(cost.E6_CostGovtChargeCodeInfo, expectedErrorMessage);
						using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.India))
						{
							cost.E6_AT_TaxRate = ObjectCreator.ServiceTax.PK;
							AssertNoError("Field is optional when tax is india service tax", cost.E6_CostGovtChargeCodeInfo, expectedErrorMessage);
							AssertHasWarning(cost.E6_CostGovtChargeCodeInfo, expectedWarningMessage);
							cost.E6_AT_TaxRate = previousTaxId; //revert to previous condition
							AssertHasError(cost.E6_CostGovtChargeCodeInfo, expectedErrorMessage);
							cost.E6_AT_TaxRate = ObjectCreator.ExtraServiceTax.PK;
							AssertNoError("Field is optional when tax is india service tax", cost.E6_CostGovtChargeCodeInfo, expectedErrorMessage);
							AssertHasWarning(cost.E6_CostGovtChargeCodeInfo, expectedWarningMessage);
						}
					}
					else
					{
						AssertNoErrors(cost.E6_CostGovtChargeCodeInfo);
					}
				}
			}
		}

		public void TestValidateE6_SellGovtChargeCode()
		{
			foreach (var enableGovtChargeCode in new[] { true, false })
			{
				using (AccountingMasterFilesRegistry.Instance.EnableGovernmentChargeCode.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, enableGovtChargeCode))
				{
					var expectedErrorMessage = "Please enter a value.";
					var expectedWarningMessage = "Sell Government Charge Code is empty.";
					var govtChargeCode = "GVTCC1";
					JobConsolCost cost = Factory.NewWithValidTestData<JobConsolCost>();
					//set up cost
					cost.E6_AC_ChargeCode = ObjectCreator.CC1.PK;
					cost.E6_AT_TaxRate = ObjectCreator.GST1.PK;
					cost.E6_SellGovtChargeCode = govtChargeCode;
					AssertEquals(govtChargeCode, cost.E6_SellGovtChargeCode);
					AssertNoErrors(cost.E6_SellGovtChargeCodeInfo);
					cost.E6_SellGovtChargeCode = "";
					if (enableGovtChargeCode)
					{
						AssertHasError(cost.E6_SellGovtChargeCodeInfo, expectedErrorMessage);
						var previousTaxId = cost.E6_AT_TaxRate;
						cost.E6_AT_TaxRate = ZGuid.Empty;
						AssertNoError("Field is optional when no tax is associated", cost.E6_SellGovtChargeCodeInfo, expectedErrorMessage);
						AssertHasWarning(cost.E6_SellGovtChargeCodeInfo, expectedWarningMessage);
						cost.E6_AT_TaxRate = previousTaxId; //revert to previous condition
						AssertHasError(cost.E6_SellGovtChargeCodeInfo, expectedErrorMessage);
						using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.India))
						{
							cost.E6_AT_TaxRate = ObjectCreator.ServiceTax.PK;
							AssertNoError("Field is optional when tax is india service tax", cost.E6_SellGovtChargeCodeInfo, expectedErrorMessage);
							AssertHasWarning(cost.E6_SellGovtChargeCodeInfo, expectedWarningMessage);
							cost.E6_AT_TaxRate = previousTaxId; //revert to previous condition
							AssertHasError(cost.E6_SellGovtChargeCodeInfo, expectedErrorMessage);
							cost.E6_AT_TaxRate = ObjectCreator.ExtraServiceTax.PK;
							AssertNoError("Field is optional when tax is india service tax", cost.E6_SellGovtChargeCodeInfo, expectedErrorMessage);
							AssertHasWarning(cost.E6_SellGovtChargeCodeInfo, expectedWarningMessage);
						}
					}
					else
					{
						AssertNoErrors(cost.E6_SellGovtChargeCodeInfo);
					}
				}
			}
		}

		public void TestValidateE6_SellGovtChargeCode_WhenConsolCostPosted_RegistryEnabled()
		{
			AssertValidateE6_SellGovtChargeCode_ConsolCostPosted(true);
		}

		public void TestValidateE6_SellGovtChargeCode_WhenConsolCostPosted_RegistryDisabled()
		{
			AssertValidateE6_SellGovtChargeCode_ConsolCostPosted(false);
		}

		void AssertValidateE6_SellGovtChargeCode_ConsolCostPosted(bool isRegistryEnabled)
		{
			AccountingMasterFilesRegistry.Instance.EnableGovernmentChargeCode.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, isRegistryEnabled);
			const string errorMessage = "Please enter a value.";

			var consolCost = GetConsolCost();
			consolCost.FillWithValidTestData();
			consolCost.E6_AT_TaxRate = ObjectCreator.GST1.PK;
			consolCost.E6_SellGovtChargeCode = string.Empty;
			Factory.Save();

			consolCost.Validation.ValidateE6_SellGovtChargeCode();
			if (isRegistryEnabled)
			{
				AssertHasError(consolCost.E6_SellGovtChargeCodeInfo, errorMessage);
			}
			else
			{
				AssertNoErrors(consolCost.E6_SellGovtChargeCodeInfo);
			}

			consolCost.E6_AH_APInvoice = Factory.NewWithValidTestData<AccTransactionHeader>().PK;
			Factory.Save();
			AssertEquals("Pre-condition", true, consolCost.IsPosted);

			consolCost.Validation.ValidateE6_SellGovtChargeCode();
			AssertNoErrors(consolCost.E6_SellGovtChargeCodeInfo);
		}

		[TestDate(2017, 12, 4)]
		public void TestCheckE6_InvoiceNumForExistingAPInvoice_Standard()
		{
			AssertCheckE6_InvoiceNumForExistingAPInvoice(
				AccountingMasterFilesConstants.AllowDuplicateInvoiceNumberRule.STD,
				invoiceDate1: ZDateTime.Today,
				invoiceDate2: ZDateTime.Now.AddMonths(AccountingUtils.DuplicateInvoiceNumberPeriodMonths));
		}

		[TestDate(2017, 12, 4)]
		public void TestCheckE6_InvoiceNumForExistingAPInvoice_Calendar()
		{
			AssertCheckE6_InvoiceNumForExistingAPInvoice(
				AccountingMasterFilesConstants.AllowDuplicateInvoiceNumberRule.CAL,
				invoiceDate1: ZDateTime.Today,
				invoiceDate2: new ZDateTime(ZDateTime.Today.Year + 1, 1, 1));
		}

		void AssertCheckE6_InvoiceNumForExistingAPInvoice(string allowDuplicateInvoiceNumberRule, ZDateTime invoiceDate1, ZDateTime invoiceDate2)
		{
			string expInvoiceNumberDuplicateMessageNoPerm = CreateDuplicateNumberErrorMessageNoPermission(allowDuplicateInvoiceNumberRule);
			string expInvoiceNumberDuplicateMessageWithPerm = CreateDuplicateNumberMessageWithPermission(allowDuplicateInvoiceNumberRule);

			using (AccountingMasterFilesRegistry.Instance.AllowDuplicateInvoiceNumberDefaultingRule.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, allowDuplicateInvoiceNumberRule))
			{
				var existingAPInvoiceNum = "TESTAP5787";
				var existingInvoice = ObjectCreator.CreateAPInvoice<APInvoice>(existingAPInvoiceNum, ObjectCreator.AUD, 1m, 100m, 0m, 0m, 100m, 0m, 0m, ObjectCreator.AALSHI);
				existingInvoice.AH_InvoiceDate = invoiceDate1;
				Factory.Save();

				var consol = ObjectCreator.CreateConsol("AUD", "LAX", "C0001");
				var shipment1 = ObjectCreator.CreateShipment("S0001", consol);
				var job1 = ObjectCreator.CreateJob(shipment1, false);
				var consolCost = ObjectCreator.CreateConsolCost(consol, ObjectCreator.CC1, ObjectCreator.AALSHI);
				consolCost.E6_RX_NKCurrency = "AUD";
				consolCost.E6_OSCostAmount = 100M;
				consolCost.E6_ApportionmentMethod = AllocationMethod.Shipment;
				consolCost.E6_InvoiceDate = ZDateTime.Empty;
				consolCost.Validation.ValidateE6_InvoiceNum();
				AssertNoErrors(consolCost.E6_InvoiceNumInfo);

				consolCost.E6_InvoiceNum = existingAPInvoiceNum;
				AssertHasError(consolCost.E6_InvoiceNumInfo, "The transaction number is already in use. Cannot check if the duplicate transaction number is allowed as the Invoice Date is empty.");

				consolCost.E6_InvoiceDate = invoiceDate2;
				consolCost.Validation.ValidateE6_InvoiceNum();
				AssertHasWarning(consolCost.E6_InvoiceNumInfo, expInvoiceNumberDuplicateMessageWithPerm);

				Env.Security.NewPayablesDuplicateInvoiceNumber.IsAllowed = false;
				consolCost.Validation.ValidateE6_InvoiceNum();
				AssertHasError(consolCost.E6_InvoiceNumInfo, expInvoiceNumberDuplicateMessageNoPerm);
			}

			ZString CreateDuplicateNumberErrorMessageNoPermission(string ruleCode)
			{
				var messageDetails = ruleCode == AccountingMasterFilesConstants.AllowDuplicateInvoiceNumberRule.STD ? "at least 12 months apart" : "in another calendar year";
				return ZString.Format("The transaction number is already in use. Last posted transaction’s invoice date is 04-Dec-17 which is {0}. You are not granted security right to Payables > Payables Transactions > New Transactions > Allow Duplicate AP Invoice Number. This transaction number cannot be used. Please enter another one.", messageDetails);
			}

			ZString CreateDuplicateNumberMessageWithPermission(string ruleCode)
			{
				var messageDetails = ruleCode == AccountingMasterFilesConstants.AllowDuplicateInvoiceNumberRule.STD ? "at least 12 months apart" : "in another calendar year";
				return ZString.Format("The transaction number is already in use. Last posted transaction’s invoice date is 04-Dec-17 which is {0}. You are granted security right to Payables > Payables Transactions > New Transactions > Allow Duplicate AP Invoice Number. This transaction number can be used.", messageDetails);
			}
		}

		[TestDate(2017, 12, 4)]
		public void TestCheckE6_InvoiceNumForExistingUAInvoice_Standard()
		{
			AssertCheckE6_InvoiceNumForExistingUAInvoice(
				AccountingMasterFilesConstants.AllowDuplicateInvoiceNumberRule.STD,
				invoiceDate1: ZDateTime.Today,
				invoiceDate2: ZDateTime.Today.AddMonths(AccountingUtils.DuplicateInvoiceNumberPeriodMonths));
		}

		[TestDate(2017, 12, 4)]
		public void TestCheckE6_InvoiceNumForExistingUAInvoice_Calendar()
		{
			AssertCheckE6_InvoiceNumForExistingUAInvoice(
				AccountingMasterFilesConstants.AllowDuplicateInvoiceNumberRule.CAL,
				ZDateTime.Today,
				ZDateTime.Today.AddMonths(AccountingUtils.DuplicateInvoiceNumberPeriodMonths));
		}

		void AssertCheckE6_InvoiceNumForExistingUAInvoice(string allowDuplicateInvoiceNumberRule, ZDateTime invoiceDate1, ZDateTime invoiceDate2)
		{
			string expInvoiceNumberDuplicateMessageNoPerm = CreateDuplicateNumberErrorMessageNoPermission(allowDuplicateInvoiceNumberRule);
			string expInvoiceNumberDuplicateMessageWarn = CreateDuplicateNumberMessageWithPermission(allowDuplicateInvoiceNumberRule);

			using (AccountingMasterFilesRegistry.Instance.AllowDuplicateInvoiceNumberDefaultingRule.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, allowDuplicateInvoiceNumberRule))
			{
				var existingAPInvoiceNum = "TESTAP5787";
				var existingInvoice = ObjectCreator.CreateAPInvoice<UAInvoice>(existingAPInvoiceNum, ObjectCreator.AUD, 1m, 100m, 0m, 0m, 100m, 0m, 0m, ObjectCreator.AALSHI);
				existingInvoice.AH_InvoiceDate = invoiceDate1;
				Factory.Save();

				var consol = ObjectCreator.CreateConsol("AUD", "LAX", "C0001");
				var shipment1 = ObjectCreator.CreateShipment("S0001", consol);
				var job1 = ObjectCreator.CreateJob(shipment1, false);
				var consolCost = ObjectCreator.CreateConsolCost(consol, ObjectCreator.CC1, ObjectCreator.AALSHI);
				consolCost.E6_RX_NKCurrency = "AUD";
				consolCost.E6_OSCostAmount = 100M;
				consolCost.E6_ApportionmentMethod = AllocationMethod.Shipment;
				consolCost.E6_InvoiceDate = ZDateTime.Empty;
				consolCost.Validation.ValidateE6_InvoiceNum();
				AssertNoErrors(consolCost.E6_InvoiceNumInfo);

				consolCost.E6_InvoiceNum = existingAPInvoiceNum;
				AssertHasError(consolCost.E6_InvoiceNumInfo, "The transaction number is already in use by Unapproved Invoice. Cannot check if the duplicate transaction number is allowed as the Invoice Date is empty.");

				consolCost.E6_InvoiceDate = invoiceDate2;
				consolCost.Validation.ValidateE6_InvoiceNum();
				AssertHasWarning(consolCost.E6_InvoiceNumInfo, expInvoiceNumberDuplicateMessageWarn);

				Env.Security.NewPayablesDuplicateInvoiceNumber.IsAllowed = false;
				consolCost.Validation.ValidateE6_InvoiceNum();
				AssertHasError(consolCost.E6_InvoiceNumInfo, expInvoiceNumberDuplicateMessageNoPerm);
			}

			ZString CreateDuplicateNumberErrorMessageNoPermission(string ruleCode)
			{
				var messageDetails = ruleCode == AccountingMasterFilesConstants.AllowDuplicateInvoiceNumberRule.STD ? "at least 12 months apart" : "in another calendar year";
				return ZString.Format("The transaction number is already in use by Unapproved Invoice. Last posted transaction’s invoice date is 04-Dec-17 which is {0}. You are not granted security right to Payables > Payables Transactions > New Transactions > Allow Duplicate AP Invoice Number. This transaction number cannot be used. Please enter another one.", messageDetails);
			}

			ZString CreateDuplicateNumberMessageWithPermission(string ruleCode)
			{
				var messageDetails = ruleCode == AccountingMasterFilesConstants.AllowDuplicateInvoiceNumberRule.STD ? "at least 12 months apart" : "in another calendar year";
				return ZString.Format("The transaction number is already in use by Unapproved Invoice. Last posted transaction’s invoice date is 04-Dec-17 which is {0}. You are granted security right to Payables > Payables Transactions > New Transactions > Allow Duplicate AP Invoice Number. This transaction number can be used.", messageDetails);
			}
		}

		public void TestCheckE6_TaxDate()
		{
			var consol = ObjectCreator.CreateConsol();
			var taxRate = CreateTaxRate();
			var chargeCode = ObjectCreator.CreateChargeCode("GSTCC");
			chargeCode.AC_AT_GSTRate = taxRate.PK;
			chargeCode.AC_GC = GlbCompany.CurrentCompany.PK;
			const string errorMessage = "No rate found for selected date.";

			var consolCost = ObjectCreator.CreateConsolCost(consol, chargeCode);
			consolCost.E6_AT_TaxRate = taxRate.PK;
			consolCost.E6_TaxDate = ZDate.Today;
			AssertNoError(consolCost.E6_TaxDateInfo, errorMessage);

			consolCost.E6_TaxDate = ZDate.Today.AddDays(5);
			AssertHasError(consolCost.E6_TaxDateInfo, errorMessage);

			consolCost.E6_TaxDate = ZDate.Today.AddDays(1);
			AssertNoError(consolCost.E6_TaxDateInfo, errorMessage);

			consolCost.E6_TaxDate = ZDate.Today.AddDays(5);
			consolCost.E6_AH_APInvoice = Factory.New<APInvoice>().PK;
			Assert("Precondition: IsPosted", consolCost.IsPosted);
			consolCost.Validation.ValidateE6_TaxDate();
			AssertNoError(consolCost.E6_TaxDateInfo, errorMessage);

			consolCost.E6_AH_APInvoice = ZGuid.Empty;
			Assert("Precondition: IsPosted", !consolCost.IsPosted);
			consolCost.Validation.ValidateE6_TaxDate();
			AssertHasError(consolCost.E6_TaxDateInfo, errorMessage);

			consolCost.E6_AT_TaxRate = ZGuid.Invalid;
			AssertNoError(consolCost.E6_TaxDateInfo, errorMessage);

			AccTaxRate CreateTaxRate()
			{
				var rate = AccTaxRate.CreateTaxRate_ForTestOnly(Factory);
				rate.SetRate_ForTestOnly(10, 1, ZDate.Today.AddDays(-1), ZDate.Today);
				rate.SetRate_ForTestOnly(18, 6, ZDate.Today.AddDays(1), ZDate.Today.AddDays(2));
				return rate;
			}
		}

		public void TestCheckE6_PPDCLT_PrepaidCollectFilters()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			Factory.Save();
			var shipment1 = AddShipmentToConsolWithJob(consol);
			var shipment2 = AddShipmentToConsolWithJob(consol);
			var cost = consol.GetApportionments().CostsCollection.TryAddNew();
			cost.FillWithValidTestData();

			var prepaidInco = "CFR";
			var collectInco = "EXW";

			AssertPrepaidCollectFilters(PrepaidCollectCodes.PPD, collectInco);
			AssertPrepaidCollectFilters(PrepaidCollectCodes.CCX, prepaidInco);

			void AssertPrepaidCollectFilters(string valueToTest, string incoForAllShipments)
			{
				AssertEquals("Precondition: E6_PPDCLT is ALL by default", "ALL", cost.E6_PPDCLT);
				shipment1.JS_INCO = prepaidInco;
				shipment2.JS_INCO = collectInco;
				cost.Validation.ValidateE6_PPDCLT();
				AssertNoErrors("Precondition: no errors before validation tests", cost);
				cost.E6_PPDCLT = valueToTest;
				cost.Validation.ValidateE6_PPDCLT();
				AssertNoError(cost.E6_PPDCLTInfo, $"Apportionment Filter - There are no jobs matching '{valueToTest}'. Select another filter to apply apportionment to.");
				shipment1.JS_INCO = incoForAllShipments;
				shipment2.JS_INCO = incoForAllShipments;
				cost.Validation.ValidateE6_PPDCLT();
				AssertHasError(cost.E6_PPDCLTInfo, $"Apportionment Filter - There are no jobs matching '{valueToTest}'. Select another filter to apply apportionment to.");
				cost.E6_PPDCLT = PrepaidCollectCodes.All;
				cost.Validation.ValidateE6_PPDCLT();
				AssertNoErrors(cost.E6_PPDCLTInfo);
			}
		}

		public void TestCheckE6_PPDCLT_OriginDestinationFilters()
		{
			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = "AU";
			GlbCompany.CurrentCompany.OrgProxy.OH_RL_NKClosestPort = "USLAX";
			AssertNotEquals("When OrgProxy and GlbCompany countries are different, GlbCompany should be used to determine local-ness or foreign-ness.", GlbCompany.CurrentCompany.Country.Code, GlbCompany.CurrentCompany.OrgProxy.Country);

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			Factory.Save();
			var shipment1 = AddShipmentToConsolWithJob(consol);
			var shipment2 = AddShipmentToConsolWithJob(consol);
			var cost = consol.GetApportionments().CostsCollection.TryAddNew();
			cost.FillWithValidTestData();

			AssertOriginDestinationFilters(PrepaidCollectCodes.LOG, "NZCHC", null, "local origin");
			AssertOriginDestinationFilters(PrepaidCollectCodes.FOG, "AUMEL", null, "foreign origin");
			AssertOriginDestinationFilters(PrepaidCollectCodes.LDT, null, "NZCHC", "local destination");
			AssertOriginDestinationFilters(PrepaidCollectCodes.FDT, null, "AUMEL", "foreign destination");

			AssertOriginDestinationFilters(PrepaidCollectCodes.LOG, "NZCHC", null, "local origin");
			AssertOriginDestinationFilters(PrepaidCollectCodes.FOG, "AUMEL", null, "foreign origin");
			AssertOriginDestinationFilters(PrepaidCollectCodes.LDT, null, "NZCHC", "local destination");
			AssertOriginDestinationFilters(PrepaidCollectCodes.FDT, null, "AUMEL", "foreign destination");

			AssertOriginDestinationFilters(PrepaidCollectCodes.LOG, "USLAX", null, "local origin");
			AssertOriginDestinationFilters(PrepaidCollectCodes.LDT, null, "USLAX", "local destination");

			void AssertOriginDestinationFilters(string valueToTest, string locationForAllOrigins, string locationForAllDestinations, string expectedValidationKeywords)
			{
				AssertEquals("Precondition: E6_PPDCLT is ALL by default", "ALL", cost.E6_PPDCLT);
				shipment1.JS_RL_NKOrigin = "AUMEL";
				shipment1.JS_RL_NKDestination = "NZAKL";
				shipment2.JS_RL_NKOrigin = "NZAKL";
				shipment2.JS_RL_NKDestination = "AUMEL";
				cost.Validation.ValidateE6_PPDCLT();
				AssertNoErrors("Precondition: no errors before validation tests", cost);
				cost.E6_PPDCLT = valueToTest;
				AssertNoError(cost.E6_PPDCLTInfo, $"Apportionment Filter - There are no shipments with {expectedValidationKeywords}. Select another filter to apply apportionment to.");
				shipment1.JS_RL_NKOrigin = locationForAllOrigins ?? shipment1.JS_RL_NKOrigin;
				shipment2.JS_RL_NKOrigin = locationForAllOrigins ?? shipment2.JS_RL_NKOrigin;
				shipment1.JS_RL_NKDestination = locationForAllDestinations ?? shipment1.JS_RL_NKDestination;
				shipment2.JS_RL_NKDestination = locationForAllDestinations ?? shipment2.JS_RL_NKDestination;
				cost.Validation.ValidateE6_PPDCLT();
				AssertHasError(cost.E6_PPDCLTInfo, $"Apportionment Filter - There are no shipments with {expectedValidationKeywords}. Select another filter to apply apportionment to.");
				cost.E6_PPDCLT = PrepaidCollectCodes.All;
				cost.Validation.ValidateE6_PPDCLT();
				AssertNoErrors(cost.E6_PPDCLTInfo);
			}
		}

		public void TestCheckExporterExemption()
		{
			var registry = AccountingMasterFilesRegistry.Instance;

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Italy))
			{
				var requireDocument = SetupDataForExporterExemption();
				var cost = GetConsolCost();

				var registryValues = new List<bool>() { true, false };
				foreach (var registryValue in registryValues)
				{
					using (registry.ValidateTaxIDApplicationForExporterExemptionItaly.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, registryValue))
					{
						cost.E6_OH_Creditor = ZGuid.Empty;
						AccountingConfigurationRegistry.Instance.ExporterExemptionCellingLimitThreshold.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, 0);
						requireDocument.EQ_ValidToDate = ZDate.Today.AddDays(31);
						cost.E6_OH_Creditor = ObjectCreator.Creditor1.PK;
						AssertNoWarnings(cost.E6_OH_CreditorInfo);

						cost.E6_OH_Creditor = ZGuid.Empty;
						requireDocument.EQ_ValidToDate = ZDate.Today.AddDays(30);
						cost.E6_OH_Creditor = ObjectCreator.Creditor1.PK;
						var warningMessage1 = $"The Exporter Exemption Certificate Number {requireDocument.EQ_DocNumber} is expiring on {requireDocument.EQ_ValidToDate.ToShortDateString()}.";
						AssertEquals("Expected certificate expired warning for Payables", true, cost.E6_OH_CreditorInfo.Notifications.GetWarnings().Contains(warningMessage1));
	
						cost.E6_OH_Creditor = ZGuid.Empty;
						AccountingConfigurationRegistry.Instance.ExporterExemptionCellingLimitThreshold.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, 50);
						var ceilingLimitAttribute = requireDocument.Attributes.AddNew();
						ceilingLimitAttribute.D0_AttribName = JobRequiredDocAttribTypeList.Codes.CeilingLimit;
						ceilingLimitAttribute.D0_AttribValue = "1000";
						cost.E6_OH_Creditor = ObjectCreator.Creditor1.PK;

						var warningMessage2 = @"The Exporter Exemption Ceiling Limit Threshold of 50% has exceeded.
The Total Certificate Ceiling Limit is EUR 1000.00.
Total Posted Transactions Amount using the exporter exemption certificates is EUR 700.00.";

						AssertEquals("Expected exemption ceiling limit threshold warning for Payables, only if Registry is false", !registryValue, cost.E6_OH_CreditorInfo.Notifications.GetWarnings().Contains(ZString.Format("{0}\r\n{1}", warningMessage1, warningMessage2)));
					}
				}
			}
		}

		JobRequiredDocument SetupDataForExporterExemption()
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

			var dichInvoice = ObjectCreator.CreateInvoice(typeof(APInvoice), "INV002", ObjectCreator.EUR, 1M, ObjectCreator.Creditor1);
			var line = ObjectCreator.CreateInvoiceLine(dichInvoice, ObjectCreator.EUR, 1M, 700M, 70M, 0M);
			line.AL_AT = taxRate.PK;

			var requireDocument = dichInvoice.Header.RequiredDocuments.AddNew();
			requireDocument.EQ_DocCategory = ReferenceTypes.ClientSupplierRelationship;
			requireDocument.EQ_DocType = RefDocTypes.VATExporterExemption;
			requireDocument.EQ_DocUsage = JobRequiredDocument.DocUsage.Creditor;
			requireDocument.EQ_RN_NKRelatedCountry = CountryCodes.Italy;
			requireDocument.EQ_DocNumber = "2019-10";
			requireDocument.EQ_DateReceived = ZDateTimeOffset.Today.AddDays(-1);
			requireDocument.EQ_ValidToDate = ZDate.Today.AddDays(31);

			Factory.Save();

			return requireDocument;
		}

		public void TestCheckHasCurrencyExchangeRate()
		{
			var testObjectCreator = new TestObjectCreator(Factory);
			var consol = testObjectCreator.CreateConsol();
			Factory.Save();
			var shipment1 = AddShipmentToConsolWithJob(consol);
			var shipment2 = AddShipmentToConsolWithJob(consol);
			var cost = consol.GetApportionments().CostsCollection.TryAddNew();
			cost.FillWithValidTestData();

			var chargeCurrencies = new ElectronicProcessingChargeCurrencyCollection();
			chargeCurrencies.Add(new ElectronicProcessingChargeCurrency { CurrencyPK = testObjectCreator.CNY.PK, ValidFromDate = ZDateTime.Today.AddDays(-10) });
			AccountingConfigurationRegistry.Instance.ElectronicProcessingChargeCurrency.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, chargeCurrencies);

			var electronicProcessingChargeProviderMock = new Mock<IElectronicProcessingChargeProvider>();
			electronicProcessingChargeProviderMock
					.Setup(x => x.HasElectronicProcessingChargeCurrencyExchangeRate(It.IsAny<Job>()))
					.Returns(false);

			using (ObjectFactory.Substitute(electronicProcessingChargeProviderMock.Object))
			{
				AccountingConfigurationRegistry.Instance.EnableElectronicProcessingChargeFunctionality.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
				cost.RunPreSaveValidation();
				AssertNull(cost.RowErrors.GetFirstMessage());

				AccountingConfigurationRegistry.Instance.EnableElectronicProcessingChargeFunctionality.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
				cost.RunPreSaveValidation();
				AssertEquals(true, cost.RowErrors.GetFirstMessage().Contains("The Invoicing Job cannot be created due to missing CNY exchange rate required for the creation of the disbursement license fee transactions."));
			}

			electronicProcessingChargeProviderMock
					.Setup(x => x.HasElectronicProcessingChargeCurrencyExchangeRate(It.IsAny<Job>()))
					.Returns(true);
			using (ObjectFactory.Substitute(electronicProcessingChargeProviderMock.Object))
			{
				cost.RunPreSaveValidation();
				AssertNull(cost.RowErrors.GetFirstMessage());
			}
		}

		ForwardingShipment AddShipmentToConsolWithJob(ForwardingConsol consol)
		{
			var shipment = consol.Shipments.AddNew();
			var job = new Job.Loader(shipment).TryCreateWithoutMutexForTestOnly();
			job.JH_GB = GlbBranch.CurrentBranch.PK;
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			return shipment;
		}

		protected InvoicePostingExRateOptionRegistryItem PostingExRateRegistryAP => AccountingConfigurationRegistry.Instance.InvoicePostingExchangeRateOptionAP;
		protected InvoicePostingExRateOptionRegistryItem PostingExRateRegistryAR => AccountingConfigurationRegistry.Instance.InvoicePostingExchangeRateOptionAR;
	}
}
