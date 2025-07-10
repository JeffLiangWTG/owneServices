using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.GB.GVMS.UniversalDataTransfer
{
	public class GVMSAsycudaManifestDataObjectReaderHelper : ASYCUDA.Business.UniversalDataTransfer.AsycudaManifestDataObjectReaderHelper
	{
		public GVMSAsycudaManifestDataObjectReaderHelper(BusinessObjectFactory factory)
			: base(Core.Constants.CountryCodes.UnitedKingdom, factory)
		{
		}

		protected override IEnumerable<GenAddOnDetail> GetAsycudaManifestHeaderGenAddOnColumnListCore(ASYCUDA.Business.AsycudaManifestHeader baseHeaderBO)
		{
			var headerBo = (AsycudaManifestHeader)baseHeaderBO;
			foreach (var detail in base.GetAsycudaManifestHeaderGenAddOnColumnListCore(headerBo))
			{
				yield return detail;
			}
			yield return new GenAddOnDetail { TypeCode = AddOnColumnDataType.GetCodeFromObject(headerBo.EmptyVehicle), AddInfoKey = AsycudaManifestHeader.Schema.EmptyVehicle, GenAddOnColumnName = AsycudaManifestHeader.Schema.EmptyVehicle, PropertyName = nameof(headerBo.EmptyVehicle) };
			yield return new GenAddOnDetail { TypeCode = AddOnColumnDataType.GetCodeFromObject(headerBo.RouteId), AddInfoKey = AsycudaManifestHeader.Schema.RouteId, GenAddOnColumnName = AsycudaManifestHeader.Schema.RouteId, PropertyName = nameof(headerBo.RouteId) };
			yield return new GenAddOnDetail { TypeCode = AddOnColumnDataType.GetCodeFromObject(headerBo.IsUnaccompanied), AddInfoKey = AsycudaManifestHeader.Schema.IsUnaccompanied, GenAddOnColumnName = AsycudaManifestHeader.Schema.IsUnaccompanied, PropertyName = nameof(headerBo.IsUnaccompanied) };
		}

		protected override void FillManifestSpecificDataCore(Shipment shipmentDataObject, IXmlImportLogger logger, ASYCUDA.Business.AsycudaManifestHeader baseHeaderBO, UniversalObjectFactory factory)
		{
			var headerBo = (AsycudaManifestHeader)baseHeaderBO;
			var customsSupportingInformationCollection = shipmentDataObject.CustomsSupportingInformationCollection;
			var differentTypes = customsSupportingInformationCollection == null ? System.Array.Empty<ZString>() : customsSupportingInformationCollection.Select(x => x.Category.GetCodeAsUpperCase()).Distinct().ToArray();
			var supportedTypes = GetSupportedCusSupportingInfoCSI_TypesFor(CountryCode, headerBo.TablePrefix, "");
			bool? isParentInDatabase = headerBo.IsInDatabase;
			if (supportedTypes != null && supportedTypes.Length > 0)
			{
				var query = new ZQuery(CusSupportingInfoSchema.CSI_ParentID, headerBo.PK);
				query.AddToFilter(CusSupportingInfoSchema.CSI_Type, supportedTypes);
				query.AddToFilter(CusSupportingInfoSchema.CSI_Type, differentTypes); // only delete type that is specified in XML and is supported
				query.FetchOnlyFromLocalCache = isParentInDatabase.HasValue && !isParentInDatabase.Value;
				var existingSupportingInfos = factory.Load<CusSupportingInfo>(query);
				existingSupportingInfos.DeleteAll(true);

				foreach (var customsSupportingInformation in shipmentDataObject.CustomsSupportingInformationCollection.Where(x => supportedTypes.Contains(x.Category.GetCodeAsUpperCase())))
				{
					_ = new CustomsSupportingInformationDataObjectReader(customsSupportingInformation, logger, factory, headerBo.PK, headerBo.TablePrefix).ReadIntoDataRow();
				}
			}
			if (shipmentDataObject.LocationOfGoodsCollection != null)
			{
				foreach (var location in shipmentDataObject.LocationOfGoodsCollection.Where(l => l.Type == LocationOfGoodsType.Inspection))
				{
					var inspectionLocation = headerBo.InspectionLocations.AddNew();
					inspectionLocation.CY_Code = location.SubType.Code ?? ZString.Empty;
					inspectionLocation.CY_Data = location.AdditionalIdentifier ?? ZString.Empty;
				}
			}
		}
	}
}

