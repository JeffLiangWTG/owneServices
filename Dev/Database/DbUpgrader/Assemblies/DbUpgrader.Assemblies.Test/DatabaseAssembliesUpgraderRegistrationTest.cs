using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Data;
using CargoWise.Data.Testing;
using Enterprise.DbUpgrader.Resource.Version;
using Enterprise.DbUpgrader.Shared;
using Moq;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Assemblies.Testing
{
	abstract class DatabaseAssembliesUpgraderRegistrationTest : TestCase
	{
		protected abstract string[] ExpectedAssemblies { get; }

		protected DatabaseAssembliesUpgrader AssembliesUpgrader { get; private set; }

		public void TestAssembliesAreRegistered()
		{
			// Arrange

			// Act
			var result = AssembliesUpgrader.RegisteredClrObjects
				.GroupBy(info => info.AssemblyName)
				.Select(infos => infos.Key)
				.ToList();

			// Assert
			AssertEquals(ExpectedAssemblies.Length, result.Count);
			foreach (var assembly in ExpectedAssemblies)
			{
				AssertCollectionContains(assembly, result);
			}
		}

		public void TestIsUpgradeRequired()
		{
			// Arrange
			using (Db.DisposableActionForDbConnection())
			{
				var ugrader = CreateAssembliesUpgrader(Mock.Of<IUpgradeManager>(), Db.Connection, SqlClrAssembliesVersion.Application);

				// Act
				var isUpgradeRequired = ugrader.IsUpgradeRequired;

				// Assert
				CombineAssertions(() =>
				{
					Assert(!isUpgradeRequired);
					AssertNotNull(ugrader.DbAssemblies);
					var notLoadedAssembliesInfo = ugrader
						.DbAssemblies
						.Values
						.Where(assembly => !assembly.ExistsInDatabase || assembly.Any(proc => !proc.Value.ExistsInDatabase))
						.Select(assembly => $"{assembly.Name}, {assembly.ExistsInDatabase}, {string.Join("|", assembly.Where(proc => !proc.Value.ExistsInDatabase).Select(p => p.Key.AssemblyName))}");
					Assert(string.Join("\r\n", notLoadedAssembliesInfo), !notLoadedAssembliesInfo.Any());
				});
			}
		}

		protected void FunctionsAreRegistered(string assemblyName, IReadOnlyCollection<Tuple<string, string>> expectedFunctions)
		{
			// Arrange

			// Act
			var result = AssembliesUpgrader.RegisteredClrObjects.Where(info => info.AssemblyName == assemblyName)
				.Select(info => new Tuple<string, string>(info.ClassFullName, info.MemberName))
				.ToList();

			// Assert
			AssertEquals(expectedFunctions.Count, result.Count);
			foreach (var tuple in expectedFunctions)
			{
				AssertCollectionContains(tuple, result);
			}
		}

		protected abstract DatabaseAssembliesUpgrader CreateAssembliesUpgrader(IUpgradeManager upgradeManager, DbConnection dbConnection, VersionLabel versionLabel);

		protected override void SetUp()
		{
			base.SetUp();
			upgradeManagerMock = new Mock<IUpgradeManager>();
			AssembliesUpgrader = CreateAssembliesUpgrader(upgradeManagerMock.Object, new NullDbConnection(), new VersionLabel(0, 0));
		}

		Mock<IUpgradeManager> upgradeManagerMock;
	}
}
