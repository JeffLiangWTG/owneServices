using CargoWise.Types;
using Enterprise.DocumentEngine.Exceptions;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.RuntimeOptions.Testing
{
	sealed class StringTreeNodeTest : TestCase
	{
		public StringTreeNodeTest()
			: base()
		{ }

		public void TestToString()
		{
			StringTreeNode n = new StringTreeNode();
			n.Value = "boo";
			AssertEquals("<boo> ()", n.ToString());
		}

		public void TestFindChild()
		{
			StringTreeNode p, c1, c2;
			p = new StringTreeNode();
			c1 = new StringTreeNode();
			c2 = new StringTreeNode();

			c1.Value = "xy";
			c2.Value = "y";

			p.Children.Add(c1);
			p.Children.Add(c2);

			AssertEquals("Should find 'y'", c2, p.FindChild("Y"));
		}

		[ExpectNoExceptions]
		public void TestTryFindSingleChild_CanFindRightNode()
		{
			var p = new StringTreeNode();
			var c1 = new StringTreeNode();
			var c2 = new StringTreeNode();

			c1.Value = "y";
			c2.Value = "xy";

			CombineAssertions(() =>
			{
				p.Children.Add(c1);
				p.Children.Add(c2);
				AssertEquals("find one", true, p.TryFindSingleChild("Y", out var c3));
				AssertEquals("Should find first", c1, c3);
			});
		}

		[ExpectNoExceptions]
		public void TestTryFindSingleChild_CannotFindNode()
		{
			var p = new StringTreeNode();
			var c1 = new StringTreeNode();
			var c2 = new StringTreeNode();

			c1.Value = "y";
			c2.Value = "xy";

			CombineAssertions(() =>
			{
				AssertEquals("not find when no children", false, p.TryFindSingleChild("Y", out var c3));
				AssertNull("null when not no children", c3);

				p.Children.Add(c1);
				p.Children.Add(c2);
				AssertEquals("not find", false, p.TryFindSingleChild("Z", out c3));
				AssertNull("null when not no find", c3);

				var c4 = new StringTreeNode();
				c4.Value = "y";
				p.Children.Add(c4);
				AssertEquals("not find when more than one match", false, p.TryFindSingleChild("Y", out c3));
				AssertNull("null when more than one match", c3);
			});
		}

		[ExpectException(typeof(TemplateDefinitionException))]
		public void TestFindChildWithDuplicates()
		{
			StringTreeNode p, c1, c2, c3;
			p = new StringTreeNode();
			c1 = new StringTreeNode();
			c2 = new StringTreeNode();
			c3 = new StringTreeNode();

			c1.Value = "xy";
			c2.Value = "y";
			c3.Value = "y";

			p.Children.Add(c1);
			p.Children.Add(c2);
			p.Children.Add(c3);

			p.FindChild("Y");
		}

		[ExpectException(typeof(TemplateDefinitionException))]
		public void TestFindNonExistantChild()
		{
			new StringTreeNode().FindChild("boris");
		}

		public void TestChildExists()
		{
			StringTreeNode p, c1, c2;
			p = new StringTreeNode();
			c1 = new StringTreeNode();
			c2 = new StringTreeNode();

			c1.Value = "xy";
			c2.Value = "y";

			p.Children.Add(c1);
			p.Children.Add(c2);

			AssertEquals("Should find 'y'", true, p.ChildExists("Y"));
			AssertEquals("Should find 'xyz'", false, p.ChildExists("XYZ"));
		}

		[ExpectException(typeof(TemplateDefinitionException))]
		public void TestChildWith3Children()
		{
			StringTreeNode p, c1, c2, c3;
			p = new StringTreeNode();
			c1 = new StringTreeNode();
			c2 = new StringTreeNode();
			c3 = new StringTreeNode();

			p.Children.Add(c1);
			p.Children.Add(c2);
			p.Children.Add(c3);

			p.Child();
		}

		[ExpectException(typeof(TemplateDefinitionException))]
		public void TestChildWithNoChildren()
		{
			new StringTreeNode().Child();
		}

		public void TestChildWithOneChild()
		{
			StringTreeNode p, c1;
			p = new StringTreeNode();
			c1 = new StringTreeNode();

			p.Children.Add(c1);

			AssertEquals(c1, p.Child());
		}

		public void TestSerialiseAndDeserialise()
		{
			StringTreeNode parentNode, childNode1, childNode2, childNode3;
			parentNode = new StringTreeNode();
			childNode1 = new StringTreeNode();
			childNode2 = new StringTreeNode();
			childNode3 = new StringTreeNode();

			childNode1.Value = "c1";
			childNode2.Value = "c2";
			childNode3.Value = "c3";

			parentNode.Children.Add(childNode1);
			parentNode.Children.Add(childNode2);
			parentNode.Children.Add(childNode3);

			ZBlob blob = StringTreeNode.Serialise(parentNode);

			StringTreeNode reloadedNode = StringTreeNode.Deserialise(blob);
			AssertEquals("Child 1 Value", childNode1.Value, reloadedNode.Children[0].Value);
			AssertEquals("Child 2 Value", childNode2.Value, reloadedNode.Children[1].Value);
			AssertEquals("Child 3 Value", childNode3.Value, reloadedNode.Children[2].Value);
		}

		public void TestDeserialiseWithEmpty()
		{
			ZBlob blob = ZBlob.Empty;
			StringTreeNode node = StringTreeNode.Deserialise(blob);
			AssertEquals("", node.Value);
		}

		public void TestOutOfRange()
		{
			var parent = new StringTreeNode();
			var child = new StringTreeNode();
			var grandChild = new StringTreeNode();

			child.Value = "Child";
			grandChild.Value = "GrandChild";

			parent.Children.Add(child);
			child.Children.Add(grandChild);
			AssertEquals(child.Children, parent.LastCollectionAtLevel(1));

			parent.Children.Remove(child);
			AssertExceptionThrown<TemplateDefinitionException>("The filter at row 1 does not have a Display Name. Please enter a value for it.", () => parent.LastCollectionAtLevel(1));
		}
	}
}
