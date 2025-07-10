using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.DocumentEngineCore.DocumentSupport;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	public class ConsolJobDocumentPrintItem : JobDocumentPrintItem
	{
		public ConsolJobDocumentPrintItem(ConsolJobDocumentPrinter printer,
				IJobCostingPlugIn consol, BusinessObjectFactory factory)
			: base(printer, null, factory)
		{
			this.Consol = consol;
		}

		#region Properties

		public new ConsolJobDocumentPrinter Printer
		{
			get { return (ConsolJobDocumentPrinter)base.Printer; }
		}

		public IJobCostingPlugIn Consol { get; private set; }

		public ZBool PrintJobByJobSummary
		{
			get { return Printer.PrintJobByJobSummary; }
		}

		public ZBool PrintContainerPackingSummary
		{
			get { return Printer.PrintContainerPackingSummary; }
		}

		internal override string JobProfitDocumentMenuName
		{
			get { return Core.Constants.MenuNameConstantsForPrinting.ConsolJobProfitDocument; }
		}

		#endregion

		#region IDocumentSupportable Members

		public override DocumentSupporter DocumentSupporter
		{
			get { return ConsolJobDocumentSupporter.New(this); }
		}

		#endregion
	}
}