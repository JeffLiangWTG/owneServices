using System.IO;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;

namespace Enterprise.Accounting.Business.DataInterface.ChinaDataInterface
{
	public class ColumnDefinitionLineTest : TestCaseWithFactory
	{
		public void TestToString()
		{
			ColumnDefinitionLine lineToTest = new ColumnDefinitionLine("ABC", "123");
			AssertEquals("ABC=123", lineToTest.ToString());
		}

		public void TestToWrite()
		{
			string testFileName = Env.TempPath + "\\TEST.txt";
			FileStream testFile = new FileStream(testFileName, FileMode.Create, FileAccess.Write);
			StreamWriter testWriter = new StreamWriter(testFile);
			ColumnDefinitionLine lineToTest = new ColumnDefinitionLine("ABC", "123");
			lineToTest.Write(testWriter);
			testWriter.Close();
			testFile = new FileStream(testFileName, FileMode.Open, FileAccess.Read);
			StreamReader testReader = new StreamReader(testFile);
			string result = testReader.ReadLine();
			AssertEquals(lineToTest.ToString(), result);
			testReader.Close();
			File.Delete(testFileName);
		}
	}
}