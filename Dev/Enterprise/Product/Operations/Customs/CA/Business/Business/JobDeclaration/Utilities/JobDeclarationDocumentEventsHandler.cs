using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.CA.Business
{
	public class JobDeclarationDocumentEventsHandler : IDocumentEventsHandler
	{
		public JobDeclarationDocumentEventsHandler(JobDeclaration declaration)
		{
			this.declaration = declaration;
		}

		#region Implementation of IDocumentEventsHandler

		public bool CanHandleMenuItem(IStmMenuItem menuItem)
		{
			return menuItem.SU_MenuName == JobDeclarationDocumentSupporter.LVSIdentifierDetailsDocument;
		}

		void IDocumentEventsHandler.HandleDocumentPrintRequested(object sender, DocumentCancelEventArgs e)
		{
		}

		void IDocumentEventsHandler.HandleDocumentPrePreviewed(object sender, DocumentPrintedEventArgs e)
		{
		}

		void IDocumentEventsHandler.HandleDocumentPrePrinted(object sender, DocumentPrintedEventArgs e)
		{
		}

		void IDocumentEventsHandler.HandleDocumentPrinted(object sender, DocumentPrintedEventArgs e)
		{
			if (e.DeliveryInstructionDestinationType != DeliveryInstructionDestination.UserCancelled
				&& e.MenuItem.SU_MenuName == JobDeclarationDocumentSupporter.LVSIdentifierDetailsDocument)
			{
				var datePrinted = ZDateTime.Now;
				foreach (JobComInvoiceHeaderToPrint invoice in declaration.LinesToPrint)
				{
					if (invoice.InvoiceHeader != null && invoice.ShouldBePrinted)
					{
						invoice.InvoiceHeader.CA_LVSLastPrintDate = datePrinted;
					}
				}

				declaration.Factory.Save();
			}
		}

		DocumentSupporter IDocumentEventsHandler.DocumentSupporter { get; set; }

		#endregion

		readonly JobDeclaration declaration;
	}
}
