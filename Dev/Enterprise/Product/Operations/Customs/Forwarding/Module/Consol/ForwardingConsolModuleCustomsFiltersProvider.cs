using System;
using System.Globalization;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.JP.AFR;
using Enterprise.Customs.Common.Module;
using Enterprise.Customs.Common.NZ;
using Enterprise.Customs.Common.US.AMS;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration.ZArchitecture;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Customs;
using static Enterprise.Integration.Customs.EUICS2;

namespace Enterprise.Customs.Forwarding.Module
{
	public class ForwardingConsolModuleCustomsFiltersProvider : IModuleCustomFiltersProvider
	{
		public ForwardingConsolModuleCustomsFiltersProvider(BusinessObjectFactory factory, FilterStripBusinessObject filterBusinessObject)
		{
			this.factory = factory;
			this.filterBusinessObject = Argument.NotNull(filterBusinessObject, nameof(filterBusinessObject));
		}

		readonly BusinessObjectFactory factory;
		readonly FilterStripBusinessObject filterBusinessObject;

		#region Constants

		public static class Descriptions
		{
			#region SuppressResourceStringsCheckRegion

			public const string AsycudaRegistrationNumber = "Manifest Registration Number";
			public const string AsycudaRegistrationDate = "Manifest Registration Date";
			public const string AsycudaRegistrationStatus = "Manifest Registration Status";
			public const string AMSBillStatus = "AMS Bill Status";
			public const string AFRBillStatus = "AFR Bill Status";
			public const string OutwardReportStatus = "Outward Report Status";
			public const string LatestAMSDisposition = "Latest AMS Disposition";

			#endregion
		}

		#endregion

		public void AddFilters(IModuleFilterCollection filterCollection)
		{
			var filters = (ModuleFilterCollection)filterCollection;

			CusEntryNumberFilterStripHelper.AddReferenceNumberDateFilter(filters, typeof(ForwardingConsol));
			ObjectFactory.Get<CA.IConsolModuleColumnsAndFiltersProvider>().AddFilters(filters, factory);

			AddAMSBillStatusIfNeeded(filters);
			AddAFRBillStatus(filters);
			AddLatestAMSDispositionFilter(filters);

			if (AsycudaIsEnabled)
			{
				var asycudaRegistrationNumberFilter = filters.AddTextFilter(Descriptions.AsycudaRegistrationNumber, GetAsycudaRegistrationNumberQuery);
				asycudaRegistrationNumberFilter.MaxLength = CusEntryNumSchema.CE_EntryNum.MaxLength;
				asycudaRegistrationNumberFilter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobConsolFilter|Asycuda.Registration", Descriptions.AsycudaRegistrationNumber);

				filters.AddDateFilter(Descriptions.AsycudaRegistrationDate, GetAsycudaRegistrationDateQuery, false).MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobConsolFilter|Asycuda.Date", Descriptions.AsycudaRegistrationDate);

				var asycudaStatusFilter = filters.AddTextFilter(Descriptions.AsycudaRegistrationStatus, GetAsycudaRegistrationStatusQuery, () => AsycudaRegistrationStatus_List);
				asycudaStatusFilter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobConsolFilter|Asycuda.Status", Descriptions.AsycudaRegistrationStatus);
				asycudaStatusFilter.MaxLength = CusEntryNumSchema.CE_EntryStatus.MaxLength;

				asycudaStatusFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.Contains);
				asycudaStatusFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotContain);
				asycudaStatusFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.IsBlank);
				asycudaStatusFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.IsNotBlank);
				asycudaStatusFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.StartsWith);
				asycudaStatusFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotStartsWith);
			}

			if (GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.NewZealand)
			{
				var outwardReportFilter = filters.AddTextFilter(Descriptions.OutwardReportStatus, GetOutwardReportStatusQuery, OutwardReportStatus_List);
				outwardReportFilter.Category = FilterCategories.StatusAndFlags;
				outwardReportFilter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobConsolFilter|OutwardReportStatusDescription", "Outward Report Status");
				outwardReportFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
				outwardReportFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.Contains);
				outwardReportFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.IsBlank);
				outwardReportFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.IsNotBlank);
				outwardReportFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotContain);
				outwardReportFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotEqual);
				outwardReportFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotStartsWith);
				outwardReportFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.StartsWith);
			}
		}

		#region GetAMSBillStatusQuery

		void AddAMSBillStatusIfNeeded(ModuleFilterCollection filters)
		{
			var amsBillStatusFilter = new AMSBillStatusFilter(Descriptions.AMSBillStatus, GetAMSBillStatusQuery, AMSBillStatusList, filterBusinessObject);
			filters.AddFilter(amsBillStatusFilter);
			amsBillStatusFilter.Category = FilterCategories.StatusAndFlags;
			amsBillStatusFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
			amsBillStatusFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.Contains);
			amsBillStatusFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.IsBlank);
			amsBillStatusFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.IsNotBlank);
			amsBillStatusFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotContain);
			amsBillStatusFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotEqual);
			amsBillStatusFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotStartsWith);
			amsBillStatusFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.StartsWith);
			amsBillStatusFilter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobConsolFilter|AMSBillStatusDescription", Descriptions.AMSBillStatus);
		}

		ZQuery GetAMSBillStatusQuery(SQLComparisonOperator filterOperator, ZString value)
		{
			if (filterBusinessObject[FilterDescriptions.CreatedTime] is ModuleDateFilter createdTimeFilter && createdTimeFilter.IsPropertySearchValid)
			{
				var createdTimeQuery = createdTimeFilter.Query;
				var createdTimeScript = createdTimeQuery.ParameterisedText.ParameterisedQueryText;
				var sqlFilterParameters = new ZSqlParameterCollection(ZSqlParameter.New("@AMSBillStatusDescription", value, OrgCusCodeSchema.OK_CustomsRegNo));
				foreach (var parameter in createdTimeQuery.Params.OrderByDescending(x => x.ParameterName.Length)
					.ThenBy(x => x.ParameterName))
				{
					var oldParameterName = parameter.ParameterName;
					var newParameterName = oldParameterName.Replace("@", "@USAMS");
					parameter.Rename(newParameterName);
					createdTimeScript = createdTimeScript.Replace(oldParameterName, newParameterName);
					sqlFilterParameters.Add(parameter);
				}

				var isOutOfSync = value == AMSConsolBillCustomsStatusList.Codes.OutOfSync;
				var headerJoinType = isOutOfSync ? "LEFT" : "INNER";
				if (!createdTimeScript.IsNullOrEmpty())
				{
					createdTimeScript = $"AND ({createdTimeScript})";
				}

				var sqlFilter = FormattableString.Invariant($@"
JK_PK IN 
(
	SELECT JK_PK
	FROM dbo.JobConsol
	{headerJoinType} JOIN
	(
			SELECT BH_PK, BH_ParentID
			FROM dbo.CusInBondHeader
			WHERE BH_ApplicationCode = '{CusInBondApplicationCodeList.Codes.AMS}'
	) AS AMSHeader ON (BH_ParentID = JK_PK)
	CROSS APPLY dbo.GetConsolAMSBillStatusInLine(JK_PK, BH_PK) AMSStatus
	WHERE JK_TransportMode IN ('SEA', 'RAI')
	AND
	(
		CASE
			WHEN SUBSTRING(JK_RL_NKDischargePort, 1, 2) IN  ('US', 'PR') THEN 1
			WHEN SUBSTRING(JK_RL_NKFirstForeignPort, 1, 2) IN ('US', 'PR') THEN 1
			WHEN SUBSTRING(JK_RL_NKLastForeignPort, 1, 2) IN ('US', 'PR') THEN 1
			WHEN SUBSTRING(JK_RL_NKPortOfFirstArrival, 1, 2) IN ('US', 'PR') THEN 1
			WHEN EXISTS
			(
					SELECT NULL
					FROM dbo.JobConsolTransport
					WHERE JW_ParentGUID = JK_PK AND SUBSTRING(JW_RL_NKDiscPort, 1, 2) IN ('US', 'PR') AND SUBSTRING(JW_RL_NKLoadPort, 1, 2) <> SUBSTRING(JW_RL_NKDiscPort, 1, 2)
			) THEN 1
			ELSE 0
		END = 1
	)
	{createdTimeScript}
	AND AMSStatus.Status = @AMSBillStatusDescription
)
");

				var result = new ZDBOnlyQuery(typeof(ForwardingConsol));
				result.AddFilterAndZSQLParameterCollection(sqlFilter, sqlFilterParameters);
				return result;
			}
			else
			{
				return ZQuery.NoResultQuery;
			}
		}

		CodeDescriptionPairList AMSBillStatusList => factory.GetCachedValue<AMSConsolBillCustomsStatusList>();

		#endregion

		#region Asycuda

		CodeDescriptionPairList AsycudaRegistrationStatus_List
		{
			get
			{
				if (asycudaRegistrationStatus_List == null)
				{
					var registrationStatusesProvider = ObjectFactory.Get<IEUICS2CustomsStatusList>();
					var fullList = new Common.Shared.AsycudaRegistrationStatuses().Cast<ICodeDescription>().Union(registrationStatusesProvider.OfType<ICodeDescription>()).OrderBy(cd => cd.Code);
					asycudaRegistrationStatus_List = new CodeDescriptionPairList();
					asycudaRegistrationStatus_List.AddPairsIfNotExist(fullList);
				}

				return asycudaRegistrationStatus_List;
			}
		}

		CodeDescriptionPairList asycudaRegistrationStatus_List;

		bool AsycudaIsEnabled
		{
			get { return true; } // Brendon I think that this should be enabled all the time. Or you could limit to having CurrentCompany.Country in (VU, SB, PG, etc). But that would mean you can't find consols for ASYCUDA from the exporting country login. 
		}

		static ZQuery GetCommonAsycudaQuery(Action<ZDBOnlySubQuery> functionToAddParameters)
		{
			var result = new ZDBOnlyQuery(typeof(ForwardingConsol));

			var manifestSubQuery = new ZDBOnlySubQuery(typeof(ASYCUDA.IAsycudaManifestHeader), AsycudaManifestHeaderSchema.AMA_ParentId);
			manifestSubQuery.AddToFilter(AsycudaManifestHeaderSchema.AMA_ParentTableCode, JobConsolSchema.Constants.Prefix);
			manifestSubQuery.AddToFilter(AsycudaManifestHeaderSchema.AMA_ApplicationCode, SQLComparisonOperator.NotEqual, "OUT");
			var fieldFilter = new ZDBOnlySubQuery(typeof(CusEntryNumber), CusEntryNumSchema.CE_ParentID);
			fieldFilter.AddToFilter(CusEntryNumSchema.CE_EntryType, CusEntryNumberTypes.ASYCUDA.AsycudaRegistration);
			fieldFilter.AddToFilter(CusEntryNumSchema.CE_ParentTable, AsycudaManifestHeaderSchema.Constants.TableName);
			functionToAddParameters(fieldFilter);
			manifestSubQuery.AddSubQuery(fieldFilter, JoinCondition.And);
			result.AddSubQuery(manifestSubQuery, JoinCondition.And);

			return result;
		}

		ZQuery GetAsycudaRegistrationDateQuery(DateComparisonOperator comparisonOperator, ZDateTime date1, ZDateTime date2)
		{
			var result = GetCommonAsycudaQuery(delegate(ZDBOnlySubQuery fieldFilter)
			{
				AddDateRange(fieldFilter, comparisonOperator, JoinCondition.And, CusEntryNumSchema.CE_IssueDate, date1.Date, date2.Date);
			});

			return result;
		}

		ZQuery GetAsycudaRegistrationNumberQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var result = GetCommonAsycudaQuery(delegate(ZDBOnlySubQuery fieldFilter)
			{
				fieldFilter.AddToFilter(CusEntryNumSchema.CE_EntryNum, comparisonOperator, value);
			});

			return result;
		}

		ZQuery GetAsycudaRegistrationStatusQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var result = GetCommonAsycudaQuery(delegate(ZDBOnlySubQuery fieldFilter)
			{
				fieldFilter.AddToFilter(CusEntryNumSchema.CE_EntryStatus, comparisonOperator, value);
			});

			return result;
		}

		#endregion

		#region GetAFRBillStatusQuery

		void AddAFRBillStatus(ModuleFilterCollection filters)
		{
			var afrBillStatusFilter = filters.AddTextFilter(Descriptions.AFRBillStatus, GetAFRBillStatusQuery, AFRBillStatusList);
			afrBillStatusFilter.Category = FilterCategories.StatusAndFlags;
			afrBillStatusFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.IsBlank);
			afrBillStatusFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.IsNotBlank);
			afrBillStatusFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotContain);
			afrBillStatusFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotEqual);
			afrBillStatusFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotStartsWith);
			afrBillStatusFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.StartsWith);
			afrBillStatusFilter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobConsolFilter|AFRBillStatusDescription", Descriptions.AFRBillStatus);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "SQL strings")]
		ZQuery GetAFRBillStatusQuery(SQLComparisonOperator filterOperator, ZString value)
		{
			var baseQuery = @"
{0} in (
	SELECT {1} FROM {2} 
	WHERE {3} = '{4}'
	AND {5} IN (
		SELECT {6} FROM {7} 
		WHERE {8} IN (@AFRBillStatusDescription, @alternativeAFRBillStatus)
	)
	AND {9} = 1
--AdditionFilterPlaceHolder1
)
--AdditionFilterPlaceHolder2
";

			baseQuery = baseQuery.Replace("--AdditionFilterPlaceHolder1", filterOperator != SQLComparisonOperator.Equal ? string.Empty : @"	AND {5} NOT IN (
SELECT {6} FROM {7}
WHERE {8} NOT IN (@AFRBillStatusDescription, @alternativeAFRBillStatus)
)");

			baseQuery = baseQuery.Replace("--AdditionFilterPlaceHolder2", value != AFRBillCustomsStatusList.Codes.NotRegistered ? string.Empty : @" OR (
	{0} IN (
		SELECT Consol.{0}
		FROM {17} Consol 
		CROSS APPLY dbo.IsGoingViaIgnoringDomesticRouteInLine(Consol.{0}, '{12}', Consol.{13}, Consol.{14}, Consol.{15}, Consol.{16}) AS GoingVia
		WHERE Consol.{10} = '{11}' 
		AND GoingVia.IsGoingVia = 1
		AND {0} NOT IN (SELECT {1} FROM {2} WHERE {3} = '{4}' AND {9} = 1 AND {5} IN (SELECT {6} from {7} ))
	)
)");
			var sqlFilter = string.Format(CultureInfo.InvariantCulture, baseQuery
				, JobConsolSchema.Constants.PK                          //{0}	JK_PK
				, JPAFRHeaderSchema.Constants.JPH_ParentId              //{1}	JPH_ParentId
				, JPAFRHeaderSchema.Constants.TableName                 //{2}	JPHAFRHeader
				, JPAFRHeaderSchema.Constants.JPH_ParentTableCode       //{3}	JPH_ParentTableCode
				, JobConsolSchema.Constants.Prefix                      //{4}	JK
				, JPAFRHeaderSchema.Constants.PK                        //{5}	JPH_PK
				, JPAFRBillsSchema.Constants.JPB_JPH_Header             //{6}	JPB_JPH_Header
				, JPAFRBillsSchema.Constants.TableName                  //{7}	JPAFRBills
				, JPAFRBillsSchema.Constants.JPB_ReleaseStatus          //{8}	JPB_ReleaseStatus
				, JPAFRHeaderSchema.Constants.JPH_IsActive              //{9}	JPH_IsActive
				, JobConsolSchema.Constants.JK_TransportMode            //{10}	JK_TransportMode
				, Core.Constants.TransportModes.Sea                          //{11}	'SEA'
				, Core.Constants.CountryCodes.Japan                          //{12}	'JP'
				, JobConsolSchema.Constants.JK_RL_NKDischargePort       //{13}	JK_RL_NKDischargePort
				, JobConsolSchema.Constants.JK_RL_NKFirstForeignPort    //{14}	JK_RL_NKFirstForeignPort
				, JobConsolSchema.Constants.JK_RL_NKLastForeignPort     //{15}	JK_RL_NKLastForeignPort
				, JobConsolSchema.Constants.JK_RL_NKPortOfFirstArrival  //{16}	JK_RL_NKPortOfFirstArrival
				, JobConsolSchema.Constants.TableName                   //{17}	JobConsol
			);

			var sqlFilterParameters = new ZSqlParameterCollection();
			sqlFilterParameters.Add(ZSqlParameter.New("@AFRBillStatusDescription", value, JPAFRBillsSchema.JPB_ReleaseStatus));
			sqlFilterParameters.Add(ZSqlParameter.New("@alternativeAFRBillStatus", value == AFRBillCustomsStatusList.Codes.NotRegistered ? ZString.Empty : value, JPAFRBillsSchema.JPB_ReleaseStatus));

			var result = new ZDBOnlyQuery(typeof(ForwardingConsol));
			result.AddFilterAndZSQLParameterCollection(sqlFilter, sqlFilterParameters);
			return result;
		}

		CodeDescriptionPairList AFRBillStatusList => factory.GetCachedValue<AFRBillCustomsStatusList>();

		#endregion

		#region GetOutwardReportStatusQuery

		ZQuery GetOutwardReportStatusQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var result = new ZDBOnlyQuery(typeof(CommonConsol));
			result.AddToFilter(JobConsolSchema.JK_TransportMode, SQLComparisonOperator.Equal, Core.Constants.TransportModes.Air);
			result.AddToFilter(JoinCondition.Or, JobConsolSchema.JK_TransportMode, SQLComparisonOperator.Equal, Core.Constants.TransportModes.Sea);

			var transportQuery = new ZDBOnlySubQuery(typeof(ForwardingConsol), JobConsolSchema.PK);
			transportQuery.AddToFilter(JobConsolSchema.JK_RL_NKLoadPort, SQLComparisonOperator.StartsWith, Core.Constants.CountryCodes.NewZealand);
			transportQuery.AddToFilter(JobConsolSchema.JK_RL_NKDischargePort, SQLComparisonOperator.DoesNotStartWith, Core.Constants.CountryCodes.NewZealand);
			result.AddSubQuery(transportQuery, JoinCondition.And);

			if (!value.IsEmpty)
			{
				var notIn = value == OutwardReportStatusList.Codes.NotSent;
				var statusFilter = new ZDBOnlySubQuery(typeof(CusEntryNumber), CusEntryNumSchema.CE_ParentID, notIn);
				statusFilter.AddToFilter(CusEntryNumSchema.CE_RN_NKCountryCode, SQLComparisonOperator.Equal, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
				statusFilter.AddToFilter(CusEntryNumSchema.CE_EntryType, CusEntryNumberTypeList.Codes.OutwardReportNumber);
				if (!notIn)
				{
					statusFilter.AddToFilter(CusEntryNumSchema.CE_EntryStatus, value);
				}

				result.AddSubQuery(statusFilter, JoinCondition.And);
			}

			return result;
		}

		#endregion

		#region LatestAMSDisposition

		void AddLatestAMSDispositionFilter(ModuleFilterCollection filters)
		{
			var dispositionFilter = filters.AddNkFilter(Descriptions.LatestAMSDisposition, new GetNkQueryWithOperator((comparisonOperator, value) => GetLatestAMSDispositionQuery(comparisonOperator, value)), ModuleIDs.Customs.Universal.ZZRefCusCodeList, (x) => AMSDispositionCollection);
			dispositionFilter.Category = FilterCategories.StatusAndFlags;
			dispositionFilter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobConsolFilter|LatestAMSDisposition", Descriptions.LatestAMSDisposition);
			dispositionFilter.ComparisonOperator_List.RemoveCode(ModuleNkFilter.ComparisonConstants.NotEqual);
			dispositionFilter.ComparisonOperator_List.RemoveCode(ModuleNkFilter.ComparisonConstants.IsBlank);
			dispositionFilter.ComparisonOperator_List.RemoveCode(ModuleNkFilter.ComparisonConstants.IsNotBlank);
		}

		ZQuery GetLatestAMSDispositionQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var result = new ZDBOnlyQuery(typeof(ForwardingConsol));
			result.AddFilterAndZSQLParameterCollection(
$@"JK_PK IN 
(
	SELECT JK_PK FROM JobConsol
	INNER JOIN CusInBondHeader ON BH_ParentID = JK_PK
	INNER JOIN CusInBondBill ON B0_BH = BH_PK
	CROSS APPLY csfn_GetLatestDispositionCodeInLine(B0_PK, '') AS LatestDisposition
	WHERE BH_ApplicationCode = 'AMS' AND B0_ShipmentType <> 'OBT' 
	AND LatestDisposition.DispositionCode = '{value}'
)"
, null);
			return result;
		}

		IBusinessObjectCollection AMSDispositionCollection
		{
			get { return DispositionCodeListLoader.GetAMSDispositionCollection(factory); }
		}

		US.IDispositionCodeListLoader DispositionCodeListLoader => factory.GetCachedValue("DispositionCodeListLoader", ObjectFactory.Get<US.IDispositionCodeListLoader>);

		#endregion

		#region Implement

		CodeDescriptionPairList OutwardReportStatus_List => outwardReportStatus_List ?? (outwardReportStatus_List = new OutwardReportStatusList());
		CodeDescriptionPairList outwardReportStatus_List;

		void AddDateRange(ZQuery query, DateComparisonOperator comparisonOperator, JoinCondition joinCondition, SchemaDateTimeColumn column, ZDate lowerEmptyOk, ZDate upperEmptyOk)
		{
			query.AddToFilter(new DateQueryBuilder().CreateDateRange(comparisonOperator, column, lowerEmptyOk, upperEmptyOk), joinCondition);
		}

		#endregion
	}
}
