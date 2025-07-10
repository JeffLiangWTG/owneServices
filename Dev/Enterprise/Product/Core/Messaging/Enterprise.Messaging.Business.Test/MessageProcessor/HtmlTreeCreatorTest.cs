using CargoWise.Types;
using NUnit.Framework;
using static Enterprise.Messaging.MessageProcessors.HtmlTreeCreator;

namespace Enterprise.Messaging.MessageProcessors.Testing
{
	public class HtmlTreeCreatorTest : TestCase
	{
		public void TestTableCreation()
		{
			var htmlTreeCreator = new HtmlTreeCreator(null);
			AssertEquals(ZString.Empty, htmlTreeCreator.ToHtml());

			var root = new TreeNode();
			htmlTreeCreator = new HtmlTreeCreator(root);
			AssertEquals("<ul><li></li></ul>", htmlTreeCreator.ToHtml());

			root.Text = "root";
			AssertEquals("<ul><li>root</li></ul>", htmlTreeCreator.ToHtml());

			var child = new TreeNode();
			root.Children.Add(child);
			AssertEquals("<ul><li>root<ul><li></li></ul></li></ul>", htmlTreeCreator.ToHtml());

			child.Text = "child";
			AssertEquals("<ul><li>root<ul><li>child</li></ul></li></ul>", htmlTreeCreator.ToHtml());

			var childChild = new TreeNode() { Text = "childChild" };
			child.Children.Add(childChild);
			AssertEquals("<ul><li>root<ul><li>child<ul><li>childChild</li></ul></li></ul></li></ul>", htmlTreeCreator.ToHtml());

			var child2 = new TreeNode() { Text = "child2" };
			root.Children.Add(child2);
			AssertEquals("<ul><li>root<ul><li>child<ul><li>childChild</li></ul></li><li>child2</li></ul></li></ul>", htmlTreeCreator.ToHtml());
		}
	}
}
