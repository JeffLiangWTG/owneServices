using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using CargoWise.Common;
using Enterprise.DocumentVisualizer.Core;
using FlexCel.Core;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.DocumentVisualizer.FlexCelIntegration
{
	[Immutable]
	public sealed class FlexCelBordersAdapter : IBorders
	{
		public FlexCelBordersAdapter(IFlexCelPalette palette, TFlxBorders flxTopLeftBorders, TFlxBorders flxBottomRightBorders)
		{
			Argument.NotNull(palette, nameof(palette));
			Argument.NotNull(flxTopLeftBorders, nameof(flxTopLeftBorders));
			Argument.NotNull(flxBottomRightBorders, nameof(flxBottomRightBorders));

			top = new FlexCelBorderAdapter(palette, flxTopLeftBorders.Top);
			bottom = new FlexCelBorderAdapter(palette, flxBottomRightBorders.Bottom);
			left = new FlexCelBorderAdapter(palette, flxTopLeftBorders.Left);
			right = new FlexCelBorderAdapter(palette, flxBottomRightBorders.Right);

			var diagonal = new FlexCelBorderAdapter(palette, flxTopLeftBorders.Diagonal);

			switch (flxBottomRightBorders.DiagonalStyle)
			{
				case TFlxDiagonalBorder.None:
					diagonalUp = Border.Empty;
					diagonalDown = Border.Empty;
					break;

				case TFlxDiagonalBorder.DiagUp:
					diagonalUp = diagonal;
					diagonalDown = Border.Empty;
					break;

				case TFlxDiagonalBorder.DiagDown:
					diagonalUp = Border.Empty;
					diagonalDown = diagonal;
					break;

				case TFlxDiagonalBorder.Both:
					diagonalDown = diagonal;
					diagonalUp = diagonal;
					break;
			}
		}

		public IBorder Top => top;
		public IBorder Bottom => bottom;
		public IBorder Left => left;
		public IBorder Right => right;
		public IBorder DiagonalUp => diagonalUp;
		public IBorder DiagonalDown => diagonalDown;

		[SuppressMessage("CargoWiseOne", "CW1023:ImmutableRule")]
		readonly IBorder top;

		[SuppressMessage("CargoWiseOne", "CW1023:ImmutableRule")]
		readonly IBorder bottom;

		[SuppressMessage("CargoWiseOne", "CW1023:ImmutableRule")]
		readonly IBorder left;

		[SuppressMessage("CargoWiseOne", "CW1023:ImmutableRule")]
		readonly IBorder right;

		[SuppressMessage("CargoWiseOne", "CW1023:ImmutableRule")]
		readonly IBorder diagonalUp;

		[SuppressMessage("CargoWiseOne", "CW1023:ImmutableRule")]
		readonly IBorder diagonalDown;

		IEnumerator<IBorder> IEnumerable<IBorder>.GetEnumerator()
		{
			yield return Top;
			yield return Bottom;
			yield return Left;
			yield return Right;
			yield return DiagonalUp;
			yield return DiagonalDown;
		}

		IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable<IBorder>)this).GetEnumerator();
	}
}