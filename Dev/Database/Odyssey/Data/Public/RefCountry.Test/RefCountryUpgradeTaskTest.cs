using System;
using System.Data;
using System.Globalization;
using CargoWise.Data;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Data.Testing
{
	sealed class RefCountryUpgradeTaskTest : TransactionedTestCase
	{
		public void TestTableUpgradeOrder()
		{
			RefCountryUpgradeTaskForTableOrderTest testUpgradeTask = new RefCountryUpgradeTaskForTableOrderTest();
			testUpgradeTask.Run();

			AssertEquals("Should have 2 tables: Country, Currency", 2, testUpgradeTask.TablesInUpgradingOrder.Length);
			AssertEquals("1st table", RefCountrySchema.Constants.TableName, testUpgradeTask.TablesInUpgradingOrder[0]);
			AssertEquals("2nd table", RefCurrencySchema.Constants.TableName, testUpgradeTask.TablesInUpgradingOrder[1]);
		}

		public void TestOnlyRequiredIfNewSystem()
		{
			DummyDataFile testDataFile = new DummyDataFile(2);
			NewSystemDataUpgradeTaskForTesting testUpgradeTask = new NewSystemDataUpgradeTaskForTesting(testDataFile);

			testDataFile.VersionInDatabase = 1;
			AssertEquals("[Version 1 to 2] Is Required?", false, testUpgradeTask.IsRequired);

			testDataFile.VersionInDatabase = 0;
			AssertEquals("[Version 0 to 2]Is Required?", true, testUpgradeTask.IsRequired);

			testDataFile = new DummyDataFile(0);
			testUpgradeTask = new NewSystemDataUpgradeTaskForTesting(testDataFile);

			testDataFile.VersionInDatabase = 1;
			AssertEquals("[Version 1 to 0] Is Required?", false, testUpgradeTask.IsRequired);

			testDataFile.VersionInDatabase = 0;
			AssertEquals("[Version 0 to 0] Is Required?", false, testUpgradeTask.IsRequired);
		}

		public void TestRX_IsActiveNotUpdated()
		{
			string assertSql = "SELECT TOP 1 RX_IsActive FROM dbo.RefCurrency WHERE RX_Code = 'AUD'";
			AssertEquals("[PRE-CONDITION] AUD is active", true, Db.Connection.ExecuteScalar(assertSql));

			string updateSql = "UPDATE dbo.RefCurrency SET RX_IsActive = 0 WHERE RX_Code = 'AUD'";
			Db.Connection.ExecuteNonQuery(updateSql);

			RefCountryUpgradeTask testTask = new RefCountryUpgradeTask();
			testTask.Run();

			AssertEquals("UpgradeTask should not update is active value", false, Db.Connection.ExecuteScalar(assertSql));
		}

		public void TestCountryRuleFieldsNotUpdated()
		{
			string updateSqlText = @"
				UPDATE dbo.RefCountry 
				SET RN_PostcodeValidationRule = '{0}', 
					RN_AddressFormattingRule = '{1}', 
					RN_StateProvinceValidationRule = '{2}' 
				WHERE RN_Code = '{3}'";
			string selectSqlText = @"
				SELECT RN_PostcodeValidationRule, RN_AddressFormattingRule, RN_StateProvinceValidationRule 
				FROM dbo.RefCountry 
				WHERE RN_Code = '{0}'";

			string sqlText = string.Format(updateSqlText, "TST", "TST", "TST", "UA");
			Db.Connection.ExecuteNonQuery(sqlText);
			sqlText = string.Format(updateSqlText, "NVR", "PBC", "MNB", "AU");
			Db.Connection.ExecuteNonQuery(sqlText);

			RefCountryUpgradeTask testTask = new RefCountryUpgradeTask();
			testTask.Run();

			using (var reader = Db.Connection.Command(string.Format(selectSqlText, "UA")).ExecuteReader())
			{
				reader.Read();
				AssertEquals("RefCountry.RN_PostcodeValidationRule should not be updated", "TST", reader["RN_PostcodeValidationRule"].ToString());
				AssertEquals("RefCountry.RN_AddressFormattingRule should not be updated", "TST", reader["RN_AddressFormattingRule"].ToString());
				AssertEquals("RefCountry.RN_StateProvinceValidationRule should not be updated", "TST", reader["RN_StateProvinceValidationRule"].ToString());
			}

			using (var reader = Db.Connection.Command(string.Format(selectSqlText, "AU")).ExecuteReader())
			{
				reader.Read();
				AssertEquals("RefCountry.RN_PostcodeValidationRule should not be updated", "NVR", reader["RN_PostcodeValidationRule"].ToString());
				AssertEquals("RefCountry.RN_AddressFormattingRule should not be updated", "PBC", reader["RN_AddressFormattingRule"].ToString());
				AssertEquals("RefCountry.RN_StateProvinceValidationRule should not be updated", "MNB", reader["RN_StateProvinceValidationRule"].ToString());
			}
		}

		class NewSystemDataUpgradeTaskForTesting : RefCountryUpgradeTask
		{
			public NewSystemDataUpgradeTaskForTesting(DummyDataFile resourceFile) : base(resourceFile)
			{
			}
		}

		class DummyDataFile : RefCountryDataFile, IDisposable
		{
			public DummyDataFile(int dataFileVersion) : base()
			{
				dummyDataSet = new DataSet(dataFileVersion.ToString(CultureInfo.InvariantCulture));
				dummyDataSet.Locale = CultureInfo.InvariantCulture;
			}

			protected override DataSet LoadDataSet()
			{
				return dummyDataSet;
			}

			public void Dispose()
			{
				dummyDataSet.Dispose();
			}

			readonly DataSet dummyDataSet;
		}
	}
}
