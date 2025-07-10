using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using CargoWise.EntityFramework;
using CargoWise.Licensing;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Client.EDI.Billing.Business;
using Enterprise.Client.EDI.FeatureControl.Business;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Client.EDI.Modules;
using Enterprise.Client.EDI.ReleaseBuilds.Business;
using Enterprise.Client.EDI.TrustedMessaging.Business;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using Res = ZClientEDI.Business.Res;
using ResString = ZClientEDI.Business.ResString;

namespace Enterprise.Client.EDI.LicenceKeyBuilder.Business
{
	public class LicenceFilters
	{
		public const string LicenceInSyncStringYes = "YES";
		public const string LicenceInSyncStringNo = "NO";

		public const string OrganisationActiveStringYes = "Y";
		public const string OrganisationActiveStringNo = "N";

		public static FilterCategory VersionAndLicence => versionAndLicence ?? (versionAndLicence = new FilterCategory((NoResString)"Version and Licence"));
		[SuppressMessage("CargoWiseOne", "CW1021:StaticFieldsAreThreadStaticRule")]
		static FilterCategory versionAndLicence;

		readonly FilterCategory databaseSystemInfo = new FilterCategory((NoResString)"Database System Info");

		#region Licence Database Hosted Location

		CodeDescriptionPairList DatabaseHostingLocationList
		{
			get
			{
				if (databaseHostingLocationList == null)
				{
					databaseHostingLocationList = new CodeDescriptionPairList();
					databaseHostingLocationList.AddPair("ALL", "Hosted With Any Data Center");
					databaseHostingLocationList.AddRange(EDIDataRegistry.Instance.DatabaseHostedLocations.Value);
				}
				return databaseHostingLocationList;
			}
		}
		CodeDescriptionPairList databaseHostingLocationList;

		#endregion

		#region LicenceType List

		public virtual CodeDescriptionPairList LicenceTypeList
		{
			get
			{
				if (fLicenceTypeList == null)
				{
					fLicenceTypeList = new LicenceTypes();
					fLicenceTypeList.Insert(0, new CodeDescriptionPair("ANY", "PUR, TRI or REN"));
				}

				return fLicenceTypeList;
			}
		}
		CodeDescriptionPairList fLicenceTypeList;

		#endregion

		#region LicenceInSync List

		public virtual CodeDescriptionPairList LicenceInSyncList
		{
			get
			{
				if (fLicenceInSyncList == null)
				{
					fLicenceInSyncList = new CodeDescriptionPairList();
					fLicenceInSyncList.AddPair(LicenceFilters.LicenceInSyncStringYes, "Licences in sync");
					fLicenceInSyncList.AddPair(LicenceFilters.LicenceInSyncStringNo, "Licences not in sync");
				}
				return fLicenceInSyncList;
			}
		}
		CodeDescriptionPairList fLicenceInSyncList;

		#endregion

		#region Active Status List

		CodeDescriptionPairList ActiveStatusList
		{
			get
			{
				CodeDescriptionPairList result = new CodeDescriptionPairList();
				result.AddPair(statusActive, FilterStripBusinessObject.CancelledStatusList[0].Description);
				result.AddPair(statusInactive, FilterStripBusinessObject.CancelledStatusList[1].Description);
				result.AddPair(statusAll, FilterStripBusinessObject.CancelledStatusList[2].Description);
				return result;
			}
		}

		#endregion

		public FilterStripBusinessObject FilterStripBusinessObject
		{
			get { return filterStripBusinessObject; }
		}
		readonly FilterStripBusinessObject filterStripBusinessObject;

		readonly MultilingualString statusActive;

		readonly MultilingualString statusAll;

		readonly MultilingualString statusInactive;

		public LicenceFilters(FilterStripBusinessObject filterStripBusObj, MultilingualString statActive, MultilingualString statInactive, MultilingualString statAll)
		{
			statusActive = statActive;
			statusInactive = statInactive;
			statusAll = statAll;
			filterStripBusinessObject = filterStripBusObj;
		}

		public static void AddDateRange(ZQuery query, DateComparisonOperator comparisonOperator, JoinCondition joinCondition, SchemaDateTimeColumn column, ZDate lowerEmptyOk, ZDate upperEmptyOk)
		{
			ZQuery dateQuery = new ZQuery();

			if (comparisonOperator == DateComparisonOperator.HasNoDateEntered)
			{
				if (column.IsNullable)
				{
					dateQuery.AddToFilter(column, SQLComparisonOperator.Equal, null);
				}
				else
				{
					dateQuery.IsNoResultQuery = true;
				}
			}
			else if (comparisonOperator == DateComparisonOperator.HasDateEntered)
			{
				if (column.IsNullable)
				{
					dateQuery.AddToFilter(column, SQLComparisonOperator.NotEqual, null);
				}
			}
			else
			{
				if (lowerEmptyOk.IsValid)
				{
					dateQuery.AddToFilter(column, SQLComparisonOperator.GreaterThanOrEqualToDatePartOnly, lowerEmptyOk);
				}

				if (upperEmptyOk.IsValid)
				{
					dateQuery.AddToFilter(column, SQLComparisonOperator.LessThanOrEqualToDatePartOnly, upperEmptyOk);
				}
			}

			query.AddToFilter(dateQuery, joinCondition);
		}

		#region Licence Company Filters Queries

		public static ZQuery GetLicenceCompanyCodeQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(LicenceHeader));
			ZDBOnlySubQuery lCQuery = new ZDBOnlySubQuery(typeof(LicenceCompany), LicenceHeaderSchema.LA_LC);
			lCQuery.AddToFilter(LicenceCompanySchema.LC_CompanyCode, comparisonOperator, value);
			query.AddSubQuery(lCQuery, JoinCondition.And);
			return query;
		}

		#endregion

		#region Licence Header Filters Queries

		public static ZQuery GetSiteLiveQuery(DateComparisonOperator comparisonOperator, ZDateTime date1, ZDateTime date2)
		{
			ZQuery result = new ZQuery();
			AddDateRange(result, comparisonOperator, JoinCondition.And, LicenceHeaderSchema.LA_SiteLiveDate, date1.Date, date2.Date);
			return result;
		}

		public static ZQuery GetAgreedLiveQuery(DateComparisonOperator comparisonOperator, ZDateTime date1, ZDateTime date2)
		{
			ZQuery result = new ZQuery();
			AddDateRange(result, comparisonOperator, JoinCondition.And, LicenceHeaderSchema.LA_AgreedLiveDate, date1.Date, date2.Date);
			return result;
		}

		public static ZQuery GetLicenceEditionTypeQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			return new ZQuery(LicenceHeaderSchema.LA_LicenceAdvStdOth, comparisonOperator, value);
		}

		public static ZQuery GetLastDiscrepancyChangeQuery(DateComparisonOperator comparisonOperator, ZDateTime date1, ZDateTime date2)
		{
			ZQuery query = new ZQuery();
			AddDateRange(query, comparisonOperator, JoinCondition.And, LicenceHeaderSchema.LA_LastFaxReport, date1.Date, date2.Date);
			return query;
		}

		public static ZQuery GetSupportExpiryDateQuery(DateComparisonOperator comparisonOperator, ZDateTime date1, ZDateTime date2)
		{
			ZQuery query = new ZQuery();
			AddDateRange(query, comparisonOperator, JoinCondition.And, LicenceHeaderSchema.LA_ContractExpiryDate, date1.Date, date2.Date);
			return query;
		}

		public static ZQuery GetLicenceInSyncQuery(ZString value)
		{
			ZQuery query = new ZQuery();
			if (value == LicenceInSyncStringYes || value == LicenceInSyncStringNo)
			{
				ZBool valueBool = value == LicenceInSyncStringYes ? ZBool.True : ZBool.False;
				query.AddToFilter(LicenceHeaderSchema.LA_LastLicenceCheckInSync, SQLComparisonOperator.Equal, valueBool);
			}
			return query;
		}

		public ZQuery GetLicenceActiveQuery(ZString value)
		{
			ZQuery query = new ZQuery();
			AddStatusFilter(query, LicenceHeaderSchema.LA_IsActive, value);
			query.IgnoreActiveFilter = true;
			return query;
		}

		public ZQuery GetDatabaseActiveQuery(ZString value)
		{
			ZQuery query = new ZQuery();
			AddStatusFilter(query, LicenceDatabaseSchema.LD_IsActive, value);
			return query;
		}

		public ZQuery GetCurrencyQuery(ZString value)
		{
			ZDBOnlyQuery result = new ZDBOnlyQuery(typeof(LicenceHeader));
			ZDBOnlySubQuery subQuery = new ZDBOnlySubQuery(typeof(LicenceCompany), LicenceHeaderSchema.LA_LC);
			subQuery.AddToFilter(LicenceCompanySchema.LC_RX_NKCurrency, value);
			result.AddSubQuery(subQuery, JoinCondition.And);
			return result;
		}

		public ZQuery GetFlagLicenceCompanyQuery(ZBool value, SchemaBoolColumn column)
		{
			ZDBOnlyQuery result = new ZDBOnlyQuery(typeof(LicenceHeader));
			ZDBOnlySubQuery subQuery = new ZDBOnlySubQuery(typeof(LicenceCompany), LicenceHeaderSchema.LA_LC);
			subQuery.AddToFilter(column, value);
			result.AddSubQuery(subQuery, JoinCondition.And);
			return result;
		}

		public ZQuery GetIsReciprocalQuery(ZBool value)
		{
			return GetFlagLicenceCompanyQuery(value, LicenceCompanySchema.LC_IsReciprocal);
		}

		public ZQuery GetIsGSTRegisteredQuery(ZBool value)
		{
			return GetFlagLicenceCompanyQuery(value, LicenceCompanySchema.LC_IsGSTRegistered);
		}

		public ZQuery GetIsGSTCashBasisQuery(ZBool value)
		{
			return GetFlagLicenceCompanyQuery(value, LicenceCompanySchema.LC_IsGSTCashBasis);
		}

		public ZQuery GetIsWHTRegisteredQuery(ZBool value)
		{
			return GetFlagLicenceCompanyQuery(value, LicenceCompanySchema.LC_IsWHTRegistered);
		}

		public ZQuery GetIsWHTCashBasisQuery(ZBool value)
		{
			return GetFlagLicenceCompanyQuery(value, LicenceCompanySchema.LC_IsWHTCashBasis);
		}

		#endregion

		#region Licence Database Filters Queries

		public static ZQuery GetReleaseRingQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			return new ZQuery(LicenceDatabaseSchema.LD_ReleaseRing, comparisonOperator, value);
		}

		public static ZQuery GetLicenceEmailQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			return new ZQuery(LicenceDatabaseSchema.LD_PublicEmailAddressForUpdate, comparisonOperator, value);
		}

		public static ZQuery GetDatabaseLicenceTypeQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			return new ZQuery(LicenceDatabaseSchema.LD_LicenceType, comparisonOperator, value);
		}

		public static ZQuery GetDatabaseServerCodeQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			return new ZQuery(LicenceDatabaseSchema.LD_ServerCode, comparisonOperator, value);
		}

		public static ZQuery GetDatabaseServerNameQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			return new ZQuery(LicenceDatabaseSchema.LD_ReportedHostServerName, comparisonOperator, value);
		}

		public static ZQuery GetDatabaseNameQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			return new ZQuery(LicenceDatabaseSchema.LD_ReportedHostDBName, comparisonOperator, value);
		}

		public static ZQuery GetDatabaseConnectionServerQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			return new ZQuery(LicenceDatabaseSchema.LD_HostConnectionServerName, comparisonOperator, value);
		}

		public static ZQuery GetDatabaseRegistrationQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			return new ZQuery(LicenceDatabaseSchema.LD_Status, comparisonOperator, value);
		}

		public static ZQuery GetSqlEditionQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			ZQuery query = new ZQuery();
			if (value == SqlServerVersionDetailsFilterHelper.Blank)
			{
				value = ZString.Empty;
			}
			query.AddToFilter(LicenceDatabaseSchema.LD_SQLEdition, comparisonOperator, value);
			return query;
		}

		public static ZQuery GetSqlVersionQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			ZQuery query = new ZQuery();
			if (value == SqlServerVersionDetailsFilterHelper.Blank)
			{
				value = ZString.Empty;
			}
			query.AddToFilter(LicenceDatabaseSchema.LD_SQLVersion, comparisonOperator, value);
			return query;
		}

		public static ZQuery GetDatabaseHostedLocationQuery(ZString value)
		{
			ZQuery query = new ZQuery();

			if (value == "ALL")
			{
				query.AddToFilter(LicenceDatabaseSchema.LD_HostedLocation, SQLComparisonOperator.NotEqual, Enterprise.Core.Constants.LicenceConstants.NotHostedWithCargoWise);
			}
			else
			{
				query.AddToFilter(LicenceDatabaseSchema.LD_HostedLocation, value);
			}

			return query;
		}

		public static ZQuery GetLastHeartbeatQuery(DateComparisonOperator comparisonOperator, ZDateTime date1, ZDateTime date2)
		{
			ZQuery query = new ZQuery();
			AddDateRange(query, comparisonOperator, JoinCondition.And, LicenceDatabaseSchema.LD_LastHeartbeat, date1.Date, date2.Date);
			return query;
		}

		public static ZQuery GetDatabaseShutdownQuery(DateComparisonOperator comparisonOperator, ZDateTime date1, ZDateTime date2)
		{
			ZQuery query = new ZQuery();
			AddDateRange(query, comparisonOperator, JoinCondition.And, LicenceDatabaseSchema.LD_LicenceExpiry, date1.Date, date2.Date);
			return query;
		}

		public static ZQuery GetDatabaseManualShutdownQuery(DateComparisonOperator comparisonOperator, ZDateTime date1, ZDateTime date2)
		{
			ZQuery query = new ZQuery();
			AddDateRange(query, comparisonOperator, JoinCondition.And, LicenceDatabaseSchema.LD_ManualLicenceExpiry, date1.Date, date2.Date);
			return query;
		}

		public static ZQuery GetDatabaseIdQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			int number;
			if (!Base27Encoding.TryDecode(value, out number))
			{
				number = -1;
			}
			return new ZQuery(LicenceDatabaseSchema.LD_DatabaseNumber, comparisonOperator, number);
		}

		public static ZQuery DatabaseStaffListReceivedQuery(ZBool value)
		{
			return new ZQuery(LicenceDatabaseSchema.LD_StaffFirstReportUtc, value ? SQLComparisonOperator.NotEqual : SQLComparisonOperator.Equal, DBNull.Value);
		}

		#endregion

		#region Licence Module Filters Queries

		public static ZQuery GetLicencedModuleQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			return new ZQuery(LicenceModulesSchema.LM_GroupModuleCode, comparisonOperator, value);
		}

		public static ZQuery GetModuleLicenceTypeQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			ZQuery query = new ZQuery();

			if (value != "ANY")
			{
				query.AddToFilter(LicenceModulesSchema.LM_LicenceType, comparisonOperator, value);
			}
			else
			{
				query.AddToFilter(LicenceModulesSchema.LM_LicenceType, SQLComparisonOperator.NotEqual, LicenceTypes.Codes.NON);
			}

			return query;
		}

		#endregion

		#region Text Filters

		public static ZQuery GetOrgPKQuery(ZGuid organisation)
		{
			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(LicenceHeader));
			ZDBOnlySubQuery companySubQuery = new ZDBOnlySubQuery(typeof(LicenceCompany), LicenceHeaderSchema.LA_LC);
			ZDBOnlySubQuery orgSubQuery = new ZDBOnlySubQuery(typeof(OrgHeader), LicenceCompanySchema.LC_OH);
			orgSubQuery.AddToFilter(OrgHeaderSchema.PK, organisation);
			companySubQuery.AddSubQuery(orgSubQuery, JoinCondition.And);
			query.AddSubQuery(companySubQuery, JoinCondition.And);
			return query;
		}

		public static ZQuery GetOrgNameQuery(SQLComparisonOperator @operator, ZString orgName)
		{
			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(LicenceHeader));
			ZDBOnlySubQuery companySubQuery = new ZDBOnlySubQuery(typeof(LicenceCompany), LicenceHeaderSchema.LA_LC);
			ZDBOnlySubQuery orgSubQuery = new ZDBOnlySubQuery(typeof(OrgHeader), LicenceCompanySchema.LC_OH);
			orgSubQuery.AddToFilter(OrgHeaderSchema.OH_FullName, @operator, orgName);
			companySubQuery.AddSubQuery(orgSubQuery, JoinCondition.And);
			query.AddSubQuery(companySubQuery, JoinCondition.And);
			return query;
		}

		[SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults")]
		public static ZQuery GetEnterprisePKQuery(SQLComparisonOperator equalsOrNotEquals, object pkValue)
		{
			var enterprisePK = ZGuid.Empty;
			ZGuid.TryParse(pkValue, out enterprisePK);

			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(LicenceHeader));
			ZDBOnlySubQuery companySubQuery = new ZDBOnlySubQuery(typeof(LicenceCompany), LicenceHeaderSchema.LA_LC);
			ZDBOnlySubQuery enterpriseSubQuery = new ZDBOnlySubQuery(typeof(LicenceEnterprise), LicenceCompanySchema.LC_LE);
			enterpriseSubQuery.AddToFilter(LicenceEnterpriseSchema.PK, equalsOrNotEquals, pkValue);
			companySubQuery.AddSubQuery(enterpriseSubQuery, JoinCondition.And);
			query.AddSubQuery(companySubQuery, JoinCondition.And);
			return query;
		}

		#endregion

		#region CORE Filters

		public void AddCoreFilters(ModuleFilterCollection filters, ModuleFilterSubGroup moduleSubGroup)
		{
			LicenceModuleFeeBasis feeBasis = new LicenceModuleFeeBasis();
			ModuleFilter filter = filters.AddTextFilter("Core Fee Basis", GetCoreFeeBasis, feeBasis.FeeBasisList);
			filter.Category = LicenceFilters.VersionAndLicence;
			filter.SubGroup = moduleSubGroup;
			filter.MaxLength = LicenceModulesSchema.LM_LicenceType.MaxLength;

			ModuleNumberRangeFilter coreSeatCountFilter = new ModuleNumberRangeFilter("Core Seat Count", GetCoreSeatCount);
			coreSeatCountFilter.PropertyType = ZCalcEditPropertyType.Short;
			coreSeatCountFilter.SubGroup = moduleSubGroup;
			coreSeatCountFilter.Category = LicenceFilters.VersionAndLicence;
			filters.AddCustomFilter(coreSeatCountFilter);
		}

		public static ZQuery GetCoreFeeBasis(SQLComparisonOperator sqlOperator, ZString value)
		{
			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(LicenceHeader));

			ZDBOnlySubQuery licModuleSubQuery = new ZDBOnlySubQuery(typeof(LicenceModules), LicenceModulesSchema.LM_LA);
			licModuleSubQuery.AddToFilter(JoinCondition.And, LicenceModulesSchema.LM_GroupModuleCode, SQLComparisonOperator.Equal, BillingConstants.CoreModuleCode);
			licModuleSubQuery.AddToFilter(LicenceModulesSchema.LM_LicenceType, sqlOperator, value);

			query.AddSubQuery(licModuleSubQuery, JoinCondition.And);

			return query;
		}

		public static ZQuery GetCoreSeatCount(INumericZType value1, INumericZType value2)
		{
			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(LicenceHeader));
			ZDBOnlySubQuery licModuleSubQuery = new ZDBOnlySubQuery(typeof(LicenceModules), LicenceModulesSchema.LM_LA);
			ModuleNumberRangeFilter.AddToFilters(licModuleSubQuery, LicenceModulesSchema.LM_UserCount, value1, value2);
			licModuleSubQuery.AddToFilter(JoinCondition.And, LicenceModulesSchema.LM_GroupModuleCode, SQLComparisonOperator.Equal, BillingConstants.CoreModuleCode);
			query.AddSubQuery(licModuleSubQuery, JoinCondition.And);

			return query;
		}

		#endregion

		public void AddStatusFilter(ZQuery query, SchemaBoolColumn column, ZString value)
		{
			var activeFilter = new ZQuery(column, SQLComparisonOperator.Equal, ZBool.True);
			var inactiveFilter = new ZQuery(column, SQLComparisonOperator.Equal, ZBool.False);

			if (statusActive.EqualsUnresolvedOrLocalized(value, ignoreCase: true))
			{
				query.AddToFilter(activeFilter);
			}
			else if (statusInactive.EqualsUnresolvedOrLocalized(value, ignoreCase: true))
			{
				query.AddToFilter(inactiveFilter);
			}
			else
			{
				var statusFilter = new ZQuery();
				statusFilter.AddToFilter(activeFilter, JoinCondition.Or);
				statusFilter.AddToFilter(inactiveFilter, JoinCondition.Or);
				query.AddToFilter(statusFilter);
			}
		}

		#region Licence Module Filters Queries

		public ZQuery GetCurrentReleaseBuildsQuery(ZGuid value1, ZGuid value2)
		{
			return GetReleaseBuildSubQuery(value1, value2, LicenceDatabaseSchema.LD_HL_CurrentRunningVersion);
		}

		public ZQuery GetDeliveredReleaseBuildsQuery(ZGuid value1, ZGuid value2)
		{
			return GetReleaseBuildSubQuery(value1, value2, LicenceDatabaseSchema.LD_HL_CurrentSentVersion);
		}

		ZQuery GetReleaseBuildSubQuery(ZGuid value1, ZGuid value2, SchemaColumn foreignKey)
		{
			ZDBOnlyQuery databaseQuery = new ZDBOnlyQuery(typeof(LicenceDatabase));
			ZDBOnlySubQuery releaseBuildSubQuery = new ZDBOnlySubQuery(typeof(ReleaseBuild), foreignKey);

			if (!value1.IsEmpty)
			{
				ReleaseBuild releaseBuildFrom = FilterStripBusinessObject.Factory.Load<ReleaseBuild>(value1);
				if (releaseBuildFrom != null)
				{
					var parameters = new ZSqlParameterCollection(
						ZSqlParameter.New("@HL_MajorVersion", releaseBuildFrom.HL_MajorVersion, ReleaseBuildSchema.HL_MajorVersion),
						ZSqlParameter.New("@HL_MinorVersion", releaseBuildFrom.HL_MinorVersion, ReleaseBuildSchema.HL_MinorVersion),
						ZSqlParameter.New("@HL_Release", releaseBuildFrom.HL_Release, ReleaseBuildSchema.HL_Release),
						ZSqlParameter.New("@HL_Patch", releaseBuildFrom.HL_Patch, ReleaseBuildSchema.HL_Patch));

					var sql = string.Format(CultureInfo.InvariantCulture, @"
						{0} > @HL_MajorVersion
						OR
						({0} >= @HL_MajorVersion AND {1} > @HL_MinorVersion)
						OR
						({0} >= @HL_MajorVersion AND {1} >= @HL_MinorVersion AND {2} > @HL_Release)
						OR
						({0} >= @HL_MajorVersion AND {1} >= @HL_MinorVersion AND {2} >= @HL_Release AND {3} >= @HL_Patch)",
						ReleaseBuildSchema.HL_MajorVersion.Name,
						ReleaseBuildSchema.HL_MinorVersion.Name,
						ReleaseBuildSchema.HL_Release.Name,
						ReleaseBuildSchema.HL_Patch.Name);

					releaseBuildSubQuery.AddFilterAndZSQLParameterCollection(sql, parameters);
				}
				else
				{
					return ZQuery.NoResultQuery;
				}
			}

			if (!value2.IsEmpty)
			{
				ReleaseBuild releaseBuildTo = FilterStripBusinessObject.Factory.Load<ReleaseBuild>(value2);
				if (releaseBuildTo != null)
				{
					var parameters = new ZSqlParameterCollection(
						ZSqlParameter.New("@HL_MajorVersion", releaseBuildTo.HL_MajorVersion, ReleaseBuildSchema.HL_MajorVersion),
						ZSqlParameter.New("@HL_MinorVersion", releaseBuildTo.HL_MinorVersion, ReleaseBuildSchema.HL_MinorVersion),
						ZSqlParameter.New("@HL_Release", releaseBuildTo.HL_Release, ReleaseBuildSchema.HL_Release),
						ZSqlParameter.New("@HL_Patch", releaseBuildTo.HL_Patch, ReleaseBuildSchema.HL_Patch));

					var sql = string.Format(CultureInfo.InvariantCulture, @"
						{0} < @HL_MajorVersion
						OR
						({0} <= @HL_MajorVersion AND {1} < @HL_MinorVersion)
						OR
						({0} <= @HL_MajorVersion AND {1} <= @HL_MinorVersion AND {2} < @HL_Release)
						OR
						({0} <= @HL_MajorVersion AND {1} <= @HL_MinorVersion AND {2} <= @HL_Release AND {3} <= @HL_Patch)",
						ReleaseBuildSchema.HL_MajorVersion.Name,
						ReleaseBuildSchema.HL_MinorVersion.Name,
						ReleaseBuildSchema.HL_Release.Name,
						ReleaseBuildSchema.HL_Patch.Name);

					releaseBuildSubQuery.AddFilterAndZSQLParameterCollection(sql, parameters);
				}
				else
				{
					return ZQuery.NoResultQuery;
				}
			}

			databaseQuery.AddSubQuery(releaseBuildSubQuery, JoinCondition.And);
			return databaseQuery;
		}

		#endregion

		public void AddLicenceHeaderFilters(ModuleFilterCollection filters, ModuleFilterSubGroup headerSubGroup)
		{
			ModuleFilter filter = filters.AddTextFilter("Licence Edition Type", GetLicenceEditionTypeQuery, new LicenceAdvStdOthList());
			filter.Category = VersionAndLicence;
			filter.SubGroup = headerSubGroup;
			filter.MaxLength = LicenceHeaderSchema.LA_LicenceAdvStdOth.MaxLength;

			filter = filters.AddTextFilter("Licence In Sync", GetLicenceInSyncQuery, LicenceInSyncList);
			filter.Category = VersionAndLicence;
			filter.SubGroup = headerSubGroup;

			filter = filters.AddDateFilter("Go-Live Agreed Date", GetAgreedLiveQuery, true);
			filter.Category = VersionAndLicence;
			filter.SubGroup = headerSubGroup;

			filter = filters.AddDateFilter("Go-Live Completed Date", GetSiteLiveQuery, true);
			filter.Category = VersionAndLicence;
			filter.SubGroup = headerSubGroup;

			ModuleCurrentDateOffsetFilter.AddCurrentDateOffsetFilter(filters, "Go-Live Date Difference", LicenceHeaderSchema.LA_SiteLiveDate, VersionAndLicence, headerSubGroup);

			filter = filters.AddTextFilter("Licence Active on Database", GetLicenceActiveQuery, ActiveStatusList);
			filter.Category = VersionAndLicence;
			filter.SubGroup = headerSubGroup;

			filter = filters.AddDateFilter("Last Discrepancy Change", GetLastDiscrepancyChangeQuery, true);
			filter.Category = VersionAndLicence;
			filter.SubGroup = headerSubGroup;

			filter = filters.AddDateFilter("Support Expiry Date", GetSupportExpiryDateQuery);
			filter.Category = VersionAndLicence;
			filter.SubGroup = headerSubGroup;

			filter = filters.AddFlagsFilter("Reciprocal Rates", new string[] { "Reciprocal Rates" }, new GetFlagsQuery[] { GetIsReciprocalQuery });
			filter.Category = VersionAndLicence;
			filter.SubGroup = headerSubGroup;

			filter = filters.AddFlagsFilter("GST Registered", new string[] { "GST Registered" }, new GetFlagsQuery[] { GetIsGSTRegisteredQuery });
			filter.Category = VersionAndLicence;
			filter.SubGroup = headerSubGroup;

			filter = filters.AddFlagsFilter("GST Cash Enabled", new string[] { "GST Cash Enabled" }, new GetFlagsQuery[] { GetIsGSTCashBasisQuery });
			filter.Category = VersionAndLicence;
			filter.SubGroup = headerSubGroup;

			filter = filters.AddFlagsFilter("Withholding Registered", new string[] { "Withholding Registered" }, new GetFlagsQuery[] { GetIsWHTRegisteredQuery });
			filter.Category = VersionAndLicence;
			filter.SubGroup = headerSubGroup;

			filter = filters.AddFlagsFilter("Withholding Cash Basis", new string[] { "Withholding Cash Basis" }, new GetFlagsQuery[] { GetIsWHTCashBasisQuery });
			filter.Category = VersionAndLicence;
			filter.SubGroup = headerSubGroup;

			filter = filters.AddNkFilter("Currency", GetCurrencyQuery, ModuleIDs.RefCurrency, Currencies);
			filter.Category = VersionAndLicence;
			filter.SubGroup = headerSubGroup;
		}

		public void AddClientCompanyFilters(ModuleFilterCollection filters, ModuleFilterSubGroup subGroup)
		{
			var filter = filters.AddNkFilter("Database Company Country",
				ClientCompanySchema.LCC_RN_NKCountryCode,
				ModuleIDs.RefCountry,
				Countries
				);
			filter.Category = VersionAndLicence;
			filter.SubGroup = subGroup;
		}

		public RefCurrencyCollection Currencies
		{
			get
			{
				return new RefCurrencyCollection(new BusinessObjectFactory() { RefreshEnabled = false });
			}
		}

		public RefCountryCollection Countries
		{
			get
			{
				return new RefCountryCollection(new BusinessObjectFactory() { RefreshEnabled = false });
			}
		}

		public void AddLicenceModuleFilters(ModuleFilterCollection filters, ModuleFilterSubGroup moduleSubGroup)
		{
			ModuleFilter filter = filters.AddTextFilter("Module Fee Basis", GetModuleLicenceTypeQuery, LicenceTypeList);
			filter.Category = VersionAndLicence;
			filter.SubGroup = moduleSubGroup;
			filter.MaxLength = LicenceModulesSchema.LM_LicenceType.MaxLength;

			filter = filters.AddNumberRangeFilter("Module User Count", LicenceModulesSchema.LM_UserCount);
			filter.Category = VersionAndLicence;
			filter.SubGroup = moduleSubGroup;

			filter = filters.AddDateFilter("Module Expiry", LicenceModulesSchema.LM_ExpiryDate);
			filter.Category = VersionAndLicence;
			filter.SubGroup = moduleSubGroup;
		}

		public static void AddPremiumServiceFilters(ModuleFilterCollection filters, ModuleFilterSubGroup subGroup)
		{
			var category = new FilterCategory((NoResString)"Premium Service");

			ModuleFilter filter = filters.AddTextFilter("Premium Service Global Price List", ClientPremiumServiceSchema.CPS_PriceHeaderCode, () => BillingConstants.PriceHeaderType.GetPriceHeaderTypeList());
			filter.Category = category;
			filter.SubGroup = subGroup;
			filter.MaxLength = ClientPremiumServiceSchema.CPS_PriceHeaderCode.MaxLength;

			filter = filters.AddTextFilter("Premium Service Type", ClientPremiumServiceSchema.CPS_Type);
			filter.Category = category;
			filter.SubGroup = subGroup;
			filter.MaxLength = ClientPremiumServiceSchema.CPS_Type.MaxLength;

			filter = filters.AddNumberRangeFilter("Premium Service Units", ClientPremiumServiceSchema.CPS_Units);
			filter.Category = category;
			filter.SubGroup = subGroup;

			filter = filters.AddDateFilter("Premium Service Start Date", ClientPremiumServiceSchema.CPS_StartDate);
			filter.Category = category;
			filter.SubGroup = subGroup;

			filter = filters.AddDateFilter("Premium Service End Date", ClientPremiumServiceSchema.CPS_EndDate);
			filter.Category = category;
			filter.SubGroup = subGroup;

			filter = filters.AddTextFilter("Premium Service Client Ref", ClientPremiumServiceSchema.CPS_ClientRef);
			filter.Category = category;
			filter.SubGroup = subGroup;
			filter.MaxLength = ClientPremiumServiceSchema.CPS_ClientRef.MaxLength;

			filter = filters.AddTextFilter("Premium Service Comment", ClientPremiumServiceSchema.CPS_Comment);
			filter.Category = category;
			filter.SubGroup = subGroup;
			filter.MaxLength = ClientPremiumServiceSchema.CPS_Comment.MaxLength;
		}

		public void AddLicenceDatabaseFilters(ModuleFilterCollection filters, ModuleFilterSubGroup databaseSubGroup)
		{
			ModuleFilter filter = filters.AddTextFilter("Release Ring", LicenceFilters.GetReleaseRingQuery, new ReleaseRingsList());
			filter.Category = VersionAndLicence;
			filter.SubGroup = databaseSubGroup;
			filter.MaxLength = LicenceDatabaseSchema.LD_ReleaseRing.MaxLength;

			filter = filters.AddTextFilter("Licence Email", LicenceFilters.GetLicenceEmailQuery);
			filter.Category = VersionAndLicence;
			filter.SubGroup = databaseSubGroup;
			filter.MaxLength = LicenceDatabaseSchema.LD_PublicEmailAddressForUpdate.MaxLength;

			filter = filters.AddTextFilter("Database Licence Type", LicenceFilters.GetDatabaseLicenceTypeQuery, new DatabaseTypes());
			filter.Category = VersionAndLicence;
			filter.SubGroup = databaseSubGroup;
			filter.MaxLength = LicenceDatabaseSchema.LD_LicenceType.MaxLength;

			filter = filters.AddDateFilter("Last Heartbeat", LicenceFilters.GetLastHeartbeatQuery, true);
			filter.Category = VersionAndLicence;
			filter.SubGroup = databaseSubGroup;

			filter = filters.AddTextFilter("Database Server Code", LicenceFilters.GetDatabaseServerCodeQuery);
			filter.Category = VersionAndLicence;
			filter.SubGroup = databaseSubGroup;
			filter.MaxLength = LicenceDatabaseSchema.LD_ServerCode.MaxLength;

			filter = filters.AddDateFilter("System Shutdown Date", LicenceFilters.GetDatabaseShutdownQuery, true);
			filter.Category = VersionAndLicence;
			filter.SubGroup = databaseSubGroup;

			filter = filters.AddDateFilter("System Shutdown (Manual) Date", LicenceFilters.GetDatabaseManualShutdownQuery, true);
			filter.Category = VersionAndLicence;
			filter.SubGroup = databaseSubGroup;

			filter = filters.AddTextFilter("Database Active", GetDatabaseActiveQuery, ActiveStatusList);
			filter.Category = VersionAndLicence;
			filter.SubGroup = databaseSubGroup;

			filter = filters.AddTextFilter("SQL Server Edition", LicenceFilters.GetSqlEditionQuery, SqlServerVersionDetailsFilterHelper.GetSqlEditionList());
			filter.Category = VersionAndLicence;
			filter.SubGroup = databaseSubGroup;
			filter.MaxLength = LicenceDatabaseSchema.LD_SQLEdition.MaxLength;

			filter = filters.AddTextFilter("SQL Server Version", LicenceFilters.GetSqlVersionQuery, SqlServerVersionDetailsFilterHelper.GetSqlVersionList());
			filter.Category = VersionAndLicence;
			filter.SubGroup = databaseSubGroup;
			filter.MaxLength = LicenceDatabaseSchema.LD_SQLVersion.MaxLength;

			filter = filters.AddTextFilter("Database Hosted Location", LicenceFilters.GetDatabaseHostedLocationQuery, DatabaseHostingLocationList);
			filter.Category = VersionAndLicence;
			filter.SubGroup = databaseSubGroup;

			filter = filters.AddNumberRangeFilter("Database Number", LicenceDatabaseSchema.LD_DatabaseNumber);
			filter.Category = VersionAndLicence;
			filter.SubGroup = databaseSubGroup;

			filter = filters.AddTextFilterForExactComparison("Database ID", LicenceFilters.GetDatabaseIdQuery);
			filter.Category = VersionAndLicence;
			filter.SubGroup = databaseSubGroup;

			filter = filters.AddTextFilter("Product", LicenceDatabaseSchema.LD_Product, new LicenceDatabaseLookups(null).ProductTypeList);
			filter.Category = VersionAndLicence;
			filter.SubGroup = databaseSubGroup;

			var description = Res.GetString("5A5F496E-58FD-45B1-856C-EF8E55BD920C", "Database Staff List Received");
			filter = filters.AddFlagsFilter("Database Staff List Received", new string[] { Res.GetString("F9A83FD0-3182-4022-822E-71DA6445D493", "Database Staff List Received") }, new GetFlagsQuery[] { DatabaseStaffListReceivedQuery });
			filter.MultilingualDescription = ResString.GetMultilingualString("1129FD08-C78D-4101-9761-C1462715E3A0", "Database Staff List Received");
			filter.Category = VersionAndLicence;
			filter.SubGroup = databaseSubGroup;

			#region Release Version

			ModuleGuidsFilter currentVersion = filters.AddGuidFilter("Current Version", Modules.ClientModuleRegistration.ReleaseBuild, GetCurrentReleaseBuildsQuery, new ReleaseBuildCollection(FilterStripBusinessObject.Factory), new ReleaseBuildCollection(FilterStripBusinessObject.Factory));
			currentVersion.Category = VersionAndLicence;
			currentVersion.SetItemDescriptions(Res.GetData("65cb5e83-92f2-4dab-bfc1-1f607cc3dd7a", "From"), Res.GetData("a1fef0bf-da13-4774-964c-a7f9e536c172", "To"));
			currentVersion.SubGroup = databaseSubGroup;

			ModuleGuidsFilter deliveredVersion = filters.AddGuidFilter("Sent Version", Modules.ClientModuleRegistration.ReleaseBuild, GetDeliveredReleaseBuildsQuery, new ReleaseBuildCollection(FilterStripBusinessObject.Factory), new ReleaseBuildCollection(FilterStripBusinessObject.Factory));
			deliveredVersion.Category = VersionAndLicence;
			deliveredVersion.SetItemDescriptions(Res.GetData("7b3dd09d-bbb5-4c05-b07f-130c85fb80a7", "From"), Res.GetData("e7d5ecbf-6968-4077-8356-edea881dd567", "To"));
			deliveredVersion.SubGroup = databaseSubGroup;

			#endregion

			#region Database System Info

			filter = filters.AddTextFilter("Database Server Name", GetDatabaseServerNameQuery);
			filter.Category = databaseSystemInfo;
			filter.SubGroup = databaseSubGroup;
			filter.MaxLength = LicenceDatabaseSchema.LD_ReportedHostServerName.MaxLength;

			filter = filters.AddTextFilter("Database Name", GetDatabaseNameQuery);
			filter.Category = databaseSystemInfo;
			filter.SubGroup = databaseSubGroup;
			filter.MaxLength = LicenceDatabaseSchema.LD_ReportedHostDBName.MaxLength;

			filter = filters.AddTextFilter("Database Connection Server", LicenceFilters.GetDatabaseConnectionServerQuery);
			filter.Category = databaseSystemInfo;
			filter.SubGroup = databaseSubGroup;

			filter = filters.AddTextFilter("Database Registration", LicenceFilters.GetDatabaseRegistrationQuery, new DatabaseStatusList());
			filter.Category = databaseSystemInfo;
			filter.SubGroup = databaseSubGroup;
			filter.MaxLength = LicenceDatabaseSchema.LD_Status.MaxLength;

			filter = filters.AddTextFilter("OS Name (Free Text)", GetOSNameQuery);
			filter.Category = databaseSystemInfo;
			filter.SubGroup = databaseSubGroup;
			filter.MaxLength = LicenceDatabaseSchema.LD_OSName.MaxLength;

			filter = filters.AddTextFilter("OS Name", GetOSNameQuery, OSNames);
			filter.Category = databaseSystemInfo;
			filter.SubGroup = databaseSubGroup;
			filter.MaxLength = LicenceDatabaseSchema.LD_OSName.MaxLength;

			filter = filters.AddTextFilter("OS Version (Free Text)", GetOSVersionQuery);
			filter.Category = databaseSystemInfo;
			filter.SubGroup = databaseSubGroup;
			filter.MaxLength = LicenceDatabaseSchema.LD_OSVersion.MaxLength;

			filter = filters.AddTextFilter("OS Version", GetOSVersionQuery, OSVersions);
			filter.Category = databaseSystemInfo;
			filter.SubGroup = databaseSubGroup;
			filter.MaxLength = LicenceDatabaseSchema.LD_OSVersion.MaxLength;

			filter = filters.AddTextFilter("System Manufacturer (Free Text)", GetSystemManufacturerQuery);
			filter.Category = databaseSystemInfo;
			filter.SubGroup = databaseSubGroup;
			filter.MaxLength = LicenceDatabaseSchema.LD_SystemManufacturer.MaxLength;

			filter = filters.AddTextFilter("System Manufacturer", GetSystemManufacturerQuery, SystemManufacturers);
			filter.Category = databaseSystemInfo;
			filter.SubGroup = databaseSubGroup;
			filter.MaxLength = LicenceDatabaseSchema.LD_SystemManufacturer.MaxLength;

			filter = filters.AddDateFilter("BIOS Release Date", GetBiosDateQuery);
			filter.Category = databaseSystemInfo;
			filter.SubGroup = databaseSubGroup;

			filter = filters.AddNumberRangeFilter("Total Physical Memory in MB", GetPhysicalMemorySizeQuery);
			((ModuleNumberRangeFilter)filter).PropertyType = ZCalcEditPropertyType.Int;
			filter.Category = databaseSystemInfo;
			filter.SubGroup = databaseSubGroup;

			filter = filters.AddNumberRangeFilter("Number of Processors", GetNoOfProcessorCoresQuery);
			((ModuleNumberRangeFilter)filter).PropertyType = ZCalcEditPropertyType.Int;
			filter.Category = databaseSystemInfo;
			filter.SubGroup = databaseSubGroup;

			filter = filters.AddTextFilter("Processor Type (Free Text)", GetProcessorType);
			filter.Category = databaseSystemInfo;
			filter.SubGroup = databaseSubGroup;

			filter = filters.AddTextFilter("Processor Type", GetProcessorType, ProcessorTypes);
			filter.Category = databaseSystemInfo;
			filter.SubGroup = databaseSubGroup;

			filter = filters.AddNumberRangeFilter("Processor Speed in Mhz", GetProcessorSpeedQuery);
			filter.Category = databaseSystemInfo;
			filter.SubGroup = databaseSubGroup;

			filter = filters.AddFlagsFilter("Is Virtual Machine", new string[] { "Is Virtual Machine" }, new GetFlagsQuery[] { GetVirtualMachineDetected });
			filter.Category = databaseSystemInfo;
			filter.SubGroup = databaseSubGroup;

			var databaseBillableFlagList = new DatabaseBillableFlagList();
			databaseBillableFlagList.RemoveCode(DatabaseBillableFlagList.Codes.Null);
			databaseBillableFlagList.AddPair(DatabaseBillableFlagList.Codes.Null, "Unknown/Blank");
			filter = filters.AddTextFilter("Billable", GetDatabaseBillableQuery, databaseBillableFlagList);
			filter.Category = databaseSystemInfo;
			filter.SubGroup = databaseSubGroup;

			filter = filters.AddGuidFilter("Production DB", ClientModuleRegistration.LicenceDatabase, LicenceDatabaseSchema.LD_LD_ParentDatabase, new LicenceDatabaseGlobalCollection(FilterStripBusinessObject.Factory));
			filter.Category = databaseSystemInfo;
			filter.SubGroup = databaseSubGroup;

			filter = filters.AddTextFilter("Tenant ID", GetTenantIDQuery);
			filter.Category = databaseSystemInfo;
			filter.SubGroup = databaseSubGroup;
			filter.MaxLength = LicenceDatabaseSchema.LD_TenantID.MaxLength;

			filter = filters.AddTextFilter("System ID", GetSystemIDQuery);
			filter.Category = databaseSystemInfo;
			filter.SubGroup = databaseSubGroup;
			filter.MaxLength = EdiTrustedSystemSchema.ETS_SystemID.MaxLength;

			filter = filters.AddDateFilter("UPG Next Run", GetUPGNextRunDateQuery);
			filter.Category = databaseSystemInfo;
			filter.SubGroup = databaseSubGroup;

			filter = filters.AddDateFilter("MUG Next Run", GetMUGNextRunDateQuery);
			filter.Category = databaseSystemInfo;
			filter.SubGroup = databaseSubGroup;

			filter = filters.AddTextFilter("Feature Set", GetFeatureSetQuery, new LicenceDatabaseLookups(null, FilterStripBusinessObject.Factory).FeatureSets);
			filter.Category = databaseSystemInfo;
			filter.SubGroup = databaseSubGroup;
			filter.MaxLength = FeatureControlSetSchema.FCS_ProductName.MaxLength;
			#endregion
		}

		public static CodeDescriptionPairList SystemManufacturers => GetDistinctLicenceDatabaseValue(LicenceDatabaseSchema.LD_SystemManufacturer);
		public static CodeDescriptionPairList OSNames => GetDistinctLicenceDatabaseValue(LicenceDatabaseSchema.LD_OSName);
		public static CodeDescriptionPairList OSVersions => GetDistinctLicenceDatabaseValue(LicenceDatabaseSchema.LD_OSVersion);
		public static CodeDescriptionPairList ProcessorTypes => GetDistinctLicenceDatabaseValue(LicenceDatabaseSchema.LD_ProcessorType);

		static CodeDescriptionPairList GetDistinctLicenceDatabaseValue(SchemaStringColumn columnName)
		{
			var factory = new BusinessObjectFactory();
			var collection = new DynamicBusinessObjectCollection(factory);
			collection.Load($"select distinct {columnName.Name} from dbo.licencedatabase");
			CodeDescriptionPairList systemManufacturers = new CodeDescriptionPairList();
			foreach (var text in collection.Select(x => (ZString)x[columnName.Name]))
			{
				systemManufacturers.AddPair(text, text);
			}

			return systemManufacturers;
		}

		#region Database Additional Info Filters Query

		ZQuery GetOSNameQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			return new ZQuery(LicenceDatabaseSchema.LD_OSName, comparisonOperator, value);
		}

		ZQuery GetOSVersionQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			return new ZQuery(LicenceDatabaseSchema.LD_OSVersion, comparisonOperator, value);
		}

		ZQuery GetSystemManufacturerQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			return new ZQuery(LicenceDatabaseSchema.LD_SystemManufacturer, comparisonOperator, value);
		}

		ZQuery GetProcessorType(SQLComparisonOperator comparisonOperator, ZString value)
		{
			return new ZQuery(LicenceDatabaseSchema.LD_ProcessorType, comparisonOperator, value);
		}

		ZQuery GetBiosDateQuery(DateComparisonOperator comparisonOperator, ZDateTime date1, ZDateTime date2)
		{
			ZQuery query = new ZQuery();
			AddDateRange(query, comparisonOperator, JoinCondition.And, LicenceDatabaseSchema.LD_BIOSDate, date1.Date, date2.Date);
			return query;
		}

		ZQuery GetPhysicalMemorySizeQuery(INumericZType value1, INumericZType value2)
		{
			return ModuleNumberRangeFilter.AddToFilters(new ZQuery(), LicenceDatabaseSchema.LD_TotalPhysicalMemoryMB, value1, value2);
		}

		ZQuery GetNoOfProcessorCoresQuery(INumericZType value1, INumericZType value2)
		{
			return ModuleNumberRangeFilter.AddToFilters(new ZQuery(), LicenceDatabaseSchema.LD_NoOfProcessorCores, value1, value2);
		}

		ZQuery GetProcessorSpeedQuery(INumericZType value1, INumericZType value2)
		{
			return ModuleNumberRangeFilter.AddToFilters(new ZQuery(), LicenceDatabaseSchema.LD_ProcessorSpeedMHz, value1, value2);
		}

		ZQuery GetVirtualMachineDetected(ZBool value)
		{
			return new ZQuery(LicenceDatabaseSchema.LD_VirtualMachineDetected, value);
		}

		ZQuery GetDatabaseBillableQuery(ZString value)
		{
			return new ZQuery(LicenceDatabaseSchema.LD_Billable, value);
		}

		ZQuery GetTenantIDQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			return new ZQuery(LicenceDatabaseSchema.LD_TenantID, comparisonOperator, value);
		}

		ZQuery GetSystemIDQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var query = new ZDBOnlyQuery(typeof(LicenceDatabase));
			var subQuery = new ZDBOnlySubQuery(typeof(EdiTrustedSystem), LicenceDatabaseSchema.LD_ETS_TrustedSystem);
			subQuery.AddToFilter(EdiTrustedSystemSchema.ETS_SystemID, comparisonOperator, value);
			query.AddSubQuery(subQuery, JoinCondition.And);
			return query;
		}

		ZQuery GetUPGNextRunDateQuery(DateComparisonOperator comparisonOperator, ZDateTime date1, ZDateTime date2)
		{
			var query = new ZQuery();
			AddDateRange(query, comparisonOperator, JoinCondition.And, LicenceDatabaseSchema.LD_NextRunTimeUtcUPG, date1.Date, date2.Date);
			return query;
		}

		ZQuery GetMUGNextRunDateQuery(DateComparisonOperator comparisonOperator, ZDateTime date1, ZDateTime date2)
		{
			var query = new ZQuery();
			AddDateRange(query, comparisonOperator, JoinCondition.And, LicenceDatabaseSchema.LD_NextRunTimeUtcMUG, date1.Date, date2.Date);
			return query;
		}

		#endregion

		#region GetFeatureSetQuery

		ZQuery GetFeatureSetQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var ldQuery = new ZDBOnlyQuery(typeof(LicenceDatabase));

			var featureSetSubQuery = new ZDBOnlySubQuery(typeof(FeatureControlSet), FeatureControlSetSchema.PK);
			featureSetSubQuery.AddToFilter(FeatureControlSetSchema.FCS_ProductName, comparisonOperator, value);

			ldQuery.AddSubQuery(LicenceDatabaseSchema.LD_FCS_FeatureSet, featureSetSubQuery, JoinCondition.And);
			return ldQuery;
		}

		#endregion
	}
}
