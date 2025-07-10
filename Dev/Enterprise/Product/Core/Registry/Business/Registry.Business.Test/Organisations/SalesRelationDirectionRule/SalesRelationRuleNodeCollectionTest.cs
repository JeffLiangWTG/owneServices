using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(SalesRelationRuleNodeCollection))]
	sealed class SalesRelationRuleNodeCollectionTest : NonPersistentBusinessObjectCollectionTestCase<SalesRelationRuleNodeCollection>
	{
		public void TestFindSucceedingNode()
		{
			var collection = new SalesRelationRuleNodeCollection();
			var node1 = collection.AddNew();
			node1.Type = "AAA";
			var node2 = collection.AddNew();
			node2.Type = "BBB";
			var node3 = collection.AddNew();
			node3.Type = "CCC";

			AssertEquals(node2, collection.FindSucceedingNode(new ZString[] { "AAA" }));
			AssertEquals(node3, collection.FindSucceedingNode(new ZString[] { "AAA", "BBB" }));
			AssertEquals(null, collection.FindSucceedingNode(new ZString[] { "AAA", "BBB", "CCC" }));

			AssertEquals(null, collection.FindSucceedingNode(new ZString[] { "DDD" }));
			AssertEquals(null, collection.FindSucceedingNode(new ZString[] { "AAA", "DDD" }));
		}

		public void TestFindSucceedingNode_WithWildCards()
		{
			var collection = new SalesRelationRuleNodeCollection();
			var node1 = collection.AddNew();
			node1.Type = "AAA";
			var node2 = collection.AddNew();
			node2.Type = "?";
			var node3 = collection.AddNew();
			node3.Type = "BBB";
			var node4 = collection.AddNew();
			node4.Type = "*";

			AssertEquals(node2, collection.FindSucceedingNode(new ZString[] { "AAA" }));
			AssertEquals(node3, collection.FindSucceedingNode(new ZString[] { "AAA", "AAA" }));
			AssertEquals(node3, collection.FindSucceedingNode(new ZString[] { "AAA", "BBB" }));
			AssertEquals(node3, collection.FindSucceedingNode(new ZString[] { "AAA", "CCC" }));
			AssertEquals(node4, collection.FindSucceedingNode(new ZString[] { "AAA", "XXX", "BBB" }));

			AssertEquals(node4, collection.FindSucceedingNode(new ZString[] { "AAA", "XXX", "BBB", "AAA" }));
			AssertEquals(node4, collection.FindSucceedingNode(new ZString[] { "AAA", "XXX", "BBB", "AAA", "DDD", "EEE" }));
		}

		public void TestIsLastNode()
		{
			var collection = new SalesRelationRuleNodeCollection();
			var node1 = collection.AddNew();
			var node2 = collection.AddNew();
			var node3 = collection.AddNew();

			AssertEquals(false, collection.IsLastNode(node1));
			AssertEquals(false, collection.IsLastNode(node2));
			AssertEquals(true, collection.IsLastNode(node3));
		}

		#region Implementation

		protected override SalesRelationRuleNodeCollection GetCollectionToTest()
		{
			return new SalesRelationRuleNodeCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new SalesRelationRuleNode();
		}

		#endregion
	}
}
