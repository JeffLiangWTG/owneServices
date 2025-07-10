using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Customs.EU.NCTS.DataTransfer
{
	public interface IUniversalNCTSDataObjectProvider
	{
		ITopLevelDataObjectReader GetNewNctsHeaderDataObjectReader(UniversalShipment declarationDataObject, IXmlImportLogger logger, UniversalObjectFactory factory, ZString applicationCode);
		ITopLevelDataObjectWriter GetNewNctsHeaderDataObjectWriter(IDataWritingManager manager, ZString applicationCode);
	}
}
