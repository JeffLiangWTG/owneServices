using System.Linq;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.GB.GVMS.UniversalDataTransfer
{
	public class GVMSAsycudaManifestHeaderDataObjectWriter<T> : ASYCUDA.Business.UniversalDataTransfer.AsycudaManifestHeaderDataObjectWriter<T>
		where T : AsycudaManifestHeader
	{
		public GVMSAsycudaManifestHeaderDataObjectWriter(IDataWritingManager manager)
			: base(manager)
		{
		}

		protected override void PopulateDataObject(T sourceManifest, Shipment uxml)
		{
			base.PopulateDataObject(sourceManifest, uxml);
			var headerHelper = CreateAsycudaManifestHeaderDataObjectWriterHelper(sourceManifest);
			PopulateCustomsSupportingInfoCollection(sourceManifest, uxml, headerHelper);
			PopulateInspectionLocations(sourceManifest, uxml);
		}

		void PopulateCustomsSupportingInfoCollection(T headerBO, Shipment headerData, ASYCUDA.Business.UniversalDataTransfer.AsycudaManifestHeaderDataObjectWriterHelper headerHelper)
		{
			var cusSupportingInfo = headerHelper.GetHeaderCustomsSupportingInfos(headerBO);
			var customsSupportingInformationList = cusSupportingInfo.Where(info => info != null).Select(info => CustomsSupportingInformationCollectionCreator.Create(info, null, writeManager)).ToList();
			headerData.SetCustomsSupportingInformationCollection(() => customsSupportingInformationList);
		}

		void PopulateInspectionLocations(T sourceManifest, Shipment headerData)
		{
			var locations = sourceManifest.InspectionLocations.Cast<GvmsInspectionAtLocationCusCodeData>().Select(l => new LocationOfGoods
			{
				Type = LocationOfGoodsType.Inspection,
				SubType = new CodeDescriptionPair2Char { Code = l.CY_Code },
				AdditionalIdentifier = l.CY_Data
			}).ToList();
			headerData.SetLocationOfGoodsCollection(() => locations);
		}
	}
}
