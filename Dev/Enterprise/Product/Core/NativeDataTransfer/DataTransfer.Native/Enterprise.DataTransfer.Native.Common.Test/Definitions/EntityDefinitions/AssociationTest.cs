using System.Collections.Generic;
using Enterprise.DataTransfer.Native.Common.Definitions.Associations;
using NUnit.Framework;

namespace Enterprise.DataTransfer.Native.Common.Definitions.EntityDefinitions
{
	public class AssociationTest : TransactionedTestCase
	{
		public void TestBuildManyToManyAssociation()
		{
			var association = (ManyToManyAssociation)AssociationDefinition
				.New(parentDefinition, childDefinition, new AssociationInfo
				{
					ThroughTable = "DummyPivot",
					ChildKey = "ZDP_ZD1",
					ParentKeys = new List<AssociationKeyInfo> { new AssociationKeyInfo { ParentKey = "ZDP_Z0" } }
				});
			AssertEquals(parentDefinition, association.To);
			AssertEquals(childDefinition, association.From);

			AssertEquals("DummyPivot", association.ParentJunctionFk.Table.Name);
			AssertEquals("DummyBizo", association.ParentJunctionFk.ReferenceTable.Name);
			AssertEquals("ZDP_Z0", association.ParentJunctionFk.Name);

			AssertEquals("DummyPivot", association.ChildJunctionFk.Table.Name);
			AssertEquals("DummyDependentBizo", association.ChildJunctionFk.ReferenceTable.Name);
			AssertEquals("ZDP_ZD1", association.ChildJunctionFk.Name);
		}

		public void TestBuildAssocaition()
		{
			association = AssociationDefinition.New(parentDefinition, childDefinition, new AssociationInfo { ParentKeys = new List<AssociationKeyInfo> { new AssociationKeyInfo { ParentKey = "ZD1_Z0" } } });

			AssertEquals("DummyDependentBizo", association.ForeignKeys[0].Table.Name);
			AssertEquals("DummyBizo", association.ForeignKeys[0].ReferenceTable.Name);
			AssertEquals("ZD1_Z0", association.ForeignKeys[0].Name);
		}

		public void TestBuildAssociation_NaturalKey()
		{
			parentDefinition = new EntityDefinition("", "OrgHeader", "", null, false);
			childDefinition = new EntityDefinition("", "DummyBizo", "", null, false);
			association = AssociationDefinition
				.New(parentDefinition, childDefinition, new AssociationInfo { Cardinality = "*", ParentKeys = new List<AssociationKeyInfo> { new AssociationKeyInfo { ParentKey = "Z0_OH_NKOrgCode" } } });
			AssertEquals("OH_Code", association.ForeignKeys[0].ReferenceColumnDef.Name);
		}

		public void TestBuildAssociation_Cardinality()
		{
			association = AssociationDefinition
				.New(parentDefinition, childDefinition, new AssociationInfo { Cardinality = "1", ParentKeys = new List<AssociationKeyInfo> { new AssociationKeyInfo { ParentKey = "ZD1_Z0" } } });

			AssertEquals(Cardinality.OneToOne, association.Cardinality);

			association = AssociationDefinition
				.New(parentDefinition, childDefinition, new AssociationInfo { Cardinality = "*", ParentKeys = new List<AssociationKeyInfo> { new AssociationKeyInfo { ParentKey = "ZD1_Z0" } } });

			AssertEquals(Cardinality.OneToMany, association.Cardinality);
		}

		public void TestBuildAssociation_CardinalityIsEmptyOrInvalid()
		{
			association = AssociationDefinition
				.New(parentDefinition, childDefinition, new AssociationInfo { ParentKeys = new List<AssociationKeyInfo> { new AssociationKeyInfo { ParentKey = "ZD1_Z0" } } });
			AssertEquals(Cardinality.OneToMany, association.Cardinality);

			association = AssociationDefinition
				.New(parentDefinition, childDefinition, new AssociationInfo { Cardinality = "hello", ParentKeys = new List<AssociationKeyInfo> { new AssociationKeyInfo { ParentKey = "ZD1_Z0" } } });
			AssertEquals(Cardinality.OneToMany, association.Cardinality);
		}

		public void TestBuildAssociation_IsExternalIsEmpty()
		{
			association = AssociationDefinition
				.New(parentDefinition, childDefinition, new AssociationInfo { ParentKeys = new List<AssociationKeyInfo> { new AssociationKeyInfo { ParentKey = "ZD1_Z0" } } });
			AssertEquals("Should be false", false, association.IsExternal);
		}

		public void TestOwnedBy()
		{
			association = AssociationDefinition
				.New(parentDefinition, childDefinition, new AssociationInfo { ParentKeys = new List<AssociationKeyInfo> { new AssociationKeyInfo { ParentKey = "ZD1_Z0" } } });
			AssertEquals(true, association.IsMateOf(parentDefinition));
			AssertEquals(true, association.IsMateOf(childDefinition));
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			TestUtil.AlterDummyTable();
			childDefinition = new EntityDefinition("", "DummyDependentBizo", "", null, false);
			parentDefinition = new EntityDefinition("", "DummyBizo", "", null, false);
		}

		#endregion

		EntityDefinition childDefinition;
		EntityDefinition parentDefinition;
		AssociationDefinition association;
	}
}
