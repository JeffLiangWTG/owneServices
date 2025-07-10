using System.Collections.Generic;
using System.Linq;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.EU.DataTransfer.Universal
{
	public class UniversalCustomsDataObjectProvider : IUniversalCustomsDataObjectProvider
	{
		public static void PopulateCustomsReference(List<CustomsReference> customsReferenceCollection, IEnumerable<CusSupplyChainActorReference> cusSupplyChainActorReferences, UniversalCommonHelper helper, IDataWritingManager writeManager)
		{
			var addEmpty = true;
			foreach (var actor in cusSupplyChainActorReferences.OrderBy(x => x.CFR_SystemCreateTimeUtc).ThenBy(x => x.CFR_Code))
			{
				addEmpty = false;
				var reference = new CustomsReference()
				{
					Type = new CodeDescriptionPair() { Code = Customs.Business.CusReferenceTypeList.Codes.SupplyChainActor, Description = Customs.Business.CusReferenceTypeList.Descriptions.SupplyChainActor },
					Owner = helper.CreateOrganizationAddressFromAddress(writeManager, actor.Owner, AddressTypes.Owner),
					Reference = actor.CFR_Reference,
					SubType = ListHelper.GetWithDescription<CodeDescriptionPair35Char>(actor.CFR_Code, actor.Lookups.CodeList),
				};
				customsReferenceCollection.Add(reference);
			}
			if (addEmpty)
			{
				customsReferenceCollection.Add(new CustomsReference()
				{
					Type = new CodeDescriptionPair() { Code = Customs.Business.CusReferenceTypeList.Codes.SupplyChainActor, Description = Customs.Business.CusReferenceTypeList.Descriptions.SupplyChainActor }
				});
			}
		}

		#region IUniversalCustomsDataObjectProvider Members

		public ICodeDescriptionPairList TableSpecificCusSupportingInfoTypeList(ZString tableCode, string dataContext = "")
		{
			return TableSpecificCusSupportingInfoTypeListCore(tableCode, dataContext);
		}

		public ICodeDescriptionPairList TableSpecificCusReferenceTypeList(ZString tableCode, string dataContext)
		{
			return TableSpecificCusReferenceTypeListCore(tableCode, dataContext);
		}

		protected virtual ICodeDescriptionPairList TableSpecificCusReferenceTypeListCore(ZString tableCode, string dataContext)
		{
			return new CusReferenceDataTypeAndCodeListProvider().TableSpecificCusReferenceDataTypeList(tableCode, dataContext);
		}

		public ICodeDescriptionPairList TableSpecificAddInfoGroupTypesNeedInsertedToOtherTableList(ZString tableCode, string dataContext)
		{
			return null;
		}

		protected virtual ICodeDescriptionPairList TableSpecificCusSupportingInfoTypeListCore(ZString tableCode, string dataContext)
		{
			return GetCusSupportingInfoTypeListProvider().TableSpecificCusSupportingInfoTypeList(tableCode, dataContext);
		}

		protected virtual CusSupportingInfoTypeListProvider GetCusSupportingInfoTypeListProvider()
		{
			return new CusSupportingInfoTypeListProvider();
		}

		public ICodeDescriptionPairList TableSpecificCusAddInfoTypeList(ZString tableCode, string dataContext)
		{
			return TableSpecificCusAddInfoTypeListCore(tableCode, dataContext);
		}

		protected virtual ICodeDescriptionPairList TableSpecificCusAddInfoTypeListCore(ZString tableCode, string dataContext)
		{
			return new CusAddInfoTypeListProvider().TableSpecificCusAddInfoTypeList(tableCode, dataContext);
		}

		public ICodeDescriptionPairList TableSpecificCusCodeDataCodeList(ZString tableCode, string dataContext)
		{
			return null;
		}

		public virtual ICodeDescriptionPairList TableSpecificCusCodeDataTypeList(ZString tableCode, string dataContext)
		{
			return TableSpecificCusCodeDataTypeListCore(tableCode, dataContext);
		}

		protected virtual ICodeDescriptionPairList TableSpecificCusCodeDataTypeListCore(ZString tableCode, string dataContext)
		{
			return GetCusCodeDataTypeAndCodeListProvider().TableSpecificCusCodeDataTypeList(tableCode, dataContext);
		}

		protected virtual CusCodeDataTypeAndCodeListProvider GetCusCodeDataTypeAndCodeListProvider()
		{
			return new CusCodeDataTypeAndCodeListProvider();
		}

		public ITopLevelDataObjectWriter GetNewDeclarationDataObjectWriter(IDataWritingManager manager)
		{
			return GetNewDeclarationDataObjectWriterCore(manager);
		}

		protected virtual ITopLevelDataObjectWriter GetNewDeclarationDataObjectWriterCore(IDataWritingManager manager)
		{
			return new DeclarationDataObjectWriter(manager);
		}

		public ITopLevelDataObjectReader GetNewJobDeclarationDataObjectReader(Shipment declarationDataObject, IXmlImportLogger logger, UniversalObjectFactory factory, ForwardingShipment shipment)
		{
			return GetNewJobDeclarationDataObjectReaderCore(declarationDataObject, logger, factory, shipment);
		}

		protected virtual ITopLevelDataObjectReader GetNewJobDeclarationDataObjectReaderCore(Shipment declarationDataObject, IXmlImportLogger logger, UniversalObjectFactory factory, ForwardingShipment shipment)
		{
			return new JobDeclarationDataObjectReader(declarationDataObject, logger, factory, shipment);
		}

		public IEnumerable<ITopLevelDataObjectReader> GetNewAirManifestDataObjectReaders(Shipment mawbDataObject, Shipment subShipment, IXmlImportLogger logger, UniversalObjectFactory factory, bool singleHAWBCheck)
		{
			return GetNewAirManifestDataObjectReadersCore(mawbDataObject, subShipment, logger, factory, singleHAWBCheck);
		}

		protected virtual IEnumerable<ITopLevelDataObjectReader> GetNewAirManifestDataObjectReadersCore(Shipment mawbDataObject, Shipment subShipment, IXmlImportLogger logger, UniversalObjectFactory factory, bool singleHAWBCheck)
		{
			return System.Linq.Enumerable.Empty<ITopLevelDataObjectReader>();
		}

		public ITopLevelDataObjectWriter GetNewAirManifestDataObjectWriter(IDataWritingManager manager)
		{
			return GetNewAirManifestDataObjectWriterCore(manager);
		}

		protected virtual ITopLevelDataObjectWriter GetNewAirManifestDataObjectWriterCore(IDataWritingManager manager)
		{
			return null;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0001:Simplify Names", Justification = "Simplification hides desired base class")]
		public ITopLevelDataObjectWriter GetNewAirManifestLineDataObjectWriter(IDataWritingManager manager, Enterprise.Customs.DataTransfer.Universal.AirManifest.AirManifestDataObjectWriterHelper helper)
		{
			return GetNewAirManifestLineDataObjectWriterCore(manager, helper);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0001:Simplify Names", Justification = "Simplification hides desired base class")]
		protected virtual ITopLevelDataObjectWriter GetNewAirManifestLineDataObjectWriterCore(IDataWritingManager manager, Enterprise.Customs.DataTransfer.Universal.AirManifest.AirManifestDataObjectWriterHelper helper)
		{
			return null;
		}

		public virtual Customs.DataTransfer.Universal.UniversalDataObjectReaderHelper GetNewUniversalDataObjectReaderHelper(UniversalObjectFactory factory, string sourceCountryCode, string dataProviderForCodeMapping = null)
		{
			return new UniversalDataObjectReaderHelper(factory, sourceCountryCode, sourceCountryCode);
		}

		public ITopLevelDataObjectReader GetNewStandaloneCommercialInvoiceDataObjectReader(Shipment shipmentDataObject, CommercialInvoiceHeader invoiceDataObject, IXmlImportLogger logger, UniversalObjectFactory factory)
		{
			return null;
		}

		public StandaloneCommercialInvoiceDataObjectWriter GetNewStandaloneCommercialInvoiceDataObjectWriter(IDataWritingManager manager)
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

		#endregion
	}
}
