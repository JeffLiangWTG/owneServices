using CargoWise.Types;
using Enterprise.DocumentEngine.DataProviders.Testing;
using static Enterprise.Accounting.ReportTableProviders.GLAccountDocumentDataProvider;

namespace Enterprise.Accounting.ReportTableProviders.Testing
{
	public class GLAccountDocumentDataProviderTableTest : ParameterisedTableProviderTest
	{
		#region TestOverriding

		public void TestOverriding()
		{
			AssertEquals("Initial GLAccountDocumentDataProvider", typeof(GLAccountDocumentDataProvider), GLAccountDocumentDataProvider.New().GetType());

			ClientGLAccountDocumentDataProvider_ForTestOnly.Register();
			AssertEquals("Should return the overridden GLAccountDocumentDataProvider", typeof(ClientGLAccountDocumentDataProvider_ForTestOnly), GLAccountDocumentDataProvider.New().GetType());

			AssertEquals("property should overriden and equal 1000000", 1000000, GLAccountDocumentDataProvider.New().MaximumRowsBeforeException_ForTestOnly);
		}

		#endregion

		public void TestThrowDocEngineExceptionWhenDataSetTooBig()
		{
			GLAccountDocumentDataProvider testProvider = GLAccountDocumentDataProvider.New();
			GLAccountListDocumentDataSet data = new GLAccountListDocumentDataSet();
			testProvider.FillDataSet(data, ZDateTime.Now.ToDateTime(), ZDateTime.Now.ToDateTime());

			GLAccountListDocumentDataSet.GLAccountListDataSetDataTable dataTable = (GLAccountListDocumentDataSet.GLAccountListDataSetDataTable)data.Tables[0];
			dataTable.MinimumCapacity = testProvider.MaximumRowsBeforeException_ForTestOnly + 2;

			AssertEquals("Should be no rows in the dataset", 0, dataTable.Rows.Count);

			for (int index = 0; index <= testProvider.MaximumRowsBeforeException_ForTestOnly; index++)
			{
				GLAccountListDocumentDataSet.GLAccountListDataSetRow row = dataTable.NewGLAccountListDataSetRow();
				row.GLAccountDesc = "BLAH";
				data.Tables[0].Rows.Add(row);
			}

			AssertEquals("Dataset row count", testProvider.MaximumRowsBeforeException_ForTestOnly + 1, dataTable.Rows.Count);

			try
			{
				testProvider.AddRow_ForTestOnly((GLAccountListDocumentDataSet.GLAccountListDataSetRow)dataTable.Rows[0], data);
				Assert("Should have thrown exception and not reached here", false);
			}
			catch (TooManyRowsException)
			{
				Assert("Should have thrown exception in here", true);
			}
		}

		public void TestFindBeginningPeriodForTheAutoJournal()
		{
			GLAccountDocumentDataProvider testProvider = GLAccountDocumentDataProvider.New();
			testProvider.PeriodRange_ForTestOnly = new int[] { 200401, 200402, 200403, 200404, 200405 };

			AssertEquals(2, testProvider.FindBeginningPeriodOffsetForTheAutoJournal_ForTestOnly(200403));
			AssertEquals(-1, testProvider.FindBeginningPeriodOffsetForTheAutoJournal_ForTestOnly(200408));
			AssertEquals(0, testProvider.FindBeginningPeriodOffsetForTheAutoJournal_ForTestOnly(200309));
		}
	}
}
