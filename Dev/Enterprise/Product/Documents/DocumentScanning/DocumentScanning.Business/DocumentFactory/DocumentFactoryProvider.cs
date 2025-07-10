using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.DocumentScanning.Business
{
	public class DocumentFactoryProvider : IDocumentFactoryProvider
	{
		public DocumentFactory GetFactory(BusinessObjectFactory factoryForEverythingExceptEDocs)
		{
			return new DbBackendDocumentFactory(factoryForEverythingExceptEDocs != null && !(factoryForEverythingExceptEDocs is NumberedBusinessObjectFactory) ? factoryForEverythingExceptEDocs : new BusinessObjectFactory());
		}

		IDocumentFactory IDocumentFactoryProvider.GetFactory(BusinessObjectFactory factoryForEverythingExceptEDocs)
		{
			return this.GetFactory(factoryForEverythingExceptEDocs);
		}
	}
}
