using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.BE.Business.Declaration;

namespace Enterprise.Customs.BE.GUI.PlugIn.Testing;

class EntryInstructionSupportingDocumentsUserControlTest : TestCaseWithFactory
{
	public void TestSupportingDocumentGroupBoxCaption()
	{
		using (var supportingDocumentsUserControl = new EntryInstructionSupportingDocumentsUserControl())
		{
			var declaration = Factory.New<JobDeclaration>();
			supportingDocumentsUserControl.SetDataBinding(declaration, null);
			AssertEquals(typeof(EntryInstructionSupportingDocumentsFieldsControl), supportingDocumentsUserControl.SupportingDocumentsFieldsControl.GetType());
		}
	}
}
