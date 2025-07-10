using NUnit.Framework;

namespace Enterprise.DataTransfer.Native.DB.Helpers
{
	public class TableNameHelperTest : TestCase
	{
		public void TestGetTableNameAndPrefix()
		{
			AssertEquals("DummyBizo", TableNameHelper.GetTableNameFromPrefix("Z0"));
			AssertEquals("Z0", TableNameHelper.GetPrefixFromTableName("DummyBizo"));
		}
	}
}
