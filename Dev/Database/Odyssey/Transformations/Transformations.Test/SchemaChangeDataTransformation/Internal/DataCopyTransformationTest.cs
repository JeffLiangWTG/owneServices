using System;
using System.Threading;
using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformation.DataModification;

namespace Enterprise.DbUpgrader.Transformations
{
	sealed class DataCopyTransformationTest : TestWithTransformationDirectorCopyDb
	{
		public void TestCopySourceData()
		{
			DataCopyForTesting testTransformation = new DataCopyForTesting();

			AssertEquals("Number of source tables", 2, testTransformation.SourceTables_Exposed.Count);
			AssertEquals("No tables should have been copied", false, testTransformation.SourceTables_Exposed.AreAllTablesCopied);

			testTransformation.Run(TransformationSection.OfflinePreUpgrade, CancellationToken.None);

			AssertEquals("All tables should have been copied", true, testTransformation.SourceTables_Exposed.AreAllTablesCopied);

			AssertEquals(
				"RefCountry table should have NO copy warnings, but it has: " + testTransformation.SourceTables_Exposed["RefCountry"].WarningMessageOnCopy,
				false, testTransformation.SourceTables_Exposed["RefCountry"].HasWarningsOnCopy);
			AssertEquals(
				"RefCountryStates table should have NO copy warnings, but it has: " + testTransformation.SourceTables_Exposed["RefCountryStates"].WarningMessageOnCopy,
				false, testTransformation.SourceTables_Exposed["RefCountryStates"].HasWarningsOnCopy);

			// Checks if copy of source tables have been created + populated
			// RefCountry
			string sqlText = "SELECT count(*) FROM " + testTransformation.SourceTables_Exposed["RefCountry"].FullyQualifiedName;
			int rowCount = Convert.ToInt32(Db.Connection.ExecuteScalar(sqlText));
			AssertEquals("Number of copied rows - RefCountry", 2, rowCount);
			sqlText = String.Format("SELECT {0} FROM {1} WHERE {2} = 'AU'", "RN_Desc", testTransformation.SourceTables_Exposed["RefCountry"].FullyQualifiedName, "RN_Code");
			string countryName = Db.Connection.ExecuteScalar(sqlText).ToString().ToUpper();
			AssertEquals("Country Name (AU)", "AUSTRALIA", countryName);
			// RefCountryStates
			sqlText = "SELECT count(*) FROM " + testTransformation.SourceTables_Exposed["RefCountryStates"].FullyQualifiedName;
			rowCount = Convert.ToInt32(Db.Connection.ExecuteScalar(sqlText));
			AssertEquals("Number of copied rows - RefCountryStates", 24, rowCount);
			sqlText = String.Format("SELECT {0} FROM {1} WHERE {2} = 'NSW'", "RW_Description", testTransformation.SourceTables_Exposed["RefCountryStates"].FullyQualifiedName, "RW_Code");
			string stateName = Db.Connection.ExecuteScalar(sqlText).ToString().ToUpper();
			AssertEquals("State Name (NSW)", "NEW SOUTH WALES", stateName);
			sqlText = String.Format("SELECT TOP 1 {0} FROM {1}", "InexistingColumnName", testTransformation.SourceTables_Exposed["RefCountryStates"].FullyQualifiedName);
			string defaultValue = Db.Connection.ExecuteScalar(sqlText).ToString();
			AssertEquals("Inexisting Column Default Value", "ABCDE", defaultValue);
		}

		public void TestCopySourceDataAndRun()
		{
			DataCopyForTesting testTransformation = new DataCopyForTesting();

			testTransformation.Run(TransformationSection.OfflinePreUpgrade, CancellationToken.None);

			AssertEquals("All tables should have been copied", true, testTransformation.SourceTables_Exposed.AreAllTablesCopied);

			testTransformation.Run(TransformationSection.OfflinePostUpgrade, CancellationToken.None);

			// Checks "pasted" data on RefCountry table
			string sqlText = "SELECT count(*) FROM dbo.RefCountry WHERE RN_NumberOfStates is not null";
			int rowCount = Convert.ToInt32(Db.Connection.ExecuteScalar(sqlText));
			AssertEquals("Number of RefCountry rows updated", 2, rowCount);
			sqlText = "SELECT RN_NumberOfStates FROM dbo.RefCountry WHERE RN_Code = 'AU'";
			int numberOfStates = Convert.ToInt32(Db.Connection.ExecuteScalar(sqlText));
			AssertEquals("Number of States - Australia(AU)", 8, numberOfStates);
			sqlText = "SELECT RN_NumberOfStates FROM dbo.RefCountry WHERE RN_Code = 'NZ'";
			numberOfStates = Convert.ToInt32(Db.Connection.ExecuteScalar(sqlText));
			AssertEquals("Number of States - New Zealand(NZ)", 16, numberOfStates);

			// Checks if source table copies have been deleted
			AssertEquals("All table copies should have been dropped", false, testTransformation.SourceTables_Exposed.AreAllTablesCopied);
			// RefCountry
			SourceTableForTesting countryTable = (SourceTableForTesting)testTransformation.SourceTables_Exposed["RefCountry"];
			bool doesSourceTableCopyExist = DbObjectCreator.TableExists(Db.Connection, countryTable.DataCopyStorageDb_Exposed, countryTable.FullName_Exposed);
			AssertEquals("Copy of source table should have been DROPPED - RefCountry", false,
				DbObjectCreator.TableExists(Db.Connection, countryTable.DataCopyStorageDb_Exposed, countryTable.FullName_Exposed));
			// RefCountryStates
			SourceTableForTesting statesTable = (SourceTableForTesting)testTransformation.SourceTables_Exposed["RefCountryStates"];
			AssertEquals("Copy of source table should have been DROPPED - RefCountryStates", false,
				DbObjectCreator.TableExists(Db.Connection, statesTable.DataCopyStorageDb_Exposed, statesTable.FullName_Exposed));
		}
	}
}
