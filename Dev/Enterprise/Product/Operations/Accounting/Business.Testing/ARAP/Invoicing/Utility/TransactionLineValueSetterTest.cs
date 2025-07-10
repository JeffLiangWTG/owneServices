using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	public class TransactionLineValueSetterTest : TestCaseWithFactory
	{
		bool OldIsGSTRegistered;
		bool OldIsWHTRegistered;
		protected override void SetUp()
		{
			base.SetUp();
			OldIsGSTRegistered = GlbCompany.CurrentCompany.GC_IsGSTRegistered;
			GlbCompany.CurrentCompany.GC_IsGSTRegistered = true;

			OldIsWHTRegistered = GlbCompany.CurrentCompany.GC_IsWHTRegistered;
			GlbCompany.CurrentCompany.GC_IsWHTRegistered = true;

			TestObjectCreator = new TestObjectCreator(Factory);
		}

		protected override void TearDown()
		{
			base.TearDown();
			GlbCompany.CurrentCompany.GC_IsWHTRegistered = OldIsWHTRegistered;
			GlbCompany.CurrentCompany.GC_IsGSTRegistered = OldIsGSTRegistered;

			TestObjectCreator = null;
		}

		public void TestSetValuesUseSellCurrencyAsTransactionCurrency()
		{
			Job job = CreateJob("Z00001011", AALSHI, true, 5M, ABIGAS, true, 10M);

			ZECTRA.CompanyData.SetAPTaxApplicable(true);
			ZECTRA.MiscServ.OM_APWHTApplicable = true;
			Charge charge = CreateCharge(job, MRG100, "Cost Transaction Test", AUD, 250M, ZECTRA, USD, 350M, AALSHI);
			MRG100.AC_AG_AccrualAccount = MRG100.AC_AG_CostAccount = MRG100.AC_AG_RevenueAccount = MRG100.AC_AG_WIPAccount = new TestObjectCreator(Factory).GLHeader1.PK;

			APInvoice newAPInvoice = Factory.New<APInvoice>();
			APInvoiceLine invoiceLine = (APInvoiceLine)newAPInvoice.Lines.AddNew();

			invoiceLine.SetCostValues(job, charge);

			AssertEquals("Currency", "AUD", invoiceLine.AL_RX_NKTransactionCurrency);
		}

		public void TestCostOverrideGovtChargeCode()
		{
			AccountingMasterFilesRegistry.Instance.EnableGovernmentChargeCode.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

			var chargeCode = TestObjectCreator.CreateChargeCode("AAA");
			chargeCode.AC_GovtChargeCode = "GOVT AAA";

			Factory.Save();

			var overriddenGovtChargeCode = "GOVT BBB";
			var job = CreateJob("Z00001011", AALSHI, true, 5M, ABIGAS, true, 10M);
			var charge = job.Charges.AddNew();
			charge.JR_AC = chargeCode.PK;
			charge.JR_CostGovtChargeCode = overriddenGovtChargeCode;
			charge.JR_OH_CostAccount = TestObjectCreator.ABIGAS.PK;

			APInvoice newAPInvoice = Factory.New<APInvoice>();
			APInvoiceLine invoiceLine = (APInvoiceLine)newAPInvoice.Lines.AddNew();

			invoiceLine.SetCostValues(job, charge);

			AssertEquals("Overridden govt charge code is respected", overriddenGovtChargeCode, invoiceLine.AL_GovtChargeCode);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1115:DoNotUseSetUserContext", Justification = "Testing")]
		public void TestDontRecalculateOSValueFromLocalForConsolCosts()
		{
			GlbCompany currentCompany = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK);
			currentCompany.GC_IsReciprocal = true;
			Factory.Save();
			Env.SetUserContext(new UserContext(Env.CurrentUser.LoginName, Env.CurrentBranch.PK, Env.CurrentDepartment.PK));

			TestObjectCreator creator = new TestObjectCreator(Factory);
			creator.GST1.SetRateNumerator_ForTestOnly(25);

			ForwardingConsol consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.Shipments.AddNew();
			consol.Shipments.AddNew();
			consol.Shipments.AddNew();
			consol.Shipments.AddNew();

			Factory.Save();

			ApportionmentListing apportionments = new ApportionmentListing(Factory, consol);
			JobConsolCost cost = apportionments.CostsCollection.TryAddNew();
			cost.E6_ApportionToRelatedShipments = true;
			foreach (ApportionSplitCharge charge in cost.ApportionmentCharges)
			{
				charge.JR_IsUsedForApportionment = true;
			}
			cost.E6_AC_ChargeCode = creator.CC1.PK;
			cost.E6_RX_NKCurrency = creator.USD.RX_Code;
			cost.E6_OH_Creditor = creator.Creditor1.PK;
			cost.E6_ExchangeRate = 0.6m;
			cost.E6_ApportionmentMethod = ZArchitecture.Core.AllocationMethod.Shipment;
			cost.E6_OSCostAmount = 171.35m;
			cost.E6_InvoiceNum = "ABC123";
			cost.E6_InvoiceDate = ZDateTime.Now;
			cost.E6_AT_TaxRate = creator.GST1.PK;

			AssertEquals("Local Cost amount on consol cost", 102.81m, cost.E6_LocalCostAmount);
			AssertEquals("OS Tax amount on consol cost", 42.84m, cost.E6_OSGSTAmount_Calc);

			Factory.Save();

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			consol = newFactory.Load<ForwardingConsol>(consol.PK);
			apportionments = new ApportionmentListing(newFactory, consol);
			apportionments.PrepareForConsolCosting();

			var jobs = new List<Job>();
			foreach (ApportionSplitCharge charge in cost.ApportionmentCharges)
			{
				jobs.Add(newFactory.Load<Job>(charge.JR_JH));
			}

			ConsolInvoicingPostManager postManager = new ConsolInvoicingPostManager(Factory, jobs, consol, apportionments);
			TransactionCreatorHashtable transactionHasTable = postManager.CreateTransactions(JobInvoicingPostingOption.Costs);
			newFactory.Save();
			AssertEquals("Should have created one AP Invoice", 1, transactionHasTable.APTransactionsCount);
			newFactory = new BusinessObjectFactory();
			APInvoice invoice = newFactory.Load<APInvoice>(transactionHasTable.GetAllAPInvoicesAndCreditNotes()[0].PK);
			AssertEquals("Invoice OS Amount", 171.35m + 42.84m, invoice.AH_OSTotalAmount);
			AssertEquals("AH_PostedToEFT", true, invoice.AH_PostedToEFT);
			AssertEquals("Exchange Rate is recaculated becasue of enabled AH_PostedToEFT(UseJobExchangeRate), 128.51 / 214.19.", 0.599981m, invoice.AH_ExchangeRate);
			AssertEquals("Should have 4 lines", 4, invoice.Lines.Count);

			foreach (InvoicingLineBase invoiceLine in invoice.Lines)
			{
				AssertEquals("OS Ex Tax Amount", invoiceLine.RelatedJobCharge.JR_OSCostAmt, invoiceLine.AL_OSExTaxAmount);
				AssertEquals("OS Tax Amount", invoiceLine.RelatedJobCharge.JR_OSCostGSTAmt_Calc, invoiceLine.AL_OSTaxAmount);
				AssertEquals("Local Ex Tax Amount", invoiceLine.RelatedJobCharge.JR_LocalCostAmt, invoiceLine.AL_LocalExTaxAmount);
				AssertEquals("OS Total Amount", invoiceLine.RelatedJobCharge.JR_OSCostAmt + invoiceLine.RelatedJobCharge.JR_OSCostGSTAmt_Calc, -invoiceLine.AL_OSAmount);
			}
		}

		public void TestLineTypeNotOverriddenForUnapprovedInvoices()
		{
			Job job = CreateJob("Z00001011", AALSHI, true, 5M, ABIGAS, true, 10M);
			ExchangeRate rate = CreateExchangeRate(job, USD, .7M);

			ZECTRA.CompanyData.SetAPTaxApplicable(true);
			ZECTRA.MiscServ.OM_APWHTApplicable = true;
			Charge charge = CreateCharge(job, MRG100, "Cost Transaction Test", USD, 250M, ZECTRA, USD, 350M, AALSHI);

			UAInvoice invoice = Factory.New<UAInvoice>();
			UAInvoiceLine invoiceLine = (UAInvoiceLine)invoice.Lines.AddNew();

			AssertEquals("Linetype should be 'UCT'", ZArchitecture.Core.TransactionLineTypes.UnapprovedCost, invoiceLine.AL_LineType);

			invoiceLine.SetCostValues(job, charge);

			AssertEquals("Linetype should still be 'UCT'", ZArchitecture.Core.TransactionLineTypes.UnapprovedCost, invoiceLine.AL_LineType);
		}

		#region AP Transaction Lines Set Values

		public void TestSetTaxBranchValue()
		{
			var job = CreateJob("Z00001011", AALSHI, true, 5M, ABIGAS, true, 10M);
			var charge = CreateCharge(job, MRG100, "Cost Transaction Test", USD, 250M, ZECTRA, USD, 350M, AALSHI, PlaceOfSupplyListProvider.Codes.OutsideTheLoginCountry);
			charge.JR_GB_CostTaxBranch = GlbBranch.CurrentBranch.PK;

			var newAPInvoice = Factory.New<APInvoice>();

			AssertSetTaxBranchValue(true);
			AssertSetTaxBranchValue(false);

			void AssertSetTaxBranchValue(bool enableTaxBranchReporting)
			{
				using (AccountingMasterFilesRegistry.Instance.EnableTaxBranchReporting.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, enableTaxBranchReporting))
				{
					var invoiceLine = (APInvoiceLine)newAPInvoice.Lines.AddNew();
					invoiceLine.SetCostValues(job, charge);
					AssertEquals("Tax Branch", enableTaxBranchReporting ? GlbBranch.CurrentBranch.PK : ZGuid.Empty, invoiceLine.AL_GB_TaxBranch);
				}
			}
		}

		#region TEST: Set Values With GST Applicable WHT Applicable

		public void TestSetValuesWithGSTApplicableWHTApplicable()
		{
			Job job = CreateJob("Z00001011", AALSHI, true, 5M, ABIGAS, true, 10M);
			ExchangeRate rate = CreateExchangeRate(job, USD, .7M);

			ZECTRA.CompanyData.SetAPTaxApplicableIgnoringRegistrySetting(true);
			ZECTRA.MiscServ.OM_APWHTApplicable = true;
			Charge charge = CreateCharge(job, MRG100, "Cost Transaction Test", USD, 250M, ZECTRA, USD, 350M, AALSHI, PlaceOfSupplyListProvider.Codes.OutsideTheLoginCountry);
			var expectedDate = ZDate.Today.AddDays(-7);
			charge.JR_CostTaxDate = expectedDate;
			charge.JR_CostSupplyType = AccountingMasterFilesConstants.SupplyTypeClassificationCodes.LOC;
			MRG100.AC_AG_AccrualAccount = MRG100.AC_AG_CostAccount = MRG100.AC_AG_RevenueAccount = MRG100.AC_AG_WIPAccount = new TestObjectCreator(Factory).GLHeader1.PK;

			APInvoice newAPInvoice = Factory.New<APInvoice>();
			APInvoiceLine invoiceLine = (APInvoiceLine)newAPInvoice.Lines.AddNew();

			invoiceLine.SetCostValues(job, charge);

			AssertEquals("Line Type", ZArchitecture.Core.TransactionLineTypes.Cost, invoiceLine.AL_LineType);
			AssertEquals("Sequence", (short)1, invoiceLine.AL_Sequence);
			AssertEquals("Description", "Cost Transaction Test", invoiceLine.AL_Desc);
			AssertEquals("Unit Quantity", 0, invoiceLine.AL_UnitQty);
			AssertEquals("Unit Price", 0M, invoiceLine.AL_UnitPrice);
			AssertEquals("OS Unit Price", 0M, invoiceLine.AL_OSUnitPrice);

			AssertEquals("Currency", "USD", invoiceLine.AL_RX_NKTransactionCurrency);
			AssertEquals("Exchange Rate", .7M, invoiceLine.AL_ExchangeRate);
			AssertEquals("Post Period", 0, invoiceLine.AL_PostPeriod);
			AssertEquals("Post Date", ZDateTime.Empty, invoiceLine.AL_PostDate);
			AssertEquals("Post To GL", "N", invoiceLine.AL_PostToGL);
			AssertEquals("Reverse Period", 0, invoiceLine.AL_ReversePeriod);
			AssertEquals("Reverse Date", ZDateTime.Empty, invoiceLine.AL_ReverseDate);
			AssertEquals("Reverse to GL", "N", invoiceLine.AL_ReverseToGL);
			AssertEquals("Prevent Invoice Print Grouping", ZBool.False, invoiceLine.AL_PreventInvoicePrintGrouping);
			AssertEquals("Transaction Header PK", newAPInvoice.PK, invoiceLine.AL_AH);
			AssertEquals("Job Header", job.PK, invoiceLine.AL_JH);
			AssertEquals("Charge Code", MRG100.PK, invoiceLine.AL_AC);
			AssertEquals("Department", GlbDepartment.CurrentDepartment.PK, invoiceLine.AL_GE);
			AssertEquals("Branch", GlbBranch.CurrentBranch.PK, invoiceLine.AL_GB);
			AssertNotEquals("General Ledger", ZGuid.Empty, invoiceLine.AL_AG);
			AssertEquals("OrgHeader", ZECTRA.PK, invoiceLine.AL_OH);
			AssertEquals("GL Percent Of", ZGuid.Empty, invoiceLine.AL_AG_PercentOf);
			AssertEquals("Percentage of Period", 0, invoiceLine.AL_PercentageOfPeriod);

			AssertEquals("Cost Tax", GST1.PK, invoiceLine.AL_AT);
			AssertEquals("Cost Tax Date", expectedDate, invoiceLine.AL_TaxDate);
			AssertEquals("Cost Tax Msg", TaxMsg1.PK, invoiceLine.AL_A9_VATClass);
			AssertEquals("Cost WHT", WHT1.PK, invoiceLine.AL_AW);

			AssertEquals("OS Ex Tax Amount", 250M, invoiceLine.AL_OSExTaxAmount);
			AssertEquals("OS Tax Amount", 25M, invoiceLine.AL_OSTaxAmount);
			AssertEquals("OS WHT Amount", 12.50M, invoiceLine.AL_OSWHTAmount);
			AssertEquals("OS Total Amount", 275M, invoiceLine.AL_OverseasTotal);

			AssertEquals("Local Ex Amount", 357.14M, invoiceLine.AL_LocalExTaxAmount);
			AssertEquals("Local Tax Amount", 35.71M, invoiceLine.AL_LocalTaxAmount);
			AssertEquals("Local WHT Amount", 17.86M, invoiceLine.AL_LocalWHTAmount);
			AssertEquals("Local Total Amount", 392.85M, invoiceLine.AL_LocalTotalAmount);

			AssertEquals("Line Amount", -357.14M, invoiceLine.AL_LineAmount);
			AssertEquals("GST Tax ID", GST1.PK, invoiceLine.AL_AT);
			AssertEquals("Tax Message ID", TaxMsg1.PK, invoiceLine.AL_A9_VATClass);
			AssertEquals("GST Amount", -35.71M, invoiceLine.AL_GSTVAT);
			AssertEquals("WHT Tax ID", WHT1.PK, invoiceLine.AL_AW);
			AssertEquals("Withholding Tax", -17.86M, invoiceLine.AL_WithholdingTax);
			AssertEquals("OS Amount", -275.00M, invoiceLine.AL_OSAmount);
			AssertEquals("Place of Supply", PlaceOfSupplyListProvider.Codes.OutsideTheLoginCountry, invoiceLine.AL_PlaceOfSupply);
			AssertEquals("Place of Supply Type", PlaceOfSupplyListProvider.GetPlaceTypeFromPlaceCode(invoiceLine.Company, invoiceLine.AL_PlaceOfSupply), invoiceLine.AL_PlaceOfSupplyType);
			AssertEquals("Supply Type", AccountingMasterFilesConstants.SupplyTypeClassificationCodes.LOC, invoiceLine.AL_SupplyType);
		}

		#endregion

		#region TEST: Set Values With GST Applicable WHT Not Applicable

		public void TestSetValuesWithGSTApplicableWHTNotApplicable()
		{
			Job job = CreateJob("Z00001011", AALSHI, true, 5M, ABIGAS, true, 10M);
			ExchangeRate rate = CreateExchangeRate(job, USD, .7M);

			ZECTRA.CompanyData.SetAPTaxApplicableIgnoringRegistrySetting(true);
			ZECTRA.MiscServ.OM_APWHTApplicable = false;
			Charge charge = CreateCharge(job, MRG100, "Cost Transaction Test", USD, 250M, ZECTRA, USD, 350M, AALSHI, PlaceOfSupplyListProvider.Codes.OutsideTheLoginCountry);
			charge.JR_CostTaxDate = ZDate.Empty;
			charge.JR_CostSupplyType = AccountingMasterFilesConstants.SupplyTypeClassificationCodes.LOC;
			MRG100.AC_AG_AccrualAccount = MRG100.AC_AG_CostAccount = MRG100.AC_AG_RevenueAccount = MRG100.AC_AG_WIPAccount = new TestObjectCreator(Factory).GLHeader1.PK;

			APInvoice newAPInvoice = Factory.New<APInvoice>();
			APInvoiceLine invoiceLine = (APInvoiceLine)newAPInvoice.Lines.AddNew();

			invoiceLine.SetCostValues(job, charge);

			AssertEquals("Line Type", ZArchitecture.Core.TransactionLineTypes.Cost, invoiceLine.AL_LineType);
			AssertEquals("Sequence", (short)1, invoiceLine.AL_Sequence);
			AssertEquals("Description", "Cost Transaction Test", invoiceLine.AL_Desc);
			AssertEquals("Unit Quantity", 0, invoiceLine.AL_UnitQty);
			AssertEquals("Unit Price", 0M, invoiceLine.AL_UnitPrice);
			AssertEquals("OS Unit Price", 0M, invoiceLine.AL_OSUnitPrice);
			AssertEquals("Currency", "USD", invoiceLine.AL_RX_NKTransactionCurrency);
			AssertEquals("Exchange Rate", .7M, invoiceLine.AL_ExchangeRate);
			AssertEquals("Post Period", 0, invoiceLine.AL_PostPeriod);
			AssertEquals("Post Date", ZDateTime.Empty, invoiceLine.AL_PostDate);
			AssertEquals("Post To GL", "N", invoiceLine.AL_PostToGL);
			AssertEquals("Reverse Period", 0, invoiceLine.AL_ReversePeriod);
			AssertEquals("Reverse Date", ZDateTime.Empty, invoiceLine.AL_ReverseDate);
			AssertEquals("Reverse to GL", "N", invoiceLine.AL_ReverseToGL);
			AssertEquals("Prevent Invoice Print Grouping", ZBool.False, invoiceLine.AL_PreventInvoicePrintGrouping);
			AssertEquals("Transaction Header PK", newAPInvoice.PK, invoiceLine.AL_AH);
			AssertEquals("Job Header", job.PK, invoiceLine.AL_JH);
			AssertEquals("Charge Code", MRG100.PK, invoiceLine.AL_AC);
			AssertEquals("Department", GlbDepartment.CurrentDepartment.PK, invoiceLine.AL_GE);
			AssertEquals("Branch", GlbBranch.CurrentBranch.PK, invoiceLine.AL_GB);
			AssertNotEquals("General Ledger", ZGuid.Empty, invoiceLine.AL_AG);
			AssertEquals("OrgHeader", ZECTRA.PK, invoiceLine.AL_OH);
			AssertEquals("GL Percent Of", ZGuid.Empty, invoiceLine.AL_AG_PercentOf);
			AssertEquals("Percentage of Period", 0, invoiceLine.AL_PercentageOfPeriod);

			AssertEquals("Cost Tax", GST1.PK, invoiceLine.AL_AT);
			AssertEquals("Cost Tax Date", ZDate.Today, invoiceLine.AL_TaxDate);
			AssertEquals("Tax Msg", TaxMsg1.PK, invoiceLine.AL_A9_VATClass);
			AssertEquals("Cost WHT", ZGuid.Empty, invoiceLine.AL_AW);

			AssertEquals("OS Ex Tax Amount", 250M, invoiceLine.AL_OSExTaxAmount);
			AssertEquals("OS Tax Amount", 25M, invoiceLine.AL_OSTaxAmount);
			AssertEquals("OS WHT Amount", 0M, invoiceLine.AL_OSWHTAmount);
			AssertEquals("OS Total Amount", 275M, invoiceLine.AL_OverseasTotal);

			AssertEquals("Local Ex Amount", 357.14M, invoiceLine.AL_LocalExTaxAmount);
			AssertEquals("Local Tax Amount", 35.71M, invoiceLine.AL_LocalTaxAmount);
			AssertEquals("Local WHT Amount", 0M, invoiceLine.AL_LocalWHTAmount);
			AssertEquals("Local Total Amount", 392.85M, invoiceLine.AL_LocalTotalAmount);

			AssertEquals("Line Amount", -357.14M, invoiceLine.AL_LineAmount);
			AssertEquals("GST Tax ID", GST1.PK, invoiceLine.AL_AT);
			AssertEquals("Tax Msg ID", TaxMsg1.PK, invoiceLine.AL_A9_VATClass);
			AssertEquals("GST Amount", -35.71M, invoiceLine.AL_GSTVAT);
			AssertEquals("WHT Tax ID", ZGuid.Empty, invoiceLine.AL_AW);
			AssertEquals("Withholding Tax", 0M, invoiceLine.AL_WithholdingTax);
			AssertEquals("OS Amount", -275.00M, invoiceLine.AL_OSAmount);
			AssertEquals("Place of Supply", PlaceOfSupplyListProvider.Codes.OutsideTheLoginCountry, invoiceLine.AL_PlaceOfSupply);
			AssertEquals("Place of Supply Type", PlaceOfSupplyListProvider.GetPlaceTypeFromPlaceCode(invoiceLine.Company, invoiceLine.AL_PlaceOfSupply), invoiceLine.AL_PlaceOfSupplyType);
			AssertEquals("Supply Type", AccountingMasterFilesConstants.SupplyTypeClassificationCodes.LOC, invoiceLine.AL_SupplyType);
		}

		#endregion

		#region TEST: Set Values With GST Not Applicable WHT Applicable

		public void TestSetValuesWithGSTNotApplicableWHTApplicable()
		{
			Job job = CreateJob("Z00001011", AALSHI, true, 5M, ABIGAS, true, 10M);
			ExchangeRate rate = CreateExchangeRate(job, USD, .7M);

			ZECTRA.CompanyData.SetAPTaxApplicable(false);
			ZECTRA.MiscServ.OM_APWHTApplicable = true;
			Charge charge = CreateCharge(job, MRG100, "Cost Transaction Test", USD, 250M, ZECTRA, USD, 350M, AALSHI, PlaceOfSupplyListProvider.Codes.OutsideTheLoginCountry);
			charge.JR_CostSupplyType = AccountingMasterFilesConstants.SupplyTypeClassificationCodes.LOC;
			MRG100.AC_AG_AccrualAccount = MRG100.AC_AG_CostAccount = MRG100.AC_AG_RevenueAccount = MRG100.AC_AG_WIPAccount = new TestObjectCreator(Factory).GLHeader1.PK;

			APInvoice newAPInvoice = Factory.New<APInvoice>();
			APInvoiceLine invoiceLine = (APInvoiceLine)newAPInvoice.Lines.AddNew();

			invoiceLine.SetCostValues(job, charge);

			AssertEquals("Line Type", ZArchitecture.Core.TransactionLineTypes.Cost, invoiceLine.AL_LineType);
			AssertEquals("Sequence", (short)1, invoiceLine.AL_Sequence);
			AssertEquals("Description", "Cost Transaction Test", invoiceLine.AL_Desc);
			AssertEquals("Unit Quantity", 0, invoiceLine.AL_UnitQty);
			AssertEquals("Unit Price", 0M, invoiceLine.AL_UnitPrice);
			AssertEquals("OS Unit Price", 0M, invoiceLine.AL_OSUnitPrice);
			AssertEquals("Currency", "USD", invoiceLine.AL_RX_NKTransactionCurrency);
			AssertEquals("Exchange Rate", .7M, invoiceLine.AL_ExchangeRate);
			AssertEquals("Post Period", 0, invoiceLine.AL_PostPeriod);
			AssertEquals("Post Date", ZDateTime.Empty, invoiceLine.AL_PostDate);
			AssertEquals("Post To GL", "N", invoiceLine.AL_PostToGL);
			AssertEquals("Reverse Period", 0, invoiceLine.AL_ReversePeriod);
			AssertEquals("Reverse Date", ZDateTime.Empty, invoiceLine.AL_ReverseDate);
			AssertEquals("Reverse to GL", "N", invoiceLine.AL_ReverseToGL);
			AssertEquals("Prevent Invoice Print Grouping", ZBool.False, invoiceLine.AL_PreventInvoicePrintGrouping);
			AssertEquals("Transaction Header PK", newAPInvoice.PK, invoiceLine.AL_AH);
			AssertEquals("Job Header", job.PK, invoiceLine.AL_JH);
			AssertEquals("Charge Code", MRG100.PK, invoiceLine.AL_AC);
			AssertEquals("Department", GlbDepartment.CurrentDepartment.PK, invoiceLine.AL_GE);
			AssertEquals("Branch", GlbBranch.CurrentBranch.PK, invoiceLine.AL_GB);
			AssertNotEquals("General Ledger", ZGuid.Empty, invoiceLine.AL_AG);
			AssertEquals("OrgHeader", ZECTRA.PK, invoiceLine.AL_OH);
			AssertEquals("GL Percent Of", ZGuid.Empty, invoiceLine.AL_AG_PercentOf);
			AssertEquals("Percentage of Period", 0, invoiceLine.AL_PercentageOfPeriod);

			AssertEquals("Cost Tax", ZGuid.Empty, invoiceLine.AL_AT);
			AssertEquals("Cost Tax Msg", ZGuid.Empty, invoiceLine.AL_A9_VATClass);
			AssertEquals("Cost WHT", WHT1.PK, invoiceLine.AL_AW);

			AssertEquals("OS Ex Tax Amount", 250M, invoiceLine.AL_OSExTaxAmount);
			AssertEquals("OS Tax Amount", 0M, invoiceLine.AL_OSTaxAmount);
			AssertEquals("OS WHT Amount", 12.50M, invoiceLine.AL_OSWHTAmount);
			AssertEquals("OS Total Amount", 250M, invoiceLine.AL_OverseasTotal);

			AssertEquals("Local Ex Amount", 357.14M, invoiceLine.AL_LocalExTaxAmount);
			AssertEquals("Local Tax Amount", 0M, invoiceLine.AL_LocalTaxAmount);
			AssertEquals("Local WHT Amount", 17.86M, invoiceLine.AL_LocalWHTAmount);
			AssertEquals("Local Total Amount", 357.14M, invoiceLine.AL_LocalTotalAmount);

			AssertEquals("Line Amount", -357.14M, invoiceLine.AL_LineAmount);
			AssertEquals("GST Tax ID", ZGuid.Empty, invoiceLine.AL_AT);
			AssertEquals("Tax Msg ID", ZGuid.Empty, invoiceLine.AL_A9_VATClass);
			AssertEquals("GST Amount", 0M, invoiceLine.AL_GSTVAT);
			AssertEquals("WHT Tax ID", WHT1.PK, invoiceLine.AL_AW);
			AssertEquals("Withholding Tax", -17.86M, invoiceLine.AL_WithholdingTax);
			AssertEquals("OS Amount", -250.00M, invoiceLine.AL_OSAmount);
			AssertEquals("Place of Supply", PlaceOfSupplyListProvider.Codes.OutsideTheLoginCountry, invoiceLine.AL_PlaceOfSupply);
			AssertEquals("Place of Supply Type", PlaceOfSupplyListProvider.GetPlaceTypeFromPlaceCode(invoiceLine.Company, invoiceLine.AL_PlaceOfSupply), invoiceLine.AL_PlaceOfSupplyType);
			AssertEquals("Supply Type", AccountingMasterFilesConstants.SupplyTypeClassificationCodes.LOC, invoiceLine.AL_SupplyType);
		}

		#endregion

		#region TEST: Set Values With GST Not Applicable WHT Not Applicable

		public void TestSetValuesWithGSTNotApplicableWHTNotApplicable()
		{
			Job job = CreateJob("Z00001011", AALSHI, true, 5M, ABIGAS, true, 10M);
			ExchangeRate rate = CreateExchangeRate(job, USD, .7M);

			ZECTRA.CompanyData.SetAPTaxApplicable(false);
			ZECTRA.MiscServ.OM_APWHTApplicable = false;
			Charge charge = CreateCharge(job, MRG100, "Cost Transaction Test", USD, 250M, ZECTRA, USD, 350M, AALSHI, PlaceOfSupplyListProvider.Codes.OutsideTheLoginCountry);
			charge.JR_CostSupplyType = AccountingMasterFilesConstants.SupplyTypeClassificationCodes.LOC;
			MRG100.AC_AG_AccrualAccount = MRG100.AC_AG_CostAccount = MRG100.AC_AG_RevenueAccount = MRG100.AC_AG_WIPAccount = new TestObjectCreator(Factory).GLHeader1.PK;

			APInvoice newAPInvoice = Factory.New<APInvoice>();
			APInvoiceLine invoiceLine = (APInvoiceLine)newAPInvoice.Lines.AddNew();

			invoiceLine.SetCostValues(job, charge);

			AssertEquals("Line Type", ZArchitecture.Core.TransactionLineTypes.Cost, invoiceLine.AL_LineType);
			AssertEquals("Sequence", (short)1, invoiceLine.AL_Sequence);
			AssertEquals("Description", "Cost Transaction Test", invoiceLine.AL_Desc);
			AssertEquals("Unit Quantity", 0, invoiceLine.AL_UnitQty);
			AssertEquals("Unit Price", 0M, invoiceLine.AL_UnitPrice);
			AssertEquals("OS Unit Price", 0M, invoiceLine.AL_OSUnitPrice);
			AssertEquals("Currency", "USD", invoiceLine.AL_RX_NKTransactionCurrency);
			AssertEquals("Exchange Rate", .7M, invoiceLine.AL_ExchangeRate);
			AssertEquals("Post Period", 0, invoiceLine.AL_PostPeriod);
			AssertEquals("Post Date", ZDateTime.Empty, invoiceLine.AL_PostDate);
			AssertEquals("Post To GL", "N", invoiceLine.AL_PostToGL);
			AssertEquals("Reverse Period", 0, invoiceLine.AL_ReversePeriod);
			AssertEquals("Reverse Date", ZDateTime.Empty, invoiceLine.AL_ReverseDate);
			AssertEquals("Reverse to GL", "N", invoiceLine.AL_ReverseToGL);
			AssertEquals("Prevent Invoice Print Grouping", ZBool.False, invoiceLine.AL_PreventInvoicePrintGrouping);
			AssertEquals("Transaction Header PK", newAPInvoice.PK, invoiceLine.AL_AH);
			AssertEquals("Job Header", job.PK, invoiceLine.AL_JH);
			AssertEquals("Charge Code", MRG100.PK, invoiceLine.AL_AC);
			AssertEquals("Department", GlbDepartment.CurrentDepartment.PK, invoiceLine.AL_GE);
			AssertEquals("Branch", GlbBranch.CurrentBranch.PK, invoiceLine.AL_GB);
			AssertNotEquals("General Ledger", ZGuid.Empty, invoiceLine.AL_AG);
			AssertEquals("OrgHeader", ZECTRA.PK, invoiceLine.AL_OH);
			AssertEquals("GL Percent Of", ZGuid.Empty, invoiceLine.AL_AG_PercentOf);
			AssertEquals("Percentage of Period", 0, invoiceLine.AL_PercentageOfPeriod);

			AssertEquals("Cost Tax", ZGuid.Empty, invoiceLine.AL_AT);
			AssertEquals("Cost Tax Msg", ZGuid.Empty, invoiceLine.AL_A9_VATClass);
			AssertEquals("Cost WHT", ZGuid.Empty, invoiceLine.AL_AW);

			AssertEquals("OS Ex Tax Amount", 250M, invoiceLine.AL_OSExTaxAmount);
			AssertEquals("OS Tax Amount", 0M, invoiceLine.AL_OSTaxAmount);
			AssertEquals("OS WHT Amount", 0M, invoiceLine.AL_OSWHTAmount);
			AssertEquals("OS Total Amount", 250M, invoiceLine.AL_OverseasTotal);

			AssertEquals("Local Ex Amount", 357.14M, invoiceLine.AL_LocalExTaxAmount);
			AssertEquals("Local Tax Amount", 0M, invoiceLine.AL_LocalTaxAmount);
			AssertEquals("Local WHT Amount", 0M, invoiceLine.AL_LocalWHTAmount);
			AssertEquals("Local Total Amount", 357.14M, invoiceLine.AL_LocalTotalAmount);

			AssertEquals("Line Amount", -357.14M, invoiceLine.AL_LineAmount);
			AssertEquals("GST Tax ID", ZGuid.Empty, invoiceLine.AL_AT);
			AssertEquals("Tax Msg ID", ZGuid.Empty, invoiceLine.AL_A9_VATClass);
			AssertEquals("GST Amount", 0M, invoiceLine.AL_GSTVAT);
			AssertEquals("WHT Tax ID", ZGuid.Empty, invoiceLine.AL_AW);
			AssertEquals("Withholding Tax", 0M, invoiceLine.AL_WithholdingTax);
			AssertEquals("OS Amount", -250.00M, invoiceLine.AL_OSAmount);
			AssertEquals("Place of Supply", PlaceOfSupplyListProvider.Codes.OutsideTheLoginCountry, invoiceLine.AL_PlaceOfSupply);
			AssertEquals("Place of Supply Type", PlaceOfSupplyListProvider.GetPlaceTypeFromPlaceCode(invoiceLine.Company, invoiceLine.AL_PlaceOfSupply), invoiceLine.AL_PlaceOfSupplyType);
			AssertEquals("Supply Type", AccountingMasterFilesConstants.SupplyTypeClassificationCodes.LOC, invoiceLine.AL_SupplyType);
		}

		#endregion

		#region TEST: Set AP line's tax date based on tax date defaulting registry setting

		void AssertSetAPLineTaxDateWithTaxDateRegistryOption(ZString taxDateDefaultingOption)
		{
			var creator = new TestObjectCreator(Factory);
			var shipment1 = creator.CreateShipment("S1");
			shipment1.JS_E_ARV = new ZDateTime(2020, 11, 09);
			var job = creator.CreateJob(shipment1, false);
			job.AgentCollectPK = creator.ABIGAS.PK;
			creator.ABIGAS.CompanyData.OB_APVATConfig = AccountingMasterFilesConstants.OrganisationTaxConfiguartionTypes.Default.Code;
			var charge = creator.CreateCharge(job, TestObjectCreator.CC1, "desc", TestObjectCreator.AUD, 100m, TestObjectCreator.AALSHI, TestObjectCreator.AUD, 100m, TestObjectCreator.ABIGAS);
			using (charge.GetValidationSuspender())
			{
				charge.JR_AT_CostGSTRate = creator.GST1.PK;
				charge.JR_CostTaxDate = ZDate.Empty;
				Factory.Save();

				var newAPInvoice = Factory.New<APInvoice>();
				newAPInvoice.AH_InvoiceDate = new ZDateTime(2020, 10, 10);
				var invoiceLine = (APInvoiceLine)newAPInvoice.Lines.AddNew();

				var collection = new TaxDateDefaultingOptionCollection();
				var taxDateOption = collection.AddNew();
				taxDateOption.JobType = "SHP";
				taxDateOption.DirectionCode = "ALL";
				taxDateOption.Mode = "ALL";
				taxDateOption.Ledger = "AP";
				taxDateOption.TaxDateOption = taxDateDefaultingOption;
				using (AccountingConfigurationRegistry.Instance.TaxDateDefaultingOption.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, collection))
				{
					invoiceLine.SetCostValues(job, charge);

					switch (taxDateDefaultingOption)
					{
						case TaxDateDefaultingOption.Code.Today:
							AssertEquals("expect to use today's date", new ZDate(2020, 11, 02), invoiceLine.AL_TaxDate);
							AssertEquals("expect to use today's date", new ZDate(2020, 11, 02), charge.JR_CostTaxDate);
							break;
						case TaxDateDefaultingOption.Code.InvoiceDate:
							AssertEquals("expect to use invoice date", new ZDate(2020, 10, 10), invoiceLine.AL_TaxDate);
							AssertEquals("expect to use invoice date", new ZDate(2020, 10, 10), charge.JR_CostTaxDate);
							break;
						case TaxDateDefaultingOption.Code.EstimatedArrivalDate:
							AssertEquals("expect to use ETA date", new ZDate(2020, 11, 09), invoiceLine.AL_TaxDate);
							AssertEquals("expect to use ETA date", new ZDate(2020, 11, 09), charge.JR_CostTaxDate);
							break;
						default:
							Fail("Invalid tax date defaulting option");
							break;
					}
				}
			}
		}

		[TestDate(2020, 11, 02)]
		public void TestSetAPLineTaxDateWhenTaxDateRegistry_Today()
		{
			AssertSetAPLineTaxDateWithTaxDateRegistryOption(TaxDateDefaultingOption.Code.Today);
		}

		[TestDate(2020, 11, 02)]
		public void TestSetAPLineTaxDateWhenTaxDateRegistry_InvoiceDate()
		{
			AssertSetAPLineTaxDateWithTaxDateRegistryOption(TaxDateDefaultingOption.Code.InvoiceDate);
		}

		[TestDate(2020, 11, 02)]
		public void TestSetAPLineTaxDateWhenTaxDateRegistry_JobOperationalDate()
		{
			AssertSetAPLineTaxDateWithTaxDateRegistryOption(TaxDateDefaultingOption.Code.EstimatedArrivalDate);
		}

		[TestDate(2020, 11, 02)]
		public void TestSetAPLineTaxDateWithTaxDateRegistryOption_ApportionedCharge_Today()
			=> AssertSetAPLineTaxDateWithTaxDateRegistryOption_ApportionedCharge(TaxDateDefaultingOption.Code.Today, false);

		[TestDate(2020, 11, 02)]
		public void TestSetAPLineTaxDateWithTaxDateRegistryOption_ApportionedCharge_Today_TaxOverridden()
			=> AssertSetAPLineTaxDateWithTaxDateRegistryOption_ApportionedCharge(TaxDateDefaultingOption.Code.Today, true);

		[TestDate(2020, 11, 02)]
		public void TestSetAPLineTaxDateWithTaxDateRegistryOption_ApportionedCharge_InvoiceDate()
			=> AssertSetAPLineTaxDateWithTaxDateRegistryOption_ApportionedCharge(TaxDateDefaultingOption.Code.InvoiceDate, false);

		[TestDate(2020, 11, 02)]
		public void TestSetAPLineTaxDateWithTaxDateRegistryOption_ApportionedCharge_InvoiceDate_TaxOverridden()
			=> AssertSetAPLineTaxDateWithTaxDateRegistryOption_ApportionedCharge(TaxDateDefaultingOption.Code.InvoiceDate, true);

		void AssertSetAPLineTaxDateWithTaxDateRegistryOption_ApportionedCharge(ZString taxDateDefaultingOption, bool isConsolCostTaxOverridden)
		{
			var creator = new TestObjectCreator(Factory);
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			var shipment = consol.Shipments.AddNew();
			var job = creator.CreateJob(shipment, false);
			Factory.Save();

			ApportionmentListing apportionments = new ApportionmentListing(Factory, consol);
			JobConsolCost cost = apportionments.CostsCollection.TryAddNew();
			cost.E6_ApportionToRelatedShipments = true;
			foreach (ApportionSplitCharge c in cost.ApportionmentCharges)
			{
				c.JR_IsUsedForApportionment = true;
			}
			cost.E6_AC_ChargeCode = creator.CC1.PK;
			cost.E6_RX_NKCurrency = creator.USD.RX_Code;
			cost.E6_OH_Creditor = creator.Creditor1.PK;
			cost.E6_ExchangeRate = 0.6m;
			cost.E6_ApportionmentMethod = ZArchitecture.Core.AllocationMethod.Shipment;
			cost.E6_OSCostAmount = 171.40m;
			cost.E6_InvoiceNum = "ABC123";
			cost.E6_InvoiceDate = new ZDateTime(2020, 10, 10);
			cost.E6_AT_TaxRate = creator.GST1.PK;
			cost.E6_TaxDate = ZDate.Empty;

			AssertEquals("Pre-Condition", 17.14m, cost.E6_OSGSTAmount_Calc);
			if (isConsolCostTaxOverridden)
			{
				cost.E6_IsTaxAmountOverridden = true;
				cost.E6_OSGSTAmount_Calc = 10m;
			}

			Factory.Save();

			var newAPInvoice = Factory.New<APInvoice>();
			newAPInvoice.AH_InvoiceDate = new ZDateTime(2020, 10, 10);
			var invoiceLine = (APInvoiceLine)newAPInvoice.Lines.AddNew();

			var collection = new TaxDateDefaultingOptionCollection();
			var taxDateOption = collection.AddNew();
			taxDateOption.JobType = "FCN";
			taxDateOption.DirectionCode = "ALL";
			taxDateOption.Mode = "ALL";
			taxDateOption.Ledger = "AP";
			taxDateOption.TaxDateOption = taxDateDefaultingOption;
			using (AccountingConfigurationRegistry.Instance.TaxDateDefaultingOption.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, collection))
			{
				invoiceLine.SetCostValues(job, job.Charges[0]);
				switch (taxDateDefaultingOption)
				{
					case TaxDateDefaultingOption.Code.Today:
						AssertEquals("expect to use today's date", new ZDate(2020, 11, 02), invoiceLine.AL_TaxDate);
						AssertEquals("expect to use today's date", new ZDate(2020, 11, 02), cost.E6_TaxDate);
						cost.ApportionmentCharges.Cast<ApportionSplitCharge>().ToList().ForEach(x => AssertEquals("expect to use today's date", new ZDate(2020, 11, 02), x.JR_CostTaxDate));
						break;
					case TaxDateDefaultingOption.Code.InvoiceDate:
						AssertEquals("expect to use invoice date", new ZDate(2020, 10, 10), invoiceLine.AL_TaxDate);
						AssertEquals("expect to use invoice date", new ZDate(2020, 10, 10), cost.E6_TaxDate);
						cost.ApportionmentCharges.Cast<ApportionSplitCharge>().ToList().ForEach(x => AssertEquals("expect to use invoice date", new ZDate(2020, 10, 10), x.JR_CostTaxDate));
						break;
					default:
						Fail("Invalid tax date defaulting option");
						break;
				}
			}

			if (isConsolCostTaxOverridden)
			{
				AssertEquals("Consol cost tax amount should be me modified after processing", 10m, cost.E6_OSGSTAmount_Calc);
			}
			else
			{
				AssertEquals("Consol cost tax amount should be same as before", 17.14m, cost.E6_OSGSTAmount_Calc);
			}
		}

		#endregion

		#endregion

		#region Test SetValues from another Line

		public void TestSetValuesFromLine()
		{
			Job job = CreateJob("Z00001011", AALSHI, true, 5M, ABIGAS, true, 10M);

			var invoice = TestObjectCreator.CreateAPInvoice<APInvoice>("10001", TestObjectCreator.USD, .8M, 100M, 10M, 5M, 125M, 12.5M, 6.25M);
			var invoiceLine = (APInvoiceLine)invoice.Lines[0];
			invoiceLine.AL_Sequence = 10;
			invoiceLine.AL_AC = TestObjectCreator.CC1.PK;
			invoiceLine.AL_JH = job.PK;
			invoiceLine.AL_AT = TestObjectCreator.GST1.PK;
			var expectedDate = ZDate.Today.AddDays(-2);
			invoiceLine.AL_TaxDate = expectedDate;
			invoiceLine.AL_A9_VATClass = TestObjectCreator.TaxMsg1.PK;
			invoiceLine.AL_AW = TestObjectCreator.WHT1.PK;
			invoiceLine.AL_PlaceOfSupply = PlaceOfSupplyListProvider.Codes.OutsideTheLoginCountry;
			invoiceLine.AL_SupplyType = AccountingMasterFilesConstants.SupplyTypeClassificationCodes.DSB;

			var creditNote = Factory.New<UACreditNote>();
			var creditNoteLine = Factory.New<UACreditNoteLine>();
			creditNote.Lines.Add(creditNoteLine);

			AssertEquals("Linetype should be 'UCT'", ZArchitecture.Core.TransactionLineTypes.UnapprovedCost, creditNoteLine.AL_LineType);
			AssertEquals("Line Currency should be 'AUD'", "AUD", creditNoteLine.AL_RX_NKTransactionCurrency);

			Action<bool> assertValues = sameSign =>
				{
					var multiplier = sameSign ? 1 : -1;
					AssertEquals("Linetype should still be 'UCT'", ZArchitecture.Core.TransactionLineTypes.UnapprovedCost, creditNoteLine.AL_LineType);
					AssertEquals("Line Currency should be set to 'USD'", "USD", creditNoteLine.AL_RX_NKTransactionCurrency);

					AssertEquals("AL_AC", invoiceLine.AL_AC, creditNoteLine.AL_AC);
					AssertEquals("AL_JH", invoiceLine.AL_JH, creditNoteLine.AL_JH);
					AssertEquals("AL_AG", invoiceLine.AL_AG, creditNoteLine.AL_AG);
					AssertEquals("AL_Desc", invoiceLine.AL_Desc, creditNoteLine.AL_Desc);
					AssertEquals("AL_AT", invoiceLine.AL_AT, creditNoteLine.AL_AT);
					AssertEquals("AL_TaxDate", expectedDate, creditNoteLine.AL_TaxDate);
					AssertEquals("AL_A9_VATClass", invoiceLine.AL_A9_VATClass, creditNoteLine.AL_A9_VATClass);
					AssertEquals("AL_AW", invoiceLine.AL_AW, creditNoteLine.AL_AW);
					AssertEquals("AL_GE", invoiceLine.AL_GE, creditNoteLine.AL_GE);
					AssertEquals("AL_GB", invoiceLine.AL_GB, creditNoteLine.AL_GB);
					AssertEquals("AL_ExchangeRate", invoiceLine.AL_ExchangeRate, creditNoteLine.AL_ExchangeRate);
					AssertNotEquals("AL_OSExTaxAmount", 0m, invoiceLine.AL_OSExTaxAmount);
					AssertEquals("AL_OSExTaxAmount", multiplier * invoiceLine.AL_OSExTaxAmount, creditNoteLine.AL_OSExTaxAmount);
					AssertNotEquals("AL_LocalExTaxAmount", 0m, invoiceLine.AL_LocalExTaxAmount);
					AssertEquals("AL_LocalExTaxAmount", multiplier * invoiceLine.AL_LocalExTaxAmount, creditNoteLine.AL_LocalExTaxAmount);
					AssertNotEquals("AL_LocalAmount", 0m, invoiceLine.AL_LineAmount);
					AssertEquals("AL_LocalAmount - different signs", -multiplier * invoiceLine.AL_LineAmount, creditNoteLine.AL_LineAmount);
					AssertNotEquals("AL_OSTaxAmount", 0m, invoiceLine.AL_OSTaxAmount);
					AssertEquals("AL_OSTaxAmount", multiplier * invoiceLine.AL_OSTaxAmount, creditNoteLine.AL_OSTaxAmount);
					AssertNotEquals("AL_LocalTaxAmount", 0m, invoiceLine.AL_LocalTaxAmount);
					AssertEquals("AL_LocalTaxAmount", multiplier * invoiceLine.AL_LocalTaxAmount, creditNoteLine.AL_LocalTaxAmount);
					AssertNotEquals("AL_OSWHTAmount", 0m, invoiceLine.AL_OSWHTAmount);
					AssertEquals("AL_OSWHTAmount", multiplier * invoiceLine.AL_OSWHTAmount, creditNoteLine.AL_OSWHTAmount);
					AssertNotEquals("AL_LocalWHTAmount", 0m, invoiceLine.AL_LocalWHTAmount);
					AssertEquals("AL_LocalWHTAmount", multiplier * invoiceLine.AL_LocalWHTAmount, creditNoteLine.AL_LocalWHTAmount);
					AssertNotEquals("The sequence should NOT be copied as part of this method", invoiceLine.AL_Sequence, creditNoteLine.AL_Sequence);
					AssertEquals("The sequence should come from the default line number of the invoice", (short)1, creditNoteLine.AL_Sequence);
					AssertEquals("Place of Supply", PlaceOfSupplyListProvider.Codes.OutsideTheLoginCountry, creditNoteLine.AL_PlaceOfSupply);
					AssertEquals("Place of Supply Type", PlaceOfSupplyListProvider.GetPlaceTypeFromPlaceCode(invoiceLine.Company, invoiceLine.AL_PlaceOfSupply), invoiceLine.AL_PlaceOfSupplyType);
					AssertEquals("AL_SupplyType", invoiceLine.AL_SupplyType, creditNoteLine.AL_SupplyType);
				};

			creditNoteLine.SetValues(invoiceLine, convertAmoutSignsBetweenTransactionTypes: false);
			assertValues(true);

			creditNoteLine.SetValues(invoiceLine, convertAmoutSignsBetweenTransactionTypes: true);
			assertValues(false);

			invoiceLine.AL_TaxDate = ZDate.Empty;
			expectedDate = ZDate.Today;
			creditNoteLine.SetValues(invoiceLine, convertAmoutSignsBetweenTransactionTypes: true);
			AssertEquals("AL_AT", invoiceLine.AL_AT, creditNoteLine.AL_AT);
			AssertEquals("AL_TaxDate", expectedDate, creditNoteLine.AL_TaxDate);
		}

		#endregion

		public void TestSetValues_SetSupplyTypeWillNotChangeTaxRate()
		{
			GlbCompany.CurrentCompany.GC_IsGSTRegistered = true;

			var job = CreateJob("Z00001011", AALSHI, true, 5M, ABIGAS, true, 10M);
			TestObjectCreator.FRT.TaxOverrides.RemoveAndDeleteAll();
			TestObjectCreator.CreateTaxOverrides(TestObjectCreator.FRT
				, taxOverride => {
					taxOverride.AO_AT = TestObjectCreator.GST2.PK;
					taxOverride.AO_SupplyType = AccountingMasterFilesConstants.SupplyTypeClassificationCodes.LOC;
				});
			Factory.Save();

			var invoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), "AP001", TestObjectCreator.AUD, 1m, TestObjectCreator.AALSHI);
			invoice.Lines.AddNew();
			invoice.Lines[0].GenericCharge = TestObjectCreator.FRT.PK;
			invoice.Lines[0].AL_JH = job.PK;
			invoice.Lines[0].AL_SupplyType = AccountingMasterFilesConstants.SupplyTypeClassificationCodes.LOC;
			AssertEquals(TestObjectCreator.GST2.PK, invoice.Lines[0].AL_AT);
			invoice.Lines[0].AL_AT = GST1.PK;
			AssertEquals(GST1.PK, invoice.Lines[0].AL_AT);

			var invoice2 = TestObjectCreator.CreateInvoice(typeof(APInvoice), "AP002", TestObjectCreator.AUD, 1m, TestObjectCreator.AALSHI);
			invoice2.Lines.AddNew();
			invoice2.Lines[0].SetValues(invoice.Lines[0], convertAmoutSignsBetweenTransactionTypes: true);
			AssertEquals("Tax Rate should still be GST1", GST1.PK, invoice2.Lines[0].AL_AT);
		}

		#region Implementation

		TestObjectCreator TestObjectCreator;

		#region Create Business Objects

		protected Job CreateJob(ZString jobNumber, OrgHeader localClient, bool billLocalClientInLocalCurrency, decimal localClientCFX,
			OrgHeader agent, bool billAgentInLocalCurrency, decimal agentCFX)
		{
			Job job = Factory.NewJobForTesting<Job>();
			job.JH_JobNum = jobNumber;
			job.JH_GB = GlbBranch.CurrentBranch.PK;
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			job.LocalChargesPK = localClient.PK;
			job.AgentCollectPK = agent.PK;
			job.JH_LocalChargesCFX = localClientCFX;
			job.JH_AgentChargesCFX = agentCFX;
			return job;
		}

		protected ExchangeRate CreateExchangeRate(Job parentJob, RefCurrency currency, decimal buyRate)
		{
			ExchangeRate exchangeRate = parentJob.ExchangeRates.AddNew();
			exchangeRate.JF_RX_NKRateCurrency = currency.RX_Code;
			exchangeRate.JF_BaseRate = buyRate;
			return exchangeRate;
		}

		protected Charge CreateCharge(Job parentJob, AccChargeCode chargeCode, ZString desc, RefCurrency costCurrency, ZDecimal oSCostAmt, OrgHeader creditor,
			RefCurrency sellCurrency, ZDecimal oSSellAmt, OrgHeader debtor, ZString costPlaceOfSupply = default)
		{
			Charge charge = parentJob.Charges.AddNew();
			charge.JR_AC = chargeCode.PK;
			charge.JR_Desc = desc;

			charge.JR_OH_CostAccount = creditor != null ? creditor.PK : ZGuid.Empty;
			charge.JR_RX_NKCostCurrency = costCurrency.RX_Code;
			charge.JR_OSCostAmt = oSCostAmt;

			charge.JR_OH_SellAccount = debtor.PK;
			charge.JR_RX_NKSellCurrency = sellCurrency.RX_Code;
			charge.JR_CostPlaceOfSupply = costPlaceOfSupply;
			charge.JR_OSSellAmt = oSSellAmt;
			return charge;
		}

		protected AccTaxRate CreateTaxRate(string code, string description, int rate, AccInvMsg taxMsg)
		{
			AccTaxRate taxRate = Factory.New<AccTaxRate>();
			taxRate.AT_Code = code;
			taxRate.AT_Description = description;
			taxRate.AT_IsActive = true;
			taxRate.AT_A9_DefaultVatClass = taxMsg.PK;
			taxRate.AT_RN_NKCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			taxRate.SetRateNumerator_ForTestOnly(rate);
			return taxRate;
		}

		protected AccInvMsg CreateTaxMsg(string code, string description, string englishMessage, string localMessage)
		{
			AccInvMsg taxMsg = Factory.New<AccInvMsg>();
			taxMsg.A9_Code = code;
			taxMsg.A9_Description = description;
			taxMsg.A9_IsActive = true;
			taxMsg.A9_EnglishMsg = englishMessage;
			taxMsg.A9_LocalMsg = localMessage;
			taxMsg.A9_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			return taxMsg;
		}

		protected AccWithholding CreateWithholdingTax(string code, string description, decimal rate)
		{
			AccWithholding taxRate = Factory.New<AccWithholding>();
			taxRate.AW_Code = code;
			taxRate.AW_Description = description;
			taxRate.AW_IsActive = true;
			taxRate.AW_Rate = rate;
			taxRate.AW_GC = GlbCompany.CurrentCompany.PK;
			return taxRate;
		}

		protected AccChargeCode CreateChargeCode(string code, string description, string chargeType, decimal marginPercentage, AccTaxRate gST, AccWithholding wHT)
		{
			AccChargeCode chargeCode = Factory.New<AccChargeCode>();
			chargeCode.AC_Code = code;
			chargeCode.AC_Desc = description;
			chargeCode.AC_ChargeType = chargeType;
			chargeCode.AC_MarginPercentage = marginPercentage;
			chargeCode.AC_AT_GSTRate = gST.PK;
			chargeCode.AC_AW_WithholdingTaxRate = wHT.PK;
			return chargeCode;
		}

		#endregion

		#region AUD

		protected RefCurrency fAUD;
		protected RefCurrency AUD
		{
			get
			{
				if (fAUD == null)
				{
					fAUD = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "AUD");
				}
				return fAUD;
			}
		}

		#endregion

		#region USD

		protected RefCurrency fUSD;
		protected RefCurrency USD
		{
			get
			{
				if (fUSD == null)
				{
					fUSD = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "USD");
				}
				return fUSD;
			}
		}

		#endregion

		#region ABIGAS

		protected OrgHeader fABIGAS;
		protected OrgHeader ABIGAS
		{
			get
			{
				if (fABIGAS == null)
				{
					fABIGAS = Factory.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, "ABIGAS");
				}
				return fABIGAS;
			}
		}

		#endregion

		#region AALSHI

		protected OrgHeader fAALSHI;
		protected OrgHeader AALSHI
		{
			get
			{
				if (fAALSHI == null)
				{
					fAALSHI = Factory.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, "AALSHI");
				}
				return fAALSHI;
			}
		}

		#endregion

		#region ZECTRA

		protected OrgHeader fZECTRA;
		protected OrgHeader ZECTRA
		{
			get
			{
				if (fZECTRA == null)
				{
					fZECTRA = Factory.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, "ZECTRA");
				}
				return fZECTRA;
			}
		}

		#endregion

		#region MRG100

		protected AccChargeCode fMRG100;
		protected AccChargeCode MRG100
		{
			get
			{
				if (fMRG100 == null)
				{
					fMRG100 = CreateChargeCode("MRG100", "Margin 100 With GST & WHT", Core.Constants.ChargeType.Margin, 100, GST1, WHT1);
				}
				return fMRG100;
			}
		}

		#endregion

		#region DIS

		protected AccChargeCode fDIS;
		protected AccChargeCode DIS
		{
			get
			{
				if (fDIS == null)
				{
					fDIS = CreateChargeCode("DIS", "Disbursement With GST & WHT", Core.Constants.ChargeType.Disbursement, 100, GST1, WHT1);
				}
				return fDIS;
			}
		}

		#endregion

		#region GST1

		protected AccTaxRate fGST1;
		protected AccTaxRate GST1
		{
			get
			{
				if (fGST1 == null)
				{
					fGST1 = CreateTaxRate("GST1", "GST Rate 1", 10, TaxMsg1);
				}
				return fGST1;
			}
		}

		#endregion

		#region WHT1

		protected AccWithholding fWHT1;
		protected AccWithholding WHT1
		{
			get
			{
				if (fWHT1 == null)
				{
					fWHT1 = CreateWithholdingTax("WHT1", "WHT Rate 1", 5);
				}
				return fWHT1;
			}
		}

		#endregion

		protected AccInvMsg fTaxMsg1;
		protected AccInvMsg TaxMsg1
		{
			get
			{
				if (fTaxMsg1 == null)
				{
					fTaxMsg1 = CreateTaxMsg("MSG1", "Tax Message 1", "English Message 1", "Local Message 1");
				}
				return fTaxMsg1;
			}
		}

		#endregion
	}
}
