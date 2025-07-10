using System.Collections.Generic;
using Enterprise.DataTransfer.Native.Common.Definitions.EntityDefinitions;
using NUnit.Framework;

namespace Enterprise.DataTransfer.Native.Common.Definitions.Associations
{
	class AssociationCollectionTest : TransactionedTestCase
	{
		public void TestGetMainAssociation()
		{
			collection = new AssociationCollection();
			collection.ChildAssociations.Add(association);
			var mainAssociation = collection.MainAssociation;
			AssertNull(mainAssociation);

			collection = new AssociationCollection();
			collection.MainAssociation = association;
			collection.ParentAssociations.Add(association);
			mainAssociation = collection.MainAssociation;
			AssertNotNull(mainAssociation);
		}

		public void TestGetChildAssociations()
		{
			collection = new AssociationCollection();
			collection.ChildAssociations.Add(association);
			var childAssociation = collection.ChildAssociations;

			AssertEquals(1, childAssociation.Count);

			collection = new AssociationCollection();
			childAssociation = collection.ChildAssociations;
			AssertEquals(0, childAssociation.Count);
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			childDefinition = new EntityDefinition("", "DummyDependentBizo", "", null, false);
			parentDefinition = new EntityDefinition("", "DummyBizo", "", null, false);
			association = AssociationDefinition.New(parentDefinition, childDefinition, new AssociationInfo { ParentKeys = new List<AssociationKeyInfo> { new AssociationKeyInfo { ParentKey = "ZD1_Z0" } } });
		}

		#endregion

		EntityDefinition childDefinition;
		EntityDefinition parentDefinition;
		AssociationCollection collection;
		AssociationDefinition association;
	}
}
