using CargoWise.EntityFramework;
using Enterprise.DocumentEngineCore.DocWrappers;

namespace Enterprise.DocumentWrappers
{
	public
	class DocChinaJournalListingLineCollection : DocumentWrapperCollection
	{
		public DocChinaJournalListingLineCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public new DocChinaJournalListingLine this[int index]
		{
			get { return (DocChinaJournalListingLine)base[index]; }
		}
	}
}
