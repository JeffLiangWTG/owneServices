using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Aga.Controls.JSInterop;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using WinzorFramework;
using WinzorFramework.Extensions;

namespace Aga.Controls.Tree
{
	public partial class TreeViewAdv
	{
		readonly List<TreeViewAdvColumnHeaderData> TreeViewAdvColumnHeadersToRender = new();
		readonly List<TreeViewAdvRowData> TreeViewAdvRowsToRender = new();

		public override bool UseParentDivForLayout => false;

		protected override void OnSizeChanged(EventArgs e)
		{
			base.OnSizeChanged(e);
			UpdateView();
		}

		void CreateColumnHeaders()
		{
			TreeViewAdvColumnHeadersToRender.Clear();

			if (UseColumns)
			{
				var x = 0;
				var i = 0;
				foreach (TreeColumn column in Columns)
				{
					if (column.IsVisible)
					{
						var columnX = x;
						var index = i++;
						TreeViewAdvColumnHeadersToRender.Add(new TreeViewAdvColumnHeaderData()
						{
							Text = column.Header,
							SortOrder = column.SortOrder,
							Bounds = new Rectangle(x, 0, column.Width, ColumnHeaderHeight - 1),
							ColumnIndex = column.Index,
							MinColumnWidth = column.MinColumnWidth,
							MaxColumnWidth = column.MaxColumnWidth,
							OnMouseDown = EventCallback.Factory.Create<WebMouseEventArgs>(this, e => OnMouseDownHeaderAsync(e, columnX + (int)e.OffsetX, (int)e.OffsetY, index)),
							OnMouseUp = EventCallback.Factory.Create<WebMouseEventArgs>(this, e => OnMouseUpAsync(e, columnX + (int)e.OffsetX, (int)e.OffsetY))
						});
						x += column.Width;
					}
				}
			}

			NotifyRenderRequired();
		}

		void CreateRows()
		{
			TreeViewAdvRowsToRender.Clear();

			foreach (var node in Root.Children)
			{
				TreeViewAdvRowsToRender.Add(CreateRowFor(node));
			}

			NotifyRenderRequired();
		}

		TreeViewAdvRowData CreateRowFor(TreeNodeAdv node)
		{
			var controls = new List<TreeViewAdvControlData>();
			var yOffset = UseColumns ? ColumnHeaderHeight : 0;
			var context = GetDrawContext(node);
			var rowRect = _rowLayout.GetRowBounds(node.Row);

			foreach (NodeControlInfo item in GetNodeControls(node))
			{
				var bounds = item.Bounds;

				context.Bounds = bounds;

				controls.Add(new TreeViewAdvControlData
				{
					CssClass = item.Control.GetCssClass(node, context),
					ControlContent = item.Control.GetRenderedContent(node, context),
					Bounds = bounds,
					LeftMargin = item.Control.LeftMargin,
					ParentColumnIndex = item.Control.ParentColumn?.Index,
					OnMouseUp = EventCallback.Factory.Create<WebMouseEventArgs>(this, e => OnMouseUpAsync(e, bounds.X + (int)e.OffsetX, (int)e.OffsetY + rowRect.Y)),
					OnMouseDown = EventCallback.Factory.Create<WebMouseEventArgs>(this, e => InvokeWinzorDispatcherAsync(() => OnMouseDown(new MouseEventArgs(e.GetMouseButtons(), 1, bounds.X + (int)e.OffsetX, (int)e.OffsetY + rowRect.Y, 0)))),
					OnClick = EventCallback.Factory.Create<WebMouseEventArgs>(this, e => InvokeWinzorDispatcherAsync(() => OnMouseClick(new MouseEventArgs(e.GetMouseButtons(), 1, bounds.X + (int)e.OffsetX, (int)e.OffsetY + rowRect.Y, 0)))),
					OnDblClick = EventCallback.Factory.Create<WebMouseEventArgs>(this, e => InvokeWinzorDispatcherAsync(() => OnMouseDoubleClick(new MouseEventArgs(e.GetMouseButtons(), 2, bounds.X + (int)e.OffsetX, (int)e.OffsetY + rowRect.Y, 0))))
				});
			}

			rowRect.Y += yOffset;
			rowRect.Width = ContentWidth;

			OnRowDraw(new PaintEventArgs(this), node, context, node.Row, rowRect);

			var rowData = new TreeViewAdvRowData()
			{
				Rows = new(),
				Controls = controls,
				Bounds = rowRect,
				IsSelected = node.IsSelected,
				CanExpand = node.CanExpand,
				IsExpanded = node.IsExpanded,
				BackgroundColor = ((node.RowBackgroundBrush as SolidBrush)?.Color ?? BackColor).GetColorStyleValue(),
				DropPositionCursors = string.Join(",", node.DropPositionEffects.Select(e => CursorString(e.Value))),
				IsMasterActivitySalesRelation = node.IsMasterActivitySalesRelation,
				TreeAllowDrop = AllowDrop,
			};

			foreach (var child in node.Children)
			{
				rowData.Rows.Add(CreateRowFor(child));
			}

			return rowData;
		}

		string CursorString(DragDropEffects? effects) => effects switch
		{
			DragDropEffects.None => "not-allowed",
			DragDropEffects.Move => "move",
			_ => string.Empty
		};

		protected async Task OnMouseDownHeaderAsync(WebMouseEventArgs e, int x, int y, int index)
		{
			await InvokeWinzorDispatcherAsync(() => OnMouseDown(new MouseEventArgs(e.GetMouseButtons(), 1, x, y, 0)));
			await (GetJSInterop<ITreeViewAdvJSInterop>()?.ReorderColumnAsync(dotNetObjectReference, e, columnReferences[index]) ?? Task.CompletedTask);
		}

		protected async Task OnMouseUpAsync(WebMouseEventArgs e, int x, int y)
		{
			await InvokeWinzorDispatcherAsync(() =>
			{
				var args = new MouseEventArgs(e.GetMouseButtons(), 1, x, y, 0);
				if (Input is ClickColumnState)
				{
					Input.MouseMove(args);
					if (Input is ReorderColumnState)
					{
						return;
					}
				}
				OnMouseUp(args);
			});
		}

		DrawContext GetDrawContext(TreeNodeAdv node)
		{
			var context = new DrawContext();
			context.Graphics = new BGraphics();
			context.Font = Font;
			context.Enabled = Enabled;
			context.DrawSelection = DrawSelectionMode.None;
			context.CurrentEditorOwner = CurrentEditorOwner;
			context.DrawFocus = Focused && CurrentNode == node;

			if (DragMode)
			{
				if ((_dropPosition.Node == node) && _dropPosition.Position == NodePosition.Inside && HighlightDropPosition)
				{
					context.DrawSelection = DrawSelectionMode.Active;
				}
			}
			else
			{
				if (node.IsSelected && Focused)
				{
					context.DrawSelection = DrawSelectionMode.Active;
				}
				else if (node.IsSelected && !Focused && !HideSelection)
				{
					context.DrawSelection = DrawSelectionMode.Inactive;
				}
			}

			return context;
		}

		internal void PreDragOverAllNodes()
		{
			foreach (var node in AllNodes)
			{
				node.DropPositionEffects.Clear();
				_dropPosition.Node = node;
				foreach (var position in Enum.GetValues<NodePosition>())
				{
					_dropPosition.Position = position;
					if (AllowDrop)
					{
						OnDragOver(new DragEventArgs(
							new DataObject(Selection.ToArray()),
							0, 0, 0,
							DragDropEffects.Move,
							DragDropEffects.Move
						));
					}
					else
					{
						DropPosition.Node.DropPositionEffects.Add(DragDropEffects.None);
					}
				}
				if (!AllowDropPositionBefore(node))
				{
					node.DropPositionEffects[1] = DragDropEffects.None;
				}
				if (!AllowDropPositionAfter(node))
				{
					node.DropPositionEffects[2] = DragDropEffects.None;
				}
			}
		}

		protected override void OnVisibleChanged(EventArgs e)
		{
			base.OnVisibleChanged(e);
			UpdateView();
		}

		[JSInvokable("SetColumnWidthAsync")]
		public async Task SetColumnWidthAsync(int columnIndex, int newWidth)
		{
			await InvokeWinzorDispatcherAsync(() =>
			{
				if (columnIndex >= 0 && columnIndex < Columns.Count && Columns[columnIndex].IsVisible)
				{
					Columns[columnIndex].Width = newWidth;
				}
			});
		}

		[JSInvokable("ReorderColumnAsync")]
		public async Task ReorderColumnAsync(WebMouseEventArgs e, int columnIndex, int offsetX)
		{
			await InvokeWinzorDispatcherAsync(() =>
			{
				if (AllowColumnReorder && columnIndex >= 0 && columnIndex < Columns.Count && Columns[columnIndex].IsVisible)
				{
					var column = Columns[columnIndex];
					Input = new ReorderColumnState(this, column, Point.Empty);

					var args = new MouseEventArgs(e.GetMouseButtons(), 1, offsetX, 0, 0);
					Input.MouseMove(args);
					OnMouseUp(args);
				}
			});
		}

		async Task MouseDownHandlerAsync(WebMouseEventArgs args, ElementReference column)
		{
			isMouseOnResizer = true;
			await (GetJSInterop<ITreeViewAdvJSInterop>()?.ChangeColumnWidthAsync(dotNetObjectReference, args, column) ?? Task.CompletedTask);
		}

		bool isMouseOnResizer;


		[JSInvokable("DragDropAsync")]
		public async Task DragDropAsync(int keyState, int x, int y)
		{
			await InvokeWinzorDispatcherAsync(() =>
			{
				SetDropPosition(new Point(x, y));
				OnDragDrop(new DragEventArgs(
					new DataObject(Selection.ToArray()),
					keyState, x, y,
					DragDropEffects.Move,
					DragDropEffects.Move
				));
			});
		}

		readonly DotNetObjectReference<TreeViewAdv> dotNetObjectReference;

		#region Data Struct

		struct TreeViewAdvColumnHeaderData
		{
			public string Text { get; init; }

			public Rectangle Bounds { get; init; }

			public SortOrder SortOrder { get; init; }

			public int ColumnIndex { get; init; }

			public int MinColumnWidth { get; init; }

			public int MaxColumnWidth { get; init; }

			public EventCallback<WebMouseEventArgs> OnMouseDown { get; init; }

			public EventCallback<WebMouseEventArgs> OnMouseUp { get; init; }
		}

		public struct TreeViewAdvRowData
		{
			public int Index { get; init; }

			public List<TreeViewAdvRowData> Rows { get; init; }

			public List<TreeViewAdvControlData> Controls { get; init; }

			public Rectangle Bounds { get; init; }

			public bool IsSelected { get; init; }

			public bool CanExpand { get; init; }

			public bool IsExpanded { get; init; }

			public bool TreeAllowDrop { get; init; }

			public string BackgroundColor { get; init; }

			public string DropPositionCursors { get; init; }

			public bool IsMasterActivitySalesRelation { get; init; }
		}

		public struct TreeViewAdvControlData
		{
			public string CssClass { get; init; }

			public MarkupString ControlContent { get; init; }

			public Rectangle Bounds { get; init; }

			public int LeftMargin { get; init; }

			public int? ParentColumnIndex { get; init; }

			public EventCallback<WebMouseEventArgs> OnMouseUp { get; init; }

			public EventCallback<WebMouseEventArgs> OnMouseDown { get; init; }

			public EventCallback<WebMouseEventArgs> OnClick { get; init; }

			public EventCallback<WebMouseEventArgs> OnDblClick { get; init; }
		}

		#endregion
	}
}
