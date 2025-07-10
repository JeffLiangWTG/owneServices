using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.Data;
using Enterprise.ZArchitecture.Schema;
using WTG.StaticAnalysis.Annotation;

[assembly: UsesConstants(typeof(RefZoneHeaderSchema))]
[assembly: UsesConstants(typeof(RefZonePivotSchema))]

namespace Enterprise.DbUpgrader.Data
{
	public class RefWRSZoneUpgradeTask : EmbeddedUpgradeTask
	{
		public RefWRSZoneUpgradeTask() : base(new RefWRSZoneHeaderDataFile())
		{
		}

		internal RefWRSZoneUpgradeTask(EmbeddedDataFile resourceDataFile) : base(resourceDataFile)
		{
		}

		#region Insert, Update and Delete

		protected override void DoInsert(DataRow sourceRow, DataTable targetTable, ref int targetIndex)
		{
			var shouldInsert = true;

			switch (targetTable.TableName)
			{
				case RefZoneHeaderSchema.Constants.TableName:
					if (CantInsertRefZoneHeader(sourceRow))
					{
						nonInsertedZonePks.Add((Guid)sourceRow[RefZoneHeaderSchema.Constants.PK]);
						shouldInsert = false;
					}
					break;

				case RefZonePivotSchema.Constants.TableName:
					if (CantInsertZonePivot(sourceRow))
					{
						shouldInsert = false;
					}
					break;
			}

			if (shouldInsert)
			{
				base.DoInsert(sourceRow, targetTable, ref targetIndex);
			}
		}

		readonly List<Guid> nonInsertedZonePks = new List<Guid>();

		protected override void UpdateColumn(string columnName, DataRow targetRow, DataRow sourceRow)
		{
			var shouldUpdate = CanUpdateOrDelete(targetRow);

			if (shouldUpdate)
			{
				switch (columnName)
				{
					case RefZonePivotSchema.Constants.F2_ParentID:
						//Should not update if UNLOCO or Country doesn't exist on user's system

						shouldUpdate = ReferencedObjectExists(targetRow);
						break;

					case RefZonePivotSchema.Constants.F2_FZ:
						//Should not update if parent RefZoneHeader doesn't exist on user's system

						shouldUpdate = IsRecordInDatabase(RefZoneHeaderSchema.Constants.PK, (Guid)targetRow[RefZonePivotSchema.Constants.F2_FZ], RefZoneHeaderSchema.Constants.TableName);
						break;
				}
			}

			if (shouldUpdate)
			{
				base.UpdateColumn(columnName, targetRow, sourceRow);
			}
		}

		protected override void DoDelete(DataRow targetRow, ref int targetIndex)
		{
			if (CanUpdateOrDelete(targetRow))
			{
				base.DoDelete(targetRow, ref targetIndex);
			}
			else
			{
				targetIndex++;
			}
		}

		bool CanUpdateOrDelete(DataRow targetRow)
		{
			switch (targetRow.Table.TableName)
			{
				case RefZoneHeaderSchema.Constants.TableName:
					return targetRow[RefZoneHeaderSchema.Constants.FZ_ZoneType].ToString() == "WRS";

				case RefZonePivotSchema.Constants.TableName:
					var sqlQuery = $"SELECT TOP 1 {RefZoneHeaderSchema.Constants.PK} FROM {RefZoneHeaderSchema.Constants.SqlSchemaName}.{RefZoneHeaderSchema.Constants.TableName} WHERE {RefZoneHeaderSchema.Constants.PK} = @zoneHeaderPk AND {RefZoneHeaderSchema.Constants.FZ_ZoneType} = 'WRS'";
					using (var command = Db.Connection.Command(sqlQuery))
					{
						command.AddParameter("@zoneHeaderPk", SqlDbType.UniqueIdentifier, (Guid)targetRow[RefZonePivotSchema.Constants.F2_FZ]);
						var objResult = command.ExecuteScalar();
						return objResult != null && objResult != DBNull.Value;
					}

				default:
					return false;
			}
		}

		protected bool CantInsertRefZoneHeader(DataRow sourceRow)
		{
			return IsRecordInDatabase(RefZoneHeaderSchema.Constants.PK, sourceRow[RefZoneHeaderSchema.Constants.PK], RefZoneHeaderSchema.Constants.TableName)
				|| sourceRow[RefZoneHeaderSchema.Constants.FZ_ZoneType].ToString() != "WRS"
				|| sourceRow[RefZoneHeaderSchema.Constants.FZ_OH_RelatedParty] != DBNull.Value;
		}

		protected bool CantInsertZonePivot(DataRow sourceRow)
		{
			return IsRecordInDatabase(RefZonePivotSchema.Constants.PK, sourceRow[RefZonePivotSchema.Constants.PK], RefZonePivotSchema.Constants.TableName)
				|| nonInsertedZonePks.Contains((Guid)sourceRow[RefZonePivotSchema.Constants.F2_FZ])
				|| !ReferencedObjectExists(sourceRow);
		}

		protected bool IsRecordInDatabase(string pkField, object sourcePkValue, string targetTableName)
		{
			var filterExpression = $"SELECT TOP 1 {pkField} FROM {targetTableName} WHERE {pkField} = @sourcePK";
			using (var command = Db.Connection.Command(filterExpression))
			{
				command.AddParameter("@sourcePK", SqlDbType.UniqueIdentifier, (Guid)sourcePkValue);
				var objResult = command.ExecuteScalar();
				return objResult != null && objResult != DBNull.Value;
			}
		}

		protected bool ReferencedObjectExists(DataRow sourceRow)
		{
			var referencedObjectPk = sourceRow[RefZonePivotSchema.Constants.F2_ParentID];
			switch (sourceRow[RefZonePivotSchema.Constants.F2_ParentTableCode].ToString())
			{
				case RefUNLOCOSchema.Constants.Prefix:
					return IsRecordInDatabase(RefUNLOCOSchema.Constants.PK, referencedObjectPk, RefUNLOCOSchema.Constants.TableName);

				case RefCountrySchema.Constants.Prefix:
					return IsRecordInDatabase(RefCountrySchema.Constants.PK, referencedObjectPk, RefCountrySchema.Constants.TableName);
				default:
					return false;
			}
		}

		#endregion
	}
}
