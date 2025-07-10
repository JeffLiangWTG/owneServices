using CargoWise.EntityFramework;
using Enterprise.DocumentEngineCore.DocWrappers;
using CusEntryLineCollection = Enterprise.Customs.Business.ICusEntryLineCollection<Enterprise.Customs.NZ.Business.Declaration.CusEntryLine>;

namespace Enterprise.DocumentWrappers.Customs.NZ.FormalEntry
{
	public class DocCusEntryLineCollection : DocumentWrapperCollection
	{
		public DocCusEntryLineCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public DocCusEntryLineCollection(CusEntryLineCollection collectionSource, BusinessObjectFactory factoryToWrap)
			: base(collectionSource, factoryToWrap)
		{
			if (Count == 0)
			{
				Add(DocCusEntryLine.New(Factory));
			}
		}

		public new DocCusEntryLine this[int index]
		{
			get { return (DocCusEntryLine)base[index]; }
		}
	}
}
