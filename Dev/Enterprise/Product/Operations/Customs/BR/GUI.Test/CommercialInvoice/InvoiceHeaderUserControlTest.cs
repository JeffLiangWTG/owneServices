using Enterprise.Customs.BR.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.BR.GUI.Testing
{
	sealed class InvoiceHeaderUserControlTest : Customs.GUI.Testing.InvoiceHeaderUserControlAbstractTest
	{
		public void TestOtherDetails()
		{
			var invoiceHeader = Factory.New<JobComInvoiceHeader>();
			using (var form = new CommercialInvoiceForm(invoiceHeader))
			{
				form.Show();
				var invoiceHeaderUserControl = form.Controls.Find("InvoiceHeaderUserControl", true)[0];
				AssertEquals(true, invoiceHeaderUserControl is InvoiceHeaderUserControl);
				var otherDetailsGroupBox = form.Controls.Find("OtherDetailsGroupBox", true)[0];
				invoiceHeader.JZ_MessageType = Common.BR.BRJobMessageTypeList.Codes.Import;
				AssertEquals(true, otherDetailsGroupBox.Visible);
				invoiceHeader.JZ_MessageType = Common.BR.BRJobMessageTypeList.Codes.Export;
				AssertEquals(false, otherDetailsGroupBox.Visible);
			}
		}

		public void TestMessageTypeChangesCaptions()
		{
			var invoiceHeader = Factory.New<JobComInvoiceHeader>();
			var declaration = (JobDeclaration)new Customs.Business.FakeDeclarationCreatorForInvoice(invoiceHeader).HeaderData;

			using (var form = new ZForm(declaration))
			{
				var control = new InvoiceHeaderUserControlForTesting();
				control.Invoice = invoiceHeader;
				form.Controls.Add(control);
				control.SetDataBinding(form.BusinessEntity, "");
				form.Show();

				invoiceHeader.JZ_MessageType = Common.BR.BRJobMessageTypeList.Codes.Export;
				AssertEquals("Column correct heading", "Dutiable", control.InvoiceChargesGridExposed.Columns[JobComInvHeaderChargeSchema.J7_IsDutiable.Name].ColumnStyle.HeaderText);
				AssertEquals("Column correct heading", "Add to CIF?", control.InvoiceChargesGridExposed.Columns[JobComInvHeaderChargeSchema.J7_IsGSTApplicable.Name].ColumnStyle.HeaderText);

				invoiceHeader.JZ_MessageType = Common.BR.BRJobMessageTypeList.Codes.Import;
				AssertEquals("Column correct heading", "Dutiable", control.InvoiceChargesGridExposed.Columns[JobComInvHeaderChargeSchema.J7_IsDutiable.Name].ColumnStyle.HeaderText);
				AssertEquals("Column correct heading", "CMT Apply", control.InvoiceChargesGridExposed.Columns[JobComInvHeaderChargeSchema.J7_IsGSTApplicable.Name].ColumnStyle.HeaderText);
			}
		}

		public void TestValuationCodeVisibility()
		{
			var invoiceHeader = Factory.New<JobComInvoiceHeader>();
			using (var form = new CommercialInvoiceForm(invoiceHeader))
			{
				form.Show();
				var valuationCode = form.Controls.Find("JZ_ValuationCodeDropEdit", true)[0];
				invoiceHeader.JZ_MessageType = Common.BR.BRJobMessageTypeList.Codes.Import;
				AssertEquals(true, valuationCode.Visible);
				invoiceHeader.JZ_MessageType = Common.BR.BRJobMessageTypeList.Codes.Export;
				AssertEquals(false, valuationCode.Visible);
				invoiceHeader.JZ_MessageType = Common.BR.BRJobMessageTypeList.Codes.ImportLicense;
				AssertEquals(false, valuationCode.Visible);
			}
		}

		protected override Customs.GUI.CommonInvoiceHeaderUserControl GetNewInvoiceHeaderUserControl() => new InvoiceHeaderUserControl();

		sealed class InvoiceHeaderUserControlForTesting : InvoiceHeaderUserControl
		{
			public ZGrid InvoiceChargesGridExposed => InvoiceChargesGrid;
		}
	}
}
