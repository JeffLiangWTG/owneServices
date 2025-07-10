using Enterprise.Accounting.Business.GeneralLedger.GLJournals;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.Module
{
	public class GLJournalControllerImport : GLJournalController
	{
		public GLJournalControllerImport()
		{
		}

		public IZForm ShowImportedDataForm(GLJournal journal)
		{
			IZForm formToReturn = ShowFormForNewEntity(journal);
			if (formToReturn != null)
			{
				formToReturn.DisplayMode = ZArchitecture.Core.ODisplayMode.Edit;
			}

			return formToReturn;
		}
	}
}
