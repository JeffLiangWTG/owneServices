using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;

namespace CargoWise.Bi.Product.DataLoad.Testing.Organization
{
	public class StmDataTest : EdwEtlExecutionTest
	{
		protected override IEnumerable<string> MainDbTableList => new[] { "StmData" };

		protected string TestSDName = "ResourceStrings-TestMaxFilterLength";

		public void TestStmData()
		{
			var testStmDataRow = factory.New<StmData>();
			testStmDataRow.SD_Name = TestSDName;
			testStmDataRow.SD_BinaryValue = ZBlob.FromAscii("TestValue1");
			factory.Save();

			RunInitialLoad();

			testStmDataRow.SD_BinaryValue = ZBlob.FromAscii("TestValue2");
			factory.Save();

			RunIncrementalLoad();

			AssertIncrementalLoadDoesNotFail();
		}
	}
}
