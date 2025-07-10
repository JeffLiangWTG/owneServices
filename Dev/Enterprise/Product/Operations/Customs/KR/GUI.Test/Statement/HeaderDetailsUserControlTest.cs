using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.KR.Business;
using Enterprise.Customs.KR.Messaging;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;

namespace Enterprise.Customs.KR.GUI.Testing
{
	sealed class HeaderDetailsUserControlTest : TestCaseWithFactory
	{
		public void TestControls_HeaderDetailsGroupBoxTypeU()
		{
			var statement = Factory.New<CusStatementHeader>();
			statement.B2_StatementType = StatementHeaderTypeList.Codes.Normal;

			using (Statements form = new Statements(statement))
			using (var control = form.FindSingle<HeaderDetailsUserControl>())
			{
				form.Show();

				var payerFindBox = control.FindSingle<ZGuidFindBox>("ImporterGuidFindBox");
				AssertEquals(true, payerFindBox.Focused || payerFindBox.FindAll<System.Windows.Forms.Control>(x => x.Focused).Count() == 1);

				var dynamicHeaderDetailsPanel = control.FindSingle<DynamicLayoutPanel>("DynamicHeaderDetailsPanel");
				DynamicLayoutPanelTest.AssertControlsOrder(dynamicHeaderDetailsPanel,
					nameof(StatementHeaderControlBag.FormattedNumberTextBox),
					nameof(StatementHeaderControlBag.StatusDropEdit),
					nameof(StatementHeaderControlBag.ProcessPortCodeFindBox),
					nameof(StatementHeaderControlBag.StatementTypeDropEdit),
					nameof(StatementHeaderControlBag.ProcessAndDueDateUserControl),
					nameof(StatementHeaderControlBag.PeriodDateUserControl),
					nameof(StatementHeaderControlBag.StatementAmountCalcEdit),
					nameof(StatementHeaderControlBag.RelatedFormattedAccountNumberCodeFindBox),
					nameof(StatementHeaderControlBag.ImporterGuidFindBox),
					nameof(StatementHeaderControlBag.RelatedProcessDateEdit),
					nameof(StatementHeaderControlBag.PayerFromCustomsTextBox));

				var entryGroupBox = control.FindSingle<ZGroupBox>("EntryGroupBox");
				AssertEquals(false, entryGroupBox.Visible);

				var feesGroupBox = control.FindSingle<ZGroupBox>("FeesGroupBox");
				AssertEquals(true, feesGroupBox.Visible);
				AssertEquals("Fees", feesGroupBox.Text);

				var feesGrid = control.FindSingle<ZGrid>("FeesGrid");
				var index = 0;
				AssertEquals(feesGrid.Columns[index++].ColumnName, CusStatementLineCharge.Schema.B4_ChargeType);
				AssertEquals(feesGrid.Columns[index++].ColumnName, nameof(CusStatementLineCharge.ChargeTypeName));
				AssertEquals(feesGrid.Columns[index++].ColumnName, CusStatementLineCharge.Schema.B4_ChargeAmount);

				var amountDetailsGroupBox = control.FindSingle<ZGroupBox>("AmountDetailsGroupBox");
				AssertEquals(false, amountDetailsGroupBox.Visible);
			}
		}

		public void TestControls_HeaderDetailsGroupBoxTypeB()
		{
			var statement = Factory.New<CusStatementHeader>();
			statement.B2_StatementType = StatementHeaderTypeList.Codes.NormalReport;

			using (Statements form = new Statements(statement))
			using (var control = form.FindSingle<HeaderDetailsUserControl>())
			{
				form.Show();

				var payerFindBox = control.FindSingle<ZGuidFindBox>("ImporterGuidFindBox");
				AssertEquals(true, payerFindBox.Focused || payerFindBox.FindAll<System.Windows.Forms.Control>(x => x.Focused).Count() == 1);

				var dynamicHeaderDetailsPanel = control.FindSingle<DynamicLayoutPanel>("DynamicHeaderDetailsPanel");
				DynamicLayoutPanelTest.AssertControlsOrder(dynamicHeaderDetailsPanel,
					nameof(StatementHeaderControlBag.FormattedNumberTextBox),
					nameof(StatementHeaderControlBag.StatusDropEdit),
					nameof(StatementHeaderControlBag.ProcessPortCodeFindBox),
					nameof(StatementHeaderControlBag.StatementTypeDropEdit),
					nameof(StatementHeaderControlBag.StatementAmountCalcEdit),
					nameof(StatementHeaderControlBag.RelatedFormattedAccountNumberCodeFindBox),
					nameof(StatementHeaderControlBag.ImporterGuidFindBox),
					nameof(StatementHeaderControlBag.RelatedProcessDateEdit));

				var entryGroupBox = control.FindSingle<ZGroupBox>("EntryGroupBox");
				AssertEquals(true, entryGroupBox.Visible);

				var entryGrid = control.FindSingle<ZGrid>("EntriesGrid");
				var index = 0;
				AssertEquals(entryGrid.Columns[index++].ColumnName, nameof(CusStatementLine.FormattedNumber));
				AssertEquals(entryGrid.Columns[index++].ColumnName, CusStatementLine.Schema.B3_EntryType);
				AssertEquals(entryGrid.Columns[index++].ColumnName, CusStatementLine.Schema.B3_CustomsFeesTotal);

				var feesGroupBox = control.FindSingle<ZGroupBox>("FeesGroupBox");
				AssertEquals(true, feesGroupBox.Visible);
				AssertEquals("Fees", feesGroupBox.Text);

				var feesGrid = control.FindSingle<ZGrid>("FeesGrid");
				index = 0;
				AssertEquals(feesGrid.Columns[index++].ColumnName, CusStatementLineCharge.Schema.B4_ChargeType);
				AssertEquals(feesGrid.Columns[index++].ColumnName, nameof(CusStatementLineCharge.ChargeTypeName));
				AssertEquals(feesGrid.Columns[index++].ColumnName, CusStatementLineCharge.Schema.B4_ChargeAmount);

				var amountDetailsGroupBox = control.FindSingle<ZGroupBox>("AmountDetailsGroupBox");
				AssertEquals(false, amountDetailsGroupBox.Visible);
			}
		}

		public void TestControls_HeaderDetailsGroupBoxTypeI()
		{
			var statement = Factory.New<CusStatementHeader>();
			statement.B2_StatementType = StatementHeaderTypeList.Codes.Invoice;

			using (Statements form = new Statements(statement))
			using (var control = form.FindSingle<HeaderDetailsUserControl>())
			{
				form.Show();

				var payerFindBox = control.FindSingle<ZGuidFindBox>("ImporterGuidFindBox");
				AssertEquals(true, payerFindBox.Focused || payerFindBox.FindAll<System.Windows.Forms.Control>(x => x.Focused).Count() == 1);

				var dynamicHeaderDetailsPanel = control.FindSingle<DynamicLayoutPanel>("DynamicHeaderDetailsPanel");
				DynamicLayoutPanelTest.AssertControlsOrder(dynamicHeaderDetailsPanel,
					nameof(StatementHeaderControlBag.FormattedNumberTextBox),
					nameof(StatementHeaderControlBag.StatusDropEdit),
					nameof(StatementHeaderControlBag.ProcessPortCodeFindBox),
					nameof(StatementHeaderControlBag.StatementTypeDropEdit),
					nameof(StatementHeaderControlBag.ProcessAndDueDateUserControl),
					nameof(StatementHeaderControlBag.PaymentPartyDropEdit),
					nameof(StatementHeaderControlBag.PaymentDateEdit),
					nameof(StatementHeaderControlBag.PeriodDateUserControl),
					nameof(StatementHeaderControlBag.StatementAmountCalcEdit),
					nameof(StatementHeaderControlBag.RelatedFormattedAccountNumberCodeFindBox),
					nameof(StatementHeaderControlBag.ImporterGuidFindBox),
					nameof(StatementHeaderControlBag.RelatedProcessDateEdit));

				var entryGroupBox = control.FindSingle<ZGroupBox>("EntryGroupBox");
				AssertEquals(true, entryGroupBox.Visible);

				var entryGrid = control.FindSingle<ZGrid>("EntriesGrid");
				var index = 0;
				AssertEquals(entryGrid.Columns[index++].ColumnName, CusStatementLine.Schema.B3_SequenceNumber);
				AssertEquals(entryGrid.Columns[index++].ColumnName, nameof(CusStatementLine.FormattedNumber));
				AssertEquals(entryGrid.Columns[index++].ColumnName, nameof(CusStatementLine.FormattedLinePaymentNumber));
				AssertEquals(entryGrid.Columns[index++].ColumnName, CusStatementLine.Schema.B3_CustomsFeesTotal);

				var feesGroupBox = control.FindSingle<ZGroupBox>("FeesGroupBox");
				AssertEquals(true, feesGroupBox.Visible);
				AssertEquals("Entry Charges", feesGroupBox.Text);

				var feesGrid = control.FindSingle<ZGrid>("FeesGrid");
				index = 0;
				AssertEquals(feesGrid.Columns[index++].ColumnName, CusStatementLineCharge.Schema.B4_ChargeType);
				AssertEquals(feesGrid.Columns[index++].ColumnName, nameof(CusStatementLineCharge.ChargeTypeName));
				AssertEquals(feesGrid.Columns[index++].ColumnName, CusStatementLineCharge.Schema.B4_ChargeAmount);

				var amountDetailsGroupBox = control.FindSingle<ZGroupBox>("AmountDetailsGroupBox");
				AssertEquals(false, amountDetailsGroupBox.Visible);
			}
		}

		public void TestControls_HeaderDetailsGroupBoxTypeRorC()
		{
			var statement = Factory.New<CusStatementHeader>();
			statement.B2_StatementType = StatementHeaderTypeList.Codes.MonthlyReceipt;

			using (Statements form = new Statements(statement))
			using (var control = form.FindSingle<HeaderDetailsUserControl>())
			{
				form.Show();

				var payerFindBox = control.FindSingle<ZGuidFindBox>("ImporterGuidFindBox");
				AssertEquals(true, payerFindBox.Focused || payerFindBox.FindAll<System.Windows.Forms.Control>(x => x.Focused).Count() == 1);

				var dynamicHeaderDetailsPanel = control.FindSingle<DynamicLayoutPanel>("DynamicHeaderDetailsPanel");
				DynamicLayoutPanelTest.AssertControlsOrder(dynamicHeaderDetailsPanel,
					nameof(StatementHeaderControlBag.FormattedNumberTextBox),
					nameof(StatementHeaderControlBag.StatusDropEdit),
					nameof(StatementHeaderControlBag.ProcessPortCodeFindBox),
					nameof(StatementHeaderControlBag.StatementTypeDropEdit),
					nameof(StatementHeaderControlBag.ProcessDateEdit),
					nameof(StatementHeaderControlBag.PaymentTypeDropEdit),
					nameof(StatementHeaderControlBag.PaymentDateEdit),
					nameof(StatementHeaderControlBag.PaymentPartyDropEdit),
					nameof(StatementHeaderControlBag.TotalVATAmountCalcEdit),
					nameof(StatementHeaderControlBag.PeriodDateUserControl),
					nameof(StatementHeaderControlBag.StatementAmountCalcEdit),
					nameof(StatementHeaderControlBag.RelatedFormattedAccountNumberCodeFindBox),
					nameof(StatementHeaderControlBag.ImporterGuidFindBox),
					nameof(StatementHeaderControlBag.RelatedProcessDateEdit));

				var entryGroupBox = control.FindSingle<ZGroupBox>("EntryGroupBox");
				AssertEquals(true, entryGroupBox.Visible);

				var entryGrid = control.FindSingle<ZGrid>("EntriesGrid");
				var index = 0;
				AssertEquals(entryGrid.Columns[index++].ColumnName, CusStatementLine.Schema.B3_SequenceNumber);
				AssertEquals(entryGrid.Columns[index++].ColumnName, nameof(CusStatementLine.FormattedNumber));
				AssertEquals(entryGrid.Columns[index++].ColumnName, nameof(CusStatementLine.FormattedLinePaymentNumber));
				AssertEquals(entryGrid.Columns[index++].ColumnName, nameof(CusStatementLine.BaseAmount));
				AssertEquals(entryGrid.Columns[index++].ColumnName, CusStatementLine.Schema.B3_CustomsFeesTotal);

				var feesGroupBox = control.FindSingle<ZGroupBox>("FeesGroupBox");
				AssertEquals(false, feesGroupBox.Visible);

				var amountDetailsGroupBox = control.FindSingle<ZGroupBox>("AmountDetailsGroupBox");
				AssertEquals(false, amountDetailsGroupBox.Visible);
			}
		}

		public void TestControls_HeaderDetailsGroupBoxTypeD()
		{
			var statement = Factory.New<CusStatementHeader>();
			statement.B2_StatementType = StatementHeaderTypeList.Codes.CustomsDisbursementBill;

			using (Statements form = new Statements(statement))
			using (var control = form.FindSingle<HeaderDetailsUserControl>())
			{
				form.Show();

				var dynamicHeaderDetailsPanel = control.FindSingle<DynamicLayoutPanel>("DynamicHeaderDetailsPanel");
				DynamicLayoutPanelTest.AssertControlsOrder(dynamicHeaderDetailsPanel,
					nameof(StatementHeaderControlBag.FormattedNumberTextBox),
					nameof(StatementHeaderControlBag.StatusDropEdit),
					nameof(StatementHeaderControlBag.FormattedEntryNumberTextBox),
					nameof(StatementHeaderControlBag.DueDateEdit),
					nameof(StatementHeaderControlBag.BillTypeDropEdit),
					nameof(StatementHeaderControlBag.IssueDateEdit),
					nameof(StatementHeaderControlBag.CustomsAccountIDTextBox),
					nameof(StatementHeaderControlBag.ProcessDateEdit),
					nameof(StatementHeaderControlBag.ProcessPortCodeFindBox),
					nameof(StatementHeaderControlBag.PaymentDateEdit),
					nameof(StatementHeaderControlBag.ImporterGuidFindBox),
					nameof(StatementHeaderControlBag.StatementAmountCalcEdit),
					nameof(StatementHeaderControlBag.TotalAmountAfterDueDateCalcEdit));

				var customsOfficeControl = dynamicHeaderDetailsPanel.FindSingle<ZCodeFindBox>(nameof(StatementHeaderControlBag.ProcessPortCodeFindBox));
				AssertEquals(false, customsOfficeControl.ReadOnly);

				var payerControl = dynamicHeaderDetailsPanel.FindSingle<ZGuidFindBox>(nameof(StatementHeaderControlBag.ImporterGuidFindBox));
				AssertEquals(false, payerControl.ReadOnly);

				var entryGroupBox = control.FindSingle<ZGroupBox>("EntryGroupBox");
				AssertEquals(false, entryGroupBox.Visible);

				var feesGroupBox = control.FindSingle<ZGroupBox>("FeesGroupBox");
				AssertEquals(false, feesGroupBox.Visible);

				var amountDetailsGroupBox = control.FindSingle<ZGroupBox>("AmountDetailsGroupBox");
				AssertEquals(true, amountDetailsGroupBox.Visible);

				var dynamicLineDetailsPanel = control.FindSingle<DynamicLayoutPanel>("AmountDetailsPanel");
				DynamicLayoutPanelTest.AssertControlsOrder(dynamicLineDetailsPanel,
					nameof(StatementLineControlBag.DutyAmountCalcEdit),
					nameof(StatementLineControlBag.TransportationTaxCalcEdit),
					nameof(StatementLineControlBag.SpecialConsumptionTaxCalcEdit),
					nameof(StatementLineControlBag.LiquorTaxCalcEdit),
					nameof(StatementLineControlBag.EducationTaxCalcEdit),
					nameof(StatementLineControlBag.VATCalcEdit),
					nameof(StatementLineControlBag.AgricultureTaxCalcEdit),
					nameof(StatementLineControlBag.InterestCalcEdit),
					nameof(StatementLineControlBag.DeclarationPenaltyCalcEdit));
			}
		}

		public void TestHeaderDetailsGroupBoxText()
		{
			AssertHeaderDetailsGroupBoxText(StatementHeaderTypeList.Codes.CustomsDisbursementBill);
			AssertHeaderDetailsGroupBoxText(StatementHeaderTypeList.Codes.Invoice);
			AssertHeaderDetailsGroupBoxText(StatementHeaderTypeList.Codes.MonthlyReceipt);
			AssertHeaderDetailsGroupBoxText(StatementHeaderTypeList.Codes.IndividualCollectionReceipt);
			AssertHeaderDetailsGroupBoxText(StatementHeaderTypeList.Codes.Normal);
			AssertHeaderDetailsGroupBoxText(StatementHeaderTypeList.Codes.NormalReport);
		}

		void AssertHeaderDetailsGroupBoxText(ZString type)
		{
			var statement = Factory.New<CusStatementHeader>();
			statement.B2_StatementType = type;

			using (Statements form = new Statements(statement))
			using (var control = form.FindSingle<HeaderDetailsUserControl>())
			{
				form.Show();

				var headerDetailsGroupBox = control.FindSingle<ZGroupBox>("HeaderDetailsGroupBox");
				AssertEquals(StatementHeaderTypeList.GetFormName(type), headerDetailsGroupBox.Text);
			}
		}
	}
}
