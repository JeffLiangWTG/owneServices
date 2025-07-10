using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.GB.DocumentWrappers
{
	public class DocSADHLineCollection : Enterprise.DocumentWrappers.Customs.EU.DocSADHLineCollection
	{
		public DocSADHLineCollection(ICusEntryLineCollection<EU.Business.Declaration.CusEntryLine> collection, BusinessObjectFactory factory)
			: base(factory)
		{
			foreach (var entryLine in collection)
			{
				Add(DocSADHLine.New(entryLine, factory));
			}
		}
	}
}
