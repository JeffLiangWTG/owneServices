using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using UniversalShipment = Enterprise.Customs.DataTransfer.Universal;

namespace Enterprise.Customs.DE.DataTransfer.Universal
{
	public class UniversalCustomsDataObjectProvider : EU.DataTransfer.Universal.UniversalCustomsDataObjectProvider, UniversalShipment.IUniversalCustomsDataObjectProvider
	{
		protected override ITopLevelDataObjectWriter GetNewDeclarationDataObjectWriterCore(IDataWritingManager manager) => new DeclarationDataObjectWriter(manager);

		protected override ICodeDescriptionPairList TableSpecificCusReferenceTypeListCore(ZString tableCode, string dataContext) => new CusReferenceTypeListProvider().TableSpecificCusReferenceTypeList(tableCode, dataContext);

		protected override ITopLevelDataObjectReader GetNewJobDeclarationDataObjectReaderCore(Shipment declarationDataObject,
			IXmlImportLogger logger, UniversalObjectFactory factory, ForwardingShipment shipment)
		{
			return new JobDeclarationDataObjectReader(declarationDataObject, logger, factory, shipment);
		}
	}
}
