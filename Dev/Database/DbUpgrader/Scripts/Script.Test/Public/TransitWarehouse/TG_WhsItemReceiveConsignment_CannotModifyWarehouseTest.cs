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
	class TG_WhsItemReceiveConsignment_CannotModifyWarehouseTest : TG_CannotModifyWarehouseTest
	{
		protected override string TableName => WhsItemReceiveConsignmentSchema.Constants.TableName;
		protected override string WarehouseFKName => WhsItemReceiveConsignmentSchema.Constants.WRC_WW_IntendedWarehouse;
		protected override string PKName => WhsItemReceiveConsignmentSchema.Constants.PK;
		protected override string LastEditTime => WhsItemReceiveConsignmentSchema.Constants.WRC_SystemLastEditTimeUtc;
		protected override string LastEditUser => WhsItemReceiveConsignmentSchema.Constants.WRC_SystemLastEditUser;

		protected override Guid GetEntityPK(SqlQueryBuilder sql, WhsWarehouse whs)
		{
			var rcn = new WhsItemReceiveConsignment(whs, "RC000001", "RC000001", "STD", "A").AppendInsertAndReturnObject(sql);
			return rcn.PK;
		}
	}
}
