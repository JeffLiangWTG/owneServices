using System.Collections.Generic;
using System.Linq;
using CargoWise.Data;
using CargoWise.Data.Testing;
using Enterprise.DbUpgrader.Shared.Testing;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Schema.OnlineUpgrade.Testing.OnlineUpgrade
{
	sealed class OnlineBiDatabaseSynchronisationWrapperTest : TestCase
	{
		[UseSnapshotProtection(new[] { DatabaseType.Audit })]
		public void TestTransformationsOrderAudit()
		{
			using (((IDbUpgradeSupport)Db.Instance).ElevateToAdminConnectionForUpgrade())
			{
				var manager = new UpgradeManagerForTestWithOutputBuffer();

				using (var auditConnection = Db.NewAdminConnection(Db.AuditDatabaseName))
				{
					var wrapper = new OnlineAuditDatabaseSynchronisationWrapper(manager, Db.AuditDatabaseName, auditConnection);
					wrapper.Run();
				}

				var expected = new[]
				{
					"Creating template database if needed",
					"Running one-off transformations in Audit Database",
					"Create new on-line indexes",
				};
				AssertLogs(manager, expected);
			}
		}

		[UseSnapshotProtection(new [] { DatabaseType.EDW })]
		public void TestTransformationsOrderEdw()
		{
			using (((IDbUpgradeSupport)Db.Instance).ElevateToAdminConnectionForUpgrade())
			{
				var manager = new UpgradeManagerForTestWithOutputBuffer();
				using (var edwConnection = Db.NewAdminConnection(Db.EdwDatabaseName))
				{
					var wrapper = new OnlineEdwDatabaseSynchronisationWrapper(manager, Db.EdwDatabaseName, edwConnection);
					wrapper.Run();
				}

				var expected = new[]
				{
					"Creating template database if needed",
					"Running one-off transformations in Edw Database",
					"Create new on-line indexes",
				};
				AssertLogs(manager, expected);
			}
		}

		void AssertLogs(UpgradeManagerForTestWithOutputBuffer manager, string[] expected)
		{
			var actual = new List<(int Ind, string Msg)>();
			foreach (var message in expected)
			{
				var ind = manager.OutputTextCollection.IndexOf(message);
				if (ind >= 0)
				{
					actual.Add((ind, message));
				}
			}

			AssertArrayEqualsByElements(expected, actual.OrderBy(item => item.Ind).Select(i => i.Msg).ToArray());
		}
	}
}
