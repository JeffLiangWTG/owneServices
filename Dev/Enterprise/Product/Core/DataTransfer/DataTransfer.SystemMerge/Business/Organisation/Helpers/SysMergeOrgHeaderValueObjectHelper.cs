using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.DataTransfer.Integration;
using Enterprise.DataTransfer.SystemMerge.Business;
using Xsd = Enterprise.DataTransfer.SystemMerge.XmlDefinition;

namespace Enterprise.DataTransfer.SystemMerge.DataAdapters
{
	public class SysMergeOrgHeaderValueObjectHelper
	{
		public SysMergeOrgHeaderValueObjectHelper(string errorContext)
		{
			this.ErrorContext = errorContext;
		}

		public readonly string ErrorContext;

		#region Import

		public void ImportFromValueObject(Xsd.SysMergeOrgHeader xsdOrg, OrgHeaderForDataTransfer org, IValueObjectImportContext context)
		{
			context.SetPropertyInfoValueIfValueNotEmpty(org.OH_CodeInfo, xsdOrg.Code);
			context.SetPropertyInfoValueIfValueNotEmpty(org.OH_IsActiveInfo, xsdOrg.IsActive.ToString());
			context.SetPropertyInfoValueIfValueNotEmpty(org.OH_FullNameInfo, xsdOrg.FullName);
			context.SetPropertyInfoValueIfValueNotEmpty(org.OH_IsConsigneeInfo, xsdOrg.IsConsignee.ToString());
			context.SetPropertyInfoValueIfValueNotEmpty(org.OH_IsConsignorInfo, xsdOrg.IsConsignor.ToString());
			context.SetPropertyInfoValueIfValueNotEmpty(org.OH_IsTransportClientInfo, xsdOrg.IsTransportClient.ToString());
			context.SetPropertyInfoValueIfValueNotEmpty(org.OH_IsWarehouseClientInfo, xsdOrg.IsWareHouseClient.ToString());
			context.SetPropertyInfoValueIfValueNotEmpty(org.OH_IsForwarderInfo, xsdOrg.IsForwarder.ToString());
			context.SetPropertyInfoValueIfValueNotEmpty(org.OH_IsShippingProviderInfo, xsdOrg.IsShippingProvider.ToString());
			context.SetPropertyInfoValueIfValueNotEmpty(org.OH_IsAirWholesalerInfo, xsdOrg.IsAirWholesaler.ToString());
			context.SetPropertyInfoValueIfValueNotEmpty(org.OH_IsSeaWholesalerInfo, xsdOrg.IsSeaWholesaler.ToString());
			context.SetPropertyInfoValueIfValueNotEmpty(org.OH_IsRailProviderInfo, xsdOrg.IsRailProvider.ToString());
			context.SetPropertyInfoValueIfValueNotEmpty(org.OH_IsLineHaulProviderInfo, xsdOrg.IsLineHaulProvider.ToString());
			context.SetPropertyInfoValueIfValueNotEmpty(org.OH_IsMiscFreightServicesInfo, xsdOrg.IsMiscFreightServices.ToString());
			context.SetPropertyInfoValueIfValueNotEmpty(org.OH_IsAirCTOInfo, xsdOrg.IsAirCTO.ToString());
			context.SetPropertyInfoValueIfValueNotEmpty(org.OH_IsAirLineInfo, xsdOrg.IsAirLine.ToString());
			context.SetPropertyInfoValueIfValueNotEmpty(org.OH_IsBrokerInfo, xsdOrg.IsBroker.ToString());
			context.SetPropertyInfoValueIfValueNotEmpty(org.OH_IsContainerYardInfo, xsdOrg.IsContainerPark.ToString());
			context.SetPropertyInfoValueIfValueNotEmpty(org.OH_IsLocalTransportInfo, xsdOrg.IsLocalTransport.ToString());
			context.SetPropertyInfoValueIfValueNotEmpty(org.OH_IsPackDepotInfo, xsdOrg.IsPackDepot.ToString());
			context.SetPropertyInfoValueIfValueNotEmpty(org.OH_IsSeaCTOInfo, xsdOrg.IsSeaCTO.ToString());
			context.SetPropertyInfoValueIfValueNotEmpty(org.OH_IsShippingLineInfo, xsdOrg.IsShippingLine.ToString());
			context.SetPropertyInfoValueIfValueNotEmpty(org.OH_IsUnpackDepotInfo, xsdOrg.IsUnpackDepot.ToString());
			context.SetPropertyInfoValueIfValueNotEmpty(org.OH_IsRailHeadInfo, xsdOrg.IsRailHead.ToString());
			context.SetPropertyInfoValueIfValueNotEmpty(org.OH_IsRoadFreightDepotInfo, xsdOrg.IsRoadFreightDepot.ToString());
			context.SetPropertyInfoValueIfValueNotEmpty(org.OH_IsShippingConsortiumInfo, xsdOrg.IsShippingConsortium.ToString());
			context.SetPropertyInfoValueIfValueNotEmpty(org.OH_IsFumigationContractorInfo, xsdOrg.IsFumigationContractor.ToString());
			context.SetPropertyInfoValueIfValueNotEmpty(org.OH_IsGlobalAccountInfo, xsdOrg.IsGlobalAccount.ToString());
			context.SetPropertyInfoValueIfValueNotEmpty(org.OH_IsNationalAccountInfo, xsdOrg.IsNationalAccount.ToString());
			context.SetPropertyInfoValueIfValueNotEmpty(org.OH_IsSalesLeadInfo, xsdOrg.IsSalesLead.ToString());
			context.SetPropertyInfoValueIfValueNotEmpty(org.OH_IsCompetitorInfo, xsdOrg.IsCompetitor.ToString());
			context.SetPropertyInfoValueIfValueNotEmpty(org.OH_IsTempAccountInfo, xsdOrg.IsTempAccount.ToString());
			context.SetPropertyInfoValueIfValueNotEmpty(org.OH_IsPersonalEffectsAccountInfo, xsdOrg.IsPersonalEffectsAccount.ToString());
			context.SetPropertyInfoValueIfValueNotEmpty(org.OH_IsUserFlag1Info, xsdOrg.IsUserFlag1.ToString());
			context.SetPropertyInfoValueIfValueNotEmpty(org.OH_IsUserFlag2Info, xsdOrg.IsUserFlag2.ToString());
			context.SetPropertyInfoValueIfValueNotEmpty(org.OH_IsUserFlag3Info, xsdOrg.IsUserFlag3.ToString());
			context.SetPropertyInfoValueIfValueNotEmpty(org.OH_IsUserFlag4Info, xsdOrg.IsUserFlag4.ToString());
			context.SetPropertyInfoValueIfValueNotEmpty(org.OH_IsUserFlag5Info, xsdOrg.IsUserFlag5.ToString());
			context.SetPropertyInfoValueIfValueNotEmpty(org.OH_IsUserFlag6Info, xsdOrg.IsUserFlag6.ToString());
			context.SetPropertyInfoValueIfValueNotEmpty(org.OH_IsUserFlag7Info, xsdOrg.IsUserFlag7.ToString());
			context.SetPropertyInfoValueIfValueNotEmpty(org.OH_IsUserFlag8Info, xsdOrg.IsUserFlag8.ToString());
			context.SetPropertyInfoValueIfValueNotEmpty(org.OH_IsUserFlag9Info, xsdOrg.IsUserFlag9.ToString());
			context.SetPropertyInfoValueIfValueNotEmpty(org.OH_IsUserFlag10Info, xsdOrg.IsUserFlag10.ToString());
			context.SetPropertyInfoValueIfValueNotEmpty(org.OH_IsUserFlag11Info, xsdOrg.IsUserFlag11.ToString());
			context.SetPropertyInfoValueIfValueNotEmpty(org.OH_IsUserFlag12Info, xsdOrg.IsUserFlag12.ToString());
			context.SetPropertyInfoValueIfValueNotEmpty(org.OH_IsUserFlag13Info, xsdOrg.IsUserFlag13.ToString());
			context.SetPropertyInfoValueIfValueNotEmpty(org.OH_IsUserFlag14Info, xsdOrg.IsUserFlag14.ToString());
			context.SetPropertyInfoValueIfValueNotEmpty(org.OH_IsUserFlag15Info, xsdOrg.IsUserFlag15.ToString());
			context.SetPropertyInfoValueIfValueNotEmpty(org.OH_IsUserFlag16Info, xsdOrg.IsUserFlag16.ToString());
			context.SetPropertyInfoValueIfValueNotEmpty(org.OH_IsUserFlag17Info, xsdOrg.IsUserFlag17.ToString());
			context.SetPropertyInfoValueIfValueNotEmpty(org.OH_IsUserFlag18Info, xsdOrg.IsUserFlag18.ToString());
			context.SetPropertyInfoValueIfValueNotEmpty(org.OH_IsUserFlag19Info, xsdOrg.IsUserFlag19.ToString());
			context.SetPropertyInfoValueIfValueNotEmpty(org.OH_IsUserFlag20Info, xsdOrg.IsUserFlag20.ToString());
			context.SetPropertyInfoValueIfValueNotEmpty(org.OH_IsUserFlag21Info, xsdOrg.IsUserFlag21.ToString());
			context.SetPropertyInfoValueIfValueNotEmpty(org.OH_IsUserFlag22Info, xsdOrg.IsUserFlag22.ToString());
			context.SetPropertyInfoValueIfValueNotEmpty(org.OH_IsUserFlag23Info, xsdOrg.IsUserFlag23.ToString());
			context.SetPropertyInfoValueIfValueNotEmpty(org.OH_IsUserFlag24Info, xsdOrg.IsUserFlag24.ToString());
			context.SetPropertyInfoValueIfValueNotEmpty(org.OH_IsUserFlag25Info, xsdOrg.IsUserFlag25.ToString());
			context.SetPropertyInfoValueIfValueNotEmpty(org.OH_IsUserFlag26Info, xsdOrg.IsUserFlag26.ToString());
			context.SetPropertyInfoValueIfValueNotEmpty(org.OH_IsUserFlag27Info, xsdOrg.IsUserFlag27.ToString());
			context.SetPropertyInfoValueIfValueNotEmpty(org.OH_IsUserFlag28Info, xsdOrg.IsUserFlag28.ToString());
			context.SetPropertyInfoValueIfValueNotEmpty(org.OH_IsUserFlag29Info, xsdOrg.IsUserFlag29.ToString());
			context.SetPropertyInfoValueIfValueNotEmpty(org.OH_IsUserFlag30Info, xsdOrg.IsUserFlag30.ToString());
			context.SetPropertyInfoValueIfValueNotEmpty(org.OH_IsUserFlag31Info, xsdOrg.IsUserFlag31.ToString());
			context.SetPropertyInfoValueIfValueNotEmpty(org.OH_IsUserFlag32Info, xsdOrg.IsUserFlag32.ToString());
			context.SetPropertyInfoValueIfValueNotEmpty(org.OH_RL_NKClosestPortInfo, xsdOrg.RL_NKClosestPort);
			context.SetPropertyInfoValueIfValueNotEmpty(org.OH_LanguageInfo, xsdOrg.Language);
			context.SetPropertyInfoValueIfValueNotEmpty(org.OH_ScreeningStatusInfo, xsdOrg.ScreeningStatus);
			context.SetPropertyInfoValueIfValueNotEmpty(org.OH_SystemLastEditUserInfo, xsdOrg.SystemLastEditUser);
			context.SetPropertyInfoValueIfValueNotEmpty(org.OH_SystemCreateUserInfo, xsdOrg.SystemCreateUser);

			if (xsdOrg.SystemLastEditTime != null)
			{
				context.SetPropertyInfoValueIfValueNotEmpty(org.OH_SystemLastEditTimeUtcInfo, (ZDateTime)xsdOrg.SystemLastEditTime);
			}

			if (xsdOrg.SystemCreateTime != null)
			{
				context.SetPropertyInfoValueIfValueNotEmpty(org.OH_SystemCreateTimeUtcInfo, (ZDateTime)xsdOrg.SystemCreateTime);
			}
		}

		#endregion

		#region Export

		public void ExportToValueObject(OrgHeaderForDataTransfer org, Xsd.SysMergeOrgHeader xsdOrg, INotifications notifications)
		{
			Xsd.SysMergeOrgHeader result = xsdOrg;

			result.PK = org.PK.ToString();

			if (org.OH_IsActive)
			{
				result.IsActive = org.OH_IsActive;
				result.IsActiveSpecified = true;
			}
			if (org.OH_IsConsignee)
			{
				result.IsConsignee = org.OH_IsConsignee;
				result.IsConsigneeSpecified = true;
			}
			if (org.OH_IsConsignor)
			{
				result.IsConsignor = org.OH_IsConsignor;
				result.IsConsignorSpecified = true;
			}
			if (org.OH_IsTransportClient)
			{
				result.IsTransportClient = org.OH_IsTransportClient;
				result.IsTransportClientSpecified = true;
			}
			if (org.OH_IsWarehouseClient)
			{
				result.IsWareHouseClient = org.OH_IsWarehouseClient;
				result.IsWareHouseClientSpecified = true;
			}
			if (org.OH_IsForwarder)
			{
				result.IsForwarder = org.OH_IsForwarder;
				result.IsForwarderSpecified = true;
			}
			if (org.OH_IsShippingProvider)
			{
				result.IsShippingProvider = org.OH_IsShippingProvider;
				result.IsShippingProviderSpecified = true;
			}
			if (org.OH_IsAirWholesaler)
			{
				result.IsAirWholesaler = org.OH_IsAirWholesaler;
				result.IsAirWholesalerSpecified = true;
			}
			if (org.OH_IsSeaWholesaler)
			{
				result.IsSeaWholesaler = org.OH_IsSeaWholesaler;
				result.IsSeaWholesalerSpecified = true;
			}
			if (org.OH_IsRailProvider)
			{
				result.IsRailProvider = org.OH_IsRailProvider;
				result.IsRailProviderSpecified = true;
			}
			if (org.OH_IsLocalTransport)
			{
				result.IsLocalTransport = org.OH_IsLocalTransport;
				result.IsLocalTransportSpecified = true;
			}
			if (org.OH_IsLineHaulProvider)
			{
				result.IsLineHaulProvider = org.OH_IsLineHaulProvider;
				result.IsLineHaulProviderSpecified = true;
			}
			if (org.OH_IsMiscFreightServices)
			{
				result.IsMiscFreightServices = org.OH_IsMiscFreightServices;
				result.IsMiscFreightServicesSpecified = true;
			}
			if (org.OH_IsAirCTO)
			{
				result.IsAirCTO = org.OH_IsAirCTO;
				result.IsAirCTOSpecified = true;
			}
			if (org.OH_IsAirLine)
			{
				result.IsAirLine = org.OH_IsAirLine;
				result.IsAirLineSpecified = true;
			}
			if (org.OH_IsBroker)
			{
				result.IsBroker = org.OH_IsBroker;
				result.IsBrokerSpecified = true;
			}
			if (org.OH_IsContainerYard)
			{
				result.IsContainerPark = org.OH_IsContainerYard;
				result.IsContainerParkSpecified = true;
			}

			if (org.OH_IsLocalTransport)
			{
				result.IsLocalTransport = org.OH_IsLocalTransport;
				result.IsLocalTransportSpecified = true;
			}
			if (org.OH_IsPackDepot)
			{
				result.IsPackDepot = org.OH_IsPackDepot;
				result.IsPackDepotSpecified = true;
			}
			if (org.OH_IsSeaCTO)
			{
				result.IsSeaCTO = org.OH_IsSeaCTO;
				result.IsSeaCTOSpecified = true;
			}
			if (org.OH_IsShippingLine)
			{
				result.IsShippingLine = org.OH_IsShippingLine;
				result.IsShippingLineSpecified = true;
			}
			if (org.OH_IsUnpackDepot)
			{
				result.IsUnpackDepot = org.OH_IsUnpackDepot;
				result.IsUnpackDepotSpecified = true;
			}
			if (org.OH_IsRailHead)
			{
				result.IsRailHead = org.OH_IsRailHead;
				result.IsRailHeadSpecified = true;
			}
			if (org.OH_IsRoadFreightDepot)
			{
				result.IsRoadFreightDepot = org.OH_IsRoadFreightDepot;
				result.IsRoadFreightDepotSpecified = true;
			}
			if (org.OH_IsShippingConsortium)
			{
				result.IsShippingConsortium = org.OH_IsShippingConsortium;
				result.IsShippingConsortiumSpecified = true;
			}
			if (org.OH_IsFumigationContractor)
			{
				result.IsFumigationContractor = org.OH_IsFumigationContractor;
				result.IsFumigationContractorSpecified = true;
			}
			if (org.OH_IsGlobalAccount)
			{
				result.IsGlobalAccount = org.OH_IsGlobalAccount;
				result.IsGlobalAccountSpecified = true;
			}
			if (org.OH_IsNationalAccount)
			{
				result.IsNationalAccount = org.OH_IsNationalAccount;
				result.IsNationalAccountSpecified = true;
			}
			if (org.OH_IsSalesLead)
			{
				result.IsSalesLead = org.OH_IsSalesLead;
				result.IsSalesLeadSpecified = true;
			}
			if (org.OH_IsCompetitor)
			{
				result.IsCompetitor = org.OH_IsCompetitor;
				result.IsCompetitorSpecified = true;
			}
			if (org.OH_IsTempAccount)
			{
				result.IsTempAccount = org.OH_IsTempAccount;
				result.IsTempAccountSpecified = true;
			}
			if (org.OH_IsPersonalEffectsAccount)
			{
				result.IsPersonalEffectsAccount = org.OH_IsPersonalEffectsAccount;
				result.IsPersonalEffectsAccountSpecified = true;
			}
			if (org.OH_IsUserFlag1)
			{
				result.IsUserFlag1 = org.OH_IsUserFlag1;
				result.IsUserFlag1Specified = true;
			}
			if (org.OH_IsUserFlag2)
			{
				result.IsUserFlag2 = org.OH_IsUserFlag2;
				result.IsUserFlag2Specified = true;
			}
			if (org.OH_IsUserFlag3)
			{
				result.IsUserFlag3 = org.OH_IsUserFlag3;
				result.IsUserFlag3Specified = true;
			}
			if (org.OH_IsUserFlag4)
			{
				result.IsUserFlag4 = org.OH_IsUserFlag4;
				result.IsUserFlag4Specified = true;
			}
			if (org.OH_IsUserFlag5)
			{
				result.IsUserFlag5 = org.OH_IsUserFlag5;
				result.IsUserFlag5Specified = true;
			}
			if (org.OH_IsUserFlag6)
			{
				result.IsUserFlag6 = org.OH_IsUserFlag6;
				result.IsUserFlag6Specified = true;
			}
			if (org.OH_IsUserFlag7)
			{
				result.IsUserFlag7 = org.OH_IsUserFlag7;
				result.IsUserFlag7Specified = true;
			}
			if (org.OH_IsUserFlag8)
			{
				result.IsUserFlag8 = org.OH_IsUserFlag8;
				result.IsUserFlag8Specified = true;
			}
			if (org.OH_IsUserFlag9)
			{
				result.IsUserFlag9 = org.OH_IsUserFlag9;
				result.IsUserFlag9Specified = true;
			}
			if (org.OH_IsUserFlag10)
			{
				result.IsUserFlag10 = org.OH_IsUserFlag10;
				result.IsUserFlag10Specified = true;
			}
			if (org.OH_IsUserFlag11)
			{
				result.IsUserFlag11 = org.OH_IsUserFlag11;
				result.IsUserFlag11Specified = true;
			}
			if (org.OH_IsUserFlag12)
			{
				result.IsUserFlag12 = org.OH_IsUserFlag12;
				result.IsUserFlag12Specified = true;
			}
			if (org.OH_IsUserFlag13)
			{
				result.IsUserFlag13 = org.OH_IsUserFlag13;
				result.IsUserFlag13Specified = true;
			}
			if (org.OH_IsUserFlag14)
			{
				result.IsUserFlag14 = org.OH_IsUserFlag14;
				result.IsUserFlag14Specified = true;
			}
			if (org.OH_IsUserFlag15)
			{
				result.IsUserFlag15 = org.OH_IsUserFlag15;
				result.IsUserFlag15Specified = true;
			}
			if (org.OH_IsUserFlag16)
			{
				result.IsUserFlag16 = org.OH_IsUserFlag16;
				result.IsUserFlag16Specified = true;
			}
			if (org.OH_IsUserFlag17)
			{
				result.IsUserFlag17 = org.OH_IsUserFlag17;
				result.IsUserFlag17Specified = true;
			}
			if (org.OH_IsUserFlag18)
			{
				result.IsUserFlag18 = org.OH_IsUserFlag18;
				result.IsUserFlag18Specified = true;
			}
			if (org.OH_IsUserFlag19)
			{
				result.IsUserFlag19 = org.OH_IsUserFlag19;
				result.IsUserFlag19Specified = true;
			}
			if (org.OH_IsUserFlag20)
			{
				result.IsUserFlag20 = org.OH_IsUserFlag20;
				result.IsUserFlag20Specified = true;
			}
			if (org.OH_IsUserFlag21)
			{
				result.IsUserFlag21 = org.OH_IsUserFlag21;
				result.IsUserFlag21Specified = true;
			}
			if (org.OH_IsUserFlag22)
			{
				result.IsUserFlag22 = org.OH_IsUserFlag22;
				result.IsUserFlag22Specified = true;
			}
			if (org.OH_IsUserFlag23)
			{
				result.IsUserFlag23 = org.OH_IsUserFlag23;
				result.IsUserFlag23Specified = true;
			}
			if (org.OH_IsUserFlag24)
			{
				result.IsUserFlag24 = org.OH_IsUserFlag24;
				result.IsUserFlag24Specified = true;
			}
			if (org.OH_IsUserFlag25)
			{
				result.IsUserFlag25 = org.OH_IsUserFlag25;
				result.IsUserFlag25Specified = true;
			}
			if (org.OH_IsUserFlag26)
			{
				result.IsUserFlag26 = org.OH_IsUserFlag26;
				result.IsUserFlag26Specified = true;
			}
			if (org.OH_IsUserFlag27)
			{
				result.IsUserFlag27 = org.OH_IsUserFlag27;
				result.IsUserFlag27Specified = true;
			}
			if (org.OH_IsUserFlag28)
			{
				result.IsUserFlag28 = org.OH_IsUserFlag28;
				result.IsUserFlag28Specified = true;
			}
			if (org.OH_IsUserFlag29)
			{
				result.IsUserFlag29 = org.OH_IsUserFlag29;
				result.IsUserFlag29Specified = true;
			}
			if (org.OH_IsUserFlag30)
			{
				result.IsUserFlag30 = org.OH_IsUserFlag30;
				result.IsUserFlag30Specified = true;
			}
			if (org.OH_IsUserFlag31)
			{
				result.IsUserFlag31 = org.OH_IsUserFlag31;
				result.IsUserFlag31Specified = true;
			}
			if (org.OH_IsUserFlag32)
			{
				result.IsUserFlag32 = org.OH_IsUserFlag32;
				result.IsUserFlag32Specified = true;
			}
			if (!org.OH_Code.IsEmpty)
			{
				result.Code = org.OH_Code;
			}

			if (!org.OH_FullName.IsEmpty)
			{
				result.FullName = org.OH_FullName;
			}

			if (!org.OH_RL_NKClosestPort.IsEmpty)
			{
				result.RL_NKClosestPort = org.OH_RL_NKClosestPort;
			}

			if (!org.OH_Language.IsEmpty)
			{
				result.Language = org.OH_Language;
			}

			if (!org.OH_ScreeningStatus.IsEmpty)
			{
				result.ScreeningStatus = org.OH_ScreeningStatus;
			}

			if (!org.OH_SystemLastEditTimeUtc.IsEmpty)
			{
				result.SystemLastEditTime = org.OH_SystemLastEditTimeUtc.ToDateTime();
				result.SystemLastEditTimeSpecified = true;
			}
			if (!org.OH_SystemLastEditUser.IsEmpty)
			{
				result.SystemLastEditUser = org.OH_SystemLastEditUser;
			}

			if (!org.OH_SystemCreateTimeUtc.IsEmpty)
			{
				result.SystemCreateTime = org.OH_SystemCreateTimeUtc.ToDateTime();
				result.SystemCreateTimeSpecified = true;
			}
			if (!org.OH_SystemCreateUser.IsEmpty)
			{
				result.SystemCreateUser = org.OH_SystemCreateUser;
			}
		}

		#endregion
	}
}
