using System.Drawing;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.VisualBoards.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using WinzorFramework;

namespace Enterprise.BufferManagement.GUI
{
	public partial class TaskCardControl : ZUserControl
	{
		protected override EventAttribute EventAttributes => EventAttribute.MouseDown | EventAttribute.ContextMenu;

		public override string ExtraStyleString => base.ExtraStyleString + (NoResString)"contain:layout;" + (!IsFrontMostCard ? (NoResString)" filter:brightness(75%);" : string.Empty);

		protected override void OnPaint(PaintEventArgs e)
		{
			base.OnPaint(e);
			PaintBorder(ClientSize, borderStyle, borderColor);
		}

		void PaintBorder([DpiState(DpiState.ScaledVariant)] Size size, SizedButtonBorderStyle borderStyle, Color borderColor)
		{
			if (!WinzorSpecificControls.Contains(borderPanel))
			{
				WinzorSpecificControls.Add(borderPanel);
			}

			borderPanel.ClientSize = size;
			borderPanel.BackColor = Color.FromArgb(0, 0, 0, 0);
			borderPanel.htmlBorder.BorderWidth = GetBorderWidth(borderStyle);
			borderPanel.htmlBorder.BorderColor = borderColor;

			switch (borderStyle.BorderStyle)
			{
				case VisualBoardButtonBorderStyle.Dashed:
					borderPanel.htmlBorder.BorderLineStyle = KBorderHtmlStyle.Dashed;
					break;
				case VisualBoardButtonBorderStyle.Dotted:
					borderPanel.htmlBorder.BorderLineStyle = KBorderHtmlStyle.Dotted;
					break;
				default:
					borderPanel.htmlBorder.BorderLineStyle = (KBorderHtmlStyle)borderStyle.BorderStyle;
					break;
			}
		}
	}
}
