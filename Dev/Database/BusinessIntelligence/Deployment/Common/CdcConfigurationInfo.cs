namespace Enterprise.ChangeDataCapture.Common
{
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.Linq;
	using CargoWise.Bi.Common;
	using CargoWise.Bi.ConfigLoader;
	using CargoWise.Data;
	using CargoWise.Database.Abstractions;
	using CargoWise.Database.Abstractions.Extensions;
	using Microsoft.Extensions.DependencyInjection;
	using WTG.StaticAnalysis.Annotation;

	#region SuppressResourceStringsCheckRegion

	public class CdcConfigurationInfo
	{
		public static IEnumerable<CdcConfigurationInfo> GetEligibleCdcTables(DbConnection connection)
		{
			var isAuditEnabled = !string.IsNullOrEmpty(BiServers.LoadAuditServerUsingCacheIfPossible(connection));
			var isEdwEnabled = !string.IsNullOrEmpty(BiServers.LoadDataWarehouseServerUsingCacheIfPossible(connection));
			var isEdiClient = GlobalServiceProvider.Instance.GetService<IExtensionObjectsSource>()?.ExtensionCode == "EDI";

			return BiAutomationConfigLoader.Instance.ConfigData.CdcTableConfig
				.Where(t => !t.IsEdiClient || isEdiClient)
				.Where(t => (isAuditEnabled && t.TableInAudit) || (isEdwEnabled && t.TableInEdw))
				.Select(t => new CdcConfigurationInfo(t.SourceSchema, t.SourceTable));
		}

		public static bool? SupportsNetChangesFlag
		{
			get
			{
				return supportsNetChangesFlag;
			}
			set
			{
				supportsNetChangesFlag = value;
			}
		}
		[ThreadSafe]
		static bool? supportsNetChangesFlag;

		public CdcConfigurationInfo(string schemaName, string tableName, string captureInstance, string columnName)
		{
			if (schemaName is null)
			{
				throw new ArgumentNullException(nameof(schemaName));
			}
			if (tableName is null)
			{
				throw new ArgumentNullException(nameof(tableName));
			}

			SchemaName = schemaName;
			TableName = tableName;
			CaptureInstance = captureInstance;
			ColumnName = columnName;
		}

		public CdcConfigurationInfo(string schemaName, string tableName, string captureInstance)
			: this(schemaName, tableName, captureInstance, columnName: null)
		{
		}

		public CdcConfigurationInfo(string schemaName, string tableName)
			: this(schemaName, tableName, captureInstance: $"{schemaName}_{tableName}", columnName: null)
		{
		}

		public string SchemaName { get; }
		public string TableName { get; }
		public string CaptureInstance { get; }
		public string ColumnName { get; }

		public override bool Equals(object obj)
		{
			var info = obj as CdcConfigurationInfo;
			if (obj != null)
			{
				return Equals(info);
			}
			return false;
		}

		public override int GetHashCode()
		{
			var combinedObject = String.Format(CultureInfo.InvariantCulture,
				"[{0}].[{1}].[{2}].[{3}]",
				SchemaName ?? "",
				TableName ?? "",
				CaptureInstance ?? "",
				ColumnName ?? "");

			return combinedObject.GetHashCode();
		}

		public static bool operator ==(CdcConfigurationInfo info1, CdcConfigurationInfo info2)
		{
			if (object.ReferenceEquals(info1, null))
			{
				return object.ReferenceEquals(info2, null);
			}

			return info1.Equals(info2);
		}

		public static bool operator !=(CdcConfigurationInfo info1, CdcConfigurationInfo info2) => !(info1 == info2);

		bool Equals(CdcConfigurationInfo other)
		{
			if (other != null)
			{
				return SchemaName == other.SchemaName
					&& TableName == other.TableName
					&& CaptureInstance == other.CaptureInstance
					&& ColumnName == other.ColumnName;
			}
			else
			{
				return false;
			}
		}
	}

	#endregion
}
