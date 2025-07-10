using CargoWise.EntityFramework;
using Enterprise.DocumentEngineCore.DocWrappers;

namespace Enterprise.DocumentWrappers
{
	public class DocNettingClearingJournalCollection : DocumentWrapperCollection<DocNettingClearingJournal>
	{
		protected DocNettingClearingJournalCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public static DocNettingClearingJournalCollection New(BusinessObjectFactory factory)
		{
			return new DocNettingClearingJournalCollection(factory);
		}
	}
}
