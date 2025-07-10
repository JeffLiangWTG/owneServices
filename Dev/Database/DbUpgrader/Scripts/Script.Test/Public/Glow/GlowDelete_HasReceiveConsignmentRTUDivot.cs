using System;
using System.Data;
using CargoWise.Data;
using CargoWise.Database.TestFramework.ObjectModel;
using Enterprise.DbUpgrader.Shared.Testing;

namespace Enterprise.Build.Database.Script.Public.Glow.Testing
{
	abstract class GlowDelete_HasReceiveConsignmentRTUDivot : DbCreateScriptTest
	{
		protected abstract string TableName { get; }
		protected abstract string PKName { get; }
		protected abstract string AutoVersion { get; }
		protected abstract string StoredProcedureName { get; }
		protected abstract string StoredProcedureAttributeName { get; }
		Guid PKValue { get; set; }
		protected Guid pk_rcn { get; set; }
		protected Guid pk_rtu { get; set; }

		public void TestDeleteRelatedRecords()
		{
			Guid pk_rcn_rtu_divot;
			(pk_rcn, pk_rtu, pk_rcn_rtu_divot) = SetupData();
			PKValue = GetEntityPK();

			AssertEquals(1, TestConnection.ExecuteScalar($"SELECT COUNT(*) FROM dbo.WhsItemReceiveConsignment WHERE WRC_PK = '{pk_rcn}'"));
			AssertEquals(1, TestConnection.ExecuteScalar($"SELECT COUNT(*) FROM dbo.WhsItemReceiveTransportationUnit WHERE WRH_PK = '{pk_rtu}'"));
			AssertEquals(1, TestConnection.ExecuteScalar($"SELECT COUNT(*) FROM dbo.WhsItemReceiveConsignmentRTUDivot WHERE WRD_PK = '{pk_rcn_rtu_divot}'"));

			RunSP(PKValue);

			AssertEquals(0, TestConnection.ExecuteScalar($"SELECT COUNT(*) FROM dbo.WhsItemReceiveConsignmentRTUDivot WHERE WRD_PK = '{pk_rcn_rtu_divot}'"));
			AssertEquals(0, TestConnection.ExecuteScalar($"SELECT COUNT(*) FROM {TableName} WHERE {PKName} = '{PKValue}'"));
		}

		(Guid rcn_pk, Guid rtu_pk, Guid rcn_rtu_divot_pk) SetupData()
		{
			var branch = new GlbBranch("BR1").InsertAndReturnObject(TestConnection);
			var whs = new WhsWarehouse("WH2", "TRW", branch.PK).WithDockDoor(TestConnection);
			var rcn = new WhsItemReceiveConsignment(whs, "RC000001", "RC000001", "STD", "A").InsertAndReturnObject(TestConnection);

			var area = new WhsArea(whs.PK, "AREA1").InsertAndReturnObject(TestConnection);
			var row = new WhsRow(whs, "A").InsertAndReturnObject(TestConnection);
			var location = new WhsLocation(row.PK, area.PK, area.PK).InsertAndReturnObject(TestConnection);
			var rtu = new WhsItemReceiveTransportationUnit(whs, "RTU000001", location, "STD", "").InsertAndReturnObject(TestConnection);

			var rcn_rtu_divot = new WhsItemReceiveConsignmentRTUDivot(rcn, rtu).InsertAndReturnObject(TestConnection);

			return (rcn.PK, rtu.PK, rcn_rtu_divot.PK);
		}

		void RunSP(Guid pk)
		{
			short version = (short)Db.Connection.ExecuteScalar($"SELECT {AutoVersion} FROM {TableName} WHERE {PKName} = '{pk}' ");
			using (var command = Db.Connection.Command(StoredProcedureName))
			{
				command.CommandType = CommandType.StoredProcedure;
				command.AddParameter(StoredProcedureAttributeName, SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@Version", SqlDbType.SmallInt, version);
				command.ExecuteScalar();
			}
		}

		protected abstract Guid GetEntityPK();
	}
}
