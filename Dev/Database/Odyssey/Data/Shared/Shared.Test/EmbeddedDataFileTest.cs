using NUnit.Framework;

namespace Enterprise.DbUpgrader.Data.Testing
{
	sealed class EmbeddedDataFileTest : TransactionedTestCase
	{
		public void TestDataSet()
		{
			var templateFile = new DataFileForTest(TestFileConstants.TestDataFileRelativeResourcePath, "StmMenuTemplatePivot", "StmTemplate", "StmMenuItem");
			var data = templateFile.DataSet;
			AssertEquals("Table Count", 3, data.Tables.Count);
			AssertEquals("Menu Name", "Test Doc", data.Tables["StmMenuItem"].Rows[0]["SU_MenuName"].ToString());

			AssertEquals("Template1 Name", "TestTemplate1", data.Tables["StmTemplate"].Rows[0]["SO_Name"].ToString());
			AssertEquals("Template2 Name", "TestTemplate2", data.Tables["StmTemplate"].Rows[1]["SO_Name"].ToString());
			AssertEquals("Template3 Name", "TestTemplate3", data.Tables["StmTemplate"].Rows[2]["SO_Name"].ToString());

			AssertEquals("Pivot Title", "Test Title", data.Tables["StmMenuTemplatePivot"].Rows[0]["SI_DocumentTitle"].ToString());
		}

		public void TestVersionNumbers()
		{
			var testFile = new DataFileForTest(TestFileConstants.TestDataFileRelativeResourcePath, "StmMenuTemplatePivot", "StmTemplate", "StmMenuItem");
			AssertEquals("Version in Assembly", 0, testFile.Version);

			AssertEquals("Version in Database", 0, testFile.VersionInDatabase);
			testFile.VersionInDatabase = 1;
			AssertEquals("Version in Database", 1, testFile.VersionInDatabase);
			testFile.VersionInDatabase = 0;
			AssertEquals("Version in Database", 0, testFile.VersionInDatabase);
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			DocumentTablesCleaner.Clean();
		}

		#endregion
	}
}
