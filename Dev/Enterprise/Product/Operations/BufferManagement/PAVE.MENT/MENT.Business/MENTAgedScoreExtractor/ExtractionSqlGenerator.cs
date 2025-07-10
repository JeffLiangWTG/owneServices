using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Integration;
using Enterprise.PAVE.MENT.Shared;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.PAVE.MENT.Business
{
	public class ExtractionSqlGenerator
	{
		public ExtractionSqlGenerator(MENTAgedScoreExtraction extraction)
		{
			Argument.NotNull(extraction, nameof(extraction));

			this.extraction = extraction;
		}

		protected readonly MENTAgedScoreExtraction extraction;
		const string instantaneousBaseSql = @"{0}
SELECT
	{1},
	{2},
	CAST ({3} as DECIMAL(18,5)) as 'YValue' 
FROM {4} " + SqlJoinString + @"
	WHERE {5}
"; // SQL for extractor

		const string historicalBaseSql = @"{0}
		SELECT
			{1},
			{2},
			{3} as 'YValue' 
		FROM {4} " + SqlJoinString + @"
		WHERE MAS_MAQ_NKCode = {5}
		AND ({6})
{7}
		"; // SQL for extractor

		const string SqlJoinString = @"
	LEFT JOIN " + BMComponentSchema.Constants.SqlSchemaName + "." + BMComponentSchema.Constants.TableName + @" on " + BMComponentSchema.Constants.PK + @" = " + MENTAgedScoreMetricSchema.Constants.MAS_FC_Component + @"
	LEFT JOIN " + GlbGroupSchema.Constants.SqlSchemaName + "." + GlbGroupSchema.Constants.TableName + @" on " + GlbGroupSchema.Constants.PK + @" = " + MENTAgedScoreMetricSchema.Constants.MAS_GG_ReleaseGroup + @"
	LEFT JOIN " + GlbStaffSchema.Constants.SqlSchemaName + "." + GlbStaffSchema.Constants.TableName + @" on " + GlbStaffSchema.Constants.GS_Code + @" = " + MENTAgedScoreMetricSchema.Constants.MAS_GS_NKStaffCode;

		public const string MAQCodeParameterName = "@MAQCode";
		public const string SectionFilterParameterName = "@SecFilter";

		public QueryTextAndParameters GenerateQueryAndParameters()
		{
			var sqlParams = new ZSqlParameterCollection();
			var seriesColumns = extraction.SeriesColumns.Cast<SQLColumnSpecification>().Where(c => c.Selected).OrderBy(c => c.Sequence).ToArray();
			var categoryColumns = extraction.CategoryColumns.Cast<SQLColumnSpecification>().Where(c => c.Selected).OrderBy(c => c.Sequence).ToArray();

			var series = GenerateSelectString(seriesColumns);
			var categories = GenerateSelectString(categoryColumns);

			var seriesFilterQueryAndParameters = GetSeriesFilterQueryAndParameters();
			sqlParams.AddRange(seriesFilterQueryAndParameters.Parameters);

			var header = FormattableString.Invariant($@"
-- Database: {Db.DatabaseName}
-- Query: {extraction.RelatedQueryCode}"); // SQL Command

			string[] variables;

			if (extraction.IsInstantaneous)
			{
				// Creating Variable array for an Instantaneous Query
				variables = new string[]
				{
					/*0*/ header,
					/*1*/ series,
					/*2*/ categories,
					/*3*/ MENTAgedScoreMetricSchema.Constants.MAS_AgedScoreValue,
					/*4*/ GenerateFromTable(),
					/*5*/ seriesFilterQueryAndParameters.QueryText
				};
			}
			else
			{
				// Creating Variable array for an Historical Query
				sqlParams.Add(MAQCodeParameterName, extraction.RelatedQuery.MAQ_Code, CargoWise.Schema.Schema.GenericStringSchemaColumn);

				variables = new string[]
				{
					/*0*/ header,
					/*1*/ series,
					/*2*/ categories,
					/*3*/ GenerateAggregationString(),
					/*4*/ MENTAgedScoreMetricSchema.Constants.TableName,
					/*5*/ MAQCodeParameterName,
					/*6*/ seriesFilterQueryAndParameters.QueryText,
					/*7*/ GenerateCombinedGroupBy(GenerateGroupByString(categoryColumns), GenerateGroupByString(seriesColumns))
				};
			}

			var queryText = string.Format(CultureInfo.InvariantCulture, extraction.IsInstantaneous ? instantaneousBaseSql : historicalBaseSql, variables);

			return new QueryTextAndParameters(queryText, sqlParams);
		}

		QueryTextAndParameters GetSeriesFilterQueryAndParameters()
		{
			var sqlParameterCollection = new ZSqlParameterCollection();
			var query = RelatedModuleFiltersHelper.GetFilterQuery(extraction.SeriesFilter);

			if (query.IsNoResultQuery)
			{
				return new QueryTextAndParameters("1 = 2", sqlParameterCollection);
			}
			else if (query.IsEmpty)
			{
				return new QueryTextAndParameters("1 = 1", sqlParameterCollection);
			}

			for (int i = 0; i < query.Params.Length; i++)
			{
				var param = query.Params[i];
				var parameterName = BMQueryParameterisationHelper.ReplaceAutoGenParamNamesWithCustomParamNames(param.ParameterName, SectionFilterParameterName);
				sqlParameterCollection.Add(ZSqlParameter.New(parameterName, param.Value, param.SchemaColumn, param.ComparisonOperator, param.ComparisonOptions));
			}

			var builder = new SqlBuilder(SqlBuilder.QueryType.Parameterised);
			foreach (ZSqlParameter parameter in query.Params)
			{
				builder.ReserveParameter(parameter);
			}

			var queryParameterisedText = query.GetDataQuery(builder).ParameterisedQueryText;
			queryParameterisedText = BMQueryParameterisationHelper.ReplaceAutoGenParamNamesWithCustomParamNames(queryParameterisedText, SectionFilterParameterName);

			return new QueryTextAndParameters(queryParameterisedText, sqlParameterCollection);
		}

		string GenerateAggregationString()
		{
			string aggregationString = string.Empty;

			switch (extraction.CollectionColumn)
			{
				case MENTColumns.Codes.Score:
					aggregationString = MENTAgedScoreMetricSchema.Constants.MAS_AgedScoreValue;
					break;
				case MENTColumns.Codes.AttributeValue:
					aggregationString = MENTAgedScoreMetricSchema.Constants.MAS_AttributeValue;
					break;
			}

			switch (extraction.AggregationType)
			{
				case ExtractionTypes.Codes.Count:
					return string.Format(CultureInfo.InvariantCulture, (NoResString)"CAST (COUNT({0}) as Decimal)", aggregationString); // SQL for extractor
				case ExtractionTypes.Codes.Sum:
					return string.Format(CultureInfo.InvariantCulture, "SUM({0})", aggregationString); // SQL for extractor
				case ExtractionTypes.Codes.Average:
					return string.Format(CultureInfo.InvariantCulture, "AVG({0})", aggregationString); // SQL for extractor
				case ExtractionTypes.Codes.Minimum:
					return string.Format(CultureInfo.InvariantCulture, "MIN({0})", aggregationString); // SQL for extractor
				case ExtractionTypes.Codes.Maximum:
					return string.Format(CultureInfo.InvariantCulture, "MAX({0})", aggregationString); // SQL for extractor
				default:
					return string.Empty;
			}
		}

		string GenerateGroupByString(SQLColumnSpecification[] orderedSelectedCategorys)
		{
			var builder = new StringBuilder();

			if (orderedSelectedCategorys.Length != 0)
			{
				builder.Append(orderedSelectedCategorys.Select(FormatColumnWithCastWhenRequired).Aggregate((current, next) => current + ", " + next));
			}

			return builder.ToString();
		}

		string GenerateSelectString(SQLColumnSpecification[] orderedSelectedCategorys)
		{
			var builder = new StringBuilder();
			var count = orderedSelectedCategorys.Length;

			if (count == 0)
			{
				builder.Append("NULL"); // SQL for extractor
				return builder.ToString();
			}
			else
			{
				builder.Append(FormatColumnWithCastWhenRequired(orderedSelectedCategorys.First()));

				if (count > 1)
				{
					foreach (var column in orderedSelectedCategorys.Skip(1))
					{
						builder.Append(", ");
						builder.Append(FormatColumnWithCastWhenRequired(column));
					}
				}

				return builder.ToString();
			}
		}

		string FormatColumnWithCastWhenRequired(SQLColumnSpecification column)
		{
			var formattedColumn = string.Format(CultureInfo.InvariantCulture, column.ColumnFunction.GetFunctionAsStringWithFormat(), column.Column);

			string result = null;
			switch (column.ColumnType)
			{
				case MENTConstants.StringColumn:
					result = string.Format(CultureInfo.InvariantCulture, "{0}", column.RequireCoalesce ? string.Format(CultureInfo.InvariantCulture, (NoResString)"COALESCE({0}, 'NONE')", formattedColumn) : formattedColumn); // SQL for extractor
					break;
				case MENTConstants.DecimalColumn:
					result = string.Format(CultureInfo.InvariantCulture, "{0}", formattedColumn); // SQL for extractor
					break;
				case MENTConstants.DateColumn:
					result = string.Format(CultureInfo.InvariantCulture, "CAST({0} AS DATE)", formattedColumn); // SQL for extractor
					break;
				default:
					break;
			}

			return result;
		}

		string GenerateFromTable()
		{
			return string.Format(CultureInfo.InvariantCulture, FromTableTemplate,
				/*0*/ MENTConstants.AgedScoreValueColumn,
				/*1*/ MENTAgedScoreMetricSchema.Constants.MAS_AgedScoreValue,
				/*2*/ MENTAgedScoreMetricSchema.Constants.MAS_TimeRecordedUtc,
				/*3*/ MENTConstants.ReleaseGroupColumn,
				/*4*/ MENTAgedScoreMetricSchema.Constants.MAS_GG_ReleaseGroup,
				/*5*/ MENTConstants.ComponentColumn,
				/*6*/ MENTAgedScoreMetricSchema.Constants.MAS_FC_Component,
				/*7*/ MENTConstants.AttributeValueColumn,
				/*8*/ MENTAgedScoreMetricSchema.Constants.MAS_AttributeValue,
				/*9*/ MENTConstants.StaffColumn,
				/*10*/ MENTAgedScoreMetricSchema.Constants.MAS_GS_NKStaffCode,
				/*11*/ ((IDataCollectionStrategy)extraction.RelatedQuery.GetQueryable().CollectionStrategy).QueryText);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "SQL for extractor")]
		const string FromTableTemplate = @"
(SELECT 
{0} as {1}, 
GETDATE() as {2}, 
{3} as {4}, 
{5} as {6}, 
{7} as {8}, 
{9} as {10} 
FROM (
{11}
) innerData) data";

		string GenerateCombinedGroupBy(string categoriesGroupBy, string seriesGroupBy)
		{
			var stringBuilder = new StringBuilder();
			var joinedGroupBys = string.Join(",", new Collection<string>
			{
				categoriesGroupBy, seriesGroupBy
			}.Where(s => !string.IsNullOrWhiteSpace(s)));

			if (!string.IsNullOrWhiteSpace(joinedGroupBys))
			{
				stringBuilder.AppendFormat(CultureInfo.InvariantCulture, (NoResString)"GROUP BY {0}", joinedGroupBys); // SQL for extractor
			}

			return stringBuilder.ToString();
		}
	}
}
