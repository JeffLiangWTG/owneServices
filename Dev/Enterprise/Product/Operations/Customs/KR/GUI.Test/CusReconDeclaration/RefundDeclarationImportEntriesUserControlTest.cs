using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.KR.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.KR.GUI.Testing
{
	[TestedType(typeof(RefundDeclarationImportEntriesUserControl))]
	sealed class RefundDeclarationImportEntriesUserControlTest : TestCaseWithFactory
	{
		public void TestUserControls()
		{
			var reconDeclaration = Factory.New<CusReconDeclaration>();
			using var form = new RefundDeclarationForm(reconDeclaration);
			form.Show();

			using var control = form.FindSingle<RefundDeclarationImportEntriesUserControl>("RefundDeclarationImportEntriesUserControl");
			var grid = control.FindSingle<ZGrid>("ImportEntriesGrid");
			AssertNotNull(grid);

			var tabControl = control.FindSingle<ZTabControl>("ImportEntriesTabControl");
			AssertNotNull(tabControl);
			var refundTab = tabControl.FindSingle<ZTabPage>("RefundDetailsTabPage");
			AssertNotNull(refundTab);
			var refundPanel = refundTab.FindSingle<DynamicLayoutPanel>("DetailsPanel");
			AssertNotNull(refundPanel);
			var invoiceLinesTab = tabControl.FindSingle<ZTabPage>("InvoiceLinesTabPage");
			AssertNotNull(invoiceLinesTab);
		}

		public void TestRefundDetailsTab()
		{
			var reconDeclaration = Factory.New<CusReconDeclaration>();
			using (var reconDeclarationForm = new RefundDeclarationForm(reconDeclaration))
			{
				reconDeclarationForm.Show();
				var tabControl = reconDeclarationForm.FindSingle<ZTemplateTabControl>("MainTabControl");
				var tabPage = tabControl.FindSingle<ZTabPage>("ImportEntriesTabPage");
				tabControl.SelectedTab = tabPage;
				var grid = tabPage.FindSingle<ZGrid>("ImportEntriesGrid");

				var index = 0;
				AssertEquals(((ZCalcEditColumnStyleInfo)grid.ColumnStyles[index++]).ColumnName, "Header+" + CusReconEntry.Schema.CRE_SequenceNumber);

				var originalEntryNumberColumnStyleInfo = (ZCodeFindBoxColumnStyleInfo)grid.ColumnStyles[index];
				AssertEquals(originalEntryNumberColumnStyleInfo.ColumnName, "Header+" + CusReconEntry.Schema.CRE_OriginalEntryNumber);
				AssertEquals(originalEntryNumberColumnStyleInfo.ModuleID, ModuleIDs.Customs.KR.ImportEntryDetails);
				index++;

				var originalEntryLineNumberColumnStyleInfo = (ZCodeFindBoxColumnStyleInfo)grid.ColumnStyles[index];
				AssertEquals(originalEntryLineNumberColumnStyleInfo.ColumnName, nameof(CusReconEntryLine.FormattedOriginalEntryLineNumber));
				AssertEquals(originalEntryLineNumberColumnStyleInfo.ModuleID, ModuleIDs.Customs.KR.EntryLineDetailsFor5UL);
				index++;

				AssertEquals(((ZCodeFindBoxColumnStyleInfo)grid.ColumnStyles[index++]).ColumnName, "Header+" + nameof(CusReconEntry.CRE_CustomsBillNumber));
				AssertEquals(((ZDropEditColumnStyleInfo)grid.ColumnStyles[index++]).ColumnName, "Header+" + nameof(CusReconEntry.Amendment5WNVersionNumber));
				AssertEquals(((ZCalcEditColumnStyleInfo)grid.ColumnStyles[index++]).ColumnName, nameof(CusReconEntryLine.DutyToRefund));
				AssertEquals(((ZCalcEditColumnStyleInfo)grid.ColumnStyles[index++]).ColumnName, nameof(CusReconEntryLine.DutyPenaltyToRefund));
				AssertEquals(((ZCalcEditColumnStyleInfo)grid.ColumnStyles[index++]).ColumnName, nameof(CusReconEntryLine.SCTToRefund));
				AssertEquals(((ZCalcEditColumnStyleInfo)grid.ColumnStyles[index++]).ColumnName, nameof(CusReconEntryLine.SCTPenaltyToRefund));
				AssertEquals(((ZCalcEditColumnStyleInfo)grid.ColumnStyles[index++]).ColumnName, nameof(CusReconEntryLine.TRTToRefund));
				AssertEquals(((ZCalcEditColumnStyleInfo)grid.ColumnStyles[index++]).ColumnName, nameof(CusReconEntryLine.TRTPenaltyToRefund));
				AssertEquals(((ZCalcEditColumnStyleInfo)grid.ColumnStyles[index++]).ColumnName, nameof(CusReconEntryLine.LQTToRefund));
				AssertEquals(((ZCalcEditColumnStyleInfo)grid.ColumnStyles[index++]).ColumnName, nameof(CusReconEntryLine.LQTPenaltyToRefund));
				AssertEquals(((ZCalcEditColumnStyleInfo)grid.ColumnStyles[index++]).ColumnName, nameof(CusReconEntryLine.EDTToRefund));
				AssertEquals(((ZCalcEditColumnStyleInfo)grid.ColumnStyles[index++]).ColumnName, nameof(CusReconEntryLine.EDTPenaltyToRefund));
				AssertEquals(((ZCalcEditColumnStyleInfo)grid.ColumnStyles[index++]).ColumnName, nameof(CusReconEntryLine.AGTToRefund));
				AssertEquals(((ZCalcEditColumnStyleInfo)grid.ColumnStyles[index++]).ColumnName, nameof(CusReconEntryLine.AGTPenaltyToRefund));
				AssertEquals(((ZCalcEditColumnStyleInfo)grid.ColumnStyles[index++]).ColumnName, nameof(CusReconEntryLine.VATToRefund));
				AssertEquals(((ZCalcEditColumnStyleInfo)grid.ColumnStyles[index++]).ColumnName, nameof(CusReconEntryLine.VATPenaltyToRefund));
				AssertEquals(((ZCalcEditColumnStyleInfo)grid.ColumnStyles[index++]).ColumnName, nameof(CusReconEntryLine.ValueForVAT));
				AssertEquals(((ZCalcEditColumnStyleInfo)grid.ColumnStyles[index++]).ColumnName, nameof(CusReconEntryLine.VATExemptionValue));
				AssertEquals(((ZCalcEditColumnStyleInfo)grid.ColumnStyles[index++]).ColumnName, nameof(CusReconEntryLine.PenaltyLateDecToRefund));
				AssertEquals(((ZCalcEditColumnStyleInfo)grid.ColumnStyles[index++]).ColumnName, nameof(CusReconEntryLine.PenaltyMissedDecToRefund));
				AssertEquals(((ZCalcEditColumnStyleInfo)grid.ColumnStyles[index++]).ColumnName, nameof(CusReconEntryLine.PenaltyLatePaymentToRefund));
				AssertEquals(((ZCalcEditColumnStyleInfo)grid.ColumnStyles[index++]).ColumnName, nameof(CusReconEntryLine.NonDutyTaxRevenueToRefund));
				AssertEquals(((ZCalcEditColumnStyleInfo)grid.ColumnStyles[index++]).ColumnName, nameof(CusReconEntryLine.TotalLateRefundAmount));
				AssertColumns(true, ((ZDropEditColumnStyleInfo)grid.ColumnStyles[index++]).ColumnName, "ContractRevocation+" + nameof(ContractRevocation5UL.CSI_Code));
				AssertColumns(false, ((ZTextBoxColumnStyleInfo)grid.ColumnStyles[index++]).ColumnName, "ContractRevocation+" + nameof(ContractRevocation5UL.DisposalNumber));
				AssertColumns(false, ((ZTextBoxColumnStyleInfo)grid.ColumnStyles[index++]).ColumnName, "ContractRevocation+" + nameof(ContractRevocation5UL.ExportEntryNumber));
				AssertColumns(false, ((ZTextBoxColumnStyleInfo)grid.ColumnStyles[index++]).ColumnName, "ContractRevocation+" + nameof(ContractRevocation5UL.FormattedLineNo));
				AssertColumns(false, ((ZDateEditColumnStyleInfo)grid.ColumnStyles[index++]).ColumnName, "ContractRevocation+" + nameof(ContractRevocation5UL.CSI_DateOfExpiry));
				AssertColumns(false, ((ZMultiLineTextBoxColumnInfo)grid.ColumnStyles[index++]).ColumnName, "ContractRevocation+" + nameof(ContractRevocation5UL.CSI_AdditionalDescription));
				AssertColumns(false, ((ZMultiLineTextBoxColumnInfo)grid.ColumnStyles[index++]).ColumnName, "ContractRevocation+" + nameof(ContractRevocation5UL.CSI_ReferenceNumber2));
				AssertColumns(false, ((ZMultiLineTextBoxColumnInfo)grid.ColumnStyles[index++]).ColumnName, "ContractRevocation+" + nameof(ContractRevocation5UL.CSI_Description));

				void AssertColumns(bool isVisible, string columnName, string propertyName)
				{
					var column = grid.Columns.First(x => x.ColumnName == columnName);
					AssertNotNull(column);
					AssertEquals(columnName, propertyName);
					AssertEquals(isVisible, grid.GetColumnStyle(propertyName).IsVisible);
				}
			}
		}

		public void TestGridIsCanceled()
		{
			var reconDeclaration = Factory.New<CusReconDeclaration>();
			using (var reconDeclarationForm = new RefundDeclarationForm(reconDeclaration))
			{
				reconDeclarationForm.Show();
				var tabControl = reconDeclarationForm.FindSingle<ZTemplateTabControl>("MainTabControl");
				var tabPage = tabControl.FindSingle<ZTabPage>("ImportEntriesTabPage");
				tabControl.SelectedTab = tabPage;
				var grid = tabPage.FindSingle<ZGrid>("ImportEntriesGrid");

				grid.PerformMouseDownForTest(0, 1);
				AssertEquals("CusReconEntry is auto generated.", 1, reconDeclaration.CusReconEntries.Count);
				AssertEquals("CusReconEntryLine is auto generated.", 1, reconDeclaration.CusReconEntryLines.Count);
				AssertEquals(reconDeclaration.CusReconEntries[0].PK, reconDeclaration.CusReconEntryLines[0].Header.PK);

				tabControl.SelectedIndex = 0;
				AssertEquals("CusReconEntry is deleted.", 0, reconDeclaration.CusReconEntries.Count);
				AssertEquals("CusReconEntryLine is deleted.", 0, reconDeclaration.CusReconEntryLines.Count);
			}
		}

		public void TestRefundDeclarationImportEntriesUserControl()
		{
			using (var control = new RefundDeclarationImportEntriesUserControl())
			{
				control.Show();

				var grid = control.FindSingle<ZGrid>("ImportEntriesGrid");
				AssertNotNull(grid);

				var detailsGroupBox = control.FindSingle<ZGroupBox>("DetailsGroupBox");
				AssertNotNull(detailsGroupBox);
				AssertNotNull(detailsGroupBox.FindSingle<DynamicLayoutPanel>("DetailsPanel"));

				var refundForCancelGroupBox = control.FindSingle<ZGroupBox>("RefundForCancelGroupBox");
				AssertNotNull(refundForCancelGroupBox);
				AssertNotNull(refundForCancelGroupBox.FindSingle<DynamicLayoutPanel>("RefundForCancelPanel"));

				var paidAndRefundGroupBox = control.FindSingle<ZGroupBox>("PaidAndRefundGroupBox");
				AssertNotNull(paidAndRefundGroupBox);
				AssertNotNull(paidAndRefundGroupBox.FindSingle<DynamicLayoutPanel>("PaidAndRefundPanel"));
			}
		}

		public void TestInvoiceLinesGrid()
		{
			using var control = new RefundDeclarationImportEntriesUserControl();
			var invoiceLinesTab = control.FindSingle<ZTabPage>("InvoiceLinesTabPage");
			var grid = invoiceLinesTab.FindSingle<ZGrid>("InvoiceLinesGrid");
			var index = 0;
			AssertEquals(((ZDropEditColumnStyleInfo)grid.ColumnStyles[index++]).ColumnName, nameof(RefundInvoiceLine.FormattedCSI_LineNo));

			AssertEquals(((ZTextBoxColumnStyleInfo)grid.ColumnStyles[index]).ColumnName, RefundInvoiceLine.Schema.CSI_AdditionalDescription);
			Assert(((ZTextBoxColumnStyleInfo)grid.ColumnStyles[index++]).IsReadOnly);

			AssertEquals(((ZTextBoxColumnStyleInfo)grid.ColumnStyles[index]).ColumnName, RefundInvoiceLine.Schema.CSI_Description);
			Assert(((ZTextBoxColumnStyleInfo)grid.ColumnStyles[index++]).IsReadOnly);

			AssertEquals(((ZCalcEditColumnStyleInfo)grid.ColumnStyles[index++]).ColumnName, RefundInvoiceLine.Schema.CSI_Quantity);

			AssertEquals(((ZCalcEditColumnStyleInfo)grid.ColumnStyles[index]).ColumnName, RefundInvoiceLine.Schema.CSI_Quantity2);
			Assert(((ZCalcEditColumnStyleInfo)grid.ColumnStyles[index++]).IsReadOnly);

			AssertEquals(((ZCalcEditColumnStyleInfo)grid.ColumnStyles[index]).ColumnName, RefundInvoiceLine.Schema.CSI_Value);
			Assert(((ZCalcEditColumnStyleInfo)grid.ColumnStyles[index++]).IsReadOnly);
		}

		public void TestIsInvoiceLinesTabVisible()
		{
			var reconDeclaration = Factory.New<CusReconDeclaration>();
			using var form = new RefundDeclarationForm(reconDeclaration);
			form.Show();
			var mainTabControl = form.FindSingle<ZTemplateTabControl>("MainTabControl");
			var tabPage = mainTabControl.FindSingle<ZTabPage>("ImportEntriesTabPage");
			mainTabControl.SelectedTab = tabPage;
			var importEntriesTabControl = tabPage.FindSingle<ZTabControl>("ImportEntriesTabControl");
			var invoiceLinesTab = importEntriesTabControl.AllTabPages.FirstOrDefault(x => x.Name == "InvoiceLinesTabPage");
			var importEntriesGrid = tabPage.FindSingle<ZGrid>("ImportEntriesGrid");
			var reconEntryLine = reconDeclaration.CusReconEntryLines.AddNew();
			reconEntryLine.CRL_OriginalEntryLineNumber = 0;
			importEntriesGrid.CurrentRowIndex = 0;
			Assert(!((ZTabPage)invoiceLinesTab).TabVisible);

			reconEntryLine = reconDeclaration.CusReconEntryLines.AddNew();
			reconEntryLine.CRL_OriginalEntryLineNumber = 1;
			importEntriesGrid.CurrentRowIndex = 1;
			Assert(((ZTabPage)invoiceLinesTab).TabVisible);
		}
	}
}
