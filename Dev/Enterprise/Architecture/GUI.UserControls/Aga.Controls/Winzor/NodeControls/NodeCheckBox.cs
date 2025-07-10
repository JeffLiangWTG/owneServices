using System.Windows.Forms;
using Microsoft.AspNetCore.Components;
using WinzorFramework.Extensions;

namespace Aga.Controls.Tree.NodeControls
{
	public partial class NodeCheckBox : InteractiveControl
	{
		public override MarkupString GetRenderedContent(TreeNodeAdv node, DrawContext context)
		{
			var checkState = GetCheckState(node);
			var checkedString = checkState is CheckState.Checked ? " checked" : string.Empty;
			var disabledString = !IsEditEnabled(node) ? " disabled" : string.Empty;
			var stringToRender = $"<input type=\"checkbox\"{checkedString}{disabledString}/>";
			return new MarkupString(stringToRender);
		}

		internal override string GetCssClass(TreeNodeAdv node, DrawContext context) => "treeviewadv__checkbox";
	}
}
