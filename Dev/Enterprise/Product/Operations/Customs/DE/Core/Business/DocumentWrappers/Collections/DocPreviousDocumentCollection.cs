using System.Collections;
using CargoWise.EntityFramework;
using Enterprise.DocumentEngineCore.DocWrappers;

namespace Enterprise.Customs.DE.Business.DocumentWrappers
{
	public class DocPreviousDocumentCollection : DocumentWrapperCollection<DocPreviousDocument>
	{
		public DocPreviousDocumentCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public DocPreviousDocumentCollection(IEnumerable collectionSource, BusinessObjectFactory factoryToWrap)
			: base(collectionSource, factoryToWrap)
		{
		}
	}
}
