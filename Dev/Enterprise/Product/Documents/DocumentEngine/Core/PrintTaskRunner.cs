using CargoWise.EntityFramework;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.DocumentEngine
{
	class PrintTaskRunner : IPrintTaskRunner
	{
		public void RunPrintTask(IStmMenuItem menuItem, IDocumentSupportable documentSupportable)
		{
			if (documentSupportable != null)
			{
				var documentCommand = Factory.Load<DocumentCommand>(menuItem.PK);
				var docPack = new DocumentPack(documentCommand, documentSupportable, null, null, false);
				var printTask = new PrintTask();
				printTask.DeliveryInstructionsDefaultPK = menuItem.PK; // without this printer choice is not saved
				printTask.Add(docPack);

				var deliveryInstructions = new DeliveryInstructions(docPack);
				foreach (DocDeliveryContact recipient in deliveryInstructions.Recipients)
				{
					recipient.DeliveryMethod = Core.Constants.ContactNotifyModes.Print;
				}

#if DEBUG
				LastCreatedDocPack = docPack;
				LastCreatedPrintTask = printTask;
				DeliveryInstructionsForTest = deliveryInstructions;
#endif

				printTask.RunWithPartialInstructions(AllowedDeliveryOptions.All, deliveryInstructions, Env.Security.None);
			}
		}

		public void RunPrintTaskIncludingChildMenus(IStmMenuItem menuItem, BusinessObject businessObject)
		{
			if (businessObject == null)
			{
				return;
			}

			var printTask = GetPrintTask(menuItem, businessObject as IDocumentSupportable);
#if DEBUG
			LastCreatedDocPack = printTask[0];
			LastCreatedPrintTask = printTask;
#endif
			printTask.Run(Env.Security.None);
		}

		PrintTask GetPrintTask(IStmMenuItem menuItem, IDocumentSupportable documentSupportable)
		{
			var documentCommand = Factory.Load<DocumentCommand>(menuItem.PK);
			if (documentSupportable != null && documentCommand != null)
			{
				documentCommand.Parent = documentSupportable;
			}
			var printTask = new PrintTask(documentCommand);
			printTask.DeliveryInstructionsDefaultPK = menuItem.PK;
			var loader = new PrintTaskDocumentPackLoader(printTask, documentCommand, null);
			loader.LoadAll();

			return printTask;
		}

#if DEBUG
		internal DocumentPack LastCreatedDocPack;
		internal PrintTask LastCreatedPrintTask;
		internal DeliveryInstructions DeliveryInstructionsForTest;
#endif

		BusinessObjectFactory Factory
		{
			get
			{
				return factory ?? (factory = new BusinessObjectFactory());
			}
		}
		BusinessObjectFactory factory;
	}
}
