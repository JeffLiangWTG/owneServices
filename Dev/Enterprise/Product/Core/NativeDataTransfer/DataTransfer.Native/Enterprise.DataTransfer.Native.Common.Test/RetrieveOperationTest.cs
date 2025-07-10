using System.Linq;
using Enterprise.DataTransfer.Native.Common.Definitions.EntityDefinitions;
using Enterprise.DataTransfer.Native.Common.Definitions.EntitySetDefinitions;
using Enterprise.DataTransfer.Native.Common.Operations;
using NUnit.Framework;

namespace Enterprise.DataTransfer.Native.Common
{
	public class RetrieveOperationTest : TransactionedTestCase
	{
		public void TestFindById_FoundEntityWithNoChild()
		{
			var pk = TestUtil.PrepareDummyBizoData();
			var definitionCollection = TestUtil.GetEntityDefinitionCollection("Dummy");
			var definition = definitionCollection.FindDefinition("DummyBizo");
			var entity = retrieveOperation.FindByInternalPK(pk, definition);
			AssertEquals("Found entity should have same primary key as input arg", pk, entity.InternalPK);
			AssertEquals("Found entity should have same definition as input arg", definition, entity.Definition);
			AssertEquals("Found entity should have same number of property as definition", definition.PropertyDefinitions.Count(), entity.Properties.Count());
		}

		public void TestFindById_FoundEntityWithChildren()
		{
			var pk = TestUtil.PrepareDummyBizoData();
			var childPk = TestUtil.PrepareDummyDependentBizoData(pk);
			TestUtil.PrepareDummyPivotData(pk, childPk);
			var definition = TestUtil.FindEntityDefinition("Dummy", "DummyBizo");
			var entity = retrieveOperation.FindByInternalPK(pk, definition);
			AssertEquals(4, entity.Children.Count());
		}

		public void TestFindByCandidateKey()
		{
			const string code = "ABC";
			var pk = TestUtil.PrepareDummyBizoData(code);
			var criteria = new EntityCriteria { PropertyName = "Code", EntityName = "DummyBizo", Value = code };
			var definition = TestUtil.FindEntityDefinition("Dummy", "DummyBizo");

			var entity = retrieveOperation.FindByCandidateKey(criteria, definition);

			AssertEquals(pk, entity.InternalPK);
		}

		public void TestFineByCriterias_CriteriaBelongsToOneTable()
		{
			const string code = "ABC";
			var pk = TestUtil.PrepareDummyBizoData(code);
			var criteria = new EntityCriteria
							{
								PropertyName = "Code",
								EntityName = "DummyBizo",
								Value = code
							};
			IEntityDefinition definition = TestUtil.FindEntityDefinition("Dummy", "DummyBizo");

			var entities = retrieveOperation.FindByCriterias(new[] { criteria }, entitySetDefinition);

			foreach (var entity in entities)
			{
				AssertEquals(pk, entity.InternalPK);
				AssertEquals("Found entity should have same definition as input arg", definition.EntityName, entity.Definition.EntityName);
				AssertEquals("Found entity should have same number of property as definition", definition.PropertyDefinitions.Count(), entity.Properties.Count());
			}
		}

		public void TestFindByCriterias_CriteriasBelongsToDifferentEntity()
		{
			const string code = "ABC";
			var pk = TestUtil.PrepareDummyBizoData(code);
			var childPk = TestUtil.PrepareDummyDependentBizoData(pk);
			var criteria = new EntityCriteria
							{
								PropertyName = "Code",
								EntityName = "DummyBizo",
								Value = code
							};
			var criteria2 = new EntityCriteria
								{
									PropertyName = "PK",
									EntityName = "DummyBizo.DummyDependentBizo",
									Value = childPk.ToString()
								};
			IEntityDefinition definition = TestUtil.FindEntityDefinition("Dummy", "DummyBizo");

			var entities = retrieveOperation.FindByCriterias(new[] { criteria, criteria2 }, entitySetDefinition);

			foreach (var entity in entities)
			{
				AssertEquals(pk, entity.InternalPK);
				AssertEquals("Found entity should have same definition as input arg", definition.EntityName, entity.Definition.EntityName);
				AssertEquals("Found entity should have same number of property as definition", definition.PropertyDefinitions.Count(), entity.Properties.Count());
				AssertEquals("Found entity should have same number of children as in db", 1, entity.Children.Count());
			}
		}

		public void TestFindByCriterias_CriteriasBelongsToExternalEntity()
		{
			const string code = "ZZZZZZ";
			var orgPk = TestUtil.PrepareOrgHeaderTableData(code);
			var pk = TestUtil.PrepareDummyBizoData("ABC", orgPk);

			var criteria = new EntityCriteria
							{
								PropertyName = "Code",
								EntityName = "DummyBizo.OrgHeader",
								Value = code
							};

			var entities = retrieveOperation.FindByCriterias(new[] { criteria }, entitySetDefinition);

			foreach (var entity in entities)
			{
				AssertEquals(pk, entity.InternalPK);
			}
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			sessionServices = new AncillaryImportServices();
			TestUtil.AlterDummyTable();
			TestUtil.PrepareOrgHeaderEntity(sessionServices);
			retrieveOperation = new RetrieveOperation(sessionServices);
			entitySetDefinition = TestUtil.GetEntitySetDefinition("Dummy");
		}

		#endregion
		AncillaryImportServices sessionServices;
		RetrieveOperation retrieveOperation;
		EntitySetDefinition entitySetDefinition;
	}
}
