
using CargoWise.EntityFramework;
using Enterprise.DocumentEngineCore.DocWrappers;

namespace Enterprise.DocumentWrappers.Customs.AU
{
	public class DocCTOCusHAWBCollection : DocumentWrapperCollection<DocCTOCusHAWB>
	{
		public DocCTOCusHAWBCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public DocCTOCusHAWBCollection(BusinessObjectCollection collection)
			: base(collection, collection.Factory)
		{
		}
	}
}
