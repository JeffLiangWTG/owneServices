using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.KR.GUI.Testing
{
	public class ProvisionalPricingReasonsUserControlTest : TestCase
	{
		public void TestBindingMembers()
		{
			AssertEquals(typeof(Business.JobComInvoiceHeader), control.BindingSource.DataSourceType);
			AssertEquals("OtherReasonTextBox Binding", "ProvisionalPricingReason119", control.FindSingle<ZTextBox>("OtherReasonTextBox").BindTo);
			AssertEquals("ProvisionalPricingReason101CheckBox Binding", "ProvisionalPricingReason101", control.FindSingle<ZCheckBox>("ProvisionalPricingReason101CheckBox").BindTo);
			AssertEquals("ProvisionalPricingReason102CheckBox Binding", "ProvisionalPricingReason102", control.FindSingle<ZCheckBox>("ProvisionalPricingReason102CheckBox").BindTo);
			AssertEquals("ProvisionalPricingReason103CheckBox Binding", "ProvisionalPricingReason103", control.FindSingle<ZCheckBox>("ProvisionalPricingReason103CheckBox").BindTo);
			AssertEquals("ProvisionalPricingReason104CheckBox Binding", "ProvisionalPricingReason104", control.FindSingle<ZCheckBox>("ProvisionalPricingReason104CheckBox").BindTo);
			AssertEquals("ProvisionalPricingReason105CheckBox Binding", "ProvisionalPricingReason105", control.FindSingle<ZCheckBox>("ProvisionalPricingReason105CheckBox").BindTo);
			AssertEquals("ProvisionalPricingReason106CheckBox Binding", "ProvisionalPricingReason106", control.FindSingle<ZCheckBox>("ProvisionalPricingReason106CheckBox").BindTo);
			AssertEquals("ProvisionalPricingReason107CheckBox Binding", "ProvisionalPricingReason107", control.FindSingle<ZCheckBox>("ProvisionalPricingReason107CheckBox").BindTo);
			AssertEquals("ProvisionalPricingReason108CheckBox Binding", "ProvisionalPricingReason108", control.FindSingle<ZCheckBox>("ProvisionalPricingReason108CheckBox").BindTo);
			AssertEquals("ProvisionalPricingReason109CheckBox Binding", "ProvisionalPricingReason109", control.FindSingle<ZCheckBox>("ProvisionalPricingReason109CheckBox").BindTo);
			AssertEquals("ProvisionalPricingReason110CheckBox Binding", "ProvisionalPricingReason110", control.FindSingle<ZCheckBox>("ProvisionalPricingReason110CheckBox").BindTo);
			AssertEquals("ProvisionalPricingReason111CheckBox Binding", "ProvisionalPricingReason111", control.FindSingle<ZCheckBox>("ProvisionalPricingReason111CheckBox").BindTo);
			AssertEquals("ProvisionalPricingReason112CheckBox Binding", "ProvisionalPricingReason112", control.FindSingle<ZCheckBox>("ProvisionalPricingReason112CheckBox").BindTo);
			AssertEquals("ProvisionalPricingReason113CheckBox Binding", "ProvisionalPricingReason113", control.FindSingle<ZCheckBox>("ProvisionalPricingReason113CheckBox").BindTo);
			AssertEquals("ProvisionalPricingReason114CheckBox Binding", "ProvisionalPricingReason114", control.FindSingle<ZCheckBox>("ProvisionalPricingReason114CheckBox").BindTo);
			AssertEquals("ProvisionalPricingReason115CheckBox Binding", "ProvisionalPricingReason115", control.FindSingle<ZCheckBox>("ProvisionalPricingReason115CheckBox").BindTo);
			AssertEquals("ProvisionalPricingReason116CheckBox Binding", "ProvisionalPricingReason116", control.FindSingle<ZCheckBox>("ProvisionalPricingReason116CheckBox").BindTo);
			AssertEquals("ProvisionalPricingReason117CheckBox Binding", "ProvisionalPricingReason117", control.FindSingle<ZCheckBox>("ProvisionalPricingReason117CheckBox").BindTo);
			AssertEquals("ProvisionalPricingReason120CheckBox Binding", "ProvisionalPricingReason120", control.FindSingle<ZCheckBox>("ProvisionalPricingReason120CheckBox").BindTo);

			control.BindToMessageSendingObject();
			AssertEquals(typeof(Business.ValuationDeclarationMessageSendingObjectParent), control.BindingSource.DataSourceType);
			AssertEquals("OtherReasonTextBox Binding", "SendingObjectsCollection.InvoiceHeader.ProvisionalPricingReason119", control.FindSingle<ZTextBox>("OtherReasonTextBox").BindTo);
			AssertEquals("ProvisionalPricingReason101CheckBox Binding", "SendingObjectsCollection.InvoiceHeader.ProvisionalPricingReason101", control.FindSingle<ZCheckBox>("ProvisionalPricingReason101CheckBox").BindTo);
			AssertEquals("ProvisionalPricingReason102CheckBox Binding", "SendingObjectsCollection.InvoiceHeader.ProvisionalPricingReason102", control.FindSingle<ZCheckBox>("ProvisionalPricingReason102CheckBox").BindTo);
			AssertEquals("ProvisionalPricingReason103CheckBox Binding", "SendingObjectsCollection.InvoiceHeader.ProvisionalPricingReason103", control.FindSingle<ZCheckBox>("ProvisionalPricingReason103CheckBox").BindTo);
			AssertEquals("ProvisionalPricingReason104CheckBox Binding", "SendingObjectsCollection.InvoiceHeader.ProvisionalPricingReason104", control.FindSingle<ZCheckBox>("ProvisionalPricingReason104CheckBox").BindTo);
			AssertEquals("ProvisionalPricingReason105CheckBox Binding", "SendingObjectsCollection.InvoiceHeader.ProvisionalPricingReason105", control.FindSingle<ZCheckBox>("ProvisionalPricingReason105CheckBox").BindTo);
			AssertEquals("ProvisionalPricingReason106CheckBox Binding", "SendingObjectsCollection.InvoiceHeader.ProvisionalPricingReason106", control.FindSingle<ZCheckBox>("ProvisionalPricingReason106CheckBox").BindTo);
			AssertEquals("ProvisionalPricingReason107CheckBox Binding", "SendingObjectsCollection.InvoiceHeader.ProvisionalPricingReason107", control.FindSingle<ZCheckBox>("ProvisionalPricingReason107CheckBox").BindTo);
			AssertEquals("ProvisionalPricingReason108CheckBox Binding", "SendingObjectsCollection.InvoiceHeader.ProvisionalPricingReason108", control.FindSingle<ZCheckBox>("ProvisionalPricingReason108CheckBox").BindTo);
			AssertEquals("ProvisionalPricingReason109CheckBox Binding", "SendingObjectsCollection.InvoiceHeader.ProvisionalPricingReason109", control.FindSingle<ZCheckBox>("ProvisionalPricingReason109CheckBox").BindTo);
			AssertEquals("ProvisionalPricingReason110CheckBox Binding", "SendingObjectsCollection.InvoiceHeader.ProvisionalPricingReason110", control.FindSingle<ZCheckBox>("ProvisionalPricingReason110CheckBox").BindTo);
			AssertEquals("ProvisionalPricingReason111CheckBox Binding", "SendingObjectsCollection.InvoiceHeader.ProvisionalPricingReason111", control.FindSingle<ZCheckBox>("ProvisionalPricingReason111CheckBox").BindTo);
			AssertEquals("ProvisionalPricingReason112CheckBox Binding", "SendingObjectsCollection.InvoiceHeader.ProvisionalPricingReason112", control.FindSingle<ZCheckBox>("ProvisionalPricingReason112CheckBox").BindTo);
			AssertEquals("ProvisionalPricingReason113CheckBox Binding", "SendingObjectsCollection.InvoiceHeader.ProvisionalPricingReason113", control.FindSingle<ZCheckBox>("ProvisionalPricingReason113CheckBox").BindTo);
			AssertEquals("ProvisionalPricingReason114CheckBox Binding", "SendingObjectsCollection.InvoiceHeader.ProvisionalPricingReason114", control.FindSingle<ZCheckBox>("ProvisionalPricingReason114CheckBox").BindTo);
			AssertEquals("ProvisionalPricingReason115CheckBox Binding", "SendingObjectsCollection.InvoiceHeader.ProvisionalPricingReason115", control.FindSingle<ZCheckBox>("ProvisionalPricingReason115CheckBox").BindTo);
			AssertEquals("ProvisionalPricingReason116CheckBox Binding", "SendingObjectsCollection.InvoiceHeader.ProvisionalPricingReason116", control.FindSingle<ZCheckBox>("ProvisionalPricingReason116CheckBox").BindTo);
			AssertEquals("ProvisionalPricingReason117CheckBox Binding", "SendingObjectsCollection.InvoiceHeader.ProvisionalPricingReason117", control.FindSingle<ZCheckBox>("ProvisionalPricingReason117CheckBox").BindTo);
			AssertEquals("ProvisionalPricingReason120CheckBox Binding", "SendingObjectsCollection.InvoiceHeader.ProvisionalPricingReason120", control.FindSingle<ZCheckBox>("ProvisionalPricingReason120CheckBox").BindTo);
		}

		protected override void SetUp()
		{
			base.SetUp();
			control = new ProvisionalPricingReasonsUserControl();
		}

		protected override void TearDown()
		{
			base.TearDown();
			control.Dispose();
		}

		ProvisionalPricingReasonsUserControl control;
	}
}
