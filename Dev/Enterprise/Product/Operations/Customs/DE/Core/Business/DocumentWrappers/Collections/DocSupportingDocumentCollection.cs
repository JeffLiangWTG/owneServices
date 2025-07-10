using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.DocumentEngineCore.DocWrappers;

namespace Enterprise.Customs.DE.Business.DocumentWrappers
{
	public class DocSupportingDocumentCollection : DocumentWrapperCollection<DocSupportingDocument>
	{
		public DocSupportingDocumentCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public DocSupportingDocumentCollection(IEnumerable<SupportingDocument> collectionSource, BusinessObjectFactory factoryToWrap)
			: base(collectionSource, factoryToWrap)
		{
		}
	}
}
