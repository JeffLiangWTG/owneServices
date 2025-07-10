using System;
using CargoWise.DbUpgrader.Scripts.Definitions.Rating;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Rating.Testing
{
	[TestedType(typeof(GetRateEntryContainerClass))]
	class GetRateEntryContainerClassTest : DbCreateScriptTest
	{
		public void TestGetRateEntryContainerClass()
		{
			var pk = Guid.NewGuid();
			var sqlCommand =
				$"INSERT INTO dbo.RefContainer (RC_PK, RC_Code, RC_StorageClass, RC_HandlingRateClass, RC_FreightRateClass) VALUES ('{pk}', '40TT', '40S', '40H', '40R')";

			using (var command = TestConnection.Command(sqlCommand))
			{
				command.ExecuteNonQuery();
			}

			var query = $"SELECT ContainerClass FROM dbo.GetRateEntryContainerClass ('{pk}', 'CST')";
			using (var command = TestConnection.Command(query))
			{
				var containerClass = command.ExecuteScalar();
				AssertEquals("Expected ContainerClass value", "40S", containerClass);
			}

			query = $"SELECT ContainerClass FROM dbo.GetRateEntryContainerClass ('{pk}', 'FCL')";
			using (var command = TestConnection.Command(query))
			{
				var containerClass = command.ExecuteScalar();
				AssertEquals("Expected FreightRateClass value", "40R", containerClass);
			}

			query = $"SELECT ContainerClass FROM dbo.GetRateEntryContainerClass ('{pk}', 'ORG')";
			using (var command = TestConnection.Command(query))
			{
				var containerClass = command.ExecuteScalar();
				AssertEquals("Expected HandlingRateClass value", "40H", containerClass);
			}
		}
	}
}

