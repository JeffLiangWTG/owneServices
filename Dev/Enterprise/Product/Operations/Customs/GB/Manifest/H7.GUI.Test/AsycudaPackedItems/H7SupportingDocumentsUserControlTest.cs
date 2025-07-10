using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.H7.GUI.Testing;
using Enterprise.Customs.GB.H7.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GB.H7.GUI.Testing
{
	sealed class H7SupportingDocumentsUserControlTest : TestCaseWithFactory
	{
		public void TestLayout()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var packedItem = bill.PackedItems.AddNew();

			using (var form = new ZForm(packedItem))
			using (var control = new H7SupportingDocumentsUserControl())
			{
				control.SetDataBinding(packedItem, string.Empty);

				form.Controls.Add(control);
				form.Show();

				var grid = control.FindSingle<ZGrid>("SupportingDocumentsGrid");

				EUH7GUITestHelper.AssertGridLayout(grid,
					[
						(CusSupportingInfo.Schema.CSI_Code, typeof(ZCodeFindBoxColumnStyleInfo)),
						(CusSupportingInfo.Schema.CSI_ReferenceNumber, typeof(ZMultiControlColumnStyleInfo)),
						(SupportingDocument.Schema.CSI_Actions, typeof(ZDropEditColumnStyleInfo)),
						(SupportingDocument.Schema.CSI_Availability, typeof(ZDropEditColumnStyleInfo)),
						(CusSupportingInfo.Schema.CSI_SubType, typeof(ZTextBoxColumnStyleInfo)),
						(CusSupportingInfo.Schema.CSI_DateOfIssue, typeof(ZDateEditColumnStyleInfo)),
						(CusSupportingInfo.Schema.CSI_DateOfExpiry, typeof(ZDateEditColumnStyleInfo)),
						(CusSupportingInfo.Schema.CSI_Description, typeof(ZTextBoxColumnStyleInfo)),
						(CusSupportingInfo.Schema.CSI_ReferenceNumber2, typeof(ZTextBoxColumnStyleInfo))
					]);

				var supportingDocumentsGroupBox = control.FindSingle<ZGroupBox>("SupportingDocumentsGroupBox");
				AssertEquals(supportingDocumentsGroupBox.CaptionResourceString.Caption, "Supporting Documents");

				var codeDropEdit = control.FindSingle<ZCodeFindBox>("CSI_CodeCodeFindBox");
				Assert("CodeDropEdit should be visible", codeDropEdit.Visible);

				var referenceTextBox = control.FindSingle<ZTextBox>("CSI_ReferenceNumberTextBox");
				Assert("ReferenceTextBox should be visible", referenceTextBox.Visible);

				var actionsDropEdit = control.FindSingle<ZDropEdit>("CSI_ActionsDropEdit");
				Assert("actionsDropEdit should be visible", actionsDropEdit.Visible);

				var availabilityDropEdit = control.FindSingle<ZDropEdit>("CSI_AvailabilityDropEdit");
				Assert("availabilityDropEdit should be visible", codeDropEdit.Visible);

				var subTypeTextBox = control.FindSingle<ZTextBox>("CSI_SubTypeTextBox");
				Assert("subTypeTextBox should be visible", subTypeTextBox.Visible);
				
				var dateOfIssueDateEdit = control.FindSingle<ZDateEdit>("CSI_DateOfIssueDateEdit");
				Assert("dateOfIssueDateEdit should be visible", dateOfIssueDateEdit.Visible);

				var dateOfExpiryDateEdit = control.FindSingle<ZDateEdit>("CSI_DateOfExpiryDateEdit");
				Assert("dateOfExpiryDateEdit should be visible", dateOfExpiryDateEdit.Visible);

				var descriptionTextBox = control.FindSingle<ZTextBox>("CSI_DescriptionTextBox");
				Assert("descriptionTextBox should be visible", descriptionTextBox.Visible);

				var referenceNumber2TextBox = control.FindSingle<ZTextBox>("CSI_ReferenceNumber2TextBox");
				Assert("referenceNumber2TextBox should be visible", referenceNumber2TextBox.Visible);
			}
		}
	}
}
