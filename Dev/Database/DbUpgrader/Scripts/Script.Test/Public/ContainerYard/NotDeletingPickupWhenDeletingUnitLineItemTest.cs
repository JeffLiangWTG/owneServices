using System;
using System.Data;
using CargoWise.Database.TestFramework.ObjectModel;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Testing.Public.ContainerYard.Testing
{
	[TestedType(typeof(CYDUnitLineItem))]
	class NotDeletingPickupWhenDeletingUnitLineItemTest : TransactionedTestCase
	{
		public void TestForPickup()
		{
			var unitLineItemPK = Guid.NewGuid();
			var releaseAdvicePK = Guid.NewGuid();
			var warehouse = new WhsWarehouse("WH1", "CYD").WithDockDoor(TestConnection);
			var yardPK = warehouse.PK;
			var pickupHeaderPK = Guid.NewGuid();
			var pickupPK = Guid.NewGuid();

			const string insertSql = @"
				INSERT INTO dbo.CYDUnitLIneItem(YLI_PK,  YLI_RC_ContainerType, YLI_Type, YLI_SystemLastEditTimeUtc, YLI_SystemLastEditUser, YLI_SystemCreateTimeUtc, YLI_SystemCreateUser)
				VALUES(@UnitLineItemPK, '65FC688B-A8BC-49FA-A17E-159DE4ED6830', 'CNT',  GetUtcDate(), '~BP', GetUtcDate(), '~BP');

				INSERT INTO dbo.CYDPickupHeader(YPH_PK, YPH_WW_Yard, YPH_JobNumber, YPH_SystemLastEditTimeUtc, YPH_SystemLastEditUser, YPH_SystemCreateTimeUtc, YPH_SystemCreateUser)
				VALUES(@PickupHeaderPK, @YardPK, 'Job123', GetUtcDate(), '~BP', GetUtcDate(), '~BP');

				INSERT INTO dbo.CYDPickup(YPL_PK, YPL_YPH_PickupHeader, YPL_YLI_UnitLineItem, YPL_PickupID, YPL_SystemLastEditTimeUtc, YPL_SystemLastEditUser, YPL_SystemCreateTimeUtc, YPL_SystemCreateUser)
				VALUES(@PickupPK, @PickupHeaderPK, @UnitLineItemPK, 'YPL000000000001', GetUtcDate(), '~BP', GetUtcDate(), '~BP');
			";

			using (var command = TestConnection.Command(insertSql))
			{
				command.AddParameter("@ReleaseAdvicePK", SqlDbType.UniqueIdentifier, releaseAdvicePK);
				command.AddParameter("@YardPK", SqlDbType.UniqueIdentifier, yardPK);
				command.AddParameter("@UnitLineItemPK", SqlDbType.UniqueIdentifier, unitLineItemPK);
				command.AddParameter("@PickupHeaderPK", SqlDbType.UniqueIdentifier, pickupHeaderPK);
				command.AddParameter("@PickupPK", SqlDbType.UniqueIdentifier, pickupPK);
				command.ExecuteNonQuery();
			}

			AssertExceptionThrown(
				"Should prevent Delete",
				typeof(SqlException),
				"The DELETE statement conflicted with the REFERENCE constraint \"CYDPickup_YPL_YLI_UnitLineItem_FK2_CYDUnitLineItem_RRR_120N\"",
				() => TestConnection.ExecuteNonQuery("DELETE FROM dbo.CYDUnitLIneItem"),
				true
			);

			var countSql = "SELECT COUNT(*) FROM dbo.CYDPickupHeader WHERE YPH_PK=@PickupHeaderPK";
			using (var command = TestConnection.Command(countSql))
			{
				command.AddParameter("@PickupHeaderPK", SqlDbType.UniqueIdentifier, pickupHeaderPK);
				var pickupHeadersCount = (int)command.ExecuteScalar();
				AssertEquals(1, pickupHeadersCount);
			}

			var countSql1 = "SELECT COUNT(*) FROM dbo.CYDPickup WHERE YPL_PK=@PickupPK";
			using (var command = TestConnection.Command(countSql1))
			{
				command.AddParameter("@PickupPK", SqlDbType.UniqueIdentifier, pickupPK);
				var pickupsCount = (int)command.ExecuteScalar();
				AssertEquals(1, pickupsCount);
			}
		}
	}
}
