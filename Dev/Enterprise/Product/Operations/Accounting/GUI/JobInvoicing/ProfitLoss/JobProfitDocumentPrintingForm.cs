using System;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI.JobInvoicing
{
	public partial class JobProfitDocumentPrintingForm : ZChildForm
	{
		protected ZCheckBox PrintChargeSummaryCheckBox;
		protected ZCheckBox PrintChargeDetailCheckBox;
		protected ZCheckBox PrintARInvoiceAnalysisCheckBox;
		protected ZCheckBox PrintAPInvoiceAnalysisCheckBox;
		protected ZButton PrintButton;
		protected ZButton CloseButton;
		protected ZCheckBox PrintProfitRecognitionByDateSummary;
		readonly System.ComponentModel.IContainer components;
		protected ZCheckBox PrintJobRevenueJournalAnalysisCheckBox;

		protected virtual ZCheckBox[] PrintCheckBoxesInOrderOfAppearance
		{
			get
			{
				return fPrintCheckBoxesInOrderOfAppearance ?? (fPrintCheckBoxesInOrderOfAppearance = new ZCheckBox[]
				{
					PrintChargeSummaryCheckBox, PrintProfitRecognitionByDateSummary,
					PrintChargeDetailCheckBox, PrintARInvoiceAnalysisCheckBox, PrintAPInvoiceAnalysisCheckBox
				});
			}
		}
		ZCheckBox[] fPrintCheckBoxesInOrderOfAppearance;

		protected override void OnVisibleChanged(EventArgs e)
		{
			base.OnVisibleChanged(e);
			RepositionPrintControlsBasedOnVisibility();
		}

		protected virtual void RepositionPrintControlsBasedOnVisibility()
		{
			if (Visible)
			{
				for (int y = 0; y < PrintCheckBoxesInOrderOfAppearance.Length; y++)
				{
					if (!PrintCheckBoxesInOrderOfAppearance[y].Visible)
					{
						int[] currentPositions = new int[PrintCheckBoxesInOrderOfAppearance.Length];
						int x = 0;
						foreach (CheckBox box in PrintCheckBoxesInOrderOfAppearance)
						{
							currentPositions[x] = box.Top;
							x++;
						}

						for (int z = y + 1; z < PrintCheckBoxesInOrderOfAppearance.Length; z++)
						{
							ControlDpiScalingHelper.SetTop(PrintCheckBoxesInOrderOfAppearance[z], currentPositions[z - 1], false);
						}
					}
				}
			}
		}

		protected JobProfitDocumentPrintingForm()
		{
		}

		public JobProfitDocumentPrintingForm(JobDocumentPrinter jobDocPrinter)
			: base(jobDocPrinter)
		{
			ZFormPostingButtonsStrategy.SetupPosting(this, null, null, (ZButton)null);
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}

		public override string FormVerb
		{
			get { return ""; }
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}
	}
}

