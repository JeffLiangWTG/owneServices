using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngine.DocumentDelivery;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.GB.Business
{
	public class PrinterFromEdiMessageHelper
	{
		public PrinterFromEdiMessageHelper(BusinessObjectFactory businessObjectFactory, ILogger logger)
		{
			this.businessObjectFactory = businessObjectFactory;
			this.logger = logger;
		}

		public void PrintToEdocsAndPaper(ZGuid guidOfMenuItemYouWantToPrint, IBranchProvider branchProviderForDeterminingPrinter, IDocumentSupportable documentSupportableBizOFromWhichEdocWillHang, IRegistryItem registryForPaperPrinter, bool printToEdocsToo = true, DocManagerInfo forcedParent = null)
		{
			var docCommand = GetDocumentCommandThatWeWillFireAsIfUserClickedIt(documentSupportableBizOFromWhichEdocWillHang, guidOfMenuItemYouWantToPrint);
			var silentPrinter = new SilentDocumentPrinter(businessObjectFactory, documentSupportableBizOFromWhichEdocWillHang, docCommand);
			pkOfSelectedPaperPrinter = ZGuid.Empty;
			var numberOfPaperCopies = 0;
			if (branchProviderForDeterminingPrinter != null)
			{
				pkOfSelectedPaperPrinter = new ZGuid((Guid)registryForPaperPrinter.GetFallBackValueAtAllLevels(branchProviderForDeterminingPrinter.Branch.Company.PK.ToGuid(), branchProviderForDeterminingPrinter.Branch.PK.ToGuid(), Guid.Empty));
				if (PrinterInRegistryIsBad(pkOfSelectedPaperPrinter, registryForPaperPrinter, branchProviderForDeterminingPrinter, documentSupportableBizOFromWhichEdocWillHang.DocumentSupporter.BusinessObject.HumanReadableName))
				{
					pkOfSelectedPaperPrinter = ZGuid.Empty;
				}
				numberOfPaperCopies = pkOfSelectedPaperPrinter.IsEmpty ? 0 : 1;
			}
			try
			{
				silentPrinter.Print(pkOfSelectedPaperPrinter, numberOfPaperCopies, printToEdocsToo, true, forcedParent);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				ErrorReporter.ReportOnce("BGB-Printing",
									   "Could not produce print for " + docCommand.SU_MenuName + " for " + documentSupportableBizOFromWhichEdocWillHang.DocumentSupporter.BusinessObject.HumanReadableName,
									   ex);
			}
		}

		DocumentCommand GetDocumentCommandThatWeWillFireAsIfUserClickedIt(IDocumentSupportable supporter, ZGuid guidOfMenuItemYouWantToPrint)
		{
			DocumentCommand result = null;
			if (supporter.DocumentSupporter != null)
			{
				var filter = new DocumentZQuery();
				filter.AddToFilter(StmMenuItemSchema.PK, guidOfMenuItemYouWantToPrint); // PK of the menu option
				result = businessObjectFactory.LoadTop1<DocumentCommand>(filter);
			}
			return result;
		}

		bool PrinterInRegistryIsBad(ZGuid pkOfSelectedPaperPrinter, IRegistryItem registryForPaperPrinter, IBranchProvider branchProviderForDeterminingPrinter, string nameOfBizO)
		{
			bool isBad = false;
			var stmPrintQueue = businessObjectFactory.Load<StmPrintQueue>(pkOfSelectedPaperPrinter);
			if (stmPrintQueue == null)
			{
				Log("Printer does not exist", registryForPaperPrinter, branchProviderForDeterminingPrinter, nameOfBizO);
				isBad = true;
			}
			else if (!stmPrintQueue.SQ_AllowPrinting)
			{
				Log(string.Format("[{0}] does not allow printing", stmPrintQueue.SQ_DisplayName), registryForPaperPrinter, branchProviderForDeterminingPrinter, nameOfBizO);
				isBad = true;
			}
			return isBad;
		}

		void Log(string p, IRegistryItem registryForPaperPrinter, IBranchProvider branchProviderForDeterminingPrinter, string nameOfBizO)
		{
			if (logger != null)
			{
				var text = string.Format("The printer named in the registry for printing documents of type [{0}] for branch [{1}] is not valid. {2}. To fix, open the registry item [{3}/{0}] and select a valid printer. Failed to print to paper for [{4}], printing to eDocs only (subject to DocType's options).",
					registryForPaperPrinter.Caption, branchProviderForDeterminingPrinter.Branch.GB_Code, p, registryForPaperPrinter.Category, nameOfBizO);
				logger.Log(LogType.Warning, text);
			}
		}

		protected ZGuid pkOfSelectedPaperPrinter;
		readonly BusinessObjectFactory businessObjectFactory;
		readonly ILogger logger;
	}
}
