using System.Collections.Generic;
using CargoWise.Data.Testing;
using CargoWise.DbUpgrader.Scripts.Definitions.Freight.Cartage;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Freight.Cartage.Testing
{
	[TestedType(typeof(ViewLocalTransportAddresses))]
	internal class ViewLocalTransportAddressesTest : DbCreateScriptTest
	{
		// This is a regression test to assert that the query doesn't use a nonperformant query pattern
		public void TestSplitLegAddressesTableIsNotPresentInQueryPlan()
		{
			var queryPlanAnalyzer = new QueryPlanalyzer("SELECT * FROM dbo.ViewLocalTransportAddresses", TestConnection);

			var constValues = new List<string>();
			foreach (var constantScan in queryPlanAnalyzer.ConstantScans)
			{
				constValues.Add(constantScan.ConstantValue);
			}

			//This assertion is designed to check for the use of the SplitLegAddresses virtual table, which sometimes causes timeout issues on execution with certain data sets. It is a single column with 3 rows, containing the numbers 1-3.
			Assert("The constant values (1), (2) and (3) should not all be present in constant scans", !constValues.Contains("(1)") || !constValues.Contains("(2)") || !constValues.Contains("(3)"));
		}
	}
}
