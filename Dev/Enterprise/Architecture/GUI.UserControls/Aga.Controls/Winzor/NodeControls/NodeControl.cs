using System.ComponentModel;
using Microsoft.AspNetCore.Components;

namespace Aga.Controls.Tree.NodeControls
{
	public abstract partial class NodeControl : Component
	{
		public abstract MarkupString GetRenderedContent(TreeNodeAdv node, DrawContext context);

		internal virtual string GetCssClass(TreeNodeAdv node, DrawContext context) => null;
	}
}
