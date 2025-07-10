using Enterprise.Client.UPE.Business;
using Enterprise.Customs.Business;
using Enterprise.DocumentEngine;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.UPE.GUI
{
	public class EntryPrintDocumentHelper
	{
		public void Run(UPECusHAWB hAWB)
		{
			if (hAWB == null)
			{
				Globals.Message.ShowError("You must select a finance item with a formal declaration in the grid");
			}
			else if (hAWB.Declaration == null)
			{
				Globals.Message.ShowError("The selected finance item doesn't have a declaration attached");
			}
			else
			{
				ShowEntryPrintDocument(hAWB.Declaration);
			}
		}

		void ShowEntryPrintDocument(BaseJobDeclaration declaration)
		{
			DocumentCommand entryPrintMenuItem = new UPEDocumentMenuItemLoader(declaration.Factory).LoadPortraitEntryPrint();
			using (DocumentPack pack = new DocumentPack(entryPrintMenuItem, declaration, null, null))
			{
				PrintTask task = new PrintTask();
				task.Add(pack);
				RunPrintTask(task);
			}
		}

		protected virtual void RunPrintTask(PrintTask task)
		{
			task.Run(Env.Security.None);
		}
	}
}
