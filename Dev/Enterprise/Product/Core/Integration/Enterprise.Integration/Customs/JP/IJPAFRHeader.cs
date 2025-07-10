using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Integration
{
	public static partial class Customs
	{
		public static partial class JP
		{
			public static partial class AFR
			{
				public interface IJPAFRHeader : IBusiness, ICancellable
				{
					ZGuid PK { get; }
					ZGuid JPH_ParentId { get; set; }
					ZString JPH_ParentTableCode { get; set; }
					ZString JPH_CarrierCode { get; set; }
					ZGuid JPH_GB_Branch { get; set; }
					ZBool JPH_OverrideFreightDefaults { get; set; }
					ZString JPH_JobReference { get; set; }
					ZString JPH_VesselName { get; set; }
					ZString JPH_Voyage { get; set; }
					ZString JPH_MasterBillNumber { get; set; }
					ZString JPH_MessageStatus { get; set; }
					ZString JPH_RL_NKDischarge { get; set; }
					ZString JPH_RL_NKLoading { get; set; }
					ZDateTime JPH_ETD { get; set; }
					ZDateTime JPH_ETA { get; set; }
					ZDateTime JPH_SystemCreateTimeUtc { get; set; }
					ZBool JPH_IsActive { get; set; }
					ZBool JPH_IsShippingLineEntry { get; set; }
				}
			}
		}
	}
}