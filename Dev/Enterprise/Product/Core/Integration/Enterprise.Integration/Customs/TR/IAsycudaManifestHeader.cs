using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Integration
{
	public static partial class Customs
	{
		public static partial class ASYCUDA
		{
			public static partial class TRManifest
			{
				public interface IAsycudaManifestHeader : ASYCUDA.IAsycudaManifestHeader
				{
					IBusinessObjectCollection Messages { get; }
					void CalculateStampDuties();
					void CreateManifestStatement();
					ZDateTime TemporaryStorageStartDate { get; set; }
					ZDateTime TemporaryStorageDueDate { get; set; }
					ZString AMA_LloydsNumber { get; set; }
					ZString AMA_RL_NKPortOfLoading { get; set; }
					ZString AMA_CustomsLoadPort { get; set; }
					ZString AMA_RL_NKPortOfDischarge { get; set; }
					ZString AMA_CustomsDischargePort { get; set; }
					ZString ManifestInternalInspectionNo { get; set; }
					ZString TransportType { get; set; }
					ZString AMA_GS_NKCustomsAgent { get; set; }
					ZString AMA_InspectionClerk { get; set; }

					void SuspendValidation();
				}
			}
		}
	}
}
