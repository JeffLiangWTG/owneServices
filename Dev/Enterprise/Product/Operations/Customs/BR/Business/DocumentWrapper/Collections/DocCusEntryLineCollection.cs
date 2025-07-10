
using CargoWise.EntityFramework;
using Enterprise.DocumentEngineCore.DocWrappers;

namespace Enterprise.Customs.BR.Business
{
	public class DocCusEntryLineCollection : DocumentWrapperCollection
	{
		public DocCusEntryLineCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public DocCusEntryLineCollection(Customs.Business.ICusEntryLineCollection<CusEntryLine> collectionSource, BusinessObjectFactory factoryToWrap)
			: base(collectionSource, factoryToWrap)
		{
		}

		public new DocCusEntryLine this[int index]
		{
			get { return (DocCusEntryLine)base[index]; }
		}
	}
}
