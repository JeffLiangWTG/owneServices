using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IT.GUI;

public class SendingModeSplitButton : ZButton
{
	public SendingModeSplitButton()
	{
		ContextMenuStrip = new ContextMenuStrip();
		ContextMenuStrip.Items.Add(new ZToolStripMenuItem(ResStrings.ManualSend, ManualSendToolStripMenuItem_Click));
		ContextMenuStrip.Items.Add(new ZToolStripMenuItem(ResStrings.FallbackProcedure, FallbackProcedureToolStripMenuItem_Click));
		Click += SendingModeSplitButton_Click;
	}

	public event EventHandler AutomaticSendSelected;
	public event EventHandler ManualSendSelected;
	public event EventHandler FallbackProcedureSelected;

	public ButtonStyle SendButtonStyle { get; private set; } = ButtonStyle.MultipleActions;

	void SendingModeSplitButton_Click(object sender, EventArgs e)
	{
		if (!SplitPartClicked(MousePosition))
		{
			switch (SendButtonStyle)
			{
				case ButtonStyle.MultipleActions:
				case ButtonStyle.AutomaticAction:
					AutomaticSendSelected?.Invoke(this, EventArgs.Empty);
					break;
				case ButtonStyle.FallbackAction:
					FallbackProcedureSelected?.Invoke(this, EventArgs.Empty);
					break;
			}
		}
	}

	void ManualSendToolStripMenuItem_Click(object sender, EventArgs e)
	{
		ManualSendSelected?.Invoke(this, EventArgs.Empty);
	}

	void FallbackProcedureToolStripMenuItem_Click(object sender, EventArgs e)
	{
		FallbackProcedureSelected?.Invoke(this, EventArgs.Empty);
	}

	protected override void OnMouseDown(MouseEventArgs mevent)
	{
		if (SendButtonStyle == ButtonStyle.MultipleActions && mevent.Button == MouseButtons.Left && SplitPartClicked(mevent.Location))
		{
			ContextMenuStrip.Show(this, 0, Height);
		}
		else
		{
			base.OnMouseDown(mevent);
		}
	}

	protected override void OnPaint(PaintEventArgs pevent)
	{
		base.OnPaint(pevent);
		if (SendButtonStyle == ButtonStyle.MultipleActions)
		{
			DrawSplitPart(pevent);
		}
	}

	void DrawSplitPart(PaintEventArgs pevent)
	{
#if !WINZOR
		int arrowX = ClientRectangle.Width - 14;
		int arrowY = ClientRectangle.Height / 2 - 1;

		var arrowBrush = Enabled ? SystemBrushes.ControlText : SystemBrushes.ButtonShadow;
		pevent.Graphics.FillPolygon(arrowBrush, new[] {
			CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(arrowX, arrowY, false),
			CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(arrowX + 7, arrowY, false),
			CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(arrowX + 3, arrowY + 4, false)
		});

		int lineX = ClientRectangle.Width - SplitWidth;
		int lineYFrom = arrowY - 4;
		int lineYTo = arrowY + 8;
		using (var separatorPen = new Pen(Brushes.DarkGray) { DashStyle = DashStyle.Dot })
		{
			pevent.Graphics.DrawLine(separatorPen, lineX, lineYFrom, lineX, lineYTo);
		}
#endif
	}

	public void SetButtonStyle(ButtonStyle buttonStyle)
	{
		SendButtonStyle = buttonStyle;
		switch (buttonStyle)
		{
			case ButtonStyle.MultipleActions:
				Text = ResStrings.Send;
				Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, 0, SplitWidth / 2, 0);
				break;
			case ButtonStyle.AutomaticAction:
				Text = ResStrings.Send;
				Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(all: 0);
				break;
			case ButtonStyle.FallbackAction:
				Text = ResStrings.FallbackProcedure;
				Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(all: 0);
				break;
		}
	}

	const int SplitWidth = 20;

	ZBool SplitPartClicked(Point unscaledLocation)
	{
		var unscaledWidth = CargoWise.Windows.UI.ControlDpiScalingHelper.UnscaleFromCurrentDpiX(Width);
		var unscaledHeight = CargoWise.Windows.UI.ControlDpiScalingHelper.UnscaleFromCurrentDpiY(Height);
		return CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledRectangle(unscaledWidth - SplitWidth, 0, SplitWidth, unscaledHeight).Contains(unscaledLocation);
	}

	static class ResStrings
	{
		public static string ManualSend => Res.GetString("69FD438B-D60E-42C1-8EB2-5A58B8612EF3", "&Manual Send");
		public static string FallbackProcedure => Res.GetString("F211482F-7BE0-45F5-9C75-388341112076", "&Fallback Procedure");
		public static string Send => Res.GetString("3B25BFAD-5F12-44AA-8D2F-1E78BD15ED9A", "&Send");
	}

	public enum ButtonStyle
	{
		MultipleActions = 0,
		FallbackAction = 1,
		AutomaticAction = 2,
	}
}
