using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Forms;
using Enterprise.Customs.FR.Business.CusStatement;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.FR.GUI.Testing
{
	public class StatementHeaderUserControlTest : TestCaseWithFactory
	{
		public void TestDetailsGroupBox()
		{
			var statement = Factory.New<CusStatementHeader>();
			using (var form = new ZForm(statement))
			using (var control = new StatementHeaderUserControl())
			{
				form.Controls.Add(control);
				form.Show();

				CombineAssertions(() =>
				{
					AssertEquals("Statement Number", control.FindSingle<ZTextBox>("StatementNumberTextBox").CaptionResourceString.Caption);
					AssertEquals("Reference Number", control.FindSingle<ZTextBox>("ReferenceNumberTextBox").CaptionResourceString.Caption);
					AssertEquals("Entry Number", control.FindSingle<ZTextBox>("EntryNumberTextBox").CaptionResourceString.Caption);
					AssertEquals("Status", control.FindSingle<ZDropEdit>("StatusDropEdit").CaptionResourceString.Caption);
					AssertEquals("Direction", control.FindSingle<ZDropEdit>("BranchDesignationDropEdit").CaptionResourceString.Caption);
					AssertEquals("Delta G Party", control.FindSingle<ZGuidFindBox>("ImporterFindBox").CaptionResourceString.Caption);
					AssertEquals("Reporting Frequency", control.FindSingle<ZDropEdit>("StatementTypeDropEdit").CaptionResourceString.Caption);
					AssertEquals("Period Start Date", control.FindSingle<ZDateEdit>("PeriodStartDateEdit").CaptionResourceString.Caption);
					AssertEquals("Period End Date", control.FindSingle<ZDateEdit>("PeriodEndDateEdit").CaptionResourceString.Caption);
					AssertEquals("Delta Agreement Number", control.FindSingle<ZDropEdit>("EntryFillerCodeDropEdit").CaptionResourceString.Caption);
					AssertEquals("Method of Payment", control.FindSingle<ZDropEdit>("PaymentTypeDropEdit").CaptionResourceString.Caption);
					AssertEquals("Operational Representative", control.FindSingle<ZTextBox>("ImporterCustomsIDTextBox").CaptionResourceString.Caption);
					AssertEquals("Deferral Account No.", control.FindSingle<ZTextBox>("CheckNoTextBox").CaptionResourceString.Caption);
				});
			}
		}

		public void TestChargesGrid()
		{
			var statement = Factory.New<CusStatementHeader>();
			using (var form = new ZForm(statement))
			using (var control = new StatementHeaderUserControl())
			{
				form.Controls.Add(control);
				form.Show();
				var grid = control.FindSingle<ZGrid>("ChargesGrid");

				CombineAssertions(() =>
				{
					AssertEquals("Charges grid should be read only.", true, grid.ReadOnly);

					AssertContainsExactElementsInAnyOrder(new[]
					{
						nameof(CusStatementLineCharge.B4_ChargeType),
						nameof(CusStatementLineCharge.B4_ChargeAmount),
						nameof(CusStatementLineCharge.B4_MethodOfPayment),
						nameof(CusStatementLineCharge.MethodOfPaymentDescription),
						nameof(CusStatementLineCharge.B4_ChargeGroup)
					}, grid.ColumnStyles.Cast<ZGridColumnInfo>().Select(x => x.ColumnName));

					AssertEquals("Method of Payment", grid.GetColumnStyle(nameof(CusStatementLineCharge.B4_MethodOfPayment)).GroupName.Caption);
					AssertEquals("Method of Payment", grid.GetColumnStyle(nameof(CusStatementLineCharge.MethodOfPaymentDescription)).GroupName.Caption);

					AssertEquals(2, ((ZCalcEditColumnStyleInfo)grid.GetColumnStyle(nameof(CusStatementLineCharge.B4_ChargeAmount))).Decimals);
				});
			}
		}

		public void TestEntriesGrid()
		{
			var statement = Factory.New<CusStatementHeader>();
			using (var form = new ZForm(statement))
			using (var control = new StatementHeaderUserControl())
			{
				form.Controls.Add(control);
				form.Show();
				var grid = control.FindSingle<ZGrid>("EntriesGrid");

				CombineAssertions(() =>
				{
					AssertEquals("Entries grid should be read only.", true, grid.ReadOnly);

					AssertContainsExactElementsInAnyOrder(new[]
					{
						nameof(CusStatementEntry.B3_EntryNum),
						nameof(CusStatementEntry.B3_BrokerReference),
						nameof(CusStatementEntry.B3_EntryType)
					}, grid.ColumnStyles.Cast<ZGridColumnInfo>().Select(x => x.ColumnName));
				});
			}
		}
	}
}
