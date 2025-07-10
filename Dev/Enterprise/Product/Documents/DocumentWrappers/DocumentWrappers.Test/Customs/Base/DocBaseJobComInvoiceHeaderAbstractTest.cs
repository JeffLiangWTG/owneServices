using System;
using System.Reflection;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.Business;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using CustomsChargeTypeList = Enterprise.Customs.AU.Declaration.Business.CustomsChargeTypeList;
using ExchangeRate = Enterprise.Accounting.Business.JobInvoicing.ExchangeRate;
using Job = Enterprise.Accounting.Business.JobInvoicing.Job;

namespace Enterprise.DocumentWrappers.Customs.Base.Testing
{
	[TestsSubclassesOf(typeof(DocBaseJobComInvoiceHeader))]
	public abstract class DocBaseJobComInvoiceHeaderAbstractTest<T, TWrapper> : DocumentWrapperTestCase
			where T : BaseJobComInvoiceHeader
			where TWrapper : DocBaseJobComInvoiceHeader
	{
		public void TestToString()
		{
			InvoiceHeaderInternal.JZ_InvoiceNumber = "InvoiceNumber";
			AssertEquals("ToString()", InvoiceHeaderInternal.JZ_InvoiceNumber, InvoiceHeaderWrapperInternal.ToString());
		}

		#region Abstract
		public abstract void TestIncoTermDescription();

		protected abstract TWrapper CreateInvoiceHeaderWrapper(T invoiceHeaderInternal);

		#endregion

		#region Virtual

		public virtual void TestTariffHeading()
		{
			AssertEquals("TariffHeading", "Tariff", InvoiceHeaderWrapperInternal.TariffHeading);
		}

		#endregion

		#region ZDecimal Fields

		public abstract void TestConversionFactorIsWrapped();

		public virtual void TestTotalDutiableChargesNotIncludedInLinesCanBeSet()
		{
			InvoiceHeaderInternal.Charges.RemoveAll();
			BaseJobComInvHeaderCharge nonDutiable1 = InvoiceHeaderInternal.Charges.AddNew(CustomsChargeTypeList.Codes.ForeignInlandFreight, 100m, GlbCompany.CurrentCompany.Country.RN_RX_NKLocalCurrency);

			ZDecimal expectedNonDutiable = 100m;
			AssertEquals(InvoiceHeaderWrapperInternal.TotalDutiableChargesNotIncludedInLines, expectedNonDutiable);
		}

		public void TestTotalDutiableChargesNotIncludedInLinesInLocalCurrency()
		{
			BaseJobComInvoiceHeader header = InvoiceHeaderInternal;

			header.Charges.RemoveAll();
			header.JZ_RX_NKInvoice_Currency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			BaseJobComInvHeaderCharge nonDutiable1 = header.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasInsurance, 100m, header.Invoice_Currency.RX_Code);
			nonDutiable1.J7_IsIncludedInITOT = true;

			BaseJobComInvHeaderCharge nonDutiable2 = header.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 250m, header.Invoice_Currency.RX_Code);
			nonDutiable2.J7_IsIncludedInITOT = true;

			AssertEquals(InvoiceHeaderWrapperInternal.TotalDutiableChargesNotIncludedInLinesInLocalCurrency, header.DutiableChargesNotIncludedInLinesInLocalCurrency.Amount);
		}

		public void TestTotalNonDutiableChargesNotIncludedInLineExcludingFreightAndInsuranceCanBeSet()
		{
			BaseJobComInvoiceHeader header = InvoiceHeaderInternal;

			header.Charges.RemoveAll();
			BaseJobComInvHeaderCharge nonDutiable1 = header.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasInsurance, 100m, GlbCompany.CurrentCompany.Country.RN_RX_NKLocalCurrency);
			nonDutiable1.J7_IsIncludedInITOT = false;

			BaseJobComInvHeaderCharge nonDutiable2 = header.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 250m, GlbCompany.CurrentCompany.Country.RN_RX_NKLocalCurrency);
			nonDutiable2.J7_IsIncludedInITOT = false;

			BaseJobComInvHeaderCharge nonDutiable3 = header.Charges.AddNew(CustomsChargeTypeList.Codes.ExWorks, 300m, GlbCompany.CurrentCompany.Country.RN_RX_NKLocalCurrency);
			nonDutiable3.J7_IsDutiable = false;
			nonDutiable3.J7_IsIncludedInITOT = false;

			ZDecimal expectedNonDutiable = 300m;
			AssertEquals(InvoiceHeaderWrapperInternal.TotalNonDutiableChargesNotIncludedInLineExcludingFreightAndInsurance, expectedNonDutiable);
		}

		public void TestTotalNonDutiableChargesNotIncludedInLineExcludingFreightAndInsuranceInLocalCurrency()
		{
			BaseJobComInvoiceHeader header = InvoiceHeaderInternal;

			header.Charges.RemoveAll();
			header.JZ_RX_NKInvoice_Currency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			BaseJobComInvHeaderCharge nonDutiable1 = header.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasInsurance, 100m, header.Invoice_Currency.RX_Code);
			nonDutiable1.J7_IsIncludedInITOT = false;

			BaseJobComInvHeaderCharge nonDutiable2 = header.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 250m, header.Invoice_Currency.RX_Code);
			nonDutiable2.J7_IsIncludedInITOT = false;

			BaseJobComInvHeaderCharge nonDutiable3 = header.Charges.AddNew(CustomsChargeTypeList.Codes.ExWorks, 300m, GlbCompany.CurrentCompany.Country.RN_RX_NKLocalCurrency);
			nonDutiable3.J7_IsDutiable = false;
			nonDutiable3.J7_IsIncludedInITOT = false;

			AssertEquals(InvoiceHeaderWrapperInternal.TotalNonDutiableChargesNotIncludedInLineExcludingFreightAndInsuranceInLocalCurrency, header.NonDutiableChargesNotIncludedInLinesExcludingFreightAndInsuranceInLocalCurrency.Amount);
		}

		public void TestTotalFreightExRateCanBeAccessedByDocWrapper()
		{
			BaseJobComInvoiceHeader header = InvoiceHeaderInternal;
			AssertEquals(header.OverseasFreight.Currency.Code, InvoiceHeaderWrapperInternal.OverseasFreightCurrency.Code);
		}

		public virtual void TestTotalIncludedCosts()
		{
			InvoiceHeaderInternal.JZ_RX_NKInvoice_Currency = GlbCompany.CurrentCompany.Country.RN_RX_NKLocalCurrency;
			InvoiceHeaderInternal.Charges.AddNew(AUChargeCodeList.Codes.OverseasFreight, 20m, GlbCompany.CurrentCompany.Country.RN_RX_NKLocalCurrency);
			InvoiceHeaderInternal.Charges.AddNew(AUChargeCodeList.Codes.OverseasInsurance, 20.5m, GlbCompany.CurrentCompany.Country.RN_RX_NKLocalCurrency);
			InvoiceHeaderInternal.Charges.AddNew(AUChargeCodeList.Codes.ForeignInlandFreight, 0m, GlbCompany.CurrentCompany.Country.RN_RX_NKLocalCurrency);
			InvoiceHeaderInternal.Charges.AddNew(AUChargeCodeList.Codes.PackingCost, 32.5m, GlbCompany.CurrentCompany.Country.RN_RX_NKLocalCurrency);
			InvoiceHeaderInternal.Charges.AddNew(AUChargeCodeList.Codes.LandingCharges, 22m, GlbCompany.CurrentCompany.Country.RN_RX_NKLocalCurrency);
			InvoiceHeaderInternal.Charges.AddNew(AUChargeCodeList.Codes.OtherCharges, 5m, GlbCompany.CurrentCompany.Country.RN_RX_NKLocalCurrency);
			InvoiceHeaderInternal.Charges.AddNew(AUChargeCodeList.Codes.BuyingCommission, 10m, GlbCompany.CurrentCompany.Country.RN_RX_NKLocalCurrency);

			AssertEquals(100.00M, InvoiceHeaderWrapperInternal.TotalIncludedCosts);

			InvoiceHeaderInternal.Charges.AddNew(AUChargeCodeList.Codes.Discount, 50m, GlbCompany.CurrentCompany.Country.RN_RX_NKLocalCurrency);

			AssertEquals(50.0M, InvoiceHeaderWrapperInternal.TotalIncludedCosts);
		}

		public virtual void TestDiscountOrSurcharge()
		{
			InvoiceHeaderInternal.JZ_RX_NKInvoice_Currency = GlbCompany.CurrentCompany.Country.RN_RX_NKLocalCurrency;
			InvoiceHeaderInternal.Charges.AddNew(AUChargeCodeList.Codes.Discount, 10m, GlbCompany.CurrentCompany.Country.RN_RX_NKLocalCurrency);

			AssertEquals("Discount", InvoiceHeaderWrapperInternal.DiscountOrSurchargeText);
			AssertEquals(-10.0M, InvoiceHeaderWrapperInternal.DiscountOrSurcharge);

			InvoiceHeaderInternal.Charges[0].J7_Amount = -10.0M;
			InvoiceHeaderInternal.Charges[0].J7_IsIncludedInITOT = true;

			AssertEquals("Surcharge", InvoiceHeaderWrapperInternal.DiscountOrSurchargeText);
			AssertEquals(10.0M, InvoiceHeaderWrapperInternal.DiscountOrSurcharge);
		}

		public void TestInvoiceLineTotal()
		{
			AssertEquals("InvoiceLineTotal", InvoiceHeaderInternal.InvoiceLineTotal, InvoiceHeaderWrapperInternal.InvoiceLineTotal);
		}

		public void TestTNI()
		{
			AssertEquals("TNI", InvoiceHeaderInternal.JZ_Calc_TNI, InvoiceHeaderWrapperInternal.TNI);
		}

		public void TestCalcOverseasFreight()
		{
			AssertCharges(CustomsChargeTypeList.Codes.OverseasFreight, "OverseasFreight", "OverseasFreightCurrency");
		}

		public void TestCalcOverseasInsurance()
		{
			AssertCharges(CustomsChargeTypeList.Codes.OverseasInsurance, "OverseasInsurance", "OverseasInsuranceCurrency");
		}

		public void TestCalcLandingCharges()
		{
			AssertCharges(CustomsChargeTypeList.Codes.LandingCharges, "LandingCharges", "LandingChargesCurrency");
		}

		public void TestCalcExWorks()
		{
			AssertCharges(CustomsChargeTypeList.Codes.ExWorks, "ExWorks", "ExWorksCurrency");
		}

		public void TestCalcForeignInlandFreight()
		{
			AssertCharges(CustomsChargeTypeList.Codes.ForeignInlandFreight, "ForeignInlandFreight", "ForeignInlandFreightCurrency");
		}

		public void TestCalcPackingCosts()
		{
			AssertCharges(CustomsChargeTypeList.Codes.PackingCost, "PackingCosts", "PackingCostsCurrency");
		}

		public void TestCalcOtherCharges1()
		{
			AssertCharges(CustomsChargeTypeList.Codes.OtherCharges, "OtherCharges1", "OtherCharges1Currency");
		}

		public void TestCalcOtherCharges2()
		{
			AssertCharges(CustomsChargeTypeList.Codes.OtherCharges, "OtherCharges2", "OtherCharges2Currency");
		}

		public void TestCalcDiscount()
		{
			AssertCharges(CustomsChargeTypeList.Codes.Discount, "Discount", "DiscountCurrency");
		}

		public void TestCalcCommission()
		{
			AssertCharges(CustomsChargeTypeList.Codes.Commission, "Commission", "CommissionCurrency");
		}

		public void TestBalance()
		{
			InvoiceHeaderInternal.JZ_InvoiceAmount = 10000;
			InvoiceHeaderInternal.JZ_RX_NKInvoice_Currency = GlbCompany.CurrentCompany.Country.RN_RX_NKLocalCurrency;
			BaseJobComInvoiceLine invLine1 = InvoiceHeaderInternal.JobComInvoiceLines.AddNew();
			invLine1.JI_LinePrice = 1000;
			BaseJobComInvoiceLine invLine2 = InvoiceHeaderInternal.JobComInvoiceLines.AddNew();
			invLine2.JI_LinePrice = 2000;
			BaseJobComInvoiceLine invLine3 = InvoiceHeaderInternal.JobComInvoiceLines.AddNew();
			invLine3.JI_LinePrice = 3000;
			InvoiceHeaderInternal.JobDeclaration?.ResumeApportionment();
			AssertEquals("PreCondition:JZ_Calc_Balance", 4000m, InvoiceHeaderInternal.JZ_Calc_Balance);
			AssertEquals("Balance", InvoiceHeaderInternal.JZ_Calc_Balance, InvoiceHeaderWrapperInternal.Balance);
		}

		public void TestCIFAmount()
		{
			InvoiceHeaderInternal.JZ_IncoTerm = Core.Constants.IncoTerms.CostInsuranceAndFreight;
			InvoiceHeaderInternal.JZ_InvoiceAmount = 1000m;
			InvoiceHeaderInternal.JZ_RX_NKInvoice_Currency = GlbCompany.CurrentCompany.Country.RN_RX_NKLocalCurrency;
			AssertEquals("CIF amount", InvoiceHeaderInternal.JZ_Calc_CIFAmount, InvoiceHeaderWrapperInternal.CIFAmount);
		}

		//		public void TestCommission()
		//		{
		//			InvoiceHeader.Charges.AddNew(CustomsChargeTypeList.Codes.Commission, 12.32m, GlbCompany.CurrentCompany.CountryCode.RN_RX_NKLocalCurrency);
		//			AssertEquals("Commission", InvoiceHeader.Charges[0].J7_Amount, InvoiceHeaderWrapper.Commission);
		//		}
		//
		//		public void TestDiscount()
		//		{
		//			InvoiceHeader.Charges.AddNew(CustomsChargeTypeList.Codes.Discount, 12.32m, GlbCompany.CurrentCompany.CountryCode.RN_RX_NKLocalCurrency);
		//			AssertEquals("Commission", InvoiceHeader.Charges[0].J7_Amount, InvoiceHeaderWrapper.Discount);
		//		}
		//
		//		public void TestExWorksAmount()
		//		{
		//			InvoiceHeader.JZ_ExWorksAmount = 12.32M;
		//			AssertEquals("ExWorksAmount", InvoiceHeader.JZ_ExWorksAmount, InvoiceHeaderWrapper.ExWorksAmount);
		//		}
		//
		public void TestFOBAmount()
		{
			BaseJobComInvoiceHeader header = InvoiceHeaderInternal;
			header.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			header.JZ_InvoiceAmount = 1000m;
			header.JZ_RX_NKInvoice_Currency = GlbCompany.CurrentCompany.Country.RN_RX_NKLocalCurrency;
			AssertEquals("FOB amount", header.JZ_Calc_FOBAmount, InvoiceHeaderWrapperInternal.FOBAmount);
		}

		//		public void TestForeignInlandFreight()
		//		{
		//			InvoiceHeader.JZ_ForeignInlandFreight = 12.32M;
		//			AssertEquals("ForeignInlandFreight", InvoiceHeader.JZ_ForeignInlandFreight, InvoiceHeaderWrapper.ForeignInlandFreight);
		//		}

		public void TestInvoiceAmount()
		{
			InvoiceHeaderInternal.JZ_InvoiceAmount = 12.32M;
			InvoiceHeaderInternal.JZ_RX_NKInvoice_Currency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			AssertEquals("InvoiceAmount", InvoiceHeaderInternal.JZ_InvoiceAmount, InvoiceHeaderWrapperInternal.InvoiceAmount);
		}

		public void TestInvoiceCurrExRate()
		{
			InvoiceHeaderInternal.JZ_InvoiceCurrExRate = 12.32M;
			InvoiceHeaderInternal.JZ_RX_NKInvoice_Currency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			AssertEquals("InvoiceCurrExRate", InvoiceHeaderWrapperInternal.InvoiceCurrExRate, InvoiceHeaderInternal.JZ_InvoiceCurrExRate);
		}

		//		public void TestLandingCharges()
		//		{
		//			InvoiceHeader.JZ_LandingCharges = 12.32M;
		//			AssertEquals("LandingCharges", InvoiceHeader.JZ_LandingCharges, InvoiceHeaderWrapper.LandingCharges);
		//		}
		//
		//		public void TestNonDutiablePreFOBCharges()
		//		{
		//			InvoiceHeader.JZ_NonDutiablePreFOBCharges = 12.32M;
		//			AssertEquals("NonDutiablePreFOBCharges", InvoiceHeader.JZ_NonDutiablePreFOBCharges, InvoiceHeaderWrapper.NonDutiablePreFOBCharges);
		//		}
		//
		//		public void TestOtherCharges1()
		//		{
		//			InvoiceHeader.JZ_OtherCharges1 = 12.32M;
		//			AssertEquals("OtherCharges1", InvoiceHeader.JZ_OtherCharges1, InvoiceHeaderWrapper.OtherCharges1);
		//		}
		//
		//		public void TestOtherCharges2()
		//		{
		//			InvoiceHeader.JZ_OtherCharges2 = 12.32M;
		//			AssertEquals("OtherCharges2", InvoiceHeader.JZ_OtherCharges2, InvoiceHeaderWrapper.OtherCharges2);
		//		}
		//
		//		public void TestOverseasFreight()
		//		{
		//			InvoiceHeader.JZ_OverseasFreight = 12.32M;
		//			AssertEquals("OverseasFreight", InvoiceHeader.JZ_OverseasFreight, InvoiceHeaderWrapper.OverseasFreight);
		//		}
		//
		//		public void TestOverseasInsurance()
		//		{
		//			InvoiceHeader.JZ_OverseasInsurance = 12.32M;
		//			AssertEquals("OverseasInsurance", InvoiceHeader.JZ_OverseasInsurance, InvoiceHeaderWrapper.OverseasInsurance);
		//		}
		//
		//		public void TestPackingCosts()
		//		{
		//			InvoiceHeader.JZ_PackingCosts = 12.32M;
		//			AssertEquals("PackingCosts", InvoiceHeader.JZ_PackingCosts, InvoiceHeaderWrapper.PackingCosts);
		//		}

		public void TestPaymentAmount()
		{
			InvoiceHeaderInternal.JZ_PaymentAmount = 12.32M;
			InvoiceHeaderInternal.JZ_RX_NKInvoice_Currency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			AssertEquals("PaymentAmount", InvoiceHeaderInternal.JZ_PaymentAmount, InvoiceHeaderWrapperInternal.PaymentAmount);
		}

		public void TestPaymentExRate()
		{
			InvoiceHeaderInternal.JZ_PaymentExRate = 12.32M;
			InvoiceHeaderInternal.JZ_RX_NKInvoice_Currency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			AssertEquals("PaymentExRate", InvoiceHeaderInternal.JZ_PaymentExRate, InvoiceHeaderWrapperInternal.PaymentExRate);
		}

		public void TestVolume()
		{
			InvoiceHeaderInternal.JZ_Volume = 12.32M;
			InvoiceHeaderInternal.JZ_RX_NKInvoice_Currency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			AssertEquals("Volume", InvoiceHeaderInternal.JZ_Volume, InvoiceHeaderWrapperInternal.Volume);
		}

		public void TestWeight()
		{
			InvoiceHeaderInternal.JZ_Weight = 12.32M;
			InvoiceHeaderInternal.JZ_RX_NKInvoice_Currency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			AssertEquals("Weight", InvoiceHeaderInternal.JZ_Weight, InvoiceHeaderWrapperInternal.Weight);
		}

		public void TestLinesEntered()
		{
			BaseJobComInvoiceLine invLine1 = InvoiceHeaderInternal.JobComInvoiceLines.AddNew();
			invLine1.JI_LinePrice = 1000;
			BaseJobComInvoiceLine invLine2 = InvoiceHeaderInternal.JobComInvoiceLines.AddNew();
			invLine2.JI_LinePrice = 2000;
			BaseJobComInvoiceLine invLine3 = InvoiceHeaderInternal.JobComInvoiceLines.AddNew();
			invLine3.JI_LinePrice = 3000;
			AssertEquals("PreCondition: Lines Entered", 6000m, InvoiceHeaderInternal.JZ_Calc_LinesEntered);
			AssertEquals("LinesEntered", InvoiceHeaderInternal.JZ_Calc_LinesEntered, InvoiceHeaderWrapperInternal.LinesEntered);
		}

		public void TestInvoiceCurrExRateFallBackToJobExRate()
		{
			Env.Registry.LandedCostingFallbackExRatesToJobInvoicing = true;

			InvoiceHeaderInternal.JZ_PaymentExRate = 0.6m;
			InvoiceHeaderInternal.JZ_InvoiceCurrExRate = 0.7m;
			AssertEquals("JZ_PaymentExRate when fallback not required", 0.6m, InvoiceHeaderWrapperInternal.InvoiceCurrExRateFallBackToJobExRate);

			InvoiceHeaderInternal.JZ_PaymentExRate = 0m;
			InvoiceHeaderInternal.JZ_InvoiceCurrExRate = 0.7m;
			AssertEquals("JZ_InvoiceCurrExRate when fallback not required", 0.7m, InvoiceHeaderWrapperInternal.InvoiceCurrExRateFallBackToJobExRate);

			InvoiceHeaderInternal.JZ_PaymentExRate = 0m;
			InvoiceHeaderInternal.JZ_InvoiceCurrExRate = 0m;
			AssertEquals("JZ_InvoiceCurrExRate when fallback required, but nothing to fall back on", 1m, InvoiceHeaderWrapperInternal.InvoiceCurrExRateFallBackToJobExRate);

			Job job = Factory.NewJobForTesting<Job>();
			job.JH_ParentID = InvoiceHeaderInternal.JobDeclaration.PK;
			job.JH_ParentTableCode = JobDeclarationSchema.Constants.Prefix;
			ExchangeRate jobInvoicingRate = job.ExchangeRates.AddNew();
			jobInvoicingRate.JF_RX_NKRateCurrency = "MKD";
			jobInvoicingRate.JF_BaseRate = 0.4m;

			AssertEquals("JZ_InvoiceCurrExRate when fallback required, but nothing to fall back on because currency not specified", 1m, InvoiceHeaderWrapperInternal.InvoiceCurrExRateFallBackToJobExRate);

			InvoiceHeaderInternal.JZ_RX_NKInvoice_Currency = "AUD";
			InvoiceHeaderInternal.JZ_InvoiceCurrExRate = 0m;
			AssertEquals("JZ_InvoiceCurrExRate when fallback required, but no currency to fall back on", 1m, InvoiceHeaderWrapperInternal.InvoiceCurrExRateFallBackToJobExRate);

			InvoiceHeaderInternal.JZ_RX_NKInvoice_Currency = "MKD";
			InvoiceHeaderInternal.JZ_InvoiceCurrExRate = 0m;
			AssertEquals("JZ_InvoiceCurrExRate when fallback required", 0.4m, InvoiceHeaderWrapperInternal.InvoiceCurrExRateFallBackToJobExRate);
		}

		public void TestInvoiceCurrExRateFallBackToJobExRate_FollowsFallbackRegistryItem()
		{
			InvoiceHeaderInternal.JZ_RX_NKInvoice_Currency = "MKD";
			InvoiceHeaderInternal.JZ_InvoiceCurrExRate = 0m;

			AssertExRateFallBackToJobExRate("InvoiceCurrExRateFallBackToJobExRate", 1m);
		}

		public void TestLandedCostingExRateFallBackToJobExRate()
		{
			InvoiceHeaderInternal.JZ_RX_NKInvoice_Currency = "MKD";
			InvoiceHeaderInternal.JZ_InvoiceCurrLandedCostExRate = 0m;

			AssertExRateFallBackToJobExRate("LandedCostingExRateFallBackToJobExRate", 0m);
		}

		public void TestLandedCostingExRateFallBackToJobExRate_Returns1ForLocalCurrency()
		{
			InvoiceHeaderInternal.JZ_RX_NKInvoice_Currency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			InvoiceHeaderInternal.JZ_InvoiceCurrLandedCostExRate = 0.3m;

			Env.Registry.LandedCostingFallbackExRatesToJobInvoicing = false;
			AssertEquals("Local currency should return 1m when no exchange rate is specified", 1m, InvoiceHeaderWrapperInternal.LandedCostingExRateFallBackToJobExRate);
		}

		protected void AssertExRateFallBackToJobExRate(string exchangeRatePropertyName, ZDecimal expectedExRateWhenNotFallingBack)
		{
			Job job = Factory.NewJobForTesting<Job>();
			job.JH_ParentID = InvoiceHeaderInternal.JobDeclaration.PK;
			job.JH_ParentTableCode = JobDeclarationSchema.Constants.Prefix;
			ExchangeRate jobInvoicingRate = job.ExchangeRates.AddNew();
			jobInvoicingRate.JF_RX_NKRateCurrency = "MKD";
			jobInvoicingRate.JF_BaseRate = 0.4m;

			Env.Registry.LandedCostingFallbackExRatesToJobInvoicing = true;
			AssertEquals("When fallback is enabled", 0.4m, InvoiceHeaderWrapperInternal[exchangeRatePropertyName]);

			Env.Registry.LandedCostingFallbackExRatesToJobInvoicing = false;
			AssertEquals("When fallback is NOT enabled", expectedExRateWhenNotFallingBack, InvoiceHeaderWrapperInternal[exchangeRatePropertyName]);
		}

		public void TestSupplierAndInvoiceNumberGroupByString()
		{
			var supplier = Factory.New<OrgHeader>();
			supplier.OH_FullName = "TESTNAME";
			InvoiceHeaderInternal.JZ_InvoiceNumber = "TESTINVOICE1";
			InvoiceHeaderInternal.JZ_OH_Supplier = supplier.PK;
			AssertEquals(supplier.OH_FullName + "__" + InvoiceHeaderInternal.JZ_InvoiceNumber, InvoiceHeaderWrapperInternal.SupplierAndInvoiceNumberGroupByString);
		}

		public void TestInvoiceAmountInLocalCurrency()
		{
			BaseJobComInvoiceHeader header = InvoiceHeaderInternal;
			header.JZ_InvoiceAmount = 150.00M;
			header.JZ_RX_NKInvoice_Currency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;

			AssertEquals("Invoice Amount in Local Currency", InvoiceHeaderWrapperInternal.InvoiceAmountInLocalCurrency, header.JZ_InvoiceAmountInLocalCurrency);
		}

		public void TestFOBInLocalCurrency()
		{
			InvoiceHeaderInternal.JZ_InvoiceAmount = 150.00M;
			InvoiceHeaderInternal.JZ_RX_NKInvoice_Currency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;

			AssertEquals("FOB Amount in ZAR", InvoiceHeaderWrapperInternal.FOBInLocalCurrency, InvoiceHeaderInternal.JZ_Calc_FOBAmountInLocalCurrency);
		}

		public void TestFOBInLocalCurrencyRounded()
		{
			InvoiceHeaderInternal.JZ_InvoiceAmount = 150.00M;
			InvoiceHeaderInternal.JZ_RX_NKInvoice_Currency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;

			AssertEquals("Rounded FOB Amount in ZAR", InvoiceHeaderWrapperInternal.FOBInLocalCurrencyRounded, InvoiceHeaderInternal.JZ_Calc_FOBAmountInLocalCurrencyRounded);
		}

		public void TestTotalInsuranceInLocalCurrency()
		{
			InvoiceHeaderInternal.Charges.RemoveAll();
			InvoiceHeaderInternal.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasInsurance, 100m, GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency);
			AssertEquals(InvoiceHeaderWrapperInternal.TotalOverseasInsuranceInLocalCurrency, InvoiceHeaderInternal.OverseasInsuranceInLocalCurrency.Amount);
		}

		public void TestTotalCIF_CInLocalCurrency()
		{
			SetTotalCIF(1000m, 100m, 10m, 0m, 200m, 50m, 0m, GlbCompany.CurrentCompany.LocalCurrency.PK);
			AssertEquals(InvoiceHeaderWrapperInternal.TotalCIFInLocalCurrency, InvoiceHeaderInternal.JZ_Calc_CIFAmount_InLocalCurrency);
		}

		public void TestTotalCIFLessFreightInLocalCurrency()
		{
			SetTotalCIF(1000m, 100m, 10m, 0m, 0m, 50m, 0m, GlbCompany.CurrentCompany.LocalCurrency.PK);
			AssertEquals(InvoiceHeaderWrapperInternal.TotalCIFLessFreightInLocalCurrency, InvoiceHeaderWrapperInternal.TotalCIFInLocalCurrency - InvoiceHeaderWrapperInternal.TotalOverseasFreightInLocalCurrency);
		}

		public void TestTotalCIFLessFreightLessInsuranceInLocalCurrency()
		{
			SetTotalCIF(1000m, 100m, 10m, 0m, 0m, 50m, 0m, GlbCompany.CurrentCompany.LocalCurrency.PK);
			AssertEquals(InvoiceHeaderWrapperInternal.TotalCIFLessFreightLessInsuranceInLocalCurrency, InvoiceHeaderWrapperInternal.TotalCIFLessFreightInLocalCurrency - InvoiceHeaderWrapperInternal.TotalOverseasInsuranceInLocalCurrency);
		}

		public void TestTotalCIFLessAllNondutiableChargesExcludingFreightAndInsuranceInLocalCurrency()
		{
			SetTotalCIF(1000m, 100m, 10m, 15m, 0m, 50m, 20m, GlbCompany.CurrentCompany.LocalCurrency.PK);
			AssertEquals(InvoiceHeaderWrapperInternal.TotalCIFLessAllNondutiableChargesExcludingFreightAndInsuranceInLocalCurrency, InvoiceHeaderWrapperInternal.TotalCIFLessFreightLessInsuranceInLocalCurrency - InvoiceHeaderWrapperInternal.TotalNonDutiableChargesNotIncludedInLineExcludingFreightAndInsuranceInLocalCurrency);
		}

		public void TestTotalOverseasFreightInLocalCurrency()
		{
			InvoiceHeaderInternal.Charges.RemoveAll();
			InvoiceHeaderInternal.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 100m, GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency);

			AssertEquals(InvoiceHeaderWrapperInternal.TotalOverseasFreightInLocalCurrency, InvoiceHeaderInternal.OverseasFreightInLocalCurrency.Amount);
		}

		#endregion

		#region ZGuid Fields

		public void TestInvoiceLineTotalCurrency()
		{
			AssertEquals("InvoiceLineTotalCurrency", InvoiceHeaderInternal.InvoiceLineTotalCurrency, InvoiceHeaderWrapperInternal.InvoiceLineTotalCurrency);
		}

		//see AssertCharges()
		//		public void TestCalcOverseasFreightCurrency()
		//		{
		//			AssertEquals("CalcOverseasFreightCurrency", InvoiceHeader.CalcOverseasFreightCurrency, InvoiceHeaderWrapper.CalcOverseasFreightCurrency);
		//		}
		//
		//		public void TestCalcOverseasInsuranceCurrency()
		//		{
		//			AssertEquals("CalcOverseasInsuranceCurrency", InvoiceHeader.CalcOverseasInsuranceCurrency, InvoiceHeaderWrapper.CalcOverseasInsuranceCurrency);
		//		}
		//
		//		public void TestCalcLandingChargesCurrency()
		//		{
		//			AssertEquals("CalcLandingChargesCurrency", InvoiceHeader.CalcLandingChargesCurrency, InvoiceHeaderWrapper.CalcLandingChargesCurrency);
		//		}
		//
		//		public void TestCalcExWorksCurrency()
		//		{
		//			AssertEquals("CalcExWorksCurrency", InvoiceHeader.CalcExWorksCurrency, InvoiceHeaderWrapper.CalcExWorksCurrency);
		//		}
		//
		//		public void TestCalcForeignInlandFreightCurrency()
		//		{
		//			AssertEquals("CalcForeignInlandFreightCurrency", InvoiceHeader.CalcForeignInlandFreightCurrency, InvoiceHeaderWrapper.CalcForeignInlandFreightCurrency);
		//		}
		//
		//		public void TestCalcPackingCostsCurrency()
		//		{
		//			AssertEquals("CalcPackingCostsCurrency", InvoiceHeader.CalcPackingCostsCurrency, InvoiceHeaderWrapper.CalcPackingCostsCurrency);
		//		}
		//
		//		public void TestCalcOtherCharges1Currency()
		//		{
		//			AssertEquals("CalcOtherCharges1Currency", InvoiceHeader.CalcOtherCharges1Currency, InvoiceHeaderWrapper.CalcOtherCharges1Currency);
		//		}
		//
		//		public void TestCalcNonDutiablePreFOBChargeCurrency()
		//		{
		//			AssertEquals("CalcNonDutiablePreFOBChargeCurrency", InvoiceHeader.CalcNonDutiablePreFOBChargeCurrency, InvoiceHeaderWrapper.CalcNonDutiablePreFOBChargeCurrency);
		//		}
		//
		//		public void TestCalcOtherCharges2Currency()
		//		{
		//			AssertEquals("CalcOtherCharges2Currency", InvoiceHeader.CalcOtherCharges2Currency, InvoiceHeaderWrapper.CalcOtherCharges2Currency);
		//		}
		//
		//		public void TestCalcDiscountCurrency()
		//		{
		//			AssertEquals("CalcDiscountCurrency", InvoiceHeader.CalcDiscountCurrency, InvoiceHeaderWrapper.CalcDiscountCurrency);
		//		}
		//
		//		public void TestCalcCommissionCurrency()
		//		{
		//			AssertEquals("CalcCommissionCurrency", InvoiceHeader.CalcCommissionCurrency, InvoiceHeaderWrapper.CalcCommissionCurrency);
		//		}

		#endregion

		#region Wrapper Fields

		public void TestOSParty()
		{
			OrgHeader supplier = OrgHeader.New(Factory);
			OrgHeader importer = OrgHeader.New(Factory);
			BaseJobDeclaration declaration = BaseJobDeclaration.New(Factory);
			BaseJobComInvoiceGroupHeader groupHeader = declaration.JobComInvoiceGroupHeaders[0];
			BaseJobComInvoiceHeader invoiceHeader = groupHeader.JobComInvoiceHeaders.AddNew();
			invoiceHeader.JZ_OH_Supplier = supplier.PK;
			invoiceHeader.JZ_OH_Buyer = importer.PK;

			declaration.JE_MessageType = JobMessageTypeCodeForExport;
			TWrapper docHeaderExport = CreateInvoiceHeaderWrapper((T)invoiceHeader);
			DocBaseWrapper intermediateWrapper = (DocBaseWrapper)docHeaderExport.OSParty.WrappedObject;
			AssertEquals("DocHeader.OSParty = Importer", importer.PK, ((BusinessObject)intermediateWrapper.WrappedObject).PK);

			declaration.JE_MessageType = JobMessageTypeCodeForImport;
			TWrapper docHeaderImport = CreateInvoiceHeaderWrapper((T)invoiceHeader);
			intermediateWrapper = (DocBaseWrapper)docHeaderImport.OSParty.WrappedObject;
			AssertEquals("DocHeader.OSParty = Supplier", supplier.PK, ((BusinessObject)intermediateWrapper.WrappedObject).PK);
		}

		public void TestPhysicalAddressForSupplier()
		{
			AssertNull("Supplier address is null", InvoiceHeaderWrapperInternal.PhysicalAddressForSupplier);

			var header = Factory.New<OrgHeader>();
			header.OH_Code = "EXPORT";
			header.OH_FullName = "Exporter";
			header.MainAddress.OA_Address1 = "Main address 1";
			header.MainAddress.OA_Address2 = "Main address 2";
			header.MainAddress.OA_City = "Main City";

			OrgAddress postalAddress = header.Addresses.AddNew();
			postalAddress.AddressCapability.SetCapabilityEnabled(OrgAddressType.Postal.Code);
			postalAddress.OA_Address1 = "Postal address 1";
			postalAddress.OA_Address2 = "Postal address 2";
			postalAddress.OA_City = "Post City";
			postalAddress.AddressCapability.SetIsMainAddress(OrgAddressType.Postal.Code);
			OrgAddress salesAddress = header.Addresses.AddNew();
			salesAddress.AddressCapability.SetCapabilityEnabled(OrgAddressType.Sales.Code);
			salesAddress.OA_Address1 = "Sales address 1";
			salesAddress.OA_Address2 = "Sales address 2";
			salesAddress.OA_City = "Sales City";
			salesAddress.AddressCapability.SetIsMainAddress(OrgAddressType.Sales.Code);
			InvoiceHeaderInternal.JZ_OH_Supplier = header.PK;
			DocAddress result = InvoiceHeaderWrapperInternal.PhysicalAddressForSupplier;
			AssertEquals("Will be OFC address", typeof(DocAddress), result.GetType());

			OrgAddress pickupAddress = header.Addresses.AddNew();
			pickupAddress.AddressCapability.SetCapabilityEnabled(OrgAddressType.Pickup.Code);
			pickupAddress.OA_Address1 = "Pickup address 1";
			pickupAddress.OA_Address2 = "Pickup address 2";
			pickupAddress.OA_City = "Pickup City";

			result = InvoiceHeaderWrapperInternal.PhysicalAddressForSupplier;
			AssertEquals("Should be OFC address as first one entered", typeof(DocAddress), result.GetType());
			AssertEquals("Address 1", "Main address 1", result.Address1);
			AssertEquals("Address 2", "Main address 2", result.Address2);
			AssertEquals("City", "Main City", result.City);
		}

		public void TestBranch()
		{
			BaseJobComInvoiceHeader invoiceHeader = (BaseJobComInvoiceHeader)InvoiceHeaderWrapperInternal.WrappedObject;
			invoiceHeader.JobDeclaration.JE_GB = ZGuid.Empty;
			AssertNull("Branch", InvoiceHeaderWrapperInternal.Branch);

			invoiceHeader.JobDeclaration.JE_GB = GlbBranch.CurrentBranch.PK;
			AssertNotNull("Branch", InvoiceHeaderWrapperInternal.Branch);
			AssertEquals("Branch is of type DocBranch", typeof(DocBranch), InvoiceHeaderWrapperInternal.Branch.GetType());
		}

		public void TestBuyer()
		{
			AssertNull("Buyer", InvoiceHeaderWrapperInternal.Buyer);

			InvoiceHeaderInternal.JZ_OH_Buyer = Factory.LoadTop1(typeof(OrgHeader), new ZQuery()).PK;
			AssertNotNull("Buyer", InvoiceHeaderWrapperInternal.Buyer);
			AssertEquals("Buyer is of type DocOrganisation", typeof(DocOrganisation), InvoiceHeaderWrapperInternal.Buyer.GetType());
		}

		public void TestSupplier()
		{
			AssertNull("Supplier", InvoiceHeaderWrapperInternal.Supplier);
		}

		public void TestNKDefaultOrigin()
		{
			AssertNull("NKDefaultOrigin", InvoiceHeaderWrapperInternal.NKDefaultOrigin);

			var country = Factory.LoadTop1<RefCountry>(new ZQuery());
			InvoiceHeaderInternal.JZ_RN_NKDefaultOrigin = country.RN_Code;
			AssertNotNull("NKDefaultOrigin", InvoiceHeaderWrapperInternal.NKDefaultOrigin);
			AssertEquals("NKDefaultOrigin is of type DocCountry", typeof(DocCountry), InvoiceHeaderWrapperInternal.NKDefaultOrigin.GetType());
		}

		public virtual void TestCIFCurrency()
		{
			AssertNull("CIFCurrency", InvoiceHeaderWrapperInternal.CIFCurrency);
		}

		public virtual void TestFOBCurrency()
		{
			AssertNull("FOBCurrency", InvoiceHeaderWrapperInternal.FOBCurrency);
		}

		public virtual void TestInvoiceCurr()
		{
			AssertNull("Invoice_Currency", InvoiceHeaderWrapperInternal.InvoiceCurr);

			InvoiceHeaderInternal.JZ_RX_NKInvoice_Currency = (Factory.LoadTop1<RefCurrency>(new ZQuery())).RX_Code;
			AssertNotNull("Invoice_Currency", InvoiceHeaderWrapperInternal.InvoiceCurr);
			AssertEquals("Invoice_Currency is of type DocCurrency", typeof(DocCurrency), InvoiceHeaderWrapperInternal.InvoiceCurr.GetType());
		}

		#endregion

		#region ZString Fields

		[ExpectNoExceptions]
		public void TestInvoiceAmountAndCurrencyCode()
		{
			InvoiceHeaderInternal.JZ_InvoiceAmount = 100.23m;
			InvoiceHeaderInternal.JZ_RX_NKInvoice_Currency = "NZD";
			AssertEquals("InvoiceAmountAndCurrencyCode", "100.23 NZD", InvoiceHeaderWrapperInternal.InvoiceAmountAndCurrencyCode);
			InvoiceHeaderInternal.JZ_RX_NKInvoice_Currency = "XXX";
			AssertEquals("InvoiceAmountAndCurrencyCode", "", InvoiceHeaderWrapperInternal.InvoiceAmountAndCurrencyCode);
		}

		public void TestCountryOfOrigin()
		{
			BaseJobComInvoiceLine line1 = InvoiceHeaderInternal.JobComInvoiceLines.AddNew();
			AssertEquals("Country Of Origin", ZString.Empty, InvoiceHeaderWrapperInternal.CountryOfOrigin);

			var aU = Factory.LoadTop1<RefCountry>(new ZQuery(RefCountrySchema.RN_Code, "AU"));
			var nZ = Factory.LoadTop1<RefCountry>(new ZQuery(RefCountrySchema.RN_Code, "NZ"));

			BaseJobComInvoiceLine line2 = InvoiceHeaderInternal.JobComInvoiceLines.AddNew();
			BaseJobComInvoiceLine line3 = InvoiceHeaderInternal.JobComInvoiceLines.AddNew();

			line1.JI_CountryOfOrigin = aU.RN_Code;
			line2.JI_CountryOfOrigin = aU.RN_Code;
			line3.JI_CountryOfOrigin = aU.RN_Code;

			AssertEquals(aU.RN_Desc, InvoiceHeaderWrapperInternal.CountryOfOrigin);

			line2.JI_CountryOfOrigin = nZ.RN_Code;
			//NB: If you need to change the word "VARIOUS" then you need to change the COMMERCIAL INVOICE.
			AssertEquals("VARIOUS", InvoiceHeaderWrapperInternal.CountryOfOrigin);
		}

		public void TestCalcGroupInvoice()
		{
			AssertEquals("CalcGroupInvoice", InvoiceHeaderInternal.JZ_Calc_GroupInvoice, InvoiceHeaderWrapperInternal.CalcGroupInvoice);
		}

		public void TestAddInfo()
		{
			InvoiceHeaderInternal.JZ_AddInfo = "AddInfo";
			AssertEquals("AddInfo", InvoiceHeaderInternal.JZ_AddInfo, InvoiceHeaderWrapperInternal.AddInfo);
		}

		public void TestInvoiceNumber()
		{
			InvoiceHeaderInternal.JZ_InvoiceNumber = "InvoiceNumber";
			AssertEquals("InvoiceNumber", InvoiceHeaderInternal.JZ_InvoiceNumber, InvoiceHeaderWrapperInternal.InvoiceNumber);
		}

		public void TestIncoTerm()
		{
			CustomsIncoTermOverrideCollection collection = DocumentsDataRegistry.Instance.CustomsIncoTermsOverride.Value;
			collection.AddNew("FDD", Core.Constants.IncoTerms.FreeAlongsideShip);
			DocumentsDataRegistry.Instance.CustomsIncoTermsOverride.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, collection);
			InvoiceHeaderInternal.JZ_IncoTerm = "III";
			AssertEquals("IncoTerm", InvoiceHeaderInternal.JZ_IncoTerm, InvoiceHeaderWrapperInternal.IncoTerm);
			InvoiceHeaderInternal.JZ_IncoTerm = "FDD";
			AssertEquals("IncoTerm Override", Core.Constants.IncoTerms.FreeAlongsideShip, InvoiceHeaderWrapperInternal.IncoTerm);
		}

		public void TestPaymentNo()
		{
			InvoiceHeaderInternal.JZ_PaymentNo = "PaymentNo";
			AssertEquals("PaymentNo", InvoiceHeaderInternal.JZ_PaymentNo, InvoiceHeaderWrapperInternal.PaymentNo);
		}

		public void TestVolumeUQ()
		{
			InvoiceHeaderInternal.JZ_VolumeUQ = "UQ";
			AssertEquals("VolumeUQ", InvoiceHeaderInternal.JZ_VolumeUQ, InvoiceHeaderWrapperInternal.VolumeUQ);
		}

		public void TestWeightUQ()
		{
			InvoiceHeaderInternal.JZ_WeightUQ = "UQ";
			AssertEquals("WeightUQ", InvoiceHeaderInternal.JZ_WeightUQ, InvoiceHeaderWrapperInternal.WeightUQ);
		}

		public void TestMasterBill()
		{
			DocBaseJobComInvoiceHeaderTestClass invoiceHeader = DocBaseJobComInvoiceHeaderTestClass.New(InvoiceHeaderInternal, Factory);
			AssertEquals("MasterBill of Declaration", "", invoiceHeader.MasterBill);
			InvoiceHeaderInternal.JobDeclaration.JE_MasterBill = "TEST";
			AssertEquals("MasterBill of Declaration", "TEST", invoiceHeader.MasterBill);
		}

		public void TestJobNumber()
		{
			DocBaseJobComInvoiceHeaderTestClass invoiceHeader = DocBaseJobComInvoiceHeaderTestClass.New(InvoiceHeaderInternal, Factory);
			AssertEquals("Declaration Ref#", InvoiceHeaderInternal.JobDeclaration.JE_DeclarationReference, invoiceHeader.JobNumber);
		}
		#endregion

		#region ZDateTime Fields

		public void TestInvoiceDate()
		{
			ZDateTime invoiceDate = new ZDateTime(2004, 04, 04);
			InvoiceHeaderInternal.JZ_InvoiceDate = invoiceDate;
			AssertEquals("InvoiceDate", invoiceDate, InvoiceHeaderWrapperInternal.InvoiceDate);
		}

		public void TestPaymentDate()
		{
			ZDateTime paymentDate = new ZDateTime(2004, 04, 04);
			InvoiceHeaderInternal.JZ_PaymentDate = paymentDate;
			AssertEquals("PaymentDate", paymentDate, InvoiceHeaderWrapperInternal.PaymentDate);
		}

		#endregion

		#region Implementation Fields Test

		public void TestDeclarationInternal()
		{
			DocBaseJobComInvoiceHeaderTestClass invoiceHeader = DocBaseJobComInvoiceHeaderTestClass.New(InvoiceHeaderInternal, Factory);
			AssertNotNull("DeclarationInternal", invoiceHeader.DeclarationInternalTestMethod);
		}

		public void TestInvoiceLinesInternal()
		{
			DocBaseJobComInvoiceHeaderTestClass invoiceHeader = DocBaseJobComInvoiceHeaderTestClass.New(InvoiceHeaderInternal, Factory);
			InvoiceHeaderInternal.JobComInvoiceLines.AddNew();
			InvoiceHeaderInternal.JobComInvoiceLines.AddNew();
			AssertEquals("InvoiceLinesInternal has 2 elements", 2, invoiceHeader.InvoiceLinesInternalTestMethod.Count);
		}

		#endregion

		#region Sub Class Tests

		public void TestDeclaration()
		{
			PropertyInfo property = InvoiceHeaderWrapperInternal.GetType().GetProperty("Declaration");
			AssertNotNull("You must implement a property call Declaration", property);
			MethodInfo method = property.GetGetMethod();
			DocBaseJobDeclaration declaration = (DocBaseJobDeclaration)method.Invoke(InvoiceHeaderWrapperInternal, Array.Empty<object>());
			AssertNotNull("Declaration is not null", declaration);
			Assert("Declaration is of type DocDeclaration", declaration.GetType().ToString().EndsWith("DocDeclaration"));
		}

		public void TestInvoiceLines()
		{
			InvoiceHeaderInternal.JobComInvoiceLines.AddNew();
			InvoiceHeaderInternal.JobComInvoiceLines.AddNew();

			PropertyInfo property = InvoiceHeaderWrapperInternal.GetType().GetProperty("InvoiceLines");
			AssertNotNull("You must implement a property call InvoiceLines", property);
			MethodInfo method = property.GetGetMethod();
			DocBaseJobComInvoiceLineCollection invoiceLines = (DocBaseJobComInvoiceLineCollection)method.Invoke(InvoiceHeaderWrapperInternal, Array.Empty<object>());
			AssertNotNull("InvoiceLines is not null", invoiceLines);
			AssertEquals("2 lines in collection", 2, invoiceLines.Count);
			Assert("InvoiceLines is of type DocJobComInvoiceLineCollection", invoiceLines.GetType().ToString().EndsWith("DocJobComInvoiceLineCollection"));
		}

		public void TestUnclassifiedInvoiceLines()
		{
			BaseJobComInvoiceLine line1 = InvoiceHeaderInternal.JobComInvoiceLines.AddNew();
			line1.JI_Tariff = "00000000";
			BaseJobComInvoiceLine line2 = InvoiceHeaderInternal.JobComInvoiceLines.AddNew();
			BaseJobComInvoiceLine line3 = InvoiceHeaderInternal.JobComInvoiceLines.AddNew();
			line3.JI_Tariff = "00000000";
			BaseJobComInvoiceLine line4 = InvoiceHeaderInternal.JobComInvoiceLines.AddNew();
			BaseJobComInvoiceLine line5 = InvoiceHeaderInternal.JobComInvoiceLines.AddNew();

			PropertyInfo property = InvoiceHeaderWrapperInternal.GetType().GetProperty("UnclassifiedInvoiceLines");
			AssertNotNull("You must implement a property call UnclassifiedInvoiceLines", property);
			MethodInfo method = property.GetGetMethod();
			DocBaseJobComInvoiceLineCollection unclassifiedInvoiceLines = (DocBaseJobComInvoiceLineCollection)method.Invoke(InvoiceHeaderWrapperInternal, Array.Empty<object>());
			AssertNotNull("UnclassifiedInvoiceLines is not null", unclassifiedInvoiceLines);
			AssertEquals("3 lines in collection", 3, unclassifiedInvoiceLines.Count);
			Assert("UnclassifiedInvoiceLines is of type DocJobComInvoiceLineCollection", unclassifiedInvoiceLines.GetType().ToString().EndsWith("DocJobComInvoiceLineCollection"));
		}
		#endregion

		public override DocumentWrapper[] GetDocumentWrappers()
		{
			return new DocumentWrapper[]
			{
				InvoiceHeaderWrapperInternal
			};
		}

		#region Implementation

		void SetTotalCIF(
			ZDecimal invoiceAmount,
			ZDecimal overseasFreight,
			ZDecimal overseasInsurance,
			ZDecimal othercharges,
			ZDecimal j7_Amount,
			ZDecimal discount,
			ZDecimal foreignInlandFreight,
			ZGuid currencyPK)
		{
			InvoiceHeaderInternal.JZ_InvoiceAmount = invoiceAmount;
			RefCurrency currency = Factory.Load<RefCurrency>(currencyPK);
			if (currency != null)
			{
				InvoiceHeaderInternal.JZ_RX_NKInvoice_Currency = currency.RX_Code;
			}
			InvoiceHeaderInternal.JZ_IncoTerm = "CIF";

			InvoiceHeaderInternal.Charges.RemoveAll();
			//Non-Dutiable
			BaseJobComInvHeaderCharge oFT = InvoiceHeaderInternal.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, overseasFreight);
			oFT.J7_IsIncludedInITOT = false;
			//Non-dutiable
			BaseJobComInvHeaderCharge oNS = InvoiceHeaderInternal.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasInsurance, overseasInsurance);
			oNS.J7_IsIncludedInITOT = false;

			//Non-dutiable
			BaseJobComInvHeaderCharge oTH = InvoiceHeaderInternal.Charges.AddNew(CustomsChargeTypeList.Codes.OtherCharges, othercharges);
			oTH.J7_IsDutiable = false;
			oTH.J7_IsIncludedInITOT = false;

			//Dutiable, but this is not included in lines
			BaseJobComInvHeaderCharge fIF = InvoiceHeaderInternal.Charges.AddNew(CustomsChargeTypeList.Codes.ForeignInlandFreight, foreignInlandFreight);
			fIF.J7_IsIncludedInITOT = false;

			InvoiceHeaderInternal.Charges.AddNew(CustomsChargeTypeList.Codes.Discount, discount);
		}

		protected T InvoiceHeaderInternal;
		protected TWrapper InvoiceHeaderWrapperInternal
		{
			get { return CreateInvoiceHeaderWrapper(InvoiceHeaderInternal); }
		}

		protected override DocumentWrapper CreateDocumentWrapperFromStaticNewMethod()
		{
			return CreateInvoiceHeaderWrapper(InvoiceHeaderInternal);
		}

		protected override void SetUp()
		{
			CustomsDataRegistry.Instance.DefaultCurrencyToLocalCurrency.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			GlbCompany.CurrentCompany.SetCountry(TestingCountry);
			InvoiceHeaderInternal = GetNewInvoice();
			base.SetUp();
		}

		protected virtual T GetNewInvoice()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			return (T)declaration.Invoices.AddNew();
		}

		protected void AssertCharges(string chargeName, string docFieldAmountName, string docFieldCurrName)
		{
			if (InvoiceHeaderInternal.IncoTermAndChargeFactory.GetCharge(chargeName) != null)
			{
				BaseJobComInvHeaderCharge charge = InvoiceHeaderInternal.Charges.AddNew(chargeName, 12.32m, GlbCompany.CurrentCompany.Country.RN_RX_NKLocalCurrency);
				if (docFieldAmountName == "OtherCharges2")
				{
					charge.J7_IsDutiable = false;
					charge.J7_IsGSTApplicable = false;
				}

				string includedAmount = "Included" + docFieldAmountName;
				string includedCurrency = "Included" + docFieldCurrName;

				ZDecimal actualAmount = (ZDecimal)InvoiceHeaderWrapperInternal[includedAmount];
				DocCurrency actualDocCurrency = InvoiceHeaderWrapperInternal[includedCurrency] as DocCurrency;

				AssertEquals(docFieldAmountName, 12.32m, actualAmount);
				AssertNotNull(docFieldCurrName, actualDocCurrency);
				AssertEquals(docFieldCurrName, GlbCompany.CurrentCompany.Country.RN_RX_NKLocalCurrency, actualDocCurrency.Code);
			}
			else
			{
				Assert("Not Applicable", true);
			}
		}

		#endregion

		protected virtual ZString JobMessageTypeCodeForImport
		{
			get { return Enterprise.Customs.AU.Declaration.Business.JobMessageTypeList.Codes.Import; }
		}

		protected virtual ZString JobMessageTypeCodeForExport
		{
			get { return Enterprise.Customs.AU.Declaration.Business.JobMessageTypeList.Codes.Export; }
		}
	}
}
