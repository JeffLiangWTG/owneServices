using NUnit.Framework;

namespace Enterprise.DbUpgrader.Schema.Testing
{
	sealed class SchemaTableNamePairTest : TestCase
	{
		public void TestConstructor()
		{
			var pair = new SchemaTableNamePair("dbo", "WorkItem");
			AssertEquals("dbo", pair.SchemaName);
			AssertEquals("WorkItem", pair.TableName);
		}

		public void TestIgnoreCaseComparer()
		{
			var comparer = new SchemaTableNamePair.IgnoreCaseComparer();
			AssertEquals(true, comparer.Equals(
				new SchemaTableNamePair("dbo", "WorkItem"),
				new SchemaTableNamePair("dbo", "WorkItem")));

			AssertEquals(true, comparer.Equals(
				new SchemaTableNamePair("dbo", "WorkItem"),
				new SchemaTableNamePair("DBO", "WorkItem")));

			AssertEquals(true, comparer.Equals(
				new SchemaTableNamePair("dbo", "WorkItem"),
				new SchemaTableNamePair("dbo", "WORKITEM")));

			AssertEquals(true, comparer.Equals(
				new SchemaTableNamePair("dbo", "WorkItem"),
				new SchemaTableNamePair("DBO", "WORKITEM")));

			AssertEquals(false, comparer.Equals(
				new SchemaTableNamePair("dbo", "WorkItem"),
				new SchemaTableNamePair("dbo", "Foo")));

			AssertEquals(false, comparer.Equals(
				new SchemaTableNamePair("dbo", "WorkItem"),
				new SchemaTableNamePair("foo", "Workitem")));
		}
	}
}
