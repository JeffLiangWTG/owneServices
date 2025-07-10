using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core.Forms;
using Enterprise.Customs.CH.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CH.GUI.Testing;

public class PermitsUserControlTest : TestCaseWithFactory
{
	public void TestPermitGridLayout()
	{
		using (var form = new JobDeclarationForm(declaration))
		using (var parent = InvoiceUserControl)
		{
			form.Controls.Add(parent);
			form.Show();
			parent.LineDetailTabControl.SelectedTab = parent.PermitsTabPage;
			var permitsGridColumsStyles = parent.PermitsUserControl.PermitsGrid.ColumnStyles.Cast<ZGridColumnInfo>().ToArray();

			CombineAssertions(() =>
			{
				UserControlTestHelper.AssertColumnStyles(permitsGridColumsStyles, Permit.Schema.CSI_Code, 0);
				UserControlTestHelper.AssertColumnStyles(permitsGridColumsStyles, Permit.Schema.CSI_IssuerType, 1);
				UserControlTestHelper.AssertColumnStyles(permitsGridColumsStyles, Permit.Schema.CSI_ReferenceNumber, 2);
				UserControlTestHelper.AssertColumnStyles(permitsGridColumsStyles, Permit.Schema.CSI_DateOfIssue, 3);
				UserControlTestHelper.AssertColumnStyles(permitsGridColumsStyles, Permit.Schema.CSI_Description, 4);
			});
		}
	}

	public void TestFieldsVisibility()
	{
		using (var form = new JobDeclarationForm(declaration))
		using (var parent = InvoiceUserControl)
		{
			form.Controls.Add(parent);
			form.Show();
			var control = parent.PermitsUserControl;
			parent.LineDetailTabControl.SelectedTab = parent.PermitsTabPage;
			CombineAssertions(() =>
			{
				AssertEquals($"IssuerTypeFindBox.Visible", true, control.IssuerTypeFindBox.Visible);
				AssertEquals($"PermitCodeFindBox.Visible", true, control.PermitCodeFindBox.Visible);
				AssertEquals($"DateOfIssueDateEdit.Visible", true, control.DateOfIssueDateEdit.Visible);
				AssertEquals($"ReferenceNumberTextBox.Visible", true, control.ReferenceNumberTextBox.Visible);
				AssertEquals($"DescriptionTextBox.Visible", true, control.DescriptionTextBox.Visible);
				AssertEquals($"PermitItemDetailsGrid.Visible", true, control.PermitItemDetailsGrid.Visible);
			});
		}
	}

	public void TestPermitDetailsVisibility()
	{
		using (var form = new JobDeclarationForm(declaration))
		using (var parent = InvoiceUserControl)
		{
			form.Controls.Add(parent);
			form.Show();
			parent.LineDetailTabControl.SelectedTab = parent.PermitsTabPage;

			CombineAssertions(() =>
			{
				var permitItemDetailsGroupBox = parent.PermitsUserControl.PermitItemDetailsGroupBox;

				permit.CSI_Code = ZString.Empty;
				AssertEquals($"when {nameof(permit.CSI_Code)}={permit.CSI_Code} PermitItemDetailsGroupBox.Visible", false, permitItemDetailsGroupBox.Visible);

				permit.CSI_Code = "11";
				AssertEquals($"when {nameof(permit.CSI_Code)}={permit.CSI_Code} PermitItemDetailsGroupBox.Visible", true, permitItemDetailsGroupBox.Visible);

				permit.CSI_Code = "1";
				AssertEquals($"when {nameof(permit.CSI_Code)}={permit.CSI_Code} PermitItemDetailsGroupBox.Visible", false, permitItemDetailsGroupBox.Visible);

				permit.CSI_Code = "12";
				AssertEquals($"when {nameof(permit.CSI_Code)}={permit.CSI_Code} PermitItemDetailsGroupBox.Visible", true, permitItemDetailsGroupBox.Visible);
			});
		}
	}

	public void TestPermitDetailsColumns()
	{
		using (var form = new JobDeclarationForm(declaration))
		using (var parent = InvoiceUserControl)
		{
			form.Controls.Add(parent);
			form.Show();
			parent.LineDetailTabControl.SelectedTab = parent.PermitsTabPage;

			var permitItemDetailsGrid = parent.PermitsUserControl.PermitItemDetailsGrid;
			var permitItemDetailsGridColumsStyles = permitItemDetailsGrid.ColumnStyles.Cast<ZGridColumnInfo>().ToArray();

			CombineAssertions(() =>
			{
				UserControlTestHelper.AssertColumnStyles(permitItemDetailsGridColumsStyles, PermitItemDetail.Schema.CY_Code, 0);
				UserControlTestHelper.AssertColumnStyles(permitItemDetailsGridColumsStyles, PermitItemDetail.Schema.CY_Data, 1);

				var dataColumnStyle = permitItemDetailsGrid.GetColumnStyle(PermitItemDetail.Schema.CY_Data) as ZMultiControlColumnStyleInfo;
				AssertEquals(nameof(PermitItemDetail.CY_DataDecimalPlaces), dataColumnStyle.BindToDecimalPlaces);
			});
		}
	}

	BaseInvoiceLineUserControl InvoiceUserControl => declaration.IsImport ? new ImportInvoiceLineUserControl() : (declaration.IsExportOrExportDeclarationActivation ? new ExportInvoiceLineUserControl() : new BaseInvoiceLineUserControl());

	protected override void SetUp()
	{
		base.SetUp();
		declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = MessageTypeCodeList.Codes.Import;
		var invoice = declaration.Invoices.AddNew();
		invoiceLine = invoice.InvoiceLines.AddNew();
		permit = invoiceLine.Permits.AddNew();
		permit.CSI_Code = UniversalReferenceConstants.PermitCodes.SingleEPermit;
	}
	JobDeclaration declaration;
	JobComInvoiceLine invoiceLine;
	Permit permit;
}
