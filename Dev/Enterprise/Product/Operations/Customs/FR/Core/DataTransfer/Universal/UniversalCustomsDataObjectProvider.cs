using Enterprise.Freight.Forwarding.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.FR.DataTransfer.Universal;

public class UniversalCustomsDataObjectProvider : EU.DataTransfer.Universal.UniversalCustomsDataObjectProvider
{
	protected override ITopLevelDataObjectWriter GetNewDeclarationDataObjectWriterCore(IDataWritingManager manager) => new DeclarationDataObjectWriter(manager);

	protected override ITopLevelDataObjectReader GetNewJobDeclarationDataObjectReaderCore(Shipment declarationDataObject, IXmlImportLogger logger, UniversalObjectFactory factory, ForwardingShipment shipment) => new JobDeclarationDataObjectReader(declarationDataObject, logger, factory, shipment);
}
