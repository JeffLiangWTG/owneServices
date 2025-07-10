using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.IE.DataTransfer.Reader;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.IE.DataTransfer.Universal
{
	public class UniversalCustomsDataObjectProvider : EU.DataTransfer.Universal.UniversalCustomsDataObjectProvider
	{
		protected override ITopLevelDataObjectReader GetNewJobDeclarationDataObjectReaderCore(Shipment declarationDataObject, IXmlImportLogger logger, UniversalObjectFactory factory, ForwardingShipment shipment)
		{
			return new JobDeclarationDataObjectReader(declarationDataObject, logger, factory, shipment);
		}

		protected override ICodeDescriptionPairList TableSpecificCusReferenceTypeListCore(ZString tableCode, string dataContext) => new CusReferenceTypeListProvider().TableSpecificCusReferenceTypeList(tableCode, dataContext);
	}
}
