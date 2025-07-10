using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.DbUpgrader.Scripts.Definitions.Accounting;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Accounting.Testing
{
	[TestedType(typeof(OrderHeaderAttribList))]
	class OrderHeaderAttribListTest : DbCreateScriptTest
	{
		public void TestSampleCallMany()
		{
			TestSampleCall(new[] { "123", "345", "AbC" }, new[] { "8", "854", "eRRRe" });
		}

		public void TestSampleCallManyWithDuplicates()
		{
			TestSampleCall(new[] { "123", "345", "ABC", "17", "345", "ABC" }, new[] { "ASD", "34", "AA", "AA", "43", "ASD", "Jrk" });
		}

		public void TestSampleCallNone()
		{
			AssertEquals(string.Empty, TestConnection.ExecuteScalar("SELECT [Value] FROM dbo.OrderHeaderAttribList('95227C02-0EBB-4AFF-8CF9-A19FDEDB949D', 1)"));
			AssertEquals(string.Empty, TestConnection.ExecuteScalar("SELECT [Value] FROM dbo.OrderHeaderAttribList('55605386-D987-4115-8BB0-F14583E0466E', 0)"));
		}

		public void TestSampleCallOne()
		{
			TestSampleCall(new[] { "741" }, new[] { "AbC" });
		}

		void TestSampleCall(IReadOnlyCollection<string> attribList1, IReadOnlyCollection<string> attribList2)
		{
			var arrangingDataSql =
$@"DECLARE @Values TABLE
(
	Value varchar(50)
)

DECLARE @L int = {attribList1.Count}

INSERT INTO @Values Values {string.Join(",", attribList1.Concat(attribList2).Select(b => "('" + b + "')"))}

DECLARE @Number int = (SELECT COUNT(1) FROM @Values)

DECLARE @BranchPK uniqueidentifier;
DECLARE @CompanyPK uniqueidentifier;
DECLARE @CountryCode VARCHAR(2);
SELECT TOP 1 @CountryCode = GC_RN_NKCountryCode, @BranchPK = GB_PK, @CompanyPK = GB_GC FROM dbo.GlbBranch INNER JOIN dbo.GlbCompany ON GB_GC = GC_PK;

INSERT INTO dbo.JobShipment (JS_PK) VALUES ('95227C02-0EBB-4AFF-8CF9-A19FDEDB949D')
INSERT INTO dbo.JobDeclaration (JE_PK, JE_DataModel, JE_GB, JE_GC, JE_ClusterKey) VALUES ('55605386-D987-4115-8BB0-F14583E0466E', @CountryCode, @BranchPK, @CompanyPK, 1)

INSERT INTO dbo.JobOrderHeader (JD_PK, JD_OA_BuyerAddress, JD_CustomAttrib1, JD_JS, JD_JE) 
SELECT [Id], OA_PK, [Value], IIF(t1.N <= @L,'95227C02-0EBB-4AFF-8CF9-A19FDEDB949D', NULL), IIF(t1.N > @L, '55605386-D987-4115-8BB0-F14583E0466E', NULL) FROM
(SELECT TOP (@Number) NEWID() as [Id], OA_PK, ROW_NUMBER() OVER (ORDER BY @Number) as N FROM dbo.OrgAddress) as t1
INNER JOIN
(SELECT [Value], ROW_NUMBER() OVER (ORDER BY @Number) as N FROM @Values) as t2
ON
t1.N = t2.N";
			TestConnection.ExecuteNonQuery(arrangingDataSql);

			// Assert

			var result1 = (string)TestConnection.ExecuteScalar("SELECT [Value] FROM dbo.OrderHeaderAttribList('95227C02-0EBB-4AFF-8CF9-A19FDEDB949D', 1)");
			AssertResultIsArrayValuesAfterComma(attribList1, result1);

			var result2 = (string)TestConnection.ExecuteScalar("SELECT [Value] FROM dbo.OrderHeaderAttribList('55605386-D987-4115-8BB0-F14583E0466E', 0)");
			AssertResultIsArrayValuesAfterComma(attribList2, result2);
		}

		static void AssertResultIsArrayValuesAfterComma(IEnumerable<string> expectedArray, string result)
		{
			AssertArrayEqualsByElements(expectedArray.Distinct(StringComparer.InvariantCultureIgnoreCase).OrderBy(s => s).ToArray(), result.Split(new[] { ", " }, StringSplitOptions.None).OrderBy(s => s).ToArray());
		}
	}
}

