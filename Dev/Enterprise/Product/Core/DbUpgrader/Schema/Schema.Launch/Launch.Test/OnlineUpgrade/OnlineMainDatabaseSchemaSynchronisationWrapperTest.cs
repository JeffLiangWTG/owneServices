using System.Collections.Generic;
using System.Linq;
using CargoWise.Data;
using CargoWise.Data.Testing;
using Enterprise.DbUpgrader.Shared.Testing;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Schema.OnlineUpgrade
{
	sealed class OnlineMainDatabaseSchemaSynchronisationWrapperTest : TestCase
	{
		[UseSnapshotProtection]
		public void TestTransformationsOrder()
		{
			using (((IDbUpgradeSupport)Db.Instance).ElevateToAdminConnectionForUpgrade())
			{
				var manager = new UpgradeManagerForTestWithOutputBuffer();
				var wrapper = new OnlineMainDatabaseSchemaSynchronisationWrapper(manager, Db.DatabaseName, Db.Connection);

				wrapper.Run();

				var expected = new[]
				{
					"Creating template database if needed",
					"Running prior transformations",
					"Running table pre-synchronisation",
					"Running one-off transformations",
					"Running auto transformations",
					"Creating new columns",
					"Create new on-line indexes",
				};

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
}
