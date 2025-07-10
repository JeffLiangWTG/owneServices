using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Integration.DocumentVisualizer;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentEngine.DocumentDelivery
{
	public class SilentDocumentPrinter
	{
		public SilentDocumentPrinter(BusinessObjectFactory factory, IDocumentSupportable documentSupportableBizO, ZString documentName, ZString menuPath, ZString filterList)
		{
			this.factory = factory;
			this.documentSupportableBizO = documentSupportableBizO;
			this.documentCommand = DocumentCommand.GetDocumentCommand(factory, documentSupportableBizO, documentName, menuPath, filterList);
		}

		public SilentDocumentPrinter(BusinessObjectFactory factory, IDocumentSupportable documentSupportableBizO, DocumentCommand documentCommand)
		{
			this.factory = factory;
			this.documentSupportableBizO = documentSupportableBizO;
			this.documentCommand = documentCommand;
		}

		readonly DocumentCommand documentCommand;
		readonly BusinessObjectFactory factory;
		readonly IDocumentSupportable documentSupportableBizO;

		public void Print(ZGuid printQueuePK, ZInt copies, ZBool copyToEDocs, bool forceCorrectBizObjWhenPrintingToBothEdocsAndPaper = false, DocManagerInfo businessObjectForPrintJobParent = null, string name = null, string title = null, string language = null)
		{
			if (documentCommand != null)
			{
				if (copies > 0 && printQueuePK != ZGuid.Empty)
				{
					using (var task = new PrintTask(documentCommand))
					{
						var docPack = new DocumentPack(documentCommand, documentSupportableBizO, new Enterprise.DocumentEngine.RuntimeOptions.UserControlProviderList(), null, bizoToForcedToBeParentOfEDoc: businessObjectForPrintJobParent);

						if (forceCorrectBizObjWhenPrintingToBothEdocsAndPaper)
						{
							docPack.ForceBusinessObjectToLogAgainst(documentSupportableBizO as BusinessObject);
						}

						task.Add(docPack);
						var printQueue = factory.Load<StmPrintQueue>(printQueuePK);

						if (printQueue != null)
						{
							var instructions = new DeliveryInstructions(docPack, new Enterprise.DocumentEngine.DeliveryMethods.FactoryStrategy.PopulateButDoNotSave(factory));
							instructions.Recipients.RemoveAll();
							var recipient = instructions.Recipients.AddNew();
							recipient.DeliveryMethod = Core.Constants.ContactNotifyModes.Print;
							instructions.Destination = DeliveryInstructionDestination.Print;
							instructions.PrinterDelivery.PrintQueuePK = printQueue.PK;
							instructions.PrinterDelivery.NumberOfCopies = copies;

							if (!string.IsNullOrEmpty(language))
							{
								docPack.SupportsLanguageSelection = true;
								docPack.Language = language;
								instructions.Language = language;
							}

							task.Run(instructions);
						}
					}
				}
				else if (copyToEDocs)
				{
					var docPack = new DocumentPack(documentCommand, documentSupportableBizO, null, null, bizoToForcedToBeParentOfEDoc: businessObjectForPrintJobParent);
					docPack.ForceBusinessObjectToLogAgainst(documentSupportableBizO as BusinessObject);

					if (documentCommand.SU_MenuType == Core.Constants.StmMenuItemTypes.Forms)
					{
						var documentEDocsDelivery = ObjectFactory.Get<IDocumentEDocsDelivery>();
						docPack.OfType<IDeliverable>().ForEach(x => documentEDocsDelivery.SaveCopyToEDocs(x, name, title));
					}
					else
					{
						using (var task = new PrintTask(documentCommand))
						{
							task.Add(docPack);
							var instructions = new DeliveryInstructions(docPack, new Enterprise.DocumentEngine.DeliveryMethods.FactoryStrategy.PopulateButDoNotSave(factory));
							instructions.Destination = DeliveryInstructionDestination.DocManager;

							if (!string.IsNullOrEmpty(language))
							{
								docPack.SupportsLanguageSelection = true;
								docPack.Language = language;
								instructions.Language = language;
							}

							task.Run(instructions);
						}
					}
				}
			}
		}
	}
}
