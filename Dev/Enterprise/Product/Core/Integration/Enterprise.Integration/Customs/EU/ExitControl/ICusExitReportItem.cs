using CargoWise.Types;

namespace Enterprise.Integration
{
	public static partial class Customs
	{
		public static partial class EUExitControl
		{
			public interface ICusExitReportItem
			{
				ZGuid PK { get; }
				object this[string propertyName] { get; set; }

				ZGuid ERI_CCI_ConsignmentItem { get; set; }
				ZGuid ERI_CER_Report { get; set; }
				ZInt ERI_ClusterKey { get; set; }
				ZGuid ERI_CXP_Package { get; set; }
				ZDecimal ERI_GrossMass { get; set; }
				ZDecimal ERI_NetMass { get; set; }
				ZInt ERI_Quantity { get; set; }
				ZDateTime ERI_SystemCreateTimeUtc { get; set; }
				ZString ERI_SystemCreateUser { get; set; }
				ZDateTime ERI_SystemLastEditTimeUtc { get; set; }
				ZString ERI_SystemLastEditUser { get; set; }
			}
		}
	}
}
