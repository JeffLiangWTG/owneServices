using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Customs.CA.Business;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Customs.CA.Module.CAExternalModuleColumnsAndFiltersProvider;

namespace Enterprise.Customs.CA.Module
{
	public class HouseBilleManifestFilterStripBusinessObject : FilterStripBusinessObject
	{
		#region SuppressResourceStringsCheckRegion

		public static class Constants
		{
			#region Master Details filters

			public const string MasterBill = "Master Bill";
			public const string MasterHouseBill = "Master House Bill";
			public const string PrimaryCCN = "Primary CCN";
			public const string MasterHouseCCN = "Master House CCN";
			public const string CBSADischargePort = "CBSA Discharge Port";
			public const string CBSADischargeSubLocation = "CBSA Discharge Sub-Location";
			public const string CBSACarrierCode = "CBSA Carrier Code";
			public const string ModeOfTransport = "Mode of Transport";
			public const string PortOfDischarge = "Port of Discharge";
			public const string ETA = "ETA";
			public const string ATA = "ATA";
			public const string ContainerNumber = "Container Number";
			public const string PlaceOfConsolidation = "Place of Consolidation";
			public const string Consolidator = "Consolidator";
			public const string MasterMessageStatus = "Master Close Message Status";
			public const string MasterCustomsStatus = "Master Close Customs Status";
			public const string MasterMessageReference = "Master Close Message Reference";
			public const string MasterBillLatestD4Notice = "Master Bill Latest D4 Notice";

			public static MultilingualString MasterBillMultilingualDescription => ResString.GetMultilingualString("CA|HouseBilleManifestFilterStripBusinessObject|MasterBill", MasterBill);
			public static MultilingualString MasterHouseBillMultilingualDescription => ResString.GetMultilingualString("CA|HouseBilleManifestFilterStripBusinessObject|MasterHouseBill", MasterHouseBill);
			public static MultilingualString PrimaryCCNMultilingualDescription => ResString.GetMultilingualString("CA|HouseBilleManifestFilterStripBusinessObject|PrimaryCCN", PrimaryCCN);
			public static MultilingualString MasterHouseCCNMultilingualDescription => ResString.GetMultilingualString("CA|HouseBilleManifestFilterStripBusinessObject|MasterHouseCCN", MasterHouseCCN);
			public static MultilingualString CBSADischargePortMultilingualDescription => ResString.GetMultilingualString("CA|HouseBilleManifestFilterStripBusinessObject|CBSADischargePort", CBSADischargePort);
			public static MultilingualString CBSADischargeSubLocationMultilingualDescription => ResString.GetMultilingualString("CA|HouseBilleManifestFilterStripBusinessObject|CBSADischargeSubLocation", CBSADischargeSubLocation);
			public static MultilingualString CBSACarrierCodeMultilingualDescription => ResString.GetMultilingualString("CA|HouseBilleManifestFilterStripBusinessObject|CBSACarrierCode", CBSACarrierCode);
			public static MultilingualString ModeOfTransportMultilingualDescription => ResString.GetMultilingualString("CA|HouseBilleManifestFilterStripBusinessObject|ModeOfTransport", ModeOfTransport);
			public static MultilingualString PortOfDischargeMultilingualDescription => ResString.GetMultilingualString("CA|HouseBilleManifestFilterStripBusinessObject|PortOfDischarge", PortOfDischarge);
			public static MultilingualString ETAMultilingualDescription => ResString.GetMultilingualString("CA|HouseBilleManifestFilterStripBusinessObject|ETA", ETA);
			public static MultilingualString ATAMultilingualDescription => ResString.GetMultilingualString("CA|HouseBilleManifestFilterStripBusinessObject|ATA", ATA);
			public static MultilingualString ContainerNumberMultilingualDescription => ResString.GetMultilingualString("CA|HouseBilleManifestFilterStripBusinessObject|ContainerNumber", ContainerNumber);
			public static MultilingualString PlaceOfConsolidationMultilingualDescription => ResString.GetMultilingualString("CA|HouseBilleManifestFilterStripBusinessObject|PlaceOfConsolidation", PlaceOfConsolidation);
			public static MultilingualString ConsolidatorMultilingualDescription => ResString.GetMultilingualString("CA|HouseBilleManifestFilterStripBusinessObject|Consolidator", Consolidator);
			public static MultilingualString MasterMessageStatusMultilingualDescription => ResString.GetMultilingualString("CA|HouseBilleManifestFilterStripBusinessObject|MasterMessageStatus", MasterMessageStatus);
			public static MultilingualString MasterCustomsStatusMultilingualDescription => ResString.GetMultilingualString("CA|HouseBilleManifestFilterStripBusinessObject|MasterCustomsStatus", MasterCustomsStatus);
			public static MultilingualString MasterMessageReferenceMultilingualDescription => ResString.GetMultilingualString("CA|HouseBilleManifestFilterStripBusinessObject|MasterMessageReference", MasterMessageReference);
			public static MultilingualString MasterBillLatestD4NoticeMultilingualDescription => ResString.GetMultilingualString("CA|HouseBilleManifestFilterStripBusinessObject|MasterBillLatestD4Notice", MasterBillLatestD4Notice);

			#endregion

			#region House Details filters

			public const string HouseBill = "House Bill";
			public const string HouseCCN = "House CCN";
			public const string CBSAReleasePort = "Port of Dest./Exit";
			public const string CBSAReleaseSubLocation = "Dest./Exit Sub-locations";
			public const string UCR = "UCR";
			public const string MovementType = "Movement Type";
			public const string HouseMessageStatus = "House Bill Message Status";
			public const string HouseCustomsStatus = "House Bill Customs Status";
			public const string AmendmentReason = "Amendment Reason";
			public const string Consignee = "Consignee";
			public const string Shipper = "Shipper";
			public const string Delivery = "Delivery";
			public const string NotifyParty = "Notify Party";
			public const string SecondaryNotifyParty = "Secondary Notify Party";
			public const string HouseMessageReference = "House Bill Message Reference";
			public const string HouseBillLatestD4Notice = "House Bill Latest D4 Notice";

			public static MultilingualString HouseBillMultilingualDescription => ResString.GetMultilingualString("CA|HouseBilleManifestFilterStripBusinessObject|HouseBill", HouseBill);
			public static MultilingualString HouseCCNMultilingualDescription => ResString.GetMultilingualString("CA|HouseBilleManifestFilterStripBusinessObject|HouseCCN", HouseCCN);
			public static MultilingualString CBSAReleasePortMultilingualDescription => ResString.GetMultilingualString("CA|HouseBilleManifestFilterStripBusinessObject|CBSAReleasePort", CBSAReleasePort);
			public static MultilingualString CBSAReleaseSubLocationMultilingualDescription => ResString.GetMultilingualString("CA|HouseBilleManifestFilterStripBusinessObject|CBSAReleaseSubLocation", CBSAReleaseSubLocation);
			public static MultilingualString UCRMultilingualDescription => ResString.GetMultilingualString("CA|HouseBilleManifestFilterStripBusinessObject|UCR", UCR);
			public static MultilingualString MovementTypeMultilingualDescription => ResString.GetMultilingualString("CA|HouseBilleManifestFilterStripBusinessObject|MovementType", MovementType);
			public static MultilingualString HouseMessageStatusMultilingualDescription => ResString.GetMultilingualString("CA|HouseBilleManifestFilterStripBusinessObject|HouseMessageStatus", HouseMessageStatus);
			public static MultilingualString HouseCustomsStatusMultilingualDescription => ResString.GetMultilingualString("CA|HouseBilleManifestFilterStripBusinessObject|HouseCustomsStatus", HouseCustomsStatus);
			public static MultilingualString AmendmentReasonMultilingualDescription => ResString.GetMultilingualString("CA|HouseBilleManifestFilterStripBusinessObject|AmendmentReason", AmendmentReason);
			public static MultilingualString ConsigneeMultilingualDescription => ResString.GetMultilingualString("CA|HouseBilleManifestFilterStripBusinessObject|Consignee", Consignee);
			public static MultilingualString ShipperMultilingualDescription => ResString.GetMultilingualString("CA|HouseBilleManifestFilterStripBusinessObject|Shipper", Shipper);
			public static MultilingualString DeliveryMultilingualDescription => ResString.GetMultilingualString("CA|HouseBilleManifestFilterStripBusinessObject|Delivery", Delivery);
			public static MultilingualString NotifyPartyMultilingualDescription => ResString.GetMultilingualString("CA|HouseBilleManifestFilterStripBusinessObject|NotifyParty", NotifyParty);
			public static MultilingualString SecondaryNotifyPartyMultilingualDescription => ResString.GetMultilingualString("CA|HouseBilleManifestFilterStripBusinessObject|SecondaryNotifyParty", SecondaryNotifyParty);
			public static MultilingualString HouseMessageReferenceMultilingualDescription => ResString.GetMultilingualString("CA|HouseBilleManifestFilterStripBusinessObject|HouseMessageReference", HouseMessageReference);
			public static MultilingualString HouseBillLatestD4NoticeMultilingualDescription => ResString.GetMultilingualString("CA|HouseBilleManifestFilterStripBusinessObject|HouseBillLatestD4Notice", HouseBillLatestD4Notice);

			#endregion
		}

		#endregion

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var result = new ModuleFilterCollection();

			AddMasterBillFilters(result);
			AddHouseBillFilters(result);

			return result;
		}

		#region Master Bill filters

		void AddMasterBillFilters(ModuleFilterCollection moduleFilters)
		{
			moduleFilters.AddTextFilter(Constants.MasterBill, CusCAeMHMasterSchema.BP_MasterBill)
				.MultilingualDescription = Constants.MasterBillMultilingualDescription;

			moduleFilters.AddTextFilter(Constants.MasterHouseBill, CusCAeMHMasterSchema.BP_MasterHouseBill)
				.MultilingualDescription = Constants.MasterHouseBillMultilingualDescription;

			moduleFilters.AddTextFilter(Constants.PrimaryCCN, CusCAeMHMasterSchema.BP_PrimaryCCN)
				.MultilingualDescription = Constants.PrimaryCCNMultilingualDescription;

			moduleFilters.AddTextFilter(Constants.MasterHouseCCN, CusCAeMHMasterSchema.BP_MasterHouseCCN)
				.MultilingualDescription = Constants.MasterHouseCCNMultilingualDescription;

			moduleFilters.AddTextFilter(Constants.ModeOfTransport, CusCAeMHMasterSchema.BP_ModeOfTransport, Factory.GetCachedValue<CusCAeMHTransportModeList>())
				.MultilingualDescription = Constants.ModeOfTransportMultilingualDescription;

			moduleFilters.AddTextFilter(Constants.ContainerNumber, GetContainerNumberQuery)
				.WithMaxLengthOf<ModuleTextFilter>(CusCAeMHContainerSchema.BQ_ContainerNumber)
				.MultilingualDescription = Constants.ContainerNumberMultilingualDescription;

			var masterMessageStatusFilter = new ModuleTextFilter(Constants.MasterMessageStatus, x => new ZQuery(CusCAeMHMasterSchema.BP_MessageStatus, CAExternalColumnsHelper.GetQueryValueForMessageStatusCode(x)), CAExternalColumnsHelper.GetMessageStatusListForFilter(Factory));
			masterMessageStatusFilter.Category = FilterCategories.StatusAndFlags;
			masterMessageStatusFilter.MultilingualDescription = Constants.MasterMessageStatusMultilingualDescription;
			moduleFilters.AddFilter(masterMessageStatusFilter);

			var masterBillLatestD4NoticeFilter = new EqualModuleStatusFilter(Constants.MasterBillLatestD4Notice, Constants.MasterBillLatestD4NoticeMultilingualDescription, (c, v) => new ZQuery(CusCAeMHMasterSchema.BP_D4MessageStatus, c, CAExternalColumnsHelper.GetQueryValueForMessageStatusCode(v)), () => NoticeReasonCodes);
			masterBillLatestD4NoticeFilter.Category = FilterCategories.StatusAndFlags;
			moduleFilters.AddFilter(masterBillLatestD4NoticeFilter);

			var masterCustomsStatusFilter = new ModuleTextFilter(Constants.MasterCustomsStatus, x => new ZQuery(CusCAeMHMasterSchema.BP_CustomsStatus, x), Factory.GetCachedValue<EManifestForwarderJobStatusList>());
			masterCustomsStatusFilter.Category = FilterCategories.StatusAndFlags;
			masterCustomsStatusFilter.MultilingualDescription = Constants.MasterCustomsStatusMultilingualDescription;
			moduleFilters.AddFilter(masterCustomsStatusFilter);

			moduleFilters.AddTextFilter(Constants.MasterMessageReference, CusCAeMHMasterSchema.BP_MessageReference)
				.MultilingualDescription = Constants.MasterMessageReferenceMultilingualDescription;

			var cbsaDischargePortFilter = moduleFilters.AddNkFilter(Constants.CBSADischargePort, CusCAeMHMasterSchema.BP_CBSADischargePort, ModuleIDs.Customs.Universal.ZZRefCusCodeList, Offices);
			cbsaDischargePortFilter.ForeignCodeColumnOverride = ZZRefCusCodeListCombinedSchema.ZZD_Code;
			cbsaDischargePortFilter.MultilingualDescription = Constants.CBSADischargePortMultilingualDescription;

			var dischargeSubLocationFilter = moduleFilters.AddNkFilter(Constants.CBSADischargeSubLocation, CusCAeMHMasterSchema.BP_CBSADischargeSubLocation, ModuleIDs.Customs.CA.SubLocation, SubLocations);
			dischargeSubLocationFilter.ForeignCodeColumnOverride = ZZRefCusCodeListCombinedSchema.ZZD_Code;
			dischargeSubLocationFilter.MultilingualDescription = Constants.CBSADischargeSubLocationMultilingualDescription;

			var cbsaCarrierCodeFilter = moduleFilters.AddNkFilter(Constants.CBSACarrierCode, CusCAeMHMasterSchema.BP_CBSACarrierCode, ModuleIDs.Customs.Universal.ZZRefCarrier, Carriers);
			cbsaCarrierCodeFilter.ForeignCodeColumnOverride = ZZRefCarrierCombinedSchema.ZZ4_Code;
			cbsaCarrierCodeFilter.MultilingualDescription = Constants.CBSACarrierCodeMultilingualDescription;

			moduleFilters.AddNkFilter(Constants.PortOfDischarge, CusCAeMHMasterSchema.BP_RL_NKDiscPort, ModuleIDs.RefUNLOCO, UNLOCOs)
				.MultilingualDescription = Constants.PortOfDischargeMultilingualDescription;

			moduleFilters.AddDateFilter(Constants.ETA, CusCAeMHMasterSchema.BP_ETA)
				.MultilingualDescription = Constants.ETAMultilingualDescription;

			moduleFilters.AddDateFilter(Constants.ATA, CusCAeMHMasterSchema.BP_ATA)
				.MultilingualDescription = Constants.ATAMultilingualDescription;

			moduleFilters.AddGuidFilter(Constants.PlaceOfConsolidation, ModuleIDs.Organisation, GetPlaceOfConsolidationQuery, Organisations)
				.MultilingualDescription = Constants.PlaceOfConsolidationMultilingualDescription;

			moduleFilters.AddGuidFilter(Constants.Consolidator, ModuleIDs.Organisation, GetConsolidatorQuery, Organisations)
				.MultilingualDescription = Constants.ConsolidatorMultilingualDescription;
		}

		ZQuery GetContainerNumberQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var result = new ZDBOnlyQuery(typeof(CusCAeMHMaster));
			var containerQuery = new ZDBOnlySubQuery(typeof(CusCAeMHContainer), CusCAeMHContainerSchema.BQ_BP_Master);
			containerQuery.AddToFilter(CusCAeMHContainerSchema.BQ_ContainerNumber, comparisonOperator, value);
			result.AddSubQuery(containerQuery, JoinCondition.And);
			return result;
		}

		ZQuery GetPlaceOfConsolidationQuery(ZGuid value)
		{
			var result = new ZDBOnlyQuery(typeof(CusCAeMHMaster));
			result.AddFilterAndZSQLParameterCollection(GetJobDocAddressOrganisationQueryText(DocAddressTypes.Codes.PlaceOfConsolidation),
				new ZSqlParameterCollection(ZSqlParameter.New("@organisationPK", value, OrgAddressSchema.OA_OH)));
			return result;
		}

		ZQuery GetConsolidatorQuery(ZGuid value)
		{
			var result = new ZDBOnlyQuery(typeof(CusCAeMHMaster));
			result.AddFilterAndZSQLParameterCollection(GetJobDocAddressOrganisationQueryText(DocAddressTypes.Codes.Consolidator),
				new ZSqlParameterCollection(ZSqlParameter.New("@organisationPK", value, OrgAddressSchema.OA_OH)));
			return result;
		}

		ZString GetJobDocAddressOrganisationQueryText(ZString docAddressType)
		{
			return string.Format(@"{0} IN ( SELECT {1} FROM {2} INNER JOIN {3} ON {4} = {5} AND {6} = '{7}' AND {8} = '{9}' WHERE {10} = @organisationPK )",
				CusCAeMHMasterSchema.Constants.PK, //0
				JobDocAddressSchema.Constants.E2_ParentID, //1
				JobDocAddressSchema.Constants.TableName, //2
				OrgAddressSchema.Constants.TableName, //3
				JobDocAddressSchema.Constants.E2_OA_Address, //4
				OrgAddressSchema.Constants.PK, //5
				JobDocAddressSchema.Constants.E2_AddressType, //6
				docAddressType, //7
				JobDocAddressSchema.Constants.E2_ParentTableCode, //8
				CusCAeMHMasterSchema.Constants.Prefix, //9
				OrgAddressSchema.Constants.OA_OH //10
			);
		}

		#endregion

		#region House Bill filters

		void AddHouseBillFilters(ModuleFilterCollection moduleFilters)
		{
			moduleFilters.AddTextFilter(Constants.HouseBill, (x, y) => CAeMHMasterQueryCreator.CreateQueryWithHouseSubQuery(CusCAeMHHouseSchema.BW_HouseBill, x, y))
				.WithMaxLengthOf<ModuleTextFilter>(CusCAeMHHouseSchema.BW_HouseBill)
				.MultilingualDescription = Constants.HouseBillMultilingualDescription;

			moduleFilters.AddTextFilter(Constants.HouseCCN, (x, y) => CAeMHMasterQueryCreator.CreateQueryWithHouseSubQuery(CusCAeMHHouseSchema.BW_HouseCCN, x, y))
				.WithMaxLengthOf<ModuleTextFilter>(CusCAeMHHouseSchema.BW_HouseCCN)
				.MultilingualDescription = Constants.HouseCCNMultilingualDescription;

			moduleFilters.AddTextFilter(Constants.UCR, (x, y) => CAeMHMasterQueryCreator.CreateQueryWithHouseSubQuery(CusCAeMHHouseSchema.BW_UCR, x, y))
				.WithMaxLengthOf<ModuleTextFilter>(CusCAeMHHouseSchema.BW_UCR)
				.MultilingualDescription = Constants.UCRMultilingualDescription;

			moduleFilters.AddTextFilter(Constants.MovementType, (x) => CAeMHMasterQueryCreator.CreateQueryWithHouseSubQuery(CusCAeMHHouseSchema.BW_MovementType, x), Factory.GetCachedValue<eMHMovementTypeList>()).WithMaxLengthOf<ModuleTextFilter>(CusCAeMHHouseSchema.BW_MovementType)
				.MultilingualDescription = Constants.MovementTypeMultilingualDescription;

			var houseMessageStatusFilter = new ModuleTextFilter(Constants.HouseMessageStatus, (x) => CAeMHMasterQueryCreator.CreateQueryWithHouseSubQuery(CusCAeMHHouseSchema.BW_MessageStatus, CAExternalColumnsHelper.GetQueryValueForMessageStatusCode(x)), CAExternalColumnsHelper.GetMessageStatusListForFilter(Factory));
			houseMessageStatusFilter.Category = FilterCategories.StatusAndFlags;
			houseMessageStatusFilter.MultilingualDescription = Constants.HouseMessageStatusMultilingualDescription;
			moduleFilters.AddFilter(houseMessageStatusFilter);

			var houseCustomsStatusFilter = new ModuleTextFilter(Constants.HouseCustomsStatus, (x) => CAeMHMasterQueryCreator.CreateQueryWithHouseSubQuery(CusCAeMHHouseSchema.BW_CustomsStatus, x), Factory.GetCachedValue<EManifestForwarderJobStatusList>());
			houseCustomsStatusFilter.Category = FilterCategories.StatusAndFlags;
			houseCustomsStatusFilter.MultilingualDescription = Constants.HouseCustomsStatusMultilingualDescription;
			moduleFilters.AddFilter(houseCustomsStatusFilter);

			moduleFilters.AddTextFilter(Constants.AmendmentReason, (x) => CAeMHMasterQueryCreator.CreateQueryWithHouseSubQuery(CusCAeMHHouseSchema.BW_AmendReasonCode, x), Factory.GetCachedValue<EManifestAmendmentReasonCodes>()).WithMaxLengthOf<ModuleTextFilter>(CusCAeMHHouseSchema.BW_AmendReasonCode)
				.MultilingualDescription = Constants.AmendmentReasonMultilingualDescription;

			moduleFilters.AddTextFilter(Constants.HouseMessageReference, (x, y) => CAeMHMasterQueryCreator.CreateQueryWithHouseSubQuery(CusCAeMHHouseSchema.BW_MessageReference, x, y))
				.WithMaxLengthOf<ModuleTextFilter>(CusCAeMHHouseSchema.BW_MessageReference)
				.MultilingualDescription = Constants.HouseMessageReferenceMultilingualDescription;

			var houseBillLatestD4NoticeFilter = new EqualModuleStatusFilter(Constants.HouseBillLatestD4Notice, Constants.HouseBillLatestD4NoticeMultilingualDescription, (c, v) => CAeMHMasterQueryCreator.CreateQueryWithHouseSubQuery(CusCAeMHHouseSchema.BW_D4MessageStatus, c, CAExternalColumnsHelper.GetQueryValueForMessageStatusCode(v)), () => NoticeReasonCodes);
			houseBillLatestD4NoticeFilter.Category = FilterCategories.StatusAndFlags;
			moduleFilters.AddFilter(houseBillLatestD4NoticeFilter);

			moduleFilters.AddNkFilter(Constants.CBSAReleasePort, (x) => CAeMHMasterQueryCreator.CreateQueryWithHouseSubQuery(CusCAeMHHouseSchema.BW_CBSAReleasePort, x), ModuleIDs.Customs.Universal.ZZRefCusCodeList, Offices)
				.MultilingualDescription = Constants.CBSAReleasePortMultilingualDescription;

			moduleFilters.AddNkFilter(Constants.CBSAReleaseSubLocation, (x) => CAeMHMasterQueryCreator.CreateQueryWithHouseSubQuery(CusCAeMHHouseSchema.BW_CBSAReleaseSubLocation, x), ModuleIDs.Customs.CA.SubLocation, SubLocations)
				.MultilingualDescription = Constants.CBSAReleaseSubLocationMultilingualDescription;

			moduleFilters.AddGuidFilter(Constants.Consignee, ModuleIDs.Organisation, GetHouseConsigneeQuery, Organisations)
				.MultilingualDescription = Constants.ConsigneeMultilingualDescription;

			moduleFilters.AddGuidFilter(Constants.Shipper, ModuleIDs.Organisation, GetHouseShipperQuery, Organisations)
				.MultilingualDescription = Constants.ShipperMultilingualDescription;

			moduleFilters.AddGuidFilter(Constants.NotifyParty, ModuleIDs.Organisation, GetHouseNotifyPartyQuery, Organisations)
				.MultilingualDescription = Constants.NotifyPartyMultilingualDescription;

			moduleFilters.AddGuidFilter(Constants.SecondaryNotifyParty, ModuleIDs.Organisation, GetHouseSecondaryNotifyPartyQuery, Organisations)
				.MultilingualDescription = Constants.SecondaryNotifyPartyMultilingualDescription;

			moduleFilters.AddGuidFilter(Constants.Delivery, ModuleIDs.Organisation, GetHouseDeliveryQuery, Organisations)
				.MultilingualDescription = Constants.DeliveryMultilingualDescription;
		}

		ZQuery GetHouseConsigneeQuery(ZGuid value)
		{
			var result = new ZDBOnlyQuery(typeof(CusCAeMHMaster));
			result.AddFilterAndZSQLParameterCollection(GetHouseJobDocAddressOrganisationQuery(DocAddressTypes.Codes.ConsigneeDocumentaryAddress), new ZSqlParameterCollection(
				ZSqlParameter.New("@organisationPK", value, OrgAddressSchema.OA_OH)));
			return result;
		}

		ZQuery GetHouseShipperQuery(ZGuid value)
		{
			var result = new ZDBOnlyQuery(typeof(CusCAeMHMaster));
			result.AddFilterAndZSQLParameterCollection(GetHouseJobDocAddressOrganisationQuery(DocAddressTypes.Codes.ConsignorDocumentaryAddress), new ZSqlParameterCollection(
				ZSqlParameter.New("@organisationPK", value, OrgAddressSchema.OA_OH)));
			return result;
		}

		ZQuery GetHouseDeliveryQuery(ZGuid value)
		{
			var result = new ZDBOnlyQuery(typeof(CusCAeMHMaster));
			result.AddFilterAndZSQLParameterCollection(GetHouseJobDocAddressOrganisationQuery(DocAddressTypes.Codes.ConsigneePickupDeliveryAddress), new ZSqlParameterCollection(
				ZSqlParameter.New("@organisationPK", value, OrgAddressSchema.OA_OH)));
			return result;
		}

		ZQuery GetHouseNotifyPartyQuery(ZGuid value)
		{
			var result = new ZDBOnlyQuery(typeof(CusCAeMHMaster));
			result.AddFilterAndZSQLParameterCollection(GetHouseJobDocAddressOrganisationQuery(DocAddressTypes.Codes.NotifyParty), new ZSqlParameterCollection(
				ZSqlParameter.New("@organisationPK", value, OrgAddressSchema.OA_OH)));
			return result;
		}

		ZQuery GetHouseSecondaryNotifyPartyQuery(ZGuid value)
		{
			var result = new ZDBOnlyQuery(typeof(CusCAeMHMaster));
			result.AddFilterAndZSQLParameterCollection(GetHouseJobDocAddressOrganisationQuery(DocAddressTypes.Codes.ImportBroker, DocAddressTypes.Codes.Carrier,
				DocAddressTypes.Codes.ReceivingForwarderAddress, DocAddressTypes.Codes.Warehouse), new ZSqlParameterCollection(
				ZSqlParameter.New("@organisationPK", value, OrgAddressSchema.OA_OH)));
			return result;
		}

		ZString GetHouseJobDocAddressOrganisationQuery(params ZString[] addressTypes)
		{
			var addressTypesInQuery = new ZStringBuilder();
			foreach (var addressType in addressTypes)
			{
				addressTypesInQuery.Append("'" + addressType + "'");
			}

			return string.Format(@"
{0} IN ( SELECT {1} FROM {2}
				INNER JOIN {3} ON {4} = {5} AND {6} = '{7}' AND {8} IN ({9})
				INNER JOIN {10} ON {11} = {12}
				WHERE {13} = @organisationPK )",
CusCAeMHMasterSchema.Constants.PK, //0
CusCAeMHHouseSchema.Constants.BW_BP_Master, //1
CusCAeMHHouseSchema.Constants.TableName, //2
JobDocAddressSchema.Constants.TableName, //3
CusCAeMHHouseSchema.Constants.PK, //4
JobDocAddressSchema.Constants.E2_ParentID, //5
JobDocAddressSchema.Constants.E2_ParentTableCode, //6
CusCAeMHHouseSchema.Constants.Prefix, //7
JobDocAddressSchema.Constants.E2_AddressType, //8
addressTypesInQuery.ToStringWithDelimiterBetweenAppends(","), //9
OrgAddressSchema.Constants.TableName, //10
JobDocAddressSchema.Constants.E2_OA_Address, //11
OrgAddressSchema.Constants.PK, //12
OrgAddressSchema.Constants.OA_OH //13
				);
		}

		#region CusCAeMHMasterQuery

		class CAeMHMasterQueryCreator
		{
			public static ZDBOnlyQuery CreateQueryWithHouseSubQuery(SchemaColumn houseSchemaColumn, SQLComparisonOperator comparisonOperator, object value)
			{
				var comparisonOperatorDictionary = new Dictionary<SQLComparisonOperator, SQLComparisonOperator> {
					{ SQLComparisonOperator.NotEqual, SQLComparisonOperator.Equal },
					{ SQLComparisonOperator.NotContains, SQLComparisonOperator.Contains },
					{ SQLComparisonOperator.DoesNotStartWith, SQLComparisonOperator.StartsWith },
					{ SQLComparisonOperator.DoesNotEndWith, SQLComparisonOperator.EndsWith },
					{ SQLComparisonOperator.IsBlank, SQLComparisonOperator.IsNotBlank },
				};
				var isNotEqual = comparisonOperatorDictionary.ContainsKey(comparisonOperator);
				var result = new ZDBOnlyQuery(typeof(CusCAeMHMaster));
				var houseQuery = new ZDBOnlySubQuery(typeof(CusCAeMHHouse), CusCAeMHHouseSchema.BW_BP_Master, isNotEqual);
				houseQuery.AddToFilter(houseSchemaColumn, isNotEqual ? comparisonOperatorDictionary[comparisonOperator] : comparisonOperator, value);
				result.AddSubQuery(houseQuery, JoinCondition.And);
				return result;
			}

			public static ZDBOnlyQuery CreateQueryWithHouseSubQuery(SchemaColumn houseSchemaColumn, object value)
			{
				return CreateQueryWithHouseSubQuery(houseSchemaColumn, SQLComparisonOperator.Equal, value);
			}
		}

		#endregion

		#endregion

		#region Lookups

		OrgHeaderCollection Organisations
		{
			get { return fOrganisations ?? (fOrganisations = new OrgHeaderCollection(Factory)); }
		}
		OrgHeaderCollection fOrganisations;

		ZZRefCusCodeListCombinedCollection Offices
		{
			get { return fOffices ?? (fOffices = ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory, Core.Constants.CountryCodes.Canada, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, ZDateTime.Today)); }
		}
		ZZRefCusCodeListCombinedCollection fOffices;

		ZZRefCusCodeListCombinedCollection NoticeReasonCodes
		{
			get
			{
				if (fNoticeReasonCodes == null)
				{
					fNoticeReasonCodes = ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory, Core.Constants.CountryCodes.Canada, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CANoticeReasonCode, ZDateTime.Today);
					fNoticeReasonCodes.Load();
					fNoticeReasonCodes.Sort(RefCusCodeListSchema.Constants.ZZD_Code);
				}
				return fNoticeReasonCodes;
			}
		}
		ZZRefCusCodeListCombinedCollection fNoticeReasonCodes;

		CACSubLocationCollection SubLocations
		{
			get { return fSubLocations ?? (fSubLocations = new CACSubLocationCollection(Factory)); }
		}
		CACSubLocationCollection fSubLocations;

		public ZZRefCarrierCombinedCollection Carriers => ZZRefCarrierCombinedCollectionExtension.GetCachedCollection(Factory, ZString.Empty);

		RefUNLOCOCollection UNLOCOs
		{
			get { return fUNLOCOs ?? (fUNLOCOs = new RefUNLOCOCollection(Factory)); }
		}
		RefUNLOCOCollection fUNLOCOs;

		#endregion
	}
}
