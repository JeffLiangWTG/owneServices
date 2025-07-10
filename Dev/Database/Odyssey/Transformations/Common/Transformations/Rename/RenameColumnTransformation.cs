using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformation.DataModification;
using static Enterprise.DbUpgrader.Shared.DbObjectCreator;

namespace Enterprise.DbUpgrader.Transformation.Common
{
	public abstract class RenameColumnTransformation : DataTransformation
	{
		protected sealed override void OfflinePreUpgradeTransform()
		{
			foreach (var renameColInfo in RenameColumnInfoList)
			{
				RenameColumn(renameColInfo.SchemaName, renameColInfo.TableName, renameColInfo.OldColumnName, renameColInfo.NewColumnName);
			}

			PostRenameTransformation();
		}

		public override sealed string UserDescription
		{
			get
			{
				var result = new StringBuilder();
				var infoList = RenameColumnInfoList.ToArray();

				if (infoList.Length > 1)
				{
					result.Append("Renaming columns: ");
				}
				else
				{
					result.Append("Renaming column ");
				}

				for (var i = 0; i < infoList.Length; i++)
				{
					var info = infoList[i];
					result.AppendFormat("{0}.{1} -> {2}", info.TableName, info.OldColumnName, info.NewColumnName);

					if (i < infoList.Length - 1)
					{
						result.Append("; ");
					}
				}

				return result.ToString();
			}
		}

		public abstract IEnumerable<IRenameColumnTransformationInfo> RenameColumnInfoList { get; }

		protected void RenameColumn(string schemaName, string tableName, string oldColumnName, string newColumnName)
		{
			if (ColumnExists(Db.Connection, new TableDescriptor(schemaName, tableName), oldColumnName))
			{
				if (!oldColumnName.Equals(newColumnName, StringComparison.OrdinalIgnoreCase) && ColumnExists(Db.Connection, new TableDescriptor(schemaName, tableName), newColumnName))
				{
					// If it was created by the pre-schema upgrade, remove it
					new DbColumnDependencyRemover(schemaName, tableName, newColumnName).DropRelateObjects(Db.Connection);
					Db.Connection.ExecuteNonQuery(FormattableString.Invariant($"ALTER TABLE [{schemaName}].[{tableName}] DROP COLUMN [{newColumnName}]"));
				}

				new DbColumnDependencyRemover(schemaName, tableName, oldColumnName).DropRelateObjectsBeforeRenamingColumn(Db.Connection, newColumnName);
				DbObjectCreator.RenameColumn(Db.Connection, schemaName, tableName, oldColumnName, newColumnName);
				var defaultConstraintName = DbObjectCreator.GenerateDefaultColumnConstraintName(tableName, newColumnName);
				DbObjectCreator.RenameDefaultColumnConstraintIfExists(Db.Connection, schemaName, tableName, newColumnName, defaultConstraintName);
			}
		}

		protected virtual void PostRenameTransformation()
		{
			if (ADAWMappingUpdateInfos.Any())
			{
				var helper = new ADAWMappingUpdateHelper();
				helper.UpdateADAWTemplateMappingField(ADAWMappingUpdateInfos);
			}
		}

		protected virtual IEnumerable<AdAWMappingUpdateInfo> ADAWMappingUpdateInfos => Array.Empty<AdAWMappingUpdateInfo>();
	}
}
