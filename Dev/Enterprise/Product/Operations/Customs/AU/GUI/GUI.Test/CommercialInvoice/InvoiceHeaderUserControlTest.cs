using System.Windows.Forms;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.AU;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.GUI.CommercialInvoice.Testing
{
	sealed class InvoiceHeaderUserControlTest : Customs.GUI.Testing.InvoiceHeaderUserControlAbstractTest
	{
		public void TestOrganisationFindBoxCaptions()
		{
			JobComInvoiceHeader invoice = Factory.New<JobComInvoiceHeader>();
			using (ZForm form = new ZForm((JobDeclaration)new FakeDeclarationCreatorForInvoice(invoice).HeaderData))
			{
				InvoiceHeaderUserControl control = new InvoiceHeaderUserControl();
				control.Invoice = invoice;
				form.Controls.Add(control);
				control.SetDataBinding(form.BusinessEntity, "");
				form.Show();
				AssertEquals("Supplier", control.FindSingle<ZOrganisationControl>("SupplierOrganisationControl").CaptionResourceString.Caption);
				AssertEquals("Importer", control.FindSingle<ZOrganisationControl>("ImporterOrganisationControl").CaptionResourceString.Caption);
			}
		}

		public void TestMessageTypeChangesVisibleControls()
		{
			var invoice = Factory.New<JobComInvoiceHeader>();
			using (var form = new ZForm((JobDeclaration)new FakeDeclarationCreatorForInvoice(invoice).HeaderData))
			{
				var testControl = new InvoiceHeaderUserControl();
				testControl.Invoice = invoice;
				form.Controls.Add(testControl);
				testControl.SetDataBinding(form.BusinessEntity, "");
				form.Show();
				var invoiceChargesGrid = testControl.FindSingle<ZGrid>("InvoiceChargesGrid");
				invoice.JZ_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
				AssertNotVisible(testControl.zA_VALB_HiddenDropEdit);
				AssertNotVisible(testControl.zA_HeaderREL_HiddenDropEdit);
				AssertNotVisible(testControl.pOCCodeFindBox);
				AssertNotVisible(testControl.preferenceSchemeTypeDropDown);
				AssertNotVisible(testControl.preferenceRuleTypeDropEdit);
				AssertNotVisible(testControl.zA_GSTECodeFindBox);
				AssertNotVisible(testControl.jZ_AddInfoBoundAddInfoControl);
				AssertNotVisible(testControl.invoiceOriginCodeFindBox);
				AssertNotVisible(testControl.exporterReferenceTextBox);
				AssertEquals("Column correct heading", "Add to FOB?", invoiceChargesGrid.Columns[Common.AutoJobComInvHeaderCharge.Schema.J7_IsDutiable].ColumnStyle.HeaderText);
				AssertEquals("Column correct heading", "Add to CIF?", invoiceChargesGrid.Columns[Common.AutoJobComInvHeaderCharge.Schema.J7_IsGSTApplicable].ColumnStyle.HeaderText);
				invoice.JZ_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
				AssertVisible(testControl.zA_VALB_HiddenDropEdit);
				AssertVisible(testControl.zA_HeaderREL_HiddenDropEdit);
				AssertVisible(testControl.pOCCodeFindBox);
				AssertVisible(testControl.preferenceSchemeTypeDropDown);
				AssertVisible(testControl.preferenceRuleTypeDropEdit);
				AssertVisible(testControl.zA_GSTECodeFindBox);
				AssertVisible(testControl.jZ_AddInfoBoundAddInfoControl);
				AssertNotVisible(testControl.exporterReferenceTextBox);
				AssertEquals("Column correct heading", "Dutiable", invoiceChargesGrid.Columns[Common.AutoJobComInvHeaderCharge.Schema.J7_IsDutiable].ColumnStyle.HeaderText);
				AssertEquals("Column correct heading", "GST Apply", invoiceChargesGrid.Columns[Common.AutoJobComInvHeaderCharge.Schema.J7_IsGSTApplicable].ColumnStyle.HeaderText);
				invoice.JZ_MessageType = AUJobMessageTypeList.Codes.Quarantine;
				AssertNotVisible(testControl.zA_VALB_HiddenDropEdit);
				AssertNotVisible(testControl.zA_HeaderREL_HiddenDropEdit);
				AssertNotVisible(testControl.pOCCodeFindBox);
				AssertNotVisible(testControl.preferenceSchemeTypeDropDown);
				AssertNotVisible(testControl.preferenceRuleTypeDropEdit);
				AssertNotVisible(testControl.zA_GSTECodeFindBox);
				AssertNotVisible(testControl.jZ_AddInfoBoundAddInfoControl);
				AssertVisible(testControl.exporterReferenceTextBox);
				AssertEquals("Column correct heading", "Add to FOB?", invoiceChargesGrid.Columns[Common.AutoJobComInvHeaderCharge.Schema.J7_IsDutiable].ColumnStyle.HeaderText);
				AssertEquals("Column correct heading", "Add to CIF?", invoiceChargesGrid.Columns[Common.AutoJobComInvHeaderCharge.Schema.J7_IsGSTApplicable].ColumnStyle.HeaderText);
			}

			void AssertNotVisible(Control control)
			{
				Assert("Control " + control.Name + " should not be visible", !control.Visible);
			}

			void AssertVisible(Control control)
			{
				Assert("Control " + control.Name + " should visible", control.Visible);
			}
		}

		protected override Customs.GUI.CommonInvoiceHeaderUserControl GetNewInvoiceHeaderUserControl() => new InvoiceHeaderUserControl();
	}
}
