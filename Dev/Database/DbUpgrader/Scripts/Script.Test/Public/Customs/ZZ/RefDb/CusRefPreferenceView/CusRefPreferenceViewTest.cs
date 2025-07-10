using System;
using System.Collections.Generic;
using CargoWise.Data;
using Enterprise.Build.Database.Script.Testing;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Customs.ZZ.RefDb.CusRefPreferenceView
{
	[TestedType(typeof(CargoWise.DbUpgrader.Scripts.Definitions.Customs.ZZ.RefDb.CusRefPreferenceView.CusRefPreferenceView))]
	class CusRefPreferenceView_Test : DbCreateScriptTest
	{
		protected override bool RequiresSchemaBinding => false;

		public void TestViewColumns()
		{
			var connection = Db.Connection;
			TestDbViewHelper.AssertViewColumnsMatchUnderlyingTable(
				connection,
				"CusRefPreferenceView", "RefDatabase_RefCusPreference",
				new[]
				{
					new TestDbViewHelper.DbColumn("ZZS_IsSystem", "bit", -1),
					new TestDbViewHelper.DbColumn("ZZS_DataSet", "varchar", 1),
					new TestDbViewHelper.DbColumn("ZZS_SystemCreateTimeUtc", "smalldatetime", -1),
					new TestDbViewHelper.DbColumn("ZZS_SystemCreateUser", "varchar", 3),
					new TestDbViewHelper.DbColumn("ZZS_SystemLastEditTimeUtc", "smalldatetime", -1),
					new TestDbViewHelper.DbColumn("ZZS_SystemLastEditUser", "varchar", 3),
				}
			);
		}

		public void TestLoad()
		{
			var connection = Db.Connection;
			connection.ExecuteNonQuery(@"
IF NOT EXISTS (SELECT NULL FROM RefDatabase_RefDataGrouping where ZZZ_DataGrouping = 'CN')
INSERT INTO RefDatabase_RefDataGrouping(ZZZ_PK, ZZZ_DataGrouping, ZZZ_Description, ZZZ_ZZZ_Grouping) 
VALUES (NEWID(), 'CN', 'China', NULL)

INSERT INTO RefDatabase_RefCusPreference(ZZS_PK, ZZS_Preference, ZZS_Description, ZZS_ZZZ_NKDataGrouping)
VALUES('D251DAA0-5A76-4E8D-AA44-501F4B705776','PRX','Test Preference ZZ','CN')

INSERT INTO dbo.CusRefPreference(CR8_PK,CR8_Preference,CR8_Description,CR8_RN_NKCountryCode, CR8_SystemCreateTimeUtc, CR8_SystemCreateUser, CR8_SystemLastEditTimeUtc, CR8_SystemLastEditUser)
VALUES('15789BE9-AA4E-410E-AC10-8BCF3882481B','PRY','Test Preference Cus','CN', '2020-07-02', '~E', '2020-07-03', '~F')
");

			var expected = new List<(string pk, string preference, string description, string grouping, string dataSet, bool isSystem, object systemCreateDate, string systemCreateUser, object systemLastEditDate, string systemLastEditUser)>
			{
				("D251DAA0-5A76-4E8D-AA44-501F4B705776","PRX","Test Preference ZZ","CN","Z",true, DBNull.Value, "~BP", DBNull.Value, "~BP"),
				("15789BE9-AA4E-410E-AC10-8BCF3882481B","PRY","Test Preference Cus","CN","O",false, new DateTime(2020, 7, 2), "~E", new DateTime(2020, 7, 3), "~F")
			};

			connection.ExecuteReader("SELECT * FROM dbo.CusRefPreferenceView", reader =>
				AssertCollectionContains((
					reader["ZZS_PK"].ToString().ToUpper(),
					(string)reader["ZZS_Preference"],
					(string)reader["ZZS_Description"],
					(string)reader["ZZS_ZZZ_NKDataGrouping"],
					(string)reader["ZZS_DataSet"],

					(bool)reader["ZZS_IsSystem"],
					reader["ZZS_SystemCreateTimeUtc"],
					(string)reader["ZZS_SystemCreateUser"],
					reader["ZZS_SystemLastEditTimeUtc"],
					(string)reader["ZZS_SystemLastEditUser"]
				), expected)
			);
		}
	}
}

