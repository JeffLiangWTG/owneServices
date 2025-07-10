using CargoWise.EntityFramework;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.DocumentWrappers.Customs.Base;

namespace Enterprise.DocumentWrappers.Customs.AU
{
	public class DocCusEntryHeaderChargesCollection : DocBaseCusEntryHeaderChargesCollection
	{
		public DocCusEntryHeaderChargesCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public DocCusEntryHeaderChargesCollection(Enterprise.Customs.Business.CusEntryHeaderChargesCollection<CusEntryHeaderCharges> collectionSource, BusinessObjectFactory factoryToWrap)
			: base(collectionSource, factoryToWrap)
		{
		}

		public new DocCusEntryHeaderCharges this[int index] => (DocCusEntryHeaderCharges)Elements[index];
	}
}
