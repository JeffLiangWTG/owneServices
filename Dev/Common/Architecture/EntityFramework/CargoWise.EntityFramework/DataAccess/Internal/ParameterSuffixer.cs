using System;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Schema;
using CargoWise.Types;
using WTG.StaticAnalysis.Annotation;
using WTG.Statistics;

namespace CargoWise.EntityFramework.Statistics
{
	public partial class ParameterSuffixer
	{
		public ParameterSuffixer()
		{
			var persister = ObjectFactory.Get<IStatisticsPersister>();
			Cache = new StatisticsCache(persister);
		}

		StatisticsCache Cache
		{
			get;
#if DEBUG
			set;
#endif
		}

		[ThreadSafe]
		public static readonly ParameterSuffixer Instance = new ParameterSuffixer();

		public string GetParameterSuffix(DateTime now, SchemaColumn schemaColumn, IComparable value)
		{
			if (schemaColumn == null)
			{
				throw new ArgumentNullException(nameof(schemaColumn));
			}

#if DEBUG
			if (value is IZType)
			{
				throw new ArgumentException("Must be simple data type matching column; instead was " + value.GetType().FullName,
					nameof(value));
			}
#endif
			var schemaName = schemaColumn.TableSchema.SqlSchemaName;
			var tableName = schemaColumn.SmartParameterizationTable;
			var columnName = schemaColumn.SmartParameterizationColumn;

			if (schemaColumn is SchemaBoolColumn boolColumn && !boolColumn.IsBitField && value is Boolean)
			{
				value = (Boolean)value ? "Y" : "N";
			}
			else if (schemaColumn.ColumnType == SchemaColumnType.String && value is Enum)
			{
				value = value.ToString();
			}

			try
			{
				return Cache.GetBucketIdentifier(schemaName, tableName, columnName, now, value);
			}
			catch (TransactionException)
			{
				return SqlHistogram.Constants.BUCKETFAIL;
			}
			catch (InvalidOperationException)
			// This comes from Microsoft's DbConnectionFactory when there's a timeout getting a connection from the pool
			{
				return SqlHistogram.Constants.BUCKETFAIL;
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				ErrorReporter.ReportDeveloperExceptionOnce("BUCKETFAIL:" + schemaColumn.Name, ex);
				return SqlHistogram.Constants.BUCKETFAIL;
			}
		}
	}
}

#region Test
#if DEBUG

namespace CargoWise.EntityFramework.Statistics
{
	public partial class ParameterSuffixer
	{
		public IDisposable TemporaryUseNewCache_ForTest()
		{
			currentCache = Cache;

			var persister = ObjectFactory.Get<IStatisticsPersister>();
			Cache = new StatisticsCache(persister);

			return new DisposableAction(() =>
			{
				Cache = currentCache;
			});
		}

		StatisticsCache currentCache;
	}
}

#endif
#endregion
