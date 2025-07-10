using Enterprise.Customs.Business;
using Enterprise.Customs.CA.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CA.GUI.Testing
{
	sealed class InvoiceHeaderUserControlTest : Customs.GUI.Testing.InvoiceHeaderUserControlAbstractTest
	{
		public void TestChangeGSTApplicableText()
		{
			var invoiceHeader = Factory.New<JobComInvoiceHeader>();
			var declaration = (JobDeclaration)new FakeDeclarationCreatorForInvoice(invoiceHeader).HeaderData;
			invoiceHeader.JZ_MessageType = Enterprise.Customs.CA.Business.JobMessageTypeList.Codes.Import;
			using (ZForm form = new ZForm(declaration))
			{
				InvoiceHeaderUserControl control = new InvoiceHeaderUserControl();
				control.Invoice = invoiceHeader;
				form.Controls.Add(control);
				control.SetDataBinding(form.BusinessEntity, "");
				form.Show();
				var invoiceChargesGrid = control.FindSingle<ZGrid>("InvoiceChargesGrid");
				AssertEquals("InvoiceChargesGrid GST Applicable column text", CACustomsSupplierHeaderUserControl.IsCIFComponent, invoiceChargesGrid.GetColumnStyle(JobComInvHeaderChargeSchema.J7_IsGSTApplicable.Name).Caption);
			}
		}

		public void TestMessageTypeChangesCaptions()
		{
			var invoiceHeader = Factory.New<JobComInvoiceHeader>();
			var declaration = (JobDeclaration)new FakeDeclarationCreatorForInvoice(invoiceHeader).HeaderData;

			using (var form = new ZForm(declaration))
			{
				var control = GetNewInvoiceHeaderUserControl();
				control.Invoice = invoiceHeader;
				form.Controls.Add(control);
				control.SetDataBinding(form.BusinessEntity, "");
				form.Show();

				var grid = control.FindSingle<ZGrid>(c => c.Name == "InvoiceChargesGrid");
				var columnIsGSTApplicable = grid.Columns[InvoiceCharge.Schema.J7_IsGSTApplicable].ColumnStyle;
				invoiceHeader.JZ_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
				AssertEquals("InvoiceChargesGrid GST Applicable column text", CACustomsSupplierHeaderUserControl.IsCIFComponent, columnIsGSTApplicable.HeaderText);

				invoiceHeader.JZ_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
				AssertEquals("InvoiceChargesGrid GST Applicable column text", CACustomsSupplierHeaderUserControl.IsCIFComponent, columnIsGSTApplicable.HeaderText);
			}
		}

		protected override Customs.GUI.CommonInvoiceHeaderUserControl GetNewInvoiceHeaderUserControl() => new InvoiceHeaderUserControl();
	}
}
