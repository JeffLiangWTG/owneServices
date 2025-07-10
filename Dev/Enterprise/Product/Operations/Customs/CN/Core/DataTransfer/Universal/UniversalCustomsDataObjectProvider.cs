using System.Collections.Generic;
using System.Linq;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.Customs.DataTransfer.Universal.AirManifest;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CN.DataTransfer.Universal
{
	public class UniversalCustomsDataObjectProvider : IUniversalCustomsDataObjectProvider
	{
		#region Implementation of IUniversalCustomsDataObjectProvider

		public ICodeDescriptionPairList TableSpecificCusSupportingInfoTypeList(ZString tableCode, string dataContext = "")
		{
			var result = new CodeDescriptionPairList();

			switch (tableCode)
			{
				case JobComInvoiceLineSchema.Constants.Prefix:
					result.AddPair(Business.Constants.CusSupportingInfoTypes.CusSupportingDocument, Res.GetString("390A3A3B-A902-4167-9957-0A2F61D8D41F", "Attached Document"));
					result.AddPair(Business.Constants.CusSupportingInfoTypes.CIQProductQualification, Res.GetString("96A3868D-EAFB-4EB1-A219-679D79B8CE77", "CIQ Product Qualification"));
					break;
			}

			return result;
		}

		public ICodeDescriptionPairList TableSpecificCusReferenceTypeList(ZString tableCode, string dataContext) => null;

		public ICodeDescriptionPairList TableSpecificCusAddInfoTypeList(ZString tableCode, string dataContext = "")
		{
			var result = new CodeDescriptionPairList();

			switch (tableCode)
			{
				case JobComInvoiceLineSchema.Constants.Prefix:
					result.AddPair(CusAddInfoTypeAttribute.Codes.CNVINData, Res.GetString("a3fc93d8-3a0d-469b-a059-2daa1f208fc4", "CN VIN Data"));
					break;
				case CusEntryInstructionSchema.Constants.Prefix:
					result.AddPair(CusAddInfoTypeAttribute.Codes.CIQRequiredDocument, Res.GetString("8d59970f-f811-4f52-a25f-732a52286431", "CIQ Required document"));
					break;
			}

			return result;
		}

		public ICodeDescriptionPairList TableSpecificAddInfoGroupTypesNeedInsertedToOtherTableList(ZString tableCode, string dataContext)
		{
			return null;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1012:ProductNamingRule")]
		public ICodeDescriptionPairList TableSpecificCusCodeDataTypeList(ZString tableCode, string dataContext = "")
		{
			var result = new CodeDescriptionPairList();

			switch (tableCode)
			{
				case CusClassPartPivotSchema.Constants.Prefix:
					result.AddPair(Business.Constants.CusCodeDataTypes.Codes.AdditionalInformation, Business.Constants.CusCodeDataTypes.Descriptions.AdditionalInformation);
					break;
				case JobComInvoiceLineSchema.Constants.Prefix:
					result.AddPair(Business.Constants.CusCodeDataTypes.Codes.CargoAttribute, Business.Constants.CusCodeDataTypes.Descriptions.CargoAttribute);
					result.AddPair(Business.Constants.CusCodeDataTypes.Codes.CIQ, Business.Constants.CusCodeDataTypes.Descriptions.CIQ);
					break;
				case JobDeclarationSchema.Constants.Prefix:
					result.AddPair(Business.Constants.CusCodeDataTypes.Codes.CustomsOffice, Business.Constants.CusCodeDataTypes.Descriptions.CustomsOffice);
					result.AddPair(Business.Constants.CusCodeDataTypes.Codes.MergingRule, Business.Constants.CusCodeDataTypes.Descriptions.MergingRule);
					break;
				case CusEntryInstructionSchema.Constants.Prefix:
					result.AddPair(Business.Constants.CusCodeDataTypes.Codes.EnterpriseQualification, Business.Constants.CusCodeDataTypes.Descriptions.EnterpriseQualification);
					result.AddPair(Business.Constants.CusCodeDataTypes.Codes.SpecialBusinessIdentifier, Business.Constants.CusCodeDataTypes.Descriptions.SpecialBusinessIdentifier);
					result.AddPair(Business.Constants.CusCodeDataTypes.Codes.Package, Business.Constants.CusCodeDataTypes.Descriptions.Package);
					result.AddPair(Business.Constants.CusCodeDataTypes.Codes.OperationMatter, Business.Constants.CusCodeDataTypes.Descriptions.OperationMatter);
					result.AddPair(Business.Constants.CusCodeDataTypes.Codes.CusAttachment, Business.Constants.CusCodeDataTypes.Descriptions.CusAttachment);
					break;
			}

			return result;
		}

		public ICodeDescriptionPairList TableSpecificCusCodeDataCodeList(ZString tableCode, string dataContext = "")
		{
			var result = new CodeDescriptionPairList();

			switch (tableCode)
			{
				case JobComInvoiceLineSchema.Constants.Prefix:
					result.AddPair(Business.Constants.CusCodeDataCode.BatchNumber, Res.GetString("8369d50e-f35d-4c2d-98dc-1cc505f56a16", "Batch Number"));
					break;
			}

			return result;
		}

		public ITopLevelDataObjectReader GetNewJobDeclarationDataObjectReader(Shipment declarationDataObject, IXmlImportLogger logger, UniversalObjectFactory factory, ForwardingShipment shipment)
		{
			return new CNJobDeclarationDataObjectReader(declarationDataObject, logger, factory, shipment);
		}

		public UniversalDataObjectReaderHelper GetNewUniversalDataObjectReaderHelper(UniversalObjectFactory factory, string sourceCountryCode, string dataProviderForCodeMapping = null)
		{
			return new CNDataObjectReaderHelper(factory, sourceCountryCode);
		}

		public ITopLevelDataObjectWriter GetNewDeclarationDataObjectWriter(IDataWritingManager manager)
		{
			return new CNJobDeclarationDataObjectWriter(manager);
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
