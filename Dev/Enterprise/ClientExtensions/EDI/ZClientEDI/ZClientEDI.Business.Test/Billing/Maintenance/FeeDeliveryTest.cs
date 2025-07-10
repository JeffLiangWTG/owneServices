using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Client.EDI.Billing.Business.Test;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Billing.Business.Maintenance.Test
{
	[TestedType(typeof(FeeDelivery))]
	internal class FeeDeliveryTest : NonPersistentBusinessObjectTestCase
	{
		public void TestValidateAll()
		{
			var licHeader = BillingTestHelper.CreateLicence(Factory, "AAA");
			ClientInvoiceDelivery delivery = null;
			var recipient = NewRecipient(licHeader);
			var fee = BillingTestHelper.CreateMaintenanceFee(licHeader.Company, "Fee 1", 111m, "UPGASS");
			var feeDelivery = new FeeDelivery(Factory, recipient, fee, ZDateTime.Empty, delivery, Enumerable.Empty<ClientChargeableUsage>());
			AssertHasRowMessageError(feeDelivery, "No Invoicing Delivery Instructions found");

			delivery = licHeader.Company.InvoiceDeliveries.AddNew();
			feeDelivery = new FeeDelivery(Factory, recipient, fee, ZDateTime.Empty, delivery, Enumerable.Empty<ClientChargeableUsage>());
			AssertNoRowMessageErrors(feeDelivery);
			AssertNoRowErrors(feeDelivery);
		}

		public void TestPopulateInvoice()
		{
			RefCurrency usd = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "USD");
			RefExchangeRate usdExchange = usd.ExchangeRates.AddNew();
			usdExchange.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.SellRate;
			usdExchange.RE_SellRate = 2;
			usdExchange.RE_StartDate = ZDateTime.Today.AddMonths(-1);
			usdExchange.RE_ExpiryDate = ZDateTime.Today.AddMonths(1);

			BillingTestHelper.CreateChargeCodeForBranch(Factory, GlbBranch.CurrentBranch, null, EDIDataRegistry.Instance.CommentChargeCode.Value)
				.AC_ChargeType = Core.Constants.ChargeType.Comment;

			Factory.Save();

			var licHeader = BillingTestHelper.CreateLicence(Factory, "AAA");
			var delivery = licHeader.Company.InvoiceDeliveries.AddNew();
			var recipient = NewRecipient(licHeader);
			var fee = BillingTestHelper.CreateMaintenanceFee(licHeader.Company, "Fee 1", 111.20m, "UPGASS");
			fee.L8_TaxDateCode = BillingConstants.Fee.TaxDateCode.FeeTaxAtEndDate;
			var feeDate = BillingTestHelper.MonthToday.AddMonths(-1);
			var feeDelivery = new FeeDelivery(Factory, recipient, fee, feeDate, delivery, Enumerable.Empty<ClientChargeableUsage>());

			// Same currencies
			ARInvoice invoice = Factory.New<ARInvoice>();
			feeDelivery.PopulateInvoice(invoice);
			AssertEquals("line count", 1, invoice.Lines.Count);
			AssertEquals(new ZDecimal(111.2m), invoice.Lines[0].AL_OSExTaxAmount);
			AssertEquals("description", "Fee 1", invoice.Lines[0].AL_Desc);
			AssertEquals("UPGASS", invoice.Lines[0].ChargeCode.AC_Code);
			AssertEquals("AL_TaxDate", feeDate.AddMonths(12).AddDays(-1), invoice.Lines[0].AL_TaxDate);

			// Different currencies
			fee.L8_RX_NKCurrency = "USD";
			fee.L8_TaxDateCode = BillingConstants.Fee.TaxDateCode.FeeTaxAtCurrentDate;
			feeDelivery = new FeeDelivery(Factory, recipient, fee, ZDateTime.Empty, delivery, Enumerable.Empty<ClientChargeableUsage>());
			AssertEquals(1m / 2m, feeDelivery.ExchangeRate);
			invoice = Factory.New<ARInvoice>();
			feeDelivery.PopulateInvoice(invoice);
			AssertEquals("line count", 2, invoice.Lines.Count);
			AssertEquals(new ZDecimal(55.6m), invoice.Lines[0].AL_OSExTaxAmount);
			AssertEquals("description", "Fee 1", invoice.Lines[0].AL_Desc);
			AssertEquals("UPGASS", invoice.Lines[0].ChargeCode.AC_Code);

			AssertEquals("description", "Original currency amount 111.20 USD", invoice.Lines[1].AL_Desc);
			AssertEquals("COMMENT", invoice.Lines[1].ChargeCode.AC_Code);

			// OnInvoiceChanged
			AssertEquals(true, Factory.Load<ClientChargeableUsage>(new ZQuery(ClientChargeableUsageSchema.U1_AH_Invoice, invoice.PK)).Any());
			var newInvoice = Factory.New<ARInvoice>();
			((IInvoiceUpdateNotifier)feeDelivery).OnInvoiceChanged(newInvoice);
			AssertEquals(newInvoice, feeDelivery.Invoice);
			AssertEquals(true, Factory.Load<ClientChargeableUsage>(new ZQuery(ClientChargeableUsageSchema.U1_AH_Invoice, newInvoice.PK)).Any());
		}

		public void TestPopulateInvoice_AlreadyInvoiced()
		{
			var licHeader = BillingTestHelper.CreateLicence(Factory, "AAA");
			var delivery = licHeader.Company.InvoiceDeliveries.AddNew();
			var recipient = NewRecipient(licHeader);
			var fee = BillingTestHelper.CreateMaintenanceFee(licHeader.Company, "Fee 1", 111m, "UPGASS");
			var feeDelivery = new FeeDelivery(Factory, recipient, fee, ZDateTime.Empty, delivery, Enumerable.Empty<ClientChargeableUsage>());

			AssertEquals("not IsInvoiced", false, feeDelivery.IsInvoiced);
			AssertNull("no Invoice", feeDelivery.Invoice);

			ARInvoice invoice = Factory.New<ARInvoice>();
			invoice.AH_OH = licHeader.Company.LC_OH;
			invoice.AH_FullyPaidDate = ZDateTime.Empty;
			invoice.AH_TransactionCategory = Enterprise.ZArchitecture.Core.InvoiceTypesList.Codes.FinalInvoice;
			invoice.AH_RX_NKTransactionCurrency = "AUD";

			feeDelivery.PopulateInvoice(invoice);

			AssertEquals("IsInvoiced", true, feeDelivery.IsInvoiced);
			AssertEquals("Invoice", invoice.PK, feeDelivery.Invoice.PK);
			AssertEquals("CanInvoice", false, feeDelivery.CanInvoice);
			AssertEquals("CanForceInvoice", true, feeDelivery.CanForceInvoice);

			Factory.Save();

			var factory2 = new BusinessObjectFactory();
			var feeDelivery2 = new FeeDelivery(factory2, recipient, fee, ZDateTime.Empty, delivery, null);
			AssertEquals("IsInvoiced in factory2", true, feeDelivery2.IsInvoiced);
			AssertEquals("Invoice in factory2", invoice.PK, feeDelivery2.Invoice.PK);
			AssertEquals("CanInvoice", false, feeDelivery2.CanInvoice);
			AssertEquals("CanForceInvoice", true, feeDelivery2.CanForceInvoice);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return new FeeDelivery(Factory, null, null, ZDateTime.Empty, null, Enumerable.Empty<ClientChargeableUsage>());
		}

		MaintenanceBillRecipient NewRecipient(LicenceHeader lic)
		{
			return new MaintenanceBillRecipient(Factory, Env.CurrentBranch.PK, lic.Company.LC_OH, "AUD", new ZDateTime(2010, 1, 1), ZDateTime.Today);
		}

		protected override void SetUp()
		{
			base.SetUp();

			BillingTestHelper.CreateChargeCode(Factory, null, "UPGASS");
			Factory.Save();
		}

		#endregion
	}

	internal class FeeDeliveryForTest : FeeDelivery
	{
		public FeeDeliveryForTest(BusinessObjectFactory factory, MaintenanceBillRecipient billRecipient, ClientLicenceFee fee, ClientInvoiceDelivery invoiceDelivery)
			: base(factory, billRecipient, fee, ZDateTime.Empty, invoiceDelivery, Enumerable.Empty<ClientChargeableUsage>())
		{
		}

		public override void PopulateInvoice(ARInvoice invoice, string description)
		{
			++PopulateInvoiceCalled;
			if (CallBasePopulateInvoice)
			{
				base.PopulateInvoice(invoice, description);
			}
		}

		public int PopulateInvoiceCalled;
		public bool CallBasePopulateInvoice;
	}
}
