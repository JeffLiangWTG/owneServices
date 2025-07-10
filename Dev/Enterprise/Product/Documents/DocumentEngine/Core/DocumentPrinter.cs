using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentSupport;

namespace Enterprise.DocumentEngine.Public
{
	using Integration;

	public class DocumentPrinter : IDocumentPrinter
	{
		public DocumentPrinter(bool showNotification = true)
		{
			ShowNotification = showNotification;
		}

		protected bool ShowNotification
		{
			get;
			private set;
		}

		public void Print(ZGuid menuPK, ZGuid printer, IDocumentSupportable parent)
		{
			Print(menuPK, printer, parent, 1);
		}

		public void Print(ZGuid menuPK, ZGuid printer, IDocumentSupportable parent, int numberOfCopies)
		{
			var factory = new BusinessObjectFactory();
			var documentCommand = factory.Load<DocumentCommand>(menuPK);
			if (documentCommand != null)
			{
				documentCommand.Parent = parent;
				PrintCore(documentCommand, printer, numberOfCopies);
			}
		}

#if DEBUG
		internal DeliveryInstructions LastDeliveryInstructionsForTesting;
		internal DocumentPrintSet LastPrintSetForTesting;
#endif

#if DEBUG
		protected virtual
#endif
		void PrintCore(DocumentCommand documentCommand, ZGuid printer, int numberOfCopies)
		{
			using (var set = new DocumentPrintSet(documentCommand, null))
			{
				if (!ShowNotification)
				{
					set.PrintTaskUIProviderType = PrintTaskUIProviderTypes.Unattended;
				}

				var instructions = new DeliveryInstructions();
				instructions.AllowAutoDelivery = false;
				instructions.PrinterDelivery.PrintQueuePK = printer;
				instructions.PrinterDelivery.NumberOfCopies = numberOfCopies;
				instructions.Destination = DeliveryInstructionDestination.Print;

				set.Run(instructions);
#if DEBUG
				LastDeliveryInstructionsForTesting = instructions;
				LastPrintSetForTesting = set;
#endif
			}
		}
	}
}
