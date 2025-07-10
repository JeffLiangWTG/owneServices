using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.IL.Business;

namespace Enterprise.Customs.IL.GUI.Testing
{
	public class PreviousDocumentsFieldsUserControlTest : TestCaseWithFactory
	{
		public void TestCodeDropEdit()
		{
			using (var control = new PreviousDocumentsFieldsUserControl())
			{
				var codeCodeFindBox = control.CodeCodeFindBox;
				AssertNotNull("CodeDropEdit", codeCodeFindBox);
				AssertEquals("CodeDropEdit BindTo", nameof(PreviousDocument.CSI_Code), codeCodeFindBox.BindTo);
			}
		}

		public void TestReferenceTextBox()
		{
			using (var control = new PreviousDocumentsFieldsUserControl())
			{
				var referenceTextBox = control.ReferenceTextBox;
				AssertNotNull("ReferenceTextBox", referenceTextBox);
				AssertEquals("ReferenceTextBox BindTo", nameof(PreviousDocument.CSI_ReferenceNumber), referenceTextBox.BindTo);
			}
		}
	}
}
