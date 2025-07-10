using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Integration
{
	public static partial class Customs
	{
		public static partial class EUExitControl
		{
			public interface ICusExitHeader : IBusiness
			{
				ZGuid PK { get; }
				object this[string propertyName] { get; set; }
				ZString CXH_ApplicationCode { get; set; }
				ZInt CXH_ClusterKey { get; set; }
				ZString CXH_CustomsProfile { get; set; }
				ZGuid CXH_GB_Branch { get; set; }
				ZGuid CXH_GC_Company { get; set; }
				ZString CXH_GS_NKCustomsAgent { get; set; }
				ZString CXH_JobReference { get; set; }
				ZGuid CXH_OA_Carrier { get; set; }
				ZGuid CXH_OC_CarrierContact { get; set; }
				ZGuid CXH_OH_Exporter { get; set; }
				ZString CXH_OwnerReference { get; set; }
				ZGuid CXH_ParentID { get; set; }
				ZString CXH_ParentTableCode { get; set; }
				ZDateTime CXH_SystemCreateTimeUtc { get; set; }
				ZString CXH_SystemCreateUser { get; set; }
				ZDateTime CXH_SystemLastEditTimeUtc { get; set; }
				ZString CXH_SystemLastEditUser { get; set; }
				IActiveBusinessObjectCollection<ICusExitConsignment> CusExitConsignments { get; }
				IActiveBusinessObjectCollection<ICusExitReport> CusExitReports { get; }
			}
		}
	}
}
