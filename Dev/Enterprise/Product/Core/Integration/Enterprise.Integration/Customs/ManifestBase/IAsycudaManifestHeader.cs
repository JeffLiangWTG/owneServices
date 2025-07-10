using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Integration
{
	public static partial class Customs
	{
		public static partial class ManifestBase
		{
			public interface IAsycudaManifestHeader : ICancellable
			{
				bool IsInDatabase { get; }
				ZGuid PK { get; }
				ZString AMA_Voyage { get; set; }
				ZString AMA_VehicleRegistration { get; set; }
				ZString AMA_TransportMode { get; set; }
				ZString AMA_SystemLastEditUser { get; set; }
				ZDateTime AMA_SystemLastEditTimeUtc { get; set; }
				ZString AMA_SystemCreateUser { get; set; }
				ZDateTime AMA_SystemCreateTimeUtc { get; set; }
				ZString AMA_VesselName { get; set; }
				ZString AMA_RN_NKConveyanceNationality { get; set; }
				ZString AMA_RadioCallSign { get; set; }
				ZString AMA_ParentTableCode { get; set; }
				ZGuid AMA_ParentId { get; set; }
				ZBool AMA_OverrideFreightDefaults { get; set; }
				ZGuid AMA_OA_DeconsolidateAddress { get; set; }
				ZGuid AMA_OA_Carrier { get; set; }
				ZString AMA_MasterInformation { get; set; }
				ZString AMA_JobReference { get; set; }
				ZInt AMA_ClusterKey { get; set; }
				ZString AMA_ContainerMode { get; set; }
				ZString AMA_ApplicationCode { get; set; }
				ZString AMA_AgentType { get; set; }
				ZString AMA_CarrierCode { get; set; }
				ZString AMA_CustomsOffice { get; set; }
				ZDateTime AMA_DateAtCustomsOffice { get; set; }
				ZString AMA_RN_NKCountry { get; set; }
				ZString AMA_ManifestType { get; set; }
				ZString AMA_MessageStatus { get; set; }
				ZString AMA_Nature { get; set; }
				ZGuid AMA_OA_ShippingAgent { get; set; }
				ZString AMA_RL_NKPortOfFirstArrival { get; set; }
				ZGuid AMA_GB { get; set; }
			}
		}
	}
}
