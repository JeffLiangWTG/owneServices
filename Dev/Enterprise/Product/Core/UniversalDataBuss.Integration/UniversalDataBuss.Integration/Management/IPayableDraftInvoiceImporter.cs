using Enterprise.Messaging.Integration;

namespace Enterprise.UniversalDataBuss.Integration;

public interface IPayableDraftInvoiceImporter
{
	bool ImportPayableDraftInvoice(IEDIMessage message, ITopLevelDataObject dataObject, IXmlImportLogger logger, IUniversalObjectFactory factory);
	IKeysResult GetKeysForBlockingParallelImport(IEDIMessage message, ITopLevelDataObject dataObject, IXmlImportLogger logger, IUniversalObjectFactory factory);
}
