using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.DocumentEngineCore.DocWrappers;

namespace Enterprise.DocumentWrappers.Customs.Base
{
	public class DocBaseCusEntryHeaderChargesCollection : DocumentWrapperCollection
	{
		public DocBaseCusEntryHeaderChargesCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public DocBaseCusEntryHeaderChargesCollection(ICusEntryHeaderChargesCollection<CusEntryHeaderCharges> collectionSource, BusinessObjectFactory factoryToWrap)
			: base(collectionSource, factoryToWrap)
		{
		}

		public new DocBaseCusEntryHeaderCharges this[int index] => (DocBaseCusEntryHeaderCharges)base[index];
	}
}
