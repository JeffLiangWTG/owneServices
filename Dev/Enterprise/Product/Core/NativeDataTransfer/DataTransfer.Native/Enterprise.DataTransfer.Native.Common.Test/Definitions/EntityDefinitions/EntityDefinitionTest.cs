using System.Collections.Generic;
using System.Linq;
using Enterprise.DataTransfer.Native.Common.Definitions.Associations;
using Enterprise.DataTransfer.Native.Common.Definitions.EntitySetDefinitions.Repository;
using Enterprise.DataTransfer.Native.DB;
using NUnit.Framework;

namespace Enterprise.DataTransfer.Native.Common.Definitions.EntityDefinitions
{
	public class EntityDefinitionTest : TransactionedTestCase
	{
		public void TestRemoveTableNameColumn()
		{
			var cusEntryNumDefinition = TestUtil.FindEntityDefinition("Declaration", "JobDeclaration.CusEntryNum");
			Assert("ParentTable column should not included in CusEntryNumber Entity", !cusEntryNumDefinition.PropertyDefinitions.Any(property => property.PropertyName.Equals("ParentTable")));
		}

		public void TestGetFullName()
		{
			var parentDefinition = TestUtil.FindEntityDefinition("Declaration", "JobDeclaration");
			var childDefinition = TestUtil.FindEntityDefinition("Declaration", "JobDeclaration.CusEntryHeader");
			var externalDefinition = TestUtil.FindEntityDefinition("Declaration", "JobDeclaration.Importer");

			AssertEquals("Root Definition should only contain its name", "JobDeclaration", parentDefinition.FullName);
			AssertEquals("Child Definition should contain its parent name + name", "JobDeclaration.CusEntryHeader", childDefinition.FullName);
			AssertEquals("External Definition should contain its child name + name", "JobDeclaration.Importer", externalDefinition.FullName);
		}

		public void TestSetEntityName()
		{
			var childDefinition = new EntityDefinition("EntityName", "TableName", "Suffix", null, false);
			AssertEquals("EntityName", childDefinition.EntityName);

			childDefinition = new EntityDefinition("", "TableName", "", null, false);
			AssertEquals("TableName", childDefinition.EntityName);

			childDefinition = new EntityDefinition("", "TableName", "Suffix", null, false);
			AssertEquals("TableName_Suffix", childDefinition.EntityName);

			childDefinition = new EntityDefinition("", "", "Suffix", null, false);
			AssertEquals("", childDefinition.EntityName);
		}

		public void TestProperties()
		{
			AssertEquals(50, parentDefinition.PropertyDefinitions.Count());
		}

		public void TestProperties_External()
		{
			parentDefinition = new EntityDefinition("", "DummyBizo", "", "", "", "","", false, false
				, true, true
				, false, false, Enumerable.Empty<string>(), Enumerable.Empty<string>(), Enumerable.Empty<string>(), Enumerable.Empty<string>(), Enumerable.Empty<string>(), Enumerable.Empty<string>(), Enumerable.Empty<string>(), null);
			AssertEquals(50, parentDefinition.PropertyDefinitions.Count());

			parentDefinition = new EntityDefinition("", "DummyBizo", "", "", "", "","", false, false
				, true, false
				, false, false, Enumerable.Empty<string>(), Enumerable.Empty<string>(), Enumerable.Empty<string>(), Enumerable.Empty<string>(), Enumerable.Empty<string>(), Enumerable.Empty<string>(), Enumerable.Empty<string>(), null);
			AssertEquals(1, parentDefinition.PropertyDefinitions.Count());
		}

		public void TestProperties_External_IncludedProperties()
		{
			parentDefinition = TestUtil.FindEntityDefinition("Order", "JobOrderHeader.LandedCostHeader.LandedCostHistory.OrgSupplierPart");
			Assert("OrgSupplierPart should always have property PartNum", parentDefinition.PropertyDefinitions.Any(p => p.PropertyName == "PartNum"));
		}

		public void TestGetParent()
		{
			TestUtil.AlterDummyTable();
			EntitySetDefinitionCache.ResetStaticCacheForTesting();
			var definition = TestUtil.FindEntityDefinition("Dummy", "DummyBizo");
			AssertNull(definition.Parent);
		}

		public void TestEquality()
		{
			TestUtil.AlterDummyTable();
			EntitySetDefinitionCache.ResetStaticCacheForTesting();
			var self = TestUtil.FindEntityDefinition("Dummy", "DummyBizo");
			var other = TestUtil.FindEntityDefinition("Dummy", "DummyBizo");
			Assert(self.Equals(other));
			Assert(self.GetHashCode() == other.GetHashCode());
			Assert(self == other);
		}

		public void TestStripCRLFWhenExcludeFromStripIsTrue()
		{
			var tableName = "DummyBizo";
			var columnName = "Z0_NVarChar";
			var table = Table.Get(tableName);
			var nvarcharColumn = table.Columns[columnName];
			CombineAssertions("Precondition: maximum length < 200, meanwhile the data type should be (n)varchar", () =>
			{
				AssertEquals("Maximu Length of current column < 200", true, nvarcharColumn.Length < 200);
				AssertEquals("The data type of current column should be (n)varchar", true, nvarcharColumn.DataType.Contains("varchar"));
			});

			var parentDefinition = new EntityDefinition("", tableName, "", "", "", "","", false, false
				, true, true
				, false, false, Enumerable.Empty<string>(), Enumerable.Empty<string>(), Enumerable.Empty<string>(), Enumerable.Empty<string>(), Enumerable.Empty<string>(), new string[] { columnName }, Enumerable.Empty<string>(), null);
			var c = parentDefinition.PropertyDefinitions;
			AssertEquals(false, parentDefinition.PropertyDefinitions[nvarcharColumn.HumanName].StripCRLF);
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			TestUtil.AddOrganizationColumnToDummy();

			childDefinition = new EntityDefinition("", "DummyDependentBizo", "", null, false);
			parentDefinition = new EntityDefinition("", "DummyBizo", "", null, false);

			childDefinition.AssociationCollection.ParentAssociations
				.Add(AssociationDefinition.New(parentDefinition, childDefinition, new AssociationInfo
				{
					ThroughTable = "DummyPivot",
					ChildKey = "ZDP_ZD1",
					ParentKeys = new List<AssociationKeyInfo> { new AssociationKeyInfo { ParentKey = "ZDP_Z0" } }
				}));
		}

		#endregion

		EntityDefinition childDefinition;
		EntityDefinition parentDefinition;
	}
}
