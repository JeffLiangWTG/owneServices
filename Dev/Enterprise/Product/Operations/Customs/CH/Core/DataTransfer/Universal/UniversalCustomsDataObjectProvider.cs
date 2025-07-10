using System.Collections.Generic;
using System.Linq;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.Customs.DataTransfer.Universal.AirManifest;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.CH.DataTransfer;

public class UniversalCustomsDataObjectProvider : IUniversalCustomsDataObjectProvider
{
	public ICodeDescriptionPairList TableSpecificCusSupportingInfoTypeList(ZString tableCode, string dataContext)
		=> new CusSupportingInfoTypeListProvider().TableSpecificCusSupportingInfoTypeList(tableCode, dataContext);

	public ICodeDescriptionPairList TableSpecificCusAddInfoTypeList(ZString tableCode, string dataContext) => null;

	public ICodeDescriptionPairList TableSpecificAddInfoGroupTypesNeedInsertedToOtherTableList(ZString tableCode,
		string dataContext) => null;

	public ICodeDescriptionPairList TableSpecificCusCodeDataTypeList(ZString tableCode, string dataContext) => null;

	public ICodeDescriptionPairList TableSpecificCusCodeDataCodeList(ZString tableCode, string dataContext) => null;

	public ICodeDescriptionPairList TableSpecificCusReferenceTypeList(ZString tableCode, string dataContext) => null;

	public ITopLevelDataObjectReader GetNewJobDeclarationDataObjectReader(Shipment declarationDataObject, IXmlImportLogger logger, UniversalObjectFactory factory, ForwardingShipment shipment)
		=> new DeclarationDataObjectReader(declarationDataObject, logger, factory, shipment);

	public UniversalDataObjectReaderHelper GetNewUniversalDataObjectReaderHelper(UniversalObjectFactory factory, string sourceCountryCode, string dataProviderForCodeMapping = null) =>
		new UniversalDataObjectReaderHelper(factory, Enterprise.Core.Constants.CountryCodes.Switzerland, sourceCountryCode, dataProviderForCodeMapping);

	public ITopLevelDataObjectWriter GetNewDeclarationDataObjectWriter(IDataWritingManager manager) => new DeclarationDataObjectWriter(manager);

	public IEnumerable<ITopLevelDataObjectReader> GetNewAirManifestDataObjectReaders(Shipment mawbDataObject,
		Shipment subShipment, IXmlImportLogger logger, UniversalObjectFactory factory, bool singleHAWBCheck) => Enumerable.Empty<ITopLevelDataObjectReader>();

	public ITopLevelDataObjectWriter GetNewAirManifestDataObjectWriter(IDataWritingManager manager) => null;

	public ITopLevelDataObjectWriter GetNewAirManifestLineDataObjectWriter(IDataWritingManager manager,
		AirManifestDataObjectWriterHelper helper) => null;

	public IEnumerable<ITopLevelDataObjectReader> GetNewCusSCAOceanBillDataObjectReaders(Shipment dataObject,
		Shipment subShipment, IXmlImportLogger logger, UniversalObjectFactory factory) => null;

	public ITopLevelDataObjectWriter GetNewCusSCAOceanBillDataObjectWriter(IDataWritingManager manager) => null;

	public ITopLevelDataObjectReader GetNewStandaloneCommercialInvoiceDataObjectReader(Shipment shipmentDataObject,
		CommercialInvoiceHeader invoiceDataObject, IXmlImportLogger logger, UniversalObjectFactory factory) => null;

	public StandaloneCommercialInvoiceDataObjectWriter GetNewStandaloneCommercialInvoiceDataObjectWriter(
		IDataWritingManager manager) => null;
}
