using CargoWise.Types;

namespace Enterprise.Integration
{
	public static partial class Customs
	{
		public static partial class ManifestBase
		{
			public interface IEUMemberStateCommunication
			{
				public ZGuid PK { get; }

				public ZInt EUS_ClusterKey { get; set; }

				public ZString EUS_HouseBillNumber { get; set; }

				public ZString EUS_HouseBillType { get; set; }

				public ZString EUS_Identifier { get; set; }

				public ZBool EUS_IncludeScreeningDetails { get; set; }

				public ZString EUS_MasterBillNumber { get; set; }

				public ZString EUS_MasterBillType { get; set; }

				public ZString EUS_MemberState { get; set; }

				public ZString EUS_MessageElement { get; set; }

				public ZGuid EUS_ParentId { get; set; }

				public ZString EUS_ParentTableCode { get; set; }

				public ZString EUS_ScreeningMethod { get; set; }

				public ZString EUS_Status { get; set; }

				public ZDateTime EUS_SystemCreateTimeUtc { get; set; }

				public ZString EUS_SystemCreateUser { get; set; }

				public ZDateTime EUS_SystemLastEditTimeUtc { get; set; }

				public ZString EUS_SystemLastEditUser { get; set; }

				public ZString EUS_TransportDocumentType { get; set; }

				public ZString EUS_Type { get; set; }
			}
		}
	}
}
