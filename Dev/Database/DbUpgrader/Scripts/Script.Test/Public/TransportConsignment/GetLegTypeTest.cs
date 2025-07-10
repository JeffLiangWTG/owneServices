using System;
using System.Collections.Generic;
using CargoWise.DbUpgrader.Scripts.Definitions.TransportConsignment;
using Enterprise.DbUpgrader.Shared;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.TransportConsignment.Testing
{
	[TestedType(typeof(GetLegType))]
	class GetLegTypeTest : DbCreateScriptTest
	{
		public static IEnumerable<Tuple<string, string, string, string>> GetTestData
		{
			get
			{
				yield return new Tuple<string, string, string, string>("LTL", "PIC", "MLT", "PICKUP");
				yield return new Tuple<string, string, string, string>("LTL", "MLT", "DLV", "DELIVERY");
				yield return new Tuple<string, string, string, string>("LTL", "MLT", "MLT", "DEPOT");
				yield return new Tuple<string, string, string, string>("LTL", "PIC", "DLV", "DIRECT");
				yield return new Tuple<string, string, string, string>("LTL", "MLT", "PIC", "OTHER");
				yield return new Tuple<string, string, string, string>("FCL", "PIC", "MLT", "EMT");
				yield return new Tuple<string, string, string, string>("FCL", "PIC", "DLV", "FCL");
				yield return new Tuple<string, string, string, string>("FCL", "MLT", "MLT", "FCL");
			}
		}
		public void TestForLegType()
		{
			var sql = new SqlQueryBuilder();
			using (TestConnection.BeginTransactionWithManager())
			{
				foreach (var item in GetTestData)
				{
					var legType = TestConnection.ExecuteScalar($"SELECT * FROM [dbo].[GetLegType]('{item.Item1}','{item.Item2}','{item.Item3}')");
					AssertEquals($"For a job Type {item.Item1} and pickup address type {item.Item1} and delivery address type {item.Item2} leg type should be", $"{item.Item4}", legType);
				}
			}
		}
	}
}

