using System;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.Freight.Forwarding;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.Freight.Forwarding
{
	[TestedType(typeof(RemoveDuplicatedCMRForConsolAndContainer))]
	public class RemoveDuplicatedCMRForConsolAndContainerTest : DataTransformationTestCase
	{
		protected override void PrepareTestData()
		{
			#region CMR for Consol

			var consolPK1 = Guid.NewGuid();
			var consolPK2 = Guid.NewGuid();
			var consolPK3 = Guid.NewGuid();
			var consolPK4 = Guid.NewGuid();
			var consolPK5 = Guid.NewGuid();

			var helper = new TransformationTestDataCreator();

			helper.CreateConsol(consolPK1, "C0005000");
			helper.CreateConsol(consolPK2, "C0005001");
			helper.CreateConsol(consolPK3, "C0005002");
			helper.CreateConsol(consolPK4, "C0005003");
			helper.CreateConsol(consolPK5, "C0005004");

			var consolParentTable = "JobConsol";
			var entryType = "CMR";
			var additionalCategory = "OTH";
			helper.CreateCusEntryNum(Guid.NewGuid(), consolPK1, consolParentTable, "CMR1000", entryType, category: additionalCategory, isSystemGenerated: true);
			helper.CreateCusEntryNum(Guid.NewGuid(), consolPK1, consolParentTable, "CMR1001", entryType, category: additionalCategory, isSystemGenerated: true);

			helper.CreateCusEntryNum(Guid.NewGuid(), consolPK2, consolParentTable, "CMR1002", entryType, category: additionalCategory, isSystemGenerated: true);
			helper.CreateCusEntryNum(Guid.NewGuid(), consolPK2, consolParentTable, "CMR1003", entryType, category: additionalCategory, isSystemGenerated: true);
			helper.CreateCusEntryNum(Guid.NewGuid(), consolPK2, consolParentTable, "CMR1003", entryType, category: additionalCategory, isSystemGenerated: true);

			helper.CreateCusEntryNum(Guid.NewGuid(), consolPK3, consolParentTable, "CMR1004", entryType, category: additionalCategory, isSystemGenerated: true);
			helper.CreateCusEntryNum(Guid.NewGuid(), consolPK3, consolParentTable, "CMR1004", entryType, category: additionalCategory, isSystemGenerated: true);
			helper.CreateCusEntryNum(Guid.NewGuid(), consolPK3, consolParentTable, "CMR1005", entryType, category: additionalCategory, isSystemGenerated: true);
			helper.CreateCusEntryNum(Guid.NewGuid(), consolPK3, consolParentTable, "CMR1005", entryType, category: additionalCategory, isSystemGenerated: true);

			helper.CreateCusEntryNum(Guid.NewGuid(), consolPK4, consolParentTable, "CMR1006", entryType, category: additionalCategory, isSystemGenerated: true);
			helper.CreateCusEntryNum(Guid.NewGuid(), consolPK4, consolParentTable, "CMR1006", entryType, category: additionalCategory, isSystemGenerated: true);
			helper.CreateCusEntryNum(Guid.NewGuid(), consolPK4, consolParentTable, "CMR1006", entryType, category: additionalCategory, isSystemGenerated: true);

			helper.CreateCusEntryNum(Guid.NewGuid(), consolPK5, consolParentTable, "CMR1007", entryType, category: additionalCategory);
			helper.CreateCusEntryNum(Guid.NewGuid(), consolPK5, consolParentTable, "CMR1007", entryType, category: additionalCategory);
			helper.CreateCusEntryNum(Guid.NewGuid(), consolPK5, consolParentTable, "CMR1008", entryType, category: "CUS", isSystemGenerated: true);
			helper.CreateCusEntryNum(Guid.NewGuid(), consolPK5, consolParentTable, "CMR1008", entryType, category: "CUS", isSystemGenerated: true);
			helper.CreateCusEntryNum(Guid.NewGuid(), consolPK5, consolParentTable, "BKG1009", "BKG", category: additionalCategory, isSystemGenerated: true);
			helper.CreateCusEntryNum(Guid.NewGuid(), consolPK5, consolParentTable, "BKG1009", "BKG", category: additionalCategory, isSystemGenerated: true);

			#endregion

			#region CMR for Container

			var containerPK1 = Guid.NewGuid();
			var containerPK2 = Guid.NewGuid();

			helper.CreateContainer(containerPK1, "CONT5555033", consolPK1);
			helper.CreateContainer(containerPK2, "CONT5555044", consolPK2);

			var containerParentTable = "JobContainer";
			helper.CreateCusEntryNum(Guid.NewGuid(), containerPK1, containerParentTable, "CMR2000", entryType, category: additionalCategory, isSystemGenerated: true);
			helper.CreateCusEntryNum(Guid.NewGuid(), containerPK1, containerParentTable, "CMR2001", entryType, category: additionalCategory, isSystemGenerated: true);
			helper.CreateCusEntryNum(Guid.NewGuid(), containerPK1, containerParentTable, "CMR2001", entryType, category: additionalCategory, isSystemGenerated: true);
			helper.CreateCusEntryNum(Guid.NewGuid(), containerPK2, containerParentTable, "CMR2002", entryType, category: additionalCategory, isSystemGenerated: true);
			helper.CreateCusEntryNum(Guid.NewGuid(), containerPK2, containerParentTable, "CMR2002", entryType, category: additionalCategory, isSystemGenerated: true);

			helper.CreateCusEntryNum(Guid.NewGuid(), containerPK2, containerParentTable, "CMR2003", entryType, category: additionalCategory);
			helper.CreateCusEntryNum(Guid.NewGuid(), containerPK2, containerParentTable, "CMR2003", entryType, category: additionalCategory);

			#endregion
		}

		protected override void AssertTransformationResults()
		{
			foreach (var entryNum in new[]
			{
				"CMR1000", "CMR1001", "CMR1002", "CMR1003",
				"CMR1004", "CMR1005", "CMR1006",
				"CMR2000", "CMR2001", "CMR2002"
			})
			{
				AssertEquals("Duplicated rows have been removed", 1, GetEntryNumCount(entryNum));
			}

			foreach (var entryNum in new[]
			{
				"CMR1007", "CMR1008", "BKG1009",
				"CMR2003"
			})
			{
				AssertEquals("Keep unchanged", 2, GetEntryNumCount(entryNum));
			}
		}

		#region Implementation

		protected override DataTransformation GetNewTestTransformationInstance()
		{
			return new RemoveDuplicatedCMRForConsolAndContainer();
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
