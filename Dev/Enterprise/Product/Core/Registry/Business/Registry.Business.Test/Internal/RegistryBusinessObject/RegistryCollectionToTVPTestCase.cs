using System.Data;
using CargoWise.Data;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	public abstract class RegistryCollectionToTVPTestCase<T> : TransactionedTestCase
			where T : RegistryBusinessObjectCollection, IRegistryCollectionToTVP
	{
		protected abstract T GetCollectionToTest();
		protected abstract T PopulateCollection();

		public void TestTVPTypeExistsAndIsATable()
		{
			var collectionToTVP = GetCollectionToTest() as IRegistryCollectionToTVP;
			var sql = $"select count(*) from sys.types where name = @Name and is_table_type = 1";
			using (var cmd = TestConnection.Command(sql))
			{
				cmd.AddParameterBasedOnDbColumn("@Name", collectionToTVP.TVPType, CargoWise.Schema.Schema.GenericStringSchemaColumn);
				int count = (int)cmd.ExecuteScalar();
				AssertEquals("One TVP type that is table", 1, count);
			}
		}

		public void TestPassingDataTable()
		{
			var collection = PopulateCollection();

			AssertGreaterThanOrEqualTo("Collection has items", collection.Count, 1);

			var collectionToTVP = collection as IRegistryCollectionToTVP;
			var dataTable = collectionToTVP.CreateDataTable();
			DataTable dataTableReturned;

			using (var cmd = TestConnection.Command("SELECT * FROM @table"))
			{
				cmd.AddTableValuedParameter("@table", $"dbo.{collectionToTVP.TVPType}", dataTable);
				dataTableReturned = DataUtils.GetDataTableFromCommand(cmd);
			}

			AssertEquals("Number of rows on Data Table same as number of items in the collection", collection.Count, dataTableReturned.Rows.Count);
		}
	}
}
