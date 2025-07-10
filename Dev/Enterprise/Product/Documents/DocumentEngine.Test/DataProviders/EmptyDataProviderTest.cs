using CargoWise.EntityFramework.Testing;

namespace Enterprise.DocumentEngine.DataProviders.Testing
{
	sealed class EmptyDataProviderTest : TestCaseWithFactory
	{
		public void TestBasicFunctionality()
		{
			IDataProvider dataProvider = new EmptyDataProvider();

			var rowSource = dataProvider.GetDataRowSource("Fut!");
			AssertEquals("dataProvider.GetDataRowSource(\"Fut!\")", typeof(EmptyDataSource), rowSource.GetType());

			rowSource = dataProvider.GetDataRowSource("Fuddock", true);
			AssertEquals("dataProvider.GetDataRowSource(\"Fuddock\", true)", typeof(EmptyDataSource), rowSource.GetType());

			rowSource = dataProvider.GetDataRowSource("Fuddock", true, -1);
			AssertEquals("dataProvider.GetDataRowSource(\"Fuddock\", true, -1)", typeof(EmptyDataSource), rowSource.GetType());

			AssertEquals("dataProvider.GetColumnValue(rowSource, -34, \"Balderdash\")", "", dataProvider.GetColumnValue(rowSource, -34, "Balderdash"));
			AssertEquals("dataProvider.GetColumnValue(rowSource, 240, \"Codswallop\")", "", dataProvider.GetColumnValue(rowSource, 240, "Codswallop"));
		}
	}
}
