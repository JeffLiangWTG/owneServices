using System;
// using System.Linq;
using System.Text;
using CargoWise.Database.TestFramework.ObjectModel;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Shared.Testing;
using Enterprise.DbUpgrader.Transformation.Common;
using Enterprise.DbUpgrader.Transformation.DataModification.Public.Registry.Testing;
using Enterprise.DbUpgrader.Transformations.ProductWarehouse;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.ProductWarehouse
{
	[TestedType(typeof(RemoveAllowPickFinalizationWithUnpackedTotes))]
	class RemoveAllowPickFinalizationWithUnpackedTotesTest : DeleteRegistryItemTest
	{
		protected override string[] GetRegistryItemNames() => new[] { AllowPickFinalizationWithUnpackedTotesRegistryName };

		protected override void PrepareTestData()
		{
			RegistryHelper.InsertStmDataRow(AllowPickFinalizationWithUnpackedTotesRegistryName, "BOL", Encoding.Unicode.GetBytes(bool.FalseString));
		}

		public void TestTranformation_WithWhsClientPickPackParamsByWhsRecord_NoRegistryRecordInDatabase()
		{
			var sql = new SqlQueryBuilder();
			var branch = new GlbBranch("BR1").AppendInsertAndReturnObject(sql);
			var client = new OrgHeader("Client").AppendInsertAndReturnObject(sql);
			var warehouse = new WhsWarehouse("WH1", "PRW", branchPK: branch.PK).WithDockDoor(sql);
			var pickPackParams = new WhsClientPickPackParamsByWhs(client, warehouse).AppendInsertAndReturnObject(sql);
			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			
			GetNewTestTransformationInstance().Run();

			var pickPackParamsLoadedFromDb = WhsClientPickPackParamsByWhs.ShallowLoadFromDB(TestConnection, pickPackParams.PK);
			pickPackParamsLoadedFromDb.BuildAssertion(TestConnection)
				.ExpectEquals("WPP_AllowPickFinalizationWithUnpackedTotes is set to false", p => p.WPP_AllowPickFinalizationWithUnpackedTotes, false)
				.VerifyAll();
			AssertTransformationResults();
		}

		public void TestTranformation_WithWhsClientPickPackParamsByWhsRecord_RegistryRecordInDatabase_FalseValue()
		{
			TestTranformation_WithWhsClientPickPackParamsByWhsRecord_RegistryRecordInDatabaseCore(false);
		}

		public void TestTranformation_WithWhsClientPickPackParamsByWhsRecord_RegistryRecordInDatabase_TrueValue()
		{
			TestTranformation_WithWhsClientPickPackParamsByWhsRecord_RegistryRecordInDatabaseCore(true);
		}

		void TestTranformation_WithWhsClientPickPackParamsByWhsRecord_RegistryRecordInDatabaseCore(bool registryValue)
		{
			var sql = new SqlQueryBuilder();
			var branch = new GlbBranch("BR1").AppendInsertAndReturnObject(sql);
			var client = new OrgHeader("Client").AppendInsertAndReturnObject(sql);
			var warehouse = new WhsWarehouse("WH1", "PRW", branchPK: branch.PK).WithDockDoor(sql);
			var pickPackParams = new WhsClientPickPackParamsByWhs(client, warehouse).AppendInsertAndReturnObject(sql);
			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			RegistryHelper.InsertStmDataRow(AllowPickFinalizationWithUnpackedTotesRegistryName, Guid.Empty, Guid.Empty, "BOL", Encoding.Unicode.GetBytes(registryValue ? bool.TrueString : bool.FalseString));
			GetNewTestTransformationInstance().Run();

			var pickPackParamsLoadedFromDb = WhsClientPickPackParamsByWhs.ShallowLoadFromDB(TestConnection, pickPackParams.PK);
			pickPackParamsLoadedFromDb.BuildAssertion(TestConnection)
				.ExpectEquals("WPP_AllowPickFinalizationWithUnpackedTotes is set to false", p => p.WPP_AllowPickFinalizationWithUnpackedTotes, registryValue)
				.VerifyAll();
			AssertTransformationResults();
		}

		public void TestTranformation_WithWhsClientPickPackParamsByWhsRecord_RegistryRecordInDatabase_NullValue()
		{
			var sql = new SqlQueryBuilder();
			var branch = new GlbBranch("BR1").AppendInsertAndReturnObject(sql);
			var client = new OrgHeader("Client").AppendInsertAndReturnObject(sql);
			var warehouse = new WhsWarehouse("WH1", "PRW", branchPK: branch.PK).WithDockDoor(sql);
			var pickPackParams = new WhsClientPickPackParamsByWhs(client, warehouse).AppendInsertAndReturnObject(sql);
			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			RegistryHelper.InsertStmDataRow(AllowPickFinalizationWithUnpackedTotesRegistryName, Guid.Empty, Guid.Empty, "BOL", StmDataSchema.SD_BinaryValue, null);
			GetNewTestTransformationInstance().Run();

			var pickPackParamsLoadedFromDb = WhsClientPickPackParamsByWhs.ShallowLoadFromDB(TestConnection, pickPackParams.PK);
			pickPackParamsLoadedFromDb.BuildAssertion(TestConnection)
				.ExpectEquals("WPP_AllowPickFinalizationWithUnpackedTotes is set to false", p => p.WPP_AllowPickFinalizationWithUnpackedTotes, false)
				.VerifyAll();
			AssertTransformationResults();
		}

		readonly RegistryTransformationHelper RegistryHelper = new RegistryTransformationHelper();

		const string AllowPickFinalizationWithUnpackedTotesRegistryName = "AllowPickFinalizationWithUnpackedTotes";
	}
}
