using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.ZArchitecture.Business.Testing
{
	public abstract class ZNodeTestCase<T, U> : TestCaseWithFactory
			where T : ZNode<U>
			where U : class, IBusiness
	{
		#region Parent

		public void TestParent()
		{
			var model = GetNewTreeModelForTest();
			var bizObj1 = GetNewBizObjForTest();
			var node1 = model.CreateNewNode(bizObj1);
			AssertNull("Should not have any parent", node1.ParentNode);

			var bizObj2 = GetNewBizObjForTest();
			var bizObj2Parent = CreateParent(bizObj2);
			var node2 = model.CreateNewNode(bizObj2);
			AssertEquals("Should have parent", bizObj2Parent, node2.ParentNode.BizObj);

			AssertEquals("Precondition", false, model.HasChanges);
			node2.ParentNode = node1;
			Assert("Should have set HasChanges", model.HasChanges);
			AssertEquals("Should have set parent node", node1, node2.ParentNode);
			AssertEquals("Should have set parent bizObj", bizObj1, GetParent(bizObj2));
			AssertContainsExactElementsInAnyOrder("Should have added child node", new[] { node2 }, node1.ChildNodes);
			AssertContainsExactElementsInAnyOrder("Should have added child bizObj", new[] { bizObj2 }, GetChildren(bizObj1));

			node2.ParentNode = null;
			AssertEquals("Should have removed parent node", null, node2.ParentNode);
			AssertEquals("Should have removed parent bizObj", null, GetParent(bizObj2));
			AssertContainsExactElementsInAnyOrder("Should have removed child node", Enumerable.Empty<T>(), node1.ChildNodes);
			AssertContainsExactElementsInAnyOrder("Should have removed child bizObj", Enumerable.Empty<U>(), GetChildren(bizObj1));
		}

		public void TestParent_WithOnStructureChangedEventHandlerAccessingNodes()
		{
			var model = GetNewTreeModelForTest();
			var bizObj1 = GetNewBizObjForTest();
			var node1 = model.CreateNewNode(bizObj1);
			AssertNull("Should not have any parent", node1.ParentNode);

			var bizObj2 = GetNewBizObjForTest();
			var bizObj2Parent = CreateParent(bizObj2);
			var node2 = model.CreateNewNode(bizObj2);
			AssertEquals("Should have parent", bizObj2Parent, node2.ParentNode.BizObj);

			object temp;
			model.StructureChanged += (sender, e) =>
				{
					temp = node2.ParentNode;
					temp = node1.ChildNodes;
				};

			node2.ParentNode = node1;
			AssertEquals("Should have set parent node", node1, node2.ParentNode);
			AssertEquals("Should have set parent bizObj", bizObj1, GetParent(bizObj2));
			AssertContainsExactElementsInAnyOrder("Should have added child node", new[] { node2 }, node1.ChildNodes);
			AssertContainsExactElementsInAnyOrder("Should have added child bizObj", new[] { bizObj2 }, GetChildren(bizObj1));
		}

		#endregion

		#region Children

		public void TestChildren()
		{
			var model = GetNewTreeModelForTest();
			var bizObj = GetNewBizObjForTest();
			var node = model.CreateNewNode(bizObj);
			var child1 = CreateChild(node.BizObj);
			var child2 = CreateChild(node.BizObj);
			AssertContainsExactElementsInAnyOrder("Accessing ChildNodes for first time should create create children nodes", new[] { child1, child2 }, node.ChildNodes.Select(childNode => childNode.BizObj));

			var child3 = CreateChild(node.BizObj);
			var child4 = CreateChild(node.BizObj);

			AssertContainsExactElementsInAnyOrder("Reaccessing ChildNodes should create new children nodes", new[] { child1, child2, child3, child4 }, node.ChildNodes.Select(childNode => childNode.BizObj));
		}

		#endregion

		#region Implementation

		protected abstract ZTreeModel<U> GetNewTreeModelForTest();
		protected abstract U GetNewBizObjForTest();

		protected abstract U CreateChild(U parent);
		protected abstract IEnumerable<U> GetChildren(U parent);

		protected abstract U CreateParent(U child);
		protected abstract U GetParent(U child);

		#endregion
	}
}
