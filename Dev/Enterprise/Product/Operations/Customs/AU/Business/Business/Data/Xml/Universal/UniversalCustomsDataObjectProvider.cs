using System.Collections.Generic;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.Customs.Common.AU;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using UniversalShipment = Enterprise.Customs.DataTransfer.Universal;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class UniversalCustomsDataObjectProvider : IUniversalCustomsDataObjectProvider
	{
		#region IUniversalCustomsDataObjectProvider Members

		public ICodeDescriptionPairList TableSpecificCusSupportingInfoTypeList(ZString tableCode, string dataContext = "")
		{
			return null;
		}

		public ICodeDescriptionPairList TableSpecificCusReferenceTypeList(ZString tableCode, string dataContext) => null;

		public ICodeDescriptionPairList TableSpecificCusAddInfoTypeList(ZString tableCode, string dataContext = "")
		{
			ICodeDescriptionPairList result = null;
			switch (tableCode)
			{
				case JobComInvoiceLineSchema.Constants.Prefix:
					result = GetCusAddInfoTypeListForJobComInvoiceLine();
					break;
			}
			return result;
		}

		public ICodeDescriptionPairList TableSpecificAddInfoGroupTypesNeedInsertedToOtherTableList(ZString tableCode, string dataContext)
		{
			return null;
		}

		public ICodeDescriptionPairList TableSpecificCusCodeDataCodeList(ZString tableCode, string dataContext = "")
		{
			ICodeDescriptionPairList result = null;
			switch (tableCode)
			{
				case JobComInvoiceLineSchema.Constants.Prefix:
					result = GetCusCodeDataListForJobComInvoiceLine();
					break;
			}
			return result;
		}

		public ICodeDescriptionPairList TableSpecificCusCodeDataTypeList(ZString tableCode, string dataContext = "")
		{
			ICodeDescriptionPairList result = null;
			switch (tableCode)
			{
				case JobComInvoiceLineSchema.Constants.Prefix:
					result = GetCusCodeDataListForJobComInvoiceLine();
					break;
			}
			return result;
		}

		public ITopLevelDataObjectWriter GetNewDeclarationDataObjectWriter(IDataWritingManager manager)
		{
			return new DeclarationDataObjectWriter(manager);
		}

		public ITopLevelDataObjectReader GetNewJobDeclarationDataObjectReader(Shipment declarationDataObject, IXmlImportLogger logger, UniversalObjectFactory factory, ForwardingShipment shipment)
		{
			return new JobDeclarationDataObjectReader(declarationDataObject, logger, factory, shipment);
		}

		public IEnumerable<ITopLevelDataObjectReader> GetNewAirManifestDataObjectReaders(Shipment mawbDataObject, Shipment shipment, IXmlImportLogger logger, UniversalObjectFactory factory, bool singleHAWBCheck)
		{
			yield return new CusMAWBDataObjectReader(mawbDataObject, shipment, logger, factory, singleHAWBCheck);
		}

		public ITopLevelDataObjectWriter GetNewAirManifestDataObjectWriter(IDataWritingManager manager)
		{
			return new CusMAWBDataObjectWriter(manager);
		}

		public ITopLevelDataObjectWriter GetNewAirManifestLineDataObjectWriter(IDataWritingManager manager, UniversalShipment.AirManifest.AirManifestDataObjectWriterHelper helper)
		{
			return new CusHAWBDataObjectWriter(manager, helper == null ? null : new AirManifestDataObjectWriterHelper(helper));
		}

		public UniversalDataObjectReaderHelper GetNewUniversalDataObjectReaderHelper(UniversalObjectFactory factory, string sourceCountryCode, string dataProviderForCodeMapping = null)
		{
			return new UniversalDataObjectReaderHelper(factory, Core.Constants.CountryCodes.Australia, sourceCountryCode, dataProviderForCodeMapping);
		}

		public ITopLevelDataObjectReader GetNewStandaloneCommercialInvoiceDataObjectReader(Shipment shipmentDataObject, UniversalDataBuss.DataObjects.Universal.Customs.CommercialInvoiceHeader invoiceDataObject, IXmlImportLogger logger, UniversalObjectFactory factory)
		{
			return new StandaloneCommercialInvoiceDataObjectReader(shipmentDataObject, invoiceDataObject, logger, factory);
		}

		public UniversalShipment.StandaloneCommercialInvoiceDataObjectWriter GetNewStandaloneCommercialInvoiceDataObjectWriter(IDataWritingManager manager)
		{
			return new StandaloneCommercialInvoiceDataObjectWriter(manager);
		}

		public IEnumerable<ITopLevelDataObjectReader> GetNewCusSCAOceanBillDataObjectReaders(Shipment dataObject, Shipment subShipment, IXmlImportLogger logger, UniversalObjectFactory factory)
		{
			yield return new CMRCusSCAOceanBillDataObjectReader(dataObject, subShipment, logger, factory);
		}

		public ITopLevelDataObjectWriter GetNewCusSCAOceanBillDataObjectWriter(IDataWritingManager manager)
		{
			return new CMRCusSCAOceanBillDataObjectWriter(manager);
		}

		#endregion

		#region Implementation

		static CodeDescriptionPairList GetCusAddInfoTypeListForJobComInvoiceLine()
		{
			var result = new CodeDescriptionPairList();
			result.AddPair(CusAddInfoTypeAttribute.Codes.AURFPNumber, Res.GetString("3DA4AB90-4C24-4CAC-ADB0-E76FD635376A", "RFP Numbers"));
			return result;
		}

		static CodeDescriptionPairList GetCusCodeDataListForJobComInvoiceLine()
		{
			var result = new CodeDescriptionPairList();
			result.AddPair(CusCodeDataTypeList.Codes.ICSPermit, CusCodeDataTypeList.Descriptions.ICSPermit);
			return result;
		}

		#endregion
	}
}
