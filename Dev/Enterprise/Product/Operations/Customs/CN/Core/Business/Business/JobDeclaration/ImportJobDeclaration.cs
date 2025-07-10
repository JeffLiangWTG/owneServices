using CargoWise.EntityFramework;

namespace Enterprise.Customs.CN.Business
{
	public class ImportJobDeclaration : Customs.Business.ImportJobDeclaration
	{
		public ImportJobDeclaration(BusinessObjectFactory factory) : base(factory)
		{
		}

		protected override void PerformCountrySpecificImporting()
		{
			base.PerformCountrySpecificImporting();
			var importedDeclaration = ImportedDeclaration as JobDeclaration;
			if (importedDeclaration != null)
			{
				importedDeclaration.DefaultOriginDistrictIfNeeded();
				importedDeclaration.DefaultDestinationDistrictIfNeeded();
				importedDeclaration.DefaultCountryOfTrade();
				importedDeclaration.TransportDataHelper.DefaultBillOfLadingOnEntryInstructions();
				importedDeclaration.UpdateJE_CNTransportMode();
				importedDeclaration.OriginDefaulter.DefaultPort();
				importedDeclaration.FinalDestinationDefaulter.DefaultPort();
				importedDeclaration.DefaultLastPortBeforeEntryIfNeeded();
				importedDeclaration.DefaultValuesFromSupplierImporterLinkTransportMode();

				var instructions = importedDeclaration.CustomsEntryInstructions;
				for (int i = 0; i < instructions.Count; i++)
				{
					var previous = i == 0 ? null : instructions[i - 1];
					var instruction = instructions[i];
					instructions.SetDefaultsForInstruction(previous, instruction);
				}

				var invoices = importedDeclaration.Invoices;
				foreach (var invoice in invoices)
				{
					invoices.SetDefaultsForInvoiceHeader(invoice);
				}

				var invoiceLines = importedDeclaration.InvoiceLines;
				foreach (JobComInvoiceLine invoiceLine in invoiceLines)
				{
					invoiceLines.DefaultPreferenceIfNeeded(invoiceLine);
					if (!invoiceLine.JI_PartNo.IsEmpty)
					{
						invoiceLine.PartSyncManager.Refresh();
					}
				}
			}
		}
	}
}
