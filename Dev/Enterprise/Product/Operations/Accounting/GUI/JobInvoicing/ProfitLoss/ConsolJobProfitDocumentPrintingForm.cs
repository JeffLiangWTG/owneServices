using System.Collections.Generic;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI.JobInvoicing
{
	public partial class ConsolJobProfitDocumentPrintingForm : JobProfitDocumentPrintingForm
	{
		#region Controls

		ZCheckBox PrintJobByJobSummaryCheckBox;
		ZCheckBox PrintContainerPackingSummaryCheckBox;
		readonly System.ComponentModel.IContainer components;

		#endregion

		public ConsolJobProfitDocumentPrintingForm(ConsolJobDocumentPrinter jobDocPrinter) : base(jobDocPrinter)
		{ }

		protected override ZCheckBox[] PrintCheckBoxesInOrderOfAppearance
		{
			get
			{
				return fPrintCheckBoxesInOrderOfAppearance ?? (fPrintCheckBoxesInOrderOfAppearance = GetFullListOfCheckBoxesIncludingBase());
			}
		}
		ZCheckBox[] fPrintCheckBoxesInOrderOfAppearance;

		ZCheckBox[] GetFullListOfCheckBoxesIncludingBase()
		{
			List<ZCheckBox> boxes = new List<ZCheckBox>(base.PrintCheckBoxesInOrderOfAppearance);
			boxes.Add(this.PrintJobByJobSummaryCheckBox);
			boxes.Add(this.PrintContainerPackingSummaryCheckBox);
			return boxes.ToArray();
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}
	}
}

