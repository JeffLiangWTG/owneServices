using System.ComponentModel;
using NUnit.Framework;

namespace CargoWise.Windows.UI.Testing
{
	[EditorBrowsable(EditorBrowsableState.Never)]
	sealed class BindingToParentRelationWhenParentChangesWhileControlDisposedTestCase : TestCase
	{
		[ExpectNoExceptions]
		public void TestIt()
		{
			TestEntityCollection collection = new TestEntityCollection();
			TestEntity entity1 = collection.AddNew();
			TestEntity entity2 = collection.AddNew();
			TestParentEntity parent1 = entity1.AllParents.AddNew();
			TestParentEntity parent2 = entity1.AllParents.AddNew();

			entity1.Parent = parent1;
			entity2.Parent = parent1;
			using (BindingToParentRelationWhenParentChangesWhileControlDisposedForm form = new BindingToParentRelationWhenParentChangesWhileControlDisposedForm())
			{
				form.SetDataBinding(collection, "");
				form.Show();
			}
			entity1.Parent = parent1;
			entity1.Parent.BoundValue = "1";
			entity1.Parent = parent2;
			entity1.Parent.BoundValue = "2";
			collection.Remove(entity1);

			entity2.Parent = parent1;
			entity2.Parent.BoundValue = "1";
			entity2.Parent = parent2;
			entity2.Parent.BoundValue = "2";
			collection.Remove(entity2);
		}

		public class TestEntityCollection : ComponentModel.Testing.KBindingList<TestEntity>
		{
		}

		public class TestEntity : ComponentModel.Testing.KComponent
		{
			public TestParentEntity Parent { get; set; }

			public TestParentEntityCollection AllParents
			{
				get { return allParents ?? (allParents = new TestParentEntityCollection()); }
			}
			TestParentEntityCollection allParents;
		}

		public class TestParentEntityCollection : ComponentModel.Testing.KBindingList<TestParentEntity>
		{
		}

		public class TestParentEntity : ComponentModel.Testing.KComponent
		{
			public string BoundValue { get; set; }
		}
	}
}
