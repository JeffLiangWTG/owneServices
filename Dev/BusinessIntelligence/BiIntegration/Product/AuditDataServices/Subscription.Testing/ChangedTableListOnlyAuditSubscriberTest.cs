namespace Enterprise.AuditDataServices.Subscription.Testing
{
	using System.Linq;
	using CargoWise.Bi.ConfigLoader;
	using Enterprise.AuditDataServices.Subscription.Common;
	using NUnit.Framework;

	[TestsSubclassesOf(typeof(ChangedTableListOnlyAuditSubscriber))]
	public abstract class ChangedTableListOnlyAuditSubscriberTest : AuditSubscriberTest
	{
		public void TestSubscribedTablesIsNotNullOrEmpty()
		{
			AssertNotNull("SubscribedTables", TestChangedTableListSubscriber.SubscribedTables);
			AssertEquals("SubscribedTables has elements.", true, TestChangedTableListSubscriber.SubscribedTables.Any());
			AssertEquals("SubscribedTables has null elements.", false, TestChangedTableListSubscriber.SubscribedTables.Any(t => t == null));
		}

		public void TestSubscribedTablesAreAudited()
		{
			AssertNotNull("SubscribedTables", TestChangedTableListSubscriber.SubscribedTables);

			var nonAuditedTables =
					from t in TestChangedTableListSubscriber.SubscribedTables
					where
						t != null
						&& !BiAutomationConfigLoader.AuditTableExists(t.SqlSchemaName, t.TableName)
					select "[" + t.SqlSchemaName + "].[" + t.TableName + "]";

			Assert(
				"The following tables are not Audited:\r\n\r\n" + string.Join("\r\n", nonAuditedTables) + "\r\n",
				!nonAuditedTables.Any());
		}

		public void TestNoDuplicatedTables()
		{
			AssertNotNull("SubscribedTables", TestChangedTableListSubscriber.SubscribedTables);

			var duplicateTables =
				from t in TestChangedTableListSubscriber.SubscribedTables
				where t != null
				group t by new { t.SqlSchemaName, t.TableName } into grp
				where grp.Count() > 1
				select "[" + grp.Key.SqlSchemaName + "].[" + grp.Key.TableName + "]";

			Assert(
				"The following tables are duplicated:\r\n\r\n" + string.Join("\r\n", duplicateTables) + "\r\n",
				!duplicateTables.Any());
		}

		#region TestSubscriber

		ChangedTableListOnlyAuditSubscriber TestChangedTableListSubscriber => testChangedTableListSubscriber ?? (testChangedTableListSubscriber = NewChangedTableListSubscriber());
		ChangedTableListOnlyAuditSubscriber testChangedTableListSubscriber;

		protected override sealed IAuditSubscriber TestSubscriber => TestChangedTableListSubscriber;
		protected abstract ChangedTableListOnlyAuditSubscriber NewChangedTableListSubscriber();

		#endregion
	}
}
