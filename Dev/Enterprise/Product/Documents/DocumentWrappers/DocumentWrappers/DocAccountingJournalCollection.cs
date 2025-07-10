using CargoWise.EntityFramework;
using Enterprise.DocumentEngineCore.DocWrappers;

namespace Enterprise.DocumentWrappers
{
	public class DocAccountingJournalCollection : DocumentWrapperCollection
	{
		public DocAccountingJournalCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public new DocAccountingJournal this[int index]
		{
			get { return (DocAccountingJournal)base[index]; }
		}
	}
}
