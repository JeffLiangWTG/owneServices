using System;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.Freight.Forwarding;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.Freight.Forwarding
{
	[TestedType(typeof(RemoveDuplicatedCSRForConsol))]
	public class RemoveDuplicatedCSRForConsolTest : DataTransformationTestCase
	{
		protected override void PrepareTestData()
		{
			var consolPK1 = Guid.NewGuid();
			var consolPK2 = Guid.NewGuid();
			var consolPK3 = Guid.NewGuid();
			var consolPK4 = Guid.NewGuid();
			var consolPK5 = Guid.NewGuid();
			var consolPK6 = Guid.NewGuid();

			var helper = new TransformationTestDataCreator();

			helper.CreateConsol(consolPK1, "C0005001");
			helper.CreateConsol(consolPK2, "C0005002");
			helper.CreateConsol(consolPK3, "C0005003");
			helper.CreateConsol(consolPK4, "C0005004");
			helper.CreateConsol(consolPK5, "C0005005");
			helper.CreateConsol(consolPK6, "C0005006");

			var consolParentTable = "JobConsol";
			var entryType = "CSR";
			var additionalCategory = "OTH";

			helper.CreateCusEntryNum(Guid.NewGuid(), consolPK1, consolParentTable, "C0005001-V1", entryType, category: additionalCategory, isSystemGenerated: true);
			helper.CreateCusEntryNum(Guid.NewGuid(), consolPK1, consolParentTable, "C0005001-V1", entryType, category: additionalCategory, isSystemGenerated: true);

			helper.CreateCusEntryNum(Guid.NewGuid(), consolPK2, consolParentTable, "C0005002-V30", entryType, category: additionalCategory, isSystemGenerated: true);
			helper.CreateCusEntryNum(Guid.NewGuid(), consolPK2, consolParentTable, "C0005002-V30", entryType, category: additionalCategory, isSystemGenerated: true);

			helper.CreateCusEntryNum(Guid.NewGuid(), consolPK3, consolParentTable, "C0005003-V100", entryType, category: additionalCategory, isSystemGenerated: true);

			helper.CreateCusEntryNum(Guid.NewGuid(), consolPK4, consolParentTable, "C0005004-V200", entryType, category: additionalCategory);
			helper.CreateCusEntryNum(Guid.NewGuid(), consolPK4, consolParentTable, "C0005004-V200", entryType, category: additionalCategory);

			helper.CreateCusEntryNum(Guid.NewGuid(), consolPK5, consolParentTable, "C0005005-V10", entryType: "CUS", category: additionalCategory);
			helper.CreateCusEntryNum(Guid.NewGuid(), consolPK5, consolParentTable, "C0005005-V10", entryType: "CUS", category: additionalCategory);

			helper.CreateCusEntryNum(Guid.NewGuid(), consolPK6, consolParentTable, "C0005006-V11", entryType: "BKG", category: additionalCategory, isSystemGenerated: true);
			helper.CreateCusEntryNum(Guid.NewGuid(), consolPK6, consolParentTable, "C0005006-V11", entryType: "BKG", category: additionalCategory, isSystemGenerated: true);
		}

		protected override void AssertTransformationResults()
		{
			foreach (var entryNum in new[]
			{
				"C0005001-V1", "C0005002-V30"
			})
			{
				AssertEquals("Duplicated rows have been removed", 1, GetEntryNumCount(entryNum));
			}

			AssertEquals("No duplicated rows", 1, GetEntryNumCount("C0005003-V100"));

			foreach (var entryNum in new[]
			{
				"C0005004-V200", "C0005005-V10", "C0005006-V11"
			})
			{
				AssertEquals("Keep unchanged", 2, GetEntryNumCount(entryNum));
			}
		}

		#region Implementation

		protected override DataTransformation GetNewTestTransformationInstance()
		{
			return new RemoveDuplicatedCSRForConsol();
		}

		int GetEntryNumCount(string entryNum)
		{
			return TestConnection.ExecuteScalar<int>($@"
SELECT Count(CE_PK)
FROM dbo.CusEntryNum
WHERE CE_EntryNum = '{entryNum}'");
		}

		#endregion
	}
}
