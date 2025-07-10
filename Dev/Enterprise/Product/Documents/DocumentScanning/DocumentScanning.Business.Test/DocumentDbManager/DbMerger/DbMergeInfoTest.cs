using NUnit.Framework;

namespace Enterprise.DocumentScanning.Business.Test
{
	class DbMergeInfoTest : TestCase
	{
		public void TestProperties()
		{
			DbMergeInfo mergeInfo = new DbMergeInfo(1, "TestDb", 20, false);
			AssertEquals("DB number", 1, mergeInfo.Number);
			AssertEquals("DB name", "TestDb", mergeInfo.Name);
			AssertEquals("DB size", 20, mergeInfo.Size);
			AssertEquals("DB is read-only", false, mergeInfo.IsReadOnly);
		}
	}
}
