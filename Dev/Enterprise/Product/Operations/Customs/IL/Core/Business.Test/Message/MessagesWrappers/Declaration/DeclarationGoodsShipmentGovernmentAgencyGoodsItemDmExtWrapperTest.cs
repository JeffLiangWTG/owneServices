using System.Linq;
using CargoWise.Customs.IL.MessageDefinitions.DEC.IMP;

namespace Enterprise.Customs.IL.Business.Testing
{
	sealed class DeclarationGoodsShipmentGovernmentAgencyGoodsItemDmExtWrapperTest : Customs.Business.Testing.DataProviderTestCase<IDeclarationGoodsShipmentGovernmentAgencyGoodsItemDmExt>
	{
		public void TestNewOrNull()
		{
			AssertNull("When invoiceLine is null", DeclarationGoodsShipmentGovernmentAgencyGoodsItemDmExtWrapper.NewOrNull(null));
			AssertNotNull("When invoiceLine is null", DeclarationGoodsShipmentGovernmentAgencyGoodsItemDmExtWrapper.NewOrNull(invoiceLine));
		}

		public void TestCustomsBookType()
		{
			AssertEquals("DmExtensions Customs Book Type iss Import", "1", Provider.CustomsBookType.Value);
		}

		public void TestDeferredCustomsTax()
		{
			AssertNull(Provider.DeferredCustomsTax);
		}

		public void TestDeferredCustomsTaxValue()
		{
			AssertNull(Provider.DeferredCustomsTaxValue);
		}

		public void TestDeferredPurchaseTax()
		{
			AssertNull(Provider.DeferredPurchaseTax);
		}

		public void TestDeferredPurchaseTaxValue()
		{
			AssertNull(Provider.DeferredPurchaseTaxValue);
		}

		public void TestGoodsItemAmount()
		{
			var goodsItemAmounts = Provider.GoodsItemAmount;
			AssertNotNull(goodsItemAmounts);
			AssertEquals(1, goodsItemAmounts.Count);
			var goodsItemAmountWrapper = goodsItemAmounts.Single();
			AssertType<DeclarationGoodsShipmentGovernmentAgencyGoodsItemDmExtGoodsItemAmountWrapper>(goodsItemAmountWrapper);

			AssertEquals(Constants.JobDeclaration.BaseAmount, goodsItemAmountWrapper.AmountType.Value);
			AssertEquals(123.45m, goodsItemAmountWrapper.CustomsValueAmount.Value);
			AssertEquals(Iso3AlphaCurrencyCodeContentType.Usd, goodsItemAmountWrapper.CustomsValueAmount.CurrencyID);
		}

		public void TestInvoiceLineNumbers()
		{
			AssertNull(Provider.InvoiceLineNumbers);
		}

		public void TestIsUsed()
		{
			AssertNull(Provider.IsUsed);
		}

		public void TestIsUsedValue()
		{
			AssertNull(Provider.IsUsedValue);
		}

		public void TestOptionalTama()
		{
			AssertNull(Provider.OptionalTama);
		}

		public void TestPreferenceDocumentNumber()
		{
			AssertNull(Provider.PreferenceDocumentNumber);
		}

		public void TestSalesTaxExemptionType()
		{
			AssertNull(Provider.SalesTaxExemptionType);
		}

		public void TestTaxExemptCode()
		{
			AssertNull(Provider.TaxExemptCode);
		}

		public void TestVehicle()
		{
			AssertNull(Provider.Vehicle);
		}

		public void TestVehicleValuationAdjustment()
		{
			AssertNull(Provider.VehicleValuationAdjustment);
		}

		protected override IDeclarationGoodsShipmentGovernmentAgencyGoodsItemDmExt GetProvider()
			=> DeclarationGoodsShipmentGovernmentAgencyGoodsItemDmExtWrapper.NewOrNull(invoiceLine);

		protected override void SetUp()
		{
			base.SetUp();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_CustomsOffice = "IL123";
			declaration.JE_MessageType = "IMP";
			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceLine = (JobComInvoiceLine)invoiceHeader.InvoiceLines.AddNew();
			invoiceHeader.JZ_RX_NKInvoice_Currency = "USD";

			var merger = new LineMerger(declaration);
			merger.DoMerge();
			invoiceLine.CusEntryLine.CL_InvoiceAmount = 123.446m;
		}

		JobComInvoiceLine invoiceLine;
	}
}
