using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IT.GUI.Testing;

sealed class ImportEntryInstructionLayoutSupportingDocumentsFieldsControlTest : TestCaseWithFactory
{
	public void TestGetLayout()
	{
		var supportingDocument = Factory.New<SupportingDocument>();

		using (var form = new ZForm(supportingDocument))
		using (var userControl = new ImportEntryInstructionLayoutSupportingDocumentsFieldsControlForTest(null))
		{
			form.Controls.Add(userControl);
			form.Show();

			AssertType<ImportEntryInstructionSupportingDocumentFieldsLayout>("Layout", userControl.GetLayoutExposed());
		}
	}

	public class ImportEntryInstructionLayoutSupportingDocumentsFieldsControlForTest : ImportEntryInstructionLayoutSupportingDocumentsFieldsControl
	{
		public ImportEntryInstructionLayoutSupportingDocumentsFieldsControlForTest(EU.Business.Declaration.JobDeclaration declaration) : base(declaration)
		{
		}

		public IPanelLayoutProvider GetLayoutExposed() => base.GetLayout();
	}
}
