using System;
using System.Data;
using CargoWise.Database.TestFramework.ObjectModel;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Testing.Public.ContainerYard.Testing
{
	[TestedType(typeof(CYDUnitLineItem))]
	class NotDeletingReceiveAdviceLineWhenDeletingUnitLineItemTest : TransactionedTestCase
	{
		public void TestForReceiveAdviceLine()
		{
			var unitLineItemPK = Guid.NewGuid();
			var receiveAdvicePK = Guid.NewGuid();
			var receiveAdviceLinePK = Guid.NewGuid();
			var warehouse = new WhsWarehouse("WH1", "CYD").WithDockDoor(TestConnection);
			var yardPK = warehouse.PK;

			const string insertSql = @"
				INSERT INTO dbo.CYDUnitLIneItem(YLI_PK,  YLI_RC_ContainerType, YLI_Type, YLI_SystemLastEditTimeUtc, YLI_SystemLastEditUser, YLI_SystemCreateTimeUtc, YLI_SystemCreateUser)
				VALUES(@UnitLineItemPK, '65FC688B-A8BC-49FA-A17E-159DE4ED6830', 'CNT',  GetUtcDate(), '~BP', GetUtcDate(), '~BP');

				INSERT INTO dbo.CYDReceiveAdvice (YRA_PK, YRA_AutoVersion, YRA_JobNumber, YRA_AcceptanceNumber, YRA_WW_Yard, YRA_FromDate, YRA_ToDate, YRA_Mode, YRA_SystemCreateTimeUtc, YRA_SystemCreateUser, YRA_SystemLastEditTimeUtc, YRA_SystemLastEditUser)
				VALUES (@ReceiveAdvicePK, 0, 23, 23, @YardPK, '2016-04-22 00:00:00', '2016-04-24 00:00:00', 'EMT', GetUtcDate(), '~BP', GetUtcDate(), '~BP');

				INSERT INTO dbo.CYDReceiveAdviceLine (YRL_PK, YRL_YRA_ReceiveAdvice, YRL_YLI_UnitLineItem, YRL_SystemCreateTimeUtc, YRL_SystemCreateUser, YRL_SystemLastEditTimeUtc, YRL_SystemLastEditUser)
				VALUES (@ReceiveAdviceLinePK, @ReceiveAdvicePK, @UnitLineItemPK, GetUtcDate(), '~BP', GetUtcDate(), '~BP')
			";

			using (var command = TestConnection.Command(insertSql))
			{
				command.AddParameter("@ReceiveAdvicePK", SqlDbType.UniqueIdentifier, receiveAdvicePK);
				command.AddParameter("@ReceiveAdviceLinePK", SqlDbType.UniqueIdentifier, receiveAdviceLinePK);
				command.AddParameter("@YardPK", SqlDbType.UniqueIdentifier, yardPK);
				command.AddParameter("@UnitLineItemPK", SqlDbType.UniqueIdentifier, unitLineItemPK);
				command.ExecuteNonQuery();
			}

			AssertExceptionThrown(
				"Should prevent Delete",
				typeof(SqlException),
				"The DELETE statement conflicted with the REFERENCE constraint \"CYDReceiveAdviceLine_YRL_YLI_UnitLineItem_FK2_CYDUnitLineItem_RRR_120N\"",
				() => TestConnection.ExecuteNonQuery("DELETE FROM dbo.CYDUnitLIneItem"),
				true
			);

			var countSql = "SELECT COUNT(*) FROM dbo.CYDReceiveAdvice WHERE YRA_PK=@ReceiveAdvicePK";
			using (var command = TestConnection.Command(countSql))
			{
				command.AddParameter("@ReceiveAdvicePK", SqlDbType.UniqueIdentifier, receiveAdvicePK);
				var receiveAdviceCount = (int)command.ExecuteScalar();
				AssertEquals(1, receiveAdviceCount);
			}

			var countSql1 = "SELECT COUNT(*) FROM dbo.CYDReceiveAdviceLine WHERE YRL_PK=@ReceiveAdviceLinePK";
			using (var command = TestConnection.Command(countSql1))
			{
				command.AddParameter("@ReceiveAdviceLinePK", SqlDbType.UniqueIdentifier, receiveAdviceLinePK);
				var receiveAdviceLinesCount = (int)command.ExecuteScalar();
				AssertEquals(1, receiveAdviceLinesCount);
			}
		}
	}
}
