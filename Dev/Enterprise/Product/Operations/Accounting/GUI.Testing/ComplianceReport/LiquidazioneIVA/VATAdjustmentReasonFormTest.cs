using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Accounting.Business.ComplianceReport;
using Enterprise.Accounting.Business.ComplianceReport.LiquidazioneIVA;
using Enterprise.Accounting.Utility.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.ComplianceReport.LiquidazioneIVA.Testing
{
	public class VATAdjustmentReasonFormTest : TransactionCreatorBaseTest
	{
		[ExpectNoExceptions]
		public void TestLoad()
		{
			using (VATAdjustmentReasonForm form = new VATAdjustmentReasonForm(Row))
			{
				form.Show();
				Application.DoEvents();
				AssertEquals(true, form.Visible);
			}
		}

		public void TestOKButton()
		{
			Columns.Adjustments.Box1_TotalVatBaseReceivables = 0m;
			AssertEquals(Row.AdjustmentsAmount, 0m);

			using (VATAdjustmentReasonForm form = new VATAdjustmentReasonForm(Row))
			{
				form.Show();
				Row.AdjustmentsAmount = 15m;
				form.AcceptButton.PerformClick();
				AssertEquals(DialogResult.None, form.DialogResult);
				AssertEquals(0m, Columns.Adjustments.Box1_TotalVatBaseReceivables);
				AssertEquals(string.Empty, Columns.ReasonHolder1.Code);
				AssertEquals(string.Empty, Columns.ReasonHolder1.Reason);

				Row.ReasonHolder.Code = "IDE";
				Row.ReasonHolder.Reason = "Free text";
				form.AcceptButton.PerformClick();
				AssertEquals(DialogResult.OK, form.DialogResult);
				AssertEquals(15m, Columns.Adjustments.Box1_TotalVatBaseReceivables);
				AssertEquals("IDE", Columns.ReasonHolder1.Code);
				AssertEquals("Free text", Columns.ReasonHolder1.Reason);
			}
		}

		public void TestCancelButton()
		{
			using (VATAdjustmentReasonForm form = new VATAdjustmentReasonForm(Row))
			{
				form.Show();
				form.CancelButton.PerformClick();
				AssertEquals(DialogResult.Cancel, form.DialogResult);
			}
		}

		public void TestTotalBox()
		{
			Columns.ComputedByCW1.Box1_TotalVatBaseReceivables = 2500m;
			Row = new LIQSubmissionDataRow(Factory, RowType.TotalVatBaseReceivables, "Test", Columns);

			using (VATAdjustmentReasonForm form = new VATAdjustmentReasonForm(Row))
			{
				form.Show();
				AssertEquals(2500m, form.CW1Box.CalcValue);
				Row.AdjustmentsAmount = -800m;
				AssertEquals(-800m, form.AdjustmentsBox.CalcValue);
				AssertEquals(1700m, form.TotalBox.CalcValue);
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			Report = Factory.NewWithValidTestData<AccComplianceReport>();
			Columns = new LIQSubmissionDataColumns(Factory, Report);
			Columns.PopulateDataFromReport();
			Columns.ReturnDueDate = ZDateTime.Today.Date;
			Row = new LIQSubmissionDataRow(Factory, RowType.TotalVatBaseReceivables, "Test", Columns);
		}

		AccComplianceReport Report;
		LIQSubmissionDataColumns Columns;
		LIQSubmissionDataRow Row;
	}
}
