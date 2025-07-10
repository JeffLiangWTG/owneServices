using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business.UniversalDataTransfer;
using Enterprise.Customs.Business;
using Enterprise.UniversalDataBuss.DataObjects.Universal;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business
{
	public class ICS2AsycudaManifestHeaderDataObjectWriterHelper : AsycudaManifestHeaderDataObjectWriterHelper
	{
		public ICS2AsycudaManifestHeaderDataObjectWriterHelper(AsycudaManifestHeader header)
			: base(header)
		{
		}

		protected override IEnumerable<Date> GetHeaderAdditionalDateAddInfosCore(ASYCUDA.Business.AsycudaManifestHeader header)
		{
			var headerBO = (AsycudaManifestHeader)header;
			var actualArrivalDate = Date.New(DateType.ActualArrival, ZBool.False, headerBO.AMA_A_ARV);
			return new Date[] { actualArrivalDate };
		}

		protected override IEnumerable<CusSupportingInfo> GetHeaderCustomsSupportingInfosCore(ASYCUDA.Business.AsycudaManifestHeader header)
		{
			var headerBO = (AsycudaManifestHeader)header;
			var additionalInfoCollection = headerBO.AdditionalInfos;

			foreach (var additionalInfo in additionalInfoCollection)
			{
				yield return additionalInfo;
			}
		}

		protected override IEnumerable<CusSupportingInfo> GetBillCustomsSupportingInfosCore(ASYCUDA.Business.AsycudaBill bill)
		{
			var billBO = (AsycudaBill)bill;
			var additionalInfoCollection = billBO.AdditionalInfos;

			foreach (var additionalInfo in additionalInfoCollection)
			{
				yield return additionalInfo;
			}
		}
	}
}
