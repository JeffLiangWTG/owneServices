using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using CargoWise.Common;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.Presentation;
using Range = Enterprise.DocumentVisualizer.Core.Range;

namespace Enterprise.DocumentVisualizer.GUI
{
	sealed class DocumentElementsCreator
	{
		public DocumentElementsCreator(IPage page, bool isReadOnly)
		{
			Argument.NotNull(page, nameof(page));
			Argument.NotNull(page.Document, nameof(page.Document));

			this.page = page;
			this.document = page.Document;
			this.isReadOnly = isReadOnly;
		}

		readonly IDocument document;
		readonly IPage page;
		readonly bool isReadOnly;

		#region CreateElements

		public IEnumerable<IDocumentElement> CreateElements()
		{
			for (int rowNumber = page.Range.TopRow; rowNumber <= page.Range.BottomRow; rowNumber++)
			{
				for (int columnNumber = page.Range.LeftColumn; columnNumber <= page.Range.RightColumn; columnNumber++)
				{
					var cell = document.GetDocumentCell(rowNumber, columnNumber);

					var hasBeenProcessed = cell.TopRow != rowNumber || cell.LeftColumn != columnNumber;

					if (hasBeenProcessed || cell.Height - 0.1 <= 0)
					{
						continue;
					}

					var cellLocation = cell.CalculateLocation(document,
						page.Range.TopRow,
						page.Range.LeftColumn,
						(float)document.HorizontalPrintOffset);

					var drawTopRow = Math.Max(cell.TopRow, page.Range.TopRow);
					var drawBottomRow = Math.Min(cell.BottomRow, page.Range.BottomRow);

					var drawHeight = 0d;

					for (int i = drawTopRow; i <= drawBottomRow; i++)
					{
						drawHeight += document.Rows.GetAt(i).Height;
					}

					var drawLeftColumn = Math.Max(cell.LeftColumn, page.Range.LeftColumn);
					var drawRightColumn = Math.Min(cell.RightColumn, page.Range.RightColumn);

					var drawWidth = 0d;

					for (int i = drawLeftColumn; i <= drawRightColumn; i++)
					{
						drawWidth += document.Columns.GetAt(i).Width;
					}

					var drawSize = new SizeF((float)drawWidth, (float)drawHeight);

					var cellOverflowsRight = cell.RightColumn > page.Range.RightColumn;
					var cellOverflowsBottom = cell.BottomRow > page.Range.BottomRow;

					if (cell.HasBorders())
					{
						foreach (var cellOutline in GetCellBorders(cell, cellLocation, drawSize, cellOverflowsBottom, cellOverflowsRight))
						{
							yield return cellOutline;
						}
					}

					if (cell.HasBackground() || cell.HasDynamicContent())
					{
						yield return GetCellBackground(cell, cellLocation, drawSize);
					}

					foreach (var cellContent in GetCellContent(cell, cellLocation, drawSize))
					{
						yield return cellContent;
					}
				}
			}
		}

		#endregion

		#region Cell Contents

		IEnumerable<IDocumentElement> GetCellContent(IDocumentCell cell, PointF cellLocation, SizeF cellSize)
		{
			if (!isReadOnly && cell.HasDynamicContent())
			{
				yield return GetDynamicContent(cell, cellLocation, cellSize);
			}
			else if (cell.HasValue())
			{
				yield return GetStaticContent(cell, Convert.ToString(cell.Value), cellLocation, cellSize);
			}

			if (cell.Drawing != null)
			{
				yield return GetCellDrawing(cell);
			}
		}

		IDocumentElement GetDynamicContent(IDocumentCell cell, PointF location, SizeF size)
		{
			cell.Evaluate();

			var dynamicContent = new DynamicContent(cell, location, size, cell.Padding)
			{
				Font = cell.Format.Font,
				HAlignment = cell.Format.HAlignment,
				VAlignment = cell.Format.VAlignment,
				BackgroundColor = cell.Format.BackgroundColor,
				Wrap = cell.Format.WrapText,
				ZOrder = DrawingOrder.Content
			};

			return dynamicContent;
		}

		IDocumentElement GetStaticContent(ICell cell, string text, PointF location, SizeF size)
		{
			var cellContentPainter = new Text(location, size, cell.Padding, text)
			{
				Font = cell.Format.Font,
				HAlignment = cell.Format.HAlignment,
				VAlignment = cell.Format.VAlignment,
				Wrap = cell.Format.WrapText,
				ZOrder = DrawingOrder.Content
			};

			return cellContentPainter;
		}

		IDocumentElement GetCellDrawing(IDocumentCell cell)
		{
			var drawing = cell.Drawing;

			var topLeftAnchor = new Range(drawing.TopRow, drawing.LeftColumn, drawing.TopRow, drawing.LeftColumn);

			var topLeftAnchorWidth = topLeftAnchor.Width(document);
			var topLeftAnchorHeight = topLeftAnchor.Height(document);

			var topLeftAnchorLocation = topLeftAnchor.CalculateLocation(document, page.Range.TopRow, page.Range.LeftColumn, (float)document.HorizontalPrintOffset);

			var bottomRightAnchor = new Range(drawing.BottomRow, drawing.RightColumn, drawing.BottomRow, drawing.RightColumn);

			var bottomRightAnchorWidth = bottomRightAnchor.Width(document);
			var bottomRightAnchorHeight = bottomRightAnchor.Height(document);

			var bottomRightAnchorLocation = bottomRightAnchor.CalculateLocation(document, page.Range.TopRow, page.Range.LeftColumn, (float)document.HorizontalPrintOffset);

			var offsetLeft = (float)(topLeftAnchorWidth * drawing.LeftColumnOffset * 0.01);
			var offsetTop = (float)(topLeftAnchorHeight * drawing.TopRowOffset * 0.01);

			var offsetRight = (float)(bottomRightAnchorWidth * drawing.RightColumnOffset * 0.01) - offsetLeft;
			var offsetBottom = (float)(bottomRightAnchorHeight * drawing.BottomRowOffset * 0.01) - offsetTop;

			var x = topLeftAnchorLocation.X + offsetLeft;
			var y = topLeftAnchorLocation.Y + offsetTop;

			var width = bottomRightAnchorLocation.X - topLeftAnchorLocation.X + offsetRight;
			var height = bottomRightAnchorLocation.Y - topLeftAnchorLocation.Y + offsetBottom;

			Action<string> errorCallback = message =>
			{
				var notification = new Notification(
					cell.CreateNotificationSource(),
					NotificationType.Error,
					message);

				cell.Add(notification);
			};

			var image = new Drawing(
				new PointF(x, y),
				new SizeF(width, height),
				cell.Drawing?.ImageData,
				errorCallback);

			image.ZOrder = DrawingOrder.Image;

			return image;
		}

		#endregion

		#region Cell Background

		IDocumentElement GetCellBackground(IDocumentCell cell, PointF location, SizeF size)
		{
			var rectangle = new Rectangle(cell, location, size)
			{
				ZOrder = DrawingOrder.Background
			};

			return rectangle;
		}

		#endregion

		#region Cell Borders

		enum BorderLocation
		{
			Top,
			Bottom,
			Left,
			Right
		}

		IEnumerable<IDocumentElement> GetCellBorders(ICell cell, PointF location, SizeF size, bool cellOverflowsBottom, bool cellOverflowsRight)
		{
			if (cell.Format.Borders.Top.Style != BorderStyle.None)
			{
				var borderLocation = GetBorderLocation(BorderLocation.Top, location, size);
				var borderSize = GetBorderSize(BorderLocation.Top, size);

				yield return GetCellBorder(cell.Format.Borders.Top, borderLocation, borderSize);
			}

			if (cell.Format.Borders.Bottom.Style != BorderStyle.None && !cellOverflowsBottom)
			{
				var borderLocation = GetBorderLocation(BorderLocation.Bottom, location, size);
				var borderSize = GetBorderSize(BorderLocation.Bottom, size);

				yield return GetCellBorder(cell.Format.Borders.Bottom, borderLocation, borderSize);
			}

			if (cell.Format.Borders.Left.Style != BorderStyle.None)
			{
				var borderLocation = GetBorderLocation(BorderLocation.Left, location, size);
				var borderSize = GetBorderSize(BorderLocation.Left, size);

				yield return GetCellBorder(cell.Format.Borders.Left, borderLocation, borderSize);
			}

			if (cell.Format.Borders.Right.Style != BorderStyle.None && !cellOverflowsRight)
			{
				var borderLocation = GetBorderLocation(BorderLocation.Right, location, size);
				var borderSize = GetBorderSize(BorderLocation.Right, size);

				yield return GetCellBorder(cell.Format.Borders.Right, borderLocation, borderSize);
			}

			if (cell.Format.Borders.DiagonalUp.Style != BorderStyle.None && !cellOverflowsRight)
			{
				var borderLocation = GetBorderLocation(BorderLocation.Bottom, location, size);
				var borderSize = new SizeF(size.Width, -size.Height);

				yield return GetCellBorder(cell.Format.Borders.DiagonalUp, borderLocation, borderSize);
			}

			if (cell.Format.Borders.DiagonalDown.Style != BorderStyle.None && !cellOverflowsRight)
			{
				var borderLocation = GetBorderLocation(BorderLocation.Top, location, size);
				var borderSize = size;

				yield return GetCellBorder(cell.Format.Borders.DiagonalDown, borderLocation, borderSize);
			}
		}

		IDocumentElement GetCellBorder(IBorder border, PointF location, SizeF size)
		{
			var pen = GetPen(border.Color, border.Style);

			var line = new Line(
				location,
				size,
				pen);

			line.ZOrder = DrawingOrder.Border;

			return line;
		}

		PointF GetBorderLocation(BorderLocation borderLocation, PointF cellLocation, SizeF cellSize)
		{
			switch (borderLocation)
			{
				case BorderLocation.Right:
					return new PointF(cellLocation.X + cellSize.Width, cellLocation.Y);

				case BorderLocation.Bottom:
					return new PointF(cellLocation.X, cellLocation.Y + cellSize.Height);

				default:
					return cellLocation;
			}
		}

		SizeF GetBorderSize(BorderLocation borderLocation, SizeF cellSize)
		{
			switch (borderLocation)
			{
				case BorderLocation.Top:
				case BorderLocation.Bottom:
					return new SizeF(cellSize.Width, 0f);

				default:
					return new SizeF(0f, cellSize.Height);
			}
		}

		Pen GetPen(Color color, BorderStyle borderStyle)
		{
			var pen = new Pen { Color = color };

			switch (borderStyle)
			{
				case BorderStyle.Thin:
					pen.Thickness = 1;
					break;

				case BorderStyle.Medium:
					pen.Thickness = 2;
					break;

				case BorderStyle.Thick:
					pen.Thickness = 3;
					break;

				case BorderStyle.Dashed:
					pen.DashStyle = DashStyle.Dash;
					break;

				case BorderStyle.Dotted:
					pen.DashStyle = DashStyle.Dot;
					break;

				case BorderStyle.DashDot:
					pen.DashStyle = DashStyle.DashDot;
					break;

				case BorderStyle.DashDotDot:
					pen.DashStyle = DashStyle.DashDotDot;
					break;
			}

			return pen;
		}

		#endregion
	}
}
