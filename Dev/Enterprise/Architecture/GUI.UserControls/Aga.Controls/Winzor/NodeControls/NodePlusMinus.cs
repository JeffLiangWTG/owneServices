using System.Linq;
using Microsoft.AspNetCore.Components;
using WinzorFramework.Extensions;

namespace Aga.Controls.Tree.NodeControls
{
	internal partial class NodePlusMinus : NodeControl
	{
		public override MarkupString GetRenderedContent(TreeNodeAdv node, DrawContext context)
		{
			if (node.CanExpand)
			{
				var img = node.IsExpanded ? _minus : _plus;
				var renderedString = $"<img src=\"{img.ToBase64DataUrl()}\"/>";
				return new MarkupString(renderedString);
			}
			return new MarkupString();
		}

		internal override string GetCssClass(TreeNodeAdv node, DrawContext context) => "treeviewadv__plusminus";
	}
}
