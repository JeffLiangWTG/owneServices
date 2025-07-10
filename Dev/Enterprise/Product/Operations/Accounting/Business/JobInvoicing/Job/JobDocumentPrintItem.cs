using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	public class JobDocumentPrintItem : NonPersistentBusinessObject, IDocumentSupportable, IObsoleteValidation
	{
		public JobDocumentPrintItem(JobDocumentPrinter printer, Job job, BusinessObjectFactory factory)
			: base(factory)
		{
			if (printer == null)
			{
				throw new ArgumentNullException(nameof(printer), "JobDocumentPrinter");
			}

			this.Job = job;
			this.Printer = printer;
		}

		#region Properties

		public Job Job { get; private set; }
		public JobDocumentPrinter Printer { get; private set; }
		public ZBool PrintChargeSummary { get { return Printer.PrintChargeSummary; } }
		public ZBool PrintProfitRecognitionByDateSummary { get { return Printer.PrintProfitRecognitionByDateSummary; } }
		public ZBool PrintChargeDetail { get { return Printer.PrintChargeDetail; } }
		public ZBool PrintARInvoiceAnalysis { get { return Printer.PrintARInvoiceAnalysis; } }
		public ZBool PrintAPInvoiceAnalysis { get { return Printer.PrintAPInvoiceAnalysis; } }
		public ZBool PrintJobRevenueJournalAnalysis { get { return Printer.PrintJobRevenueJournalAnalysis; } }
		public ZBool IsProfitLossDoc { get { return Printer.IsProfitLossDoc; } }

		internal virtual string JobProfitDocumentMenuName
		{
			get { return (NoResString)"Job Profit Document"; }
		}

		#endregion

		#region IDocumentSupportable Members

		public virtual DocumentSupporter DocumentSupporter
		{
			get { return JobDocumentSupporter.New(this); }
		}

		#endregion

	}
}
