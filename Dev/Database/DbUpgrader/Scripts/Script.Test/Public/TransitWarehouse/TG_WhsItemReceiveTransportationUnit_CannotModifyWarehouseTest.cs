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
	class TG_WhsItemReceiveTransportationUnit_CannotModifyWarehouseTest : TG_CannotModifyWarehouseTest
	{
		protected override string TableName => WhsItemReceiveTransportationUnitSchema.Constants.TableName;
		protected override string WarehouseFKName => WhsItemReceiveTransportationUnitSchema.Constants.WRH_WW_Warehouse;
		protected override string PKName => WhsItemReceiveTransportationUnitSchema.Constants.PK;
		protected override string LastEditTime => WhsItemReceiveTransportationUnitSchema.Constants.WRH_SystemLastEditTimeUtc;
		protected override string LastEditUser => WhsItemReceiveTransportationUnitSchema.Constants.WRH_SystemLastEditUser;

		protected override Guid GetEntityPK(SqlQueryBuilder sql, WhsWarehouse whs)
		{
			var area = new WhsArea(whs.PK, "AREA1").AppendInsertAndReturnObject(sql);
			var row = new WhsRow(whs, "A").AppendInsertAndReturnObject(sql);
			var location = new WhsLocation(row.PK, area.PK, area.PK).AppendInsertAndReturnObject(sql);
			var rtu = new WhsItemReceiveTransportationUnit(whs, "RTU000001", location, "STD", "").AppendInsertAndReturnObject(sql);
			return rtu.PK;
		}
	}
}
