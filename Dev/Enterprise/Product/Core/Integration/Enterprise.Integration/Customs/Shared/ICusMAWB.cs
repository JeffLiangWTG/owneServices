using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Integration
{
	public static partial class Customs
	{
		public static partial class Shared
		{
			public interface ICusMAWB : IBusiness, ICancellable
			{
				ZGuid PK { get; }

				#region CM_ApplicationCode

				ZString CM_ApplicationCode
				{
					get;
					set;
				}

				ZPropertyInfo CM_ApplicationCodeInfo
				{
					get;
				}

				#endregion

				#region CM_ArrivalDate

				ZDateTime CM_ArrivalDate
				{
					get;
					set;
				}

				ZPropertyInfo CM_ArrivalDateInfo
				{
					get;
				}

				#endregion

				#region CM_CustomsEntryNum

				ZString CM_CustomsEntryNum
				{
					get;
					set;
				}

				ZPropertyInfo CM_CustomsEntryNumInfo
				{
					get;
				}

				#endregion

				#region CM_CustomsStatus

				ZString CM_CustomsStatus
				{
					get;
					set;
				}

				ZPropertyInfo CM_CustomsStatusInfo
				{
					get;
				}

				#endregion

				#region CM_DateOfFirstArrival

				ZDateTime CM_DateOfFirstArrival
				{
					get;
					set;
				}

				ZPropertyInfo CM_DateOfFirstArrivalInfo
				{
					get;
				}

				#endregion

				#region CM_DepartureDate

				ZDateTime CM_DepartureDate
				{
					get;
					set;
				}

				ZPropertyInfo CM_DepartureDateInfo
				{
					get;
				}

				#endregion

				#region CM_FlightNo

				ZString CM_FlightNo
				{
					get;
					set;
				}

				ZPropertyInfo CM_FlightNoInfo
				{
					get;
				}

				#endregion

				#region CM_Folio

				ZString CM_Folio
				{
					get;
					set;
				}

				ZPropertyInfo CM_FolioInfo
				{
					get;
				}

				#endregion

				#region CM_GB

				ZGuid CM_GB
				{
					get;
					set;
				}

				ZPropertyInfo CM_GBInfo
				{
					get;
				}

				#endregion

				#region CM_HasProhibitedPackaging

				ZBool CM_HasProhibitedPackaging
				{
					get;
					set;
				}

				ZPropertyInfo CM_HasProhibitedPackagingInfo
				{
					get;
				}

				#endregion

				#region CM_HouseMessageIsSent

				ZBool CM_HouseMessageIsSent
				{
					get;
					set;
				}

				ZPropertyInfo CM_HouseMessageIsSentInfo
				{
					get;
				}

				#endregion

				#region CM_IsActive

				ZBool CM_IsActive
				{
					get;
					set;
				}

				ZPropertyInfo CM_IsActiveInfo
				{
					get;
				}

				#endregion

				#region CM_IsBureau

				ZBool CM_IsBureau
				{
					get;
					set;
				}

				ZPropertyInfo CM_IsBureauInfo
				{
					get;
				}

				#endregion

				#region CM_IsCTOMAWB

				ZBool CM_IsCTOMAWB
				{
					get;
					set;
				}

				ZPropertyInfo CM_IsCTOMAWBInfo
				{
					get;
				}

				#endregion

				#region CM_IsFinalManifest

				ZBool CM_IsFinalManifest
				{
					get;
					set;
				}

				ZPropertyInfo CM_IsFinalManifestInfo
				{
					get;
				}

				#endregion

				#region CM_JK

				ZGuid CM_JK
				{
					get;
					set;
				}

				ZPropertyInfo CM_JKInfo
				{
					get;
				}

				#endregion

				#region CM_SystemLastEditTimeUtc

				ZDateTime CM_SystemLastEditTimeUtc
				{
					get;
					set;
				}

				ZPropertyInfo CM_SystemLastEditTimeUtcInfo
				{
					get;
				}

				#endregion

				#region CM_SystemLastEditUser

				ZString CM_SystemLastEditUser
				{
					get;
					set;
				}

				ZPropertyInfo CM_SystemLastEditUserInfo
				{
					get;
				}

				#endregion

				#region CM_MasterHouseBill

				ZString CM_MasterHouseBill
				{
					get;
					set;
				}

				ZPropertyInfo CM_MasterHouseBillInfo
				{
					get;
				}

				#endregion

				#region CM_MAWB

				ZString CM_MAWB
				{
					get;
					set;
				}

				ZPropertyInfo CM_MAWBInfo
				{
					get;
				}

				#endregion

				#region CM_MessageReference

				ZString CM_MessageReference
				{
					get;
					set;
				}

				ZPropertyInfo CM_MessageReferenceInfo
				{
					get;
				}

				#endregion

				#region CM_OH_ResponsibleParty

				ZGuid CM_OH_ResponsibleParty
				{
					get;
					set;
				}

				ZPropertyInfo CM_OH_ResponsiblePartyInfo
				{
					get;
				}

				#endregion

				#region CM_OH_WebUser

				ZGuid CM_OH_WebUser
				{
					get;
					set;
				}

				ZPropertyInfo CM_OH_WebUserInfo
				{
					get;
				}

				#endregion

				#region CM_QuarantineCode

				ZString CM_QuarantineCode
				{
					get;
					set;
				}

				ZPropertyInfo CM_QuarantineCodeInfo
				{
					get;
				}

				#endregion

				#region CM_ResponsiblePartyID

				ZString CM_ResponsiblePartyID
				{
					get;
					set;
				}

				ZPropertyInfo CM_ResponsiblePartyIDInfo
				{
					get;
				}

				#endregion

				#region CM_RL_NKDischargePort

				ZString CM_RL_NKDischargePort
				{
					get;
					set;
				}

				ZPropertyInfo CM_RL_NKDischargePortInfo
				{
					get;
				}

				#endregion

				#region CM_RL_NKFirstArrivalPort

				ZString CM_RL_NKFirstArrivalPort
				{
					get;
					set;
				}

				ZPropertyInfo CM_RL_NKFirstArrivalPortInfo
				{
					get;
				}

				#endregion

				#region CM_RL_NKLoadPort

				ZString CM_RL_NKLoadPort
				{
					get;
					set;
				}

				ZPropertyInfo CM_RL_NKLoadPortInfo
				{
					get;
				}

				#endregion

				#region CM_RL_NKRoutePort1

				ZString CM_RL_NKRoutePort1
				{
					get;
					set;
				}

				ZPropertyInfo CM_RL_NKRoutePort1Info
				{
					get;
				}

				#endregion

				#region CM_RL_NKRoutePort2

				ZString CM_RL_NKRoutePort2
				{
					get;
					set;
				}

				ZPropertyInfo CM_RL_NKRoutePort2Info
				{
					get;
				}

				#endregion

				#region CM_RL_NKRoutePort3

				ZString CM_RL_NKRoutePort3
				{
					get;
					set;
				}

				ZPropertyInfo CM_RL_NKRoutePort3Info
				{
					get;
				}

				#endregion

				#region CM_SystemCreateTimeUtc

				ZDateTime CM_SystemCreateTimeUtc
				{
					get;
					set;
				}

				ZPropertyInfo CM_SystemCreateTimeUtcInfo
				{
					get;
				}

				#endregion

				#region CM_SystemCreateUser

				ZString CM_SystemCreateUser
				{
					get;
					set;
				}

				ZPropertyInfo CM_SystemCreateUserInfo
				{
					get;
				}

				#endregion

				BusinessObject DeConsolidatorOrgHeader
				{
					get;
				}

				BusinessObject EffectiveResponsiblePartyOrgHeader
				{
					get;
				}

				ZString ReasonEffectiveResponsiblePartyIsUnavailable
				{
					get;
				}
			}
		}
	}
}