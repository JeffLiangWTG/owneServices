using CargoWise.Types;

namespace Enterprise.Integration
{
	public static partial class Customs
	{
		public static partial class EU
		{
			public interface ICusAuthorizationUsage
			{
				ZGuid PK { get; }
				ZInt AGC_ClusterKey { get; set; }
				ZString AGC_Code { get; set; }
				ZBool AGC_IsSystemGenerated { get; set; }
				ZString AGC_Number { get; set; }
				ZGuid AGC_OH_Owner { get; set; }
				ZGuid AGC_ParentID { get; set; }
				ZString AGC_ParentTableCode { get; set; }
				ZString AGC_Location { get; set; }
				ZDateTime AGC_SystemCreateTimeUtc { get; set; }
				ZString AGC_SystemCreateUser { get; set; }
				ZDateTime AGC_SystemLastEditTimeUtc { get; set; }
				ZString AGC_SystemLastEditUser { get; set; }
			}
		}
	}
}
