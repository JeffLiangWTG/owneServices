using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business.Declaration;
using EUCusEntryLineCollection = Enterprise.Customs.Business.ICusEntryLineCollection<Enterprise.Customs.EU.Business.Declaration.CusEntryLine>;

namespace Enterprise.DocumentWrappers.Customs.EU
{
	public class DocSADHLineCollection : DocBaseWrapperCollection<DocSADHLine>
	{
		public DocSADHLineCollection(EUCusEntryLineCollection collection, BusinessObjectFactory factory)
			: base(factory)
		{
			foreach (CusEntryLine entryLine in collection)
			{
				Add(DocSADHLine.New(entryLine, factory));
			}
		}

		protected DocSADHLineCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}
	}
}
