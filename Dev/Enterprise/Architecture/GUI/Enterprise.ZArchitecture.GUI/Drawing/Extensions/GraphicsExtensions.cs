using System.Drawing;

namespace Enterprise.ZArchitecture.GUI.Drawing
{
	public static class GraphicsExtensions
	{
		const int maxDrawableStringLengthWithoutLineBreak = 32000;

		/// <summary>
		/// Trims the end of the specified text string if necessary so that it can be used in MeasureString() and DrawString() without throwing exception.
		/// </summary>
		/// <param name="graphics"></param>
		/// <param name="text"></param>
		/// <returns></returns>
		public static string GetDrawableStringByTrimmingEnd(this Graphics graphics, string text)
		{
			if (text.Length <= maxDrawableStringLengthWithoutLineBreak)
			{
				return text;
			}

			return text.Substring(0, maxDrawableStringLengthWithoutLineBreak);
		}
	}
}
