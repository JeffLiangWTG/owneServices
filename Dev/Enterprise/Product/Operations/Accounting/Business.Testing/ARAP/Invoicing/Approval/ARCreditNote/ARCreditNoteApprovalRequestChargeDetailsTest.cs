using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	[TestedType(typeof(ARCreditNoteApprovalRequestChargeDetails))]
	public class ARCreditNoteApprovalRequestChargeDetailsTest : ApprovalRequestChargeDetailsTest<ARCreditNoteApprovalRequestChargeDetails>
	{
		public void TestOSSellAmountForDisplayAndLocalSellAmountForDisplay()
		{
			var charge1 = new ARCreditNoteApprovalRequestChargeDetails(Factory);
			charge1.ApprovalType = Enterprise.Core.Constants.GenApprovalRequestApprovalType.ARCreditNote;
			charge1.SellAccount = "account";
			charge1.SellCurrency = "currency";
			charge1.OSSellAmount = 10M;
			charge1.LocalSellAmount = 20M;
			charge1.InvoiceType = "type";
			AssertEquals(10m, charge1.OSSellAmountForDisplay);
			AssertEquals(20m, charge1.LocalSellAmountForDisplay);

			var charge2 = new ARCreditNoteApprovalRequestChargeDetails(Factory);
			charge2.ApprovalType = Enterprise.Core.Constants.GenApprovalRequestApprovalType.ARCreditNoteForReversal;
			charge2.SellAccount = "account";
			charge2.SellCurrency = "currency";
			charge2.OSSellAmount = 30M;
			charge2.LocalSellAmount = 40M;
			charge2.InvoiceType = "type";
			AssertEquals(-30m, charge2.OSSellAmountForDisplay);
			AssertEquals(-40m, charge2.LocalSellAmountForDisplay);
		}

		public override void TestOpertorEqual()
		{
			base.TestOpertorEqual();

			var a = (ARCreditNoteApprovalRequestChargeDetails)GetNewBusinessObject();
			var b = (ARCreditNoteApprovalRequestChargeDetails)GetNewBusinessObject();

			a.SellAccount = "1";
			AssertEquals(false, a == b);
			b.SellAccount = "1";
			AssertEquals(true, a == b);

			a.SellCurrency = "1";
			AssertEquals(false, a == b);
			b.SellCurrency = "1";
			AssertEquals(true, a == b);

			a.OSSellAmount = 1M;
			AssertEquals(false, a == b);
			b.OSSellAmount = 1M;
			AssertEquals(true, a == b);

			a.LocalSellAmount = 1M;
			AssertEquals(false, a == b);
			b.LocalSellAmount = 1M;
			AssertEquals(true, a == b);

			a.InvoiceType = "1";
			AssertEquals(false, a == b);
			b.InvoiceType = "1";
			AssertEquals(true, a == b);

			a.OSTaxAmount = 2M;
			AssertEquals(false, a == b);
			b.OSTaxAmount = 2M;
			AssertEquals(true, a == b);

			a.LocalTaxAmount = 4M;
			AssertEquals(false, a == b);
			b.LocalTaxAmount = 4M;
			AssertEquals(true, a == b);

			a.TaxCode = "GST";
			AssertEquals(false, a == b);
			b.TaxCode = "GST";
			AssertEquals(true, a == b);

			a.ExchangeRate = 2.5;
			AssertEquals(false, a == b);
			b.ExchangeRate = 2.5M;
			AssertEquals(true, a == b);

			a.SupplyType = "LOC";
			AssertEquals(false, a == b);
			b.SupplyType = "LOC";
			AssertEquals(true, a == b);
		}

		public override void TestCopyFrom()
		{
			base.TestCopyFrom();

			var charge = (ARCreditNoteApprovalRequestChargeDetails)GetNewBusinessObject();

			charge.SellAccount = "account";
			charge.SellCurrency = "currency";
			charge.OSSellAmount = 10M;
			charge.LocalSellAmount = 20M;
			charge.InvoiceType = "type";
			charge.OSTaxAmount = 30M;
			charge.LocalTaxAmount = 60m;
			charge.ExchangeRate = 3.5M;
			charge.TaxCode = "GST";
			charge.SupplyType = "LOC";

			var newCharge = (ARCreditNoteApprovalRequestChargeDetails)GetNewBusinessObject();

			newCharge.CopyFrom(charge);
			AssertEquals("SellAccount", "account", newCharge.SellAccount);
			AssertEquals("SellCurrency", "currency", newCharge.SellCurrency);
			AssertEquals("OSSellAmount", 10M, newCharge.OSSellAmount);
			AssertEquals("LocalSellAmount", 20M, newCharge.LocalSellAmount);
			AssertEquals("InvoiceType", "type", newCharge.InvoiceType);
			AssertEquals("OSTaxAmount", 30M, newCharge.OSTaxAmount);
			AssertEquals("LocalTaxAmount", 60M, newCharge.LocalTaxAmount);
			AssertEquals("ExchangeRate", 3.5M, newCharge.ExchangeRate);
			AssertEquals("TaxCode", "GST", newCharge.TaxCode);
			AssertEquals("SupplyType", "LOC", newCharge.SupplyType);
		}

		protected override void SetInstanceSpecificBizOPropertiesForXMLTest(ARCreditNoteApprovalRequestChargeDetails charge)
		{
			charge.SellAccount = "account";
			charge.SellCurrency = "currency";
			charge.OSSellAmount = 10M;
			charge.LocalSellAmount = 20M;
			charge.InvoiceType = "type";
			charge.OSTaxAmount = 30M;
			charge.LocalTaxAmount = 60M;
			charge.ExchangeRate = 3.5;
			charge.TaxCode = "GST";
			charge.SupplyType = "LOC";
		}

		protected override string GetInstanceSpecificExpectedXML()
		{
			return "<SellAccount>account</SellAccount><SellCurrency>currency</SellCurrency><OSSellAmount>10</OSSellAmount><LocalSellAmount>20</LocalSellAmount><InvoiceType>type</InvoiceType><OSTaxAmount>30</OSTaxAmount><LocalTaxAmount>60</LocalTaxAmount><ExchangeRate>3.5</ExchangeRate><TaxCode>GST</TaxCode><SupplyType>LOC</SupplyType>";
		}

		protected override void AssertInstanceSpecificBizOPropertiesForXMLTest(ARCreditNoteApprovalRequestChargeDetails charge)
		{
			AssertEquals("SellAccount", "account", charge.SellAccount);
			AssertEquals("SellCurrency", "currency", charge.SellCurrency);
			AssertEquals("OSSellAmount", 10M, charge.OSSellAmount);
			AssertEquals("LocalSellAmount", 20M, charge.LocalSellAmount);
			AssertEquals("InvoiceType", "type", charge.InvoiceType);
			AssertEquals("OSTaxAmount", 30M, charge.OSTaxAmount);
			AssertEquals("LocalTaxAmount", 60M, charge.LocalTaxAmount);
			AssertEquals("ExchangeRate", 3.5M, charge.ExchangeRate);
			AssertEquals("TaxCode", "GST", charge.TaxCode);
			AssertEquals("SupplyType", "LOC", charge.SupplyType);
		}
	}
}
