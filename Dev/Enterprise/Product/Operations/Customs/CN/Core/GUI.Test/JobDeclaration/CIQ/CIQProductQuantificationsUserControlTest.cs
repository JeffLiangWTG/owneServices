using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.CN.Business.Testing;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CN.GUI.Testing
{
	class CIQProductQuantificationsUserControlTest : TestCaseWithFactory
	{
		public void TestControlsExistence()
		{
			using (var control = new CIQProductQuantificationsUserControl())
			{
				TestUtility.AssertControlExistance(control, "QuantificationGrid", "CIQProductQualifications");
				TestUtility.AssertControlExistance(control, "CSI_CodeDropEdit", "CIQProductQualifications.CSI_Code");
				TestUtility.AssertControlExistance(control, "CSI_ReferenceNumberTextBox", "CIQProductQualifications.CSI_ReferenceNumber");
				TestUtility.AssertControlExistance(control, "CSI_LineNoCalcEdit", "CIQProductQualifications.CSI_LineNo");
				TestUtility.AssertControlExistance(control, "CSI_UnitOfQuantityDescriptionTextBox", "CIQProductQualifications.CSI_UnitOfQuantityDescription");
				TestUtility.AssertControlExistance(control, "BillOfLadingDateEdit", "EntryInstruction.BillOfLadingDate");
				TestUtility.AssertControlExistance(control, "VIN_ChassisNoTextBox", "VINDataCollection.XC_ChassisNo");
				TestUtility.AssertControlExistance(control, "VIN_EngineNoTextBox", "VINDataCollection.XC_EngineNo");
				TestUtility.AssertControlExistance(control, "VIN_ModelENTextBox", "VINDataCollection.XC_ModelEN");
				TestUtility.AssertControlExistance(control, "VIN_ProductNameCNTextBox", "VINDataCollection.XC_ProductNameCN");
				TestUtility.AssertControlExistance(control, "VIN_ProductNameENTextBox", "VINDataCollection.XC_ProductNameEN");
				TestUtility.AssertControlExistance(control, "VIN_QGPTextBox", "VINDataCollection.XC_QGP");
				TestUtility.AssertControlExistance(control, "VIN_VINTextBox", "VINDataCollection.XC_VIN");
				TestUtility.AssertControlExistance(control, "VINCollectionGrid", "VINDataCollection");
			}
		}

		public void TestVINControlsVisibility()
		{
			var testItems = CNCusEntryHeaderHelper.SetupCusEntryHeader(Factory, () => Factory.Save());
			testItems.JobDeclaration.JE_MessageType = "IMP";
			var invoiceLine = testItems.InvoiceLine;
			var pq1 = invoiceLine.CIQProductQualifications.AddNew();
			pq1.CSI_Code = "203";
			using (var form = new JobDeclarationForm(testItems.JobDeclaration))
			{
				form.Show();
				var invoiceLineUserControl = form.FindInvoiceLineUserControl();
				invoiceLineUserControl.CustomsInvoiceLinesBoundGrid.Select(0);
				invoiceLineUserControl.LineDetailTabControl.SelectedIndex = 4;
				var productQuantUserControl = (CIQProductQuantificationsUserControl)invoiceLineUserControl.LineDetailTabControl.Controls.Find("CIQProductQuantificationsUserControl", true).First();
				var vinGroup = (ZGroupBox)productQuantUserControl.Controls.Find("VINGroupBox", true)[0];
				productQuantUserControl.QuantificationGrid.Select(0);
				Assert("VIN Group should always be visible!", vinGroup.Visible);
				var pq2 = invoiceLine.CIQProductQualifications.AddNew();
				pq2.CSI_Code = "408";
				productQuantUserControl.QuantificationGrid.ListManager.Position = 1;
				productQuantUserControl.QuantificationGrid.Select(1);
				Assert("VIN Group should always be visible!", vinGroup.Visible);
				pq2.CSI_Code = "325";
				Assert("VIN Group should always be visible!", vinGroup.Visible);
				pq2.CSI_Code = "409";
				Assert("VIN Group should always be visible!", vinGroup.Visible);
				productQuantUserControl.QuantificationGrid.ListManager.Position = 0;
				productQuantUserControl.QuantificationGrid.Select(0);
				Assert("VIN Group should always be visible!", vinGroup.Visible);
			}

			testItems.JobDeclaration.JE_MessageType = "EXP";
			using (var form = new JobDeclarationForm(testItems.JobDeclaration))
			{
				form.Show();
				var invoiceLineUserControl = form.FindInvoiceLineUserControl();
				invoiceLineUserControl.CustomsInvoiceLinesBoundGrid.Select(0);
				invoiceLineUserControl.LineDetailTabControl.SelectedIndex = 4;
				var productQuantUserControl = (CIQProductQuantificationsUserControl)invoiceLineUserControl.LineDetailTabControl.Controls.Find("CIQProductQuantificationsUserControl", true).First();
				var vinGroup = (ZGroupBox)productQuantUserControl.Controls.Find("VINGroupBox", true)[0];
				Assert("VIN Group should be invisible when EXP", !vinGroup.Visible);
			}
		}
	}
}
