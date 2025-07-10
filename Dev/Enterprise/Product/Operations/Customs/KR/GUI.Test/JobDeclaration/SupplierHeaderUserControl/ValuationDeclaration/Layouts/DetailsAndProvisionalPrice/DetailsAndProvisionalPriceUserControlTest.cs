using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.KR.GUI.Testing
{
	public class DetailsAndProvisionalPriceUserControlTest : TestCase
	{
		public void TestBindingMembers()
		{
			AssertEquals(typeof(Business.JobComInvoiceHeader), control.BindingSource.DataSourceType);
			var invoiceNoTextBox = control.FindSingle<ZTextBox>("InvoiceNoTextBox");
			var invoiceDateEdit = control.FindSingle<ZDateEdit>("InvoiceDateEdit");
			var purchaseOrderNoTextBox = control.FindSingle<ZTextBox>("PurchaseOrderNoTextBox");
			var purchaseOrderDateEdit = control.FindSingle<ZDateEdit>("PurchaseOrderDateEdit");
			var contractNoTextBox = control.FindSingle<ZTextBox>("ContractNoTextBox");
			var contractDateEdit = control.FindSingle<ZDateEdit>("ContractDateEdit");
			var totalCustomsValueCalcEdit = control.FindSingle<ZCalcEdit>("TotalCustomsValueCalcEdit");
			var provisionalPricingDropEdit = control.FindSingle<ZDropEdit>("ProvisionalPricingDropEdit");
			var provisionalAdditionRateCalcEdit = control.FindSingle<ZCalcEdit>("ProvisionalAdditionRateCalcEdit");
			var provisionalAdditionAmountCalcEdit = control.FindSingle<ZCalcEdit>("ProvisionalAdditionAmountCalcEdit");
			var estimatedDateOfFinalPriceDateEdit = control.FindSingle<ZDateEdit>("EstimatedDateOfFinalPriceDateEdit");
			var contractExpirationDateEdit = control.FindSingle<ZDateEdit>("ContractExpirationDateEdit");

			AssertEquals("JZ_InvoiceNumber", invoiceNoTextBox.BindTo);
			AssertEquals("JZ_InvoiceDate", invoiceDateEdit.BindTo);
			AssertEquals("PurchaseOrderNumber", purchaseOrderNoTextBox.BindTo);
			AssertEquals("PurchaseOrderDate", purchaseOrderDateEdit.BindTo);
			AssertEquals("ContractNumber", contractNoTextBox.BindTo);
			AssertEquals("ContractDate", contractDateEdit.BindTo);
			AssertEquals("CustomsValueKRW", totalCustomsValueCalcEdit.BindTo);
			AssertEquals("JZ_ProvPricingYN", provisionalPricingDropEdit.BindTo);
			AssertEquals("JZ_ProvAdditionalRate", provisionalAdditionRateCalcEdit.BindTo);
			AssertEquals("JZ_ProvAdditionalAmount", provisionalAdditionAmountCalcEdit.BindTo);
			AssertEquals("JZ_EstimatedDateOfFinalPrice", estimatedDateOfFinalPriceDateEdit.BindTo);
			AssertEquals("JZ_ImpContractExpiryDate", contractExpirationDateEdit.BindTo);

			Assert(!invoiceNoTextBox.ReadOnly);
			Assert(!invoiceDateEdit.ReadOnly);
			Assert(!purchaseOrderNoTextBox.ReadOnly);
			Assert(!purchaseOrderDateEdit.ReadOnly);
			Assert(!contractNoTextBox.ReadOnly);
			Assert(!contractDateEdit.ReadOnly);
			Assert(!totalCustomsValueCalcEdit.ReadOnly);
			Assert(!provisionalPricingDropEdit.ReadOnly);
			Assert(!provisionalAdditionRateCalcEdit.ReadOnly);
			Assert(!provisionalAdditionAmountCalcEdit.ReadOnly);
			Assert(!estimatedDateOfFinalPriceDateEdit.ReadOnly);
			Assert(!contractExpirationDateEdit.ReadOnly);

			control.BindToMessageSendingObject();
			invoiceNoTextBox = control.FindSingle<ZTextBox>("InvoiceNoTextBox");
			invoiceDateEdit = control.FindSingle<ZDateEdit>("InvoiceDateEdit");
			purchaseOrderNoTextBox = control.FindSingle<ZTextBox>("PurchaseOrderNoTextBox");
			purchaseOrderDateEdit = control.FindSingle<ZDateEdit>("PurchaseOrderDateEdit");
			contractNoTextBox = control.FindSingle<ZTextBox>("ContractNoTextBox");
			contractDateEdit = control.FindSingle<ZDateEdit>("ContractDateEdit");
			totalCustomsValueCalcEdit = control.FindSingle<ZCalcEdit>("TotalCustomsValueCalcEdit");
			provisionalPricingDropEdit = control.FindSingle<ZDropEdit>("ProvisionalPricingDropEdit");
			provisionalAdditionRateCalcEdit = control.FindSingle<ZCalcEdit>("ProvisionalAdditionRateCalcEdit");
			provisionalAdditionAmountCalcEdit = control.FindSingle<ZCalcEdit>("ProvisionalAdditionAmountCalcEdit");
			estimatedDateOfFinalPriceDateEdit = control.FindSingle<ZDateEdit>("EstimatedDateOfFinalPriceDateEdit");
			contractExpirationDateEdit = control.FindSingle<ZDateEdit>("ContractExpirationDateEdit");

			AssertEquals(typeof(Business.ValuationDeclarationMessageSendingObjectParent), control.BindingSource.DataSourceType);
			AssertEquals("SendingObjectsCollection.InvoiceHeader.JZ_InvoiceNumber", invoiceNoTextBox.BindTo);
			AssertEquals("SendingObjectsCollection.InvoiceHeader.JZ_InvoiceDate", invoiceDateEdit.BindTo);
			AssertEquals("SendingObjectsCollection.InvoiceHeader.PurchaseOrderNumber", purchaseOrderNoTextBox.BindTo);
			AssertEquals("SendingObjectsCollection.InvoiceHeader.PurchaseOrderDate", purchaseOrderDateEdit.BindTo);
			AssertEquals("SendingObjectsCollection.InvoiceHeader.ContractNumber", contractNoTextBox.BindTo);
			AssertEquals("SendingObjectsCollection.InvoiceHeader.ContractDate", contractDateEdit.BindTo);
			AssertEquals("SendingObjectsCollection.InvoiceHeader.CustomsValueKRW", totalCustomsValueCalcEdit.BindTo);
			AssertEquals("SendingObjectsCollection.InvoiceHeader.JZ_ProvPricingYN", provisionalPricingDropEdit.BindTo);
			AssertEquals("SendingObjectsCollection.InvoiceHeader.JZ_ProvAdditionalRate", provisionalAdditionRateCalcEdit.BindTo);
			AssertEquals("SendingObjectsCollection.InvoiceHeader.JZ_ProvAdditionalAmount", provisionalAdditionAmountCalcEdit.BindTo);
			AssertEquals("SendingObjectsCollection.InvoiceHeader.JZ_EstimatedDateOfFinalPrice", estimatedDateOfFinalPriceDateEdit.BindTo);
			AssertEquals("SendingObjectsCollection.InvoiceHeader.JZ_ImpContractExpiryDate", contractExpirationDateEdit.BindTo);

			Assert(invoiceNoTextBox.ReadOnly);
			Assert(invoiceDateEdit.ReadOnly);
			Assert(purchaseOrderNoTextBox.ReadOnly);
			Assert(purchaseOrderDateEdit.ReadOnly);
			Assert(contractNoTextBox.ReadOnly);
			Assert(contractDateEdit.ReadOnly);
			Assert(!totalCustomsValueCalcEdit.ReadOnly);
			Assert(provisionalPricingDropEdit.ReadOnly);
			Assert(provisionalAdditionRateCalcEdit.ReadOnly);
			Assert(provisionalAdditionAmountCalcEdit.ReadOnly);
			Assert(estimatedDateOfFinalPriceDateEdit.ReadOnly);
			Assert(contractExpirationDateEdit.ReadOnly);
		}

		protected override void SetUp()
		{
			base.SetUp();
			control = new DetailsAndProvisionalPriceUserControl();
		}

		protected override void TearDown()
		{
			base.TearDown();
			control.Dispose();
		}

		DetailsAndProvisionalPriceUserControl control;
	}
}
