using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Integration
{
	public static partial class Customs
	{
		public static partial class ASYCUDA
		{
			public static partial class TRETrade
			{
				public interface IAsycudaManifestHeader : ASYCUDA.IAsycudaManifestHeader
				{
					IBusinessObjectCollection Messages { get; }
					#region ETradeData fields
					ZString TempRegNo { get; set; }
					ZDateTime TempRegNoDate { get; set; }
					ZString DischargeRecordNo { get; set; }
					ZDateTime DischargeRecordNoDate { get; set; }
					ZString ClosureNo { get; set; }
					ZDateTime ClosureNoDate { get; set; }
					ZString InspectionClerk { get; set; }
					System.Collections.ICollection Bills { get; }
					ZString MessageMode { get; set; }
					ZBool IsImport { get; }
					ZString DepartureFlight { get; set; }
					ZString DepartureCountryCode { get; set; }
					#endregion
				}
			}
		}
	}
}
