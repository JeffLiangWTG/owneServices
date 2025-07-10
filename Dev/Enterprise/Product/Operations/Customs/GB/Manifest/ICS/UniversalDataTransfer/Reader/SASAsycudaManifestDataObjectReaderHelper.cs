using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.Customs.GB.ICS.Business;
using Enterprise.Customs.GB.ICS.CodeDescriptionPairLists;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.GB.ICS.UniversalDataTransfer;

public class SASAsycudaManifestDataObjectReaderHelper : ASYCUDA.Business.UniversalDataTransfer.AsycudaManifestDataObjectReaderHelper
{
	public SASAsycudaManifestDataObjectReaderHelper(BusinessObjectFactory factory)
		: base(Core.Constants.CountryCodes.UnitedKingdom, factory)
	{
	}

	protected override void FillManifestSpecificDataCore(Shipment shipmentDataObject, IXmlImportLogger logger, ASYCUDA.Business.AsycudaManifestHeader baseHeaderBO, UniversalObjectFactory factory)
	{
		var headerBo = (AsycudaManifestHeaderSS)baseHeaderBO;
		var customsReferenceCollection = shipmentDataObject.CustomsReferenceCollection;
		var differentTypes = customsReferenceCollection == null ? System.Array.Empty<ZString>() : customsReferenceCollection.Select(x => x.Type.GetCodeAsUpperCase()).Distinct().ToArray();
		var supportedTypes = GetSupportedCusCodeDataCY_TypesFor(CountryCode, headerBo.TablePrefix, "");
		bool? isParentInDatabase = headerBo.IsInDatabase;
		if (supportedTypes != null && supportedTypes.Length > 0)
		{
			var query = new ZQuery(CusCodeDataSchema.CY_ParentID, headerBo.PK);
			query.AddToFilter(CusCodeDataSchema.CY_Type, supportedTypes);
			query.AddToFilter(CusCodeDataSchema.CY_Type, differentTypes);
			query.FetchOnlyFromLocalCache = isParentInDatabase.HasValue && !isParentInDatabase.Value;
			var existingCusCodeData = factory.Load<CusCodeData>(query);
			existingCusCodeData.DeleteAll(true);

			if (customsReferenceCollection != null)
			{
				foreach (var customsReference in customsReferenceCollection.Where(x => supportedTypes.Contains(x.Type.GetCodeAsUpperCase())))
				{
					_ = new CustomsReferenceDataObjectReader(customsReference, logger, headerBo.PK, headerBo.TablePrefix, factory).ReadIntoDataRowCusCodeData();
				}
			}
		}
	}

	protected override void FillVoyageFlightNoCore(ZString? voyageFlightNo, ZString transportMode, IXmlImportLogger logger, ASYCUDA.Business.AsycudaManifestHeader header)
	{
		if (voyageFlightNo.HasValue)
		{
			switch (transportMode)
			{
				case GBSSTransportTypeList.Codes.RoadFreight:
				case GBSSTransportTypeList.Codes.RoroAccompanied:
				case GBSSTransportTypeList.Codes.RoroUnaccompanied:
					header.AMA_VehicleRegistration = voyageFlightNo.Value;
					break;
				default:
					header.AMA_Voyage = voyageFlightNo.Value;
					break;
			}
		}
	}
}
