using System;
using System.Windows.Forms;

namespace Enterprise.DocumentVisualizer.GUI
{
	sealed class MouseHook : IMessageFilter, IDisposable
	{
		#region Events

		public event MouseEventHandler OnMouseClick
		{
			add
			{
				if (value != null)
				{
					onMouseClick += value;
					AddInvocationAndMessageFilter();
				}
			}
			remove
			{
				if (value != null)
				{
					onMouseClick -= value;
					RemoveInvocationAndMessageFilter();
				}
			}
		}

		public event MouseEventHandler OnMouseWheelUp
		{
			add
			{
				if (value != null)
				{
					onMouseWheelUp += value;
					AddInvocationAndMessageFilter();
				}
			}
			remove
			{
				if (value != null)
				{
					onMouseWheelUp -= value;
					RemoveInvocationAndMessageFilter();
				}
			}
		}

		public event MouseEventHandler OnMouseWheelDown
		{
			add
			{
				if (value != null)
				{
					onMouseWheelDown += value;
					AddInvocationAndMessageFilter();
				}
			}
			remove
			{
				if (value != null)
				{
					onMouseWheelDown -= value;
					RemoveInvocationAndMessageFilter();
				}
			}
		}

		void AddInvocationAndMessageFilter()
		{
			invocationListCount++;

			if (invocationListCount == 1)
			{
				Application.AddMessageFilter(this);
			}
		}

		void RemoveInvocationAndMessageFilter()
		{
			invocationListCount--;

			if (invocationListCount == 0)
			{
				Application.RemoveMessageFilter(this);
			}
		}

		#endregion

		int invocationListCount;
		event MouseEventHandler onMouseClick;
		event MouseEventHandler onMouseWheelUp;
		event MouseEventHandler onMouseWheelDown;

		#region IMessageFilter

		const int WM_LBUTTONDOWN = 0x201;
		const int WM_RBUTTONDOWN = 0x204;
		const int WM_MBUTTONDOWN = 0x207;
		const int WM_MOUSEWHEEL = 0x020A;
		const int MK_CONTROL = 0x0008;

		bool IMessageFilter.PreFilterMessage(ref Message m)
		{
			switch (m.Msg)
			{
				case WM_LBUTTONDOWN:
					NotifyClicked(MouseButtons.Left);
					break;

				case WM_MBUTTONDOWN:
					NotifyClicked(MouseButtons.Middle);
					break;

				case WM_RBUTTONDOWN:
					NotifyClicked(MouseButtons.Right);
					break;

				case WM_MOUSEWHEEL:
					var delta = unchecked((short)(m.WParam.ToInt64() >> 16));
					var key = unchecked((short)(m.WParam.ToInt64()));
					if (key == MK_CONTROL)
					{
						Action<int> notifyWheelEvent = (delta < 0) ? NotifyWheelDown : NotifyWheelUp;
						notifyWheelEvent(delta);
					}
					break;
			}

			return false;
		}

		void NotifyClicked(MouseButtons button)
		{
			var mousePosition = Cursor.Position;

			var args = new MouseEventArgs(button, 1, mousePosition.X, mousePosition.Y, 0);

			onMouseClick(this, args);
		}

		void NotifyWheelDown(int delta)
		{
			var mousePosition = Cursor.Position;
			var args = new MouseEventArgs(MouseButtons.None, 1, mousePosition.X, mousePosition.Y, delta);
			onMouseWheelDown(this, args);
		}

		void NotifyWheelUp(int delta)
		{
			var mousePosition = Cursor.Position;
			var args = new MouseEventArgs(MouseButtons.None, 1, mousePosition.X, mousePosition.Y, delta);
			onMouseWheelUp(this, args);
		}

		#endregion

		public void Dispose()
		{
			Application.RemoveMessageFilter(this);
		}
	}
}
