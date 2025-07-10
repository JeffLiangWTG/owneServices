using System.Collections.Generic;
using System.Linq;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Common.BR;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.Customs.DataTransfer.Universal.AirManifest;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.BR.DataTransfer.Universal
{
	public class UniversalCustomsDataObjectProvider : IUniversalCustomsDataObjectProvider
	{
		#region Implementation of IUniversalCustomsDataObjectProvider

		public ICodeDescriptionPairList TableSpecificCusSupportingInfoTypeList(ZString tableCode, string dataContext = "")
		{
			var result = new CodeDescriptionPairList();
			switch (tableCode)
			{
				case JobComInvoiceHeaderSchema.Constants.Prefix:
					result.AddPair(CusSupportingInfoTypeList.Codes.ExchangeHedge, CusSupportingInfoTypeList.Descriptions.ExchangeHedge);
					break;
				case JobComInvoiceLineSchema.Constants.Prefix:
					result.AddPair(CusSupportingInfoTypeList.Codes.ElectronicLogisticInvoice, CusSupportingInfoTypeList.Descriptions.ElectronicLogisticInvoice);
					result.AddPair(CusSupportingInfoTypeList.Codes.ImportLicense, CusSupportingInfoTypeList.Descriptions.ImportLicense);
					result.AddPair(CusSupportingInfoTypeList.Codes.MercosulForeignDeclaration, CusSupportingInfoTypeList.Descriptions.MercosulForeignDeclaration);
					result.AddPair(CusSupportingInfoTypeList.Codes.TaxRegime, CusSupportingInfoTypeList.Descriptions.TaxRegime);
					result.AddPair(CusSupportingInfoTypeList.Codes.LegalAct, CusSupportingInfoTypeList.Descriptions.LegalAct);
					result.AddPair(CusSupportingInfoTypeList.Codes.Drawback, CusSupportingInfoTypeList.Descriptions.Drawback);
					result.AddPair(CusSupportingInfoTypeList.Codes.ConsentingProcess, CusSupportingInfoTypeList.Descriptions.ConsentingProcess);
					result.AddPair(CusSupportingInfoTypeList.Codes.CertificateOfOrigin, CusSupportingInfoTypeList.Descriptions.CertificateOfOrigin);
					result.AddPair(CusSupportingInfoTypeList.Codes.PreviousDocument, CusSupportingInfoTypeList.Descriptions.PreviousDocument);
					result.AddPair(CusSupportingInfoTypeList.Codes.Permit, CusSupportingInfoTypeList.Descriptions.Permit);
					result.AddPair(CusSupportingInfoTypeList.Codes.DuimpTaxRegime, CusSupportingInfoTypeList.Descriptions.DuimpTaxRegime);
					break;
			}
			return result;
		}

		public ICodeDescriptionPairList TableSpecificCusReferenceTypeList(ZString tableCode, string dataContext) => null;

		public ICodeDescriptionPairList TableSpecificCusAddInfoTypeList(ZString tableCode, string dataContext = "")
		{
			var result = new CodeDescriptionPairList();
			return result;
		}

		public ICodeDescriptionPairList TableSpecificAddInfoGroupTypesNeedInsertedToOtherTableList(ZString tableCode, string dataContext)
		{
			return null;
		}

		public ICodeDescriptionPairList TableSpecificCusCodeDataTypeList(ZString tableCode, string dataContext = "")
		{
			var result = new CodeDescriptionPairList();
			switch (tableCode)
			{
				case JobDeclarationSchema.Constants.Prefix:
					result.AddPair(CusCodeDataTypeList.Codes.CustomsOffice, CusCodeDataTypeList.Descriptions.CustomsOffice);
					result.AddPair(CusCodeDataTypeList.Codes.CustomsEnclosure, CusCodeDataTypeList.Descriptions.CustomsEnclosure);
					result.AddPair(CusCodeDataTypeList.Codes.WarehouseArea, CusCodeDataTypeList.Descriptions.WarehouseArea);
					break;
				case JobComInvoiceLineSchema.Constants.Prefix:
					result.AddPair(CusCodeDataTypeList.Codes.Attribute, CusCodeDataTypeList.Descriptions.Attribute);
					result.AddPair(CusCodeDataTypeList.Codes.NVE, CusCodeDataTypeList.Descriptions.NVE);
					result.AddPair(CusCodeDataTypeList.Codes.TariffDetach, CusCodeDataTypeList.Descriptions.TariffDetach);
					break;
			}
			return result;
		}

		public ICodeDescriptionPairList TableSpecificCusCodeDataCodeList(ZString tableCode, string dataContext = "")
		{
			var result = new CodeDescriptionPairList();
			return result;
		}

		public ITopLevelDataObjectReader GetNewJobDeclarationDataObjectReader(Shipment declarationDataObject, IXmlImportLogger logger, UniversalObjectFactory factory, ForwardingShipment shipment)
		{
			return new BRJobDeclarationDataObjectReader(declarationDataObject, logger, factory, shipment);
		}

		public UniversalDataObjectReaderHelper GetNewUniversalDataObjectReaderHelper(UniversalObjectFactory factory, string sourceCountryCode, string dataProviderForCodeMapping = null)
		{
			return new BRDataObjectReaderHelper(factory, sourceCountryCode);
		}

		public ITopLevelDataObjectWriter GetNewDeclarationDataObjectWriter(IDataWritingManager manager)
		{
			return new BRJobDeclarationDataObjectWriter(manager);
		}

		public IEnumerable<ITopLevelDataObjectReader> GetNewAirManifestDataObjectReaders(Shipment mawbDataObject, Shipment subShipment, IXmlImportLogger logger, UniversalObjectFactory factory, bool singleHAWBCheck)
		{
			return Enumerable.Empty<ITopLevelDataObjectReader>();
		}

		public ITopLevelDataObjectWriter GetNewAirManifestDataObjectWriter(IDataWritingManager manager)
		{
			return null;
		}

		public ITopLevelDataObjectWriter GetNewAirManifestLineDataObjectWriter(IDataWritingManager manager, AirManifestDataObjectWriterHelper helper)
		{
			return null;
		}

		public IEnumerable<ITopLevelDataObjectReader> GetNewCusSCAOceanBillDataObjectReaders(Shipment dataObject, Shipment subShipment, IXmlImportLogger logger, UniversalObjectFactory factory)
		{
			return System.Linq.Enumerable.Empty<ITopLevelDataObjectReader>();
		}

		public ITopLevelDataObjectWriter GetNewCusSCAOceanBillDataObjectWriter(IDataWritingManager manager)
		{
			return null;
		}

		public ITopLevelDataObjectReader GetNewStandaloneCommercialInvoiceDataObjectReader(Shipment shipmentDataObject, CommercialInvoiceHeader invoiceDataObject, IXmlImportLogger logger, UniversalObjectFactory factory)
		{
			return null;
		}

		public StandaloneCommercialInvoiceDataObjectWriter GetNewStandaloneCommercialInvoiceDataObjectWriter(IDataWritingManager manager)
		{
			return null;
		}

		#endregion
	}
}
