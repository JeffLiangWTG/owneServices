using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.Common.Shared;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.Declaration.GUI.Testing
{
	sealed class AUCMRMessageUserControlTest : TestCaseWithFactory
	{
		public void TestEntryHeaderInfoControls()
		{
			var testDec = Factory.New<JobDeclaration>();
			testDec.JE_MessageType = JobMessageTypeList.Codes.Import;
			using (var form = new ZForm(testDec))
			using (var userControl = new AUCMRMessageUserControl())
			{
				form.Controls.Add(userControl);
				form.Show();
				CombineAssertions(() =>
				{
					AssertEquals("Label text when is not Export", "Message Status :", userControl.MessageStatusLabel.Text);
					AssertEquals("Visible when is not Export", true, userControl.EntryAdviceLabel.Visible);
					AssertEquals("Visible when is not Export", true, userControl.EntryAdviceTextBox.Visible);
					AssertEquals("Visible when is not Export", true, userControl.ATDCodeLabel.Visible);
					AssertEquals("Visible when is not Export", true, userControl.ATDCodeTextBox.Visible);
					AssertEquals("Visible when is not Export", true, userControl.CustomsPaymentLabel.Visible);
					AssertEquals("Visible when is not Export", true, userControl.CustomsPaymentTextBox.Visible);
				});

				testDec.JE_MessageType = JobMessageTypeList.Codes.Export;
				CombineAssertions(() =>
				{
					AssertEquals("Label text when is Export", "Entry Status :", userControl.MessageStatusLabel.Text);
					AssertEquals("Not visible when is Export", false, userControl.EntryAdviceLabel.Visible);
					AssertEquals("Not visible when is Export", false, userControl.EntryAdviceTextBox.Visible);
					AssertEquals("Not visible when is Export", false, userControl.ATDCodeLabel.Visible);
					AssertEquals("Not visible when is Export", false, userControl.ATDCodeTextBox.Visible);
					AssertEquals("Not visible when is Export", false, userControl.CustomsPaymentLabel.Visible);
					AssertEquals("Not visible when is Export", false, userControl.CustomsPaymentTextBox.Visible);
				});
			}
		}

		public void TestRemoveEdificeColumnsFromGrid()
		{
			var testDec = Factory.New<JobDeclaration>();
			using (var form = new ZForm())
			using (var userControl = new AUCMRMessageUserControl())
			{
				form.Controls.Add(userControl);
				userControl.SetDataBinding(testDec, "");
				form.Show();
				CombineAssertions(() =>
				{
					AssertEquals("User control should not have EntryFee column", null, userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.EntryFee]);
					AssertEquals("User control should not have MessageFee column", null, userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.MessageFee]);
					AssertEquals("User control should not have ScreenFreeCharge column", null, userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.ScreenFreeCharge]);
					AssertEquals("User control should not have TradegateGST column", null, userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.TradegateGST]);
				});
			}
		}

		public void TestWarehouseNumberOfPacksColumn()
		{
			var testDec = Factory.New<JobDeclaration>();
			testDec.JE_MessageType = "EXW";
			using (var form = new ZForm())
			using (var userControl = new AUCMRMessageUserControl())
			{
				form.Controls.Add(userControl);
				userControl.SetDataBinding(testDec, "");
				form.Show();
				AssertNotNull("User control should have WarehouseNumberOfPacks column", userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.WarehouseNumberOfPacks]);
				testDec.JE_MessageType = "IMP";
				AssertNull("User control should not have WarehouseNumberOfPacks column", userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.WarehouseNumberOfPacks]);
			}
		}

		public void TestSetupEntryHeaderColumns()
		{
			var testDec = Factory.New<JobDeclaration>();
			testDec.JE_MessageType = JobMessageTypeList.Codes.Import;
			using (var form = new ZForm(testDec))
			using (var userControl = new AUCMRMessageUserControl())
			{
				form.Controls.Add(userControl);
				form.Show();
				CombineAssertions(() =>
				{
					AssertNotNull("User control should have AQISServicePaymentAmount column", userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.AQISServicePaymentAmount]);
					var refundReasonCodeColumn = userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.RefundReasonCode];
					AssertNotNull("User control should have RefundReasonCode column", refundReasonCodeColumn);
					AssertEquals("Refund Group", "F3D765EA-8D5E-47EC-8F20-0CB0831BD71B", refundReasonCodeColumn.GroupName.Key);
					var totalPaidColumn = userControl.EntriesBoundGrid.Columns[Customs.Business.AutoCusEntryHeader.Schema.CH_TotalPaid];
					AssertNotNull("User control should have CH_TotalPaid column", totalPaidColumn);
					AssertEquals("Refund Group", "F3D765EA-8D5E-47EC-8F20-0CB0831BD71B", totalPaidColumn.GroupName.Key);
					AssertNotNull("User control should have WoodLevyIncludingWHEstimate column", userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.WoodLevyIncludingWHEstimate]);
					AssertNotNull("User control should have TAndI column", userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.TAndI]);
					AssertNotNull("User control should have DutyAmountIncludingWHEstimate column", userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.DutyAmountIncludingWHEstimate]);
					AssertNotNull("User control should have DutyAmountIncludingWHEstimate column", userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.GSTAmountIncludingWHEstimate]);
					AssertNotNull("User control should have LCTAmount column", userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.LCTAmount]);
					AssertNotNull("User control should have WETAmountIncludingWHEstimate column", userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.WETAmountIncludingWHEstimate]);
					AssertNotNull("User control should have CustomsFactor column", userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.CustomsFactor]);
					AssertNotNull("User control should have ImportEntryAdvice column", userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.ImportEntryAdvice]);
					AssertNotNull("User control should have AQISContainerCharges column", userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.AQISContainerCharges]);
					AssertNotNull("User control should have AQISProcessingCharge column", userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.AQISProcessingCharge]);
					AssertNotNull("User control should have DeclarationProcessingCharge column", userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.DeclarationProcessingCharge]);
					AssertNotNull("User control should have TotalPayableAdmin column", userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.TotalPayableAdmin]);
					AssertNotNull("User control should have OtherEntryCharges column", userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.OtherEntryCharge]);
					AssertNotNull("User control should have TotalSecurityConcession column", userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.TotalSecurityConcession]);
					AssertNotNull("User control should have TotalSecurityLiability column", userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.TotalSecurityLiability]);
				});
			}
		}

		public void TestEntryHeaderColumns_ExportEntryLineMerge()
		{
			var testDec = Factory.New<JobDeclaration>();
			testDec.JE_MessageType = JobMessageTypeList.Codes.Export;
			using (var form = new ZForm(testDec))
			using (var userControl = new AUCMRMessageUserControl())
			{
				form.Controls.Add(userControl);
				form.Show();
				CombineAssertions(() =>
				{
					var entriesGrid = userControl.EntriesBoundGrid;

					AssertNotNull("User control should have EntryNumber column", entriesGrid.Columns["EntryNumber"]);
					AssertNotNull("User control should have CH_BGMReference column", entriesGrid.Columns["CH_BGMReference"]);
					AssertNotNull("User control should have CH_MessageType column", entriesGrid.Columns["CH_MessageType"]);
					AssertNotNull("User control should have CH_MessageTypeDescription column", entriesGrid.Columns["CH_MessageTypeDescription"]);

					AssertNull("User control should not have AQISServicePaymentAmount column", entriesGrid.Columns[CusEntryHeader.Schema.AQISServicePaymentAmount]);
					AssertNull("User control should not have RefundReasonCode column", entriesGrid.Columns[CusEntryHeader.Schema.RefundReasonCode]);
					AssertNull("User control should not have CH_TotalPaid column", entriesGrid.Columns[Customs.Business.AutoCusEntryHeader.Schema.CH_TotalPaid]);

					AssertNull("User control should not have WoodLevyIncludingWHEstimate column", entriesGrid.Columns[CusEntryHeader.Schema.WoodLevyIncludingWHEstimate]);
					AssertNull("User control should not have TAndI column", entriesGrid.Columns[CusEntryHeader.Schema.TAndI]);
					AssertNull("User control should not have DutyAmountIncludingWHEstimate column", entriesGrid.Columns[CusEntryHeader.Schema.DutyAmountIncludingWHEstimate]);
					AssertNull("User control should not have DutyAmountIncludingWHEstimate column", entriesGrid.Columns[CusEntryHeader.Schema.GSTAmountIncludingWHEstimate]);
					AssertNull("User control should not have LCTAmount column", entriesGrid.Columns[CusEntryHeader.Schema.LCTAmount]);
					AssertNull("User control should not have WETAmountIncludingWHEstimate column", entriesGrid.Columns[CusEntryHeader.Schema.WETAmountIncludingWHEstimate]);
					AssertNull("User control should not have CustomsFactor column", entriesGrid.Columns[CusEntryHeader.Schema.CustomsFactor]);
					AssertNull("User control should not have ImportEntryAdvice column", entriesGrid.Columns[CusEntryHeader.Schema.ImportEntryAdvice]);
					AssertNull("User control should not have AQISContainerCharges column", entriesGrid.Columns[CusEntryHeader.Schema.AQISContainerCharges]);
					AssertNull("User control should not have AQISProcessingCharge column", entriesGrid.Columns[CusEntryHeader.Schema.AQISProcessingCharge]);
					AssertNull("User control should not have DeclarationProcessingCharge column", entriesGrid.Columns[CusEntryHeader.Schema.DeclarationProcessingCharge]);
					AssertNull("User control should not have TotalPayableAdmin column", entriesGrid.Columns[CusEntryHeader.Schema.TotalPayableAdmin]);
					AssertNull("User control should not have OtherEntryCharges column", entriesGrid.Columns[CusEntryHeader.Schema.OtherEntryCharge]);
					AssertNull("User control should not have TotalSecurityConcession column", entriesGrid.Columns[CusEntryHeader.Schema.TotalSecurityConcession]);
					AssertNull("User control should not have TotalSecurityLiability column", entriesGrid.Columns[CusEntryHeader.Schema.TotalSecurityLiability]);

					AssertNull("User control should not have WarehouseNumberOfPacks column", entriesGrid.Columns[CusEntryHeader.Schema.WarehouseNumberOfPacks]);
				});
			}

			testDec.JE_MessageType = JobMessageTypeList.Codes.ExWarehouse;
			using (var form = new ZForm(testDec))
			using (var userControl = new AUCMRMessageUserControl())
			{
				form.Controls.Add(userControl);
				form.Show();
				CombineAssertions(() =>
				{
					var entriesGrid = userControl.EntriesBoundGrid;

					AssertNotNull("User control should have EntryNumber column", entriesGrid.Columns["EntryNumber"]);
					AssertNotNull("User control should have CH_BGMReference column", entriesGrid.Columns["CH_BGMReference"]);
					AssertNotNull("User control should have CH_MessageType column", entriesGrid.Columns["CH_MessageType"]);
					AssertNotNull("User control should have CH_MessageTypeDescription column", entriesGrid.Columns["CH_MessageTypeDescription"]);

					AssertNotNull("User control should have AQISServicePaymentAmount column", entriesGrid.Columns[CusEntryHeader.Schema.AQISServicePaymentAmount]);
					var refundReasonCodeColumn = entriesGrid.Columns[CusEntryHeader.Schema.RefundReasonCode];
					AssertNotNull("User control should have RefundReasonCode column", refundReasonCodeColumn);
					AssertEquals("Refund Group", "F3D765EA-8D5E-47EC-8F20-0CB0831BD71B", refundReasonCodeColumn.GroupName.Key);
					var totalPaidColumn = entriesGrid.Columns[Customs.Business.AutoCusEntryHeader.Schema.CH_TotalPaid];
					AssertNotNull("User control should have CH_TotalPaid column", totalPaidColumn);
					AssertEquals("Refund Group", "F3D765EA-8D5E-47EC-8F20-0CB0831BD71B", totalPaidColumn.GroupName.Key);

					AssertNotNull("User control should have WoodLevyIncludingWHEstimate column", entriesGrid.Columns[CusEntryHeader.Schema.WoodLevyIncludingWHEstimate]);
					AssertNotNull("User control should have TAndI column", entriesGrid.Columns[CusEntryHeader.Schema.TAndI]);
					AssertNotNull("User control should have DutyAmountIncludingWHEstimate column", entriesGrid.Columns[CusEntryHeader.Schema.DutyAmountIncludingWHEstimate]);
					AssertNotNull("User control should have DutyAmountIncludingWHEstimate column", entriesGrid.Columns[CusEntryHeader.Schema.GSTAmountIncludingWHEstimate]);
					AssertNotNull("User control should have LCTAmount column", entriesGrid.Columns[CusEntryHeader.Schema.LCTAmount]);
					AssertNotNull("User control should have WETAmountIncludingWHEstimate column", entriesGrid.Columns[CusEntryHeader.Schema.WETAmountIncludingWHEstimate]);
					AssertNotNull("User control should have CustomsFactor column", entriesGrid.Columns[CusEntryHeader.Schema.CustomsFactor]);
					AssertNotNull("User control should have ImportEntryAdvice column", entriesGrid.Columns[CusEntryHeader.Schema.ImportEntryAdvice]);
					AssertNotNull("User control should have AQISContainerCharges column", entriesGrid.Columns[CusEntryHeader.Schema.AQISContainerCharges]);
					AssertNotNull("User control should have AQISProcessingCharge column", entriesGrid.Columns[CusEntryHeader.Schema.AQISProcessingCharge]);
					AssertNotNull("User control should have DeclarationProcessingCharge column", entriesGrid.Columns[CusEntryHeader.Schema.DeclarationProcessingCharge]);
					AssertNotNull("User control should have TotalPayableAdmin column", entriesGrid.Columns[CusEntryHeader.Schema.TotalPayableAdmin]);
					AssertNotNull("User control should have OtherEntryCharges column", entriesGrid.Columns[CusEntryHeader.Schema.OtherEntryCharge]);
					AssertNotNull("User control should have TotalSecurityConcession column", entriesGrid.Columns[CusEntryHeader.Schema.TotalSecurityConcession]);
					AssertNotNull("User control should have TotalSecurityLiability column", entriesGrid.Columns[CusEntryHeader.Schema.TotalSecurityLiability]);

					AssertNotNull("User control should have WarehouseNumberOfPacks column", entriesGrid.Columns[CusEntryHeader.Schema.WarehouseNumberOfPacks]);
				});
			}
		}

		public void TestEntryLineGridColums()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "IMP";
			using (var userControl = new AUCMRMessageUserControl())
			{
				userControl.SetDataBinding(declaration, "");
				var entryLinesGrid = userControl.FindSingle<ZGrid>("EntryLineGrid");
				AssertEquals("EntryLineGrid column for import should have RefundReasonCode column", true, entryLinesGrid.GetColumnStyle("RefundReasonCode").IsVisible);
				AssertEquals("EntryLineGrid column for import should have TotalDutyTaxAdvisedInLastClearanceMessage column", true, entryLinesGrid.GetColumnStyle("TotalDutyTaxAdvisedInLastClearanceMessage").IsVisible);
				AssertEquals("EntryLineGrid column for import should have CurrentTotalDutyTax column", true, entryLinesGrid.GetColumnStyle("CurrentTotalDutyTax").IsVisible);
				AssertEquals("EntryLineGrid column for import should have CL_DutyPercent column", true, entryLinesGrid.GetColumnStyle("CL_DutyPercent").IsVisible);
				AssertEquals("EntryLineGrid column for import should have FlatRateDescription column", true, entryLinesGrid.GetColumnStyle("FlatRateDescription").IsVisible);
				AssertEquals("EntryLineGrid column for import should have IsNonAQISAEPLine column", true, entryLinesGrid.GetColumnStyle("IsNonAQISAEPLine").IsVisible);
				AssertEquals("EntryLineGrid column for import should have SecurityConcessionAmount column", true, entryLinesGrid.GetColumnStyle("SecurityConcessionAmount").IsVisible);
				AssertEquals("EntryLineGrid column for import should have SecurityLiabilityAmount column", true, entryLinesGrid.GetColumnStyle("SecurityLiabilityAmount").IsVisible);
				AssertEquals("EntryLineGrid column for import shouldn't have AggregateEntryLineNumber column", false, entryLinesGrid.GetColumnStyle("ZA_AggregateEntryLineNumber").IsVisible);
			}

			declaration.JE_MessageType = "EXP";
			using (var userControl = new AUCMRMessageUserControl())
			{
				userControl.SetDataBinding(declaration, "");
				var entryLinesGrid = userControl.FindSingle<ZGrid>("EntryLineGrid");

				AssertEquals("EntryLineGrid column for export shouldn't have RefundReasonCode column", false, entryLinesGrid.GetColumnStyle("RefundReasonCode").IsVisible);
				AssertEquals("EntryLineGrid column for export shouldn't have TotalDutyTaxAdvisedInLastClearanceMessage column", false, entryLinesGrid.GetColumnStyle("TotalDutyTaxAdvisedInLastClearanceMessage").IsVisible);
				AssertEquals("EntryLineGrid column for export shouldn't have CurrentTotalDutyTax column", false, entryLinesGrid.GetColumnStyle("CurrentTotalDutyTax").IsVisible);
				AssertEquals("EntryLineGrid column for export shouldn't have CL_DutyPercent column", false, entryLinesGrid.GetColumnStyle("CL_DutyPercent").IsVisible);
				AssertEquals("EntryLineGrid column for export shouldn't have FlatRateDescription column", false, entryLinesGrid.GetColumnStyle("FlatRateDescription").IsVisible);
				AssertEquals("EntryLineGrid column for export shouldn't have IsNonAQISAEPLine column", false, entryLinesGrid.GetColumnStyle("IsNonAQISAEPLine").IsVisible);
				AssertEquals("EntryLineGrid column for export shouldn't have SecurityConcessionAmount column", false, entryLinesGrid.GetColumnStyle("SecurityConcessionAmount").IsVisible);
				AssertEquals("EntryLineGrid column for export shouldn't have SecurityLiabilityAmount column", false, entryLinesGrid.GetColumnStyle("SecurityLiabilityAmount").IsVisible);
				AssertEquals("EntryLineGrid column for export shouldn't have AggregateEntryLineNumber column", false, entryLinesGrid.GetColumnStyle("ZA_AggregateEntryLineNumber").IsVisible);
			}

			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_MessageType = "IMP";
			declaration2.JE_EntryStatus = ConsolidatedEntryStatusList.Codes.AppliedToConsolidation;
			AssertEquals("Precondition: declaration is consolidated", true, ConsolidatedDeclaration.IsConsolidated(declaration2));
			using (var userControl = new AUCMRMessageUserControl())
			{
				userControl.SetDataBinding(declaration2, "");
				var entryLinesGrid = userControl.FindSingle<ZGrid>("EntryLineGrid");
				AssertEquals("EntryLineGrid column for import should have AggregateEntryLineNumber column", true, entryLinesGrid.GetColumnStyle("ZA_AggregateEntryLineNumber").IsVisible);
			}
		}
	}
}
