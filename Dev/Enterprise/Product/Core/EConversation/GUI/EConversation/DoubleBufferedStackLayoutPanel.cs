using System;
using System.ComponentModel.Design.Serialization;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.Layout;
using CargoWise.Windows.UI;

namespace Enterprise.EConversation.GUI
{
	public class DoubleBufferedStackLayoutPanel : StackLayoutPanel
	{
		public DoubleBufferedStackLayoutPanel()
			: base()
		{
			this.DoubleBuffered = true;
			Padding = ControlDpiScalingHelper.NewScaledPadding(0, 0, ControlDpiScalingHelper.UnscaleFromCurrentDpiX(SystemInformation.VerticalScrollBarWidth), 0);
			this.VisibleChanged += (o, e) => { if (Visible) { PerformLayout(); } };
		}
	}

	[DesignerSerializer(typeof(CargoWise.Windows.UI.Design.ControlDpiScalingCodeDomSerializer), typeof(CodeDomSerializer))]
	public class StackLayoutPanel : Panel
	{
		public StackLayoutPanel()
		{
			base.AutoScroll = true;
		}

		public override LayoutEngine LayoutEngine
		{
			get { return layoutEngine ?? (layoutEngine = new StackLayoutEngine()); }
		}
		StackLayoutEngine layoutEngine;

		public bool Initialized { get; set; }
	}

	public class StackLayoutEngine : LayoutEngine
	{
		public StackLayoutEngine()
		{
		}
#if WINZOR
		internal bool forceLayoutChange;
#endif
		int lastParentWidth = -1;
		int lastParentControls = -1;
		int lastParentVisibleControls = -1;

		public override bool Layout(object container, LayoutEventArgs layoutEventArgs)
		{
			var stackPanel = container as StackLayoutPanel;
			if (stackPanel == null)
			{
				return false;
			}
			if (!stackPanel.Initialized)
			{
				return false;
			}
#if !WINZOR
			if (lastParentWidth == stackPanel.Width && lastParentControls == stackPanel.Controls.Count && lastParentVisibleControls == stackPanel.Controls.Cast<Control>().Count(c => c.Visible))
			{
				return false;
			}
#else
			if (!forceLayoutChange && lastParentWidth == stackPanel.Width && lastParentControls == stackPanel.Controls.Count && lastParentVisibleControls == stackPanel.Controls.Cast<Control>().Count(c => c.Visible))
			{
				return false;
			}
#endif
			lastParentWidth = stackPanel.Width;
			lastParentControls = stackPanel.Controls.Count;
			lastParentVisibleControls = stackPanel.Controls.Cast<Control>().Count(c => c.Visible);

			var clientRectangle = stackPanel.ClientRectangle;
			var nextControlLocation = stackPanel.AutoScrollPosition;

			foreach (var control in stackPanel.Controls.OfType<ConversationMessageUserControl>().Where(control => control.Message.IsDeleted))
			{
				control.Dispose();
			}

			try
			{
				stackPanel.SuspendDrawing();
				foreach (Control control in stackPanel.Controls)
				{
					if (!control.Visible)
					{
						continue;
					}

					try
					{
						control.SuspendDrawing();

						var conv = control as ConversationMessageUserControl;

						nextControlLocation.Offset(control.Margin.Left, control.Margin.Top);

						control.Location = nextControlLocation;

						var size = control.GetPreferredSize(clientRectangle.Size);

						var maxWidth = clientRectangle.Width - control.Margin.Left - control.Margin.Right - SystemInformation.VerticalScrollBarWidth;

						//force width to maxWidth if it's greater than it, or if it's a non-conversation non-autosize control
						if (size.Width > maxWidth || (conv == null && !control.AutoSize))
						{
							size = ControlDpiScalingHelper.NewScaledSize(maxWidth, size.Height, false);
						}
						control.Size = size;

						if (conv != null && conv.bodyTextBox.ScrollBars == RichTextBoxScrollBars.None)
						{
							//if NewRectangle is taller than surrounding bodyTextBox, then vertical scrollbar exists.
							//Temporarily pretend it doesn't and add extra height to fix problem.
							//We have to iterate it a few times but it converges quickly and very satisfyingly.
							var extra = conv.LastNewRectangle.Height - conv.bodyTextBox.Size.Height;
							if (extra > 0)
							{
								var retries = 0;
								var retries_max = 10;
								do
								{
									//first, increase width and retake measurement. Assume it's not smaller than a single line.
									var dx = conv.bodyTextBox.Width - conv.bodyTextBox.ClientSize.Width;
									ControlDpiScalingHelper.SetWidth(conv, conv.Width + dx, false);
									var extra2 = Math.Max(ControlDpiScalingHelper.ScaleToCurrentDpiY(14), conv.LastNewRectangle.Height - conv.bodyTextBox.Size.Height);
									//then reset width and use new measurement.
									ControlDpiScalingHelper.SetWidth(conv, conv.Width - dx, false);
									ControlDpiScalingHelper.SetHeight(conv, conv.Height + extra2, false);
									extra = conv.LastNewRectangle.Height - conv.bodyTextBox.Size.Height;
									++retries;
								}
								while (extra > 0 && retries < retries_max);
							}
						}

						ControlDpiScalingHelper.SetX(ref nextControlLocation, clientRectangle.X, false);
						ControlDpiScalingHelper.SetY(ref nextControlLocation, nextControlLocation.Y + control.Height + control.Margin.Bottom, false);
					}
					finally
					{
						control.ResumeDrawing();
					}
				}
			}
			finally
			{
				stackPanel.ResumeDrawing();
			}
			return false;
		}
	}
}
