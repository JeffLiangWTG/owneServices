using System;
using System.Collections.Generic;
using CargoWise.Common;
using Enterprise.DocumentVisualizer.Core;
using FlexCel.Core;

namespace Enterprise.DocumentVisualizer.FlexCelIntegration
{
	sealed class FlexCelDrawing : IDrawing
	{
		public FlexCelDrawing(FlexCelWorksheet worksheet, int drawingIndex)
		{
			this.worksheet = Argument.NotNull(worksheet, "worksheet");
			this.drawingIndex = drawingIndex;
		}

		readonly FlexCelWorksheet worksheet;
		readonly int drawingIndex;

		public const double MaxColumnOffset = 1024;
		public const double MaxRowOffset = 255;

		#region Name

		public string Name
		{
			get { return ImageProperties.ShapeName; }
		}

		#endregion

		#region Image

		public IReadOnlyCollection<byte> ImageData
		{
			get
			{
				if (imageData == null)
				{
					var imageType = TXlsImgType.Unknown;
					imageData = Array.AsReadOnly(worksheet.GetImage(drawingIndex, ref imageType));
				}

				return imageData;
			}
		}

		IReadOnlyCollection<byte> imageData;

		#endregion

		#region LeftColumn

		public int LeftColumn
		{
			get { return ImageProperties.Anchor.Col1; }
		}

		public double LeftColumnOffset
		{
			get
			{
				if (!leftColumnOffset.HasValue)
				{
					leftColumnOffset = (ImageProperties.Anchor.Dx1 / MaxColumnOffset) * 100d;
				}

				return leftColumnOffset.Value;
			}
		}

		double? leftColumnOffset;

		#endregion

		#region RightColumn

		public int RightColumn
		{
			get { return ImageProperties.Anchor.Col2; }
		}

		public double RightColumnOffset
		{
			get
			{
				if (!rightColumnOffset.HasValue)
				{
					rightColumnOffset = (ImageProperties.Anchor.Dx2 / MaxColumnOffset) * 100d;
				}

				return rightColumnOffset.Value;
			}
		}

		double? rightColumnOffset;

		#endregion

		#region TopRow

		public int TopRow
		{
			get { return ImageProperties.Anchor.Row1; }
		}

		public double TopRowOffset
		{
			get
			{
				if (!topRowOffset.HasValue)
				{
					topRowOffset = (ImageProperties.Anchor.Dy1 / MaxRowOffset) * 100d;
				}

				return topRowOffset.Value;
			}
		}

		double? topRowOffset;

		#endregion

		#region BottomRow

		public int BottomRow
		{
			get { return ImageProperties.Anchor.Row2; }
		}

		public double BottomRowOffset
		{
			get
			{
				if (!bottomRowOffset.HasValue)
				{
					bottomRowOffset = (ImageProperties.Anchor.Dy2 / MaxRowOffset) * 100d;
				}

				return bottomRowOffset.Value;
			}
		}

		double? bottomRowOffset;

		#endregion

		#region ImageProperties

		TImageProperties ImageProperties
		{
			get { return imageProperties ?? (imageProperties = worksheet.GetImageProperties(drawingIndex)); }
		}

		TImageProperties imageProperties;

		#endregion
	}
}