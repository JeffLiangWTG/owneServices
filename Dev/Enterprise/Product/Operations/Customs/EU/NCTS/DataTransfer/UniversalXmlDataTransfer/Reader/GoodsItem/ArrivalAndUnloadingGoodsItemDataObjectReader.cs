using CargoWise.Types;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.EU.NCTS.DataTransfer.Phase4
{
	public class ArrivalAndUnloadingGoodsItemDataObjectReader : GoodsItemDataObjectReader<NctsArrivalAndUnloadingCargoDesc>
	{
		public ArrivalAndUnloadingGoodsItemDataObjectReader(Shipment moveHeaderDataObject, IXmlImportLogger logger, UniversalDataObjectReaderHelper helper, NctsHeader header, NctsCommonMovementHeader moveHeader, CommercialInvoiceLine commercialInvoiceLine, ZShort currentLineNumber)
			: base(moveHeaderDataObject, logger, helper, header, moveHeader, commercialInvoiceLine, currentLineNumber)
		{
		}
	}
}
