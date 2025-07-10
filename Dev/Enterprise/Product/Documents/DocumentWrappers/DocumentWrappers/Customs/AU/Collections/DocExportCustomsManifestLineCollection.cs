
using CargoWise.EntityFramework;
using Enterprise.DocumentEngineCore.DocWrappers;

namespace Enterprise.DocumentWrappers.Customs.AU
{
	public class DocExportCustomsManifestLineCollection : DocumentWrapperCollection<DocExportCustomsManifestLine>
	{
		public DocExportCustomsManifestLineCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public DocExportCustomsManifestLineCollection(BusinessObjectCollection collection)
			: base(collection, collection.Factory)
		{
		}
	}
}
