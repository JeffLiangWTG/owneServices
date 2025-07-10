using System;
using System.Data;
using System.Linq;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Telematics;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Telematics
{
	[TestedType(typeof(TelEdgeFindRoots))]
	class TelEdgeFindRootsTest : TelEdgeTestCase
	{
		public void TestResults()
		{
			// Arrange
			GenerateData();

			// Act
			var result = DataUtils.GetDataTableFromQuery(TestConnection, @"
SELECT
	RootNodeTableCode,
	RootNodeId
FROM
	dbo.TelEdgeFindRoots('TSE', '00000000-0000-0000-1111-000000000000', '2017-01-15')
ORDER BY
	RootNodeTableCode,
	RootNodeId
")
				.Rows.Cast<DataRow>()
				.Select(row => Tuple.Create(row["RootNodeTableCode"].ToString(), Guid.Parse(row["RootNodeId"].ToString())))
				.ToArray();

			// Assert
			AssertArrayEqualsByElements(new[]
			{
				Tuple.Create("RQ", new Guid("00000000-1000-1000-0000-000000000000")),
				Tuple.Create("RQ", new Guid("00000000-2000-1000-0000-000000000000")),
				Tuple.Create("RQ", new Guid("00000000-3000-1000-0000-000000000000"))
			}, result);
		}
	}
}
