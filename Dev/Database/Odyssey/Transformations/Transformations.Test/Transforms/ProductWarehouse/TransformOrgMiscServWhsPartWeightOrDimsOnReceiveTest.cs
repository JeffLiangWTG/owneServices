using System;
using System.Linq;
using CargoWise.Database.TestFramework.ObjectModel;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Shared.Testing;
using Enterprise.DbUpgrader.Transformation.Common;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.ProductWarehouse;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.Transforms.ProductWarehouse
{
	[TestedType(typeof(TransformOrgMiscServWhsPartWeightOrDimsOnReceive))]
	class TransformOrgMiscServWhsPartWeightOrDimsOnReceiveTest : DataTransformationTestCase
	{
		protected override void PrepareTestData()
		{
			var sql = new SqlQueryBuilder();
			var client = new OrgHeader("Client").AppendInsertAndReturnObject(sql);
			var client1 = new OrgHeader("Client1").AppendInsertAndReturnObject(sql);
			var client2 = new OrgHeader("Client2").AppendInsertAndReturnObject(sql);
			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			var sql1 = new SqlQueryBuilder();
			var orgMisc = new OrgMiscServOld_V05(client) { OM_WhsPreventReceiveOfPartsWithNoWeightOrDims = true }.AppendInsertAndReturnObject(sql1);
			var orgMisc1 = new OrgMiscServOld_V05(client1) { OM_WhsPreventReceiveOfPartsWithNoWeightOrDims = false }.AppendInsertAndReturnObject(sql1);
			var orgMisc2 = new OrgMiscServOld_V05(client2) { OM_WhsPreventReceiveOfPartsWithNoWeightOrDims = true }.AppendInsertAndReturnObject(sql1);
			TestConnection.ExecuteNonQuery(sql1.ToStringWithNewLineBetweenAppends());
		}

		protected override void AssertTransformationResults()
		{
			var client = OrgHeader.ShallowLoadFromDB(TestConnection, o => o.OH_Code == "Client").Single();
			var client1 = OrgHeader.ShallowLoadFromDB(TestConnection, o => o.OH_Code == "Client1").Single();
			var client2 = OrgHeader.ShallowLoadFromDB(TestConnection, o => o.OH_Code == "Client2").Single();
			var orgMiscServ = OrgMiscServ.ShallowLoadFromDB(TestConnection, o => o.OM_OH == client.PK).Single();
			var orgMiscServ1 = OrgMiscServ.ShallowLoadFromDB(TestConnection, o => o.OM_OH == client1.PK).Single();
			var orgMiscServ2 = OrgMiscServ.ShallowLoadFromDB(TestConnection, o => o.OM_OH == client2.PK).Single();
			AssertEquals("ALL", orgMiscServ.OM_WhsCheckPartWeightOrDimsOnReceive);
			AssertEquals("NON", orgMiscServ1.OM_WhsCheckPartWeightOrDimsOnReceive);
			AssertEquals("ALL", orgMiscServ2.OM_WhsCheckPartWeightOrDimsOnReceive);
		}

		protected override DataTransformation GetNewTestTransformationInstance() => new TransformOrgMiscServWhsPartWeightOrDimsOnReceive();

		protected override void SetUp()
		{
			base.SetUp();
			DbObjectCreator.CreateColumnIfNotExists(TestConnection, OrgMiscServSchema.Constants.TableName, "OM_WhsPreventReceiveOfPartsWithNoWeightOrDims", "BIT", "0");
		}

		#region TestTransformOrgMiscServWhsPartWeightOrDimsOnReceiveBy_Index

		public override string[] expectedIndex => new string[]
		{
			"NONCLUSTERED INDEX [_WTG__Transform WhsPreventReceiveOfPartsWithNoWeightOrDims to WhsCheckPartWeightOrDimsOnReceive._1] ON [dbo].[OrgMiscServ] ([OM_WhsPreventReceiveOfPartsWithNoWeightOrDims]) WHERE ([OM_WhsPreventReceiveOfPartsWithNoWeightOrDims]=(1)) WITH (ALLOW_PAGE_LOCKS = OFF, ONLINE = ON)"
		};

		#endregion

		#region TestUserDescription

		public void TestUserDescription()
		{
			AssertEquals("Transform WhsPreventReceiveOfPartsWithNoWeightOrDims to WhsCheckPartWeightOrDimsOnReceive.", GetNewTestTransformationInstance().UserDescription);
		}

		#endregion

		#region TestCreateWhsCheckPartWeightOrDimsOnReceiveIfIsNotExist

		public void TestCreateWhsCheckPartWeightOrDimsOnReceiveIfIsNotExist()
		{
			DBTransformationTestHelper.DropConstraintIfExists(OrgMiscServSchema.Constants.TableName, "DF_OrgMiscServ_OM_WhsCheckPartWeightOrDimsOnReceive", TestConnection);
			DBTransformationTestHelper.DropConstraintIfExists(OrgMiscServSchema.Constants.TableName, "Constraint_OM_WhsCheckPartWeightOrDimsOnReceive", TestConnection);
			DBTransformationTestHelper.DropColumnIfExists(OrgMiscServSchema.Constants.TableName, OrgMiscServSchema.Constants.OM_WhsCheckPartWeightOrDimsOnReceive, TestConnection);

			PrepareTestData();

			AssertNoExceptionThrown(() => GetNewTestTransformationInstance().Run());
			AssertTransformationResults();
		}

		#endregion
	}

	class TransformOrgMiscServWhsPartWeightOrDimsOnReceiveTest_WithColumnCheck : TransactionedTestCase
	{
		public void TestIndexProviderWhenColumnNotExist()
		{
			AssertEquals(false, DbObjectCreator.ColumnExists(TestConnection, OrgMiscServSchema.Constants.TableName, "OM_WhsPreventReceiveOfPartsWithNoWeightOrDims"));

			var transformation = new TransformOrgMiscServWhsPartWeightOrDimsOnReceive();

			AssertContainsExactElementsInAnyOrder
			(
				expected: Array.Empty<string>(),
				((ITransformationIndexProvider)transformation).IndexProvider.Select(index => index.Definition)
			);
		}
	}
}
