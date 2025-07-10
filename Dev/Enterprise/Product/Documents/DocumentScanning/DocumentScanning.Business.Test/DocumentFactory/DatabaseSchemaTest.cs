using CargoWise.Data;
using NUnit.Framework;

namespace Enterprise.DocumentScanning.Business.Testing
{
	sealed class DatabaseSchemaTest : TestCase
	{
		public void TestDocManagerStorageDocsTableMatchesOdysseyDbOne()
		{
			int newDBNumber = 12;
			DocManagerDBHelper dBUtilities = new DocManagerDBHelper();
			Assert("Precondition: DB " + newDBNumber + " Doesn't exist", !(dBUtilities.DatabaseExists(newDBNumber)));
			DocManagerDBHelperTestClass dBUtilitiesTestClass = new DocManagerDBHelperTestClass();
			string newDBName = dBUtilities.GetDatabaseName(dBUtilitiesTestClass.CreateDatabase(newDBNumber));

			// Thanks Dalmo!
			string sqlCommand = @"
				SELECT
					count(*)
				FROM
					(SELECT * FROM information_schema.columns WHERE table_name = 'StorageDocs') MainCol
					FULL JOIN (SELECT * FROM " + newDBName + @".information_schema.columns WHERE table_name = 'StorageDocs') SdCol
						ON  MainCol.table_name = SdCol.table_name
						AND MainCol.column_name = SdCol.column_name
						AND MainCol.data_type = SdCol.data_type
						--AND MainCol.ordinal_position = SdCol.ordinal_position --for some reason, not the same in main and SDxxx anymore. does this matter?
						AND MainCol.is_nullable = SdCol.is_nullable
						AND (
									(MainCol.column_default = SdCol.column_default)
									OR (MainCol.column_default is null AND SdCol.column_default is null)
								)
						AND (
									(MainCol.character_maximum_length = SdCol.character_maximum_length)
									OR (MainCol.character_maximum_length is null AND SdCol.character_maximum_length is null)
								)
						AND (
									(MainCol.numeric_precision = SdCol.numeric_precision)
									OR (MainCol.numeric_precision is null AND SdCol.numeric_precision is null)
								)
						AND (
									(MainCol.numeric_scale = SdCol.numeric_scale)
									OR (MainCol.numeric_scale is null AND SdCol.numeric_scale is null)
								) 
				WHERE
					MainCol.column_name is null OR SdCol.column_name is null";

			DbCommand command = Db.Connection.Command(sqlCommand); // Need to use db.connection; can't use factory to get this information

			try
			{
				AssertEquals("Number of columns differing between Odyssey schema and Odyssey_SDXXX schema", 0, (int)command.ExecuteScalar());
			}
			finally
			{
				dBUtilitiesTestClass.DropDatabase(newDBName);
			}
		}
	}
}
