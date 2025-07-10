using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.Data;
using CargoWise.EntityFramework.Extensions;
using NUnit.Framework;

namespace CargoWise.EntityFramework.Testing
{
	sealed class ApplockExtensionsTests : TestCase
	{
		public void TestAppLockKeyParameterIsFixedSizeForPlan()
		{
			var factory = new BusinessObjectFactory();
			var query = new ZDBOnlyQuery(typeof(DummyBusinessObject));
			factory.LoadWithApplocks<DummyBusinessObject>("Small", query, 50);
			Assert(SqlEventTracker.Instance.LastSqlQuery.Contains("@AppLockKey = \"Small\", NVarChar(200)"));

			factory.LoadWithApplocks<DummyBusinessObject>("Not as small", query, 50);
			Assert(SqlEventTracker.Instance.LastSqlQuery.Contains("@AppLockKey = \"Not as small\", NVarChar(200)"));

			factory.LoadWithApplocks<DummyBusinessObject>("A really really long string from the point of view of an applock key", query, 50);
			Assert(SqlEventTracker.Instance.LastSqlQuery.Contains("@AppLockKey = \"A really really long string from the point of view of an applock key\", NVarChar(200)"));
		}

		public void TestNoImplicitConvertInApplockPredicate()
		{
			using (Db.Connection.TrackExecutedCommands(includeQueryPlansForExecuteReaderCommands: true))
			{
				var factory = new BusinessObjectFactory();
				var query = new ZDBOnlyQuery(typeof(DummyBusinessObject));
				factory.LoadWithApplocks<DummyBusinessObject>("Small", query, 50);
				var queryPlan = Db.Connection.ExecutedCommandsAndQueryPlans.Single(t => t.Item1.Contains(@"APPLOCK_TEST")).Item2;
				AssertEquals(1, queryPlan.Count);
				AssertEquals(0, Regex.Matches(queryPlan[0], @"CONVERT_IMPLICIT\(nvarchar\([\d]*\),CONVERT\(varchar\([\d]*\)").Count);
			}
		}
	}
}
