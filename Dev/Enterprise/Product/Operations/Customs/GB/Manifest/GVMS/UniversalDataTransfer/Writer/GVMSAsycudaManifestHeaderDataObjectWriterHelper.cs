using System.Collections.Generic;
using Enterprise.Customs.Business;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.MasterFiles.Business.CustomValues;

namespace Enterprise.Customs.GB.GVMS.UniversalDataTransfer
{
	public class GVMSAsycudaManifestHeaderDataObjectWriterHelper : ASYCUDA.Business.UniversalDataTransfer.AsycudaManifestHeaderDataObjectWriterHelper
	{
		public GVMSAsycudaManifestHeaderDataObjectWriterHelper(AsycudaManifestHeader header)
			: base(header)
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

		protected override IEnumerable<CusSupportingInfo> GetHeaderCustomsSupportingInfosCore(ASYCUDA.Business.AsycudaManifestHeader baseHeaderBO)
		{
			var headerBo = (AsycudaManifestHeader)baseHeaderBO;
			foreach (var supportingInfo in headerBo.CustomsReferences)
			{
				yield return supportingInfo;
			}

			foreach (var supportingInfo in headerBo.CustomsTransitReferences)
			{
				yield return supportingInfo;
			}

			foreach (var supportingInfo in headerBo.CustomsEidrAndOralReferences)
			{
				yield return supportingInfo;
			}
		}
	}
}
