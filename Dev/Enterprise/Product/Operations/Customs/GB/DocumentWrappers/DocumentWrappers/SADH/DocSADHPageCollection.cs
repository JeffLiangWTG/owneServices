using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.GB.DocumentWrappers
{
	public class DocSADHPageCollection : Enterprise.DocumentWrappers.Customs.EU.DocSADHPageCollection
	{
		public DocSADHPageCollection(ICusEntryLineCollection<EU.Business.Declaration.CusEntryLine> collection, BusinessObjectFactory factory)
			: base(collection, factory)
		{
		}

		protected DocSADHPageCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override Enterprise.DocumentWrappers.Customs.EU.DocSADHPage NewFirstPage(BusinessObjectFactory factory, EU.Business.Declaration.CusEntryLine entryLine)
		{
			return DocSADHPage.New(factory, entryLine);
		}

		protected override Enterprise.DocumentWrappers.Customs.EU.DocSADHPage NewPageWithUpToThreeLines(BusinessObjectFactory factory,
			ICusEntryLineCollection<EU.Business.Declaration.CusEntryLine> collection, int startLineIndex)
		{
			return DocSADHPage.New(factory, collection, startLineIndex);
		}
	}
}
