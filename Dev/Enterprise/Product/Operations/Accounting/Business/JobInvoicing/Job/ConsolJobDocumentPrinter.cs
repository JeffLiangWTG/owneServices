using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngineCore.DocumentSupport;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	public class ConsolJobDocumentPrinter : JobDocumentPrinter
	{
		public ConsolJobDocumentPrinter(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		#region Properties

		public ZBool PrintJobByJobSummary { get; set; }
		public ZBool PrintContainerPackingSummary { get; set; }

		protected override string JobProfitDocumentMenuName
		{
			get { return Core.Constants.MenuNameConstantsForPrinting.ConsolJobProfitDocument; }
		}

		#endregion

		protected override DocumentCommand LoadDocumentCommand(ZQuery commandFilter, JobDocumentPrintItem jobDocumentPrintItem = null)
		{
			if (jobDocumentPrintItem is ConsolJobDocumentPrintItem consolPrintItem)
			{
				var documentCommands = new DocumentCommandCollection(consolPrintItem.Consol as IDocumentSupportable);
				documentCommands.Load();

				var commands = documentCommands.Find(commandFilter);
				if (commands.Length > 0)
				{
					return commands[0] as DocumentCommand;
				}
			}

			return base.LoadDocumentCommand(commandFilter, jobDocumentPrintItem);
		}
	}
}