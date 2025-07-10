using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.DbUpgrader.Scripts.Definitions.Accounting;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Accounting.Testing
{
	[TestedType(typeof(OrderCustomContact1List))]
	class OrderCustomContact1ListTest : DbCreateScriptTest
	{
		public void TestSampleCallMany()
		{
			TestSampleCall("123", "345", "Abc");
		}

		public void TestSampleCallManyWithDuplicates()
		{
			TestSampleCall("123", "345", "ABC", "17", "ABC", "345");
		}

		public void TestSampleCallNone()
		{
			AssertEquals(string.Empty, TestConnection.ExecuteScalar("SELECT [Value] FROM dbo.OrderCustomContact1List('95227C02-0EBB-4AFF-8CF9-A19FDEDB949D')"));
		}

		public void TestSampleCallOne()
		{
			TestSampleCall("741");
		}

		void TestSampleCall(params string[] firstBuyers)
		{
			var arrangingDataSql =
$@"DECLARE @Values TABLE
(
	Value varchar(50)
)
INSERT INTO @Values Values {string.Join(",", firstBuyers.Select(b => "('" + b + "')"))}

DECLARE @Number int = (SELECT COUNT(1) FROM @Values)

INSERT INTO dbo.JobShipment (JS_PK) VALUES ('95227C02-0EBB-4AFF-8CF9-A19FDEDB949D')

INSERT INTO dbo.JobOrderHeader (JD_PK, JD_OA_BuyerAddress, JD_FirstBuyerContact, JD_JS) 
SELECT [Id], OA_PK, [Value], '95227C02-0EBB-4AFF-8CF9-A19FDEDB949D' FROM
(SELECT TOP (@Number) NEWID() as [Id], OA_PK, ROW_NUMBER() OVER (ORDER BY @Number) as N FROM dbo.OrgAddress) as t1
INNER JOIN
(SELECT [Value], ROW_NUMBER() OVER (ORDER BY @Number) as N FROM @Values) as t2
ON
t1.N = t2.N";
			TestConnection.ExecuteNonQuery(arrangingDataSql);

			// Assert

			var result = (string)TestConnection.ExecuteScalar("SELECT [Value] FROM dbo.OrderCustomContact1List('95227C02-0EBB-4AFF-8CF9-A19FDEDB949D')");
			AssertResultIsArrayValuesAfterComma(firstBuyers, result);
		}

		static void AssertResultIsArrayValuesAfterComma(IEnumerable<string> expectedArray, string result)
		{
			AssertArrayEqualsByElements(expectedArray.Distinct(StringComparer.InvariantCultureIgnoreCase).OrderBy(s => s).ToArray(), result.Split(new[] { ", " }, StringSplitOptions.None).OrderBy(s => s).ToArray());
		}
	}
}

