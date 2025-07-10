namespace Enterprise.AuditDataServices.Business
{
	using System;
	using System.Collections.Generic;
	using System.Data;
	using System.Globalization;
	using System.Linq;
	using System.Text.RegularExpressions;
	using CargoWise.Data;
	using CargoWise.EntityFramework;
	using CargoWise.Schema;
	using CargoWise.Types;
	using Enterprise.MasterFiles.Business;
	using Enterprise.ZArchitecture.Schema;

	public class AuditChangeCollection : NonPersistentBusinessObjectCollection<AuditChange>
	{
		public AuditChangeCollection(AuditEvent auditEvent) : base(auditEvent?.AuditServerFactory)
		{
			if (auditEvent == null)
			{
				throw new ArgumentNullException(nameof(auditEvent));
			}

			this.auditEvent = auditEvent;
		}

		readonly AuditEvent auditEvent;

		ITableSchema AuditedTable
		{
			get { return auditEvent.AuditedTableSchema; }
		}

		public void Reload()
		{
			using (SuspendListChanged())
			{
				RemoveAndDeleteAll();

				if (auditEvent != null)
				{
					DynamicBusinessObject dynamicBeforeChangeObject = null;
					DynamicBusinessObject dynamicAfterChangeObject = null;

					switch (auditEvent.Operation)
					{
						case (int)Audit.ChangeOperation.Insert:
							dynamicAfterChangeObject = GetAuditRowObject(auditEvent.Operation);
							break;
						case (int)Audit.ChangeOperation.AfterUpdate:
							dynamicBeforeChangeObject = GetAuditRowObject((int)Audit.ChangeOperation.BeforeUpdate);
							dynamicAfterChangeObject = GetAuditRowObject(auditEvent.Operation);
							break;
						case (int)Audit.ChangeOperation.Delete:
							dynamicBeforeChangeObject = GetAuditRowObject(auditEvent.Operation);
							break;
						case (int)Audit.ChangeOperation.Transaction:
							AddTransactionedAuditRowObject();
							break;
					}

					if (auditEvent != (int)Audit.ChangeOperation.Transaction && (dynamicBeforeChangeObject != null || dynamicAfterChangeObject != null))
					{
						AddChangeItems(dynamicBeforeChangeObject, dynamicAfterChangeObject, auditEvent.Operation);
					}
				}
			}
		}

		void AddTransactionedAuditRowObject()
		{
			var queryParams = new ZSqlParameterCollection
			{
				{ "@ParentPk", auditEvent.ParentPk, AuditedTable.PK },
				{ "@ChangePeriod", auditEvent.ChangePeriod, Schema.GenericShortSchemaColumn },
				{ "@Lsn", auditEvent.ChangeLsn, Schema.GenericBinaryColumn }
			};

			var dynamicChangeCollection = new DynamicBusinessObjectCollection(Factory);
			dynamicChangeCollection.Load(TransactionedAuditChangeSql, queryParams);

			var index = 0;
			while (index < dynamicChangeCollection.Count)
			{
				DynamicBusinessObject dynamicBeforeChangeObject = null;
				DynamicBusinessObject dynamicAfterChangeObject = null;
				var operation = (ZInt)dynamicChangeCollection[index]["__$operation"];

				switch (operation)
				{
					case (int)Audit.ChangeOperation.Insert:
						dynamicAfterChangeObject = dynamicChangeCollection[index];
						index++;
						break;
					case (int)Audit.ChangeOperation.BeforeUpdate:
						dynamicBeforeChangeObject = dynamicChangeCollection[index];
						dynamicAfterChangeObject = dynamicChangeCollection[index + 1];
						index += 2;
						break;
					case (int)Audit.ChangeOperation.Delete:
						dynamicBeforeChangeObject = dynamicChangeCollection[index];
						index++;
						break;
				}

				AddChangeItems(dynamicBeforeChangeObject, dynamicAfterChangeObject, operation);
			}
		}

		DynamicBusinessObject GetAuditRowObject(ZInt operation)
		{
			var queryParams = new ZSqlParameterCollection
			{
				{ "@ParentPk", auditEvent.ParentPk, AuditedTable.PK },
				{ "@ChangePeriod", auditEvent.ChangePeriod, Schema.GenericShortSchemaColumn },
				{ "@Lsn", auditEvent.ChangeLsn, Schema.GenericBinaryColumn },
				{ "@Operation", operation, Schema.GenericIntSchemaColumn }
			};

			var dynamicChangeCollection = new DynamicBusinessObjectCollection(Factory);
			dynamicChangeCollection.Load(AuditChangesSql, queryParams);
			return (dynamicChangeCollection.Count > 0) ? dynamicChangeCollection[0] : null;
		}

		void AddChangeItems(DynamicBusinessObject beforeChangeObject, DynamicBusinessObject afterChangeObject, ZInt auditEventOperation)
		{
			const string cdcColumnPrefix = "__$";

			var columns = ((INeedTable)(afterChangeObject ?? beforeChangeObject)).Table.Columns;

			BusinessObject relatedObject = null;
			foreach (DataColumn column in columns)
			{
				var propertyName = column.ColumnName;
				if (!propertyName.StartsWith(cdcColumnPrefix, StringComparison.OrdinalIgnoreCase))
				{
					var dataType = column.DataType;
					ZBlob binaryBytes = null;
					var valueBefore = GetValue(beforeChangeObject, propertyName, dataType);
					var valueAfter = GetValue(afterChangeObject, propertyName, dataType);

					var hasBinaryColumnUpdate = false;

					if (dataType == typeof(byte[]))
					{
						hasBinaryColumnUpdate = ConvertToStringForBinaryColumn(auditEventOperation, beforeChangeObject, afterChangeObject, propertyName, out valueBefore, out valueAfter);

						binaryBytes = afterChangeObject != null && !((ZBlob)afterChangeObject[propertyName]).IsEmpty ? (ZBlob)afterChangeObject[propertyName] : null;
					}

					if (beforeChangeObject == null || afterChangeObject == null || valueAfter != valueBefore || hasBinaryColumnUpdate)
					{
						if (passwordPtyRegex.IsMatch(propertyName))
						{
							ObfuscatePassword(beforeChangeObject, propertyName, ref valueBefore);
							ObfuscatePassword(afterChangeObject, propertyName, ref valueAfter);
						}

						ProcessSensitiveData(propertyName, beforeChangeObject, afterChangeObject, relatedObject, ref valueBefore, ref valueAfter);
						ProcessSpecifiedColumns(propertyName, ref valueBefore, ref valueAfter);

						AddNewAuditChange(dataType, propertyName, valueBefore, valueAfter, binaryBytes);
					}
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Default Column Text")]
		bool ConvertToStringForBinaryColumn(int operation, DynamicBusinessObject beforeChangeObject, DynamicBusinessObject afterChangeObject, string propertyName, out string valueBefore, out string valueAfter)
		{
			const string binaryColumnText = "<changed>";
			var hasBinaryColumnUpdate = false;
			var isAfterUpdate = operation == (int)Audit.ChangeOperation.AfterUpdate;
			if (isAfterUpdate)
			{
				hasBinaryColumnUpdate = CheckIsUpdateForBinaryColumn(beforeChangeObject, afterChangeObject, propertyName);
				valueBefore = hasBinaryColumnUpdate && !((ZBlob)beforeChangeObject[propertyName]).IsEmpty ? binaryColumnText : ZString.Empty;
				valueAfter = hasBinaryColumnUpdate && !((ZBlob)afterChangeObject[propertyName]).IsEmpty ? binaryColumnText : ZString.Empty;
			}
			else
			{
				valueBefore = beforeChangeObject == null || ((ZBlob)beforeChangeObject[propertyName]).IsEmpty ? ZString.Empty : binaryColumnText;
				valueAfter = afterChangeObject == null || ((ZBlob)afterChangeObject[propertyName]).IsEmpty ? ZString.Empty : binaryColumnText;
			}
			return hasBinaryColumnUpdate;
		}

		bool CheckIsUpdateForBinaryColumn(DynamicBusinessObject beforeChangeObject, DynamicBusinessObject afterChangeObject, string propertyName)
		{
			if (beforeChangeObject != null && afterChangeObject != null)
			{
				if (beforeChangeObject[propertyName] != null && !((ZBlob)beforeChangeObject[propertyName]).IsEmpty && beforeChangeObject[propertyName] != afterChangeObject[propertyName])
				{
					return true;
				}
			}
			return false;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Date time format")]
		string GetValue(DynamicBusinessObject obj, string propName, Type propType)
		{
			const string dateFormat = "yyyy-MM-dd HH:mm:ss.fff";

			if (obj == null)
			{
				return string.Empty;
			}

			var propValue = obj[propName];

			if (propType == typeof(Guid))
			{
				return ((ZGuid)propValue).IsEmpty ? string.Empty : propValue.ToString();
			}

			if (propType == typeof(DateTime))
			{
				return ((ZDateTime)propValue).IsEmpty ? string.Empty : ((ZDateTime)propValue).ToString(dateFormat, CultureInfo.InvariantCulture);
			}

			return propValue?.ToString() ?? string.Empty;
		}

		readonly List<(string, string)> DurationColumns = new ()
		{
			(ProcessTasksSchema.Constants.TableName, ProcessTasksSchema.P9_ActualDuration.Name)
		};

		void ProcessSpecifiedColumns(string propertyName, ref string valueBefore, ref string valueAfter)
		{
			if (DurationColumns.Contains((AuditedTable.TableName, propertyName)))
			{
				ConvertDurationToHoursAndMinutes(ref valueBefore);
				ConvertDurationToHoursAndMinutes(ref valueAfter);
			}
		}

		void ConvertDurationToHoursAndMinutes(ref string value)
		{
			if (!string.IsNullOrWhiteSpace(value) && ZDateTime.TryParseIgnoreTimezone(value, CultureInfo.InvariantCulture, out var parsedTime))
			{
				var hours = TaskDurationCalculator.GetHoursFromDuration(parsedTime);
				value = TimeSpan.FromHours(hours).ToHoursAndMinutesString();
			}
		}

		void ProcessSensitiveData(string propertyName, DynamicBusinessObject beforeChangeObject, DynamicBusinessObject afterChangeObject, BusinessObject relatedObject, ref string valueBefore,
			ref string valueAfter)
		{
			var entity = afterChangeObject ?? beforeChangeObject;

			if (AuditedTable.TableName == GlbStaffSchema.Constants.TableName)
			{
				ProcessSensitiveDataForGlbStaff(propertyName, entity, relatedObject, ref valueBefore, ref valueAfter);
			}
		}

		void ProcessSensitiveDataForGlbStaff(string propertyName, DynamicBusinessObject entity, BusinessObject relatedObject, ref string valueBefore, ref string valueAfter)
		{
			if (relatedObject is null)
			{
				relatedObject = Factory.Load<GlbStaff>((ZGuid)entity[AuditedTable.PK.Name]);
			}

			var glbStaff = relatedObject as GlbStaff;

			if (glbStaff != null)
			{
				var methodPrefix = "get_";

				var methodInfo = glbStaff.GetType().GetMethods().FirstOrDefault(m => m.Name.Equals(methodPrefix + propertyName, StringComparison.OrdinalIgnoreCase));

				if (methodInfo != null && methodInfo.ReturnType == typeof(ZString))
				{
					var value = (ZString)methodInfo.Invoke(glbStaff, null);
					if (value == glbStaff.ViewDeniedMessage)
					{
						valueBefore = glbStaff.ViewDeniedMessage;
						valueAfter = glbStaff.ViewDeniedMessage;
					}
				}
			}
		}

		void ObfuscatePassword(DynamicBusinessObject changeObject, string propertyName, ref string propertyValue)
		{
			if (changeObject != null && !string.IsNullOrWhiteSpace(propertyValue))
			{
				var pwdColumn = auditEvent.AuditedTableSchema.GetSchemaColumn(propertyName);

				if (pwdColumn != null && typeof(ZString).IsAssignableFrom(pwdColumn.GetEquivalentZType()) && pwdColumn.HasMaxLength && pwdColumn.MaxLength > 3)
				{
					propertyValue = "***";
				}
			}
		}

		static readonly Regex passwordPtyRegex = new Regex("password", RegexOptions.IgnoreCase | RegexOptions.Compiled);

		void AddNewAuditChange(Type dataType, string propertyName, string valueBefore, string valueAfter, ZBlob binaryBytes)
		{
			var auditChangeDetail = dataType != typeof(byte[]) ? new AuditChange(Factory) : new AuditChange.AuditChangeWithBinaryValue(Factory) { BinaryValue = binaryBytes };
			auditChangeDetail.RealColumnName = propertyName;
			auditChangeDetail.ColumnName = GetHumanReadablePropertyName(propertyName);
			auditChangeDetail.ValueBefore = valueBefore;
			auditChangeDetail.ValueAfter = valueAfter;
			Add(auditChangeDetail);
		}

		ZString GetHumanReadablePropertyName(string propertyName)
		{
			string originalPropertyName = propertyName;
			var match = historicalColumnRegex.Match(propertyName);

			if (match.Success)
			{
				var nameGroup = match.Groups["ORIGINALNAME"];

				if (nameGroup != null && !string.IsNullOrWhiteSpace(nameGroup.Value))
				{
					originalPropertyName = nameGroup.Value;
				}
			}

			string result = DataBoundResourceStrings.GetColumnDescriptiveName(AuditedTable.TableName, originalPropertyName);

			if (result.StartsWith(AuditedTable.TableName, StringComparison.OrdinalIgnoreCase))
			{
				result = ZPropertyInfo.GetFriendlyColumnNameShared(originalPropertyName);
			}

			return result;
		}

		static readonly Regex historicalColumnRegex = new Regex(@"(?<ORIGINALNAME>[^!]*)$", RegexOptions.Compiled);

		string AuditChangesSql =>
			FormattableString.Invariant($@"
				SELECT *
				FROM
					[{Db.AuditDatabaseName}].[{AuditedTable.SqlSchemaName}].[{AuditedTable.TableName}] As AuditTable
				WHERE
					AuditTable.[__$lsn_period] = @ChangePeriod
					AND AuditTable.[{AuditedTable.PK.Name}] = @ParentPk
					AND AuditTable.[__$start_lsn] = @Lsn
					AND AuditTable.[__$operation] = @Operation" // This is an SQL query
			);

		string TransactionedAuditChangeSql =>
			FormattableString.Invariant($@"
				SELECT *
				FROM
					[{Db.AuditDatabaseName}].[{AuditedTable.SqlSchemaName}].[{AuditedTable.TableName}] As AuditTable
				WHERE
					AuditTable.[__$lsn_period] = @ChangePeriod
					AND AuditTable.[{AuditedTable.PK.Name}] = @ParentPk
					AND AuditTable.[__$start_lsn] = @Lsn
				ORDER BY AuditTable.[__$command_id], AuditTable.[__$seqval]" // This is an SQL query
			);

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
			return new AuditChange(Factory);
		}

		#endregion
	}
}
