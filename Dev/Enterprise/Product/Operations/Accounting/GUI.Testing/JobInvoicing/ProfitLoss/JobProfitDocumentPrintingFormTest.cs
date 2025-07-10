using System.Windows.Forms;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.JobInvoicing.Testing
{
	[TestedType(typeof(JobProfitDocumentPrintingForm))]
	public class JobProfitDocumentPrintingFormTest : ZFormBasherTest
	{
		class JobProfitDocumentPrintingFormForTest : JobProfitDocumentPrintingForm
		{
			public JobProfitDocumentPrintingFormForTest()
				: base()
			{
				TopsofControlsInOrder = new int[PrintCheckBoxesInOrderOfAppearance.Length];
				int x = 0;
				foreach (CheckBox box in PrintCheckBoxesInOrderOfAppearance)
				{
					TopsofControlsInOrder[x] = box.Top;
					x++;
				}
				OriginalFormHeight = this.Height;
			}
			public CheckBox[] PrintCheckBoxesInOrder
			{
				get { return base.PrintCheckBoxesInOrderOfAppearance; }
			}

			public CheckBox PrintProfitRecognitionByDateSummaryForTest
			{
				get { return this.PrintProfitRecognitionByDateSummary; }
			}

			public CheckBox PrintJobRevenueJournalAnalysisCheckBoxForTest
			{
				get { return this.PrintJobRevenueJournalAnalysisCheckBox; }
			}

			internal readonly int[] TopsofControlsInOrder;
			internal readonly int OriginalFormHeight;
		}

		public void TestPrintJobRevenueJournalAnalysisCheckBox()
		{
			using (var form = new JobProfitDocumentPrintingFormForTest())
			{
				form.Show();
				AssertNotNull(form.PrintJobRevenueJournalAnalysisCheckBoxForTest);
				Assert(form.PrintJobRevenueJournalAnalysisCheckBoxForTest.Visible);
			}
		}

		public void TestPrintControlPositionAndFormSize()
		{
			using (JobProfitDocumentPrintingFormForTest form = new JobProfitDocumentPrintingFormForTest())
			{
				form.PrintProfitRecognitionByDateSummaryForTest.Visible = true;
				form.Show();
				AssertNoControlsHaveMoved(form);

				form.Hide();
				form.PrintProfitRecognitionByDateSummaryForTest.Visible = false;
				form.Show();
				AssertCheckBoxes1And2DontMove3AndUpDo(form);
			}
		}

		void AssertNoControlsHaveMoved(JobProfitDocumentPrintingFormForTest form)
		{
			int x = 0;
			foreach (CheckBox box in form.PrintCheckBoxesInOrder)
			{
				AssertEquals(form.TopsofControlsInOrder[x], form.PrintCheckBoxesInOrder[x].Top);
				x++;
			}
		}

		void AssertCheckBoxes1And2DontMove3AndUpDo(JobProfitDocumentPrintingFormForTest form)
		{
			for (int x = 0; x < 2; x++)
			{
				AssertEquals(form.TopsofControlsInOrder[x], form.PrintCheckBoxesInOrder[x].Top);
			}

			for (int x = 2; x < form.PrintCheckBoxesInOrder.Length; x++)
			{
				AssertEquals(form.TopsofControlsInOrder[x - 1], form.PrintCheckBoxesInOrder[x].Top);
			}
		}

		public void TestFormVerb()
		{
			using (JobProfitDocumentPrintingForm form = (JobProfitDocumentPrintingForm)GetFormToBashCore())
			{
				AssertEquals("FormVerb", "", form.FormVerb);
			}
		}

		protected override Form GetFormToBashCore()
		{
			JobDocumentPrinter printer = new JobDocumentPrinter(Factory);
			Factory.Save();
			return new JobProfitDocumentPrintingForm(printer);
		}

		protected override bool AllowFormSizeFixed => true;
	}
}
