using System;
using System.Threading;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.Core;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.Core
{
	[TestedType(typeof(SeedUniversalClusterKeyNumberFountain))]
	public class SeedUniversalClusterKeyNumberFountainTest : DataTransformationTestCase
	{
		protected override DataTransformation GetNewTestTransformationInstance() => new SeedUniversalClusterKeyNumberFountain();

		protected override void PrepareTestData()
		{
			SetupNumberFountains(createPreviousNumberFountains: true, asycudaManifestHeaderClusterKey: 300);
		}

		protected override void AssertTransformationResults()
		{
			AssertSeedUniversalClusterKeyNumberFountainTransformation(300);
		}

		public void TestTransformationWithNoExistingClusterKeyNumberFountains()
		{
			SetupNumberFountains(createPreviousNumberFountains: false);
			GetNewTestTransformationInstance().Run(TransformationSection.OfflinePostUpgrade, CancellationToken.None);
			AssertSeedUniversalClusterKeyNumberFountainTransformation(1);
		}

		public void TestTransformationIdempotencyWithExistingClusterKeyNumberFountains()
		{
			SetupNumberFountains(createPreviousNumberFountains: true, asycudaManifestHeaderClusterKey: 200);
			GetNewTestTransformationInstance().Run(TransformationSection.OfflinePostUpgrade, CancellationToken.None);
			GetNewTestTransformationInstance().Run(TransformationSection.OfflinePostUpgrade, CancellationToken.None);
			AssertSeedUniversalClusterKeyNumberFountainTransformation(200);
		}

		public void TestTransformationIdempotencyWithNoExistingClusterKeyNumberFountains()
		{
			SetupNumberFountains(createPreviousNumberFountains: false);
			GetNewTestTransformationInstance().Run(TransformationSection.OfflinePostUpgrade, CancellationToken.None);
			GetNewTestTransformationInstance().Run(TransformationSection.OfflinePostUpgrade, CancellationToken.None);
			AssertSeedUniversalClusterKeyNumberFountainTransformation(1);
		}

		void SetupNumberFountains(
			bool createPreviousNumberFountains = true,
			int asycudaManifestHeaderClusterKey = 10,
			int cusExitHeaderClusterKey = 20,
			int customsDeclarationClusterKey = 30,
			int cusIntrastatGroupClusterKey = 5,
			int cusIntrastatHeaderClusterKey = 6,
			int cusUSLVClearanceClusterKey = 7)
		{
			var sqlRemovePreviousNumberFountains = @$"
				DELETE FROM dbo.StmNums WHERE SN_Name IN
					(
						'AsycudaManifestHeaderClusterKey',
						'CusExitHeaderClusterKey',
						'CusIntrastatGroupClusterKey',
						'CusIntrastatHeaderClusterKey',
						'CustomsDeclarationClusterKey',
						'CusUSLVClearanceClusterKey',
						'HVLVConsignmentClusterKey',
						'ClusterKeyNumber')";

			var sqlCreatePreviousNumberFountains = @$"
				INSERT INTO dbo.StmNums(SN_Name, SN_Value, SN_MinimumValue, SN_MaximumValue, SN_CanRollover, SN_Sequence, SN_SystemCreateTimeUtc)
				VALUES ('AsycudaManifestHeaderClusterKey', {asycudaManifestHeaderClusterKey}, 1, 2147483647, 0, 0, GETUTCDATE())

				INSERT INTO dbo.StmNums(SN_Name, SN_Value, SN_MinimumValue, SN_MaximumValue, SN_CanRollover, SN_Sequence, SN_SystemCreateTimeUtc)
				VALUES ('CusExitHeaderClusterKey', {cusExitHeaderClusterKey}, 1, 2147483647, 0, 0, GETUTCDATE())

				INSERT INTO dbo.StmNums(SN_Name, SN_Value, SN_MinimumValue, SN_MaximumValue, SN_CanRollover, SN_Sequence, SN_SystemCreateTimeUtc)
				VALUES ('CusIntrastatGroupClusterKey', {cusIntrastatGroupClusterKey}, 1, 2147483647, 0, 0, GETUTCDATE())

				INSERT INTO dbo.StmNums(SN_Name, SN_Value, SN_MinimumValue, SN_MaximumValue, SN_CanRollover, SN_Sequence, SN_SystemCreateTimeUtc)
				VALUES ('CusIntrastatHeaderClusterKey', {cusIntrastatHeaderClusterKey}, 1, 2147483647, 0, 0, GETUTCDATE())

				INSERT INTO dbo.StmNums(SN_Name, SN_Value, SN_MinimumValue, SN_MaximumValue, SN_CanRollover, SN_Sequence, SN_SystemCreateTimeUtc)
				VALUES ('CustomsDeclarationClusterKey', {customsDeclarationClusterKey}, 1, 2147483647, 0, 0, GETUTCDATE())

				INSERT INTO dbo.StmNums(SN_Name, SN_Value, SN_MinimumValue, SN_MaximumValue, SN_CanRollover, SN_Sequence, SN_SystemCreateTimeUtc)
				VALUES ('CusUSLVClearanceClusterKey', {cusUSLVClearanceClusterKey}, 1, 2147483647, 0, 0, GETUTCDATE())";

			var sql = sqlRemovePreviousNumberFountains + (createPreviousNumberFountains ? sqlCreatePreviousNumberFountains : string.Empty);
			TestConnection.ExecuteNonQuery(sql);
		}

		void AssertSeedUniversalClusterKeyNumberFountainTransformation(int universalClusterKeySeedValue)
		{
			AssertUniversalClusterKeyNumberFountainIsCreated(universalClusterKeySeedValue);
			AssertUnusedNumberFountainsAreRemoved();
		}

		void AssertUniversalClusterKeyNumberFountainIsCreated(int value)
		{
			CombineAssertions("Universal ClusterKey Number Fountain initialized with MAX from existing ClusterKey Number Fountains", () =>
			{
				var universalClusterKeyNumberFountain = GetNumberFountainDefinition("ClusterKeyNumber");

				AssertEquals("Owner", Guid.Empty, universalClusterKeyNumberFountain.Owner);
				AssertEquals("Value", value, universalClusterKeyNumberFountain.Value);
				AssertEquals("MinimumValue", 1, universalClusterKeyNumberFountain.MinimumValue);
				AssertEquals("MaximumValue", int.MaxValue, universalClusterKeyNumberFountain.MaximimValue);
				AssertEquals("Sequence", (short)0, universalClusterKeyNumberFountain.Sequence);
				AssertEquals("CanRollOver", false, universalClusterKeyNumberFountain.CanRollOver);
			});
		}

		void AssertUnusedNumberFountainsAreRemoved()
		{
			CombineAssertions("ClusterKey Number Fountains that were previously in use are removed", () =>
			{
				AssertNumberFountainIsRemoved("AsycudaManifestHeaderClusterKey");
				AssertNumberFountainIsRemoved("CusExitHeaderClusterKey");
				AssertNumberFountainIsRemoved("CusIntrastatGroupClusterKey");
				AssertNumberFountainIsRemoved("CusIntrastatHeaderClusterKey");
				AssertNumberFountainIsRemoved("CustomsDeclarationClusterKey");
				AssertNumberFountainIsRemoved("CusUSLVClearanceClusterKey");
			});

			void AssertNumberFountainIsRemoved(string name) => AssertNull($"{name} should be removed", GetNumberFountainDefinition(name));
		}

		NumberFountainDefinition GetNumberFountainDefinition(string name)
		{
			NumberFountainDefinition result = null;
			var count = 0;
			TestConnection.ExecuteReader(
				$"SELECT SN_Owner, SN_Value, SN_MinimumValue, SN_MaximumValue, SN_Sequence, SN_CanRollover FROM dbo.StmNums WHERE SN_Name = '{name}'",
				record =>
				{
					if (record is not null)
					{
						result = new NumberFountainDefinition
						{
							Name = name,
							Owner = (Guid)record["SN_Owner"],
							Value = (long)record["SN_Value"],
							MinimumValue = (long)record["SN_MinimumValue"],
							MaximimValue = (long)record["SN_MaximumValue"],
							Sequence = (short)record["SN_Sequence"],
							CanRollOver = (bool)record["SN_CanRollover"]
						};
					}
					count++;
				});
			Assert(count is 0 or 1);
			return result;
		}

		class NumberFountainDefinition
		{
			public string Name { get; set; }
			public Guid Owner { get; set; }
			public long Value { get; set; }
			public long MinimumValue { get; set; }
			public long MaximimValue { get; set; }
			public short Sequence { get; set; }
			public bool CanRollOver { get; set; }
		}
	}
}
