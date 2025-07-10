using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.ASYCUDA.Business.UniversalDataTransfer;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business
{
	public class ICS2AsycudaManifestDataObjectReaderHelper : AsycudaManifestDataObjectReaderHelper, ICustomsReferenceCollectionReaderHelper
	{
		public ICS2AsycudaManifestDataObjectReaderHelper(ZString countryCode, BusinessObjectFactory factory)
			: base(countryCode, factory)
		{
		}

		protected override void FillAdditionalDatesCore(List<Date> dateCollection, IColumnIndexer header)
		{
			var headerBO = (AsycudaManifestHeader)header;
			var date = dateCollection.FirstOrDefault(DateType.ActualArrival, ZBool.False);
			if (date != null && date.Value.HasValue)
			{
				headerBO.AMA_A_ARV = date.Value.Value;
			}
		}

		protected override ZString[] GetHeaderSupportedCusSupportingInfoCSI_TypesCore(ASYCUDA.Business.AsycudaManifestHeader header)
		{
			return new ZString[] { CusSupportingInfoTypeList.Codes.AdditionalInfo };
		}

		protected override ZString[] GetBillSupportedCusSupportingInfoCSI_TypesCore(ASYCUDA.Business.AsycudaBill bill)
		{
			return new ZString[] { CusSupportingInfoTypeList.Codes.AdditionalInfo };
		}

		protected override void FillManifestSpecificDataCore(Shipment shipmentDataObject, IXmlImportLogger logger, ASYCUDA.Business.AsycudaManifestHeader header, UniversalObjectFactory factory)
		{
			Factory = factory;

			FillHeaderGenAddOnColumns(shipmentDataObject, logger, header);
			FillMasterBillGenAddOnColumns(shipmentDataObject, logger, header);
			FillCusSupplyChainActorReferences(shipmentDataObject, logger, header, factory);
			FillMasterBillScreenings(shipmentDataObject, logger, header);

			if (shipmentDataObject.IsHVLV())
			{
				FillAddressedMemberState(shipmentDataObject, logger, header);
				FillActualArrival(shipmentDataObject, logger, header);
				FillMasterBill(shipmentDataObject, logger, header);
			}
		}

		public void FillActualArrival(Shipment shipmentDataObject, IXmlImportLogger logger, ASYCUDA.Business.AsycudaManifestHeader header)
		{
			if (shipmentDataObject.TransportLegCollection != null && shipmentDataObject.TransportLegCollection.Count > 0)
			{
				var dates = shipmentDataObject.TransportLegCollection.Select(x => x.ActualArrival).Where(x => x.HasValue).OrderByDescending(x => x.Value);
				if (dates.Any())
				{
					if (header.MasterBill is AsycudaBill masterBillBO)
					{
						var headerBO = (AsycudaManifestHeader)header;
						headerBO.AMA_A_ARV = dates.First().Value;
					}
				}
			}
		}

		public void FillMasterBill(Shipment shipmentDataObject, IXmlImportLogger logger, ASYCUDA.Business.AsycudaManifestHeader header)
		{
			var headerBO = (AsycudaManifestHeader)header;
			headerBO.AMA_MasterBill = shipmentDataObject.GetMasterBill();
		}

		public void FillAddressedMemberState(Shipment shipmentDataObject, IXmlImportLogger logger, ASYCUDA.Business.AsycudaManifestHeader header)
		{
			var headerBO = (AsycudaManifestHeader)header;
			if (shipmentDataObject.PortOfDischarge != null)
			{
				headerBO.AddressedMemberState = shipmentDataObject.PortOfDischarge.GetCodeAsUpperCase().Left(2);
			}
		}

		void FillMasterBillScreenings(Shipment shipmentDataObject, IXmlImportLogger logger, ASYCUDA.Business.AsycudaManifestHeader header)
		{
			if (header.MasterBill is AsycudaBill masterBillBO)
			{
				var reader = new ICS2AsycudaBillDataObjectReader(shipmentDataObject, logger, Factory, header, this, false);
				reader.FillMasterBillScreens(masterBillBO);
			}
		}

		void FillHeaderGenAddOnColumns(Shipment shipmentDataObject, IXmlImportLogger logger, ASYCUDA.Business.AsycudaManifestHeader header)
		{
			var headerBO = (AsycudaManifestHeader)header;
			var headerGenAddOnColumns = new List<GenAddOnDetail>
			{
				new GenAddOnDetail { TypeCode = AddOnColumnDataType.GetCodeFromObject(headerBO.SpecificCircumstanceIndicator), AddInfoKey = AddInfoConstants.Header.SpecificCircumstanceIndicator, GenAddOnColumnName = AsycudaManifestHeader.Schema.SpecificCircumstanceIndicator, PropertyName = AsycudaManifestHeader.Schema.SpecificCircumstanceIndicator },
				new GenAddOnDetail { TypeCode = AddOnColumnDataType.GetCodeFromObject(headerBO.ReEntryIndicator), AddInfoKey = AddInfoConstants.Header.ReEntryIndicator, GenAddOnColumnName = AsycudaManifestHeader.Schema.ReEntryIndicator, PropertyName = AsycudaManifestHeader.Schema.ReEntryIndicator },
				new GenAddOnDetail { TypeCode = AddOnColumnDataType.GetCodeFromObject(headerBO.SplitConsignmentIndicator), AddInfoKey = AddInfoConstants.Header.SplitConsignmentIndicator, GenAddOnColumnName = AsycudaManifestHeader.Schema.SplitConsignmentIndicator, PropertyName = AsycudaManifestHeader.Schema.SplitConsignmentIndicator }
			};

			new GenAddOnColumnCollectionDataObjectReader(logger).ReadIntoBusinessObject(shipmentDataObject.AddInfoCollection, headerGenAddOnColumns, headerBO);
		}

		void FillMasterBillGenAddOnColumns(Shipment shipmentDataObject, IXmlImportLogger logger, ASYCUDA.Business.AsycudaManifestHeader header)
		{
			var headerBO = (AsycudaManifestHeader)header;
			var masterBillBO = headerBO.MasterBill;

			if (masterBillBO != null)
			{
				var masterBillGenAddOnColumns = new List<GenAddOnDetail>
				{
					new GenAddOnDetail { TypeCode = AddOnColumnDataType.GetCodeFromObject(masterBillBO.TransportDocumentType), AddInfoKey = AddInfoConstants.Bill.TransportDocumentType, GenAddOnColumnName = AsycudaBill.Schema.TransportDocumentType, PropertyName = AsycudaBill.Schema.TransportDocumentType },
				};

				new GenAddOnColumnCollectionDataObjectReader(logger).ReadIntoBusinessObject(shipmentDataObject.AddInfoCollection, masterBillGenAddOnColumns, masterBillBO);
			}
		}

		void FillCusSupplyChainActorReferences(Shipment shipmentDataObject, IXmlImportLogger logger, ASYCUDA.Business.AsycudaManifestHeader header, UniversalObjectFactory factory)
		{
			var reader = ObjectFactory.New<Integration.Customs.EU.ICustomsReferenceCollectionDataObjectReader>(logger, this, string.Empty) as CustomsReferenceCollectionDataObjectReader;
			reader.ReadIntoDataRows(header.PK, header.TablePrefix, header.IsInDatabase, shipmentDataObject);
		}

		#region ICustomsReferenceCollectionReaderHelper Members

		public UniversalObjectFactory Factory { get; private set; }

		public ZString[] GetSupportedCusCodeDataCY_TypesFor(ZString parentTableCode, string dataContext)
		{
			return Array.Empty<ZString>();
		}

		public ZString[] GetSupportedCusReferenceCFR_TypesFor(ZString parentTableCode, string dataContext)
		{
			var supportedTypes = new List<ZString>();

			switch (parentTableCode)
			{
				case AsycudaManifestHeaderSchema.Constants.Prefix:
				case AsycudaBillSchema.Constants.Prefix:
				case AsycudaPackSchema.Constants.Prefix:
					supportedTypes.Add(CusReferenceTypeList.Codes.SupplyChainActor);
					break;
			}

			return supportedTypes.ToArray();
		}

		protected override AsycudaBillDataObjectReader GetBillDataObjectReaderCore(Shipment dataObject, IXmlImportLogger logger, UniversalObjectFactory factory, ASYCUDA.Business.AsycudaManifestHeader header, AsycudaManifestDataObjectReaderHelper helper, bool isUpdateEnabled)
		{
			var isHVLV = dataObject.GetMatchingDataSource(DataContextType.HVLVConsignment) != null;
			return isHVLV ?
				new ICS2HVLVAsycudaBillDataObjectReader(dataObject, logger, factory, header, helper, isUpdateEnabled) :
				new ICS2AsycudaBillDataObjectReader(dataObject, logger, factory, header, helper, isUpdateEnabled);
		}

		#endregion
	}
}
