using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture;

namespace Enterprise.Core.Environment
{
	[ToolboxItem(false)]
	public partial class UserConfirmationStringLabel : ZLabel
	{
		#region Properties

		string ExpectedString
		{
			get { return Text; }
		}

		internal string correctPartToHighlight = string.Empty;
		internal string wrongPartToHighlight = string.Empty;

		#endregion

		public void UpdateInput(string inputText)
		{
			CalculateCorrectAndWrongPart(inputText);
			Refresh();
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			HighlightInputProgress(e);
			base.OnPaint(e);
		}

		internal void CalculateCorrectAndWrongPart(string inputText)
		{
			var wrongInputPosition = -1;
			for (var i = 0; i < inputText.Length && i < ExpectedString.Length; i++)
			{
				if (ExpectedString[i] != inputText[i])
				{
					wrongInputPosition = i;
					break;
				}
			}

			if (wrongInputPosition >= 0)
			{
				correctPartToHighlight = ExpectedString.Substring(0, wrongInputPosition);
				wrongPartToHighlight = ExpectedString.Substring(wrongInputPosition, ExpectedString.Length - wrongInputPosition);
			}
			else if (inputText.Length <= ExpectedString.Length)
			{
				correctPartToHighlight = inputText;
				wrongPartToHighlight = string.Empty;
			}
			else
			{
				correctPartToHighlight = string.Empty;
				wrongPartToHighlight = ExpectedString;
			}
		}

		#if !WINZOR

		void HighlightInputProgress(PaintEventArgs e)
		{
			var correctPartSize = TextRenderer.MeasureText(e.Graphics, correctPartToHighlight, Font, Size, TextFormatFlags.NoPadding);
			var wrongPartSize = TextRenderer.MeasureText(e.Graphics, wrongPartToHighlight, Font, Size, TextFormatFlags.NoPadding);
			var startPoint = ControlDpiScalingHelper.NewScaledPoint(Margin.Left + Padding.Left, Margin.Top + Padding.Top, false);
			var correctPartLayoutRect = ControlDpiScalingHelper.NewScaledRectangle(startPoint.X, startPoint.Y, correctPartSize.Width, correctPartSize.Height, false);
			using (var brush = new SolidBrush(Color.FromArgb(50, Color.Green)))
			{
				e.Graphics.FillRegion(brush, new Region(correctPartLayoutRect));
			}

			if (wrongPartToHighlight.Length > 0)
			{
				var wrongPartLayoutRect = ControlDpiScalingHelper.NewScaledRectangle(startPoint.X + correctPartSize.Width, startPoint.Y, wrongPartSize.Width, wrongPartSize.Height, false);
				using (var brush = new SolidBrush(Color.FromArgb(50, Color.Red)))
				{
					e.Graphics.FillRegion(brush, new Region(wrongPartLayoutRect));
				}
			}
		}

		#endif
	}
}
