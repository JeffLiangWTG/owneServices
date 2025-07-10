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
	class TG_WhsItemDispatchTransportationUnit_CannotModifyWarehouseTest : TG_CannotModifyWarehouseTest
	{
		protected override string TableName => WhsItemDispatchTransportationUnitSchema.Constants.TableName;
		protected override string WarehouseFKName => WhsItemDispatchTransportationUnitSchema.Constants.WDH_WW_Warehouse;
		protected override string PKName => WhsItemDispatchTransportationUnitSchema.Constants.PK;
		protected override string LastEditTime => WhsItemDispatchTransportationUnitSchema.Constants.WDH_SystemLastEditTimeUtc;
		protected override string LastEditUser => WhsItemDispatchTransportationUnitSchema.Constants.WDH_SystemLastEditUser;

		protected override Guid GetEntityPK(SqlQueryBuilder sql, WhsWarehouse whs)
		{
			var dtu = new WhsItemDispatchTransportationUnit(whs, "DTU000001", "").AppendInsertAndReturnObject(sql);
			return dtu.PK;
		}
	}
}
