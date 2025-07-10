using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common.IE;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.ZArchitecture.GUI;
namespace Enterprise.Customs.IE.GUI.Testing
{
	sealed class InvoiceLayoutSupportingDocumentsFieldsControlTest : TestCaseWithFactory
	{
		public void TestSetCaption()
		{
			var declaration = Factory.New<JobDeclaration>();
			using var ucc5 = ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetDeclarationConfiguration(declaration, "IsUCC5Core", true);

			var invoice = declaration.Invoices.AddNew();
			var document = invoice.SupportingDocuments.AddNew();

			using var form = new ZForm(document);
			using var control = new LayoutSupportingDocumentsFieldsControlForTesting(declaration);
			form.Controls.Add(control);
			form.Show();
			declaration.JE_MessageType = IEJobMessageTypeList.Codes.Export;
			control.SetCaption();
			var supportingDocumentsGroupBox = control.FindSingle<ZGroupBox>("SupportingDocumentsGroupBox");
			AssertEquals("SupportingDocumentsGroupBox.Caption", "Supporting Documents", supportingDocumentsGroupBox.CaptionResourceString.Caption);

			declaration.JE_MessageType = IEJobMessageTypeList.Codes.Import;
			control.SetCaption();
			AssertEquals("SupportingDocumentsGroupBox.Caption", "[2/3] Supporting Documents", supportingDocumentsGroupBox.CaptionResourceString.Caption);
		}
	}

	sealed class LayoutSupportingDocumentsFieldsControlForTesting : InvoiceLayoutSupportingDocumentsFieldsControl
	{
		public LayoutSupportingDocumentsFieldsControlForTesting(JobDeclaration declaration)
			: base(declaration)
		{
		}
		public new void SetCaption()
		{
			base.SetCaption();
		}
	}
}
