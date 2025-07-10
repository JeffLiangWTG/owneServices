using System;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.DataTransfer.Native.Common.Logging;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DataTransfer.Native.Common.Behaviours
{
	internal class OrgRateTariffLevelDateRangeUpdateBehaviour : DateRangeUpdateBehaviour
	{
		public OrgRateTariffLevelDateRangeUpdateBehaviour(AncillaryImportServices sessionServices) : base(sessionServices)
		{
		}

		protected override void UpdateStartOrEndDate(BehaviourContext behaviourContext, IEntity entity, ZDateTime entityFrom, ZDateTime entityTo, DataRow row, ZDateTime rowFrom)
		{
			string propertyToUpdate;
			DateTime newDate;

			if (!entityFrom.IsEmpty && (rowFrom.IsEmpty || rowFrom < entityFrom))
			{
				propertyToUpdate = entity.Definition.DateRangeEndField;
				newDate = entityFrom.AddDays(-1).ToDateTime();
			}
			else
			{
				propertyToUpdate = entity.Definition.DateRangeStartField;
				newDate = entityTo.AddDays(1).ToDateTime();
			}

			var originalDate = row[propertyToUpdate] == DBNull.Value ? DateTime.MinValue : (DateTime)row[propertyToUpdate];
			row[propertyToUpdate] = newDate;
			propertyToUpdate = propertyToUpdate.Remove(0, entity.Definition.TablePrefix.Length + 1);

			sessionServices.Logger.Log(LogType.Information,
				LogHelper.GetExpirationInfoLogMessage(behaviourContext, entity, propertyToUpdate, originalDate, newDate));
		}

		protected override void ThrowIfDateRangeConflict(BehaviourContext behaviourContext, IEntity entity, ZDateTime entityFrom, ZDateTime entityTo, ZDateTime rowFrom, ZDateTime rowTo)
		{
			if ((entityFrom.IsEmpty || (!rowFrom.IsEmpty && rowFrom >= entityFrom)) && (entityTo.IsEmpty || rowTo <= entityTo))
			{
				string formattedMessage = LogHelper.GetDateRangeConflictLogMessage(behaviourContext, entity, rowFrom, rowTo, entityFrom, entityTo);

				throw new NativeXMLUserVisibleException(formattedMessage);
			}
		}

		protected override void ThrowIfMissingStartDate()
		{
			return;
		}

		protected override ZDateTime GetDate(IEntity entity, string fieldName)
		{
			var field = entity.Properties.FirstOrDefault(x => x.Definition.ColumnDef.Name == fieldName);
			if (field != null)
			{
				return GetDate(field.Value).Date;
			}

			var tableSchema = EnterpriseSchema.GetTableSchema(entity.Definition.TableName);
			var pkColumn = tableSchema.GetSchemaColumn(OrgRateTariffLevelSchema.Constants.PK);
			var filterQuery = new ZQuery(pkColumn, entity.InternalPK);
			var matchingRowsInDb = RowRepository.Load(entity.Definition.TableName, filterQuery);
			var row = matchingRowsInDb.FirstOrDefault();

			return row != null ? (ZDateTime)row[fieldName] : ZDateTime.Empty;
		}

		protected override ZQuery GetDateRangeFilter(SchemaColumn startDateSchemaColumn, SchemaColumn endDateSchemaColumn, ZDateTime startDate, ZDateTime endDate)
		{
			var filterQuery = new ZQuery();
			if (!startDate.IsEmpty)
			{
				var startDateQuery = new ZQuery(endDateSchemaColumn, SQLComparisonOperator.GreaterThanOrEqualToDatePartOnly, startDate);
				startDateQuery.AddToFilter(JoinCondition.Or, endDateSchemaColumn, DBNull.Value);
				filterQuery.AddToFilter(startDateQuery);
			}

			if (!endDate.IsEmpty)
			{
				var endDateQuery = new ZQuery(startDateSchemaColumn, SQLComparisonOperator.LessThanOrEqualToDatePartOnly, endDate);
				endDateQuery.AddToFilter(JoinCondition.Or, startDateSchemaColumn, DBNull.Value);
				filterQuery.AddToFilter(endDateQuery);
			}
			return filterQuery;
		}
	}
}
