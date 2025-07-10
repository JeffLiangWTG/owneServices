using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI.PlugIn.Testing
{
	class InvoiceLayoutSupportingDocumentsFieldsControlTest : TestCaseWithFactory
	{
		public void TestGetLayout()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var document = invoice.SupportingDocuments.AddNew();

			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
			{
				declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Export;
				using (var form = new ZForm(document))
				using (var control = new InvoiceLayoutSupportingDocumentsFieldsControlForTest(declaration))
				{
					form.Controls.Add(control);
					form.Show();

					AssertType<InvoiceUCC6AndExportSupportingDocumentFieldsLayout>("UCC6 Export", control.GetLayout());
				}

				declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Import;
				using (var form = new ZForm(document))
				using (var control = new InvoiceLayoutSupportingDocumentsFieldsControlForTest(declaration))
				{
					form.Controls.Add(control);
					form.Show();

					AssertType<InvoiceUCC6AndImportSupportingDocumentFieldsLayout>("UCC6 Import", control.GetLayout());
				}
			}

			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, false))
			using (var form = new ZForm(document))
			using (var control = new InvoiceLayoutSupportingDocumentsFieldsControlForTest(declaration))
			{
				form.Controls.Add(control);
				form.Show();

				AssertType<NonUCC6SupportingDocumentFieldsLayout>("non-UCC6", control.GetLayout());
			}
		}

		public class InvoiceLayoutSupportingDocumentsFieldsControlForTest : InvoiceLayoutSupportingDocumentsFieldsControl
		{
			public InvoiceLayoutSupportingDocumentsFieldsControlForTest(JobDeclaration declaration) : base(declaration)
			{
			}

			public new IPanelLayoutProvider GetLayout() => base.GetLayout();
		}
	}
}
