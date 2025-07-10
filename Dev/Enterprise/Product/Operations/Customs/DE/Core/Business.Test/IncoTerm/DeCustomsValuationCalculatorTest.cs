using System;
using System.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.DE.Business.Testing
{
	sealed class DeCustomsValuationCalculatorTest : TestCaseWithFactory
	{
		public void TestGetValueForVat()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			var inv = declaration.Invoices.AddNew();
			inv.JZ_InvoiceAmount = 1000m;
			inv.JZ_RX_NKInvoice_Currency = declaration.LocalCurrencyCode;

			var invoiceLine = inv.InvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = inv.JZ_InvoiceAmount;

			var charge1 = invoiceLine.Charges.AddNew();
			charge1.J7_Amount = 100m;
			charge1.J7_ChargeType = ImportChargeCodeList.Codes.OPF;
			charge1.J7_IsDutiable = false;
			charge1.J7_IsGSTApplicable = true;
			charge1.J7_IsIncludedInITOT = true;
			var charge2 = invoiceLine.Charges.AddNew();
			charge2.J7_Amount = 50m;
			charge2.J7_IsDutiable = false;
			charge2.J7_IsGSTApplicable = true;
			charge1.J7_IsIncludedInITOT = false;
			var charge3 = invoiceLine.Charges.AddNew();
			charge3.J7_Amount = 300m;
			charge3.J7_IsDutiable = true;
			charge3.J7_IsGSTApplicable = true;
			var charge4 = invoiceLine.Charges.AddNew();
			charge4.J7_Amount = 300m;
			charge4.J7_IsDutiable = false;
			charge4.J7_IsGSTApplicable = false;
			var charge5 = invoiceLine.ApportionedCharges.AddNew();
			charge5.J7_Amount = 20m;
			charge5.J7_IsDutiable = false;
			charge5.J7_IsGSTApplicable = true;
			charge5.J7_IsIncludedInITOT = true;
			charge5.J7_RX_NKCurrency = invoiceLine.LocalCurrency.Code;
			var charge6 = invoiceLine.ApportionedCharges.AddNew();
			charge6.J7_Amount = 10m;
			charge6.J7_IsDutiable = false;
			charge6.J7_IsGSTApplicable = true;
			charge6.J7_IsIncludedInITOT = false;
			charge6.J7_RX_NKCurrency = invoiceLine.LocalCurrency.Code;

			var calculator = new DeCustomsValuationCalculator(invoiceLine);
			AssertEquals(180m, calculator.GetValueForVat(invoiceLine));
		}

		public void TestGetValuesNoIATA() => CombineAssertions(() =>
		{
			var (invoiceLine, charge1, charge2, invoice) = SetupInvoiceAndInvoiceLine();

			charge1.IsJ7_ExchangeRateIATA = false;
			charge2.IsJ7_ExchangeRateIATA = false;

			AssertEquals("Customs Value", 5283.67m, invoiceLine.JI_CustomsValue);
			AssertEquals("Statistical Value", 5283.67m, invoiceLine.JI_Calc_StatisticalValue);
			AssertEquals("VAT Value", 5348.28m, invoiceLine.JI_Calc_ValueForVat);
		});

		public void TestGetValuesIATA() => CombineAssertions(() =>
		{
			var (invoiceLine, charge1, charge2, invoice) = SetupInvoiceAndInvoiceLine();

			charge1.IsJ7_ExchangeRateIATA = true;
			charge2.IsJ7_ExchangeRateIATA = true;

			AssertEquals("Customs Value", 5290.45m, invoiceLine.JI_CustomsValue);
			AssertEquals("Statistical Value", 5290.45m, invoiceLine.JI_Calc_StatisticalValue);
			AssertEquals("VAT Value", 5356.91m, invoiceLine.JI_Calc_ValueForVat);
		});

		public void TestGetValuesIATAIncluded() => CombineAssertions(() =>
		{
			var (invoiceLine, charge1, charge2, invoice) = SetupInvoiceAndInvoiceLine();

			charge1.IsJ7_ExchangeRateIATA = true;
			charge2.IsJ7_ExchangeRateIATA = true;
			charge2.J7_IsIncludedInITOT = true;

			AssertEquals("Customs Value", 5223.99m, invoiceLine.JI_CustomsValue);
			AssertEquals("Statistical Value", 5223.99m, invoiceLine.JI_Calc_StatisticalValue);
			AssertEquals("VAT Value", 5290.45m, invoiceLine.JI_Calc_ValueForVat);
		});

		public void TestIATAConverterDateRefreshed() => CombineAssertions(() =>
		{
			this.SetExchangeRate(ExchangeRateType.Customs, new DateTime(2000, 1, 1), new DateTime(2022, 1, 1), 1.1m);
			this.SetExchangeRate(ExchangeRateType.IATA, new DateTime(2000, 1, 1), new DateTime(2022, 1, 1), 1.2m);
			var (invoiceLine, charge1, charge2, invoice) = SetupInvoiceAndInvoiceLine();

			charge1.IsJ7_ExchangeRateIATA = true;
			charge2.IsJ7_ExchangeRateIATA = true;

			AssertEquals("Customs Value old date", 5290.45m, invoiceLine.JI_CustomsValue);
			invoice.JZ_ValuationDateOverride = new DateTime(2022, 1, 1);
			AssertEquals("Customs Value new date", 4740.45m, invoiceLine.JI_CustomsValue);
			AssertEquals("date", new DateTime(2022, 1, 1), charge1.CurrencyConverter.DateForRate);
		});

		(JobComInvoiceLine invoiceLine, InvoiceLineCharge charge1, InvoiceLineCharge charge2, JobComInvoiceHeader invoice) SetupInvoiceAndInvoiceLine()
		{
			var now = DateTime.Now;
			SetExchangeRate(ExchangeRateType.Customs, now.AddDays(-10), now.AddDays(10), 0.9906m);
			SetExchangeRate(ExchangeRateType.IATA, now.AddDays(-10), now.AddDays(10), 0.96295m);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			var inv = declaration.Invoices.AddNew();
			inv.JZ_InvoiceDate = now;
			inv.JZ_InvoiceAmount = 5000m;
			inv.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
			inv.JZ_IncoTerm = "EXW";
			inv.ZG_AgreedPlaceCode = "1";
			inv.JZ_ValuationCode = "11";

			var invoiceLine = inv.InvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = inv.JZ_InvoiceAmount;

			var charge1 = invoiceLine.Charges.AddNew();
			charge1.J7_ChargeType = ImportChargeCodeList.Codes._010;
			charge1.J7_Amount = 234m;
			charge1.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			charge1.J7_IsDutiable = true;
			charge1.J7_IsStatisticalValueApplicable = true;
			charge1.J7_IsGSTApplicable = true;
			charge1.J7_IsIncludedInITOT = false;

			var charge2 = invoiceLine.Charges.AddNew();
			charge2.J7_ChargeType = ImportChargeCodeList.Codes._014;
			charge2.J7_Amount = 64m;
			charge2.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			charge2.J7_IsDutiable = false;
			charge2.J7_IsStatisticalValueApplicable = false;
			charge2.J7_IsGSTApplicable = true;
			charge2.J7_IsIncludedInITOT = false;

			return (invoiceLine, charge1, charge2, inv);
		}

		void SetExchangeRate(ExchangeRateType type, DateTime startDate, DateTime endDate, decimal exchangeRate)
		{
			var fi = type.GetType().GetField(type.ToString());
			var attributes = (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);
			var typeDescription = (attributes?.Length > 0) ? attributes[0].Description : type.ToString();

			ZQuery sQLFilter = new ZQuery();
			sQLFilter.AddToFilter(RefExchangeRateSchema.RE_RX_NKExCurrency, "USD");
			sQLFilter.AddToFilter(RefExchangeRateSchema.RE_GC, GlbCompany.CurrentCompany.PK);
			sQLFilter.AddToFilter(RefExchangeRateSchema.RE_ExRateType, typeDescription);
			sQLFilter.AddToFilter(RefExchangeRateSchema.RE_StartDate, SQLComparisonOperator.LessThan, startDate.AddDays(1));
			sQLFilter.AddToFilter(RefExchangeRateSchema.RE_ExpiryDate, SQLComparisonOperator.GreaterThanOrEqualTo, endDate);

			var exchangeRateDuty = Factory.LoadTop1<RefExchangeRate>(sQLFilter);
			if (exchangeRateDuty != null)
			{
				exchangeRateDuty.Delete();
			}

			RefExchangeRate newOne = Factory.New<RefExchangeRate>();
			newOne.RE_ExpiryDate = endDate;
			newOne.RE_ExRateType = typeDescription;
			newOne.RE_GC = GlbCompany.CurrentCompany.PK;
			newOne.RE_RX_NKExCurrency = "USD";
			newOne.RE_StartDate = startDate;
			newOne.RE_SellRate = exchangeRate;
		}
	}
}
