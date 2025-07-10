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

namespace Enterprise.DbUpgrader.Transformations.Test.Customs.Shared;

[TestedType(typeof(HydrateClusterKeyToCusEngine))]
public class HydrateClusterKeyToCusEngineTest : DataTransformationTestCase
{
	public void TestOnlinePreUpgrade()
	{
		TransformationRunTwiceTestContextSetupAndDispose();
		PrepareTestData();

		var manager = new UpgradeManagerForTestWithOutputBuffer();
		var transform = new HydrateClusterKeyToCusEngine();
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
		var transform = new HydrateClusterKeyToCusEngine();
		transform.Initialise(null, manager);
		transform.Run(TransformationSection.OfflinePreUpgrade, CancellationToken.None);
		AssertTransformationResults();
		transform.Run(TransformationSection.OfflinePreUpgrade, CancellationToken.None);
		AssertTransformationResults();
	}

	protected override DataTransformation GetNewTestTransformationInstance()
	{
		var manager = new UpgradeManagerForTestWithOutputBuffer();
		var transform = new HydrateClusterKeyToCusEngine();
		transform.Initialise(null, manager);
		return transform;
	}

	TransformationTestDataCreator dataCreator;
	Guid companyPK, branchPK;
	Guid ceg_pk1, ceg_pk2;

	protected override void PrepareTestData()
	{
		DBTransformationTestHelper.DropConstraintIfExists(CusEngineSchema.Constants.TableName,
			"Constraint_CEG_ClusterKey");

		var je_pk1 = Guid.NewGuid();
		dataCreator.CreateDeclaration(je_pk1, "Ref1", 31, branchPK, companyPK, dataModel: "CA", messageType: "EXP");

		var jz_pk = dataCreator.CreateJobComInvoiceHeader(branchPK, je_pk1, 31, dataModel: "CA");
		var ji_pk = dataCreator.CreateJobComInvoiceLine(jz_pk, 31, dataModel: "CA");
		ceg_pk1 = dataCreator.CreateCusEngine(ji_pk, "JI", "CA");

		var je_pk2 = Guid.NewGuid();
		dataCreator.CreateDeclaration(je_pk2, "Ref2", 99, branchPK, companyPK, dataModel: "CA", messageType: "EXP");
		var cvh_pk = dataCreator.CreateCusVehicle(je_pk2, "JE", "CA", 99);
		ceg_pk2 = dataCreator.CreateCusEngine(cvh_pk, "CVH", "CA");
	}

	protected override void AssertTransformationResults()
	{
		AssertEquals("Should have CEG_ClusterKey value of 31", 31,
			Db.Connection.ExecuteScalar($"SELECT CEG_ClusterKey FROM dbo.CusEngine WHERE CEG_PK = '{ceg_pk1}'"));
		AssertEquals("Should have CEG_ClusterKey value of 99", 99,
			Db.Connection.ExecuteScalar($"SELECT CEG_ClusterKey FROM dbo.CusEngine WHERE CEG_PK = '{ceg_pk2}'"));
	}

	protected override void SetUp()
	{
		base.SetUp();
		dataCreator = new TransformationTestDataCreator();
		companyPK = dataCreator.CreateCompany(Guid.NewGuid(), "CP1", "CA", "CAD");
		branchPK = dataCreator.CreateBranch("BR1", "BR001", companyPK);
	}
}
