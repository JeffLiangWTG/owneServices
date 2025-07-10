using CargoWise.EntityFramework;
using CargoWise.Types;

namespace CargoWise.Bi.Product.Manager.Business
{
	public class SsasCube : NonPersistentBusinessObject
	{
		public SsasCube(ZString modelFileName)
		{
			ModelFileName = modelFileName;
		}

		public ZString ModelFileName { get; set; }
		public ZString ModelVersion { get; set; }

		public ZDecimal MemoryUsage { get; set; }

		public ZDateTime LastSourceLsnDateTimeUtc { get; set; }
		public ZDateTime LastSourceLsnDateTimeLocal { get; set; }

		public ZDateTime LastProcessingStartDateTimeUtc { get; set; }
		public ZDateTime LastProcessingStartDateTimeLocal { get; set; }

		public ZDateTime LastProcessingFinishDateTimeUtc { get; set; }
		public ZDateTime LastProcessingFinishDateTimeLocal { get; set; }

		public ZDateTime LastResetModelInfoUTC { get; set; }
		public ZDateTime LastResetModelInfoLocal { get; set; }

		public ZString IsCubeProcessing { get; set; }

		public ZBool DeployToServer { get; set; }
		public ZBool EnableEtl { get; set; }
		public ZBool RedeployOnNextBID { get; set; }

		public enum CubeStatus
		{
			Not_Processed = 0,
			Processing = 1,
			Processed = 2
		}
	}
}
