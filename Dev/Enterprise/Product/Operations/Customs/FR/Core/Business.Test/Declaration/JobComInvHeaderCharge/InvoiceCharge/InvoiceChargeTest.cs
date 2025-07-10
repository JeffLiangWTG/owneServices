using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Business.Declaration.Testing
{
	[TestedType(typeof(InvoiceCharge))]
	sealed class InvoiceChargeTest : EU.Business.Declaration.Testing.InvoiceChargeTest
	{
		public void TestVatibilityAtExport()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "EXP";
			var invoice = declaration.Invoices.AddNew();
			var nonTransportCharge = invoice.Charges.AddNew();
			AssertEquals("Prerequisite.", false, nonTransportCharge.J7_IsDutiable);
			AssertEquals("Non transport charge vatibility should follow dutiability (EU behaviour).", false, nonTransportCharge.J7_IsGSTApplicable);
			nonTransportCharge.J7_IsDutiable = true;
			AssertEquals("Non transport charge vatibility should follow dutiability (EU behaviour).", true, nonTransportCharge.J7_IsGSTApplicable);

			var transportCharge = invoice.Charges.AddNew();
			transportCharge.J7_ChargeType = FRCustomsChargeTypeList.Codes.AirInsuranceCostsCharge;
			AssertEquals("Prerequisite.", true, transportCharge.J7_IsDutiable);
			AssertEquals("Tansport charge vatibility should always be false at export.", false, transportCharge.J7_IsGSTApplicable);
			transportCharge.J7_IsDutiable = false;
			AssertEquals("Tansport charge vatibility should always be false at export.", false, transportCharge.J7_IsGSTApplicable);
		}

		public override void TestIsIncludedInLinesReadOnly()
		{
			Assert("FR has its specific rule for the logic of IntoTerm -> ChargeCode readonly/requires.", true);
		}

		public override void TestJ7_Calc_IsIncludedInInvoice()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var charge = invoice.Charges.AddNew();
			charge.J7_ChargeType = UCCCustomsChargeTypeList.Codes.AdjustmentCharge;
			AssertChargeFlags(charge, false, false, false, false, true);

			charge.J7_ChargeType = UCCCustomsChargeTypeList.Codes.BuyingCommissionsCharge;
			AssertChargeFlags(charge, true, true, false, false, true);

			charge.J7_ChargeType = UCCCustomsChargeTypeList.Codes.CommissionExceptBuyingCommissionsCharge;
			AssertChargeFlags(charge, false, false, true, true, true);

			charge.J7_ChargeType = UCCCustomsChargeTypeList.Codes.ContainersAndPackingCharge;
			AssertChargeFlags(charge, false, false, true, true, true);

			charge.J7_ChargeType = UCCCustomsChargeTypeList.Codes.EngineeringDevelopmentArtworkCharge;
			AssertChargeFlags(charge, false, false, true, true, true);

			charge.J7_ChargeType = UCCCustomsChargeTypeList.Codes.InterestCharge;
			AssertChargeFlags(charge, true, true, false, false, true);

			charge.J7_ChargeType = UCCCustomsChargeTypeList.Codes.ImportDutiesOrOtherCharge;
			AssertChargeFlags(charge, true, true, false, false, false);

			charge.J7_ChargeType = UCCCustomsChargeTypeList.Codes.MaterialsConsumedCharge;
			AssertChargeFlags(charge, false, false, true, true, true);

			charge.J7_ChargeType = UCCCustomsChargeTypeList.Codes.MaterialsComponentsPartsCharge;
			AssertChargeFlags(charge, false, false, true, true, true);

			charge.J7_ChargeType = UCCCustomsChargeTypeList.Codes.ProceedsOfAnySubsequentResaleCharge;
			AssertChargeFlags(charge, false, false, true, true, true);

			charge.J7_ChargeType = UCCCustomsChargeTypeList.Codes.RoyaltiesLicenseFeeCharge;
			AssertChargeFlags(charge, false, false, true, true, true);
		}

		void AssertChargeFlags(InvoiceCharge charge, bool isIncludedInInvoice, bool j7_IsIncludedInITOT, bool j7_IsDutiable, bool j7_IsStatisticalValueApplicable, bool j7_IsGSTApplicable)
		{
			CombineAssertions(() =>
			{
				AssertEquals("Included In Invoice", isIncludedInInvoice, !charge.J7_IsNotIncludedInInvoice);
				AssertEquals("Included In Invoice Line", j7_IsIncludedInITOT, charge.J7_IsIncludedInITOT);
				AssertEquals("Dutiable", j7_IsDutiable, charge.J7_IsDutiable);
				AssertEquals("Statable", j7_IsStatisticalValueApplicable, charge.J7_IsStatisticalValueApplicable);
				AssertEquals("VATible", j7_IsGSTApplicable, charge.J7_IsGSTApplicable);
			});
		}

		public override void TestReadOnlyOfIsIncludedInInvoice()
		{
			Assert("FR has its specific rule for the logic of IntoTerm -> ChargeCode readonly/requires.", true);
		}

		public override void TestJ7_IsNotIncludedInInvoiceSetOnFactorySaving()
		{
			Assert("FR has its specific rule for the logic of IntoTerm -> ChargeCode readonly/requires.", true);
		}

		public void TestInvoiceHeaderBalanceRecalculatedIfJ7_AmountEditedForChargeCodeCUT()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_RX_NKInvoice_Currency = declaration.LocalCurrencyCode;
			invoiceHeader.JZ_IncoTerm = "FOB";
			invoiceHeader.JZ_InvoiceAmount = 573m;
			var invoiceLine1 = invoiceHeader.InvoiceLines.AddNew();
			var invoiceLine2 = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine1.JI_LinePrice = 273m;
			invoiceLine2.JI_LinePrice = 381m;
			Factory.Save();

			invoiceHeader.Reload();
			AssertEquals("Invoice Header Balance should not be zero", -81m, invoiceHeader.JZ_Calc_Balance);

			var chargeCode = invoiceHeader.Charges.AddNew();
			chargeCode.J7_ChargeType = FRCustomsChargeTypeList.Codes.Cut;
			chargeCode.J7_IsIncludedInITOT = true;
			chargeCode.J7_DistributeBy = "VAL";
			chargeCode.J7_RX_NKCurrency = invoiceHeader.JobDeclaration.LocalCurrencyCode;

			chargeCode.J7_Amount = 20m;
			Assert("Declaration should be dirty if CUT amout changed", declaration.ApportionmentDirty);
			declaration.ResumeApportionment();
			AssertEquals("Invoice Header Balance refreshed when CUT amount edited", -61m, invoiceHeader.JZ_Calc_Balance);

			chargeCode.J7_Amount = 81m;
			declaration.ResumeApportionment();
			AssertEquals("Invoice Header Balance refreshed when CUT amount edited", 0m, invoiceHeader.JZ_Calc_Balance);

			chargeCode.J7_ChargeType = FRCustomsChargeTypeList.Codes.Additions71Charge;
			declaration.ResumeApportionment();
			AssertEquals("Invoice Header Balance refreshed when ChargeType edited from CUT", -81m, invoiceHeader.JZ_Calc_Balance);

			chargeCode.J7_ChargeType = FRCustomsChargeTypeList.Codes.Cut;
			declaration.ResumeApportionment();
			AssertEquals("Invoice Header Balance refreshed when ChargeType edited to CUT", 0m, invoiceHeader.JZ_Calc_Balance);

			invoiceHeader.Charges.RemoveAndDelete(chargeCode);
			declaration.ResumeApportionment();
			AssertEquals("Invoice Header Balance refreshed when CUT ChargeCode delelted", -81m, invoiceHeader.JZ_Calc_Balance);
		}

		public void TestJ7_ChargeType_Readonly()
		{
			var invoiceCharge = Factory.New<InvoiceCharge>();
			var chargeTypeInfo = invoiceCharge.J7_ChargeTypeInfo;
			AssertEquals("J7_ChargeType should be writable for new FR InvoiceCharge", false, chargeTypeInfo.ReadOnly);

			invoiceCharge.J7_ChargeType = UCCCustomsChargeTypeList.Codes.BuyingCommissionsCharge;
			AssertEquals("J7_ChargeType should be writable for none-system InvoiceCharge", false, chargeTypeInfo.ReadOnly);

			invoiceCharge.J7_IsCalculated = true;
			AssertEquals("J7_ChargeType should be readonly for system InvoiceCharge", true, chargeTypeInfo.ReadOnly);
		}

		public void TestJ7_Percentage_Readonly()
		{
			var invoiceCharge = Factory.New<InvoiceCharge>();
			var percentageInfo = invoiceCharge.J7_PercentageInfo;
			AssertEquals("7_Percentage should be writable for new FR InvoiceCharge", false, percentageInfo.ReadOnly);

			invoiceCharge.J7_ChargeType = UCCCustomsChargeTypeList.Codes.BuyingCommissionsCharge;
			AssertEquals("7_Percentage should be writable for none-system BCM InvoiceCharge", false, percentageInfo.ReadOnly);

			invoiceCharge.J7_IsCalculated = true;
			AssertEquals("7_Percentage should be readonly for system BCM InvoiceCharge", true, percentageInfo.ReadOnly);
		}

		public void TestJ7_Amount_Readonly()
		{
			var invoiceCharge = Factory.New<InvoiceCharge>();
			var amountInfo = invoiceCharge.J7_AmountInfo;
			AssertEquals("J7_Amount should be writable for new FR InvoiceCharge", false, amountInfo.ReadOnly);

			invoiceCharge.J7_ChargeType = UCCCustomsChargeTypeList.Codes.BuyingCommissionsCharge;
			AssertEquals("J7_Amount should be writable for none-system BCM InvoiceCharge", false, amountInfo.ReadOnly);

			invoiceCharge.J7_IsCalculated = true;
			AssertEquals("J7_Amount should be readonly for system BCM InvoiceCharge", true, amountInfo.ReadOnly);
		}

		public void TestJ7_Calc_IsIncludedInInvoiceAmountReadOnly()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();
			var chargeCode = invoiceHeader.Charges.AddNew();
			chargeCode.J7_ChargeType = UCCCustomsChargeTypeList.Codes.BuyingCommissionsCharge;
			AssertEquals("IsIncludedInInvoiceAmount should be readonly when J7_ChargeType is BCM.", true, chargeCode.J7_Calc_IsIncludedInInvoiceAmountInfo.ReadOnly);
			chargeCode.J7_ChargeType = UCCCustomsChargeTypeList.Codes.OtherNotElsewhereDeclaredCharge;
			AssertEquals("IsIncludedInInvoiceAmount should be writable when J7_ChargeType is not BCM.", false, chargeCode.J7_Calc_IsIncludedInInvoiceAmountInfo.ReadOnly);
		}

		public void TestIsSystemCalculated_Caption()
		{
			var invoiceCharge = Factory.New<InvoiceCharge>();
			var caption = DataBoundResourceStrings.GetDataForProperty(invoiceCharge.IsSystemCalculatedInfo);
			AssertEquals("Caption", "System Calculated", caption.Caption);
		}
	}
}
