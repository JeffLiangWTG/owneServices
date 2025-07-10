using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Reflection;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;

namespace Enterprise.AuditDataServices.Business
{
	public class AuditEventCollection : NonPersistentBusinessObjectCollection<AuditEvent>
	{
		public AuditEventCollection(BusinessObjectFactory auditServerFactory, AuditMasterTable masterTable) : base(auditServerFactory)
		{
			if (masterTable == null)
			{
				throw new ArgumentNullException(nameof(masterTable));
			}

			this.masterTable = masterTable;
		}

		readonly AuditMasterTable masterTable;

		public void Reload(AuditEntity sourceEntity, ZString changeUserCode, ZDateTime utcTimeFrom, ZDateTime utcTimeTo)
		{
			ValidateTimeFromAndTo(utcTimeFrom, utcTimeTo);

			using (SuspendListChanged())
			{
				RemoveAndDeleteAll();

				if (sourceEntity == null)
				{
					foreach (var entity in masterTable.AuditEntities)
					{
						ReloadTable(entity, changeUserCode, utcTimeFrom, utcTimeTo);
					}
				}
				else
				{
					ReloadTable(sourceEntity, changeUserCode, utcTimeFrom, utcTimeTo);
				}
			}
		}

		/// <summary>
		/// LSN Period calculation assumes date from year 2000 onwards.
		/// This was done to make the period field human readable as YYMM and fit in a 2-byte integer type.
		/// Hence valid time from/to values start from 2000-01-01.
		/// </summary>
		/// <param name="utcTimeFrom">UTC Date-Time From</param>
		/// <param name="utcTimeTo">UTC Date-Time To</param>
		void ValidateTimeFromAndTo(ZDateTime utcTimeFrom, ZDateTime utcTimeTo)
		{
			if (!utcTimeFrom.IsValid || utcTimeFrom.Year < 2000)
			{
				throw new ArgumentException("Invalid time-from value.", nameof(utcTimeFrom));
			}

			if (!utcTimeTo.IsValid || utcTimeTo.Year < 2000)
			{
				throw new ArgumentException("Invalid time-to value.", nameof(utcTimeTo));
			}
		}

		void ReloadTable(AuditEntity entity, ZString changeUserCode, ZDateTime utcTimeFrom, ZDateTime utcTimeTo)
		{
			try
			{
				ReloadTableUnsafe(entity, changeUserCode, utcTimeFrom, utcTimeTo);
			}
			catch (SqlException ex)
			{
				var error = new DbErrorMatch(ex);

				bool isInvalidTable = (error.ExceptionType == DbErrorType.InvalidObjectName && ex.Message.Contains(entity.KeyColumn.TableName));
				bool isInvalidColumn = (error.ExceptionType == DbErrorType.InvalidColumnName && ex.Message.Contains(entity.KeyColumn.Name));

				if (!isInvalidTable && !isInvalidColumn)
				{
					throw;
				}
			}
		}

		void ReloadTableUnsafe(AuditEntity entity, ZString changeUserCode, ZDateTime utcTimeFrom, ZDateTime utcTimeTo)
		{
			string loadSql = GetAuditEventSql(entity);

			var queryParams = new ZSqlParameterCollection();

			if (entity.KeyColumn is SchemaGuidColumn)
			{
				queryParams.Add("@TopLevelKey", masterTable.PkValue, masterTable.TableSchema.PK);
			}
			else if (entity.KeyColumn is SchemaIntColumn)
			{
				queryParams.Add("@TopLevelKey", masterTable.ClusterKeyValue, masterTable.ClusterKeySchemaColumn);
			}
			else
			{
				throw new NotSupportedException("Only Guid and Int key columns are supported.");
			}

			queryParams.Add("@ChangeUserCode", ((changeUserCode.IsValid && !changeUserCode.IsEmpty) ? changeUserCode : DBNull.Value), Schema.GenericStringSchemaColumn);
			queryParams.Add("@UtcFrom", utcTimeFrom, Schema.GenericDateTimeColumn);
			queryParams.Add("@PeriodFrom", Convert.ToInt16(((utcTimeFrom.Year - 2000) * 100) + utcTimeFrom.Month), Schema.GenericShortSchemaColumn);
			queryParams.Add("@UtcTo", utcTimeTo, Schema.GenericDateTimeColumn);
			queryParams.Add("@PeriodTo", Convert.ToInt16(((utcTimeTo.Year - 2000) * 100) + utcTimeTo.Month), Schema.GenericShortSchemaColumn);

			var dynamicEventCollection = new DynamicBusinessObjectCollection(AuditServerFactory);
			dynamicEventCollection.Load(loadSql, queryParams);

			var propertyAndTableInfoDict = GetGuidPropertyAndTableInfoDict(entity.KeyColumn.ColumnPrefix);

			foreach (DynamicBusinessObject dynamicEventObject in dynamicEventCollection)
			{
				AddNewAuditEvent(entity, dynamicEventObject, propertyAndTableInfoDict);
			}
		}

		protected Dictionary<string, TableInfo> GetGuidPropertyAndTableInfoDict(string tablePrefix)
		{
			var propertyAndTableNameDict = new Dictionary<string, TableInfo>();
			var businessObjectBaseType = BusinessObjectFactory.GetBusinessObjectBaseTypeFromTablePrefix(tablePrefix, false);
			if (businessObjectBaseType != null)
			{
				var properties = businessObjectBaseType.GetProperties().Where(p => IsForeignKey(p));
				foreach (var property in properties)
				{
					Type relatedBizObjType = null;
					if (property.GetCustomAttribute(typeof(RelatedBusinessObjectAttribute), true) is RelatedBusinessObjectAttribute relatedAttribute)
					{
						var relatedBizObjProperties = businessObjectBaseType.GetProperties().Where(p => p.Name == relatedAttribute.RelatedBizObjName);
						if (relatedBizObjProperties.Any())
						{
							relatedBizObjType = (relatedBizObjProperties.FirstOrDefault(p => p.DeclaringType == businessObjectBaseType) ?? relatedBizObjProperties.First()).PropertyType;
						}
					}

					if (relatedBizObjType != null)
					{
						var tableSchema = BusinessObjectFactory.GetTableSchemaFromType(relatedBizObjType, false);
						if (tableSchema != null)
						{
							try
							{
								var codeProperty = CodePropertyAttribute.CodePropertyNameFromType(relatedBizObjType);
								if (!string.IsNullOrEmpty(codeProperty))
								{
									propertyAndTableNameDict[property.Name] = new TableInfo(tableSchema.TableName,
										tableSchema.PK.ColumnPrefix,
										tableSchema.PK,
										tableSchema.SqlSchemaName,
										codeProperty,
										relatedBizObjType,
										new HashSet<ZGuid>());
								}
							}
							catch (NoCodePropertyException)
							{
							}
						}
					}
				}
			}

			return propertyAndTableNameDict;
		}

		protected Boolean IsForeignKey(PropertyInfo p)
		{
			return p.PropertyType == typeof(ZGuid) &&
				(p.GetCustomAttribute(typeof(ListAttribute), true) != null || ZRowRelationshipManager.GetForeignKeyTablePrefix(p.Name) != null);
		}

		AuditEvent AddNewAuditEvent(AuditEntity entity, DynamicBusinessObject dynamicEventObject, Dictionary<string, TableInfo> propertyAndTableInfoDict)
		{
			var auditEvent = new AuditEvent(OltpServerFactory, AuditServerFactory, entity, MainDBFactory);
			auditEvent.ParentPk = (ZGuid)dynamicEventObject["ParentPk"];
			auditEvent.ChangePeriod = (ZShort)dynamicEventObject["LsnPeriod"];
			auditEvent.ChangeLsn = (ZBlob)dynamicEventObject["StartLsn"];
			auditEvent.Operation = (ZInt)dynamicEventObject["Operation"];
			auditEvent.TimeUtc = (ZDateTime)dynamicEventObject["ChangeUtc"];
			auditEvent.UserCode = (ZString)dynamicEventObject["UserCode"];
			auditEvent.RowInfo = (ZString)dynamicEventObject["RowInfo"];
			auditEvent.PropertyAndTableInfoDict = propertyAndTableInfoDict;
			this.Add(auditEvent);
			return auditEvent;
		}

		BusinessObjectFactory OltpServerFactory => masterTable.Factory;

		BusinessObjectFactory AuditServerFactory => Factory;

		BusinessObjectFactory MainDBFactory => mainDBFactory ??= new BusinessObjectFactory(Db.DatabaseName);
		BusinessObjectFactory mainDBFactory;

		/// <summary>
		/// Query to load audit events.
		/// It uses NOLOCK on LsnTimeMaping to avoid blocking Audit ETL.
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "This is an SQL query, This is an SQL expression")]
		string GetAuditEventSql(AuditEntity entity)
		{
			string tablePrefix = ObjectFactory.Get<IApplicationSchemaResolver>().GetColumnNamePrefix(entity.KeyColumn.TableName);
			var lastEditUserColumn = entity.KeyColumn.TableSchema.GetSchemaColumn(tablePrefix + "_SystemLastEditUser");
			var sql = string.Format(CultureInfo.InvariantCulture, @"
				SELECT 
					LsnPeriod = AuditTable.[__$lsn_period],
					StartLsn = AuditTable.[__$start_lsn],
					Operation = AuditTable.[__$operation],
					ParentPk = AuditTable.[{3}],
					UserCode = {5},
					RowInfo = {7},
					ChangeUtc = LsnMapping.TranEndTimeUtc
				FROM
					[{0}].[{1}].[{2}] AS AuditTable WITH (NOLOCK)
					INNER JOIN [{0}].biadmin.LsnTimeMapping LsnMapping WITH (READPAST, READCOMMITTEDLOCK) ON LsnMapping.StartLsn = AuditTable.[__$start_lsn]
				WHERE
					AuditTable.[__$lsn_period] >= @PeriodFrom
					AND AuditTable.[__$lsn_period] <= @PeriodTo
					AND AuditTable.[{4}] = @TopLevelKey
					AND (@ChangeUserCode is null OR {6} = @ChangeUserCode)
					AND LsnMapping.TranEndTimeUtc >= @UtcFrom
					AND LsnMapping.TranEndTimeUtc <= @UtcTo
					AND AuditTable.[__$operation] in (1,2,4)
				",
				/*0*/Db.AuditDatabaseName,
				/*1*/entity.KeyColumn.TableSchema.SqlSchemaName,
				/*2*/entity.KeyColumn.TableName,
				/*3*/entity.KeyColumn.TableSchema.PK.Name,
				/*4*/entity.KeyColumn.Name,
				/*5*/((lastEditUserColumn == null) ? "''" : "AuditTable.[" + lastEditUserColumn.Name + "]"),
				/*6*/((lastEditUserColumn == null) ? "@ChangeUserCode" : "AuditTable.[" + lastEditUserColumn.Name + "]"),
				/*7*/((entity.InfoColumn == null) ? "''" : "left(AuditTable.[" + entity.InfoColumn.Name + "], 25)")
			);

			return sql;
		}

		#region Implementation

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		protected override bool AllowRemoveCore
		{
			get { return false; }
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new AuditEvent(OltpServerFactory, AuditServerFactory, new AuditEntity(Schema.GenericGuidSchemaColumn, Schema.GenericStringSchemaColumn));
		}

		#endregion
	}
}
