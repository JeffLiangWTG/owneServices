using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.Customs.GB.ICS.Business;
using Enterprise.Customs.GB.ICS.CodeDescriptionPairLists;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.GB.ICS.UniversalDataTransfer;

public class SASAsycudaManifestHeaderDataObjectWriter<T> : ASYCUDA.Business.UniversalDataTransfer.AsycudaManifestHeaderDataObjectWriter<T>
		where T : AsycudaManifestHeaderSS
{
	public SASAsycudaManifestHeaderDataObjectWriter(IDataWritingManager manager) : base(manager)
	{
	}

	protected override void PopulateDataObject(T sourceManifest, Shipment uxml)
	{
		base.PopulateDataObject(sourceManifest, uxml);
		var headerHelper = CreateAsycudaManifestHeaderDataObjectWriterHelper(sourceManifest);
		PopulateCustomsReferenceCollection(sourceManifest, uxml, headerHelper);
	}

	protected override void PopulateVoyageFlightNo(T headerBO, Shipment headerData)
	{
		switch (headerBO.AMA_TransportMode)
		{
			case GBSSTransportTypeList.Codes.RoadFreight:
			case GBSSTransportTypeList.Codes.RoroAccompanied:
			case GBSSTransportTypeList.Codes.RoroUnaccompanied:
				headerData.VoyageFlightNo = headerBO.AMA_VehicleRegistration;
				break;
			default:
				headerData.VoyageFlightNo = headerBO.AMA_Voyage;
				break;
		}
	}

	void PopulateCustomsReferenceCollection(T headerBO, Shipment headerData, ASYCUDA.Business.UniversalDataTransfer.AsycudaManifestHeaderDataObjectWriterHelper headerHelper)
	{
		var writer = GetUniversalDataObjectWriterHelper(headerBO.Factory, headerBO.CountryCode);
		UniversalDataObjectWriterHelper helper = GetUniversalDataObjectWriterHelper(headerBO.Factory, headerBO.CountryCode);
		headerData.SetCustomsReferenceCollection(() => CustomsReferenceCollectionCreator.CreateCollection(helper, headerBO, writeManager));
	}

	UniversalDataObjectWriterHelper GetUniversalDataObjectWriterHelper(BusinessObjectFactory factory, ZString countryCode)
	{
		return new UniversalDataObjectWriterHelper(factory, countryCode);
	}
}
