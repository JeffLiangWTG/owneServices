using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Client.EDI.ReleaseBuilds.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.ReleaseBuilds.Module
{
	public class ReleaseBuildFilterBusinessObject : FilterStripBusinessObject
	{
		public ReleaseBuildFilterBusinessObject()
			: base()
		{
		}

		#region Filter

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			ModuleFilterCollection filters = new ModuleFilterCollection();
			ModuleTextFilter versionNumberFilter = filters.AddTextFilter("Version Number", GetVersionNumberFilter);
			versionNumberFilter.PropertyValidation = ValidateVersionNumber;

			filters.AddTextFilter("Status", GetStatusFilter, Statuses);
			filters.AddTextFilter("Release Ring", ReleaseBuildSchema.HL_ReleaseStatus, ReleaseRings);
			filters.AddTextFilter("Comment", ReleaseBuildSchema.HL_Comment);
			filters.AddTextFilter("Product", ReleaseBuildSchema.HL_Product, ProductTypeList);

			filters.AddDateFilter(DateTypeCodes.ExeDate, ReleaseBuildSchema.HL_ExeVersionDate);
			filters.AddDateFilter(DateTypeCodes.DateImported, GetDateImportedQuery);

			return filters;
		}

		void ValidateVersionNumber(ZPropertyInfo versionNumberInfo)
		{
			ZString versionNumber = (ZString)versionNumberInfo.Value;
			if (!versionNumber.IsEmpty)
			{
				ZString[] versionParts = versionNumber.Trim().TrimEnd('.').Split('.');
				bool validVersionNumber = (versionParts.Length > 0) && (versionParts.Length < 5);
				if (validVersionNumber)
				{
					int dummy;
					for (int i = 0; i < versionParts.Length; i++)
					{
						validVersionNumber = int.TryParse(versionParts[i], out dummy);
						if (!validVersionNumber)
						{
							break;
						}
					}
				}
				if (!validVersionNumber)
				{
					versionNumberInfo.AddError("Please enter a valid Version Number.");
				}
			}
		}

		#region Status Filter

		ZQuery GetStatusFilter(ZString status)
		{
			ZQuery result = new ZQuery();
			if (status == StatusCodes.NonSuperseded)
			{
				result.AddToFilter(ReleaseBuildSchema.HL_Superceded, ZBool.False);
			}
			else if (status == StatusCodes.Superseded)
			{
				result.AddToFilter(ReleaseBuildSchema.HL_Superceded, ZBool.True);
			}
			return result;
		}

		#endregion

		#region Date Filter

		ZQuery GetDateImportedQuery(DateComparisonOperator comparisonOperator, ZDateTime dateTime1, ZDateTime dateTime2)
		{
			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(ReleaseBuild));
			ZDBOnlySubQuery subQuery = new ZDBOnlySubQuery(typeof(StmALog), StmALogSchema.SL_Parent);
			subQuery.AddToFilter(StmALogSchema.SL_Table, SQLComparisonOperator.Equal, ReleaseBuildSchema.Constants.TableName);
			subQuery.AddToFilter(StmALogSchema.SL_SE_NKEvent, SQLComparisonOperator.Equal, Events.AddedARecordToTheSystem.Code);
			AddDateRange(subQuery, comparisonOperator, JoinCondition.And, StmALogSchema.SL_PostedTimeUtc, dateTime1.Date, dateTime2.Date);
			query.AddSubQuery(subQuery, JoinCondition.And);
			return query;
		}

		internal static class DateTypeCodes
		{
			public const string DateImported = "Date Imported";
			public const string ExeDate = "Exe Date";
			public const string ReleaseDate = "Release Date";
		}

		#endregion

		#region Version Number

		ZQuery GetVersionNumberFilter(SQLComparisonOperator @operator, ZString versionNumber)
		{
			ZDBOnlyQuery result = new ZDBOnlyQuery(typeof(ReleaseBuild));

			string columnString = "CAST(" + ReleaseBuildSchema.Constants.HL_MajorVersion + " AS varchar)"
				+ " + '.' + CAST(" + ReleaseBuildSchema.Constants.HL_MinorVersion + " AS varchar)"
				+ " + '.' + CAST(" + ReleaseBuildSchema.Constants.HL_Release + " AS varchar)"
				+ " + '.' + CAST(" + ReleaseBuildSchema.Constants.HL_Patch + " AS varchar)";

			string sqlFilter = string.Format("{0} {1} '{2}'", columnString,
				@operator.ComparisonText(versionNumber),
				@operator.EscapedSqlValue(versionNumber));
			result.AddFilterAndZSQLParameterCollection(sqlFilter, null);

			return result;
		}

		#endregion

		#endregion

		#region Lookups

		public CodeDescriptionPairList ReleaseRings
		{
			get
			{
				if (releaseRings == null)
				{
					releaseRings = new ReleaseRingsList();
				}
				return releaseRings;
			}
		}

		CodeDescriptionPairList releaseRings;

		public CodeDescriptionPairList Statuses
		{
			get
			{
				if (statuses == null)
				{
					statuses = new CodeDescriptionPairList();
					statuses.AddPair(StatusCodes.NonSuperseded);
					statuses.AddPair(StatusCodes.Superseded);
				}
				return statuses;
			}
		}

		public static class StatusCodes
		{
			public const string NonSuperseded = "Non-superseded";
			public const string Superseded = "Superseded";
		}

		CodeDescriptionPairList statuses;

		#region ProductTypeList

		public CodeDescriptionPairList ProductTypeList
		{
			get
			{
				return new ProductTypes(includeCargoWiseOne: true, includeInternal: true);
			}
		}

		#endregion

		#endregion
	}
}
