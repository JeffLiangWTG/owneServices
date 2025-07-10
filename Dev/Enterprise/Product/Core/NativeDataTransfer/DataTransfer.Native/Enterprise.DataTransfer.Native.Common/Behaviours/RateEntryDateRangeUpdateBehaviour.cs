using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DataTransfer.Native.Common.Behaviours
{
	class RateEntryDateRangeUpdateBehaviour : DateRangeUpdateBehaviour
	{
		public RateEntryDateRangeUpdateBehaviour(AncillaryImportServices sessionServices) : base(sessionServices)
		{
		}

		const string RateLinesTableName = RateLinesSchema.Constants.TableName;
		const string RateLinesStartDateColumnName = RateLinesSchema.Constants.TL_RateStartDate;
		const string RateLinesEndDateColumnName = RateLinesSchema.Constants.TL_RateEndDate;
		const string RateLinesToRateEntryColumnName = RateLinesSchema.Constants.TL_TI;
		const string RateEntryStartDateColumnName = RateEntrySchema.Constants.TI_RateStartDate;
		const string RateEntryEndDateColumnName = RateEntrySchema.Constants.TI_RateEndDate;
		const string RateEntryPKColumnName = RateEntrySchema.Constants.PK;

		protected override void UpdateDateRange(BehaviourContext behaviourContext, IEntity entity, IEnumerable<DataRow> rows, ZDateTime entityFrom, ZDateTime entityTo)
		{
			CheckAnyRowDateRangeIsWithinEntityDateRange(behaviourContext, entity, rows, entityFrom, entityTo);

			foreach (var row in rows)
			{
				var rowFrom = GetDate(row[entity.Definition.DateRangeStartField]);
				UpdateStartOrEndDate(behaviourContext, entity, entityFrom, entityTo, row, rowFrom);
				UpdateStartOrEndDateForRateLines(behaviourContext, entity, row);
			}
		}

		void UpdateStartOrEndDateForRateLines(BehaviourContext behaviourContext, IEntity entity, DataRow rowOfRateEntry)
		{
			var rateEntryStartDate = GetDate(rowOfRateEntry[RateEntryStartDateColumnName]);
			var rateEntryEndDate = GetDate(rowOfRateEntry[RateEntryEndDateColumnName]);

			var matchingRateLineRowsInDb = GetRelativeRateLines(rateEntryStartDate, rateEntryEndDate, rowOfRateEntry);
			foreach (var row in matchingRateLineRowsInDb)
			{
				var rowFrom = GetDate(row[RateLinesStartDateColumnName]);
				var rowTo = GetDate(row[RateLinesEndDateColumnName]);
				string propertyToUpdate;
				DateTime newDate;

				if (rowFrom < rateEntryStartDate)
				{
					propertyToUpdate = RateLinesStartDateColumnName;
					newDate = rateEntryStartDate.ToDateTime();
					row[propertyToUpdate] = newDate;
				}
				if (rowTo > rateEntryEndDate)
				{
					propertyToUpdate = RateLinesEndDateColumnName;
					newDate = rateEntryEndDate.ToDateTime();
					row[propertyToUpdate] = newDate;
				}
			}
		}

		DataRow[] GetRelativeRateLines(ZDateTime rateEntryStartDate, ZDateTime rateEntryEndDate, DataRow rowOfRateEntry)
		{
			var tableSchema = EnterpriseSchema.GetTableSchema(RateLinesTableName);
			var rateLinesStartDateSchemaColumn = tableSchema.GetSchemaColumn(RateLinesStartDateColumnName);
			var rateLinesEndDateSchemaColumn = tableSchema.GetSchemaColumn(RateLinesEndDateColumnName);
			var rateLinesToRateEntrySchemaColumn = tableSchema.GetSchemaColumn(RateLinesToRateEntryColumnName);

			var filterQuery = new ZQuery();

			if (!rateEntryStartDate.IsEmpty)
			{
				var query = new ZQuery();
				var startDateQuery = new ZQuery(rateLinesStartDateSchemaColumn, SQLComparisonOperator.LessThanOrEqualToDatePartOnly, rateEntryStartDate);
				var endDateQuery = new ZQuery(rateLinesEndDateSchemaColumn, SQLComparisonOperator.GreaterThanOrEqualToDatePartOnly, rateEntryStartDate);
				endDateQuery.AddToFilter(JoinCondition.Or, rateLinesEndDateSchemaColumn, DBNull.Value);
				query.AddToFilter(startDateQuery, JoinCondition.And);
				query.AddToFilter(endDateQuery, JoinCondition.And);
				filterQuery.AddToFilter(query, JoinCondition.Or);
			}

			if (!rateEntryEndDate.IsEmpty)
			{
				var query = new ZQuery();
				var endDateQuery = new ZQuery(rateLinesEndDateSchemaColumn, SQLComparisonOperator.GreaterThanOrEqualToDatePartOnly, rateEntryEndDate);
				var startDateQuery = new ZQuery(rateLinesStartDateSchemaColumn, SQLComparisonOperator.LessThanOrEqualToDatePartOnly, rateEntryEndDate);
				startDateQuery.AddToFilter(JoinCondition.Or, rateLinesStartDateSchemaColumn, DBNull.Value);
				query.AddToFilter(endDateQuery, JoinCondition.And);
				query.AddToFilter(startDateQuery, JoinCondition.And);
				filterQuery.AddToFilter(query, JoinCondition.Or);
			}

			var rateEntryPKQuery = new ZQuery(rateLinesToRateEntrySchemaColumn, (Guid)rowOfRateEntry[RateEntryPKColumnName]);
			filterQuery.AddToFilter(rateEntryPKQuery);

			return RowRepository.Load(RateLinesTableName, filterQuery);
		}
	}
}
