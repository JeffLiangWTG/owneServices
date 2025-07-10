using System.Drawing;
using System.Net;
using System.Web;
using Aga.Controls.Tree;
using Microsoft.AspNetCore.Components;
using WinzorFramework.Extensions;

namespace Aga.Controls.Tree.NodeControls
{
	public abstract partial class BaseTextControl : EditableControl
	{
		public override MarkupString GetRenderedContent(TreeNodeAdv node, DrawContext context)
		{
			if (context.CurrentEditorOwner == this && node == Parent.CurrentNode)
			{
				return new MarkupString();
			}

			var label = GetLabel(node);

			Brush backgroundBrush;
			Color textColor;
			Font font;
			CreateBrushes(node, context, label, out backgroundBrush, out textColor, out font, ref label);
			font = GetDrawingFont(node, context, label);

			var treeFont = node.Tree.Font;
			var fontStyle = string.Empty;

			if (font.Size != treeFont.Size)
			{
				fontStyle += $"font-size:{font.Size}pt;";
			}

			if (font.Bold && !treeFont.Bold)
			{
				fontStyle += "font-weight:bold;";
			}

			if (font.Italic && !treeFont.Italic)
			{
				fontStyle += "font-style:italic;";
			}

			var colorStyle = $"color:{textColor.GetColorStyleValue()};";
			var backgroundColor = (backgroundBrush as SolidBrush)?.Color.GetColorStyleValue();
			var backgroundColorStyle = backgroundColor != null ? $"background-color:{backgroundColor};" : string.Empty;
			var boundsStyle = $"height:{node.Height ?? node.Tree.RowHeight}px;";
			var outlineColor = context.DrawSelection == DrawSelectionMode.None ? SystemColors.ControlText : SystemColors.InactiveCaption;
			var outlineStyle = context.DrawFocus ? $"outline:{outlineColor.GetColorStyleValue()} dotted 1px;outline-offset:-1px;" : string.Empty;
			var stringToRender = $"<p style=\"{fontStyle}{colorStyle}{backgroundColorStyle}{boundsStyle}{outlineStyle}\">{WebUtility.HtmlEncode(label)}</p>";
			return new MarkupString(stringToRender);
		}
	}
}
