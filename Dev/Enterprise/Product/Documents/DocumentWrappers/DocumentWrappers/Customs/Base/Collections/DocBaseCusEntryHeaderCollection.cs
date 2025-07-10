using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.DocumentEngineCore.DocWrappers;

namespace Enterprise.DocumentWrappers.Customs.Base
{
	public abstract class DocBaseCusEntryHeaderCollection : DocumentWrapperCollection
	{
		protected DocBaseCusEntryHeaderCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected DocBaseCusEntryHeaderCollection(ICusEntryHeaderCollection<CusEntryHeader> collectionSource, BusinessObjectFactory factoryToWrap)
			: base(collectionSource, factoryToWrap)
		{
		}

		public new DocBaseCusEntryHeader this[int index] => (DocBaseCusEntryHeader)base[index];
	}
}
