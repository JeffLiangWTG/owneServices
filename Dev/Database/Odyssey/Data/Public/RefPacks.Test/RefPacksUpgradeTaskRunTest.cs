using System;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Data.Testing
{
	public class RefPacksUpgradeTaskRunTest : TransactionedTestCase
	{
		public void TestConfusingConcurrencyException()
		{
			var task = new RefPacksUpgradeTask(new RefPacksDataFile());
			AssertNoExceptionThrown(() =>
			{
				task.Run();
			});
		}

		public void TestAllForRefPacks()
		{
			string sql = "DELETE FROM dbo.RefPacks WHERE RP_CustomsCountry in ('SB', 'FJ','TW') ";
			TestConnection.ExecuteScalar(sql);

			var task = new RefPacksUpgradeTask(new RefPacksDataFile());
			task.Run();

			sql = "SELECT COUNT(*) FROM dbo.RefPacks WHERE RP_CustomsCountry in ('SB', 'FJ') ";
			var counter = (int)TestConnection.ExecuteScalar(sql);
			Assert(counter > 0);

			sql = "SELECT COUNT(1) FROM dbo.RefPacks WHERE RP_CustomsCountry in ('TW') ";
			counter = (int)TestConnection.ExecuteScalar(sql);
			Assert(counter > 0);
		}

		public void TestHonoursUniqueKey()
		{
			var originalRpPk = Guid.NewGuid();
			string sql = "DELETE FROM dbo.RefPacks WHERE RP_CustomsCountry in ('SB', 'FJ') ";
			sql += "\r\n	delete from dbo.StmData where SD_Name like '%refpacks.xml%'";
			sql += "\r\n	insert into dbo.refPacks (RP_PK, RP_CommercialPack,RP_CustomsCountry, RP_CustomsPack, RP_IsSystem ) values ('" + originalRpPk.ToString() + "','CAS', 'FJ', 'CS', 0)";
			TestConnection.ExecuteNonQuery(sql);

			var task = new RefPacksUpgradeTask(new RefPacksDataFile());
			task.Run();

			sql = "SELECT RP_PK FROM dbo.RefPacks WHERE RP_CustomsCountry ='FJ' and RP_CommercialPack='CAS' and RP_CustomsPack='CS' ";
			var rpPkAfterTransform = TestConnection.ExecuteScalar(sql);
			AssertEquals("Row is not replaced by the transformation and the original one remains.", originalRpPk, rpPkAfterTransform);
		}

		public void TestTaiwanRP_Type()
		{
			var originalRpPk = Guid.NewGuid();
			string sql = "DELETE FROM dbo.RefPacks WHERE RP_CustomsCountry in ('TW') ";
			TestConnection.ExecuteNonQuery(sql);

			var task = new RefPacksUpgradeTask(new RefPacksDataFile());
			task.Run();

			sql = "SELECT COUNT(1) FROM dbo.RefPacks WHERE RP_CustomsCountry ='TW'";
			var counter = (int)TestConnection.ExecuteScalar(sql);

			sql = "SELECT COUNT(1) FROM dbo.RefPacks WHERE RP_CustomsCountry ='TW' and RP_Type='CIP'";
			var cipCounter = (int)TestConnection.ExecuteScalar(sql);
			Assert("Taiwan Row have RP_Type.", counter > 0);
			Assert(cipCounter == counter);
		}
	}
}
