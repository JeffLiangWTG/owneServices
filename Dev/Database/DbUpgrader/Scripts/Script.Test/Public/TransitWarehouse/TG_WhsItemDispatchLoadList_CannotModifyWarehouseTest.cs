using System;
using CargoWise.Database.TestFramework.ObjectModel;
using CargoWise.DbUpgrader.Scripts.Definitions.TransitWarehouse;
using Enterprise.Build.Database.Script.Testing.Public.TransitWarehouse;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Shared.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.TransitWarehouse.Testing
{
	[TestedType(typeof(TG_WhsItemDispatchConsignment_CannotModifyWarehouse))]
	class TG_WhsItemDispatchLoadList_CannotModifyWarehouseTest : TG_CannotModifyWarehouseTest
	{
		protected override string TableName => WhsItemDispatchLoadListSchema.Constants.TableName;
		protected override string WarehouseFKName => WhsItemDispatchLoadListSchema.Constants.WDL_WW_Warehouse;
		protected override string PKName => WhsItemDispatchLoadListSchema.Constants.PK;
		protected override string LastEditTime => WhsItemDispatchLoadListSchema.Constants.WDL_SystemLastEditTimeUtc;
		protected override string LastEditUser => WhsItemDispatchLoadListSchema.Constants.WDL_SystemLastEditUser;

		protected override Guid GetEntityPK(SqlQueryBuilder sql, WhsWarehouse whs)
		{
			var dll = new WhsItemDispatchLoadList("DLL000001", whs).AppendInsertAndReturnObject(sql);
			return dll.PK;
		}
	}
}
