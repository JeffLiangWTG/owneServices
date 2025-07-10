using System.Collections;
using CargoWise.Types;

namespace Enterprise.Integration
{
	public static partial class Customs
	{
		public static partial class CA
		{
			public interface ICusCAeMHMaster
			{
				ZGuid PK { get; }
				ZString BP_AmendReasonCode { get; set; }
				ZDateTime BP_ATA { get; set; }
				ZString BP_CBSACarrierCode { get; set; }
				ZString BP_CBSADischargePort { get; set; }
				ZString BP_CBSADischargeSubLocation { get; set; }
				ZString BP_CustomsStatus { get; set; }
				ZGuid BP_GB_Branch { get; set; }
				ZBool BP_IsActive { get; set; }
				ZString BP_MasterBill { get; set; }
				ZString BP_MasterHouseBill { get; set; }
				ZString BP_MasterHouseCCN { get; set; }
				ZString BP_MessageReference { get; set; }
				ZString BP_MessageStatus { get; set; }
				ZString BP_ModeOfTransport { get; set; }
				ZGuid BP_ParentID { get; set; }
				ZString BP_ParentTableCode { get; set; }
				ZString BP_PrimaryCCN { get; set; }
				ZString BP_RL_NKDiscPort { get; set; }
				ZDateTime BP_SystemCreateTimeUtc { get; set; }
				ZString BP_SystemCreateUser { get; set; }
				ICollection Containers { get; }
			}
		}
	}
}
