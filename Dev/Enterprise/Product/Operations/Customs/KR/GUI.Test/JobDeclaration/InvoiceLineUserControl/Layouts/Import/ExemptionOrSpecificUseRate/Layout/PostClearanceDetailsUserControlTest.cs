using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.KR.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;

namespace Enterprise.Customs.KR.GUI.Testing
{
	public class PostClearanceDetailsUserControlTest : TestCaseWithFactory
	{
		public void TestBindingMembers()
		{
			AssertEquals(typeof(JobDeclaration), control.BindingSource.DataSourceType);
			var postClearanceYNDropEdit = control.FindSingle<ZDropEdit>("PostClearanceYNDropEdit");
			var productTypeDropEdit = control.FindSingle<ZDropEdit>("ProductTypeDropEdit");
			var useCodeDescriptionTextBox = control.FindSingle<ZTextBox>("UseCodeDescriptionTextBox");
			var customsOfficeCodeFindBox = control.FindSingle<ZCodeFindBox>("CustomsOfficeCodeFindBox");
			var serialNumberTextBox = control.FindSingle<ZTextBox>("SerialNumberTextBox");
			var goodsLocationAddressControl = control.FindSingle<ZAddressControl>("GoodsLocationAddressControl");

			AssertEquals("FilteredInvoiceLines.JI_PCProcedure", postClearanceYNDropEdit.BindTo);
			AssertEquals("FilteredInvoiceLines.JI_SpecificUseProductType", productTypeDropEdit.BindTo);
			AssertEquals("FilteredInvoiceLines.JI_SpecificUseCodeDescription", useCodeDescriptionTextBox.BindTo);
			AssertEquals("FilteredInvoiceLines.JI_JurisdictionalCusOffice", customsOfficeCodeFindBox.BindTo);
			AssertEquals("FilteredInvoiceLines.JI_SerialNumber", serialNumberTextBox.BindTo);
			AssertEquals("FilteredInvoiceLines.JI_OA_ConsigneeAddress", goodsLocationAddressControl.BindTo);

			Assert(!postClearanceYNDropEdit.ReadOnly);
			Assert(!productTypeDropEdit.ReadOnly);
			Assert(!useCodeDescriptionTextBox.ReadOnly);
			Assert(!customsOfficeCodeFindBox.ReadOnly);
			Assert(!serialNumberTextBox.ReadOnly);
			Assert(!goodsLocationAddressControl.ReadOnly);
			Assert(!goodsLocationAddressControl.FindSingle<ZAddressDropEdit>("AddressDropEdit").ReadOnly);

			control.BindToMessageSendingObject();
			postClearanceYNDropEdit = control.FindSingle<ZDropEdit>("PostClearanceYNDropEdit");
			productTypeDropEdit = control.FindSingle<ZDropEdit>("ProductTypeDropEdit");
			useCodeDescriptionTextBox = control.FindSingle<ZTextBox>("UseCodeDescriptionTextBox");
			customsOfficeCodeFindBox = control.FindSingle<ZCodeFindBox>("CustomsOfficeCodeFindBox");
			serialNumberTextBox = control.FindSingle<ZTextBox>("SerialNumberTextBox");
			goodsLocationAddressControl = control.FindSingle<ZAddressControl>("GoodsLocationAddressControl");

			AssertEquals(typeof(JobDeclarationMiscMessageSendingObjectParent), control.BindingSource.DataSourceType);
			AssertEquals("SendingObjectsCollection.MessageSendingEntryLines.InvoiceLine.JI_PCProcedure", postClearanceYNDropEdit.BindTo);
			AssertEquals("SendingObjectsCollection.MessageSendingEntryLines.InvoiceLine.JI_SpecificUseProductType", productTypeDropEdit.BindTo);
			AssertEquals("SendingObjectsCollection.MessageSendingEntryLines.InvoiceLine.JI_SpecificUseCodeDescription", useCodeDescriptionTextBox.BindTo);
			AssertEquals("SendingObjectsCollection.MessageSendingEntryLines.InvoiceLine.JI_JurisdictionalCusOffice", customsOfficeCodeFindBox.BindTo);
			AssertEquals("SendingObjectsCollection.MessageSendingEntryLines.InvoiceLine.JI_SerialNumber", serialNumberTextBox.BindTo);
			AssertEquals("SendingObjectsCollection.MessageSendingEntryLines.InvoiceLine.JI_OA_ConsigneeAddress", goodsLocationAddressControl.BindTo);

			Assert(postClearanceYNDropEdit.ReadOnly);
			Assert(productTypeDropEdit.ReadOnly);
			Assert(useCodeDescriptionTextBox.ReadOnly);
			Assert(customsOfficeCodeFindBox.ReadOnly);
			Assert(serialNumberTextBox.ReadOnly);
			Assert(goodsLocationAddressControl.ReadOnly);
			Assert(goodsLocationAddressControl.FindSingle<ZAddressDropEdit>("AddressDropEdit").ReadOnly);
		}

		protected override void SetUp()
		{
			base.SetUp();
			control = new PostClearanceDetailsUserControl();
		}

		protected override void TearDown()
		{
			base.TearDown();
			control.Dispose();
		}

		PostClearanceDetailsUserControl control;
	}
}
