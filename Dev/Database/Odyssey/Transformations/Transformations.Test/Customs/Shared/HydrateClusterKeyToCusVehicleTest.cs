using System;
using System.Threading;
using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Shared.Testing;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.Customs.Shared;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.Customs.Shared
{
	[TestedType(typeof(HydrateClusterKeyToCusVehicle))]
	class HydrateClusterKeyToCusVehicleTest : DataTransformationTestCase
	{
		public void TestOnlinePreUpgrade()
		{
			TransformationRunTwiceTestContextSetupAndDispose();
			PrepareTestData();

			var manager = new UpgradeManagerForTestWithOutputBuffer();
			var transform = new HydrateClusterKeyToCusVehicle();
			transform.Initialise(null, manager);
			transform.Run(TransformationSection.OnlinePreUpgrade, CancellationToken.None);
			AssertTransformationResults();
			transform.Run(TransformationSection.OnlinePreUpgrade, CancellationToken.None);
			AssertTransformationResults();
		}

		public void TestOffLinePreUpgrade()
		{
			PrepareTestData();

			var manager = new UpgradeManagerForTestWithOutputBuffer();
			var transform = new HydrateClusterKeyToCusVehicle();
			transform.Initialise(null, manager);
			transform.Run(TransformationSection.OfflinePreUpgrade, CancellationToken.None);
			AssertTransformationResults();
			transform.Run(TransformationSection.OfflinePreUpgrade, CancellationToken.None);
			AssertTransformationResults();
		}

		protected override DataTransformation GetNewTestTransformationInstance()
		{
			var manager = new UpgradeManagerForTestWithOutputBuffer();
			var transform = new HydrateClusterKeyToCusVehicle();
			transform.Initialise(null, manager);
			return transform;
		}

		TransformationTestDataCreator dataCreator;
		Guid companyPK, branchPK;
		Guid cvh_pk1, cvh_pk2;

		protected override void PrepareTestData()
		{
			DBTransformationTestHelper.DropConstraintIfExists(CusVehicleSchema.Constants.TableName,
				"Constraint_CVH_ClusterKey");

			var je_pk1 = Guid.NewGuid();
			dataCreator.CreateDeclaration(je_pk1, "Ref1", 33, branchPK, companyPK, dataModel: "CA", messageType: "EXP");

			var jz_pk = dataCreator.CreateJobComInvoiceHeader(branchPK, je_pk1, 33, dataModel: "CA");
			var ji_pk = dataCreator.CreateJobComInvoiceLine(jz_pk, 33, dataModel: "CA");
			cvh_pk1 = dataCreator.CreateCusVehicle(ji_pk, "JI", "CA");

			var je_pk2 = Guid.NewGuid();
			dataCreator.CreateDeclaration(je_pk2, "Ref2", 66, branchPK, companyPK, dataModel: "CA", messageType: "EXP");
			cvh_pk2 = dataCreator.CreateCusVehicle(je_pk2, "JE", "CA");
		}

		protected override void AssertTransformationResults()
		{
			AssertEquals("Should have CVH_ClusterKey value of 33", 33,
				Db.Connection.ExecuteScalar($"SELECT CVH_ClusterKey FROM dbo.CusVehicle WHERE CVH_PK = '{cvh_pk1}'"));
			AssertEquals("Should have CVH_ClusterKey value of 66", 66,
				Db.Connection.ExecuteScalar($"SELECT CVH_ClusterKey FROM dbo.CusVehicle WHERE CVH_PK = '{cvh_pk2}'"));
		}

		protected override void SetUp()
		{
			base.SetUp();
			dataCreator = new TransformationTestDataCreator();
			companyPK = dataCreator.CreateCompany(Guid.NewGuid(), "CP1", "CA", "CAD");
			branchPK = dataCreator.CreateBranch("BR1", "BR001", companyPK);
		}
	}
}
