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
	[TestedType(typeof(ConstraintPJ_ParentTableCode))]
	sealed class ConstraintPJ_ParentTableCodeTest : DataTransformationTestCase
	{
		public override string[] expectedIndex => new string[]
		{
			"NONCLUSTERED INDEX [_WTG__Cleanup data for new constraint on column PJ_ParentTableCode_1] ON [dbo].[StmEntityScreeningLog] ([PJ_ParentID]) INCLUDE ([PJ_SystemCreateTimeUtc], [PJ_SystemLastEditTimeUtc]) WHERE ([PJ_ParentTableCode]<>'' AND [PJ_ParentTableCode]<>'E2' AND [PJ_ParentTableCode]<>'JE' AND [PJ_ParentTableCode]<>'JK' AND [PJ_ParentTableCode]<>'JS' AND [PJ_ParentTableCode]<>'JW' AND [PJ_ParentTableCode]<>'OH' AND [PJ_ParentTableCode]<>'RN' AND [PJ_ParentTableCode]<>'RV' AND [PJ_ParentTableCode]<>'TH' AND [PJ_ParentTableCode]<>'WD') WITH (ALLOW_PAGE_LOCKS = OFF, ONLINE = ON)"
		};

		public void TestOffLinePostUpgrade()
		{
			DBTransformationTestHelper.DropConstraintIfExists("StmEntityScreeningLog", "Constraint_PJ_ParentTableCode_NoCheck");
			PrepareTestData();

			var manager = new UpgradeManagerForTestWithOutputBuffer();
			var transform = new ConstraintPJ_ParentTableCode();
			transform.Initialise(null, manager);
			transform.Run(TransformationSection.OfflinePostUpgrade, CancellationToken.None);
			AssertTransformationResults();
			transform.Run(TransformationSection.OfflinePostUpgrade, CancellationToken.None);
			AssertTransformationResults();
		}

		protected override void AssertTransformationResults()
		{
			var sqlText = "SELECT count(*) FROM dbo.StmEntityScreeningLog WHERE PJ_ParentTableCode NOT IN ('', 'E2', 'JE', 'JK', 'JS', 'JW', 'OH', 'RN', 'RV', 'TH', 'WD')";
			AssertEquals("Invalid data should be deleted", 0, Db.Connection.ExecuteScalar(sqlText));

			sqlText = "SELECT count(*) FROM dbo.StmEntityScreeningLog WHERE PJ_ParentTableCode IN ('', 'E2', 'JE', 'JK', 'JS', 'JW', 'OH', 'RN', 'RV', 'TH', 'WD')";
			AssertEquals("Valid data should be retained", 11, Db.Connection.ExecuteScalar(sqlText));
		}

		protected override void PrepareTestData()
		{
			var transformationTestDataCreator = new TransformationTestDataCreator();
			var parentID = Guid.NewGuid();
			var sourceID = Guid.NewGuid();

			transformationTestDataCreator.CreateStmEntityScreeningLog(null, "", sourceID, "OH", 1);
			transformationTestDataCreator.CreateStmEntityScreeningLog(parentID, "E2", sourceID, "OH", 2);
			transformationTestDataCreator.CreateStmEntityScreeningLog(parentID, "JE", sourceID, "OH", 3);
			transformationTestDataCreator.CreateStmEntityScreeningLog(parentID, "JK", sourceID, "OH", 4);
			transformationTestDataCreator.CreateStmEntityScreeningLog(parentID, "JS", sourceID, "OH", 5);
			transformationTestDataCreator.CreateStmEntityScreeningLog(parentID, "JW", sourceID, "OH", 6);
			transformationTestDataCreator.CreateStmEntityScreeningLog(parentID, "OH", sourceID, "OH", 7);
			transformationTestDataCreator.CreateStmEntityScreeningLog(parentID, "RN", sourceID, "OH", 8);
			transformationTestDataCreator.CreateStmEntityScreeningLog(parentID, "RV", sourceID, "OH", 9);
			transformationTestDataCreator.CreateStmEntityScreeningLog(parentID, "TH", sourceID, "OH", 10);
			transformationTestDataCreator.CreateStmEntityScreeningLog(parentID, "WD", sourceID, "OH", 11);
			transformationTestDataCreator.CreateStmEntityScreeningLog(parentID, "INV", sourceID, "OH", 12);
		}

		protected override DataTransformation GetNewTestTransformationInstance()
		{
			var manager = new UpgradeManagerForTestWithOutputBuffer();
			var transform = new ConstraintPJ_ParentTableCode();
			transform.Initialise(null, manager);
			return transform;
		}

		protected override IDisposable TransformationRunTwiceTestContextSetupAndDispose()
		{
			DBTransformationTestHelper.DropConstraintIfExists("StmEntityScreeningLog", "Constraint_PJ_ParentTableCode_NoCheck");
			return DisposableAction.NoAction;
		}
	}
}
