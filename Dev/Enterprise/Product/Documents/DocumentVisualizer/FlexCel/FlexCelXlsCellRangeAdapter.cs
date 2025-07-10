using System.Diagnostics;
using CargoWise.Common;
using Enterprise.DocumentVisualizer.Core;
using FlexCel.Core;

namespace Enterprise.DocumentVisualizer.FlexCelIntegration
{
	[DebuggerDisplay("[{range.TopRow},{range.LeftColumn}] : [{range.BottomRow},{range.RightColumn}]")]
	public class FlexCelXlsRangeAdapter : IRange
	{
		public FlexCelXlsRangeAdapter(TXlsCellRange flxCelXlsCellRange)
		{
			this.flxCelXlsCellRange = Argument.NotNull(flxCelXlsCellRange, "flxCelXlsCellRange");
		}

		readonly TXlsCellRange flxCelXlsCellRange;

		int IRange.LeftColumn
		{
			get { return flxCelXlsCellRange.Left; }
		}

		int IRange.TopRow
		{
			get { return flxCelXlsCellRange.Top; }
		}

		int IRange.RightColumn
		{
			get { return flxCelXlsCellRange.Right; }
		}

		int IRange.BottomRow
		{
			get { return flxCelXlsCellRange.Bottom; }
		}
	}
}