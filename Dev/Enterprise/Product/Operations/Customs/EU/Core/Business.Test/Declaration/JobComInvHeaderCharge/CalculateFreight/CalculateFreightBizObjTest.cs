using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.Declaration.Testing
{
	[TestedType(typeof(CalculateFreightBizObj))]
	public class CalculateFreightBizObjTest : NonPersistentBusinessObjectTestCase
	{
		public void TestAmountAndCurrency()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var bizObj = CalculateFreightBizObj.New(invoice.Charges, declaration);
			AssertEquals("No freight charge is available, Currency should be defaulted to EUR.", "EUR", bizObj.Currency);
			AssertEquals("No freight charge is available, Amount should be 0.", 0m, bizObj.Amount);

			var freightCharge = invoice.Charges.AddNew();
			freightCharge.J7_ChargeType = ChargeTypeList.Codes.InternationalFreight;
			freightCharge.J7_Amount = 100m;
			freightCharge.J7_RX_NKCurrency = "USD";

			var freightCharge2 = invoice.Charges.AddNew();
			freightCharge2.J7_ChargeType = ChargeTypeList.Codes.InternationalFreight;
			freightCharge2.J7_Amount = 200m;
			freightCharge2.J7_RX_NKCurrency = "USD";

			bizObj = CalculateFreightBizObj.New(invoice.Charges, declaration);
			AssertEquals("Currency should be defaulted with common currency code of charges of type OFT found.", "USD", bizObj.Currency);
			AssertEquals("Freight charges share same currency so Amount should be the sum of OFT charges.", 300m, bizObj.Amount);

			var freightCharge3 = invoice.Charges.AddNew();
			freightCharge3.J7_ChargeType = ChargeTypeList.Codes.InternationalFreight;
			freightCharge3.J7_Amount = 400m;
			freightCharge3.J7_RX_NKCurrency = "EUR";

			bizObj = CalculateFreightBizObj.New(invoice.Charges, declaration);
			AssertEquals("Currency should not be defaulted because all charges of type OFT found don't share same currency.", ZString.Empty, bizObj.Currency);
			AssertEquals("Currency could not be determined, Amount should be 0.", 0m, bizObj.Amount);

			freightCharge.J7_RX_NKCurrency = ZString.Empty;
			freightCharge2.J7_RX_NKCurrency = ZString.Empty;
			freightCharge3.J7_RX_NKCurrency = ZString.Empty;

			bizObj = CalculateFreightBizObj.New(invoice.Charges, declaration);
			AssertEquals("Currency should be defaulted with common currency code of charges of type OFT found, even if empty.", ZString.Empty, bizObj.Currency);
			AssertEquals("Currency could be determined as empty, Amount should be the sum of OFT charges.", 700m, bizObj.Amount);
		}

		public void TestCalculate()
		{
			var declaration = Factory.New<JobDeclaration>();
			var euIncoTermAndChargeFactory = (EUIncoTermAndCustomsChargeFactory)declaration.IncoTermAndChargeFactory;
			var invoice = declaration.Invoices.AddNew();
			var charge1 = invoice.Charges.AddNew();
			SetAsFreightBeforeEUBorder(charge1, euIncoTermAndChargeFactory);
			var charge2 = invoice.Charges.AddNew();
			SetAsFreightBeforeEUBorder(charge2, euIncoTermAndChargeFactory);
			var charge3 = invoice.Charges.AddNew();
			SetAsFreightAfterEUBorder(charge3, euIncoTermAndChargeFactory);
			var charge4 = invoice.Charges.AddNew();
			SetAsFreightAfterEUBorder(charge4, euIncoTermAndChargeFactory);
			var charge5 = invoice.Charges.AddNew();
			charge5.J7_ChargeType = "ONS";

			var bizObj = CalculateFreightBizObj.New(invoice.Charges, declaration);
			AssertEquals(false, bizObj.Calculate());
			AssertEquals(5, invoice.Charges.Count);
			AssertEquals(false, charge1.IsDeleted);
			AssertEquals(false, charge2.IsDeleted);
			AssertEquals(false, charge3.IsDeleted);
			AssertEquals(false, charge4.IsDeleted);
			AssertEquals(false, charge5.IsDeleted);

			bizObj.ClearAllNotifications();
			AssertEquals(false, bizObj.HasErrors);
			AssertEquals(false, bizObj.Calculate());
			AssertEquals(5, invoice.Charges.Count);
			AssertEquals(false, charge1.IsDeleted);
			AssertEquals(false, charge2.IsDeleted);
			AssertEquals(false, charge3.IsDeleted);
			AssertEquals(false, charge4.IsDeleted);
			AssertEquals(false, charge5.IsDeleted);

			bizObj.Amount = 1000m;
			bizObj.Currency = Core.Constants.CurrencyCodes.EuropeanUnion;
			AssertEquals(false, bizObj.HasErrors);
			bizObj.Percentage = -10;
			AssertEquals(true, bizObj.HasErrors);
			bizObj.ClearAllNotifications();
			AssertEquals(false, bizObj.HasErrors);
			AssertEquals(false, bizObj.Calculate());
			AssertEquals(5, invoice.Charges.Count);
			AssertEquals(false, charge1.IsDeleted);
			AssertEquals(false, charge2.IsDeleted);
			AssertEquals(false, charge3.IsDeleted);
			AssertEquals(false, charge4.IsDeleted);
			AssertEquals(false, charge5.IsDeleted);
			AssertEquals(true, bizObj.HasErrors);

			bizObj.Percentage = 60;
			AssertEquals(false, bizObj.HasErrors);
			bizObj.Currency = ZString.Empty;
			AssertEquals(true, bizObj.HasErrors);
			bizObj.ClearAllNotifications();
			AssertEquals(false, bizObj.HasErrors);
			AssertEquals(false, bizObj.Calculate());
			AssertEquals(5, invoice.Charges.Count);
			AssertEquals(false, charge1.IsDeleted);
			AssertEquals(false, charge2.IsDeleted);
			AssertEquals(false, charge3.IsDeleted);
			AssertEquals(false, charge4.IsDeleted);
			AssertEquals(false, charge5.IsDeleted);
			AssertEquals(true, bizObj.HasErrors);

			bizObj.Currency = Core.Constants.CurrencyCodes.Dominica;
			AssertEquals(false, bizObj.HasErrors);
			AssertEquals(true, bizObj.Calculate());
			AssertEquals(3, invoice.Charges.Count);
			AssertEquals(true, charge1.IsDeleted);
			AssertEquals(true, charge2.IsDeleted);
			AssertEquals(true, charge3.IsDeleted);
			AssertEquals(true, charge4.IsDeleted);
			AssertEquals(false, charge5.IsDeleted);
			AssertEquals(false, bizObj.HasErrors);
			var charge6 = invoice.Charges.OfType<InvoiceCharge>().FirstOrDefault(x => IsFreightBeforeEUBorder(x, euIncoTermAndChargeFactory));
			AssertEquals("charge6.J7_Amount", 600m, charge6.J7_Amount);
			AssertEquals("charge6.J7_RX_NKCurrency", Core.Constants.CurrencyCodes.Dominica, charge6.J7_RX_NKCurrency);
			Assert(charge6.J7_IsDutiable);
			Assert(charge6.J7_IsGSTApplicable);
			Assert(charge6.J7_IsStatisticalValueApplicable);

			var charge7 = invoice.Charges.OfType<InvoiceCharge>().FirstOrDefault(x => IsFreightAfterEUBorder(x, euIncoTermAndChargeFactory));
			Assert(!charge7.J7_IsDutiable);
			Assert(charge7.J7_IsGSTApplicable);
			Assert(charge7.J7_IsStatisticalValueApplicable);
			AssertEquals("charge7.J7_Amount", 400m, charge7.J7_Amount);
			AssertEquals("charge7.J7_RX_NKCurrency", Core.Constants.CurrencyCodes.Dominica, charge7.J7_RX_NKCurrency);

			bizObj.Percentage = 100;
			AssertEquals(false, bizObj.HasErrors);
			AssertEquals(true, bizObj.Calculate());
			AssertEquals(2, invoice.Charges.Count);
			AssertEquals(false, charge5.IsDeleted);
			AssertEquals(true, charge6.IsDeleted);
			AssertEquals(true, charge7.IsDeleted);
			AssertEquals(false, bizObj.HasErrors);
			var charge8 = invoice.Charges.OfType<InvoiceCharge>().FirstOrDefault(x => IsFreightBeforeEUBorder(x, euIncoTermAndChargeFactory));
			AssertEquals("charge8.J7_Amount", 1000m, charge8.J7_Amount);
			AssertEquals("charge8.J7_RX_NKCurrency", Core.Constants.CurrencyCodes.Dominica, charge8.J7_RX_NKCurrency);

			bizObj.Percentage = 0;
			AssertEquals(false, bizObj.HasErrors);
			AssertEquals(true, bizObj.Calculate());
			AssertEquals(2, invoice.Charges.Count);
			AssertEquals(false, charge5.IsDeleted);
			AssertEquals(true, charge8.IsDeleted);
			AssertEquals(false, bizObj.HasErrors);
			var charge9 = invoice.Charges.OfType<InvoiceCharge>().FirstOrDefault(x => IsFreightAfterEUBorder(x, euIncoTermAndChargeFactory));
			AssertEquals("charge9.J7_Amount", 1000m, charge9.J7_Amount);
			AssertEquals("charge9.J7_RX_NKCurrency", Core.Constants.CurrencyCodes.Dominica, charge9.J7_RX_NKCurrency);

			if (ErrorReporter.LastKeyReported == "CheckValidationActionClear:Percentage")
			{
				ErrorReporter.Clear();
			}
		}

		public void TestProperties()
		{
			var bizObj = (CalculateFreightBizObj)GetNewBusinessObject();
			bizObj.Amount = 1000m;
			AssertEquals("AmountToEUBorder", 1000m, bizObj.AmountToEUBorder);
			AssertEquals("AmountAfterEUBorder", 0m, bizObj.AmountAfterEUBorder);
			bizObj.Percentage = 60m;
			AssertEquals("AmountToEUBorder", 600m, bizObj.AmountToEUBorder);
			AssertEquals("AmountAfterEUBorder", 400m, bizObj.AmountAfterEUBorder);
			bizObj.Percentage = 0m;
			AssertEquals("AmountToEUBorder", 0m, bizObj.AmountToEUBorder);
			AssertEquals("AmountAfterEUBorder", 1000m, bizObj.AmountAfterEUBorder);
			bizObj.Percentage = 30m;
			AssertEquals("AmountToEUBorder", 300m, bizObj.AmountToEUBorder);
			AssertEquals("AmountAfterEUBorder", 700m, bizObj.AmountAfterEUBorder);
			bizObj.Amount = 2000m;
			AssertEquals("AmountToEUBorder", 600m, bizObj.AmountToEUBorder);
			AssertEquals("AmountAfterEUBorder", 1400m, bizObj.AmountAfterEUBorder);
		}

		public void TestCreatedChargesIsIncludedInITOT()
		{
			var declaration = Factory.New<JobDeclaration>();
			var euIncoTermAndChargeFactory = (EUIncoTermAndCustomsChargeFactory)declaration.IncoTermAndChargeFactory;
			var invoice = declaration.Invoices.AddNew();
			var bizObj = CalculateFreightBizObj.New(invoice.Charges, declaration);
			bizObj.Amount = 1000m;
			bizObj.Percentage = 75m;

			bizObj.IsFreightIncludedInLines = false;
			bizObj.Calculate();
			AssertEquals(2, invoice.Charges.Count);
			AssertEquals(0, invoice.Charges.Cast<InvoiceCharge>().Count(c => c.J7_IsIncludedInITOT));

			bizObj.IsFreightIncludedInLines = true;
			bizObj.Calculate();
			AssertEquals(2, invoice.Charges.Cast<InvoiceCharge>().Count(c => c.J7_IsIncludedInITOT && c.J7_ChargeType == euIncoTermAndChargeFactory.FreightToEUBorderCode));

			bizObj.IsFreightIncludedInLines = false;
			bizObj.Calculate();
			AssertEquals(0, invoice.Charges.Cast<InvoiceCharge>().Count(c => c.J7_IsIncludedInITOT && c.J7_ChargeType == euIncoTermAndChargeFactory.FreightToEUBorderCode));
		}

		public void TestBizObj_IsFreightIncludedInLines()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var freightCharge1 = invoice.Charges.AddNew();

			freightCharge1.J7_ChargeType = UCCCustomsChargeTypeList.Codes.OtherNotElsewhereDeclaredCharge;
			freightCharge1.J7_IsIncludedInITOT = false;
			var bizObj = CalculateFreightBizObj.New(invoice.Charges, declaration);
			AssertEquals(false, bizObj.IsFreightIncludedInLines);

			freightCharge1.J7_IsIncludedInITOT = true;
			bizObj = CalculateFreightBizObj.New(invoice.Charges, declaration);
			AssertEquals(false, bizObj.IsFreightIncludedInLines);

			freightCharge1.J7_ChargeType = ChargeTypeList.Codes.InternationalFreight;
			freightCharge1.J7_IsIncludedInITOT = false;
			bizObj = CalculateFreightBizObj.New(invoice.Charges, declaration);
			AssertEquals(false, bizObj.IsFreightIncludedInLines);

			freightCharge1.J7_IsIncludedInITOT = true;
			bizObj = CalculateFreightBizObj.New(invoice.Charges, declaration);
			AssertEquals(true, bizObj.IsFreightIncludedInLines);
		}

		void SetAsFreightBeforeEUBorder(InvoiceCharge invoiceCharge, EUIncoTermAndCustomsChargeFactory euIncoTermAndCustomsChargeFactory)
		{
			invoiceCharge.J7_ChargeType = euIncoTermAndCustomsChargeFactory.FreightToEUBorderCode;
			invoiceCharge.J7_IsDutiable = true;
		}

		void SetAsFreightAfterEUBorder(InvoiceCharge invoiceCharge, EUIncoTermAndCustomsChargeFactory euIncoTermAndCustomsChargeFactory)
		{
			invoiceCharge.J7_ChargeType = euIncoTermAndCustomsChargeFactory.FreightAfterEUBorderCode;
			invoiceCharge.J7_IsDutiable = false;
		}

		bool IsFreightBeforeEUBorder(InvoiceCharge invoiceCharge, EUIncoTermAndCustomsChargeFactory euIncoTermAndCustomsChargeFactory)
		{
			return invoiceCharge.J7_ChargeType == euIncoTermAndCustomsChargeFactory.FreightToEUBorderCode && invoiceCharge.J7_IsDutiable;
		}

		bool IsFreightAfterEUBorder(InvoiceCharge invoiceCharge, EUIncoTermAndCustomsChargeFactory euIncoTermAndCustomsChargeFactory)
		{
			return invoiceCharge.J7_ChargeType == euIncoTermAndCustomsChargeFactory.FreightAfterEUBorderCode && !invoiceCharge.J7_IsDutiable;
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			return CalculateFreightBizObj.New(invoice.Charges, declaration);
		}
	}
}
