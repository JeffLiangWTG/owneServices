using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ZArchitecture.GUI
{
	[ToolboxItem(false)]
	[SuppressBindingMemberBashingTest]
	public class ZListBox : KListBox, ICaptionedComponents
	{
		public ZListBox()
		{
			UserEventTracker.Instance.AddUserEventToControl(this);
			translationFeedbackManager = new TranslationFeedbackManager(this);
		}

		public void EnableDragAndDrop(bool allowRearrangeItems)
		{
			AllowDrop = true;
			DragDrop += ZListBox_DragDrop;
			MouseDown += ZListBox_MouseDown;
			MouseUp += ZListBox_MouseUp;
#if WINZOR
			DragEnter += ZListBox_DragEnter;
			DragEnd += ZListBox_DragEnd;
			Draggable = allowRearrangeItems;
#else
			DragOver += ZListBox_DragOver;
			DragLeave += ZListBox_DragLeave;
			MouseMove += ZListBox_MouseMove;
#endif
			executeDragOver = allowRearrangeItems;
		}

		#region DragNDrop

		public bool IsDragging => (isDraggingPoint != null);

		public event EventHandler DragAndDropItemChanged;
		public event EventHandler PreviewDropItem;
		public event EventHandler AfterClearSelected;

		Point? isDraggingPoint;

		bool executeDragOver;
		int lastIndexItemOver = -1;
		(int left, int top, int right)? lastDrawLine;
#if !WINZOR
		Color lastBackgroundColour;
#endif

		internal void ZListBox_MouseDown(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left)
			{
				//The following line is required to allow for shift+click+drag within 'Show Columns in this order' to drag all rows.
				//It also breaks DoubleClick/MouseDoubleClick events and prevents them from firing.
				//Neither me (TST) nor Fernando (FTM) knows why for either of these things, sorry.
				Capture = false;
				isDraggingPoint = PointToClient(MousePosition);
			}
		}

		void ZListBox_MouseUp(object sender, MouseEventArgs e)
		{
			ResetLocals();
		}

		void ZListBox_MouseMove(object sender, MouseEventArgs e)
		{
			if (isDraggingPoint.HasValue && e.Button == MouseButtons.Left && SelectedIndex >= 0)
			{
				var pointToClient = PointToClient(MousePosition);

				if (isDraggingPoint.Value != pointToClient)
				{
					lastIndexItemOver = -1;

					var dropResult = DoDragDrop(SelectedItems, DragDropEffects.Move);
					if (dropResult == DragDropEffects.None)
					{
						ClearSelected();
						Refresh();
					}
					ResetLocals();
				}
			}
		}

		void ResetLocals()
		{
			isDraggingPoint = null;
			lastDrawLine = null;
		}

		public override void Refresh()
		{
			base.Refresh();
			lastDrawLine = null;
		}

		void ZListBox_DragEnter(object sender, DragEventArgs e)
		{
			lastIndexItemOver = (int)e.Data.GetData(typeof(int));
		}

		void ZListBox_DragEnd(object sender, DragEventArgs e)
		{
			Refresh();
			ResetLocals();
		}

		internal void ZListBox_DragOver(object sender, DragEventArgs e)
		{
			e.Effect = DragDropEffects.Move;

			if (executeDragOver)
			{
				var pointToClient = PointToClient(MousePosition);
#if DEBUG
				if (Globals.IsTest)
				{
					pointToClient = ControlDpiScalingHelper.NewScaledPoint(e.X, e.Y);
				}
#endif
				var currentIndexItemOver = IndexFromPoint(pointToClient);
				var linePositionIncrement = 0; // If it's below the last item, we need to draw the line under the last item (+20) and add the item in the end of the list (currentIndexItemOver = Items.Count)
				var indexItemDecrement = 0;

				if (currentIndexItemOver < 0)
				{
					linePositionIncrement = 20;
					currentIndexItemOver = Items.Count;
					indexItemDecrement = -1;
				}

				if (currentIndexItemOver != lastIndexItemOver && (currentIndexItemOver >= 0 || lastIndexItemOver != Items.Count)) // Prevent the ListBox to be refreshed over and over again when not over an item
				{
					EraseLine();
					DrawSelectedItemUnderline(currentIndexItemOver + indexItemDecrement, linePositionIncrement);
				}

				lastIndexItemOver = currentIndexItemOver;
			}
		}

		internal void ZListBox_DragDrop(object sender, DragEventArgs e)
		{
			PreviewDropItem?.Invoke(sender, e);

			if (lastIndexItemOver >= 0 && SelectedIndex >= 0 && lastIndexItemOver != SelectedIndex)
			{
				var increment = 0;
				var itemsCount = SelectedItems.Count;
				var copiedItems = new object[itemsCount];
				SelectedItems.CopyTo(copiedItems, 0);

				var shouldIncrement = false;

				if (SelectedIndices[0] > lastIndexItemOver)
				{
					shouldIncrement = true;
				}
				else
				{
					increment = -1; // if moving down
				}

				for (var i = 0; i < itemsCount; i++)
				{
					var data = copiedItems[i];
					Items.Remove(data);

					var newIndex = lastIndexItemOver + increment;
					if (newIndex >= Items.Count)
					{
						Items.Add(data);
					}
					else
					{
						Items.Insert(newIndex, data);
					}

					if (shouldIncrement)
					{
						increment++;
					}
				}

				// Select the items in the new position
				for (var i = 0; i < itemsCount; i++)
				{
					SelectedIndices.Add(Items.IndexOf(copiedItems[i]));
				}

				DragAndDropItemChanged?.Invoke(this, EventArgs.Empty);
			}
			else if (lastDrawLine != null)
			{
				Refresh();
			}
		}

		void ZListBox_DragLeave(object sender, EventArgs e)
		{
			lastIndexItemOver = -1;
		}

		void DrawSelectedItemUnderline(int index, int increment)
		{
#if !WINZOR
			using (var graphics = CreateGraphics())
			using (var pen = new Pen(Color.FromArgb(93, 198, 234), 2))
			{
				var rect = GetItemRectangle(index);

				lastDrawLine = (rect.Left, rect.Top + increment, rect.Right);
				LastBackgroundColour();
				graphics.DrawLine(pen, lastDrawLine.Value.left, lastDrawLine.Value.top, lastDrawLine.Value.right, lastDrawLine.Value.top);
			}
#endif
		}

#if !WINZOR
		[SuppressMessage("CargoWiseOne", "CW1017", Justification = "Internal use")]
		void LastBackgroundColour()
		{
#if DEBUG
			if (Globals.IsTest)
			{
				lastBackgroundColour = Color.White;
				return;
			}
#endif
			using (var bmp = new Bitmap(1, 1))
			using (var graphics = Graphics.FromImage(bmp))
			{
				var screenPoint = PointToScreen(new Point(lastDrawLine.Value.left, lastDrawLine.Value.top));
				graphics.CopyFromScreen(screenPoint.X, screenPoint.Y, 0, 0, new Size(1, 1));
				var colour = bmp.GetPixel(0, 0);
				lastBackgroundColour = colour;
			}
		}
#endif

		void EraseLine()
		{
#if !WINZOR
			if (lastDrawLine.HasValue)
			{
				using (var graphics = CreateGraphics())
				using (var pen = new Pen(lastBackgroundColour, 2))
				{
					graphics.DrawLine(pen, lastDrawLine.Value.left, lastDrawLine.Value.top, lastDrawLine.Value.right, lastDrawLine.Value.top);
				}
			}
#endif
		}

#endregion

		protected override void Dispose(bool isNotFinalizing)
		{
			if (isNotFinalizing)
			{
				if (translationFeedbackManager != null)
				{
					translationFeedbackManager.Dispose();
				}

				DataBindings.Clear();
			}

			base.Dispose(isNotFinalizing);
		}

		public new void ClearSelected()
		{
			base.ClearSelected();
			for (var i = 0; i < Items.Count; i++)
			{
				SetSelected(i, false);
			}
			AfterClearSelected?.Invoke(null, EventArgs.Empty);
		}

#region ICaptionedComponents

		object ICaptionedComponents.GetCaptionedComponentAt(Point p)
		{
			int? itemIndex = null;
			for (var i = 0; i < this.Items.Count; i++)
			{
				if (this.GetItemRectangle(i).Contains(p))
				{
					itemIndex = i;
					break;
				}
			}
			return itemIndex;
		}

		Rectangle ICaptionedComponents.GetCaptionedComponentRect(object component)
		{
			return this.GetItemRectangle((int)component);
		}

		object ICaptionedComponents.GetCaptionedComponentData(object component)
		{
			return this.Items[(int)component];
		}

#endregion

		readonly internal TranslationFeedbackManager translationFeedbackManager;
	}
}
