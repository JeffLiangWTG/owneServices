using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business.Declaration;
using EUCusEntryLineCollection = Enterprise.Customs.Business.ICusEntryLineCollection<Enterprise.Customs.EU.Business.Declaration.CusEntryLine>;

namespace Enterprise.DocumentWrappers.Customs.EU
{
	public class DocSADHPageCollection : DocBaseWrapperCollection<DocSADHPage>
	{
		public DocSADHPageCollection(EUCusEntryLineCollection collection, BusinessObjectFactory factory)
			: base(factory)
		{
			if (collection.Count > 0)
			{
				GenerateDocSADHCollection(collection, factory);
			}
		}

		protected virtual void GenerateDocSADHCollection(EUCusEntryLineCollection collection, BusinessObjectFactory factory)
		{
			Add(NewFirstPage(factory, collection[0]));
			AddLinePages(collection, factory);
		}

		protected DocSADHPageCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected virtual void AddLinePages(EUCusEntryLineCollection collection, BusinessObjectFactory factory)
		{
			for (int index = 1; index < collection.Count; index += 3)
			{
				Add(NewPageWithUpToThreeLines(factory, collection, index));
			}
		}

		protected virtual DocSADHPage NewFirstPage(BusinessObjectFactory factory, CusEntryLine entryLine) => DocSADHPage.New(factory, entryLine);

		protected virtual DocSADHPage NewPageWithUpToThreeLines(BusinessObjectFactory factory, EUCusEntryLineCollection collection, int startLineIndex) => DocSADHPage.New(factory, collection, startLineIndex);
	}
}
