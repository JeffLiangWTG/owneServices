using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Integration
{
	public static partial class Customs
	{
		public static partial class EUExitControl
		{
			public interface ICusExitConsignment : IBusiness
			{
				ZGuid PK { get; }
				object this[string propertyName] { get; set; }

				ZInt CXC_ClusterKey { get; set; }
				ZGuid CXC_CXH_Header { get; set; }
				ZString CXC_LocalReference { get; set; }
				ZString CXC_MovementReference { get; set; }
				ZString CXC_ReferenceNumber { get; set; }
				ZString CXC_Status { get; set; }
				ZDateTime CXC_SystemCreateTimeUtc { get; set; }
				ZString CXC_SystemCreateUser { get; set; }
				ZDateTime CXC_SystemLastEditTimeUtc { get; set; }
				ZString CXC_SystemLastEditUser { get; set; }
				ZString CXC_UniqueConsignmentReference { get; set; }
				IActiveBusinessObjectCollection<ICusExitConsignmentItem> CusExitConsignmentItems { get; }
			}
		}
	}
}
