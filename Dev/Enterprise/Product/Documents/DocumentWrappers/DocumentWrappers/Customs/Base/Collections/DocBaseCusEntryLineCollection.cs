using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.DocumentEngineCore.DocWrappers;

namespace Enterprise.DocumentWrappers.Customs.Base
{
	public class DocBaseCusEntryLineCollection : DocumentWrapperCollection
	{
		public DocBaseCusEntryLineCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public DocBaseCusEntryLineCollection(ICusEntryLineCollection<CusEntryLine> collectionSource, BusinessObjectFactory factoryToWrap)
			: base(collectionSource, factoryToWrap)
		{
		}

		public new DocBaseCusEntryLine this[int index]
		{
			get { return (DocBaseCusEntryLine)base[index]; }
		}
	}
}
