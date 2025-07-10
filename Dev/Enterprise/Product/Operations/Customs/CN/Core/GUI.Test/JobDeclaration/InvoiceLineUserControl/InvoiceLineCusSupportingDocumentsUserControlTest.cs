using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.CN.Business;

namespace Enterprise.Customs.CN.GUI.Testing
{
	public class InvoiceLineCusSupportingDocumentsUserControlTest : TestCaseWithFactory
	{
		public void TestControlsExistence()
		{
			using (var control = new InvoiceLineCusSupportingDocumentsUserControl())
			{
				TestUtility.AssertControlExistance(control, "CusSupportingDocumentsGrid", "FilteredCusSupportingDocuments");
				TestUtility.AssertControlExistance(control, "CusSupportingDocumentsGroupBox");
			}
		}

		public void TestCusSupportingDocumentsGridHasLineNumber()
		{
			using (var control = new InvoiceLineCusSupportingDocumentsUserControl())
			{
				AssertNotNull(control.CusSupportingDocumentsGrid.GetColumnStyle(CusSupportingDocument.Schema.DocumentType));
				AssertNotNull(control.CusSupportingDocumentsGrid.GetColumnStyle(CusSupportingDocument.Schema.CSI_ReferenceNumber));
				AssertNotNull(control.CusSupportingDocumentsGrid.GetColumnStyle(CusSupportingDocument.Schema.CSI_LineNo));
			}
		}
	}
}
