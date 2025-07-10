using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using CargoWise.Common;
using FlexCel.Pdf;

namespace Enterprise.RemotePrinting.Engine
{
	public enum WatermarkHorizontalAlign
	{
		Left,
		Centre,
		Right
	}

	public enum WatermarkVerticalAlign
	{
		Top,
		Middle,
		Bottom
	}

	/// <summary>
	/// A class to encapsulate the watermark information.
	/// </summary>
	public abstract class Watermark
	{
		public Watermark(WatermarkHorizontalAlign horizontalAlign, WatermarkVerticalAlign verticalAlign, float horizontalOffset, float verticalOffset, int rotation)
		{
			HorizontalAlign = horizontalAlign;
			VerticalAlign = verticalAlign;
			HorizontalOffset = horizontalOffset;
			VerticalOffset = verticalOffset;
			Rotation = rotation;
		}

		public static WatermarkHorizontalAlign GetHorizontalAlignment(string registryValue)
		{
			Argument.NotNull(registryValue, nameof(registryValue)); // Suggested By ReviewBot 

			WatermarkHorizontalAlign alignment;

			if (Enum.TryParse<WatermarkHorizontalAlign>(registryValue, ignoreCase: true, out var result))
			{
				alignment = result;
			}
			else
			{
				alignment = WatermarkHorizontalAlign.Centre;
			}

			return alignment;
		}

		public static WatermarkVerticalAlign GetVerticalAlignment(string registryValue)
		{
			Argument.NotNull(registryValue, nameof(registryValue)); // Suggested By ReviewBot 

			WatermarkVerticalAlign alignment;

			if (Enum.TryParse<WatermarkVerticalAlign>(registryValue, ignoreCase: true, out var result))
			{
				alignment = result;
			}
			else
			{
				alignment = WatermarkVerticalAlign.Middle;
			}

			return alignment;
		}

		#region Properties

		public WatermarkHorizontalAlign HorizontalAlign { get; }
		public WatermarkVerticalAlign VerticalAlign { get; }

		public float HorizontalOffset { get; }
		public float VerticalOffset { get; }

		/// <summary>
		/// 0 means no rotation. Positive angle rotates anti-clockwise on degrees.
		/// </summary>
		public int Rotation { get; }

		public virtual string AsText
		{
			get { return string.Empty; }
		}

		public virtual byte[] AsImage
		{
			get { return null; }
		}

		#endregion

		public abstract void Draw(Graphics gr, SizeF pageSize);
		public abstract void Draw(PdfWriter pdf, SizeF pageSize);

		public PointF GetAlign(SizeF textSize, SizeF pageSize)
		{
			PointF result = new Point(0, 0);

			switch (HorizontalAlign)
			{
				case WatermarkHorizontalAlign.Left: result.X = HorizontalOffset; break;
				case WatermarkHorizontalAlign.Right: result.X = pageSize.Width - (textSize.Width + HorizontalOffset); break;
				case WatermarkHorizontalAlign.Centre:
				default:
					result.X = (pageSize.Width - textSize.Width) / 2 + HorizontalOffset;
					break;
			}

			switch (VerticalAlign)
			{
				case WatermarkVerticalAlign.Top: result.Y = VerticalOffset; break;
				case WatermarkVerticalAlign.Bottom: result.Y = pageSize.Height - (textSize.Height + VerticalOffset); break;
				case WatermarkVerticalAlign.Middle:
				default:
					result.Y = (pageSize.Height - textSize.Height) / 2 + VerticalOffset;
					break;
			}

			return result;
		}

		protected void RotateCanvas(Graphics gr, SizeF objectSize, PointF objectPos)
		{
			Argument.NotNull(gr, nameof(gr));

			if (Rotation != 0)
			{
				var pos = new PointF(objectPos.X + objectSize.Width / 2, objectPos.Y + objectSize.Height / 2);

				using (var myMatrix = new Matrix())
				{
					myMatrix.RotateAt(-Rotation, pos, MatrixOrder.Append);
					gr.Transform = myMatrix;
				}
			}
		}

		protected void RotatePdfCanvas(PdfWriter pdf, SizeF objectSize, PointF objectPos)
		{
			Argument.NotNull(pdf, nameof(pdf));

			if (Rotation != 0)
			{
				var pos = new PointF(objectPos.X + objectSize.Width / 2, objectPos.Y + objectSize.Height / 2);
				pdf.Rotate(pos.X, pos.Y, Rotation);
			}
		}
	}
}
