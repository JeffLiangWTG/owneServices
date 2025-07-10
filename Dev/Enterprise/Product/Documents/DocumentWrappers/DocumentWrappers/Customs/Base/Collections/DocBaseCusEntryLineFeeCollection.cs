using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.DocumentEngineCore.DocWrappers;

namespace Enterprise.DocumentWrappers.Customs.Base
{
	public class DocBaseCusEntryLineFeeCollection : DocumentWrapperCollection
	{
		public DocBaseCusEntryLineFeeCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public DocBaseCusEntryLineFeeCollection(ICusEntryLineFeeCollection<CusEntryLineFee, CusEntryLine> collectionSource, BusinessObjectFactory factoryToWrap)
			: base(collectionSource, factoryToWrap)
		{
		}

		public new DocBaseCusEntryLineFee this[int index]
		{
			get { return (DocBaseCusEntryLineFee)base[index]; }
		}
	}
}
