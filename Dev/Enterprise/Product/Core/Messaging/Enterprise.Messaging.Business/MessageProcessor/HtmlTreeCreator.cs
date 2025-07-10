using System.Collections.Generic;
using System.Text;

namespace Enterprise.Messaging.MessageProcessors
{
	public class HtmlTreeCreator
	{
		public class TreeNode
		{
			public string Text { get; set; }
			public List<TreeNode> Children { get; set; } = new List<TreeNode>();
		}

		public HtmlTreeCreator(TreeNode root)
		{
			this.root = root;
		}

		public TreeNode root { get; set; }

		public string ToHtml()
		{
			if (root == null)
			{
				return string.Empty;
			}

			var sb = new StringBuilder();

			sb.Append($"<ul>");
			BuildList(root, sb);
			sb.Append($"</ul>");

			return sb.ToString();
		}

		static void BuildList(TreeNode node, StringBuilder sb)
		{
			if (node == null)
			{
				return;
			}

			sb.Append($"<li>{node.Text}");

			if (node.Children.Count > 0)
			{
				sb.Append($"<ul>");
				foreach (var child in node.Children)
				{
					BuildList(child, sb);
				}
				sb.Append($"</ul>");
			}

			sb.Append($"</li>");
		}
	}
}
