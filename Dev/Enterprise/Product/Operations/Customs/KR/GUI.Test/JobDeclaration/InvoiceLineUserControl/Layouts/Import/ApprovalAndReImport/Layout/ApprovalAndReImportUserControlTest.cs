using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Forms;
using Enterprise.Customs.Business;
using Enterprise.Customs.KR.Business;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.KR.GUI.Testing
{
	[TestedType(typeof(ApprovalAndReImportUserControl))]
	public class ApprovalAndReImportUserControlTest : TestCaseWithFactory
	{
		public void TestApprovalAndReImportTabPage()
		{
			using (var testForm = new JobDeclarationFormForTest(declaration))
			{
				testForm.Show();

				var brokerageControl = testForm.CustomsBrokerageUserControl as CustomsBrokerageUserControlForTest;
				brokerageControl.MainTabControl.SelectedTab = brokerageControl.InvoiceLinesTabPage;
				using (var control = testForm.FindSingle<ApprovalAndReImportUserControl>("ApprovalAndReImportUserControl"))
				{
					var grid = control.ApprovalDocumentGrid;
					var index = 0;
					AssertEquals(((ZGridColumnInfo)grid.ColumnStyles[index++]).ColumnName, GAApproval.Schema.CSI_LineNo);
					AssertEquals(((ZGridColumnInfo)grid.ColumnStyles[index++]).ColumnName, GAApproval.Schema.CSI_Code);
					AssertEquals(((ZGridColumnInfo)grid.ColumnStyles[index++]).ColumnName, GAApproval.Schema.CSI_Procedure);
					AssertEquals(((ZGridColumnInfo)grid.ColumnStyles[index++]).ColumnName, GAApproval.Schema.CSI_Description);
					AssertEquals(((ZGridColumnInfo)grid.ColumnStyles[index++]).ColumnName, GAApproval.Schema.CSI_ReferenceNumber);
					AssertEquals(((ZGridColumnInfo)grid.ColumnStyles[index++]).ColumnName, GAApproval.Schema.CSI_DateOfIssue);
					AssertEquals(((ZGridColumnInfo)grid.ColumnStyles[index++]).ColumnName, GAApproval.Schema.CSI_SubType);
					AssertEquals(((ZGridColumnInfo)grid.ColumnStyles[index++]).ColumnName, GAApproval.Schema.CSI_ReferenceNumber2);

					Assert(grid.GetColumnStyle(GAApproval.Schema.CSI_LineNo).IsReadOnly);

					grid = control.NonApprovalDocumentGrid;
					index = 0;
					AssertEquals(((ZGridColumnInfo)grid.ColumnStyles[index++]).ColumnName, NonGADetail.Schema.CSI_LineNo);
					AssertEquals(((ZGridColumnInfo)grid.ColumnStyles[index++]).ColumnName, NonGADetail.Schema.CSI_Procedure);
					AssertEquals(((ZGridColumnInfo)grid.ColumnStyles[index++]).ColumnName, NonGADetail.Schema.CSI_Code);
					AssertEquals(((ZGridColumnInfo)grid.ColumnStyles[index++]).ColumnName, nameof(NonGADetail.NonGAReasonType));
					AssertEquals(((ZGridColumnInfo)grid.ColumnStyles[index++]).ColumnName, NonGADetail.Schema.CSI_Description);
					AssertEquals(((ZGridColumnInfo)grid.ColumnStyles[index++]).ColumnName, nameof(NonGADetail.ImportNonGAMandatoryDocument));

					Assert(grid.GetColumnStyle(NonGADetail.Schema.CSI_LineNo).IsReadOnly);
					Assert(grid.GetColumnStyle(nameof(NonGADetail.ImportNonGAMandatoryDocument)).IsReadOnly);

					grid = control.ReImportGrid;
					index = 0;
					AssertEquals(((ZGridColumnInfo)grid.ColumnStyles[index++]).ColumnName, PreviousExpDecLine.Schema.CSI_LineNo);
					AssertEquals(((ZGridColumnInfo)grid.ColumnStyles[index++]).ColumnName, PreviousExpDecLine.Schema.CSI_ReferenceNumber);
					AssertEquals(((ZGridColumnInfo)grid.ColumnStyles[index++]).ColumnName, nameof(PreviousExpDecLine.EntryLineNumber));
					AssertEquals(((ZGridColumnInfo)grid.ColumnStyles[index++]).ColumnName, PreviousExpDecLine.Schema.CSI_ItemNumber);
					AssertEquals(((ZGridColumnInfo)grid.ColumnStyles[index++]).ColumnName, PreviousExpDecLine.Schema.CSI_Quantity);
					AssertEquals(((ZGridColumnInfo)grid.ColumnStyles[index++]).ColumnName, PreviousExpDecLine.Schema.CSI_UnitOfQuantity);

					Assert(grid.GetColumnStyle(PreviousExpDecLine.Schema.CSI_LineNo).IsReadOnly);
				}
			}
		}

		protected override void SetUp()
		{
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			invoice.InvoiceLines.AddNew();
		}
		JobDeclaration declaration;
	}
}
