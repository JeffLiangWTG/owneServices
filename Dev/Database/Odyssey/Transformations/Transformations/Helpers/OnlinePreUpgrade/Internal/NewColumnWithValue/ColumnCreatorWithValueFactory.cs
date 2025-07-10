using System;
using System.Collections.Generic;
using System.Linq;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformation.Common;
using Enterprise.DbUpgrader.Transformations.PreUpgrade;
using Enterprise.DbUpgrader.Transformations.Transforms;

namespace Enterprise.DbUpgrader.Schema.OnlineUpgrade
{
	public interface IColumnCreatorWithValue
	{
		string PopulateExpression { get; }
		string PopulateSource { get; }
		string PopulateTriggerExpression { get; }
		string PopulateTriggerSource { get; }
		string OrderByExpression { get; }
	}

	public class ColumnCreatorWithValueFactory
	{
		public ColumnCreatorWithValueFactory(IUpgradeManager manager)
		{
			var mapper = new Mapper(manager);
			populateAuditTimeAndUserInfoList = mapper.GetAllRunningPopulateCreateTimeAndUserInfo();
			renameColumnTransformationList = mapper.GetAllRunningRenameColumnTransformations();
		}

		public string GetPreUpgradeTransformationRenamedColumnList()
		{
			return String.Join(",",
				renameColumnTransformationList
					.OrderBy(r => r.SchemaName)
					.ThenBy(r => r.TableName)
					.ThenBy(r => r.OldColumnName)
					.Select(r => String.Format("('{0}', '{1}', '{2}', '{3}')", r.SchemaName, r.TableName, r.OldColumnName, r.NewColumnName)));
		}

		public
#if DEBUG
			virtual
#endif
			IColumnCreatorWithValue GetColumnCreator(IUpgradeManager manager, ColumnChangeMetadata columnMetadata, string dbBeingUpgraded)
		{
			IPopulateAuditTimeAndUserInfo matchingCreateTimeAndUserInfo = FindMatchingCreateTimeAndUserInfo(columnMetadata, dbBeingUpgraded);

			if (matchingCreateTimeAndUserInfo != null)
			{
				return new ColumnCreatorPopulateAuditTimeAndUser(manager, columnMetadata, dbBeingUpgraded, matchingCreateTimeAndUserInfo);
			}
			else
			{
				return null;
			}
		}

		IPopulateAuditTimeAndUserInfo FindMatchingCreateTimeAndUserInfo(ColumnChangeMetadata columnMetadata, string dbBeingUpgraded)
		{
			return populateAuditTimeAndUserInfoList.FirstOrDefault(info =>
				info.TableSchema.TableName.Equals(columnMetadata.TableName, StringComparison.OrdinalIgnoreCase)
				&&
				(
					info.CreateTime != null && info.CreateTime.Name.Equals(columnMetadata.ColumnName, StringComparison.OrdinalIgnoreCase)
					|| info.CreateUser != null && info.CreateUser.Name.Equals(columnMetadata.ColumnName, StringComparison.OrdinalIgnoreCase)
					|| info.LastEditTime != null && info.LastEditTime.Name.Equals(columnMetadata.ColumnName, StringComparison.OrdinalIgnoreCase)
					|| info.LastEditUser != null && info.LastEditUser.Name.Equals(columnMetadata.ColumnName, StringComparison.OrdinalIgnoreCase)
				));
		}

		readonly IEnumerable<IPopulateAuditTimeAndUserInfo> populateAuditTimeAndUserInfoList;
		readonly IEnumerable<IRenameColumnTransformationInfo> renameColumnTransformationList;
	}
}
