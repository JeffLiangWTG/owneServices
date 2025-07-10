using System;
using System.Data;
using CargoWise.Database.TestFramework.ObjectModel;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Testing.Public.ContainerYard.Testing
{
	[TestedType(typeof(CYDUnitLineItem))]
	class NotDeletingReleaseAdviceLineWhenDeletingUnitLineItemTest : TransactionedTestCase
	{
		public void TestForReleaseAdviceLine()
		{
			var unitLineItemPK = Guid.NewGuid();
			var releaseAdvicePK = Guid.NewGuid();
			var releaseAdviceLinePK = Guid.NewGuid();
			var warehouse = new WhsWarehouse("WH1", "CYD").WithDockDoor(TestConnection);
			var yardPK = warehouse.PK;

			const string insertSql = @"
				INSERT INTO dbo.CYDUnitLIneItem(YLI_PK,  YLI_RC_ContainerType, YLI_Type, YLI_SystemLastEditTimeUtc, YLI_SystemLastEditUser, YLI_SystemCreateTimeUtc, YLI_SystemCreateUser)
				VALUES(@UnitLineItemPK, '65FC688B-A8BC-49FA-A17E-159DE4ED6830', 'CNT',  GetUtcDate(), '~BP', GetUtcDate(), '~BP');

				INSERT INTO dbo.CYDReleaseAdvice (YRE_PK, YRE_AutoVersion, YRE_JobNumber, YRE_ReleaseNumber, YRE_WW_Yard, YRE_FromDate, YRE_ToDate, YRE_Mode, YRE_SystemCreateTimeUtc, YRE_SystemCreateUser, YRE_SystemLastEditTimeUtc, YRE_SystemLastEditUser)
				VALUES (@ReleaseAdvicePK, 0, 23, 'REL1', @YardPK, '2016-04-22 00:00:00', '2016-04-24 00:00:00', 'EMT', GetUtcDate(), '~BP', GetUtcDate(), '~BP');
				
				INSERT INTO dbo.CYDReleaseAdviceLine (YEL_PK, YEL_YRE_ReleaseAdvice, YEL_YLI_UnitLineItem, YEL_PickupDate, YEL_ReadyDate,  YEL_SystemCreateTimeUtc, YEL_SystemCreateUser, YEL_SystemLastEditTimeUtc, YEL_SystemLastEditUser)
				VALUES (@ReleaseAdviceLinePK, @ReleaseAdvicePK, @UnitLineItemPK, GetUtcDate(), GetUtcDate(), GetUtcDate(), '~BP', GetUtcDate(), '~BP')
			";

			using (var command = TestConnection.Command(insertSql))
			{
				command.AddParameter("@ReleaseAdvicePK", SqlDbType.UniqueIdentifier, releaseAdvicePK);
				command.AddParameter("@ReleaseAdviceLinePK", SqlDbType.UniqueIdentifier, releaseAdviceLinePK);
				command.AddParameter("@YardPK", SqlDbType.UniqueIdentifier, yardPK);
				command.AddParameter("@UnitLineItemPK", SqlDbType.UniqueIdentifier, unitLineItemPK);
				command.ExecuteNonQuery();
			}

			AssertExceptionThrown(
				"Should prevent Delete",
				typeof(SqlException),
				"The DELETE statement conflicted with the REFERENCE constraint \"CYDReleaseAdviceLine_YEL_YLI_UnitLineItem_FK2_CYDUnitLineItem_RRR_120N\"",
				() => TestConnection.ExecuteNonQuery("DELETE FROM dbo.CYDUnitLIneItem"),
				true
			);

			var countSql = "SELECT COUNT(*) FROM dbo.CYDReleaseAdvice WHERE YRE_PK=@ReleaseAdvicePK";
			using (var command = TestConnection.Command(countSql))
			{
				command.AddParameter("@ReleaseAdvicePK", SqlDbType.UniqueIdentifier, releaseAdvicePK);
				var releaseAdviceCount = (int)command.ExecuteScalar();
				AssertEquals(1, releaseAdviceCount);
			}

			var countSql1 = "SELECT COUNT(*) FROM dbo.CYDReleaseAdviceLine WHERE YEL_PK=@ReleaseAdviceLinePK";
			using (var command = TestConnection.Command(countSql1))
			{
				command.AddParameter("@ReleaseAdviceLinePK", SqlDbType.UniqueIdentifier, releaseAdviceLinePK);
				var releaseAdviceLinesCount = (int)command.ExecuteScalar();
				AssertEquals(1, releaseAdviceLinesCount);
			}
		}
	}
}
