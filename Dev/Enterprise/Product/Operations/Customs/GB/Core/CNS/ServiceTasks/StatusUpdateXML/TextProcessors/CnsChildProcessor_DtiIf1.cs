using CargoWise.Types;
using Enterprise.Customs.GB.Business.Declaration;

namespace Enterprise.Customs.GB.CNS.ServiceTasks.StatusUpdateXML.TextProcessors
{
	// Inventory failure report (E0)

	class CnsChildProcessor_DtiIf1 : CnsChildProcessor
	{
		protected override ZString RegexPatternForEntryNumberAndDate
		{
			get { return @"Entry:? *-? *(?:\d{3}-)*?(\d{3}-.{7}|[^- ]{1,35}) *-? *(\d\d/\d\d/\d\d\d\d)"; }
		}

		protected override ZString DateMaskPatternForEntryDate
		{
			get { return "dd/MM/yyyy"; }
		}

		protected override void DoFurtherProcessingForSuccessfullyFoundEntry(CusEntryHeader entryHeader)
		{
			base.DoFurtherProcessingForSuccessfullyFoundEntry(entryHeader);
			UpdateICS(entryHeader);
			UpdateIRC(entryHeader);
		}

		void UpdateIRC(CusEntryHeader entryHeader)
		{
			var inventoryReturnCode = GetFirstGroupMatchFromMessageText(@"Inventory response\s+(\d\d\d)\s?");
			if (entryHeader != null)
			{
				entryHeader.CH_IrcInventoryReturnCode = inventoryReturnCode;
			}
		}

		void UpdateICS(CusEntryHeader entryHeader)
		{
			var importClearanceStatus = GetFirstGroupMatchFromMessageText(@"Import clearance status\s+([A-Za-z0-9]{2})\s?");
			if (!importClearanceStatus.IsEmpty && entryHeader != null)
			{
				entryHeader.CH_ImportClearanceStatusICS = importClearanceStatus;
			}
		}
	}
}
