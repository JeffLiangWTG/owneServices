using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.ASYCUDA.Business.UniversalDataTransfer
{
	public class AsycudaManifestDataObjectReaderHelper : AsycudaManifestUniversalCommonHelper
	{
		public AsycudaManifestDataObjectReaderHelper(ZString countryCode, BusinessObjectFactory factory)
			: base(factory)
		{
			CountryCode = countryCode;
		}

		public readonly ZString CountryCode;

		public ZString GetManifestTypeByDataObject(Shipment shipment)
		{
			return GetManifestTypeByDataObjectCore(shipment);
		}

		protected virtual ZString GetManifestTypeByDataObjectCore(Shipment shipment) => ZString.Empty;

		public void FillAdditionalDates(List<Date> dateCollection, IColumnIndexer header)
		{
			if (header != null && header is AsycudaManifestHeader && dateCollection != null && dateCollection.Count > 0)
			{
				FillAdditionalDatesCore(dateCollection, header);
			}
		}

		protected virtual void FillAdditionalDatesCore(List<Date> dateCollection, IColumnIndexer header)
		{
		}

		public IEnumerable<GenAddOnDetail> GetAdditionalInfoColumnList(AsycudaBill countryBO) => GetAdditionalInfoColumnListCore(countryBO);

		protected virtual IEnumerable<GenAddOnDetail> GetAdditionalInfoColumnListCore(AsycudaBill billBO)
		{
			return Enumerable.Empty<GenAddOnDetail>();
		}

		public void FillManifestSpecificData(Shipment shipmentDataObject, IXmlImportLogger logger, AsycudaManifestHeader header, UniversalObjectFactory factory)
		{
			FillManifestSpecificDataCore(shipmentDataObject, logger, header, factory);
		}

		protected virtual void FillManifestSpecificDataCore(Shipment shipmentDataObject, IXmlImportLogger logger, AsycudaManifestHeader header, UniversalObjectFactory factory)
		{
		}

		public void FillVoyageFlightNo(ZString? voyageFlightNo, ZString transportMode, IXmlImportLogger logger, AsycudaManifestHeader header)
		{
			FillVoyageFlightNoCore(voyageFlightNo, transportMode, logger, header);
		}

		protected virtual void FillVoyageFlightNoCore(ZString? voyageFlightNo, ZString transportMode, IXmlImportLogger logger, AsycudaManifestHeader header)
		{
			if (voyageFlightNo.HasValue)
			{
				switch (transportMode)
				{
					case Core.Constants.TransportModes.Road:
						header.AMA_VehicleRegistration = voyageFlightNo.Value;
						break;
					default:
						header.AMA_Voyage = voyageFlightNo.Value;
						break;
				}
			}
		}

		public ZString[] GetHeaderSupportedCusSupportingInfoCSI_Types(AsycudaManifestHeader header)
		{
			return GetHeaderSupportedCusSupportingInfoCSI_TypesCore(header);
		}

		protected virtual ZString[] GetHeaderSupportedCusSupportingInfoCSI_TypesCore(AsycudaManifestHeader header)
		{
			return Array.Empty<ZString>();
		}

		public ZString[] GetBillSupportedCusSupportingInfoCSI_Types(AsycudaBill bill)
		{
			return GetBillSupportedCusSupportingInfoCSI_TypesCore(bill);
		}

		protected virtual ZString[] GetBillSupportedCusSupportingInfoCSI_TypesCore(AsycudaBill bill)
		{
			return Array.Empty<ZString>();
		}

		public AsycudaBillsReaderHelper BillsReaderHelper
		{
			get { return billsReaderHelper ?? (billsReaderHelper = new AsycudaBillsReaderHelper()); }
		}
		AsycudaBillsReaderHelper billsReaderHelper;

		public AyscudaContainersReaderHelper ContainersReaderHelper
		{
			get { return containersReaderHelper ?? (containersReaderHelper = new AyscudaContainersReaderHelper()); }
		}
		AyscudaContainersReaderHelper containersReaderHelper;

		public AyscudaPacksReaderHelper PacksReaderHelper
		{
			get { return packsReaderHelper ?? (packsReaderHelper = new AyscudaPacksReaderHelper()); }
		}
		AyscudaPacksReaderHelper packsReaderHelper;

		public AsycudaBillDataObjectReader GetBillDataObjectReader(Shipment dataObject, IXmlImportLogger logger, UniversalObjectFactory factory, AsycudaManifestHeader header, AsycudaManifestDataObjectReaderHelper helper, bool isUpdateEnabled)
		{
			return GetBillDataObjectReaderCore(dataObject, logger, factory, header, helper, isUpdateEnabled);
		}

		protected virtual AsycudaBillDataObjectReader GetBillDataObjectReaderCore(Shipment dataObject, IXmlImportLogger logger, UniversalObjectFactory factory, AsycudaManifestHeader header, AsycudaManifestDataObjectReaderHelper helper, bool isUpdateEnabled)
		{
			return new AsycudaBillDataObjectReader(dataObject, logger, factory, header, helper, isUpdateEnabled);
		}

		public void FilterAndSet<TColumn>(TColumn column, Shipment dataObject, Action<TColumn> set) where TColumn : SchemaColumn
		{
			if (ShouldReadColumn(column, dataObject))
			{
				set(column);
			}
		}

		protected virtual bool ShouldReadColumn(SchemaColumn column, Shipment dataObject) => true;
	}
}
