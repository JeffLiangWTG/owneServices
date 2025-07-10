using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.Data;

namespace Enterprise.AuditDataServices.Business
{
	using System;
	using CargoWise.EntityFramework;
	using CargoWise.Schema;
	using CargoWise.Types;
	using Enterprise.MasterFiles.Business;
	using Enterprise.ZArchitecture.Schema;

	public class AuditEvent : NonPersistentBusinessObject, IObsoleteValidation
	{
		public static class Schema
		{
			public const string TimeUtc = "TimeUtc";
		}

		public AuditEvent(BusinessObjectFactory factory, BusinessObjectFactory auditServerFactory, AuditEntity auditedEntity, BusinessObjectFactory mainDBFactory = null) : base(factory)
		{
			if (auditServerFactory == null)
			{
				throw new ArgumentNullException(nameof(auditServerFactory));
			}

			if (auditedEntity == null)
			{
				throw new ArgumentNullException(nameof(auditedEntity));
			}

			this.auditServerFactory = auditServerFactory;
			this.auditedEntity = auditedEntity;
			this.mainBusinessObjectFactory = mainDBFactory;
		}

		readonly BusinessObjectFactory auditServerFactory;
		readonly AuditEntity auditedEntity;
		readonly BusinessObjectFactory mainBusinessObjectFactory;

		public Dictionary<string, TableInfo> PropertyAndTableInfoDict { get; set; }

		internal BusinessObjectFactory AuditServerFactory
		{
			get { return auditServerFactory; }
		}

		internal ITableSchema AuditedTableSchema
		{
			get { return auditedEntity.KeyColumn.TableSchema; }
		}

		public AuditChangeCollection ChangeCollection
		{
			get
			{
				if (changeCollection == null)
				{
					var auxCollection = new AuditChangeCollection(this);
					auxCollection.Reload();
					ReplaceGuidWithCode(auxCollection);
					changeCollection = auxCollection;
				}

				return changeCollection;
			}
		}
		AuditChangeCollection changeCollection;

		void ReplaceGuidWithCode(AuditChangeCollection auditChanges)
		{
			if (PropertyAndTableInfoDict == null || mainBusinessObjectFactory == null)
			{
				return;
			}

			var queryParameters = CombineQueryParametersGroupByTableName(auditChanges);
			var codeAuditDataCollection = GetCodeAuditDataForGuidListFromAuditDatabase(queryParameters);
			var codeCollectionFromMainDB = GetCodeDataForGuidListFromMainDB(queryParameters);

			foreach (AuditChange auditChange in auditChanges)
			{
				ReplaceGuidWithCodeForAuditChangeFromAuditDB(codeAuditDataCollection, auditChange);
				ReplaceGuidWithCodeForAuditChange(codeCollectionFromMainDB, auditChange);
			}
		}

		List<TableInfo> CombineQueryParametersGroupByTableName(AuditChangeCollection auditChanges)
		{
			var tableInfosForQuery = new List<TableInfo>();

			foreach (var auditChange in auditChanges.Cast<AuditChange>())
			{
				if (PropertyAndTableInfoDict.TryGetValue(auditChange.RealColumnName, out var tableInfo))
				{
					var tableInfoForQuery = tableInfosForQuery.FirstOrDefault(n => n.TableName == tableInfo.TableName);
					if (tableInfoForQuery == null)
					{
						tableInfoForQuery = (TableInfo)tableInfo.Clone();
						tableInfosForQuery.Add(tableInfoForQuery);
					}

					if (tableInfoForQuery.UtcTimeTo.IsEmpty || TimeUtc > tableInfoForQuery.UtcTimeTo)
					{
						tableInfoForQuery.UtcTimeTo = TimeUtc;
					}

					if (ZGuid.TryParse(auditChange.ValueBefore, out var beforeZGuid))
					{
						tableInfoForQuery.Guids.Add(beforeZGuid);
					}

					if (ZGuid.TryParse(auditChange.ValueAfter, out var afterGuid))
					{
						tableInfoForQuery.Guids.Add(afterGuid);
					}
				}
			}

			return tableInfosForQuery;
		}

		Dictionary<ZGuid, string> GetCodeDataForGuidListFromMainDB(List<TableInfo> queryParameters)
		{
			var codeCollectionFromMainDB = new Dictionary<ZGuid, string>();
			foreach (var queryParameter in queryParameters)
			{
				if (queryParameter.Guids.Count > 0)
				{
					var zQuery = new ZQuery();
					zQuery.AddToFilter(queryParameter.PkSchemaColumn, queryParameter.Guids);

					var businessObjects = mainBusinessObjectFactory.Load(queryParameter.RelatedBizObjType, zQuery);
					foreach (var businessObject in businessObjects)
					{
						codeCollectionFromMainDB[businessObject.PK] = (ZString)businessObject[queryParameter.CodeProperty];
					}
				}
			}

			return codeCollectionFromMainDB;
		}

		List<CodeAuditData> GetCodeAuditDataForGuidListFromAuditDatabase(List<TableInfo> queryParameters)
		{
			var codeAuditDataCollection = new List<CodeAuditData>();
			foreach (var parameter in queryParameters)
			{
				if (parameter.Guids.Count == 0 || !TableAndColumnExists(parameter))
				{
					continue;
				}

				var sql = GetCodesForGuidListSql(parameter);
				var dynamicEventCollection = new DynamicBusinessObjectCollection(AuditServerFactory);
				var queryParams = new ZSqlParameterCollection();
				queryParams.Add("@PksString", string.Join(",", parameter.Guids), CargoWise.Schema.Schema.GenericStringSchemaColumn);
				queryParams.Add("@UtcTo", parameter.UtcTimeTo, CargoWise.Schema.Schema.GenericDateTimeColumn);
				queryParams.Add("@PeriodTo", Convert.ToInt16(((parameter.UtcTimeTo.Year - 2000) * 100) + parameter.UtcTimeTo.Month), CargoWise.Schema.Schema.GenericShortSchemaColumn);
				dynamicEventCollection.Load(sql, queryParams);

				foreach (DynamicBusinessObject dynamicEventObject in dynamicEventCollection)
				{
					var codeAuditData = new CodeAuditData();
					codeAuditData.Code = (ZString)dynamicEventObject["Code"];
					codeAuditData.Pk = (ZGuid)dynamicEventObject["Pk"];
					codeAuditData.ChangeUtc = (ZDateTime)dynamicEventObject["ChangeUtc"];
					codeAuditDataCollection.Add(codeAuditData);
				}
			}

			codeAuditDataCollection.Sort((x1, x2) => x2.ChangeUtc.CompareTo(x1.ChangeUtc));

			return codeAuditDataCollection;
		}

		void ReplaceGuidWithCodeForAuditChangeFromAuditDB(List<CodeAuditData> codeAuditDataCollection, AuditChange auditChange)
		{
			var codeAuditDataBefore = ZGuid.IsGuid(auditChange.ValueBefore) ? codeAuditDataCollection.FirstOrDefault(n => n.Pk == ZGuid.ParseSafe(auditChange.ValueBefore) && n.ChangeUtc < TimeUtc) : null;
			var codeAuditDataAfter = ZGuid.IsGuid(auditChange.ValueAfter) ? codeAuditDataCollection.FirstOrDefault(n => n.Pk == ZGuid.ParseSafe(auditChange.ValueAfter) && n.ChangeUtc < TimeUtc) : null;
			auditChange.ValueBefore = codeAuditDataBefore?.Code ?? auditChange.ValueBefore;
			auditChange.ValueAfter = codeAuditDataAfter?.Code ?? auditChange.ValueAfter;
		}

		void ReplaceGuidWithCodeForAuditChange(Dictionary<ZGuid, string> codeCollectionFromMainDb, AuditChange auditChange)
		{
			var valueBeforeGuid = ZGuid.ParseSafe(auditChange.ValueBefore);
			var valueAfterGuid = ZGuid.ParseSafe(auditChange.ValueAfter);
			auditChange.ValueBefore = codeCollectionFromMainDb.TryGetValue(valueBeforeGuid, out var beforeCode) ? beforeCode : auditChange.ValueBefore;
			auditChange.ValueAfter = codeCollectionFromMainDb.TryGetValue(valueAfterGuid, out var afterCode) ? afterCode : auditChange.ValueAfter;
		}

		class CodeAuditData
		{
			public ZGuid Pk { get; set; }
			public ZString Code { get; set; }
			public ZDateTime ChangeUtc { get; set; }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		protected bool TableAndColumnExists(TableInfo parameter)
		{
			var existsSql = $@"
        SELECT
            CASE WHEN EXISTS (
                SELECT 1
                FROM [{Db.AuditDatabaseName}].INFORMATION_SCHEMA.TABLES
                WHERE TABLE_NAME = @TableName
            )
            AND EXISTS (
                SELECT 1
                FROM [{Db.AuditDatabaseName}].INFORMATION_SCHEMA.COLUMNS
                WHERE TABLE_NAME = @TableName
                AND COLUMN_NAME = @CodeProperty
            )
            THEN 1
            ELSE 0
            END AS ExistsFlag";

			using (var cmd = ((IDbConnected)AuditServerFactory).Connection.Command(existsSql))
			{
				cmd.AddParameter("@TableName", SqlDbType.VarChar, parameter.TableName);
				cmd.AddParameter("@CodeProperty", SqlDbType.VarChar, parameter.CodeProperty);
				var existsFlag = (int)cmd.ExecuteScalar();
				return existsFlag == 1;
			}
		}

		string GetCodesForGuidListSql(TableInfo parameter)
		{
			return string.Format(CultureInfo.InvariantCulture, @"
			DECLARE @Pks varchar(max) = @PksString
				SELECT DISTINCT
					LsnPeriod = AuditTable.[__$lsn_period],
					StartLsn = AuditTable.[__$start_lsn],
					SeqVal = AuditTable.[__$seqval],
					Operation = AuditTable.[__$operation],
					Pk = AuditTable.[{3}],
					Code = AuditTable.[{4}],
					ChangeUtc = LsnMapping.TranEndTimeUtc
				FROM
					[{0}].[{1}].[{2}] AS AuditTable WITH (NOLOCK)
					INNER JOIN [{0}].biadmin.LsnTimeMapping LsnMapping WITH (READPAST, READCOMMITTEDLOCK) ON LsnMapping.StartLsn = AuditTable.[__$start_lsn]
				WHERE
					AuditTable.[__$lsn_period] <= @PeriodTo
					AND AuditTable.[{3}] IN (SELECT value FROM string_split(@Pks, ','))
					AND LsnMapping.TranEndTimeUtc <= @UtcTo
					AND AuditTable.[__$operation] in (1,2,4)
				", // This is an SQL query
				/*0*/Db.AuditDatabaseName,
				/*1*/parameter.SqlSchemaName,
				/*2*/parameter.TableName,
				/*3*/parameter.PkSchemaColumn.Name,
				/*4*/parameter.CodeProperty
			);
		}

		#region Properties

		public ZGuid ParentPk
		{ get; set; }

		public ZShort ChangePeriod
		{ get; set; }

		public ZBlob ChangeLsn
		{ get; set; }

		public ZInt Operation
		{ get; set; }

		public ZString OperationSymbol
		{
			get
			{
				switch (Operation)
				{
					case (int)Audit.ChangeOperation.Insert:
						return "+";
					case (int)Audit.ChangeOperation.Delete:
						return "-";
					default:
						return ZString.Empty;
				}
			}
		}

		public ZString UserCode
		{ get; set; }

		public ZString UserName
		{
			get
			{
				if (UserCode.IsEmpty)
				{
					return ZString.Empty;
				}
				else
				{
					var staff = Factory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_Code, UserCode);
					return ((staff == null) ? "-" : staff.GS_FullName.ToString()) + " (" + UserCode + ")";
				}
			}
		}

		public ZDateTime TimeUtc
		{ get; set; }

		public ZDateTime TimeLocal
		{
			get { return TimeUtc.ToLocalBranchTime(Factory); }
		}

		public ZString SourceName
		{
			get
			{
				return auditedEntity.ToString();
			}
		}

		public ZString RowInfo
		{ get; set; }

		public override string TablePrefix => auditedEntity.KeyColumn.ColumnPrefix;

		#endregion
	}
}
