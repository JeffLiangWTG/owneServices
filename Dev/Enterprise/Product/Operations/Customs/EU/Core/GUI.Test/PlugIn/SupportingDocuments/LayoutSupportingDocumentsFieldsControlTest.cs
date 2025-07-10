using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI.PlugIn.Testing
{
	class LayoutSupportingDocumentsFieldsControlTest : TestCaseWithFactory
	{
		public void TestSetCaption()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var document = invoice.SupportingDocuments.AddNew();

			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
			using (var form = new ZForm(document))
			using (var control = new LayoutSupportingDocumentsFieldsControlForTest(declaration))
			{
				form.Controls.Add(control);
				form.Show();

				AssertEquals("SupportingDocumentsGroupBox.Caption", "[2/3] Supporting Documents", control.SupportingDocumentsGroupBox.CaptionResourceString.Caption);
			}

			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, false))
			using (var form = new ZForm(document))
			using (var control = new LayoutSupportingDocumentsFieldsControlForTest(declaration))
			{
				form.Controls.Add(control);
				form.Show();

				AssertEquals("SupportingDocumentsGroupBox.Caption", "[44] Supporting Documents", control.SupportingDocumentsGroupBox.CaptionResourceString.Caption);
			}
		}

		public void TestGetLayout()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var document = invoice.SupportingDocuments.AddNew();

			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
			using (var form = new ZForm(document))
			using (var control = new LayoutSupportingDocumentsFieldsControlForTest(declaration))
			{
				form.Controls.Add(control);
				form.Show();

				AssertType<UCC6AndExportSupportingDocumentFieldsLayout>("LayoutSupportingDocumentsFieldsControl.Layout", control.GetLayout());
			}

			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, false))
			using (var form = new ZForm(document))
			using (var control = new LayoutSupportingDocumentsFieldsControlForTest(declaration))
			{
				form.Controls.Add(control);
				form.Show();

				AssertType<NonUCC6SupportingDocumentFieldsLayout>("LayoutSupportingDocumentsFieldsControl.Layout", control.GetLayout());
			}
		}

		public class LayoutSupportingDocumentsFieldsControlForTest : LayoutSupportingDocumentsFieldsControl
		{
			public LayoutSupportingDocumentsFieldsControlForTest(JobDeclaration declaration) : base(declaration)
			{
			}

			public new IPanelLayoutProvider GetLayout() => base.GetLayout();
		}
	}
}
