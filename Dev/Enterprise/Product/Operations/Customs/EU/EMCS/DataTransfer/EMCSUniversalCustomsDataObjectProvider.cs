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
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using CusCodeDataTypeList = Enterprise.Customs.EU.Business.CusCodeDataTypeList;

namespace Enterprise.Customs.EU.EMCS.DataTransfer
{
	public class EMCSUniversalCustomsDataObjectProvider : IUniversalCustomsDataObjectProvider
	{
		public ICodeDescriptionPairList TableSpecificCusSupportingInfoTypeList(ZString tableCode, string dataContext = "")
		{
			return new EMCSCusSupportingInfoTypeListProvider().TableSpecificCusSupportingInfoTypeList(tableCode);
		}

		public ICodeDescriptionPairList TableSpecificCusReferenceTypeList(ZString tableCode, string dataContext) => null;

		public ICodeDescriptionPairList TableSpecificCusCodeDataTypeList(ZString tableCode, string dataContext)
		{
			ICodeDescriptionPairList result;
			switch (tableCode)
			{
				case JobDeclarationSchema.Constants.Prefix:
					result = GetTypeListForJobDeclaration();
					break;
				case JobComInvoiceLineSchema.Constants.Prefix:
					result = GetTypeListForJobComInvoiceLine();
					break;
				default:
					result = null;
					break;
			}
			return result;
		}

		public ICodeDescriptionPairList TableSpecificAddInfoGroupTypesNeedInsertedToOtherTableList(ZString tableCode, string dataContext)
		{
			return null;
		}

		static CodeDescriptionPairList GetTypeListForJobDeclaration()
		{
			var result = new CodeDescriptionPairList();
			result.AddPair(CusCodeDataTypeList.Codes.OfficeCode, CusCodeDataTypeList.Descriptions.OfficeCode);
			return result;
		}

		static CodeDescriptionPairList GetTypeListForJobComInvoiceLine()
		{
			var result = new CodeDescriptionPairList();
			result.AddPair(Business.CusCodeDataTypeList.Codes.WineCode, Business.CusCodeDataTypeList.Descriptions.WineCode);
			return result;
		}

		protected ITopLevelDataObjectWriter GetNewDeclarationDataObjectWriterCore(IDataWritingManager manager) => new EMCSDeclarationDataObjectWriter(manager);

		protected ITopLevelDataObjectReader GetNewJobDeclarationDataObjectReaderCore(Shipment declarationDataObject, IXmlImportLogger logger, UniversalObjectFactory factory, ForwardingShipment shipment)
		{
			return new EMCSDeclarationDataObjectReader(declarationDataObject, logger, factory, shipment);
		}

		public UniversalDataObjectReaderHelper GetNewUniversalDataObjectReaderHelper(UniversalObjectFactory factory, string sourceCountryCode, string dataProviderForCodeMapping = null)
		{
			return new EMCSUniversalDataObjectReaderHelper(factory, sourceCountryCode, sourceCountryCode, dataProviderForCodeMapping);
		}

		public ICodeDescriptionPairList TableSpecificCusAddInfoTypeList(ZString tableCode, string dataContext) => null;

		public ICodeDescriptionPairList TableSpecificCusCodeDataCodeList(ZString tableCode, string dataContext) => null;

		public ITopLevelDataObjectReader GetNewJobDeclarationDataObjectReader(Shipment declarationDataObject, IXmlImportLogger logger, UniversalObjectFactory factory, ForwardingShipment shipment)
		{
			return new EMCSDeclarationDataObjectReader(declarationDataObject, logger, factory, shipment);
		}

		public ITopLevelDataObjectWriter GetNewDeclarationDataObjectWriter(IDataWritingManager manager) => new EMCSDeclarationDataObjectWriter(manager);

		public IEnumerable<ITopLevelDataObjectReader> GetNewAirManifestDataObjectReaders(Shipment mawbDataObject, Shipment subShipment, IXmlImportLogger logger, UniversalObjectFactory factory, bool singleHAWBCheck) => Enumerable.Empty<ITopLevelDataObjectReader>();

		public ITopLevelDataObjectWriter GetNewAirManifestDataObjectWriter(IDataWritingManager manager) => null;

		public ITopLevelDataObjectWriter GetNewAirManifestLineDataObjectWriter(IDataWritingManager manager, AirManifestDataObjectWriterHelper helper) => null;

		public ITopLevelDataObjectReader GetNewStandaloneCommercialInvoiceDataObjectReader(Shipment shipmentDataObject, CommercialInvoiceHeader invoiceDataObject, IXmlImportLogger logger, UniversalObjectFactory factory) => null;

		public StandaloneCommercialInvoiceDataObjectWriter GetNewStandaloneCommercialInvoiceDataObjectWriter(IDataWritingManager manager) => null;

		public IEnumerable<ITopLevelDataObjectReader> GetNewCusSCAOceanBillDataObjectReaders(Shipment dataObject, Shipment subShipment, IXmlImportLogger logger, UniversalObjectFactory factory)
		{
			return System.Linq.Enumerable.Empty<ITopLevelDataObjectReader>();
		}

		public ITopLevelDataObjectWriter GetNewCusSCAOceanBillDataObjectWriter(IDataWritingManager manager) => null;
	}
}
