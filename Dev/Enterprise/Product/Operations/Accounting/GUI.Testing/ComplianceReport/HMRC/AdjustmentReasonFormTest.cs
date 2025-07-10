using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Accounting.Business.ComplianceReport;
using Enterprise.Accounting.Business.ComplianceReport.HMRC;
using Enterprise.Accounting.Utility.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.ComplianceReport.HMRC.Testing
{
	public class AdjustmentReasonFormTest : TransactionCreatorBaseTest
	{
		[ExpectNoExceptions]
		public void TestLoad()
		{
			using (AdjustmentReasonForm form = new AdjustmentReasonForm(Row))
			{
				form.Show();
				Application.DoEvents();
			}
		}

		public void TestOKButton()
		{
			Columns.Adjustments.Box1_VATDue = 0m;
			AssertEquals(Row.AdjustmentsAmount, 0m);

			using (AdjustmentReasonForm form = new AdjustmentReasonForm(Row))
			{
				form.Show();
				Row.AdjustmentsAmount = 15m;
				form.AcceptButton.PerformClick();
				AssertEquals(DialogResult.None, form.DialogResult);
				AssertEquals(0m, Columns.Adjustments.Box1_VATDue);
				AssertEquals(string.Empty, Columns.ReasonHolder1.Code);
				AssertEquals(string.Empty, Columns.ReasonHolder1.Reason);

				Row.ReasonHolder.Code = "IDE";
				Row.ReasonHolder.Reason = "Free text";
				form.AcceptButton.PerformClick();
				AssertEquals(DialogResult.OK, form.DialogResult);
				AssertEquals(15m, Columns.Adjustments.Box1_VATDue);
				AssertEquals("IDE", Columns.ReasonHolder1.Code);
				AssertEquals("Free text", Columns.ReasonHolder1.Reason);
			}
		}

		public void TestCancelButton()
		{
			using (AdjustmentReasonForm form = new AdjustmentReasonForm(Row))
			{
				form.Show();
				form.CancelButton.PerformClick();
				AssertEquals(DialogResult.Cancel, form.DialogResult);
			}
		}

		public void TestTotalBox()
		{
			Columns.ComputedByCW1.Box1_VATDue = 2300m;
			Columns.UnsubmitedPreviousValues.Box1_VATDue = 1200m;
			Row = new MTDSubmissionDataRow(Factory, 1, "Test", Columns);

			using (AdjustmentReasonForm form = new AdjustmentReasonForm(Row))
			{
				form.Show();
				AssertEquals(2300m, form.CW1Box.CalcValue);
				AssertEquals(1200m, form.ErrorsBox.CalcValue);
				Row.AdjustmentsAmount = -600m;
				AssertEquals(-600m, form.AdjustmentsBox.CalcValue);
				AssertEquals(2900m, form.TotalBox.CalcValue);
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			Report = Factory.NewWithValidTestData<AccComplianceReport>();
			Columns = new MTDSubmissionDataColumns(Factory, Report);
			Columns.PopulateDataFromReport();
			Columns.ReturnDueDate = ZDateTime.Today.Date;
			Row = new MTDSubmissionDataRow(Factory, 1, "Test", Columns);
		}

		AccComplianceReport Report;
		MTDSubmissionDataColumns Columns;
		MTDSubmissionDataRow Row;
	}
}
