using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IT.GUI.Testing;

sealed class ExportEntryInstructionLayoutSupportingDocumentsFieldsControlTest : TestCaseWithFactory
{
	public void TestGetLayout()
	{
		var supportingDocument = Factory.New<SupportingDocument>();

		using (var form = new ZForm(supportingDocument))
		using (var userControl = new ExportEntryInstructionLayoutSupportingDocumentsFieldsControlForTest(null))
		{
			form.Controls.Add(userControl);
			form.Show();

			AssertType<ExportEntryInstructionSupportingDocumentFieldsLayout>("Layout", userControl.GetLayoutExposed());
		}
	}

	public class ExportEntryInstructionLayoutSupportingDocumentsFieldsControlForTest : ExportEntryInstructionLayoutSupportingDocumentsFieldsControl
	{
		public ExportEntryInstructionLayoutSupportingDocumentsFieldsControlForTest(EU.Business.Declaration.JobDeclaration declaration) : base(declaration)
		{
		}

		public IPanelLayoutProvider GetLayoutExposed() => base.GetLayout();
	}
}
