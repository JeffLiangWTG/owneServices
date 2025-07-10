using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.VisualBoards.GUI
{
	public class DragDropDescriptor
	{
		public DragDropDescriptor()
		{
			AllowHorizontalDrag = AllowVerticalDrag = AllowDragOutsideParentBounds = true;
			ControlTypesToIgnore = Type.EmptyTypes;
		}

		[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
		public Type[] ControlTypesToIgnore { get; set; }
		public bool AllowVerticalDrag { get; set; }
		public bool AllowHorizontalDrag { get; set; }
		public bool AllowDragOutsideParentBounds { get; set; }
	}

	public static class DragDropHelper
	{
		public static DragDropHandler AddDragDropSupport(Control control)
		{
			return AddDragDropSupport(control, new DragDropDescriptor());
		}

		public static DragDropHandler AddDragDropSupport(Control control, DragDropDescriptor dragDropDescriptor)
		{
			if (!ShouldSubscribeControl(control, dragDropDescriptor.ControlTypesToIgnore))
			{
				return null;
			}

			return controlPositions.GetOrAdd(control, (key) =>
			{
				control.Disposed += Dispose;

				return new DragDropHandler(control, dragDropDescriptor);
			});
		}

		public static bool IsAnyPartOfControlVisibleWithinParent(Control control, int innerMargin = 0)
		{
			var parent = control.Parent;
			return parent != null
				&& control.Left < parent.Width - innerMargin && control.Top + innerMargin < parent.Height
				&& control.Left + control.Width > innerMargin && control.Top + control.Height > innerMargin;
		}

		public static bool ShouldSubscribeControl(Control control, Type[] controlTypesToIgnore)
		{
			return !IsUserInteractiveControl(control) && !controlTypesToIgnore.Any(t => t.IsAssignableFrom(control.GetType()));
		}

		public static bool IsUserInteractiveControl(Control control)
		{
			return control is ZDropEdit || control is TextBox || control is Button || control is CheckBox || control is PictureBox;
		}

		#region Dispose

		static void Dispose(object sender, EventArgs e)
		{
			var control = (Control)sender;

			if (controlPositions.TryRemove(control, out var handler))
			{
				handler.Dispose();
			}
		}

		#endregion

		[SuppressMessage("CargoWiseOne", "CW1021:StaticFieldsAreThreadStaticRule")]
		static readonly ConcurrentDictionary<Control, DragDropHandler> controlPositions = new ConcurrentDictionary<Control, DragDropHandler>();

		public sealed class DragDropHandler : IDisposable
		{
			internal DragDropHandler(Control parent, DragDropDescriptor dragDropDescriptor)
			{
				this.parentControl = parent;
				this.dragDropDescriptor = dragDropDescriptor;
#if WINZOR
				parent.AllowItemDrag = true;
				parent.DragTargetHidden = true;
#endif

				HookupDragEventsRecursive(parent, dragDropDescriptor);
			}

			readonly Control parentControl;
			readonly DragDropDescriptor dragDropDescriptor;

			public event MouseEventHandler Dragging;
			public event MouseEventHandler DragStarting;
			public event MouseEventHandler DragFinished;

			#region Event hookup

			void HookupDragEventsRecursive(Control parent, DragDropDescriptor descriptor)
			{
				HookupDragEvents(parent);

				foreach (Control control in parent.Controls)
				{
					if (ShouldSubscribeControl(control, descriptor.ControlTypesToIgnore))
					{
						HookupDragEventsRecursive(control, descriptor);
					}
				}
			}

			void RemoveDragEventsRecursive(Control parent, DragDropDescriptor descriptor)
			{
				RemoveDragEvents(parent);

				foreach (Control control in parent.Controls)
				{
					if (ShouldSubscribeControl(control, descriptor.ControlTypesToIgnore))
					{
						RemoveDragEventsRecursive(control, descriptor);
					}
				}
			}

			void HookupDragEvents(Control control)
			{
#if !WINZOR
				control.MouseMove += Control_MouseMove;
				control.MouseDown += Control_MouseDown;
				control.MouseUp += Control_MouseUp;
#else
				control.DragStart += Control_DragStart;
				control.DragEnd += Control_DragEnd;	
#endif
				SubscribeToDisposed(control);
			}

			void RemoveDragEvents(Control control)
			{
#if !WINZOR
				control.MouseMove -= Control_MouseMove;
				control.MouseDown -= Control_MouseDown;
				control.MouseUp -= Control_MouseUp;
#else
				control.DragStart -= Control_DragStart;
				control.DragEnd -= Control_DragEnd;
#endif
			}

			void SubscribeToDisposed(Control control)
			{
				EventHandler disposedHandler = null;
				disposedHandler = (s, e) =>
				{
					RemoveDragEvents(control);
					control.Disposed -= disposedHandler;
				};
				control.Disposed += disposedHandler;
			}

			#endregion

			#region Event handlers

#if WINZOR
			Point GetRelativePoint(int x, int y)
			{
				var parent = parentControl;
				while (parent != null)
				{
					x -= parent.Left;
					y -= parent.Top;
					parent = parent.Parent;
				}
				return new Point(x, y);
			}

			void Control_DragStart(object sender, DragEventArgs e)
			{
				parentControl.Visibility = (ZArchitecture.Core.NoResString)"hidden";
				var newPoint = GetRelativePoint(e.X, e.Y);
				Control_MouseDown(sender, new MouseEventArgs(MouseButtons.Left, 1, newPoint.X, newPoint.Y, 0));
			}

			void Control_DragEnd(object sender, DragEventArgs e)
			{
				parentControl.Visibility = (ZArchitecture.Core.NoResString)"visible";
				var newPoint = GetRelativePoint(e.X, e.Y);
				Control_MouseMove(sender, new MouseEventArgs(MouseButtons.Left, 1, newPoint.X, newPoint.Y, 0));
				newPoint = GetRelativePoint(e.X, e.Y);
				Control_MouseMove(sender, new MouseEventArgs(MouseButtons.Left, 1, newPoint.X, newPoint.Y, 0));
				Control_MouseUp(sender, new MouseEventArgs(MouseButtons.Left, 1, newPoint.X, newPoint.Y, 0));
			}
#endif

			void Control_MouseMove(object sender, MouseEventArgs e)
			{
				if (!parentControl.IsDisposed)
				{
					if (e.Button == MouseButtons.Left)
					{
						int dx = e.X - lastPos.X;
						int dy = e.Y - lastPos.Y;

						var newLeft = dragDropDescriptor.AllowHorizontalDrag ? parentControl.Left + dx : parentControl.Left;
						var newTop = dragDropDescriptor.AllowVerticalDrag ? parentControl.Top + dy : parentControl.Top;

						if (!dragDropDescriptor.AllowDragOutsideParentBounds)
						{
							if (newLeft < 0)
							{
								newLeft = 0;
							}
							else if (newLeft + parentControl.Width > parentControl.Parent.Width)
							{
								newLeft = parentControl.Parent.Width - parentControl.Width;
							}

							if (newTop < 0)
							{
								newTop = 0;
							}
							else if (newTop + parentControl.Height > parentControl.Parent.Height)
							{
								newTop = parentControl.Parent.Height - parentControl.Height;
							}
						}

						if (dx != 0 && dy != 0)
						{
							parentControl.Location = ControlDpiScalingHelper.NewScaledPoint(newLeft, newTop, false);
							if (Dragging != null)
							{
								Dragging(sender, e);
							}
						}
					}
				}
			}

			void Control_MouseDown(object sender, MouseEventArgs e)
			{
				if (!parentControl.IsDisposed && e.Button == MouseButtons.Left)
				{
					lastPos = e.Location;
					parentControl.BringToFront();
					parentControl.Capture = true;

					if (DragStarting != null)
					{
						DragStarting(sender, e);
					}
				}
			}

			void Control_MouseUp(object sender, MouseEventArgs e)
			{
				if (!parentControl.IsDisposed && e.Button == MouseButtons.Left)
				{
					parentControl.Capture = false;

					if (DragFinished != null)
					{
						DragFinished(sender, e);
					}
				}
			}

			Point lastPos;

			#endregion

			#region Dispose

			public void Dispose()
			{
				Dragging = null;
				DragStarting = null;
				DragFinished = null;

				RemoveDragEventsRecursive(parentControl, dragDropDescriptor);
			}

			#endregion
		}
	}
}
