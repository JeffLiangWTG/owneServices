using System;
using System.Data;
using System.Linq;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Telematics;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Telematics
{
	[TestedType(typeof(TelEdgeGrowTree))]
	class TelEdgeGrowTreeTest : TelEdgeTestCase
	{
		public void TestResults()
		{
			// Arrange
			GenerateData();

			// Act
			var result = DataUtils.GetDataTableFromQuery(TestConnection, @"
SELECT
	TE_PK,
	TE_EntityTableCodeFrom,
	TE_EntityIdFrom,
	TE_EntityTableCodeTo,
	TE_EntityIdTo,
	TE_StartTime,
	TE_EndTime,
	TE_TemplateMapping,
	Depth
FROM
	dbo.TelEdgeGrowTree('RQ', '00000000-1000-1000-0000-000000000000', '2017-01-15')
ORDER BY
	Depth
")
				.Rows.Cast<DataRow>()
				.Select(row => Tuple.Create(row["TE_EntityTableCodeFrom"].ToString(), Guid.Parse(row["TE_EntityIdFrom"].ToString()), row["TE_EntityTableCodeTo"].ToString(), Guid.Parse(row["TE_EntityIdTo"].ToString()), int.Parse(row["Depth"].ToString())))
				.ToArray();

			// Assert
			AssertArrayEqualsByElements(new[]
			{
				Tuple.Create("RQ", new Guid("00000000-1000-1000-0000-000000000000"), "TSE", new Guid("00000000-0000-0000-1111-000000000000"), 0),
				Tuple.Create("TSE", new Guid("00000000-0000-0000-1111-000000000000"), "TSE", new Guid("00000000-0000-A000-0000-000000000000"), 1),
				Tuple.Create("TSE", new Guid("00000000-0000-0000-1111-000000000000"), "TSE", new Guid("00000000-0000-B000-0000-000000000000"), 1),
				Tuple.Create("TSE", new Guid("00000000-0000-B000-0000-000000000000"), "TSE", new Guid("00000000-0000-C000-0000-000000000000"), 2)
			}, result);
		}
	}
}
