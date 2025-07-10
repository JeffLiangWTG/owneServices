using System;
using System.Drawing;
using System.Threading.Tasks;
using Aga.Controls.Tree;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using WinzorFramework.JSInterop;

namespace Aga.Controls.JSInterop
{
	public interface ITreeViewAdvJSInterop : IJSInterop
	{
		Task ChangeColumnWidthAsync(DotNetObjectReference<TreeViewAdv> treeViewAdv, WebMouseEventArgs args, ElementReference column);
		Task DragDropRowsAsync(DotNetObjectReference<TreeViewAdv> treeViewAdv, ElementReference rows, float topEdgeSensivity, float bottomEdgeSensivity);
		Task ReorderColumnAsync(DotNetObjectReference<TreeViewAdv> treeViewAdv, WebMouseEventArgs args, ElementReference column);
	}

	public sealed class TreeViewAdvJSInterop : JSInteropBase, ITreeViewAdvJSInterop
	{
		public TreeViewAdvJSInterop(IJSRuntimeWithMonitor jsRuntime, IFileVersionHash fileVersionHash)
			: base(jsRuntime, "/_content/Aga.Controls/js/treeviewadv.js", fileVersionHash)
		{
		}

		public async Task ChangeColumnWidthAsync(DotNetObjectReference<TreeViewAdv> treeViewAdv, WebMouseEventArgs args, ElementReference column)
		{
			await InvokeJsAsync("changeColumnWidth", treeViewAdv, args, column);
		}

		public async Task ReorderColumnAsync(DotNetObjectReference<TreeViewAdv> treeViewAdv, WebMouseEventArgs args, ElementReference column)
		{
			await InvokeJsAsync("reorderColumnHeader", treeViewAdv, args, column);
		}

		public async Task DragDropRowsAsync(DotNetObjectReference<TreeViewAdv> treeViewAdv, ElementReference rows, float topEdgeSensivity, float bottomEdgeSensivity)
		{
			await InvokeJsAsync("dragDropRows", treeViewAdv, rows, topEdgeSensivity, bottomEdgeSensivity);
		}
	}
}
