using System;
using System.Data;
using CargoWise.Database.TestFramework.ObjectModel;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Testing.Public.ContainerYard.Testing
{
	[TestedType(typeof(CYDUnitLineItem))]
	class NotDeletingDeliveryWhenDeletingUnitLineItemTest : TransactionedTestCase
	{
		public void TestForDelivery()
		{
			var unitLineItemPK = Guid.NewGuid();
			var receiveAdvicePK = Guid.NewGuid();
			var warehouse = new WhsWarehouse("WH1", "CYD").WithDockDoor(TestConnection);
			var yardPK = warehouse.PK;
			var deliveryHeaderPK = Guid.NewGuid();
			var deliveryPK = Guid.NewGuid();

			const string insertSql = @"
				INSERT INTO dbo.CYDUnitLIneItem(YLI_PK,  YLI_RC_ContainerType, YLI_Type, YLI_SystemLastEditTimeUtc, YLI_SystemLastEditUser, YLI_SystemCreateTimeUtc, YLI_SystemCreateUser)
				VALUES(@UnitLineItemPK, '65FC688B-A8BC-49FA-A17E-159DE4ED6830', 'CNT',  GetUtcDate(), '~BP', GetUtcDate(), '~BP');

				INSERT INTO dbo.CYDDeliveryHeader(YDH_PK, YDH_WW_Yard, YDH_JobNumber, YDH_SystemLastEditTimeUtc, YDH_SystemLastEditUser, YDH_SystemCreateTimeUtc, YDH_SystemCreateUser)
				VALUES(@DeliveryHeaderPK, @YardPK, 'Job123', GetUtcDate(), '~BP', GetUtcDate(), '~BP');

				INSERT INTO dbo.CYDDelivery(YDL_PK, YDL_YDH_DeliveryHeader, YDL_YLI_UnitLineItem, YDL_DeliveryID, YDL_SystemLastEditTimeUtc, YDL_SystemLastEditUser, YDL_SystemCreateTimeUtc, YDL_SystemCreateUser)
				VALUES(@DeliveryPK, @DeliveryHeaderPK, @UnitLineItemPK, 'YDL000000000001', GetUtcDate(), '~BP', GetUtcDate(), '~BP');
			";

			using (var command = TestConnection.Command(insertSql))
			{
				command.AddParameter("@ReceiveAdvicePK", SqlDbType.UniqueIdentifier, receiveAdvicePK);
				command.AddParameter("@YardPK", SqlDbType.UniqueIdentifier, yardPK);
				command.AddParameter("@UnitLineItemPK", SqlDbType.UniqueIdentifier, unitLineItemPK);
				command.AddParameter("@DeliveryHeaderPK", SqlDbType.UniqueIdentifier, deliveryHeaderPK);
				command.AddParameter("@DeliveryPK", SqlDbType.UniqueIdentifier, deliveryPK);
				command.ExecuteNonQuery();
			}

			AssertExceptionThrown(
				"Should prevent Delete",
				typeof(SqlException),
				"The DELETE statement conflicted with the REFERENCE constraint \"CYDDelivery_YDL_YLI_UnitLineItem_FK2_CYDUnitLineItem_RRR_120N\"",
				() => TestConnection.ExecuteNonQuery("DELETE FROM dbo.CYDUnitLIneItem"),
				true
			);

			var countSql = "SELECT COUNT(*) FROM dbo.CYDDeliveryHeader WHERE YDH_PK=@DeliveryHeaderPK";
			using (var command = TestConnection.Command(countSql))
			{
				command.AddParameter("@DeliveryHeaderPK", SqlDbType.UniqueIdentifier, deliveryHeaderPK);
				var deliveryHeadersCount = (int)command.ExecuteScalar();
				AssertEquals(1, deliveryHeadersCount);
			}

			var countSql1 = "SELECT COUNT(*) FROM dbo.CYDDelivery WHERE YDL_PK=@DeliveryPK";
			using (var command = TestConnection.Command(countSql1))
			{
				command.AddParameter("@DeliveryPK", SqlDbType.UniqueIdentifier, deliveryPK);
				var deliveriesCount = (int)command.ExecuteScalar();
				AssertEquals(1, deliveriesCount);
			}
		}
	}
}
