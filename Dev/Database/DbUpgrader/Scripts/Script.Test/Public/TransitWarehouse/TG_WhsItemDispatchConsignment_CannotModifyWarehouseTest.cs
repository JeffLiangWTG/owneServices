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
	class TG_WhsItemDispatchConsignment_CannotModifyWarehouseTest : TG_CannotModifyWarehouseTest
	{
		protected override string TableName => WhsItemDispatchConsignmentSchema.Constants.TableName;
		protected override string WarehouseFKName => WhsItemDispatchConsignmentSchema.Constants.WDC_WW_Warehouse;
		protected override string PKName => WhsItemDispatchConsignmentSchema.Constants.PK;
		protected override string LastEditTime => WhsItemDispatchConsignmentSchema.Constants.WDC_SystemLastEditTimeUtc;
		protected override string LastEditUser => WhsItemDispatchConsignmentSchema.Constants.WDC_SystemLastEditUser;

		protected override Guid GetEntityPK(SqlQueryBuilder sql, WhsWarehouse whs)
		{
			var dcn = new WhsItemDispatchConsignment(whs, "DC000001", "DC000001", "STD").AppendInsertAndReturnObject(sql);
			return dcn.PK;
		}
	}
}
