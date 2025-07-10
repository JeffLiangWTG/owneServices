using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI.Testing
{
	public class ImportMethodTwoToSixUserControlTest : TestCaseWithFactory
	{
		public void TestBindingMembers()
		{
			AssertEquals(typeof(Business.JobDeclaration), control.BindingSource.DataSourceType);
			var expectedCustomsValueCalcEdit = control.FindSingle<ZCalcEdit>("ExpectedCustomsValueCalcEdit");
			var supportingDocument1TextBox = control.FindSingle<ZTextBox>("SupportingDocument1TextBox");
			var supportingDocument2TextBox = control.FindSingle<ZTextBox>("SupportingDocument2TextBox");
			var sampleItemCheckBox = control.FindSingle<ZCheckBox>("SampleItemCheckBox");
			var giftOrFreeDonationCheckBox = control.FindSingle<ZCheckBox>("GiftOrFreeDonationCheckBox");
			var advertisingUseCheckBox = control.FindSingle<ZCheckBox>("AdvertisingUseCheckBox");
			var forProductionAndManufactureCheckBox = control.FindSingle<ZCheckBox>("ForProductionAndManufactureCheckBox");
			var useOfDefectiveRepairCheckBox = control.FindSingle<ZCheckBox>("UseOfDefectiveRepairCheckBox");
			var replacementItemCheckBox = control.FindSingle<ZCheckBox>("ReplacementItemCheckBox");
			var performancePriceOfPaidTransactionCheckBox = control.FindSingle<ZCheckBox>("PerformancePriceOfPaidTransactionCheckBox");
			var invoiceCheckBox = control.FindSingle<ZCheckBox>("InvoiceCheckBox");
			var priceListCheckBox = control.FindSingle<ZCheckBox>("PriceListCheckBox");
			var manufacturingCostCheckBox = control.FindSingle<ZCheckBox>("ManufacturingCostCheckBox");
			var itemUseCodeOtherReasonTextBox = control.FindSingle<ZTextBox>("ItemUseCodeOtherReasonTextBox");
			var goodsPricingBasisOtherReasonTextBox = control.FindSingle<ZTextBox>("GoodsPricingBasisOtherReasonTextBox");

			AssertEquals("ExpectedCustomsValueCalcEdit Binding", "Invoices.ExpectedCustomsValue", expectedCustomsValueCalcEdit.BindTo);
			AssertEquals("SupportingDocument1TextBox Binding", "Invoices.ValuationSupportingDocument1", supportingDocument1TextBox.BindTo);
			AssertEquals("SupportingDocument2TextBox Binding", "Invoices.ValuationSupportingDocument2", supportingDocument2TextBox.BindTo);
			AssertEquals("SampleItemCheckBox Binding", "Invoices.ValuationDeclarationCode301", sampleItemCheckBox.BindTo);
			AssertEquals("GiftOrFreeDonationCheckBox Binding", "Invoices.ValuationDeclarationCode305", giftOrFreeDonationCheckBox.BindTo);
			AssertEquals("AdvertisingUseCheckBox Binding", "Invoices.ValuationDeclarationCode302", advertisingUseCheckBox.BindTo);
			AssertEquals("ForProductionAndManufactureCheckBox Binding", "Invoices.ValuationDeclarationCode306", forProductionAndManufactureCheckBox.BindTo);
			AssertEquals("UseOfDefectiveRepairCheckBox Binding", "Invoices.ValuationDeclarationCode303", useOfDefectiveRepairCheckBox.BindTo);
			AssertEquals("ReplacementItemCheckBox Binding", "Invoices.ValuationDeclarationCode304", replacementItemCheckBox.BindTo);
			AssertEquals("PerformancePriceOfPaidTransactionCheckBox Binding", "Invoices.ValuationDeclarationCode401", performancePriceOfPaidTransactionCheckBox.BindTo);
			AssertEquals("InvoiceCheckBox Binding", "Invoices.ValuationDeclarationCode404", invoiceCheckBox.BindTo);
			AssertEquals("PriceListCheckBox Binding", "Invoices.ValuationDeclarationCode402", priceListCheckBox.BindTo);
			AssertEquals("ManufacturingCostCheckBox Binding", "Invoices.ValuationDeclarationCode403", manufacturingCostCheckBox.BindTo);
			AssertEquals("ItemUseCodeOtherReasonTextBox Binding", "Invoices.ValuationDeclarationCode307", itemUseCodeOtherReasonTextBox.BindTo);
			AssertEquals("GoodsPricingBasisOtherReasonTextBox Binding", "Invoices.ValuationDeclarationCode405", goodsPricingBasisOtherReasonTextBox.BindTo);

			Assert(!supportingDocument1TextBox.ReadOnly);
			Assert(!supportingDocument2TextBox.ReadOnly);
			Assert(!sampleItemCheckBox.ReadOnly);
			Assert(!giftOrFreeDonationCheckBox.ReadOnly);
			Assert(!advertisingUseCheckBox.ReadOnly);
			Assert(!forProductionAndManufactureCheckBox.ReadOnly);
			Assert(!useOfDefectiveRepairCheckBox.ReadOnly);
			Assert(!replacementItemCheckBox.ReadOnly);
			Assert(!performancePriceOfPaidTransactionCheckBox.ReadOnly);
			Assert(!invoiceCheckBox.ReadOnly);
			Assert(!priceListCheckBox.ReadOnly);
			Assert(!manufacturingCostCheckBox.ReadOnly);
			Assert(!itemUseCodeOtherReasonTextBox.ReadOnly);
			Assert(!goodsPricingBasisOtherReasonTextBox.ReadOnly);

			control.BindToMessageSendingObject();
			expectedCustomsValueCalcEdit = control.FindSingle<ZCalcEdit>("ExpectedCustomsValueCalcEdit");
			supportingDocument1TextBox = control.FindSingle<ZTextBox>("SupportingDocument1TextBox");
			supportingDocument2TextBox = control.FindSingle<ZTextBox>("SupportingDocument2TextBox");
			sampleItemCheckBox = control.FindSingle<ZCheckBox>("SampleItemCheckBox");
			giftOrFreeDonationCheckBox = control.FindSingle<ZCheckBox>("GiftOrFreeDonationCheckBox");
			advertisingUseCheckBox = control.FindSingle<ZCheckBox>("AdvertisingUseCheckBox");
			forProductionAndManufactureCheckBox = control.FindSingle<ZCheckBox>("ForProductionAndManufactureCheckBox");
			useOfDefectiveRepairCheckBox = control.FindSingle<ZCheckBox>("UseOfDefectiveRepairCheckBox");
			replacementItemCheckBox = control.FindSingle<ZCheckBox>("ReplacementItemCheckBox");
			performancePriceOfPaidTransactionCheckBox = control.FindSingle<ZCheckBox>("PerformancePriceOfPaidTransactionCheckBox");
			invoiceCheckBox = control.FindSingle<ZCheckBox>("InvoiceCheckBox");
			priceListCheckBox = control.FindSingle<ZCheckBox>("PriceListCheckBox");
			manufacturingCostCheckBox = control.FindSingle<ZCheckBox>("ManufacturingCostCheckBox");
			itemUseCodeOtherReasonTextBox = control.FindSingle<ZTextBox>("ItemUseCodeOtherReasonTextBox");
			goodsPricingBasisOtherReasonTextBox = control.FindSingle<ZTextBox>("GoodsPricingBasisOtherReasonTextBox");

			AssertEquals(typeof(Business.ValuationDeclarationMessageSendingObjectParent), control.BindingSource.DataSourceType);
			AssertEquals("ExpectedCustomsValueCalcEdit Binding", "SendingObjectsCollection.InvoiceHeader.ExpectedCustomsValue", expectedCustomsValueCalcEdit.BindTo);
			AssertEquals("SupportingDocument1TextBox Binding", "SendingObjectsCollection.InvoiceHeader.ValuationSupportingDocument1", supportingDocument1TextBox.BindTo);
			AssertEquals("SupportingDocument2TextBox Binding", "SendingObjectsCollection.InvoiceHeader.ValuationSupportingDocument2", supportingDocument2TextBox.BindTo);
			AssertEquals("SampleItemCheckBox Binding", "SendingObjectsCollection.InvoiceHeader.ValuationDeclarationCode301", sampleItemCheckBox.BindTo);
			AssertEquals("GiftOrFreeDonationCheckBox Binding", "SendingObjectsCollection.InvoiceHeader.ValuationDeclarationCode305", giftOrFreeDonationCheckBox.BindTo);
			AssertEquals("AdvertisingUseCheckBox Binding", "SendingObjectsCollection.InvoiceHeader.ValuationDeclarationCode302", advertisingUseCheckBox.BindTo);
			AssertEquals("ForProductionAndManufactureCheckBox Binding", "SendingObjectsCollection.InvoiceHeader.ValuationDeclarationCode306", forProductionAndManufactureCheckBox.BindTo);
			AssertEquals("UseOfDefectiveRepairCheckBox Binding", "SendingObjectsCollection.InvoiceHeader.ValuationDeclarationCode303", useOfDefectiveRepairCheckBox.BindTo);
			AssertEquals("ReplacementItemCheckBox Binding", "SendingObjectsCollection.InvoiceHeader.ValuationDeclarationCode304", replacementItemCheckBox.BindTo);
			AssertEquals("PerformancePriceOfPaidTransactionCheckBox Binding", "SendingObjectsCollection.InvoiceHeader.ValuationDeclarationCode401", performancePriceOfPaidTransactionCheckBox.BindTo);
			AssertEquals("InvoiceCheckBox Binding", "SendingObjectsCollection.InvoiceHeader.ValuationDeclarationCode404", invoiceCheckBox.BindTo);
			AssertEquals("PriceListCheckBox Binding", "SendingObjectsCollection.InvoiceHeader.ValuationDeclarationCode402", priceListCheckBox.BindTo);
			AssertEquals("ManufacturingCostCheckBox Binding", "SendingObjectsCollection.InvoiceHeader.ValuationDeclarationCode403", manufacturingCostCheckBox.BindTo);
			AssertEquals("ItemUseCodeOtherReasonTextBox Binding", "SendingObjectsCollection.InvoiceHeader.ValuationDeclarationCode307", itemUseCodeOtherReasonTextBox.BindTo);
			AssertEquals("GoodsPricingBasisOtherReasonTextBox Binding", "SendingObjectsCollection.InvoiceHeader.ValuationDeclarationCode405", goodsPricingBasisOtherReasonTextBox.BindTo);

			Assert(supportingDocument1TextBox.ReadOnly);
			Assert(supportingDocument2TextBox.ReadOnly);
			Assert(sampleItemCheckBox.ReadOnly);
			Assert(giftOrFreeDonationCheckBox.ReadOnly);
			Assert(advertisingUseCheckBox.ReadOnly);
			Assert(forProductionAndManufactureCheckBox.ReadOnly);
			Assert(useOfDefectiveRepairCheckBox.ReadOnly);
			Assert(replacementItemCheckBox.ReadOnly);
			Assert(performancePriceOfPaidTransactionCheckBox.ReadOnly);
			Assert(invoiceCheckBox.ReadOnly);
			Assert(priceListCheckBox.ReadOnly);
			Assert(manufacturingCostCheckBox.ReadOnly);
			Assert(itemUseCodeOtherReasonTextBox.ReadOnly);
			Assert(goodsPricingBasisOtherReasonTextBox.ReadOnly);
		}

		protected override void SetUp()
		{
			base.SetUp();
			control = new MethodTwoToSixUserControl();
		}

		protected override void TearDown()
		{
			base.TearDown();
			control.Dispose();
		}

		MethodTwoToSixUserControl control;
	}
}
