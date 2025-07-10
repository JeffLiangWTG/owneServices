using System;
using System.Threading;
using CargoWise.Common;
using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Shared.Testing;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.Customs.Shared;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.Transforms.Customs.Shared
{
	[TestedType(typeof(ConstraintPJ_SourceTableCode))]
	sealed class ConstraintPJ_SourceTableCodeTest : DataTransformationTestCase
	{
		public override string[] expectedIndex => new string[]
		{
			"NONCLUSTERED INDEX [_WTG__Cleanup data for new constraint on column PJ_SourceTableCode_1] ON [dbo].[StmEntityScreeningLog] ([PJ_ParentID]) INCLUDE ([PJ_SystemCreateTimeUtc], [PJ_SystemLastEditTimeUtc]) WHERE ([PJ_SourceTableCode]<>'' AND [PJ_SourceTableCode]<>'E2' AND [PJ_SourceTableCode]<>'JE' AND [PJ_SourceTableCode]<>'JK' AND [PJ_SourceTableCode]<>'JS' AND [PJ_SourceTableCode]<>'JW' AND [PJ_SourceTableCode]<>'OH' AND [PJ_SourceTableCode]<>'RN' AND [PJ_SourceTableCode]<>'RV' AND [PJ_SourceTableCode]<>'TH' AND [PJ_SourceTableCode]<>'WD') WITH (ALLOW_PAGE_LOCKS = OFF, ONLINE = ON)"
		};

		public void TestOffLinePostUpgrade()
		{
			DBTransformationTestHelper.DropConstraintIfExists("StmEntityScreeningLog", "Constraint_PJ_SourceTableCode_NoCheck");
			PrepareTestData();

			var manager = new UpgradeManagerForTestWithOutputBuffer();
			var transform = new ConstraintPJ_SourceTableCode();
			transform.Initialise(null, manager);
			transform.Run(TransformationSection.OfflinePostUpgrade, CancellationToken.None);
			AssertTransformationResults();
			transform.Run(TransformationSection.OfflinePostUpgrade, CancellationToken.None);
			AssertTransformationResults();
		}

		protected override void AssertTransformationResults()
		{
			var sqlText = "SELECT count(*) FROM dbo.StmEntityScreeningLog WHERE PJ_SourceTableCode NOT IN ('', 'E2', 'JE', 'JK', 'JS', 'JW', 'OH', 'RN', 'RV', 'TH', 'WD')";
			AssertEquals("Invalid data should be deleted", 0, Db.Connection.ExecuteScalar(sqlText));

			sqlText = "SELECT count(*) FROM dbo.StmEntityScreeningLog WHERE PJ_SourceTableCode IN ('', 'E2', 'JE', 'JK', 'JS', 'JW', 'OH', 'RN', 'RV', 'TH', 'WD')";
			AssertEquals("Valid data should be retained", 11, Db.Connection.ExecuteScalar(sqlText));
		}

		protected override void PrepareTestData()
		{
			var transformationTestDataCreator = new TransformationTestDataCreator();
			var parentID = Guid.NewGuid();
			var sourceID = Guid.NewGuid();

			transformationTestDataCreator.CreateStmEntityScreeningLog(parentID, "OH", null, "", 1);
			transformationTestDataCreator.CreateStmEntityScreeningLog(parentID, "OH", sourceID, "E2", 2);
			transformationTestDataCreator.CreateStmEntityScreeningLog(parentID, "OH", sourceID, "JE", 3);
			transformationTestDataCreator.CreateStmEntityScreeningLog(parentID, "OH", sourceID, "JK", 4);
			transformationTestDataCreator.CreateStmEntityScreeningLog(parentID, "OH", sourceID, "JS", 5);
			transformationTestDataCreator.CreateStmEntityScreeningLog(parentID, "OH", sourceID, "JW", 6);
			transformationTestDataCreator.CreateStmEntityScreeningLog(parentID, "OH", sourceID, "OH", 7);
			transformationTestDataCreator.CreateStmEntityScreeningLog(parentID, "OH", sourceID, "RN", 8);
			transformationTestDataCreator.CreateStmEntityScreeningLog(parentID, "OH", sourceID, "RV", 9);
			transformationTestDataCreator.CreateStmEntityScreeningLog(parentID, "OH", sourceID, "TH", 10);
			transformationTestDataCreator.CreateStmEntityScreeningLog(parentID, "OH", sourceID, "WD", 11);
			transformationTestDataCreator.CreateStmEntityScreeningLog(parentID, "OH", sourceID, "INV", 12);
		}

		protected override DataTransformation GetNewTestTransformationInstance()
		{
			var manager = new UpgradeManagerForTestWithOutputBuffer();
			var transform = new ConstraintPJ_SourceTableCode();
			transform.Initialise(null, manager);
			return transform;
		}

		protected override IDisposable TransformationRunTwiceTestContextSetupAndDispose()
		{
			DBTransformationTestHelper.DropConstraintIfExists("StmEntityScreeningLog", "Constraint_PJ_SourceTableCode_NoCheck");
			return DisposableAction.NoAction;
		}
	}
}
