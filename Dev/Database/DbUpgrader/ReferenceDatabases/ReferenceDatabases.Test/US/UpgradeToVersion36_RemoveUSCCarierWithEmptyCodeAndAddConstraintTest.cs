using System;
using CargoWise.Data;

namespace Enterprise.DbUpgrader.ReferenceDatabases.US.Testing
{
	sealed class UpgradeToVersion36_RemoveUSCCarierWithEmptyCodeAndAddConstraintTest : USReferenceDbUpgraderVersionTest
	{
		protected override void PrepareTestData(DbConnection conn)
		{
			base.PrepareTestData(conn);
			string sqlText = string.Format(@"
IF EXISTS (SELECT null FROM [{3}].sys.check_constraints WHERE object_id = OBJECT_ID(N'[{3}].[dbo].[CK_USCCarrier_UI_CodeNotEmpty]') AND parent_object_id = OBJECT_ID(N'[{3}].[dbo].[USCCarrier]'))
ALTER TABLE [{3}].[dbo].[USCCarrier] DROP CONSTRAINT [CK_USCCarrier_UI_CodeNotEmpty]

INSERT [dbo].[USCCarrier] ([UI_PK], [UI_Code], [UI_Name], [UI_AirwayBillPrefix], [UI_ModeOfTransportation], [UI_Address]) VALUES ('{0}', '', 'DUMMY 1', '', '', '')
INSERT [dbo].[USCCarrier] ([UI_PK], [UI_Code], [UI_Name], [UI_AirwayBillPrefix], [UI_ModeOfTransportation], [UI_Address]) VALUES ('{1}', 'ZZ!Z', 'DUMMY 2', '', '', '')
INSERT [dbo].[USCCarrier] ([UI_PK], [UI_Code], [UI_Name], [UI_AirwayBillPrefix], [UI_ModeOfTransportation], [UI_Address]) VALUES ('{2}', '', 'DUMMY 3', '', '', '')
", carrierPK1, carrierPK2, carrierPK3, refDbUpgrader.DbName);
			conn.ExecuteNonQuery(sqlText);
		}

		readonly Guid carrierPK1 = Guid.NewGuid();
		readonly Guid carrierPK2 = Guid.NewGuid();
		readonly Guid carrierPK3 = Guid.NewGuid();

		protected override void AssertUpgradeResult()
		{
			AssertEquals(1, testConnection.ExecuteScalar(string.Format("SELECT COUNT(*) FROM [{0}].sys.check_constraints WHERE object_id = OBJECT_ID(N'[{0}].[dbo].[CK_USCCarrier_UI_CodeNotEmpty]') AND parent_object_id = OBJECT_ID(N'[{0}].[dbo].[USCCarrier]')", refDbUpgrader.DbName)));
			AssertEquals(0, testConnection.ExecuteScalar(string.Format("select count(*) from [{0}]..USCCarrier where UI_PK = '{1}'", refDbUpgrader.DbName, carrierPK1)));
			AssertEquals(1, testConnection.ExecuteScalar(string.Format("select count(*) from [{0}]..USCCarrier where UI_PK = '{1}'", refDbUpgrader.DbName, carrierPK2)));
			AssertEquals(0, testConnection.ExecuteScalar(string.Format("select count(*) from [{0}]..USCCarrier where UI_PK = '{1}'", refDbUpgrader.DbName, carrierPK3)));
		}

		protected override int LatestVersionNumber
		{
			get { return 36; }
		}
	}
}
