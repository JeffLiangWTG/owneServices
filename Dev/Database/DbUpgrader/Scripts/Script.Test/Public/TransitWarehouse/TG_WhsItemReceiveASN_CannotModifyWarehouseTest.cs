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
	class TG_WhsItemReceiveASN_CannotModifyWarehouseTest : TG_CannotModifyWarehouseTest
	{
		protected override string TableName => WhsItemReceiveASNSchema.Constants.TableName;
		protected override string WarehouseFKName => WhsItemReceiveASNSchema.Constants.WRP_WW_IntendedWarehouse;
		protected override string PKName => WhsItemReceiveASNSchema.Constants.PK;
		protected override string LastEditTime => WhsItemReceiveASNSchema.Constants.WRP_SystemLastEditTimeUtc;
		protected override string LastEditUser => WhsItemReceiveASNSchema.Constants.WRP_SystemLastEditUser;

		protected override Guid GetEntityPK(SqlQueryBuilder sql, WhsWarehouse whs)
		{
			var asn = new WhsItemReceiveASN(whs, "ASN000001", "ASN000001").AppendInsertAndReturnObject(sql);
			return asn.PK;
		}
	}
}
