using System;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Windows.Forms;
using CargoWise.Interop;
using CargoWise.Windows.UI;

namespace Enterprise.DocumentScanning
{
	class MouseMessageFilter : IMessageFilter
	{
		public Control Parent { get; }
		readonly FilterMouseMessage callback;

		public delegate bool FilterMouseMessage(ref Message m, Point mousePosition);

		public MouseMessageFilter(Control parent, FilterMouseMessage callback)
		{
			Parent = parent;
			this.callback = callback;
		}

		public bool PreFilterMessage(ref Message m)
		{
			if (IsMouseMessage(m.Msg) && Parent.Visible)
			{
				var specifiedControl = Control.FromHandle(m.HWnd);
				if (specifiedControl != null && specifiedControl == Parent || Parent.ContainsIncludingChildren(specifiedControl))
				{
#pragma warning disable WFDEV001 // 'Message.LParam' is obsolete: 'Casting to/from IntPtr is unsafe, use LParamInternal.'
					var screenMousePosition = specifiedControl.PointToScreen(PointFromLParam(m.LParam));
#pragma warning restore WFDEV001
					var clientMousePosition = Parent.PointToClient(screenMousePosition);
					var bounds = ControlDpiScalingHelper.NewScaledRectangle(Point.Empty, Parent.ClientSize, false);

					if (bounds.Contains(clientMousePosition))
					{
						return callback(ref m, clientMousePosition);
					}
				}
			}

			return false;
		}

		[SuppressMessage("CargoWiseOne", "CW1017", Justification = "This constructor is not available through ControlDpiScalingHelper")]
		[return: DpiState(DpiState.ScaledVariant)]
		Point PointFromLParam(IntPtr lparam)
				=> new Point(lparam.ToInt32());

		bool IsMouseMessage(int msg)
				=> msg >= WindowsMessage.WM_MOUSEFIRST && msg <= WindowsMessage.WM_MOUSELAST;
	}
}
