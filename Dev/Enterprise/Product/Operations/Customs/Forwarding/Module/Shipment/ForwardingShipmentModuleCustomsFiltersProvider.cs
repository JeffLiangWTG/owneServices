using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.Customs.Common.JP.AFR;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.Universal;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration.ZArchitecture;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.Messaging.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Customs;
using USISF = Enterprise.Customs.Common.US.ISF;

namespace Enterprise.Customs.Forwarding.Module
{
	public class ForwardingShipmentModuleCustomsFiltersProvider : IModuleCustomFiltersProvider
	{
		public ForwardingShipmentModuleCustomsFiltersProvider(BusinessObjectFactory factory)
		{
			this.factory = factory;
		}

		readonly BusinessObjectFactory factory;

		#region Constants

		public static class Descriptions
		{
			#region SuppressResourceStringsCheckRegion

			public const string SeaCargoMessageStatus = "Sea Cargo Message Status";
			public const string SeaCargoCustomsStatus = "Sea Cargo Customs Status";
			public const string AirCargoMessageStatus = "Air Cargo Message Status";
			public const string AirCargoCustomsStatus = "Air Cargo Customs Status";

			public const string EntryType = "Entry Type";
			public const string ITNumber = "IT Number";
			public const string ITDate = "IT Date";
			public const string ITType = "IT Type";
			public const string ITCarrier = "IT Carrier - Departure";
			public const string ITCarrierTOL = "IT Carrier - TOL";
			public const string CargoReleaseStatus = "CRL (Cargo Release) Status";
			public const string SimplifiedEntryBillStatus = "SE (Simplified Entry) Bill Status";
			public const string BillHoldOrExam = "Bill Hold or Exam";
			public const string EntrySummaryStatus = "ENS (Entry Summary) Status";
			public const string ExportStatus = "EXP (Export) Status";

			public const string ISFBillStatus = "ISF Bill Status";
			public const string AFRBillStatus = "AFR Bill Status";

			public const string CustomsEntryStatus = "Customs Entry Status";

			#endregion
		}

		const string NotClearCargoCustomsStatus = "NCL";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "US Code list filter value")]
		public static class FilterStatus
		{
			public const string NotSentCustomsStatusForFilter = "NOT";
			public const string NotSentForFilterDescription = "Not Sent - only valid for exact match";
			public const string MultipleEntriesStatus = "MES";
			public const string MultipleBillsStatus = "Multiple bills have different statuses.";
		}

		public static class CusEntryHeaderMessageType
		{
			public const string BorderCargoRelease = "BCR";
			public const string CargoRelease = "CRL";
			public const string SimplifiedEntry = "SE";
			public const string EntrySummary = "ENS";
			public const string Export = "ITN";
			public const string InBond = "INB";
			public const string NAFTADutyDeferral = "NAF";
		}

		#endregion

		public void AddFilters(IModuleFilterCollection filterCollection)
		{
			var filters = (ModuleFilterCollection)filterCollection;

			var customsEntryNoFilter = filters.AddNumberFilter("Customs Entry #", GetCustomsEntryNoQuery).WithMaxLengthOf<ModuleNumberFilter>(CusEntryNumSchema.CE_EntryNum);
			customsEntryNoFilter.IsPublishedOnWeb = false;
			customsEntryNoFilter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobShipmentFilter|CustomsEntry", "Customs Entry #");

			EntryStatusFilter cusEntryStatusFilter = null;
			var cusEntryStatusFilterMultilingualDescription = ResString.GetMultilingualString("Forwarding|JobShipmentFilter|CustomsEntryStatus", "Customs Entry Status");
			var companyCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			if (Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(companyCountryCode) == Core.Constants.CountryCodes.UnitedStates)
			{
				AddITFilters(filters);

				var entryTypeFilter = filters.AddTextFilter(Descriptions.EntryType, GetEntryTypeQuery, GetNewEntryType_List()).WithMaxLengthOf<ModuleTextFilter>(GenAddOnColumnSchema.XA_Data);
				entryTypeFilter.Category = FilterCategories.ModesAndTypes;
				entryTypeFilter.MultilingualDescription = ResString.GetMultilingualString("eb138da7-3be7-44ef-addd-2c1f29520e63", "Entry Type");

				var cargoReleaseStatusFilter = filters.AddTextFilter(Descriptions.CargoReleaseStatus, GetCargoReleaseStatusQuery, MessageStatus_ListCRL).WithMaxLengthOf<ModuleTextFilter>(CusEntryHeaderSchema.CH_Status);
				cargoReleaseStatusFilter.Category = FilterCategories.StatusAndFlags;
				cargoReleaseStatusFilter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobShipmentFilter|CRLCargoReleaseStatus", "CRL (Cargo Release) Status");

				var seBillDispositionFilter = filters.AddTextFilter(Descriptions.SimplifiedEntryBillStatus, GetSEBillStatusQuery, () => SEBillStatusList).WithMaxLengthOf<ModuleTextFilter>(CusDecHouseBillSchema.CU_MessageStatus);
				seBillDispositionFilter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobShipmentFilter|SimplifiedEntryBillStatus", "SE (Simplified Entry) Bill Status");
				seBillDispositionFilter.Category = FilterCategories.StatusAndFlags;
				seBillDispositionFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
				seBillDispositionFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.Contains);
				seBillDispositionFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotStartsWith);
				seBillDispositionFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotContain);
				seBillDispositionFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.IsBlank);
				seBillDispositionFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.IsNotBlank);
				seBillDispositionFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.StartsWith);
				seBillDispositionFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotEqual);

				var billHoldOrExamFilter = filters.AddTextFilter(Descriptions.BillHoldOrExam, GetBillHoldOrExamStatusQuery, HLDOrEXMStatusList).WithMaxLengthOf<ModuleTextFilter>(CusDecHouseBillSchema.CU_MessageStatus);
				billHoldOrExamFilter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobShipmentFilter|BillHoldOrExamStatus", "Bill Hold or Exam");
				billHoldOrExamFilter.Category = FilterCategories.StatusAndFlags;
				billHoldOrExamFilter.DefaultProperty = HLDOrEXMStatusList.Codes.All;

				ModuleFilter entrySummaryStatusFilter = filters.AddTextFilter(Descriptions.EntrySummaryStatus, GetEntrySummaryStatusQuery, MessageStatus_ListENS).WithMaxLengthOf<ModuleTextFilter>(CusEntryHeaderSchema.CH_Status);
				entrySummaryStatusFilter.Category = FilterCategories.StatusAndFlags;
				entrySummaryStatusFilter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobShipmentFilter|ENSEntrySummaryStatus", "ENS (Entry Summary) Status");

				ModuleFilter exportStatusFilter = filters.AddTextFilter(Descriptions.ExportStatus, GetExportStatusQuery, MessageStatus_ListEXP).WithMaxLengthOf<ModuleTextFilter>(CusEntryHeaderSchema.CH_Status);
				exportStatusFilter.Category = FilterCategories.StatusAndFlags;
				exportStatusFilter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobShipmentFilter|EXPExportStatus", "EXP (Export) Status");

				var releaseStatusFilter = filters.AddTextFilter(USDeclarationFilter.ReleaseStatusDescription, GetReleaseStatusQuery, USDeclarationFilter.ReleaseStatusList).WithMaxLengthOf<ModuleTextFilter>(GenAddOnColumnSchema.XA_Data);
				SetFilterConstraints(releaseStatusFilter, FilterCategories.StatusAndFlags);
				releaseStatusFilter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobShipmentFilter|ReleaseStatus", "Release Status");

				var fdaStatusFilter = filters.AddTextFilter(USDeclarationFilter.FDAStatusDescription, GetFDAStatusQuery, USDeclarationFilter.FDAStatusList).WithMaxLengthOf<ModuleTextFilter>(GenAddOnColumnSchema.XA_Data);
				fdaStatusFilter.Category = FilterCategories.StatusAndFlags;
				fdaStatusFilter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobShipmentFilter|FDAStatus", "FDA Status");

				var fdaMsgStatusFilter = filters.AddTextFilter(USDeclarationFilter.FDAMsgStatusDescription, GetFDAMsgStatusQuery, USDeclarationFilter.FDAMsgStatusList).WithMaxLengthOf<ModuleTextFilter>(GenAddOnColumnSchema.XA_Data);
				fdaMsgStatusFilter.Category = FilterCategories.StatusAndFlags;
				fdaMsgStatusFilter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobShipmentFilter|FDAMsgStatus", "FDA Message Status");
			}
			else if (companyCountryCode == Core.Constants.CountryCodes.Australia)
			{
				cusEntryStatusFilter = new EntryStatusFilter("Customs Entry Status",
					(ZString status) => GetAUCustomsEntryStatusQuery(status),
					() => GetCustomsEntryStatusList(Core.Constants.CountryCodes.Australia), false, false).WithMaxLengthOf<EntryStatusFilter>(JobDeclarationSchema.JE_EntryStatus);

				filters.AddFilter(cusEntryStatusFilter);

				var airCustomsFilter = filters.AddTextFilter(Descriptions.AirCargoCustomsStatus, GetAUAirCargoCustomsStatusQuery, GetNewAirSeaCargoCustomsStatus_List()).WithMaxLengthOf<ModuleTextFilter>(CusHAWBSchema.CS_CustomsStatus);
				airCustomsFilter.Category = FilterCategories.StatusAndFlags;
				airCustomsFilter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobShipmentFilter|AirCargoCustomsStatus", "Air Cargo Customs Status");

				var airMessageFilter = filters.AddTextFilter(Descriptions.AirCargoMessageStatus, GetAUAirCargoMessageStatusQuery, new CMRBaseStatuses()).WithMaxLengthOf<ModuleTextFilter>(CusHAWBSchema.CS_MsgStatus);
				airMessageFilter.Category = FilterCategories.StatusAndFlags;
				airMessageFilter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobShipmentFilter|AirCargoMessageStatus", "Air Cargo Message Status");

				var seaCustomsFilter = filters.AddTextFilter(Descriptions.SeaCargoCustomsStatus, GetAUSeaCargoCustomsStatusQuery, GetNewAirSeaCargoCustomsStatus_List()).WithMaxLengthOf<ModuleTextFilter>(CusSCAPivotSchema.CV_CargoStatus);
				seaCustomsFilter.Category = FilterCategories.StatusAndFlags;
				seaCustomsFilter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobShipmentFilter|SeaCargoCustomsStatus", "Sea Cargo Customs Status");

				var seaMessageFilter = filters.AddTextFilter(Descriptions.SeaCargoMessageStatus, GetAUSeaCargoMessageStatusQuery, new CMRBaseStatuses()).WithMaxLengthOf<ModuleTextFilter>(CusSCAHouseSchema.CA_MessageStatus);
				seaMessageFilter.Category = FilterCategories.StatusAndFlags;
				seaMessageFilter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobShipmentFilter|SeaCargoMessageStatus", "Sea Cargo Message Status");
			}
			else
			{
				var entryStatusFilter = new EntryStatusFilter(Descriptions.CustomsEntryStatus,
					new JobDeclarationEntryStatusFilterHelper(CustomsEntryStatusFilterHelper.GetEntryStatusQuery, CustomsEntryStatusFilterHelper.GetEntryStatusQueryAllEntries)
						.GetEntryStatusFilter, () => GetCustomsEntryStatusList(companyCountryCode)).WithMaxLengthOf<EntryStatusFilter>(CusEntryHeaderSchema.CH_EntryStatus);

				var showFilterType = companyCountryCode != Core.Constants.CountryCodes.Singapore && companyCountryCode != Core.Constants.CountryCodes.NewZealand;
				entryStatusFilter.ShowFilterType = showFilterType;
				entryStatusFilter.MultilingualDescription = cusEntryStatusFilterMultilingualDescription;

				filters.AddFilter(entryStatusFilter);

				#region Customs Message status

				if (ObjectFactory.Get<Shared.IEuropeanUnionCustomsMembersProvider>().IsInEuropeanCustomsUnionOrInheritsFromEU(Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(companyCountryCode)))
				{
					var cusMessageStatusFilter = filters.AddTextFilter("Customs Message Status", GetCustomsMessageStatusQuery, GetCustomsMessagingStatusList(companyCountryCode)).WithMaxLengthOf<ModuleTextFilter>(CusEntryHeaderSchema.CH_Status);
					if (cusMessageStatusFilter != null)
					{
						cusMessageStatusFilter.Category = FilterCategories.StatusAndFlags;
						cusMessageStatusFilter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobShipmentFilter|CustomsMessageStatus", "Customs Message Status");
					}
				}

				#endregion
			}

			if (cusEntryStatusFilter != null)
			{
				cusEntryStatusFilter.Category = FilterCategories.StatusAndFlags;
				cusEntryStatusFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.StartsWith);
				cusEntryStatusFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.Contains);
				cusEntryStatusFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotStartsWith);
				cusEntryStatusFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotContain);
				cusEntryStatusFilter.MultilingualDescription = cusEntryStatusFilterMultilingualDescription;
			}

			if (companyCountryCode == Core.Constants.CountryCodes.Singapore)
			{
				filters.AddTextFilter("Customs URN", GetSGCustomsURN).WithMaxLengthOf<ModuleTextFilter>(EDIMessageSchema.EM_ApplicationReference).MultilingualDescription = ResString.GetMultilingualString("8393b93b-dbb5-45f7-9a68-9ba878c343a0", "Customs URN");
			}

			var isfBillStatusFilter = filters.AddTextFilter(Descriptions.ISFBillStatus, GetISFBillStatusQuery, MessageStatus_ListISFBillStatus).WithMaxLengthOf<ModuleTextFilter>(CusISFBillSchema.BB_CustomsStatus);
			isfBillStatusFilter.Category = FilterCategories.StatusAndFlags;
			isfBillStatusFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
			isfBillStatusFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.Contains);
			isfBillStatusFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.StartsWith);
			isfBillStatusFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotStartsWith);
			isfBillStatusFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotContain);
			isfBillStatusFilter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobShipmentFilter|ISFBillStatus", "ISF Bill Status");

			AddAFRStatusFilter(filters);
		}

		#region SetFilterConstraints

		static void SetFilterConstraints(ModuleTextFilter moduleFilter, FilterCategory category)
		{
			moduleFilter.Category = category;
			moduleFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
			moduleFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.Contains);
			moduleFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotStartsWith);
			moduleFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotContain);
			moduleFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.IsBlank);
			moduleFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.IsNotBlank);
		}

		#endregion

		#region GetCustomsEntryNoQuery

		ZQuery GetCustomsEntryNoQuery(SQLComparisonOperator comparisonOperator, ZString customsEntryNo)
		{
			return ForwardingShipmentFilterProvider.GetJobShipmentFromEntryNumber(comparisonOperator, customsEntryNo);
		}

		#endregion

		#region AU Filters

		#region GetAirCargoMessageStatusQuery & GetAirCargoCustomsStatusQuery

		ZQuery GetAUAirCargoMessageStatusQuery(ZString status)
		{
			return GetAUAirCargoQuery(CusHAWBSchema.CS_MsgStatus, status);
		}

		ZQuery GetAUAirCargoCustomsStatusQuery(ZString status)
		{
			ZQuery result;

			if (status.EqualsIgnoringCase(NotClearCargoCustomsStatus))
			{
				result = new ZDBOnlyQuery(typeof(ForwardingShipment));
				ZDBOnlySubQuery houseQuery = new ZDBOnlySubQuery(typeof(Shared.ICusHAWB), CusHAWBSchema.CS_JS);
				foreach (CodeDescriptionPair codeDescription in CMRConsolidatedCargoStatuses.AllClearStatus)
				{
					houseQuery.AddToFilter(CusHAWBSchema.CS_CustomsStatus, SQLComparisonOperator.NotEqual, (ZString)codeDescription.Code);
				}
				ZDBOnlySubQuery cusMAWBQuery = new ZDBOnlySubQuery(typeof(Shared.ICusMAWB), CusHAWBSchema.CS_CM);
				cusMAWBQuery.AddToFilter(CusMAWBSchema.CM_ApplicationCode, AUApplicationCodes);
				houseQuery.AddSubQuery(cusMAWBQuery, JoinCondition.And);
				((ZDBOnlyQuery)result).AddSubQuery(houseQuery, JoinCondition.And);
			}
			else
			{
				result = GetAUAirCargoQuery(CusHAWBSchema.CS_CustomsStatus, status);
			}

			return result;
		}

		ZQuery GetAUAirCargoQuery(SchemaStringColumn filterColumn, ZString value)
		{
			ZDBOnlyQuery result = new ZDBOnlyQuery(typeof(ForwardingShipment));
			ZSqlParameterCollection @params = new ZSqlParameterCollection();
			string sQL = Freight.Common.Business.AutoJobShipment.Schema.PK + " IN " +
				"(SELECT CS_JS FROM dbo.CusHawb" +
				" WHERE " + filterColumn.Name + " = @Status and " +
				CusHAWBSchema.CS_CM.Name + " IN (SELECT " + CusMAWBSchema.PK.Name + " FROM " + CusMAWBSchema.Constants.SqlSchemaName + "." + CusMAWBSchema.Constants.TableName + " WHERE " +
				CusMAWBSchema.CM_ApplicationCode.Name + " in (" + QuotedStrings(AUApplicationCodes) + ")))";

			@params.Add("@Status", value, filterColumn);
			result.AddFilterAndZSQLParameterCollection(sQL, @params);

			return result;
		}

		#endregion

		#region GetSeaCargoMessageStatusQuery & GetSeaCargoCustomsStatusQuery

		ZQuery GetAUSeaCargoMessageStatusQuery(ZString status)
		{
			return GetAUSeaCargoQuery(CusSCAHouseSchema.CA_MessageStatus, status);
		}

		ZQuery GetAUSeaCargoCustomsStatusQuery(ZString status)
		{
			var result = new ZDBOnlyQuery(typeof(ForwardingShipment));
			var houseQuery = new ZDBOnlySubQuery(typeof(AU.ICusSCAHouse), CusSCAHouseSchema.CA_JS);

			var cusSCAOceanBillQuery = new ZDBOnlySubQuery(typeof(AU.ICusSCAOceanBill), CusSCAHouseSchema.CA_CB);
			cusSCAOceanBillQuery.AddToFilter(CusSCAOceanBillSchema.CB_ApplicationCode, AUApplicationCodes);
			houseQuery.AddSubQuery(cusSCAOceanBillQuery, JoinCondition.And);

			var pivotQuery = new ZDBOnlySubQuery(typeof(AU.ICusSCAPivot), CusSCAPivotSchema.CV_CA);
			if (status.EqualsIgnoringCase(NotClearCargoCustomsStatus))
			{
				foreach (CodeDescriptionPair codeDescription in CMRConsolidatedCargoStatuses.AllClearStatus)
				{
					pivotQuery.AddToFilter(CusSCAPivotSchema.CV_CargoStatus, SQLComparisonOperator.NotEqual, (ZString)codeDescription.Code);
				}
			}
			else
			{
				var values = new List<ZString>();
				values.Add(status);
				if (status == "NOT")
				{
					values.Add(string.Empty);
				}
				pivotQuery.AddToFilter(CusSCAPivotSchema.CV_CargoStatus, values);
			}

			houseQuery.AddSubQuery(pivotQuery, JoinCondition.And);
			result.AddSubQuery(houseQuery, JoinCondition.And);
			return result;
		}

		ZString[] AUApplicationCodes
		{
			get
			{
				return new ZString[] { Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages };
			}
		}

		ZQuery GetAUSeaCargoQuery(SchemaColumn filterColumn, ZString value)
		{
			ZDBOnlyQuery result = new ZDBOnlyQuery(typeof(ForwardingShipment));

			string notSentClause = "";
			if (value == "NOT")
			{
				notSentClause = " OR " + filterColumn.Name + "=''";
			}

			string sQL = Freight.Common.Business.AutoJobShipment.Schema.PK + " IN " +
				"(SELECT " + CusSCAHouseSchema.CA_JS.Name + " FROM " + CusSCAHouseSchema.Constants.SqlSchemaName + "." + CusSCAHouseSchema.Constants.TableName +
				" WHERE (" + filterColumn.Name + " = @" + filterColumn.Name + notSentClause +
				") and " + CusSCAHouseSchema.CA_CB.Name + " IN (SELECT " + CusSCAOceanBillSchema.PK.Name + " FROM " + CusSCAOceanBillSchema.Constants.SqlSchemaName + "." + CusSCAOceanBillSchema.Constants.TableName + " WHERE " +
				CusSCAOceanBillSchema.CB_ApplicationCode.Name + " in (" + QuotedStrings(AUApplicationCodes) + ")))";

			ZSqlParameterCollection @params = new ZSqlParameterCollection();
			@params.Add("@" + filterColumn.Name + "", value, filterColumn);
			result.AddFilterAndZSQLParameterCollection(sQL, @params);

			return result;
		}

		protected ZString QuotedStrings(ZString[] strings)
		{
			StringBuilder result = new StringBuilder();
			for (int i = 0; i < strings.Length; i++)
			{
				if (i != 0)
				{
					result.Append(", ");
				}

				result.Append("'" + strings[i].ToString() + "' ");
			}
			return result.ToString();
		}

		#endregion

		#endregion

		#region IT (US) Filters

		void AddITFilters(ModuleFilterCollection filters)
		{
			var itNumberFilter = filters.AddNumberFilter(Descriptions.ITNumber, GetITNumberQuery).WithMaxLengthOf<ModuleNumberFilter>(GenAddOnColumnSchema.XA_Data);
			itNumberFilter.Category = ITCategory;
			itNumberFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;
			itNumberFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.Contains);
			itNumberFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotEqual);
			itNumberFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotStartsWith);
			itNumberFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotContain);
			itNumberFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.IsBlank);
			itNumberFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.IsNotBlank);
			itNumberFilter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobShipmentFilter|ITNumber", "IT Number");

			var itDateFilter = filters.AddDateFilter(Descriptions.ITDate, GetITDate);
			itDateFilter.Category = ITCategory;
			itDateFilter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobShipmentFilter|ITDate", "IT Date");

			var itTypeFilter = filters.AddTextFilter(Descriptions.ITType, GetITType).WithMaxLengthOf<ModuleTextFilter>(GenAddOnColumnSchema.XA_Data);
			itTypeFilter.Category = ITCategory;
			itTypeFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
			itTypeFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.Contains);
			itTypeFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotEqual);
			itTypeFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotStartsWith);
			itTypeFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotContain);
			itTypeFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.IsBlank);
			itTypeFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.IsNotBlank);
			itTypeFilter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobShipmentFilter|ITType", "IT Type");
		}

		#region ITNumber Query

		ZQuery GetITNumberQuery(SQLComparisonOperator filterOperator, ZString value)
		{
			var result = new ZDBOnlyQuery(ObjectFactory.GetType<IBaseJobDeclaration>());

			var query = new ZDBOnlySubQuery(ObjectFactory.GetType<US.IBill>(), CusDecHouseBillSchema.CU_JE);

			var subQuery = new ZDBOnlySubQuery(ObjectFactory.GetType<ICusAddInfo>(), CusAddInfoSchema.B7_ParentID);

			var addOnColumnSubQuery = new ZDBOnlySubQuery(typeof(GenAddOnColumn), GenAddOnColumnSchema.XA_ParentID);
			addOnColumnSubQuery.AddToFilter(GenAddOnColumnSchema.XA_Name, USITNumberAddInfoSchema.Constants.US_ITNumber);
			if (!value.IsEmpty)
			{
				addOnColumnSubQuery.AddToFilter_PossiblyCommaSeparated(GenAddOnColumnSchema.XA_Data, filterOperator, value);
			}

			subQuery.AddSubQuery(addOnColumnSubQuery, JoinCondition.And);

			query.AddSubQuery(subQuery, JoinCondition.And);

			result.AddSubQuery(query, JoinCondition.And);

			return GetQueryForFilter(result);
		}

		#endregion

		#region ITDate Query

		ZQuery GetITDate(DateComparisonOperator comparisonOperator, ZDateTime date1, ZDateTime date2)
		{
			var subQuery = QueryHelper.GetQueryWithSubQueryOnGenAddOnColumn(USAddInfoSchema.Constants.US_ITDate, comparisonOperator, date1, date2);
			return GetQueryForFilter(subQuery);
		}

		ZQuery GetQueryForFilter(ZQuery queryOnGenAddOnColumn)
		{
			var result = new ZDBOnlyQuery(typeof(ForwardingShipment));
			var subQuery = new ZDBOnlySubQuery(typeof(IBaseJobDeclaration), JobDeclarationSchema.JE_JS);
			subQuery.AddToFilter(queryOnGenAddOnColumn);
			result.AddSubQuery(subQuery, JoinCondition.And);
			return result;
		}

		#endregion

		#region ITType Query

		ZQuery GetITType(SQLComparisonOperator filterOperator, ZString value)
		{
			return SimpleQueryHelper.GetQueryWithSubQueryOnGenAddOnColumn(USAddInfoSchema.Constants.US_InbondType, filterOperator, value);
		}

		#endregion

		#region GetEntryTypeQuery

		ZQuery GetEntryTypeQuery(ZString value)
		{
			return SimpleQueryHelper.GetQueryWithSubQueryOnGenAddOnColumn(USAddInfoSchema.Constants.US_EntryType, value);
		}

		#endregion

		FilterCategory ITCategory
		{
			get { return iTCategory ?? (iTCategory = new FilterCategory(ResString.GetMultilingualString("Forwarding|JobShipmentFilter|ITInBond", "IT (InBond) (Obsolete)"))); }
		}
		FilterCategory iTCategory;

		#endregion

		#region US Status Queries

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "special case filter requires SQL statement")]
		ZQuery GetNotSentStatusQueryHelper(ZString[] entryHeaderMessageTypes, ZString messageType)
		{
			var result = new ZDBOnlyQuery(typeof(ForwardingShipment));

			var branchList = new ZStringBuilder();
			foreach (var branch in GetUSBranchPKs())
			{
				branchList.Append("{GUID" + "'" + branch + "'}");
			}
			string usBranches = branchList.ToStringWithDelimiterBetweenAppends(",");

			var subQuery = new ZDBOnlySubQuery(typeof(IBaseJobDeclaration), JobDeclarationSchema.JE_JS);
			subQuery.IsForceSeek = true;
			subQuery.AddToFilter(JobDeclarationSchema.JE_MessageType, messageType);

			ZString multipleStatusQueryText = @" CH_Status <> '')) and ((JE_GB in ({0})";
			var multipleStatusQuery = new ZDBOnlySubQuery(typeof(ICusEntryHeader), CusEntryHeaderSchema.CH_JE, true);
			if (!entryHeaderMessageTypes.Any())
			{
				multipleStatusQuery.AddToFilter(CusEntryHeaderSchema.CH_MessageType, entryHeaderMessageTypes);
			}
			multipleStatusQuery.AddFilterAndZSQLParameterCollection(string.Format(CultureInfo.InvariantCulture, multipleStatusQueryText, usBranches), new ZSqlParameterCollection());

			subQuery.AddSubQuery(multipleStatusQuery, JoinCondition.And);
			result.AddSubQuery(subQuery, JoinCondition.And);
			return result;
		}

		ZQuery GetCargoReleaseStatusQuery(ZString value)
		{
			if (value == FilterStatus.NotSentCustomsStatusForFilter)
			{
				ZString[] messageTypeList = new ZString[] { CusEntryHeaderMessageType.BorderCargoRelease,
														CusEntryHeaderMessageType.CargoRelease,
														CusEntryHeaderMessageType.SimplifiedEntry };
				return GetNotSentStatusQueryHelper(messageTypeList, "IMP");
			}
			else
			{
				ZDBOnlyQuery result = new ZDBOnlyQuery(typeof(ForwardingShipment));

				if (!value.IsEmpty)
				{
					ZDBOnlySubQuery cRLQuery = new ZDBOnlySubQuery(typeof(IBaseJobDeclaration), JobDeclarationSchema.JE_JS);
					ZDBOnlySubQuery cRLSubQuery = new ZDBOnlySubQuery(typeof(ICusEntryHeader), CusEntryHeaderSchema.CH_JE);
					cRLSubQuery.AddToFilter(CusEntryHeaderSchema.CH_MessageType, CusEntryHeaderMessageType.BorderCargoRelease);
					cRLSubQuery.AddToFilter(JoinCondition.Or, CusEntryHeaderSchema.CH_MessageType, CusEntryHeaderMessageType.CargoRelease);
					cRLSubQuery.AddToFilter(JoinCondition.Or, CusEntryHeaderSchema.CH_MessageType, CusEntryHeaderMessageType.SimplifiedEntry);
					cRLSubQuery.AddToFilter(CusEntryHeaderSchema.CH_Status, value);
					cRLQuery.AddSubQuery(cRLSubQuery, JoinCondition.And);
					result.AddSubQuery(cRLQuery, JoinCondition.And);
				}

				return result;
			}
		}

		ZQuery GetSEBillStatusQuery(SQLComparisonOperator filterOperator, ZString value)
		{
			var result = new ZDBOnlyQuery(typeof(ForwardingShipment));
			var provider = ObjectFactory.Get<US.IForwardingShipmentCustomsQueryProvider>();

			var declarationQuery = new ZDBOnlySubQuery(typeof(IBaseJobDeclaration), JobDeclarationSchema.JE_JS);
			declarationQuery.AddToFilter(JobDeclarationSchema.JE_GC, GlbCompany.CurrentCompany.PK);
			declarationQuery.AddToFilter(provider.GetSimplifiedEntryBillStatusQuery(filterOperator, value));
			result.AddSubQuery(declarationQuery, JoinCondition.And);
			return result;
		}

		ZQuery GetBillHoldOrExamStatusQuery(ZString value)
		{
			var result = new ZDBOnlyQuery(typeof(ForwardingShipment));
			var provider = ObjectFactory.Get<US.IForwardingShipmentCustomsQueryProvider>();

			var declarationQuery = new ZDBOnlySubQuery(typeof(IBaseJobDeclaration), JobDeclarationSchema.JE_JS);
			declarationQuery.AddToFilter(JobDeclarationSchema.JE_GC, GlbCompany.CurrentCompany.PK);
			declarationQuery.AddToFilter(provider.GetHoldExamBillStatusQuery(value));
			result.AddSubQuery(declarationQuery, JoinCondition.And);
			return result;
		}

		ZQuery GetEntrySummaryStatusQuery(ZString value)
		{
			if (value == FilterStatus.NotSentCustomsStatusForFilter)
			{
				ZString[] messageTypeList = new ZString[] { CusEntryHeaderMessageType.EntrySummary };
				return GetNotSentStatusQueryHelper(messageTypeList, "IMP");
			}
			else
			{
				ZDBOnlyQuery result = new ZDBOnlyQuery(typeof(ForwardingShipment));

				if (!value.IsEmpty)
				{
					ZDBOnlySubQuery eNSQuery = new ZDBOnlySubQuery(typeof(IBaseJobDeclaration), JobDeclarationSchema.JE_JS);
					ZDBOnlySubQuery eNSSubQuery = new ZDBOnlySubQuery(typeof(ICusEntryHeader), CusEntryHeaderSchema.CH_JE);
					eNSSubQuery.AddToFilter(CusEntryHeaderSchema.CH_MessageType, CusEntryHeaderMessageType.EntrySummary);
					eNSSubQuery.AddToFilter(CusEntryHeaderSchema.CH_Status, value);
					eNSQuery.AddSubQuery(eNSSubQuery, JoinCondition.And);
					result.AddSubQuery(eNSQuery, JoinCondition.And);
				}

				return result;
			}
		}

		#region Export Status

		ZQuery GetExportStatusQuery(ZString value)
		{
			if (value == FilterStatus.MultipleEntriesStatus)
			{
				return GetMultipleExportStatusQuery();
			}
			else if (value == FilterStatus.NotSentCustomsStatusForFilter)
			{
				ZString[] messageTypeList = new ZString[] { CusEntryHeaderMessageType.Export };
				return GetNotSentStatusQueryHelper(messageTypeList, "EXP");
			}
			else
			{
				ZDBOnlyQuery result = new ZDBOnlyQuery(typeof(ForwardingShipment));

				if (!value.IsEmpty)
				{
					ZDBOnlySubQuery eXPQuery = new ZDBOnlySubQuery(typeof(IBaseJobDeclaration), JobDeclarationSchema.JE_JS);
					ZDBOnlySubQuery eXPSubQuery = new ZDBOnlySubQuery(typeof(ICusEntryHeader), CusEntryHeaderSchema.CH_JE);
					eXPSubQuery.AddToFilter(CusEntryHeaderSchema.CH_MessageType, CusEntryHeaderMessageType.Export);
					eXPSubQuery.AddToFilter(CusEntryHeaderSchema.CH_Status, value);
					eXPQuery.AddSubQuery(eXPSubQuery, JoinCondition.And);

					result.AddSubQuery(eXPQuery, JoinCondition.And);
				}

				return result;
			}
		}

		ZQuery GetMultipleExportStatusQuery()
		{
			var result = new ZDBOnlyQuery(typeof(ForwardingShipment));

			var eXPDecQuery = new ZDBOnlySubQuery(typeof(IBaseJobDeclaration), JobDeclarationSchema.JE_JS);
			eXPDecQuery.AddToFilter(JobDeclarationSchema.JE_MessageType, "EXP");

			string multipleStatusQueryText = @" CH_MessageType = 'ITN' and CH_AddInfo not like '%IsDeactivated=Y%' group by CH_JE having count(distinct CH_Status) > 1";

			ZDBOnlySubQuery multipleStatusQuery = new ZDBOnlySubQuery(typeof(ICusEntryHeader), CusEntryHeaderSchema.CH_JE);
			multipleStatusQuery.AddFilterAndZSQLParameterCollection(multipleStatusQueryText, new ZSqlParameterCollection());
			eXPDecQuery.AddSubQuery(multipleStatusQuery, JoinCondition.And);
			result.AddSubQuery(eXPDecQuery, JoinCondition.And);

			return result;
		}

		ZString[] GetUSBranchPKs()
		{
			return factory.GetCachedValue("USBranchList", delegate
			{
				ZDBOnlyQuery branchQuery = new ZDBOnlyQuery(typeof(GlbBranch));
				branchQuery.AddToFilter(GlbBranchSchema.GB_IsActive, true);

				ZDBOnlySubQuery companyQuery = new ZDBOnlySubQuery(typeof(GlbCompany), GlbBranchSchema.GB_GC);
				companyQuery.AddToFilter(GlbCompanySchema.GC_RN_NKCountryCode, Core.Constants.CountryCodes.UnitedStates);
				branchQuery.AddSubQuery(companyQuery, JoinCondition.And);

				var branches = new GlbBranchCollection(factory);
				branches.LoadWithMoreFiltering(branchQuery);

				var branchPKs = new List<ZString>();
				foreach (GlbBranch branch in branches)
				{
					branchPKs.Add(branch.PK.ToString());
				}

				return branchPKs.ToArray();
			});
		}

		#endregion

		ZQuery GetReleaseStatusQuery(SQLComparisonOperator filterOperator, ZString value)
		{
			var result = new ZDBOnlyQuery(typeof(ForwardingShipment));
			var declarationQuery = new ZDBOnlySubQuery(typeof(IBaseJobDeclaration), JobDeclarationSchema.JE_JS);
			declarationQuery.AddToFilter(((GetTextQueryWithOperator)USDeclarationFilter.GetReleaseStatusTextQueryWithOperator).Invoke(filterOperator, value));
			result.AddSubQuery(declarationQuery, JoinCondition.And);
			return result;
		}

		ZQuery GetFDAStatusQuery(SQLComparisonOperator filterOperator, ZString value)
		{
			var result = new ZDBOnlyQuery(typeof(ForwardingShipment));
			var declarationQuery = new ZDBOnlySubQuery(typeof(IBaseJobDeclaration), JobDeclarationSchema.JE_JS);
			declarationQuery.AddToFilter(((GetTextQueryWithOperator)USDeclarationFilter.GetFDAStatusTextQueryWithOperator).Invoke(filterOperator, value));
			result.AddSubQuery(declarationQuery, JoinCondition.And);
			return result;
		}

		ZQuery GetFDAMsgStatusQuery(SQLComparisonOperator filterOperator, ZString value)
		{
			var result = new ZDBOnlyQuery(typeof(ForwardingShipment));
			var declarationQuery = new ZDBOnlySubQuery(typeof(IBaseJobDeclaration), JobDeclarationSchema.JE_JS);
			declarationQuery.AddToFilter(((GetTextQueryWithOperator)USDeclarationFilter.GetFDAMsgStatusTextQueryWithOperator).Invoke(filterOperator, value));
			result.AddSubQuery(declarationQuery, JoinCondition.And);
			return result;
		}

		ZQuery GetISFBillStatusQuery(SQLComparisonOperator filterOperator, ZString value)
		{
			var isNotMatching = filterOperator == SpecialComparisonOperator.IsBlank || filterOperator == SQLComparisonOperator.NotEqual;
			var isBlackTypeFilter = filterOperator == SpecialComparisonOperator.IsNotBlank || filterOperator == SpecialComparisonOperator.IsBlank;
			var recyclePeriod = USISF.ISFStatusHelper.TimeFrame6MonthsForSearching;

			var billList = new ZStringBuilder(JobShipmentSchema.Constants.JS_HouseBill);
			var scacs = factory.GetOrgProxySCACs();
			if (scacs != null)
			{
				foreach (var scac in scacs)
				{
					billList.Append(string.Format(CultureInfo.InvariantCulture, "CASE WHEN LEN(JS_HouseBill) BETWEEN 1 AND 12 THEN '{0}' + JS_HouseBill ELSE NULL END", scac));
				}
			}

			var sqlFilter = string.Format(CultureInfo.InvariantCulture, @"
JS_PK {0} IN (
	SELECT JS_PK
	FROM
	(
		SELECT CASE WHEN COUNT(JS_PK) > 1 AND MAX(BB_CustomsStatus) IS NOT NULL THEN @MultipleValue ELSE MAX(BB_CustomsStatus) END AS BB_CustomsStatus, JS_PK
		FROM
		(
			SELECT DISTINCT BB_CustomsStatus, BB_PK, JS_PK
			FROM dbo.JobShipment
			LEFT JOIN
			(
				SELECT JS_PK AS ShipmentPK, BB_PK, BB_CustomsStatus
				FROM dbo.JobShipment
				JOIN dbo.CusEntryNum ON JS_PK = CE_ParentID AND CE_EntryType = @AMSTypeValue AND CE_EntryNum <> ''
				JOIN dbo.CusISFBill ON BB_BillNum = CE_EntryNum
									AND BB_BillType IN (@OceanBillType, @HouseBillType)
									AND BB_BF IN (SELECT BF_PK
													FROM dbo.CusISFHeader
													WHERE BF_SystemCreateTimeUtc >= CASE WHEN JS_SystemCreateTimeUtc IS NULL THEN @FromCreateTime ELSE DATEADD(MONTH, -{2}, JS_SystemCreateTimeUtc) END
													AND BF_SystemCreateTimeUtc <= CASE WHEN JS_SystemCreateTimeUtc IS NULL THEN @ToCreateTime ELSE DATEADD(MONTH, {2}, JS_SystemCreateTimeUtc) END)
				WHERE JS_IsForwardRegistered = 1 AND JS_IsCancelled = 0 AND JS_TransportMode IN (@SeaTransportMode, @AirSeaTransportMode, @SeaAirTransportMode)

				UNION

				SELECT JS_PK AS ShipmentPK, BB_PK, BB_CustomsStatus
				FROM dbo.JobShipment
				JOIN dbo.CusISFBill ON BB_BillNum IN ({1})
									AND BB_BillType IN (@OceanBillType, @HouseBillType)
									AND BB_BF IN (SELECT BF_PK
													FROM dbo.CusISFHeader
													WHERE BF_SystemCreateTimeUtc >= CASE WHEN JS_SystemCreateTimeUtc IS NULL THEN @FromCreateTime ELSE DATEADD(MONTH, -{2}, JS_SystemCreateTimeUtc) END
													AND BF_SystemCreateTimeUtc <= CASE WHEN JS_SystemCreateTimeUtc IS NULL THEN @ToCreateTime ELSE DATEADD(MONTH, {2}, JS_SystemCreateTimeUtc) END)
				WHERE JS_IsForwardRegistered = 1 AND JS_IsCancelled = 0 AND JS_TransportMode IN (@SeaTransportMode, @AirSeaTransportMode, @SeaAirTransportMode)
					AND JS_HouseBill <> ''
					AND NOT EXISTS (SELECT 1 FROM dbo.CusEntryNum WHERE CE_ParentID = JS_PK AND CE_EntryType = @AMSTypeValue AND CE_EntryNum <> '')
					AND NOT EXISTS
					(
						SELECT 1
						FROM dbo.JobConShipLink
						JOIN dbo.JobConsol ON JK_PK = JN_JK
						WHERE JN_JS = JS_PK AND JK_AgentType = 'DRT'
					)

				UNION

				SELECT JS_PK AS ShipmentPK, BB_PK, BB_CustomsStatus
				FROM dbo.JobShipment
				JOIN dbo.JobDocAddress ShipmentHouseBillIssuingPartyAddress ON ShipmentHouseBillIssuingPartyAddress.E2_ParentID = JS_PK AND ShipmentHouseBillIssuingPartyAddress.E2_AddressType = 'HBI'
				JOIN dbo.OrgAddress ShipmentHouseBillIssuingPartyOrgAddress ON ShipmentHouseBillIssuingPartyAddress.E2_OA_Address = ShipmentHouseBillIssuingPartyOrgAddress.OA_PK
				JOIN dbo.OrgCusCode HBIOK ON HBIOK.OK_OH = ShipmentHouseBillIssuingPartyOrgAddress.OA_OH AND HBIOK.OK_CodeType = @CarrierCode AND HBIOK.OK_RN_NKCodeCountry = @USCountryCode AND HBIOK.OK_CustomsRegNo <> ''
				JOIN dbo.CusISFBill ON BB_BillNum = (HBIOK.OK_CustomsRegNo + JS_HouseBill)
									AND BB_BillType IN (@OceanBillType, @HouseBillType)
									AND BB_BF IN (SELECT BF_PK
													FROM dbo.CusISFHeader
													WHERE BF_SystemCreateTimeUtc >= CASE WHEN JS_SystemCreateTimeUtc IS NULL THEN @FromCreateTime ELSE DATEADD(MONTH, -{2}, JS_SystemCreateTimeUtc) END
													AND BF_SystemCreateTimeUtc <= CASE WHEN JS_SystemCreateTimeUtc IS NULL THEN @ToCreateTime ELSE DATEADD(MONTH, {2}, JS_SystemCreateTimeUtc) END)
				WHERE JS_IsForwardRegistered = 1 AND JS_IsCancelled = 0 AND JS_TransportMode IN (@SeaTransportMode, @AirSeaTransportMode, @SeaAirTransportMode)
					AND JS_HouseBill <> ''
					AND NOT EXISTS (SELECT 1 FROM dbo.CusEntryNum WHERE CE_ParentID = JS_PK AND CE_EntryType = @AMSTypeValue AND CE_EntryNum <> '')
					AND NOT EXISTS
					(
						SELECT 1
						FROM dbo.JobConShipLink
						JOIN dbo.JobConsol ON JK_PK = JN_JK
						WHERE JN_JS = JS_PK AND JK_AgentType = 'DRT'
					)

				UNION

				SELECT JS_PK AS ShipmentPK, BB_PK, BB_CustomsStatus
				FROM dbo.JobShipment
				JOIN dbo.JobConShipLink ON JN_JS = JS_PK
				JOIN dbo.JobConsol ON JK_PK = JN_JK
				JOIN dbo.OrgAddress SendingForwarderAddress ON SendingForwarderAddress.OA_PK = JK_OA_SendingForwarderAddress
				JOIN dbo.OrgCusCode HBIOK ON HBIOK.OK_OH = SendingForwarderAddress.OA_OH AND HBIOK.OK_CodeType = @CarrierCode AND HBIOK.OK_RN_NKCodeCountry = @USCountryCode AND HBIOK.OK_CustomsRegNo <> ''
				JOIN dbo.CusISFBill ON BB_BillNum = (HBIOK.OK_CustomsRegNo + JS_HouseBill)
									AND BB_BillType IN (@OceanBillType, @HouseBillType)
									AND BB_BF IN (SELECT BF_PK
													FROM dbo.CusISFHeader
													WHERE BF_SystemCreateTimeUtc >= CASE WHEN JS_SystemCreateTimeUtc IS NULL THEN @FromCreateTime ELSE DATEADD(MONTH, -{2}, JS_SystemCreateTimeUtc) END
													AND BF_SystemCreateTimeUtc <= CASE WHEN JS_SystemCreateTimeUtc IS NULL THEN @ToCreateTime ELSE DATEADD(MONTH, {2}, JS_SystemCreateTimeUtc) END)
				WHERE JS_IsForwardRegistered = 1 AND JS_IsCancelled = 0 AND JS_TransportMode IN (@SeaTransportMode, @AirSeaTransportMode, @SeaAirTransportMode)
					AND JS_HouseBill <> ''
					AND NOT EXISTS (SELECT 1 FROM dbo.CusEntryNum WHERE CE_ParentID = JS_PK AND CE_EntryType = @AMSTypeValue AND CE_EntryNum <> '')
					AND NOT EXISTS
					(
						SELECT 1
						FROM dbo.JobConShipLink
						JOIN dbo.JobConsol ON JK_PK = JN_JK
						WHERE JN_JS = JS_PK AND JK_AgentType = 'DRT'
					)

				UNION

				SELECT JS_PK AS ShipmentPK, BB_PK, BB_CustomsStatus
				FROM dbo.JobShipment
				JOIN dbo.JobConShipLink ON JN_JS = JS_PK
				JOIN dbo.JobConsol ON JK_PK = JN_JK
				LEFT JOIN dbo.CusEntryNum ON CE_ParentID = JK_PK AND CE_EntryType = @AMSTypeValue AND CE_EntryNum <> ''
				JOIN dbo.CusISFBill ON BB_BillNum = COALESCE(CE_EntryNum, JK_MasterBillNum)
									AND BB_BillType IN (@OceanBillType, @HouseBillType)
									AND BB_BF IN (SELECT BF_PK
													FROM dbo.CusISFHeader
													WHERE BF_SystemCreateTimeUtc >= CASE WHEN JS_SystemCreateTimeUtc IS NULL THEN @FromCreateTime ELSE DATEADD(MONTH, -{2}, JS_SystemCreateTimeUtc) END
													AND BF_SystemCreateTimeUtc <= CASE WHEN JS_SystemCreateTimeUtc IS NULL THEN @ToCreateTime ELSE DATEADD(MONTH, {2}, JS_SystemCreateTimeUtc) END)
				WHERE JS_IsForwardRegistered = 1 AND JS_IsCancelled = 0 AND JS_TransportMode IN (@SeaTransportMode, @AirSeaTransportMode, @SeaAirTransportMode)
					AND JK_AgentType = @DirectAgentType AND JK_TransportMode = @SeaTransportMode
					AND NOT EXISTS (SELECT 1 FROM dbo.CusEntryNum WHERE CE_ParentID = JS_PK AND CE_EntryType = @AMSTypeValue AND CE_EntryNum <> '')
					AND BB_BillNum <> ''

				UNION

				SELECT JS_PK AS ShipmentPK, BB_PK, BB_CustomsStatus
				FROM dbo.JobShipment
				JOIN dbo.JobConShipLink ON JN_JS = JS_PK
				JOIN dbo.JobConsol ON JK_PK = JN_JK
				JOIN dbo.OrgAddress ON LEN(JK_MasterBillNum) BETWEEN 1 AND 12 AND JK_OA_ShippingLineAddress = OA_PK
				JOIN dbo.OrgCusCode ShippingLineOK ON ShippingLineOK.OK_OH = OA_OH AND ShippingLineOK.OK_CodeType = @CarrierCode AND ShippingLineOK.OK_RN_NKCodeCountry = @USCountryCode AND ShippingLineOK.OK_CustomsRegNo <> ''
				JOIN dbo.CusISFBill ON BB_BillNum = (ShippingLineOK.OK_CustomsRegNo + JK_MasterBillNum)
									AND BB_BillType IN (@OceanBillType, @HouseBillType)
									AND BB_BF IN (SELECT BF_PK
													FROM dbo.CusISFHeader
													WHERE BF_SystemCreateTimeUtc >= CASE WHEN JS_SystemCreateTimeUtc IS NULL THEN @FromCreateTime ELSE DATEADD(MONTH, -{2}, JS_SystemCreateTimeUtc) END
													AND BF_SystemCreateTimeUtc <= CASE WHEN JS_SystemCreateTimeUtc IS NULL THEN @ToCreateTime ELSE DATEADD(MONTH, {2}, JS_SystemCreateTimeUtc) END)
				WHERE JS_IsForwardRegistered = 1 AND JS_IsCancelled = 0 AND JS_TransportMode IN (@SeaTransportMode, @AirSeaTransportMode, @SeaAirTransportMode)
					AND JK_AgentType = @DirectAgentType AND JK_TransportMode = @SeaTransportMode
					AND NOT EXISTS (SELECT 1 FROM dbo.CusEntryNum WHERE CE_ParentID = JS_PK AND CE_EntryType = @AMSTypeValue AND CE_EntryNum <> '')
					AND NOT EXISTS (SELECT 1 FROM dbo.CusEntryNum WHERE CE_ParentID = JK_PK AND CE_EntryType = @AMSTypeValue AND CE_EntryNum <> '')

				UNION

				SELECT JS_PK AS ShipmentPK, BB_PK, BB_CustomsStatus
				FROM dbo.JobShipment
				JOIN dbo.JobConShipLink ON JN_JS = JS_PK
				JOIN dbo.JobConsol ON JK_PK = JN_JK
				JOIN dbo.JobDocAddress ConsolMasterBillIssuingPartyAddress ON ConsolMasterBillIssuingPartyAddress.E2_ParentID = JK_PK AND ConsolMasterBillIssuingPartyAddress.E2_AddressType = 'MBI'
				JOIN dbo.OrgAddress ConsolMasterBillIssuingPartyOrgAddress ON ConsolMasterBillIssuingPartyAddress.E2_OA_Address = ConsolMasterBillIssuingPartyOrgAddress.OA_PK
				JOIN dbo.OrgCusCode MBIOK ON MBIOK.OK_OH = ConsolMasterBillIssuingPartyOrgAddress.OA_OH AND MBIOK.OK_CodeType = @CarrierCode AND MBIOK.OK_RN_NKCodeCountry = @USCountryCode AND MBIOK.OK_CustomsRegNo <> ''
				JOIN dbo.CusISFBill ON BB_BillNum = (MBIOK.OK_CustomsRegNo + JK_MasterBillNum)
									AND BB_BillType IN (@OceanBillType, @HouseBillType)
									AND BB_BF IN (SELECT BF_PK
													FROM dbo.CusISFHeader
													WHERE BF_SystemCreateTimeUtc >= CASE WHEN JS_SystemCreateTimeUtc IS NULL THEN @FromCreateTime ELSE DATEADD(MONTH, -{2}, JS_SystemCreateTimeUtc) END
													AND BF_SystemCreateTimeUtc <= CASE WHEN JS_SystemCreateTimeUtc IS NULL THEN @ToCreateTime ELSE DATEADD(MONTH, {2}, JS_SystemCreateTimeUtc) END)
				WHERE JS_IsForwardRegistered = 1 AND JS_IsCancelled = 0 AND JS_TransportMode IN (@SeaTransportMode, @AirSeaTransportMode, @SeaAirTransportMode)
					AND JK_AgentType = @DirectAgentType AND JK_TransportMode = @SeaTransportMode
					AND NOT EXISTS (SELECT 1 FROM dbo.CusEntryNum WHERE CE_ParentID = JS_PK AND CE_EntryType = @AMSTypeValue AND CE_EntryNum <> '')
			) AS ISFBillStatus1 ON ISFBillStatus1.ShipmentPK = JS_PK
		) AS ISFBillStatus2
		GROUP BY JS_PK
	) AS ISFBillStatus3
	WHERE ISFBillStatus3.BB_CustomsStatus {3} @CustomsValue
)"
	, isNotMatching ? "NOT" : "" // {0}
	, billList.ToStringWithDelimiterBetweenAppends(",") // {1}
	, recyclePeriod // {2}
	, isBlackTypeFilter ? "<>" : "=" // {3}
	);

			var sqlFilterParameters = new ZSqlParameterCollection(
		ZSqlParameter.New("@MultipleValue", USISF.ISFStatusHelper.Multiple, CusISFBillSchema.BB_CustomsStatus),
		ZSqlParameter.New("@AMSTypeValue", CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.AMS, CusEntryNumSchema.CE_EntryType),
		ZSqlParameter.New("@OceanBillType", USISF.BillTypeList.Codes.OceanBillOfLading, CusISFBillSchema.BB_BillType),
		ZSqlParameter.New("@HouseBillType", USISF.BillTypeList.Codes.HouseBillOfLading, CusISFBillSchema.BB_BillType),
		ZSqlParameter.New("@FromCreateTime", ZDateTime.UtcNow.AddMonths(-recyclePeriod), CusISFHeaderSchema.BF_SystemCreateTimeUtc),
		ZSqlParameter.New("@ToCreateTime", ZDateTime.UtcNow.AddMonths(recyclePeriod), CusISFHeaderSchema.BF_SystemCreateTimeUtc),
		ZSqlParameter.New("@SeaTransportMode", Core.Constants.TransportModes.Sea, JobShipmentSchema.JS_TransportMode),
		ZSqlParameter.New("@AirSeaTransportMode", Core.Constants.TransportModes.AirSea, JobShipmentSchema.JS_TransportMode),
		ZSqlParameter.New("@SeaAirTransportMode", Core.Constants.TransportModes.SeaAir, JobShipmentSchema.JS_TransportMode),
		ZSqlParameter.New("@CustomsValue", isBlackTypeFilter ? ZString.Empty : value, CusISFBillSchema.BB_CustomsStatus),
		ZSqlParameter.New("@DirectAgentType", Core.Constants.AgentType.Direct, JobConsolSchema.JK_AgentType),
		ZSqlParameter.New("@CarrierCode", OrgCusCode.CodeTypes.CarrierCode, OrgCusCodeSchema.OK_CodeType),
		ZSqlParameter.New("@USCountryCode", Core.Constants.CountryCodes.UnitedStates, OrgCusCodeSchema.OK_RN_NKCodeCountry)
		);

			var result = new ZDBOnlyQuery(typeof(ForwardingShipment));
			result.AddFilterAndZSQLParameterCollection(sqlFilter, sqlFilterParameters);
			return result;
		}

		#endregion

		#region SG Filters

		ZQuery GetSGCustomsURN(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var result = new ZDBOnlyQuery(typeof(ForwardingShipment));

			var declarationQuery = new ZDBOnlySubQuery(typeof(IBaseJobDeclaration), JobDeclarationSchema.JE_JS);
			var cusEntriesSubQuery = new ZDBOnlySubQuery(typeof(ICusEntryHeader), CusEntryHeaderSchema.CH_JE);

			var ediMessageSubQuery = new ZDBOnlySubQuery(typeof(EDIMessage), EDIMessageSchema.EM_LinkUniqueID);
			ediMessageSubQuery.AddToFilter(EDIMessageSchema.EM_ApplicationReference, comparisonOperator, value);
			ediMessageSubQuery.AddToFilter(EDIMessageSchema.EM_LinkTable, CusEntryHeaderSchema.Constants.TableName);

			cusEntriesSubQuery.AddSubQuery(ediMessageSubQuery, JoinCondition.And);
			declarationQuery.AddSubQuery(cusEntriesSubQuery, JoinCondition.And);

			result.AddSubQuery(declarationQuery, JoinCondition.And);
			return result;
		}

		#endregion

		#region JP AFR Bill Status

		void AddAFRStatusFilter(ModuleFilterCollection filters)
		{
			var afrBillStatusFilter = filters.AddTextFilter(Descriptions.AFRBillStatus, GetAFRBillStatusQuery, AFRBillStatusList).WithMaxLengthOf<ModuleTextFilter>(JPAFRBillsSchema.JPB_ReleaseStatus);
			afrBillStatusFilter.Category = FilterCategories.StatusAndFlags;
			afrBillStatusFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.IsBlank);
			afrBillStatusFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.IsNotBlank);
			afrBillStatusFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotContain);
			afrBillStatusFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotEqual);
			afrBillStatusFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotStartsWith);
			afrBillStatusFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.StartsWith);
			afrBillStatusFilter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobShipmentFilter|AFRBillStatusDescription", Descriptions.AFRBillStatus);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Part of SQL query")]
		ZQuery GetAFRBillStatusQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var basequery = @" {0} IN (
SELECT {0}
FROM (
SELECT {0} ,
CASE
WHEN {1} = @releaseStatus THEN 1
WHEN {1} = @alternativeReleaseStatus THEN 1
WHEN @releaseStatus = '{21}' AND {1} IS NULL THEN 1
ELSE 0
END AS matchStatusCount
FROM {2}
{3} JOIN {4}  ON {5} = {0}
{3} JOIN {6}  ON {7} = {8}
{3} JOIN {9}  ON {8} = {10} AND {11}='{12}' AND {13} = 1
{3} JOIN {14}  ON {15} = {16} AND ({17}= {18} OR {17} LIKE '____'+{18})
CROSS APPLY dbo.IsGoingViaIgnoringDomesticRouteInLine({8}, '{22}',{23},{24},{25},{26}) AS GoingVia
WHERE GoingVia.IsGoingVia = 1
AND {19} = '{20}'
) AS aggregatedTable
GROUP BY {0}
{27}
)";

			var sqlFilter = string.Format(CultureInfo.InvariantCulture, basequery,
JobShipmentSchema.Constants.PK,                         //{0}	JS_PK
JPAFRBillsSchema.Constants.JPB_ReleaseStatus,           //{1}	JPB_ReleaseStatus
JobShipmentSchema.Constants.TableName,                  //{2}	JobShipment
value == AFRBillCustomsStatusList.Codes.NotRegistered ? "LEFT" : "INNER",
JobConShipLinkSchema.Constants.TableName,               //{4}	JobConShipLink
JobConShipLinkSchema.Constants.JN_JS,                   //{5}	JN_JS
JobConsolSchema.Constants.TableName,                    //{6}	JobConsol
JobConShipLinkSchema.Constants.JN_JK,                   //{7}	JN_JK
JobConsolSchema.Constants.PK,                           //{8}	JK_PK
JPAFRHeaderSchema.Constants.TableName,                  //{9}	JPAFRHeader
JPAFRHeaderSchema.Constants.JPH_ParentId,               //{10}	JPH_ParentId
JPAFRHeaderSchema.Constants.JPH_ParentTableCode,        //{11}	JPH_ParentTableCode
JobConsolSchema.Constants.Prefix,                       //{12}	JK
JPAFRHeaderSchema.Constants.JPH_IsActive,               //{13}	JPH_IsActive
JPAFRBillsSchema.Constants.TableName,                   //{14}	JPAFRBills
JPAFRHeaderSchema.Constants.PK,                         //{15}	JPH_PK
JPAFRBillsSchema.Constants.JPB_JPH_Header,              //{16}	JPB_JPH_Header
JPAFRBillsSchema.Constants.JPB_BillNumber,              //{17}	JPB_BillNumber
JobShipmentSchema.Constants.JS_HouseBill,               //{18}	JS_HouseBill
JobConsolSchema.Constants.JK_TransportMode,             //{19}	JK_TransportMode
Core.Constants.TransportModes.Sea,                      //{20}	'SEA'
AFRBillCustomsStatusList.Codes.NotRegistered,           //{21}	'NOT'
Core.Constants.CountryCodes.Japan,                      //{22}	'JP'
JobConsolSchema.Constants.JK_RL_NKDischargePort,        //{23}	JK_RL_NKDischargePort
JobConsolSchema.Constants.JK_RL_NKFirstForeignPort,     //{24}	JK_RL_NKFirstForeignPort
JobConsolSchema.Constants.JK_RL_NKLastForeignPort,      //{25}	JK_RL_NKLastForeignPort
JobConsolSchema.Constants.JK_RL_NKPortOfFirstArrival,   //{26}	JK_RL_NKPortOfFirstArrival
comparisonOperator == SQLComparisonOperator.Equal ?
"HAVING SUM(aggregatedTable.matchStatusCount) = COUNT(aggregatedTable.matchStatusCount)"
: "HAVING SUM(aggregatedTable.matchStatusCount) > 0"
);

			var sqlFilterParameters = new ZSqlParameterCollection();
			sqlFilterParameters.Add(ZSqlParameter.New("@releaseStatus", value, JPAFRBillsSchema.JPB_ReleaseStatus));
			sqlFilterParameters.Add(ZSqlParameter.New("@alternativeReleaseStatus", value == AFRBillCustomsStatusList.Codes.NotRegistered ? ZString.Empty : value, JPAFRBillsSchema.JPB_ReleaseStatus));

			var result = new ZDBOnlyQuery(typeof(ForwardingShipment));
			result.AddFilterAndZSQLParameterCollection(sqlFilter, sqlFilterParameters);

			return result;
		}

		CodeDescriptionPairList AFRBillStatusList
		{
			get { return factory.GetCachedValue<AFRBillCustomsStatusList>(); }
		}

		#endregion

		#region GetCustomsEntryStatusQuery

		ZQuery GetCustomsMessageStatusQuery(ZString customsEntryStatus)
		{
			return GetCustomsStatusQuery(customsEntryStatus);
		}

		ZQuery GetCustomsStatusQuery(ZString customsEntryStatus)
		{
			var result = new ZDBOnlyQuery(typeof(ForwardingShipment));
			if (!customsEntryStatus.IsEmpty)
			{
				string sQL = string.Format(CultureInfo.InvariantCulture, @"
{0} IN ( SELECT {1} FROM {2}
						INNER JOIN {3}  ON {4} = {5}
						INNER JOIN {6}  ON {7} = {8}
					WHERE {9} = @customsStatus AND {10} = @CurrentCompany )"
, Freight.Common.Business.AutoJobShipment.Schema.PK //0
, JobDeclarationSchema.Constants.JE_JS //1
, JobDeclarationSchema.Constants.TableName //2
, GlbBranchSchema.Constants.TableName //3
, GlbBranchSchema.Constants.PK //4
, JobDeclarationSchema.Constants.JE_GB //5
, CusEntryHeaderSchema.Constants.TableName //6
, CusEntryHeaderSchema.Constants.CH_JE //7
, JobDeclarationSchema.Constants.PK //8
, CusEntryHeaderSchema.Constants.CH_Status //9
, GlbBranchSchema.Constants.GB_GC //10
				);

				result.AddFilterAndZSQLParameterCollection(
										sQL,
										new ZSqlParameterCollection(
						ZSqlParameter.New("@customsStatus", customsEntryStatus == FilterStatus.NotSentCustomsStatusForFilter ? ZString.Empty : customsEntryStatus, CusEntryHeaderSchema.CH_Status),
											ZSqlParameter.New("@CurrentCompany", GlbCompany.CurrentCompany.PK, GlbBranchSchema.GB_GC)
										)
									);
			}

			return result;
		}

		ZQuery GetAUCustomsEntryStatusQuery(ZString customsEntryStatus)
		{
			var result = new ZDBOnlyQuery(typeof(ForwardingShipment));

			string sQL = string.Format(CultureInfo.InvariantCulture, @"
 {0} IN ( SELECT {1} FROM {2}
					INNER JOIN {3}  ON {4} = {5}
					WHERE {6} = @customsStatus AND {7} = @CurrentCompany )"
, Freight.Common.Business.AutoJobShipment.Schema.PK //0
, JobDeclarationSchema.Constants.JE_JS //1
, JobDeclarationSchema.Constants.TableName //2
, GlbBranchSchema.Constants.TableName //3
, GlbBranchSchema.Constants.PK //4
, JobDeclarationSchema.Constants.JE_GB //5
, JobDeclarationSchema.Constants.JE_EntryStatus //6
, GlbBranchSchema.Constants.GB_GC //7
			);

			result.AddFilterAndZSQLParameterCollection(
				sQL,
				new ZSqlParameterCollection(
						ZSqlParameter.New("@customsStatus", customsEntryStatus == FilterStatus.NotSentCustomsStatusForFilter ? ZString.Empty : customsEntryStatus, JobDeclarationSchema.JE_EntryStatus),
						ZSqlParameter.New("@CurrentCompany", GlbCompany.CurrentCompany.PK, GlbBranchSchema.GB_GC)
				)
			);

			return result;
		}

		/// <summary>
		/// Status of the last MESSAGING transaction, which may not be the same as the entry's status.
		/// </summary>
		/// <param name="countryCode"></param>
		/// <returns></returns>
		CodeDescriptionPairList GetCustomsMessagingStatusList(ZString countryCode)
		{
			var result = new CodeDescriptionPairList();
			if (ObjectFactory.Get<Shared.IEuropeanUnionCustomsMembersProvider>().IsInEuropeanCustomsUnionOrInheritsFromEU(countryCode))
			{
				result = new Common.EU.MessageStatusList();
			}
			return result;
		}

		/// <summary>
		/// Codes for the overall status of the ENTRY, not the messaging/transaction status
		/// </summary>
		CodeDescriptionPairList GetCustomsEntryStatusList(ZString countryCode) => (CodeDescriptionPairList)EntryStatusListHelper.EntryStatusListForShipments(factory, countryCode);

		#endregion

		#region ModuleFilter Lists

		CodeDescriptionPairList GetNewEntryType_List()
		{
			CodeDescriptionPairList result = new CustomsEntryType();
			return result;
		}

		public CodeDescriptionPairList MessageStatus_ListCRL
		{
			get
			{
				return factory.GetCachedValue("CRLStatusListForFilter", delegate
				{
					CodeDescriptionPairList result = new MessageStatusListCRL();

					result.RemoveCode(MessageStatusListCRL.Codes.NotSent);
					result.AddPair(FilterStatus.NotSentCustomsStatusForFilter, FilterStatus.NotSentForFilterDescription);

					return result;
				});
			}
		}

		public CodeDescriptionPairList SEBillStatusList
		{
			get
			{
				var today = ZDateTime.Today;
				return factory.GetCachedValue("SO50RecordDispCodes" + today.ToString("MMddyy"), delegate
				{
					var result = new CodeDescriptionPairList();
					var so50RecordDispCodes = ZZRefCusCodeListCombined.Loader.Load(factory, Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SO50RecordDispCode, today);
					foreach (var code in so50RecordDispCodes)
					{
						result.AddPairIfNotExist(code.ZZD_Code, code.ZZD_Description);
					}
					result.AddPair(SEBillProcessingResultList.BillStatusHoldOrExam, SEBillProcessingResultList.BillStatusHoldOrExamDesc);
					result.Sort();
					return result;
				});
			}
		}

		public HLDOrEXMStatusList HLDOrEXMStatusList
		{
			get
			{
				return factory.GetCachedValue<HLDOrEXMStatusList>();
			}
		}

		public CodeDescriptionPairList MessageStatus_ListENS
		{
			get
			{
				return factory.GetCachedValue("ENSStatusListForFilter", delegate
				{
					CodeDescriptionPairList result = new MessageStatusListENS();

					result.RemoveCode(MessageStatusListENS.Codes.NotSent);
					result.AddPair(FilterStatus.NotSentCustomsStatusForFilter, FilterStatus.NotSentForFilterDescription);

					return result;
				});
			}
		}

		public CodeDescriptionPairList MessageStatus_ListISFBillStatus
		{
			get
			{
				return factory.GetCachedValue("ShipmentUSISF.DispositionCodeListWithMultiple", delegate
				{
					var result = new USISF.DispositionCodeList();
					result.AddPair(USISF.ISFStatusHelper.Multiple, USISF.ISFStatusHelper.BillFoundOnMultipleISF);
					return result;
				});
			}
		}

		internal CodeDescriptionPairList MessageStatus_ListEXP
		{
			get
			{
				return factory.GetCachedValue("EXPStatusListForFilter", delegate
				{
					CodeDescriptionPairList result = new AESDirectCustomsEntryStatus();

					result.RemoveCode(AESDirectCustomsEntryStatus.Codes.NotSent);
					result.AddPair(FilterStatus.NotSentCustomsStatusForFilter, FilterStatus.NotSentForFilterDescription);

					return result;
				});
			}
		}

		public CodeDescriptionPairList MessageStatus_ListIT
		{
			get
			{
				return factory.GetCachedValue("ITStatusListForFilter", delegate
				{
					CodeDescriptionPairList result = new MessageStatusListIT();

					result.RemoveCode(MessageStatusListIT.Codes.NotSent);
					result.AddPair(FilterStatus.NotSentCustomsStatusForFilter, FilterStatus.NotSentForFilterDescription);

					return result;
				});
			}
		}

		CodeDescriptionPairList GetNewAirSeaCargoCustomsStatus_List()
		{
			CodeDescriptionPairList result = new CMRConsolidatedCargoStatuses();
			result.Insert(0, new CodeDescriptionPair(NotClearCargoCustomsStatus, Res.GetString("Forwarding|JobShipmentFilter|NotClear", "Not Clear")));
			return result;
		}

		#endregion

		#region US Declaration Business Object Filter

		internal US.IJobDeclarationFilterBusinessObject USDeclarationFilter
		{
			get
			{
				if (fUSDeclarationFilter == null)
				{
					fUSDeclarationFilter = (US.IJobDeclarationFilterBusinessObject)Activator.CreateInstance(ObjectFactory.GetType<US.IJobDeclarationFilterBusinessObject>());
				}
				return fUSDeclarationFilter;
			}
		}
		US.IJobDeclarationFilterBusinessObject fUSDeclarationFilter;

		#endregion

		#region QueryHelper

		GenAddOnColumnQueryHelper QueryHelper
		{
			get
			{
				if (queryHelper == null)
				{
					queryHelper = new GenAddOnColumnQueryHelper(ObjectFactory.GetType<IBaseJobDeclaration>(), ObjectFactory.GetType<US.IBill>(), CusDecHouseBillSchema.CU_JE);
				}
				return queryHelper;
			}
		}
		GenAddOnColumnQueryHelper queryHelper;

		GenAddOnColumnQueryHelper SimpleQueryHelper
		{
			get
			{
				if (simpleQueryHelper == null)
				{
					simpleQueryHelper = new GenAddOnColumnQueryHelper(typeof(ForwardingShipment), ObjectFactory.GetType<IBaseJobDeclaration>(), JobDeclarationSchema.JE_JS);
				}
				return simpleQueryHelper;
			}
		}
		GenAddOnColumnQueryHelper simpleQueryHelper;

		#endregion
	}
}
