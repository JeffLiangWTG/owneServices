using System.Windows.Forms;

namespace Enterprise.Core.Environment
{
	public partial class UserConfirmationStringLabel
	{
		protected override string ControlStyleString => $"{base.ControlStyleString}background: linear-gradient(90deg,{labelHighlightColor})";

		string labelHighlightColor = string.Empty;

		#region HighlightColor Constant
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Color code")]
		const string redHighlightColor = "rgba(255,0,0,0.2)";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Color code")]
		const string greenHighlightColor = "rgba(0,128,0,0.2)";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Color code")]
		const string noHighlightColor = "rgba(255,255,255,0)";
		#endregion

		void HighlightInputProgress(PaintEventArgs e)
		{
			var paddingWidth = TextRenderer.GetWidthAdjustment();
			var correctPartWidth = TextRenderer.MeasureText(e.Graphics, correctPartToHighlight, Font, Size, TextFormatFlags.NoPadding).Width;
			var wrongPartWidth = TextRenderer.MeasureText(e.Graphics, wrongPartToHighlight, Font, Size, TextFormatFlags.NoPadding).Width;
			var expectedStringWidth = TextRenderer.MeasureText(e.Graphics, ExpectedString, Font, Size, TextFormatFlags.NoPadding).Width;
			if (expectedStringWidth > 0)
			{
				var correctPartPercentage = (decimal)(correctPartWidth - paddingWidth) / expectedStringWidth * 100;
				var wrongPartPercentage = (decimal)(wrongPartWidth - paddingWidth) / expectedStringWidth * 100;
				if (!string.IsNullOrEmpty(correctPartToHighlight) && string.IsNullOrEmpty(wrongPartToHighlight))
				{
					labelHighlightColor = $"{greenHighlightColor}{correctPartPercentage}%,{noHighlightColor}0%";
				}
				else if (!string.IsNullOrEmpty(correctPartToHighlight) && !string.IsNullOrEmpty(wrongPartToHighlight))
				{
					labelHighlightColor = $"{greenHighlightColor}{correctPartPercentage}%,{redHighlightColor}0% {wrongPartPercentage + correctPartPercentage}%,{noHighlightColor}0%";
				}
				else if (!string.IsNullOrEmpty(wrongPartToHighlight))
				{
					labelHighlightColor = $"{redHighlightColor}{wrongPartPercentage}%,{noHighlightColor}0%";
				}
				else
				{
					labelHighlightColor = $"{noHighlightColor}100%,{noHighlightColor}0%";
				}
			}
		}
	}
}
