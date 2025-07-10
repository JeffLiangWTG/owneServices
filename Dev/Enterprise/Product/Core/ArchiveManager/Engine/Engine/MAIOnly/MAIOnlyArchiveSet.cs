using System;
using System.Data;
using System.Globalization;
using System.Text;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ArchiveManager.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ArchiveManager.Engine
{
	public class MAIOnlyArchiveSet : ArchiveSet
	{
		public MAIOnlyArchiveSet(IArchiveSystemDescriptor systemDescriptor, IArchiveStageDescriptor stageDescriptor, string stageName, IArchiveSchedule schedule, ArchiveableType mainArchiveableType, ZQuery mainArchiveableTypeFilter, int batchSize)
			: base(systemDescriptor, stageName, schedule.SchedulePK, null, mainArchiveableType, mainArchiveableTypeFilter)
		{
			itemBatchSize = batchSize;
			this.schedule = Argument.NotNull(schedule, nameof(schedule));
			this.stageDescriptor = Argument.NotNull(stageDescriptor, nameof(stageDescriptor));
		}

		public override IArchiveStepResult Load(IArchiveLogger logger)
		{
			var stepResult = new ArchiveStepResult();
			var numberOfTimesToTryBeforeGivingUp = 3;
			do
			{
				try
				{
					LoadBatchOfArchiveableItemsQuery = MainArchiveableTypeFilter;
					LoadBatchOfArchiveableItemsQuery.MaximumRows = itemBatchSize;
					var pkColumn = mainArchiveableType.PKColumn;
					var type = BusinessObjectFactory.GetBusinessObjectBaseTypeFromTablePrefix(pkColumn.ColumnPrefix);

					var watermark = schedule.GetWatermark(StageName);
					DateRangeStartWatermark = schedule.GetWatermark(StageName);

					if (watermark != null)
					{
						LoadBatchOfArchiveableItemsQuery.AddToFilter(GetWatermarkFilter(watermark));
					}

					var mainArchiveableItems = Factory.Load(type, LoadBatchOfArchiveableItemsQuery);

					var maxDate = ZDateTime.MinSmallDateTimeValue;
					var maxDatePk = ZGuid.Empty;
					var maxDateNk = ZString.Empty;

					foreach (var item in mainArchiveableItems)
					{
						var pk = item.PK;
						var rawDate = item[stageDescriptor.MainDateFilterColumn.Name];
						var date = rawDate is ZDate d ? (ZDateTime)d : (ZDateTime)rawDate;
						var nk = string.IsNullOrEmpty(stageDescriptor.MainArchiveNKColumn?.Name)
							? item[stageDescriptor.MainArchivePKColumn.Name].ToString()
							: item[stageDescriptor.MainArchiveNKColumn.Name].ToString();

						if ((date > maxDate)
							|| ((date == maxDate) && (nk > maxDateNk))
							|| ((date == maxDate) && (nk == maxDateNk) && (pk.CompareTo(maxDatePk) > 0 )))
						{
							maxDate = date;
							maxDatePk = pk;
							maxDateNk = nk;
						}

						itemDictionary.Add(pk.ToGuid(), new ArchiveItem(pkColumn, pk.ToGuid()) { TableCode = pkColumn.ColumnPrefix });
					}

					if (maxDate > ZDateTime.MinSmallDateTimeValue)
					{
						var newWatermark = new ArchiveWatermark
						{
							WatermarkDate = maxDate,
							WatermarkNK = maxDateNk,
							WatermarkPK = maxDatePk.ToGuid()
						};

						schedule.SetWatermark(StageName, newWatermark);
					}
					else
					{
						schedule.SetWatermark(StageName, null);
					}

					DateRangeEndWatermark = schedule.GetWatermark(StageName);
					break;
				}

				catch (Exception e) when (!e.IsCriticalException())
				{
					if (numberOfTimesToTryBeforeGivingUp-- == 0)
					{
						UpdateWatermarkForSkippedMAIOnlyArchiveSet();
						itemDictionary.Clear();

						var builder = new StringBuilder(500);
						_ = builder.AppendLine(string.Format(CultureInfo.InvariantCulture, (NoResString)"Unable to load ArchiveSet for main record type: {0}", mainArchiveableType.PKColumn.TableName));
						_ = builder.AppendLine((NoResString)"This set will be skipped. Full error below:");
						_ = builder.AppendLine();
						_ = builder.AppendLine(e.ToString());

						var errorMessage = builder.ToString();
						logger.LogAndReportError("ArchiveStepResult.LoadException", SystemDescriptor.Code, errorMessage, e);
						stepResult.ErrorsEncountered.Add(errorMessage);
						break;
					}
				}
			}
			while (true);

			return stepResult;
		}

		protected ZDBOnlyQuery GetWatermarkFilter(IArchiveWatermark watermark)
		{
			var dateCol = stageDescriptor.MainDateFilterColumn;
			var nkCol = stageDescriptor.MainArchiveNKColumn;
			var pkCol = stageDescriptor.MainArchivePKColumn;
			var type = BusinessObjectFactory.GetBusinessObjectBaseTypeFromTablePrefix(pkCol.ColumnPrefix);
			var query = new ZDBOnlyQuery(type);

			query.AddToFilter(dateCol, SQLComparisonOperator.GreaterThan, watermark.WatermarkDate);

			if (nkCol != null)
			{
				var dateEqualsAndNkGreaterThan = new ZDBOnlyQuery(type);
				dateEqualsAndNkGreaterThan.AddToFilter(dateCol, SQLComparisonOperator.Equal, watermark.WatermarkDate);
				dateEqualsAndNkGreaterThan.AddToFilter(nkCol, SQLComparisonOperator.GreaterThan, watermark.WatermarkNK);

				var dateAndNkEqualsAndPkGreaterThan = new ZDBOnlyQuery(type);
				dateAndNkEqualsAndPkGreaterThan.AddToFilter(dateCol, SQLComparisonOperator.Equal, watermark.WatermarkDate);
				dateAndNkEqualsAndPkGreaterThan.AddToFilter(nkCol, SQLComparisonOperator.Equal, watermark.WatermarkNK);
				dateAndNkEqualsAndPkGreaterThan.AddToFilter(pkCol, SQLComparisonOperator.GreaterThan, watermark.WatermarkPK);

				query.AddToFilter(dateEqualsAndNkGreaterThan, JoinCondition.Or);
				query.AddToFilter(dateAndNkEqualsAndPkGreaterThan, JoinCondition.Or);
			}
			else
			{
				var dateEqualsAndPkGreaterThan = new ZDBOnlyQuery(type);
				dateEqualsAndPkGreaterThan.AddToFilter(dateCol, SQLComparisonOperator.Equal, watermark.WatermarkDate);
				dateEqualsAndPkGreaterThan.AddToFilter(pkCol, SQLComparisonOperator.GreaterThan, watermark.WatermarkPK);

				query.AddToFilter(dateEqualsAndPkGreaterThan, JoinCondition.Or);
			}

			return query;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		void UpdateWatermarkForSkippedMAIOnlyArchiveSet()
		{
			var watermarkSelect = @$"SELECT TOP 1 @newWatermarkPK = {stageDescriptor.MainArchivePKColumn.Name}, @newWatermarkDate = {stageDescriptor.MainDateFilterColumn.Name}";
			if (stageDescriptor.MainArchiveNKColumn?.Name != null)
			{
				watermarkSelect += @$", @newWatermarkNK = {stageDescriptor.MainArchiveNKColumn.Name}";
			}

			var newWatermarkQuery = @$"
{watermarkSelect} FROM
	(SELECT TOP {itemBatchSize} *
	FROM {stageDescriptor.MainArchivePKColumn.TableName}
	WHERE {LoadBatchOfArchiveableItemsQuery.LiteralTextSqlFormatted}
	ORDER BY {stageDescriptor.MainDateFilterColumn.Name}) FailedSet
ORDER BY {stageDescriptor.MainDateFilterColumn.Name} DESC";

			using (var cmd = Db.Connection.Command(newWatermarkQuery))
			{
				cmd.AddOutputParameter("@newWatermarkDate", SqlDbType.DateTime, 8, 0, 0, DBNull.Value);
				cmd.AddOutputParameter("@newWatermarkNK", SqlDbType.NVarChar, 125, 0, 0, DBNull.Value);
				cmd.AddOutputParameter("@newWatermarkPK", SqlDbType.UniqueIdentifier, 36, 0, 0, DBNull.Value);

				_ = cmd.ExecuteNonQuery();

				Helpers.SetWatermarkUsingQueryResults(schedule, StageName, cmd);

				DateRangeEndWatermark = schedule.GetWatermark(StageName);
			}
		}

		public IArchiveWatermark DateRangeStartWatermark { get; private set; }
		public IArchiveWatermark DateRangeEndWatermark { get; private set; }
		public BusinessObjectFactory Factory { get; set; } = new BusinessObjectFactory();
		ZQuery LoadBatchOfArchiveableItemsQuery;
		readonly int itemBatchSize;
		readonly IArchiveSchedule schedule;
		readonly IArchiveStageDescriptor stageDescriptor;
	}
}
